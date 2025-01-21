using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.UI.Admin.Shipping.Internal;
using EPiServer.Security;
using Foundation.Features.CatalogContent.DynamicCatalogContent.DynamicVariation;
using Foundation.Features.Checkout;
using Foundation.Features.Checkout.ViewModels;
using Foundation.Features.NamedCarts;
using Foundation.Features.Checkout.Services;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Security;
using System.Text;
using IShippingService = Foundation.Features.Checkout.Services.IShippingService;

namespace Foundation.Services
{
    public class CTCartService : ICTCartService
    {
        private readonly string VariantOptionCodesProperty = "VariantOptionCodes";
        private readonly ReferenceConverter _referenceConverter;
        private readonly IContentLoader _contentLoader;
        private readonly IRelationRepository _relationRepository;
        private readonly IShippingService _shippingManagerFacade;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private ShippingMethodInfoModel _instorePickup;
        private readonly CustomerContext _customerContext;
        private readonly ILineItemValidator _lineItemValidator;
        private readonly IPlacedPriceProcessor _placedPriceProcessor;
        private readonly IInventoryProcessor _inventoryProcessor;
        private readonly IPromotionEngine _promotionEngine;
        private readonly ICurrencyService _currencyService;
        private readonly ICurrentMarket _currentMarket;

        public CTCartService(ReferenceConverter referenceConverter, 
            IContentLoader contentLoader,
            IRelationRepository relationRepository,
            IWarehouseRepository warehouseRepository,
            IOrderRepository orderRepository,
            IOrderGroupFactory orderGroupFactory,
            IShippingService shippingService,
            CustomerContext customerContext,
            ILineItemValidator lineItemValidator,
            IPlacedPriceProcessor placedPriceProcessor,
            IInventoryProcessor inventoryProcessor,
            IPromotionEngine promotionEngine,
            ICurrentMarket currentMarket
            ) {
            _referenceConverter = referenceConverter;
            _contentLoader = contentLoader;
            _relationRepository = relationRepository;
            _warehouseRepository = warehouseRepository;
            _orderRepository = orderRepository; 
            _orderGroupFactory = orderGroupFactory;
            _shippingManagerFacade = shippingService;
            _customerContext = customerContext;
            _lineItemValidator = lineItemValidator;
            _placedPriceProcessor = placedPriceProcessor;
            _inventoryProcessor = inventoryProcessor;
            _promotionEngine = promotionEngine;
            _currentMarket = currentMarket;
        }
        public ShippingMethodInfoModel InStorePickupInfoModel => _instorePickup ?? (_instorePickup = _shippingManagerFacade.GetInstorePickupModel());
        public string DefaultCartName => "Default" + SiteDefinition.Current.StartPage.ID;

        public string DefaultWishListName => "WishList" + SiteDefinition.Current.StartPage.ID;
        public string DefaultSharedCartName => "Shared" + SiteDefinition.Current.StartPage.ID;

        public string DefaultOrderPadName => "OrderPad" + SiteDefinition.Current.StartPage.ID;

        public AddToCartResult AddToCart(ICart cart, RequestParamsToCart requestParams)
        {
            var contentLink = _referenceConverter.GetContentLink(requestParams.Code);
            var entryContent = _contentLoader.Get<EntryContentBase>(contentLink);
            return AddToCart(cart, entryContent, requestParams.Quantity, requestParams.Store, requestParams.SelectedStore, requestParams.DynamicCodes);
        }

        public AddToCartResult AddToCart(ICart cart, EntryContentBase entryContent, decimal quantity, string deliveryMethod, string warehouseCode, List<string> dynamicVariantOptionCodes)
        {
            var result = new AddToCartResult();
            var contact = PrincipalInfo.CurrentPrincipal.GetCustomerContact();

            if (contact?.OwnerId != null)
            {
                var org = cart.GetString("OwnerOrg");
                if (string.IsNullOrEmpty(org))
                {
                    cart.Properties["OwnerOrg"] = contact.OwnerId.Value.ToString().ToLower();
                }
            }

            IWarehouse warehouse = null;

            if (deliveryMethod.Equals("instore") && !string.IsNullOrEmpty(warehouseCode))
            {
                warehouse = _warehouseRepository.Get(warehouseCode);
            }

            if (entryContent is BundleContent)
            {
                foreach (var relation in _relationRepository.GetChildren<BundleEntry>(entryContent.ContentLink))
                {
                    var entry = _contentLoader.Get<EntryContentBase>(relation.Child);
                    var recursiveResult = AddToCart(cart, entry, (relation.Quantity ?? 1) * quantity, deliveryMethod, warehouseCode, dynamicVariantOptionCodes);
                    if (recursiveResult.EntriesAddedToCart)
                    {
                        result.EntriesAddedToCart = true;
                    }

                    foreach (var message in recursiveResult.ValidationMessages)
                    {
                        result.ValidationMessages.Add(message);
                    }
                }

                return result;
            }

            var form = cart.GetFirstForm();
            if (form == null)
            {
                form = _orderGroupFactory.CreateOrderForm(cart);
                form.Name = cart.Name;
                cart.Forms.Add(form);
            }

            var shipment = cart.GetFirstForm().Shipments.FirstOrDefault(x => string.IsNullOrEmpty(warehouseCode) || (x.WarehouseCode == warehouseCode && x.ShippingMethodId == InStorePickupInfoModel.MethodId));
            if (warehouse != null)
            {
                if (shipment != null && !shipment.LineItems.Any())
                {
                    shipment.WarehouseCode = warehouseCode;
                    shipment.ShippingMethodId = InStorePickupInfoModel.MethodId;
                    shipment.ShippingAddress = GetOrderAddressFromWarehosue(cart, warehouse);
                }
                else
                {
                    shipment = form.Shipments.FirstOrDefault(x => !string.IsNullOrEmpty(x.WarehouseCode) && x.WarehouseCode.Equals(warehouse.Code));
                    if (shipment == null)
                    {
                        if (cart.GetFirstShipment().LineItems.Count > 0)
                        {
                            shipment = _orderGroupFactory.CreateShipment(cart);
                        }
                        else
                        {
                            shipment = cart.GetFirstShipment();
                        }

                        shipment.WarehouseCode = warehouseCode;
                        shipment.ShippingMethodId = InStorePickupInfoModel.MethodId;
                        shipment.ShippingAddress = GetOrderAddressFromWarehosue(cart, warehouse);

                        if (cart.GetFirstShipment().LineItems.Count > 0)
                        {
                            cart.GetFirstForm().Shipments.Add(shipment);
                        }
                    }
                }
            }

            if (shipment == null)
            {
                var cartFirstShipment = cart.GetFirstShipment();
                if (cartFirstShipment == null)
                {
                    shipment = _orderGroupFactory.CreateShipment(cart);
                    cart.GetFirstForm().Shipments.Add(shipment);
                }
                else
                {
                    if (cartFirstShipment.LineItems.Count > 0)
                    {
                        shipment = _orderGroupFactory.CreateShipment(cart);
                        cart.GetFirstForm().Shipments.Add(shipment);
                    }
                    else
                    {
                        shipment = cartFirstShipment;
                    }
                }
            }

            var lineItem = shipment.LineItems.FirstOrDefault(x => x.Code == entryContent.Code);
            decimal originalLineItemQuantity = 0;

            if (lineItem == null)
            {
                lineItem = cart.CreateLineItem(entryContent.Code, _orderGroupFactory);
                var lineDisplayName = entryContent.DisplayName;
                if (dynamicVariantOptionCodes?.Count > 0)
                {
                    lineItem.Properties[VariantOptionCodesProperty] = string.Join(",", dynamicVariantOptionCodes.OrderBy(x => x));
                    lineDisplayName += " - " + lineItem.Properties[VariantOptionCodesProperty];
                }

                lineItem.DisplayName = lineDisplayName;
                lineItem.Quantity = quantity;
                cart.AddLineItem(shipment, lineItem);
            }
            else
            {
                if (lineItem.Properties[VariantOptionCodesProperty] != null)
                {
                    var variantOptionCodesLineItem = lineItem.Properties[VariantOptionCodesProperty].ToString().Split(',');
                    var intersectCodes = variantOptionCodesLineItem.Intersect(dynamicVariantOptionCodes);

                    if (intersectCodes != null && intersectCodes.Any()
                        && intersectCodes.Count() == variantOptionCodesLineItem.Length
                        && intersectCodes.Count() == dynamicVariantOptionCodes.Count)
                    {
                        originalLineItemQuantity = lineItem.Quantity;
                        cart.UpdateLineItemQuantity(shipment, lineItem, lineItem.Quantity + quantity);
                    }
                    else
                    {
                        lineItem = cart.CreateLineItem(entryContent.Code, _orderGroupFactory);
                        lineItem.Properties[VariantOptionCodesProperty] = string.Join(",", dynamicVariantOptionCodes.OrderBy(x => x));
                        lineItem.DisplayName = entryContent.DisplayName + " - " + lineItem.Properties[VariantOptionCodesProperty];
                        lineItem.Quantity = quantity;
                        cart.AddLineItem(shipment, lineItem);
                    }
                }
                else
                {
                    originalLineItemQuantity = lineItem.Quantity;
                    cart.UpdateLineItemQuantity(shipment, lineItem, lineItem.Quantity + quantity);
                }
            }

            var validationIssues = ValidateCart(cart);
            var newLineItem = shipment.LineItems.FirstOrDefault(x => x.Code == entryContent.Code);
            var isAdded = (newLineItem != null ? newLineItem.Quantity : 0) - originalLineItemQuantity > 0;

            AddValidationMessagesToResult(result, lineItem, validationIssues, isAdded);

            return result;
        }

        public Dictionary<ILineItem, List<ValidationIssue>> ValidateCart(ICart cart)
        {
            var validationIssues = new Dictionary<ILineItem, List<ValidationIssue>>();
            if (cart.Name.Equals(DefaultWishListName))
            {
                cart.UpdatePlacedPriceOrRemoveLineItems(_customerContext.GetContactById(cart.CustomerId), (item, issue) => validationIssues.AddValidationIssues(item, issue), _placedPriceProcessor);
                return validationIssues;
            }

            cart.ValidateOrRemoveLineItems((item, issue) => validationIssues.AddValidationIssues(item, issue), _lineItemValidator);
            cart.UpdatePlacedPriceOrRemoveLineItems(_customerContext.GetContactById(cart.CustomerId), (item, issue) => validationIssues.AddValidationIssues(item, issue), _placedPriceProcessor);
            cart.UpdateInventoryOrRemoveLineItems((item, issue) => validationIssues.AddValidationIssues(item, issue), _inventoryProcessor);

            var shipments = cart.GetFirstForm().Shipments;
            foreach (var shipment in shipments)
            {
                var dynamicLineItems = shipment.LineItems.Where(x => !string.IsNullOrEmpty(x.Properties[VariantOptionCodesProperty]?.ToString()));

                foreach (var item in dynamicLineItems)
                {
                    var dynamicCodesStr = item.Properties[VariantOptionCodesProperty].ToString();
                    var dynamicCodes = dynamicCodesStr.Split(',');
                    var contentLink = _referenceConverter.GetContentLink(item.Code);
                    var variant = _contentLoader.Get<IContent>(contentLink) as DynamicVariant;
                    var dynamicVariants = variant.VariantOptions.Where(x => dynamicCodes.Contains(x.Code));
                    var totalDynamicVariantsPrice = dynamicVariants.Sum(x => x.Prices.FirstOrDefault(p => p.Currency == cart.Currency.CurrencyCode).Amount);
                    item.Properties["BasePrice"] = item.PlacedPrice;
                    item.Properties["OptionPrice"] = totalDynamicVariantsPrice;
                    item.PlacedPrice += totalDynamicVariantsPrice;

                    cart.UpdateLineItemQuantity(shipment, item, item.Quantity);
                }
            }

            cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());

            return validationIssues;
        }

        public Dictionary<ILineItem, List<ValidationIssue>> RequestInventory(ICart cart)
        {
            var validationIssues = new Dictionary<ILineItem, List<ValidationIssue>>();
            cart.AdjustInventoryOrRemoveLineItems((item, issue) => validationIssues.AddValidationIssues(item, issue), _inventoryProcessor);
            return validationIssues;
        }


        private IOrderAddress GetOrderAddressFromWarehosue(ICart cart, IWarehouse warehouse)
        {
            var address = _orderGroupFactory.CreateOrderAddress(cart);
            address.Id = warehouse.Code;
            address.City = warehouse.ContactInformation.City;
            address.CountryCode = warehouse.ContactInformation.CountryCode;
            address.CountryName = warehouse.ContactInformation.CountryName;
            address.DaytimePhoneNumber = warehouse.ContactInformation.DaytimePhoneNumber;
            address.Email = warehouse.ContactInformation.Email;
            address.EveningPhoneNumber = warehouse.ContactInformation.EveningPhoneNumber;
            address.FaxNumber = warehouse.ContactInformation.FaxNumber;
            address.FirstName = warehouse.ContactInformation.FirstName;
            address.LastName = warehouse.ContactInformation.LastName;
            address.Line1 = warehouse.ContactInformation.Line1;
            address.Line2 = warehouse.ContactInformation.Line2;
            address.Organization = warehouse.ContactInformation.Organization;
            address.PostalCode = warehouse.ContactInformation.PostalCode;
            address.RegionName = warehouse.ContactInformation.RegionName;
            address.RegionCode = warehouse.ContactInformation.RegionCode;
            return address;
        }

        private static void AddValidationMessagesToResult(AddToCartResult result, ILineItem lineItem, Dictionary<ILineItem, List<ValidationIssue>> validationIssues, bool isHasAddedItem)
        {
            foreach (var validationIssue in validationIssues)
            {
                var warning = new StringBuilder();
                warning.Append(string.Format("Line Item with code {0} ", lineItem.Code));
                validationIssue.Value.Aggregate(warning, (current, issue) => current.Append(issue).Append(", "));

                result.ValidationMessages.Add(warning.ToString().TrimEnd(',', ' '));
            }

            if (!validationIssues.HasItemBeenRemoved(lineItem) && isHasAddedItem)
            {
                result.EntriesAddedToCart = true;
            }
        }
        public ICart LoadOrCreateCart(string name) => LoadOrCreateCart(name, _customerContext.CurrentContactId.ToString());

        public ICart LoadOrCreateCart(string name, string contactId)
        {
            if (string.IsNullOrEmpty(contactId))
            {
                return null;
            }
            else
            {
                var cart = _orderRepository.LoadOrCreateCart<ICart>(new Guid(contactId), name, _currentMarket);
                if (cart != null)
                {
                    SetCartCurrency(cart, _currencyService.GetCurrentCurrency());
                }

                return cart;
            }
        }
        public void SetCartCurrency(ICart cart, Currency currency)
        {
            if (currency.IsEmpty || currency == cart.Currency)
            {
                return;
            }

            cart.Currency = currency;
            foreach (var lineItem in cart.GetAllLineItems())
            {
                // If there is an item which has no price in the new currency, a NullReference exception will be thrown.
                // Mixing currencies in cart is not allowed.
                // It's up to site's managers to ensure that all items have prices in allowed currency.
                lineItem.PlacedPrice = PriceCalculationService.GetSalePrice(lineItem.Code, cart.MarketId, currency).UnitPrice.Amount;
            }

            ValidateCart(cart);
        }
        public CartWithValidationIssues LoadCart(string name, bool validate) => LoadCart(name, _customerContext.CurrentContactId.ToString(), validate);

        public CartWithValidationIssues LoadCart(string name, string contactId, bool validate)
        {
            var validationIssues = new Dictionary<ILineItem, List<ValidationIssue>>();
            var cart = !string.IsNullOrEmpty(contactId) ? _orderRepository.LoadOrCreateCart<ICart>(new Guid(contactId), name, _currentMarket) : null;
            if (cart != null)
            {
                SetCartCurrency(cart, _currencyService.GetCurrentCurrency());
                if (validate)
                {
                    validationIssues = ValidateCart(cart);
                    if (validationIssues.Any())
                    {
                        _orderRepository.Save(cart);
                    }
                }
            }

            return new CartWithValidationIssues
            {
                Cart = cart,
                ValidationIssues = validationIssues
            };
        }



    }
}
