import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { forkJoin, of, Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { DashboardHeaderComponent } from '../../shared/components/dashboard-header/dashboard-header.component';
import { DashboardSidebarComponent } from '../../shared/components/dashboard-sidebar/dashboard-sidebar.component';
import { AdminApiService } from '../../core/services/admin-api.service';
import { formatFriendlyError, sanitizeTechnicalText } from '../../core/helpers/error-formatter';
import { PRESET_PLATFORMS, resolveSocialPlatform, getSocialIconName, getSocialSvg, getSocialColor } from '../../core/helpers/social-icon.helper';
import {
  searchSkillCatalog,
  resolveSkillIcon,
  registerLearnedSkill,
  getSkillIconDisplay,
  CatalogSkill,
  SkillIconDisplay
} from '../../core/helpers/skill-registry.helper';
import {
  CertificationDto,
  CreateCertificationRequest,
  CreateEducationRequest,
  CreateExperienceRequest,
  CreateProjectRequest,
  CvPortfolioDraft,
  EducationDto,
  ExperienceDto,
  ProjectDto,
  SkillCategoryDto,
  SkillDto,
  SocialLinkDto
} from '../../core/models/admin.models';
import { 
  MetricsVisibilityConfig, 
  parseMetricsConfig, 
  encodeMetricsConfig, 
  DEFAULT_METRICS_CONFIG 
} from '../../core/helpers/metrics-config.helper';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-cv-import-profile-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, DashboardHeaderComponent, DashboardSidebarComponent],
  templateUrl: './cv-import-profile-editor.component.html',
  styleUrl: './cv-import-profile-editor.component.css'
})
export class CvImportProfileEditorComponent implements OnInit {
  readonly api = inject(AdminApiService);

  // Hero Metrics & Stat Cards Visibility Configuration
  metricsConfig: MetricsVisibilityConfig = { ...DEFAULT_METRICS_CONFIG };
  existingParagraphs: string[] = [];

  // Profile Form
  formData = {
    name: 'Alex Rivera',
    title: 'Staff Distributed Systems Engineer',
    location: 'San Francisco, CA • Remote',
    email: 'alex@rivera.systems',
    phone: '+1 (415) 890-2384',
    bio: 'Architecting ultra-low latency distributed state engines & real-time telemetry pipelines handling petabyte-scale throughput. Passionate about consensus mechanisms, zero-allocation runtimes, and Linux kernel observability.',
    avatarUrl: '',
    cvUrl: '',
    availableForHire: true
  };

  // State flags & messages
  isParsing = false;
  isSaving = false;
  toastMsg = '';
  errorMsg = '';
  cvErrorMsg = '';
  selectedCv?: File;
  pendingCvDraft: CvPortfolioDraft | null = null;
  hasPendingCvImport = false;
  isApplyingDraft = false;
  previousDbIds = {
    experiences: [] as string[],
    projects: [] as string[],
    categories: [] as string[],
    educations: [] as string[],
    certifications: [] as string[],
    socialLinks: [] as string[]
  };
  isUploadingCv = false;
  isDraggingCv = false;
  autoProcessOnDrop = true;
  isUploadingAvatar = false;
  isDraggingAvatar = false;
  isUploadingProjectImage = false;
  isDraggingProjectImage = false;

  // Skills
  categories: SkillCategoryDto[] = [];
  selectedCategoryId: string = '';
  newSkillName: string = '';
  skillSuggestions: CatalogSkill[] = [];
  showSkillSuggestions = false;
  customSkillIcon = '';
  isUploadingSkillIcon = false;
  detectedSkillIconDisplay: SkillIconDisplay = { type: 'material', value: 'code' };
  newCategoryName: string = '';

  // Experiences
  experiences: ExperienceDto[] = [];
  editingExperience: ExperienceDto | null = null;
  experienceModalOpen = false;
  experienceForm: CreateExperienceRequest = this.emptyExperience();

  // Projects
  projects: ProjectDto[] = [];
  editingProject: ProjectDto | null = null;
  projectModalOpen = false;
  projectForm: CreateProjectRequest = this.emptyProject();
  projectTechInput = '';

  // Education & Certifications
  educations: EducationDto[] = [];
  certifications: CertificationDto[] = [];
  credentialType: 'education' | 'certification' = 'education';
  credentialModalOpen = false;
  editingEducation: EducationDto | null = null;
  educationForm: CreateEducationRequest = this.emptyEducation();
  editingCertification: CertificationDto | null = null;
  certificationForm: CreateCertificationRequest = this.emptyCertification();

  // Social Links
  socialLinks: SocialLinkDto[] = [];
  socialForm = { platform: '', url: '', icon: '' };
  editingSocialId: string | null = null;
  presetPlatforms = PRESET_PLATFORMS;
  selectedPresetPlatform = '';

  // Delete Confirmation Modal State (In-Page Card)
  deleteModal = {
    isOpen: false,
    title: '',
    itemName: '',
    itemType: '',
    message: '',
    isDeleting: false,
    errorMessage: '',
    onConfirm: () => {}
  };

  openDeleteConfirm(options: {
    title: string;
    itemName: string;
    itemType?: string;
    message?: string;
    action: () => void;
  }): void {
    this.deleteModal = {
      isOpen: true,
      title: options.title,
      itemName: options.itemName,
      itemType: options.itemType || 'Item',
      message: options.message || 'Are you sure you want to delete this item? This action cannot be undone.',
      isDeleting: false,
      errorMessage: '',
      onConfirm: options.action
    };
  }

  closeDeleteConfirm(force: boolean = false): void {
    if (this.deleteModal.isDeleting && !force) return;
    this.deleteModal.isOpen = false;
    this.deleteModal.isDeleting = false;
    this.deleteModal.errorMessage = '';
    this.deleteModal.onConfirm = () => {};
  }

  confirmDelete(): void {
    if (this.deleteModal.onConfirm) {
      this.deleteModal.onConfirm();
    }
  }

  get detectedIconPreview(): string {
    return getSocialIconName(this.socialForm.platform, this.socialForm.url, this.socialForm.icon);
  }

  onPresetPlatformChange(presetId: string): void {
    const preset = this.presetPlatforms.find(p => p.id === presetId);
    if (preset) {
      this.socialForm.platform = preset.name;
      this.socialForm.icon = preset.icon;
      if (!this.socialForm.url || this.socialForm.url === 'https://') {
        this.socialForm.url = preset.defaultUrlPrefix;
      }
    }
  }

  onSocialInput(): void {
    const resolved = resolveSocialPlatform(this.socialForm.platform || this.socialForm.url, this.socialForm.icon);
    if (!this.socialForm.platform && resolved.id !== 'link') {
      this.socialForm.platform = resolved.name;
    }
    this.socialForm.icon = resolved.materialIcon;
  }

  getSocialIcon(link: any): string {
    return getSocialIconName(link?.platform, link?.url, link?.icon);
  }

  ngOnInit(): void {
    this.loadProfile();
    this.loadSkills();
    this.loadExperiences();
    this.loadProjects();
    this.loadCredentials();
    this.loadSocialLinks();
  }

  // ==========================================
  // 1. Profile Load & Save
  // ==========================================
  loadProfile(): void {
    this.api.getProfile().subscribe({
      next: response => {
        if (response.succeeded && response.data) {
          const p = response.data;
          this.existingParagraphs = p.paragraphs ?? [];
          this.metricsConfig = parseMetricsConfig(p.paragraphs);
          this.formData = {
            name: p.fullName ?? '',
            title: p.title ?? '',
            location: p.location ?? '',
            email: p.email ?? '',
            phone: p.phone ?? '',
            bio: p.summary ?? '',
            avatarUrl: p.avatarUrl ?? '',
            cvUrl: p.cvUrl ?? '',
            availableForHire: p.availableForHire ?? true
          };
        }
      },
      error: () => this.errorMsg = 'Profile could not be loaded. Please verify API connection.'
    });
  }

  saveMetricsConfig(): void {
    const updatedParagraphs = encodeMetricsConfig(this.metricsConfig, this.existingParagraphs);
    this.existingParagraphs = updatedParagraphs;
    this.api.updateProfile({
      fullName: this.formData.name,
      title: this.formData.title,
      location: this.formData.location,
      email: this.formData.email,
      phone: this.formData.phone,
      summary: this.formData.bio,
      avatarUrl: this.formData.avatarUrl,
      cvUrl: this.formData.cvUrl,
      availableForHire: this.formData.availableForHire,
      paragraphs: updatedParagraphs
    }).subscribe({
      next: () => {
        this.toastMsg = 'تم حفظ إعدادات إظهار وإخفاء إحصائيات الهيدر بنجاح.';
        setTimeout(() => this.toastMsg = '', 3500);
      },
      error: () => {
        this.errorMsg = 'تعذر حفظ إعدادات الإحصائيات، تحقق من الاتصال.';
      }
    });
  }

  saveAndDeploy(): void {
    this.isSaving = true;
    this.errorMsg = '';
    const updatedParagraphs = encodeMetricsConfig(this.metricsConfig, this.existingParagraphs);
    this.existingParagraphs = updatedParagraphs;
    this.api.updateProfile({
      fullName: this.formData.name,
      title: this.formData.title,
      location: this.formData.location,
      email: this.formData.email,
      phone: this.formData.phone,
      summary: this.formData.bio,
      avatarUrl: this.formData.avatarUrl,
      cvUrl: this.formData.cvUrl,
      availableForHire: this.formData.availableForHire,
      paragraphs: updatedParagraphs
    }).subscribe({
      next: response => {
        if (!response.succeeded) {
          this.isSaving = false;
          this.errorMsg = response.message || 'تعذر حفظ بيانات الملف الشخصي.';
          return;
        }

        if (this.hasPendingCvImport) {
          this.commitCvItemsToDatabase();
        } else {
          this.isSaving = false;
          this.toastMsg = 'تم حفظ ونشر كافة تعديلات الملف الشخصي بنجاح!';
          setTimeout(() => this.toastMsg = '', 3500);
        }
      },
      error: () => {
        this.isSaving = false;
        this.errorMsg = 'تعذر حفظ الملف الشخصي. يرجى التحقق من الاتصال بالخادم.';
      }
    });
  }

  // ==========================================
  // 2. CV Upload & Full AI Parse (AutoComplete)
  // ==========================================
  onCvDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingCv = true;
  }

  onCvDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingCv = false;
  }

  onCvDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingCv = false;
    const file = event.dataTransfer?.files?.[0];
    if (file) {
      this.handleCvFile(file);
    }
  }

  selectCv(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      this.handleCvFile(file);
    }
  }

  handleCvFile(file: File): void {
    if (file && (file.type === 'application/pdf' || file.name.toLowerCase().endsWith('.pdf'))) {
      this.selectedCv = file;
      this.cvErrorMsg = '';
      this.errorMsg = '';
      this.toastMsg = `CV "${file.name}" loaded.`;
      setTimeout(() => this.toastMsg = '', 3000);

      if (this.autoProcessOnDrop) {
        this.triggerParse();
        this.uploadCv();
      }
    } else {
      this.selectedCv = undefined;
      this.cvErrorMsg = 'Please select or drop a valid PDF CV file.';
    }
  }

  uploadCv(): void {
    if (!this.selectedCv) return;
    this.isUploadingCv = true;
    this.cvErrorMsg = '';
    this.errorMsg = '';
    this.api.uploadCv(this.selectedCv).subscribe({
      next: response => {
        this.isUploadingCv = false;
        if (!response.succeeded) {
          this.cvErrorMsg = response.message || 'CV upload failed.';
          return;
        }
        this.formData.cvUrl = response.relativeUrl;
        this.toastMsg = 'CV uploaded and attached to profile.';
        setTimeout(() => this.toastMsg = '', 4000);
      },
      error: error => {
        this.isUploadingCv = false;
        this.cvErrorMsg = error?.error?.message || 'CV upload failed.';
      }
    });
  }

  formatIsoDate(raw: any, fallback = new Date().toISOString().slice(0, 10)): string {
    if (!raw || typeof raw !== 'string') return fallback;
    const trimmed = raw.trim();
    if (/^\d{4}-\d{2}-\d{2}$/.test(trimmed)) return trimmed;
    if (/^\d{4}-\d{2}$/.test(trimmed)) return `${trimmed}-01`;
    if (/^\d{4}$/.test(trimmed)) return `${trimmed}-01-01`;
    const parsed = Date.parse(trimmed);
    if (!isNaN(parsed)) return new Date(parsed).toISOString().slice(0, 10);
    const yearMatch = trimmed.match(/\b(19\d\d|20\d\d)\b/);
    if (yearMatch) {
      const year = yearMatch[1];
      const months = ['jan', 'feb', 'mar', 'apr', 'may', 'jun', 'jul', 'aug', 'sep', 'oct', 'nov', 'dec'];
      const lower = trimmed.toLowerCase();
      const mIdx = months.findIndex(m => lower.includes(m));
      const month = mIdx >= 0 ? String(mIdx + 1).padStart(2, '0') : '01';
      return `${year}-${month}-01`;
    }
    return fallback;
  }

  triggerParse(): void {
    if (!this.selectedCv) {
      this.cvErrorMsg = 'يرجى اختيار ملف سيرة ذاتية (PDF) أولاً.';
      return;
    }
    this.isParsing = true;
    this.cvErrorMsg = '';
    this.errorMsg = '';
    this.toastMsg = 'جاري تحليل واستخراج بيانات السيرة الذاتية عبر محرك الذكاء الاصطناعي...';

    this.api.parseCv(this.selectedCv).subscribe({
      next: response => {
        this.isParsing = false;
        const draft = response.data;
        if (!response.succeeded || !draft) {
          this.cvErrorMsg = formatFriendlyError(response.message, 'تعذر قراءة بيانات السيرة الذاتية، يرجى التأكد من محتوى الملف.');
          return;
        }

        // Clean & extract concise title vs bio if title is too long
        let title = draft.personalInfo?.jobTitle || this.formData.title;
        let bio = draft.personalInfo?.about || this.formData.bio;
        if (title && title.length > 50) {
          const match = title.match(/^([A-Za-z0-9+#.\s]+?)(?:\s+(?:with|having|specializing|experienced|who)\b|[.,\n]|$)/i);
          if (match && match[1] && match[1].trim().length >= 3) {
            const extractedTitle = match[1].trim();
            if (!bio || bio.length < title.length) {
              bio = title;
            }
            title = extractedTitle;
          }
        }

        // 1. Populate form fields in-memory
        if (draft.personalInfo) {
          this.formData.name = draft.personalInfo.name || this.formData.name;
          this.formData.title = title;
          this.formData.bio = bio;
        }

        const now = Date.now();

        // Snapshot existing non-temp database IDs so they can be cleanly replaced upon saveAndDeploy
        this.previousDbIds = {
          experiences: this.experiences.filter(e => !e.id.startsWith('temp-')).map(e => e.id),
          projects: this.projects.filter(p => !p.id.startsWith('temp-')).map(p => p.id),
          categories: this.categories.filter(c => !c.id.startsWith('temp-')).map(c => c.id),
          educations: this.educations.filter(e => !e.id.startsWith('temp-')).map(e => e.id),
          certifications: this.certifications.filter(c => !c.id.startsWith('temp-')).map(c => c.id),
          socialLinks: this.socialLinks.filter(s => !s.id.startsWith('temp-')).map(s => s.id)
        };

        // 2. Populate skills into categories array in-memory
        if (draft.skills && draft.skills.length > 0) {
          this.categories = draft.skills.map((cat, catIdx) => ({
            id: `temp-cat-${catIdx}-${now}`,
            name: cat.category,
            displayOrder: catIdx,
            isVisible: true,
            skills: cat.skills.map((s, sIdx) => ({
              id: `temp-skill-${catIdx}-${sIdx}-${now}`,
              name: s.name,
              icon: s.icon || resolveSkillIcon(s.name),
              displayOrder: sIdx,
              isVisible: true,
              skillCategoryId: `temp-cat-${catIdx}-${now}`
            }))
          }));
          this.selectedCategoryId = this.categories[0]?.id || '';
        }

        // 3. Populate experiences into experiences array in-memory
        if (draft.experiences && draft.experiences.length > 0) {
          this.experiences = draft.experiences.map((exp, idx) => ({
            id: `temp-exp-${idx}-${now}`,
            jobTitle: exp.jobTitle,
            companyName: exp.companyName,
            description: exp.description || '',
            startDate: this.formatIsoDate(exp.startDate),
            endDate: exp.isCurrent ? null : (exp.endDate ? this.formatIsoDate(exp.endDate) : null),
            isCurrent: exp.isCurrent,
            displayOrder: idx,
            isVisible: true
          }));
        }

        // 4. Populate projects into projects array in-memory
        if (draft.projects && draft.projects.length > 0) {
          this.projects = draft.projects.map((proj, idx) => ({
            id: `temp-proj-${idx}-${now}`,
            title: proj.title,
            description: proj.description || '',
            image: proj.image || '',
            liveUrl: proj.liveUrl || '',
            githubUrl: proj.githubUrl || '',
            technologies: proj.technologies || [],
            isFeatured: false,
            isVisible: true,
            displayOrder: idx
          }));
        }

        // 5. Populate education into educations array in-memory
        if (draft.education && draft.education.length > 0) {
          this.educations = draft.education.map((edu, idx) => ({
            id: `temp-edu-${idx}-${now}`,
            degree: edu.degree || 'Degree',
            institutionName: edu.institutionName || 'University',
            fieldOfStudy: edu.fieldOfStudy || '',
            description: edu.description || '',
            startDate: this.formatIsoDate(edu.startDate),
            endDate: edu.isCurrent ? null : (edu.endDate ? this.formatIsoDate(edu.endDate) : null),
            isCurrent: edu.isCurrent,
            displayOrder: idx,
            isVisible: true
          }));
        }

        // 6. Populate certifications into certifications array in-memory
        if (draft.certifications && draft.certifications.length > 0) {
          this.certifications = draft.certifications.map((cert, idx) => ({
            id: `temp-cert-${idx}-${now}`,
            name: cert.name,
            issuingOrganization: cert.issuingOrganization || '',
            issueDate: this.formatIsoDate(cert.issueDate),
            credentialId: cert.credentialId || '',
            credentialUrl: cert.credentialUrl || '',
            description: cert.description || '',
            displayOrder: idx,
            isVisible: true
          }));
        }

        // 7. Populate social links into socialLinks array in-memory
        if (draft.socialLinks && draft.socialLinks.length > 0) {
          this.socialLinks = draft.socialLinks.map((s, idx) => ({
            id: `temp-social-${idx}-${now}`,
            platform: s.platform,
            url: s.url,
            icon: getSocialIconName(s.platform, s.url),
            displayOrder: idx,
            isVisible: true
          }));
        }

        this.hasPendingCvImport = true;
        this.pendingCvDraft = null;
        this.toastMsg = 'تم استخراج وتعبئة جميع بيانات السيرة الذاتية في أماكنها بنجاح! راجع البيانات أدناه وعدّلها كما تريد، ولن يتم حفظها في قاعدة البيانات إلا عند الضغط على "Save Profile Changes".';
        setTimeout(() => this.toastMsg = '', 7000);
      },
      error: err => {
        this.isParsing = false;
        this.cvErrorMsg = formatFriendlyError(err, 'تعذر قراءة ملف السيرة الذاتية حالياً، يرجى المحاولة لاحقاً.');
      }
    });
  }

  commitCvItemsToDatabase(): void {
    this.isSaving = true;
    this.toastMsg = 'جاري حفظ واعتماد جميع بيانات الملف الشخصي والأقسام في قاعدة البيانات...';

    // 1. Prepare cleanup of old database entities replaced by this import
    const cleanup$: Observable<any>[] = [];
    if (this.previousDbIds.experiences?.length) {
      this.previousDbIds.experiences.forEach(id => cleanup$.push(this.api.deleteExperience(id).pipe(catchError(() => of(null)))));
    }
    if (this.previousDbIds.projects?.length) {
      this.previousDbIds.projects.forEach(id => cleanup$.push(this.api.deleteProject(id).pipe(catchError(() => of(null)))));
    }
    if (this.previousDbIds.categories?.length) {
      this.previousDbIds.categories.forEach(id => cleanup$.push(this.api.deleteSkillCategory(id).pipe(catchError(() => of(null)))));
    }
    if (this.previousDbIds.educations?.length) {
      this.previousDbIds.educations.forEach(id => cleanup$.push(this.api.deleteEducation(id).pipe(catchError(() => of(null)))));
    }
    if (this.previousDbIds.certifications?.length) {
      this.previousDbIds.certifications.forEach(id => cleanup$.push(this.api.deleteCertification(id).pipe(catchError(() => of(null)))));
    }
    if (this.previousDbIds.socialLinks?.length) {
      this.previousDbIds.socialLinks.forEach(id => cleanup$.push(this.api.deleteSocialLink(id).pipe(catchError(() => of(null)))));
    }

    const executeCreations = () => {
      const createOps$: Observable<any>[] = [];

      // Experiences
      this.experiences.forEach((exp, idx) => {
        const jobTitle = exp.jobTitle || 'Developer';
        const companyName = exp.companyName || 'Company';
        const desc = exp.description?.trim() || `${jobTitle} at ${companyName}`;
        createOps$.push(
          this.api.createExperience({
            jobTitle,
            companyName,
            description: desc,
            startDate: exp.startDate,
            endDate: exp.isCurrent ? null : exp.endDate,
            isCurrent: exp.isCurrent,
            displayOrder: idx
          }).pipe(catchError(() => of(null)))
        );
      });

      // Projects
      this.projects.forEach((proj, idx) => {
        createOps$.push(
          this.api.createProject({
            title: proj.title,
            description: proj.description || '',
            technologies: proj.technologies || [],
            liveUrl: proj.liveUrl,
            githubUrl: proj.githubUrl,
            image: proj.image,
            isFeatured: proj.isFeatured,
            displayOrder: idx
          }).pipe(catchError(() => of(null)))
        );
      });

      // Skills (Categories with Skills)
      this.categories.forEach((cat, idx) => {
        const skillNames = (cat.skills || []).map(s => s.name);
        createOps$.push(
          this.api.createCategoryWithSkills({
            name: cat.name,
            skills: skillNames,
            displayOrder: idx
          }).pipe(catchError(() => of(null)))
        );
      });

      // Education
      this.educations.forEach((edu, idx) => {
        const degree = edu.degree || 'Degree';
        const institutionName = edu.institutionName || 'University';
        const desc = edu.description?.trim() || `${degree} - ${institutionName}`;
        createOps$.push(
          this.api.createEducation({
            degree,
            institutionName,
            fieldOfStudy: edu.fieldOfStudy || null,
            startDate: edu.startDate,
            endDate: edu.isCurrent ? null : edu.endDate,
            description: desc,
            isCurrent: edu.isCurrent,
            displayOrder: idx
          }).pipe(catchError(() => of(null)))
        );
      });

      // Certifications
      this.certifications.forEach((cert, idx) => {
        createOps$.push(
          this.api.createCertification({
            name: cert.name,
            issuingOrganization: cert.issuingOrganization || 'Organization',
            issueDate: cert.issueDate,
            credentialId: cert.credentialId,
            credentialUrl: cert.credentialUrl,
            description: cert.description || '',
            displayOrder: idx
          }).pipe(catchError(() => of(null)))
        );
      });

      // Social Links
      this.socialLinks.forEach((link, idx) => {
        const iconName = link.icon || getSocialIconName(link.platform, link.url);
        createOps$.push(
          this.api.createSocialLink({
            platform: link.platform,
            url: link.url,
            icon: iconName,
            displayOrder: idx
          }).pipe(catchError(() => of(null)))
        );
      });

      const finalizeCommit = () => {
        this.isSaving = false;
        this.hasPendingCvImport = false;
        this.previousDbIds = { experiences: [], projects: [], categories: [], educations: [], certifications: [], socialLinks: [] };
        this.toastMsg = 'تم حفظ ونشر كافة تعديلات الملف الشخصي وبيانات السيرة الذاتية بنجاح!';
        setTimeout(() => this.toastMsg = '', 4500);
        this.loadProfile();
        this.loadSkills();
        this.loadExperiences();
        this.loadProjects();
        this.loadCredentials();
        this.loadSocialLinks();
      };

      if (createOps$.length > 0) {
        forkJoin(createOps$).subscribe({
          next: () => finalizeCommit(),
          error: () => finalizeCommit()
        });
      } else {
        finalizeCommit();
      }
    };

    if (cleanup$.length > 0) {
      forkJoin(cleanup$).subscribe({
        next: () => executeCreations(),
        error: () => executeCreations()
      });
    } else {
      executeCreations();
    }
  }

  // ==========================================
  // 3. Skills CRUD
  // ==========================================
  loadSkills(): void {
    this.api.getSkillCategories().subscribe({
      next: res => {
        if (res.succeeded) {
          this.categories = res.data ?? [];
          if (!this.selectedCategoryId && this.categories.length) {
            this.selectedCategoryId = this.categories[0].id;
          }
        }
      }
    });
  }

  onSkillNameInput(): void {
    if (!this.newSkillName.trim()) {
      this.skillSuggestions = [];
      this.showSkillSuggestions = false;
      this.detectedSkillIconDisplay = { type: 'material', value: 'code' };
      return;
    }
    this.skillSuggestions = searchSkillCatalog(this.newSkillName);
    this.showSkillSuggestions = this.skillSuggestions.length > 0;
    this.detectedSkillIconDisplay = getSkillIconDisplay(this.newSkillName, this.customSkillIcon);
  }

  selectSkillSuggestion(item: CatalogSkill): void {
    this.newSkillName = item.name;
    this.customSkillIcon = item.icon;
    this.detectedSkillIconDisplay = getSkillIconDisplay(item.name, item.icon);
    this.showSkillSuggestions = false;

    // Auto-select category if matching category exists
    const matchCat = this.categories.find(c =>
      c.name.toLowerCase().includes(item.category.toLowerCase()) ||
      item.category.toLowerCase().includes(c.name.toLowerCase())
    );
    if (matchCat) {
      this.selectedCategoryId = matchCat.id;
    }
  }

  hideSkillSuggestionsWithDelay(): void {
    setTimeout(() => {
      this.showSkillSuggestions = false;
    }, 250);
  }

  uploadCustomSkillIcon(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || !input.files[0]) return;
    const file = input.files[0];
    this.isUploadingSkillIcon = true;
    this.api.uploadImage(file).subscribe({
      next: res => {
        this.isUploadingSkillIcon = false;
        if (res.succeeded && res.url) {
          this.customSkillIcon = res.url;
          this.detectedSkillIconDisplay = { type: 'image', value: res.url };
          this.toastMsg = 'تم رفع صورة أيقونة المهارة بنجاح.';
          setTimeout(() => this.toastMsg = '', 3000);
        }
      },
      error: () => {
        this.isUploadingSkillIcon = false;
        this.errorMsg = 'تعذر رفع صورة الأيقونة.';
      }
    });
  }

  clearCustomSkillIcon(): void {
    this.customSkillIcon = '';
    this.detectedSkillIconDisplay = getSkillIconDisplay(this.newSkillName);
  }

  getSkillDisplay(skill: any): SkillIconDisplay {
    return getSkillIconDisplay(skill);
  }

  getSocialSvg(link: any): string | null {
    return getSocialSvg(link);
  }

  getSocialColor(link: any): string {
    return getSocialColor(link);
  }

  addSkill(): void {
    if (!this.newSkillName.trim()) return;
    if (!this.selectedCategoryId && this.categories.length) {
      this.selectedCategoryId = this.categories[0].id;
    }
    if (!this.selectedCategoryId) {
      this.errorMsg = 'Please add a category first.';
      return;
    }

    const cat = this.categories.find(c => c.id === this.selectedCategoryId);
    const order = cat?.skills?.length ?? 0;
    const name = this.newSkillName.trim();
    const resolvedIcon = this.customSkillIcon || resolveSkillIcon(name);

    if (this.selectedCategoryId.startsWith('temp-')) {
      const newSkill: SkillDto = {
        id: `temp-skill-${Date.now()}`,
        name,
        icon: resolvedIcon,
        displayOrder: order,
        isVisible: true,
        skillCategoryId: this.selectedCategoryId
      };
      registerLearnedSkill(name, resolvedIcon, cat?.name);
      if (cat) {
        cat.skills = [...(cat.skills || []), newSkill];
      }
      this.newSkillName = '';
      this.customSkillIcon = '';
      this.showSkillSuggestions = false;
      this.detectedSkillIconDisplay = { type: 'material', value: 'code' };
      this.toastMsg = `Skill "${name}" added.`;
      setTimeout(() => this.toastMsg = '', 3000);
      return;
    }

    this.api.createSkill({
      name,
      skillCategoryId: this.selectedCategoryId,
      icon: resolvedIcon,
      displayOrder: order
    }).subscribe({
      next: res => {
        if (res.succeeded && res.data) {
          registerLearnedSkill(name, resolvedIcon, cat?.name);
          if (cat) {
            cat.skills = [...(cat.skills || []), res.data];
          }
          this.newSkillName = '';
          this.customSkillIcon = '';
          this.showSkillSuggestions = false;
          this.detectedSkillIconDisplay = { type: 'material', value: 'code' };
          this.toastMsg = `Skill "${res.data.name}" added.`;
          setTimeout(() => this.toastMsg = '', 3000);
        } else {
          this.errorMsg = res.message || 'Could not add skill.';
        }
      },
      error: () => this.errorMsg = 'Could not add skill.'
    });
  }

  addCategory(): void {
    if (!this.newCategoryName.trim()) return;

    if (this.hasPendingCvImport) {
      const newCat: SkillCategoryDto = {
        id: `temp-cat-${Date.now()}`,
        name: this.newCategoryName.trim(),
        displayOrder: this.categories.length,
        isVisible: true,
        skills: []
      };
      this.categories.push(newCat);
      this.selectedCategoryId = newCat.id;
      this.newCategoryName = '';
      this.toastMsg = `Category "${newCat.name}" created.`;
      setTimeout(() => this.toastMsg = '', 3000);
      return;
    }

    this.api.createSkillCategory({
      name: this.newCategoryName.trim(),
      displayOrder: this.categories.length
    }).subscribe({
      next: res => {
        if (res.succeeded && res.data) {
          this.categories.push({ ...res.data, skills: [] });
          this.selectedCategoryId = res.data.id;
          this.newCategoryName = '';
          this.toastMsg = `Category "${res.data.name}" created.`;
          setTimeout(() => this.toastMsg = '', 3000);
        } else {
          this.errorMsg = res.message || 'Could not create category.';
        }
      },
      error: () => this.errorMsg = 'Could not create category.'
    });
  }

  removeSkill(category: SkillCategoryDto, skillId: string): void {
    if (skillId.startsWith('temp-')) {
      category.skills = (category.skills || []).filter(s => s.id !== skillId);
      this.toastMsg = 'تمت إزالة المهارة بنجاح.';
      setTimeout(() => this.toastMsg = '', 2000);
      return;
    }

    this.api.deleteSkill(skillId).subscribe({
      next: () => {
        category.skills = (category.skills || []).filter(s => s.id !== skillId);
        this.toastMsg = 'تمت إزالة المهارة بنجاح.';
        setTimeout(() => this.toastMsg = '', 2000);
      },
      error: () => this.errorMsg = 'تعذر حذف المهارة.'
    });
  }

  renameSkill(category: SkillCategoryDto, skillId: string, currentName: string): void {
    const name = prompt('Edit skill name:', currentName)?.trim();
    if (!name || name === currentName) return;

    if (skillId.startsWith('temp-')) {
      const item = category.skills?.find(s => s.id === skillId);
      if (item) item.name = name;
      return;
    }

    this.api.updateSkill(skillId, {
      name,
      icon: null,
      displayOrder: 0
    }).subscribe({
      next: res => {
        if (res.succeeded && res.data) {
          const item = category.skills?.find(s => s.id === skillId);
          if (item) item.name = res.data.name;
        }
      },
      error: () => this.errorMsg = 'Could not update skill.'
    });
  }

  deleteCategory(category: SkillCategoryDto): void {
    const skillsCount = category.skills?.length || 0;
    this.openDeleteConfirm({
      title: 'حذف قسم المهارات / Delete Category',
      itemName: category.name,
      itemType: 'Category',
      message: `هل أنت متأكد من حذف قسم "${category.name}" وجميع المهارات التابعة له (${skillsCount} مهارة)؟`,
      action: () => {
        if (category.id.startsWith('temp-')) {
          this.categories = this.categories.filter(c => c.id !== category.id);
          if (this.selectedCategoryId === category.id) {
            this.selectedCategoryId = this.categories[0]?.id || '';
          }
          this.closeDeleteConfirm(true);
          this.toastMsg = `تم حذف قسم "${category.name}".`;
          setTimeout(() => this.toastMsg = '', 3000);
          return;
        }

        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteSkillCategory(category.id).subscribe({
          next: () => {
            this.categories = this.categories.filter(c => c.id !== category.id);
            if (this.selectedCategoryId === category.id) {
              this.selectedCategoryId = this.categories[0]?.id || '';
            }
            this.closeDeleteConfirm(true);
            this.toastMsg = `تم حذف قسم "${category.name}".`;
            setTimeout(() => this.toastMsg = '', 3000);
          },
          error: (err: any) => {
            if (err?.status === 404) {
              this.categories = this.categories.filter(c => c.id !== category.id);
              if (this.selectedCategoryId === category.id) {
                this.selectedCategoryId = this.categories[0]?.id || '';
              }
              this.closeDeleteConfirm(true);
              this.toastMsg = `تم حذف قسم "${category.name}".`;
              setTimeout(() => this.toastMsg = '', 3000);
              return;
            }
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = 'تعذر حذف قسم المهارات.';
          }
        });
      }
    });
  }

  // ==========================================
  // 4. Experience CRUD
  // ==========================================
  loadExperiences(): void {
    this.api.getExperiences().subscribe({
      next: res => {
        if (res.succeeded) this.experiences = res.data ?? [];
      }
    });
  }

  openAddExperience(): void {
    this.editingExperience = null;
    this.experienceForm = this.emptyExperience();
    this.experienceModalOpen = true;
  }

  editExperience(exp: ExperienceDto): void {
    this.editingExperience = exp;
    this.experienceForm = {
      jobTitle: exp.jobTitle,
      companyName: exp.companyName,
      description: exp.description || '',
      startDate: exp.startDate ? exp.startDate.slice(0, 10) : '',
      endDate: exp.endDate ? exp.endDate.slice(0, 10) : null,
      isCurrent: exp.isCurrent,
      displayOrder: exp.displayOrder
    };
    this.experienceModalOpen = true;
  }

  saveExperience(): void {
    if (!this.experienceForm.jobTitle.trim() || !this.experienceForm.companyName.trim()) return;

    if (this.editingExperience) {
      if (this.editingExperience.id.startsWith('temp-')) {
        const idx = this.experiences.findIndex(e => e.id === this.editingExperience!.id);
        if (idx > -1) {
          this.experiences[idx] = {
            ...this.experiences[idx],
            jobTitle: this.experienceForm.jobTitle,
            companyName: this.experienceForm.companyName,
            description: this.experienceForm.description,
            startDate: this.experienceForm.startDate,
            endDate: this.experienceForm.isCurrent ? null : this.experienceForm.endDate,
            isCurrent: this.experienceForm.isCurrent
          };
        }
        this.experienceModalOpen = false;
        this.toastMsg = 'تم تعديل الخبرة الوظيفية.';
        setTimeout(() => this.toastMsg = '', 3000);
        return;
      }

      this.api.updateExperience(this.editingExperience.id, this.experienceForm).subscribe({
        next: res => {
          if (res.succeeded && res.data) {
            const idx = this.experiences.findIndex(e => e.id === this.editingExperience!.id);
            if (idx > -1) this.experiences[idx] = res.data;
            this.experienceModalOpen = false;
            this.toastMsg = 'Experience updated.';
            setTimeout(() => this.toastMsg = '', 3000);
          } else {
            this.errorMsg = res.message || 'Could not update experience.';
          }
        },
        error: () => this.errorMsg = 'Could not update experience.'
      });
    } else {
      if (this.hasPendingCvImport) {
        this.experiences.push({
          id: `temp-exp-${Date.now()}`,
          jobTitle: this.experienceForm.jobTitle,
          companyName: this.experienceForm.companyName,
          description: this.experienceForm.description,
          startDate: this.experienceForm.startDate,
          endDate: this.experienceForm.isCurrent ? null : this.experienceForm.endDate,
          isCurrent: this.experienceForm.isCurrent,
          displayOrder: this.experiences.length,
          isVisible: true
        });
        this.experienceModalOpen = false;
        this.toastMsg = 'تمت إضافة الخبرة بنجاح.';
        setTimeout(() => this.toastMsg = '', 3000);
        return;
      }

      this.api.createExperience({
        ...this.experienceForm,
        displayOrder: this.experiences.length
      }).subscribe({
        next: res => {
          if (res.succeeded && res.data) {
            this.experiences.push(res.data);
            this.experienceModalOpen = false;
            this.toastMsg = 'Experience added.';
            setTimeout(() => this.toastMsg = '', 3000);
          } else {
            this.errorMsg = res.message || 'Could not add experience.';
          }
        },
        error: () => this.errorMsg = 'Could not add experience.'
      });
    }
  }

  deleteExperience(exp: ExperienceDto): void {
    this.openDeleteConfirm({
      title: 'حذف خبرة وظيفية / Delete Experience',
      itemName: `${exp.jobTitle} @ ${exp.companyName}`,
      itemType: 'Experience',
      message: `هل أنت متأكد من حذف دور "${exp.jobTitle}" في شركة "${exp.companyName}" من مسارك المهني؟`,
      action: () => {
        if (exp.id.startsWith('temp-')) {
          this.experiences = this.experiences.filter(e => e.id !== exp.id);
          this.closeDeleteConfirm(true);
          this.toastMsg = 'تم حذف الخبرة الوظيفية.';
          setTimeout(() => this.toastMsg = '', 3000);
          return;
        }

        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteExperience(exp.id).subscribe({
          next: () => {
            this.experiences = this.experiences.filter(e => e.id !== exp.id);
            this.closeDeleteConfirm(true);
            this.toastMsg = 'تم حذف الخبرة الوظيفية بنجاح.';
            setTimeout(() => this.toastMsg = '', 3000);
          },
          error: (err: any) => {
            if (err?.status === 404) {
              this.experiences = this.experiences.filter(e => e.id !== exp.id);
              this.closeDeleteConfirm(true);
              this.toastMsg = 'تم حذف الخبرة الوظيفية بنجاح.';
              setTimeout(() => this.toastMsg = '', 3000);
              return;
            }
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = 'تعذر حذف الخبرة الوظيفية. يرجى المحاولة مرة أخرى.';
          }
        });
      }
    });
  }

  private emptyExperience(): CreateExperienceRequest {
    return {
      jobTitle: '',
      companyName: '',
      description: '',
      startDate: new Date().toISOString().slice(0, 10),
      endDate: null,
      isCurrent: true,
      displayOrder: 0
    };
  }

  // ==========================================
  // 5. Projects CRUD
  // ==========================================
  loadProjects(): void {
    this.api.getProjects().subscribe({
      next: res => {
        if (res.succeeded) this.projects = res.data ?? [];
      }
    });
  }

  openAddProject(): void {
    this.editingProject = null;
    this.projectForm = this.emptyProject();
    this.projectTechInput = '';
    this.projectModalOpen = true;
  }

  editProject(proj: ProjectDto): void {
    this.editingProject = proj;
    this.projectForm = {
      title: proj.title,
      description: proj.description || '',
      technologies: [...(proj.technologies || [])],
      liveUrl: proj.liveUrl,
      githubUrl: proj.githubUrl,
      image: proj.image,
      isFeatured: proj.isFeatured,
      displayOrder: proj.displayOrder
    };
    this.projectTechInput = (proj.technologies || []).join(', ');
    this.projectModalOpen = true;
  }

  saveProject(): void {
    if (!this.projectForm.title.trim()) return;

    const techs = this.projectTechInput.split(',').map(t => t.trim()).filter(Boolean);
    const req: CreateProjectRequest = {
      ...this.projectForm,
      technologies: techs
    };

    if (this.editingProject) {
      if (this.editingProject.id.startsWith('temp-')) {
        const idx = this.projects.findIndex(p => p.id === this.editingProject!.id);
        if (idx > -1) {
          this.projects[idx] = {
            ...this.projects[idx],
            title: this.projectForm.title,
            description: this.projectForm.description || '',
            technologies: techs,
            liveUrl: this.projectForm.liveUrl,
            githubUrl: this.projectForm.githubUrl,
            image: this.projectForm.image,
            isFeatured: this.projectForm.isFeatured
          };
        }
        this.projectModalOpen = false;
        this.toastMsg = 'تم تعديل المشروع بنجاح.';
        setTimeout(() => this.toastMsg = '', 3000);
        return;
      }

      this.api.updateProject(this.editingProject.id, {
        ...req,
        isVisible: this.editingProject.isVisible
      }).subscribe({
        next: res => {
          if (res.succeeded && res.data) {
            const idx = this.projects.findIndex(p => p.id === this.editingProject!.id);
            if (idx > -1) this.projects[idx] = res.data;
            this.projectModalOpen = false;
            this.toastMsg = 'Project updated.';
            setTimeout(() => this.toastMsg = '', 3000);
          } else {
            this.errorMsg = res.message || 'Could not update project.';
          }
        },
        error: () => this.errorMsg = 'Could not update project.'
      });
    } else {
      if (this.hasPendingCvImport) {
        this.projects.push({
          id: `temp-proj-${Date.now()}`,
          title: this.projectForm.title,
          description: this.projectForm.description || '',
          technologies: techs,
          liveUrl: this.projectForm.liveUrl,
          githubUrl: this.projectForm.githubUrl,
          image: this.projectForm.image,
          isFeatured: this.projectForm.isFeatured,
          isVisible: true,
          displayOrder: this.projects.length
        });
        this.projectModalOpen = false;
        this.toastMsg = 'تمت إضافة المشروع بنجاح.';
        setTimeout(() => this.toastMsg = '', 3000);
        return;
      }

      this.api.createProject({
        ...req,
        displayOrder: this.projects.length
      }).subscribe({
        next: res => {
          if (res.succeeded && res.data) {
            this.projects.push(res.data);
            this.projectModalOpen = false;
            this.toastMsg = 'Project added.';
            setTimeout(() => this.toastMsg = '', 3000);
          } else {
            this.errorMsg = res.message || 'Could not add project.';
          }
        },
        error: () => this.errorMsg = 'Could not add project.'
      });
    }
  }

  deleteProject(proj: ProjectDto): void {
    this.openDeleteConfirm({
      title: 'حذف المشروع / Delete Project',
      itemName: proj.title,
      itemType: 'Project',
      message: `هل أنت متأكد من حذف المشروع "${proj.title}" نهائياً؟ سيتم حذفه من قاعدة البيانات ومعرض أعمالك فوراً.`,
      action: () => {
        if (proj.id.startsWith('temp-')) {
          this.projects = this.projects.filter(p => p.id !== proj.id);
          this.closeDeleteConfirm(true);
          this.toastMsg = `تم حذف المشروع "${proj.title}" بنجاح.`;
          setTimeout(() => this.toastMsg = '', 3000);
          return;
        }

        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteProject(proj.id).subscribe({
          next: () => {
            this.projects = this.projects.filter(p => p.id !== proj.id);
            this.closeDeleteConfirm(true);
            this.toastMsg = `تم حذف المشروع "${proj.title}" بنجاح.`;
            setTimeout(() => this.toastMsg = '', 3000);
          },
          error: (err: any) => {
            if (err?.status === 404) {
              this.projects = this.projects.filter(p => p.id !== proj.id);
              this.closeDeleteConfirm(true);
              this.toastMsg = `تم حذف المشروع "${proj.title}" بنجاح.`;
              setTimeout(() => this.toastMsg = '', 3000);
              return;
            }
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = 'تعذر حذف المشروع. يرجى المحاولة مرة أخرى.';
          }
        });
      }
    });
  }

  private emptyProject(): CreateProjectRequest {
    return {
      title: '',
      description: '',
      technologies: [],
      liveUrl: '',
      githubUrl: '',
      image: null,
      isFeatured: false,
      displayOrder: 0
    };
  }

  // ==========================================
  // 6. Credentials (Education & Certifications) CRUD
  // ==========================================
  loadCredentials(): void {
    this.api.getEducations().subscribe({
      next: res => { if (res.succeeded) this.educations = res.data ?? []; }
    });
    this.api.getCertifications().subscribe({
      next: res => { if (res.succeeded) this.certifications = res.data ?? []; }
    });
  }

  openAddCredential(): void {
    this.editingEducation = null;
    this.editingCertification = null;
    this.educationForm = this.emptyEducation();
    this.certificationForm = this.emptyCertification();
    this.credentialModalOpen = true;
  }

  editEducation(edu: EducationDto): void {
    this.credentialType = 'education';
    this.editingEducation = edu;
    this.educationForm = {
      degree: edu.degree,
      institutionName: edu.institutionName,
      fieldOfStudy: edu.fieldOfStudy,
      startDate: edu.startDate ? edu.startDate.slice(0, 10) : '',
      endDate: edu.endDate ? edu.endDate.slice(0, 10) : null,
      description: edu.description,
      isCurrent: edu.isCurrent,
      displayOrder: edu.displayOrder
    };
    this.credentialModalOpen = true;
  }

  saveEducation(): void {
    if (!this.educationForm.degree.trim() || !this.educationForm.institutionName.trim()) return;

    if (this.editingEducation) {
      if (this.editingEducation.id.startsWith('temp-')) {
        const idx = this.educations.findIndex(e => e.id === this.editingEducation!.id);
        if (idx > -1) {
          this.educations[idx] = {
            ...this.educations[idx],
            degree: this.educationForm.degree,
            institutionName: this.educationForm.institutionName,
            fieldOfStudy: this.educationForm.fieldOfStudy || '',
            description: this.educationForm.description || '',
            startDate: this.educationForm.startDate,
            endDate: this.educationForm.isCurrent ? null : this.educationForm.endDate,
            isCurrent: this.educationForm.isCurrent
          };
        }
        this.credentialModalOpen = false;
        this.toastMsg = 'تم تعديل المؤهل التعليمي بنجاح.';
        setTimeout(() => this.toastMsg = '', 3000);
        return;
      }

      this.api.updateEducation(this.editingEducation.id, this.educationForm).subscribe({
        next: res => {
          if (res.succeeded && res.data) {
            const idx = this.educations.findIndex(e => e.id === this.editingEducation!.id);
            if (idx > -1) this.educations[idx] = res.data;
            this.credentialModalOpen = false;
            this.toastMsg = 'Education updated.';
            setTimeout(() => this.toastMsg = '', 3000);
          }
        },
        error: () => this.errorMsg = 'Could not update education.'
      });
    } else {
      if (this.hasPendingCvImport) {
        this.educations.push({
          id: `temp-edu-${Date.now()}`,
          degree: this.educationForm.degree,
          institutionName: this.educationForm.institutionName,
          fieldOfStudy: this.educationForm.fieldOfStudy || '',
          description: this.educationForm.description || '',
          startDate: this.educationForm.startDate,
          endDate: this.educationForm.isCurrent ? null : this.educationForm.endDate,
          isCurrent: this.educationForm.isCurrent,
          displayOrder: this.educations.length,
          isVisible: true
        });
        this.credentialModalOpen = false;
        this.toastMsg = 'تمت إضافة المؤهل التعليمي بنجاح.';
        setTimeout(() => this.toastMsg = '', 3000);
        return;
      }

      this.api.createEducation({
        ...this.educationForm,
        displayOrder: this.educations.length
      }).subscribe({
        next: res => {
          if (res.succeeded && res.data) {
            this.educations.push(res.data);
            this.credentialModalOpen = false;
            this.toastMsg = 'Education added.';
            setTimeout(() => this.toastMsg = '', 3000);
          }
        },
        error: () => this.errorMsg = 'Could not add education.'
      });
    }
  }

  deleteEducation(edu: EducationDto): void {
    this.openDeleteConfirm({
      title: 'حذف المؤهل التعليمي / Delete Education',
      itemName: `${edu.degree} (${edu.institutionName})`,
      itemType: 'Education',
      message: `هل أنت متأكد من حذف مؤهل "${edu.degree}" في "${edu.institutionName}" من بياناتك؟`,
      action: () => {
        if (edu.id.startsWith('temp-')) {
          this.educations = this.educations.filter(e => e.id !== edu.id);
          this.closeDeleteConfirm(true);
          this.toastMsg = 'تم حذف المؤهل التعليمي بنجاح.';
          setTimeout(() => this.toastMsg = '', 3000);
          return;
        }

        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteEducation(edu.id).subscribe({
          next: () => {
            this.educations = this.educations.filter(e => e.id !== edu.id);
            this.closeDeleteConfirm(true);
            this.toastMsg = 'تم حذف المؤهل التعليمي بنجاح.';
            setTimeout(() => this.toastMsg = '', 3000);
          },
          error: (err: any) => {
            if (err?.status === 404) {
              this.educations = this.educations.filter(e => e.id !== edu.id);
              this.closeDeleteConfirm(true);
              this.toastMsg = 'تم حذف المؤهل التعليمي بنجاح.';
              setTimeout(() => this.toastMsg = '', 3000);
              return;
            }
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = 'تعذر حذف المؤهل التعليمي. يرجى المحاولة مرة أخرى.';
          }
        });
      }
    });
  }

  editCertification(cert: CertificationDto): void {
    this.credentialType = 'certification';
    this.editingCertification = cert;
    this.certificationForm = {
      name: cert.name,
      issuingOrganization: cert.issuingOrganization,
      issueDate: cert.issueDate ? cert.issueDate.slice(0, 10) : '',
      credentialId: cert.credentialId,
      credentialUrl: cert.credentialUrl,
      description: cert.description,
      displayOrder: cert.displayOrder
    };
    this.credentialModalOpen = true;
  }

  saveCertification(): void {
    if (!this.certificationForm.name.trim() || !this.certificationForm.issuingOrganization.trim()) return;

    if (this.editingCertification) {
      if (this.editingCertification.id.startsWith('temp-')) {
        const idx = this.certifications.findIndex(c => c.id === this.editingCertification!.id);
        if (idx > -1) {
          this.certifications[idx] = {
            ...this.certifications[idx],
            name: this.certificationForm.name,
            issuingOrganization: this.certificationForm.issuingOrganization,
            issueDate: this.certificationForm.issueDate,
            credentialId: this.certificationForm.credentialId || '',
            credentialUrl: this.certificationForm.credentialUrl || '',
            description: this.certificationForm.description || ''
          };
        }
        this.credentialModalOpen = false;
        this.toastMsg = 'تم تعديل الشهادة المهنية بنجاح.';
        setTimeout(() => this.toastMsg = '', 3000);
        return;
      }

      this.api.updateCertification(this.editingCertification.id, this.certificationForm).subscribe({
        next: res => {
          if (res.succeeded && res.data) {
            const idx = this.certifications.findIndex(c => c.id === this.editingCertification!.id);
            if (idx > -1) this.certifications[idx] = res.data;
            this.credentialModalOpen = false;
            this.toastMsg = 'Certification updated.';
            setTimeout(() => this.toastMsg = '', 3000);
          }
        },
        error: () => this.errorMsg = 'Could not update certification.'
      });
    } else {
      if (this.hasPendingCvImport) {
        this.certifications.push({
          id: `temp-cert-${Date.now()}`,
          name: this.certificationForm.name,
          issuingOrganization: this.certificationForm.issuingOrganization,
          issueDate: this.certificationForm.issueDate,
          credentialId: this.certificationForm.credentialId || '',
          credentialUrl: this.certificationForm.credentialUrl || '',
          description: this.certificationForm.description || '',
          displayOrder: this.certifications.length,
          isVisible: true
        });
        this.credentialModalOpen = false;
        this.toastMsg = 'تمت إضافة الشهادة المهنية بنجاح.';
        setTimeout(() => this.toastMsg = '', 3000);
        return;
      }

      this.api.createCertification({
        ...this.certificationForm,
        displayOrder: this.certifications.length
      }).subscribe({
        next: res => {
          if (res.succeeded && res.data) {
            this.certifications.push(res.data);
            this.credentialModalOpen = false;
            this.toastMsg = 'Certification added.';
            setTimeout(() => this.toastMsg = '', 3000);
          }
        },
        error: () => this.errorMsg = 'Could not add certification.'
      });
    }
  }

  deleteCertification(cert: CertificationDto): void {
    this.openDeleteConfirm({
      title: 'حذف الشهادة المهنية / Delete Certification',
      itemName: `${cert.name} (${cert.issuingOrganization})`,
      itemType: 'Certification',
      message: `هل أنت متأكد من حذف شهادة "${cert.name}" الصادرة من "${cert.issuingOrganization}"؟`,
      action: () => {
        if (cert.id.startsWith('temp-')) {
          this.certifications = this.certifications.filter(c => c.id !== cert.id);
          this.closeDeleteConfirm(true);
          this.toastMsg = 'تم حذف الشهادة المهنية بنجاح.';
          setTimeout(() => this.toastMsg = '', 3000);
          return;
        }

        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteCertification(cert.id).subscribe({
          next: () => {
            this.certifications = this.certifications.filter(c => c.id !== cert.id);
            this.closeDeleteConfirm(true);
            this.toastMsg = 'تم حذف الشهادة المهنية بنجاح.';
            setTimeout(() => this.toastMsg = '', 3000);
          },
          error: (err: any) => {
            if (err?.status === 404) {
              this.certifications = this.certifications.filter(c => c.id !== cert.id);
              this.closeDeleteConfirm(true);
              this.toastMsg = 'تم حذف الشهادة المهنية بنجاح.';
              setTimeout(() => this.toastMsg = '', 3000);
              return;
            }
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = 'تعذر حذف الشهادة. يرجى المحاولة مرة أخرى.';
          }
        });
      }
    });
  }

  private emptyEducation(): CreateEducationRequest {
    return {
      degree: '',
      institutionName: '',
      fieldOfStudy: '',
      startDate: new Date().toISOString().slice(0, 10),
      endDate: null,
      description: '',
      isCurrent: false,
      displayOrder: 0
    };
  }

  private emptyCertification(): CreateCertificationRequest {
    return {
      name: '',
      issuingOrganization: '',
      issueDate: new Date().toISOString().slice(0, 10),
      credentialId: null,
      credentialUrl: null,
      description: null,
      displayOrder: 0
    };
  }

  // ==========================================
  // 7. Social Links CRUD
  // ==========================================
  loadSocialLinks(): void {
    this.api.getSocialLinks().subscribe({
      next: response => { if (response.succeeded) this.socialLinks = response.data ?? []; },
      error: () => this.errorMsg = 'Social links could not be loaded.'
    });
  }

  editSocialLink(link: SocialLinkDto): void {
    this.editingSocialId = link.id;
    const resolved = resolveSocialPlatform(link.platform, link.icon);
    this.socialForm = {
      platform: link.platform,
      url: link.url,
      icon: link.icon || resolved.materialIcon
    };
    this.selectedPresetPlatform = this.presetPlatforms.some(p => p.id === resolved.id) ? resolved.id : '';
  }

  saveSocialLink(): void {
    if (!this.socialForm.platform.trim() || !this.socialForm.url.trim()) return;

    const icon = this.socialForm.icon || getSocialIconName(this.socialForm.platform, this.socialForm.url);
    if (this.editingSocialId) {
      if (this.editingSocialId.startsWith('temp-')) {
        const link = this.socialLinks.find(l => l.id === this.editingSocialId);
        if (link) {
          link.platform = this.socialForm.platform.trim();
          link.url = this.socialForm.url.trim();
          link.icon = icon;
        }
        this.editingSocialId = null;
        this.socialForm = { platform: '', url: '', icon: '' };
        this.selectedPresetPlatform = '';
        this.toastMsg = 'تم تعديل الرابط بنجاح.';
        setTimeout(() => this.toastMsg = '', 3000);
        return;
      }
    } else {
      if (this.hasPendingCvImport) {
        this.socialLinks.push({
          id: `temp-social-${Date.now()}`,
          platform: this.socialForm.platform.trim(),
          url: this.socialForm.url.trim(),
          icon: icon,
          displayOrder: this.socialLinks.length,
          isVisible: true
        });
        this.socialForm = { platform: '', url: '', icon: '' };
        this.selectedPresetPlatform = '';
        this.toastMsg = 'تمت إضافة الرابط بنجاح.';
        setTimeout(() => this.toastMsg = '', 3000);
        return;
      }
    }

    const request = {
      platform: this.socialForm.platform.trim(),
      url: this.socialForm.url.trim(),
      icon: icon,
      displayOrder: this.editingSocialId
        ? this.socialLinks.find(link => link.id === this.editingSocialId)?.displayOrder ?? this.socialLinks.length
        : this.socialLinks.length
    };
    const operation = this.editingSocialId
      ? this.api.updateSocialLink(this.editingSocialId, request)
      : this.api.createSocialLink(request);
    operation.subscribe({
      next: response => {
        if (!response.succeeded) {
          this.errorMsg = formatFriendlyError(response.message, 'تعذر حفظ الرابط.');
          return;
        }
        this.socialLinks = this.editingSocialId
          ? this.socialLinks.map(link => link.id === response.data.id ? response.data : link)
          : [...this.socialLinks, response.data];
        this.editingSocialId = null;
        this.socialForm = { platform: '', url: '', icon: '' };
        this.selectedPresetPlatform = '';
        this.toastMsg = 'تم حفظ الرابط بنجاح.';
        setTimeout(() => this.toastMsg = '', 3000);
      },
      error: err => this.errorMsg = formatFriendlyError(err, 'تعذر حفظ الرابط الاجتماعي.')
    });
  }

  toggleSocialLink(link: SocialLinkDto): void {
    if (link.id.startsWith('temp-')) {
      link.isVisible = !link.isVisible;
      return;
    }

    this.api.setSocialLinkVisibility(link.id, !link.isVisible).subscribe({
      next: response => response.succeeded ? link.isVisible = response.data.isVisible : this.errorMsg = sanitizeTechnicalText(response.message, 'تعذر تعديل ظهور الرابط.'),
      error: err => this.errorMsg = formatFriendlyError(err, 'فشل تعديل حالة ظهور الرابط.')
    });
  }

  deleteSocialLink(link: SocialLinkDto): void {
    this.openDeleteConfirm({
      title: 'حذف الرابط الاجتماعي / Delete Social Link',
      itemName: link.platform,
      itemType: 'Social Link',
      message: `هل أنت متأكد من حذف رابط "${link.platform}" (${link.url})؟`,
      action: () => {
        if (link.id.startsWith('temp-')) {
          this.socialLinks = this.socialLinks.filter(item => item.id !== link.id);
          this.closeDeleteConfirm(true);
          this.toastMsg = 'تم حذف الرابط بنجاح.';
          setTimeout(() => this.toastMsg = '', 3000);
          return;
        }

        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteSocialLink(link.id).subscribe({
          next: () => {
            this.socialLinks = this.socialLinks.filter(item => item.id !== link.id);
            this.closeDeleteConfirm(true);
            this.toastMsg = 'تم حذف الرابط بنجاح.';
            setTimeout(() => this.toastMsg = '', 3000);
          },
          error: (err: any) => {
            if (err?.status === 404) {
              this.socialLinks = this.socialLinks.filter(item => item.id !== link.id);
              this.closeDeleteConfirm(true);
              this.toastMsg = 'تم حذف الرابط بنجاح.';
              setTimeout(() => this.toastMsg = '', 3000);
              return;
            }
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = formatFriendlyError(err, 'تعذر حذف الرابط الاجتماعي.');
          }
        });
      }
    });
  }

  moveSocialLink(link: SocialLinkDto, offset: number): void {
    const index = this.socialLinks.findIndex(item => item.id === link.id);
    const target = index + offset;
    if (target < 0 || target >= this.socialLinks.length) return;
    const reordered = [...this.socialLinks];
    [reordered[index], reordered[target]] = [reordered[target], reordered[index]];

    if (this.hasPendingCvImport || reordered.some(l => l.id.startsWith('temp-'))) {
      this.socialLinks = reordered.map((item, displayOrder) => ({ ...item, displayOrder }));
      return;
    }

    this.api.reorderSocialLinks(reordered.map((item, displayOrder) => ({ id: item.id, displayOrder }))).subscribe({
      next: response => response.succeeded ? this.socialLinks = reordered.map((item, displayOrder) => ({ ...item, displayOrder })) : this.errorMsg = response.message,
      error: () => this.errorMsg = 'Social link order could not be saved.'
    });
  }

  // ==========================================
  // Image Upload Handlers (Avatar & Projects)
  // ==========================================
  uploadAvatar(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) this.handleAvatarFile(file);
  }

  onAvatarDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingAvatar = true;
  }

  onAvatarDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingAvatar = false;
  }

  onAvatarDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingAvatar = false;
    const file = event.dataTransfer?.files?.[0];
    if (file) this.handleAvatarFile(file);
  }

  resolveMediaUrl(url: string | null | undefined): string {
    if (!url) return '';
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

  private handleAvatarFile(file: File): void {
    if (!file.type.startsWith('image/')) {
      this.errorMsg = 'Only image files (JPG, PNG, WebP, SVG, GIF) can be uploaded for profile avatar.';
      return;
    }

    // Instant local preview for immediate visual feedback
    const localPreviewUrl = URL.createObjectURL(file);
    this.formData.avatarUrl = localPreviewUrl;

    this.isUploadingAvatar = true;
    this.errorMsg = '';
    this.api.uploadImage(file).subscribe({
      next: res => {
        this.isUploadingAvatar = false;
        if (res.succeeded) {
          const uploadedUrl = res.relativeUrl || res.url;
          this.formData.avatarUrl = uploadedUrl;
          this.toastMsg = 'Profile avatar uploaded successfully. Saving profile...';

          // Auto-save avatar to profile immediately
          this.api.updateProfile({
            fullName: this.formData.name,
            title: this.formData.title,
            location: this.formData.location,
            email: this.formData.email,
            phone: this.formData.phone,
            summary: this.formData.bio,
            avatarUrl: uploadedUrl,
            cvUrl: this.formData.cvUrl,
            availableForHire: this.formData.availableForHire
          }).subscribe({
            next: () => {
              this.toastMsg = 'Profile avatar updated and saved successfully!';
              setTimeout(() => this.toastMsg = '', 3500);
            },
            error: () => {
              this.toastMsg = 'Avatar uploaded. Click "Save & Deploy" to commit all changes.';
              setTimeout(() => this.toastMsg = '', 4000);
            }
          });
        } else {
          this.errorMsg = 'Failed to upload avatar image.';
        }
      },
      error: err => {
        this.isUploadingAvatar = false;
        this.errorMsg = err?.error?.message || 'Failed to upload avatar image.';
      }
    });
  }

  uploadProjectImage(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) this.handleProjectImageFile(file);
  }

  onProjectImageDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingProjectImage = true;
  }

  onProjectImageDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingProjectImage = false;
  }

  onProjectImageDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingProjectImage = false;
    const file = event.dataTransfer?.files?.[0];
    if (file) this.handleProjectImageFile(file);
  }

  private handleProjectImageFile(file: File): void {
    if (!file.type.startsWith('image/')) {
      this.errorMsg = 'Only image files (JPG, PNG, WebP, SVG, GIF) can be uploaded for projects.';
      return;
    }
    this.isUploadingProjectImage = true;
    this.errorMsg = '';
    this.api.uploadImage(file).subscribe({
      next: res => {
        this.isUploadingProjectImage = false;
        if (res.succeeded) {
          this.projectForm.image = res.relativeUrl || res.url;
          this.toastMsg = 'Project image uploaded successfully.';
          setTimeout(() => this.toastMsg = '', 3000);
        } else {
          this.errorMsg = 'Failed to upload project image.';
        }
      },
      error: err => {
        this.isUploadingProjectImage = false;
        this.errorMsg = err?.error?.message || 'Failed to upload project image.';
      }
    });
  }

  removeProjectImage(): void {
    this.projectForm.image = null;
  }
}
