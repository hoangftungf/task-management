using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Entities;
using TaskManagementApi.Dtos;

namespace TaskManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // <--- Khóa toàn bộ Controller này lại, bắt buộc phải có Token mới gọi được
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        // --- 1. API: LẤY DANH SÁCH TASK CỦA USER ĐANG ĐĂNG NHẬP ---
        [HttpGet]
        public async Task<IActionResult> GetAllMyTasks()
        {
            int userId = GetCurrentUserId();

            // Chỉ lọc ra những Task nào có UserId trùng với người đang gọi API
            var tasks = await _context.TaskItems
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tasks);
        }

        // --- 2. API: TẠO MỚI MỘT TASK ---
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            int userId = GetCurrentUserId();

            var newTask = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                UserId = userId // Gắn chặt Task này vào User đang đăng nhập
            };

            _context.TaskItems.Add(newTask);
            await _context.SaveChangesAsync();

            return Ok(newTask);
        }

        // --- 3. API: CẬP NHẬT TASK
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
        {
            int userId = GetCurrentUserId();

            //Tìm đúng Task có Id trùng và phải thuộc về user đang đăng nhập
            var task = await _context.TaskItems.FirstOrDefaultAsync
            (
                t => t.Id == id && t.UserId == userId
            );

            if (task == null)
            {
                return NotFound("Không tìm thấy công việc hoặc bạn không có quyền sửa công việc này.");
            }

            //Cập nhật thông tin mới từ DTO vào Entity
            task.Title = dto.Title;
            task.Description = dto.Description;
            task.IsCompleted = dto.IsCompleted;

            await _context.SaveChangesAsync();
            return Ok(task);
        }

        // ---4. API: XÓA TASK---
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            int userId = GetCurrentUserId();

            var task = await _context.TaskItems.FirstOrDefaultAsync
            (
                t => t.Id == id && t.UserId == userId
            );

            if (task == null)
            {
                return NotFound("Không tìm thấy công việc hoặc bạn không có quyền xóa công việc này.");
            }

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Xóa công việc thành công!"
            });
        }

        // --- HÀM TRỢ GIÚP: Trích xuất UserId từ JWT Token ---
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("Không tìm thấy định danh người dùng trong Token.");
            }
            return int.Parse(userIdClaim);
        }
    }
}