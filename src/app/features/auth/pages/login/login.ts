import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth';
import { LoginRequest } from '../../interfaces/auth';
import { ToastService } from '../../../../core/services/toast';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {
  
  loginData: LoginRequest = { email: '', password: '' };

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService // Bơm Loa trung tâm vào
  ) {}

  onLogin() {
    if (!this.loginData.email || !this.loginData.password) {
      this.toastService.showError('Vui lòng nhập email và mật khẩu!');
      return;
    }

    this.authService.login(this.loginData).subscribe({
      next: (response: any) => {
        // 1. Cất Token
        localStorage.setItem('token', response.token); 

        console.log('token: ', response.token, 'email: ', response.email)
        
        // 2. Phát loa thông báo đăng nhập thành công
        this.toastService.showSuccess('Đăng nhập thành công!');

        // 3. Bay thẳng vào trang hệ thống
        this.router.navigate(['/tasks']); 
      },
      error: (err) => {
        if (err.status === 401 || err.status === 400) {
          this.toastService.showError(err.error || 'Sai tài khoản hoặc mật khẩu!');
        } else {
          this.toastService.showError('Lỗi kết nối đến máy chủ!');
        }
      }
    });
  }
}