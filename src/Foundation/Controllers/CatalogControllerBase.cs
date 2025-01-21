using EPiServer;
using EPiServer.Commerce.Catalog; // AssetUrlResolver
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Security;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Foundation.Controllers
{
    public class CatalogControllerBase<T>
        : ContentController<T> where T : CatalogContentBase
    {
        public readonly IContentLoader _contentLoader;
        public readonly UrlResolver _urlResolver;
        public readonly AssetUrlResolver _assetUrlResolver; // ECF
        public readonly ThumbnailUrlResolver _thumbnailUrlResolver; // ECF

        public CatalogControllerBase(
            IContentLoader contentLoader
            , UrlResolver urlResolver
            , AssetUrlResolver assetUrlResolver
            , ThumbnailUrlResolver thumbnailUrlResolver
            )
        {
            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
            _assetUrlResolver = assetUrlResolver;
            _thumbnailUrlResolver = thumbnailUrlResolver;
        }

        #region Media/Assets in 9 +

        public string GetDefaultAsset(IAssetContainer contentInstance)
        {
            return _assetUrlResolver.GetAssetUrl(contentInstance);
        }

        public string GetNamedAsset(IAssetContainer contentInstance, string propName)
        {
            return _thumbnailUrlResolver.GetThumbnailUrl(contentInstance, propName);
        }

        #endregion

        #region ...asset-work in ECF 8

        //public string GetDefaultAsset(EntryContentBase contentInstance)
        //{
        //    return _assetUrlResolver.GetAssetUrl(contentInstance);
        //}

        //public string GetDefaultAsset(NodeContent contentInstance)
        //{
        //    return _assetUrlResolver.GetAssetUrl(contentInstance);
        //}

        //public string GetNamedAsset(EntryContentBase contentInstance, string propName) // propName = "Group"
        //{
        //    return _thumbnailUrlResolver.GetThumbnailUrl(contentInstance, propName); // could have a better name
        //}

        //public string GetNamedAsset(NodeContent contentInstance, string propName)
        //{
        //    return _thumbnailUrlResolver.GetThumbnailUrl(contentInstance, propName);
        //}

        #endregion

        public string GetUrl(ContentReference contentReference)
        {
            return _urlResolver.GetUrl(contentReference);
        }

    }
}