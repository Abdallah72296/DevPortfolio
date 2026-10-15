import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class DashboardNavigationService {
  isMobileMenuOpen = false;

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }
}
