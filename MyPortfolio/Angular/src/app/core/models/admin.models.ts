export interface ApiResponse<T> {
  data: T;
  succeeded: boolean;
  message: string;
  errors: string[];
  pagination?: { pageNumber: number; pageSize: number; totalCount: number; totalPages: number };
  summary?: { totalUsers: number; activeUsers: number; lockedUsers: number; adminUsers: number; standardUsers: number; availableForHireUsers: number };
}

export interface PaginatedResult<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface AuthResponseDto {
  accessToken: string;
  expiresAt: string;
  email: string;
  roles: string[];
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface PortfolioSectionDto {
  id: string;
  key: string;
  title: string;
  displayOrder: number;
  isVisible: boolean;
  isSystemSection: boolean;
}

export interface CreatePortfolioSectionRequest {
  key: string;
  title: string;
  displayOrder: number;
}

export interface UpdatePortfolioSectionRequest {
  title: string;
  displayOrder: number;
}

export interface ReorderItem {
  id: string;
  displayOrder: number;
}

export interface SkillCategoryDto {
  id: string;
  name: string;
  displayOrder: number;
  isVisible: boolean;
  skills?: SkillDto[];
}

export interface CreateSkillCategoryRequest {
  name: string;
  displayOrder: number;
}

export interface UpdateSkillCategoryRequest {
  name: string;
  displayOrder: number;
}

export interface SkillDto {
  id: string;
  name: string;
  icon: string | null;
  displayOrder: number;
  isVisible: boolean;
  skillCategoryId: string;
}

export interface CreateSkillRequest {
  name: string;
  icon: string | null;
  displayOrder: number;
  skillCategoryId: string;
}

export interface UpdateSkillRequest {
  name: string;
  icon: string | null;
  displayOrder: number;
}

export interface ExperienceDto {
  id: string;
  jobTitle: string;
  companyName: string;
  description: string;
  startDate: string;
  endDate: string | null;
  isCurrent: boolean;
  displayOrder: number;
  isVisible: boolean;
}

export interface CreateExperienceRequest {
  jobTitle: string;
  companyName: string;
  description: string;
  startDate: string;
  endDate: string | null;
  isCurrent: boolean;
  displayOrder: number;
}

export interface UpdateExperienceRequest {
  jobTitle: string;
  companyName: string;
  description: string;
  startDate: string;
  endDate: string | null;
  isCurrent: boolean;
  displayOrder: number;
}

export interface ProjectDto {
  id: string;
  title: string;
  description: string;
  image: string | null;
  liveUrl: string | null;
  githubUrl: string | null;
  technologies: string[];
  isFeatured: boolean;
  displayOrder: number;
  isVisible: boolean;
}

export interface CreateProjectRequest {
  title: string;
  description: string;
  image: string | null;
  liveUrl: string | null;
  githubUrl: string | null;
  technologies: string[];
  isFeatured: boolean;
  displayOrder: number;
}

export interface UpdateProjectRequest {
  title: string;
  description: string;
  image: string | null;
  liveUrl: string | null;
  githubUrl: string | null;
  technologies: string[];
  isFeatured: boolean;
  displayOrder: number;
  isVisible: boolean;
}

export interface EducationDto {
  id: string;
  institutionName: string;
  degree: string;
  fieldOfStudy: string | null;
  description: string | null;
  startDate: string;
  endDate: string | null;
  isCurrent: boolean;
  displayOrder: number;
  isVisible: boolean;
}

export interface CreateEducationRequest {
  institutionName: string;
  degree: string;
  fieldOfStudy: string | null;
  description: string | null;
  startDate: string;
  endDate: string | null;
  isCurrent: boolean;
  displayOrder: number;
}

export interface UpdateEducationRequest {
  institutionName: string;
  degree: string;
  fieldOfStudy: string | null;
  description: string | null;
  startDate: string;
  endDate: string | null;
  isCurrent: boolean;
  displayOrder: number;
}

export interface CertificationDto {
  id: string;
  name: string;
  issuingOrganization: string;
  issueDate: string;
  credentialId: string | null;
  credentialUrl: string | null;
  description: string | null;
  displayOrder: number;
  isVisible: boolean;
}

export interface CreateCertificationRequest {
  name: string;
  issuingOrganization: string;
  issueDate: string;
  credentialId: string | null;
  credentialUrl: string | null;
  description: string | null;
  displayOrder: number;
}

export interface UpdateCertificationRequest {
  name: string;
  issuingOrganization: string;
  issueDate: string;
  credentialId: string | null;
  credentialUrl: string | null;
  description: string | null;
  displayOrder: number;
}

export interface SocialLinkDto {
  id: string;
  platform: string;
  url: string;
  icon: string | null;
  displayOrder: number;
  isVisible: boolean;
}

export interface PortfolioSiteSettingsDto {
  themeKey: string;
  customDomain: string;
  seoTitle: string;
  seoDescription: string;
  seoKeywords: string;
  isPublished: boolean;
  publishedAt: string | null;
}

export interface AdminUserDto {
  id: string;
  email: string;
  userName: string;
  roles: string[];
  lockoutEnd: string | null;
  fullName: string;
  portfolioTitle: string;
  location: string;
  phone: string;
  availableForHire: boolean;
}

export interface CreateSocialLinkRequest {
  platform: string;
  url: string;
  icon: string | null;
  displayOrder: number;
}

export interface UpdateSocialLinkRequest {
  platform: string;
  url: string;
  icon: string | null;
  displayOrder: number;
}

export interface ContactMessageDto {
  id: string;
  name: string;
  email: string;
  subject: string;
  message: string;
  isRead: boolean;
  readAt: string | null;
  createdAt: string;
}

export interface PortfolioAnalyticsSummary {
  monthlyPortfolioViews: number;
  monthlyCvDownloads: number;
  monthlyContactMessages: number;
  unreadContactMessages: number;
  totalContactMessages: number;
  periodStartUtc: string;
}

// ==========================================
// CV Portfolio Draft Extracted by Gemini
// ==========================================

export interface CvPersonalInfoDraft {
  name: string | null;
  jobTitle: string | null;
  about: string | null;
  email: string | null;
  phone: string | null;
  location: string | null;
}

export interface RegistrationResponseDto {
  email: string;
  confirmationEmailSent: boolean;
}

export interface CvSkillDraft {
  name: string;
  icon: string | null;
}

export interface CvSkillCategoryDraft {
  category: string;
  skills: CvSkillDraft[];
}

export interface CvExperienceDraft {
  jobTitle: string;
  companyName: string;
  description: string | null;
  startDate: string | null;
  endDate: string | null;
  isCurrent: boolean;
}

export interface CvProjectDraft {
  title: string;
  description: string | null;
  image: string | null;
  liveUrl: string | null;
  githubUrl: string | null;
  technologies: string[];
  isFeatured: boolean;
}

export interface CvEducationDraft {
  institutionName: string;
  degree: string;
  fieldOfStudy: string | null;
  description: string | null;
  startDate: string | null;
  endDate: string | null;
  isCurrent: boolean;
}

export interface CvCertificationDraft {
  name: string;
  issuingOrganization: string;
  issueDate: string | null;
  credentialId: string | null;
  credentialUrl: string | null;
  description: string | null;
}

export interface CvSocialLinkDraft {
  platform: string;
  url: string;
}

export interface CvPortfolioDraft {
  personalInfo: CvPersonalInfoDraft | null;
  skills: CvSkillCategoryDraft[];
  experiences: CvExperienceDraft[];
  projects: CvProjectDraft[];
  education: CvEducationDraft[];
  certifications: CvCertificationDraft[];
  socialLinks: CvSocialLinkDraft[];
}

// UI Draft Item wrappers allowing edit and save per item
export interface EditableDraftExperience extends CreateExperienceRequest {
  draftId: string;
  isDraft: boolean;
  saving?: boolean;
  saved?: boolean;
}

export interface EditableDraftProject extends CreateProjectRequest {
  draftId: string;
  isDraft: boolean;
  saving?: boolean;
  saved?: boolean;
}

export interface EditableDraftSkill {
  draftId: string;
  name: string;
  icon: string | null;
  categoryName: string;
  isDraft: boolean;
  saving?: boolean;
  saved?: boolean;
}

export interface EditableDraftEducation extends CreateEducationRequest {
  draftId: string;
  isDraft: boolean;
  saving?: boolean;
  saved?: boolean;
}

export interface EditableDraftCertification extends CreateCertificationRequest {
  draftId: string;
  isDraft: boolean;
  saving?: boolean;
  saved?: boolean;
}

export interface EditableDraftSocialLink extends CreateSocialLinkRequest {
  draftId: string;
  isDraft: boolean;
  saving?: boolean;
  saved?: boolean;
}

export interface UserProfileDto {
  id?: string;
  fullName: string;
  title: string;
  headlineRole?: string;
  badge?: string;
  location?: string;
  cardCaption?: string;
  email?: string;
  phone?: string;
  yearsOfExperience?: string;
  summary: string;
  longBio?: string;
  paragraphs?: string[];
  avatarUrl: string;
  cvUrl?: string;
  availableForHire: boolean;
}

export interface UpdateUserProfileRequest {
  fullName: string;
  title: string;
  headlineRole?: string;
  badge?: string;
  location?: string;
  cardCaption?: string;
  email?: string;
  phone?: string;
  yearsOfExperience?: string;
  summary: string;
  longBio?: string;
  paragraphs?: string[];
  avatarUrl: string;
  cvUrl?: string;
  availableForHire: boolean;
}

export interface BatchCreateSkillsRequest {
  skillCategoryId: string;
  skillNames: string[];
  defaultIcon?: string;
}

export interface UploadImageResponse {
  succeeded: boolean;
  url: string;
  relativeUrl: string;
  fileName: string;
}

