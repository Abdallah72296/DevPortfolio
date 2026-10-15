import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { AdminApiService } from './admin-api.service';

@Injectable({ providedIn: 'root' })
export class PortfolioPresentationService {
  private readonly document = inject(DOCUMENT);
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);
  private readonly api = inject(AdminApiService);

  get selectedTheme(): string {
    return this.document.body?.dataset['portfolioTheme'] 
      || this.document.documentElement.dataset['portfolioTheme'] 
      || 'obsidian-dark';
  }

  loadPublicSettings(): void {
    this.preparePublicTheme();
    this.api.getSiteSettings().subscribe({
      next: response => {
        const settings = response.succeeded ? response.data : null;
        if (!settings) return;
        this.applyTheme(settings.themeKey || 'obsidian-dark');
        if (settings.seoTitle) this.title.setTitle(settings.seoTitle);
        if (settings.seoDescription) this.meta.updateTag({ name: 'description', content: settings.seoDescription });
        if (settings.seoKeywords) this.meta.updateTag({ name: 'keywords', content: settings.seoKeywords });
      },
      error: () => undefined
    });
  }

  preparePublicTheme(username?: string): void {
    const cached = (username ? this.readTheme(username) : null) || this.readGlobalTheme() || 'obsidian-dark';
    this.applyTheme(cached);
  }

  applyPublicSettings(settings: {
    themeKey?: string | null;
    seoTitle?: string | null;
    seoDescription?: string | null;
    seoKeywords?: string | null;
    profile?: { fullName?: string | null; summary?: string | null; longBio?: string | null } | null;
  }, username?: string): void {
    const themeKey = settings.themeKey || 'obsidian-dark';
    this.applyTheme(themeKey);
    this.writeTheme(username, themeKey);
    this.title.setTitle(settings.seoTitle || `${settings.profile?.fullName || username || 'Engineering'} — Portfolio`);
    this.meta.updateTag({
      name: 'description',
      content: settings.seoDescription || settings.profile?.summary || settings.profile?.longBio || 'Public portfolio'
    });
    this.meta.updateTag({ name: 'keywords', content: settings.seoKeywords || '' });
  }

  applyTheme(themeKey: string): void {
    const theme = themeKey || 'obsidian-dark';
    const isLight = ['light', 'paper', 'ecosync'].includes(theme);

    this.document.documentElement.dataset['portfolioTheme'] = theme;
    if (this.document.body) {
      this.document.body.dataset['portfolioTheme'] = theme;
    }

    if (isLight) {
      this.document.documentElement.classList.remove('dark');
      this.document.documentElement.classList.add('light');
      if (this.document.body) {
        this.document.body.classList.remove('dark');
        this.document.body.classList.add('light');
      }
    } else {
      this.document.documentElement.classList.remove('light');
      this.document.documentElement.classList.add('dark');
      if (this.document.body) {
        this.document.body.classList.remove('light');
        this.document.body.classList.add('dark');
      }
    }
  }

  preview(themeKey: string): void {
    this.applyTheme(themeKey || 'obsidian-dark');
  }

  saveTheme(themeKey: string, username?: string): void {
    const savedTheme = themeKey || 'obsidian-dark';
    this.applyTheme(savedTheme);
    this.writeTheme(username, savedTheme);
  }

  private themeStorageKey(username: string): string {
    return `portfolio.theme.${username.trim().toLowerCase()}`;
  }

  private readTheme(username: string): string | null {
    try {
      return this.document.defaultView?.localStorage.getItem(this.themeStorageKey(username)) || null;
    } catch {
      return null;
    }
  }

  private readGlobalTheme(): string | null {
    try {
      return this.document.defaultView?.localStorage.getItem('portfolio.theme.global') || null;
    } catch {
      return null;
    }
  }

  private writeTheme(username: string | undefined, themeKey: string): void {
    try {
      if (username) {
        this.document.defaultView?.localStorage.setItem(this.themeStorageKey(username), themeKey);
      }
      this.document.defaultView?.localStorage.setItem('portfolio.theme.global', themeKey);
    } catch {
      // The server theme remains authoritative when browser storage is disabled.
    }
  }
}
