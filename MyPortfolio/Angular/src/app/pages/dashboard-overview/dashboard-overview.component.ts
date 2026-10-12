import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
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
  private readonly auth = inject(AuthService);

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

  ngOnInit(): void {
    this.refreshHealth();
    this.portfolioData.getDashboardSummary().subscribe(summary => {
      this.portfolioSummary = summary;
      this.portfolioDataAvailable = summary !== null;
    });

    this.isAuthenticated = this.auth.isAuthenticated();
    if (this.isAuthenticated) {
      this.loadContactMessages();
      this.adminApi.getAnalyticsSummary().subscribe({
        next: response => this.analyticsSummary = response.succeeded ? response.data : null,
        error: () => this.analyticsSummary = null
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
}
