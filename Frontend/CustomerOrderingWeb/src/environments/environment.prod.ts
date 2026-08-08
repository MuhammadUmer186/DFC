export const environment = {
  production: true,
  // Relative paths: nginx proxies /api, /hubs and /uploads to the backend
  // container, so this works regardless of which domain the site is served from.
  apiBaseUrl: '/api',
  apihub: ''
};
