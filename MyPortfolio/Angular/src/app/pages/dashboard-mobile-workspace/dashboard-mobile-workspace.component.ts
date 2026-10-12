import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-dashboard-mobile-workspace',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-mobile-workspace.component.html',
  styleUrl: './dashboard-mobile-workspace.component.css'
})
export class DashboardMobileWorkspaceComponent {
  openDrawer = false;
  activeState: 'live' | 'draft' | 'saving' | 'error' = 'live';
  isPublishing = false;
  copied = false;

  publishMobile() {
    this.isPublishing = true;
    setTimeout(() => {
      this.isPublishing = false;
      this.activeState = 'live';
    }, 1200);
  }

  copyUrl() {
    this.copied = true;
    navigator?.clipboard?.writeText('alexrivera.devfolio.io');
    setTimeout(() => this.copied = false, 2000);
  }
}
