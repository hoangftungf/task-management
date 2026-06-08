import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TaskItem, CreateTaskRequest, UpdateTaskRequest } from '../interfaces/task';
import { environment } from '../../../environment';

@Injectable({
  providedIn: 'root'
})
export class TaskService {

  private TaskApiUrl = `${environment.apiUrl}/tasks`; 

  constructor(private http: HttpClient) {}

  // 1. Lấy tất cả task (Interceptor sẽ tự kẹp Token vào đây)
  getAllMyTasks(): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(this.TaskApiUrl);
  }

  // 2. Tạo mới task
  createTask(dto: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(this.TaskApiUrl, dto);
  }

  // 3. Cập nhật task
  updateTask(id: number, dto: UpdateTaskRequest): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${this.TaskApiUrl}/${id}`, dto);
  }

  // 4. Xóa task
  deleteTask(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.TaskApiUrl}/${id}`);
  }
}