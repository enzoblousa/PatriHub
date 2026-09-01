// Ambiente de produção. Backend hospedado no Render (Docker), frontend no Cloudflare Pages —
// ver README.md §Deploy. `apiBaseUrl` é resolvido em build-time via file replacement
// (angular.json), não há indireção de config em runtime.
export const environment = {
  production: true,
  apiBaseUrl: 'https://patrihub-api-3lz1.onrender.com',
};
