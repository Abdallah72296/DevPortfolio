import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminHeaderComponent } from '../../shared/components/admin-header/admin-header.component';
import { AdminSidebarComponent } from '../../shared/components/admin-sidebar/admin-sidebar.component';
import { MockDataService, UserAccount } from '../../core/services/mock-data.service';

@Component({
  selector: 'app-admin-overview',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    FormsModule,
    AdminHeaderComponent, 
    AdminSidebarComponent
  ],
  templateUrl: './admin-overview.component.html',
  styleUrl: './admin-overview.component.css'
})
export class AdminOverviewComponent implements OnInit {
  activeModule = 'users';
  searchQuery = 'alex';
  statusFilter = 'active';
  portfolioFilter = 'published';

  users: UserAccount[] = [];
  selectedUser: UserAccount | null = null;
  showDeactivateModal = true;

  constructor(private mockDataService: MockDataService) {}

  ngOnInit() {
    this.mockDataService.getUsers().subscribe(users => {
      this.users = users;
      if (users.length > 0) {
        this.selectedUser = users[0];
      }
    });
  }

  get filteredUsers(): UserAccount[] {
    return this.users.filter(u => {
      const matchesSearch = !this.searchQuery || 
        u.name.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        u.email.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        u.subdomain.toLowerCase().includes(this.searchQuery.toLowerCase());
      
      const matchesStatus = this.statusFilter === 'all' || 
        (this.statusFilter === 'active' && u.status === 'ACTIVE') ||
        (this.statusFilter === 'inactive' && u.status === 'INACTIVE');

      const matchesPortfolio = this.portfolioFilter === 'all' ||
        (this.portfolioFilter === 'published' && u.portfolioStatus === 'PUBLISHED') ||
        (this.portfolioFilter === 'draft' && u.portfolioStatus === 'DRAFT_SAVED') ||
        (this.portfolioFilter === 'empty' && u.portfolioStatus === 'EMPTY');

      return matchesSearch && matchesStatus && matchesPortfolio;
    });
  }

  selectUser(user: UserAccount) {
    this.selectedUser = user;
    this.showDeactivateModal = false;
  }

  confirmDeactivation() {
    if (this.selectedUser) {
      this.selectedUser.status = 'INACTIVE';
      this.showDeactivateModal = false;
    }
  }
}
