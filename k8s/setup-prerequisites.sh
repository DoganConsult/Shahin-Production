#!/bin/bash
# =============================================================================
# Shahin GRC Platform - Kubernetes Prerequisites Installation
# =============================================================================
# This script installs all required components for the Shahin GRC Platform:
# - Longhorn (Storage)
# - Cert-Manager (TLS Certificates)
# - Traefik (Ingress Controller)
# - Metrics Server (HPA)
# - Prometheus Operator (Monitoring) - Optional
#
# Prerequisites:
# - kubectl configured with cluster admin access
# - helm v3 installed
# - Kubernetes cluster with 3+ nodes (for HA)
# =============================================================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_status() { echo -e "${GREEN}[INFO]${NC} $1"; }
print_warning() { echo -e "${YELLOW}[WARN]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }
print_header() { echo -e "\n${BLUE}========================================${NC}"; echo -e "${BLUE}  $1${NC}"; echo -e "${BLUE}========================================${NC}\n"; }

# Check prerequisites
check_prerequisites() {
    print_header "Checking Prerequisites"

    if ! command -v kubectl &> /dev/null; then
        print_error "kubectl not found. Please install kubectl first."
        exit 1
    fi
    print_status "kubectl found"

    if ! command -v helm &> /dev/null; then
        print_error "helm not found. Please install helm v3 first."
        echo "Install with: curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash"
        exit 1
    fi
    print_status "helm found"

    if ! kubectl cluster-info &> /dev/null; then
        print_error "Cannot connect to Kubernetes cluster"
        exit 1
    fi
    print_status "Connected to Kubernetes cluster"

    # Check node count
    NODE_COUNT=$(kubectl get nodes --no-headers | wc -l)
    if [[ $NODE_COUNT -lt 3 ]]; then
        print_warning "Only $NODE_COUNT node(s) found. HA requires at least 3 nodes."
        print_warning "Some features may not work optimally."
    else
        print_status "Found $NODE_COUNT nodes"
    fi
}

# Install Longhorn Storage
install_longhorn() {
    print_header "Installing Longhorn Storage"

    # Check if already installed
    if kubectl get namespace longhorn-system &> /dev/null; then
        print_warning "Longhorn already installed, skipping..."
        return
    fi

    print_status "Adding Longhorn Helm repository..."
    helm repo add longhorn https://charts.longhorn.io
    helm repo update

    print_status "Installing Longhorn..."
    kubectl create namespace longhorn-system || true

    helm install longhorn longhorn/longhorn \
        --namespace longhorn-system \
        --set defaultSettings.defaultDataPath="/var/lib/longhorn" \
        --set defaultSettings.defaultReplicaCount=2 \
        --set persistence.defaultClassReplicaCount=2 \
        --set csi.attacherReplicaCount=2 \
        --set csi.provisionerReplicaCount=2 \
        --wait --timeout 10m

    print_status "Waiting for Longhorn to be ready..."
    kubectl wait --for=condition=ready pod -l app=longhorn-manager -n longhorn-system --timeout=300s

    # Set as default storage class
    print_status "Setting Longhorn as default storage class..."
    kubectl patch storageclass longhorn -p '{"metadata": {"annotations":{"storageclass.kubernetes.io/is-default-class":"true"}}}'

    print_status "Longhorn installed successfully!"
}

# Install Cert-Manager
install_cert_manager() {
    print_header "Installing Cert-Manager"

    if kubectl get namespace cert-manager &> /dev/null; then
        print_warning "Cert-Manager already installed, skipping..."
        return
    fi

    print_status "Adding Jetstack Helm repository..."
    helm repo add jetstack https://charts.jetstack.io
    helm repo update

    print_status "Installing Cert-Manager CRDs..."
    kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.14.0/cert-manager.crds.yaml

    print_status "Installing Cert-Manager..."
    helm install cert-manager jetstack/cert-manager \
        --namespace cert-manager \
        --create-namespace \
        --version v1.14.0 \
        --set installCRDs=false \
        --wait --timeout 5m

    print_status "Waiting for Cert-Manager to be ready..."
    kubectl wait --for=condition=ready pod -l app=cert-manager -n cert-manager --timeout=300s

    # Create Let's Encrypt ClusterIssuers
    print_status "Creating Let's Encrypt ClusterIssuers..."
    cat <<EOF | kubectl apply -f -
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-staging
spec:
  acme:
    server: https://acme-staging-v02.api.letsencrypt.org/directory
    email: admin@shahin-ai.com
    privateKeySecretRef:
      name: letsencrypt-staging-key
    solvers:
    - http01:
        ingress:
          class: traefik
---
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@shahin-ai.com
    privateKeySecretRef:
      name: letsencrypt-prod-key
    solvers:
    - http01:
        ingress:
          class: traefik
EOF

    print_status "Cert-Manager installed successfully!"
}

# Install Traefik Ingress Controller
install_traefik() {
    print_header "Installing Traefik Ingress Controller"

    if kubectl get namespace traefik &> /dev/null; then
        print_warning "Traefik already installed, skipping..."
        return
    fi

    print_status "Adding Traefik Helm repository..."
    helm repo add traefik https://traefik.github.io/charts
    helm repo update

    print_status "Installing Traefik..."
    helm install traefik traefik/traefik \
        --namespace traefik \
        --create-namespace \
        --set deployment.replicas=2 \
        --set ingressClass.enabled=true \
        --set ingressClass.isDefaultClass=true \
        --set ports.web.redirectTo.port=websecure \
        --set ports.websecure.tls.enabled=true \
        --set service.type=LoadBalancer \
        --set additionalArguments="{--api.insecure=false,--log.level=INFO}" \
        --wait --timeout 5m

    print_status "Waiting for Traefik to be ready..."
    kubectl wait --for=condition=ready pod -l app.kubernetes.io/name=traefik -n traefik --timeout=300s

    # Get LoadBalancer IP
    print_status "Getting Traefik LoadBalancer IP..."
    sleep 10
    TRAEFIK_IP=$(kubectl get svc traefik -n traefik -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "pending")
    if [[ "$TRAEFIK_IP" == "pending" || -z "$TRAEFIK_IP" ]]; then
        print_warning "LoadBalancer IP not yet assigned. Check later with:"
        echo "  kubectl get svc traefik -n traefik"
    else
        print_status "Traefik LoadBalancer IP: $TRAEFIK_IP"
        echo ""
        print_status "Configure your DNS records to point to: $TRAEFIK_IP"
    fi

    print_status "Traefik installed successfully!"
}

# Install Metrics Server
install_metrics_server() {
    print_header "Installing Metrics Server"

    if kubectl get deployment metrics-server -n kube-system &> /dev/null; then
        print_warning "Metrics Server already installed, skipping..."
        return
    fi

    print_status "Installing Metrics Server..."
    kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml

    # Patch for self-signed certificates (common in on-prem setups)
    print_status "Patching Metrics Server for self-signed certificates..."
    kubectl patch deployment metrics-server -n kube-system \
        --type='json' \
        -p='[{"op": "add", "path": "/spec/template/spec/containers/0/args/-", "value": "--kubelet-insecure-tls"}]' || true

    print_status "Waiting for Metrics Server to be ready..."
    kubectl wait --for=condition=available deployment/metrics-server -n kube-system --timeout=300s

    print_status "Metrics Server installed successfully!"
}

# Install Prometheus Operator (Optional)
install_prometheus() {
    print_header "Installing Prometheus Operator (Optional)"

    read -p "Install Prometheus Operator for monitoring? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_warning "Skipping Prometheus installation"
        return
    fi

    if kubectl get namespace monitoring &> /dev/null; then
        print_warning "Prometheus already installed, skipping..."
        return
    fi

    print_status "Adding Prometheus Helm repository..."
    helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
    helm repo update

    print_status "Installing Prometheus Operator stack..."
    helm install prometheus prometheus-community/kube-prometheus-stack \
        --namespace monitoring \
        --create-namespace \
        --set prometheus.prometheusSpec.retention=15d \
        --set prometheus.prometheusSpec.storageSpec.volumeClaimTemplate.spec.storageClassName=longhorn \
        --set prometheus.prometheusSpec.storageSpec.volumeClaimTemplate.spec.resources.requests.storage=50Gi \
        --set alertmanager.alertmanagerSpec.storage.volumeClaimTemplate.spec.storageClassName=longhorn \
        --set alertmanager.alertmanagerSpec.storage.volumeClaimTemplate.spec.resources.requests.storage=10Gi \
        --set grafana.persistence.enabled=true \
        --set grafana.persistence.storageClassName=longhorn \
        --set grafana.persistence.size=10Gi \
        --wait --timeout 10m

    print_status "Prometheus Operator installed successfully!"
    print_status "Access Grafana with: kubectl port-forward svc/prometheus-grafana 3000:80 -n monitoring"
    print_status "Default credentials: admin / prom-operator"
}

# Verify installation
verify_installation() {
    print_header "Verifying Installation"

    echo ""
    print_status "Storage Classes:"
    kubectl get storageclass
    echo ""

    print_status "Cert-Manager Status:"
    kubectl get pods -n cert-manager
    echo ""

    print_status "Traefik Status:"
    kubectl get pods -n traefik
    kubectl get svc -n traefik
    echo ""

    print_status "Metrics Server Status:"
    kubectl get deployment metrics-server -n kube-system
    echo ""

    # Test metrics
    print_status "Testing Metrics Server..."
    kubectl top nodes 2>/dev/null || print_warning "Metrics not yet available (may take a minute)"
}

# Print DNS configuration guide
print_dns_guide() {
    print_header "DNS Configuration Guide"

    TRAEFIK_IP=$(kubectl get svc traefik -n traefik -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "PENDING")

    echo "Configure the following DNS records pointing to: $TRAEFIK_IP"
    echo ""
    echo "Development:"
    echo "  dev.shahin-ai.com          -> $TRAEFIK_IP"
    echo ""
    echo "Staging:"
    echo "  staging.shahin-ai.com      -> $TRAEFIK_IP"
    echo "  staging-portal.shahin-ai.com -> $TRAEFIK_IP"
    echo ""
    echo "Production:"
    echo "  portal.shahin-ai.com       -> $TRAEFIK_IP"
    echo "  app.shahin-ai.com          -> $TRAEFIK_IP"
    echo "  www.shahin-ai.com          -> $TRAEFIK_IP"
    echo ""
    echo "For local development without DNS, use port-forwarding:"
    echo "  kubectl port-forward svc/dev-grc-portal 8080:8080 -n shahin-grc-dev"
}

# Main
main() {
    print_header "Shahin GRC Platform - Prerequisites Setup"

    check_prerequisites

    echo ""
    echo "This script will install:"
    echo "  1. Longhorn Storage Class"
    echo "  2. Cert-Manager (TLS certificates)"
    echo "  3. Traefik Ingress Controller"
    echo "  4. Metrics Server (for HPA)"
    echo "  5. Prometheus Operator (optional)"
    echo ""

    read -p "Continue with installation? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_warning "Installation cancelled"
        exit 0
    fi

    install_longhorn
    install_cert_manager
    install_traefik
    install_metrics_server
    install_prometheus

    verify_installation
    print_dns_guide

    print_header "Setup Complete!"
    echo ""
    echo "Next steps:"
    echo "1. Configure DNS records as shown above"
    echo "2. Update secrets in k8s/overlays/<env>/secrets.yaml"
    echo "3. Deploy with: ./deploy.sh <environment> apply"
    echo ""
}

# Run
main "$@"
