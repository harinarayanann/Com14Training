using Microsoft.AspNetCore.Mvc.Rendering;

namespace Foundation.Models.ViewModels
{
    public class CarProductViewModel
    {
        public ProductContent CurrentContent { get; set; }

        public string ImageUrl { get; set; }

        public IEnumerable<ContentReference> VariantLinks { get; set; }

        public IEnumerable<SelectListItem> Sizes { get; set; }

        public IEnumerable<SelectListItem> Colors { get; set; }
    }
}