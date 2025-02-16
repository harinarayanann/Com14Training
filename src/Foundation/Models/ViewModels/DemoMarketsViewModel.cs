using Foundation.Models.Catalog;
using Mediachase.Commerce.Orders;

namespace Foundation.Models.ViewModels
{
    public class DemoMarketsViewModel
    {
        public IMarket SelectedMarket { get; set; }
        public IEnumerable<IMarket> MarketList { get; set; }
        public CarVariation Shirt { get; set; }
        public Money? TaxAmount { get; set; }
        public Money? TaxAmountOldSchool { get; set; }
        public IEnumerable<TaxValue> Taxes { get; set; }
        public IEnumerable<IPriceValue> OptimizedPrices { get; set; }
        public IEnumerable<IPriceValue> FilteredPrices { get; set; }
        public decimal PromotionsTotal { get; set; }
        public Money SellingPrice { get; set; }
        public bool PromotionsApplied { get; set; } = false;
    }
}