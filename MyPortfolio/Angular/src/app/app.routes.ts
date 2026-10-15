import { Routes } from '@angular/router';
import { adminAuthGuard, adminOnlyGuard } from './core/guards/admin-auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    canActivate: [adminAuthGuard],
    loadComponent: () => import('./pages/dashboard-overview/dashboard-overview.component').then(m => m.DashboardOverviewComponent),
    title: 'DevFolio - Dashboard Overview'
  },
  {
    path: 'workspace',
    canActivate: [adminAuthGuard],
    loadComponent: () => import('./pages/dashboard-workspace/dashboard-workspace.component').then(m => m.DashboardWorkspaceComponent),
    title: 'DevFolio - Workspace & Simulator'
  },
  {
    path: 'mobile-workspace',
    canActivate: [adminAuthGuard],
    loadComponent: () => import('./pages/dashboard-mobile-workspace/dashboard-mobile-workspace.component').then(m => m.DashboardMobileWorkspaceComponent),
    title: 'DevFolio - Mobile Workspace'
  },
  {
    path: 'integrations',
    canActivate: [adminAuthGuard],
    loadComponent: () => import('./pages/site-integrations-settings/site-integrations-settings.component').then(m => m.SiteIntegrationsSettingsComponent),
    title: 'DevFolio - Portfolio Themes'
  },
  {
    path: 'profile-editor',
    canActivate: [adminAuthGuard],
    loadComponent: () => import('./pages/cv-import-profile-editor/cv-import-profile-editor.component').then(m => m.CvImportProfileEditorComponent),
    title: 'DevFolio - CV Import & Profile Editor'
  },
  {
    path: 'projects',
    canActivate: [adminAuthGuard],
    loadComponent: () => import('./pages/projects-repositories/projects-repositories.component').then(m => m.ProjectsRepositoriesComponent),
    title: 'DevFolio - Projects & Repositories'
  },
  {
    path: 'skills-experience',
    canActivate: [adminAuthGuard],
    loadComponent: () => import('./pages/skills-experience-manager/skills-experience-manager.component').then(m => m.SkillsExperienceManagerComponent),
    title: 'DevFolio - Skills & Experience Manager'
  },
  {
    path: 'portfolio',
    loadComponent: () => import('./pages/public-portfolio-desktop/public-portfolio-desktop.component').then(m => m.PublicPortfolioDesktopComponent),
    title: 'Alex Rivera - Staff Distributed Systems Engineer'
  },
  {
    path: 'portfolio/:username',
    loadComponent: () => import('./pages/public-portfolio-desktop/public-portfolio-desktop.component').then(m => m.PublicPortfolioDesktopComponent),
    title: 'DevFolio - Public Portfolio'
  },
  {
    path: 'u/:username',
    loadComponent: () => import('./pages/public-portfolio-desktop/public-portfolio-desktop.component').then(m => m.PublicPortfolioDesktopComponent),
    title: 'DevFolio - Public Portfolio'
  },
  {
    path: 'portfolio-mobile/:username',
    loadComponent: () => import('./pages/public-portfolio-mobile/public-portfolio-mobile.component').then(m => m.PublicPortfolioMobileComponent),
    title: 'DevFolio - Mobile Portfolio Preview'
  },
  {
    path: 'auth',
    loadComponent: () => import('./pages/authentication/authentication.component').then(m => m.AuthenticationComponent),
    title: 'DevFolio - Sign In & Registration'
  },
  {
    path: 'admin',
    redirectTo: 'access-control',
    pathMatch: 'full'
  },
  {
    path: 'access-control',
    canActivate: [adminOnlyGuard],
    loadComponent: () => import('./pages/access-control/access-control.component').then(m => m.AccessControlComponent),
    title: 'DevFolio - Access Control'
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
