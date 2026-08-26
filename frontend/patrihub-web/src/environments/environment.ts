// Ambiente de desenvolvimento (usado por `ng serve` e pelo serviço `frontend` do
// docker-compose). A API roda em outra origin (porta 8080) — por isso o backend precisa de
// uma policy de CORS liberando a origin deste dev server (ver Program.cs).
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:8080',
};
