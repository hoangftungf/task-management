import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tasks.html',
  styleUrls: ['./tasks.css']
})
export class TasksComponent {
  // Dữ liệu tĩnh để test giao diện trước khi nối API
  taskList = [
    { id: 1, title: 'Thiết kế Database PostgreSQL', isCompleted: true },
    { id: 2, title: 'Làm luồng Đăng nhập / Đăng ký', isCompleted: true },
    { id: 3, title: 'Dựng giao diện trang Tasks', isCompleted: false },
    { id: 4, title: 'Làm Interceptor gắn Token', isCompleted: false }
  ];

  newTaskTitle: string = '';

  constructor(private router: Router) {}

  // Nút đăng xuất tạm thời
  onLogout() {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
}