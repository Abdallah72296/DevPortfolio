import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { AdminApiService } from './admin-api.service';

@Injectable({ providedIn: 'root' })
export class PortfolioPresentationService {
  private readonly api = inject(AdminApiService);
  private readonly document = inject(DOCUMENT);
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);
  private previewTheme: string | null = null;
  private readonly localThemeKey = 'portfolio.theme.preview';

  get selectedTheme(): string {
    return this.document.body.dataset['portfolioTheme'] || this.readLocalTheme() || 'obsidian-dark';
  }

  loadPublicSettings(): void {
    this.applyTheme(this.previewTheme || this.readLocalTheme() || 'obsidian-dark');
    this.api.getPublicSiteSettings().subscribe({
      next: response => {
        const settings = response.succeeded ? response.data : null;
        if (!settings) return;
        this.applyTheme(this.previewTheme || settings.themeKey || 'obsidian-dark');
        if (settings.seoTitle) this.title.setTitle(settings.seoTitle);
        if (settings.seoDescription) this.meta.updateTag({ name: 'description', content: settings.seoDescription });
        if (settings.seoKeywords) this.meta.updateTag({ name: 'keywords', content: settings.seoKeywords });
      }, error: () => undefined
    });
  }

  applyTheme(themeKey: string): void {
    this.document.body.dataset['portfolioTheme'] = themeKey || 'obsidian-dark';
  }

  preview(themeKey: string): void {
    this.previewTheme = themeKey || 'obsidian-dark';
    this.writeLocalTheme(this.previewTheme);
    this.applyTheme(this.previewTheme);
  }

  saveTheme(themeKey: string): void {
    this.previewTheme = null;
    const savedTheme = themeKey || 'obsidian-dark';
    this.writeLocalTheme(savedTheme);
    this.applyTheme(savedTheme);
  }

  private readLocalTheme(): string | null {
    try {
      return this.document.defaultView?.localStorage.getItem(this.localThemeKey) || null;
    } catch {
      return null;
    }
  }

  private writeLocalTheme(themeKey: string): void {
    try {
      this.document.defaultView?.localStorage.setItem(this.localThemeKey, themeKey);
    } catch {
      // Preview remains available for the current session when browser storage is disabled.
    }
  }
}
