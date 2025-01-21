using EPiServer.ContentApi.Commerce.Internal.Services;
using Foundation.Features.Checkout.Services;
using Foundation.Features.Checkout.ViewModels;
using Foundation.Features.NamedCarts;
using Foundation.Infrastructure.Personalization;
using Foundation.Models.Pages;
using Foundation.Services;

namespace Foundation.Controllers
{
    public class CTDefaultCartController : PageController<CartPage>
    {
        private CartWithValidationIssues _cart;
        private readonly ICartService _cartService;
        private readonly IOrderRepository _orderRepository;
        private readonly IContentLoader _contentLoader;
        private readonly ICommerceTrackingService _recommendationService;
        private readonly ReferenceConverter _referenceConverter;
        private readonly CartViewModelFactory _cartViewModelFactory;
        private const string b2cMinicart = "/Features/Shared/Views/Header/_HeaderCart.cshtml";

        public CTDefaultCartController(
            ICartService cartService,
            IOrderRepository orderRepository,
            IContentLoader contentLoader,
            ICommerceTrackingService recommendationService,
            ReferenceConverter referenceConverter,
            CartViewModelFactory cartViewModelFactory) {
            this._cartService = cartService;
            this._orderRepository = orderRepository;
            this._contentLoader = contentLoader;
            this._recommendationService = recommendationService;
            this._referenceConverter = referenceConverter;
            this._cartViewModelFactory = cartViewModelFactory;
        }
        private CartWithValidationIssues CartWithValidationIssues => _cart ?? (_cart = _cartService.LoadCart(_cartService.DefaultCartName, true));

        public IActionResult Index()
        {
            return View();
        }

        [AcceptVerbs(new string[] { "GET", "POST" })]
        public ActionResult MiniCartDetails()
        {
            var viewModel = _cartViewModelFactory.CreateMiniCartViewModel(CartWithValidationIssues.Cart);
            return PartialView(b2cMinicart, viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> AddToCart([FromBody] RequestParamsToCart param)
        {
            var warningMessage = string.Empty;

            ModelState.Clear();

            if (CartWithValidationIssues.Cart == null)
            {
                _cart = new CartWithValidationIssues
                {
                    Cart = _cartService.LoadOrCreateCart(_cartService.DefaultCartName),
                    ValidationIssues = new Dictionary<ILineItem, List<ValidationIssue>>()
                };
            }

            var result = _cartService.AddToCart(CartWithValidationIssues.Cart, param);

            if (result.EntriesAddedToCart)
            {
                _orderRepository.Save(CartWithValidationIssues.Cart);
                await _recommendationService.TrackCart(HttpContext, CartWithValidationIssues.Cart);
                if (string.Equals(param.RequestFrom, "axios", StringComparison.OrdinalIgnoreCase))
                {
                    var product = "";
                    var entryLink = _referenceConverter.GetContentLink(param.Code);
                    var entry = _contentLoader.Get<EntryContentBase>(entryLink);
                    if (entry is BundleContent || entry is PackageContent)
                    {
                        product = entry.DisplayName;
                    }
                    else
                    {
                        var parentProduct = _contentLoader.Get<EntryContentBase>(entry.GetParentProducts().FirstOrDefault());
                        product = parentProduct?.DisplayName;
                    }

                    if (result.ValidationMessages.Count > 0)
                    {
                        return Json(new ChangeCartJsonResult
                        {
                            StatusCode = result.EntriesAddedToCart ? 1 : 0,
                            CountItems = (int)CartWithValidationIssues.Cart.GetAllLineItems().Sum(x => x.Quantity),
                            Message = product + " is added to the cart successfully.\n" + result.GetComposedValidationMessage(),
                            SubTotal = CartWithValidationIssues.Cart.GetSubTotal()
                        });
                    }

                    return Json(new ChangeCartJsonResult
                    {
                        StatusCode = result.EntriesAddedToCart ? 1 : 0,
                        CountItems = (int)CartWithValidationIssues.Cart.GetAllLineItems().Sum(x => x.Quantity),
                        Message = product + " is added to the cart successfully.",
                        SubTotal = CartWithValidationIssues.Cart.GetSubTotal()
                    });
                }

                return MiniCartDetails();
            }

            return StatusCode(500, result.GetComposedValidationMessage());
        }

    }
}
