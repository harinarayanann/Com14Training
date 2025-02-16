using Foundation.Models.Catalog;
using EPiServer.Find.Api.Facets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Foundation.Models.ViewModels
{
    public class FindResultViewModel
    {
        public string SearchText { get; set; }
        public string ResultCount { get; set; }
        public List<CarVariation> CarVariants { get; set; }
        public List<TermCount> ColorFacets { get; set; }
    }
}