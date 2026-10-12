import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AdminApiService } from '../../core/services/admin-api.service';
import { PortfolioSiteSettingsDto } from '../../core/models/admin.models';
import { PortfolioPresentationService } from '../../core/services/portfolio-presentation.service';

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
  private readonly presentation = inject(PortfolioPresentationService);
  settings: Omit<PortfolioSiteSettingsDto, 'isPublished' | 'publishedAt'> = {
    themeKey: 'obsidian-dark', customDomain: '', seoTitle: '', seoDescription: '', seoKeywords: ''
  };
  savingSettings = false;
  readonly themes = [
    { key: 'obsidian-dark', name: 'Obsidian', description: 'Charcoal with violet accents', swatches: ['#0f131c', '#262a33', '#d0bcff'] },
    { key: 'light', name: 'Porcelain', description: 'Bright, clean and minimal', swatches: ['#f7f8fc', '#e5e8f0', '#7048d8'] },
    { key: 'midnight', name: 'Midnight', description: 'Deep navy with cool blue tones', swatches: ['#080d19', '#121d31', '#91aaff'] },
    { key: 'emerald', name: 'Emerald', description: 'Dark forest with vivid green accents', swatches: ['#0b1512', '#17251f', '#65e6ad'] },
    { key: 'rose', name: 'Rose Quartz', description: 'Deep plum with soft rose accents', swatches: ['#191117', '#2b1b26', '#ff9fc8'] },
    { key: 'ocean', name: 'Ocean', description: 'Blue slate with bright aqua accents', swatches: ['#0b1419', '#14262d', '#5ee6e0'] },
    { key: 'graphite', name: 'Graphite', description: 'Monochrome, crisp and understated', swatches: ['#101112', '#252729', '#f0f0f0'] },
    { key: 'sunset', name: 'Sunset', description: 'Warm charcoal with coral and amber accents', swatches: ['#1b1212', '#30201d', '#ff9b75'] },
    { key: 'lavender', name: 'Lavender', description: 'Soft violet palette for a creative portfolio', swatches: ['#15121f', '#282238', '#c5a7ff'] },
    { key: 'paper', name: 'Paper', description: 'Warm editorial white with ink accents', swatches: ['#f7f3eb', '#e9e2d6', '#a14d2b'] },
    { key: 'ecosync', name: 'EcoSync', description: 'Clean canvas with deep forest green and electric water blue', swatches: ['#f8fafc', '#005a36', '#00df81'] },
    { key: 'ecosync-dark', name: 'EcoSync Dark', description: 'Night navy with neon energy green & water blue', swatches: ['#0b111e', '#111c2d', '#00df81'] }
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
        this.presentation.saveTheme(this.settings.themeKey);
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
