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
    path: 'analytics/geofences',
    loadComponent: () => import('./modules/analytics/geofence-management.component').then((c) => c.GeofenceManagementComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'analytics/eta-risk',
    loadComponent: () => import('./modules/analytics/eta-risk-dashboard.component').then((c) => c.EtaRiskDashboardComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'analytics/warehouse',
    loadComponent: () => import('./modules/analytics/warehouse-report.component').then((c) => c.WarehouseReportComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'analytics/ml',
    loadComponent: () => import('./modules/analytics/ml-summary.component').then((c) => c.MlSummaryComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'analytics/vehicles',
    loadComponent: () => import('./modules/analytics/vehicle-fleet-dashboard.component').then((c) => c.VehicleFleetDashboardComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'analytics/optimize',
    loadComponent: () => import('./modules/analytics/route-optimizer.component').then((c) => c.RouteOptimizerComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'analytics/time-to-failure',
    loadComponent: () => import('./modules/analytics/time-to-failure.component').then((c) => c.TimeToFailureComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'analytics/notifications',
    loadComponent: () => import('./modules/analytics/maintenance-notifications.component').then((c) => c.MaintenanceNotificationsComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'analytics/cockpit',
    loadComponent: () => import('./modules/analytics/operations-cockpit.component').then((c) => c.OperationsCockpitComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'analytics/dispatch-history',
    loadComponent: () => import('./modules/analytics/dispatch-history.component').then((c) => c.DispatchHistoryComponent),
    canActivate: [() => import('./core/auth.guard').then(m => m.AuthGuard)]
  },
  {
    path: 'analytics/digital-twin',
    loadComponent: () => import('./modules/analytics/digital-twin.component').then((c) => c.DigitalTwinComponent),
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
