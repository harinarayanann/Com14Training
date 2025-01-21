using EPiServer.Security;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Security;
using Foundation.Models.Pages;
using Foundation.Models.ViewModels;
using EPiServer.Commerce.Order;
using Foundation.Features.Checkout.Services;
using Foundation.Features.Home;

namespace Foundation.Controllers
{
    public class CTCartController : PageController<CartPage>
    {
        
        private const string DefaultCartName = "Default";

        private readonly IOrderRepository _orderRepository;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IPromotionEngine _promotionEngine;
        private readonly IContentLoader _contentLoader;
        private readonly ILineItemCalculator _lineItemCalculator;
        private readonly IInventoryProcessor _inventoryProcessor;
        private readonly ILineItemValidator _lineItemValidator;
        private readonly IPlacedPriceProcessor _placedPriceProcessor;
        private readonly ICartService _cartService;

        public CTCartController(IOrderRepository orderRepository
            , IOrderGroupFactory orderGroupFactory
            , IOrderGroupCalculator orderGroupCalculator
            , IContentLoader contentLoader
            , ILineItemCalculator lineItemCalculator
            , IPlacedPriceProcessor placedPriceProcessor
            , IInventoryProcessor inventoryProcessor
            , ILineItemValidator lineItemValidator
            , IPromotionEngine promotionEngine
            , ICartService cartService)
        {
            _orderRepository = orderRepository;
            _orderGroupFactory = orderGroupFactory;
            _orderGroupCalculator = orderGroupCalculator;
            _contentLoader = contentLoader;
            _promotionEngine = promotionEngine;
            _lineItemCalculator = lineItemCalculator;
            _inventoryProcessor = inventoryProcessor;
            _lineItemValidator = lineItemValidator;
            _placedPriceProcessor = placedPriceProcessor;
            _cartService = cartService;
        }
        public ActionResult Index(CartPage currentPage)
        {
            // ToDo: (exercise D2)
            var cart = _orderRepository.LoadCart<ICart>(
                GetContactId()
                , DefaultCartName);

            // a few different ways of loading
            var cart1 = _orderRepository.Load<ICart>(GetContactId(), DefaultCartName).FirstOrDefault();
            var cart2 = _orderRepository.LoadCart<ICart>(GetContactId(), DefaultCartName);
            //var cart3 = _orderRepository.Load(); // all IOrderGroups for current user... 8 overloads
            var cart3 = _cartService.LoadOrCreateCart(_cartService.DefaultCartName);

            if (cart3 == null) 
            {
                return View("NoCart"); // ...do this nicer
            }
            else
            {
                string warningMessages = ValidateCart(cart3);

                if (String.IsNullOrEmpty(warningMessages))
                {
                    warningMessages += "No messages";
                }

                var model = new CartViewModel
                {
                    LineItems = cart3.GetAllLineItems(), // Extension method
                    SubTotal = _orderGroupCalculator.GetSubTotal(cart3),
                    WarningMessage = warningMessages,
                    PromotionMessage = GetPromotions(cart3)
                };

                _orderRepository.Save(cart3);

                return View("index", model);
            }


            // The below is a dummy, remove when lab D2 is done
            //return null;
        }

        public string GetPromotions(ICart cart)
        {
            String str = String.Empty;
            var rewardDescriptions = _promotionEngine.Run(cart).ToList();
            rewardDescriptions.ForEach(r => str += r.Description);

            // example with Coupons in QS

            return str;
        }

        public ActionResult Checkout()
        {
            // Final steps and go to checkout
            HomePage home = _contentLoader.Get<HomePage>(ContentReference.StartPage);
            ContentReference theRef = home.Settings.checkoutPage;
            string passingValue = "Coding is fun"; // could pass something of the cart instead 
            return RedirectToAction("Index", "CTCheckOut", new { node = theRef, passedAlong = passingValue });

        }

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

        protected static CustomerContact GetContact()
        {
            return CustomerContext.Current.GetContactById(GetContactId());
        }

        protected static Guid GetContactId()
        {
            return PrincipalInfo.CurrentPrincipal.GetContactId();
        }

    }
}