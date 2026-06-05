using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Entities;

namespace TaskManagementApi.Data
{
    // Lớp này phải kế thừa từ DbContext của EF Core
    public class AppDbContext : DbContext
    {
        // Constructor này dùng để nhận các cấu hình (như Connection String) từ bên ngoài truyền vào
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        // Khai báo bảng dữ liệu: DbSet<User> đại diện cho bảng sẽ tên là "Users" trong Database
        public DbSet<User> Users { get; set; } //Khai bao bang Users
        public DbSet<TaskItem> TaskItems { get; set; } //Khai bao bang TaskItem
    }
}