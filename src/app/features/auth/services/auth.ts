import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environment';
import { LoginRequest, LoginResponse, RegisterDto } from '../interfaces/auth';

@Injectable({ //@Injecteble: Class này có thể được Angular DI container quản lý
  providedIn: 'root', //Tạo singleton toàn app
})

export class AuthService {

  private authApiUrl = `${environment.apiUrl}/auth`; // đường dẫn gốc: http://localhost:5243/api/auth

  constructor(private http: HttpClient) {

  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.authApiUrl}/login`, request)
      .pipe(
        tap(response => {
          // Khi Backend trả về 200 OK, lưu ngay Token và Email vào trình duyệt
          localStorage.setItem('token', response.token);
          localStorage.setItem('email', response.email);
        })
      );
  }

  //Gửi idToken xuống BE
  googleLogin(idToken: string): Observable<any> {
    // Đóng gói thành object { idToken: "chuỗi_dài_ngoằng" } để khớp với GoogleLoginDto ở Backend
    return this.http.post(`${this.authApiUrl}/google-login`, { idToken: idToken});
  }

  register(dto: RegisterDto): Observable<string> {
    return this.http.post(`${this.authApiUrl}/register`, dto, { responseType: 'text'});
  }

  // Hàm bổ trợ lấy Token ra để dùng cho các bài sau (lấy danh sách Task)
  getToken(): string | null {
    return localStorage.getItem('token');
  }

  // Hàm xóa token khi người dùng nhấn Logout
  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('email');
  }
}
