export const environment = {
  production: true,
  // Relative paths: nginx proxies /api and /hubs to the backend container,
  // so this works regardless of which domain the site is served from.
  apiBaseUrl: '/api',
  apihub: ''
};