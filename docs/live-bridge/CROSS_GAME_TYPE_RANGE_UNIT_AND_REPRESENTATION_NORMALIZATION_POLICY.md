# CROSS-GAME TYPE, RANGE, UNIT AND REPRESENTATION NORMALIZATION POLICY

**Document Identifier:** `NLE-POL-TRU-001`  
**Lifecycle Stage:** Authoritative Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 74 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Mathematical Rigor  
**Upstream Dependencies:** `NLE-SPC-CSM-001` (Step 74), `NLE-CON-SEC-001` (Step 74), `NLE-POL-GPE-001` (Step 70)  
**Downstream Dependents:** `NLE-POL-IES-001` (Step 74), `NLE-SPC-DMD-001` (Step 74)

---

## 1. Purpose and Scope

This policy defines the authoritative conversion rules, numerical precision requirements, clamping bounds, coordinate transformation matrices, and unit normalization standards across heterogeneous game runtimes. 

It establishes the **NexLudic Canonical Representation System (NCRS)**, ensuring that physical quantities (position, velocity, angle, mass, time, force) and scalar data types are converted deterministically without NaN poisoning, integer overflow, or coordinate warping.

---

## 2. Type Compatibility and Conversion Matrix

```
+---------------+-------------------------------------------------------------------------------------------+
| Source Type   | Permitted Target Conversions & Conversion Semantics                                      |
+---------------+-------------------------------------------------------------------------------------------+
| INT8 / UINT8  | -> INT16, INT32, INT64, FLOAT32, FLOAT64 (Lossless Promotion)                             |
| INT16/ UINT16 | -> INT32, INT64, FLOAT32, FLOAT64 (Lossless Promotion); -> INT8 (Clamped with Warning)    |
| INT32/ UINT32 | -> INT64, FLOAT64 (Lossless); -> FLOAT32 (Lossless if |x| <= 2^24); -> INT16/8 (Clamped)  |
| INT64/ UINT64 | -> FLOAT64 (Lossless if |x| <= 2^53); -> INT32 (Clamped with Warning)                     |
| FLOAT32       | -> FLOAT64 (Lossless Promotion); -> INT32 (Truncate or Round with NaN/Inf Check)          |
| FLOAT64       | -> FLOAT32 (Bounded Precision Loss if in range; Overflows mapped to Clamp Bound)         |
| BOOLEAN       | -> INT32 (0 or 1); -> STRING ("true"/"false")                                             |
| VECTOR3       | -> Normalized Coordinate Transform -> Target VECTOR3 (Right/Left-Handed Adjustment)       |
| QUATERNION    | -> Normalized Renormalization (||q|| = 1.0) -> Frame Basis Rotation -> Target QUATERNION  |
+---------------+-------------------------------------------------------------------------------------------+
```

### NaN, Infinity, and Subnormal Guard Policy:
- Any incoming floating-point value matching IEEE 754 `NaN`, `+Infinity`, `-Infinity`, or subnormal representation is strictly rejected at the normalization layer.
- Emits `ERR_SEM_INVALID_NUMERIC_VALUE`.
- Under no circumstance shall a `NaN` be propagated to the Canonical Intermediate Representation or to a target game runtime.

---

## 3. Canonical Physical Units of Measurement (SI Standards)

All physical quantities in NexLudic Engine must be normalized to International System of Units (SI) baseline standards within the Canonical Intermediate Representation:

| Physical Dimension | Canonical Unit | Canonical Symbol | Non-Canonical Source Units & Exact Conversion Factors |
| :--- | :--- | :--- | :--- |
| **Length / Distance** | Meter | $\text{m}$ | Unreal Unit: $1.0\text{ UU} = 0.01\text{ m}$<br>Hammer Unit: $1.0\text{ HU} = 0.01905\text{ m}$<br>Foot: $1.0\text{ ft} = 0.3048\text{ m}$<br>Inch: $1.0\text{ in} = 0.0254\text{ m}$ |
| **Mass** | Kilogram | $\text{kg}$ | Gram: $1.0\text{ g} = 0.001\text{ kg}$<br>Pound (lb): $1.0\text{ lb} = 0.45359237\text{ kg}$<br>Slug: $1.0\text{ slug} = 14.593903\text{ kg}$ |
| **Time** | Second | $\text{s}$ | Millisecond: $1.0\text{ ms} = 10^{-3}\text{ s}$<br>Microsecond: $1.0\ \mu\text{s} = 10^{-6}\text{ s}$<br>Tick (60Hz): $1\text{ tick} = 0.01666667\text{ s}$ |
| **Angle** | Radian | $\text{rad}$ | Degree: $1^\circ = \frac{\pi}{180}\text{ rad}$<br>Revolution: $1\text{ rev} = 2\pi\text{ rad}$ |
| **Linear Velocity** | Meter/second | $\text{m/s}$ | $\text{km/h}: 1.0\text{ km/h} = \frac{1}{3.6}\text{ m/s}$<br>$\text{mph}: 1.0\text{ mph} = 0.44704\text{ m/s}$<br>$\text{knot}: 1.0\text{ kn} = 0.514444\text{ m/s}$ |
| **Angular Velocity**| Radian/second| $\text{rad/s}$ | $\text{RPM}: 1.0\text{ RPM} = \frac{2\pi}{60}\text{ rad/s}$<br>$\text{deg/s}: 1^\circ/\text{s} = \frac{\pi}{180}\text{ rad/s}$ |
| **Force** | Newton | $\text{N}$ | $\text{dyne}: 1.0\text{ dyn} = 10^{-5}\text{ N}$<br>$\text{lbf}: 1.0\text{ lbf} = 4.4482216\text{ N}$ |

---

## 4. Canonical Spatial Coordinate System & Transformation Matrices

### 4.1 Canonical Coordinate System Standard
NexLudic Engine adopts the following Canonical Coordinate Frame:
- **Handedness:** Right-Handed (RH)
- **Up Axis:** $+Y$ Up
- **Forward Axis:** $-Z$ Forward (toward viewer/camera default) or $+Z$ Forward depending on domain profile; baseline is **$+Z$ Forward, $+X$ Right, $+Y$ Up**.

### 4.2 Cross-Engine Coordinate Conversion Matrices

#### Case A: Unreal Engine (Left-Handed, Z-Up, X-Forward, cm) to Canonical (Right-Handed, Y-Up, Z-Forward, m)
Given Unreal position vector $\mathbf{p}_{UE} = [x_{ue}, y_{ue}, z_{ue}]^T$ in centimeters:

$$\mathbf{p}_{canonical} = 0.01 \cdot \begin{bmatrix} 0 & 1 & 0 \\ 0 & 0 & 1 \\ 1 & 0 & 0 \end{bmatrix} \begin{bmatrix} x_{ue} \\ y_{ue} \\ z_{ue} \end{bmatrix} = \begin{bmatrix} 0.01 \cdot y_{ue} \\ 0.01 \cdot z_{ue} \\ 0.01 \cdot x_{ue} \end{bmatrix}$$

For Quaternions $\mathbf{q}_{UE} = [x, y, z, w]$:
$$\mathbf{q}_{canonical} = [y, z, x, -w] \quad \text{(with normalization } \|\mathbf{q}\| = 1.0\text{)}$$

#### Case B: Unity Engine (Left-Handed, Y-Up, Z-Forward, m) to Canonical (Right-Handed, Y-Up, Z-Forward, m)
Given Unity position vector $\mathbf{p}_{Unity} = [x_u, y_u, z_u]^T$:

$$\mathbf{p}_{canonical} = \begin{bmatrix} -1 & 0 & 0 \\ 0 & 1 & 0 \\ 0 & 0 & 1 \end{bmatrix} \begin{bmatrix} x_u \\ y_u \\ z_u \end{bmatrix} = \begin{bmatrix} -x_u \\ y_u \\ z_u \end{bmatrix}$$

For Quaternions:
$$\mathbf{q}_{canonical} = [-x, y, z, -w]$$

---

## 5. Explicit Range Clamping and Precision Bounds

1. **Physical Spatial Boundary:** Position vectors are strictly clamped to $[-10^7, +10^7]$ meters (sufficient to cover planetary orbital scale while preventing float32 precision breakdown).
2. **Quaternion Normalization:** Quaternions must satisfy $|\|\mathbf{q}\| - 1.0| < 10^{-4}$. If outside tolerance, renormalizing is attempted. If $\|\mathbf{q}\| < 10^{-6}$, it is rejected as degenerate and replaced by identity quaternion $[0, 0, 0, 1]$ with an audit alert.
3. **Clamping Transparency:** Any value altered via clamping must set the `clamped: true` flag in the mapping metadata.
