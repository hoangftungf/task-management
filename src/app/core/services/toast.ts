import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface ToastMessage {
  message: string;
  type: 'success' | 'error';
}

@Injectable({
  providedIn: 'root' // Đảm bảo Service này sống xuyên suốt vòng đời của app
})
export class ToastService {
  toast$ = new Subject<ToastMessage | null>();

  showSuccess(message: string) {
    this.toast$.next({ message, type: 'success' });
    this.autoHide();
  }

  showError(message: string) {
    this.toast$.next({ message, type: 'error' });
    this.autoHide();
  }

  private autoHide() {
    // Tự động xóa thông báo sau 3 giây, bất kể người dùng đang ở trang nào
    setTimeout(() => {
      this.toast$.next(null);
    }, 3000);
  }
}