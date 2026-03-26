using CarMS_API.Data;
using CarMS_API.Models;
using CarMS_API.Models.Mapper;
using CarMS_API.Repositorys;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Services;
using CarMS_API.Services.IServices;
using CarMS_API.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
            "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
            "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
           new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

//CONTEXT
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.Configure<BookingSettings>(
    builder.Configuration.GetSection("BookingSettings"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 2; // ��駤�Ҥ�����Ǣ�鹵�Ӣͧ���ʼ�ҹ
});

#region JWT
var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");
builder.Services.AddAuthentication(u =>
{
    u.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    u.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(u =>
{
    u.RequireHttpsMetadata = false;
    u.SaveToken = true;
    u.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
#endregion

// Register AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
//SERVICES
builder.Services.AddScoped<RoleSeeder>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IFileUpload, FileUpload>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddHostedService<BookingExpiryService>();

// CORS
builder.Services.AddCors();

// 🌟 จุดที่ 1: เพิ่ม Service ของ SignalR (ใส่ไว้ก่อน builder.Build())
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseCors(opt =>
{
   opt.AllowAnyHeader()
       .AllowAnyMethod()
       .AllowCredentials()
       .WithOrigins("https://localhost:5178", "http://localhost:3000");
});

// app.UseCors(opt =>
// {
//     opt.AllowAnyHeader()
//         .AllowAnyMethod()
//         .AllowCredentials()
//         .WithOrigins("http://10.103.0.17", "http://10.103.0.17/cs66/next/s07/wepcar");
// });

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// 🌟 จุดที่ 2: แมปเส้นทาง (Endpoint) สำหรับให้ Frontend ยิงเข้ามาเชื่อมต่อ
app.MapHub<CarMS_API.Hubs.ChatHub>("/chatHub");
app.MapHub<CarMS_API.Hubs.NotificationHub>("/notificationHub");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleSeeder = new RoleSeeder(services.GetRequiredService<RoleManager<IdentityRole>>());
    await roleSeeder.SeedRolesAsync();
}

app.Run();