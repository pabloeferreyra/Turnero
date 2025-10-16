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
    try {
        // No interceptar requests que no sean GET (evita interferir con POST/PUT/DELETE como el login)
        if (event.request.method !== 'GET') {
            return; // dejar que el navegador maneje la solicitud normalmente
        }

        // Siempre devolver la petición de red por defecto (no caché ni respuesta personalizada)
        event.respondWith(fetch(event.request));
    }
    catch (e) {
        console.error('Service worker fetch error', e);
    }
});
