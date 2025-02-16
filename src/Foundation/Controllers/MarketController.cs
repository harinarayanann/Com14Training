//using EPiServer.Reference.Commerce.Site.Features.Market.Models;
//using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using Mediachase.Commerce.Markets;
using Microsoft.AspNetCore.Mvc.Rendering;
using Foundation.Models.ViewModels;

namespace Foundation.Controllers
{
    public class MarketController : Controller
    {
        private readonly IMarketService _marketService;
        private readonly ICurrentMarket _currentMarket;
        private readonly UrlResolver _urlResolver;

        public MarketController(IMarketService marketService, ICurrentMarket currentMarket, UrlResolver urlResolver) //, //LanguageService languageService)
        {
            _marketService = marketService;
            _currentMarket = currentMarket;
            _urlResolver = urlResolver;
        }

        //[ChildActionOnly]
        public ActionResult Index(ContentReference contentLink)
        {
            var model = new MarketViewModel
            {
                Markets = _marketService.GetAllMarkets().Where(x => x.IsEnabled).OrderBy(x => x.MarketName)
                    .Select(x => new SelectListItem
                    {
                        Selected = false,
                        Text = x.MarketName,
                        Value = x.MarketId.Value
                    }),
                MarketId = _currentMarket.GetCurrentMarket().MarketId.Value,
                ContentLink = contentLink
            };
            return PartialView(model);
        }
    }
}