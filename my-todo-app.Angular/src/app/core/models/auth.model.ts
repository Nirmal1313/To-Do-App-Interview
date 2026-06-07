export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface LoginResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  jti: string;
}
