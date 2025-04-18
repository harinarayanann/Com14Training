using EPiServer.Commerce.Marketing.Promotions;

namespace Foundation.Models.Promotions
{
    [ContentType(DisplayName = "FreeStuffPromotion", 
        GUID = "D0485C5E-6353-41E5-828E-67C09D535CC7", 
        Description = "Free Stuff Promotion")]
    public class FreeStuffPromotion: EntryPromotion
    {
        [PromotionRegion(PromotionRegionName.Condition)]
        [Display(Order = 20)]
        public virtual PurchaseQuantity RequiredQty { get; set; }

        [PromotionRegion(PromotionRegionName.Reward)]
        [Display(Order = 30)]
        public virtual IList<ContentReference> FreeItem { get; set; }

    }
}
