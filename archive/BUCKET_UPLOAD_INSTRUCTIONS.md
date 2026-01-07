# 📦 Upload GRC Archive to Cloud Bucket

## 🚀 Quick Upload

### Option 1: Use the Upload Script (Recommended)

```bash
cd /home/dogan/grc-system/archive

# For AWS S3
export BUCKET_NAME="your-bucket-name"
export REGION="us-east-1"
./upload-to-bucket.sh

# For Azure
export STORAGE_ACCOUNT="your-storage-account"
export CONTAINER_NAME="archives"
export PROVIDER="azure"
./upload-to-bucket.sh

# For Google Cloud
export BUCKET_NAME="your-bucket-name"
export PROVIDER="gcs"
./upload-to-bucket.sh
```

### Option 2: Manual Upload Commands

#### AWS S3
```bash
aws s3 cp grc-permissions-policy-system.tar.gz \
  s3://your-bucket-name/grc-permissions-policy-system.tar.gz \
  --metadata "project=grc-system,component=permissions-policy,version=1.0.0,created=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
```

#### Azure Blob Storage
```bash
az storage blob upload \
  --account-name your-storage-account \
  --container-name archives \
  --name grc-permissions-policy-system.tar.gz \
  --file grc-permissions-policy-system.tar.gz \
  --metadata project=grc-system component=permissions-policy version=1.0.0
```

#### Google Cloud Storage
```bash
gsutil cp grc-permissions-policy-system.tar.gz \
  gs://your-bucket-name/grc-permissions-policy-system.tar.gz
```

## 📋 Archive Details

- **File:** `grc-permissions-policy-system.tar.gz`
- **Size:** 23 KB (compressed)
- **Location:** `/home/dogan/grc-system/archive/`
- **Contents:**
  - Permissions System (7 files)
  - Policy Enforcement (15 files)
  - Menu System (2 files)
  - Documentation (3 files)

## ✅ Verification

After upload, verify:

```bash
# AWS S3
aws s3 ls s3://your-bucket-name/grc-permissions-policy-system.tar.gz --human-readable

# Azure
az storage blob show --account-name your-storage-account \
  --container-name archives --name grc-permissions-policy-system.tar.gz

# GCS
gsutil ls -lh gs://your-bucket-name/grc-permissions-policy-system.tar.gz
```

## 🔧 Troubleshooting

1. **AWS CLI not found:** `pip install awscli && aws configure`
2. **Azure CLI not found:** Install from https://aka.ms/InstallAzureCLIDeb
3. **GCS not found:** Install Google Cloud SDK
4. **Permission denied:** Check IAM roles and bucket policies

---

**Need help?** See `README_BUCKET_UPLOAD.md` for detailed instructions.
