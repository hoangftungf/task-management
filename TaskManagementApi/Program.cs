using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); //Enable controller system

//fix lỗi CORS cho phép Angular frontend gọi API
builder.Services.AddCors(option =>
{
    option.AddPolicy("AllowAngular",
    policy =>
    {
        policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// 1. Đọc chuỗi kết nối từ file appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký AppDbContext vào DI Container của .NET
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
// -------------------------------------

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAngular");

app.MapControllers(); //Map HTTP routes → controller methods

app.Run();