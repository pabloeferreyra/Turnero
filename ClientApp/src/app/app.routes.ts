import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'turns'
  },
  {
    path: 'turns',
    loadComponent: () => import('./features/turns/turns-page.component').then((m) => m.TurnsPageComponent)
  },
  {
    path: '**',
    redirectTo: 'turns'
  }
];
