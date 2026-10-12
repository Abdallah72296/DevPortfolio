export const environment = {
  production: true,
  // Supports runtime override via window.__env if supplied, else uses the production API URL
  apiUrl: (typeof window !== 'undefined' && (window as any).__env?.apiUrl)
    ? (window as any).__env.apiUrl
    : 'http://portofilo.runasp.net'
};
