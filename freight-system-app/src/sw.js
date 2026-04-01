self.addEventListener('install', event => {
  event.waitUntil(self.skipWaiting());
});

self.addEventListener('activate', event => {
  event.waitUntil(self.clients.claim());
});

self.addEventListener('push', event => {
  const data = event.data?.json() || { title: 'Freight alert', body: 'New event' };
  const options = {
    body: data.body,
    icon: '/assets/icon-72x72.png',
    data: data,
    tag: 'freight-alert'
  };
  event.waitUntil(self.registration.showNotification(data.title, options));
});

self.addEventListener('notificationclick', event => {
  event.notification.close();
  event.waitUntil(self.clients.openWindow('/analytics/cockpit'));
});
