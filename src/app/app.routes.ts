import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/pages/login/login';
import { RegisterComponent } from './features/auth/pages/register/register';
import { TasksComponent } from './features/tasks/pages/tasks/tasks';
import { authGuard } from './core/guards/auth-guard';


// File này dùng để cấu hình đường dẫn các trang trong dự án
export const routes: Routes = [
    {
        // Cấu hình đường dẫn login
        path: 'login',
        component: LoginComponent
    },
    {
        // Cấu hình đường dẫn register
        path: 'register',
        component: RegisterComponent
    },
    {
        path: 'tasks',
        component: TasksComponent,
        canActivate: [authGuard]
    },
    {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
    }
];
