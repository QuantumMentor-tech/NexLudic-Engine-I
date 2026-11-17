# PROJECT WORKSPACE ACCESSIBILITY AND RESPONSIVE LAYOUT PLAN

**Document Identifier:** `NLE-PLN-ACC-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 21 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Accessibility (WCAG 2.1 AA), Keyboard Architecture & Responsive Scaling  
**Parent Document:** `NLE-SPC-WKS-001` ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_WORKSPACE_LAYOUT_AND_NAVIGATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_WORKSPACE_LAYOUT_AND_NAVIGATION_SPECIFICATION.md))  

---

## 1. Keyboard Navigation & Pane Cycling Architecture

The workspace provides 100% complete mouse-free operation conforming to Microsoft Fluent accessibility standards:

```
[F6] Cycles Primary Focus Panes:
Global Nav Rail -> Project Sidebar -> Main Content Header -> Main Data Grid -> Contextual Panel -> Status Tray
```

### 1.1 Standard Keyboard Shortcuts

| Shortcut | Scope | Action Executed |
| :--- | :--- | :--- |
| `F6` | Global | Cycle forward through workspace panes |
| `Shift + F6` | Global | Cycle backward through workspace panes |
| `Ctrl + 1` .. `Ctrl + 9` | Project | Jump directly to sidebar destinations 1 through 9 |
| `Alt + Left Arrow` | Navigation | Navigate to previous location in history |
| `Alt + Right Arrow` | Navigation | Navigate forward in history |
| `Ctrl + B` or `F4` | Viewport | Toggle Contextual Detail Panel (Expand / Collapse) |
| `Ctrl + S` | Project | Trigger atomic project save (`ACT-PRJ-001`) |
| `Ctrl + F` | Destination | Shift focus to local search / filter input |
| `Escape` | Contextual | Close active modal dialog or contextual detail panel |

---

## 2. Focus Management & Screen Reader Semantics

### 2.1 Focus Restoration Matrix
- **Destination Transition:** When navigating to a new destination, focus is placed on the destination's primary `<h1>` heading or the first interactive element, announcing the page title.
- **Detail Panel Toggle:** When closing the Contextual Detail Panel via `Escape` or `Ctrl+B`, focus returns to the specific item in the main data grid that initiated the inspection.
- **Modal Closure:** Closing any modal dialog restores focus precisely to the triggering button.

### 2.2 UI Automation Landmarks & Live Announcements
All major regions expose standardized `AutomationProperties`:
```xml
<!-- Example WPF XAML Automation Declaration -->
<Border AutomationProperties.AutomationId="ProjectNavRegion"
        AutomationProperties.Name="Project Destinations Navigation"
        AutomationProperties.HelpText="List of available workflow areas for the active project">
    <ListBox AutomationProperties.ItemStatus="{Binding ActiveDestinationStatus}" ... />
</Border>

<TextBlock AutomationProperties.LiveSetting="Assertive"
           Text="{Binding CriticalBlockerAlertMessage}" />
```

---

## 3. Responsive Desktop Layout & DPI Scaling

### 3.1 Supported Resolution Tiers
The desktop application shell supports window sizes from **1024 x 700 pixels** up to multi-monitor 4K displays:

```
+----------------------------------------------------------------------------------------------------+
| WINDOW RESOLUTION BREAKPOINT RULES                                                                 |
+-------------------+---------------------+-------------------------+--------------------------------+
| Viewport Width    | Project Sidebar     | Contextual Panel        | Layout Behavior                |
+-------------------+---------------------+-------------------------+--------------------------------+
| >= 1440px (Wide)  | Pinned Expanded     | Pinned Expanded (400px) | Full 3-column view             |
| 1200px - 1439px   | Pinned Standard     | Pinned Compact (320px)  | Adaptive content grid          |
| 1100px - 1199px   | Pinned Standard     | Overlay Drawer          | Detail panel slides over content|
| 1024px - 1099px   | Collapsed Rail (48px)| Overlay Drawer (Modal) | Compact icon-only navigation   |
| < 1024px (Warn)   | Min Size Enforced   | Window Clamped to 1024  | Horizontal scroll disallowed   |
+-------------------+---------------------+-------------------------+--------------------------------+
```

### 3.2 High DPI & Text Scaling Behavior
- All iconography uses vector-based Geometry / Path shapes or SVG resources to eliminate pixelation at 125%, 150%, 175%, and 200% scaling.
- Long project titles, game names, and file paths utilize middle-truncation or trailing ellipsis with full-text tooltips:
  ```
  Display: "The Elder Scrolls V: Skyrim Special Edition" -> "Skyrim Special Edition..."
  Tooltip: "The Elder Scrolls V: Skyrim Special Edition (Build 1.6.640.0)"
  ```
- Contrast ratios strictly meet WCAG 2.1 Level AA (4.5:1 for body text, 3:1 for large headings and active border chips) across both Light and Dark themes.

