﻿namespace Foundation.Models.ViewModels
{
    public class AdminPageViewModel
    {
        public IEnumerable<IPriceValue> prices { get; set; }
        public string price { get; set; }
        public string imageFromNamedGroup { get; set; }
        public string imageInfo { get; set; }

        // new for Find
        //public IEnumerable<OrderValues> OrderValues { get; set; }
    }
}