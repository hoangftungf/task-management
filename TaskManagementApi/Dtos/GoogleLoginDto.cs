using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.Dtos
{
    public class GoogleLoginDto
    {
        [Required(ErrorMessage = "Token không được để trống.")]
        public string IdToken { get; set; } = string.Empty;
    }
}