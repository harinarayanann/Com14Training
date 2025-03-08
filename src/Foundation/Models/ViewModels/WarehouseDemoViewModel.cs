using Mediachase.Commerce.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Foundation.Models.ViewModels
{
    public class WarehouseDemoViewModel
    {
        public IEnumerable<IWarehouse> Warehouses { get; set; }
        public IWarehouse SelectedWarehouse { get; set; }
        public bool IsNew { get; set; }
    }
}