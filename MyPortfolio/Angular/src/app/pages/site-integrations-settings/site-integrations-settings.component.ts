import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AdminApiService } from '../../core/services/admin-api.service';
import { PortfolioSiteSettingsDto } from '../../core/models/admin.models';
import { PortfolioPresentationService } from '../../core/services/portfolio-presentation.service';
import { AuthService } from '../../core/services/auth.service';

import { DashboardHeaderComponent } from '../../shared/components/dashboard-header/dashboard-header.component';
import { DashboardSidebarComponent } from '../../shared/components/dashboard-sidebar/dashboard-sidebar.component';

@Component({
  selector: 'app-site-integrations-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, DashboardHeaderComponent, DashboardSidebarComponent],
  templateUrl: './site-integrations-settings.component.html'
})
export class SiteIntegrationsSettingsComponent implements OnInit {
  private readonly api = inject(AdminApiService);
  readonly auth = inject(AuthService);
  private readonly presentation = inject(PortfolioPresentationService);
  settings: Omit<PortfolioSiteSettingsDto, 'isPublished' | 'publishedAt'> = {
    themeKey: 'obsidian-dark', customDomain: '', seoTitle: '', seoDescription: '', seoKeywords: ''
  };
  savingSettings = false;
  readonly themes = [
    { key: 'obsidian-dark', name: 'Obsidian', description: 'Charcoal with violet accents', swatches: ['#0f131c', '#1c2028', '#d0bcff'] },
    { key: 'light', name: 'Porcelain', description: 'Bright, clean and minimal', swatches: ['#f8fafc', '#ffffff', '#6366f1'] },
    { key: 'midnight', name: 'Midnight', description: 'Deep navy with cool blue tones', swatches: ['#080d19', '#10192b', '#8da4ff'] },
    { key: 'emerald', name: 'Emerald', description: 'Dark forest with vivid green accents', swatches: ['#081410', '#12231d', '#4ee39d'] },
    { key: 'rose', name: 'Rose Quartz', description: 'Deep plum with soft rose accents', swatches: ['#170f15', '#281a25', '#ff94bf'] },
    { key: 'ocean', name: 'Ocean', description: 'Blue slate with bright aqua accents', swatches: ['#08131a', '#11222d', '#38bdf8'] },
    { key: 'graphite', name: 'Graphite', description: 'Monochrome, crisp and understated', swatches: ['#0f1011', '#1e2022', '#e4e4e7'] },
    { key: 'sunset', name: 'Sunset', description: 'Warm charcoal with coral and amber accents', swatches: ['#170f0e', '#291a18', '#ff8c69'] },
    { key: 'lavender', name: 'Lavender', description: 'Soft violet palette for a creative portfolio', swatches: ['#120e1a', '#231b33', '#c084fc'] },
    { key: 'paper', name: 'Paper', description: 'Warm editorial white with ink accents', swatches: ['#fbf8f2', '#ffffff', '#a84b29'] },
    { key: 'ecosync', name: 'EcoSync', description: 'Clean canvas with deep forest green and electric water blue', swatches: ['#f8fafc', '#ffffff', '#005a36'] },
    { key: 'ecosync-dark', name: 'EcoSync Dark', description: 'Night navy with neon energy green & water blue', swatches: ['#0b111e', '#121c2d', '#00df81'] }
  ];
  loading = true;
  error = '';
  notice = '';

  ngOnInit(): void {
    this.api.getSiteSettings().subscribe({
      next: response => {
        if (response.succeeded && response.data) {
          const { themeKey, customDomain, seoTitle, seoDescription, seoKeywords } = response.data;
          this.settings = { themeKey, customDomain, seoTitle, seoDescription, seoKeywords };
          this.presentation.applyTheme(themeKey);
        } else {
          this.settings.themeKey = this.presentation.selectedTheme;
          this.error = response.message || 'Theme settings could not be loaded.';
        }
        this.loading = false;
      }, error: error => {
        this.settings.themeKey = this.presentation.selectedTheme;
        this.error = this.getError(error, 'Theme settings could not be loaded.');
        this.loading = false;
      }
    });
  }

  saveSettings(): void {
    this.savingSettings = true;
    this.error = '';
    this.api.updateSiteSettings(this.settings).subscribe({
      next: response => {
        this.savingSettings = false;
        if (!response.succeeded) { this.error = response.message || 'Settings could not be saved.'; return; }
        this.presentation.saveTheme(this.settings.themeKey, this.auth.getUsername() || '');
        this.notice = 'Settings saved.';
      }, error: error => { this.savingSettings = false; this.error = this.getError(error, 'Settings could not be saved.'); }
    });
  }

  selectTheme(themeKey: string): void {
    this.settings.themeKey = themeKey;
    this.presentation.preview(themeKey);
  }

  private getError(error: any, fallback: string): string {
    return error?.error?.message || error?.error?.title || fallback;
  }
}
