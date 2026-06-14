import { ChangeDetectorRef, Component, OnInit, OnDestroy } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms'; import { CommonModule } from '@angular/common';
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
export class RegisterComponent implements OnInit, OnDestroy {

  registerForm!: FormGroup; // Khai báo một đối tượng quản lý Form dữ liệu
  otpForm!: FormGroup; // Form mới dành riêng cho OTP

  isOtpStep = false; // Cờ hiệu chuyển đổi 2 màn hình
  registeredEmail = ''; // Lưu tạm email để tý gửi lên cùng OTP

  isLoading = false; // Thêm biến trạng thái loading

  // BIẾN CHO GIAO DIỆN OTP 6 Ô
  otpValues: string[] = ['', '', '', '', '', ''];
  countdown: number = 60; // Đếm ngược 60 giây
  timerInterval: any;
  canResend: boolean = false;

  constructor(
    private fb: FormBuilder, // Tiêm FormBuilder để dựng form nhanh hơn
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    // 1. Khởi tạo cấu trúc Form và cài đặt các điều kiện rào cản (Validation)
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

    // 2. Form nhập OTP (Chỉ có 1 ô duy nhất, bắt buộc nhập đúng 6 số)
    this.otpForm = this.fb.group({
      otpCode: ['', [Validators.required, Validators.pattern('^[0-9]{6}$')]]
    });
  }

  // Hàm helper giúp gọi nhanh các thuộc tính ngoài file HTML để check lỗi
  get f() { return this.registerForm.controls; }

  // 2. Dọn dẹp bộ đếm khi chuyển trang để tránh rò rỉ bộ nhớ (Memory Leak)
  ngOnDestroy() {
    if (this.timerInterval) clearInterval(this.timerInterval);
  }

  // 3. HÀM KHỞI ĐỘNG BỘ ĐẾM
  startTimer() {
    this.countdown = 60;
    this.canResend = false;
    if (this.timerInterval) clearInterval(this.timerInterval);

    this.timerInterval = setInterval(() => {
      this.countdown--;
      this.cdr.detectChanges(); // Ép Angular vẽ lại số giây

      if (this.countdown <= 0) {
        clearInterval(this.timerInterval);
        this.canResend = true; // Mở khóa nút Gửi lại
        this.cdr.detectChanges();
      }
    }, 1000);
  }

  onRegister() {
    // 5. Chặn đứng hành vi bấm nút nếu form chưa hợp lệ
    if (this.registerForm.invalid) {
      this.toastService.showError('Vui lòng điền đúng và đầy đủ thông tin!');
      return;
    }

    // 3. BẬT LOADING: Khóa nút lại không cho bấm lần 2
    this.isLoading = true;

    // Lấy ra gói dữ liệu JSON sạch sẽ sau khi đã qua bộ lọc validation
    const registerData = this.registerForm.value;

    this.authService.register(registerData).subscribe({
      next: (res: any) => {

        this.isLoading = false; // Tắt loading

        // res can be a string or an object with a message property
        const successMessage = typeof res === 'string' ? res : (res?.message || "Chúng tôi đã gửi cho bạn một email kèm mã OTP. Hãy kiểm tra hòm thư của bạn.");
        this.toastService.showSuccess(successMessage); // Báo "Kiểm tra hộp thư..."

        // CHUYỂN TRẠNG THÁI GIAO DIỆN
        this.registeredEmail = registerData.email;
        this.isOtpStep = true;

        // GỌI HÀM ĐẾM NGƯỢC KHI ĐĂNG KÝ THÀNH CÔNG
        this.startTimer();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false; // Bị lỗi cũng phải tắt loading để người dùng bấm lại
        this.cdr.detectChanges();
        this.toastService.showError(err.error?.message || 'Đăng ký thất bại!');
      }
    });
  }

  // Hàm Xử lý khi người dùng bấm xác nhận OTP
  onVerifyOtp() {
    if (this.otpForm.invalid) {
      this.toastService.showError('Vui lòng nhập mã OTP gồm 6 chữ số hợp lệ!');
      return;
    }

    const payload = {
      email: this.registeredEmail, // Lấy email đã lưu ở bước trên
      otpCode: this.otpForm.value.otpCode
    };

    this.authService.verifyOtp(payload).subscribe({
      next: (res: any) => {
        this.toastService.showSuccess(res.message); // Báo "Xác thực thành công!"
        this.router.navigate(['/login']); // Đá về trang đăng nhập
      },
      error: (err) => {
        this.toastService.showError(err.error?.message || 'Xác thực thất bại!');
      }
    });
  }

  // 4. HÀM GỬI LẠI MÃ (Lợi dụng luôn luồng Register ở backend)
  onResendOtp() {
    if (!this.canResend) return;

    this.isLoading = true;
    this.authService.register(this.registerForm.value).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        this.toastService.showSuccess('Đã gửi lại mã OTP mới!');
        this.startTimer(); // Chạy lại bộ đếm 60s
      },
      error: (err) => {
        this.isLoading = false;
        this.toastService.showError('Lỗi gửi lại mã, vui lòng thử lại!');
        this.cdr.detectChanges();
      }
    });
  }

  // LOGIC XỬ LÝ GIAO DIỆN 6 Ô NHẬP TỰ ĐỘNG
  onOtpInput(event: any, index: number) {
    const val = event.target.value;
    this.otpValues[index] = val;
    this.otpForm.patchValue({ otpCode: this.otpValues.join('') });

    // Tự động nhảy sang ô tiếp theo nếu đã nhập
    if (val && index < 5) {
      document.getElementById(`otp-${index + 1}`)?.focus();
    }
  }

  onOtpKeydown(event: KeyboardEvent, index: number) {
    // Tự động lùi về ô trước đó khi bấm Backspace ở ô trống
    if (event.key === 'Backspace' && !this.otpValues[index] && index > 0) {
      document.getElementById(`otp-${index - 1}`)?.focus();
    }
  }

  onOtpPaste(event: ClipboardEvent) {
    event.preventDefault();
    const paste = event.clipboardData?.getData('text')?.trim() || '';
    if (!/^\d+$/.test(paste)) return; // Chỉ cho phép dán số

    // Cắt lấy tối đa 6 số và rải đều vào các ô
    const chars = paste.slice(0, 6).split('');
    chars.forEach((char, i) => {
      this.otpValues[i] = char;
      const input = document.getElementById(`otp-${i}`) as HTMLInputElement;
      if (input) input.value = char;
    });

    this.otpForm.patchValue({ otpCode: this.otpValues.join('') });

    // Đưa con trỏ nháy về ô cuối cùng sau khi dán
    const focusIndex = chars.length < 6 ? chars.length : 5;
    document.getElementById(`otp-${focusIndex}`)?.focus();
  }
}