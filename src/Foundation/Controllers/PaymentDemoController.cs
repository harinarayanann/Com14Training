using Foundation.Models.ViewModels;
using EPiServer.Commerce.Catalog;
using EPiServer.Find.Commerce.Services.Internal;
using Foundation.Infrastructure.Commerce.GiftCard;
using Foundation.Models.Catalog;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders.Managers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using EPiServer.Globalization;
using Mediachase.Commerce.Orders.Dto;


namespace Foundation.Controllers
{
    public class PaymentDemoController : Controller
    {
        private ReferenceConverter _referenceConverter;
        private IContentLoader _contentLoader;
        private AssetUrlResolver _assetUrlResolver;
        private ICurrentMarket _currentMarket;
        private IPaymentProcessor _paymentProcessor;
        private IOrderRepository _orderRepository;
        private IOrderGroupFactory _orderGroupFactory;
        private IOrderGroupCalculator _orderGroupCalculator;
        private IGiftCardService _giftCardService;

        public PaymentDemoController(ReferenceConverter referenceConverter, IContentLoader contentLoader,
            AssetUrlResolver assetUrlResolver, ICurrentMarket currentMarket, IPaymentProcessor paymentProcessor,
            IOrderRepository orderRepository, IOrderGroupFactory orderGroupFactory, IOrderGroupCalculator orderGroupCalculator,
            IGiftCardService giftCardService)
        {
            _referenceConverter = referenceConverter;
            _contentLoader = contentLoader;
            _assetUrlResolver = assetUrlResolver;
            _currentMarket = currentMarket;
            _paymentProcessor = paymentProcessor;
            _orderRepository = orderRepository;
            _orderGroupFactory = orderGroupFactory;
            _orderGroupCalculator = orderGroupCalculator;
            _giftCardService = giftCardService;
        }
        public ActionResult Index()
        {
            var viewModel = new PaymentDemoViewModel();
            InitializeModel(viewModel);

            return View(viewModel);
        }

        private void InitializeModel(PaymentDemoViewModel viewModel)
        {
            ICart cart = _orderRepository.LoadOrCreateCart<ICart>(CustomerContext.Current.CurrentContactId, "Default");

            var shirtRef = _referenceConverter.GetContentLink("Swift-CT_1");
            var wheelCapRef = _referenceConverter.GetContentLink("WheelCap_1");

            if (viewModel.Variants == null)
            {
                viewModel.Variants = new List<CarVariation>
                {
                    _contentLoader.Get<CarVariation>(shirtRef),
                    _contentLoader.Get<CarVariation>(wheelCapRef)
                };
            }

            viewModel.PayMethods = PaymentManager.GetPaymentMethods(ContentLanguage.PreferredCulture.Name, true).PaymentMethod; //GetPaymentMethodsByMarket(_currentMarket.GetCurrentMarket().MarketId.Value).PaymentMethod;
            viewModel.CartItems = cart.GetAllLineItems();
            viewModel.CartTotal = cart.GetTotal();
            //viewModel.GiftCards = GiftCardService.GetCustomerGiftCards(CustomerContext.Current.CurrentContactId.ToString());

            viewModel.GiftCardsCustom = _giftCardService.GetAllGiftCards(); //GiftCardService.GetCustomerGiftCards(CustomerContext.Current.CurrentContactId.ToString());
            viewModel.ShippingMethods = ShippingManager.GetShippingMethodsByMarket(MarketId.Default.Value, false).ShippingMethod.ToList();
        }

        public ActionResult UpdateCart(int PurchaseQuantity, string itemCode)
        {
            var itemRef = _referenceConverter.GetContentLink(itemCode);
            var itemVar = _contentLoader.Get<CarVariation>(itemRef);

            var cart = _orderRepository.LoadOrCreateCart<ICart>(CustomerContext.Current.CurrentContactId, "Default");
            var lineItem = cart.GetAllLineItems().FirstOrDefault(x => x.Code == itemCode);

            if (lineItem == null)
            {
                lineItem = _orderGroupFactory.CreateLineItem(itemCode, cart);
                lineItem.Quantity = PurchaseQuantity;
                lineItem.PlacedPrice = itemVar.GetDefaultPrice().UnitPrice;
                if (itemVar.RequireSpecialShipping)
                {
                    IShipment specialShip = _orderGroupFactory.CreateShipment(cart);
                    specialShip.ShippingMethodId = GetShipMethodByParam(itemCode);
                    specialShip.ShippingAddress = GetOrderAddress(cart);
                    cart.AddShipment(specialShip);
                    cart.AddLineItem(specialShip, lineItem);
                }
                else
                {
                    var ship = cart.GetFirstShipment();
                    ship.ShippingAddress = GetOrderAddress(cart);

                    cart.AddLineItem(lineItem);
                }
            }
            else
            {
                var shipment = cart.GetFirstForm().Shipments.Where(s => s.LineItems.Contains(lineItem) == true).FirstOrDefault();
                cart.UpdateLineItemQuantity(shipment, lineItem, PurchaseQuantity);
            }

            _orderRepository.Save(cart);

            return RedirectToAction("Index");
        }

        private Guid GetShipMethodByParam(string paramCodeValue)
        {
            var paramRow = ShippingManager.GetShippingMethodsByMarket(MarketId.Default.Value, false).
                ShippingMethodParameter.Where(p => p.Value == paramCodeValue).FirstOrDefault();
            return paramRow.ShippingMethodId;
        }

        private Guid GetSipMethodByOptionParam(string itemCode)
        {
            string paramName = itemCode.Split('_')[0];
            ShippingMethodDto dto = ShippingManager.GetShippingMethodsByMarket(MarketId.Default.Value, false);
            Guid optionParam = dto.ShippingOptionParameter.Where(r => r.Parameter.Contains(paramName)).FirstOrDefault().ShippingOptionId;
            return dto.ShippingMethod.Where(r => r.ShippingOptionId == optionParam).FirstOrDefault().ShippingMethodId;
        }
        private IOrderAddress GetOrderAddress(IOrderGroup cart)
        {
            var shipAddress = _orderGroupFactory.CreateOrderAddress(cart);
            shipAddress.City = "Atlanta";
            shipAddress.CountryCode = "USA";
            shipAddress.CountryName = "United States";
            shipAddress.Id = "DemoShipAddress";

            return shipAddress;
        }



        public ActionResult SimulatePurchase(PaymentDemoViewModel viewModel)
        {
            var cart = _orderRepository.LoadOrCreateCart<ICart>(CustomerContext.Current.CurrentContactId, "Default");

            //Payment processing code goes here
            cart.GetFirstShipment().ShippingMethodId = viewModel.SelectedShippingMethodId;
            var primaryPayment = _orderGroupFactory.CreatePayment(cart);
            primaryPayment.PaymentMethodId = viewModel.SelectedPaymentId;
            primaryPayment.Amount = _orderGroupCalculator.GetTotal(cart).Amount;
            primaryPayment.PaymentMethodName = PaymentManager.GetPaymentMethod(viewModel.SelectedPaymentId).PaymentMethod[0].Name;
            if (viewModel.UseGiftCard)
            {
                var giftMethod = PaymentManager.GetPaymentMethodBySystemName("GiftCard", ContentLanguage.PreferredCulture.Name);
                var giftPayment = _orderGroupFactory.CreatePayment(cart);
                giftPayment.PaymentMethodId = giftMethod.PaymentMethod[0].PaymentMethodId;
                giftPayment.Amount = viewModel.GiftCardDebitAmt;
                giftPayment.ValidationCode = viewModel.RedemtionCode;
                giftPayment.PaymentMethodName = giftMethod.PaymentMethod[0].Name;

                PaymentProcessingResult giftPayResult = _paymentProcessor.ProcessPayment(cart, giftPayment, cart.GetFirstShipment());
                if (giftPayResult.IsSuccessful)
                {
                    primaryPayment.Amount -= giftPayment.Amount;
                    cart.AddPayment(giftPayment);
                }
                viewModel.GiftInfoMessage = giftPayResult.Message;
            }

            PaymentProcessingResult payResult = _paymentProcessor.ProcessPayment(cart, primaryPayment, cart.GetFirstShipment());

            if (payResult.IsSuccessful)
            {
                cart.AddPayment(primaryPayment);
                _orderRepository.SaveAsPurchaseOrder(cart);
                _orderRepository.Delete(cart.OrderLink);
            }
            viewModel.MessageOutput = payResult.Message;


            InitializeModel(viewModel);

            return View("Index", viewModel);
        }
    }

    //public static class CustomHelpers
    //{
    //    public static IHtmlString UrlResolver(this HtmlHelper helper, IAssetContainer asset)
    //    {
    //        var resolver = ServiceLocator.Current.GetInstance<AssetUrlResolver>();
    //        var Url = resolver.GetAssetUrl(asset);
    //        return new MvcHtmlString(Url);
    //    }
    //}
}