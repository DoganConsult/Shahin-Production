/**
 * Onboarding Wizard File Upload
 * Drag & drop file upload with preview
 */

class WizardFileUpload {
    constructor(containerId, options = {}) {
        this.container = document.getElementById(containerId);
        if (!this.container) {
            console.warn(`File upload container not found: ${containerId}`);
            return;
        }

        this.options = {
            maxSize: options.maxSize || 5 * 1024 * 1024, // 5MB default
            allowedTypes: options.allowedTypes || ['image/jpeg', 'image/png', 'image/gif', 'application/pdf'],
            multiple: options.multiple || false,
            uploadUrl: options.uploadUrl || '/OnboardingWizard/UploadFile',
            ...options
        };

        this.files = [];
        this.init();
    }

    init() {
        this.createUploadArea();
        this.attachEvents();
    }

    createUploadArea() {
        this.container.innerHTML = `
            <div class="file-upload-area" id="fileUploadDropZone">
                <div class="file-upload-icon">
                    <i class="fas fa-cloud-upload-alt"></i>
                </div>
                <h5>Drag & Drop files here</h5>
                <p class="text-muted">or click to browse</p>
                <input type="file" id="fileInput" class="d-none"
                       ${this.options.multiple ? 'multiple' : ''}
                       accept="${this.options.allowedTypes.join(',')}">
                <small class="text-muted">
                    Max file size: ${this.formatFileSize(this.options.maxSize)}
                </small>
            </div>
            <div id="uploadedFilesList" class="mt-3"></div>
        `;

        this.dropZone = this.container.querySelector('#fileUploadDropZone');
        this.fileInput = this.container.querySelector('#fileInput');
        this.filesList = this.container.querySelector('#uploadedFilesList');
    }

    attachEvents() {
        // Click to browse
        this.dropZone.addEventListener('click', () => {
            this.fileInput.click();
        });

        // File selection
        this.fileInput.addEventListener('change', (e) => {
            this.handleFiles(e.target.files);
        });

        // Drag & drop events
        this.dropZone.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.dropZone.classList.add('drag-over');
        });

        this.dropZone.addEventListener('dragleave', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.dropZone.classList.remove('drag-over');
        });

        this.dropZone.addEventListener('drop', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.dropZone.classList.remove('drag-over');
            this.handleFiles(e.dataTransfer.files);
        });
    }

    handleFiles(fileList) {
        const files = Array.from(fileList);

        files.forEach(file => {
            // Validate file
            if (!this.validateFile(file)) {
                return;
            }

            // Upload file
            this.uploadFile(file);
        });
    }

    validateFile(file) {
        // Check file size
        if (file.size > this.options.maxSize) {
            this.showError(`File "${file.name}" is too large. Max size: ${this.formatFileSize(this.options.maxSize)}`);
            return false;
        }

        // Check file type
        if (!this.options.allowedTypes.includes(file.type)) {
            this.showError(`File "${file.name}" type is not allowed. Allowed types: ${this.options.allowedTypes.join(', ')}`);
            return false;
        }

        return true;
    }

    async uploadFile(file) {
        const fileItem = this.createFileItem(file);
        this.filesList.appendChild(fileItem);

        const formData = new FormData();
        formData.append('file', file);
        formData.append('tenantId', this.options.tenantId || '');
        formData.append('fileType', this.options.fileType || 'document');

        try {
            const response = await fetch(this.options.uploadUrl, {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                throw new Error('Upload failed');
            }

            const result = await response.json();

            // Update file item with success
            this.updateFileItemSuccess(fileItem, result);

            // Store file reference
            this.files.push({
                name: file.name,
                url: result.url,
                id: result.id
            });

            // Trigger callback if provided
            if (this.options.onUploadSuccess) {
                this.options.onUploadSuccess(result);
            }

        } catch (error) {
            this.updateFileItemError(fileItem, error.message);

            if (this.options.onUploadError) {
                this.options.onUploadError(error);
            }
        }
    }

    createFileItem(file) {
        const div = document.createElement('div');
        div.className = 'uploaded-file';
        div.innerHTML = `
            <div class="file-info">
                <i class="fas ${this.getFileIcon(file.type)} file-icon"></i>
                <div>
                    <div class="file-name">${file.name}</div>
                    <small class="text-muted file-size">${this.formatFileSize(file.size)}</small>
                </div>
            </div>
            <div class="file-status">
                <div class="spinner-border spinner-border-sm text-primary" role="status">
                    <span class="visually-hidden">Uploading...</span>
                </div>
            </div>
        `;

        return div;
    }

    updateFileItemSuccess(fileItem, result) {
        const statusDiv = fileItem.querySelector('.file-status');
        statusDiv.innerHTML = `
            <i class="fas fa-check-circle text-success me-2"></i>
            <i class="fas fa-times file-remove" onclick="this.closest('.uploaded-file').remove()"></i>
        `;

        // Add hidden input with file URL/ID
        const hiddenInput = document.createElement('input');
        hiddenInput.type = 'hidden';
        hiddenInput.name = this.options.fieldName || 'uploadedFiles';
        hiddenInput.value = result.url || result.id;
        fileItem.appendChild(hiddenInput);
    }

    updateFileItemError(fileItem, errorMessage) {
        const statusDiv = fileItem.querySelector('.file-status');
        statusDiv.innerHTML = `
            <i class="fas fa-exclamation-circle text-danger" title="${errorMessage}"></i>
            <i class="fas fa-times file-remove" onclick="this.closest('.uploaded-file').remove()"></i>
        `;
        fileItem.classList.add('border-danger');
    }

    getFileIcon(mimeType) {
        const iconMap = {
            'application/pdf': 'fa-file-pdf',
            'image/jpeg': 'fa-file-image',
            'image/png': 'fa-file-image',
            'image/gif': 'fa-file-image',
            'application/zip': 'fa-file-archive',
            'text/plain': 'fa-file-alt',
            'application/msword': 'fa-file-word',
            'application/vnd.openxmlformats-officedocument.wordprocessingml.document': 'fa-file-word',
            'application/vnd.ms-excel': 'fa-file-excel',
            'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': 'fa-file-excel'
        };

        return iconMap[mimeType] || 'fa-file';
    }

    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';

        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));

        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    }

    showError(message) {
        const alert = document.createElement('div');
        alert.className = 'alert alert-danger alert-dismissible fade show mt-2';
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        this.container.appendChild(alert);

        setTimeout(() => {
            alert.remove();
        }, 5000);
    }

    getFiles() {
        return this.files;
    }

    clearFiles() {
        this.files = [];
        this.filesList.innerHTML = '';
    }
}

// Export for use in views
window.WizardFileUpload = WizardFileUpload;
