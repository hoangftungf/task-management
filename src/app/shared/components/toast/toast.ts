import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastMessage } from '../../../core/services/toast';
@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.html',
  styleUrls: ['./toast.css']
})
export class ToastComponent implements OnInit {
  toast: ToastMessage | null = null;

  constructor(
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    // Lắng nghe loa phát thanh, hễ có thông báo là cập nhật UI
    this.toastService.toast$.subscribe(msg => {
      this.toast = msg;

      this.cdr.detectChanges()
    });
  }
}