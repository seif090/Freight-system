import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./modules/auth/login.component').then((c) => c.LoginComponent),
  },
  {
    path: 'shipments',
    loadComponent: () => import('./modules/shipments/components/shipment-list/shipment-list.component').then((c) => c.ShipmentListComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'shipments/new',
    loadComponent: () => import('./modules/shipments/components/shipment-form/shipment-form.component').then((c) => c.ShipmentFormComponent),
    canActivate: [() => import('./core/role.guard').then(m => m.RoleGuard)],
    data: { roles: ['Admin', 'Operation'] }
  },
  {
    path: 'shipments/:id/edit',
    loadComponent: () => import('./modules/shipments/components/shipment-form/shipment-form.component').then((c) => c.ShipmentFormComponent),
    canActivate: [() => import('./core/role.guard').then(m => m.RoleGuard)],
    data: { roles: ['Admin', 'Operation'] }
  },
  {
    path: 'customers',
    loadComponent: () => import('./modules/customers/components/customer-list/customer-list.component').then((c) => c.CustomerListComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'customers/new',
    loadComponent: () => import('./modules/customers/components/customer-form/customer-form.component').then((c) => c.CustomerFormComponent),
  },
  {
    path: 'customers/:id/edit',
    loadComponent: () => import('./modules/customers/components/customer-form/customer-form.component').then((c) => c.CustomerFormComponent),
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./modules/reports/dashboard.component').then((c) => c.DashboardComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'documents',
    loadComponent: () => import('./modules/documents/documents.component').then((c) => c.DocumentsComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'search',
    loadComponent: () => import('./modules/search/search.component').then((c) => c.SearchComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'analytics',
    loadComponent: () => import('./modules/analytics/analytics.component').then((c) => c.AnalyticsComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'map',
    loadComponent: () => import('./modules/map/map.component').then((c) => c.MapComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'shipments' },
];
