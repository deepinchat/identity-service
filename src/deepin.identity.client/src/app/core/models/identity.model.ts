export interface LoginRequest {
    username: string;
    password: string;
    rememberLogin: boolean;
}

export interface RegisterRequest {
    username: string;
    email: string;
    password: string;
    confirmPassword: string;
}

export interface ResetPasswordRequest {
    email: string;
    password: string;
    confirmPassword: string;
    code: string;
}

export interface ForgotPasswordRequest {
    email: string;
}

export interface LoginWithTwoFactorRequest {
    twoFactorCode: string;
    rememberMe: boolean;
    rememberMachine: boolean;
}