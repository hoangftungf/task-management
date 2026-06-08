import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../../../core/services/toast';
import { GoogleSigninButtonModule, SocialAuthService } from '@abacritt/angularx-social-login';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, GoogleSigninButtonModule, RouterModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent implements OnInit {
  //Khai báo đối tượng quản lý Form
  loginForm!: FormGroup;

  constructor(
    private fb: FormBuilder, // Tiêm FormBuilder
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router,
    private socialAuthService: SocialAuthService
  ) {}

  ngOnInit() {
    // 4. Định nghĩa cấu trúc Form và luật kiểm tra đầu vào cho Login
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]], // Bắt buộc và phải là định dạng email
      password: ['', [Validators.required]] // Chỉ cần bắt buộc nhập, không cần check độ dài hay regex ở đây
    });

    //Lắng nghe tín hiệu từ Google
    this.socialAuthService.authState.subscribe({
      next: (user) => {
        if (user) {
          //Khi Google trả về user, lập tức lấy idToken ném xuống BE
          this.authService.googleLogin(user.idToken).subscribe({
            next: (res: any) => {
              //Thành công! BE trả về JWT của hệ thống
              localStorage.setItem('token', res.token);

              //Hiện thông báo chào mừng từ BE
              this.toastService.showSuccess(res.message ||  "Đăng nhập bằng Google thành công!")

              //Chuyển hướng vào trang quản lý task
              this.router.navigate(['/tasks']);
            }
          })
        }
      },
      error: (err) => {
        this.toastService.showError('Đăng nhập bằng Google thất bại!');
      }
    })
  }

  // Hàm helper giúp gọi nhanh các trạng thái lỗi ngoài file HTML
  get f() { return this.loginForm.controls; }

  onLogin() {
    // Chặn đứng nếu người dùng cố tình lách luật để bấm nút
    if (this.loginForm.invalid) {
      this.toastService.showError('Vui lòng nhập đầy đủ và đúng thông tin đăng nhập!');
      return;
    }

    // Lấy gói dữ liệu JSON sạch
    const loginData = this.loginForm.value;

    this.authService.login(loginData).subscribe({
      next: (res: any) => {
        localStorage.setItem('token', res.token); // Lưu token vào kho
        this.toastService.showSuccess('Đăng nhập thành công!');
        this.router.navigate(['/tasks']); // Phi thẳng vào trang quản lý công việc
      },
      error: (err) => {
        this.toastService.showError(err.error?.message || 'Sai tài khoản hoặc mật khẩu!');
      }
    });
  }
}