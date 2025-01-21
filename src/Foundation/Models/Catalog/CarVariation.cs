using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.SpecializedProperties;
using Foundation.Features.Blocks.ElevatedRoleBlock;
using Foundation.Features.CatalogContent;
using Foundation.Features.CatalogContent.Variation;
using Foundation.Infrastructure.Commerce.Models.EditorDescriptors;

namespace Foundation.Models.Catalog
{
    [CatalogContentType(DisplayName = "Car Variant", GUID = "6582C432-B04E-4C0E-A4A0-FAC49CF51283", 
        Description = "Car variant supports multiple variation types")]
    [ImageUrl("/icons/cms/pages/CMS-icon-page-23.png")]
    public class CarVariation : VariationContent, IProductRecommendations, IFoundationContent/*, IDashboardItem*/
    {
        #region Car Properties
        [CultureSpecific]
        [Display(Name = "Waiting Period",
        GroupName = SystemTabNames.Content,
        Order = 80)]
        public virtual string WaitingPeriod { get; set; }
        #endregion

        #region Generic ProductProperties
        [Tokenize]
        [Searchable]
        [IncludeInDefaultSearch]
        [BackingType(typeof(PropertyString))]
        [Display(Name = "Size", Order = 5)]
        public virtual string Size { get; set; }

        [Tokenize]
        [Searchable]
        [CultureSpecific]
        [IncludeInDefaultSearch]
        [BackingType(typeof(PropertyString))]
        [Display(Name = "Color", Order = 10)]
        public virtual string Color { get; set; }

        [Tokenize]
        [Searchable]
        [CultureSpecific]
        [IncludeInDefaultSearch]
        [Display(Name = "Description", Order = 15)]
        public virtual XhtmlString Description { get; set; }

        [CultureSpecific]
        [Display(Name = "Content area", Order = 20)]
        [AllowedTypes(new[] { typeof(IContentData) })]
        public virtual ContentArea ContentArea { get; set; }

        [CultureSpecific]
        [Display(Name = "Associations title", Order = 25)]
        public virtual string AssociationsTitle { get; set; }

        [CultureSpecific]
        [Display(Name = "Show recommendations", Order = 30)]
        public virtual bool ShowRecommendations { get; set; }

        [Required]
        [Display(Name = "Virtual product mode", Order = 35)]
        [SelectOne(SelectionFactoryType = typeof(VirtualVariantTypeSelectionFactory))]
        public virtual string VirtualProductMode { get; set; }

        [Display(Name = "Virtual product role", Order = 40)]
        [SelectOne(SelectionFactoryType = typeof(ElevatedRoleSelectionFactory))]
        [BackingType(typeof(PropertyString))]
        public virtual string VirtualProductRole { get; set; }

        #region Manufacturer

        [Display(Name = "Mpn", GroupName = Infrastructure.TabNames.Manufacturer, Order = 5)]
        [BackingType(typeof(PropertyString))]
        public virtual string Mpn { get; set; }

        [Display(Name = "Package quantity", GroupName = Infrastructure.TabNames.Manufacturer, Order = 10)]
        [BackingType(typeof(PropertyString))]
        public virtual string PackageQuantity { get; set; }

        [Display(Name = "Part number", GroupName = Infrastructure.TabNames.Manufacturer, Order = 15)]
        [BackingType(typeof(PropertyString))]
        public virtual string PartNumber { get; set; }

        [Display(Name = "Region code", GroupName = Infrastructure.TabNames.Manufacturer, Order = 20)]
        [BackingType(typeof(PropertyString))]
        public virtual string RegionCode { get; set; }

        [Display(Name = "Sku", GroupName = Infrastructure.TabNames.Manufacturer, Order = 25)]
        [BackingType(typeof(PropertyString))]
        public virtual string Sku { get; set; }

        [Display(Name = "Subscription length", GroupName = Infrastructure.TabNames.Manufacturer, Order = 30)]
        [BackingType(typeof(PropertyString))]
        public virtual string SubscriptionLength { get; set; }

        [Display(Name = "Upc", GroupName = Infrastructure.TabNames.Manufacturer, Order = 35)]
        [BackingType(typeof(PropertyString))]
        public virtual string Upc { get; set; }

        #endregion

        #region Implement IFoundationContent

        [CultureSpecific]
        [Display(Name = "Hide site header", GroupName = Infrastructure.TabNames.Settings, Order = 100)]
        public virtual bool HideSiteHeader { get; set; }

        [CultureSpecific]
        [Display(Name = "Hide site footer", GroupName = Infrastructure.TabNames.Settings, Order = 200)]
        public virtual bool HideSiteFooter { get; set; }

        [Display(Name = "CSS files", GroupName = Infrastructure.TabNames.Styles, Order = 100)]
        public virtual LinkItemCollection CssFiles { get; set; }

        [Display(Name = "CSS", GroupName = Infrastructure.TabNames.Styles, Order = 200)]
        [UIHint(UIHint.Textarea)]
        public virtual string Css { get; set; }

        [Display(Name = "Script files", GroupName = Infrastructure.TabNames.Scripts, Order = 100)]
        public virtual LinkItemCollection ScriptFiles { get; set; }

        [UIHint(UIHint.Textarea)]
        [Display(GroupName = Infrastructure.TabNames.Scripts, Order = 200)]
        public virtual string Scripts { get; set; }

        #endregion

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);

            VirtualProductMode = "None";
            VirtualProductRole = "None";
            AssociationsTitle = "You May Also Like";
        }

        #endregion

        #region Training Properties
                [CultureSpecific]
        [IncludeInDefaultSearch]
        [Searchable]
        [Tokenize]
        public virtual XhtmlString MainBody { get; set; }

        public virtual bool CanBeMonogrammed { get; set; }

        [Searchable]
        [Tokenize]
        [IncludeValuesInSearchResults]
        public virtual string ThematicTag { get; set; }
        #endregion
    }
}