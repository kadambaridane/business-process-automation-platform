export interface AuthenticatedUser {
  id: number;
  fullName: string;
  email: string;
  username: string;
  roles: string[];
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: AuthenticatedUser;
}
