import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse,
  CertificationDto,
  ContactMessageDto,
  PaginatedResult,
  PortfolioAnalyticsSummary,
  CreateCertificationRequest,
  CreateEducationRequest,
  CreateExperienceRequest,
  CreatePortfolioSectionRequest,
  CreateProjectRequest,
  CreateSkillCategoryRequest,
  CreateSkillRequest,
  CreateSocialLinkRequest,
  CvPortfolioDraft,
  EducationDto,
  ExperienceDto,
  PortfolioSectionDto,
  ProjectDto,
  ReorderItem,
  SkillCategoryDto,
  SkillDto,
  SocialLinkDto,
  UpdateCertificationRequest,
  UpdateEducationRequest,
  UpdateExperienceRequest,
  UpdatePortfolioSectionRequest,
  UpdateProjectRequest,
  UpdateSkillCategoryRequest,
  UpdateSkillRequest,
  UpdateSocialLinkRequest,
  PortfolioSiteSettingsDto,
  AdminUserDto
} from '../models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class AdminApiService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return environment.apiUrl;
  }

  getApiHealth(): Observable<ApiResponse<{ status: string; service: string; timestamp: string; framework: string }>> {
    return this.http.get<ApiResponse<{ status: string; service: string; timestamp: string; framework: string }>>(
      `${this.apiUrl}/api/health`
    );
  }

  getDatabaseHealth(): Observable<ApiResponse<{ connected: boolean; provider: string; serverTime: string }>> {
    return this.http.get<ApiResponse<{ connected: boolean; provider: string; serverTime: string }>>(
      `${this.apiUrl}/api/health/db`
    );
  }

  trackPortfolioEvent(eventType: 'portfolio_view' | 'cv_download', username?: string): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.apiUrl}/api/analytics/events`, { eventType, username });
  }

  getAnalyticsSummary(): Observable<ApiResponse<PortfolioAnalyticsSummary>> {
    return this.http.get<ApiResponse<PortfolioAnalyticsSummary>>(`${this.apiUrl}/api/analytics/summary`);
  }

  getCvDownloadUrl(username?: string): string {
    const query = username ? `?u=${encodeURIComponent(username)}` : '';
    return `${this.apiUrl}/api/portfolio/cv-download${query}`;
  }

  uploadCv(file: File): Observable<{ succeeded: boolean; url: string; relativeUrl: string; fileName: string; message?: string }> {
    const formData = new FormData();
    const pdf = file.type === 'application/pdf' ? file : new File([file], file.name, { type: 'application/pdf' });
    formData.append('file', pdf, pdf.name);
    return this.http.post<{ succeeded: boolean; url: string; relativeUrl: string; fileName: string; message?: string }>(`${this.apiUrl}/api/upload/cv`, formData);
  }

  getSiteSettings(): Observable<ApiResponse<PortfolioSiteSettingsDto>> {
    return this.http.get<ApiResponse<PortfolioSiteSettingsDto>>(`${this.apiUrl}/api/admin/site-settings`);
  }

  updateSiteSettings(settings: Omit<PortfolioSiteSettingsDto, 'isPublished' | 'publishedAt'>): Observable<ApiResponse<PortfolioSiteSettingsDto>> {
    return this.http.put<ApiResponse<PortfolioSiteSettingsDto>>(`${this.apiUrl}/api/admin/site-settings`, settings);
  }

  getAdminUsers(pageNumber = 1, pageSize = 10, search = '', role = 'All', status = 'All'): Observable<ApiResponse<AdminUserDto[]>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (search.trim()) params = params.set('search', search.trim());
    if (role !== 'All') params = params.set('role', role);
    if (status !== 'All') params = params.set('status', status);
    return this.http.get<ApiResponse<AdminUserDto[]>>(`${this.apiUrl}/api/admin/users`, { params });
  }

  createAdminUser(email: string, password: string, role: 'Admin' | 'User', fullName = ''): Observable<ApiResponse<AdminUserDto>> {
    return this.http.post<ApiResponse<AdminUserDto>>(`${this.apiUrl}/api/admin/users`, { email, password, role, fullName });
  }

  setAdminUserRole(id: string, role: 'Admin' | 'User'): Observable<ApiResponse<AdminUserDto>> {
    return this.http.put<ApiResponse<AdminUserDto>>(`${this.apiUrl}/api/admin/users/${id}/role`, { role });
  }

  setAdminUserLocked(id: string, locked: boolean): Observable<ApiResponse<AdminUserDto>> {
    return this.http.put<ApiResponse<AdminUserDto>>(`${this.apiUrl}/api/admin/users/${id}/lock`, { locked });
  }

  deleteAdminUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/admin/users/${id}`);
  }

  // ==========================================
  // 1. AI CV Parse (Gemini Multimodal)
  // ==========================================
  parseCv(file: File): Observable<ApiResponse<CvPortfolioDraft>> {
    const formData = new FormData();
    const pdf = file.type === 'application/pdf' ? file : new File([file], file.name, { type: 'application/pdf' });
    formData.append('file', pdf, pdf.name);
    return this.http.post<ApiResponse<CvPortfolioDraft>>(
      `${this.apiUrl}/api/admin/cv/parse`,
      formData
    );
  }

  // ==========================================
  // 2. Portfolio Sections
  // ==========================================
  getSections(): Observable<ApiResponse<PortfolioSectionDto[]>> {
    return this.http.get<ApiResponse<PortfolioSectionDto[]>>(`${this.apiUrl}/api/admin/portfolio-sections`);
  }

  createSection(data: CreatePortfolioSectionRequest): Observable<ApiResponse<PortfolioSectionDto>> {
    return this.http.post<ApiResponse<PortfolioSectionDto>>(`${this.apiUrl}/api/admin/portfolio-sections`, data);
  }

  updateSection(id: string, data: UpdatePortfolioSectionRequest): Observable<ApiResponse<PortfolioSectionDto>> {
    return this.http.put<ApiResponse<PortfolioSectionDto>>(`${this.apiUrl}/api/admin/portfolio-sections/${id}`, data);
  }

  setSectionVisibility(id: string, isVisible: boolean): Observable<ApiResponse<PortfolioSectionDto>> {
    return this.http.patch<ApiResponse<PortfolioSectionDto>>(`${this.apiUrl}/api/admin/portfolio-sections/${id}/visibility`, { isVisible });
  }

  deleteSection(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/admin/portfolio-sections/${id}`);
  }

  reorderSections(items: ReorderItem[]): Observable<ApiResponse<PortfolioSectionDto[]>> {
    return this.http.put<ApiResponse<PortfolioSectionDto[]>>(`${this.apiUrl}/api/admin/portfolio-sections/reorder`, { items });
  }

  // ==========================================
  // 3. Skill Categories & Skills
  // ==========================================
  getSkillCategories(): Observable<ApiResponse<SkillCategoryDto[]>> {
    return this.http.get<ApiResponse<SkillCategoryDto[]>>(`${this.apiUrl}/api/admin/skill-categories`);
  }

  createSkillCategory(data: CreateSkillCategoryRequest): Observable<ApiResponse<SkillCategoryDto>> {
    return this.http.post<ApiResponse<SkillCategoryDto>>(`${this.apiUrl}/api/admin/skill-categories`, data);
  }

  createCategoryWithSkills(data: { name: string; skills: string[]; displayOrder?: number }): Observable<ApiResponse<SkillCategoryDto>> {
    return this.http.post<ApiResponse<SkillCategoryDto>>(`${this.apiUrl}/api/admin/skill-categories/with-skills`, data);
  }

  updateSkillCategory(id: string, data: UpdateSkillCategoryRequest): Observable<ApiResponse<SkillCategoryDto>> {
    return this.http.put<ApiResponse<SkillCategoryDto>>(`${this.apiUrl}/api/admin/skill-categories/${id}`, data);
  }

  deleteSkillCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/admin/skill-categories/${id}`);
  }

  reorderSkillCategories(items: ReorderItem[]): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.apiUrl}/api/admin/skill-categories/reorder`, { items });
  }

  setSkillCategoryVisibility(id: string, isVisible: boolean): Observable<ApiResponse<SkillCategoryDto>> {
    return this.http.patch<ApiResponse<SkillCategoryDto>>(`${this.apiUrl}/api/admin/skill-categories/${id}/visibility`, { isVisible });
  }

  getSkills(categoryId?: string): Observable<ApiResponse<SkillDto[]>> {
    const url = categoryId
      ? `${this.apiUrl}/api/admin/skills?categoryId=${categoryId}`
      : `${this.apiUrl}/api/admin/skills`;
    return this.http.get<ApiResponse<SkillDto[]>>(url);
  }

  createSkill(data: CreateSkillRequest): Observable<ApiResponse<SkillDto>> {
    return this.http.post<ApiResponse<SkillDto>>(`${this.apiUrl}/api/admin/skills`, data);
  }

  updateSkill(id: string, data: UpdateSkillRequest): Observable<ApiResponse<SkillDto>> {
    return this.http.put<ApiResponse<SkillDto>>(`${this.apiUrl}/api/admin/skills/${id}`, data);
  }

  deleteSkill(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/admin/skills/${id}`);
  }

  setSkillVisibility(id: string, isVisible: boolean): Observable<ApiResponse<SkillDto>> {
    return this.http.patch<ApiResponse<SkillDto>>(`${this.apiUrl}/api/admin/skills/${id}/visibility`, { isVisible });
  }

  reorderSkills(skillCategoryId: string, items: ReorderItem[]): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.apiUrl}/api/admin/skills/reorder`, { skillCategoryId, items });
  }

  // ==========================================
  // 4. Experiences
  // ==========================================
  getExperiences(): Observable<ApiResponse<ExperienceDto[]>> {
    return this.http.get<ApiResponse<ExperienceDto[]>>(`${this.apiUrl}/api/admin/experiences`);
  }

  createExperience(data: CreateExperienceRequest): Observable<ApiResponse<ExperienceDto>> {
    return this.http.post<ApiResponse<ExperienceDto>>(`${this.apiUrl}/api/admin/experiences`, data);
  }

  updateExperience(id: string, data: UpdateExperienceRequest): Observable<ApiResponse<ExperienceDto>> {
    return this.http.put<ApiResponse<ExperienceDto>>(`${this.apiUrl}/api/admin/experiences/${id}`, data);
  }

  deleteExperience(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/admin/experiences/${id}`);
  }

  setExperienceVisibility(id: string, isVisible: boolean): Observable<ApiResponse<ExperienceDto>> {
    return this.http.patch<ApiResponse<ExperienceDto>>(`${this.apiUrl}/api/admin/experiences/${id}/visibility`, { isVisible });
  }

  reorderExperiences(items: ReorderItem[]): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.apiUrl}/api/admin/experiences/reorder`, { items });
  }

  // ==========================================
  // 5. Projects
  // ==========================================
  getProjects(): Observable<ApiResponse<ProjectDto[]>> {
    return this.http.get<ApiResponse<ProjectDto[]>>(`${this.apiUrl}/api/admin/projects`);
  }

  createProject(data: CreateProjectRequest): Observable<ApiResponse<ProjectDto>> {
    return this.http.post<ApiResponse<ProjectDto>>(`${this.apiUrl}/api/admin/projects`, data);
  }

  updateProject(id: string, data: UpdateProjectRequest): Observable<ApiResponse<ProjectDto>> {
    return this.http.put<ApiResponse<ProjectDto>>(`${this.apiUrl}/api/admin/projects/${id}`, data);
  }

  deleteProject(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/admin/projects/${id}`);
  }

  setProjectVisibility(id: string, isVisible: boolean): Observable<ApiResponse<ProjectDto>> {
    return this.http.patch<ApiResponse<ProjectDto>>(`${this.apiUrl}/api/admin/projects/${id}/visibility`, { isVisible });
  }

  setProjectFeatured(id: string, isFeatured: boolean): Observable<ApiResponse<ProjectDto>> {
    return this.http.patch<ApiResponse<ProjectDto>>(`${this.apiUrl}/api/admin/projects/${id}/featured`, { isFeatured });
  }

  reorderProjects(items: ReorderItem[]): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.apiUrl}/api/admin/projects/reorder`, { items });
  }

  // ==========================================
  // 6. Educations
  // ==========================================
  getEducations(): Observable<ApiResponse<EducationDto[]>> {
    return this.http.get<ApiResponse<EducationDto[]>>(`${this.apiUrl}/api/admin/educations`);
  }

  createEducation(data: CreateEducationRequest): Observable<ApiResponse<EducationDto>> {
    return this.http.post<ApiResponse<EducationDto>>(`${this.apiUrl}/api/admin/educations`, data);
  }

  updateEducation(id: string, data: UpdateEducationRequest): Observable<ApiResponse<EducationDto>> {
    return this.http.put<ApiResponse<EducationDto>>(`${this.apiUrl}/api/admin/educations/${id}`, data);
  }

  deleteEducation(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/admin/educations/${id}`);
  }

  setEducationVisibility(id: string, isVisible: boolean): Observable<ApiResponse<EducationDto>> {
    return this.http.patch<ApiResponse<EducationDto>>(`${this.apiUrl}/api/admin/educations/${id}/visibility`, { isVisible });
  }

  reorderEducations(items: ReorderItem[]): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.apiUrl}/api/admin/educations/reorder`, { items });
  }

  // ==========================================
  // 7. Certifications
  // ==========================================
  getCertifications(): Observable<ApiResponse<CertificationDto[]>> {
    return this.http.get<ApiResponse<CertificationDto[]>>(`${this.apiUrl}/api/admin/certifications`);
  }

  createCertification(data: CreateCertificationRequest): Observable<ApiResponse<CertificationDto>> {
    return this.http.post<ApiResponse<CertificationDto>>(`${this.apiUrl}/api/admin/certifications`, data);
  }

  updateCertification(id: string, data: UpdateCertificationRequest): Observable<ApiResponse<CertificationDto>> {
    return this.http.put<ApiResponse<CertificationDto>>(`${this.apiUrl}/api/admin/certifications/${id}`, data);
  }

  deleteCertification(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/admin/certifications/${id}`);
  }

  setCertificationVisibility(id: string, isVisible: boolean): Observable<ApiResponse<CertificationDto>> {
    return this.http.patch<ApiResponse<CertificationDto>>(`${this.apiUrl}/api/admin/certifications/${id}/visibility`, { isVisible });
  }

  reorderCertifications(items: ReorderItem[]): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.apiUrl}/api/admin/certifications/reorder`, { items });
  }

  // ==========================================
  // 8. Social Links
  // ==========================================
  getSocialLinks(): Observable<ApiResponse<SocialLinkDto[]>> {
    return this.http.get<ApiResponse<SocialLinkDto[]>>(`${this.apiUrl}/api/admin/social-links`);
  }

  createSocialLink(data: CreateSocialLinkRequest): Observable<ApiResponse<SocialLinkDto>> {
    return this.http.post<ApiResponse<SocialLinkDto>>(`${this.apiUrl}/api/admin/social-links`, data);
  }

  updateSocialLink(id: string, data: UpdateSocialLinkRequest): Observable<ApiResponse<SocialLinkDto>> {
    return this.http.put<ApiResponse<SocialLinkDto>>(`${this.apiUrl}/api/admin/social-links/${id}`, data);
  }

  deleteSocialLink(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/admin/social-links/${id}`);
  }

  setSocialLinkVisibility(id: string, isVisible: boolean): Observable<ApiResponse<SocialLinkDto>> {
    return this.http.patch<ApiResponse<SocialLinkDto>>(`${this.apiUrl}/api/admin/social-links/${id}/visibility`, { isVisible });
  }

  reorderSocialLinks(items: ReorderItem[]): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.apiUrl}/api/admin/social-links/reorder`, { items });
  }

  // ==========================================
  // 9. Contact Messages
  // ==========================================
  getContactMessages(): Observable<ApiResponse<PaginatedResult<ContactMessageDto>>> {
    return this.http.get<ApiResponse<PaginatedResult<ContactMessageDto>>>(`${this.apiUrl}/api/admin/contact-messages`);
  }

  markMessageAsRead(id: string): Observable<ApiResponse<ContactMessageDto>> {
    return this.http.patch<ApiResponse<ContactMessageDto>>(`${this.apiUrl}/api/admin/contact-messages/${id}/read`, {});
  }

  markMessageAsUnread(id: string): Observable<ApiResponse<ContactMessageDto>> {
    return this.http.patch<ApiResponse<ContactMessageDto>>(`${this.apiUrl}/api/admin/contact-messages/${id}/unread`, {});
  }

  deleteContactMessage(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/admin/contact-messages/${id}`);
  }

  // ==========================================
  // 10. Profile & Bio Management
  // ==========================================
  getProfile(): Observable<{ succeeded: boolean; data: any; message?: string }> {
    return this.http.get<{ succeeded: boolean; data: any; message?: string }>(`${this.apiUrl}/api/profile`);
  }

  updateProfile(data: any): Observable<{ succeeded: boolean; data: any; message?: string }> {
    return this.http.put<{ succeeded: boolean; data: any; message?: string }>(`${this.apiUrl}/api/profile`, data);
  }

  // ==========================================
  // 11. Image Upload
  // ==========================================
  uploadImage(file: File): Observable<{ succeeded: boolean; url: string; relativeUrl: string; fileName: string }> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<{ succeeded: boolean; url: string; relativeUrl: string; fileName: string }>(
      `${this.apiUrl}/api/upload/image`,
      formData
    );
  }

  // ==========================================
  // 12. Batch Skills Creation
  // ==========================================
  createBatchSkills(data: { skillCategoryId: string; skillNames: string[]; defaultIcon?: string }): Observable<ApiResponse<SkillDto[]>> {
    return this.http.post<ApiResponse<SkillDto[]>>(`${this.apiUrl}/api/admin/skills/batch`, data);
  }
}
