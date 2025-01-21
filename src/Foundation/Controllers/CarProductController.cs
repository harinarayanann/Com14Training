using EPiServer.Framework.DataAnnotations;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.Linking;
using Microsoft.AspNetCore.Mvc.Rendering;
using Foundation.Models.Catalog;
using Foundation.Models.ViewModels;

namespace Foundation.Controllers
{

    [TemplateDescriptor(Default =true)]
    public class CarProductController : CatalogControllerBase<CarProduct>
    {
        
        private readonly IRelationRepository _relationRepository;

        public CarProductController(IContentLoader contentLoader, UrlResolver urlResolver, AssetUrlResolver assetUrlResolver, ThumbnailUrlResolver thumbnailUrlResolver, IRelationRepository relationRepository)
            : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver)
        {
            _relationRepository = relationRepository;
        }

        public ActionResult Index(CarProduct currentContent, string size = null, string color = null)
        {
            var variants = currentContent.GetVariants(_relationRepository)
                .Select(contentLink => _contentLoader.Get<CarVariation>(contentLink));
            var sizes = variants.Select(variant => variant.Size).Distinct();
            var colors = variants.Select(variant => variant.Color).Distinct();
            string imgUrl = _assetUrlResolver.GetAssetUrl(currentContent);

            if (!string.IsNullOrEmpty(size))
            {
                variants = variants.Where(variant => variant.Size == size);
            }

            if (!string.IsNullOrEmpty(color))
            {
                variants = variants.Where(variant => variant.Color == color);
            }

            var model = new CarProductViewModel
            {
                CurrentContent = currentContent,
                VariantLinks = variants.Select(variant => variant.ContentLink),
                ImageUrl = _assetUrlResolver.GetAssetUrl(currentContent),
                Sizes = sizes.Select(s => new SelectListItem { Text = s, Value = s, Selected = s == size }),
                Colors = colors.Select(c => new SelectListItem { Text = c, Value = c, Selected = c == color })
            };

            return View(model);
        }

    }


}