using Foundation.Models.ViewModels;
using Mediachase.Commerce.Inventory;

namespace Foundation.Controllers
{
    public class WareHouseDemoController : Controller
    {
        private IWarehouseRepository _warehouseRepository;

        public WareHouseDemoController(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }
        public ActionResult Index()
        {
            var viewModel = new WarehouseDemoViewModel();
            viewModel.Warehouses = _warehouseRepository.List();
            return View(viewModel);
        }
        public ActionResult Select(string code)
        {
            var viewModel = new WarehouseDemoViewModel();
            viewModel.Warehouses = _warehouseRepository.List();
            viewModel.SelectedWarehouse = viewModel.Warehouses.Where(w => w.Code == code).First();
            viewModel.IsNew = false;

            return View("Index", viewModel);
        }

        public ActionResult New()
        {
            var viewModel = new WarehouseDemoViewModel();
            viewModel.Warehouses = _warehouseRepository.List();
            viewModel.SelectedWarehouse = new Warehouse();
            viewModel.IsNew = true;

            return View("Index", viewModel);
        }

        public ActionResult Submit(Warehouse warehouse,
            [Bind(Prefix = "SelectedWarehouse.ContactInformation")]WarehouseContactInformation warehouseContact, bool isNew)
        {
            var viewModel = new WarehouseDemoViewModel();
            viewModel.Warehouses = _warehouseRepository.List();
            warehouse.ContactInformation = warehouseContact;
            warehouse.Modified = DateTime.UtcNow;
            warehouse.ModifierId = "admin";

            if (isNew)
            {
                warehouse.Created = DateTime.UtcNow;
                warehouse.CreatorId = "admin";
            }

            _warehouseRepository.Save(warehouse);

            return View("Index", viewModel);
        }

        public ActionResult Delete(int? warehouseId)
        {
            var viewModel = new WarehouseDemoViewModel();
            if (warehouseId != null)
            {
                _warehouseRepository.Delete((int)warehouseId);
            }

            viewModel.Warehouses = _warehouseRepository.List();
            return View("Index", viewModel);
        }
    }
}