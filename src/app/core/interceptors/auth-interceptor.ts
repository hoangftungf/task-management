import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // 1. Móc token từ trong localStorage ra
  const token = localStorage.getItem('token');

  // 2. Nếu có token, tiến hành "nhét" nó vào Header của request
  if (token) {
    // Lưu ý: Request trong Angular là "Immutability" (không thể sửa trực tiếp)
    // Nên ta phải dùng lệnh .clone() để tạo bản sao và thêm Header Authorization vào
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    
    console.log('>>> Interceptor đã kẹp Token vào Request:', req.url);
    return next(authReq); // Thả cho request đã kẹp token bay đi
  }

  // 3. Nếu không có token (ví dụ lúc gọi API Login/Register), cứ để request đi tự nhiên
  return next(req);
};