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
                CarRegistrationNumber = "กข-1234 กทม",
                CarIdentificationNumber = "1HGCM82633A004352",
                EngineNumber = "ENG12345678",
                Model = "Civic",
                Year = 2020,
                Price = 750000,
                ReservationPrice = 4500,
                Mileage = 30000,
                Color = "White",
                EngineType = EngineType.Gasoline,
                GearType = GearType.Automatic,
                CarType = CarType.FourDoorSedan,
                Description = "Honda Civic 2020, well maintained and fuel-efficient.",
                ImageUrl = "https://cdn.pixabay.com/photo/2019/08/04/23/28/honda-4384888_1280.jpg",
                CreatedAt = new DateTime(2023, 5, 10),
                UpdatedAt = new DateTime(2024, 2, 15),
                Status = Status.Available,
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
                ReservationPrice = 5000,
                Mileage = 45000,
                Color = "Black",
                EngineType = EngineType.Diesel,
                GearType = GearType.Manual,
                CarType = CarType.PickUpTruck,
                Description = "Ford Ranger Wildtrak 2019, excellent for off-road.",
                ImageUrl = "https://cdn.pixabay.com/photo/2014/07/13/19/45/edsel-ranger-392745_1280.jpg",
                CreatedAt = new DateTime(2023, 6, 5),
                UpdatedAt = new DateTime(2024, 1, 10),
                Status = Status.Sold,
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
                ReservationPrice = 3500,
                Mileage = 12000,
                Color = "Red",
                EngineType = EngineType.Electric,
                GearType = GearType.Automatic,
                CarType = CarType.CarSUV,
                Description = "Tesla Model 3, eco-friendly and powerful EV.",
                ImageUrl = "https://cdn.pixabay.com/photo/2023/01/02/09/21/super-car-7691660_1280.jpg",
                CreatedAt = new DateTime(2024, 3, 1),
                UpdatedAt = new DateTime(2025, 1, 25),
                Status = Status.Available,
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
                ReservationPrice = 3000,
                Mileage = 52000,
                Color = "Gray",
                EngineType = EngineType.Gasoline,
                GearType = GearType.Automatic,
                CarType = CarType.FourDoorSedan,
                Description = "Honda Accord, premium comfort with spacious interior.",
                ImageUrl = "https://cdn.pixabay.com/photo/2017/04/21/02/20/jdm-2247450_1280.jpg",
                CreatedAt = new DateTime(2022, 12, 10),
                UpdatedAt = new DateTime(2023, 10, 10),
                Status = Status.Reserved,
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
                ReservationPrice = 4000,
                Mileage = 20000,
                Color = "Silver",
                EngineType = EngineType.Hybrid,
                GearType = GearType.Automatic,
                CarType = CarType.FourDoorSedan,
                Description = "Toyota Camry Hybrid, smooth ride with great fuel economy.",
                ImageUrl = "https://cdn.pixabay.com/photo/2022/03/27/16/11/car-7095541_1280.jpg",
                CreatedAt = new DateTime(2023, 1, 20),
                UpdatedAt = new DateTime(2024, 6, 12),
                Status = Status.Available,
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
                ReservationPrice = 4500,
                Mileage = 60000,
                Color = "Blue",
                EngineType = EngineType.Diesel,
                GearType = GearType.Automatic,
                CarType = CarType.CarSUV,
                Description = "BMW X5, luxury SUV with powerful performance.",
                ImageUrl = "https://cdn.pixabay.com/photo/2020/06/06/01/44/bmw-x5-5264945_1280.jpg",
                CreatedAt = new DateTime(2023, 3, 15),
                UpdatedAt = new DateTime(2024, 8, 5),
                Status = Status.Sold,
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
                ReservationPrice = 6000,
                Mileage = 28000,
                Color = "Yellow",
                EngineType = EngineType.Gasoline,
                GearType = GearType.Manual,
                CarType = CarType.FourDoorSedan,
                Description = "Toyota Yaris, compact and fuel-efficient.",
                ImageUrl = "https://cdn.pixabay.com/photo/2021/10/29/12/26/toyota-gr-yaris-6751759_1280.jpg",
                CreatedAt = new DateTime(2022, 11, 5),
                UpdatedAt = new DateTime(2024, 2, 20),
                Status = Status.Available,
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
                ReservationPrice = 6500,
                Mileage = 15000,
                Color = "Orange",
                EngineType = EngineType.Gasoline,
                GearType = GearType.Manual,
                CarType = CarType.CarSUV,
                Description = "Ford Mustang, sporty design and roaring engine.",
                ImageUrl = "https://cdn.pixabay.com/photo/2017/03/24/21/44/mustang-2172273_1280.jpg",
                CreatedAt = new DateTime(2024, 4, 8),
                UpdatedAt = new DateTime(2025, 1, 1),
                Status = Status.Available,
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
                ReservationPrice = 7000,
                Mileage = 88000,
                Color = "White",
                EngineType = EngineType.Diesel,
                GearType = GearType.Manual,
                CarType = CarType.CarVan,
                Description = "Toyota Hiace, spacious and ideal for group travel.",
                ImageUrl = "https://cdn.pixabay.com/photo/2016/05/12/19/23/mexico-1388435_1280.jpg",
                CreatedAt = new DateTime(2022, 10, 1),
                UpdatedAt = new DateTime(2023, 7, 25),
                Status = Status.Reserved,
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
                ReservationPrice = 7500,
                Mileage = 18000,
                Color = "Green",
                EngineType = EngineType.Hybrid,
                GearType = GearType.Automatic,
                CarType = CarType.CarSUV,
                Description = "Mitsubishi Outlander PHEV, plug-in hybrid with 4WD.",
                ImageUrl = "https://cdn.pixabay.com/photo/2017/09/14/07/08/mitsubishi-2748155_1280.jpg",
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
