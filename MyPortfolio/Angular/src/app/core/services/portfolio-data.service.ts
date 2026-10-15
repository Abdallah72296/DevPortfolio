import { environment } from '../../../environments/environment';
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map, shareReplay, switchMap } from 'rxjs/operators';
import {
  PersonalBio,
  SkillItem,
  ExperienceItem,
  ProjectItem,
  EducationItem,
  CertificationItem,
  ContactMessageDto
} from '../models/portfolio.models';

export interface PortfolioDashboardSummary {
  profileName: string | null;
  profileTitle: string | null;
  currentExperience: string | null;
  skillNames: string[];
  projectCount: number;
  experienceCount: number;
  skillCount: number;
  educationAndCertificationCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class PortfolioDataService {

  private readonly bio: PersonalBio = {
    fullName: 'Abdallah Abdeltawab',
    firstName: 'Abdallah',
    lastName: 'Abdeltawab',
    title: '.NET Backend Developer',
    headlineRole: '.NET Backend Developer',
    badge: '.NET BACKEND DEVELOPER • CLEAN ARCHITECTURE',
    location: 'Giza, Egypt (Remote & Worldwide)',
    cardCaption: '.NET Backend Developer • Giza, Egypt',
    email: 'abdallahabdeltawab6@gmail.com',
    availableForHire: true,
    yearsOfExperience: '3+',
    summary: '.NET Backend Developer with professional and freelance experience building backend systems using C#, ASP.NET Core Web API, EF Core, Clean Architecture, CQRS, and RabbitMQ. Experienced in financial, banking, project management, and business systems.',
    longBio: 'I am a passionate .NET Backend Developer with professional and freelance experience building robust enterprise backend architectures. With deep expertise in C#, ASP.NET Core Web API, EF Core, Clean Architecture, CQRS, and RabbitMQ, I specialize in financial transaction processing, external banking integrations, asynchronous workflows, and high-performance relational databases. Military Service: Completed.',
    paragraphs: [
      '.NET Backend Developer with professional and freelance experience building robust enterprise backend systems using C#, ASP.NET Core Web API, EF Core, Clean Architecture, CQRS, and RabbitMQ.',
      'Experienced in financial, banking, project management, and business systems, with hands-on experience in RESTful API development, external banking integrations, asynchronous workflows, authentication, and relational databases.',
      'Proven track record delivering mission-critical applications at Jedra and international freelance clients, including RabbitMQ-based pipelines and financial transaction aggregation engines. Military Service: Completed.'
    ],
    avatarUrl: 'assets/images/developer_avatar.jpg',
    cvUrl: '#contact',
    socialLinks: [
      { platform: 'email', url: 'mailto:abdallahabdeltawab6@gmail.com', icon: 'bi-envelope-fill', label: 'Email' },
      { platform: 'linkedin', url: 'https://linkedin.com/in/abdallah-abdeltawab-54b58b226', icon: 'bi-linkedin', label: 'LinkedIn' },
      { platform: 'github', url: 'https://github.com/Abdallah72296', icon: 'bi-github', label: 'GitHub' },
      { platform: 'phone', url: 'tel:+201002215065', icon: 'bi-telephone-fill', label: '+20 1002215065' }
    ],
    highlights: [
      { number: '20+', label: 'RESTful APIs Shipped', description: 'Engineered with Clean Architecture & CQRS' },
      { number: 'A+', label: 'Graduation Project', description: 'Bustan Mall - Enterprise Marketplace' },
      { number: '100%', label: 'Military Service', description: 'Completed with full honor' },
      { number: '2+', label: 'Banking & Financial Systems', description: 'Integrated with RabbitMQ & external APIs' }
    ]
  };

  private readonly skills: SkillItem[] = [
    // Languages & Frameworks
    { name: 'C#', category: 'Languages & Frameworks', icon: 'bi-filetype-cs', description: 'Modern language features, LINQ, Async/Await, OOP', featured: true },
    { name: 'ASP.NET Core Web API', category: 'Languages & Frameworks', icon: 'bi-gear-wide-connected', description: 'High-performance REST APIs, Middleware & DI', featured: true },
    { name: 'EF Core', category: 'Languages & Frameworks', icon: 'bi-database-fill-gear', description: 'Advanced mappings, query tuning & migrations', featured: true },
    { name: 'LINQ', category: 'Languages & Frameworks', icon: 'bi-filter-square', description: 'Complex relational querying & collection manipulation', featured: true },
    { name: 'TypeScript & JavaScript', category: 'Languages & Frameworks', icon: 'bi-filetype-tsx', description: 'Modern typed frontend integration', featured: false },
    { name: 'Python & C++', category: 'Languages & Frameworks', icon: 'bi-code-square', description: 'Object-oriented programming and problem solving', featured: false },
    { name: 'HTML5 & CSS3', category: 'Languages & Frameworks', icon: 'bi-filetype-css', description: 'Semantic markup, styling and web layouts', featured: false },

    // Databases & Messaging
    { name: 'SQL Server', category: 'Databases & Messaging', icon: 'bi-database', description: 'Relational database design, indexes, stored procedures', featured: true },
    { name: 'PostgreSQL', category: 'Databases & Messaging', icon: 'bi-database-check', description: 'Reliable open-source relational database management', featured: true },
    { name: 'RabbitMQ', category: 'Databases & Messaging', icon: 'bi-arrow-left-right', description: 'Asynchronous event-driven messaging pipelines', featured: true },
    { name: 'Redis', category: 'Databases & Messaging', icon: 'bi-lightning-charge-fill', description: 'In-memory distributed caching and performance boost', featured: true },
    { name: 'Firebase', category: 'Databases & Messaging', icon: 'bi-fire', description: 'Cloud messaging, notification workflows & services', featured: false },

    // Architecture & Concepts
    { name: 'Clean Architecture', category: 'Architecture & Concepts', icon: 'bi-diagram-3-fill', description: 'Decoupled domain models & separation of concerns', featured: true },
    { name: 'CQRS & MediatR', category: 'Architecture & Concepts', icon: 'bi-arrow-repeat', description: 'Segregated command and query responsibilities', featured: true },
    { name: 'SOLID & Design Patterns', category: 'Architecture & Concepts', icon: 'bi-layers-half', description: 'Repository Pattern, Unit of Work, maintainable design', featured: true },
    { name: 'System Design & Distributed Systems', category: 'Architecture & Concepts', icon: 'bi-grid-1x2-fill', description: 'Scalable backend services and decoupled workflows', featured: true },

    // Security & Integration
    { name: 'JWT & ASP.NET Core Identity', category: 'Security & Integration', icon: 'bi-shield-lock-fill', description: 'Token-based authentication & role-based access control', featured: true },
    { name: 'RESTful APIs', category: 'Security & Integration', icon: 'bi-hdd-network', description: 'Standard HTTP methods, status codes, documentation', featured: true },
    { name: 'Google Calendar API', category: 'Security & Integration', icon: 'bi-calendar-check', description: 'External third-party schedule & event integration', featured: false },
    { name: 'FluentValidation', category: 'Security & Integration', icon: 'bi-check2-circle', description: 'Strong validation pipelines for request DTOs', featured: false },

    // Tools & DevOps
    { name: 'Git & GitHub', category: 'Tools & DevOps', icon: 'bi-git', description: 'Version control, branching strategies & CI/CD collaboration', featured: true },
    { name: 'Docker', category: 'Tools & DevOps', icon: 'bi-box-seam', description: 'Containerization of ASP.NET Core APIs and SQL Server', featured: true },
    { name: 'Automated Testing', category: 'Tools & DevOps', icon: 'bi-check-all', description: 'Unit testing and backend verification pipelines', featured: false }
  ];

  private readonly experiences: ExperienceItem[] = [
    {
      id: 'exp-1',
      role: 'Backend Developer (.NET)',
      company: 'Jedra',
      location: 'Remote, Full-time',
      period: 'Dec. 2024 – Dec. 2025',
      isCurrent: true,
      type: 'Full-time',
      description: 'Engineered backend services for financial transaction analysis, integrating multiple external banking APIs and automating transaction processing workflows with ASP.NET Core Web API.',
      achievements: [
        'Engineered backend services for financial transaction analysis, integrating multiple external banking APIs and automating transaction processing workflows with ASP.NET Core Web API.',
        'Implemented an asynchronous RabbitMQ-based pipeline that decoupled transaction processing from the main API flow, improving reliability and scalability under load.',
        'Designed and implemented 20+ RESTful APIs using Clean Architecture and CQRS for business and project management systems.'
      ],
      technologies: ['C#', 'ASP.NET Core Web API', 'Clean Architecture', 'CQRS', 'RabbitMQ', 'EF Core', 'SQL Server', 'Banking APIs']
    },
    {
      id: 'exp-2',
      role: 'Coding Instructor',
      company: 'Udacity, Al Mentor, iSchool',
      location: 'Remote, Part-time',
      period: 'Aug. 2024 – Mar. 2025',
      isCurrent: false,
      type: 'Part-time',
      description: 'Delivered technical sessions on backend development, software engineering principles, and problem-solving methodologies.',
      achievements: [
        'Delivered technical sessions on backend development, software engineering principles, and problem-solving methodologies.',
        'Mentored students on real-world programming, Clean Code, and industry development practices.'
      ],
      technologies: ['C#', 'OOP', 'Data Structures', 'Algorithms', 'ASP.NET Core', 'Software Engineering Principles']
    }
  ];

  private readonly projects: ProjectItem[] = [
    {
      id: 'proj-1',
      title: 'Social Development Bank',
      description: 'Engineered financial analysis services for transaction aggregation, spending categorization, and account analytics, integrating multiple external banking APIs.',
      technologies: ['ASP.NET Core Web API', 'CQRS', 'RabbitMQ', 'Firebase', 'Banking APIs'],
      imageUrl: 'assets/images/project_preview_1.png',
      demoUrl: '#contact',
      githubUrl: 'https://github.com/Abdallah72296',
      featured: true,
      category: 'Enterprise Financial System',
      impact: 'Jedra – Saudi Arabia'
    },
    {
      id: 'proj-2',
      title: 'MaintenanceVisits',
      description: 'Built and delivered an end-to-end maintenance management system for Gulf Emergency Safety Tools and Materials, streamlining inspections, technical reporting, and spare-part quotation workflows. Designed and implemented RESTful APIs using Clean Architecture and CQRS.',
      technologies: ['ASP.NET Core Web API', 'Clean Architecture', 'CQRS', 'EF Core', 'SQL Server'],
      imageUrl: 'assets/images/project_preview_2.png',
      demoUrl: '#contact',
      githubUrl: 'https://github.com/Abdallah72296',
      featured: true,
      category: 'Business & Field Service Management',
      impact: 'Freelance – Gulf Emergency Safety Tools'
    },
    {
      id: 'proj-3',
      title: 'Project Management System',
      description: 'Engineered backend services for end-to-end project and task lifecycle management, including authorization, notification workflows, and Google Calendar integration.',
      technologies: ['ASP.NET Core Web API', 'JWT', 'Google Calendar API', 'CQRS', 'SQL Server'],
      imageUrl: 'assets/images/project_preview_3.png',
      demoUrl: '#contact',
      githubUrl: 'https://github.com/Abdallah72296',
      featured: true,
      category: 'Enterprise Productivity',
      impact: 'Jedra'
    },
    {
      id: 'proj-4',
      title: 'Bustan Mall',
      description: 'Designed and built a full-stack marketplace platform supporting store management, product publishing, and online ordering workflows. Graded A+ with highest honors.',
      technologies: ['ASP.NET Core Web API', 'Flutter', 'React', 'SQL Server'],
      imageUrl: 'assets/images/project_preview_1.png',
      demoUrl: '#contact',
      githubUrl: 'https://github.com/Abdallah72296',
      featured: true,
      category: 'E-Commerce Marketplace',
      impact: 'Graduation Project (Grade A+)'
    }
  ];

  private readonly education: EducationItem[] = [
    {
      id: 'edu-1',
      degree: "Bachelor's Degree in Computer Science & Information Technology",
      institution: 'Sinai University',
      location: 'Egypt',
      period: 'Oct. 2020 – Jul. 2024',
      grade: 'Graduation Project: Grade A+',
      details: 'Graduation Project graded A+ (Bustan Mall - Enterprise multi-vendor marketplace platform). Comprehensive study of Algorithms, Data Structures, Software Engineering, Database Systems, and Distributed Computing.',
      description: 'Graduation Project graded A+ (Bustan Mall - Enterprise multi-vendor marketplace platform). Comprehensive study of Algorithms, Data Structures, Software Engineering, Database Systems, and Distributed Computing.',
      honors: 'Graduation Project graded A+'
    }
  ];

  private readonly certifications: CertificationItem[] = [
    {
      id: 'cert-1',
      name: 'Military Service: Completed',
      issuer: 'Egyptian Armed Forces',
      issueYear: 'Completed',
      icon: 'bi-shield-fill-check'
    },
    {
      id: 'cert-2',
      name: 'Languages: Arabic (Native) & English (Professional)',
      issuer: 'Bilingual Proficiency',
      issueYear: 'Fluent',
      icon: 'bi-translate'
    },
    {
      id: 'cert-3',
      name: 'Graduation Project: Graded A+',
      issuer: 'Sinai University Faculty of Computer Science',
      issueYear: '2024',
      icon: 'bi-award-fill'
    }
  ];

  private readonly http = inject(HttpClient);
  private get apiUrl(): string {
    return environment.apiUrl;
  }

  private getUsernameFromUrl(): string | null {
    if (typeof window === 'undefined') return null;
    const path = window.location.pathname;
    const match = path.match(/\/(?:u|portfolio)\/([a-zA-Z0-9_\-\.]+)/i);
    if (match && match[1]) return match[1];
    const params = new URLSearchParams(window.location.search);
    return params.get('u');
  }

  private readonly portfolioData$ = of(null).pipe(
    map(() => this.getUsernameFromUrl()),
    switchMap(username => {
      if (!username) return of(null);
      const url = `${this.apiUrl}/api/portfolio/${encodeURIComponent(username)}`;
      return this.http.get<any>(url).pipe(
        catchError(() => of(null))
      );
    }),
    shareReplay(1)
  );

  getPublicPortfolio(username?: string): Observable<any> {
    if (!username) return of(null);
    return this.http.get<any>(`${this.apiUrl}/api/portfolio/${encodeURIComponent(username)}`);
  }

  getMobilePortfolioPreview(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/api/portfolio/mobile-preview`);
  }

  getDashboardSummary(): Observable<PortfolioDashboardSummary | null> {
    return this.getMobilePortfolioPreview().pipe(
      map(response => {
        const portfolio = response?.data;
        if (!portfolio) return null;

        return {
          profileName: portfolio.profile?.fullName ?? null,
          profileTitle: portfolio.profile?.headlineRole || portfolio.profile?.title || null,
          currentExperience: portfolio.experiences?.[0]
            ? `${portfolio.experiences[0].jobTitle} @ ${portfolio.experiences[0].companyName}`
            : null,
          skillNames: (portfolio.skills ?? []).flatMap((category: any) =>
            (category.skills ?? []).map((skill: any) => skill.name)
          ),
          projectCount: portfolio.projects?.length ?? 0,
          experienceCount: portfolio.experiences?.length ?? 0,
          skillCount: (portfolio.skills ?? []).reduce(
            (total: number, category: any) => total + (category.skills?.length ?? 0), 0
          ),
          educationAndCertificationCount: (portfolio.education?.length ?? 0) + (portfolio.certifications?.length ?? 0)
        };
      })
    );
  }

  getBio(): Observable<PersonalBio> {
    return this.portfolioData$.pipe(
      map(res => {
        const p = res?.data?.profile;
        if (!p) return this.bio;
        return {
          ...this.bio,
          fullName: p.fullName || this.bio.fullName,
          title: p.title || this.bio.title,
          headlineRole: p.headlineRole || p.title || this.bio.headlineRole,
          badge: p.badge || this.bio.badge,
          location: p.location || this.bio.location,
          cardCaption: p.cardCaption || `${p.title} • ${p.location}`,
          email: p.email || this.bio.email,
          phone: p.phone || this.bio.phone,
          summary: p.summary || this.bio.summary,
          longBio: p.longBio || this.bio.longBio,
          paragraphs: p.paragraphs?.length ? p.paragraphs : this.bio.paragraphs,
          avatarUrl: p.avatarUrl || this.bio.avatarUrl,
          cvUrl: p.cvUrl || this.bio.cvUrl,
          yearsOfExperience: p.yearsOfExperience || this.bio.yearsOfExperience,
          availableForHire: p.availableForHire ?? true
        };
      })
    );
  }

  getSkills(): Observable<SkillItem[]> {
    return this.portfolioData$.pipe(
      map(res => {
        const cats = res?.data?.skills;
        if (!cats || cats.length === 0) return this.skills;
        const allSkills: SkillItem[] = [];
        cats.forEach((cat: any) => {
          (cat.skills || []).forEach((s: any) => {
            allSkills.push({
              name: s.name,
              category: cat.name,
              icon: s.icon || 'bi-check-circle-fill',
              description: '',
              featured: false
            });
          });
        });
        return allSkills.length > 0 ? allSkills : this.skills;
      })
    );
  }

  getExperiences(): Observable<ExperienceItem[]> {
    return this.portfolioData$.pipe(
      map(res => {
        const list = res?.data?.experiences;
        if (!list || list.length === 0) return this.experiences;
        return list.map((e: any, idx: number) => {
          const startDate = e.startDate ? new Date(e.startDate).toLocaleDateString('en-US', { month: 'short', year: 'numeric' }) : '';
          const endDate = e.isCurrent ? 'Present' : (e.endDate ? new Date(e.endDate).toLocaleDateString('en-US', { month: 'short', year: 'numeric' }) : 'Present');
          const period = startDate ? `${startDate} – ${endDate}` : (e.period || '2024 – Present');

          const achievements = e.description
            ? e.description.split(/(?<=[.!?])\s+/).filter((s: string) => s.trim().length > 10)
            : [];

          return {
            id: e.id || `exp-${idx}`,
            role: e.jobTitle,
            company: e.companyName,
            location: e.location || 'Remote',
            period: period,
            isCurrent: e.isCurrent,
            description: e.description,
            achievements: achievements.length > 0 ? achievements : [e.description],
            technologies: e.technologies || []
          };
        });
      })
    );
  }

  getProjects(): Observable<ProjectItem[]> {
    return this.portfolioData$.pipe(
      map(res => {
        const list = res?.data?.projects;
        if (!list || list.length === 0) return this.projects;
        return list.map((p: any, idx: number) => ({
          id: p.id || `proj-${idx}`,
          title: p.title,
          description: p.description,
          technologies: p.technologies || [],
          imageUrl: p.image || (idx % 2 === 0 ? 'assets/images/project_preview_1.png' : 'assets/images/project_preview_2.png'),
          demoUrl: p.liveUrl || '#contact',
          githubUrl: p.githubUrl || 'https://github.com/Abdallah72296',
          featured: p.isFeatured ?? false,
          category: 'Enterprise Backend System',
          impact: p.technologies?.slice(0, 3).join(', ') || ''
        }));
      })
    );
  }

  getEducation(): Observable<EducationItem[]> {
    return this.portfolioData$.pipe(
      map(res => {
        const list = res?.data?.education;
        if (!list || list.length === 0) return this.education;
        return list.map((ed: any, idx: number) => {
          const startYear = ed.startDate ? new Date(ed.startDate).getFullYear().toString() : '2020';
          const endYear = ed.endDate ? new Date(ed.endDate).getFullYear().toString() : '2024';
          return {
            id: ed.id || `edu-${idx}`,
            degree: ed.degree,
            institution: ed.institutionName,
            period: `${startYear} — ${endYear}`,
            field: ed.fieldOfStudy || 'Computer Science',
            grade: ed.grade || 'Graduation Project Graded A+',
            details: ed.description || ''
          };
        });
      })
    );
  }

  getCertifications(): Observable<CertificationItem[]> {
    return this.portfolioData$.pipe(
      map(res => {
        const list = res?.data?.certifications;
        if (!list || list.length === 0) return this.certifications;
        return list.map((c: any, idx: number) => ({
          id: c.id || `cert-${idx}`,
          name: c.name,
          issuer: c.issuingOrganization,
          issueYear: c.issueDate ? new Date(c.issueDate).getFullYear().toString() : '2024',
          credentialUrl: c.credentialUrl || '',
          icon: c.icon || 'bi-award-fill'
        }));
      })
    );
  }

  sendContactMessage(dto: Pick<ContactMessageDto, 'name' | 'email' | 'subject' | 'message'>, username?: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<any>(`${this.apiUrl}/api/contact`, {
      name: dto.name,
      email: dto.email,
      subject: dto.subject,
      message: dto.message,
      username
    }).pipe(
      map(() => ({
        success: true,
        message: 'Thank you! Your message has been received. The portfolio owner will get in touch shortly.'
      }))
    );
  }
}
