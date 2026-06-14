# Task Management UI

Ứng dụng quản lý công việc với frontend Angular và backend ASP.NET Core Web API.

## Kiến trúc dự án

Dự án gồm hai phần chính:

### Frontend — Angular 21

```
src/
├── app/
│   ├── core/
│   │   ├── guards/          # Auth guard — bảo vệ route
│   │   ├── interceptors/    # Auth interceptor — gắn JWT vào request
│   │   └── services/        # Toast service — thông báo
│   ├── features/
│   │   ├── auth/            # Đăng nhập, đăng ký, Google login
│   │   └── tasks/           # Quản lý công việc CRUD
│   └── shared/
│       └── components/      # Toast component dùng chung
├── index.html
├── main.ts
└── styles.css
```

### Backend — ASP.NET Core 9 Web API

```
TaskManagementApi/
├── Controllers/
│   ├── AuthController.cs    # Xác thực (login, register, Google, OTP)
│   └── TasksController.cs   # CRUD công việc
├── Data/
│   └── AppDbContext.cs      # DbContext Entity Framework
├── Dtos/                    # Data Transfer Objects
├── Entities/
│   ├── TaskItem.cs
│   └── User.cs
├── Migrations/              # EF Core migrations
├── Services/
│   ├── IEmailService.cs
│   └── EmailService.cs      # Gửi email OTP qua MailKit
└── Program.cs
```

## Công nghệ sử dụng

### Frontend

| Công nghệ | Phiên bản |
|---|---|
| Angular | 21 |
| TypeScript | 5.9 |
| Angular Router | Quản lý điều hướng |
| Angular Forms | Xử lý form |
| angularx-social-login | Đăng nhập Google |
| Vitest | Kiểm thử |

### Backend

| Công nghệ | Phiên bản |
|---|---|
| .NET | 9.0 |
| Entity Framework Core | 9.0 |
| PostgreSQL (Npgsql) | 9.0 |
| JWT Bearer Authentication | 9.0 |
| BCrypt.Net-Next | 4.2 |
| MailKit | 4.17 |
| Google.Apis.Auth | 1.75 |
| OpenAPI | 9.0 |

## Tính năng

- **Xác thực người dùng**: Đăng ký, đăng nhập, đăng nhập bằng Google
- **Xác thực OTP**: Gửi mã OTP qua email khi đăng ký
- **JWT Authentication**: Bảo mật API bằng JWT bearer token
- **Quản lý công việc**: Tạo, xem, sửa, xoá công việc
- **Bảo vệ route**: Auth guard chặn truy cập trái phép
- **Toast notification**: Thông báo cho người dùng

## Cài đặt và chạy

### Yêu cầu

- Node.js 22+
- Angular CLI 21+
- .NET 9.0 SDK
- PostgreSQL

### Backend

```bash
# Di chuyển vào thư mục backend
cd TaskManagementApi

# Cấu hình chuỗi kết nối trong appsettings.json
# Sửa "ConnectionStrings:DefaultConnection" cho phù hợp

# Chạy migration
dotnet ef database update

# Khởi chạy API
dotnet run
```

API sẽ chạy tại `http://localhost:5000` (cấu hình trong `Properties/launchSettings.json`).

### Frontend

```bash
# Cài đặt dependencies
npm install

# Khởi chạy dev server
ng serve
```

Truy cập `http://localhost:4200/`. Ứng dụng sẽ tự động reload khi có thay đổi.

## Scripts

| Lệnh | Mô tả |
|---|---|
| `ng serve` | Chạy dev server |
| `ng build` | Build dự án ra thư mục `dist/` |
| `ng test` | Chạy unit tests (Vitest) |
| `ng generate component <tên>` | Tạo component mới |
| `dotnet run` | Chạy backend API |
| `dotnet ef database update` | Cập nhật database |

## API Endpoints

### Auth

| Method | Endpoint | Mô tả |
|---|---|---|
| POST | `/api/auth/register` | Đăng ký tài khoản |
| POST | `/api/auth/login` | Đăng nhập |
| POST | `/api/auth/verify-otp` | Xác thực OTP |
| POST | `/api/auth/google-login` | Đăng nhập Google |

### Tasks

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/tasks` | Lấy danh sách công việc |
| GET | `/api/tasks/{id}` | Lấy chi tiết công việc |
| POST | `/api/tasks` | Tạo công việc mới |
| PUT | `/api/tasks/{id}` | Cập nhật công việc |
| DELETE | `/api/tasks/{id}` | Xoá công việc |
