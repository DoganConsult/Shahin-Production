# Structured Development: frontend - Architecture

## Overview

This document describes the architecture of the Structured Development: frontend workflow.

## Agent Nodes (6)

### 1. Requirements Analyst
- **ID**: `requirements-analyst`
- **Description**: Requirements Analyst: 요구사항 분석 → 스펙 문서(MD) 작성
- **Model**: opus

### 2. Architect
- **ID**: `architect`
- **Description**: Architect: 아키텍처 설계 → 설계 문서 및 다이어그램 코드 작성
- **Model**: opus

### 3. Implementer
- **ID**: `implementer`
- **Description**: Implementer: 소스 코드 구현
- **Model**: sonnet

### 4. Test Writer
- **ID**: `test-writer`
- **Description**: Test Writer: 테스트 코드 작성
- **Model**: sonnet

### 5. Code Reviewer
- **ID**: `code-reviewer`
- **Description**: Code Reviewer: 코드 품질/보안 검토
- **Model**: opus

### 6. Shahin
- **ID**: `shahin`
- **Description**: shahin
- **Model**: sonnet

## Document Nodes (5)

### 1. Project Analysis
- **ID**: `project-analysis`

### 2. Blueprint Report
- **ID**: `blueprint-report`

### 3. Guardrails
- **ID**: `guardrails`

### 4. Code Convention
- **ID**: `code-convention`

### 5. Project Guidelines
- **ID**: `project-guidelines`

## Execution Flow

The workflow executes agents in topological order based on edge connections.

## Generated Files

- `.claude/commands/frontend.md` - Orchestrator command
- `.claude/agents/*.md` - Individual agent definitions
- `.mcp.json` - MCP server configuration (if applicable)

