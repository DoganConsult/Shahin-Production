/**
 * Onboarding Wizard Auto-save Functionality
 * Automatically saves form data every 30 seconds and on field changes
 */

class WizardAutoSave {
    constructor(formId, tenantId, stepName) {
        this.form = document.getElementById(formId);
        this.tenantId = tenantId;
        this.stepName = stepName;
        this.saveInterval = 30000; // 30 seconds
        this.lastSaveTime = null;
        this.isDirty = false;
        this.saveTimer = null;
        this.lastSaveIndicator = null;

        this.init();
    }

    init() {
        if (!this.form) {
            console.warn('Form not found for auto-save');
            return;
        }

        // Create last save indicator
        this.createSaveIndicator();

        // Load saved data from localStorage
        this.loadFromLocalStorage();

        // Track form changes
        this.form.addEventListener('input', () => {
            this.isDirty = true;
            this.scheduleAutoSave();
        });

        this.form.addEventListener('change', () => {
            this.isDirty = true;
            this.scheduleAutoSave();
        });

        // Save before page unload
        window.addEventListener('beforeunload', (e) => {
            if (this.isDirty) {
                this.saveToLocalStorage();
            }
        });

        // Start auto-save interval
        this.startAutoSave();
    }

    createSaveIndicator() {
        const indicator = document.createElement('div');
        indicator.id = 'autoSaveIndicator';
        indicator.className = 'auto-save-indicator';
        indicator.innerHTML = `
            <span class="save-status">
                <i class="fas fa-circle-notch fa-spin" style="display: none;"></i>
                <i class="fas fa-check-circle text-success" style="display: none;"></i>
                <i class="fas fa-exclamation-triangle text-warning" style="display: none;"></i>
                <span class="save-text">Auto-save enabled</span>
            </span>
        `;

        // Insert at top of form
        this.form.insertBefore(indicator, this.form.firstChild);
        this.lastSaveIndicator = indicator;
    }

    updateSaveIndicator(status, message) {
        if (!this.lastSaveIndicator) return;

        const icons = {
            saving: this.lastSaveIndicator.querySelector('.fa-spin'),
            success: this.lastSaveIndicator.querySelector('.fa-check-circle'),
            error: this.lastSaveIndicator.querySelector('.fa-exclamation-triangle')
        };

        const textSpan = this.lastSaveIndicator.querySelector('.save-text');

        // Hide all icons
        Object.values(icons).forEach(icon => icon.style.display = 'none');

        // Show appropriate icon and message
        switch(status) {
            case 'saving':
                icons.saving.style.display = 'inline-block';
                textSpan.textContent = 'Saving...';
                break;
            case 'success':
                icons.success.style.display = 'inline-block';
                textSpan.textContent = message || `Last saved at ${this.formatTime(new Date())}`;
                break;
            case 'error':
                icons.error.style.display = 'inline-block';
                textSpan.textContent = message || 'Auto-save failed';
                break;
        }
    }

    scheduleAutoSave() {
        // Clear existing timer
        if (this.saveTimer) {
            clearTimeout(this.saveTimer);
        }

        // Schedule new save after 3 seconds of inactivity
        this.saveTimer = setTimeout(() => {
            this.performAutoSave();
        }, 3000);
    }

    startAutoSave() {
        // Save every 30 seconds if dirty
        setInterval(() => {
            if (this.isDirty) {
                this.performAutoSave();
            }
        }, this.saveInterval);
    }

    async performAutoSave() {
        if (!this.isDirty) return;

        this.updateSaveIndicator('saving');

        try {
            // Save to localStorage as backup
            this.saveToLocalStorage();

            // Save to server
            await this.saveToServer();

            this.lastSaveTime = new Date();
            this.isDirty = false;
            this.updateSaveIndicator('success');

        } catch (error) {
            console.error('Auto-save failed:', error);
            this.updateSaveIndicator('error', 'Auto-save failed - saved locally');
            // Still saved to localStorage, so data isn't lost
        }
    }

    getFormData() {
        const formData = new FormData(this.form);
        const data = {};

        for (let [key, value] of formData.entries()) {
            if (data[key]) {
                // Handle multiple values (checkboxes, multi-select)
                if (Array.isArray(data[key])) {
                    data[key].push(value);
                } else {
                    data[key] = [data[key], value];
                }
            } else {
                data[key] = value;
            }
        }

        return data;
    }

    saveToLocalStorage() {
        const data = this.getFormData();
        const storageKey = `wizard_${this.tenantId}_${this.stepName}`;

        try {
            localStorage.setItem(storageKey, JSON.stringify({
                data: data,
                timestamp: new Date().toISOString()
            }));
        } catch (error) {
            console.error('Failed to save to localStorage:', error);
        }
    }

    loadFromLocalStorage() {
        const storageKey = `wizard_${this.tenantId}_${this.stepName}`;

        try {
            const saved = localStorage.getItem(storageKey);
            if (saved) {
                const { data, timestamp } = JSON.parse(saved);

                // Check if saved data is less than 24 hours old
                const savedTime = new Date(timestamp);
                const now = new Date();
                const hoursDiff = (now - savedTime) / (1000 * 60 * 60);

                if (hoursDiff < 24) {
                    // Ask user if they want to restore
                    if (confirm(`Found auto-saved data from ${this.formatTime(savedTime)}. Restore it?`)) {
                        this.restoreFormData(data);
                    } else {
                        localStorage.removeItem(storageKey);
                    }
                }
            }
        } catch (error) {
            console.error('Failed to load from localStorage:', error);
        }
    }

    restoreFormData(data) {
        Object.entries(data).forEach(([key, value]) => {
            const elements = this.form.elements[key];

            if (!elements) return;

            if (elements.length !== undefined) {
                // Multiple elements (checkboxes, radio buttons)
                const values = Array.isArray(value) ? value : [value];
                Array.from(elements).forEach(element => {
                    if (element.type === 'checkbox' || element.type === 'radio') {
                        element.checked = values.includes(element.value);
                    }
                });
            } else {
                // Single element
                if (elements.type === 'checkbox') {
                    elements.checked = value === 'on' || value === 'true';
                } else {
                    elements.value = value;
                }
            }
        });
    }

    async saveToServer() {
        const data = this.getFormData();

        const response = await fetch(`/OnboardingWizard/AutoSave/${this.tenantId}/${this.stepName}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': this.getAntiForgeryToken()
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error('Server save failed');
        }

        return await response.json();
    }

    getAntiForgeryToken() {
        const tokenInput = this.form.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    formatTime(date) {
        return date.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit',
            hour12: true
        });
    }

    clearLocalStorage() {
        const storageKey = `wizard_${this.tenantId}_${this.stepName}`;
        localStorage.removeItem(storageKey);
    }
}

// Export for use in views
window.WizardAutoSave = WizardAutoSave;
