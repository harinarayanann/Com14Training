using Foundation.Models.Pages;

namespace Foundation.Controllers
{
    public class CTOrderController : PageController<CTOrderPage>
    {
        public ActionResult Index(CTOrderPage currentPage, string passedAlong)
        {
            var model = new CTOrderViewModel()
            {
                TrackingNumber = passedAlong
            };

            return View(model);
        }
    }
}