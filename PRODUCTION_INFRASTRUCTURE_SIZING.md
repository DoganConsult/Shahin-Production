# Production Infrastructure Sizing Guide
## Self-Hosted Deployment + DoganCopilot AI Architecture
**Date**: 2026-01-16  
**Purpose**: Complete server sizing for self-hosted production with AI/LLM capabilities

---

## Executive Summary

### Total Server Requirements (Self-Hosted Production)

| Deployment Size | vCPUs | RAM | Storage | GPU | Monthly Cost* |
|-----------------|-------|-----|---------|-----|---------------|
| **Minimum (10 tenants)** | 16 | 64 GB | 500 GB SSD | Optional | ~$500/mo |
| **Standard (50 tenants)** | 32 | 128 GB | 1 TB NVMe | 1× RTX 4090 | ~$1,500/mo |
| **Enterprise (200+ tenants)** | 64 | 256 GB | 2 TB NVMe | 2× A100 40GB | ~$5,000/mo |

*Estimated self-hosted hardware amortization + electricity

---

## 1. CORE PLATFORM SERVICES

### 1.1 GRC Application Server

```yaml
Service: shahin-grc-portal
Replicas: 2-4 (HA)

Per Instance:
  CPU: 2 cores (request), 4 cores (limit)
  RAM: 2 GB (request), 4 GB (limit)
  
Total for HA (2 replicas):
  CPU: 4-8 cores
  RAM: 4-8 GB
```

### 1.2 PostgreSQL Database

```yaml
Service: postgres-master
Type: Master-Replica (Patroni for HA)

Master Node:
  CPU: 4 cores
  RAM: 8 GB (for 10-50 tenants)
  RAM: 16 GB (for 100+ tenants)
  Storage: 100 GB + (2 GB × tenants)
  
Replica Node (optional HA):
  CPU: 2 cores
  RAM: 4 GB
  Storage: Same as master

Connection Pooler (PgBouncer):
  CPU: 1 core
  RAM: 512 MB
```

### 1.3 Redis Cache/Sessions

```yaml
Service: redis-cluster
Type: Master + 2 Replicas + 3 Sentinels

Master:
  CPU: 1 core
  RAM: 1 GB (for 10-50 tenants)
  RAM: 4 GB (for 100+ tenants)
  
Per Replica (2×):
  CPU: 0.5 core
  RAM: 1 GB

Sentinels (3×):
  CPU: 0.1 core
  RAM: 128 MB
  
Total:
  CPU: 2.3 cores
  RAM: 3.4-6.4 GB
```

### 1.4 Hangfire Background Jobs

```yaml
Service: hangfire-worker
Replicas: 2-3

Per Worker:
  CPU: 1 core
  RAM: 1 GB

Total:
  CPU: 2-3 cores
  RAM: 2-3 GB
```

### 1.5 Reverse Proxy (Nginx/Traefik)

```yaml
Service: ingress
Replicas: 2

Per Instance:
  CPU: 0.5 core
  RAM: 256 MB
  
Total:
  CPU: 1 core
  RAM: 512 MB
```

---

## 2. AI/LLM SERVICES - BUILD vs BUY ANALYSIS

### 2.1 Decision Matrix

| Factor | Cloud LLM (Buy) | Local LLM (Build) | Recommendation |
|--------|-----------------|-------------------|----------------|
| **Initial Cost** | $0 | $5,000-$50,000 (GPU) | Buy for start |
| **Per-Request Cost** | $0.003-$0.06/1K tokens | ~$0 after hardware | Build for scale |
| **Latency** | 200-2000ms | 50-500ms | Build for speed |
| **Data Privacy** | Data leaves network | 100% on-premise | Build for compliance |
| **KSA Regulatory** | ⚠️ Data residency issues | ✅ Full compliance | **BUILD** |
| **Scalability** | Unlimited | Hardware limited | Buy for burst |
| **Model Updates** | Automatic | Manual | Buy for latest |
| **Offline Operation** | ❌ No | ✅ Yes | Build for reliability |

### 2.2 Recommended Hybrid Strategy

```
┌─────────────────────────────────────────────────────────────┐
│                    DOGAN COPILOT HYBRID AI                  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────┐    ┌──────────────────┐              │
│  │   LOCAL LLM      │    │   CLOUD LLM      │              │
│  │   (Primary)      │    │   (Fallback)     │              │
│  ├──────────────────┤    ├──────────────────┤              │
│  │ • Ollama/vLLM    │    │ • Claude API     │              │
│  │ • Llama 3.2 70B  │    │ • OpenAI GPT-4   │              │
│  │ • Qwen 2.5 72B   │    │ • Azure OpenAI   │              │
│  │ • Mistral Large  │    │ • Google Gemini  │              │
│  └────────┬─────────┘    └────────┬─────────┘              │
│           │                       │                         │
│           └───────────┬───────────┘                         │
│                       ▼                                     │
│              ┌────────────────┐                             │
│              │  UNIFIED AI    │                             │
│              │   SERVICE      │                             │
│              │  (Router)      │                             │
│              └────────┬───────┘                             │
│                       │                                     │
│    ┌──────────────────┼──────────────────┐                  │
│    ▼                  ▼                  ▼                  │
│ ┌──────────┐   ┌──────────┐   ┌──────────────┐             │
│ │ RAG      │   │ Agents   │   │ DoganCopilot │             │
│ │ Pipeline │   │ (6 types)│   │ Per-Org KB   │             │
│ └──────────┘   └──────────┘   └──────────────┘             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. LOCAL LLM OPTIONS

### 3.1 Recommended Models for GRC

| Model | Parameters | VRAM Required | Use Case | Quality |
|-------|------------|---------------|----------|---------|
| **Llama 3.2 8B** | 8B | 8 GB | Fast queries, chat | ⭐⭐⭐ |
| **Llama 3.2 70B** | 70B | 40 GB | Complex analysis | ⭐⭐⭐⭐⭐ |
| **Qwen 2.5 7B** | 7B | 8 GB | Multilingual (Arabic) | ⭐⭐⭐⭐ |
| **Qwen 2.5 72B** | 72B | 48 GB | Best Arabic support | ⭐⭐⭐⭐⭐ |
| **Mistral 7B** | 7B | 8 GB | General purpose | ⭐⭐⭐⭐ |
| **Mixtral 8x7B** | 46.7B | 24 GB | Best value/quality | ⭐⭐⭐⭐⭐ |
| **CodeLlama 34B** | 34B | 20 GB | Code generation | ⭐⭐⭐⭐ |

### 3.2 Local LLM Serving Options

| Solution | Type | GPU Support | Scalability | Effort |
|----------|------|-------------|-------------|--------|
| **Ollama** | Single node | ✅ | Low | ⭐ Easy |
| **vLLM** | Production | ✅ | High | ⭐⭐⭐ |
| **Text-Generation-Inference** | Production | ✅ | High | ⭐⭐⭐ |
| **LocalAI** | Multi-model | ✅ | Medium | ⭐⭐ |
| **LMStudio** | Development | ✅ | Low | ⭐ Easy |

### 3.3 Recommended: vLLM for Production

```yaml
# vLLM Configuration for Production
Service: vllm-server
Image: vllm/vllm-openai:latest

Resources:
  GPU: 1× NVIDIA A100 40GB (or 2× RTX 4090)
  CPU: 8 cores
  RAM: 32 GB
  Storage: 100 GB (for model weights)

Models Hosted:
  - Qwen 2.5 72B (Arabic + English)
  - Llama 3.2 70B (fallback)

Configuration:
  tensor_parallel_size: 1  # or 2 for multi-GPU
  max_model_len: 32768
  gpu_memory_utilization: 0.9
```

---

## 4. DOGAN COPILOT - PER-ORG KNOWLEDGE BASE

### 4.1 Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    DOGAN COPILOT ARCHITECTURE                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  TENANT A                 TENANT B                 TENANT C     │
│  ┌─────────────────┐      ┌─────────────────┐     ┌──────────┐ │
│  │ Knowledge Base  │      │ Knowledge Base  │     │ KB       │ │
│  │ ├─ Documents    │      │ ├─ Documents    │     │ ├─ Docs  │ │
│  │ ├─ Policies     │      │ ├─ Policies     │     │ ├─ Pol   │ │
│  │ ├─ Controls     │      │ ├─ Controls     │     │ ├─ Ctrl  │ │
│  │ ├─ Evidence     │      │ ├─ Evidence     │     │ ├─ Evid  │ │
│  │ └─ Chat History │      │ └─ Chat History │     │ └─ Chat  │ │
│  └────────┬────────┘      └────────┬────────┘     └────┬─────┘ │
│           │                        │                    │       │
│           └────────────────────────┼────────────────────┘       │
│                                    ▼                            │
│                    ┌───────────────────────────┐                │
│                    │     VECTOR DATABASE       │                │
│                    │   (pgvector / Qdrant)     │                │
│                    │                           │                │
│                    │  tenant_id + embedding    │                │
│                    │  ────────────────────     │                │
│                    │  Full tenant isolation    │                │
│                    └─────────────┬─────────────┘                │
│                                  │                              │
│                    ┌─────────────▼─────────────┐                │
│                    │     EMBEDDING SERVICE      │                │
│                    │  ┌─────────────────────┐  │                │
│                    │  │ Local: all-MiniLM   │  │                │
│                    │  │ or: UAE-Large-V1    │  │                │
│                    │  │ or: multilingual-e5 │  │                │
│                    │  └─────────────────────┘  │                │
│                    └─────────────┬─────────────┘                │
│                                  │                              │
│                    ┌─────────────▼─────────────┐                │
│                    │      RAG PIPELINE          │                │
│                    │  1. Query → Embed          │                │
│                    │  2. Vector Search          │                │
│                    │  3. Context Assembly       │                │
│                    │  4. LLM Generation         │                │
│                    │  5. Response + Citations   │                │
│                    └─────────────┬─────────────┘                │
│                                  │                              │
│                    ┌─────────────▼─────────────┐                │
│                    │     DOGAN COPILOT UI       │                │
│                    │  • Chat Interface          │                │
│                    │  • Document Analysis       │                │
│                    │  • Control Suggestions     │                │
│                    │  • Compliance Queries      │                │
│                    │  • Arabic + English        │                │
│                    └───────────────────────────┘                │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 4.2 Knowledge Base Entity Design

```csharp
/// <summary>
/// Per-tenant knowledge base for DoganCopilot
/// </summary>
public class TenantKnowledgeBase : BaseEntity
{
    public Guid TenantId { get; set; }
    
    // Document metadata
    public string Title { get; set; }
    public string TitleAr { get; set; }
    public string DocumentType { get; set; } // Policy, Control, Evidence, FAQ, etc.
    public string Content { get; set; }
    public string ContentAr { get; set; }
    
    // Vector embedding (1536 dimensions for OpenAI, 384 for MiniLM)
    public float[] Embedding { get; set; }
    
    // Source tracking
    public string SourceType { get; set; } // Upload, GrcEntity, External
    public Guid? SourceEntityId { get; set; }
    public string SourceEntityType { get; set; }
    
    // Chunking for long documents
    public int ChunkIndex { get; set; }
    public int TotalChunks { get; set; }
    public string ParentDocumentId { get; set; }
    
    // Metadata
    public string Tags { get; set; }
    public string Frameworks { get; set; } // Applicable frameworks
    public DateTime LastIndexedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Copilot conversation history per user per tenant
/// </summary>
public class CopilotConversation : BaseEntity
{
    public Guid TenantId { get; set; }
    public string UserId { get; set; }
    public string ConversationId { get; set; }
    
    public string UserMessage { get; set; }
    public string AssistantResponse { get; set; }
    public string CitedDocumentIds { get; set; } // JSON array
    
    public string Model { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int LatencyMs { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

### 4.3 RAG Pipeline Implementation

```csharp
/// <summary>
/// DoganCopilot RAG Service
/// </summary>
public class DoganCopilotService : IDoganCopilotService
{
    private readonly IVectorDatabase _vectorDb;
    private readonly IEmbeddingService _embedding;
    private readonly IUnifiedAiService _llm;
    private readonly ITenantContextService _tenant;
    
    public async Task<CopilotResponse> ChatAsync(string query, Guid? conversationId)
    {
        var tenantId = _tenant.GetCurrentTenantId();
        
        // 1. Generate query embedding
        var queryEmbedding = await _embedding.EmbedAsync(query);
        
        // 2. Vector similarity search (tenant-isolated)
        var relevantDocs = await _vectorDb.SearchAsync(
            tenantId: tenantId,
            embedding: queryEmbedding,
            topK: 5,
            minScore: 0.7f
        );
        
        // 3. Build context from retrieved documents
        var context = BuildContext(relevantDocs);
        
        // 4. Generate response with LLM
        var systemPrompt = $"""
            You are DoganCopilot, an AI assistant for {_tenant.GetOrgName()}.
            Use the following context to answer the user's question.
            Always cite your sources using [Source: document title].
            If you don't know, say so - don't make up information.
            Respond in the same language as the user's question.
            
            Context:
            {context}
            """;
        
        var response = await _llm.ChatAsync(
            message: query,
            systemPrompt: systemPrompt,
            tenantId: tenantId,
            preferredProvider: "Local" // Prefer local LLM
        );
        
        // 5. Save conversation
        await SaveConversationAsync(tenantId, query, response, relevantDocs);
        
        return new CopilotResponse
        {
            Answer = response.Content,
            Citations = relevantDocs.Select(d => d.Title).ToList(),
            Confidence = CalculateConfidence(relevantDocs),
            Model = response.Model
        };
    }
}
```

---

## 5. VECTOR DATABASE OPTIONS

### 5.1 Comparison for Self-Hosted

| Solution | Type | Scalability | Effort | Tenant Isolation | Recommendation |
|----------|------|-------------|--------|------------------|----------------|
| **pgvector** | PostgreSQL ext | Medium | ⭐ Easy | ✅ Built-in (TenantId) | **Best for start** |
| **Qdrant** | Native vector DB | High | ⭐⭐ | ✅ Collections | Best for scale |
| **Milvus** | Distributed | Very High | ⭐⭐⭐ | ✅ Partitions | Enterprise |
| **Weaviate** | GraphQL-based | High | ⭐⭐ | ✅ Multi-tenant | Good alternative |
| **ChromaDB** | Simple | Low | ⭐ Easy | ⚠️ Manual | Development only |

### 5.2 Recommended: pgvector (Start) → Qdrant (Scale)

**Phase 1: pgvector**
```sql
-- Add pgvector extension
CREATE EXTENSION vector;

-- Create knowledge base table with vector column
CREATE TABLE tenant_knowledge_base (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    title TEXT NOT NULL,
    content TEXT NOT NULL,
    embedding vector(384), -- all-MiniLM-L6-v2
    -- or embedding vector(1024), -- UAE-Large-V1
    document_type VARCHAR(50),
    source_type VARCHAR(50),
    created_at TIMESTAMP DEFAULT NOW()
);

-- Create index for fast similarity search
CREATE INDEX ON tenant_knowledge_base 
USING ivfflat (embedding vector_cosine_ops)
WITH (lists = 100);

-- Tenant-isolated similarity search
SELECT id, title, content, 
       1 - (embedding <=> $1::vector) as similarity
FROM tenant_knowledge_base
WHERE tenant_id = $2
ORDER BY embedding <=> $1::vector
LIMIT 5;
```

**pgvector Resource Requirements:**
```yaml
PostgreSQL with pgvector:
  CPU: 4-8 cores
  RAM: 16 GB (for 1M vectors)
  RAM: 32 GB (for 5M vectors)
  Storage: 500 GB SSD
```

---

## 6. EMBEDDING SERVICE

### 6.1 Local Embedding Models

| Model | Dimensions | Languages | Speed | Quality |
|-------|------------|-----------|-------|---------|
| **all-MiniLM-L6-v2** | 384 | English | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| **multilingual-e5-large** | 1024 | 100+ (Arabic ✅) | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **UAE-Large-V1** | 1024 | Arabic specialized | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **bge-m3** | 1024 | Multilingual | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |

### 6.2 Recommended for Arabic + English

```yaml
Service: embedding-service
Model: intfloat/multilingual-e5-large

Resources:
  CPU: 4 cores (GPU optional)
  RAM: 8 GB
  GPU: Optional (3× faster with GPU)
  
Throughput:
  CPU: ~50 embeddings/sec
  GPU: ~500 embeddings/sec
```

---

## 7. COMPLETE SERVER SIZING

### 7.1 Minimum Production (10-50 Tenants, No Local LLM)

```
┌─────────────────────────────────────────────────────────────┐
│                   SINGLE SERVER DEPLOYMENT                  │
│                  (Cloud LLM Only)                           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Hardware:                                                  │
│  ├─ CPU: 16 cores (Intel Xeon / AMD EPYC)                  │
│  ├─ RAM: 64 GB DDR4 ECC                                    │
│  ├─ Storage: 500 GB NVMe SSD                               │
│  ├─ Network: 1 Gbps                                        │
│  └─ GPU: None (using Claude/OpenAI API)                    │
│                                                             │
│  Services:                                                  │
│  ├─ GRC Portal (2 replicas): 4 cores, 8 GB                 │
│  ├─ PostgreSQL: 4 cores, 16 GB                             │
│  ├─ Redis Cluster: 2 cores, 4 GB                           │
│  ├─ Hangfire Workers: 2 cores, 2 GB                        │
│  ├─ Nginx: 1 core, 512 MB                                  │
│  ├─ Embedding Service: 2 cores, 4 GB                       │
│  └─ Monitoring (Prometheus/Grafana): 1 core, 2 GB          │
│                                                             │
│  Total Used: 16 cores, 36.5 GB                             │
│  Headroom: 0 cores, 27.5 GB                                │
│                                                             │
│  Estimated Cost: ~$300-500/month (bare metal)              │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 7.2 Standard Production (50-100 Tenants, Local LLM)

```
┌─────────────────────────────────────────────────────────────┐
│                   TWO SERVER DEPLOYMENT                     │
│                  (Local LLM + Full HA)                      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  SERVER 1: Application + Database                          │
│  ──────────────────────────────────                        │
│  ├─ CPU: 32 cores                                          │
│  ├─ RAM: 128 GB DDR4 ECC                                   │
│  ├─ Storage: 1 TB NVMe RAID 1                              │
│  ├─ Network: 10 Gbps                                       │
│  │                                                         │
│  │  Services:                                              │
│  │  ├─ GRC Portal (4 replicas): 16 cores, 16 GB           │
│  │  ├─ PostgreSQL + pgvector: 8 cores, 32 GB              │
│  │  ├─ Redis Cluster: 4 cores, 8 GB                       │
│  │  ├─ Hangfire Workers (4): 4 cores, 4 GB                │
│  │  ├─ Embedding Service: 4 cores, 8 GB                   │
│  │  └─ Nginx + Monitoring: 2 cores, 4 GB                  │
│  │                                                         │
│  │  Total Used: 38 cores, 72 GB                           │
│                                                             │
│  SERVER 2: AI/LLM Server                                   │
│  ─────────────────────────                                 │
│  ├─ CPU: 16 cores                                          │
│  ├─ RAM: 64 GB DDR4                                        │
│  ├─ Storage: 500 GB NVMe                                   │
│  ├─ GPU: 2× NVIDIA RTX 4090 (24GB each)                   │
│  │        OR 1× NVIDIA A100 40GB                          │
│  │                                                         │
│  │  Services:                                              │
│  │  ├─ vLLM Server: 8 cores, 32 GB, 2× GPU               │
│  │  │  └─ Model: Qwen 2.5 72B (quantized)                │
│  │  └─ Ollama (backup): 4 cores, 16 GB                   │
│  │                                                         │
│  │  Total Used: 12 cores, 48 GB, 48 GB VRAM              │
│                                                             │
│  Estimated Cost: ~$1,200-1,800/month (bare metal)          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 7.3 Enterprise Production (200+ Tenants, Full HA)

```
┌─────────────────────────────────────────────────────────────┐
│               ENTERPRISE CLUSTER DEPLOYMENT                 │
│              (Full HA, Local LLM, Kubernetes)               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  KUBERNETES CLUSTER (3 Control Plane + 5 Workers)          │
│                                                             │
│  Control Plane Nodes (3×):                                 │
│  ├─ CPU: 4 cores each                                      │
│  ├─ RAM: 8 GB each                                         │
│  └─ Storage: 100 GB SSD each                               │
│                                                             │
│  Worker Nodes - Application (3×):                          │
│  ├─ CPU: 32 cores each                                     │
│  ├─ RAM: 128 GB each                                       │
│  ├─ Storage: 500 GB NVMe each                              │
│  │                                                         │
│  │  Runs:                                                  │
│  │  ├─ GRC Portal (8 replicas): 32 cores, 32 GB           │
│  │  ├─ Hangfire Workers (8): 8 cores, 8 GB                │
│  │  ├─ Embedding Service (2): 4 cores, 8 GB               │
│  │  └─ Nginx Ingress (3): 3 cores, 1.5 GB                 │
│                                                             │
│  Worker Nodes - Data (2×):                                 │
│  ├─ CPU: 16 cores each                                     │
│  ├─ RAM: 64 GB each                                        │
│  ├─ Storage: 2 TB NVMe RAID 10 each                        │
│  │                                                         │
│  │  Runs:                                                  │
│  │  ├─ PostgreSQL Patroni (3-node): 24 cores, 96 GB       │
│  │  ├─ Redis Sentinel (6 pods): 3 cores, 6 GB             │
│  │  └─ Qdrant Vector DB (3 pods): 6 cores, 24 GB          │
│                                                             │
│  GPU Nodes - AI (2×):                                      │
│  ├─ CPU: 32 cores each                                     │
│  ├─ RAM: 128 GB each                                       │
│  ├─ GPU: 2× NVIDIA A100 80GB each                         │
│  │                                                         │
│  │  Runs:                                                  │
│  │  ├─ vLLM (2 replicas): 16 cores, 64 GB, 160 GB VRAM   │
│  │  │  └─ Models: Qwen 2.5 72B, Llama 3.2 70B            │
│  │  └─ Embedding Service (GPU): 4 cores, 16 GB            │
│                                                             │
│  TOTALS:                                                   │
│  ├─ CPU: 248 cores                                         │
│  ├─ RAM: 712 GB                                            │
│  ├─ Storage: 7.4 TB NVMe                                   │
│  └─ GPU VRAM: 320 GB                                       │
│                                                             │
│  Estimated Cost: ~$8,000-12,000/month (bare metal)         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 8. BUILD vs BUY RECOMMENDATION

### 8.1 For KSA Regulatory Compliance: BUILD (Local LLM)

**Reasons:**
1. **Data Residency**: All data stays on-premise in KSA
2. **SAMA Compliance**: No third-party data processing
3. **NCA Requirements**: Full control over AI processing
4. **PDPL Compliance**: Personal data never leaves organization

### 8.2 Recommended Stack

```yaml
Phase 1 (Month 1-2):
  LLM: Claude API (cloud) for quick start
  Embedding: Local (multilingual-e5-large)
  Vector DB: pgvector (in existing PostgreSQL)
  Cost: ~$500/month API + existing infra

Phase 2 (Month 3-4):
  LLM: Add vLLM with Qwen 2.5 72B
  Hardware: Purchase 2× RTX 4090 or 1× A100
  Keep Claude as fallback
  Cost: ~$15,000 one-time + $200/month

Phase 3 (Month 5+):
  LLM: Primary local, cloud fallback only
  Vector DB: Migrate to Qdrant if > 5M vectors
  Full DoganCopilot per-tenant KB
  Cost: ~$300/month (electricity + maintenance)
```

---

## 9. QUICK START - MINIMUM VIABLE AI

### 9.1 Add Local LLM Support (4 hours)

```yaml
# docker-compose.ai.yml
services:
  ollama:
    image: ollama/ollama:latest
    container_name: shahin-ollama
    ports:
      - "11434:11434"
    volumes:
      - ollama_models:/root/.ollama
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: 1
              capabilities: [gpu]
    environment:
      - OLLAMA_HOST=0.0.0.0
    restart: always

  # Pull models on startup
  ollama-init:
    image: ollama/ollama:latest
    depends_on:
      - ollama
    entrypoint: >
      /bin/sh -c "
        sleep 10 &&
        ollama pull qwen2.5:7b &&
        ollama pull llama3.2:8b
      "

volumes:
  ollama_models:
```

### 9.2 Add pgvector for Knowledge Base (2 hours)

```sql
-- Run on PostgreSQL
CREATE EXTENSION IF NOT EXISTS vector;

-- Add to GrcDbContext
public DbSet<TenantKnowledgeBase> KnowledgeBases { get; set; }
```

### 9.3 Update UnifiedAiService for Local Priority

```csharp
// In GetBestConfigurationAsync, prioritize local:
var configs = await query
    .OrderBy(c => c.Provider == "Ollama" ? 0 : 1) // Local first
    .ThenBy(c => c.Priority)
    .ToListAsync(ct);
```

---

## 10. SUMMARY

### Self-Hosted Production Requirements

| Scenario | Servers | CPU | RAM | GPU | Storage | Est. Cost/mo |
|----------|---------|-----|-----|-----|---------|--------------|
| **Minimum** (Cloud LLM) | 1 | 16 | 64 GB | None | 500 GB | $500 |
| **Standard** (Local LLM) | 2 | 48 | 192 GB | 2×4090 | 1.5 TB | $1,500 |
| **Enterprise** (HA + Local) | 10 | 248 | 712 GB | 4×A100 | 7 TB | $10,000 |

### DoganCopilot Per-Org Features

✅ **Will Deliver:**
- Per-tenant isolated knowledge base
- RAG over company documents
- Arabic + English support
- Local LLM for data privacy
- Cloud LLM fallback
- Conversation history
- Citation tracking
- Compliance-aware responses

### Implementation Priority

1. **Week 1**: Add pgvector + embedding service
2. **Week 2**: Implement RAG pipeline
3. **Week 3**: Add Ollama/vLLM for local inference
4. **Week 4**: Build DoganCopilot UI + per-tenant KB

---

*Infrastructure Sizing Guide Complete*  
*Generated: 2026-01-16*
