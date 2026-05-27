---
name: Orbital Mission Control
colors:
  surface: '#10131a'
  surface-dim: '#10131a'
  surface-bright: '#363940'
  surface-container-lowest: '#0b0e14'
  surface-container-low: '#191c22'
  surface-container: '#1d2026'
  surface-container-high: '#272a31'
  surface-container-highest: '#32353c'
  on-surface: '#e1e2eb'
  on-surface-variant: '#b9cacb'
  inverse-surface: '#e1e2eb'
  inverse-on-surface: '#2e3037'
  outline: '#849495'
  outline-variant: '#3b494b'
  surface-tint: '#00dbe9'
  primary: '#dbfcff'
  on-primary: '#00363a'
  primary-container: '#00f0ff'
  on-primary-container: '#006970'
  inverse-primary: '#006970'
  secondary: '#ffdb9d'
  on-secondary: '#412d00'
  secondary-container: '#feb700'
  on-secondary-container: '#6b4b00'
  tertiary: '#fff3f1'
  on-tertiary: '#690003'
  tertiary-container: '#ffcec7'
  on-tertiary-container: '#c1000a'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#7df4ff'
  primary-fixed-dim: '#00dbe9'
  on-primary-fixed: '#002022'
  on-primary-fixed-variant: '#004f54'
  secondary-fixed: '#ffdea8'
  secondary-fixed-dim: '#ffba20'
  on-secondary-fixed: '#271900'
  on-secondary-fixed-variant: '#5e4200'
  tertiary-fixed: '#ffdad5'
  tertiary-fixed-dim: '#ffb4aa'
  on-tertiary-fixed: '#410001'
  on-tertiary-fixed-variant: '#930005'
  background: '#10131a'
  on-background: '#e1e2eb'
  surface-variant: '#32353c'
typography:
  display-lg:
    fontFamily: Geist
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 56px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Geist
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
  headline-md:
    fontFamily: Geist
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  body-lg:
    fontFamily: Geist
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Geist
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  data-mono-lg:
    fontFamily: JetBrains Mono
    fontSize: 16px
    fontWeight: '500'
    lineHeight: 24px
  data-mono-sm:
    fontFamily: JetBrains Mono
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 16px
    letterSpacing: 0.05em
  label-caps:
    fontFamily: JetBrains Mono
    fontSize: 11px
    fontWeight: '700'
    lineHeight: 16px
    letterSpacing: 0.1em
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  unit: 4px
  gutter: 24px
  margin-mobile: 16px
  margin-desktop: 32px
  panel-gap: 12px
---

## Brand & Style
This design system embodies the high-stakes environment of aerospace telemetry and orbital mechanics. The personality is **Precise, Authoritative, and Futuristic**, designed to convey absolute reliability in a mission-critical context. 

The visual style is **Sci-Fi Industrial**, merging the clarity of a modern SaaS interface with the aesthetic markers of a mission control room. It utilizes **Glassmorphism** to represent layered data streams without losing visual context of the underlying orbital maps. High data density is balanced by rigorous alignment and "Technical Minimalism"—where every line, glow, and border serves a functional purpose in identifying satellite trajectories or debris hazards.

## Colors
The palette is rooted in the vacuum of space, using **Deep Space Navy (#0B0E14)** as the primary canvas to minimize eye strain during long-duration monitoring. 

- **Cyber Cyan (#00F0FF)**: The primary action and data color. It represents "Active/Normal" states, telemetry paths, and interactive HUD elements.
- **Warning Amber (#FFB800)**: Reserved for non-critical alerts, predicted conjunctions, and system maintenance notifications.
- **Emergency Red (#FF3B30)**: High-priority collision warnings and hardware failures. 
- **Neutral Grays**: Used for secondary technical data and UI scaffolding, ensuring the vibrant primary accents remain meaningful.

## Typography
The typography system prioritizes legibility of complex strings (coordinates, IDs, timestamps). 

**Geist** provides a clean, modern geometric foundation for navigation and structural headers. **JetBrains Mono** is utilized for all telemetry readouts, data tables, and status labels to ensure that numerical characters are easily distinguishable and perfectly aligned in vertical columns. 

Large display titles should be reserved for dashboard-level summaries, while the bulk of the interface relies on `data-mono-sm` for high-density information displays typical of aerospace monitoring.

## Layout & Spacing
The layout follows a **Modular Fluid Grid** philosophy. Content is organized into discrete panels that can be expanded or collapsed based on the operator's focus. 

A strict **4px baseline grid** ensures technical alignment across all components. Gutters are kept wide (24px) to prevent data-heavy panels from appearing cluttered, creating "breathable" industrial sectors. On desktop, the layout utilizes a fixed-position left-hand command rail and a right-hand telemetry sidebar, with a fluid 3D orbital viewport in the center. On mobile, panels stack vertically, prioritizing the "Alert" and "Status" modules.

## Elevation & Depth
Depth is created through **Tonal Layering and Glassmorphism**, rather than traditional soft shadows.

1.  **Background Layer**: Solid Deep Space Navy.
2.  **Glass Panels**: Semi-transparent surfaces (80% opacity) with a `backdrop-filter: blur(12px)`. These panels feature a 1px solid border (Cyan at 20% opacity) to define their edges.
3.  **Active Elevation**: Elements requiring immediate attention use an outer "Cyan Glow" (bloom effect) instead of a shadow, simulating a light-emitting screen or HUD.
4.  **Information Hierarchy**: Higher-priority information is rendered in brighter, more opaque containers, while background data is dimmed and desaturated.

## Shapes
The shape language is **Technical and Structured**. We use "Soft" (0.25rem) rounding to maintain a modern feel while preserving the rigid, industrial nature of aerospace equipment. 

- **Containers**: Use a standard `rounded-sm` (4px) for cards and panels.
- **Interactive Elements**: Buttons and inputs follow the same 4px radius. 
- **Decorative Elements**: 45-degree chamfered corners are used sparingly on header accents to reinforce the "Military/Aerospace" aesthetic.
- **Status Indicators**: Small circular pips are used for live connection statuses.

## Components

### Buttons & Inputs
Buttons are rendered with a 1px Cyan border and transparent background by default (Ghost style). On hover, they fill with a low-opacity Cyan tint. "Primary Action" buttons use a solid Cyan background with black text for maximum contrast. Input fields are dark with a subtle 1px bottom border, glowing when active.

### Data Chips
Small, mono-spaced tags used for satellite designations (e.g., `NORAD: 25544`). These feature a light-gray background with a high-contrast border corresponding to the satellite's health status (Cyan, Amber, or Red).

### Telemetry Lists
High-density rows with alternating background tints. Every row includes a small sparkline or "active pulse" animation to indicate real-time data streaming.

### Orbital Cards
Glassmorphic containers used to house 3D visualizations or trajectory graphs. These must include a "Technical Header" with a coordinate label in the top-right corner.

### Alert Banners
Full-width bars at the top of the viewport. Collision alerts (Red) should utilize a pulsing opacity animation to ensure they are impossible to miss during critical maneuvers.