# Kanban UI Component Specification

## Overview

This specification defines the Kanban board and advanced view components for the GRC platform. The UI provides intuitive workflow visualization, drag-drop state transitions, and direct actionable items integrated with the gate evaluation engine.

---

## 1. Kanban Board Architecture

### 1.1 Component Hierarchy

```
┌─────────────────────────────────────────────────────────────────────┐
│                        KanbanBoardContainer                         │
├─────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                     KanbanToolbar                            │   │
│  │  [Filters] [Search] [View Toggle] [Export] [Settings]        │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                     KanbanBoard                              │   │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐           │   │
│  │  │KanbanCol│ │KanbanCol│ │KanbanCol│ │KanbanCol│  ...      │   │
│  │  │ (Draft) │ │(Review) │ │(Approve)│ │(Active) │           │   │
│  │  │         │ │         │ │         │ │         │           │   │
│  │  │┌───────┐│ │┌───────┐│ │┌───────┐│ │┌───────┐│           │   │
│  │  ││ Card  ││ ││ Card  ││ ││ Card  ││ ││ Card  ││           │   │
│  │  │└───────┘│ │└───────┘│ │└───────┘│ │└───────┘│           │   │
│  │  │┌───────┐│ │┌───────┐│ │         │ │         │           │   │
│  │  ││ Card  ││ ││ Card  ││ │         │ │         │           │   │
│  │  │└───────┘│ │└───────┘│ │         │ │         │           │   │
│  │  └─────────┘ └─────────┘ └─────────┘ └─────────┘           │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                   KanbanFooter (Stats)                       │   │
│  │  Total: 45 | Draft: 12 | Review: 8 | Approved: 15 | Active: 10│   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

### 1.2 Data Flow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   API Layer  │────▶│  State Store │────▶│   UI Layer   │
│              │     │  (Zustand/   │     │  (React)     │
│  - Fetch     │◀────│   Redux)     │◀────│              │
│  - Update    │     │              │     │  - Render    │
│  - Subscribe │     │  - Items     │     │  - Events    │
└──────────────┘     │  - Filters   │     └──────────────┘
                     │  - Selection │
                     └──────────────┘
```

---

## 2. Column Configuration

### 2.1 Workflow Stages (Default)

```typescript
interface KanbanColumn {
  id: string;
  key: string;
  title: string;
  titleAr: string;
  color: string;
  icon: string;
  order: number;
  acceptsFrom: string[];      // Which columns can drop here
  allowDrop: boolean;
  allowCreate: boolean;
  wipLimit?: number;          // Work-in-progress limit
  autoTransition?: {
    condition: string;
    targetColumn: string;
  };
}

const defaultColumns: KanbanColumn[] = [
  {
    id: 'col-draft',
    key: 'draft',
    title: 'Draft',
    titleAr: 'مسودة',
    color: '#6B7280',
    icon: 'edit-3',
    order: 1,
    acceptsFrom: ['rejected'],
    allowDrop: true,
    allowCreate: true,
    wipLimit: null,
  },
  {
    id: 'col-pending-review',
    key: 'pending_review',
    title: 'Pending Review',
    titleAr: 'في انتظار المراجعة',
    color: '#F59E0B',
    icon: 'clock',
    order: 2,
    acceptsFrom: ['draft'],
    allowDrop: true,
    allowCreate: false,
    wipLimit: 20,
  },
  {
    id: 'col-in-review',
    key: 'in_review',
    title: 'In Review',
    titleAr: 'قيد المراجعة',
    color: '#3B82F6',
    icon: 'eye',
    order: 3,
    acceptsFrom: ['pending_review'],
    allowDrop: true,
    allowCreate: false,
    wipLimit: 10,
  },
  {
    id: 'col-pending-approval',
    key: 'pending_approval',
    title: 'Pending Approval',
    titleAr: 'في انتظار الموافقة',
    color: '#8B5CF6',
    icon: 'check-circle',
    order: 4,
    acceptsFrom: ['in_review'],
    allowDrop: true,
    allowCreate: false,
    wipLimit: 15,
  },
  {
    id: 'col-approved',
    key: 'approved',
    title: 'Approved',
    titleAr: 'معتمد',
    color: '#10B981',
    icon: 'check',
    order: 5,
    acceptsFrom: ['pending_approval'],
    allowDrop: true,
    allowCreate: false,
    wipLimit: null,
  },
  {
    id: 'col-rejected',
    key: 'rejected',
    title: 'Rejected',
    titleAr: 'مرفوض',
    color: '#EF4444',
    icon: 'x-circle',
    order: 6,
    acceptsFrom: ['in_review', 'pending_approval'],
    allowDrop: true,
    allowCreate: false,
    wipLimit: null,
  },
];
```

### 2.2 Stage-Specific Column Configurations

```yaml
# Per-stage column configurations
column_configs:
  assessment:
    columns:
      - { key: draft, title: "Not Started", titleAr: "لم يبدأ" }
      - { key: in_progress, title: "In Progress", titleAr: "قيد التنفيذ" }
      - { key: pending_review, title: "Awaiting Review", titleAr: "في انتظار المراجعة" }
      - { key: reviewed, title: "Reviewed", titleAr: "تمت المراجعة" }
      - { key: completed, title: "Completed", titleAr: "مكتمل" }

  risk:
    columns:
      - { key: identified, title: "Identified", titleAr: "محدد" }
      - { key: assessing, title: "Assessing", titleAr: "قيد التقييم" }
      - { key: treatment_planning, title: "Treatment Planning", titleAr: "تخطيط المعالجة" }
      - { key: treating, title: "Treating", titleAr: "قيد المعالجة" }
      - { key: monitoring, title: "Monitoring", titleAr: "قيد المراقبة" }
      - { key: closed, title: "Closed", titleAr: "مغلق" }

  compliance:
    columns:
      - { key: gap_identified, title: "Gap Identified", titleAr: "فجوة محددة" }
      - { key: remediation_planned, title: "Remediation Planned", titleAr: "خطة المعالجة" }
      - { key: remediation_in_progress, title: "In Remediation", titleAr: "قيد المعالجة" }
      - { key: verification, title: "Verification", titleAr: "التحقق" }
      - { key: compliant, title: "Compliant", titleAr: "متوافق" }

  evidence:
    columns:
      - { key: requested, title: "Requested", titleAr: "مطلوب" }
      - { key: collecting, title: "Collecting", titleAr: "قيد الجمع" }
      - { key: uploaded, title: "Uploaded", titleAr: "تم الرفع" }
      - { key: under_review, title: "Under Review", titleAr: "قيد المراجعة" }
      - { key: approved, title: "Approved", titleAr: "معتمد" }
      - { key: rejected, title: "Rejected", titleAr: "مرفوض" }
```

---

## 3. Card Component

### 3.1 Card Structure

```typescript
interface KanbanCard {
  id: string;
  serialCode: string;
  title: string;
  description?: string;
  status: string;
  priority: 'critical' | 'high' | 'medium' | 'low';
  owner: {
    id: string;
    name: string;
    avatar?: string;
  };
  dueDate?: Date;
  tags: string[];
  metrics?: CardMetrics;
  gateStatus?: GateStatus;
  actions: CardAction[];
  metadata: Record<string, any>;
  createdAt: Date;
  updatedAt: Date;
}

interface CardMetrics {
  score?: number;
  progress?: number;
  riskLevel?: number;
  complianceScore?: number;
}

interface GateStatus {
  canTransition: boolean;
  nextGate: string;
  blockers: GateBlocker[];
  warnings: GateWarning[];
}

interface CardAction {
  id: string;
  label: string;
  labelAr: string;
  icon: string;
  type: 'primary' | 'secondary' | 'danger';
  action: string;
  enabled: boolean;
  tooltip?: string;
}
```

### 3.2 Card Visual Design

```
┌─────────────────────────────────────────┐
│ ●●● Critical    [RSK-ACME-02-2026-00012]│  ← Priority indicator + Serial
├─────────────────────────────────────────┤
│                                         │
│  Data Breach Risk - Unpatched Systems   │  ← Title
│                                         │
│  ┌─────────────────────────────────┐   │
│  │ Risk Score: 20 (Critical)       │   │  ← Metrics panel
│  │ Progress: ████████░░ 75%        │   │
│  │ Due: 15 Jan 2026 (3 days)       │   │
│  └─────────────────────────────────┘   │
│                                         │
│  [Security] [Infrastructure] [Q1]       │  ← Tags
│                                         │
├─────────────────────────────────────────┤
│ 👤 Ahmed K.        ⚠️ 2 blockers        │  ← Owner + Gate status
├─────────────────────────────────────────┤
│ [Assess] [Treat] [View] [···]           │  ← Action buttons
└─────────────────────────────────────────┘
```

### 3.3 Card Component Implementation

```tsx
interface KanbanCardProps {
  card: KanbanCard;
  isDragging: boolean;
  onAction: (cardId: string, action: string) => void;
  onSelect: (cardId: string) => void;
  locale: 'en' | 'ar';
}

const KanbanCardComponent: React.FC<KanbanCardProps> = ({
  card,
  isDragging,
  onAction,
  onSelect,
  locale,
}) => {
  const priorityColors = {
    critical: '#DC2626',
    high: '#F59E0B',
    medium: '#3B82F6',
    low: '#6B7280',
  };

  const isOverdue = card.dueDate && new Date(card.dueDate) < new Date();
  const hasBlockers = card.gateStatus?.blockers?.length > 0;

  return (
    <div
      className={cn(
        'kanban-card',
        isDragging && 'kanban-card--dragging',
        isOverdue && 'kanban-card--overdue',
        hasBlockers && 'kanban-card--blocked'
      )}
      onClick={() => onSelect(card.id)}
    >
      {/* Header */}
      <div className="kanban-card__header">
        <span
          className="kanban-card__priority"
          style={{ backgroundColor: priorityColors[card.priority] }}
        >
          {card.priority}
        </span>
        <code className="kanban-card__serial">{card.serialCode}</code>
      </div>

      {/* Title */}
      <h4 className="kanban-card__title">{card.title}</h4>

      {/* Metrics */}
      {card.metrics && (
        <div className="kanban-card__metrics">
          {card.metrics.score !== undefined && (
            <MetricBadge
              label={locale === 'ar' ? 'الدرجة' : 'Score'}
              value={card.metrics.score}
              max={25}
              thresholds={[5, 10, 15, 20]}
            />
          )}
          {card.metrics.progress !== undefined && (
            <ProgressBar value={card.metrics.progress} />
          )}
          {card.dueDate && (
            <DueDateBadge date={card.dueDate} isOverdue={isOverdue} />
          )}
        </div>
      )}

      {/* Tags */}
      <div className="kanban-card__tags">
        {card.tags.map((tag) => (
          <span key={tag} className="kanban-card__tag">
            {tag}
          </span>
        ))}
      </div>

      {/* Footer */}
      <div className="kanban-card__footer">
        <div className="kanban-card__owner">
          <Avatar user={card.owner} size="sm" />
          <span>{card.owner.name}</span>
        </div>
        {hasBlockers && (
          <Tooltip content={formatBlockers(card.gateStatus.blockers, locale)}>
            <span className="kanban-card__blockers">
              ⚠️ {card.gateStatus.blockers.length}
            </span>
          </Tooltip>
        )}
      </div>

      {/* Actions */}
      <div className="kanban-card__actions">
        {card.actions.slice(0, 3).map((action) => (
          <Button
            key={action.id}
            size="sm"
            variant={action.type}
            disabled={!action.enabled}
            onClick={(e) => {
              e.stopPropagation();
              onAction(card.id, action.action);
            }}
          >
            <Icon name={action.icon} />
            {locale === 'ar' ? action.labelAr : action.label}
          </Button>
        ))}
        {card.actions.length > 3 && (
          <DropdownMenu actions={card.actions.slice(3)} onAction={onAction} />
        )}
      </div>
    </div>
  );
};
```

---

## 4. Drag and Drop System

### 4.1 DnD Configuration

```typescript
interface DragDropConfig {
  enableDragDrop: boolean;
  validateDrop: (source: string, target: string, card: KanbanCard) => DropValidation;
  onDragStart: (cardId: string, sourceColumn: string) => void;
  onDragEnd: (result: DragResult) => void;
  onDropValidationFail: (validation: DropValidation) => void;
}

interface DropValidation {
  allowed: boolean;
  reason?: string;
  reasonAr?: string;
  gateCheck?: GateCheckResult;
  requiresConfirmation?: boolean;
  confirmationMessage?: string;
}

interface DragResult {
  cardId: string;
  sourceColumn: string;
  targetColumn: string;
  newIndex: number;
  timestamp: Date;
}
```

### 4.2 Gate-Aware Drop Validation

```typescript
async function validateDrop(
  sourceColumn: string,
  targetColumn: string,
  card: KanbanCard
): Promise<DropValidation> {
  // Check if transition is allowed by column config
  const targetConfig = columns.find((c) => c.key === targetColumn);
  if (!targetConfig.acceptsFrom.includes(sourceColumn)) {
    return {
      allowed: false,
      reason: `Cannot move directly from ${sourceColumn} to ${targetColumn}`,
      reasonAr: `لا يمكن النقل مباشرة من ${sourceColumn} إلى ${targetColumn}`,
    };
  }

  // Check WIP limit
  const currentWip = getColumnItemCount(targetColumn);
  if (targetConfig.wipLimit && currentWip >= targetConfig.wipLimit) {
    return {
      allowed: false,
      reason: `WIP limit reached (${targetConfig.wipLimit})`,
      reasonAr: `تم الوصول إلى الحد الأقصى للعمل الجاري (${targetConfig.wipLimit})`,
    };
  }

  // Check gate conditions
  const gateResult = await gateEvaluationService.evaluateTransition(
    card.id,
    sourceColumn,
    targetColumn
  );

  if (!gateResult.passed) {
    return {
      allowed: false,
      reason: `Gate requirements not met: ${gateResult.blockers.join(', ')}`,
      reasonAr: `متطلبات البوابة غير مستوفاة`,
      gateCheck: gateResult,
    };
  }

  // Check if approval is required
  if (gateResult.requiresApproval) {
    return {
      allowed: true,
      requiresConfirmation: true,
      confirmationMessage: `This transition requires ${gateResult.approvalLevel} approval. Proceed?`,
    };
  }

  return { allowed: true };
}
```

### 4.3 DnD Event Handlers

```typescript
const handleDragEnd = async (result: DropResult) => {
  const { draggableId, source, destination } = result;

  if (!destination) return;
  if (
    source.droppableId === destination.droppableId &&
    source.index === destination.index
  ) {
    return;
  }

  const card = getCardById(draggableId);
  const validation = await validateDrop(
    source.droppableId,
    destination.droppableId,
    card
  );

  if (!validation.allowed) {
    showToast({
      type: 'error',
      message: validation.reason,
      messageAr: validation.reasonAr,
    });
    return;
  }

  if (validation.requiresConfirmation) {
    const confirmed = await showConfirmDialog({
      title: 'Confirm Transition',
      message: validation.confirmationMessage,
    });
    if (!confirmed) return;
  }

  // Optimistic update
  setCards((prev) =>
    moveCard(prev, draggableId, source.droppableId, destination.droppableId, destination.index)
  );

  // Server update
  try {
    await workflowService.transition(card.id, destination.droppableId);
    showToast({
      type: 'success',
      message: 'Item moved successfully',
      messageAr: 'تم نقل العنصر بنجاح',
    });
  } catch (error) {
    // Rollback
    setCards((prev) =>
      moveCard(prev, draggableId, destination.droppableId, source.droppableId, source.index)
    );
    showToast({
      type: 'error',
      message: error.message,
    });
  }
};
```

---

## 5. Direct Actionable Items

### 5.1 Inline Actions

```typescript
interface InlineAction {
  id: string;
  type: 'button' | 'dropdown' | 'toggle' | 'input';
  label: string;
  labelAr: string;
  icon: string;
  action: string;
  variant: 'primary' | 'secondary' | 'danger' | 'ghost';
  position: 'card' | 'hover' | 'menu';
  conditions: ActionCondition[];
  shortcuts?: string[];
}

const standardActions: InlineAction[] = [
  {
    id: 'action-view',
    type: 'button',
    label: 'View Details',
    labelAr: 'عرض التفاصيل',
    icon: 'eye',
    action: 'VIEW_DETAILS',
    variant: 'ghost',
    position: 'card',
    conditions: [],
    shortcuts: ['Enter', 'Space'],
  },
  {
    id: 'action-edit',
    type: 'button',
    label: 'Edit',
    labelAr: 'تعديل',
    icon: 'edit-2',
    action: 'EDIT',
    variant: 'ghost',
    position: 'hover',
    conditions: [{ field: 'status', operator: 'in', value: ['draft', 'rejected'] }],
    shortcuts: ['e'],
  },
  {
    id: 'action-submit',
    type: 'button',
    label: 'Submit for Review',
    labelAr: 'إرسال للمراجعة',
    icon: 'send',
    action: 'SUBMIT_REVIEW',
    variant: 'primary',
    position: 'card',
    conditions: [
      { field: 'status', operator: 'eq', value: 'draft' },
      { field: 'gateStatus.canTransition', operator: 'eq', value: true },
    ],
    shortcuts: ['s'],
  },
  {
    id: 'action-approve',
    type: 'button',
    label: 'Approve',
    labelAr: 'موافقة',
    icon: 'check',
    action: 'APPROVE',
    variant: 'primary',
    position: 'card',
    conditions: [
      { field: 'status', operator: 'eq', value: 'pending_approval' },
      { field: 'userRole', operator: 'in', value: ['approver', 'admin'] },
    ],
    shortcuts: ['a'],
  },
  {
    id: 'action-reject',
    type: 'button',
    label: 'Reject',
    labelAr: 'رفض',
    icon: 'x',
    action: 'REJECT',
    variant: 'danger',
    position: 'card',
    conditions: [
      { field: 'status', operator: 'in', value: ['in_review', 'pending_approval'] },
      { field: 'userRole', operator: 'in', value: ['reviewer', 'approver', 'admin'] },
    ],
    shortcuts: ['r'],
  },
  {
    id: 'action-assign',
    type: 'dropdown',
    label: 'Assign',
    labelAr: 'تعيين',
    icon: 'user-plus',
    action: 'ASSIGN',
    variant: 'ghost',
    position: 'menu',
    conditions: [{ field: 'userRole', operator: 'in', value: ['manager', 'admin'] }],
  },
  {
    id: 'action-add-evidence',
    type: 'button',
    label: 'Add Evidence',
    labelAr: 'إضافة دليل',
    icon: 'paperclip',
    action: 'ADD_EVIDENCE',
    variant: 'secondary',
    position: 'card',
    conditions: [
      { field: 'entityType', operator: 'in', value: ['control', 'compliance', 'risk'] },
    ],
  },
  {
    id: 'action-escalate',
    type: 'button',
    label: 'Escalate',
    labelAr: 'تصعيد',
    icon: 'alert-triangle',
    action: 'ESCALATE',
    variant: 'danger',
    position: 'menu',
    conditions: [{ field: 'isOverdue', operator: 'eq', value: true }],
  },
];
```

### 5.2 Action Handler

```typescript
const actionHandlers: Record<string, ActionHandler> = {
  VIEW_DETAILS: async (card) => {
    router.push(`/items/${card.entityType}/${card.id}`);
  },

  EDIT: async (card) => {
    openEditModal(card);
  },

  SUBMIT_REVIEW: async (card) => {
    const validation = await gateService.validateTransition(card.id, 'pending_review');
    if (!validation.passed) {
      showBlockersModal(validation.blockers);
      return;
    }
    await workflowService.submit(card.id);
    refreshCard(card.id);
  },

  APPROVE: async (card) => {
    const confirmed = await showConfirmDialog({
      title: 'Confirm Approval',
      message: `Are you sure you want to approve ${card.serialCode}?`,
    });
    if (confirmed) {
      await workflowService.approve(card.id);
      refreshCard(card.id);
    }
  },

  REJECT: async (card) => {
    const reason = await showRejectDialog();
    if (reason) {
      await workflowService.reject(card.id, reason);
      refreshCard(card.id);
    }
  },

  ASSIGN: async (card, params) => {
    await workflowService.assign(card.id, params.userId);
    refreshCard(card.id);
  },

  ADD_EVIDENCE: async (card) => {
    openEvidenceUploadModal(card.id);
  },

  ESCALATE: async (card) => {
    const reason = await showEscalationDialog();
    if (reason) {
      await workflowService.escalate(card.id, reason);
      refreshCard(card.id);
    }
  },
};
```

---

## 6. Advanced Views

### 6.1 View Modes

```typescript
type ViewMode = 'kanban' | 'table' | 'timeline' | 'calendar' | 'matrix';

interface ViewConfig {
  mode: ViewMode;
  groupBy?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  filters?: Filter[];
  columns?: string[];  // For table view
  dateRange?: DateRange;  // For timeline/calendar
}
```

### 6.2 Table View

```
┌──────────────────────────────────────────────────────────────────────────────────┐
│ Serial Code      │ Title                    │ Status  │ Priority │ Owner │ Due   │
├──────────────────────────────────────────────────────────────────────────────────┤
│ RSK-ACME-02-001  │ Data Breach Risk         │ ●Draft  │ Critical │ Ahmed │ Jan 15│
│ RSK-ACME-02-002  │ Vendor Access Control    │ ●Review │ High     │ Sara  │ Jan 20│
│ CMP-ACME-03-001  │ SAMA-CSF Gap Analysis    │ ●Active │ Medium   │ Omar  │ Feb 01│
│ EVD-ACME-00-001  │ Firewall Config Evidence │ ●Pending│ Low      │ Ali   │ Jan 10│
└──────────────────────────────────────────────────────────────────────────────────┘

[Bulk Actions: Assign | Move | Export | Delete]
```

### 6.3 Timeline View

```
                    Jan 2026                              Feb 2026
    ─────────────────────────────────────────────────────────────────────

    Assessment Phase
    ├── RSK-001 ████████████████░░░░░░░░░░░░░░░░░░░░░░░
    │           [In Progress - 60%]
    │
    ├── RSK-002     ████████████████████░░░░░░░░░░░░░░
    │               [In Progress - 70%]
    │
    Risk Treatment
    ├── RSK-T-001           ░░░░░░░░░░░░░░░░░░░░░░░░░░░░
    │                       [Not Started]
    │
    Compliance
    ├── CMP-001                 ████████████████████████░░
    │                           [In Progress - 85%]

    ─────────────────────────────────────────────────────────────────────
    Legend: ████ Completed  ░░░░ Remaining  ▓▓▓▓ Overdue
```

### 6.4 Risk Matrix View

```
┌─────────────────────────────────────────────────────────────────┐
│                    RISK HEAT MAP                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│     │  1-Rare  │ 2-Unlikely │ 3-Possible │ 4-Likely │ 5-Certain│
│ ────┼──────────┼────────────┼────────────┼──────────┼──────────│
│  5  │          │            │    ◉ 2     │   ◉◉ 4   │   ◉ 1    │
│ Cat │   Low    │   Medium   │    High    │ Critical │ Critical │
│ ────┼──────────┼────────────┼────────────┼──────────┼──────────│
│  4  │          │     ◉ 1    │    ◉ 3     │   ◉◉ 5   │   ◉ 2    │
│ Maj │   Low    │   Medium   │    High    │   High   │ Critical │
│ ────┼──────────┼────────────┼────────────┼──────────┼──────────│
│  3  │   ◉ 1    │     ◉ 2    │    ◉◉ 6    │   ◉ 3    │          │
│ Mod │   Low    │    Low     │   Medium   │   High   │   High   │
│ ────┼──────────┼────────────┼────────────┼──────────┼──────────│
│  2  │   ◉ 3    │            │     ◉ 1    │          │          │
│ Min │   Low    │    Low     │    Low     │  Medium  │  Medium  │
│ ────┼──────────┼────────────┼────────────┼──────────┼──────────│
│  1  │   ◉◉ 8   │     ◉ 4    │            │          │          │
│ Neg │   Low    │    Low     │    Low     │   Low    │  Medium  │
│                                                                 │
│     ◉ = Risk item (click to view)                              │
└─────────────────────────────────────────────────────────────────┘
```

### 6.5 Calendar View

```
┌─────────────────────────────────────────────────────────────────┐
│                    January 2026                                 │
├─────┬─────┬─────┬─────┬─────┬─────┬─────┬───────────────────────┤
│ Sun │ Mon │ Tue │ Wed │ Thu │ Fri │ Sat │                       │
├─────┼─────┼─────┼─────┼─────┼─────┼─────┤     Selected Day      │
│     │     │     │  1  │  2  │  3  │  4  │     ─────────────     │
│     │     │     │     │     │     │     │                       │
├─────┼─────┼─────┼─────┼─────┼─────┼─────┤     Jan 15, 2026      │
│  5  │  6  │  7  │  8  │  9  │ 10  │ 11  │                       │
│     │     │     │     │     │ ●2  │     │     Due Items:        │
├─────┼─────┼─────┼─────┼─────┼─────┼─────┤     • RSK-001 (Risk)  │
│ 12  │ 13  │ 14  │ 15  │ 16  │ 17  │ 18  │     • EVD-003 (Evid)  │
│     │     │     │ ●3  │     │     │     │                       │
├─────┼─────┼─────┼─────┼─────┼─────┼─────┤     Milestones:       │
│ 19  │ 20  │ 21  │ 22  │ 23  │ 24  │ 25  │     • Q1 Assessment   │
│     │ ●1  │     │     │     │     │     │       Deadline        │
├─────┼─────┼─────┼─────┼─────┼─────┼─────┤                       │
│ 26  │ 27  │ 28  │ 29  │ 30  │ 31  │     │     [View All]        │
│     │     │     │     │     │ ●5  │     │     [Add Reminder]    │
└─────┴─────┴─────┴─────┴─────┴─────┴─────┴───────────────────────┘
```

---

## 7. Filtering and Search

### 7.1 Filter Configuration

```typescript
interface FilterConfig {
  field: string;
  label: string;
  labelAr: string;
  type: 'select' | 'multiselect' | 'date' | 'daterange' | 'text' | 'number' | 'boolean';
  options?: FilterOption[];
  operators?: FilterOperator[];
  defaultValue?: any;
}

const filterConfigs: FilterConfig[] = [
  {
    field: 'status',
    label: 'Status',
    labelAr: 'الحالة',
    type: 'multiselect',
    options: [
      { value: 'draft', label: 'Draft', labelAr: 'مسودة' },
      { value: 'pending_review', label: 'Pending Review', labelAr: 'في انتظار المراجعة' },
      { value: 'in_review', label: 'In Review', labelAr: 'قيد المراجعة' },
      { value: 'approved', label: 'Approved', labelAr: 'معتمد' },
      { value: 'rejected', label: 'Rejected', labelAr: 'مرفوض' },
    ],
  },
  {
    field: 'priority',
    label: 'Priority',
    labelAr: 'الأولوية',
    type: 'multiselect',
    options: [
      { value: 'critical', label: 'Critical', labelAr: 'حرج' },
      { value: 'high', label: 'High', labelAr: 'عالي' },
      { value: 'medium', label: 'Medium', labelAr: 'متوسط' },
      { value: 'low', label: 'Low', labelAr: 'منخفض' },
    ],
  },
  {
    field: 'owner',
    label: 'Owner',
    labelAr: 'المالك',
    type: 'multiselect',
    options: [], // Populated dynamically
  },
  {
    field: 'dueDate',
    label: 'Due Date',
    labelAr: 'تاريخ الاستحقاق',
    type: 'daterange',
  },
  {
    field: 'tags',
    label: 'Tags',
    labelAr: 'العلامات',
    type: 'multiselect',
    options: [], // Populated dynamically
  },
  {
    field: 'stage',
    label: 'Stage',
    labelAr: 'المرحلة',
    type: 'multiselect',
    options: [
      { value: 1, label: 'Assessment', labelAr: 'التقييم' },
      { value: 2, label: 'Risk', labelAr: 'المخاطر' },
      { value: 3, label: 'Compliance', labelAr: 'الامتثال' },
      { value: 4, label: 'Resilience', labelAr: 'المرونة' },
      { value: 5, label: 'Excellence', labelAr: 'التميز' },
      { value: 6, label: 'Sustainability', labelAr: 'الاستدامة' },
    ],
  },
  {
    field: 'riskScore',
    label: 'Risk Score',
    labelAr: 'درجة المخاطر',
    type: 'number',
    operators: ['eq', 'gt', 'gte', 'lt', 'lte', 'between'],
  },
  {
    field: 'isOverdue',
    label: 'Overdue',
    labelAr: 'متأخر',
    type: 'boolean',
  },
];
```

### 7.2 Search Implementation

```typescript
interface SearchConfig {
  placeholder: string;
  placeholderAr: string;
  searchFields: string[];
  debounceMs: number;
  minLength: number;
}

const searchConfig: SearchConfig = {
  placeholder: 'Search by title, serial code, or description...',
  placeholderAr: 'البحث بالعنوان أو الرمز التسلسلي أو الوصف...',
  searchFields: ['title', 'serialCode', 'description', 'tags'],
  debounceMs: 300,
  minLength: 2,
};
```

---

## 8. Real-Time Updates

### 8.1 WebSocket Integration

```typescript
interface KanbanWebSocketConfig {
  url: string;
  channels: string[];
  reconnectInterval: number;
  maxReconnectAttempts: number;
}

interface KanbanEvent {
  type: 'CARD_CREATED' | 'CARD_UPDATED' | 'CARD_MOVED' | 'CARD_DELETED' | 'COLUMN_UPDATED';
  payload: {
    cardId?: string;
    columnId?: string;
    data: any;
    timestamp: Date;
    actor: string;
  };
}

// WebSocket handler
const useKanbanWebSocket = (config: KanbanWebSocketConfig) => {
  const [connected, setConnected] = useState(false);
  const socketRef = useRef<WebSocket>();

  useEffect(() => {
    const connect = () => {
      socketRef.current = new WebSocket(config.url);

      socketRef.current.onopen = () => {
        setConnected(true);
        config.channels.forEach((channel) => {
          socketRef.current.send(JSON.stringify({ type: 'SUBSCRIBE', channel }));
        });
      };

      socketRef.current.onmessage = (event) => {
        const message: KanbanEvent = JSON.parse(event.data);
        handleKanbanEvent(message);
      };

      socketRef.current.onclose = () => {
        setConnected(false);
        setTimeout(connect, config.reconnectInterval);
      };
    };

    connect();

    return () => socketRef.current?.close();
  }, [config]);

  return { connected };
};
```

### 8.2 Optimistic Updates

```typescript
const useOptimisticUpdate = () => {
  const [pendingUpdates, setPendingUpdates] = useState<Map<string, any>>(new Map());

  const applyOptimistic = (cardId: string, update: Partial<KanbanCard>) => {
    setPendingUpdates((prev) => new Map(prev).set(cardId, update));
  };

  const confirmUpdate = (cardId: string) => {
    setPendingUpdates((prev) => {
      const next = new Map(prev);
      next.delete(cardId);
      return next;
    });
  };

  const rollbackUpdate = (cardId: string) => {
    setPendingUpdates((prev) => {
      const next = new Map(prev);
      next.delete(cardId);
      return next;
    });
    // Trigger refresh from server
  };

  return { pendingUpdates, applyOptimistic, confirmUpdate, rollbackUpdate };
};
```

---

## 9. Accessibility

### 9.1 Keyboard Navigation

```typescript
const keyboardShortcuts: Record<string, KeyboardShortcut> = {
  // Navigation
  'ArrowUp': { action: 'SELECT_PREV_CARD', scope: 'board' },
  'ArrowDown': { action: 'SELECT_NEXT_CARD', scope: 'board' },
  'ArrowLeft': { action: 'SELECT_PREV_COLUMN', scope: 'board' },
  'ArrowRight': { action: 'SELECT_NEXT_COLUMN', scope: 'board' },
  'Home': { action: 'SELECT_FIRST_CARD', scope: 'column' },
  'End': { action: 'SELECT_LAST_CARD', scope: 'column' },

  // Actions
  'Enter': { action: 'OPEN_CARD', scope: 'card' },
  'Space': { action: 'TOGGLE_SELECT', scope: 'card' },
  'e': { action: 'EDIT_CARD', scope: 'card' },
  's': { action: 'SUBMIT_CARD', scope: 'card' },
  'a': { action: 'APPROVE_CARD', scope: 'card' },
  'r': { action: 'REJECT_CARD', scope: 'card' },
  'Delete': { action: 'DELETE_CARD', scope: 'card', requiresConfirm: true },

  // Drag simulation
  'd': { action: 'START_DRAG', scope: 'card' },
  'Escape': { action: 'CANCEL_DRAG', scope: 'drag' },

  // Search and filter
  '/': { action: 'FOCUS_SEARCH', scope: 'board' },
  'f': { action: 'OPEN_FILTERS', scope: 'board' },

  // View switching
  '1': { action: 'SWITCH_VIEW', payload: 'kanban', scope: 'board' },
  '2': { action: 'SWITCH_VIEW', payload: 'table', scope: 'board' },
  '3': { action: 'SWITCH_VIEW', payload: 'timeline', scope: 'board' },
};
```

### 9.2 ARIA Labels

```tsx
<div
  role="application"
  aria-label={locale === 'ar' ? 'لوحة كانبان' : 'Kanban Board'}
>
  <div role="region" aria-label="Filters and search">
    {/* Toolbar */}
  </div>

  <div
    role="list"
    aria-label={locale === 'ar' ? 'أعمدة سير العمل' : 'Workflow columns'}
  >
    {columns.map((column) => (
      <div
        key={column.id}
        role="listitem"
        aria-label={`${locale === 'ar' ? column.titleAr : column.title} - ${getColumnCount(column.id)} items`}
      >
        <div role="list" aria-label={`Cards in ${column.title}`}>
          {getCardsForColumn(column.id).map((card) => (
            <article
              key={card.id}
              role="listitem"
              aria-label={card.title}
              tabIndex={0}
            >
              {/* Card content */}
            </article>
          ))}
        </div>
      </div>
    ))}
  </div>
</div>
```

---

## 10. Performance Optimization

### 10.1 Virtualization

```typescript
// For large boards, use windowing
import { FixedSizeList } from 'react-window';

const VirtualizedColumn: React.FC<VirtualizedColumnProps> = ({
  cards,
  columnHeight,
  cardHeight,
}) => (
  <FixedSizeList
    height={columnHeight}
    itemCount={cards.length}
    itemSize={cardHeight}
    width="100%"
  >
    {({ index, style }) => (
      <div style={style}>
        <KanbanCardComponent card={cards[index]} />
      </div>
    )}
  </FixedSizeList>
);
```

### 10.2 Memoization

```typescript
// Memoize cards to prevent unnecessary re-renders
const MemoizedCard = React.memo(
  KanbanCardComponent,
  (prev, next) =>
    prev.card.id === next.card.id &&
    prev.card.updatedAt === next.card.updatedAt &&
    prev.isDragging === next.isDragging
);

// Memoize column card lists
const columnCards = useMemo(
  () => cards.filter((card) => card.status === columnKey),
  [cards, columnKey]
);
```

---

## 11. Theming

### 11.1 CSS Variables

```css
:root {
  /* Colors */
  --kanban-bg: #f3f4f6;
  --kanban-column-bg: #ffffff;
  --kanban-card-bg: #ffffff;
  --kanban-card-border: #e5e7eb;
  --kanban-card-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);

  /* Priority colors */
  --priority-critical: #dc2626;
  --priority-high: #f59e0b;
  --priority-medium: #3b82f6;
  --priority-low: #6b7280;

  /* Status colors */
  --status-draft: #6b7280;
  --status-pending: #f59e0b;
  --status-active: #3b82f6;
  --status-approved: #10b981;
  --status-rejected: #ef4444;

  /* Spacing */
  --kanban-gap: 16px;
  --card-padding: 12px;
  --column-width: 320px;

  /* Typography */
  --card-title-size: 14px;
  --card-meta-size: 12px;
}

/* Dark mode */
[data-theme='dark'] {
  --kanban-bg: #111827;
  --kanban-column-bg: #1f2937;
  --kanban-card-bg: #374151;
  --kanban-card-border: #4b5563;
}

/* RTL support */
[dir='rtl'] {
  --kanban-direction: rtl;
}
```

---

## 12. Mobile Responsiveness

### 12.1 Responsive Breakpoints

```scss
// Mobile-first responsive design
.kanban-board {
  display: flex;
  gap: var(--kanban-gap);
  overflow-x: auto;
  padding: var(--kanban-gap);

  // Mobile: Single column view
  @media (max-width: 640px) {
    flex-direction: column;

    .kanban-column {
      width: 100%;
      max-height: 50vh;
    }
  }

  // Tablet: 2-3 columns visible
  @media (min-width: 641px) and (max-width: 1024px) {
    .kanban-column {
      min-width: 280px;
      flex: 1;
    }
  }

  // Desktop: All columns visible
  @media (min-width: 1025px) {
    .kanban-column {
      width: var(--column-width);
      flex-shrink: 0;
    }
  }
}
```

### 12.2 Touch Interactions

```typescript
const touchConfig = {
  // Long press to start drag on mobile
  longPressDelay: 500,

  // Swipe actions
  swipeThreshold: 50,
  swipeActions: {
    left: 'QUICK_REJECT',
    right: 'QUICK_APPROVE',
  },

  // Pull to refresh
  pullToRefresh: true,
  pullThreshold: 80,
};
```

---

## Summary

This Kanban UI specification provides:

1. **Visual Workflow Management**: Drag-drop board with gate-aware transitions
2. **Direct Actions**: Context-aware action buttons on every card
3. **Multiple Views**: Kanban, table, timeline, calendar, and matrix views
4. **Real-Time Updates**: WebSocket integration for collaborative use
5. **Gate Integration**: Visual blockers and validation on transitions
6. **Accessibility**: Full keyboard navigation and ARIA support
7. **Bilingual**: English/Arabic with RTL support
8. **Mobile-Ready**: Responsive design with touch interactions
9. **Performance**: Virtualization and memoization for large boards
10. **Theming**: CSS variables for customization and dark mode
