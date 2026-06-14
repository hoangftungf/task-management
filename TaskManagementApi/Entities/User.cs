namespace TaskManagementApi.Entities
{
    public class User
    {
        // EF Core sẽ tự hiểu thuộc tính có tên là "Id" (hoặc "UserId") là Primary Key (Khóa chính)
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        // Giai đoạn này ta lưu tạm Password thô, Phase 10 sẽ học Password Hashing sau
        public string Password { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        //Trạng thái kích hoạt tài khoản (Mặc định là false khi mới đăng ký)
        public bool IsEmailVerified { get; set; } = false;

        //Mã OTP gồm 6 chữ số (Có thể để null nếu chưa gửi hoặc đã dùng xong)
        public string? OtpCode { get; set; }

        //Thời gian hết hạn của OTP
        public DateTime? OtpExpiryTime { get; set; }

        // Theo dõi ngày tạo tài khoản của User
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}