# Structured Development: frontend

9단계 AI Agent 기반 코드 작성 프로세스 - Set up a workflow for any visitor to the front end to pop up his name and any info from his IP as friendly.

---

## Overview

This workflow orchestrates 6 agent(s) to accomplish the task.

## Output Configuration

This workflow's artifacts are stored as follows:

- **Base Path**: `output/`
- **Project Folder**: Ask user at runtime
- **Structure**: Simple (all in root)

### Execution Start

1. **Ask user for project name**: Prompt the user to provide a project name
2. Create output folder: `output/{project_name}/`
3. Begin workflow execution

---

## Shared Documents

### Project Analysis

# Project Analysis

프로젝트 구조 및 컨벤션 분석 결과

<!-- Content will be filled by agents using [[project-analysis]] -->

### Blueprint Report

# Blueprint Report

## Summary

**Review Status**: ✅ APPROVED / ⚠️ NEEDS_CHANGES / ❌ REJECTED

**Reviewer**: [Agent ID]
**Date**: 2026-01-20T13:18:28.849Z

## Overall Assessment
<!-- High-level summary of the review -->

| Category | Score | Notes |
|----------|-------|-------|
| Code Quality | ⭐⭐⭐⭐⭐ | ... |
| Security | ⭐⭐⭐⭐⭐ | ... |
| Performance | ⭐⭐⭐⭐⭐ | ... |
| Testing | ⭐⭐⭐⭐⭐ | ... |
| Documentation | ⭐⭐⭐⭐⭐ | ... |

## Findings

### 🔴 Critical Issues
<!-- Must fix before approval -->

1. **[File:Line]** - Issue description
   - **Problem**:
   - **Recommendation**:
   - **Code**:
   ```typescript
   // problematic code
   ```

### 🟡 Warnings
<!-- Should fix, but not blocking -->

1. **[File:Line]** - Issue description
   - **Problem**:
   - **Recommendation**:

### 🟢 Suggestions
<!-- Nice to have improvements -->

1. **[File:Line]** - Suggestion
   - **Improvement**:

## What's Good
<!-- Positive feedback -->

- ✅ Good practice 1
- ✅ Good practice 2

## Checklist

### Code Quality
- [ ] Follows coding standards
- [ ] No code duplication
- [ ] Proper error handling
- [ ] Clear naming conventions

### Security
- [ ] No hardcoded credentials
- [ ] Input validation in place
- [ ] No SQL/XSS vulnerabilities
- [ ] Proper authentication/authorization

### Performance
- [ ] No N+1 queries
- [ ] Proper caching
- [ ] Optimized algorithms
- [ ] No memory leaks

### Testing
- [ ] Unit tests present
- [ ] Tests are meaningful
- [ ] Edge cases covered
- [ ] Mocks used appropriately

## Decision

**Final Decision**: APPROVED / NEEDS_CHANGES

**Reason**:
<!-- Explain the decision -->

**Required Changes** (if NEEDS_CHANGES):
1. [ ] Change 1
2. [ ] Change 2

---
*Review completed: 2026-01-20T13:18:28.849Z*


<!-- Document binding: [[blueprint-report]] -->

### Guardrails

# Guardrails

가드레일 및 안전 가이드라인

<!-- Content will be filled by agents using [[guardrails]] -->

### Code Convention

# Code Convention

코드 스타일 가이드 (텍스트

<!-- Content will be filled by agents using [[code-convention]] -->

### Project Guidelines

# Project Guidelines

프로젝트 구조 가이드 (텍스트

<!-- Content will be filled by agents using [[project-guidelines]] -->

## Execution Steps

### Step 1: Requirements Analyst

**Description:** Requirements Analyst: 요구사항 분석 → 스펙 문서(MD) 작성

Use the Task tool to invoke the `requirements-analyst` agent:

```
Task(subagent_type="requirements-analyst", prompt="[Your task description here]")
```

### Step 2: Shahin

**Description:** shahin

Use the Task tool to invoke the `shahin` agent:

```
Task(subagent_type="shahin", prompt="[Your task description here]")
```

⚠️ **Feedback Loop Enabled**
- Check the agent's response for success/failure status
- Retry up to 3 times if needed

⚠️ **Human Approval Required**
- After this agent completes, ask the user: "Please review the work completed by this agent. Do you approve?"
- Wait for user confirmation before proceeding

### Step 3: Architect

**Description:** Architect: 아키텍처 설계 → 설계 문서 및 다이어그램 코드 작성

Use the Task tool to invoke the `architect` agent:

```
Task(subagent_type="architect", prompt="[Your task description here]")
```

### Step 4: Implementer

**Description:** Implementer: 소스 코드 구현

Use the Task tool to invoke the `implementer` agent:

```
Task(subagent_type="implementer", prompt="[Your task description here]")
```

### Step 5: Test Writer

**Description:** Test Writer: 테스트 코드 작성

Use the Task tool to invoke the `test-writer` agent:

```
Task(subagent_type="test-writer", prompt="[Your task description here]")
```

### Step 6: Code Reviewer

**Description:** Code Reviewer: 코드 품질/보안 검토

Use the Task tool to invoke the `code-reviewer` agent:

```
Task(subagent_type="code-reviewer", prompt="[Your task description here]")
```

⚠️ **Feedback Loop Enabled**
- Check the agent's response for success/failure status
- Retry up to 2 times if needed

---

## Important Notes

1. Execute agents in the order specified above
2. Pass relevant context and previous results to each agent
3. Handle feedback loops by checking response status
4. Log all execution steps for debugging
5. Stop execution if any agent fails fatally

