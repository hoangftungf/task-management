using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using TaskManagementApi.Data;
using TaskManagementApi.Dtos;
using TaskManagementApi.Entities;
using TaskManagementApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;

namespace TaskManagementApi.Controllers;

[ApiController] //giống decorator trong Angular @Controller
[Route("api/[controller]")] //routing configuration [controller] = auth vì classname là AuthController (framework tự bỏ hậu tố Controller)
public class AuthController : ControllerBase //kế thừa ControllerBase
{
    private readonly AppDbContext _context; //Khai báo một biến để chứa "Hóa đơn kết nối" Database
    private readonly IConfiguration _configuration;

    private readonly IEmailService _emailService;
    // Dependency Injection (DI)
    //.Net DI Container sẽ tự động tạo AppDbContext và ném vào đây mỗi khi có Request tới
    public AuthController(AppDbContext context, IConfiguration configuration, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    //Api register
    [HttpPost("register")] // HTTP route metadata: POST /api/auth/register
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var userEmail = dto.Email.Trim().ToLower();

        // Thay vì dùng AnyAsync, chúng ta lấy hẳn user ra để kiểm tra
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == userEmail);

        // Sinh mã OTP và băm mật khẩu chuẩn bị sẵn
        var random = new Random();
        string generatedOtp = random.Next(100000, 999999).ToString();
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // NẾU EMAIL ĐÃ TỒN TẠI TRONG DB
        if (existingUser != null)
        {
            if (existingUser.IsEmailVerified)
            {
                // Trường hợp 1: Đã xác thực thành công từ trước -> Chặn không cho đăng ký lại
                return BadRequest(new { message = "Email này đã được sử dụng!" });
            }
            else
            {
                // Trường hợp 2: Bị kẹt ở trạng thái chưa xác thực (Limbo)
                // -> Cập nhật lại thông tin mới nhất, cấp OTP mới và gia hạn thời gian
                existingUser.Password = passwordHash;
                existingUser.FullName = dto.FullName;
                existingUser.OtpCode = generatedOtp;
                existingUser.OtpExpiryTime = DateTime.UtcNow.AddMinutes(10);

                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();

                // (Không return ở đây, để code chạy tiếp xuống phần gửi mail bên dưới)
            }
        }
        else
        {
            // NẾU EMAIL HOÀN TOÀN MỚI
            var newUser = new User
            {
                Email = userEmail,
                Password = passwordHash,
                FullName = dto.FullName,
                IsEmailVerified = false,
                OtpCode = generatedOtp,
                OtpExpiryTime = DateTime.UtcNow.AddMinutes(10)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
        }

        // 5. TIẾN HÀNH GỬI EMAIL BẤT ĐỒNG BỘ
        try
        {
            string subject = "Mã OTP Xác Thực Tài Khoản - TaskManagement";

            // Thiết kế giao diện Email HTML sạch sẽ, chuyên nghiệp
            string emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #007bff; text-align: center; margin-bottom: 20px;'>XÁC THỰC TÀI KHOẢN</h2>
                    <p>Xin chào <strong>{dto.FullName}</strong>,</p>
                    <p>Cảm ơn bạn đã lựa chọn hệ thống của chúng tôi. Dưới đây là mã OTP để hoàn tất quá trình đăng ký tài khoản của bạn:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <span style='font-size: 26px; font-weight: bold; color: #28a745; letter-spacing: 6px; padding: 12px 25px; background-color: #f8f9fa; border: 2px dashed #28a745; border-radius: 4px;'>
                            {generatedOtp}
                        </span>
                    </div>
                    <p style='color: #dc3545; font-size: 13px;'>* Lưu ý: Mã OTP này chỉ có hiệu lực trong vòng <strong>10 phút</strong> và tuyệt đối không chia sẻ cho bất kỳ ai.</p>
                    <hr style='border: none; border-top: 1px solid #eeeeee; margin: 20px 0;'>
                    <p style='font-size: 11px; color: #888888; text-align: center;'>Đây là email tự động từ hệ thống bảo mật, vui lòng không phản hồi email này.</p>
                </div>";

            await _emailService.SendEmailAsync(userEmail, subject, emailBody);
        }
        catch (Exception ex)
        {
            // Tránh việc gửi mail lỗi làm sập cả luồng trả về, ghi log để kiểm tra App Password
            Console.WriteLine($">>> LỖI GỬI MAIL OTP: {ex.Message}");
            return Ok(new { message = "Đăng ký thành công nhưng hệ thống gặp sự cố gửi mail. Vui lòng yêu cầu gửi lại OTP." });
        }

        return Ok(new { message = "Đăng ký tài khoản thành công! Vui lòng kiểm tra hộp thư đến để lấy mã OTP xác thực." });
    }

    //Api login
    [HttpPost("login")] //http route metadata POST /api/auth/login --> request routing
    public async Task<IActionResult> Login([FromBody] LoginRequest request) //IActionResult: trả về 200 OK, 400 Bad Request...
    {
        // Tìm user trong DB
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return Unauthorized("Email không tồn tại");

        // CHẶN CỬA: Nếu chưa kích hoạt email thì không cho login!
        if (!user.IsEmailVerified)
        {
            return BadRequest(new { message = "Tài khoản của bạn chưa được xác thực qua OTP. Vui lòng xác thực trước khi đăng nhập." });
        }

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

            var userEmail = payload.Email.Trim().ToLower();
            var userName = payload.Name;

            // KIỂM TRA & TẠO USER TỰ ĐỘNG
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == userEmail);

            // Nếu user chưa có trong DB -> Tự động đăng ký
            if (user == null)
            {
                user = new User
                {
                    Email = userEmail,
                    FullName = userName,
                    Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Băm một chuỗi ngẫu nhiên làm mật khẩu giả
                    IsEmailVerified = true //Google đã xác thực hộ vậy nên không cần xác thực nữa
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


    // API XÁC THỰC MÃ OTP (VERIFY OTP)
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var userEmail = dto.Email.Trim().ToLower();

        // Tìm kiếm user tương ứng
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == userEmail);
        if (user == null)
            return NotFound(new { message = "Không tìm thấy thông tin tài khoản." });

        if (user.IsEmailVerified)
            return BadRequest(new { message = "Tài khoản này đã được xác thực từ trước." });

        // Kiểm tra xem mã OTP có khớp không
        if (user.OtpCode != dto.OtpCode)
            return BadRequest(new { message = "Mã OTP không chính xác." });

        // Kiểm tra xem OTP còn hạn không (So sánh với thời gian UTC hiện tại)
        if (user.OtpExpiryTime < DateTime.UtcNow)
            return BadRequest(new { message = "Mã OTP đã hết hạn sử dụng. Vui lòng yêu cầu mã mới." });

        // ĐỐI CHIẾU THÀNH CÔNG -> KÍCH HOẠT TÀI KHOẢN
        user.IsEmailVerified = true;
        user.OtpCode = null;       // Xóa mã cũ đi chống tái sử dụng
        user.OtpExpiryTime = null; // Xóa hạn dùng

        await _context.SaveChangesAsync();

        return Ok(new { message = "Xác thực tài khoản thành công! Bây giờ bạn đã có thể đăng nhập." });
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