#!/bin/bash

# Script to upload GRC Permissions & Policy System archive to cloud storage bucket
# Supports: AWS S3, Azure Blob Storage, Google Cloud Storage

set -e

ARCHIVE_FILE="grc-permissions-policy-system.tar.gz"
ARCHIVE_PATH="./grc-permissions-policy-system.tar.gz"
BUCKET_NAME="${BUCKET_NAME:-grc-system-archives}"
REGION="${REGION:-us-east-1}"

echo "🚀 Uploading GRC Permissions & Policy System Archive to Bucket"
echo "================================================================"
echo "Archive: $ARCHIVE_FILE"
echo "Bucket: $BUCKET_NAME"
echo ""

# Check if archive exists
if [ ! -f "$ARCHIVE_PATH" ]; then
    echo "❌ Error: Archive file not found at $ARCHIVE_PATH"
    exit 1
fi

# Get file size
FILE_SIZE=$(du -h "$ARCHIVE_PATH" | cut -f1)
echo "📦 Archive size: $FILE_SIZE"
echo ""

# Function: Upload to AWS S3
upload_to_s3() {
    if ! command -v aws &> /dev/null; then
        echo "❌ AWS CLI not found. Install with: pip install awscli"
        return 1
    fi
    
    echo "📤 Uploading to AWS S3..."
    aws s3 cp "$ARCHIVE_PATH" "s3://${BUCKET_NAME}/${ARCHIVE_FILE}" \
        --region "$REGION" \
        --metadata "project=grc-system,component=permissions-policy,version=1.0.0,created=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
    
    if [ $? -eq 0 ]; then
        echo "✅ Successfully uploaded to S3: s3://${BUCKET_NAME}/${ARCHIVE_FILE}"
        aws s3 ls "s3://${BUCKET_NAME}/${ARCHIVE_FILE}" --human-readable
        return 0
    else
        echo "❌ Upload failed"
        return 1
    fi
}

# Function: Upload to Azure Blob Storage
upload_to_azure() {
    if ! command -v az &> /dev/null; then
        echo "❌ Azure CLI not found. Install with: curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash"
        return 1
    fi
    
    CONTAINER_NAME="${CONTAINER_NAME:-archives}"
    STORAGE_ACCOUNT="${STORAGE_ACCOUNT:-}"
    
    if [ -z "$STORAGE_ACCOUNT" ]; then
        echo "❌ Error: STORAGE_ACCOUNT environment variable not set"
        return 1
    fi
    
    echo "📤 Uploading to Azure Blob Storage..."
    az storage blob upload \
        --account-name "$STORAGE_ACCOUNT" \
        --container-name "$CONTAINER_NAME" \
        --name "$ARCHIVE_FILE" \
        --file "$ARCHIVE_PATH" \
        --metadata "project=grc-system" "component=permissions-policy" "version=1.0.0" "created=$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
        --overwrite
    
    if [ $? -eq 0 ]; then
        echo "✅ Successfully uploaded to Azure: $CONTAINER_NAME/$ARCHIVE_FILE"
        return 0
    else
        echo "❌ Upload failed"
        return 1
    fi
}

# Function: Upload to Google Cloud Storage
upload_to_gcs() {
    if ! command -v gsutil &> /dev/null; then
        echo "❌ gsutil not found. Install Google Cloud SDK"
        return 1
    fi
    
    echo "📤 Uploading to Google Cloud Storage..."
    gsutil cp "$ARCHIVE_PATH" "gs://${BUCKET_NAME}/${ARCHIVE_FILE}" \
        -h "x-goog-meta-project:grc-system" \
        -h "x-goog-meta-component:permissions-policy" \
        -h "x-goog-meta-version:1.0.0" \
        -h "x-goog-meta-created:$(date -u +%Y-%m-%dT%H:%M:%SZ)"
    
    if [ $? -eq 0 ]; then
        echo "✅ Successfully uploaded to GCS: gs://${BUCKET_NAME}/${ARCHIVE_FILE}"
        gsutil ls -lh "gs://${BUCKET_NAME}/${ARCHIVE_FILE}"
        return 0
    else
        echo "❌ Upload failed"
        return 1
    fi
}

# Function: Upload via HTTP/API (generic)
upload_via_http() {
    UPLOAD_URL="${UPLOAD_URL:-}"
    
    if [ -z "$UPLOAD_URL" ]; then
        echo "❌ Error: UPLOAD_URL environment variable not set"
        return 1
    fi
    
    echo "📤 Uploading via HTTP..."
    
    if command -v curl &> /dev/null; then
        curl -X PUT \
            -H "Content-Type: application/gzip" \
            -H "X-Archive-Project: grc-system" \
            -H "X-Archive-Component: permissions-policy" \
            -H "X-Archive-Version: 1.0.0" \
            --data-binary "@$ARCHIVE_PATH" \
            "$UPLOAD_URL/$ARCHIVE_FILE"
    elif command -v wget &> /dev/null; then
        wget --method=PUT \
            --header="Content-Type: application/gzip" \
            --body-file="$ARCHIVE_PATH" \
            "$UPLOAD_URL/$ARCHIVE_FILE"
    else
        echo "❌ Error: curl or wget not found"
        return 1
    fi
    
    if [ $? -eq 0 ]; then
        echo "✅ Successfully uploaded via HTTP"
        return 0
    else
        echo "❌ Upload failed"
        return 1
    fi
}

# Main: Detect provider and upload
PROVIDER="${PROVIDER:-auto}"

if [ "$PROVIDER" = "auto" ]; then
    # Auto-detect provider
    if command -v aws &> /dev/null && aws sts get-caller-identity &> /dev/null; then
        PROVIDER="s3"
    elif command -v az &> /dev/null && az account show &> /dev/null; then
        PROVIDER="azure"
    elif command -v gsutil &> /dev/null && gsutil ls &> /dev/null; then
        PROVIDER="gcs"
    else
        echo "⚠️  No cloud provider detected. Available options:"
        echo "   - Set PROVIDER=s3 for AWS S3"
        echo "   - Set PROVIDER=azure for Azure Blob Storage"
        echo "   - Set PROVIDER=gcs for Google Cloud Storage"
        echo "   - Set PROVIDER=http for HTTP upload"
        exit 1
    fi
fi

case "$PROVIDER" in
    s3|aws)
        upload_to_s3
        ;;
    azure)
        upload_to_azure
        ;;
    gcs|gcp|google)
        upload_to_gcs
        ;;
    http|https)
        upload_via_http
        ;;
    *)
        echo "❌ Unknown provider: $PROVIDER"
        echo "Supported: s3, azure, gcs, http"
        exit 1
        ;;
esac

echo ""
echo "✅ Upload complete!"
echo ""
echo "📋 Archive Info:"
echo "   File: $ARCHIVE_FILE"
echo "   Size: $FILE_SIZE"
echo "   Location: See above for bucket URL"
echo ""
