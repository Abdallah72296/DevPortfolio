import { Component, ElementRef, HostListener, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { DashboardNavigationService } from '../../../core/services/dashboard-navigation.service';
import { AdminApiService } from '../../../core/services/admin-api.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-dashboard-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-header.component.html',
  styleUrl: './dashboard-header.component.css'
})
export class DashboardHeaderComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly navigation = inject(DashboardNavigationService);
  private readonly api = inject(AdminApiService);
  private elementRef = inject(ElementRef);
  userMenuOpen = false;
  copyStatus = '';
  private _avatarUrl = '';
  get avatarUrl(): string {
    return this._avatarUrl;
  }
  set avatarUrl(val: string) {
    this._avatarUrl = val || '';
    this.avatarFailed = false;
  }
  avatarFailed = false;
  userFullName = '';

  ngOnInit(): void {
    this.loadUserAvatar();
  }

  loadUserAvatar(): void {
    if (typeof localStorage !== 'undefined') {
      const cached = localStorage.getItem('myportfolio_avatar_url');
      if (cached && !cached.startsWith('blob:')) {
        this.avatarUrl = cached;
      }
      const cachedName = localStorage.getItem('myportfolio_full_name');
      if (cachedName) {
        this.userFullName = cachedName;
      }
    }

    if (this.auth.isAuthenticated()) {
      this.api.getProfile().subscribe({
        next: res => {
          if (res?.succeeded && res.data) {
            if (res.data.avatarUrl) {
              this.avatarUrl = res.data.avatarUrl;
              if (typeof localStorage !== 'undefined') {
                localStorage.setItem('myportfolio_avatar_url', res.data.avatarUrl);
              }
            }
            if (res.data.fullName) {
              this.userFullName = res.data.fullName;
              if (typeof localStorage !== 'undefined') {
                localStorage.setItem('myportfolio_full_name', res.data.fullName);
              }
            }
          }
        },
        error: () => {}
      });
    }
  }

  @HostListener('window:myportfolio_profile_updated', ['$event'])
  onProfileUpdated(event: any): void {
    const detail = event?.detail;
    if (detail?.avatarUrl !== undefined) {
      this.avatarUrl = detail.avatarUrl;
    }
    if (detail?.fullName) {
      this.userFullName = detail.fullName;
    }
  }

  resolveMediaUrl(url: string | null | undefined): string {
    if (!url) return '';
    if (url.startsWith('data:') || url.startsWith('blob:')) {
      return url;
    }
    if (url.startsWith('assets/') || url.startsWith('/assets/')) {
      return url.startsWith('/') ? url : `/${url}`;
    }
    if (url.startsWith('/uploads/') || url.startsWith('uploads/')) {
      const clean = url.startsWith('/') ? url : `/${url}`;
      return `${environment.apiUrl}${clean}`;
    }
    if (url.includes('localhost:') && url.includes('/uploads/')) {
      const path = url.substring(url.indexOf('/uploads/'));
      return `${environment.apiUrl}${path}`;
    }
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }
    return `${environment.apiUrl}/${url.replace(/^\/+/, '')}`;
  }

  getUserInitial(): string {
    if (this.userFullName && this.userFullName.trim()) {
      return this.userFullName.trim().charAt(0).toUpperCase();
    }
    const email = this.auth.getUserEmail();
    if (email && email.trim()) {
      return email.trim().charAt(0).toUpperCase();
    }
    return 'A';
  }

  async copyPublicPortfolioLink(): Promise<void> {
    const username = this.auth.getUsername();
    if (!username || typeof window === 'undefined') return;

    const publicUrl = new URL(`/portfolio/${encodeURIComponent(username)}`, window.location.origin).toString();
    try {
      await navigator.clipboard.writeText(publicUrl);
      this.copyStatus = 'Link copied';
    } catch {
      this.copyStatus = 'Copy failed';
    }
    window.setTimeout(() => this.copyStatus = '', 2500);
  }

  toggleUserMenu(event?: MouseEvent) {
    if (event) {
      event.stopPropagation();
    }
    this.userMenuOpen = !this.userMenuOpen;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.userMenuOpen = false;
    }
  }

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    this.navigation.closeMobileMenu();
    this.userMenuOpen = false;
  }

  logout(): void { this.auth.logout(); }
}
