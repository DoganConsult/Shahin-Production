/**
 * Onboarding Wizard Real-time Validation
 * Provides instant feedback as users fill out forms
 */

class WizardValidation {
    constructor() {
        this.validators = {
            email: this.validateEmail,
            domain: this.validateDomain,
            url: this.validateUrl,
            required: this.validateRequired,
            minLength: this.validateMinLength,
            maxLength: this.validateMaxLength,
            pattern: this.validatePattern
        };

        this.init();
    }

    init() {
        // Attach validation to all form fields with validation attributes
        document.querySelectorAll('[data-validate]').forEach(field => {
            this.attachValidation(field);
        });

        // Attach validation to required fields
        document.querySelectorAll('[required]').forEach(field => {
            if (!field.hasAttribute('data-validate')) {
                this.attachValidation(field, 'required');
            }
        });
    }

    attachValidation(field, validationType = null) {
        const type = validationType || field.getAttribute('data-validate');

        // Validate on blur
        field.addEventListener('blur', () => {
            this.validateField(field, type);
        });

        // Validate on input (with debounce)
        let timeout;
        field.addEventListener('input', () => {
            clearTimeout(timeout);
            timeout = setTimeout(() => {
                this.validateField(field, type);
            }, 500);
        });
    }

    validateField(field, type) {
        const value = field.value.trim();
        const validation = type.split(':');
        const validationType = validation[0];
        const param = validation[1];

        let isValid = true;
        let message = '';

        switch (validationType) {
            case 'email':
                isValid = this.validateEmail(value);
                message = isValid ? 'Valid email address' : 'Please enter a valid email address';
                break;

            case 'domain':
                isValid = this.validateDomain(value);
                message = isValid ? 'Valid domain' : 'Please enter a valid domain (e.g., example.com)';
                break;

            case 'url':
                isValid = this.validateUrl(value);
                message = isValid ? 'Valid URL' : 'Please enter a valid URL';
                break;

            case 'required':
                isValid = this.validateRequired(value);
                message = isValid ? '' : 'This field is required';
                break;

            case 'minLength':
                isValid = this.validateMinLength(value, parseInt(param));
                message = isValid ? '' : `Minimum ${param} characters required`;
                break;

            case 'maxLength':
                isValid = this.validateMaxLength(value, parseInt(param));
                message = isValid ? '' : `Maximum ${param} characters allowed`;
                break;

            case 'pattern':
                isValid = this.validatePattern(value, param);
                message = isValid ? 'Valid format' : 'Invalid format';
                break;

            default:
                return;
        }

        this.updateFieldStatus(field, isValid, message);
    }

    updateFieldStatus(field, isValid, message) {
        // Remove existing classes
        field.classList.remove('field-valid', 'field-invalid');

        // Add appropriate class
        if (field.value.trim()) {
            field.classList.add(isValid ? 'field-valid' : 'field-invalid');
        }

        // Update or create validation message
        let messageElement = field.parentElement.querySelector('.validation-message');

        if (!messageElement) {
            messageElement = document.createElement('small');
            messageElement.className = 'validation-message';
            field.parentElement.appendChild(messageElement);
        }

        messageElement.textContent = message;
        messageElement.className = `validation-message ${isValid ? 'success' : 'error'}`;
    }

    // Validator functions
    validateEmail(value) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(value);
    }

    validateDomain(value) {
        const domainRegex = /^[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]?\.[a-zA-Z]{2,}$/;
        return domainRegex.test(value);
    }

    validateUrl(value) {
        try {
            new URL(value);
            return true;
        } catch {
            return false;
        }
    }

    validateRequired(value) {
        return value.length > 0;
    }

    validateMinLength(value, minLength) {
        return value.length >= minLength;
    }

    validateMaxLength(value, maxLength) {
        return value.length <= maxLength;
    }

    validatePattern(value, pattern) {
        const regex = new RegExp(pattern);
        return regex.test(value);
    }
}

// Export for use in views
window.WizardValidation = WizardValidation;

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    new WizardValidation();
});
