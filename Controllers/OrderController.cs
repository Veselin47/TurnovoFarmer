using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using TurnovoFarmer.Data;
using TurnovoFarmer.Models;
using TurnovoFarmer.Models.Constants;

namespace TurnovoFarmer.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "FarmerCart";

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCartFromSession();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.Schedules = await _context.DeliverySchedules
                .Include(s => s.Zone)
                .Where(s => s.DeliveryDate >= DateTime.Today &&
                            s.Status == ScheduleStatus.Collecting)
                .OrderBy(s => s.DeliveryDate)
                .ThenBy(s => s.Zone!.ZoneName)
                .ToListAsync();

            return View(cart);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(
            string name,
            string phone,
            int scheduleId,
            string deliveryType,
            string? deliveryAddress,
            double? latitude,
            double? longitude,
            string paymentMethod,
            string honeyField)
        {
            if (!string.IsNullOrEmpty(honeyField))
            {
                return BadRequest("Неоторизирана активност.");
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                TempData["Error"] = "Моля, попълнете име и телефон за връзка.";
                return RedirectToAction("Checkout");
            }

            var cart = GetCartFromSession();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var schedule = await _context.DeliverySchedules
                .Include(s => s.Zone)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null)
            {
                TempData["Error"] = "Избраният график за доставка не съществува.";
                return RedirectToAction("Checkout");
            }

            if (schedule.Status != ScheduleStatus.Collecting || schedule.DeliveryDate < DateTime.Today)
            {
                TempData["Error"] = "Избраният график вече не приема нови поръчки. Моля, изберете друг ден.";
                return RedirectToAction("Checkout");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            decimal totalAmount = cart.Sum(item => item.TotalPrice);
            decimal totalWeight = cart.Sum(item => item.Quantity);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    UserId = userId,
                    CustomerName = name.Trim(),
                    CustomerPhone = phone.Trim(),
                    ScheduleId = scheduleId,
                    DeliveryType = deliveryType,
                    DeliveryAddress = deliveryAddress,
                    Latitude = latitude,
                    Longitude = longitude,
                    TotalAmount = totalAmount,
                    PaymentMethod = paymentMethod,
                    PaymentStatus = paymentMethod == "card" ? "pending_stripe" : "pending",
                    OrderStatus = "pending_course",
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var item in cart)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "Един от продуктите вече не е наличен.";
                        return RedirectToAction("Checkout");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = $"Продуктът {product.Name} вече не е наличен в исканото количество!";
                        return RedirectToAction("Checkout");
                    }

                    product.StockQuantity -= item.Quantity;

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        PriceAtPurchase = item.Price
                    });
                }

                schedule.CurrentTotalWeight += totalWeight;

                if (schedule.Zone != null &&
                    schedule.CurrentTotalWeight >= schedule.Zone.MinWeightRequired &&
                    schedule.Status == ScheduleStatus.Collecting)
                {
                    schedule.Status = ScheduleStatus.Confirmed;
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                HttpContext.Session.Remove(CartSessionKey);

                return RedirectToAction("Success", new { id = order.Id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Възникна грешка при обработката на поръчката. Моля, опитайте отново.";
                return RedirectToAction("Checkout");
            }
        }

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Schedule)
                    .ThenInclude(s => s!.Zone)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        [Authorize]
        public async Task<IActionResult> Success(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Schedule)
                    .ThenInclude(s => s!.Zone)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.UserId != null && order.UserId != userId &&
                !User.IsInRole(AppRoles.Farmer) && !User.IsInRole(AppRoles.Admin))
            {
                return Forbid();
            }

            return View(order);
        }

        private List<CartItem> GetCartFromSession()
        {
            var sessionData = HttpContext.Session.GetString(CartSessionKey);
            return sessionData == null
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(sessionData) ?? new List<CartItem>();
        }
    }
}
