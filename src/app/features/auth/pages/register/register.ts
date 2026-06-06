import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth';
import { RegisterDto } from '../../interfaces/auth';
import { ToastService } from '../../../../core/services/toast';


@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent {
  
  registerData: RegisterDto = { fullName: '', email: '', password: '' };

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService // Bơm Loa trung tâm vào
  ) {}

  onRegister() {
    // Kiểm tra rỗng
    if (!this.registerData.fullName || !this.registerData.email || !this.registerData.password) {
      this.toastService.showError('Vui lòng điền đầy đủ thông tin!');
      return;
    }

    this.authService.register(this.registerData).subscribe({
      next: (response) => {
        // 1. Phát loa thông báo đăng ký thành công
        this.toastService.showSuccess('Đăng ký thành công! Vui lòng đăng nhập.');
        
        // 2. Chuyển thẳng sang trang Login NGAY LẬP TỨC. 
        // Component Toast ở ngoài sẽ tự lo việc hiển thị 3 giây.
        this.router.navigate(['/login']); 
      },
      error: (err) => {
        if (err.status === 400) {
          this.toastService.showError(err.error || 'Email này đã được sử dụng!');
        } else {
          this.toastService.showError('Lỗi kết nối đến máy chủ!');
        }
      }
    });
  }
}