
using EPiServer.Commerce.Catalog;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Customers;
using EPiServer.Security;
using Mediachase.Commerce.Security; // for ext-m. on CurrentPrincipal
using EPiServer.Commerce.Catalog.Linking;
using Foundation.Models.Catalog;
using EPiServer.Commerce.Shell.Catalog;
using EPiServer.Find.Commerce.Services.Internal;
using Foundation.Features.Home;
using EPiServer.Globalization;
using Foundation.Models.Pages;
using Foundation.Models.ViewModels;

namespace Foundation.Controllers
{
    public class CarVariationController : CatalogControllerBase<CarVariation>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderGroupFactory _orderFactory;
        private readonly ILineItemValidator _lineItemValidator;
        // add for promos
        private readonly IPromotionEngine _promotionEngine;
        private readonly ICurrentMarket _currentMarket;

        // ToDo: (Exewrcise C6)
        public CarVariationController(
            IContentLoader contentLoader
            , UrlResolver urlResolver
            , AssetUrlResolver assetUrlResolver
            , ThumbnailUrlResolver thumbnailUrlResolver // use this in node listing instead
            , IOrderRepository orderRepository
            , IOrderGroupFactory orderFactory
            , ILineItemValidator lineItemValidator
            // add for promo-price
            , IPromotionEngine promotionEngine
            , ICurrentMarket currentMarket
            )
            : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver)
        {
            _orderRepository = orderRepository; // AddToCart & AddToWishList
            _orderFactory = orderFactory; // AddToCart
            _lineItemValidator = lineItemValidator; // AddToCart

            // added for promos
            _promotionEngine = promotionEngine;
            _currentMarket = currentMarket;
        }

        // should go in the .ctor 
        //Injected<ReferenceConverter> _refConv; // can do like this, but .ctor is better
        //Injected<ILinksRepository> _linksRep; Obsoleted
        //Injected<ReadOnlyPricingLoader> _readOnlyPricingLoader;

        public ActionResult Index(CarVariation currentContent)
        {
            LoadingExamples(currentContent);

            // Dont't do like this
            //var myVariable = ServiceLocator.Current
            //    .GetInstance<IPriceDetailService>();

            // Don't use this
            //CatalogContentLoader ccc = new CatalogContentLoader();
            //ccc.GetCatalogEntries()
            
            var currM = _currentMarket.GetCurrentMarket();
            var link = currentContent.ContentLink;
            //Price price = _readOnlyPricingLoader.Service.GetDefaultPrice(link, currM.MarketId, currM.DefaultCurrency, DateTime.UtcNow);

            // just checking
            //IEnumerable<DiscountedEntry> entries =
            //    _promotionEngine.GetDiscountPrices(
            //    currentContent.ContentLink, _currentMarket.GetCurrentMarket());

            Decimal theDiscount = 0;
            string discountString = String.Empty;

            // ...can also get the price direct by _promotionEngine.GetDiscountPrices(....) (above)
            List<RewardDescription> rewards = new List<RewardDescription>();
            rewards = _promotionEngine.Evaluate(currentContent.ContentLink).ToList();
            if (rewards.Count != 0)
            {
                theDiscount = rewards.First().SavedAmount; // just take one to have a look
                discountString = rewards.First().Description;
                discountString += " : ";
                discountString += rewards.First().Promotion.Description;
            }
            else
            {
                discountString = "...no discount";
            }

            
            var model = new Foundation.Models.ViewModels.CarVariationViewModel
            {
                MainBody = currentContent.MainBody,
                priceString = currentContent.GetDefaultPrice().UnitPrice.Amount.ToString("C"),

                discountString = discountString,
                discountPrice = currentContent.GetDefaultPrice().UnitPrice.Amount - theDiscount, // could be other than "Default"

                image = GetDefaultAsset(currentContent),
                CanBeMonogrammed = currentContent.CanBeMonogrammed,
                name = currentContent.Name,
                code = currentContent.Code,

            };

            return View(model);
        }

        // should go in the .ctor
        Injected<ReadOnlyPricingLoader> roPriceLoader; // Note: the "optimized"
        Injected<PricingLoader> rwPriceLoader; // just to show
                                               //Injected<ICurrentMarket> currentMarketService;
                                               //Injected<ReferenceConverter> _refConv;


        // Fund: Pricing Extensions 
        //private void CheckPrices(VariationContent currentContent)
        //{
        //    // Don't want to see the below lines
        //    //StoreHelper.GetBasePrice(currentContent.LoadEntry());
        //    // StoreHelper.GetSalePrice(currentContent.LoadEntry(), 11, theMarket, new Currency("USD")); // Get currency from market or thread... or something

        //    IMarket theMarket = _currentMarket.GetCurrentMarket();

        //    var priceRef = currentContent.PriceReference; // a ContentReference
        //    var gotPrices = currentContent.GetPrices(); // Gets all, recieve "Specialized" Price/ItemCollection
        //    var defaultPrice = currentContent.GetDefaultPrice(); // All Cust + qty 0 ... market sensitive
        //    var custSpecificPrices = currentContent.GetCustomerPrices();

        //    var PriceCheck = (IPricing)currentContent; // null if not a SKU

        //    var p1 = roPriceLoader.Service.GetPrices(
        //        currentContent.ContentLink
        //        , MarketId.Default
        //        , new CustomerPricing(CustomerPricing.PriceType.PriceGroup, "VIP"));
        //    // arbitrary Price-Group, could read 
        //    // ...CustomerContext.Current.CurrentContact.EffectiveCustomerGroup;

        //    var p2 = roPriceLoader.Service.GetCustomerPrices(
        //        currentContent.ContentLink
        //        , theMarket.DefaultCurrency // ...or something
        //        , 8M
        //        , true); // bool - return customer pricing

        //    // Loader examples "Infrastructure/PriceCalculator"
        //}

        // different loading examples and further extension methods 
        private void LoadingExamples(CarVariation currentContent)
        {
            // AdminPageController have demo of ReferenceConverter

            #region Catalog

            ContentReference parent = currentContent.ParentLink; //...from "me" as the variation
            // note: in 11 --> more strict

            //var x = base._contentLoader.Get<EntryContentBase>(parent);
            var y = base._contentLoader.Get<NodeContentBase>(parent); // gets the ShirtNodeProxy

            IEnumerable<EntryContentBase> children =
                base._contentLoader.GetChildren<EntryContentBase>(parent);

            IEnumerable<ContentReference> allLinks = currentContent.GetCategories(); // Relations
            IEnumerable<Relation> nodes = currentContent.GetNodeRelations(); // older, avoid

            var theType = currentContent.GetOriginalType(); // handy
            var proxy = currentContent.GetType(); // gives the CastleProxy

            IEnumerable<ContentReference> prodParents = currentContent.GetParentProducts();
            IEnumerable<ContentReference> parentPackages = currentContent.GetParentPackages();
            IEnumerable<ContentReference> allParents = currentContent.GetParentEntries(); // newer

            IMarket market = _currentMarket.GetCurrentMarket(); // Gets "DEFAULT" if not "custom"
            bool available = currentContent.IsAvailableInMarket(market.MarketId); // if we want to know about another market
            bool available2 = currentContent.IsAvailableInCurrentMarket();

            //ISecurityDescriptor sec = currentContent.GetSecurityDescriptor();

            CatalogEntryResponseGroup respG = new CatalogEntryResponseGroup(
                   CatalogEntryResponseGroup.ResponseGroup.CatalogEntryFull);

            // Finally in 12 we get squiggles :)
            // old school, not needed after 9.19 ... avoid. 
            // ...this is why we have the extension method
            //Mediachase.Commerce.Catalog.Objects.Entry entry =
            //    currentContent.LoadEntry(); // Consider RG

            // the IoC-way to get the above, but use the .ctor not do like this...
            ICatalogSystem catSys = ServiceLocator.Current.GetInstance<ICatalogSystem>();
            // catSys

            //Entry shouldNotUseThis = catSys.GetCatalogEntry(2, respG);
            //var p = shouldNotUseThis.PriceValues; // still populating from price-service, from ECF R3

            // native ECF, just to have a look, still used a lot ... can be handy
            var entryDto = CatalogContext.Current.GetCatalogEntryDto // singular
                (currentContent.Code, respG);
            var x = entryDto.SalePrice; // not used anymore, zero prices back

            // used a lot previously, in combination with the search provider model
            // we can use "search" to easily get an array of "ints" back.
            var entryDto2 = CatalogContext.Current.GetCatalogEntriesDto // Plural, check the overloads
                (new int[] { 2, 3, 4, 5, 6 }, respG);

            #endregion


            #region Orders

            //var p0 = OrderContext.Current.FindActiveOrders(); // InProgress & Partially shipped
            ////OrderContext.Current.FindPurchaseOrders(); // Not fun... ref Shannons blog (carts too)
            //var p1 = OrderContext.Current.FindPurchaseOrdersByStatus(OrderStatus.AwaitingExchange); // array of statuses as arg
            //var p2 = OrderContext.Current.GetPurchaseOrders(new Guid()); // ContactGuid
            //var p3 = OrderContext.Current.GetPurchaseOrder(-1); // TrackingNo or DB-PK

            //var po0 = _orderRepository.Load(); // all for current user - only POs
            //var po1 = _orderRepository.Load(CustomerContext.Current.CurrentContactId); // Only POs
            //var po2 = _orderRepository.Load(CustomerContext.Current.CurrentContactId, "WishList");
            //var po3 = _orderRepository.Load<ICart>();
            //var po4 = _orderRepository.Load<IPurchaseOrder>(21);

            #endregion

        }

        // RoCe: move to .ctor
        Injected<IPlacedPriceProcessor> _placedPriceProcessor;
        Injected<IInventoryService> _invService;
        Injected<IWarehouseRepository> _whRep;

        [HttpPost]
        public ActionResult AddToCart(CarVariationViewModel currentContent, decimal Quantity, string Monogram)
        {
            // LoadOrCreateCart - in EPiServer.Commerce.Order.IOrderRepositoryExtensions
            var cart = _orderRepository.LoadOrCreateCart<ICart>(
                PrincipalInfo.CurrentPrincipal.GetContactId(), "Default");

            // ToDo: (lab D1) add a LineItem to the Cart



            // if we want to redirect
            ContentReference cartRef = _contentLoader.Get<HomePage>(ContentReference.StartPage).Settings.cartPage;
            CartPage cartPage = _contentLoader.Get<CartPage>(cartRef);
            var name = cartPage.URLSegment;
            var lang = ContentLanguage.PreferredCulture;
            string passingValue = cart.Name;

            // go to the cart page, if needed

            return RedirectToAction("Index", lang + "/" + name, new { passedAlong = passingValue });

            //return HttpStatusCode.OK;
        }


        public void AddToWishList(CarVariation currentContent)
        {

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