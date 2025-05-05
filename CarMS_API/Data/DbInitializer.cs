using CarMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Data
{
    public class DbInitializer
    {
        public static void InitDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            SeedData(scope.ServiceProvider.GetService<ApplicationDbContext>());
        }

        private static void SeedData(ApplicationDbContext context)
        {
            context.Database.Migrate();

            if (context.Cars.Any())
            {
                Console.WriteLine("Already have data - no need to seed");
                return;
            }

            var cars = new List<Car>
            {
                new Car
            {
                SellerId = 1,
                BrandId = 1,
                Model = "Civic",
                Year = 2020,
                Price = 750000,
                Mileage = 30000,
                Color = "White",
                EngineType = EngineType.Gasoline,
                GearType = GearType.Automatic,
                CarType = CarType.FourDoorSedan,
                Description = "Honda Civic 2020, well maintained and fuel-efficient.",
                ImageUrl = "https://cdn.pixabay.com/photo/2016/11/18/15/07/auto-1838789_1280.jpg",
                CreatedAt = new DateTime(2023, 5, 10),
                UpdatedAt = new DateTime(2024, 2, 15),
                Status = Status.Available,
                IsUsed = true,
                IsDeleted = false
            },
            new Car
            {
                SellerId = 2,
                BrandId = 3,
                Model = "Ranger",
                Year = 2019,
                Price = 820000,
                Mileage = 45000,
                Color = "Black",
                EngineType = EngineType.Diesel,
                GearType = GearType.Manual,
                CarType = CarType.PickUpTruck,
                Description = "Ford Ranger Wildtrak 2019, excellent for off-road.",
                ImageUrl = "https://cdn.pixabay.com/photo/2017/08/06/06/53/auto-2581113_1280.jpg",
                CreatedAt = new DateTime(2023, 6, 5),
                UpdatedAt = new DateTime(2024, 1, 10),
                Status = Status.Sold,
                IsUsed = true,
                IsDeleted = false
            },
            new Car
            {
                SellerId = 3,
                BrandId = 4,
                Model = "Model 3",
                Year = 2022,
                Price = 1600000,
                Mileage = 12000,
                Color = "Red",
                EngineType = EngineType.Electric,
                GearType = GearType.Automatic,
                CarType = CarType.CarSUV,
                Description = "Tesla Model 3, eco-friendly and powerful EV.",
                ImageUrl = "https://cdn.pixabay.com/photo/2020/02/26/14/45/tesla-4880455_1280.jpg",
                CreatedAt = new DateTime(2024, 3, 1),
                UpdatedAt = new DateTime(2025, 1, 25),
                Status = Status.Available,
                IsUsed = false,
                IsDeleted = false
            },
            new Car
            {
                SellerId = 4,
                BrandId = 5,
                Model = "Accord",
                Year = 2018,
                Price = 680000,
                Mileage = 52000,
                Color = "Gray",
                EngineType = EngineType.Gasoline,
                GearType = GearType.Automatic,
                CarType = CarType.FourDoorSedan,
                Description = "Honda Accord, premium comfort with spacious interior.",
                ImageUrl = "https://cdn.pixabay.com/photo/2016/11/29/05/08/auto-1868726_1280.jpg",
                CreatedAt = new DateTime(2022, 12, 10),
                UpdatedAt = new DateTime(2023, 10, 10),
                Status = Status.Reserved,
                IsUsed = true,
                IsDeleted = false
            },
            new Car
            {
                SellerId = 5,
                BrandId = 6,
                Model = "Camry",
                Year = 2021,
                Price = 890000,
                Mileage = 20000,
                Color = "Silver",
                EngineType = EngineType.Hybrid,
                GearType = GearType.Automatic,
                CarType = CarType.FourDoorSedan,
                Description = "Toyota Camry Hybrid, smooth ride with great fuel economy.",
                ImageUrl = "https://cdn.pixabay.com/photo/2020/01/19/08/15/toyota-4775213_1280.jpg",
                CreatedAt = new DateTime(2023, 1, 20),
                UpdatedAt = new DateTime(2024, 6, 12),
                Status = Status.Available,
                IsUsed = false,
                IsDeleted = false
            },
            new Car
            {
                SellerId = 6,
                BrandId = 7,
                Model = "X5",
                Year = 2019,
                Price = 2100000,
                Mileage = 60000,
                Color = "Blue",
                EngineType = EngineType.Diesel,
                GearType = GearType.Automatic,
                CarType = CarType.CarSUV,
                Description = "BMW X5, luxury SUV with powerful performance.",
                ImageUrl = "https://cdn.pixabay.com/photo/2018/05/02/22/28/bmw-3366247_1280.jpg",
                CreatedAt = new DateTime(2023, 3, 15),
                UpdatedAt = new DateTime(2024, 8, 5),
                Status = Status.Sold,
                IsUsed = true,
                IsDeleted = false
            },
            new Car
            {
                SellerId = 7,
                BrandId = 8,
                Model = "Yaris",
                Year = 2020,
                Price = 490000,
                Mileage = 28000,
                Color = "Yellow",
                EngineType = EngineType.Gasoline,
                GearType = GearType.Manual,
                CarType = CarType.FourDoorSedan,
                Description = "Toyota Yaris, compact and fuel-efficient.",
                ImageUrl = "https://cdn.pixabay.com/photo/2022/05/14/20/10/car-7196175_1280.jpg",
                CreatedAt = new DateTime(2022, 11, 5),
                UpdatedAt = new DateTime(2024, 2, 20),
                Status = Status.Available,
                IsUsed = true,
                IsDeleted = false
            },
            new Car
            {
                SellerId = 8,
                BrandId = 9,
                Model = "Mustang",
                Year = 2021,
                Price = 1800000,
                Mileage = 15000,
                Color = "Orange",
                EngineType = EngineType.Gasoline,
                GearType = GearType.Manual,
                CarType = CarType.CarSUV,
                Description = "Ford Mustang, sporty design and roaring engine.",
                ImageUrl = "https://cdn.pixabay.com/photo/2020/05/30/15/11/mustang-5241805_1280.jpg",
                CreatedAt = new DateTime(2024, 4, 8),
                UpdatedAt = new DateTime(2025, 1, 1),
                Status = Status.Available,
                IsUsed = false,
                IsDeleted = false
            },
            new Car
            {
                SellerId = 9,
                BrandId = 10,
                Model = "Hiace",
                Year = 2017,
                Price = 630000,
                Mileage = 88000,
                Color = "White",
                EngineType = EngineType.Diesel,
                GearType = GearType.Manual,
                CarType = CarType.CarVan,
                Description = "Toyota Hiace, spacious and ideal for group travel.",
                ImageUrl = "https://cdn.pixabay.com/photo/2020/01/19/07/55/van-4775182_1280.jpg",
                CreatedAt = new DateTime(2022, 10, 1),
                UpdatedAt = new DateTime(2023, 7, 25),
                Status = Status.Reserved,
                IsUsed = true,
                IsDeleted = false
            },
            new Car
            {
                SellerId = 10,
                BrandId = 11,
                Model = "Outlander PHEV",
                Year = 2022,
                Price = 1250000,
                Mileage = 18000,
                Color = "Green",
                EngineType = EngineType.Hybrid,
                GearType = GearType.Automatic,
                CarType = CarType.CarSUV,
                Description = "Mitsubishi Outlander PHEV, plug-in hybrid with 4WD.",
                ImageUrl = "https://cdn.pixabay.com/photo/2023/01/09/20/40/suv-7708877_1280.jpg",
                CreatedAt = new DateTime(2024, 5, 1),
                UpdatedAt = new DateTime(2025, 3, 28),
                Status = Status.Available,
                IsUsed = false,
                IsDeleted = false
            },
            };

            try
            {
                context.AddRange(cars);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Seed Error]: {ex.Message}");
            }
        }
    }
}
