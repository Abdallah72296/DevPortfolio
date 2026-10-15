# PortfolioApp - Angular 17+ Enterprise SPA

Modern developer portfolio frontend and administrative studio built with Angular 17.3, TypeScript, and a modern CSS tokens design system.

## Key Modules & Routes

- **Public Portfolio**: Dedicated multi-tenant views available via `/portfolio/{username}` or `/u/{username}` (desktop and mobile optimized).
- **Studio Dashboard**: Career management workbench available via `/dashboard`.
- **Multimodal CV Ingestion**: Autonomous resume parsing and schema staging at `/dashboard/cv-import`.
- **Access Control**: Administrative user management and directory at `/dashboard/access-control`.
- **Authentication**: JWT login, registration, and OTP email confirmation at `/auth`.

## Development Server

Run `npm start` (or `ng serve`) to launch the development server. The application will automatically reload if you change any of the source files.

## Production Build & Packaging

To generate the production bundle packaged for IIS / MonsterASP deployment:

```powershell
npm run build:zip
```

The output archive is created at `MyPortfolio/portfolio-frontend.zip`, containing the complete production build along with `web.config` configured for SPA URL rewriting and security headers.
