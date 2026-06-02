import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ //@Injecteble: Class này có thể được Angular DI container quản lý
  providedIn: 'root', //Tạo singleton toàn app
})

export class AuthService {
  
  constructor(private http: HttpClient) {

  }

  login(email: string, password: string) {

    return this.http.post(
      'http://localhost:5243/api/auth/login',
      {
        email: email,
        password: password
      }
    );
  }
}
