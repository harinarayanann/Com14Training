using Foundation.Infrastructure.Commerce.GiftCard;
using Foundation.Models.Catalog;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.Commerce.Orders.Dto;

namespace Foundation.Models.ViewModels
{
    public class PaymentDemoViewModel
    {
        public Guid SelectedPaymentId { get; set; }
        public IEnumerable<PaymentMethodDto.PaymentMethodRow> PayMethods { get; set; }
        public List<CarVariation> Variants { get; set; }
        public Money CartTotal { get; set; }
        public IEnumerable<ILineItem> CartItems { get; set; }
        public IEnumerable<EntityObject> GiftCards { get; set; }
        public IEnumerable<GiftCard> GiftCardsCustom { get; set; }
        public bool UseGiftCard { get; set; }
        public string RedemtionCode { get; set; }
        public decimal GiftCardDebitAmt { get; set; }
        public string MessageOutput { get; set; }
        public string GiftInfoMessage { get; set; }
        public IEnumerable<ShippingMethodDto.ShippingMethodRow> ShippingMethods { get; set; }
        public Guid SelectedShippingMethodId { get; set; }

    }
}