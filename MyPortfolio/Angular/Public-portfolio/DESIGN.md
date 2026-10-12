---
name: Obsidian Studio
colors:
  surface: '#0f131c'
  surface-dim: '#0f131c'
  surface-bright: '#353942'
  surface-container-lowest: '#0a0e16'
  surface-container-low: '#181c24'
  surface-container: '#1c2028'
  surface-container-high: '#262a33'
  surface-container-highest: '#31353e'
  on-surface: '#dfe2ee'
  on-surface-variant: '#cbc3d7'
  inverse-surface: '#dfe2ee'
  inverse-on-surface: '#2c3039'
  outline: '#958ea0'
  outline-variant: '#494454'
  surface-tint: '#d0bcff'
  primary: '#d0bcff'
  on-primary: '#3c0091'
  primary-container: '#a078ff'
  on-primary-container: '#340080'
  inverse-primary: '#6d3bd7'
  secondary: '#4cd7f6'
  on-secondary: '#003640'
  secondary-container: '#03b5d3'
  on-secondary-container: '#00424e'
  tertiary: '#4edea3'
  on-tertiary: '#003824'
  tertiary-container: '#00a572'
  on-tertiary-container: '#00311f'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#e9ddff'
  primary-fixed-dim: '#d0bcff'
  on-primary-fixed: '#23005c'
  on-primary-fixed-variant: '#5516be'
  secondary-fixed: '#acedff'
  secondary-fixed-dim: '#4cd7f6'
  on-secondary-fixed: '#001f26'
  on-secondary-fixed-variant: '#004e5c'
  tertiary-fixed: '#6ffbbe'
  tertiary-fixed-dim: '#4edea3'
  on-tertiary-fixed: '#002113'
  on-tertiary-fixed-variant: '#005236'
  background: '#0f131c'
  on-background: '#dfe2ee'
  surface-variant: '#31353e'
typography:
  display-hero:
    fontFamily: Geist
    fontSize: 48px
    fontWeight: '600'
    lineHeight: 56px
    letterSpacing: -0.03em
  display-hero-mobile:
    fontFamily: Geist
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Geist
    fontSize: 28px
    fontWeight: '600'
    lineHeight: 36px
    letterSpacing: -0.02em
  headline-md:
    fontFamily: Geist
    fontSize: 22px
    fontWeight: '500'
    lineHeight: 30px
    letterSpacing: -0.015em
  headline-sm:
    fontFamily: Geist
    fontSize: 18px
    fontWeight: '500'
    lineHeight: 26px
    letterSpacing: -0.01em
  body-lg:
    fontFamily: Geist
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  body-md:
    fontFamily: Geist
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  body-sm:
    fontFamily: Geist
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 18px
  code-lg:
    fontFamily: JetBrains Mono
    fontSize: 14px
    fontWeight: '500'
    lineHeight: 22px
  code-md:
    fontFamily: JetBrains Mono
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 18px
  code-sm:
    fontFamily: JetBrains Mono
    fontSize: 11px
    fontWeight: '400'
    lineHeight: 16px
  label-mono:
    fontFamily: JetBrains Mono
    fontSize: 11px
    fontWeight: '600'
    lineHeight: 14px
    letterSpacing: 0.06em
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  gutter: 1rem
  gutter-desktop: 1.5rem
  margin: 1rem
  margin-desktop: 2rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 1rem
  space-lg: 1.5rem
  space-xl: 2.5rem
---

## Brand & Style

This design system serves high-caliber software engineers, tech leads, and systems architects who treat their portfolio not merely as a resume, but as an interactive, production-grade deployment artifact. 

The aesthetic is precision-driven dark minimalism anchored in IDE and developer-tool ergonomics. It synthesizes utilitarian restraint with subtle, high-fidelity luminescent details:
- **Atmosphere:** Pitch-black command centers, low-noise obsidian tiers, micro-etched lines, and ultra-crisp alphanumeric typography.
- **Tone:** Methodical, unyielding, highly technical, and architecturally solid. Zero extraneous decorations; every boundary exists to clarify structural intent.
- **Visual Movement:** High-tech precision dark UI combined with structural wireframe geometry and luminous telemetry accents (cyan pulses, electric violet interactive nodes, emerald health indicators).

## Colors

The palette establishes high contrast between deep slate-black substrates and clinical foreground typography, punctuated by deliberate, neon-accurate chromatic indicators.

### Surface Architecture
- **Canvas Base (`#0B0F17`):** The master canvas, root frame, and viewport gutters.
- **Surface 1 (`#111827`):** Sidebar navigation, fixed workbench rails, and collapsible utility panels.
- **Surface 2 (`#161F30`):** Cards, code editor containers, form field backdrops, and active inspectors.
- **Surface 3 / Interactive Hover (`#1E293B`):** Modal windows, elevated sheets, dropdown menus, and hovered card surfaces.

### Boundary & Divide
- **Subtle Perimeter:** `rgba(255, 255, 255, 0.08)` or `#263348` applied strictly as 1px dividers.
- **Active / Focused Perimeter:** `#8B5CF6` (Electric Violet) with 20% alpha glow for inputs and selected canvas elements.

### Semantic & Accents
- **Primary Accent (`#8B5CF6` - Electric Violet):** Primary actions, active navigation states, branch markers, and focused canvas handles.
- **Secondary Accent (`#06B6D4` - Glowing Cyan):** Live preview viewport toggles, AST schema inspectors, telemetry readouts, and interactive preview viewports.
- **Tertiary Accent (`#10B981` - Emerald):** Deployment status ("Live", "Published", "Healthy CI/CD pipeline"), git commit hashes, and uptime badges.
- **Destructive / Warning:** Crimson (`#EF4444`) for build failures and delete actions; Amber (`#F59E0B`) for draft or uncommitted changes.
- **Foreground Text:** `#F8FAFC` (High emphasis headlines and active code), `#94A3B8` (Body, field labels, metadata), `#64748B` (De-emphasized timestamps, comments, disabled states).

## Typography

The type scale marries the structural neutrality of `Geist` with the technical rigor of `JetBrains Mono`.

- **Geist (Body & Display):** Optimized for low-light legibility and dense user interface assemblies. Headlines use negative tracking (`-0.015em` to `-0.03em`) to deliver a compressed, surgical punch.
- **JetBrains Mono (Metadata, Code & Telemetry):** Used for code blocks, tech stack chips, API endpoints, commit SHAs, keyboard shortcuts, and uppercase UI badges.
- **Rule of Rhythm:** Never use monospaced fonts for long-form narrative blocks. Monospace is restricted to structural identifiers, attributes, and key-value pairs.

## Layout & Spacing

The system is architected around an IDE-style split pane layout optimized for widescreen and multi-monitor workstations, with fluid downscaling to tablets and mobile views.

### Viewport Tiers
- **Desktop (>= 1280px):** 3-column split view (Config/Navigation Rail: 240px; Canvas/Editor: Fluid; Live Preview/Inspector: 420px–600px with resizable handle).
- **Tablet (768px – 1279px):** 2-column view with collapsible inspector drawers and toggleable preview overlay.
- **Mobile (< 768px):** Single-column stacked mode with tabbed viewport switching between "Editor" and "Live Output".

### Spatial Logic
- **Dense Grid:** Internal form controls and code panes stick to strict 4px/8px baselines.
- **Component Padding:** `space-xs` (4px) and `space-sm` (8px) for compact toolbars, buttons, and badges. `space-md` (16px) for interior card and pane padding. `space-lg` (24px) for section splits.

## Elevation & Depth

This design system rejects heavy, muddy drop shadows in favor of **Tonal Layering** combined with **Low-Contrast Micro-Borders** and **Selective Chromatic Glows**.

### Surface Hierarchy
1. **Root Bed (`#0B0F17`):** Zero elevation.
2. **Structural Rails (`#111827`):** Differentiated by a crisp `1px solid rgba(255, 255, 255, 0.08)` border on abutting edges.
3. **Cards & Modules (`#161F30`):** Framed in `1px solid #263348`, resting on top of `#111827`. Hovering elevates them to `#1E293B` with a subtle inner border highlight (`rgba(255, 255, 255, 0.12)`).
4. **Overlays & Floating Panels (`#1E293B`):** Positioned with `box-shadow: 0 20px 40px -15px rgba(0, 0, 0, 0.7), 0 0 0 1px rgba(255, 255, 255, 0.1)`.

### Chromatic Glows
- When preview nodes are active or a project is live, borders emit targeted, non-intrusive ambient light:
  - Cyan Focus: `box-shadow: 0 0 16px -4px rgba(6, 182, 212, 0.25)`
  - Violet Focus: `box-shadow: 0 0 16px -4px rgba(139, 92, 246, 0.3)`
  - Emerald Live Ping: A glowing 6px indicator ring using `rgba(16, 185, 129, 0.3)`.

## Shapes

The shape vocabulary employs a restrained **Level 1 (Soft)** approach, maximizing screen real estate and mirroring modern code environments.

- **Standard Elements (Buttons, Inputs, Badges, Tech Tags):** `rounded` (0.25rem / 4px).
- **Cards, Preview Frames & Modals:** `rounded-lg` (0.5rem / 8px).
- **Tooltips & Floating Action Hubs:** `rounded` (0.25rem / 4px).
- **Status Dots & Avatar Radii:** Fully rounded circles (`rounded-full`).

## Components

### Buttons
- **Primary:** Background `#8B5CF6`, text `#FFFFFF`, font `Geist` 500 weight, 1px solid `rgba(255,255,255,0.15)`. Hover: `#7C3AED` with `box-shadow: 0 0 12px rgba(139, 92, 246, 0.35)`.
- **Secondary / Ghost:** Background `transparent`, text `#F8FAFC`, 1px solid `rgba(255, 255, 255, 0.08)`. Hover: `#1E293B`, border `#263348`.
- **Subtle Command Button:** Background `#161F30`, text `#94A3B8`, with keyboard shortcut badge (`JetBrains Mono`) anchored to the right edge.

### Tech Stack Chips & Tags
- Height: 24px.
- Background `#111827`, border `1px solid #263348`, text `#06B6D4` (or `#94A3B8` when unselected), font `JetBrains Mono` 11px uppercase.
- Optional interactive remove/drag handle for building tech stacks.

### Form Inputs & Code Editors
- Background `#0B0F17`, border `1px solid #263348`, text `#F8FAFC`, placeholder `#64748B`.
- Focus state: Border color transitions to `#8B5CF6` with a micro outline ring of `rgba(139, 92, 246, 0.15)`.
- Label placed in `JetBrains Mono` 11px uppercase with high contrast `#94A3B8`.

### Cards & Project Containers
- Background `#161F30`, border `1px solid rgba(255, 255, 255, 0.08)`.
- Header bar includes split metadata (e.g., repository branch, build status, live domain).
- Hover: border transition to `#263348` and subtle highlight gradient along top edge (`linear-gradient(90deg, transparent, rgba(6, 182, 212, 0.2), transparent)`).

### Split-Preview Controls
- Segmented toggle control embedded in the workbench header: `[ Desktop | Tablet | Mobile | JSON View ]`.
- Active segment: `#1E293B` background, `#F8FAFC` foreground, accented with a 2px Cyan bottom indicator line.
- Inactive segments: `#64748B` foreground.

### Publication & Health Indicators
- Live status pill: `#10B981` at 10% background opacity, 1px solid `rgba(16, 185, 129, 0.2)`, text `#10B981`. Includes a pulsating 6px circular dot (`#10B981`) on the left margin.