import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { DashboardHeaderComponent } from '../../shared/components/dashboard-header/dashboard-header.component';
import { DashboardSidebarComponent } from '../../shared/components/dashboard-sidebar/dashboard-sidebar.component';
import { AdminApiService } from '../../core/services/admin-api.service';
import { PortfolioDataService, PortfolioDashboardSummary } from '../../core/services/portfolio-data.service';
import { AuthService } from '../../core/services/auth.service';
import { ContactMessageDto, PortfolioAnalyticsSummary } from '../../core/models/admin.models';

@Component({
  selector: 'app-dashboard-overview',
  standalone: true,
  imports: [CommonModule, RouterModule, DashboardHeaderComponent, DashboardSidebarComponent],
  templateUrl: './dashboard-overview.component.html',
  styleUrl: './dashboard-overview.component.css'
})
export class DashboardOverviewComponent implements OnInit {
  private readonly adminApi = inject(AdminApiService);
  private readonly portfolioData = inject(PortfolioDataService);
  private readonly sanitizer = inject(DomSanitizer);
  readonly auth = inject(AuthService);

  viewportMode: 'desktop' | 'tablet' | 'mobile' = 'desktop';
  checkingHealth = false;
  apiState: 'checking' | 'online' | 'offline' = 'checking';
  databaseState: 'checking' | 'connected' | 'offline' = 'checking';
  apiFramework = 'API';
  portfolioSummary: PortfolioDashboardSummary | null = null;
  portfolioDataAvailable = false;
  contactMessages: ContactMessageDto[] = [];
  contactMessagesAvailable = false;
  contactMessagesLoading = false;
  contactMessageError = '';
  unreadContactMessageCount = 0;
  analyticsSummary: PortfolioAnalyticsSummary | null = null;
  isAuthenticated = false;
  portfolioPreviewUrl: SafeResourceUrl | null = null;
  currentThemeKey = 'obsidian-dark';
  currentThemeName = 'Obsidian';

  ngOnInit(): void {
    this.refreshHealth();
    this.portfolioData.getDashboardSummary().subscribe(summary => {
      this.portfolioSummary = summary;
      this.portfolioDataAvailable = summary !== null;
    });

    this.isAuthenticated = this.auth.isAuthenticated();
    const username = this.auth.getUsername();
    if (this.isAuthenticated && username) {
      this.portfolioPreviewUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
        `/portfolio/${encodeURIComponent(username)}?preview=dashboard`
      );
    }
    if (this.isAuthenticated) {
      this.loadContactMessages();
      this.adminApi.getAnalyticsSummary().subscribe({
        next: response => this.analyticsSummary = response.succeeded ? response.data : null,
        error: () => this.analyticsSummary = null
      });
      this.adminApi.getSiteSettings().subscribe({
        next: response => {
          if (response.succeeded && response.data?.themeKey) {
            this.currentThemeKey = response.data.themeKey;
            this.currentThemeName = this.getThemeDisplayName(response.data.themeKey);
          }
        }
      });
    }
  }

  setViewport(mode: 'desktop' | 'tablet' | 'mobile') {
    this.viewportMode = mode;
  }

  runHealthcheck() {
    this.checkingHealth = true;
    this.refreshHealth();
  }

  private refreshHealth(): void {
    this.apiState = 'checking';
    this.databaseState = 'checking';

    this.adminApi.getApiHealth().subscribe({
      next: response => {
        this.apiState = response.succeeded && response.data?.status?.toLowerCase() === 'healthy'
          ? 'online'
          : 'offline';
        this.apiFramework = response.data?.framework ?? 'API';
      },
      error: () => this.apiState = 'offline'
    });

    this.adminApi.getDatabaseHealth().subscribe({
      next: response => {
        this.databaseState = response.succeeded && response.data?.connected ? 'connected' : 'offline';
        this.checkingHealth = false;
      },
      error: () => {
        this.databaseState = 'offline';
        this.checkingHealth = false;
      }
    });
  }

  private loadContactMessages(): void {
    this.contactMessagesLoading = true;
    this.adminApi.getContactMessages().subscribe({
      next: response => {
        this.contactMessages = response.data?.items ?? [];
        this.unreadContactMessageCount = this.contactMessages.filter(message => !message.isRead).length;
        this.contactMessagesAvailable = response.succeeded;
        this.contactMessagesLoading = false;
      },
      error: () => {
        this.contactMessagesAvailable = false;
        this.contactMessagesLoading = false;
        this.contactMessageError = 'Could not load messages. Check your admin access and API connection.';
      }
    });
  }

  markContactMessageRead(message: ContactMessageDto): void {
    if (message.isRead) return;

    this.adminApi.markMessageAsRead(message.id).subscribe({
      next: () => {
        message.isRead = true;
        this.unreadContactMessageCount = Math.max(0, this.unreadContactMessageCount - 1);
      }
    });
  }

  toggleContactMessageRead(message: ContactMessageDto): void {
    if (message.isRead) {
      this.adminApi.markMessageAsUnread(message.id).subscribe({
        next: () => { message.isRead = false; this.unreadContactMessageCount++; }
      });
    } else this.markContactMessageRead(message);
  }

  deleteContactMessage(message: ContactMessageDto): void {
    if (!confirm(`Delete message from ${message.name}?`)) return;
    this.adminApi.deleteContactMessage(message.id).subscribe({
      next: () => {
        this.contactMessages = this.contactMessages.filter(item => item.id !== message.id);
        if (!message.isRead) this.unreadContactMessageCount = Math.max(0, this.unreadContactMessageCount - 1);
      },
      error: () => this.contactMessageError = 'Message could not be deleted.'
    });
  }

  getThemeDisplayName(key: string): string {
    const names: Record<string, string> = {
      'obsidian-dark': 'Obsidian',
      'light': 'Porcelain',
      'midnight': 'Midnight',
      'emerald': 'Emerald',
      'rose': 'Rose Quartz',
      'ocean': 'Ocean',
      'graphite': 'Graphite',
      'sunset': 'Sunset',
      'lavender': 'Lavender',
      'paper': 'Paper',
      'ecosync': 'EcoSync',
      'ecosync-dark': 'EcoSync Dark'
    };
    return names[key] || key || 'Obsidian';
  }

  getThemeDescription(key: string): string {
    const desc: Record<string, string> = {
      'obsidian-dark': 'Charcoal with violet accents & JetBrains Mono telemetry',
      'light': 'Bright, clean and minimal porcelain aesthetic',
      'midnight': 'Deep navy with cool blue tones & luminous accents',
      'emerald': 'Dark forest with vivid neon green telemetry',
      'rose': 'Deep plum with soft rose quartz accents',
      'ocean': 'Blue slate with bright aqua & teal accents',
      'graphite': 'Monochrome, crisp and understated engineering theme',
      'sunset': 'Warm charcoal with coral and amber accents',
      'lavender': 'Soft violet palette for a creative technical portfolio',
      'paper': 'Warm editorial white with clean typography',
      'ecosync': 'Clean canvas with deep forest green and electric water blue',
      'ecosync-dark': 'Night navy with neon energy green & water blue'
    };
    return desc[key] || 'Visual style applied to your public engineering portfolio.';
  }
}
