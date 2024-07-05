// service-worker.js

// Instalar el Service Worker
self.addEventListener('install', function (event) {
    console.log('Service Worker instalado');
});

// Activar el Service Worker
self.addEventListener('activate', function (event) {
    console.log('Service Worker activado');
});

// Intercepta las solicitudes de red
self.addEventListener('fetch', function (event) {
    console.log('Interceptando solicitud: ', event.request.url);
});