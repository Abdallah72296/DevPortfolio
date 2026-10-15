/**
 * Skill Registry & Auto-Icon Service
 * Provides rich catalog of 130+ tech skills, real-time search & autocomplete,
 * auto-icon detection (Devicon, Material, custom image), and dynamic learning/caching.
 */

export interface CatalogSkill {
  name: string;
  category: string;
  icon: string;
  iconType: 'devicon' | 'material' | 'image';
  color?: string;
}

export const BASE_SKILL_CATALOG: CatalogSkill[] = [
  // Backend Systems
  { name: 'C#', category: 'Backend Systems', icon: 'devicon-csharp-plain colored', iconType: 'devicon', color: '#9b4993' },
  { name: '.NET', category: 'Backend Systems', icon: 'devicon-dotnetcore-plain colored', iconType: 'devicon', color: '#512bd4' },
  { name: 'ASP.NET Core', category: 'Backend Systems', icon: 'devicon-dotnetcore-plain colored', iconType: 'devicon', color: '#512bd4' },
  { name: 'Entity Framework Core', category: 'Backend Systems', icon: 'devicon-dotnetcore-plain colored', iconType: 'devicon', color: '#512bd4' },
  { name: 'Rust', category: 'Backend Systems', icon: 'devicon-rust-plain colored', iconType: 'devicon', color: '#dea584' },
  { name: 'Go / Golang', category: 'Backend Systems', icon: 'devicon-go-original-wordmark colored', iconType: 'devicon', color: '#00add8' },
  { name: 'Python', category: 'Backend Systems', icon: 'devicon-python-plain colored', iconType: 'devicon', color: '#3776ab' },
  { name: 'Django', category: 'Backend Systems', icon: 'devicon-django-plain colored', iconType: 'devicon', color: '#092e20' },
  { name: 'FastAPI', category: 'Backend Systems', icon: 'devicon-fastapi-plain colored', iconType: 'devicon', color: '#05998b' },
  { name: 'Flask', category: 'Backend Systems', icon: 'devicon-flask-original colored', iconType: 'devicon', color: '#000000' },
  { name: 'Java', category: 'Backend Systems', icon: 'devicon-java-plain colored', iconType: 'devicon', color: '#b07219' },
  { name: 'Spring Boot', category: 'Backend Systems', icon: 'devicon-spring-plain colored', iconType: 'devicon', color: '#6db33f' },
  { name: 'Node.js', category: 'Backend Systems', icon: 'devicon-nodejs-plain colored', iconType: 'devicon', color: '#339933' },
  { name: 'Express.js', category: 'Backend Systems', icon: 'devicon-express-original colored', iconType: 'devicon', color: '#ffffff' },
  { name: 'NestJS', category: 'Backend Systems', icon: 'devicon-nestjs-plain colored', iconType: 'devicon', color: '#ea2845' },
  { name: 'PHP', category: 'Backend Systems', icon: 'devicon-php-plain colored', iconType: 'devicon', color: '#777bb4' },
  { name: 'Laravel', category: 'Backend Systems', icon: 'devicon-laravel-plain colored', iconType: 'devicon', color: '#ff2d20' },
  { name: 'C++', category: 'Backend Systems', icon: 'devicon-cplusplus-plain colored', iconType: 'devicon', color: '#f34b7d' },
  { name: 'C', category: 'Backend Systems', icon: 'devicon-c-plain colored', iconType: 'devicon', color: '#555555' },
  { name: 'Ruby', category: 'Backend Systems', icon: 'devicon-ruby-plain colored', iconType: 'devicon', color: '#cc342d' },
  { name: 'Rails', category: 'Backend Systems', icon: 'devicon-rails-plain colored', iconType: 'devicon', color: '#cc0000' },
  { name: 'Scala', category: 'Backend Systems', icon: 'devicon-scala-plain colored', iconType: 'devicon', color: '#dc322f' },

  // Frontend & Mobile
  { name: 'Angular', category: 'Frontend', icon: 'devicon-angularjs-plain colored', iconType: 'devicon', color: '#dd0031' },
  { name: 'React', category: 'Frontend', icon: 'devicon-react-original colored', iconType: 'devicon', color: '#61dafb' },
  { name: 'Vue.js', category: 'Frontend', icon: 'devicon-vuejs-plain colored', iconType: 'devicon', color: '#4fc08d' },
  { name: 'TypeScript', category: 'Frontend', icon: 'devicon-typescript-plain colored', iconType: 'devicon', color: '#3178c6' },
  { name: 'JavaScript', category: 'Frontend', icon: 'devicon-javascript-plain colored', iconType: 'devicon', color: '#f7df1e' },
  { name: 'HTML5', category: 'Frontend', icon: 'devicon-html5-plain colored', iconType: 'devicon', color: '#e34f26' },
  { name: 'CSS3', category: 'Frontend', icon: 'devicon-css3-plain colored', iconType: 'devicon', color: '#1572b6' },
  { name: 'Tailwind CSS', category: 'Frontend', icon: 'devicon-tailwindcss-plain colored', iconType: 'devicon', color: '#06b6d4' },
  { name: 'Bootstrap', category: 'Frontend', icon: 'devicon-bootstrap-plain colored', iconType: 'devicon', color: '#7952b3' },
  { name: 'Sass / SCSS', category: 'Frontend', icon: 'devicon-sass-original colored', iconType: 'devicon', color: '#cc6699' },
  { name: 'Next.js', category: 'Frontend', icon: 'devicon-nextjs-plain colored', iconType: 'devicon', color: '#ffffff' },
  { name: 'Nuxt', category: 'Frontend', icon: 'devicon-nuxtjs-plain colored', iconType: 'devicon', color: '#00c58e' },
  { name: 'Svelte', category: 'Frontend', icon: 'devicon-svelte-plain colored', iconType: 'devicon', color: '#ff3e00' },
  { name: 'Flutter', category: 'Mobile', icon: 'devicon-flutter-plain colored', iconType: 'devicon', color: '#02569b' },
  { name: 'Dart', category: 'Mobile', icon: 'devicon-dart-plain colored', iconType: 'devicon', color: '#0175c2' },
  { name: 'Swift', category: 'Mobile', icon: 'devicon-swift-plain colored', iconType: 'devicon', color: '#f05138' },
  { name: 'Android / Kotlin', category: 'Mobile', icon: 'devicon-android-plain colored', iconType: 'devicon', color: '#3ddc84' },
  { name: 'Redux', category: 'Frontend', icon: 'devicon-redux-original colored', iconType: 'devicon', color: '#764abc' },

  // Databases & Storage
  { name: 'SQL Server', category: 'Databases', icon: 'devicon-microsoftsqlserver-plain colored', iconType: 'devicon', color: '#cc292b' },
  { name: 'PostgreSQL', category: 'Databases', icon: 'devicon-postgresql-plain colored', iconType: 'devicon', color: '#336791' },
  { name: 'MySQL', category: 'Databases', icon: 'devicon-mysql-plain colored', iconType: 'devicon', color: '#4479a1' },
  { name: 'MongoDB', category: 'Databases', icon: 'devicon-mongodb-plain colored', iconType: 'devicon', color: '#47a248' },
  { name: 'Redis', category: 'Databases', icon: 'devicon-redis-plain colored', iconType: 'devicon', color: '#dc382d' },
  { name: 'SQLite', category: 'Databases', icon: 'devicon-sqlite-plain colored', iconType: 'devicon', color: '#003b57' },
  { name: 'Oracle', category: 'Databases', icon: 'devicon-oracle-original colored', iconType: 'devicon', color: '#f80000' },
  { name: 'Elasticsearch', category: 'Databases', icon: 'devicon-elasticsearch-plain colored', iconType: 'devicon', color: '#005571' },
  { name: 'Firebase', category: 'Databases', icon: 'devicon-firebase-plain colored', iconType: 'devicon', color: '#ffca28' },
  { name: 'Supabase', category: 'Databases', icon: 'devicon-supabase-plain colored', iconType: 'devicon', color: '#3ecf8e' },

  // Distributed Systems & Messaging
  { name: 'Kafka', category: 'Distributed Systems', icon: 'devicon-apachekafka-original colored', iconType: 'devicon', color: '#231f20' },
  { name: 'RabbitMQ', category: 'Distributed Systems', icon: 'devicon-rabbitmq-plain colored', iconType: 'devicon', color: '#ff6600' },
  { name: 'GraphQL', category: 'Distributed Systems', icon: 'devicon-graphql-plain colored', iconType: 'devicon', color: '#e10098' },
  { name: 'gRPC', category: 'Distributed Systems', icon: 'devicon-googlecloud-plain colored', iconType: 'devicon', color: '#244c5a' },
  { name: 'REST APIs', category: 'Distributed Systems', icon: 'dns', iconType: 'material', color: '#4cd7f6' },
  { name: 'Microservices', category: 'Distributed Systems', icon: 'hub', iconType: 'material', color: '#a078ff' },
  { name: 'Raft Consensus', category: 'Distributed Systems', icon: 'sync_alt', iconType: 'material', color: '#4edea3' },
  { name: 'SignalR', category: 'Distributed Systems', icon: 'sensors', iconType: 'material', color: '#512bd4' },
  { name: 'WebSockets', category: 'Distributed Systems', icon: 'cable', iconType: 'material', color: '#03b5d3' },

  // DevOps & Cloud
  { name: 'Docker', category: 'DevOps & Infra', icon: 'devicon-docker-plain colored', iconType: 'devicon', color: '#2496ed' },
  { name: 'Kubernetes', category: 'DevOps & Infra', icon: 'devicon-kubernetes-plain colored', iconType: 'devicon', color: '#326ce5' },
  { name: 'AWS', category: 'DevOps & Infra', icon: 'devicon-amazonwebservices-original colored', iconType: 'devicon', color: '#ff9900' },
  { name: 'Azure', category: 'DevOps & Infra', icon: 'devicon-azure-plain colored', iconType: 'devicon', color: '#0089d6' },
  { name: 'Google Cloud (GCP)', category: 'DevOps & Infra', icon: 'devicon-googlecloud-plain colored', iconType: 'devicon', color: '#4285f4' },
  { name: 'Git', category: 'DevOps & Infra', icon: 'devicon-git-plain colored', iconType: 'devicon', color: '#f05032' },
  { name: 'GitHub', category: 'DevOps & Infra', icon: 'devicon-github-original colored', iconType: 'devicon', color: '#ffffff' },
  { name: 'GitLab', category: 'DevOps & Infra', icon: 'devicon-gitlab-plain colored', iconType: 'devicon', color: '#fc6d26' },
  { name: 'Terraform', category: 'DevOps & Infra', icon: 'devicon-terraform-plain colored', iconType: 'devicon', color: '#7b42bc' },
  { name: 'Ansible', category: 'DevOps & Infra', icon: 'devicon-ansible-plain colored', iconType: 'devicon', color: '#ee0000' },
  { name: 'Jenkins', category: 'DevOps & Infra', icon: 'devicon-jenkins-plain colored', iconType: 'devicon', color: '#d24939' },
  { name: 'Linux', category: 'DevOps & Infra', icon: 'devicon-linux-plain colored', iconType: 'devicon', color: '#fcc624' },
  { name: 'Ubuntu', category: 'DevOps & Infra', icon: 'devicon-ubuntu-plain colored', iconType: 'devicon', color: '#e95420' },
  { name: 'Nginx', category: 'DevOps & Infra', icon: 'devicon-nginx-original colored', iconType: 'devicon', color: '#009639' },
  { name: 'Apache', category: 'DevOps & Infra', icon: 'devicon-apache-plain colored', iconType: 'devicon', color: '#d22128' },
  { name: 'CI/CD Pipelines', category: 'DevOps & Infra', icon: 'sync', iconType: 'material', color: '#4edea3' },

  // Tools & Observability
  { name: 'Prometheus', category: 'Observability', icon: 'devicon-prometheus-original colored', iconType: 'devicon', color: '#e6522c' },
  { name: 'Grafana', category: 'Observability', icon: 'devicon-grafana-plain colored', iconType: 'devicon', color: '#f46800' },
  { name: 'OpenTelemetry', category: 'Observability', icon: 'visibility', iconType: 'material', color: '#4cd7f6' },
  { name: 'eBPF Tracing', category: 'Observability', icon: 'monitor_heart', iconType: 'material', color: '#acedff' },
  { name: 'Postman', category: 'Tools', icon: 'devicon-postman-plain colored', iconType: 'devicon', color: '#ff6c37' },
  { name: 'Swagger', category: 'Tools', icon: 'devicon-swagger-plain colored', iconType: 'devicon', color: '#85ea2d' },
  { name: 'Figma', category: 'Tools', icon: 'devicon-figma-plain colored', iconType: 'devicon', color: '#f24e1e' },
  { name: 'Jira', category: 'Tools', icon: 'devicon-jira-plain colored', iconType: 'devicon', color: '#0052cc' },
  { name: 'Trello', category: 'Tools', icon: 'devicon-trello-plain colored', iconType: 'devicon', color: '#0079bf' },
  { name: 'Webpack', category: 'Tools', icon: 'devicon-webpack-plain colored', iconType: 'devicon', color: '#8dd6f9' },
  { name: 'Vite', category: 'Tools', icon: 'devicon-vitejs-plain colored', iconType: 'devicon', color: '#646cff' },
  { name: 'Jest', category: 'Tools', icon: 'devicon-jest-plain colored', iconType: 'devicon', color: '#c21325' },
  // Architecture & Concepts
  { name: 'Clean Architecture', category: 'Architecture', icon: 'layers', iconType: 'material', color: '#4edea3' },
  { name: 'CQRS', category: 'Architecture', icon: 'sync', iconType: 'material', color: '#4cd7f6' },
  { name: 'MediatR', category: 'Architecture', icon: 'hub', iconType: 'material', color: '#a078ff' },
  { name: 'SOLID Principles', category: 'Architecture', icon: 'verified', iconType: 'material', color: '#4edea3' },
  { name: 'Repository Pattern', category: 'Architecture', icon: 'inventory_2', iconType: 'material', color: '#4cd7f6' },
  { name: 'Unit of Work', category: 'Architecture', icon: 'task_alt', iconType: 'material', color: '#4edea3' },
  { name: 'System Design', category: 'Architecture', icon: 'architecture', iconType: 'material', color: '#d0bcff' },
  { name: 'Distributed Systems', category: 'Architecture', icon: 'device_hub', iconType: 'material', color: '#4cd7f6' },
  { name: 'LINQ', category: 'Backend Systems', icon: 'filter_alt', iconType: 'material', color: '#512bd4' },
  { name: 'EF Core', category: 'Backend Systems', icon: 'devicon-dotnetcore-plain colored', iconType: 'devicon', color: '#512bd4' },
  { name: 'Google Calendar API', category: 'Tools', icon: 'calendar_month', iconType: 'material', color: '#4285f4' }
];

const LEARNED_SKILLS_STORAGE_KEY = 'devfolio_learned_skills_v1';

function getLearnedSkills(): CatalogSkill[] {
  try {
    const raw = localStorage.getItem(LEARNED_SKILLS_STORAGE_KEY);
    if (raw) {
      const parsed: CatalogSkill[] = JSON.parse(raw);
      return parsed.map(s => s.icon === 'alt_route' ? { ...s, icon: 'sync' } : s);
    }
  } catch {}
  return [];
}

function saveLearnedSkills(skills: CatalogSkill[]): void {
  try {
    localStorage.setItem(LEARNED_SKILLS_STORAGE_KEY, JSON.stringify(skills));
  } catch {}
}

export function getAllKnownSkills(): CatalogSkill[] {
  const learned = getLearnedSkills();
  const map = new Map<string, CatalogSkill>();

  BASE_SKILL_CATALOG.forEach(s => map.set(s.name.toLowerCase(), s));
  learned.forEach(s => map.set(s.name.toLowerCase(), s));

  return Array.from(map.values());
}

export function searchSkillCatalog(query: string): CatalogSkill[] {
  if (!query || !query.trim()) return getAllKnownSkills().slice(0, 15);
  const q = query.trim().toLowerCase();

  const all = getAllKnownSkills();
  const startsWith = all.filter(s => s.name.toLowerCase().startsWith(q));
  const contains = all.filter(s => !s.name.toLowerCase().startsWith(q) && (s.name.toLowerCase().includes(q) || s.category.toLowerCase().includes(q)));

  return [...startsWith, ...contains].slice(0, 12);
}

export function resolveSkillIcon(skillName?: string | null, customIcon?: string | null): string {
  if (customIcon && customIcon.trim()) return customIcon.trim();
  if (!skillName || !skillName.trim()) return 'code';

  const clean = skillName.trim().toLowerCase();

  // 1. Exact match in catalog
  const all = getAllKnownSkills();
  const exact = all.find(s => s.name.toLowerCase() === clean);
  if (exact) return exact.icon;

  // 2. Keyword/substring heuristics (ignore short names <= 3 chars to avoid matching 'c', 'go', 'r', etc. inside random words)
  const match = all.find(s => {
    const sName = s.name.toLowerCase();
    if (sName.length <= 3) {
      // Word boundary match only
      const regex = new RegExp(`\\b${sName.replace('+', '\\+')}\\b`, 'i');
      return regex.test(clean);
    }
    return clean.includes(sName) || sName.includes(clean);
  });
  if (match) return match.icon;

  // 3. Category/concept heuristics
  if (clean.includes('sql') || clean.includes('db') || clean.includes('data')) return 'storage';
  if (clean.includes('cloud') || clean.includes('server') || clean.includes('infra')) return 'cloud';
  if (clean.includes('security') || clean.includes('auth') || clean.includes('crypto')) return 'shield';
  if (clean.includes('api') || clean.includes('microservice')) return 'hub';
  if (clean.includes('calendar') || clean.includes('event')) return 'calendar_month';
  if (clean.includes('test') || clean.includes('qa')) return 'verified';
  if (clean.includes('design') || clean.includes('ui') || clean.includes('ux')) return 'palette';
  if (clean.includes('ai') || clean.includes('ml') || clean.includes('model')) return 'psychology';
  if (clean.includes('arch') || clean.includes('pattern') || clean.includes('solid')) return 'layers';

  return 'code';
}

export function registerLearnedSkill(name: string, icon: string, category: string = 'Technical Skills'): void {
  if (!name || !name.trim()) return;
  const cleanName = name.trim();
  const current = getLearnedSkills();

  const existingIdx = current.findIndex(s => s.name.toLowerCase() === cleanName.toLowerCase());
  const skillEntry: CatalogSkill = {
    name: cleanName,
    category,
    icon: icon || resolveSkillIcon(cleanName),
    iconType: (icon?.startsWith('http') || icon?.startsWith('data:') || icon?.startsWith('/uploads')) ? 'image' : (icon?.includes('devicon-') ? 'devicon' : 'material')
  };

  if (existingIdx >= 0) {
    current[existingIdx] = skillEntry;
  } else {
    current.push(skillEntry);
  }

  saveLearnedSkills(current);
}

export interface SkillIconDisplay {
  type: 'image' | 'devicon' | 'material';
  value: string;
}

export function getSkillIconDisplay(skillOrName: any, customIcon?: string | null): SkillIconDisplay {
  const name = typeof skillOrName === 'object' ? skillOrName?.name : skillOrName;
  const rawIcon = (typeof skillOrName === 'object' ? skillOrName?.icon : customIcon) || customIcon;

  let icon = resolveSkillIcon(name, rawIcon);
  if (icon === 'alt_route') icon = 'sync';

  if (icon.startsWith('http') || icon.startsWith('data:') || icon.startsWith('/uploads') || icon.startsWith('blob:')) {
    return { type: 'image', value: icon };
  }

  if (icon.includes('devicon-')) {
    if (!icon.includes('colored')) {
      icon = `${icon} colored`;
    }
    return { type: 'devicon', value: icon };
  }

  if (icon.includes('bi-') || icon.includes('fa-')) {
    return { type: 'devicon', value: icon };
  }

  return { type: 'material', value: icon || 'code' };
}
