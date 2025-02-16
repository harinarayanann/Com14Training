using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Foundation.Models.ViewModels
{
    public class ProjectedCarViewModel
    {
        public string SearchText { get; set; }
        public string ResultCount { get; set; }
        public List<ProjectedCar> Cars { get; set; }
    }

    public class ProjectedCar
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public string Brand { get; set; }
        public ContentReference UrlLink { get; set; }
    }
}