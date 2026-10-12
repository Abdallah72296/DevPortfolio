import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { DashboardHeaderComponent } from '../../shared/components/dashboard-header/dashboard-header.component';
import { DashboardSidebarComponent } from '../../shared/components/dashboard-sidebar/dashboard-sidebar.component';
import { AdminApiService } from '../../core/services/admin-api.service';
import { CertificationDto, CreateCertificationRequest, CreateEducationRequest, CreateExperienceRequest, EducationDto, ExperienceDto, SkillCategoryDto } from '../../core/models/admin.models';
import { formatFriendlyError, sanitizeTechnicalText } from '../../core/helpers/error-formatter';
import { 
  searchSkillCatalog, 
  resolveSkillIcon, 
  registerLearnedSkill, 
  getSkillIconDisplay, 
  CatalogSkill, 
  SkillIconDisplay 
} from '../../core/helpers/skill-registry.helper';

@Component({
  selector: 'app-skills-experience-manager',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, DashboardHeaderComponent, DashboardSidebarComponent],
  templateUrl: './skills-experience-manager.component.html',
  styleUrl: './skills-experience-manager.component.css'
})
export class SkillsExperienceManagerComponent implements OnInit {
  private readonly api = inject(AdminApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  activeTab: 'experience' | 'skills' | 'education' = 'skills';
  categories: SkillCategoryDto[] = [];
  experiences: ExperienceDto[] = [];
  education: EducationDto[] = [];
  certifications: CertificationDto[] = [];
  loading = false;
  saving = false;
  error = '';
  notice = '';
  newSkillName = '';
  skillSuggestions: CatalogSkill[] = [];
  showSkillSuggestions = false;
  customSkillIcon = '';
  isUploadingSkillIcon = false;
  detectedSkillIconDisplay: SkillIconDisplay = { type: 'material', value: 'code' };
  newCategoryName = '';
  selectedCategoryId = '';
  experienceDrawerOpen = false;
  experienceDrawerError = '';
  experienceFormTouched = false;
  taxonomyConfigOpen = false;
  inlineSkillInput: { [categoryId: string]: string } = {};
  inlineSkillActive: { [categoryId: string]: boolean } = {};
  editingExperience: ExperienceDto | null = null;
  experienceForm: CreateExperienceRequest = this.emptyExperience();
  editingEducation: EducationDto | null = null;
  educationForm: CreateEducationRequest = this.emptyEducation();
  editingCertification: CertificationDto | null = null;
  certificationForm: CreateCertificationRequest = this.emptyCertification();

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

  closeDeleteConfirm(): void {
    if (this.deleteModal.isDeleting) return;
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

  get totalSkillsCount(): number {
    return this.categories.reduce((acc, cat) => acc + (cat.skills?.length || 0), 0);
  }

  getClusterColor(index: number): string {
    const colors = ['secondary', 'primary', 'tertiary', 'secondary-fixed'];
    return colors[index % colors.length];
  }

  getClusterBadgeClass(index: number): string {
    const color = this.getClusterColor(index);
    if (color === 'secondary') return 'text-secondary bg-secondary/10';
    if (color === 'primary') return 'text-primary bg-primary/10';
    if (color === 'tertiary') return 'text-tertiary bg-tertiary/10';
    return 'text-secondary-fixed bg-secondary-fixed/10';
  }

  getClusterDotClass(index: number): string {
    const color = this.getClusterColor(index);
    if (color === 'secondary') return 'bg-secondary';
    if (color === 'primary') return 'bg-primary';
    if (color === 'tertiary') return 'bg-tertiary';
    return 'bg-secondary-fixed';
  }

  openExperienceDrawer(item?: ExperienceDto): void {
    this.experienceDrawerError = '';
    this.experienceFormTouched = false;
    if (item) {
      this.editExperience(item);
    } else {
      this.newExperience();
    }
    this.experienceDrawerOpen = true;
  }

  closeExperienceDrawer(): void {
    this.experienceDrawerOpen = false;
    this.editingExperience = null;
    this.experienceDrawerError = '';
    this.experienceFormTouched = false;
  }

  toggleInlineSkill(categoryId: string): void {
    this.inlineSkillActive[categoryId] = !this.inlineSkillActive[categoryId];
  }

  addInlineSkill(category: SkillCategoryDto): void {
    const name = (this.inlineSkillInput[category.id] || '').trim();
    if (!name) return;
    this.error = '';
    const resolvedIcon = resolveSkillIcon(name);
    this.api.createSkill({ name, icon: resolvedIcon, displayOrder: category.skills?.length ?? 0, skillCategoryId: category.id }).subscribe({
      next: response => {
        if (!response.succeeded) {
          this.error = sanitizeTechnicalText(response.message, 'تعذر إضافة المهارة.');
          return;
        }
        registerLearnedSkill(name, resolvedIcon, category.name);
        this.inlineSkillInput[category.id] = '';
        this.inlineSkillActive[category.id] = false;
        category.skills = [...(category.skills ?? []), response.data];
        this.notice = `تمت إضافة مهارة "${name}" بنجاح.`;
        setTimeout(() => this.notice = '', 3000);
      },
      error: err => this.error = formatFriendlyError(err, 'تعذر إضافة المهارة حالياً.')
    });
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      const tab = params.get('tab');
      if (tab === 'skills' || tab === 'experience' || tab === 'education') this.activeTab = tab;
    });
    this.reload();
  }

  selectTab(tab: 'skills' | 'experience' | 'education'): void {
    this.activeTab = tab;
    this.router.navigate([], { relativeTo: this.route, queryParams: { tab }, queryParamsHandling: 'merge' });
  }

  reload(): void {
    this.loading = true;
    this.error = '';
    this.api.getSkillCategories().subscribe({
      next: response => {
        if (response.succeeded) {
          this.categories = response.data ?? [];
          this.selectedCategoryId ||= this.categories[0]?.id ?? '';
        } else {
          this.error = sanitizeTechnicalText(response.message, 'تعذر تحميل تصنيفات المهارات.');
        }
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.error = formatFriendlyError(err, 'تعذر تحميل المهارات. يرجى التأكد من تشغيل الخادم وتسجيل الدخول.');
      }
    });
    this.api.getExperiences().subscribe({
      next: response => { if (response.succeeded) this.experiences = response.data ?? []; },
      error: err => this.error ||= formatFriendlyError(err, 'تعذر تحميل سجلات الخبرة.')
    });
    this.api.getEducations().subscribe({
      next: response => { if (response.succeeded) this.education = response.data ?? []; },
      error: err => this.error ||= formatFriendlyError(err, 'تعذر تحميل السجلات التعليمية.')
    });
    this.api.getCertifications().subscribe({
      next: response => { if (response.succeeded) this.certifications = response.data ?? []; },
      error: err => this.error ||= formatFriendlyError(err, 'تعذر تحميل الشهادات المهنية.')
    });
  }

  addCategory(): void {
    const name = this.newCategoryName.trim();
    if (!name) {
      this.error = 'يرجى كتابة اسم التصنيف أو القسم أولاً (حقل إجباري).';
      return;
    }
    this.error = '';
    this.api.createSkillCategory({ name, displayOrder: this.categories.length }).subscribe({
      next: response => {
        if (!response.succeeded) {
          this.error = sanitizeTechnicalText(response.message, 'تعذر إضافة التصنيف، يرجى المحاولة ثانية.');
          return;
        }
        this.newCategoryName = '';
        this.selectedCategoryId = response.data.id;
        this.categories = [...this.categories, response.data];
        this.taxonomyConfigOpen = false;
        this.notice = `تمت إضافة تصنيف "${name}" بنجاح.`;
        setTimeout(() => this.notice = '', 3500);
      },
      error: err => {
        this.error = formatFriendlyError(err, 'تعذر إضافة التصنيف، يرجى مراجعة البيانات والمحاولة مجدداً.');
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
          this.notice = 'تم رفع صورة أيقونة المهارة بنجاح.';
          setTimeout(() => this.notice = '', 3000);
        }
      },
      error: () => {
        this.isUploadingSkillIcon = false;
        this.error = 'تعذر رفع صورة الأيقونة.';
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

  addSkill(): void {
    const name = this.newSkillName.trim();
    if (!name) {
      this.error = 'يرجى كتابة اسم المهارة أولاً (حقل إجباري).';
      return;
    }
    const category = this.categories.find(item => item.id === this.selectedCategoryId);
    if (!category) {
      this.error = 'يرجى اختيار القسم أو التصنيف المطلوب لإضافة المهارة إليه أولاً.';
      return;
    }
    this.error = '';
    const resolvedIcon = this.customSkillIcon || resolveSkillIcon(name);
    this.api.createSkill({ 
      name, 
      icon: resolvedIcon, 
      displayOrder: category.skills?.length ?? 0, 
      skillCategoryId: category.id 
    }).subscribe({
      next: response => {
        if (!response.succeeded) {
          this.error = sanitizeTechnicalText(response.message, 'تعذر إضافة المهارة، يرجى التأكد من البيانات.');
          return;
        }
        registerLearnedSkill(name, resolvedIcon, category.name);
        this.newSkillName = '';
        this.customSkillIcon = '';
        this.showSkillSuggestions = false;
        this.detectedSkillIconDisplay = { type: 'material', value: 'code' };
        category.skills = [...(category.skills ?? []), response.data];
        this.notice = `تمت إضافة مهارة "${name}" إلى ${category.name} بنجاح.`;
        setTimeout(() => this.notice = '', 3500);
      },
      error: err => {
        this.error = formatFriendlyError(err, 'تعذر إضافة المهارة، يرجى المحاولة لاحقاً.');
      }
    });
  }

  renameSkill(category: SkillCategoryDto, skillId: string, currentName: string): void {
    const name = prompt('Skill name', currentName)?.trim();
    if (!name || name === currentName) return;
    const current = category.skills?.find(item => item.id === skillId);
    this.api.updateSkill(skillId, { name, icon: current?.icon ?? null, displayOrder: current?.displayOrder ?? 0 }).subscribe({
      next: response => {
        if (response.succeeded) {
          const skill = category.skills?.find(item => item.id === skillId);
          if (skill) skill.name = response.data.name;
        } else this.error = response.message;
      }, error: () => this.error = 'Skill could not be updated.'
    });
  }

  removeSkill(category: SkillCategoryDto, skillId: string): void {
    this.api.deleteSkill(skillId).subscribe({
      next: () => category.skills = (category.skills ?? []).filter(item => item.id !== skillId),
      error: () => this.error = 'Skill could not be deleted.'
    });
  }

  toggleSkill(skill: NonNullable<SkillCategoryDto['skills']>[number]): void {
    this.api.setSkillVisibility(skill.id, !skill.isVisible).subscribe({
      next: response => response.succeeded ? skill.isVisible = response.data.isVisible : this.error = response.message,
      error: () => this.error = 'Skill visibility could not be changed.'
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
        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteSkillCategory(category.id).subscribe({
          next: () => {
            this.categories = this.categories.filter(item => item.id !== category.id);
            if (this.selectedCategoryId === category.id) {
              this.selectedCategoryId = this.categories[0]?.id ?? '';
            }
            this.closeDeleteConfirm();
            this.notice = `تم حذف قسم "${category.name}" بنجاح.`;
            setTimeout(() => this.notice = '', 3000);
          },
          error: () => {
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = 'تعذر حذف قسم المهارات. يرجى المحاولة مرة أخرى.';
          }
        });
      }
    });
  }

  toggleCategory(category: SkillCategoryDto): void {
    this.api.setSkillCategoryVisibility(category.id, !category.isVisible).subscribe({
      next: response => response.succeeded ? category.isVisible = response.data.isVisible : this.error = response.message,
      error: () => this.error = 'Category visibility could not be changed.'
    });
  }

  moveCategory(category: SkillCategoryDto, offset: number): void {
    const index = this.categories.findIndex(item => item.id === category.id);
    const target = index + offset;
    if (index < 0 || target < 0 || target >= this.categories.length) return;
    const reordered = [...this.categories];
    [reordered[index], reordered[target]] = [reordered[target], reordered[index]];
    this.api.reorderSkillCategories(reordered.map((item, displayOrder) => ({ id: item.id, displayOrder }))).subscribe({
      next: result => result.succeeded ? this.categories = reordered.map((item, displayOrder) => ({ ...item, displayOrder })) : this.error = result.message,
      error: () => this.error = 'Category order could not be saved.'
    });
  }

  moveSkill(category: SkillCategoryDto, skillId: string, offset: number): void {
    const skills = [...(category.skills ?? [])];
    const index = skills.findIndex(item => item.id === skillId);
    const target = index + offset;
    if (index < 0 || target < 0 || target >= skills.length) return;
    [skills[index], skills[target]] = [skills[target], skills[index]];
    this.api.reorderSkills(category.id, skills.map((item, displayOrder) => ({ id: item.id, displayOrder }))).subscribe({
      next: response => response.succeeded ? category.skills = skills.map((item, displayOrder) => ({ ...item, displayOrder })) : this.error = response.message,
      error: () => this.error = 'Skill order could not be saved.'
    });
  }

  moveExperience(item: ExperienceDto, offset: number): void {
    const index = this.experiences.findIndex(value => value.id === item.id);
    const target = index + offset;
    if (index < 0 || target < 0 || target >= this.experiences.length) return;
    const reordered = [...this.experiences];
    [reordered[index], reordered[target]] = [reordered[target], reordered[index]];
    this.api.reorderExperiences(reordered.map((value, displayOrder) => ({ id: value.id, displayOrder }))).subscribe({
      next: response => response.succeeded
        ? this.experiences = reordered.map((value, displayOrder) => ({ ...value, displayOrder }))
        : this.error = response.message,
      error: () => this.error = 'Experience order could not be saved.'
    });
  }

  newExperience(): void {
    this.editingExperience = null;
    this.experienceForm = this.emptyExperience();
  }

  editExperience(item: ExperienceDto): void {
    this.editingExperience = item;
    this.experienceForm = {
      jobTitle: item.jobTitle, companyName: item.companyName, description: item.description,
      startDate: item.startDate.slice(0, 10), endDate: item.endDate?.slice(0, 10) ?? null,
      isCurrent: item.isCurrent, displayOrder: item.displayOrder
    };
  }

  saveExperience(): void {
    this.experienceFormTouched = true;
    this.experienceDrawerError = '';

    if (!this.experienceForm.jobTitle?.trim()) {
      this.experienceDrawerError = 'يرجى إدخال المسمى الوظيفي أولاً (حقل إجباري).';
      return;
    }
    if (!this.experienceForm.companyName?.trim()) {
      this.experienceDrawerError = 'يرجى إدخال اسم الشركة أو المؤسسة أولاً (حقل إجباري).';
      return;
    }
    if (!this.experienceForm.startDate) {
      this.experienceDrawerError = 'يرجى تحديد تاريخ بداية العمل (حقل إجباري).';
      return;
    }

    this.saving = true;
    const request = { ...this.experienceForm, endDate: this.experienceForm.isCurrent ? null : this.experienceForm.endDate };
    const operation = this.editingExperience
      ? this.api.updateExperience(this.editingExperience.id, request)
      : this.api.createExperience(request);
    operation.subscribe({
      next: response => {
        this.saving = false;
        if (!response.succeeded) {
          this.experienceDrawerError = sanitizeTechnicalText(response.message, 'تعذر حفظ الخبرة، يرجى مراجعة البيانات المدخلة.');
          return;
        }
        this.notice = 'تم حفظ سجل الخبرة بنجاح.';
        setTimeout(() => this.notice = '', 3500);
        this.experiences = this.editingExperience
          ? this.experiences.map(item => item.id === response.data.id ? response.data : item)
          : [...this.experiences, response.data];
        this.closeExperienceDrawer();
      },
      error: err => {
        this.saving = false;
        this.experienceDrawerError = formatFriendlyError(err, 'حدث خطأ أثناء حفظ الخبرة، يرجى المحاولة ثانية.');
      }
    });
  }

  toggleExperience(item: ExperienceDto): void {
    this.api.setExperienceVisibility(item.id, !item.isVisible).subscribe({
      next: response => response.succeeded ? item.isVisible = response.data.isVisible : this.error = sanitizeTechnicalText(response.message, 'تعذر تعديل ظهور الخبرة.'),
      error: err => this.error = formatFriendlyError(err, 'فشل تحديث حالة الظهور.')
    });
  }

  deleteExperience(item: ExperienceDto): void {
    this.openDeleteConfirm({
      title: 'حذف سجل الخبرة / Delete Experience',
      itemName: `${item.jobTitle} @ ${item.companyName}`,
      itemType: 'Experience',
      message: `هل أنت متأكد من رغبتك في حذف خبرة "${item.jobTitle}" في "${item.companyName}"؟`,
      action: () => {
        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteExperience(item.id).subscribe({
          next: () => {
            this.experiences = this.experiences.filter(value => value.id !== item.id);
            this.closeDeleteConfirm();
            this.notice = 'تم حذف سجل الخبرة بنجاح.';
            setTimeout(() => this.notice = '', 3000);
          },
          error: err => {
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = formatFriendlyError(err, 'تعذر حذف سجل الخبرة حالياً.');
          }
        });
      }
    });
  }

  deleteEducation(item: EducationDto): void {
    this.openDeleteConfirm({
      title: 'حذف السجل الأكاديمي / Delete Education',
      itemName: `${item.degree} (${item.institutionName})`,
      itemType: 'Education',
      message: `هل أنت متأكد من حذف مؤهل "${item.degree}" في "${item.institutionName}"؟`,
      action: () => {
        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteEducation(item.id).subscribe({
          next: () => {
            this.education = this.education.filter(value => value.id !== item.id);
            this.closeDeleteConfirm();
            this.notice = 'تم حذف السجل الأكاديمي بنجاح.';
            setTimeout(() => this.notice = '', 3000);
          },
          error: err => {
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = formatFriendlyError(err, 'تعذر حذف السجل التعليمي حالياً.');
          }
        });
      }
    });
  }

  moveEducation(item: EducationDto, offset: number): void {
    const index = this.education.findIndex(value => value.id === item.id);
    const target = index + offset;
    if (index < 0 || target < 0 || target >= this.education.length) return;
    const reordered = [...this.education];
    [reordered[index], reordered[target]] = [reordered[target], reordered[index]];
    this.api.reorderEducations(reordered.map((value, displayOrder) => ({ id: value.id, displayOrder }))).subscribe({
      next: result => result.succeeded ? this.education = reordered.map((value, displayOrder) => ({ ...value, displayOrder })) : this.error = sanitizeTechnicalText(result.message, 'تعذر حفظ الترتيب.'),
      error: err => this.error = formatFriendlyError(err, 'فشل حفظ الترتيب.')
    });
  }

  editEducation(item: EducationDto): void {
    this.editingEducation = item;
    this.educationForm = {
      institutionName: item.institutionName, degree: item.degree, fieldOfStudy: item.fieldOfStudy,
      description: item.description, startDate: item.startDate.slice(0, 10),
      endDate: item.endDate?.slice(0, 10) ?? null, isCurrent: item.isCurrent, displayOrder: item.displayOrder
    };
  }

  saveEducation(): void {
    const request = { ...this.educationForm, endDate: this.educationForm.isCurrent ? null : this.educationForm.endDate };
    const operation = this.editingEducation
      ? this.api.updateEducation(this.editingEducation.id, request)
      : this.api.createEducation(request);
    operation.subscribe({
      next: response => {
        if (!response.succeeded) {
          this.error = sanitizeTechnicalText(response.message, 'تعذر حفظ البيانات التعليمية.');
          return;
        }
        this.notice = 'تم حفظ السجل التعليمي بنجاح.';
        setTimeout(() => this.notice = '', 3000);
        this.education = this.editingEducation
          ? this.education.map(item => item.id === response.data.id ? response.data : item)
          : [...this.education, response.data];
        this.editingEducation = null;
        this.educationForm = this.emptyEducation();
      },
      error: err => this.error = formatFriendlyError(err, 'تعذر حفظ السجل التعليمي حالياً.')
    });
  }

  toggleEducation(item: EducationDto): void {
    this.api.setEducationVisibility(item.id, !item.isVisible).subscribe({
      next: response => response.succeeded ? item.isVisible = response.data.isVisible : this.error = sanitizeTechnicalText(response.message, 'تعذر تعديل ظهور السجل.'),
      error: err => this.error = formatFriendlyError(err, 'فشل تعديل حالة الظهور.')
    });
  }

  editCertification(item: CertificationDto): void {
    this.editingCertification = item;
    this.certificationForm = {
      name: item.name, issuingOrganization: item.issuingOrganization, issueDate: item.issueDate.slice(0, 10),
      credentialId: item.credentialId, credentialUrl: item.credentialUrl,
      description: item.description, displayOrder: item.displayOrder
    };
  }

  saveCertification(): void {
    const operation = this.editingCertification
      ? this.api.updateCertification(this.editingCertification.id, this.certificationForm)
      : this.api.createCertification(this.certificationForm);
    operation.subscribe({
      next: response => {
        if (!response.succeeded) {
          this.error = sanitizeTechnicalText(response.message, 'تعذر حفظ الشهادة المهنية.');
          return;
        }
        this.notice = 'تم حفظ الشهادة المهنية بنجاح.';
        setTimeout(() => this.notice = '', 3000);
        this.certifications = this.editingCertification
          ? this.certifications.map(item => item.id === response.data.id ? response.data : item)
          : [...this.certifications, response.data];
        this.editingCertification = null;
        this.certificationForm = this.emptyCertification();
      },
      error: err => this.error = formatFriendlyError(err, 'تعذر حفظ الشهادة المهنية حالياً.')
    });
  }

  toggleCertification(item: CertificationDto): void {
    this.api.setCertificationVisibility(item.id, !item.isVisible).subscribe({
      next: response => response.succeeded ? item.isVisible = response.data.isVisible : this.error = sanitizeTechnicalText(response.message, 'تعذر تعديل ظهور الشهادة.'),
      error: err => this.error = formatFriendlyError(err, 'فشل تعديل حالة الظهور.')
    });
  }

  deleteCertification(item: CertificationDto): void {
    this.openDeleteConfirm({
      title: 'حذف الشهادة المهنية / Delete Certification',
      itemName: `${item.name} (${item.issuingOrganization})`,
      itemType: 'Certification',
      message: `هل أنت متأكد من حذف شهادة "${item.name}"؟`,
      action: () => {
        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteCertification(item.id).subscribe({
          next: () => {
            this.certifications = this.certifications.filter(value => value.id !== item.id);
            this.closeDeleteConfirm();
            this.notice = 'تم حذف الشهادة المهنية بنجاح.';
            setTimeout(() => this.notice = '', 3000);
          },
          error: err => {
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = formatFriendlyError(err, 'تعذر حذف الشهادة المهنية حالياً.');
          }
        });
      }
    });
  }

  moveCertification(item: CertificationDto, offset: number): void {
    const index = this.certifications.findIndex(value => value.id === item.id);
    const target = index + offset;
    if (index < 0 || target < 0 || target >= this.certifications.length) return;
    const reordered = [...this.certifications];
    [reordered[index], reordered[target]] = [reordered[target], reordered[index]];
    this.api.reorderCertifications(reordered.map((value, displayOrder) => ({ id: value.id, displayOrder }))).subscribe({
      next: result => result.succeeded ? this.certifications = reordered.map((value, displayOrder) => ({ ...value, displayOrder })) : this.error = sanitizeTechnicalText(result.message, 'تعذر حفظ الترتيب.'),
      error: err => this.error = formatFriendlyError(err, 'فشل حفظ الترتيب.')
    });
  }

  private emptyExperience(): CreateExperienceRequest {
    return { jobTitle: '', companyName: '', description: '', startDate: new Date().toISOString().slice(0, 10), endDate: null, isCurrent: false, displayOrder: this.experiences.length };
  }

  private emptyEducation(): CreateEducationRequest {
    return { institutionName: '', degree: '', fieldOfStudy: null, description: null, startDate: new Date().toISOString().slice(0, 10), endDate: null, isCurrent: false, displayOrder: this.education.length };
  }

  private emptyCertification(): CreateCertificationRequest {
    return { name: '', issuingOrganization: '', issueDate: new Date().toISOString().slice(0, 10), credentialId: null, credentialUrl: null, description: null, displayOrder: this.certifications.length };
  }
}
