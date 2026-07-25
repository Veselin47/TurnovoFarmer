using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurnovoFarmer.Data;

namespace TurnovoFarmer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Главна страница - показва всички активни продукти, които не са изтрити (Soft Delete)
        public async Task<IActionResult> Index()
        {
            // Взимаме продуктите, които са активни и DeletedAt е NULL (филтър за сигурност)
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.DeletedAt == null)
                .ToListAsync();

            // Взимаме графиците за доставка, за да ги покажем на клиента
            ViewBag.DeliveryZones = await _context.DeliveryZones.ToListAsync();

            return View(products);
        }

        public IActionResult About()
        {
            return View();
        }

        // Страница за детайли на конкретен продукт
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}