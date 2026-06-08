import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router); // Tiêm Router để điều hướng linh hoạt
  
  // Kiểm tra xem trong kho đã có Token chưa
  const token = localStorage.getItem('token');

  if (token) {
    // 1. Có token rồi -> Hợp pháp! Cho phép đi tiếp vào trang
    return true; 
  }

  // 2. Chưa đăng nhập mà đòi vào ké? 
  console.warn('>>> Cảnh báo an ninh: Truy cập trái phép! Đang đá về trang Login...');
  router.navigate(['/login']); // Điều hướng người dùng về trang đăng nhập
  return false; // Chặn đứng tuyến đường này lại
};