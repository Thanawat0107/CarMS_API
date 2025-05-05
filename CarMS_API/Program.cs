using CarMS_API.Data;
using CarMS_API.Models;
using CarMS_API.Models.Mapper;
using CarMS_API.Repositorys;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));
//SERVICES
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ISearchableRepository<Car, CarSearchParams>, CarSearchRepository>();
builder.Services.AddScoped<ISearchableRepository<Brand, BrandSearchParams>, BrandSearchRepository>();
builder.Services.AddScoped<ISearchableRepository<Seller, SellerSearchParams>, SellerSearchRepository>();
//CONTEXT
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
