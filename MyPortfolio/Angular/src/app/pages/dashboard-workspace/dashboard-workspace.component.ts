import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DashboardHeaderComponent } from '../../shared/components/dashboard-header/dashboard-header.component';
import { DashboardSidebarComponent } from '../../shared/components/dashboard-sidebar/dashboard-sidebar.component';
import { AdminApiService } from '../../core/services/admin-api.service';
import { PortfolioSectionDto } from '../../core/models/admin.models';

type RuntimeState = 'published' | 'draft' | 'saving' | 'error' | 'empty';
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
  sections: PortfolioSectionDto[] = [];
  sectionTitle = '';
  loadingSections = false;
  sectionError = '';
  sectionNotice = '';
  currentState: RuntimeState = 'published';
  viewportMode: ViewportType = 'desktop';
  toastMessage = '';
  copied = false;

  ngOnInit(): void { this.loadSections(); }

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

  setState(state: RuntimeState) {
    this.currentState = state;
    if (state === 'saving') {
      this.showToast('Saving to ASP.NET Core API...');
      setTimeout(() => {
        this.currentState = 'published';
        this.showToast('All changes persistent in ASP.NET Core cluster');
      }, 1500);
    } else if (state === 'draft') {
      this.showToast('Draft state saved locally.');
    } else if (state === 'error') {
      this.showToast('Error syncing with remote endpoint.');
    }
  }

  setViewport(mode: ViewportType) {
    this.viewportMode = mode;
  }

  deployDraft() {
    this.currentState = 'saving';
    this.api.publishSite().subscribe({
      next: response => {
        this.currentState = response.succeeded && response.data.isPublished ? 'published' : 'error';
        this.showToast(response.message || 'Publish state was not recorded.');
      },
      error: () => { this.currentState = 'error'; this.showToast('Could not record publish state.'); }
    });
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
    navigator?.clipboard?.writeText('https://alexrivera.devfolio.io');
    setTimeout(() => this.copied = false, 2500);
  }

  private showToast(msg: string) {
    this.toastMessage = msg;
    setTimeout(() => {
      this.toastMessage = '';
    }, 3000);
  }
}
