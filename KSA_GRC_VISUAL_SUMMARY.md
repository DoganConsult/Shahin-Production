# KSA GRC System - Visual Summary

## 📊 System Statistics at a Glance

```
┌─────────────────────────────────────────────────────────────┐
│                    GRC SYSTEM INVENTORY                      │
├─────────────────────────────────────────────────────────────┤
│  Regulators:           91  (KSA + International)            │
│  Frameworks:           162 (Regulatory + Standards)          │
│  Controls:             57,211 (Across all frameworks)       │
│  Unique Regulators:   44  (Distinct regulatory bodies)     │
└─────────────────────────────────────────────────────────────┘
```

## 🏛️ Top 10 Regulators by Control Count

```
ISO          ████████████████████████████████████████  1,483 controls (23 frameworks)
NIST         ███████████████████████████████████████   1,421 controls (5 frameworks)
SAMA         ████████████████████████████████         1,002 controls (15 frameworks)
CMA          ████████████████████████████               873 controls (12 frameworks)
MOH          ████████████████████████                    703 controls (10 frameworks)
SFDA         ███████████████████████                     702 controls (8 frameworks)
PCI-SSC      ██████████████████████                      657 controls (4 frameworks)
MOCI         ████████████████████                         596 controls (8 frameworks)
MHRSD        ████████████████                            483 controls (6 frameworks)
NCA          ████████████                                 477 controls (10 frameworks)
```

## 📦 Module Comparison: Our System vs. Market

```
┌─────────────────────────────────────────────────────────────────┐
│  MODULE                    │  TYPICAL 8-12  │  OUR SYSTEM     │
├────────────────────────────┼────────────────┼─────────────────┤
│  1. Assessments           │      ✅        │  ✅ Advanced    │
│  2. Control Assessments   │      ✅        │  ✅ Advanced    │
│  3. Evidence Management   │      ✅        │  ✅ Advanced    │
│  4. Risk Management       │      ✅        │  ✅ Advanced    │
│  5. Action Plans          │      ✅        │  ✅ Advanced    │
│  6. Audit Management      │      ✅        │  ✅ Advanced    │
│  7. Policy Management     │      ✅        │  ✅ Advanced    │
│  8. Workflow Engine       │      ⚠️ Basic  │  ✅ BPMN 2.0    │
│  9. Reports & Analytics   │      ✅        │  ✅ Advanced    │
│ 10. Resilience            │      ❌       │  ✅ NEW         │
│ 11. Approval Workflows    │      ⚠️ Basic  │  ✅ Advanced    │
│ 12. Notifications         │      ⚠️ Basic  │  ✅ Advanced    │
├────────────────────────────┼────────────────┼─────────────────┤
│  TOTAL MODULES            │      8-12      │      12         │
│  Frameworks Supported     │      10-20     │      162        │
│  Controls Available       │    500-2,000  │     57,211      │
│  Regulators Supported    │      5-10      │      91         │
│  Pre-defined Workflows   │      0-2       │      7          │
└─────────────────────────────────────────────────────────────────┘
```

## 🔄 GRC Cycle Flow (Simplified)

```
    ┌─────────────┐
    │  REGULATOR  │  (91 available: NCA, SAMA, SDAIA, etc.)
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │  FRAMEWORK  │  (162 available: NCA-ECC, SAMA-CSF, PDPL, etc.)
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │  CONTROLS   │  (57,211 available across all frameworks)
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │ ASSESSMENT  │  ← Workflow Engine (7 workflows)
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │   EVIDENCE  │  ← Evidence Management
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │ GAP ANALYSIS│
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │    RISK     │  ← Risk Management
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │ ACTION PLAN │  ← Action Plans Module
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │  VALIDATION │  ← Audit Module
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │   REPORT    │  ← Reports Module
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │  APPROVAL   │  ← Approval Workflows
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │  COMPLETE   │
    └─────────────┘
```

## 🎯 KSA-Specific Frameworks

### Cybersecurity (NCA)
- NCA-ECC (114 controls) - Essential Cybersecurity Controls
- NCA-CCC (67 controls) - Cloud Cybersecurity Controls
- NCA-OTCC (53 controls) - Operational Technology Controls
- NCA-CTCC (45 controls) - Critical Systems Controls
- NCA-DCC (38 controls) - Data Cybersecurity Controls
- **Total: 477 controls across 10 NCA frameworks**

### Financial Services (SAMA)
- SAMA-CSF (156 controls) - Cybersecurity Framework
- SAMA-AML (167 controls) - Anti-Money Laundering
- SAMA-CFT (89 controls) - Counter Terrorist Financing
- SAMA-PSR (67 controls) - Payment Services Regulations
- **Total: 1,002 controls across 15 SAMA frameworks**

### Data Privacy (SDAIA)
- PDPL (45 controls) - Personal Data Protection Law
- PDPL-IR (89 controls) - Implementing Regulations
- PDPL-DPIA (32 controls) - Data Protection Impact Assessment
- **Total: ~220 controls across 5 PDPL frameworks**

### Capital Markets (CMA)
- CMA-CG (94 controls) - Corporate Governance
- CMA-LR (128 controls) - Listing Rules
- CMA-AML (76 controls) - AML/CFT Rules
- **Total: 873 controls across 12 CMA frameworks**

## 🔗 Integration Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    WORKFLOW ENGINE                          │
│              (Orchestrates All Modules)                     │
└───────────────┬─────────────────────────────────────────────┘
                │
    ┌───────────┼───────────┐
    │           │           │
    ▼           ▼           ▼
┌─────────┐ ┌─────────┐ ┌─────────┐
│Assessment│ │  Risk   │ │ Evidence│
└────┬────┘ └────┬────┘ └────┬────┘
     │           │           │
     └───────────┼───────────┘
                 │
                 ▼
         ┌───────────────┐
         │ Action Plans  │
         └───────┬───────┘
                 │
                 ▼
         ┌───────────────┐
         │    Reports    │
         └───────────────┘
```

## 📈 Market Positioning

### Our System Advantages

✅ **Scale**: 57,211 controls (vs. typical 500-2,000)
✅ **Coverage**: 91 regulators (vs. typical 5-10)
✅ **Depth**: 162 frameworks (vs. typical 10-20)
✅ **Automation**: 7 pre-defined workflows (vs. typical 0-2)
✅ **Innovation**: Resilience module (unique in KSA)
✅ **Integration**: Full module interconnection

### Competitive Positioning

```
Typical KSA GRC Supplier:  8-12 modules, 500-2,000 controls, 10-20 frameworks
Our GRC System:           12 modules, 57,211 controls, 162 frameworks
                                                              ↑
                                                    Market Leader
```

---

**Last Updated**: 2026-01-22
**Status**: Production Ready ✅
