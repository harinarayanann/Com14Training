using Foundation.Models.Blocks;

namespace Foundation.Models.Pages
{
    [ContentType(DisplayName = "StartPage", GUID = "6e6c728c-3bf8-48e9-b160-30539f676136"
        , Description = "PageType used for the HomePage")]
    public class StartPage : SitePageBase, IHasSettingsBlock
    {

        [CultureSpecific]
        [Display(
            Name = "Main body Start Page",
            Description = "The main body will be used with the XHTML-editor.",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString MainBody { get; set; }

        public virtual SettingsBlock Settings { get; set; }

        public virtual ContentReference campaignLink { get; set; }

    }
}