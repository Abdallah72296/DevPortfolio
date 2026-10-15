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
  educationModalOpen = false;
  credentialModalType: 'education' | 'certification' = 'education';
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
          this.error = sanitizeTechnicalText(response.message, 'Could not add the skill.');
          return;
        }
        registerLearnedSkill(name, resolvedIcon, category.name);
        this.inlineSkillInput[category.id] = '';
        this.inlineSkillActive[category.id] = false;
        category.skills = [...(category.skills ?? []), response.data];
        this.notice = `Skill “${name}” added.`;
        setTimeout(() => this.notice = '', 3000);
      },
      error: err => this.error = formatFriendlyError(err, 'Could not add the skill right now.')
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
          this.error = sanitizeTechnicalText(response.message, 'Could not load skill categories.');
        }
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.error = formatFriendlyError(err, 'Could not load skills. Check that the server is running and you are signed in.');
      }
    });
    this.api.getExperiences().subscribe({
      next: response => { if (response.succeeded) this.experiences = response.data ?? []; },
      error: err => this.error ||= formatFriendlyError(err, 'Could not load work experience.')
    });
    this.api.getEducations().subscribe({
      next: response => { if (response.succeeded) this.education = response.data ?? []; },
      error: err => this.error ||= formatFriendlyError(err, 'Could not load education records.')
    });
    this.api.getCertifications().subscribe({
      next: response => { if (response.succeeded) this.certifications = response.data ?? []; },
      error: err => this.error ||= formatFriendlyError(err, 'Could not load certifications.')
    });
  }

  addCategory(): void {
    const name = this.newCategoryName.trim();
    if (!name) {
      this.error = 'Enter a category name first. This field is required.';
      return;
    }
    this.error = '';
    this.api.createSkillCategory({ name, displayOrder: this.categories.length }).subscribe({
      next: response => {
        if (!response.succeeded) {
          this.error = sanitizeTechnicalText(response.message, 'Could not add the category. Please try again.');
          return;
        }
        this.newCategoryName = '';
        this.selectedCategoryId = response.data.id;
        this.categories = [...this.categories, { ...response.data, skills: response.data.skills ?? [] }];
        this.taxonomyConfigOpen = false;
        this.notice = `Category “${name}” added.`;
        setTimeout(() => this.notice = '', 3500);
      },
      error: err => {
        this.error = formatFriendlyError(err, 'Could not add the category. Review the details and try again.');
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
          this.notice = 'Skill icon uploaded.';
          setTimeout(() => this.notice = '', 3000);
        }
      },
      error: () => {
        this.isUploadingSkillIcon = false;
        this.error = 'Could not upload the icon.';
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
      this.error = 'Enter a skill name first. This field is required.';
      return;
    }
    const category = this.categories.find(item => item.id === this.selectedCategoryId);
    if (!category) {
      this.error = 'Choose a category for this skill first.';
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
          this.error = sanitizeTechnicalText(response.message, 'Could not add the skill. Check the details and try again.');
          return;
        }
        registerLearnedSkill(name, resolvedIcon, category.name);
        this.newSkillName = '';
        this.customSkillIcon = '';
        this.showSkillSuggestions = false;
        this.detectedSkillIconDisplay = { type: 'material', value: 'code' };
        category.skills = [...(category.skills ?? []), response.data];
        this.notice = `Skill “${name}” added to ${category.name}.`;
        setTimeout(() => this.notice = '', 3500);
      },
      error: err => {
        this.error = formatFriendlyError(err, 'Could not add the skill. Please try again later.');
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
      title: 'Delete Skill Category',
      itemName: category.name,
      itemType: 'Category',
      message: `Delete “${category.name}” and all ${skillsCount} skills in this category?`,
      action: () => {
        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteSkillCategory(category.id).subscribe({
          next: () => {
            this.categories = this.categories.filter(item => item.id !== category.id);
            if (this.selectedCategoryId === category.id) {
              this.selectedCategoryId = this.categories[0]?.id ?? '';
            }
            this.closeDeleteConfirm(true);
            this.notice = `Category “${category.name}” deleted.`;
            setTimeout(() => this.notice = '', 3000);
          },
          error: (err: any) => {
            if (err?.status === 404) {
              this.categories = this.categories.filter(item => item.id !== category.id);
              if (this.selectedCategoryId === category.id) {
                this.selectedCategoryId = this.categories[0]?.id ?? '';
              }
              this.closeDeleteConfirm(true);
              this.notice = `Category “${category.name}” deleted.`;
              setTimeout(() => this.notice = '', 3000);
              return;
            }
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = 'Could not delete the skill category. Please try again.';
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
      this.experienceDrawerError = 'Enter a job title first. This field is required.';
      return;
    }
    if (!this.experienceForm.companyName?.trim()) {
      this.experienceDrawerError = 'Enter a company or organization first. This field is required.';
      return;
    }
    if (!this.experienceForm.startDate) {
      this.experienceDrawerError = 'Enter a start date. This field is required.';
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
          this.experienceDrawerError = sanitizeTechnicalText(response.message, 'Could not save the experience. Review the details and try again.');
          return;
        }
        this.notice = 'Work experience saved.';
        setTimeout(() => this.notice = '', 3500);
        this.experiences = this.editingExperience
          ? this.experiences.map(item => item.id === response.data.id ? response.data : item)
          : [...this.experiences, response.data];
        this.closeExperienceDrawer();
      },
      error: err => {
        this.saving = false;
        this.experienceDrawerError = formatFriendlyError(err, 'Could not save the experience. Please try again.');
      }
    });
  }

  toggleExperience(item: ExperienceDto): void {
    this.api.setExperienceVisibility(item.id, !item.isVisible).subscribe({
      next: response => response.succeeded ? item.isVisible = response.data.isVisible : this.error = sanitizeTechnicalText(response.message, 'Could not update experience visibility.'),
      error: err => this.error = formatFriendlyError(err, 'Could not update visibility.')
    });
  }

  deleteExperience(item: ExperienceDto): void {
    this.openDeleteConfirm({
      title: 'Delete Work Experience',
      itemName: `${item.jobTitle} @ ${item.companyName}`,
      itemType: 'Experience',
      message: `Delete “${item.jobTitle}” at “${item.companyName}”?`,
      action: () => {
        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteExperience(item.id).subscribe({
          next: () => {
            this.experiences = this.experiences.filter(value => value.id !== item.id);
            this.closeDeleteConfirm(true);
            this.notice = 'Work experience deleted.';
            setTimeout(() => this.notice = '', 3000);
          },
          error: (err: any) => {
            if (err?.status === 404) {
              this.experiences = this.experiences.filter(value => value.id !== item.id);
              this.closeDeleteConfirm(true);
              this.notice = 'Work experience deleted.';
              setTimeout(() => this.notice = '', 3000);
              return;
            }
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = formatFriendlyError(err, 'Could not delete work experience right now.');
          }
        });
      }
    });
  }

  deleteEducation(item: EducationDto): void {
    this.openDeleteConfirm({
      title: 'Delete Education',
      itemName: `${item.degree} (${item.institutionName})`,
      itemType: 'Education',
      message: `Delete the “${item.degree}” qualification from “${item.institutionName}”?`,
      action: () => {
        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteEducation(item.id).subscribe({
          next: () => {
            this.education = this.education.filter(value => value.id !== item.id);
            this.closeDeleteConfirm(true);
            this.notice = 'Education deleted.';
            setTimeout(() => this.notice = '', 3000);
          },
          error: (err: any) => {
            if (err?.status === 404) {
              this.education = this.education.filter(value => value.id !== item.id);
              this.closeDeleteConfirm(true);
              this.notice = 'Education deleted.';
              setTimeout(() => this.notice = '', 3000);
              return;
            }
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = formatFriendlyError(err, 'Could not delete education right now.');
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
      next: result => result.succeeded ? this.education = reordered.map((value, displayOrder) => ({ ...value, displayOrder })) : this.error = sanitizeTechnicalText(result.message, 'Could not save the order.'),
      error: err => this.error = formatFriendlyError(err, 'Could not save the order.')
    });
  }

  openAddEducation(): void {
    this.editingEducation = null;
    this.educationForm = this.emptyEducation();
    this.credentialModalType = 'education';
    this.educationModalOpen = true;
  }

  openAddCertification(): void {
    this.editingCertification = null;
    this.certificationForm = this.emptyCertification();
    this.credentialModalType = 'certification';
    this.educationModalOpen = true;
  }

  editEducation(item: EducationDto): void {
    this.editingEducation = item;
    this.educationForm = {
      institutionName: item.institutionName, degree: item.degree, fieldOfStudy: item.fieldOfStudy || '',
      description: item.description || '', startDate: item.startDate ? item.startDate.slice(0, 10) : '',
      endDate: item.endDate ? item.endDate.slice(0, 10) : null, isCurrent: item.isCurrent, displayOrder: item.displayOrder
    };
    this.credentialModalType = 'education';
    this.educationModalOpen = true;
  }

  saveEducation(): void {
    const request = { ...this.educationForm, endDate: this.educationForm.isCurrent ? null : this.educationForm.endDate };
    const operation = this.editingEducation
      ? this.api.updateEducation(this.editingEducation.id, request)
      : this.api.createEducation(request);
    operation.subscribe({
      next: response => {
        if (!response.succeeded) {
          this.error = sanitizeTechnicalText(response.message, 'Could not save education details.');
          return;
        }
        this.notice = 'Education saved.';
        setTimeout(() => this.notice = '', 3000);
        this.education = this.editingEducation
          ? this.education.map(item => item.id === response.data.id ? response.data : item)
          : [...this.education, response.data];
        this.editingEducation = null;
        this.educationForm = this.emptyEducation();
        this.educationModalOpen = false;
      },
      error: err => this.error = formatFriendlyError(err, 'Could not save education right now.')
    });
  }

  toggleEducation(item: EducationDto): void {
    this.api.setEducationVisibility(item.id, !item.isVisible).subscribe({
      next: response => response.succeeded ? item.isVisible = response.data.isVisible : this.error = sanitizeTechnicalText(response.message, 'Could not update record visibility.'),
      error: err => this.error = formatFriendlyError(err, 'Could not update visibility.')
    });
  }

  editCertification(item: CertificationDto): void {
    this.editingCertification = item;
    this.certificationForm = {
      name: item.name, issuingOrganization: item.issuingOrganization,
      issueDate: item.issueDate ? item.issueDate.slice(0, 10) : '',
      credentialId: item.credentialId || '', credentialUrl: item.credentialUrl || '',
      description: item.description || '', displayOrder: item.displayOrder
    };
    this.credentialModalType = 'certification';
    this.educationModalOpen = true;
  }

  saveCertification(): void {
    const operation = this.editingCertification
      ? this.api.updateCertification(this.editingCertification.id, this.certificationForm)
      : this.api.createCertification(this.certificationForm);
    operation.subscribe({
      next: response => {
        if (!response.succeeded) {
          this.error = sanitizeTechnicalText(response.message, 'Could not save the certification.');
          return;
        }
        this.notice = 'Certification saved.';
        setTimeout(() => this.notice = '', 3000);
        this.certifications = this.editingCertification
          ? this.certifications.map(item => item.id === response.data.id ? response.data : item)
          : [...this.certifications, response.data];
        this.editingCertification = null;
        this.certificationForm = this.emptyCertification();
        this.educationModalOpen = false;
      },
      error: err => this.error = formatFriendlyError(err, 'Could not save the certification right now.')
    });
  }

  toggleCertification(item: CertificationDto): void {
    this.api.setCertificationVisibility(item.id, !item.isVisible).subscribe({
      next: response => response.succeeded ? item.isVisible = response.data.isVisible : this.error = sanitizeTechnicalText(response.message, 'Could not update certification visibility.'),
      error: err => this.error = formatFriendlyError(err, 'Could not update visibility.')
    });
  }

  deleteCertification(item: CertificationDto): void {
    this.openDeleteConfirm({
      title: 'Delete Certification',
      itemName: `${item.name} (${item.issuingOrganization})`,
      itemType: 'Certification',
      message: `Delete the “${item.name}” certification?`,
      action: () => {
        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteCertification(item.id).subscribe({
          next: () => {
            this.certifications = this.certifications.filter(value => value.id !== item.id);
            this.closeDeleteConfirm(true);
            this.notice = 'Certification deleted.';
            setTimeout(() => this.notice = '', 3000);
          },
          error: (err: any) => {
            if (err?.status === 404) {
              this.certifications = this.certifications.filter(value => value.id !== item.id);
              this.closeDeleteConfirm(true);
              this.notice = 'Certification deleted.';
              setTimeout(() => this.notice = '', 3000);
              return;
            }
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = formatFriendlyError(err, 'Could not delete the certification right now.');
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
      next: result => result.succeeded ? this.certifications = reordered.map((value, displayOrder) => ({ ...value, displayOrder })) : this.error = sanitizeTechnicalText(result.message, 'Could not save the order.'),
      error: err => this.error = formatFriendlyError(err, 'Could not save the order.')
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
