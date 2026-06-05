import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth'; // ──> Import đúng AuthService chung thư mục tính năng
import { Router } from '@angular/router';
import { LoginRequest } from '../../interfaces/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule], // ──> Nhớ giữ FormsModule để HTML dùng được [(ngModel)]
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {

  loginData: LoginRequest = {
    email: '',
    password: ''
  }

  errorMessage = ''; // Biến để hứng lỗi hiển thị ra màn hình

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  onLogin() {
    this.errorMessage = ''; // reset thông báo lỗi mỗi lần bấm lại

    if (!this.loginData.email || !this.loginData.password) {
      this.errorMessage = 'Vui lòng nhập đầy đủ tài khoản và mật khẩu.';
      return;
    }

    this.authService.login(this.loginData).subscribe({
      next: (response) => {
        console.log('Đăng nhập thành công!', response);
        // Sau khi có Token, chuyển hướng user sang trang danh sách tasks
        this.router.navigate(['/tasks']);
      },
      error: (err) => {
        // Hứng thông báo lỗi "Email không tồn tại" hoặc "Sai mật khẩu" từ .NET gửi qua
        if (err.status === 401 || err.status === 400) {
          this.errorMessage = err.error || 'Thông tin đăng nhập không chính xác.';
        } else {
          this.errorMessage = 'Không thể kết nối đến máy chủ Backend.';
        }
        console.error(err);
      }
    });
  }
}