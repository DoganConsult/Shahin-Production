# Claude Code Configuration

This project uses auto-generated multi-agent workflows.

## Available Commands

- `/frontend` - Run the Structured Development: frontend workflow

## Agents

- **@requirements-analyst**: Requirements Analyst: 요구사항 분석 → 스펙 문서(MD) 작성
- **@architect**: Architect: 아키텍처 설계 → 설계 문서 및 다이어그램 코드 작성
- **@implementer**: Implementer: 소스 코드 구현
- **@test-writer**: Test Writer: 테스트 코드 작성
- **@code-reviewer**: Code Reviewer: 코드 품질/보안 검토
- **@shahin**: shahin

## Important Notes

When running workflows:
- Follow orchestrator instructions exactly
- Always return structured responses as specified
- Do not skip validation steps
- Report all errors clearly

