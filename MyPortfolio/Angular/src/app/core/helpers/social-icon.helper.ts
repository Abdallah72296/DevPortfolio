/**
 * Social Icon & Platform Resolver
 * Automatically resolves brand names, colors, official SVG logos, and Material Symbol icons
 * for any social platform name or URL string provided by the user.
 */

export interface SocialPlatformInfo {
  id: string;
  name: string;
  materialIcon: string;
  color: string;
  svgPath?: string;
}

export const PRESET_PLATFORMS: { id: string; name: string; icon: string; defaultUrlPrefix: string }[] = [
  { id: 'github', name: 'GitHub', icon: 'code', defaultUrlPrefix: 'https://github.com/' },
  { id: 'linkedin', name: 'LinkedIn', icon: 'badge', defaultUrlPrefix: 'https://linkedin.com/in/' },
  { id: 'x', name: 'X / Twitter', icon: 'alternate_email', defaultUrlPrefix: 'https://x.com/' },
  { id: 'telegram', name: 'Telegram', icon: 'send', defaultUrlPrefix: 'https://t.me/' },
  { id: 'discord', name: 'Discord', icon: 'forum', defaultUrlPrefix: 'https://discord.gg/' },
  { id: 'youtube', name: 'YouTube', icon: 'smart_display', defaultUrlPrefix: 'https://youtube.com/@' },
  { id: 'whatsapp', name: 'WhatsApp', icon: 'chat', defaultUrlPrefix: 'https://wa.me/' },
  { id: 'instagram', name: 'Instagram', icon: 'photo_camera', defaultUrlPrefix: 'https://instagram.com/' },
  { id: 'facebook', name: 'Facebook', icon: 'public', defaultUrlPrefix: 'https://facebook.com/' },
  { id: 'website', name: 'Website / Portfolio', icon: 'language', defaultUrlPrefix: 'https://' },
  { id: 'email', name: 'Email', icon: 'mail', defaultUrlPrefix: 'mailto:' },
  { id: 'medium', name: 'Medium', icon: 'article', defaultUrlPrefix: 'https://medium.com/@' },
  { id: 'devto', name: 'Dev.to', icon: 'terminal', defaultUrlPrefix: 'https://dev.to/' },
  { id: 'stackoverflow', name: 'Stack Overflow', icon: 'layers', defaultUrlPrefix: 'https://stackoverflow.com/users/' },
  { id: 'gitlab', name: 'GitLab', icon: 'source', defaultUrlPrefix: 'https://gitlab.com/' }
];

export function resolveSocialPlatform(platformOrUrl: string, explicitIcon?: string | null): SocialPlatformInfo {
  const input = `${platformOrUrl || ''} ${explicitIcon || ''}`.toLowerCase().trim();

  // 1. GitHub
  if (input.includes('github') || input.includes('gh')) {
    return {
      id: 'github',
      name: 'GitHub',
      materialIcon: 'code',
      color: '#ffffff',
      svgPath: 'M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0024 12c0-6.63-5.37-12-12-12z'
    };
  }

  // 2. LinkedIn
  if (input.includes('linkedin')) {
    return {
      id: 'linkedin',
      name: 'LinkedIn',
      materialIcon: 'badge',
      color: '#0a66c2',
      svgPath: 'M19 0h-14c-2.761 0-5 2.239-5 5v14c0 2.761 2.239 5 5 5h14c2.762 0 5-2.239 5-5v-14c0-2.761-2.238-5-5-5zm-11 19h-3v-11h3v11zm-1.5-12.268c-.966 0-1.75-.79-1.75-1.764s.784-1.764 1.75-1.764 1.75.79 1.75 1.764-.783 1.764-1.75 1.764zm13.5 12.268h-3v-5.604c0-3.368-4-3.113-4 0v5.604h-3v-11h3v1.765c1.396-2.586 7-2.777 7 2.476v6.759z'
    };
  }

  // 3. X / Twitter
  if (input.includes('twitter') || input.includes('x.com') || input === 'x' || input.includes('tweet')) {
    return {
      id: 'x',
      name: 'X (Twitter)',
      materialIcon: 'alternate_email',
      color: '#1da1f2',
      svgPath: 'M18.244 2.25h3.308l-7.227 8.26 8.502 11.24H16.17l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231zm-1.161 17.52h1.833L7.084 4.126H5.117z'
    };
  }

  // 4. Telegram
  if (input.includes('telegram') || input.includes('t.me')) {
    return {
      id: 'telegram',
      name: 'Telegram',
      materialIcon: 'send',
      color: '#229ed9',
      svgPath: 'M12 0C5.373 0 0 5.373 0 12s5.373 12 12 12 12-5.373 12-12S18.627 0 12 0zm5.894 8.221l-1.97 9.28c-.145.658-.537.818-1.084.508l-3-2.21-1.446 1.394c-.14.18-.357.295-.6.295-.002 0-.003 0-.005 0l.213-3.054 5.56-5.022c.24-.213-.054-.334-.373-.121l-6.869 4.326-2.96-.924c-.643-.204-.657-.643.136-.953l11.57-4.458c.538-.196 1.006.128.832.943z'
    };
  }

  // 5. Discord
  if (input.includes('discord')) {
    return {
      id: 'discord',
      name: 'Discord',
      materialIcon: 'forum',
      color: '#5865f2',
      svgPath: 'M20.317 4.37a19.791 19.791 0 0 0-4.885-1.515.074.074 0 0 0-.079.037c-.21.375-.444.864-.608 1.25a18.27 18.27 0 0 0-5.487 0 12.64 12.64 0 0 0-.617-1.25.077.077 0 0 0-.079-.037A19.736 19.736 0 0 0 3.677 4.37a.07.07 0 0 0-.032.027C.533 9.046-.32 13.58.099 18.057a.082.082 0 0 0 .031.057 19.9 19.9 0 0 0 5.993 3.03.078.078 0 0 0 .084-.028c.462-.63.874-1.295 1.226-1.994.021-.041.001-.09-.041-.106a13.107 13.107 0 0 1-1.872-.892.077.077 0 0 1-.008-.128 10.2 10.2 0 0 0 .372-.292.074.074 0 0 1 .077-.01c3.929 1.793 8.18 1.793 12.061 0a.074.074 0 0 1 .078.01c.12.098.246.198.373.292a.077.077 0 0 1-.006.127 12.299 12.299 0 0 1-1.873.893.077.077 0 0 0-.041.107c.36.698.772 1.362 1.225 1.993a.076.076 0 0 0 .084.028 19.839 19.839 0 0 0 6.002-3.03.077.077 0 0 0 .032-.054c.5-5.177-.838-9.674-3.549-13.66a.061.061 0 0 0-.031-.028z'
    };
  }

  // 6. YouTube
  if (input.includes('youtube') || input.includes('youtu.be')) {
    return {
      id: 'youtube',
      name: 'YouTube',
      materialIcon: 'smart_display',
      color: '#ff0000',
      svgPath: 'M23.498 6.186a3.016 3.016 0 0 0-2.122-2.136C19.505 3.545 12 3.545 12 3.545s-7.505 0-9.377.505A3.017 3.017 0 0 0 .502 6.186C0 8.07 0 12 0 12s0 3.93.502 5.814a3.016 3.016 0 0 0 2.122 2.136c1.871.505 9.376.505 9.376.505s7.505 0 9.377-.505a3.015 3.015 0 0 0 2.122-2.136C24 15.93 24 12 24 12s0-3.93-.502-5.814zM9.545 15.568V8.432L15.818 12l-6.273 3.568z'
    };
  }

  // 7. WhatsApp
  if (input.includes('whatsapp') || input.includes('wa.me')) {
    return {
      id: 'whatsapp',
      name: 'WhatsApp',
      materialIcon: 'chat',
      color: '#25d366',
      svgPath: 'M12.031 0C5.399 0 0 5.399 0 12.031c0 2.115.553 4.183 1.602 6.005L.051 24l6.125-1.606a11.968 11.968 0 0 0 5.855 1.524h.005c6.632 0 12.031-5.4 12.031-12.031 0-3.214-1.252-6.236-3.526-8.51A11.956 11.956 0 0 0 12.031 0zm0 22.029h-.004a9.98 9.98 0 0 1-5.086-1.393l-.365-.216-3.778.991 1.008-3.684-.238-.378a9.976 9.976 0 0 1-1.534-5.318C2.034 6.52 6.518 2.036 12.031 2.036c2.671 0 5.182 1.04 7.072 2.93 1.889 1.89 2.928 4.401 2.928 7.072 0 5.513-4.484 9.991-10 9.991z'
    };
  }

  // 8. Instagram
  if (input.includes('instagram')) {
    return {
      id: 'instagram',
      name: 'Instagram',
      materialIcon: 'photo_camera',
      color: '#e4405f',
      svgPath: 'M12 2.163c3.204 0 3.584.012 4.85.07 3.252.148 4.771 1.691 4.919 4.919.058 1.265.069 1.645.069 4.849 0 3.205-.012 3.584-.069 4.849-.149 3.225-1.664 4.771-4.919 4.919-1.266.058-1.644.07-4.85.07-3.204 0-3.584-.012-4.849-.07-3.26-.149-4.771-1.699-4.919-4.92-.058-1.265-.07-1.644-.07-4.849 0-3.204.013-3.583.07-4.849.149-3.227 1.664-4.771 4.919-4.919 1.266-.057 1.645-.069 4.849-.069zm0-2.163c-3.259 0-3.667.014-4.947.072-4.358.2-6.78 2.618-6.98 6.98-.059 1.281-.073 1.689-.073 4.948 0 3.259.014 3.668.072 4.948.2 4.358 2.618 6.78 6.98 6.98 1.281.058 1.689.072 4.948.072 3.259 0 3.668-.014 4.948-.072 4.354-.2 6.782-2.618 6.979-6.98.059-1.28.073-1.689.073-4.948 0-3.259-.014-3.667-.072-4.947-.196-4.354-2.617-6.78-6.979-6.98-1.281-.059-1.69-.073-4.949-.073zm0 5.838c-3.403 0-6.162 2.759-6.162 6.162s2.759 6.163 6.162 6.163 6.162-2.759 6.162-6.163c0-3.403-2.759-6.162-6.162-6.162zm0 10.162c-2.209 0-4-1.79-4-4 0-2.209 1.791-4 4-4s4 1.791 4 4c0 2.21-1.791 4-4 4zm6.406-11.845c-.796 0-1.441.645-1.441 1.44s.645 1.44 1.441 1.44c.795 0 1.439-.645 1.439-1.44s-.644-1.44-1.439-1.44z'
    };
  }

  // 9. Facebook
  if (input.includes('facebook') || input.includes('fb.com')) {
    return {
      id: 'facebook',
      name: 'Facebook',
      materialIcon: 'public',
      color: '#1877f2',
      svgPath: 'M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z'
    };
  }

  // 10. GitLab
  if (input.includes('gitlab')) {
    return {
      id: 'gitlab',
      name: 'GitLab',
      materialIcon: 'source',
      color: '#fc6d26',
      svgPath: 'M23.955 13.587l-1.342-4.135-2.664-8.189c-.135-.417-.724-.417-.859 0L16.425 9.45H7.575L4.91 1.263c-.135-.417-.724-.417-.859 0L1.387 9.452.045 13.587c-.114.352.008.74.305.955l11.65 8.463 11.65-8.463c.297-.215.419-.603.305-.955z'
    };
  }

  // 11. Dev.to
  if (input.includes('dev.to') || input.includes('devto')) {
    return {
      id: 'devto',
      name: 'Dev.to',
      materialIcon: 'terminal',
      color: '#ffffff',
      svgPath: 'M7.42 10.05c-.18-.16-.46-.23-.84-.23H5v4.35h1.58c.38 0 .66-.08.84-.23.18-.16.27-.42.27-.79v-2.31c0-.37-.09-.63-.27-.79zm5.33 0c-.18-.16-.46-.23-.84-.23h-1.58v4.35h1.58c.38 0 .66-.08.84-.23.18-.16.27-.42.27-.79v-2.31c0-.37-.09-.63-.27-.79zM21 3H3C1.9 3 1 3.9 1 5v14c0 1.1.9 2 2 2h18c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM8.88 12.36c0 .76-.23 1.34-.69 1.74-.46.4-1.11.6-1.96.6H3.6V9.3h2.63c.85 0 1.5.2 1.96.6.46.4.69.98.69 1.74v.72zm5.33 0c0 .76-.23 1.34-.69 1.74-.46.4-1.11.6-1.96.6h-2.63V9.3h2.63c.85 0 1.5.2 1.96.6.46.4.69.98.69 1.74v.72zm5.19-2.24h-3.8v1.34h2.4v1.2h-2.4v1.34h3.8V15.2h-5.2V8.8h5.2v1.32z'
    };
  }

  // 12. Stack Overflow
  if (input.includes('stackoverflow')) {
    return {
      id: 'stackoverflow',
      name: 'Stack Overflow',
      materialIcon: 'layers',
      color: '#f48024',
      svgPath: 'M18.986 21.865v-6.404h2.134V24H2.88v-8.539h2.134v6.404h13.972zM6.92 14.654l10.147 2.115.438-2.09L7.358 12.564l-.438 2.09zm1.758-5.34l9.267 4.708.975-1.921-9.267-4.708-.975 1.921zm3.805-5.01l7.464 7.234 1.488-1.564-7.464-7.234-1.488 1.564zm8.653-4.304l-1.921-.975-5.01 9.267 1.921.975 5.01-9.267zM7.001 19.539h10.428v-2.134H7.001v2.134z'
    };
  }

  // 13. Medium
  if (input.includes('medium')) {
    return {
      id: 'medium',
      name: 'Medium',
      materialIcon: 'article',
      color: '#00ab6c',
      svgPath: 'M13.54 12a6.8 6.8 0 0 1-6.77 6.82A6.8 6.8 0 0 1 0 12a6.8 6.8 0 0 1 6.77-6.82A6.8 6.8 0 0 1 13.54 12zM20.96 12c0 3.54-1.51 6.42-3.38 6.42-1.87 0-3.39-2.88-3.39-6.42s1.52-6.42 3.39-6.42 3.38 2.88 3.38 6.42M24 12c0 3.17-.53 5.75-1.19 5.75-.66 0-1.19-2.58-1.19-5.75s.53-5.75 1.19-5.75C23.47 6.25 24 8.83 24 12z'
    };
  }

  // 14. Email
  if (input.includes('mail') || input.includes('@')) {
    return {
      id: 'email',
      name: 'Email',
      materialIcon: 'mail',
      color: '#ea4335',
      svgPath: 'M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z'
    };
  }

  // 15. Phone
  if (input.includes('phone') || input.includes('tel') || input.includes('call')) {
    return {
      id: 'phone',
      name: 'Phone',
      materialIcon: 'call',
      color: '#34a853',
      svgPath: 'M6.62 10.79c1.44 2.83 3.76 5.14 6.59 6.59l2.2-2.2c.27-.27.67-.36 1.02-.24 1.12.37 2.33.57 3.57.57.55 0 1 .45 1 1V20c0 .55-.45 1-1 1-9.39 0-17-7.61-17-17 0-.55.45-1 1-1h3.5c.55 0 1 .45 1 1 0 1.25.2 2.45.57 3.57.11.35.03.74-.25 1.02l-2.2 2.2z'
    };
  }

  // 16. Website / Portfolio
  if (input.includes('portfolio') || input.includes('website') || input.includes('web') || input.includes('site') || input.includes('http')) {
    return {
      id: 'website',
      name: 'Website / Portfolio',
      materialIcon: 'language',
      color: '#4cd7f6',
      svgPath: 'M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1 17.93c-3.95-.49-7-3.85-7-7.93 0-.62.08-1.21.21-1.79L9 15v1c0 1.1.9 2 2 2v1.93zm6.9-2.54c-.26-.81-1-1.39-1.9-1.39h-1v-3c0-.55-.45-1-1-1H8v-2h2c.55 0 1-.45 1-1V7h2c1.1 0 2-.9 2-2v-.41c2.93 1.19 5 4.06 5 7.41 0 2.08-.8 3.97-2.1 5.39z'
    };
  }

  // Fallback
  return {
    id: 'link',
    name: platformOrUrl || 'Link',
    materialIcon: explicitIcon || 'link',
    color: '#958ea0'
  };
}

export function getSocialIconName(platform?: string, url?: string, icon?: string | null): string {
  return resolveSocialPlatform(`${platform || ''} ${url || ''}`, icon).materialIcon;
}

export function getSocialSvg(platformOrLink?: any, url?: string, icon?: string | null): string | null {
  if (!platformOrLink) return null;
  const platform = typeof platformOrLink === 'object' ? platformOrLink.platform : platformOrLink;
  const linkUrl = typeof platformOrLink === 'object' ? platformOrLink.url : url;
  const explicitIcon = typeof platformOrLink === 'object' ? platformOrLink.icon : icon;

  const info = resolveSocialPlatform(`${platform || ''} ${linkUrl || ''}`, explicitIcon);
  return info.svgPath || null;
}

export function getSocialColor(platformOrLink?: any, url?: string): string {
  if (!platformOrLink) return '#958ea0';
  const platform = typeof platformOrLink === 'object' ? platformOrLink.platform : platformOrLink;
  const linkUrl = typeof platformOrLink === 'object' ? platformOrLink.url : url;

  return resolveSocialPlatform(`${platform || ''} ${linkUrl || ''}`).color;
}
