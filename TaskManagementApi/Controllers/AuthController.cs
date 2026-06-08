using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Dtos;
using TaskManagementApi.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Google.Apis.Auth;

namespace TaskManagementApi.Controllers;

[ApiController] //giống decorator trong Angular @Controller
[Route("api/[controller]")] //routing configuration [controller] = auth vì classname là AuthController (framework tự bỏ hậu tố Controller)
public class AuthController : ControllerBase //kế thừa ControllerBase
{
    private readonly AppDbContext _context; //Khai báo một biến để chứa "Hóa đơn kết nối" Database
    private readonly IConfiguration _configuration;
    // Dependency Injection (DI)
    //.Net DI Container sẽ tự động tạo AppDbContext và ném vào đây mỗi khi có Request tới
    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    //Api login
    [HttpPost("login")] //http route metadata POST /api/auth/login --> request routing
    public async Task<IActionResult> Login([FromBody] LoginRequest request) //IActionResult: trả về 200 OK, 400 Bad Request...
    {
        // Tìm user trong DB
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return Unauthorized("Email không tồn tại");

        // Đối chiếu mật khẩu (Bcrypt băm mật khẩu nhập vào và so sánh với mật khẩu băm trong db)
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
        if (!isPasswordValid) return Unauthorized("Sai mật khẩu");

        // Nếu đúng, tạo Jwt Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("MaiHoangTung_Secret_Key_2026_Project_TaskManagement_SuperSecure");

        var tokenDesciptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDesciptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { token = tokenString, email = user.Email });
    }

    //Api Google Login
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        try
        {
            // Báo cáo cấu hình: Lấy ClientId từ appsettings.json (Lưu ý dùng dấu 2 chấm Google:ClientId)
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google:ClientId is missing.") }
            };

            // Giải mã và xác thực Token với Google
            var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);

            var userEmail = payload.Email;
            var userName = payload.Name;

            // KIỂM TRA & TẠO USER TỰ ĐỘNG
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            // Nếu user chưa có trong DB -> Tự động đăng ký
            if (user == null)
            {
                user = new User
                {
                    Email = userEmail,
                    FullName = userName,
                    Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()) // Băm một chuỗi ngẫu nhiên làm mật khẩu giả
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Sinh Token của hệ thống
            var tokenString = GenerateJwtToken(user);

            // Trả về cho Frontend y hệt như hàm Login thường
            return Ok(new { token = tokenString, email = user.Email, message = $"Xin chào {userName}" });
        }
        catch (InvalidJwtException)
        {
            return BadRequest(new { message = "Token Google không hợp lệ hoặc đã hết hạn" });
        }
    }

    //Api register
    [HttpPost("register")] // HTTP route metadata: POST /api/auth/register
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // THÊM ĐOẠN NÀY ĐỂ DEBUG
        var connString = _context.Database.GetConnectionString();
        Console.WriteLine(">>> DEBUG CONNECTION STRING: " + connString);

        // 1. Kiểm tra xem Email này đã có ai đăng ký trong Database chưa
        // Dịch sang SQL ngầm: SELECT EXISTS(SELECT 1 FROM "Users" WHERE "Email" = dto.Email)
        var userExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (userExists)
        {
            return BadRequest("Email này đã được sử dụng!"); // Trả về HTTP 400
        }

        // 2. Băm bảo mật mật khẩu thô bằng BCrypt trước khi lưu xuống kho
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // 3. Khởi tạo đối tượng User mới từ bản thiết kế Entity
        var newUser = new User
        {
            Email = dto.Email,
            Password = passwordHash, // Lưu chuỗi đã băm bảo mật vĩnh viễn
            FullName = dto.FullName
        };

        // 4. Đưa đối tượng newUser vào danh sách theo dõi của "Tờ hóa đơn" DbContext
        _context.Users.Add(newUser);

        // 5. Bắn lệnh qua mạng xuống PostgreSQL để lưu trữ thật vào ổ đĩa cứng
        // Dịch sang SQL ngầm: INSERT INTO "Users" ("Email", "Password", ...) VALUES (...)
        await _context.SaveChangesAsync();

        return Ok("Đăng ký tài khoản thành công!"); // Trả về HTTP 200
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetMyProfile()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        return Ok(new
        {
            message = "Chúc mừng Tùng đã truy cập thành công vào khu vực bảo mật!",
            userEmail = email
        });
    }

    //Hàm sinh token dùng chung
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("MaiHoangTung_Secret_Key_2026_Project_TaskManagement_SuperSecure");

        var tokenDesciptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] 
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
                new Claim(ClaimTypes.Email, user.Email) 
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDesciptor);
        return tokenHandler.WriteToken(token);
    }
}