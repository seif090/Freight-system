import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'shipments',
    loadComponent: () => import('./modules/shipments/components/shipment-list/shipment-list.component').then((c) => c.ShipmentListComponent),
  },
  {
    path: 'customers',
    loadComponent: () => import('./modules/customers/components/customer-list/customer-list.component').then((c) => c.CustomerListComponent),
  },
  { path: '', redirectTo: 'shipments', pathMatch: 'full' },
  { path: '**', redirectTo: 'shipments' },
];
