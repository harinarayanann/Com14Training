using EPiServer.Commerce;

namespace Foundation.Models.Blocks
{
    [ContentType(DisplayName = "SettingsBlock", GUID = "fdfd33be-91ca-4366-a3ac-ea126c66f0e7", Description = "")]
    public class SettingsBlock : BlockData
    {
        [UIHint(EPiServer.Commerce.UIHint.CatalogContent)]
        public virtual ContentReference topCategory { get; set; }

        public virtual ContentReference cartPage { get; set; }

        public virtual ContentReference checkoutPage { get; set; }

        public virtual ContentReference orderPage { get; set; }

        public virtual ContentReference catalogStartPageLink { get; set; }

        //public virtual bool InitializedOrNot { get; set; } wait with this
    }
}