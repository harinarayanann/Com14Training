using Foundation.Models.Catalog;
using Foundation.Models.ViewModels;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Find.Commerce;
using EPiServer.Find.Framework;

namespace Foundation.Controllers
{
    public class FindSearchDemoController : Controller
    {
        // GET: FindSearchDemo
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FindQueryIntegrated(string keyWord)
        {
            var viewModel = new FindResultViewModel
            {
                SearchText = keyWord
            };

            IClient client = SearchClient.Instance;


            var result = client.Search<CarVariation>()
                .For(keyWord)
                //.Filter(x => x.InStockQuantityLessThan(100))
                .Take(50)
                .FilterOnLanguages(new string[] { "en" })
                .TermsFacetFor(x => x.Color)
                .GetContentResult();

            viewModel.ColorFacets = result.TermsFacetFor(x => x.Color).Terms.ToList();

            viewModel.ResultCount = result.TotalMatching.ToString();
            viewModel.CarVariants = result.ToList();

            return View(viewModel);
        }

        public ActionResult FacetFilteredSearch(string keyWord, string facet)
        {
            var viewModel = new FindResultViewModel
            {
                SearchText = keyWord
            };

            IClient client = SearchClient.Instance;

            var result = client.Search<CarVariation>()
                .For(keyWord)
                .Filter(x => x.Color.Match(facet))
                .Take(50)
                .FilterOnLanguages(new string[] { "en" })
                .TermsFacetFor(x => x.Color)
                .GetContentResult();

            viewModel.ColorFacets = result.TermsFacetFor(x => x.Color).Terms.ToList();

            viewModel.ResultCount = result.TotalMatching.ToString();
            viewModel.CarVariants = result.ToList();

            return View("FindQueryIntegrated", viewModel);
        }

        public ActionResult ProjectionResult(string keyWord)
        {
            var viewModel = new ProjectedCarViewModel()
            {
                SearchText = keyWord
            };

            IClient client = SearchClient.Instance;

            var result = client.Search<CarVariation>()
                .For(keyWord)
                .Take(50)
                .FilterOnLanguages(new string[] { "en" })
                .Select(x => new ProjectedCar
                {
                    Name = x.Name,
                    Color = x.Color,
                    Brand = x.PartNumber,
                    UrlLink = x.ContentLink
                })
                .GetResult();

            viewModel.Cars = result.ToList();

            viewModel.ResultCount = result.TotalMatching.ToString();

            return View(viewModel);
        }
    }
}
//