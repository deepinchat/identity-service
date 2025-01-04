import { Route } from "@angular/router";
import { AccountsComponent } from "./accounts.component";

export default [
    {
        path: '',
        component: AccountsComponent,
        children: [
            { path: 'signin', loadComponent: () => import('./login/login.component').then(m => m.LoginComponent) },
            { path: 'signup', loadComponent: () => import('./register/register.component').then(c => c.RegisterComponent) },
            { path: 'signout', loadComponent: () => import('./logout/logout.component').then(c => c.LogoutComponent) },
            { path: 'forgot-password', loadComponent: () => import('./forgot-password/forgot-password.component').then(c => c.ForgotPasswordComponent) },
            { path: 'confirm-register', loadComponent: () => import('./register-confirmation/register-confirmation.component').then(c => c.RegisterConfirmationComponent) },
            { path: 'reset-password', loadComponent: () => import('./reset-password/reset-password.component').then(c => c.ResetPasswordComponent) },
        ]
    }
] satisfies Route[];
