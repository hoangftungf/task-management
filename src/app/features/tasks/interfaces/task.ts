export interface TaskItem {
  id: number;
  title: string;
  description: string;
  isCompleted: boolean;
  createdAt: string;
  userId: number;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
}

export interface UpdateTaskRequest {
  title: string;
  description: string;
  isCompleted: boolean;
}