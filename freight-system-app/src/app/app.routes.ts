import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./modules/auth/login.component').then((c) => c.LoginComponent),
  },
  {
    path: 'shipments',
    loadComponent: () => import('./modules/shipments/components/shipment-list/shipment-list.component').then((c) => c.ShipmentListComponent),
  },
  {
    path: 'shipments/new',
    loadComponent: () => import('./modules/shipments/components/shipment-form/shipment-form.component').then((c) => c.ShipmentFormComponent),
  },
  {
    path: 'shipments/:id/edit',
    loadComponent: () => import('./modules/shipments/components/shipment-form/shipment-form.component').then((c) => c.ShipmentFormComponent),
  },
  {
    path: 'customers',
    loadComponent: () => import('./modules/customers/components/customer-list/customer-list.component').then((c) => c.CustomerListComponent),
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
    path: 'documents',
    loadComponent: () => import('./modules/documents/documents.component').then((c) => c.DocumentsComponent),
  },
  { path: '', redirectTo: 'shipments', pathMatch: 'full' },
  { path: '**', redirectTo: 'shipments' },
];
