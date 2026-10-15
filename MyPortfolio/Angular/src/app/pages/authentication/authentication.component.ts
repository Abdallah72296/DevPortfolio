import { Component, OnDestroy, OnInit, inject } from '@angular/core';
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
export class AuthenticationComponent implements OnInit, OnDestroy {
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
  confirmationMessage = '';
  confirmationEmailSent = true;
  isConfirmingEmail = false;
  showOtpVerification = false;
  otpCode = '';
  isConfirmingOtp = false;
  resendCooldownSeconds = 0;
  private resendCooldownTimer: ReturnType<typeof setInterval> | null = null;
  isResendingConfirmation = false;

  ngOnInit(): void {
    const params = this.route.snapshot.queryParamMap;
    const email = params.get('email');
    const token = params.get('token');
    if (params.get('confirmEmail') !== 'true' || !email || !token) return;

    this.activeTab = 'login';
    this.isConfirmingEmail = true;
    this.auth.confirmEmail(email, token).subscribe({
      next: response => {
        this.isConfirmingEmail = false;
        this.confirmationEmailSent = true;
        this.confirmationMessage = response.message || 'Email confirmed. You can now sign in.';
        this.loginEmail = email;
        void this.router.navigate([], {
          relativeTo: this.route,
          queryParams: { confirmEmail: null, email: null, token: null },
          queryParamsHandling: 'merge',
          replaceUrl: true
        });
      },
      error: error => {
        this.isConfirmingEmail = false;
        this.confirmationEmailSent = false;
        this.authError = this.getRequestErrorMessage(error, 'This confirmation link is invalid or expired.');
        void this.router.navigate([], {
          relativeTo: this.route,
          queryParams: { confirmEmail: null, email: null, token: null },
          queryParamsHandling: 'merge',
          replaceUrl: true
        });
      }
    });
  }

  ngOnDestroy(): void {
    if (this.resendCooldownTimer) clearInterval(this.resendCooldownTimer);
  }

  private startResendCooldown(seconds = 60): void {
    if (this.resendCooldownTimer) clearInterval(this.resendCooldownTimer);
    this.resendCooldownSeconds = seconds;
    this.resendCooldownTimer = setInterval(() => {
      this.resendCooldownSeconds = Math.max(0, this.resendCooldownSeconds - 1);
      if (this.resendCooldownSeconds === 0 && this.resendCooldownTimer) {
        clearInterval(this.resendCooldownTimer);
        this.resendCooldownTimer = null;
      }
    }, 1000);
  }

  // Strength Evaluator State
  strengthScore = 0;
  strengthLabel = 'Entropy: Weak';
  strengthClass = 'text-error';
  strengthHelp = 'Strong: 12+ chars, uppercase, number, symbol';

  get regUsername(): string {
    const emailPrefix = this.regEmail.trim().split('@')[0] ?? '';
    return emailPrefix.toLowerCase().replace(/[^a-z0-9-]/g, '-').replace(/-+/g, '-').replace(/^-|-$/g, '');
  }

  get passwordsMatch(): boolean {
    return this.regPassword.length > 0 && this.regPassword === this.regConfirmPassword;
  }

  switchTab(tab: 'login' | 'register') {
    this.activeTab = tab;
    this.showOtpVerification = false;
    this.authError = '';
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

  private getRequestErrorMessage(error: any, fallback: string): string {
    if (error?.status === 0) {
      return 'The server cannot be reached right now. Check your connection or try again when the service is available.';
    }
    if (error?.status === 503) {
      return error?.error?.message || 'The service is temporarily unavailable. Please try again later.';
    }
    if (typeof error?.status === 'number' && error.status >= 500) {
      return 'A server error prevented this request. Please try again later.';
    }

    const message = error?.error?.message || error?.error?.title;
    if (typeof message === 'string' && message.trim()) return message;

    const errors = error?.error?.errors;
    if (Array.isArray(errors) && errors.length) return errors.join(' ');
    if (errors && typeof errors === 'object') {
      const messages = Object.values(errors).flatMap(value => Array.isArray(value) ? value : [value]);
      const validMessages = messages.filter((value): value is string => typeof value === 'string' && !!value.trim());
      if (validMessages.length) return validMessages.join(' ');
    }
    return fallback;
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
        if (error?.status === 401) {
          this.authError = error?.error?.message || 'Email or password is incorrect.';
          return;
        }
        if (error?.status === 403) {
          this.authError = error?.error?.message || 'Confirm your email before signing in.';
          return;
        }
        this.authError = this.getRequestErrorMessage(error, 'Sign-in could not be completed. Please try again.');
      }
    });
  }

  handleRegisterSubmit() {
    if (this.isRegistering) return;
    if (!this.passwordsMatch) {
      this.authError = 'Enter the same password in both password fields.';
      return;
    }
    if (!this.agreeTerms) {
      this.authError = 'Please accept the Terms of Service and Privacy Policy to create your account.';
      return;
    }
    this.isRegistering = true;
    this.regBtnText = 'Provisioning Edge Node...';
    this.authError = '';
    const username = this.regUsername;
    this.auth.register({ fullName: this.regName.trim(), username, email: this.regEmail.trim(), password: this.regPassword }).subscribe({
      next: response => {
        this.isRegistering = false;
        this.regBtnText = 'Create Account & Start Portfolio';
        if (!response.succeeded) { this.authError = response.message || 'Account could not be created.'; return; }
        this.confirmationEmailSent = !!response.data?.confirmationEmailSent;
        this.confirmationMessage = this.confirmationEmailSent
          ? (response.message || 'Account created. Check your email to confirm your address before signing in.')
          : (response.message || 'Account created, but the confirmation email could not be sent. Check your mail settings, then request a new code.');
        this.loginEmail = this.regEmail.trim();
        this.otpCode = '';
        this.showOtpVerification = true;
        if (this.confirmationEmailSent) this.startResendCooldown();
      },
      error: error => {
        this.isRegistering = false;
        this.regBtnText = 'Create Account & Start Portfolio';
        this.authError = this.getRequestErrorMessage(error, 'Account could not be created. Please review your details and try again.');
      }
    });
  }

  handleOtpSubmit(): void {
    if (this.isConfirmingOtp) return;
    if (!/^\d{6}$/.test(this.otpCode)) {
      this.authError = 'Enter the six-digit code sent to your email.';
      return;
    }

    this.isConfirmingOtp = true;
    this.authError = '';
    this.auth.confirmEmailOtp(this.loginEmail, this.otpCode).subscribe({
      next: response => {
        this.isConfirmingOtp = false;
        this.showOtpVerification = false;
        this.activeTab = 'login';
        this.confirmationEmailSent = true;
        this.confirmationMessage = response.message || 'Email confirmed. You can now sign in.';
        this.otpCode = '';
      },
      error: error => {
        this.isConfirmingOtp = false;
        this.authError = this.getRequestErrorMessage(error, 'The code could not be verified. Check it and try again.');
      }
    });
  }

  resendConfirmationEmail(): void {
    const email = this.loginEmail.trim();
    if (!email || this.isResendingConfirmation) return;
    this.isResendingConfirmation = true;
    this.confirmationMessage = '';
    this.authError = '';
    this.auth.resendConfirmationEmail(email).subscribe({
      next: response => {
        this.isResendingConfirmation = false;
        this.confirmationEmailSent = true;
        this.confirmationMessage = response.message || 'If the account exists and needs confirmation, a new code will be sent.';
        this.otpCode = '';
        this.showOtpVerification = true;
        this.startResendCooldown();
      },
      error: error => {
        this.isResendingConfirmation = false;
        this.confirmationEmailSent = false;
        this.authError = this.getRequestErrorMessage(error, 'Confirmation email could not be sent. Please try again later.');
      }
    });
  }
}
