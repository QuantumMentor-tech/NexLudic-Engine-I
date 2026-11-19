# Content Normalization & Intermediate Representation Specification

**Document Identifier:** `NLE-SPC-CNI-001`  
**Classification:** Technical Architecture & Normalization Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 63 of 100  
**Upstream Dependencies:** `NLE-SPC-TRF-001`, `NLE-CON-FCC-001`, `NLE-SPC-CFT-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Normalization Strategy

To avoid an $M \times N$ conversion combinatorial explosion between different game engines, NexLudic Engine employs canonical **Intermediate Representations (IR)**. All source assets are first normalized into an engine-neutral, standardized representation before being compiled into target-specific proprietary formats.

---

## 2. Canonical Coordinate System & Unit Normalization

Different game engines employ conflicting coordinate frames and spatial unit conventions:

| Game Engine | Coordinate System | Up Vector | Forward Vector | World Unit Standard |
| :--- | :--- | :--- | :--- | :--- |
| **NexLudic IR** | Right-Handed (OpenGL) | $+Y$ Up | $-Z$ Forward | $1.0\text{ unit} = 1.0\text{ meter}$ |
| **Unreal Engine** | Left-Handed (Z-Up) | $+Z$ Up | $+X$ Forward | $1.0\text{ unit} = 1.0\text{ centimeter}$ ($0.01\text{ m}$) |
| **Unity Engine** | Left-Handed (Y-Up) | $+Y$ Up | $+Z$ Forward | $1.0\text{ unit} = 1.0\text{ meter}$ |
| **Creation Engine** | Right-Handed (Z-Up) | $+Z$ Up | $+Y$ Forward | $1.0\text{ unit} = 0.01428\text{ meters}$ ($1\text{ yard} \approx 64\text{ units}$) |
| **Source Engine** | Right-Handed (Z-Up) | $+Z$ Up | $+X$ Forward | $1.0\text{ unit} = 0.01905\text{ meters}$ ($1\text{ inch} = 1\text{ unit}$) |

### Coordinate Normalization Transform Matrix ($\mathbf{M}_{\text{norm}}$):
To convert from NexLudic IR (Right-Handed, Y-Up) to Unreal Engine (Left-Handed, Z-Up, Centimeters):

$$\mathbf{M}_{\text{IR} \rightarrow \text{UE}} = \begin{bmatrix} 0 & 0 & -100 & 0 \\ 100 & 0 & 0 & 0 \\ 0 & 100 & 0 & 0 \\ 0 & 0 & 0 & 1 \end{bmatrix}$$

- **Polygon Winding Order:** Any transformation that negates an odd number of spatial axes must invert vertex index winding (e.g. Counter-Clockwise $\leftrightarrow$ Clockwise) to preserve surface normal orientation.

---

## 3. Permitted Defaults & Schema Invariants

When normalising into IR, missing values must NOT receive arbitrary defaults:
- **Default Bounding Volume:** Must be explicitly computed from the vertex extrema: $[\min(\mathbf{v}), \max(\mathbf{v})]$.
- **Default Normals:** If absent, normals must be computed using cross-products of adjacent triangle edges ($\mathbf{n} = \frac{\mathbf{e}_1 \times \mathbf{e}_2}{\|\mathbf{e}_1 \times \mathbf{e}_2\|}$).
- **Default Roughness/Metallic:** If PBR parameters are unspecified, default to calibrated baseline: $\text{Roughness} = 0.5$, $\text{Metallic} = 0.0$.
- **Explicit Invariant:** Fabricating unverified game-specific tags or custom shader scripts during normalization is strictly prohibited.
