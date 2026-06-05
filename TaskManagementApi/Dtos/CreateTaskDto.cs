using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.Dtos
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Tiêu đề công việc không được để trống.")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá {1} ký tự.")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}