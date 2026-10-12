import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { filter } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-page-switcher',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './page-switcher.component.html',
  styleUrl: './page-switcher.component.css'
})
export class PageSwitcherComponent {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  isOpen = false;
  shouldShow = this.canShowOnCurrentPage();

  constructor() {
    this.router.events.pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(() => this.shouldShow = this.canShowOnCurrentPage());
  }

  private canShowOnCurrentPage(): boolean {
    if (environment.production || !this.auth.isAuthenticated()) return false;
    const path = this.router.url.split('?')[0];
    return path !== '/auth' && path !== '/portfolio' && path !== '/portfolio-mobile' && !path.startsWith('/u/');
  }

  toggle() {
    this.isOpen = !this.isOpen;
  }
}
