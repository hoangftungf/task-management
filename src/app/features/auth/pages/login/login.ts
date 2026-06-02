import { Component } from '@angular/core';
import { Form, FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  email: string = '';
  password: string = '';
  constructor(private authService: AuthService) {

  }

  onLogin() {
    this.authService.login(
      this.email,
      this.password
    ).subscribe(response => { //Bắt đầu lắng nghe async result (xử lý bất đồng bộ)
      console.log(response);

    });
  }
}
