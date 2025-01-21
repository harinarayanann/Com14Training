using EPiServer.ServiceApi.Commerce.Models.Catalog;
using Mediachase.Commerce.InventoryService;

namespace Foundation.Models.ViewModels
{
    public class CarVariationViewModel
    {
        //public IEnumerable<RewardDescription> rewards { get; set; }

        public string discountString { get; set; }
        public decimal discountPrice { get; set; }
        public string priceString { get; set; }
        public string image { get; set; }
        public string name { get; set; }
        public string code { get; set; }

        public string url { get; set; }
        public bool CanBeMonogrammed { get; set; }
        public XhtmlString MainBody { get; set; }

        // not in the Fund. course... check this
        public ContentArea ProductArea { get; set; }
        public IEnumerable<WarehouseInventory> WHOldSchool { get; set; } // not using custom WarehouseInfo-class... yet
        public IEnumerable<InventoryRecord> WHNewSchool { get; set; }

    }
}