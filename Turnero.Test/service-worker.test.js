const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const source = fs.readFileSync(
  path.join(__dirname, '..', 'src', 'Turnero.Web', 'wwwroot', 'service-worker.js'),
  'utf8'
);

function createWorker() {
  const listeners = {};
  const cachedRequests = [];
  const response = { status: 200, type: 'basic', clone() { return this; } };
  const context = {
    URL,
    console,
    self: { addEventListener(type, listener) { listeners[type] = listener; } },
    caches: {
      match: async () => undefined,
      open: async () => ({ put: async (request) => { cachedRequests.push(request.url); } })
    },
    fetch: async () => response
  };
  vm.runInNewContext(source, context);

  return {
    cachedRequests,
    dispatch(url, method = 'GET') {
      let result;
      listeners.fetch({
        request: { url, method },
        respondWith(promise) { result = promise; }
      });
      return result;
    }
  };
}

test('ignores non-HTTP requests and non-GET requests', () => {
  const worker = createWorker();
  assert.equal(worker.dispatch('chrome-extension://extension/script.js'), undefined);
  assert.equal(worker.dispatch('chrome-extension://extension/resource'), undefined);
  assert.equal(worker.dispatch('https://example.com/script.js', 'POST'), undefined);
  assert.deepEqual(worker.cachedRequests, []);
});

test('continues to handle and cache HTTP and HTTPS requests', async () => {
  const worker = createWorker();
  const staticUrl = 'https://example.com/script.js';
  const dynamicUrl = 'http://example.com/page';
  assert.equal((await worker.dispatch(staticUrl)).status, 200);
  assert.equal((await worker.dispatch(dynamicUrl)).status, 200);
  assert.deepEqual(worker.cachedRequests, [staticUrl, dynamicUrl]);
});
