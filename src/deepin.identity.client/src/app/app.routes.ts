import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    {
        path: 'callback/:type',
        loadChildren: () => import('./callback/callback.routes')
    },
    {
        path: '',
        loadChildren: () => import('./accounts/accounts.routes')
    },
    {
        path: 'home',
        loadChildren: () => import('./home/home.routes'),
        canActivate: [authGuard]
    },
    {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full'
    }
];
