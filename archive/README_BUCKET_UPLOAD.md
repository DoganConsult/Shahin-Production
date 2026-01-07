# Upload Archive to Cloud Bucket

## Quick Start

### AWS S3

```bash
cd /home/dogan/grc-system/archive

# Set environment variables
export BUCKET_NAME="your-bucket-name"
export REGION="us-east-1"  # Optional, defaults to us-east-1

# Make script executable
chmod +x upload-to-bucket.sh

# Upload
./upload-to-bucket.sh
```

Or manually:
```bash
aws s3 cp grc-permissions-policy-system.tar.gz s3://your-bucket-name/ \
  --metadata "project=grc-system,component=permissions-policy,version=1.0.0"
```

### Azure Blob Storage

```bash
cd /home/dogan/grc-system/archive

# Set environment variables
export STORAGE_ACCOUNT="your-storage-account"
export CONTAINER_NAME="archives"  # Optional, defaults to "archives"
export PROVIDER="azure"

# Make script executable
chmod +x upload-to-bucket.sh

# Upload
./upload-to-bucket.sh
```

Or manually:
```bash
az storage blob upload \
  --account-name your-storage-account \
  --container-name archives \
  --name grc-permissions-policy-system.tar.gz \
  --file grc-permissions-policy-system.tar.gz
```

### Google Cloud Storage

```bash
cd /home/dogan/grc-system/archive

# Set environment variables
export BUCKET_NAME="your-bucket-name"
export PROVIDER="gcs"

# Make script executable
chmod +x upload-to-bucket.sh

# Upload
./upload-to-bucket.sh
```

Or manually:
```bash
gsutil cp grc-permissions-policy-system.tar.gz gs://your-bucket-name/
```

### Generic HTTP Upload

```bash
cd /home/dogan/grc-system/archive

# Set environment variables
export UPLOAD_URL="https://your-api.com/upload"
export PROVIDER="http"

# Upload
./upload-to-bucket.sh
```

## Environment Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `BUCKET_NAME` | Bucket/container name | `grc-system-archives` | Yes |
| `REGION` | AWS region (S3 only) | `us-east-1` | No |
| `STORAGE_ACCOUNT` | Azure storage account | - | Yes (Azure) |
| `CONTAINER_NAME` | Azure container name | `archives` | No (Azure) |
| `PROVIDER` | Cloud provider (`s3`, `azure`, `gcs`, `http`) | `auto` | No |
| `UPLOAD_URL` | HTTP upload URL | - | Yes (HTTP) |

## Prerequisites

### AWS S3
```bash
# Install AWS CLI
pip install awscli

# Configure credentials
aws configure
```

### Azure Blob Storage
```bash
# Install Azure CLI
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Login
az login
```

### Google Cloud Storage
```bash
# Install Google Cloud SDK
# See: https://cloud.google.com/sdk/docs/install

# Authenticate
gcloud auth login
```

## Script Features

- ✅ Auto-detects cloud provider
- ✅ Supports AWS S3, Azure Blob, GCS, HTTP
- ✅ Adds metadata (project, component, version, timestamp)
- ✅ Shows upload progress and file info
- ✅ Error handling and validation

## Manual Upload Examples

### Using curl (any HTTP endpoint)
```bash
curl -X PUT \
  -H "Content-Type: application/gzip" \
  --data-binary "@grc-permissions-policy-system.tar.gz" \
  https://your-api.com/upload/grc-permissions-policy-system.tar.gz
```

### Using rclone (supports many providers)
```bash
# Install rclone: https://rclone.org/install/
rclone copy grc-permissions-policy-system.tar.gz remote:bucket-name/
```

## Archive Information

- **File:** `grc-permissions-policy-system.tar.gz`
- **Size:** 23 KB (compressed), 204 KB (uncompressed)
- **Contents:** Permissions system, Policy enforcement, Menu contributor
- **Version:** 1.0.0
- **Created:** 2025-01-22

## Verification

After upload, verify the file:

### AWS S3
```bash
aws s3 ls s3://your-bucket-name/grc-permissions-policy-system.tar.gz --human-readable
```

### Azure
```bash
az storage blob show \
  --account-name your-storage-account \
  --container-name archives \
  --name grc-permissions-policy-system.tar.gz
```

### GCS
```bash
gsutil ls -lh gs://your-bucket-name/grc-permissions-policy-system.tar.gz
```
