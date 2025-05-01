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
                    Brand = "Toyota",
                    Model = "Camry",
                    Year = 2021,
                    Price = 950000,
                    Mileage = 20000,
                    Color = "Black",
                    EngineType = "Hybrid",
                    GearType = "Automatic",
                    Description = "Toyota Camry hybrid รถประหยัดน้ำมัน นั่งสบาย",
                    ImageUrl = "https://cdn.pixabay.com/photo/2020/04/14/09/21/toyota-5042747_1280.jpg",
                    CreatedAt =  new DateTime(2024, 1, 1),
                    UpdatedAt =  new DateTime(2024, 1, 1),
                    IsUsed = true,
                    IsDeleted = false,
                    CarType = CarType.FourDoorSedan,
                    SellerId = "user1"
                },
                new Car
                {
                    Brand = "Honda",
                    Model = "Civic",
                    Year = 2020,
                    Price = 850000,
                    Mileage = 30000,
                    Color = "White",
                    EngineType = "Petrol",
                    GearType = "Automatic",
                    Description = "Honda Civic สภาพดี พร้อมใช้งาน",
                    ImageUrl = "https://cdn.pixabay.com/photo/2018/05/16/08/50/honda-3405840_1280.jpg",
                    CreatedAt =  new DateTime(2024, 1, 1),
                    UpdatedAt =  new DateTime(2024, 1, 1),
                    IsUsed = true,
                    IsDeleted = false,
                    CarType = CarType.FourDoorSedan,
                    SellerId = "user2"
                },
                new Car
                {
                    Brand = "Ford",
                    Model = "Ranger",
                    Year = 2019,
                    Price = 790000,
                    Mileage = 50000,
                    Color = "Blue",
                    EngineType = "Diesel",
                    GearType = "Manual",
                    Description = "Ford Ranger กระบะสายลุย ทนทาน",
                    ImageUrl = "https://cdn.pixabay.com/photo/2018/03/06/19/17/ford-3203747_1280.jpg",
                    CreatedAt =  new DateTime(2024, 1, 1),
                    UpdatedAt =  new DateTime(2024, 1, 1),
                    IsUsed = true,
                    IsDeleted = false,
                    CarType = CarType.PickUpTruck,
                    SellerId = "user3"
                },
                new Car
                {
                    Brand = "Nissan",
                    Model = "Navara",
                    Year = 2022,
                    Price = 920000,
                    Mileage = 10000,
                    Color = "Silver",
                    EngineType = "Diesel",
                    GearType = "Automatic",
                    Description = "Nissan Navara ขับสบาย พลังแรง",
                    ImageUrl = "https://cdn.pixabay.com/photo/2022/10/20/14/32/nissan-7533615_1280.jpg",
                    CreatedAt =  new DateTime(2024, 1, 1),
                    UpdatedAt =  new DateTime(2024, 1, 1),
                    IsUsed = false,
                    IsDeleted = false,
                    CarType = CarType.PickUpTruck,
                    SellerId = "user4"
                },
                new Car
                {
                    Brand = "BMW",
                    Model = "X5",
                    Year = 2023,
                    Price = 2800000,
                    Mileage = 5000,
                    Color = "Gray",
                    EngineType = "Hybrid",
                    GearType = "Automatic",
                    Description = "BMW X5 SUV หรู พร้อมระบบช่วยขับ",
                    ImageUrl = "https://cdn.pixabay.com/photo/2020/04/12/12/38/bmw-5034925_1280.jpg",
                    CreatedAt =  new DateTime(2024, 1, 1),
                    UpdatedAt =  new DateTime(2024, 1, 1),
                    IsUsed = false,
                    IsDeleted = false,
                    CarType = CarType.CarSUV,
                    SellerId = "user5"
                },
                new Car
                {
                    Brand = "Isuzu",
                    Model = "D-Max",
                    Year = 2018,
                    Price = 620000,
                    Mileage = 70000,
                    Color = "Red",
                    EngineType = "Diesel",
                    GearType = "Manual",
                    Description = "Isuzu D-Max แกร่งและประหยัด",
                    ImageUrl = "https://cdn.pixabay.com/photo/2020/06/09/05/34/pickup-5277204_1280.jpg",
                    CreatedAt =  new DateTime(2024, 1, 1),
                    UpdatedAt =  new DateTime(2024, 1, 1),
                    IsUsed = true,
                    IsDeleted = false,
                    CarType = CarType.PickUpTruck,
                    SellerId = "user6"
                },
                new Car
                {
                    Brand = "Mercedes-Benz",
                    Model = "E-Class",
                    Year = 2022,
                    Price = 3200000,
                    Mileage = 15000,
                    Color = "Black",
                    EngineType = "Petrol",
                    GearType = "Automatic",
                    Description = "Mercedes-Benz หรูหราระดับพรีเมียม",
                    ImageUrl = "https://cdn.pixabay.com/photo/2020/09/06/11/10/mercedes-benz-5548353_1280.jpg",
                    CreatedAt =  new DateTime(2024, 1, 1),
                    UpdatedAt =  new DateTime(2024, 1, 1),
                    IsUsed = false,
                    IsDeleted = false,
                    CarType = CarType.FourDoorSedan,
                    SellerId = "user7"
                },
                new Car
                {
                    Brand = "Hyundai",
                    Model = "H1",
                    Year = 2020,
                    Price = 1300000,
                    Mileage = 25000,
                    Color = "White",
                    EngineType = "Diesel",
                    GearType = "Automatic",
                    Description = "Hyundai H1 รถตู้ครอบครัว 11 ที่นั่ง",
                    ImageUrl = "https://cdn.pixabay.com/photo/2019/06/12/06/11/hyundai-4269967_1280.jpg",
                    CreatedAt =  new DateTime(2024, 1, 1),
                    UpdatedAt =  new DateTime(2024, 1, 1),
                    IsUsed = true,
                    IsDeleted = false,
                    CarType = CarType.CarVan,
                    SellerId = "user8"
                },
                new Car
                {
                    Brand = "Mazda",
                    Model = "CX-5",
                    Year = 2021,
                    Price = 1200000,
                    Mileage = 30000,
                    Color = "White",
                    EngineType = "Petrol",
                    GearType = "Automatic",
                    Description = "Mazda CX-5 SUV สุดทันสมัย",
                    ImageUrl = "https://cdn.pixabay.com/photo/2019/05/09/16/41/mazda-4188511_1280.jpg",
                    CreatedAt =  new DateTime(2024, 1, 1),
                    UpdatedAt =  new DateTime(2024, 1, 1),
                    IsUsed = true,
                    IsDeleted = false,
                    CarType = CarType.CarSUV,
                    SellerId = "user9"
                },
                new Car
                {
                    Brand = "Chevrolet",
                    Model = "Trailblazer",
                    Year = 2017,
                    Price = 680000,
                    Mileage = 90000,
                    Color = "Gray",
                    EngineType = "Diesel",
                    GearType = "Automatic",
                    Description = "Chevrolet Trailblazer SUV สำหรับครอบครัว",
                    ImageUrl = "https://cdn.pixabay.com/photo/2018/06/03/22/32/suv-3451693_1280.jpg",
                    CreatedAt =  new DateTime(2024, 1, 1),
                    UpdatedAt =  new DateTime(2024, 1, 1),
                    IsUsed = true,
                    IsDeleted = false,
                    CarType = CarType.CarSUV,
                    SellerId = "user10"
                }
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
