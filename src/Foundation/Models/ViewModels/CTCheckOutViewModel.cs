using Foundation.Models.Pages;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;

namespace Foundation.Models.ViewModels
{
    public class CTCheckOutViewModel
    {
        
        public CTCheckOutPage CurrentPage { get; set; }

        // Lab E1 - create properties below
        public IEnumerable<PaymentMethodDto.PaymentMethodRow> PaymentMethods { get; set; }
        public IEnumerable<ShippingMethodDto.ShippingMethodRow> ShippingMethods { get; set; }
        public IEnumerable<ShippingRate> ShippingRates { get; set; }
        public Guid SelectedShipId { get; set; }
        public Guid SelectedPayId { get; set; }

        public string cartValidationMessages { get; set; }


        public CTCheckOutViewModel()
        { }

        public CTCheckOutViewModel(CTCheckOutPage currentPage) { 
            CurrentPage = currentPage;
        }


    }
}