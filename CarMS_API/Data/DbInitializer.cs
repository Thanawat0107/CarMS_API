using CarMS_API.Models;
using CarMS_API.Utility;
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
                CarRegistrationNumber = "กข-1234 กทม",
                CarIdentificationNumber = "1HGCM82633A004352",
                EngineNumber = "ENG12345678",
                Model = "Civic",
                Year = 2020,
                Price = 750000,
                BookingPrice = 4500,
                Mileage = 30000,
                Color = "White",
                EngineType = SD.Engine_Gasoline,
                GearType = SD.Gear_Automatic,
                CarType = SD.CarType_FourDoorSedan,
                Description = "Honda Civic 2020, well maintained and fuel-efficient.",
                CarImages = "https://cdn.pixabay.com/photo/2019/08/04/23/28/honda-4384888_1280.jpg",
                CreatedAt = new DateTime(2023, 5, 10),
                UpdatedAt = new DateTime(2024, 2, 15),
                CarStatus = SD.Status_Available,
                IsApproved = true,
                IsUsed = true,
                IsDeleted = false
            },
            new Car
            {
                CarRegistrationNumber = "งจ-5678 เชียงใหม่",
                CarIdentificationNumber = "1FTRX18W1XNB12345",
                EngineNumber = "ENG23456789",
                IsApproved = false,
                Model = "Ranger",
                Year = 2019,
                Price = 820000,
                BookingPrice = 5000,
                Mileage = 45000,
                Color = "Black",
                EngineType = SD.Engine_Diesel,
                GearType = SD.Gear_Manual,
                CarType = SD.CarType_PickUpTruck,
                Description = "Ford Ranger Wildtrak 2019, excellent for off-road.",
                CarImages = "https://cdn.pixabay.com/photo/2014/07/13/19/45/edsel-ranger-392745_1280.jpg",
                CreatedAt = new DateTime(2023, 6, 5),
                UpdatedAt = new DateTime(2024, 1, 10),
                CarStatus = SD.Status_Sold,
                IsUsed = true,
                IsDeleted = false
            },
            new Car
            {
                CarRegistrationNumber = "ขย-9087 ภูเก็ต",
                CarIdentificationNumber = "5YJ3E1EA7KF317235",
                EngineNumber = "ENG34567890",
                IsApproved = true,
                Model = "Model 3",
                Year = 2022,
                Price = 1600000,
                BookingPrice = 3500,
                Mileage = 12000,
                Color = "Red",
                EngineType = SD.Engine_Electric,
                GearType = SD.Gear_Automatic,
                CarType = SD.CarType_SUV,
                Description = "Tesla Model 3, eco-friendly and powerful EV.",
                CarImages = "https://cdn.pixabay.com/photo/2023/01/02/09/21/super-car-7691660_1280.jpg",
                CreatedAt = new DateTime(2024, 3, 1),
                UpdatedAt = new DateTime(2025, 1, 25),
                CarStatus = SD.Status_Available,
                IsUsed = false,
                IsDeleted = false
            },
            new Car
            {
                CarRegistrationNumber = "ทล-1111 ขอนแก่น",
                CarIdentificationNumber = "1HGCR2F3XFA027534",
                EngineNumber = "ENG45678901",
                IsApproved = false,
                Model = "Accord",
                Year = 2018,
                Price = 680000,
                BookingPrice = 3000,
                Mileage = 52000,
                Color = "Gray",
                EngineType = SD.Engine_Gasoline,
                GearType = SD.Gear_Automatic,
                CarType = SD.CarType_FourDoorSedan,
                Description = "Honda Accord, premium comfort with spacious interior.",
                CarImages = "https://cdn.pixabay.com/photo/2017/04/21/02/20/jdm-2247450_1280.jpg",
                CreatedAt = new DateTime(2022, 12, 10),
                UpdatedAt = new DateTime(2023, 10, 10),
                CarStatus = SD.Status_Booked,
                IsUsed = true,
                IsDeleted = false
            },
            new Car
            {
                CarRegistrationNumber = "ศร-2222 ระยอง",
                CarIdentificationNumber = "JTNB11HK8K3001234",
                EngineNumber = "ENG56789012",
                IsApproved = true,
                Model = "Camry",
                Year = 2021,
                Price = 890000,
                BookingPrice = 4000,
                Mileage = 20000,
                Color = "Silver",
                EngineType = SD.Engine_Hybrid,
                GearType = SD.Gear_Automatic,
                CarType = SD.CarType_FourDoorSedan,
                Description = "Toyota Camry Hybrid, smooth ride with great fuel economy.",
                CarImages = "https://cdn.pixabay.com/photo/2022/03/27/16/11/car-7095541_1280.jpg",
                CreatedAt = new DateTime(2023, 1, 20),
                UpdatedAt = new DateTime(2024, 6, 12),
                CarStatus = SD.Status_Available,
                IsUsed = false,
                IsDeleted = false
            },
            new Car
            {
                CarRegistrationNumber = "นห-3333 นครราชสีมา",
                CarIdentificationNumber = "WBAXH5C51CDW12345",
                EngineNumber = "ENG67890123",
                IsApproved = true,
                Model = "X5",
                Year = 2019,
                Price = 2100000,
                BookingPrice = 4500,
                Mileage = 60000,
                Color = "Blue",
                EngineType = SD.Engine_Diesel,
                GearType = SD.Gear_Automatic,
                CarType = SD.CarType_SUV,
                Description = "BMW X5, luxury SUV with powerful performance.",
                CarImages = "https://cdn.pixabay.com/photo/2020/06/06/01/44/bmw-x5-5264945_1280.jpg",
                CreatedAt = new DateTime(2023, 3, 15),
                UpdatedAt = new DateTime(2024, 8, 5),
                CarStatus = SD.Status_Sold,
                IsUsed = true,
                IsDeleted = false
            },
            new Car
            {
                CarRegistrationNumber = "อค-4444 ลำปาง",
                CarIdentificationNumber = "JTDKBRFU9J3054321",
                EngineNumber = "ENG78901234",
                IsApproved = false,
                Model = "Yaris",
                Year = 2020,
                Price = 490000,
                BookingPrice = 6000,
                Mileage = 28000,
                Color = "Yellow",
                EngineType = SD.Engine_Gasoline,
                GearType = SD.Gear_Manual,
                CarType = SD.CarType_FourDoorSedan,
                Description = "Toyota Yaris, compact and fuel-efficient.",
                CarImages = "https://cdn.pixabay.com/photo/2021/10/29/12/26/toyota-gr-yaris-6751759_1280.jpg",
                CreatedAt = new DateTime(2022, 11, 5),
                UpdatedAt = new DateTime(2024, 2, 20),
                CarStatus = SD.Status_Available,
                IsUsed = true,
                IsDeleted = false
            },
            new Car
            {
                CarRegistrationNumber = "ภก-5555 สงขลา",
                CarIdentificationNumber = "1FA6P8CF4H5301234",
                EngineNumber = "ENG89012345",
                IsApproved = true,
                Model = "Mustang",
                Year = 2021,
                Price = 1800000,
                BookingPrice = 6500,
                Mileage = 15000,
                Color = "Orange",
                EngineType = SD.Engine_Gasoline,
                GearType = SD.Gear_Manual,
                CarType = SD.CarType_SUV,
                Description = "Ford Mustang, sporty design and roaring engine.",
                CarImages = "https://cdn.pixabay.com/photo/2017/03/24/21/44/mustang-2172273_1280.jpg",
                CreatedAt = new DateTime(2024, 4, 8),
                UpdatedAt = new DateTime(2025, 1, 1),
                CarStatus = SD.Status_Available,
                IsUsed = false,
                IsDeleted = false
            },
            new Car
            {
                CarRegistrationNumber = "ชข-6666 อุดรธานี",
                CarIdentificationNumber = "JTFHX02P3V0001234",
                EngineNumber = "ENG90123456",
                IsApproved = false,
                Model = "Hiace",
                Year = 2017,
                Price = 630000,
                BookingPrice = 7000,
                Mileage = 88000,
                Color = "White",
                EngineType = SD.Engine_Diesel,
                GearType = SD.Gear_Manual,
                CarType = SD.CarType_Van,
                Description = "Toyota Hiace, spacious and ideal for group travel.",
                CarImages = "https://cdn.pixabay.com/photo/2016/05/12/19/23/mexico-1388435_1280.jpg",
                CreatedAt = new DateTime(2022, 10, 1),
                UpdatedAt = new DateTime(2023, 7, 25),
                CarStatus = SD.Status_Booked,
                IsUsed = true,
                IsDeleted = false
            },
            new Car
            {
                CarRegistrationNumber = "พท-7777 นครปฐม",
                CarIdentificationNumber = "JA4J24A59NZ012345",
                EngineNumber = "ENG01234567",
                IsApproved = true,
                Model = "Outlander PHEV",
                Year = 2022,
                Price = 1250000,
                BookingPrice = 7500,
                Mileage = 18000,
                Color = "Green",
                EngineType = SD.Engine_Hybrid,
                GearType = SD.Gear_Automatic,
                CarType = SD.CarType_SUV,
                Description = "Mitsubishi Outlander PHEV, plug-in hybrid with 4WD.",
                CarImages = "https://cdn.pixabay.com/photo/2017/09/14/07/08/mitsubishi-2748155_1280.jpg",
                CreatedAt = new DateTime(2024, 5, 1),
                UpdatedAt = new DateTime(2025, 3, 28),
                CarStatus = SD.Status_Available,
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
