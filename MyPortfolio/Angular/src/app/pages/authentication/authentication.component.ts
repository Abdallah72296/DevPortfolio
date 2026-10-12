import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-authentication',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './authentication.component.html',
  styleUrl: './authentication.component.css'
})
export class AuthenticationComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  activeTab: 'login' | 'register' = 'login';
  
  // Login State
  loginEmail = '';
  loginPassword = '';
  showLoginPass = false;
  rememberDevice = true;
  loginAlertVisible = false;
  isLoggingIn = false;
  loginBtnText = 'Sign In to DevFolio';

  // Register State
  regName = '';
  regEmail = '';
  regPassword = '';
  regConfirmPassword = '';
  showRegPass = false;
  agreeTerms = false;
  isRegistering = false;
  regBtnText = 'Create Account & Start Portfolio';
  authError = '';

  // Strength Evaluator State
  strengthScore = 0;
  strengthLabel = 'Entropy: Weak';
  strengthClass = 'text-error';
  strengthHelp = 'Strong: 12+ chars, uppercase, number, symbol';

  get regSubdomain(): string {
    if (!this.regName) return 'your-name';
    return this.regName.toLowerCase().trim().replace(/[^a-z0-9]/g, '-').replace(/-+/g, '-').replace(/^-|-$/g, '');
  }

  get passwordsMatch(): boolean {
    return this.regPassword.length > 0 && this.regPassword === this.regConfirmPassword;
  }

  switchTab(tab: 'login' | 'register') {
    this.activeTab = tab;
  }

  onPasswordInput(val: string) {
    let score = 0;
    if (val.length >= 8) score++;
    if (val.length >= 12) score++;
    if (/[A-Z]/.test(val) && /[0-9]/.test(val)) score++;
    if (/[^A-Za-z0-9]/.test(val)) score++;

    this.strengthScore = score;
    if (score <= 1) {
      this.strengthLabel = 'Entropy: Weak';
      this.strengthClass = 'text-error';
    } else if (score === 2) {
      this.strengthLabel = 'Entropy: Moderate';
      this.strengthClass = 'text-secondary';
    } else {
      this.strengthLabel = score === 4 ? 'Entropy: Very Strong' : 'Entropy: Strong';
      this.strengthClass = 'text-tertiary';
    }
  }

  barClass(index: number): string {
    if (this.strengthScore <= 1) {
      return index === 1 ? 'bg-error' : 'bg-surface-container-highest';
    }
    if (this.strengthScore === 2) {
      return index <= 2 ? 'bg-secondary' : 'bg-surface-container-highest';
    }
    if (this.strengthScore >= 3) {
      if (index <= this.strengthScore) {
        return 'bg-tertiary';
      }
      return 'bg-surface-container-highest';
    }
    return 'bg-surface-container-highest';
  }

  handleLoginSubmit() {
    if (this.isLoggingIn) return;
    this.isLoggingIn = true;
    this.loginBtnText = 'Authenticating Cluster...';
    this.authError = '';
    this.auth.login({ email: this.loginEmail.trim(), password: this.loginPassword }).subscribe({
      next: response => {
        this.isLoggingIn = false;
        this.loginBtnText = 'Sign In to DevFolio';
        if (!response.succeeded) { this.authError = response.message || 'Sign in failed.'; return; }
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        void this.router.navigateByUrl(returnUrl && returnUrl.startsWith('/') ? returnUrl : '/dashboard');
      },
      error: error => {
        this.isLoggingIn = false;
        this.loginBtnText = 'Sign In to DevFolio';
        this.authError = error?.error?.message || 'Email or password is incorrect.';
      }
    });
  }

  handleRegisterSubmit() {
    if (this.isRegistering || !this.passwordsMatch || !this.agreeTerms) return;
    this.isRegistering = true;
    this.regBtnText = 'Provisioning Edge Node...';
    this.authError = '';
    const username = this.regEmail.trim().split('@')[0];
    this.auth.register({ fullName: this.regName.trim(), username, email: this.regEmail.trim(), password: this.regPassword }).subscribe({
      next: response => {
        this.isRegistering = false;
        this.regBtnText = 'Create Account & Start Portfolio';
        if (!response.succeeded) { this.authError = response.message || 'Account could not be created.'; return; }
        void this.router.navigate(['/dashboard']);
      },
      error: error => {
        this.isRegistering = false;
        this.regBtnText = 'Create Account & Start Portfolio';
        this.authError = error?.error?.message || error?.error?.errors?.join(' ') || 'Account could not be created.';
      }
    });
  }
}
