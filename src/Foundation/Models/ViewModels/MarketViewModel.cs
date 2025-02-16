using Microsoft.AspNetCore.Mvc.Rendering;

namespace Foundation.Models.ViewModels
{
    public class MarketViewModel
    {

        public IEnumerable<SelectListItem> Markets { get; set; }
        public string MarketId { get; set; }
        public ContentReference ContentLink { get; set; }
    }
}