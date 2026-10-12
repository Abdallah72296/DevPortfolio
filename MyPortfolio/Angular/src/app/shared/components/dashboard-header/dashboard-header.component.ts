import { Component, ElementRef, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-dashboard-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-header.component.html',
  styleUrl: './dashboard-header.component.css'
})
export class DashboardHeaderComponent {
  readonly auth = inject(AuthService);
  private elementRef = inject(ElementRef);
  isPublishing = false;
  showToast = false;
  userMenuOpen = false;

  onPublish() {
    this.isPublishing = true;
    setTimeout(() => {
      this.isPublishing = false;
      this.showToast = true;
      setTimeout(() => this.showToast = false, 3500);
    }, 1200);
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

  logout(): void { this.auth.logout(); }
}
