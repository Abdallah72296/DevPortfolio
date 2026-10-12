import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../../core/services/admin-api.service';
import { AdminUserDto } from '../../core/models/admin.models';
import { AuthService } from '../../core/services/auth.service';
import { DashboardHeaderComponent } from '../../shared/components/dashboard-header/dashboard-header.component';
import { DashboardSidebarComponent } from '../../shared/components/dashboard-sidebar/dashboard-sidebar.component';

@Component({
  selector: 'app-access-control',
  standalone: true,
  imports: [CommonModule, FormsModule, DashboardHeaderComponent, DashboardSidebarComponent],
  templateUrl: './access-control.component.html'
})
export class AccessControlComponent implements OnInit {
  private readonly api = inject(AdminApiService);
  readonly auth = inject(AuthService);
  users: AdminUserDto[] = [];
  selected: AdminUserDto | null = null;
  search = '';
  loading = false;
  saving = false;
  error = '';
  notice = '';
  createOpen = false;
  newUser = { fullName: '', email: '', password: '', role: 'User' as 'Admin' | 'User' };

  ngOnInit(): void { this.loadUsers(); }

  get filteredUsers(): AdminUserDto[] {
    const query = this.search.trim().toLowerCase();
    return this.users.filter(user => !query || [user.email, user.userName, user.fullName, user.portfolioTitle]
      .some(value => value?.toLowerCase().includes(query)));
  }

  loadUsers(): void {
    this.loading = true;
    this.error = '';
    this.api.getAdminUsers().subscribe({
      next: result => {
        this.loading = false;
        if (!result.succeeded) { this.error = result.message || 'Could not load user accounts.'; return; }
        this.users = result.data ?? [];
        this.selected = this.users.find(user => user.id === this.selected?.id) ?? this.users[0] ?? null;
      },
      error: error => { this.loading = false; this.error = error?.error?.message || 'Access control requires an administrator session.'; }
    });
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
        this.loadUsers();
      },
      error: error => { this.saving = false; this.error = error?.error?.message || error?.error?.errors?.join(' ') || 'Account could not be created.'; }
    });
  }

  setRole(user: AdminUserDto, role: 'Admin' | 'User'): void {
    if (user.roles.includes(role)) return;
    this.error = '';
    this.api.setAdminUserRole(user.id, role).subscribe({
      next: result => {
        if (!result.succeeded) { this.error = result.message || 'Role could not be changed.'; return; }
        user.roles = result.data.roles;
        this.notice = `${user.email} is now ${role}.`;
      },
      error: error => { this.error = error?.error?.message || 'Role could not be changed.'; }
    });
  }

  setLocked(user: AdminUserDto, locked: boolean): void {
    this.error = '';
    this.api.setAdminUserLocked(user.id, locked).subscribe({
      next: result => {
        if (!result.succeeded) { this.error = result.message || 'Account status could not be changed.'; return; }
        user.lockoutEnd = result.data.lockoutEnd;
        this.notice = `${user.email} ${locked ? 'locked' : 'unlocked'}.`;
      },
      error: error => { this.error = error?.error?.message || 'Account status could not be changed.'; }
    });
  }

  deleteUser(user: AdminUserDto): void {
    if (user.email.toLowerCase() === (this.auth.getUserEmail() ?? '').toLowerCase()) return;
    if (!confirm(`Delete the account for ${user.email}? This cannot be undone.`)) return;
    this.api.deleteAdminUser(user.id).subscribe({
      next: () => { this.users = this.users.filter(item => item.id !== user.id); this.selected = this.users[0] ?? null; this.notice = 'User account deleted.'; },
      error: error => { this.error = error?.error?.message || 'Account could not be deleted.'; }
    });
  }

  isCurrentUser(user: AdminUserDto): boolean {
    return user.email.toLowerCase() === (this.auth.getUserEmail() ?? '').toLowerCase();
  }

  isLocked(user: AdminUserDto): boolean {
    return !!user.lockoutEnd && new Date(user.lockoutEnd).getTime() > Date.now();
  }
}
