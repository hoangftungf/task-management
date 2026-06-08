import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth';
import { RegisterDto } from '../../interfaces/auth';
import { ToastService } from '../../../../core/services/toast';


@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent implements OnInit {
  // 3. Khai báo một đối tượng quản lý Form dữ liệu
  registerForm!: FormGroup;

  constructor(
    private fb: FormBuilder, // Tiêm FormBuilder để dựng form nhanh hơn
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    // 4. Khởi tạo cấu trúc Form và cài đặt các điều kiện rào cản (Validation)
    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]], // Bắt buộc nhập và phải đúng định dạng @
      password: ['', [
        Validators.required, 
        Validators.minLength(6),
        // Regex bắt buộc: Ít nhất 1 chữ hoa (?=.*[A-Z]), 1 chữ thường (?=.*[a-z]), 1 chữ số (?=.*[0-9])
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])/) 
      ]]
    });
  }

  // Hàm helper giúp gọi nhanh các thuộc tính ngoài file HTML để check lỗi
  get f() { return this.registerForm.controls; }

  onRegister() {
    // 5. Chặn đứng hành vi bấm nút nếu form chưa hợp lệ
    if (this.registerForm.invalid) {
      this.toastService.showError('Vui lòng điền đúng và đầy đủ thông tin!');
      return;
    }

    // Lấy ra gói dữ liệu JSON sạch sẽ sau khi đã qua bộ lọc validation
    const registerData = this.registerForm.value;

    this.authService.register(registerData).subscribe({
      next: (res) => {
        this.toastService.showSuccess('Đăng ký tài khoản thành công!');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.toastService.showError(err.error?.message || 'Đăng ký thất bại, email có thể đã tồn tại!');
      }
    });
  }
}