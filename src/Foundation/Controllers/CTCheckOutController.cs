using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Orders;
using EPiServer.Security;
using Mediachase.Commerce.Customers;
using Mediachase.Data.Provider;

// for the extension-method
using Mediachase.Commerce.Security;
using Mediachase.Commerce.Markets;
using Foundation.Models.Pages;
using Foundation.Models.ViewModels;
using Foundation.Features.Checkout.Services;
using Mediachase.Commerce.Core;
using Foundation.Features.Home;

namespace Foundation.Controllers
{
    public class CTCheckOutController : PageController<CTCheckOutPage>
    {
        private const string DefaultCartName = "Default";

        private readonly IContentLoader _contentLoader; // To get the StartPage --> Settings-links
        private readonly ICurrentMarket _currentMarket; // not in fund... yet
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IPromotionEngine _promotionEngine;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly ILineItemCalculator _lineItemCalculator;
        private readonly IInventoryProcessor _inventoryProcessor;
        private readonly ILineItemValidator _lineItemValidator;
        private readonly IPlacedPriceProcessor _placedPriceProcessor;
        private readonly ICartService _cartService;
        public CTCheckOutController(IContentLoader contentLoader
            , ICurrentMarket currentMarket
            , IOrderRepository orderRepository
            , IPlacedPriceProcessor placedPriceProcessor
            , IInventoryProcessor inventoryProcessor
            , ILineItemValidator lineItemValidator
            , IOrderGroupCalculator orderGroupCalculator
            , ILineItemCalculator lineItemCalculator
            , IOrderGroupFactory orderGroupFactory
            , IPaymentProcessor paymentProcessor
            , IPromotionEngine promotionEngine
            , ICartService cartService)
        {
            _contentLoader = contentLoader;
            _currentMarket = currentMarket;
            _orderRepository = orderRepository;
            _orderGroupCalculator = orderGroupCalculator;
            _orderGroupFactory = orderGroupFactory;
            _paymentProcessor = paymentProcessor;
            _promotionEngine = promotionEngine;
            _lineItemCalculator = lineItemCalculator;
            _inventoryProcessor = inventoryProcessor;
            _lineItemValidator = lineItemValidator;
            _placedPriceProcessor = placedPriceProcessor;
            _cartService = cartService;
        }

        // ToDo: in the first exercise (E1) Ship & Pay
        public ActionResult Index(CTCheckOutPage currentPage)
        {
            string cartValidationMessages = String.Empty;
            //Cleaning up may be needed
            
            // a few different ways of loading
            var cart = _orderRepository.Load<ICart>(GetContactId(), DefaultCartName).FirstOrDefault();
            var cart2 = _orderRepository.LoadCart<ICart>(GetContactId(), DefaultCartName);
            var cart3 = _cartService.LoadOrCreateCart(_cartService.DefaultCartName);
            if (cart3 == null)
            {
                throw new InvalidOperationException("No cart found"); // Fix this
            }

            // May want to validate the cart initially
            //cartValidationMessages = ValidateCart(cart);

            var model = new CTCheckOutViewModel(currentPage)
            {
                // ToDo: get shipments & payments and populate the UI
                PaymentMethods = GetPaymentMethods(),
                ShippingMethods = GetShipmentMethods(),
                ShippingRates = GetShippingRates(),
                cartValidationMessages = ValidateCart(cart3)
            };

            return View(model);
        }
        
        // Exercise (E1) creation of GetPaymentMethods(), GetShipmentMethods() and GetShippingRates() goes below
        // ToDo: Get IEnumerables of Shipping and Payment methods along with ShippingRates
        private IEnumerable<PaymentMethodDto.PaymentMethodRow> GetPaymentMethods()
        {
            var paymentMethods = PaymentManager.GetPaymentMethods(SiteContext.Current.LanguageName).PaymentMethod;
            return new List<PaymentMethodDto.PaymentMethodRow>(paymentMethods);
            //return new List<PaymentMethodDto.PaymentMethodRow>(PaymentManager.GetPaymentMethodsByMarket(MarketId.Default.Value).PaymentMethod);
        }

        private IEnumerable<ShippingMethodDto.ShippingMethodRow> GetShipmentMethods()
        {
            var marketShippingMethods = ShippingManager.GetShippingMethodsByMarket(MarketId.Default.Value, false).ShippingMethod;
            var shippingMethods =  new List<ShippingMethodDto.ShippingMethodRow>(marketShippingMethods);

            //Get the list of all shipping methods to be filtered 
            ShippingMethodDto methods = ShippingManager.GetShippingMethods(SiteContext.Current.LanguageName);

            // filter the list for only methods that apply to this particular cart's shipping address 
            List<ShippingMethodDto.ShippingMethodRow> shippingRows = new List<ShippingMethodDto.ShippingMethodRow>();

            foreach (ShippingMethodDto.ShippingMethodRow method in methods.ShippingMethod.Rows)
            {
                shippingRows.Add(method);
            }
            return shippingRows;
        }

        private IEnumerable<ShippingRate> GetShippingRates() // 
        {
            List<ShippingRate> shippingRates = new List<ShippingRate>();
            IEnumerable<ShippingMethodDto.ShippingMethodRow> shippingMethods = GetShipmentMethods();

            foreach (var item in shippingMethods)
            {
                shippingRates.Add(new ShippingRate(item.ShippingMethodId, item.DisplayName, new Money(item.BasePrice, item.Currency)));
            }

            return shippingRates;
        }
        
        //Exercise (E2) Do CheckOut
        public ActionResult CheckOut(CTCheckOutViewModel model)
        {
            // ToDo: Load the cart... remember the other ways of doing this
            //var cart = _orderRepository.Load<ICart>(GetContactId(), DefaultCartName).FirstOrDefault();
            var cart3 = _cartService.LoadOrCreateCart(_cartService.DefaultCartName);
            if (cart3 == null)
            {
                throw new InvalidOperationException("No cart found"); // make nicer
            }

            // ToDo: Add an OrderAddress
            IOrderAddress theAddress = AddAddressToOrder(cart3);

            // ToDo: Define/update Shipping
            AdjustFirstShipmentInOrder(cart3, theAddress, model.SelectedShipId);

            // ToDo: Discounts for the cart, not in exercises yet...
            // a lot easier than before, does  promotionEngine.Run(orderGroup, settings);
            // Return IEnumerable<RewardDescription>
            var rewards = cart3.ApplyDiscounts();

            // ToDo: Add a Payment to the Order 
            AddPaymentToOrder(cart3, model.SelectedPayId);

            // ToDo: Add a transaction scope and convert the cart to PO
            IPurchaseOrder purchaseOrder;
            OrderReference orderReference;
            using (var scope = new TransactionScope())
            {
                var validationIssues = new Dictionary<ILineItem, ValidationIssue>();

                // Added - sets a lock on inventory... could come earlier (outside tran) depending on TypeOf-"store"
                _inventoryProcessor.AdjustInventoryOrRemoveLineItem(cart3.GetFirstShipment()
                    , OrderStatus.InProgress, (item, issue) => validationIssues.Add(item, issue));

                if (validationIssues.Count >= 1)
                {
                    throw new Exception("Not possible right now"); // ...change approach and fork
                }

                // ECF 10 - void back
                //cart3.ProcessPayments();
                // ECF 11
                IEnumerable<PaymentProcessingResult> PaymentProcessingResult = cart3.ProcessPayments();
                var isSuccess = PaymentProcessingResult.First().IsSuccessful;

                // just looking around - (nice extension methods)
                var cartTotal = cart3.GetTotal();
                var handling = cart3.GetHandlingTotal(); // OG-Calculator... aggregate on "all forms"
                var form = cart3.GetFirstForm(); // 
                var formHandling = form.HandlingTotal; // "handling" sits here on OF

                /* orderGroupCalculator does:
                  - Catches up with the "IOrderFormCalculator"
                  - GetSubTotal - Form-Calculator does
                  - GetHandlingTotal - Form-Calc does
                  - GetShippingCost - Form-Calc. does with ShippingCalc. - "processes" the shipment
                  - GetTaxTotal - FormCalc. does with Tax-Calc.
                 */

                var totalProcessedAmount = cart3.GetFirstForm().Payments.Where
                    (x => x.Status.Equals(PaymentStatus.Processed.ToString())).Sum(x => x.Amount);
                
                // could do more than this, but we just processed payment(s)
                if (totalProcessedAmount != cart3.GetTotal(_orderGroupCalculator).Amount)
                {
                    // ...we're not happy, put back the reserved request
                    _inventoryProcessor.AdjustInventoryOrRemoveLineItem(cart3.GetFirstShipment()
                        , OrderStatus.Cancelled, (item, issue) => validationIssues.Add(item, issue));

                    #region OldSchool Inventory check
                    //List<InventoryRequestItem> requestItems = new List<InventoryRequestItem>(); // holds the "items"
                    //InventoryRequestItem requestItem = new InventoryRequestItem();

                    //// calls for some logic
                    //requestItem.RequestType = InventoryRequestType.Cancel; // as a demo
                    //requestItem.OperationKey = reqKey;

                    //requestItems.Add(requestItem);

                    //InventoryRequest inventoryRequest = new InventoryRequest(DateTime.UtcNow, requestItems, null);
                    //InventoryResponse inventoryResponse = _invService.Service.Request(inventoryRequest);
                    //InventoryRecord rec4 = _invService.Service.Get(LI.Code, wh.Code);
                    #endregion OldSchool
                    
                    throw new InvalidOperationException("Wrong amount"); // maybe change approach
                }

                // we're happy
                // ...could do this here - look at dhe different statuses
                cart3.GetFirstShipment().OrderShipmentStatus = OrderShipmentStatus.InventoryAssigned;

                // decrement inventory and let it go
                _inventoryProcessor.AdjustInventoryOrRemoveLineItem(cart3.GetFirstShipment()
                    , OrderStatus.Completed, (item, issue) => validationIssues.Add(item, issue));

                #region OldSchool Inventory check
                //List<InventoryRequestItem> requestItems1 = new List<InventoryRequestItem>(); // holds the "items"
                //InventoryRequestItem requestItem1 = new InventoryRequestItem();

                //// calls for some logic
                //requestItem1.RequestType = InventoryRequestType.Complete; // as a demo
                //requestItem1.OperationKey = reqKey;

                //requestItems1.Add(requestItem1);

                //InventoryRequest inventoryRequest1 = new InventoryRequest(DateTime.UtcNow, requestItems1, null);
                //InventoryResponse inventoryResponse1 = _invService.Service.Request(inventoryRequest1);
                #endregion OldSchool

                // we're even happier
                orderReference = _orderRepository.SaveAsPurchaseOrder(cart3);
                _orderRepository.Delete(cart3.OrderLink);

                scope.Complete();
            } // End TransactionScope

            // ToDo: Housekeeping (Statuses for Shipping and PO, OrderNotes and save the order)
            // ...may need this below
            purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
            //_orderRepository.Load<IPurchaseOrder>()

            // check the below
            var theType = purchaseOrder.OrderLink.OrderType;
            var toString = purchaseOrder.OrderLink.ToString(); // Gets ID and Type ... combined

            #region ThisAndThat
            // should/could do some with OrderStatusManager, slightly old-school
            // OrderStatusManager.

            OrderStatus poStatus;
            poStatus = purchaseOrder.OrderStatus;
            //purchaseOrder.OrderStatus = OrderStatus.InProgress;

            //var info = OrderStatusManager.GetPurchaseOrderStatus(PO); // old-school
            //PO.Status = OrderStatus.InProgress.ToString();

            var shipment = purchaseOrder.GetFirstShipment();
            var status = shipment.OrderShipmentStatus;

            //shipment. ... no that much to do
            //shipment.OrderShipmentStatus = OrderShipmentStatus.InventoryAssigned;

            // Don't use OrderNoteManager ... it doesn't see the "new" stuff
            var notes = purchaseOrder.Notes; // IOrderNote is 0

            Mediachase.Commerce.Orders.OrderNote otherNote = new OrderNote //IOrderNote 
            {
                // Created = DateTime.Now, // do we need to set this ?? Nope .ctor does
                CustomerId = new Guid(), // can set this - regarded
                Detail = "Order ToString(): " + toString + " - Shipment tracking number: " + shipment.ShipmentTrackingNumber,
                LineItemId = purchaseOrder.GetAllLineItems().First().LineItemId,
                // OrderGroupId = 12, R/O - error
                // OrderNoteId = 12, // can define it, but it's disregarded - no error
                Title = "Some title",
                Type = OrderNoteTypes.Custom.ToString()
            };

            // not intended to be used, as it's "Internal"
            //IOrderNote theNote = new EPiServer.Commerce.Order.Internal.SerializableNote();

            purchaseOrder.Notes.Add(otherNote); // void back
            purchaseOrder.ExpirationDate = DateTime.Now.AddMonths(1);

            // ...need to come after adding notes
            _orderRepository.Save(purchaseOrder);

            #endregion

            // Final steps, navigate to the order confirmation page
            HomePage home = _contentLoader.Get<HomePage>(ContentReference.StartPage);
            ContentReference orderPageReference = home.Settings.orderPage;

            // the below is a dummy, change to "PO".OrderNumber when done
            //string passingValue = String.Empty;
            string passingValue = purchaseOrder.OrderNumber;

            return RedirectToAction("Index", "CTOrder", new { node = orderPageReference, passedAlong = passingValue });
        }
        
        // Prewritten 
        private string ValidateCart(ICart cart)
        {
            var validationMessages = string.Empty;

            cart.ValidateOrRemoveLineItems((item, issue) =>
                validationMessages += CreateValidationMessages(item, issue), _lineItemValidator);

            cart.UpdatePlacedPriceOrRemoveLineItems(GetContact(), (item, issue) =>
                validationMessages += CreateValidationMessages(item, issue), _placedPriceProcessor);

            cart.UpdateInventoryOrRemoveLineItems((item, issue) =>
                validationMessages += CreateValidationMessages(item, issue), _inventoryProcessor);

            return validationMessages; 
        }

        private static string CreateValidationMessages(ILineItem item, ValidationIssue issue)
        {
            return string.Format("Line item with code {0} had the validation issue {1}.", item.Code, issue);
        }

        private void AdjustFirstShipmentInOrder(ICart cart, IOrderAddress orderAddress, Guid selectedShip)
        {

            // Need to set the guid (name is good to have too) of some "real shipmentment in the DB"
            // RoCe - this step is not needed, actually - code and lab-steps can be updated
            // We'll do it to show how it works
            var shippingMethod = ShippingManager.GetShippingMethod(selectedShip).ShippingMethod.First();

            IShipment theShip = cart.GetFirstShipment(); // ...as we get one "for free"

            // Need the choice of shipment from DropDowns
            theShip.ShippingMethodId = shippingMethod.ShippingMethodId;
            //theShip.ShippingMethodName = "TucTuc"; ...12

            theShip.ShippingAddress = orderAddress;

            #region Hard coded and cheating just to show

            var mSrv = ServiceLocator.Current.GetInstance<IMarketService>();
            var defaultMarket = mSrv.GetMarket(MarketId.Default); // cheating some

            Money cost00 = theShip.GetShippingCost(_currentMarket.GetCurrentMarket(), new Currency("USD"));
            Money cost000 = theShip.GetShippingCost(_currentMarket.GetCurrentMarket(), cart.Currency);
            #endregion

            Money cost0 = theShip.GetShippingCost(
                _currentMarket.GetCurrentMarket()
                , _currentMarket.GetCurrentMarket().DefaultCurrency); // to make it easy

            // done by the "default calculator"
            Money cost1 = theShip.GetShippingItemsTotal(_currentMarket.GetCurrentMarket().DefaultCurrency);

            theShip.ShipmentTrackingNumber = "ABC123";
        } // End AdjustFirstShipmentInOrder

        private void AddPaymentToOrder(ICart cart, Guid selectedPaymentGuid)
        {
            if (cart.GetFirstForm().Payments.Any())
            {
                // should maybe clean up in the cart here
            }

            var selectedPaymentMethod =
                PaymentManager.GetPaymentMethod(selectedPaymentGuid).PaymentMethod.First();

            var payment = _orderGroupFactory.CreatePayment(cart);

            // check if str "soliciting" still works
            payment.PaymentMethodId = selectedPaymentMethod.PaymentMethodId;
            //payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodName = selectedPaymentMethod.Name;
            
            // check this thing with "classname"/PaymentType as string

            // usless - crash, doesn't support "Serializable"
            //var type = PaymentTransactionTypeManager.GetPaymentTransactionType((Payment)payment);

            // this one... but not really useful
            var className = selectedPaymentMethod.ClassName;
            // it's a string (as we see it in "Adv" when creating options & methods)

            // ...we also have - cart.GetTotal(_orderGroupCalculator)
            payment.Amount = _orderGroupCalculator.GetTotal(cart).Amount; // need a debug here

            cart.AddPayment(payment);
            // could add payment.BillingAddress = theAddress ... if we had it here
        } // End AddPaymentToOrder

        private IOrderAddress AddAddressToOrder(ICart cart)
        {
            IOrderAddress shippingAddress;

            if (CustomerContext.Current.CurrentContact == null) // anonymous
            {
                // Anonymous... one way of "doing it"... for example, if no other address exist
                var shipment = cart.GetFirstShipment(); // ... moved to shipment - prev. = .OrderAddresses.Add(

                if (shipment.ShippingAddress != null)
                {
                    //return false/true; // Should we clean upfirst? 
                }
                
                // New School
                IOrderAddress myOrderAddress = _orderGroupFactory.CreateOrderAddress(cart);
                myOrderAddress.CountryName = "Sweden";
                myOrderAddress.Id = "MyNewAddress";
                myOrderAddress.Email = "SomeGuy@epi.com";

                // temp-fix this
                shippingAddress = myOrderAddress;

                // OldSchool
                //shippingAddress = shipment.ShippingAddress = // 
                //    new OrderAddress
                //    {
                //        CountryCode = "USA",
                //        CountryName = "United States",
                //        Name = "SomeCustomerAddressName",
                //        DaytimePhoneNumber = "123456",
                //        FirstName = "John",
                //        LastName = "Smith",
                //        Email = "John@company.com",
                //    };

            } // end anonymous
            else
            {
                // Logged in
                if (CustomerContext.Current.CurrentContact.PreferredShippingAddress == null)
                {
                    // no pref. address set... so we set one for the contact
                    CustomerAddress newCustAddress = CustomerAddress.CreateInstance();
                    newCustAddress.AddressType = CustomerAddressTypeEnum.Shipping | CustomerAddressTypeEnum.Public; // mandatory
                    newCustAddress.ContactId = CustomerContext.Current.CurrentContact.PrimaryKeyId;
                    newCustAddress.CountryCode = "SWE";
                    newCustAddress.CountryName = "Sweden";
                    newCustAddress.Name = "new customer address"; // mandatory
                    newCustAddress.DaytimePhoneNumber = "123456";
                    newCustAddress.FirstName = CustomerContext.Current.CurrentContact.FirstName;
                    newCustAddress.LastName = CustomerContext.Current.CurrentContact.LastName;
                    newCustAddress.Email = "GuitarWorld@Thule.com";

                    // note: Line1 & City is what is shown in CM at a few places... not the Name
                    CustomerContext.Current.CurrentContact.AddContactAddress(newCustAddress);
                    CustomerContext.Current.CurrentContact.SaveChanges();

                    // ... needs to be in this order
                    CustomerContext.Current.CurrentContact.PreferredShippingAddress = newCustAddress;
                    CustomerContext.Current.CurrentContact.SaveChanges(); // need this ...again 

                    // then, for the cart
                    //.Cart.OrderAddresses.Add(new OrderAddress(newCustAddress)); - OLD
                    shippingAddress = new OrderAddress(newCustAddress); // - NEW
                }
                else
                {
                    // ...a 3:rd vay, there is a preferred address set
                    shippingAddress = new OrderAddress(
                        CustomerContext.Current.CurrentContact.PreferredShippingAddress);
                }
            }

            return shippingAddress;
        } //End AddAddressToOrder

        private static CustomerContact GetContact()
        {
            return CustomerContext.Current.GetContactById(GetContactId());
        }

        private static Guid GetContactId()
        {
            return PrincipalInfo.CurrentPrincipal.GetContactId();
        }
    }
}