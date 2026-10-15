import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AdminApiService } from '../../core/services/admin-api.service';
import { AdminUserDto } from '../../core/models/admin.models';
import { AuthService } from '../../core/services/auth.service';
import { DashboardHeaderComponent } from '../../shared/components/dashboard-header/dashboard-header.component';
import { DashboardSidebarComponent } from '../../shared/components/dashboard-sidebar/dashboard-sidebar.component';

@Component({
  selector: 'app-access-control',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, DashboardHeaderComponent, DashboardSidebarComponent],
  templateUrl: './access-control.component.html'
})
export class AccessControlComponent implements OnInit, OnDestroy {
  private readonly api = inject(AdminApiService);
  readonly auth = inject(AuthService);
  users: AdminUserDto[] = [];
  selected: AdminUserDto | null = null;
  search = '';
  loading = false;
  saving = false;
  busyUserId: string | null = null;
  error = '';
  notice = '';
  createOpen = false;
  roleFilter: 'All' | 'Admin' | 'User' = 'All';
  statusFilter: 'All' | 'Active' | 'Locked' = 'All';
  pageSize = 10;
  currentPage = 1;
  pageInfo: { pageNumber: number; pageSize: number; totalCount: number; totalPages: number } | null = null;
  directorySummary = { totalUsers: 0, activeUsers: 0, lockedUsers: 0, adminUsers: 0, standardUsers: 0, availableForHireUsers: 0 };
  private request?: Subscription;
  private searchTimer?: ReturnType<typeof setTimeout>;
  pendingAction: { user: AdminUserDto; action: 'lock' | 'unlock' | 'delete' } | null = null;
  newUser = { fullName: '', email: '', password: '', role: 'User' as 'Admin' | 'User' };

  ngOnInit(): void { this.loadUsers(); }

  get filteredUsers(): AdminUserDto[] { return this.users; }

  get pageUsers(): AdminUserDto[] { return this.users; }

  get totalPages(): number { return Math.max(1, this.pageInfo?.totalPages ?? 1); }
  get resultCount(): number { return this.pageInfo?.totalCount ?? 0; }
  get activeCount(): number { return this.directorySummary.activeUsers; }
  get lockedCount(): number { return this.directorySummary.lockedUsers; }
  get adminCount(): number { return this.directorySummary.adminUsers; }
  get standardUserCount(): number { return this.directorySummary.standardUsers; }
  get availableForHireCount(): number { return this.directorySummary.availableForHireUsers; }

  loadUsers(): void {
    this.loadPage(this.currentPage);
  }

  private loadPage(page: number): void {
    this.currentPage = Math.max(1, page);
    this.request?.unsubscribe();
    this.loading = true;
    this.error = '';
    this.request = this.api.getAdminUsers(this.currentPage, this.pageSize, this.search, this.roleFilter, this.statusFilter).subscribe({
      next: result => {
        this.loading = false;
        if (!result.succeeded) { this.error = result.message || 'Could not load user accounts.'; return; }
        this.users = result.data ?? [];
        this.pageInfo = result.pagination ?? null;
        this.directorySummary = result.summary ?? this.directorySummary;
        this.selected = this.users.find(user => user.id === this.selected?.id) ?? this.users[0] ?? null;
        if (this.currentPage > this.totalPages) this.loadPage(this.totalPages);
      },
      error: error => { this.loading = false; this.error = error?.error?.message || 'Access control requires an administrator session.'; }
    });
  }

  ngOnDestroy(): void {
    this.request?.unsubscribe();
    if (this.searchTimer) clearTimeout(this.searchTimer);
  }

  createUser(): void {
    if (!this.newUser.email.trim() || !this.newUser.password) return;
    this.saving = true;
    this.error = '';
    this.api.createAdminUser(this.newUser.email.trim(), this.newUser.password, this.newUser.role, this.newUser.fullName.trim()).subscribe({
      next: result => {
        this.saving = false;
        if (!result.succeeded) { this.error = result.message || 'Account could not be created.'; return; }
        this.notice = 'User account created.';
        this.createOpen = false;
        this.newUser = { fullName: '', email: '', password: '', role: 'User' };
        this.loadPage(1);
      },
      error: error => { this.saving = false; this.error = error?.error?.message || error?.error?.errors?.join(' ') || 'Account could not be created.'; }
    });
  }

  setRole(user: AdminUserDto, role: 'Admin' | 'User'): void {
    if (user.roles.includes(role)) return;
    this.error = '';
    this.busyUserId = user.id;
    this.api.setAdminUserRole(user.id, role).subscribe({
      next: result => {
        this.busyUserId = null;
        if (!result.succeeded || !result.data) { this.error = result.message || 'Role could not be changed.'; return; }
        user.roles = result.data.roles;
        if (this.selected?.id === user.id) this.selected = { ...user };
        this.notice = `${user.email} is now ${role}.`;
        this.loadPage(this.currentPage);
      },
      error: error => { this.busyUserId = null; this.error = error?.error?.message || 'Role could not be changed.'; }
    });
  }

  setLocked(user: AdminUserDto, locked: boolean): void {
    this.pendingAction = { user, action: locked ? 'lock' : 'unlock' };
  }

  requestDelete(user: AdminUserDto): void {
    if (!this.isCurrentUser(user)) this.pendingAction = { user, action: 'delete' };
  }

  confirmAction(): void {
    const pending = this.pendingAction;
    if (!pending || this.busyUserId) return;
    const { user, action } = pending;
    this.error = '';
    this.busyUserId = user.id;
    if (action === 'delete') {
      this.api.deleteAdminUser(user.id).subscribe({
        next: () => {
          this.busyUserId = null;
          this.pendingAction = null;
          this.selected = null;
          this.loadPage(this.currentPage);
          this.notice = 'User account and associated data deleted.';
        },
        error: error => { this.busyUserId = null; this.error = error?.error?.message || 'Account could not be deleted.'; }
      });
      return;
    }

    const locked = action === 'lock';
    this.api.setAdminUserLocked(user.id, locked).subscribe({
      next: result => {
        this.busyUserId = null;
        if (!result.succeeded || !result.data) { this.error = result.message || 'Account status could not be changed.'; return; }
        user.lockoutEnd = result.data.lockoutEnd;
        if (this.selected?.id === user.id) this.selected = { ...user };
        this.pendingAction = null;
        this.notice = `${user.email} ${locked ? 'locked' : 'unlocked'}.`;
        this.loadPage(this.currentPage);
      },
      error: error => { this.busyUserId = null; this.error = error?.error?.message || 'Account status could not be changed.'; }
    });
  }

  exportCsv(): void {
    const columns = ['Full name', 'Email', 'Username', 'Role', 'Account status', 'Profile title', 'Location', 'Phone', 'Available for hire'];
    const escape = (value: unknown) => `"${String(value ?? '').replace(/"/g, '""')}"`;
    const rows = this.users.map(user => [
      user.fullName, user.email, user.userName, user.roles.join('|'), this.isLocked(user) ? 'Locked' : 'Active',
      user.portfolioTitle, user.location, user.phone, user.availableForHire ? 'Yes' : 'No'
    ].map(escape).join(','));
    const blob = new Blob(['\uFEFF', columns.map(escape).join(','), '\r\n', rows.join('\r\n')], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'user-access-control.csv';
    link.click();
    URL.revokeObjectURL(url);
  }

  setPage(page: number): void { this.loadPage(Math.min(Math.max(1, page), this.totalPages)); }
  resetPage(): void { this.loadPage(1); }
  searchChanged(): void {
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => this.resetPage(), 300);
  }
  clearSearch(): void { this.search = ''; this.resetPage(); }
  portfolioUrl(user: AdminUserDto): string[] { return ['/portfolio', user.userName]; }
  closeActionDialog(): void {
    if (!this.pendingAction || this.busyUserId !== this.pendingAction.user.id) this.pendingAction = null;
  }

  isCurrentUser(user: AdminUserDto): boolean {
    return user.email.toLowerCase() === (this.auth.getUserEmail() ?? '').toLowerCase();
  }

  isLocked(user: AdminUserDto): boolean {
    return !!user.lockoutEnd && new Date(user.lockoutEnd).getTime() > Date.now();
  }
}
