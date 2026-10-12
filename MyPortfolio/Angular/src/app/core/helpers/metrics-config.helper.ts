export interface MetricsVisibilityConfig {
  showMetricsStrip: boolean;
  showProjects: boolean;
  showSkills: boolean;
  showExperience: boolean;
  showEducation: boolean;
}

export const DEFAULT_METRICS_CONFIG: MetricsVisibilityConfig = {
  showMetricsStrip: true,
  showProjects: true,
  showSkills: true,
  showExperience: true,
  showEducation: true
};

const METRICS_PREFIX = 'METRICS_CONFIG:';
const LOCAL_STORAGE_KEY = 'portfolio.metrics.config';

export function parseMetricsConfig(paragraphs?: string[] | null): MetricsVisibilityConfig {
  if (Array.isArray(paragraphs)) {
    const found = paragraphs.find(p => p && p.startsWith(METRICS_PREFIX));
    if (found) {
      try {
        const json = found.substring(METRICS_PREFIX.length);
        const parsed = JSON.parse(json);
        return {
          showMetricsStrip: parsed.showMetricsStrip ?? true,
          showProjects: parsed.showProjects ?? true,
          showSkills: parsed.showSkills ?? true,
          showExperience: parsed.showExperience ?? true,
          showEducation: parsed.showEducation ?? true
        };
      } catch {
        // fallback
      }
    }
  }

  try {
    const local = localStorage.getItem(LOCAL_STORAGE_KEY);
    if (local) {
      const parsed = JSON.parse(local);
      return {
        showMetricsStrip: parsed.showMetricsStrip ?? true,
        showProjects: parsed.showProjects ?? true,
        showSkills: parsed.showSkills ?? true,
        showExperience: parsed.showExperience ?? true,
        showEducation: parsed.showEducation ?? true
      };
    }
  } catch {
    // fallback
  }

  return { ...DEFAULT_METRICS_CONFIG };
}

export function encodeMetricsConfig(config: MetricsVisibilityConfig, existingParagraphs: string[] = []): string[] {
  try {
    localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(config));
  } catch {
    // ignore
  }

  const clean = existingParagraphs.filter(p => !p || !p.startsWith(METRICS_PREFIX));
  const encoded = `${METRICS_PREFIX}${JSON.stringify(config)}`;
  return [encoded, ...clean];
}
