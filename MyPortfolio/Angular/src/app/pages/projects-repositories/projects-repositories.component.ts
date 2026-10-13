import { Component, OnInit, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DashboardHeaderComponent } from '../../shared/components/dashboard-header/dashboard-header.component';
import { DashboardSidebarComponent } from '../../shared/components/dashboard-sidebar/dashboard-sidebar.component';
import { AdminApiService } from '../../core/services/admin-api.service';
import { ProjectDto } from '../../core/models/admin.models';
import { formatFriendlyError, sanitizeTechnicalText } from '../../core/helpers/error-formatter';

@Component({
  selector: 'app-projects-repositories',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, DashboardHeaderComponent, DashboardSidebarComponent],
  templateUrl: './projects-repositories.component.html',
  styleUrl: './projects-repositories.component.css'
})
export class ProjectsRepositoriesComponent implements OnInit {
  private readonly api = inject(AdminApiService);

  projects: ProjectDto[] = [];
  searchQuery = '';
  featuredOnly = false;
  statusFilter: 'all' | 'published' | 'draft' = 'all';
  categoryFilter = 'all';
  viewMode: 'grid' | 'list' | 'architecture' = 'grid';

  // Reorder mode state
  isReorderMode = false;

  // GitHub Sync State
  syncingGithub = false;
  syncGithubSuccess = false;

  // Loading & Alerts
  loading = false;
  saving = false;
  error = '';
  notice = '';

  // Slide-in Drawer State & In-drawer Validation
  drawerOpen = false;
  selected: ProjectDto | null = null;
  tagInput = '';
  drawerError = '';
  drawerTitleTouched = false;

  // Image Upload State
  isUploadingImage = false;
  isDraggingImage = false;

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

  ngOnInit(): void {
    this.loadProjects();
  }

  get filteredProjects(): ProjectDto[] {
    const query = this.searchQuery.trim().toLowerCase();
    return this.projects.filter(project => {
      const text = `${project.title} ${project.description || ''} ${(project.technologies || []).join(' ')}`.toLowerCase();
      const matchesQuery = !query || text.includes(query);
      const matchesFeatured = !this.featuredOnly || project.isFeatured;
      const matchesStatus = this.statusFilter === 'all' ||
        (this.statusFilter === 'published' && project.isVisible) ||
        (this.statusFilter === 'draft' && !project.isVisible);
      const category = this.getProjectCategory(project);
      const matchesCategory = this.categoryFilter === 'all' || category === this.categoryFilter;
      return matchesQuery && matchesFeatured && matchesStatus && matchesCategory;
    });
  }

  get publishedCount(): number {
    return this.projects.filter(p => p.isVisible).length;
  }

  get featuredCount(): number {
    return this.projects.filter(p => p.isFeatured).length;
  }

  get liveDemosCount(): number {
    return this.projects.filter(p => !!p.liveUrl && p.liveUrl.trim().length > 0 && p.liveUrl !== '#').length;
  }

  getProjectCategory(project: ProjectDto): string {
    const joined = (project.technologies || []).join(' ').toLowerCase();
    if (joined.includes('rust') || joined.includes('raft') || joined.includes('kafka') || joined.includes('postgres') || joined.includes('sql') || joined.includes('c#') || joined.includes('.net')) return 'distributed';
    if (joined.includes('angular') || joined.includes('react') || joined.includes('vue') || joined.includes('html') || joined.includes('css') || joined.includes('tailwind') || joined.includes('typescript') || joined.includes('javascript')) return 'frontend';
    if (joined.includes('envoy') || joined.includes('k8s') || joined.includes('kubernetes') || joined.includes('cloud') || joined.includes('docker') || joined.includes('aws') || joined.includes('azure')) return 'cloud';
    if (joined.includes('onnx') || joined.includes('ai') || joined.includes('vector') || joined.includes('qdrant') || joined.includes('python') || joined.includes('ml')) return 'ai';
    return 'distributed';
  }

  loadProjects(): void {
    this.loading = true;
    this.error = '';
    this.api.getProjects().subscribe({
      next: result => {
        this.loading = false;
        if (result.succeeded) {
          this.projects = result.data ?? [];
        } else {
          this.error = sanitizeTechnicalText(result.message, 'تعذر تحميل المشاريع حالياً.');
        }
      },
      error: err => {
        this.loading = false;
        this.error = formatFriendlyError(err, 'تعذر الاتصال بالخادم لتحميل المشاريع.');
      }
    });
  }

  openDrawer(project?: ProjectDto): void {
    this.drawerError = '';
    this.drawerTitleTouched = false;
    if (project) {
      this.selected = {
        ...project,
        technologies: [...(project.technologies || [])]
      };
    } else {
      this.selected = {
        id: '',
        title: '',
        description: '',
        image: null,
        liveUrl: '',
        githubUrl: '',
        technologies: [],
        isFeatured: false,
        displayOrder: this.projects.length,
        isVisible: true
      };
    }
    this.tagInput = '';
    this.drawerOpen = true;
  }

  closeDrawer(): void {
    this.drawerOpen = false;
    this.selected = null;
    this.drawerError = '';
    this.drawerTitleTouched = false;
  }

  addTag(): void {
    const tag = this.tagInput.trim();
    if (tag && this.selected) {
      if (!this.selected.technologies) {
        this.selected.technologies = [];
      }
      if (!this.selected.technologies.includes(tag)) {
        this.selected.technologies.push(tag);
      }
      this.tagInput = '';
    }
  }

  removeTag(index: number): void {
    if (this.selected && this.selected.technologies) {
      this.selected.technologies.splice(index, 1);
    }
  }

  saveProject(): void {
    if (!this.selected) return;

    this.drawerTitleTouched = true;
    this.drawerError = '';

    const title = (this.selected.title || '').trim();
    if (!title) {
      this.drawerError = 'يرجى إدخال اسم المشروع أولاً للمتابعة (حقل إجباري).';
      return;
    }

    this.saving = true;
    const form = this.selected;

    const request = {
      title,
      description: form.description ? form.description.trim() : '',
      image: form.image,
      liveUrl: form.liveUrl ? form.liveUrl.trim() : null,
      githubUrl: form.githubUrl ? form.githubUrl.trim() : null,
      technologies: form.technologies || [],
      isFeatured: form.isFeatured,
      displayOrder: form.displayOrder
    };

    const save$ = form.id
      ? this.api.updateProject(form.id, { ...request, isVisible: form.isVisible })
      : this.api.createProject(request);

    save$.subscribe({
      next: result => {
        this.saving = false;
        if (result.succeeded) {
          this.closeDrawer();
          this.notice = 'تم حفظ المشروع بنجاح في قاعدة البيانات.';
          setTimeout(() => this.notice = '', 3500);
          this.loadProjects();
        } else {
          this.drawerError = sanitizeTechnicalText(result.message, 'تعذر حفظ المشروع، يرجى التأكد من البيانات المدخلة والمحاولة مرة أخرى.');
        }
      },
      error: err => {
        this.saving = false;
        this.drawerError = formatFriendlyError(err, 'حدث خطأ أثناء حفظ المشروع، يرجى مراجعة الحقول والمحاولة ثانية.');
      }
    });
  }

  toggleVisibility(project: ProjectDto): void {
    const nextVal = !project.isVisible;
    this.api.setProjectVisibility(project.id, nextVal).subscribe({
      next: res => {
        if (res.succeeded) {
          project.isVisible = nextVal;
        } else {
          this.error = sanitizeTechnicalText(res.message, 'تعذر تعديل حالة ظهور المشروع.');
        }
      },
      error: err => this.error = formatFriendlyError(err, 'فشل تحديث حالة الظهور.')
    });
  }

  toggleFeatured(project: ProjectDto): void {
    const nextVal = !project.isFeatured;
    this.api.setProjectFeatured(project.id, nextVal).subscribe({
      next: res => {
        if (res.succeeded) {
          project.isFeatured = nextVal;
        } else {
          this.error = sanitizeTechnicalText(res.message, 'تعذر تحديث تمييز المشروع.');
        }
      },
      error: err => this.error = formatFriendlyError(err, 'فشل تحديث حالة المشروع المميز.')
    });
  }

  deleteProject(project: ProjectDto): void {
    this.openDeleteConfirm({
      title: 'حذف المشروع / Delete Project',
      itemName: project.title,
      itemType: 'Project',
      message: `هل أنت متأكد من حذف المشروع "${project.title}" نهائياً من قاعدة البيانات؟`,
      action: () => {
        this.deleteModal.isDeleting = true;
        this.deleteModal.errorMessage = '';
        this.api.deleteProject(project.id).subscribe({
          next: () => {
            this.projects = this.projects.filter(p => p.id !== project.id);
            this.closeDrawer();
            this.closeDeleteConfirm(true);
            this.notice = 'تم حذف المشروع بنجاح.';
            setTimeout(() => this.notice = '', 3000);
          },
          error: (err: any) => {
            if (err?.status === 404) {
              this.projects = this.projects.filter(p => p.id !== project.id);
              this.closeDrawer();
              this.closeDeleteConfirm(true);
              this.notice = 'تم حذف المشروع بنجاح.';
              setTimeout(() => this.notice = '', 3000);
              return;
            }
            this.deleteModal.isDeleting = false;
            this.deleteModal.errorMessage = formatFriendlyError(err, 'تعذر حذف المشروع حالياً.');
          }
        });
      }
    });
  }

  toggleReorderMode(): void {
    this.isReorderMode = !this.isReorderMode;
  }

  moveProject(project: ProjectDto, offset: number): void {
    const currentIndex = this.projects.findIndex(item => item.id === project.id);
    const targetIndex = currentIndex + offset;
    if (currentIndex < 0 || targetIndex < 0 || targetIndex >= this.projects.length) return;
    const reordered = [...this.projects];
    [reordered[currentIndex], reordered[targetIndex]] = [reordered[targetIndex], reordered[currentIndex]];
    this.projects = reordered;
  }

  saveOrder(): void {
    this.isReorderMode = false;
    const items = this.projects.map((p, idx) => ({ id: p.id, displayOrder: idx }));
    this.api.reorderProjects(items).subscribe({
      next: res => {
        if (res.succeeded) {
          this.notice = 'تم حفظ ترتيب عرض المشاريع بنجاح.';
          setTimeout(() => this.notice = '', 3000);
          this.loadProjects();
        } else {
          this.error = sanitizeTechnicalText(res.message, 'تعذر حفظ ترتيب المشاريع.');
        }
      },
      error: err => this.error = formatFriendlyError(err, 'فشل حفظ ترتيب المشاريع.')
    });
  }

  setViewMode(mode: 'grid' | 'list' | 'architecture'): void {
    this.viewMode = mode;
  }

  syncGithub(): void {
    this.syncingGithub = true;
    this.syncGithubSuccess = false;
    this.loadProjects();
    setTimeout(() => {
      this.syncingGithub = false;
      this.syncGithubSuccess = true;
      setTimeout(() => {
        this.syncGithubSuccess = false;
      }, 2500);
    }, 1200);
  }

  uploadProjectImage(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      this.handleImageFile(file);
    }
  }

  onImageDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingImage = true;
  }

  onImageDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingImage = false;
  }

  onImageDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDraggingImage = false;
    const file = event.dataTransfer?.files?.[0];
    if (file) {
      this.handleImageFile(file);
    }
  }

  private handleImageFile(file: File): void {
    if (!file.type.startsWith('image/')) {
      const msg = 'يرجى اختيار ملف صورة صالح بصيغة (JPG, PNG, WebP, SVG, GIF).';
      if (this.drawerOpen) {
        this.drawerError = msg;
      } else {
        this.error = msg;
      }
      return;
    }
    this.isUploadingImage = true;
    if (this.drawerOpen) this.drawerError = ''; else this.error = '';
    this.api.uploadImage(file).subscribe({
      next: res => {
        this.isUploadingImage = false;
        if (res.succeeded && this.selected) {
          this.selected.image = res.relativeUrl || res.url;
          this.notice = 'تم رفع صورة المشروع بنجاح.';
          setTimeout(() => this.notice = '', 3000);
        } else {
          const failMsg = sanitizeTechnicalText((res as any).message, 'تعذر حفظ ملف الصورة، يرجى المحاولة مرة أخرى.');
          if (this.drawerOpen) this.drawerError = failMsg; else this.error = failMsg;
        }
      },
      error: err => {
        this.isUploadingImage = false;
        const errMsg = formatFriendlyError(err, 'حدث خطأ أثناء رفع الصورة. يرجى التأكد من أن حجم الصورة أقل من 10 ميجابايت.');
        if (this.drawerOpen) this.drawerError = errMsg; else this.error = errMsg;
      }
    });
  }

  removeImage(): void {
    if (this.selected) {
      this.selected.image = null;
    }
  }

  @HostListener('window:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent): void {
    if ((event.metaKey || event.ctrlKey) && event.key === 'k') {
      event.preventDefault();
      const el = document.getElementById('projectSearchInput') as HTMLInputElement;
      el?.focus();
    }
    if (event.key === 'n' || event.key === 'N') {
      const activeTag = (document.activeElement?.tagName || '').toLowerCase();
      if (activeTag !== 'input' && activeTag !== 'textarea') {
        event.preventDefault();
        this.openDrawer();
      }
    }
    if (event.key === 'Escape' && this.drawerOpen) {
      this.closeDrawer();
    }
  }
}
