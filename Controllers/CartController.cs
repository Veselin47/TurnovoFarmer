using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TurnovoFarmer.Data;
using TurnovoFarmer.Models;

namespace TurnovoFarmer.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "FarmerCart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Преглед на количката
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        // 2. Добавяне на продукт (или килограми) от каталога
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int productId, decimal quantity)
        {
            var product = _context.Products.Find(productId);
            if (product == null || !product.IsActive || product.DeletedAt != null)
            {
                return NotFound();
            }

            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                          Request.Headers["Fetch"] == "true";

            if (quantity > product.StockQuantity)
            {
                string errorMsg = $"Чичо Стефан няма толкова количество в момента. Максимум: {product.StockQuantity} {product.UnitType}.";
                if (isAjax) return BadRequest(errorMsg);

                TempData["Error"] = errorMsg;
                return RedirectToAction("Index", "Home");
            }

            var cart = GetCartFromSession();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existingItem != null)
            {
                if (existingItem.Quantity + quantity > product.StockQuantity)
                {
                    string errorMsg = "Общото количество в количката надвишава наличността на фермата!";
                    if (isAjax) return BadRequest(errorMsg);

                    TempData["Error"] = errorMsg;
                    return RedirectToAction("Index");
                }
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.PricePerUnit,
                    Quantity = quantity,
                    UnitType = product.UnitType
                });
            }

            SaveCartToSession(cart);

            if (isAjax) return Ok();

            TempData["Success"] = $"Успешно добавихте {quantity} {product.UnitType} {product.Name} в количката.";
            return RedirectToAction("Index", "Home");
        }

        // 🌟 3. ИНТЕРАКТИВНО ОБНОВЯВАНЕ НА КОЛИЧЕСТВОТО (ТОВА ЛИПСВАШЕ!)
        [HttpPost]
        [ValidateAntiForgeryToken] // Поддържа сигурността, въведена от Cursor
        public IActionResult UpdateQuantity(int productId, string quantity)
        {
            if (string.IsNullOrEmpty(quantity))
            {
                return BadRequest("Количеството не може да бъде празно.");
            }

            // Почистваме числото спрямо българските и английските настройки на Windows
            string sanitizedQuantity = quantity.Replace('.', ',');
            if (!decimal.TryParse(sanitizedQuantity, System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("bg-BG"), out decimal parsedQuantity))
            {
                if (!decimal.TryParse(quantity.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedQuantity))
                {
                    return BadRequest("Невалиден формат на количеството.");
                }
            }

            if (parsedQuantity < 0.5m)
            {
                return BadRequest("Минималното количество е 0.5.");
            }

            var product = _context.Products.Find(productId);
            if (product == null)
            {
                return NotFound("Продуктът не е намерен.");
            }

            if (parsedQuantity > product.StockQuantity)
            {
                return BadRequest($"Чичо Стефан няма толкова наличност! Максимум: {product.StockQuantity} {product.UnitType}.");
            }

            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                item.Quantity = parsedQuantity; // Записваме реално в сесията на .NET 10!
                SaveCartToSession(cart);

                return Json(new
                {
                    itemTotalPrice = item.TotalPrice,
                    cartTotalAmount = cart.Sum(c => c.TotalPrice),
                    cartTotalQty = cart.Sum(c => c.Quantity)
                });
            }

            return NotFound("Продуктът не е намерен в количката.");
        }

        // 4. Премахване на продукт от количката
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);
            }
            return RedirectToAction("Index");
        }

        // Помощни методи за работа със сесията
        private List<CartItem> GetCartFromSession()
        {
            var sessionData = HttpContext.Session.GetString(CartSessionKey);
            return sessionData == null ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(sessionData) ?? new List<CartItem>();
        }

        private void SaveCartToSession(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }
    }
}