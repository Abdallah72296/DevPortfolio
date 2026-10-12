export interface PersonalBio {
  fullName: string;
  firstName: string;
  lastName: string;
  title: string;
  headlineRole: string;
  badge: string;
  location: string;
  cardCaption: string;
  email: string;
  phone?: string;
  availableForHire: boolean;
  yearsOfExperience: string;
  summary: string;
  longBio: string;
  paragraphs: string[];
  avatarUrl: string;
  cvUrl: string;
  socialLinks: Array<{ platform: string; url: string; icon: string; label: string }>;
  highlights: Array<{ number: string; label: string; description: string }>;
}

export interface SkillItem {
  name: string;
  category: string;
  icon: string;
  description: string;
  featured: boolean;
}

export interface ExperienceItem {
  id: string;
  role: string;
  company: string;
  location: string;
  period: string;
  isCurrent: boolean;
  type?: string;
  description: string;
  achievements: string[];
  technologies: string[];
}

export interface ProjectItem {
  id: string;
  title: string;
  description: string;
  technologies: string[];
  imageUrl: string;
  demoUrl: string;
  githubUrl: string;
  featured: boolean;
  category: string;
  impact: string;
}

export interface EducationItem {
  id: string;
  degree: string;
  institution: string;
  location: string;
  period: string;
  grade: string;
  details: string;
  description: string;
  honors: string;
}

export interface CertificationItem {
  id: string;
  name: string;
  issuer: string;
  issueYear: string;
  credentialUrl?: string;
  icon: string;
}

export interface ContactMessageDto {
  name: string;
  email: string;
  subject: string;
  message: string;
}
