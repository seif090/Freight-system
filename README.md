# Freight System (Logistics)

## Overview
Freight system is a Freight Forwarding & Logistics Management System built with:
- Backend: ASP.NET Core Web API (.NET 10)
- Frontend: Angular 19
- Database: SQL Server LocalDB
- SignalR: Live tracking
- Hangfire: Background jobs / notifications
- JWT: Auth & role-based security

## Backend Setup
1. افتح Terminal في `c:\Users\seaif\Desktop\Freight system`.
2. تأكد من وجود SQL Server LocalDB.
3. اعمل restore و build:
   - `dotnet restore`
   - `dotnet build`
4. طبق المايجريشن:
   - `cd src\FreightSystem.Infrastructure`
   - `dotnet ef database update --startup-project ..\FreightSystem.Api --context FreightDbContext`
5. شغل السيرفر:
   - `cd ..\FreightSystem.Api`
   - `dotnet run --urls "https://localhost:5001"`
6. افتح Swagger:
   - `https://localhost:5001/swagger/index.html`

### Default Users
- admin/Admin123! (Admin)
- operation/Op123! (Operation)
- accountant/Ac123! (Accountant)
- sales/Sales123! (Sales)

## Frontend Setup
1. افتح Terminal في `c:\Users\seaif\Desktop\Freight system\freight-system-app`.
2. ثبت الحزم:
   - `npm install`
3. شغل الواجهة:
   - `npm start`
   - يفتح على `http://localhost:4200`

## توصيف المهمات المتاحة
- بيانات العملاء CRUD.
- بيانات الشحن CRUD.
- تحميل المستندات (Documents upload) للشحنة.
- سجل عملاء وارسال إشعارات Hangfire.
- Live shipment tracking عبر SignalR.
- JWT auth + interceptor.

## APIs Demo (Postman)
- `postman_collection.json` يحتوي الطلبات:
  - Login
  - Get Customers
  - Create Customer

## e2e tests (Cypress)
1. `npm run e2e` لفتح UI.
2. `npm run e2e:run` للتشغيل بدون واجهة.

## تكملة UI
- الـ项目 يحتوي:
  - `/login` صفحة تسجيل الدخول.
  - `/shipments` جدول شحنات + Create/Edit/Delete.
  - `/customers` جدول عملاء + Create/Edit/Delete.
  - `/documents` رفع مستندات للشحنة.

## Validation
- Forms template-driven تستخدم `required`, `type='email'`, و تدقيق بسيط.
- UI يظهر رسالة على خطأ.

## New Features Added
- Dashboard API endpoint: `GET /api/v1.0/reports/dashboard`.
- Advanced reports: `GET /api/v1.0/reports/overdue`, `GET /api/v1.0/reports/top-customers`.
- Swagger x-description added via `XDescriptionAttribute` for Arabic/English description.
- Hangfire email/SMS mock notifications via `INotificationService`.
- Shipments overdue alert scheduler (Hangfire daily 02:00) via `ShipmentMonitoringService`.
- RBAC from DB: user/role/userrole entities plus dynamic JWT claim roles and `DbInitializer` seeding.
- Audit trail: request path/method/status saved to `AuditLogs` with middleware.
- Admin RBAC endpoints: `GET /api/v1.0/admin/roles`, `POST /api/v1.0/admin/roles`, `POST /api/v1.0/admin/users/{id}/roles`.

## Note
Hangfire dashboard: `https://localhost:5001/hangfire`
SignalR endpoint: `https://localhost:5001/hubs/tracking`

## Troubleshooting
- اذا ظهر تحذير شهادة تطور: `dotnet dev-certs https --trust`.
- اذا ظهرت مشكلة Lock على .dll تأكد سكربت `dotnet run` مقفل وليس يعمل أثناء `dotnet build`.
