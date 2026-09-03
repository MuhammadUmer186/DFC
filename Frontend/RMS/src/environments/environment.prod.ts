export const environment = {
  production: true,
  // Relative paths: nginx proxies /api and /hubs to the backend container,
  // so this works regardless of which domain the site is served from.
  apiBaseUrl: '/api',
  apihub: '',
  // Browser-side printing via QZ Tray: logical slot -> exact Windows printer name.
  printerNames: {
    usb1: 'POS80Printer',
    usb2: 'Black Copper 80',
    ethernet: '',
    bluetooth: ''
  }
};
