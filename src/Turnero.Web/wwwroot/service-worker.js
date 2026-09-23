// service-worker.js
// Versión del caché — incrementar al cambiar los archivos cacheados
const CACHE_NAME = 'turnero-static-v1';

// Rutas locales de activos estáticos que se precachearán durante la instalación
const PRECACHE_PATHS = [
  '/offline.html',
  '/css/site.css',
  '/css/Login.css',
  '/css/table.css',
  '/js/modalUtils.js',
  '/js/common.js',
  '/js/site.js',
  '/manifest.json',
  '/favicon.ico'
];

// ─── Estrategias de caché ───────────────────────────────────────────────────

/**
 * Determina si la solicitud corresponde a un activo estático (CSS, JS, imágenes,
 * fuentes, manifest) que debe servirse con estrategia cache-first.
 */
function isStaticAsset(url) {
  const path = url.pathname;
  // Coincidir con rutas de precaché
  if (PRECACHE_PATHS.includes(path)) return true;
  // Coincidir por extensión de archivo
  return /\.(css|js|png|jpg|jpeg|gif|svg|ico|webp|woff2?|ttf|eot)$/i.test(path);
}

// ─── Evento: Instalación ────────────────────────────────────────────────────

self.addEventListener('install', function (event) {
  console.log('Service Worker: Instalando y precachéando activos estáticos...');
  event.waitUntil(
    caches.open(CACHE_NAME).then(function (cache) {
      return cache.addAll(PRECACHE_PATHS).catch(function (error) {
        console.error('Service Worker: Error al precachear:', error);
        // No bloquear la instalación si algún recurso falla
      });
    })
  );
  // Forzar la activación inmediata sin esperar a que todas las pestañas se cierren
  self.skipWaiting();
});

// ─── Evento: Activación ────────────────────────────────────────────────────

self.addEventListener('activate', function (event) {
  console.log('Service Worker: Activado. Limpiando cachés antiguos...');
  event.waitUntil(
    caches.keys().then(function (cacheNames) {
      return Promise.all(
        cacheNames.map(function (name) {
          if (name !== CACHE_NAME) {
            console.log('Service Worker: Eliminando caché antiguo:', name);
            return caches.delete(name);
          }
        })
      );
    }).then(function () {
      // Tomar control inmediato de todas las pestañas abiertas
      return self.clients.claim();
    })
  );
});

// ─── Evento: Fetch ──────────────────────────────────────────────────────────

self.addEventListener('fetch', function (event) {
  // Solo interceptar solicitudes GET
  if (event.request.method !== 'GET') return;

  const url = new URL(event.request.url);

  // ── Estrategia Cache-First para activos estáticos ────────────────────────
  if (isStaticAsset(url)) {
    event.respondWith(
      caches.match(event.request).then(function (cachedResponse) {
        // Devolver desde caché si existe
        if (cachedResponse) {
          return cachedResponse;
        }
        // Si no está en caché, obtener de la red y almacenar para futuras visitas
        return fetch(event.request).then(function (networkResponse) {
          // Verificar que la respuesta sea válida antes de cachear
          if (!networkResponse || networkResponse.status !== 200 || networkResponse.type === 'opaque') {
            return networkResponse;
          }
          // Clonar la respuesta porque solo se puede consumir una vez
          var responseClone = networkResponse.clone();
          caches.open(CACHE_NAME).then(function (cache) {
            cache.put(event.request, responseClone);
          });
          return networkResponse;
        }).catch(function (error) {
          console.error('Service Worker: Error al obtener activo estático', url.href, error);
          return new Response('Error de conexión', {
            status: 503,
            statusText: 'Service Unavailable'
          });
        });
      })
    );
    return;
  }

  // ── Estrategia Network-First con fallback a caché para el resto ──────────
  // (navegación HTML, llamadas API, SignalR, etc.)
  event.respondWith(
    fetch(event.request).then(function (networkResponse) {
      // Cachear respuestas exitosas de CDN para que funcionen offline
      if (networkResponse && networkResponse.status === 200) {
        var responseClone = networkResponse.clone();
        caches.open(CACHE_NAME).then(function (cache) {
          cache.put(event.request, responseClone);
        });
      }
      return networkResponse;
    }).catch(function (error) {
      console.warn('Service Worker: Red no disponible para', url.href, error);
      // Intentar servir desde caché como fallback
      return caches.match(event.request).then(function (cachedResponse) {
        if (cachedResponse) {
          return cachedResponse;
        }
        // Si es una navegación (página HTML), servir la página offline personalizada
        if (event.request.mode === 'navigate') {
          return caches.match('/offline.html');
        }
        return new Response('Error de conexión', {
          status: 503,
          statusText: 'Service Unavailable'
        });
      });
    })
  );
});
