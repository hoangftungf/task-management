using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Dtos;
using TaskManagementApi.Entities;

namespace TaskManagementApi.Controllers;

[ApiController] //giống decorator trong Angular @Controller
[Route("api/[controller]")] //routing configuration [controller] = auth vì classname là AuthController (framework tự bỏ hậu tố Controller)
public class AuthController : ControllerBase //kế thừa ControllerBase
{
    private readonly AppDbContext _context; //Khai báo một biến để chứa "Hóa đơn kết nối" Database

    // Dependency Injection (DI)
    //.Net DI Container sẽ tự động tạo AppDbContext và ném vào đây mỗi khi có Request tới
    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    //Api login
    [HttpPost("login")] //http route metadata POST /api/auth/login --> request routing
    public IActionResult Login([FromBody] LoginRequest request) //IActionResult: trả về 200 OK, 400 Bad Request...
    {
        Console.WriteLine(request.Email);
        Console.WriteLine(request.Password);

        return Ok(new //Ok() từ ControllerBase
        {
            token = "fake-jwt-token",
            email = request.Email
        });
    }

    //Api register
    [HttpPost("register")] // HTTP route metadata: POST /api/auth/register
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
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
}



public class LoginRequest //đây là DTO/Data Model, đại diện request data structure giống interface
{
    public string Email { get; set; } = ""; //get, set = cho phép đọc, ghi --> tính đóng gói (encapsulation)
    public string Password { get; set; } = "";
}