using EPiServer.Core;
using EPiServer.Find.UnifiedSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Find;

namespace Foundation.Models.ViewModels
{
    public class FindUnifiedViewModel
    {
        public string SearchQuery { get; set; }
        public UnifiedSearchResults Results { get; set; }
        public PageDataCollection MPageDataCollection { get; set; }
    }
}