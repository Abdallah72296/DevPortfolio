import { Component, inject, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { MockDataService } from '../../core/services/mock-data.service';
import { AdminApiService } from '../../core/services/admin-api.service';
import { PortfolioDataService } from '../../core/services/portfolio-data.service';
import { PortfolioPresentationService } from '../../core/services/portfolio-presentation.service';
import { getSocialIconName, getSocialSvg, getSocialColor } from '../../core/helpers/social-icon.helper';
import { getSkillIconDisplay, SkillIconDisplay } from '../../core/helpers/skill-registry.helper';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-public-portfolio-desktop',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './public-portfolio-desktop.component.html',
  styleUrl: './public-portfolio-desktop.component.css'
})
export class PublicPortfolioDesktopComponent implements OnInit {
  readonly adminApi = inject(AdminApiService);
  readonly authService = inject(AuthService);
  private readonly portfolioData = inject(PortfolioDataService);
  readonly route = inject(ActivatedRoute);
  private readonly presentation = inject(PortfolioPresentationService);
  private mockData = inject(MockDataService);
  profile = this.mockData.profile;
  projects = this.mockData.projects;
  experiences = this.mockData.experience;
  portfolio: any = null;
  portfolioLoading = true;
  portfolioError = '';

  activeNav: string = 'projects';
  mobileMenuOpen: boolean = false;
  filterCategory: string = 'all';

  contactForm = {
    name: '',
    organization: '',
    email: '',
    inquiryType: 'lead',
    message: ''
  };

  isSending = false;
  sentSuccess = false;
  sendError = '';
  isWhatsAppFeedback = false;

  getWhatsAppNumber(): string | null {
    if (this.portfolio?.socialLinks) {
      const waSocial = this.portfolio.socialLinks.find((s: any) =>
        (s.platform && s.platform.toLowerCase().includes('whatsapp')) ||
        (s.url && (s.url.includes('wa.me') || s.url.includes('whatsapp.com')))
      );
      if (waSocial?.url) {
        const match = waSocial.url.match(/wa\.me\/([0-9+]+)/);
        if (match) return match[1].replace(/\D/g, '');
      }
    }
    if (this.portfolio?.profile?.phone) {
      const digits = this.portfolio.profile.phone.replace(/\D/g, '');
      if (digits.length >= 7) return digits;
    }
    return null;
  }

  getWhatsAppUrl(): string | null {
    if (this.portfolio?.socialLinks) {
      const waSocial = this.portfolio.socialLinks.find((s: any) =>
        (s.platform && s.platform.toLowerCase().includes('whatsapp')) ||
        (s.url && (s.url.includes('wa.me') || s.url.includes('whatsapp.com')))
      );
      if (waSocial?.url) return waSocial.url;
    }
    const num = this.getWhatsAppNumber();
    if (num) {
      return `https://wa.me/${num}`;
    }
    return null;
  }

  getWhatsAppDisplay(): string {
    if (this.portfolio?.profile?.phone) {
      return this.portfolio.profile.phone;
    }
    const num = this.getWhatsAppNumber();
    if (num) return `+${num}`;
    return 'Chat on WhatsApp';
  }

  get contactRows(): Array<{
    id: string;
    label: string;
    value: string;
    href?: string | null;
    iconType: 'material' | 'svg';
    icon: string;
    colorClass: string;
    isExternal?: boolean;
  }> {
    const rows: Array<{
      id: string;
      label: string;
      value: string;
      href?: string | null;
      iconType: 'material' | 'svg';
      icon: string;
      colorClass: string;
      isExternal?: boolean;
    }> = [];

    const prof = this.portfolio?.profile;

    // 1. Email (Direct Inbox)
    if (prof?.email) {
      rows.push({
        id: 'email',
        label: 'DIRECT INBOX',
        value: prof.email,
        href: `mailto:${prof.email}`,
        iconType: 'material',
        icon: 'mark_email_read',
        colorClass: 'text-secondary'
      });
    }

    // 2. WhatsApp Channel
    const waUrl = this.getWhatsAppUrl();
    if (waUrl) {
      rows.push({
        id: 'whatsapp',
        label: 'WHATSAPP DIRECT',
        value: this.getWhatsAppDisplay(),
        href: waUrl,
        iconType: 'svg',
        icon: 'whatsapp',
        colorClass: 'text-[#25d366]',
        isExternal: true
      });
    }

    // 3. Mobile / Direct Phone
    if (prof?.phone) {
      rows.push({
        id: 'phone',
        label: 'DIRECT MOBILE',
        value: prof.phone,
        href: `tel:${prof.phone}`,
        iconType: 'material',
        icon: 'call',
        colorClass: 'text-secondary'
      });
    }

    // 4. Primary Location
    if (prof?.location) {
      rows.push({
        id: 'location',
        label: 'LOCATION / BASE',
        value: prof.location,
        href: null,
        iconType: 'material',
        icon: 'location_on',
        colorClass: 'text-tertiary'
      });
    }

    // 5. If fewer than 3 rows, populate from connected social channels (e.g. LinkedIn, Telegram)
    if (rows.length < 3 && this.portfolio?.socialLinks?.length) {
      for (const social of this.portfolio.socialLinks) {
        if (rows.length >= 4) break;
        const plat = (social.platform || '').toLowerCase();
        if (plat.includes('whatsapp') || plat.includes('mail')) continue;
        const cleanVal = (social.url || '').replace(/^https?:\/\/(www\.)?/, '').replace(/\/$/, '');
        rows.push({
          id: social.id || plat,
          label: `${(social.platform || 'NETWORK').toUpperCase()} PROFILE`,
          value: cleanVal || social.platform,
          href: social.url,
          iconType: this.getSocialSvg(social) ? 'svg' : 'material',
          icon: this.getSocialSvg(social) ? social.platform : this.getSocialIcon(social),
          colorClass: 'text-secondary',
          isExternal: true
        });
      }
    }

    // 6. Guarantee at least 3 rows: add Response SLA if needed
    if (rows.length < 3) {
      rows.push({
        id: 'sla',
        label: 'RESPONSE SLA',
        value: '< 24 hours (Guaranteed)',
        href: null,
        iconType: 'material',
        icon: 'schedule',
        colorClass: 'text-tertiary'
      });
    }

    // 7. Guarantee at least 3 rows: add Location fallback if needed
    if (rows.length < 3 && !rows.some(r => r.id === 'location')) {
      rows.push({
        id: 'location',
        label: 'LOCATION / BASE',
        value: 'Worldwide • Remote Available',
        href: null,
        iconType: 'material',
        icon: 'location_on',
        colorClass: 'text-tertiary'
      });
    }

    // Strictly 3 or 4 rows
    return rows.slice(0, 4);
  }

  sendViaWhatsApp(e?: Event): void {
    if (e) {
      e.preventDefault();
    }
    if (!this.contactForm.name?.trim() || !this.contactForm.message?.trim()) {
      this.sendError = 'Please fill in your name and message before opening WhatsApp.';
      return;
    }
    this.sendError = '';

    const lines: string[] = [
      `*New Inquiry via Portfolio*`,
      `*Name:* ${this.contactForm.name.trim()}`,
      this.contactForm.organization ? `*Organization:* ${this.contactForm.organization.trim()}` : '',
      this.contactForm.email ? `*Email:* ${this.contactForm.email.trim()}` : '',
      `*Purpose:* ${this.contactForm.inquiryType}`,
      `*Message:*`,
      this.contactForm.message.trim()
    ].filter(Boolean);

    const messageText = lines.join('\n');
    const waBase = this.getWhatsAppUrl();

    let targetUrl: string;
    if (waBase) {
      const sep = waBase.includes('?') ? '&' : '?';
      targetUrl = `${waBase}${sep}text=${encodeURIComponent(messageText)}`;
    } else {
      targetUrl = `https://wa.me/?text=${encodeURIComponent(messageText)}`;
    }

    if (typeof window !== 'undefined') {
      window.open(targetUrl, '_blank', 'noopener,noreferrer');
      this.isWhatsAppFeedback = true;
      this.sentSuccess = true;
      setTimeout(() => this.sentSuccess = false, 6000);
    }
  }

  getProjectGithubUrl(project: any): string | null {
    if (!project) return null;
    const url = project.githubUrl || project.repoUrl;
    if (url && typeof url === 'string' && url.trim().length > 0) {
      return url.trim();
    }
    if (project.repo && typeof project.repo === 'string' && project.repo.trim().length > 0) {
      const r = project.repo.trim();
      return r.startsWith('http') ? r : `https://github.com/${r}`;
    }
    return null;
  }

  getProjectDemoUrl(project: any): string | null {
    if (!project) return null;
    const url = project.liveUrl || project.demoUrl;
    if (url && typeof url === 'string' && url.trim().length > 0) {
      return url.trim();
    }
    return null;
  }

  dispatchMessage(e: Event): void {
    e.preventDefault();
    if (!this.contactForm.name || !this.contactForm.email || !this.contactForm.message) return;
    this.isSending = true;
    this.isWhatsAppFeedback = false;

    const subject = `${this.contactForm.inquiryType} inquiry${this.contactForm.organization ? ` from ${this.contactForm.organization}` : ''}`;
    this.sendError = '';
    this.portfolioData.sendContactMessage({
      name: this.contactForm.name, email: this.contactForm.email,
      subject, message: this.contactForm.message
    }, this.route.snapshot.paramMap.get('username') ?? undefined).subscribe({
      next: () => {
        this.isSending = false;
        this.sentSuccess = true;
        this.contactForm = { name: '', organization: '', email: '', inquiryType: 'lead', message: '' };
        setTimeout(() => this.sentSuccess = false, 6000);
      },
      error: () => {
        this.isSending = false;
        this.sendError = 'Message could not be sent. Please try again.';
      }
    });
  }

  getSocialIcon(link: any): string {
    if (!link) return 'link';
    return getSocialIconName(link.platform, link.url, link.icon);
  }

  getSocialSvg(link: any): string | null {
    return getSocialSvg(link);
  }

  ngOnInit(): void {
    this.presentation.loadPublicSettings();
    this.route.paramMap.subscribe(params => {
      const username = params.get('username') ?? undefined;
      this.adminApi.trackPortfolioEvent('portfolio_view', username).subscribe({ error: () => undefined });
      this.portfolioLoading = true;
      this.portfolioError = '';
      this.portfolioData.getPublicPortfolio(username).subscribe({
      next: result => {
        this.portfolio = result?.data ?? null;
        this.portfolioLoading = false;
        if (!this.portfolio) this.portfolioError = 'This portfolio could not be found.';
      },
      error: () => { this.portfolioLoading = false; this.portfolioError = 'Portfolio content is temporarily unavailable.'; }
      });
    });
    this.updateActiveSectionOnScroll();
  }

  isManualScrolling = false;

  @HostListener('window:scroll')
  onWindowScroll(): void {
    if (this.isManualScrolling) return;
    this.updateActiveSectionOnScroll();
  }

  profileMenuOpen = false;
  linkCopied = false;

  toggleProfileMenu(event?: Event): void {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.profileMenuOpen = !this.profileMenuOpen;
  }

  closeProfileMenu(): void {
    this.profileMenuOpen = false;
  }

  @HostListener('document:click')
  onDocumentClick(): void {
    this.profileMenuOpen = false;
  }

  copyPortfolioLink(): void {
    const url = window.location.href;
    navigator.clipboard.writeText(url).then(() => {
      this.linkCopied = true;
      setTimeout(() => this.linkCopied = false, 2000);
    });
  }

  signOut(): void {
    this.profileMenuOpen = false;
    this.authService.logout();
  }

  get hasCv(): boolean {
    const cv = this.portfolio?.profile?.cvUrl?.trim();
    return !!(cv && !cv.startsWith('#') && cv.length > 3);
  }

  get hasEducation(): boolean {
    return Array.isArray(this.portfolio?.education) && this.portfolio.education.length > 0;
  }

  get hasCertifications(): boolean {
    return Array.isArray(this.portfolio?.certifications) && this.portfolio.certifications.length > 0;
  }

  get hasCredentials(): boolean {
    return this.hasEducation || this.hasCertifications;
  }

  get credentialsLabel(): string {
    if (this.hasEducation && this.hasCertifications) return 'Credentials';
    if (this.hasEducation) return 'Education';
    if (this.hasCertifications) return 'Certifications';
    return 'Credentials';
  }

  get credentialsTitle(): string {
    if (this.hasEducation && this.hasCertifications) return 'Formal Foundations & Certifications';
    if (this.hasEducation) return 'Academic & Formal Foundations';
    if (this.hasCertifications) return 'Industry Accreditations & Badges';
    return 'Formal Foundations';
  }

  get credentialsSubtitle(): string {
    if (this.hasEducation && this.hasCertifications) return 'Education & Industry Accreditations';
    if (this.hasEducation) return 'Academic Education & Higher Degrees';
    if (this.hasCertifications) return 'Professional Industry Accreditations';
    return 'Education & Accreditations';
  }

  normalizeSectionKey(key: string, title?: string): string | null {
    const k = (key || '').toLowerCase();
    const t = (title || '').toLowerCase();
    if (k.includes('project') || t.includes('project') || k === 'systems') return 'projects';
    if (k.includes('skill') || k.includes('stack') || t.includes('skill') || t.includes('stack')) return 'stack';
    if (k.includes('exp') || t.includes('exp') || k.includes('career') || t.includes('career')) return 'experience';
    if (k.includes('edu') || k.includes('cert') || k.includes('cred') || t.includes('cert') || t.includes('edu') || t.includes('cred')) return 'credentials';
    if (k.includes('contact') || t.includes('contact') || k.includes('reach')) return 'contact';
    return null;
  }

  get navItems(): Array<{ key: string; id: string; label: string; order: number }> {
    const baseItems: Record<string, { id: string; label: string; defaultOrder: number }> = {
      projects: { id: 'featured-systems', label: 'Projects', defaultOrder: 1 },
      stack: { id: 'stack', label: 'Stack', defaultOrder: 2 },
      experience: { id: 'experience', label: 'Experience', defaultOrder: 3 },
      contact: { id: 'contact', label: 'Contact', defaultOrder: 5 }
    };

    if (this.hasCredentials) {
      baseItems['credentials'] = { id: 'credentials', label: this.credentialsLabel, defaultOrder: 4 };
    }

    const sections = this.portfolio?.sections;
    if (Array.isArray(sections) && sections.length > 0) {
      const result: Array<{ key: string; id: string; label: string; order: number }> = [];
      const seen = new Set<string>();

      // 1. Read sections in their saved DisplayOrder from the database
      for (const s of sections) {
        if (s.isVisible === false) continue;
        const normKey = this.normalizeSectionKey(s.key, s.title);
        if (normKey && baseItems[normKey] && !seen.has(normKey)) {
          seen.add(normKey);
          result.push({
            key: normKey,
            id: baseItems[normKey].id,
            label: normKey === 'credentials' ? this.credentialsLabel : baseItems[normKey].label,
            order: typeof s.displayOrder === 'number' ? s.displayOrder : result.length + 1
          });
        }
      }

      // 2. Append any standard sections that were not explicitly in the backend response
      for (const [key, base] of Object.entries(baseItems)) {
        if (!seen.has(key)) {
          result.push({
            key,
            id: base.id,
            label: base.label,
            order: 100 + base.defaultOrder
          });
        }
      }

      return result.sort((a, b) => a.order - b.order);
    }

    // Default portfolio order
    const defaults = [
      { key: 'projects', id: 'featured-systems', label: 'Projects', order: 1 },
      { key: 'stack', id: 'stack', label: 'Stack', order: 2 },
      { key: 'experience', id: 'experience', label: 'Experience', order: 3 }
    ];
    if (this.hasCredentials) {
      defaults.push({ key: 'credentials', id: 'credentials', label: this.credentialsLabel, order: 4 });
    }
    defaults.push({ key: 'contact', id: 'contact', label: 'Contact', order: 5 });
    return defaults;
  }

  getSectionOrder(key: string): number {
    const items = this.navItems;
    const index = items.findIndex(item => item.key === key);
    return index !== -1 ? index + 1 : 99;
  }

  getSectionNumber(key: string): string {
    const order = this.getSectionOrder(key);
    return order < 10 ? `0${order}` : `${order}`;
  }

  updateActiveSectionOnScroll(): void {
    const scrollY = window.pageYOffset || document.documentElement.scrollTop;
    const windowHeight = window.innerHeight;
    const docHeight = document.documentElement.scrollHeight;

    const items = this.navItems;
    if (!items.length) return;

    // Hero / Top of page
    if (scrollY < 180) {
      this.activeNav = items[0].key;
      return;
    }

    // Bottom reached (Contact Form)
    if (scrollY + windowHeight >= docHeight - 120) {
      this.activeNav = items[items.length - 1].key;
      return;
    }

    const viewportTrigger = 180;
    let foundNav = this.activeNav;

    // Follow the dynamic visual sequence of sections
    for (const item of items) {
      const el = document.getElementById(item.id);
      if (el) {
        const rect = el.getBoundingClientRect();
        if (rect.top <= viewportTrigger && rect.bottom > viewportTrigger) {
          foundNav = item.key;
          break;
        }
      }
    }

    this.activeNav = foundNav;
  }

  scrollToSection(sectionId: string, navKey: string, event?: Event): void {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.activeNav = navKey;
    this.mobileMenuOpen = false;
    this.isManualScrolling = true;
    setTimeout(() => {
      this.isManualScrolling = false;
    }, 850);

    const element = document.getElementById(sectionId);
    if (element) {
      const headerOffset = 70;
      const elementPosition = element.getBoundingClientRect().top;
      const offsetPosition = elementPosition + window.pageYOffset - headerOffset;
      window.scrollTo({
        top: offsetPosition,
        behavior: 'smooth'
      });
    }
  }

  setFilter(category: 'all' | 'distributed' | 'cloud' | 'core' | 'wasm' | 'opensource'): void {
    this.filterCategory = category;
  }

  getSocialColor(link: any): string {
    return getSocialColor(link);
  }

  getSkillDisplay(skill: any): SkillIconDisplay {
    return getSkillIconDisplay(skill);
  }

  getBullets(description: string | null | undefined): string[] {
    if (!description) return [];
    const lines = description.split(/\r?\n/).map(l => l.trim()).filter(l => l.length > 0);
    if (lines.length > 1) return lines;
    if (description.includes('.') && description.length > 130) {
      const sentences = description.split(/(?<=\.)\s+/).map(s => s.trim()).filter(s => s.length > 0);
      if (sentences.length > 1) return sentences;
    }
    return [description];
  }

  getTotalNodes(): number {
    if (!this.portfolio) return 0;
    const skillsCount = (this.portfolio.skills ?? []).reduce(
      (total: number, cat: any) => total + (cat.skills?.length ?? 0), 0
    );
    const expCount = this.portfolio.experiences?.length ?? 0;
    const projCount = this.portfolio.projects?.length ?? 0;
    const eduCount = (this.portfolio.education?.length ?? 0) + (this.portfolio.certifications?.length ?? 0);
    return skillsCount + expCount + projCount + eduCount;
  }

  copiedShare = false;
  copyShareLink(): void {
    if (typeof window !== 'undefined' && navigator.clipboard) {
      navigator.clipboard.writeText(window.location.href);
      this.copiedShare = true;
      setTimeout(() => this.copiedShare = false, 3000);
    }
  }

  get totalSkillsCount(): number {
    if (!this.portfolio?.skills) return 0;
    return this.portfolio.skills.reduce((sum: number, cat: any) => sum + (cat.skills?.length || 0), 0);
  }

  get filteredProjects(): any[] {
    if (!this.portfolio?.projects) return [];
    if (this.filterCategory === 'all') return this.portfolio.projects;
    const term = this.filterCategory.toLowerCase();
    return this.portfolio.projects.filter((p: any) => {
      const tech = (p.technologies || []).join(' ').toLowerCase();
      const title = (p.title || '').toLowerCase();
      const desc = (p.description || '').toLowerCase();
      return tech.includes(term) || title.includes(term) || desc.includes(term);
    });
  }

  getCategoryIcon(categoryName: string): string {
    const name = (categoryName || '').toLowerCase();
    if (name.includes('lang') || name.includes('backend') || name.includes('code')) return 'code';
    if (name.includes('cloud') || name.includes('arch') || name.includes('distrib')) return 'hub';
    if (name.includes('front') || name.includes('ui') || name.includes('web')) return 'layers';
    if (name.includes('devops') || name.includes('infra') || name.includes('sys') || name.includes('observ')) return 'terminal';
    if (name.includes('data') || name.includes('db') || name.includes('sql')) return 'database';
    return 'category';
  }

  avatarImageFailed = false;
  onAvatarError(): void {
    this.avatarImageFailed = true;
  }

  resolveMediaUrl(url: string | null | undefined): string | null {
    if (!url) return null;
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
    const cleanUrl = url.startsWith('/') ? url : `/${url}`;
    return `${environment.apiUrl}${cleanUrl}`;
  }
}


