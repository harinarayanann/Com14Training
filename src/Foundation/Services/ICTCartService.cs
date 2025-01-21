using Foundation.Features.Checkout;
using Foundation.Features.Checkout.ViewModels;
using Foundation.Features.NamedCarts;

namespace Foundation.Services
{
    public interface ICTCartService
    {
        AddToCartResult AddToCart(ICart cart, RequestParamsToCart requestParams/*, string code, decimal quantity, string deliveryMethod, string warehouseCode, List<string> dynamicVariantOptionCodes*/);
        string DefaultCartName { get; }
        string DefaultWishListName { get; }
        string DefaultSharedCartName { get; }
        string DefaultOrderPadName { get; }
        ICart LoadOrCreateCart(string name);
        ICart LoadOrCreateCart(string name, string contactId);

        CartWithValidationIssues LoadCart(string name, bool validate);
        CartWithValidationIssues LoadCart(string name, string contactId, bool validate);


    }
}
