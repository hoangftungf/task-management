import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CreateTaskRequest, TaskItem, UpdateTaskRequest } from '../../interfaces/task';
import { TaskService } from '../../services/task';
import { ToastService } from '../../../../core/services/toast';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tasks.html',
  styleUrls: ['./tasks.css']
})
export class TasksComponent implements OnInit {

  taskList: TaskItem[] = []; //Mang chua du lieu that tu DB
  newTaskTitle: string = '';
  newTaskDescription: string = ''; //Them mo ta neu muon dung form day du

  constructor(
    private taskService: TaskService,
    private toastService: ToastService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadTasks();
  }

  //Ham tai danh sach Task tu BE
  loadTasks() {
    this.taskService.getAllMyTasks().subscribe({
      next: (tasks) => {
        this.taskList = tasks;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.toastService.showError('Không thể tải danh sách công việc!');
      }
    });
  }

  //Hàm thêm mới Task
  onAddTask() {
    if (!this.newTaskTitle.trim()) {
      this.toastService.showError('Vui lòng nhập tiêu đề công việc!');
      return;
    }

    const newDto: CreateTaskRequest = {
      title: this.newTaskTitle,
      description: this.newTaskDescription
    };

    this.taskService.createTask(newDto).subscribe({
      next: (createdTask) => {
        this.taskList.unshift(createdTask); //Đẩy task mới lên đầu mảng để hiển thị ngay
        this.newTaskTitle = ''; //Reset input
        this.newTaskDescription = '';

        this.cdr.detectChanges();
        this.toastService.showSuccess("Thêm công việc thành công!");
      },
      error: (err) => {
        this.toastService.showError("Thêm công việc thất bại!");
      }
    });
  }

  //Hàm tick chọn hoàn thành / chưa hoàn thành
  onToggleComplete(task: TaskItem) {
    const updateDto: UpdateTaskRequest = {
      title: task.title,
      description: task.description,
      isCompleted: !task.isCompleted
    };

    this.taskService.updateTask(task.id, updateDto).subscribe({
      next: (updatedTask) => {
        task.isCompleted = updatedTask.isCompleted; //Cập nhật lại UI trạng thái

        this.cdr.detectChanges();
        this.toastService.showSuccess('Đã cập nhật trạng thái công việc!');
      },
      error: (err) => {
        this.toastService.showError('Không thể cập nhật trạng thái!');
      }
    });
  }

  //Hàm xóa Task
  onDeleteTask(id: number) {
    if (confirm('Bạn có chắc chắn muốn xóa công việc này không?')) {
      this.taskService.deleteTask(id).subscribe({
        next: () => {
          this.taskList = this.taskList.filter(t => t.id !== id); //Xóa khỏi mảng UI
          this.toastService.showSuccess('Xóa công việc thành công!');
        },
        error: (err) => {
          this.toastService.showError('Xóa công việc thất bại!');
        }
      });
    }
  }

  // Nút đăng xuất tạm thời
  onLogout() {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
}