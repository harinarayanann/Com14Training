using EPiServer.Cms.Shell.UI.ObjectEditing.EditorDescriptors.SelectionFactories;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.SpecializedProperties;
using Foundation.Features.CatalogContent;
using Foundation.Features.CatalogContent.Product;
using Foundation.Infrastructure.Commerce.Models.EditorDescriptors;

namespace Foundation.Models.Catalog
{
    [CatalogContentType(
        GUID = "4E94E37E-23C1-4001-89BE-12A7ECF522D4",
        MetaClassName = "CarProduct",
        DisplayName = "Car Product",
        Description = "Car product supports mutiple products")]
    [ImageUrl("/icons/cms/pages/CMS-icon-page-23.png")]
    public class CarProduct : ProductContent, IProductRecommendations, IFoundationContent/*, IDashboardItem*/
    {
        #region Car Properties
        [CultureSpecific]
        [SelectMany(SelectionFactoryType = typeof(CarColorOptionSelectionFactory))]
        public virtual string MultipleLanguage { get; set; }

        [CultureSpecific]
        [Display(Name = "Product teaser", GroupName = SystemTabNames.Content, Order = 10)]
        public virtual XhtmlString ProductTeaser { get; set; }
        #endregion

        #region Content
        [Searchable]
        [CultureSpecific]
        [Tokenize]
        [IncludeInDefaultSearch]
        [Display(Name = "Sizing", GroupName = SystemTabNames.Content, Order = 5)]
        public virtual XhtmlString Sizing { get; set; }

        [Searchable]
        [CultureSpecific]
        [Tokenize]
        [IncludeInDefaultSearch]
        [BackingType(typeof(PropertyString))]
        [Display(Name = "Brand", GroupName = SystemTabNames.Content, Order = 15)]
        public virtual string Brand { get; set; }

        [CultureSpecific]
        [BackingType(typeof(PropertyString))]
        [Display(Name = "Department", GroupName = SystemTabNames.Content, Order = 20)]
        public virtual string Department { get; set; }

        [Searchable]
        [CultureSpecific]
        [Tokenize]
        [IncludeInDefaultSearch]
        [Display(Name = "Description", GroupName = SystemTabNames.Content, Order = 25)]
        public virtual XhtmlString Description { get; set; }

        [CultureSpecific]
        [Display(Name = "Legal disclaimer", GroupName = SystemTabNames.Content, Order = 30)]
        public virtual string LegalDisclaimer { get; set; }

        [CultureSpecific]
        [BackingType(typeof(PropertyString))]
        [Display(Name = "Product group", GroupName = SystemTabNames.Content, Order = 35)]
        public virtual string ProductGroup { get; set; }

        [CultureSpecific]
        [BackingType(typeof(PropertyString))]
        [Display(Name = "Product type name", GroupName = SystemTabNames.Content, Order = 40)]
        public virtual string ProductTypeName { get; set; }

        [CultureSpecific]
        [BackingType(typeof(PropertyString))]
        [Display(Name = "Product type sub category", GroupName = SystemTabNames.Content, Order = 45)]
        public virtual string ProductTypeSubcategory { get; set; }

        [Display(Name = "On sale",
            GroupName = SystemTabNames.Content,
            Order = 50)]
        public virtual bool OnSale { get; set; }

        [Display(Name = "New arrival",
            GroupName = SystemTabNames.Content,
            Order = 55)]
        public virtual bool NewArrival { get; set; }

        [Searchable]
        [CultureSpecific]
        [Tokenize]
        [IncludeInDefaultSearch]
        [Display(Name = "Long description", GroupName = SystemTabNames.Content, Order = 60)]
        public virtual XhtmlString LongDescription { get; set; }

        [CultureSpecific]
        [Display(Name = "Content area",
            GroupName = SystemTabNames.Content,
            Order = 65)]
        [AllowedTypes(new[] { typeof(IContentData) })]
        public virtual ContentArea ContentArea { get; set; }

        [CultureSpecific]
        [Display(Name = "Associations title",
            GroupName = SystemTabNames.Content,
            Order = 70)]
        public virtual string AssociationsTitle { get; set; }

        [CultureSpecific]
        [Display(Name = "Show recommendations",
            GroupName = SystemTabNames.Content,
            Description = "This will determine whether or not to show recommendations.",
            Order = 75)]
        public virtual bool ShowRecommendations { get; set; }

        [CultureSpecific]
        [Display(Name = "Product Status",
            GroupName = SystemTabNames.Content,
            Order = 80)]
        [SelectOne(SelectionFactoryType = typeof(ProductStatusSelectionFactory))]
        public virtual string ProductStatus { get; set; }
        #endregion

        #region SearchSettings
        [Range(1, 5)]
        [Display(Name = "Search Boost (1-5)", GroupName = Infrastructure.TabNames.SearchSettings,
            Description = "Boost product in search results with default sort", Order = 1)]
        public virtual int Boost
        {
            get
            {
                var boost = this.GetPropertyValue(p => p.Boost);

                return boost == 0 ? 1 : boost;
            }
            set => this.SetPropertyValue(p => p.Boost, value);
        }

        [Display(Name = "Bury", GroupName = Infrastructure.TabNames.SearchSettings,
            Description = "This will determine whether or not to hide product in search results.", Order = 2)]
        public virtual bool Bury { get; set; }
        #endregion

        #region Manufacturer

        [BackingType(typeof(PropertyString))]
        [Display(Name = "Manufacturer", GroupName = Infrastructure.TabNames.Manufacturer, Order = 5)]
        public virtual string Manufacturer { get; set; }

        [CultureSpecific]
        [Display(Name = "Manufacturer parts warranty description", GroupName = Infrastructure.TabNames.Manufacturer, Order = 10)]
        public virtual string ManufacturerPartsWarrantyDescription { get; set; }

        [BackingType(typeof(PropertyString))]
        [Display(Name = "Model", GroupName = Infrastructure.TabNames.Manufacturer, Order = 15)]
        public virtual string Model { get; set; }

        [Display(Name = "Model year", GroupName = Infrastructure.TabNames.Manufacturer, Order = 20)]
        [BackingType(typeof(PropertyString))]
        public virtual string ModelYear { get; set; }

        [BackingType(typeof(PropertyString))]
        [Display(Name = "Warranty", GroupName = Infrastructure.TabNames.Manufacturer, Order = 25)]
        public virtual string Warranty { get; set; }

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

            AssociationsTitle = "You May Also Like";
            ProductStatus = "Active";
            Boost = 1;
            Bury = false;
        }

    }
}