import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { DashboardHeaderComponent } from '../../shared/components/dashboard-header/dashboard-header.component';
import { DashboardSidebarComponent } from '../../shared/components/dashboard-sidebar/dashboard-sidebar.component';
import { AdminApiService } from '../../core/services/admin-api.service';
import { PortfolioAnalyticsSummary, PortfolioSectionDto } from '../../core/models/admin.models';
import { AuthService } from '../../core/services/auth.service';
import { PortfolioDataService } from '../../core/services/portfolio-data.service';

type ViewportType = 'desktop' | 'tablet' | 'mobile';

@Component({
  selector: 'app-dashboard-workspace',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DashboardHeaderComponent, DashboardSidebarComponent],
  templateUrl: './dashboard-workspace.component.html',
  styleUrl: './dashboard-workspace.component.css'
})
export class DashboardWorkspaceComponent implements OnInit {
  private readonly api = inject(AdminApiService);
  private readonly router = inject(Router);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly portfolioData = inject(PortfolioDataService);
  private readonly adminApi = inject(AdminApiService);
  readonly auth = inject(AuthService);
  sections: PortfolioSectionDto[] = [];
  sectionTitle = '';
  loadingSections = false;
  sectionError = '';
  sectionNotice = '';
  viewportMode: ViewportType = 'desktop';
  toastMessage = '';
  copied = false;
  portfolio: any = null;
  portfolioPreviewUrl: SafeResourceUrl | null = null;
  previewLoading = true;
  previewError = '';
  analyticsSummary: PortfolioAnalyticsSummary | null = null;
  analyticsLoading = true;
  apiState: 'checking' | 'online' | 'offline' = 'checking';
  databaseState: 'checking' | 'connected' | 'offline' = 'checking';
  apiFramework = 'API';

  ngOnInit(): void {
    this.loadSections();
    this.loadLiveMetrics();
    const username = this.auth.getUsername();
    if (this.auth.isAuthenticated() && username) {
      this.portfolioPreviewUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
        `/portfolio/${encodeURIComponent(username)}?preview=dashboard`
      );
      this.portfolioData.getMobilePortfolioPreview().subscribe({
        next: response => {
          this.previewLoading = false;
          this.portfolio = response?.succeeded ? response.data ?? null : null;
          if (!this.portfolio) this.previewError = response?.message || 'Portfolio data could not be loaded.';
        },
        error: () => {
          this.previewLoading = false;
          this.previewError = 'Portfolio data could not be loaded. Check the API connection.';
        }
      });
    } else {
      this.previewLoading = false;
      this.previewError = 'Sign in to preview your portfolio.';
    }
  }

  get completenessScore(): number {
    const profile = this.portfolio?.profile;
    if (!profile) return 0;
    const checks = [
      !!profile.fullName?.trim(),
      !!(profile.headlineRole || profile.title)?.trim(),
      !!(profile.summary || profile.longBio)?.trim(),
      !!profile.avatarUrl?.trim(),
      !!profile.email?.trim(),
      !!profile.phone?.trim(),
      !!profile.location?.trim(),
      (this.portfolio?.experiences?.length ?? 0) > 0,
      (this.portfolio?.skills?.length ?? 0) > 0,
      (this.portfolio?.projects?.length ?? 0) > 0
    ];
    return Math.round(checks.filter(Boolean).length / checks.length * 100);
  }

  private loadLiveMetrics(): void {
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
      next: response => this.databaseState = response.succeeded && response.data?.connected ? 'connected' : 'offline',
      error: () => this.databaseState = 'offline'
    });
    this.adminApi.getAnalyticsSummary().subscribe({
      next: response => {
        this.analyticsSummary = response.succeeded ? response.data ?? null : null;
        this.analyticsLoading = false;
      },
      error: () => {
        this.analyticsSummary = null;
        this.analyticsLoading = false;
      }
    });
  }

  loadSections(): void {
    this.loadingSections = true;
    this.api.getSections().subscribe({
      next: response => {
        this.loadingSections = false;
        if (response.succeeded) this.sections = response.data ?? [];
        else this.sectionError = response.message || 'Sections could not be loaded.';
      },
      error: () => { this.loadingSections = false; this.sectionError = 'Sections could not be loaded. Sign in and check the API.'; }
    });
  }

  createSection(): void {
    const title = this.sectionTitle.trim();
    if (!title) return;
    const key = title.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '') || `section-${Date.now()}`;
    this.api.createSection({ title, key, displayOrder: this.sections.length }).subscribe({
      next: response => {
        if (!response.succeeded) { this.sectionError = response.message; return; }
        this.sectionTitle = '';
        this.sectionNotice = 'Section created.';
        this.loadSections();
      }, error: () => this.sectionError = 'Section could not be created.'
    });
  }

  toggleSection(section: PortfolioSectionDto): void {
    this.api.setSectionVisibility(section.id, !section.isVisible).subscribe({
      next: response => response.succeeded ? section.isVisible = response.data.isVisible : this.sectionError = response.message,
      error: () => this.sectionError = 'Section visibility could not be changed.'
    });
  }

  renameSection(section: PortfolioSectionDto): void {
    const title = prompt('Section title', section.title)?.trim();
    if (!title || title === section.title) return;
    this.api.updateSection(section.id, { title, displayOrder: section.displayOrder }).subscribe({
      next: response => response.succeeded ? section.title = response.data.title : this.sectionError = response.message,
      error: () => this.sectionError = 'Section could not be updated.'
    });
  }

  deleteSection(section: PortfolioSectionDto): void {
    if (section.isSystemSection || !confirm(`Delete section "${section.title}"?`)) return;
    this.api.deleteSection(section.id).subscribe({
      next: () => this.sections = this.sections.filter(value => value.id !== section.id),
      error: () => this.sectionError = 'Section could not be deleted.'
    });
  }

  moveSection(section: PortfolioSectionDto, offset: number): void {
    const index = this.sections.findIndex(value => value.id === section.id);
    const target = index + offset;
    if (index < 0 || target < 0 || target >= this.sections.length) return;
    const reordered = [...this.sections];
    [reordered[index], reordered[target]] = [reordered[target], reordered[index]];
    this.api.reorderSections(reordered.map((value, displayOrder) => ({ id: value.id, displayOrder }))).subscribe({
      next: response => response.succeeded
        ? this.sections = reordered.map((value, displayOrder) => ({ ...value, displayOrder }))
        : this.sectionError = response.message,
      error: () => this.sectionError = 'Section order could not be saved.'
    });
  }

  setViewport(mode: ViewportType) {
    this.viewportMode = mode;
  }

  modifyTheme() {
    this.router.navigate(['/integrations']);
  }

  flushCache() {
    this.showToast('CDN cache purge is not available in the configured backend.');
  }

  openProjects(): void { this.router.navigate(['/projects']); }

  copyLink() {
    this.copied = true;
    const username = this.auth.getUsername();
    if (username) navigator?.clipboard?.writeText(`${window.location.origin}/portfolio/${encodeURIComponent(username)}`);
    setTimeout(() => this.copied = false, 2500);
  }

  private showToast(msg: string) {
    this.toastMessage = msg;
    setTimeout(() => {
      this.toastMessage = '';
    }, 3000);
  }
}
