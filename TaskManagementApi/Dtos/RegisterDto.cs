using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.Dtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải dài tối thiểu 6 ký tự.")]
        // Khớp 100% luật Regex với Angular: Ít nhất 1 chữ hoa, 1 chữ thường, 1 chữ số
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).*$", 
            ErrorMessage = "Mật khẩu quá yếu! Phải bao gồm cả chữ hoa, chữ thường và chữ số.")]
        public string Password { get; set; } = string.Empty;
    }
}