using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManagementApi.Data;
using TaskManagementApi.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. ĐĂNG KÝ HỆ THỐNG CONTROLLER & CORS ---
builder.Services.AddControllers(); // Kích hoạt hệ thống Controller

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

// --- 2. ĐĂNG KÝ DATABASE (EF CORE + POSTGRESQL) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- 3. ĐĂNG KÝ DỊCH VỤ XÁC THỰC (JWT AUTHENTICATION) ---
// Định nghĩa cách .NET giải mã và kiểm tra tính hợp pháp của Token
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // Bắt buộc phải kiểm tra chữ ký bí mật
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("MaiHoangTung_Secret_Key_2026_Project_TaskManagement_SuperSecure")),
            ValidateIssuer = false, // Tạm thời bỏ qua kiểm tra bên phát hành (để test localhost)
            ValidateAudience = false // Tạm thời bỏ qua kiểm tra bên nhận (để test localhost)
        };
    });

builder.Services.AddAuthorization(); // Kích hoạt dịch vụ phân quyền

// Đăng ký Email Service
builder.Services.AddScoped<IEmailService, EmailService>();


var app = builder.Build();

// --- 4. THIẾT LẬP PIPELINE MIDDLEWARE (THỨ TỰ LÀ SỐNG CÒN) ---

app.UseCors("AllowAngular"); // Cho phép Angular gọi qua trước

// Trạm kiểm soát 1: Khách hàng là ai? (Đọc và giải mã JWT Token từ Header gửi lên)
app.UseAuthentication(); 

// Trạm kiểm soát 2: Khách hàng có quyền vào phòng này không? (Kiểm tra xem có nhãn [Authorize] không)
app.UseAuthorization(); 

app.MapControllers(); // Định tuyến HTTP Request vào đúng hàm trong Controller

app.Run();