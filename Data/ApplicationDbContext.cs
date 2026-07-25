using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TurnovoFarmer.Models;
using TurnovoFarmer.Models.Constants;

namespace TurnovoFarmer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<DeliveryZone> DeliveryZones { get; set; }
        public DbSet<DeliverySchedule> DeliverySchedules { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1. Добавяне на начални категории
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Зеленчуци", Slug = "zelenchutsi" },
                new Category { Id = 2, Name = "Плодове", Slug = "plodove" },
                new Category { Id = 3, Name = "Пчелни продукти", Slug = "pchelni-produkti" }
            );

            // 2. Добавяне на начални продукти
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, CategoryId = 1, Name = "Розов домат (био)", Description = "Сладък градински домат, откъснат в деня на доставката.", PricePerUnit = 4.50m, UnitType = "kg", StockQuantity = 100m, IsActive = true },
                new Product { Id = 2, CategoryId = 1, Name = "Пресни картофи", Description = "Ранни картофи, перфектни за печене.", PricePerUnit = 2.20m, UnitType = "kg", StockQuantity = 250m, IsActive = true },
                new Product { Id = 3, CategoryId = 3, Name = "Домашен мед от букет", Description = "Буркан натурален пчелен мед от региона на В. Търново.", PricePerUnit = 12.00m, UnitType = "jar", StockQuantity = 40m, IsActive = true }
            );

            // 3. Добавяне на зони за доставка около Велико Търново
            modelBuilder.Entity<DeliveryZone>().HasData(
                new DeliveryZone { Id = 1, ZoneName = "Велико Търново (Градска зона)", DeliveryDay = "Сряда", PickupLocationDescription = "Доставка до личен адрес в рамките на града.", MinWeightRequired = 30.00m },
                new DeliveryZone { Id = 2, ZoneName = "Горна Оряховица и Лясковец", DeliveryDay = "Събота", PickupLocationDescription = "Доставка до адрес за Г. Оряховица и Лясковец.", MinWeightRequired = 50.00m }
            );
        }
        public void SeedData()
        {
            // Проверяваме и добавяме категориите и продуктите, само ако ги няма
            if (!this.Categories.Any())
            {
                var cat1 = new Category { Name = "Зеленчуци", Slug = "zelenchutsi" };
                var cat2 = new Category { Name = "Плодове", Slug = "plodove" };

                this.Categories.AddRange(cat1, cat2);

                this.Products.AddRange(
                    new Product { Category = cat1, Name = "Розов домат", PricePerUnit = 4.50m, StockQuantity = 50, UnitType = "kg", IsActive = true },
                    new Product { Category = cat1, Name = "Краставици", PricePerUnit = 2.80m, StockQuantity = 30, UnitType = "kg", IsActive = true },
                    new Product { Category = cat2, Name = "Ягоди", PricePerUnit = 6.00m, StockQuantity = 20, UnitType = "kg", IsActive = true }
                );
            }

            // Проверяваме и добавяме зоните, само ако ги няма
            if (!this.DeliveryZones.Any())
            {
                this.DeliveryZones.AddRange(
                    new DeliveryZone { ZoneName = "Велико Търново", DeliveryDay = "Сряда", PickupLocationDescription = "Доставка до личен адрес в рамките на града.", MinWeightRequired = 30.00m },
                    new DeliveryZone { ZoneName = "Горна Оряховица и Лясковец", DeliveryDay = "Събота", PickupLocationDescription = "Доставка до адрес за Г. Оряховица и Лясковец.", MinWeightRequired = 50.00m }
                );
                this.SaveChanges();
            }

            EnsureUpcomingDeliverySchedules();
            this.SaveChanges();
        }

        /// <summary>
        /// Гарантира бъдещи графици: маркира минали като completed и създава нови при нужда.
        /// </summary>
        public void EnsureUpcomingDeliverySchedules()
        {
            var today = DateTime.Today;

            var pastOpenSchedules = this.DeliverySchedules
                .Where(s => s.DeliveryDate.Date < today &&
                            (s.Status == ScheduleStatus.Collecting ||
                             s.Status == ScheduleStatus.Confirmed ||
                             s.Status == ScheduleStatus.Closed))
                .ToList();

            foreach (var schedule in pastOpenSchedules)
            {
                schedule.Status = ScheduleStatus.Completed;
            }

            var zones = this.DeliveryZones.ToList();
            foreach (var zone in zones)
            {
                var hasOpenSchedule = this.DeliverySchedules.Any(s =>
                    s.ZoneId == zone.Id &&
                    s.DeliveryDate.Date >= today &&
                    s.Status == ScheduleStatus.Collecting);
                {
                    this.DeliverySchedules.Add(new DeliverySchedule
                    {
                        ZoneId = zone.Id,
                        DeliveryDate = GetNextDeliveryDate(zone.DeliveryDay, today),
                        CurrentTotalWeight = 0,
                        Status = "collecting"
                    });
                }
            }
        }

        private static DateTime GetNextDeliveryDate(string deliveryDay, DateTime fromDate)
        {
            var targetDay = deliveryDay switch
            {
                "Сряда" => DayOfWeek.Wednesday,
                "Събота" => DayOfWeek.Saturday,
                "Понеделник" => DayOfWeek.Monday,
                "Вторник" => DayOfWeek.Tuesday,
                "Четвъртък" => DayOfWeek.Thursday,
                "Петък" => DayOfWeek.Friday,
                "Неделя" => DayOfWeek.Sunday,
                _ => DayOfWeek.Wednesday
            };

            int daysUntil = ((int)targetDay - (int)fromDate.DayOfWeek + 7) % 7;
            if (daysUntil == 0)
            {
                daysUntil = 7;
            }

            return fromDate.AddDays(daysUntil);
        }
    }
}