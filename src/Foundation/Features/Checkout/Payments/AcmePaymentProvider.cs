
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;

namespace Foundation.Features.Checkout.Payments
{
    public class AcmePaymentProvider : IPaymentPlugin
    {
        public IDictionary<string, string> Settings { get; set; }

        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            decimal CreditLimit = 2500;

            payment.TransactionType = TransactionType.Sale.ToString();
            if (payment.Amount <= CreditLimit)
            {
                return PaymentProcessingResult.CreateSuccessfulResult($"Acme credit approved payment for {payment.Amount}! Secret Code: {Settings["SecretKeyExample"]}");
            }
            else
            {
                return PaymentProcessingResult.CreateUnsuccessfulResult($"Sorry, you are over your limit!");
            }

        }
        /*
        private string[] GetAvailableCurrencies(this PaymentMethodDto.PaymentMethodRow paymentMethodRow)
        {
            var parameterRows = paymentMethodRow?.GetPaymentMethodParameterRows()
                                ?? Array.Empty<PaymentMethodDto.PaymentMethodParameterRow>();
            var parameterValue = parameterRows.FirstOrDefault(
                    parameterRow =>
                        parameterRow.Parameter == Constants.PaymentMethodParameters.AllowedCurrencies);
            return parameterValue != null
                ? parameterValue.Value?.Split(',')
                : Array.Empty<string>();
        }*/
    }
}
