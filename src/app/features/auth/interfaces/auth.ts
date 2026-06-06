export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  email: string;
}

export interface RegisterDto {
  email: string;
  password: string;
  fullName: string;
}