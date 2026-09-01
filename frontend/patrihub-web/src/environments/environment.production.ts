// Ambiente de produção. Backend hospedado no Render (Docker), frontend no Cloudflare Pages —
// ver README.md §Deploy. `apiBaseUrl` é resolvido em build-time via file replacement
// (angular.json), não há indireção de config em runtime.
//
// URL abaixo assume que o serviço no Render foi criado com o nome "patrihub-api" (mesmo nome
// usado em render.yaml) — se o nome real escolhido no dashboard do Render for diferente,
// atualize este valor antes do primeiro deploy do frontend.
export const environment = {
  production: true,
  apiBaseUrl: 'https://patrihub-api.onrender.com',
};
