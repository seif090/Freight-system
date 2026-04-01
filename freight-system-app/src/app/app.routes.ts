import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'shipments',
    loadComponent: () => import('./modules/shipments/components/shipment-list/shipment-list.component').then((c) => c.ShipmentListComponent),
  },
  { path: '', redirectTo: 'shipments', pathMatch: 'full' },
  { path: '**', redirectTo: 'shipments' },
];
