import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

export interface ProjectItem {
  id: string;
  name: string;
  slug: string;
  description: string;
  category: 'distributed' | 'frontend' | 'cloud' | 'ai';
  status: 'published' | 'draft' | 'archived';
  featured: boolean;
  version: string;
  repo: string;
  demoUrl: string;
  stars: number;
  forks: number;
  qps: string;
  latency: string;
  tags: string[];
  images?: string[];
  topology?: {
    client: string;
    leader: string;
    followers: string[];
  };
}

export interface SkillNode {
  name: string;
  cluster: string;
  active?: boolean;
}

export interface WorkExperience {
  id: string;
  role: string;
  company: string;
  logo: string;
  period: string;
  location: string;
  type: string;
  description: string;
  achievements: string[];
  stack: string[];
  verified: boolean;
  public: boolean;
}

export interface UserAccount {
  id: string;
  name: string;
  email: string;
  role: string;
  status: 'ACTIVE' | 'INACTIVE' | 'SUSPENDED' | 'PENDING';
  subdomain: string;
  twoFactor?: boolean;
  lastActive?: string;
  avatar?: string;
  initials?: string;
  badge?: string;
  registeredDate?: string;
  portfolioStatus?: 'PUBLISHED' | 'DRAFT_SAVED' | 'EMPTY';
}

@Injectable({
  providedIn: 'root'
})
export class MockDataService {
  readonly profile = {
    name: 'Alex Rivera',
    initials: 'AR',
    title: 'Staff Distributed Systems Engineer',
    headline: 'Architecting ultra-low latency distributed state engines & real-time telemetry pipelines.',
    bio: 'Specialized in Raft consensus implementations, high-throughput asynchronous network runtimes (Tokio / Rust & Go), and zero-copy packet serialization. Currently leading core financial systems engineering at hyper-scale with provable data invariants.',
    location: 'San Francisco, CA • Remote',
    email: 'alex@rivera.systems',
    phone: '+1 (415) 890-2384',
    subdomain: 'alexrivera.devfolio.io',
    pgp: '4F91 2EAA 01BC 88DE C492',
    avatar: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCWSo5yr7tiYXoynZzxuNe6cGNb-TLNBFKhXvf05WwITb8juScmdL4YOfK6SS0UXQ5WX_myc6KmicYA_FiPzxrW11I6Sq2dAaeycfsmxnVDrHL6bIg3rnNhtNATOFlrWqm8ZttqSQrC2GUZa4fskfVbWGc5kjEHHhSMdNtEGe26Xoe2NVOmQn9C01ge36PUv8VGhXhAWR733uDWvX2puaN1cL1YYW_cmopDduxP-U46dseYcAOMRFX_vQ',
    heroPortrait: 'https://lh3.googleusercontent.com/aida-public/AB6AXuD3NVehCg38PhTAyNAYlJknpegckCH44-LXb36Ny6JQoA__371xKgAJWInxhgtGwSkDkn9_iZ03QzvqQsZAPCOjjAx7bZy6Y7zU3wWMKcO7yfyQdCwqKKHlzwuyULJRW3TMIu7J-khBondssFEwQ_Fo5qRe1bAnSWJXOTpeoTBj08N7VIQfoMHlsYARzIsGvIGbDdulgKBnm4lPlte9ZZW_j__cQkfe4VYbRtfIF5Xl7850M3OGsvRggA',
    mobilePortrait: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDvdqUkL3k5B97WdUZ4Q39890TpEQ9zvKT6jRZyZdlOkkBBaBwButR-Zq3692vfKcsBSarkT1XddgjR9XSq-vDXHenNy9_GBK8xp73hlBjKbUlKckihTCU5w_Ac7_LG5gj-bNwSBf8nzpKkChmlYc9PPh2xfPCvuz3CpTiGVphJ-1GtkwVd7NGiQ6LSD9CICMp3dixi8KmW4-TTv1su9C-z0_w56M_pl3upwWRi85Bo-s5jeN-JSyu5kA',
    logo: 'https://lh3.googleusercontent.com/aida/AEtjO1Wcvtnd2z1m_7xjzvtlsVkIru3-9ZtgEQwrtTGkgLu9thumgpYkH08_2dQpq7l5AzUgpMMYauXA3MUMbp9YNZgkZuZRADRLv7cru6pPlXLZVOmzN413r0I9FkfIHP0BDjakU5IAbdwA13IvQXFVGgf2PRTiP1S0x9KIingXI23PQEkjDHz5eeVuRuAIUs3Nppjr-AgtyeBHBkwXbW-Cqm3itTypXCeGvj5UEaD-uGV_VyLIQzMDaQSXkzen'
  };

  readonly projects: ProjectItem[] = [
    {
      id: 'proj-1',
      name: 'HyperScale KV Store',
      slug: 'hyperscale-kv',
      description: 'Distributed Raft-consensus key-value database built in Rust & Go with persistent WAL, snapshotting, and high-throughput zero-copy serialization.',
      category: 'distributed',
      status: 'published',
      featured: true,
      version: 'v2.4.1',
      repo: 'alexrivera/hyperscale-kv',
      demoUrl: 'https://kv.alexrivera.dev',
      stars: 1240,
      forks: 142,
      qps: '120k QPS',
      latency: '< 1.4ms p99',
      tags: ['Rust', 'Raft', 'gRPC', 'Docker'],
      images: [
        'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=800&q=80',
        'https://images.unsplash.com/photo-1551288049-bebda4e38f71?auto=format&fit=crop&w=800&q=80',
        'https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?auto=format&fit=crop&w=800&q=80'
      ],
      topology: {
        client: 'Client gRPC',
        leader: 'Leader (Raft)',
        followers: ['Follower #1', 'Follower #2']
      }
    },
    {
      id: 'proj-2',
      name: 'RaftConsensus Core',
      slug: 'raft-consensus-core',
      description: 'Deterministic Go Raft cluster algorithm library with chaos injection simulation suites and leader election heartbeat guarantees.',
      category: 'distributed',
      status: 'published',
      featured: true,
      version: 'v1.9.0',
      repo: 'alexrivera/raft-consensus-core',
      demoUrl: 'https://raft.alexrivera.dev',
      stars: 620,
      forks: 84,
      qps: '85k QPS',
      latency: '< 2.1ms p99',
      tags: ['Go', 'Raft', 'etcd', 'Protobuf'],
      images: [
        'https://images.unsplash.com/photo-1544197150-b99a580bb7a8?auto=format&fit=crop&w=800&q=80',
        'https://images.unsplash.com/photo-1504639725590-34d0984388bd?auto=format&fit=crop&w=800&q=80'
      ]
    },
    {
      id: 'proj-3',
      name: 'CloudNative Ingress Proxy',
      slug: 'cloudnative-ingress',
      description: 'Envoy-compatible programmable proxy controller providing automated zero-trust mutual TLS mTLS rotation and dynamic routing.',
      category: 'cloud',
      status: 'published',
      featured: false,
      version: 'v3.1.2',
      repo: 'alexrivera/cloudnative-ingress',
      demoUrl: 'https://ingress.alexrivera.dev',
      stars: 480,
      forks: 56,
      qps: '350k QPS',
      latency: '0.8ms p99',
      tags: ['Go', 'eBPF', 'Envoy Gateway', 'K8s'],
      images: [
        'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=800&q=80'
      ]
    },
    {
      id: 'proj-4',
      name: 'ZeroCopy Packet Broker',
      slug: 'zerocopy-packet-broker',
      description: 'Ultra high-throughput network packet broker utilizing DPDK and eBPF kernel bypass for multi-gigabit streaming analytics.',
      category: 'distributed',
      status: 'draft',
      featured: false,
      version: 'v0.9.4',
      repo: 'alexrivera/zerocopy-broker',
      demoUrl: 'https://broker.alexrivera.dev',
      stars: 310,
      forks: 29,
      qps: '500k QPS',
      latency: '0.4ms p99',
      tags: ['C++', 'eBPF', 'DPDK', 'Linux'],
      images: [] // Explicitly no images
    },
    {
      id: 'proj-5',
      name: 'DevFolio Portfolio Engine',
      slug: 'devfolio-engine',
      description: 'High-performance developer portfolio builder powered by Angular 17 standalone components and real-time state synchronization.',
      category: 'frontend',
      status: 'published',
      featured: true,
      version: 'v2.4.0',
      repo: 'alexrivera/devfolio-engine',
      demoUrl: 'https://alexrivera.devfolio.io',
      stars: 890,
      forks: 110,
      qps: 'Edge CDN',
      latency: '14ms TTFB',
      tags: ['Angular 17', 'TypeScript', 'Tailwind CSS', 'SignalR'],
      images: [
        'https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?auto=format&fit=crop&w=800&q=80',
        'https://images.unsplash.com/photo-1507238691740-187a5b1d37b8?auto=format&fit=crop&w=800&q=80'
      ]
    }
  ];

  readonly experience: WorkExperience[] = [
    {
      id: 'exp-1',
      role: 'Staff Software Engineer',
      company: 'Stripe',
      logo: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCs9lpY3Yc6VrW044cF0D_QyK_DwYsMLcD174fDSq_4B2Pn0AxsPUTxd8yg5g-JOXG127qcE4D4ccBDaRiJUiqn6Z54MEzNECm6SHhfg9RPWUOwNKq8EuExeZABlx1mn1tUkEXwBOdrKaXYMnXMzGAkDxNanbbZFavfTsKrtx53qeaEFXvHECLPcXq2Rc6RKmk-MNe7Kc8a2bi0Vvi2wVKYkP4v-rNa_jE-ueKVuAihO9QBFOYBEySl7w',
      period: '2021 — Present (3 yrs 4 mos)',
      location: 'San Francisco, CA (Remote)',
      type: 'FULL-TIME',
      description: 'Technical Lead for Core Payment Routing engine handling over $850B+ in annualized gross merchandise value. Architected distributed fault-tolerant idempotency tiers across multi-region deployments with sub-5ms p99 SLA guarantees.',
      achievements: [
        'Reduced routing failover latency by 41% across EMEA/APAC cloud regions during peak holiday spikes.',
        'Authored unified ledger migration consensus RFC, safely migrating 120M active accounts with 0 downtime.'
      ],
      stack: ['Go', 'Kafka Streams', 'CockroachDB', 'AWS EKS', 'gRPC'],
      verified: true,
      public: true
    },
    {
      id: 'exp-2',
      role: 'Senior Software Engineer',
      company: 'Datadog',
      logo: 'https://lh3.googleusercontent.com/aida-public/AB6AXuACkTVCdikrnCoL3DDeN_5hl7dVPBNmJL6FvBKCPa0Xe0FZbHlvlHfxmL8_wNvZGUx4kUgmrNRx-yA_NeE0Piv-b1K6XdWcK4Z6axlkBsfQ3XPhXeimMR_BIm9cvjM5d2GIJHISlvOAki_d12cta9gHvC-e-bnc66vrAcYx4xKUho2BYk1L2ZIwOfD_B-xn_LkyeDN06X5BrMgrBOjWeHc-tIILfVkN0y_E3iSEnWTw3qHFjQ_47tRCKA',
      period: '2018 — 2021 (3 yrs 2 mos)',
      location: 'New York, NY',
      type: 'FULL-TIME',
      description: 'Scaled high-throughput APM distributed tracing pipeline processing 45M+ spans per second. Engineered ring buffer aggregators and zero-allocation serialization format reducing node CPU overhead significantly.',
      achievements: [
        'Cut cluster infrastructure cost by $1.8M annually by revamping trace tail-sampling algorithms.',
        'Promoted to Core Ingestion Architecture Working Group to standardize telemetry standards.'
      ],
      stack: ['Rust', 'Go', 'Kubernetes', 'Cassandra', 'OpenTelemetry'],
      verified: true,
      public: true
    },
    {
      id: 'exp-3',
      role: 'Systems Software Engineer',
      company: 'CloudScale Technologies',
      logo: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCs9lpY3Yc6VrW044cF0D_QyK_DwYsMLcD174fDSq_4B2Pn0AxsPUTxd8yg5g-JOXG127qcE4D4ccBDaRiJUiqn6Z54MEzNECm6SHhfg9RPWUOwNKq8EuExeZABlx1mn1tUkEXwBOdrKaXYMnXMzGAkDxNanbbZFavfTsKrtx53qeaEFXvHECLPcXq2Rc6RKmk-MNe7Kc8a2bi0Vvi2wVKYkP4v-rNa_jE-ueKVuAihO9QBFOYBEySl7w',
      period: '2016 — 2018 (2 yrs)',
      location: 'Austin, TX',
      type: 'FULL-TIME',
      description: 'Built high-concurrency event brokers and container orchestration scheduling primitives. Implemented Linux cgroups resource limits and network policy enforcement.',
      achievements: [
        'Maintained 99.999% availability for mission-critical customer cluster controllers.',
        'Engineered automated zero-downtime rolling upgrades for 400+ production hosts.'
      ],
      stack: ['C++', 'Linux', 'Docker', 'PostgreSQL', 'Redis'],
      verified: true,
      public: true
    }
  ];

  readonly skillClusters = [
    {
      id: 'c1',
      name: 'Languages & Runtimes',
      color: 'secondary',
      tag: 'CLUSTER_01',
      skills: ['C# (.NET 8)', 'TypeScript', 'Rust', 'Go (Golang)', 'Python', 'PostgreSQL / SQL']
    },
    {
      id: 'c2',
      name: 'Architecture & Cloud',
      color: 'primary',
      tag: 'CLUSTER_02',
      skills: ['Distributed Systems', 'Kubernetes (K8s)', 'AWS (EKS / Lambda)', 'Apache Kafka', 'Microservices Arch', 'Redis Cache Clusters']
    },
    {
      id: 'c3',
      name: 'Frontend & UI Systems',
      color: 'tertiary',
      tag: 'CLUSTER_03',
      skills: ['Angular (Signals & RxJS)', 'Tailwind CSS & Design Systems', 'Web Components & Shadow DOM', 'Accessibility (WCAG 2.2 AAA)']
    },
    {
      id: 'c4',
      name: 'DevOps & Tooling',
      color: 'secondary-fixed',
      tag: 'CLUSTER_04',
      skills: ['Docker & Containerization', 'GitHub Actions CI/CD', 'Terraform (IaC)', 'OpenTelemetry', 'Grafana / Prometheus']
    }
  ];

  readonly users: UserAccount[] = [
    {
      id: 'user_1',
      name: 'Alex Rivera',
      badge: 'STAFF',
      email: 'alex@rivera.systems',
      registeredDate: 'Reg: Oct 14, 2024',
      subdomain: 'alexrivera.devfolio.io',
      portfolioStatus: 'PUBLISHED',
      status: 'ACTIVE',
      role: 'Staff Distributed Systems Engineer',
      twoFactor: true,
      lastActive: '2m ago',
      avatar: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCSEf3pyCGMKOaofiihiRCQCOJwFcUGOFfv_zRbsL5bdBfYSkwZC4Blk0bQ5jtE-s19WlJGPnbc4vwvH6bCMozHPYr8AfCkSAiyzIwmYiDSBngeAuKgVKiBFyJTlrlFQeBJPLz9OKnV6UvY0z3pvXqt7OR3Wluelr1y1wDfqfpdZIrP2VcreWrJ-XLqPERTenCJr6-EmIe8HVLK0vf34kWRjBw7OEEjm2xH1edwNCql8IcuY_5yE3dDNQ'
    },
    {
      id: 'user_2',
      name: 'Elena Rostova',
      email: 'e.rostova@cloudtech.io',
      registeredDate: 'Reg: Sep 28, 2024',
      subdomain: 'elenarostova.devfolio.io',
      portfolioStatus: 'PUBLISHED',
      status: 'ACTIVE',
      role: 'Tech Lead / Principal Engineer',
      twoFactor: true,
      lastActive: '14m ago',
      avatar: 'https://lh3.googleusercontent.com/aida-public/AB6AXuANLdmWR6zMrtRQfk4EiKJiXrRe3l0jTeayhspmfdblXzdECZihe9LTGMRA5EHRze7EccgjCiSFEpmdwIvaG3fOOSVxKrSFlbFI5I0twNB_nxLO_fyGieb2XOdduYyAKQaYOWM7QWiC5x93m39yl6tpmv4aIfA5Obd0t10OV4w5ZrPX-56WAlplLaocYLuPktkYwdB0tM3_2UsoB-WlRQzSOZlmWo7jRbltsg7GBRNumx-KccjbAjCUhQ'
    },
    {
      id: 'user_3',
      name: 'Marcus Vance',
      initials: 'MV',
      email: 'mvance@distrib.org',
      registeredDate: 'Reg: Aug 12, 2024',
      subdomain: 'marcus-vance.devfolio.io',
      portfolioStatus: 'DRAFT_SAVED',
      status: 'ACTIVE',
      role: 'Systems Architect',
      twoFactor: false,
      lastActive: '1d ago'
    },
    {
      id: 'user_4',
      name: 'Priya Sharma',
      email: 'priya.sharma@alum.mit.edu',
      registeredDate: 'Reg: Jul 03, 2024',
      subdomain: 'priyasharma.devfolio.io',
      portfolioStatus: 'PUBLISHED',
      status: 'ACTIVE',
      role: 'AI Software Architect',
      twoFactor: true,
      lastActive: '3h ago',
      avatar: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDaH0Fn-2WEaiDUXpR2CB8l5kMwDP1BxNFsCpWePxEbB7qhqDT27-0y0pM7lclkZ26ndevZ2Xh1Z13vV0dwb7KC_fnbeEPJm7VhFhB4w08-CNpzlNWUfHDnFAlZy241wzd1_nELTCnL1CiBAUry8V1XjsDq5xJ1yyZ4AwrxOP81t2WIAcYvefyvkXGZuUT4y7Ugl-M_zNtyea_Rl4mYU6sn8j2HGhxbvAQ_R1V2BtZdLL9OoyJ01tejug'
    },
    {
      id: 'user_5',
      name: 'Jordan Lee',
      initials: 'JL',
      email: 'jlee.code@gmail.com',
      registeredDate: 'Reg: May 19, 2024',
      subdomain: '<NO_DOMAIN>',
      portfolioStatus: 'EMPTY',
      status: 'INACTIVE',
      role: 'Associate Engineer',
      twoFactor: false,
      lastActive: '5d ago'
    }
  ];

  getUsers(): Observable<UserAccount[]> {
    return of(this.users);
  }

  getProjects(): Observable<ProjectItem[]> {
    return of(this.projects);
  }

  getExperience(): Observable<WorkExperience[]> {
    return of(this.experience);
  }

  sendContactMessage(name: string, email: string, message: string): Observable<boolean> {
    return of(true);
  }
}
