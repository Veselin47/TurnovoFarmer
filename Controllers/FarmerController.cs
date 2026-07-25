using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TurnovoFarmer.Data;
using TurnovoFarmer.Models;
using TurnovoFarmer.Models.Constants;
using TurnovoFarmer.Models.ViewModels;

namespace TurnovoFarmer.Controllers
{
    [Authorize(Roles = AppRoles.FarmerOrAdmin)]
    public class FarmerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FarmerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Главно табло за управление на графиците
        public async Task<IActionResult> Index()
        {
            var schedules = await _context.DeliverySchedules
                .Include(s => s.Zone)
                .OrderByDescending(s => s.DeliveryDate)
                .Select(s => new FarmerDashboardScheduleViewModel
                {
                    Id = s.Id,
                    ZoneName = s.Zone!.ZoneName,
                    DeliveryDay = s.Zone.DeliveryDay,
                    DeliveryDate = s.DeliveryDate,
                    Status = s.Status,
                    CurrentTotalWeight = s.CurrentTotalWeight,
                    MinWeightRequired = s.Zone.MinWeightRequired,
                    OrderCount = s.Orders.Count,
                    TotalRevenue = s.Orders.Sum(o => o.TotalAmount)
                })
                .ToListAsync();

            return View(schedules);
        }

        // Шофьорски лист и калкулация на реколтата
        public async Task<IActionResult> DeliverySheet(int id)
        {
            var schedule = await _context.DeliverySchedules
                .Include(s => s.Zone)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            var harvestTotals = await (
                from oi in _context.OrderItems
                join o in _context.Orders on oi.OrderId equals o.Id
                join p in _context.Products on oi.ProductId equals p.Id
                where o.ScheduleId == id
                group oi by new { p.Name, p.UnitType } into g
                select new HarvestAggregateViewModel
                {
                    ProductName = g.Key.Name,
                    UnitType = g.Key.UnitType,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderBy(h => h.ProductName)
                .ToListAsync();

            var orders = await _context.Orders
                .Where(o => o.ScheduleId == id)
                .OrderBy(o => o.CustomerName)
                .ToListAsync();

            var viewModel = new DriverDeliverySheetViewModel
            {
                ScheduleId = schedule.Id,
                ZoneName = schedule.Zone?.ZoneName ?? "—",
                DeliveryDay = schedule.Zone?.DeliveryDay ?? "—",
                DeliveryDate = schedule.DeliveryDate,
                Status = schedule.Status,
                CurrentTotalWeight = schedule.CurrentTotalWeight,
                MinWeightRequired = schedule.Zone?.MinWeightRequired ?? 0,
                HarvestTotals = harvestTotals,
                Orders = orders
            };

            return View(viewModel);
        }

        // Мениджмънт на продуктите
        public async Task<IActionResult> ManageProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.DeletedAt == null)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> CreateProduct()
        {
            await PopulateCategoriesAsync();
            return View(new ProductFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync();
                return View(model);
            }

            if (!await _context.Categories.AnyAsync(c => c.Id == model.CategoryId))
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Избраната категория не съществува.");
                await PopulateCategoriesAsync();
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name.Trim(),
                Description = model.Description?.Trim() ?? string.Empty,
                PricePerUnit = model.PricePerUnit,
                UnitType = model.UnitType,
                StockQuantity = model.StockQuantity,
                ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl)
                    ? "/images/products/default.jpg"
                    : model.ImageUrl.Trim(),
                CategoryId = model.CategoryId,
                IsActive = model.IsActive
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Продуктът '{product.Name}' беше създаден успешно.";
            return RedirectToAction(nameof(ManageProducts));
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);

            if (product == null)
            {
                return NotFound();
            }

            await PopulateCategoriesAsync();

            return View(new ProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                PricePerUnit = product.PricePerUnit,
                UnitType = product.UnitType,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                IsActive = product.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, ProductFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);

            if (product == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync();
                return View(model);
            }

            if (!await _context.Categories.AnyAsync(c => c.Id == model.CategoryId))
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Избраната категория не съществува.");
                await PopulateCategoriesAsync();
                return View(model);
            }

            product.Name = model.Name.Trim();
            product.Description = model.Description?.Trim() ?? string.Empty;
            product.PricePerUnit = model.PricePerUnit;
            product.UnitType = model.UnitType;
            product.StockQuantity = model.StockQuantity;
            product.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl)
                ? "/images/products/default.jpg"
                : model.ImageUrl.Trim();
            product.CategoryId = model.CategoryId;
            product.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Продуктът '{product.Name}' беше обновен успешно.";
            return RedirectToAction(nameof(ManageProducts));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);

            if (product == null)
            {
                return NotFound();
            }

            product.DeletedAt = DateTime.UtcNow;
            product.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Продуктът '{product.Name}' беше архивиран (soft delete).";
            return RedirectToAction(nameof(ManageProducts));
        }

        private async Task PopulateCategoriesAsync()
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.OrderBy(c => c.Name).ToListAsync(),
                "Id", "Name");
        }

        // --- УПРАВЛЕНИЕ НА ГРАФИЦИТЕ ОТ СТРАНА НА ФЕРМЕРА ---
        public async Task<IActionResult> ManageSchedules()
        {
            var today = DateTime.Today;
            var bgCulture = new System.Globalization.CultureInfo("bg-BG");

            var schedules = await _context.DeliverySchedules
                .Include(s => s.Zone)
                .Include(s => s.Orders)
                .OrderByDescending(s => s.DeliveryDate >= today)
                .ThenBy(s => s.DeliveryDate)
                .ToListAsync();

            var viewModels = schedules.Select(s => new ManageScheduleViewModel
            {
                Id = s.Id,
                ZoneName = s.Zone?.ZoneName ?? "—",
                ZoneDeliveryDay = s.Zone?.DeliveryDay ?? "—",
                DeliveryDate = s.DeliveryDate,
                DayOfWeekBg = s.DeliveryDate.ToString("dddd", bgCulture),
                CurrentTotalWeight = s.CurrentTotalWeight,
                MinWeightRequired = s.Zone?.MinWeightRequired ?? 0,
                Status = s.Status,
                OrderCount = s.Orders.Count,
                IsPast = s.DeliveryDate.Date < today,
                CanDelete = s.Status != ScheduleStatus.Cancelled,
                CanClose = s.Status == ScheduleStatus.Collecting
            }).ToList();

            return View(viewModels);
        }

        public async Task<IActionResult> CreateSchedule()
        {
            await PopulateZonesAsync();
            return View(new ScheduleFormViewModel { DeliveryDate = DateTime.Today.AddDays(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSchedule(ScheduleFormViewModel model)
        {
            var deliveryDate = model.DeliveryDate.Date;

            if (deliveryDate < DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.DeliveryDate), "Датата трябва да е днес или в бъдеще.");
            }

            if (!await _context.DeliveryZones.AnyAsync(z => z.Id == model.ZoneId))
            {
                ModelState.AddModelError(nameof(model.ZoneId), "Избраната зона не съществува.");
            }

            if (await _context.DeliverySchedules.AnyAsync(s =>
                    s.ZoneId == model.ZoneId &&
                    s.DeliveryDate.Date == deliveryDate &&
                    s.Status != "cancelled"))
            {
                ModelState.AddModelError(string.Empty, "Вече съществува график за тази зона и дата.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateZonesAsync();
                return View(model);
            }

            var schedule = new DeliverySchedule
            {
                ZoneId = model.ZoneId,
                DeliveryDate = deliveryDate,
                CurrentTotalWeight = 0,
                Status = "collecting"
            };

            _context.DeliverySchedules.Add(schedule);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Графикът за {deliveryDate:dd.MM.yyyy} беше създаден успешно.";
            return RedirectToAction(nameof(ManageSchedules));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseSchedule(int id, string status)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            if (status is not (ScheduleStatus.Confirmed or ScheduleStatus.Closed))
            {
                TempData["Error"] = "Невалиден статус за затваряне на графика.";
                return RedirectToAction(nameof(ManageSchedules));
            }

            if (schedule.Status != ScheduleStatus.Collecting && schedule.Status != ScheduleStatus.Confirmed)
            {
                TempData["Error"] = "Този график вече е затворен или отменен.";
                return RedirectToAction(nameof(ManageSchedules));
            }

            schedule.Status = status;
            await _context.SaveChangesAsync();

            var label = status == ScheduleStatus.Confirmed ? "потвърден" : "затворен";
            TempData["Success"] = $"Графикът беше маркиран като {label}. Нови поръчки вече не се приемат.";
            return RedirectToAction(nameof(ManageSchedules));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.DeliverySchedules
                .Include(s => s.Orders)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            if (schedule.Orders.Any())
            {
                schedule.Status = "cancelled";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Графикът има прикачени поръчки и беше маркиран като отменен.";
            }
            else
            {
                _context.DeliverySchedules.Remove(schedule);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Графикът беше изтрит успешно.";
            }

            return RedirectToAction(nameof(ManageSchedules));
        }

        private async Task PopulateZonesAsync()
        {
            ViewBag.Zones = new SelectList(
                await _context.DeliveryZones.OrderBy(z => z.ZoneName).ToListAsync(),
                "Id", "ZoneName");
        }
    }
}