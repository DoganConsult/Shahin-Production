#!/bin/bash
#
# Shahin AI GRC System - Multi-Environment Kubernetes Deployment Script
# Supports: dev, staging, production environments with Kustomize overlays
#
# Usage: ./deploy.sh <environment> [action]
# Environments: dev, staging, production
# Actions: apply, delete, diff, status, health (default: apply)

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Default values
ENVIRONMENT="${1:-}"
ACTION="${2:-apply}"

# Print functions
print_status() { echo -e "${GREEN}[INFO]${NC} $1"; }
print_warning() { echo -e "${YELLOW}[WARN]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }
print_header() { echo -e "${BLUE}$1${NC}"; }

# Validate environment
validate_environment() {
    if [[ -z "$ENVIRONMENT" ]]; then
        print_error "Environment not specified"
        echo ""
        echo "Usage: $0 <environment> [action]"
        echo ""
        echo "Environments:"
        echo "  dev        - Development (minimal resources, single replicas)"
        echo "  staging    - Staging (moderate resources, 2 replicas)"
        echo "  production - Production (full HA, 3 replicas)"
        echo ""
        echo "Actions:"
        echo "  apply   - Deploy/update the environment (default)"
        echo "  delete  - Delete all resources"
        echo "  diff    - Show changes before applying"
        echo "  status  - Show current status"
        echo "  health  - Run health checks"
        exit 1
    fi

    if [[ ! "$ENVIRONMENT" =~ ^(dev|staging|production)$ ]]; then
        print_error "Invalid environment '$ENVIRONMENT'"
        echo "Valid environments: dev, staging, production"
        exit 1
    fi
}

# Set namespace based on environment
set_namespace() {
    case $ENVIRONMENT in
        dev)
            NAMESPACE="shahin-grc-dev"
            ;;
        staging)
            NAMESPACE="shahin-grc-staging"
            ;;
        production)
            NAMESPACE="shahin-grc"
            ;;
    esac
    OVERLAY_DIR="$SCRIPT_DIR/overlays/$ENVIRONMENT"
}

# Check prerequisites
check_prerequisites() {
    print_status "Checking prerequisites..."

    if ! command -v kubectl &> /dev/null; then
        print_error "kubectl not found. Please install kubectl."
        exit 1
    fi

    # Check for kustomize
    if command -v kustomize &> /dev/null; then
        KUSTOMIZE_CMD="kustomize build"
    else
        print_warning "kustomize not found, using kubectl kustomize"
        KUSTOMIZE_CMD="kubectl kustomize"
    fi

    # Check cluster connectivity
    if ! kubectl cluster-info &> /dev/null; then
        print_error "Cannot connect to Kubernetes cluster. Check your kubeconfig."
        exit 1
    fi

    # Check overlay directory exists
    if [[ ! -d "$OVERLAY_DIR" ]]; then
        print_error "Overlay directory not found: $OVERLAY_DIR"
        exit 1
    fi

    print_status "Prerequisites OK"
}

# Show diff before applying
show_diff() {
    print_header "\n=== Changes to be applied for $ENVIRONMENT ==="
    $KUSTOMIZE_CMD "$OVERLAY_DIR" | kubectl diff -f - 2>/dev/null || true
}

# Apply deployment
apply_deployment() {
    print_header "\n=========================================="
    print_header "  Deploying Shahin GRC - $ENVIRONMENT"
    print_header "==========================================\n"

    # Validate manifests first
    print_status "Validating manifests..."
    if ! $KUSTOMIZE_CMD "$OVERLAY_DIR" | kubectl apply --dry-run=client -f - > /dev/null 2>&1; then
        print_error "Manifest validation failed"
        exit 1
    fi
    print_status "Validation passed"

    # Show what will be deployed
    echo ""
    print_status "Resources to be deployed:"
    $KUSTOMIZE_CMD "$OVERLAY_DIR" | kubectl apply --dry-run=client -f - 2>&1 | grep -E "^[a-z]" | head -20

    echo ""
    read -p "Proceed with deployment? (yes/no): " confirm
    if [[ "$confirm" != "yes" ]]; then
        print_warning "Deployment cancelled."
        exit 0
    fi

    # Apply with server-side apply
    print_status "Applying manifests..."
    $KUSTOMIZE_CMD "$OVERLAY_DIR" | kubectl apply --server-side -f -

    print_status "Deployment applied successfully"

    # Wait for rollout
    wait_for_deployment
}

# Wait for deployment to complete
wait_for_deployment() {
    print_status "Waiting for deployment to complete..."

    # Wait for etcd (if more than 0 replicas)
    ETCD_REPLICAS=$(kubectl get statefulset -n "$NAMESPACE" -l app.kubernetes.io/name=etcd -o jsonpath='{.items[0].spec.replicas}' 2>/dev/null || echo "0")
    if [[ "$ETCD_REPLICAS" -gt 0 ]]; then
        print_status "Waiting for etcd ($ETCD_REPLICAS replicas)..."
        kubectl rollout status statefulset -l app.kubernetes.io/name=etcd -n "$NAMESPACE" --timeout=300s 2>/dev/null || true
    fi

    # Wait for PostgreSQL
    POSTGRES_REPLICAS=$(kubectl get statefulset -n "$NAMESPACE" -l app.kubernetes.io/name=postgresql -o jsonpath='{.items[0].spec.replicas}' 2>/dev/null || echo "0")
    if [[ "$POSTGRES_REPLICAS" -gt 0 ]]; then
        print_status "Waiting for PostgreSQL ($POSTGRES_REPLICAS replicas)..."
        kubectl rollout status statefulset -l app.kubernetes.io/name=postgresql -n "$NAMESPACE" --timeout=600s 2>/dev/null || true
    fi

    # Wait for Redis
    print_status "Waiting for Redis..."
    kubectl rollout status statefulset -l app.kubernetes.io/name=redis -n "$NAMESPACE" --timeout=300s 2>/dev/null || true

    # Wait for Kafka (if exists)
    KAFKA_REPLICAS=$(kubectl get statefulset -n "$NAMESPACE" -l app.kubernetes.io/name=kafka -o jsonpath='{.items[0].spec.replicas}' 2>/dev/null || echo "0")
    if [[ "$KAFKA_REPLICAS" -gt 0 ]]; then
        print_status "Waiting for Kafka ($KAFKA_REPLICAS brokers)..."
        kubectl rollout status statefulset -l app.kubernetes.io/name=kafka -n "$NAMESPACE" --timeout=600s 2>/dev/null || true
    fi

    # Wait for application
    print_status "Waiting for GRC Portal..."
    kubectl rollout status deployment -l app.kubernetes.io/name=grc-portal -n "$NAMESPACE" --timeout=300s 2>/dev/null || true

    print_status "All resources ready!"
}

# Delete deployment
delete_deployment() {
    print_warning "This will DELETE ALL resources in namespace: $NAMESPACE"
    print_warning "Environment: $ENVIRONMENT"
    echo ""

    read -p "Type the environment name to confirm deletion: " confirm
    if [[ "$confirm" != "$ENVIRONMENT" ]]; then
        print_warning "Deletion cancelled. Input did not match environment name."
        exit 0
    fi

    print_status "Deleting resources..."
    $KUSTOMIZE_CMD "$OVERLAY_DIR" | kubectl delete -f - --ignore-not-found

    # Optionally delete namespace
    read -p "Delete namespace $NAMESPACE as well? (yes/no): " delete_ns
    if [[ "$delete_ns" == "yes" ]]; then
        kubectl delete namespace "$NAMESPACE" --ignore-not-found
        print_status "Namespace deleted"
    fi

    print_status "Deletion complete"
}

# Show status
show_status() {
    print_header "\n=========================================="
    print_header "  Status: $ENVIRONMENT ($NAMESPACE)"
    print_header "==========================================\n"

    print_header "=== Namespace ==="
    kubectl get namespace "$NAMESPACE" 2>/dev/null || echo "Namespace not found"

    print_header "\n=== Pods ==="
    kubectl get pods -n "$NAMESPACE" -o wide 2>/dev/null || echo "No pods"

    print_header "\n=== StatefulSets ==="
    kubectl get statefulsets -n "$NAMESPACE" 2>/dev/null || echo "No statefulsets"

    print_header "\n=== Deployments ==="
    kubectl get deployments -n "$NAMESPACE" 2>/dev/null || echo "No deployments"

    print_header "\n=== Services ==="
    kubectl get services -n "$NAMESPACE" 2>/dev/null || echo "No services"

    print_header "\n=== HPA ==="
    kubectl get hpa -n "$NAMESPACE" 2>/dev/null || echo "No HPA"

    print_header "\n=== PDBs ==="
    kubectl get pdb -n "$NAMESPACE" 2>/dev/null || echo "No PDBs"

    print_header "\n=== Ingress ==="
    kubectl get ingress -n "$NAMESPACE" 2>/dev/null || echo "No ingress"

    print_header "\n=== Recent Events (last 10) ==="
    kubectl get events -n "$NAMESPACE" --sort-by='.lastTimestamp' 2>/dev/null | tail -10 || echo "No events"
}

# Health check
health_check() {
    print_header "\n=========================================="
    print_header "  Health Check: $ENVIRONMENT"
    print_header "==========================================\n"

    # Check PostgreSQL
    print_header "=== PostgreSQL ==="
    POSTGRES_POD=$(kubectl get pod -n "$NAMESPACE" -l app.kubernetes.io/name=postgresql -o jsonpath='{.items[0].metadata.name}' 2>/dev/null)
    if [[ -n "$POSTGRES_POD" ]]; then
        if [[ "$ENVIRONMENT" != "dev" ]]; then
            kubectl exec -n "$NAMESPACE" "$POSTGRES_POD" -- patronictl list 2>/dev/null || print_warning "Cannot get Patroni status"
        else
            kubectl exec -n "$NAMESPACE" "$POSTGRES_POD" -- pg_isready -U shahin_admin 2>/dev/null && print_status "PostgreSQL: OK" || print_warning "PostgreSQL: FAILED"
        fi
    else
        print_warning "No PostgreSQL pods found"
    fi

    # Check Redis
    print_header "\n=== Redis ==="
    REDIS_POD=$(kubectl get pod -n "$NAMESPACE" -l app.kubernetes.io/name=redis,app.kubernetes.io/component=master -o jsonpath='{.items[0].metadata.name}' 2>/dev/null)
    if [[ -n "$REDIS_POD" ]]; then
        kubectl exec -n "$NAMESPACE" "$REDIS_POD" -- redis-cli ping 2>/dev/null && print_status "Redis: OK" || print_warning "Redis: FAILED"
    else
        print_warning "No Redis pods found"
    fi

    # Check Sentinel (if not dev)
    if [[ "$ENVIRONMENT" != "dev" ]]; then
        print_header "\n=== Redis Sentinel ==="
        SENTINEL_POD=$(kubectl get pod -n "$NAMESPACE" -l app.kubernetes.io/name=redis-sentinel -o jsonpath='{.items[0].metadata.name}' 2>/dev/null)
        if [[ -n "$SENTINEL_POD" ]]; then
            kubectl exec -n "$NAMESPACE" "$SENTINEL_POD" -- redis-cli -p 26379 SENTINEL masters 2>/dev/null | head -5 || print_warning "Cannot get Sentinel status"
        fi
    fi

    # Check Kafka (if exists)
    KAFKA_POD=$(kubectl get pod -n "$NAMESPACE" -l app.kubernetes.io/name=kafka -o jsonpath='{.items[0].metadata.name}' 2>/dev/null)
    if [[ -n "$KAFKA_POD" ]]; then
        print_header "\n=== Kafka ==="
        kubectl exec -n "$NAMESPACE" "$KAFKA_POD" -- kafka-topics --bootstrap-server localhost:9092 --list 2>/dev/null | head -5 && print_status "Kafka: OK" || print_warning "Kafka: Cannot list topics"
    fi

    # Check Application
    print_header "\n=== GRC Portal ==="
    APP_POD=$(kubectl get pod -n "$NAMESPACE" -l app.kubernetes.io/name=grc-portal -o jsonpath='{.items[0].metadata.name}' 2>/dev/null)
    if [[ -n "$APP_POD" ]]; then
        HEALTH=$(kubectl exec -n "$NAMESPACE" "$APP_POD" -- curl -sf http://localhost:8080/health 2>/dev/null)
        if [[ -n "$HEALTH" ]]; then
            print_status "GRC Portal: $HEALTH"
        else
            print_warning "GRC Portal: Health check failed"
        fi
    else
        print_warning "No GRC Portal pods found"
    fi

    print_header "\n=== Overall Status ==="
    READY_PODS=$(kubectl get pods -n "$NAMESPACE" -o jsonpath='{range .items[*]}{.status.containerStatuses[0].ready}{"\n"}{end}' 2>/dev/null | grep -c true || echo "0")
    TOTAL_PODS=$(kubectl get pods -n "$NAMESPACE" --no-headers 2>/dev/null | wc -l | tr -d ' ')
    print_status "Ready pods: $READY_PODS / $TOTAL_PODS"
}

# Main execution
print_header "\n=========================================="
print_header "  Shahin AI GRC Platform Deployment"
print_header "==========================================\n"

validate_environment
set_namespace

echo "Environment: $ENVIRONMENT"
echo "Namespace:   $NAMESPACE"
echo "Action:      $ACTION"
echo ""

check_prerequisites

case $ACTION in
    apply)
        apply_deployment
        show_status
        health_check
        ;;
    delete)
        delete_deployment
        ;;
    diff)
        show_diff
        ;;
    status)
        show_status
        ;;
    health)
        health_check
        ;;
    *)
        print_error "Unknown action: $ACTION"
        echo "Valid actions: apply, delete, diff, status, health"
        exit 1
        ;;
esac

print_header "\n=========================================="
print_header "  Done!"
print_header "==========================================\n"
