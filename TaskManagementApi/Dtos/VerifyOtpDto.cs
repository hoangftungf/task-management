using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.Dtos
{
    public class VerifyOtpDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có đúng 6 chữ số.")]
        public string OtpCode { get; set; } = string.Empty;
    }
}