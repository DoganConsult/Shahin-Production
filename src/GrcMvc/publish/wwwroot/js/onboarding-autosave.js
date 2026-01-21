/**
 * Onboarding Wizard Auto-Save and Progress Refresh
 * Ensures search step progress is saved immediately and reflected in next step
 */

(function() {
    'use strict';

    // Configuration
    const AUTO_SAVE_DELAY = 2000; // 2 seconds after last change
    const PROGRESS_REFRESH_INTERVAL = 5000; // Refresh progress every 5 seconds
    let autoSaveTimer = null;
    let progressRefreshInterval = null;

    /**
     * Initialize auto-save for search steps
     */
    function initAutoSave(tenantId, stepName) {
        const searchInputs = document.querySelectorAll(
            'input[type="search"], select[multiple], .search-select, [data-autosave="true"]'
        );

        if (searchInputs.length === 0) return;

        console.log(`[ONBOARDING_AUTOSAVE] Initializing auto-save for step ${stepName}, tenant ${tenantId}`);

        searchInputs.forEach(input => {
            // Listen for changes
            input.addEventListener('change', () => {
                scheduleAutoSave(tenantId, stepName);
            });

            // Listen for input events (for search fields)
            if (input.type === 'search' || input.tagName === 'INPUT') {
                input.addEventListener('input', () => {
                    scheduleAutoSave(tenantId, stepName);
                });
            }
        });

        // Also listen for custom events from search components
        document.addEventListener('onboarding:search:changed', (e) => {
            scheduleAutoSave(tenantId, stepName);
        });
    }

    /**
     * Schedule auto-save after delay
     */
    function scheduleAutoSave(tenantId, stepName) {
        if (autoSaveTimer) {
            clearTimeout(autoSaveTimer);
        }

        autoSaveTimer = setTimeout(() => {
            performAutoSave(tenantId, stepName);
        }, AUTO_SAVE_DELAY);
    }

    /**
     * Perform auto-save
     */
    async function performAutoSave(tenantId, stepName) {
        try {
            const formData = collectFormData(stepName);
            
            const response = await fetch(`/OnboardingWizard/AutoSave/${tenantId}/${stepName}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(formData)
            });

            const result = await response.json();

            if (result.success) {
                console.log(`[ONBOARDING_AUTOSAVE] Progress saved. Progress: ${result.progressPercent}%`);
                
                // Update progress indicator
                updateProgressIndicator(result);
                
                // Show save indicator
                showSaveIndicator(true);
            } else {
                console.error('[ONBOARDING_AUTOSAVE] Save failed:', result.message);
                showSaveIndicator(false, result.message);
            }
        } catch (error) {
            console.error('[ONBOARDING_AUTOSAVE] Error:', error);
            showSaveIndicator(false, 'Network error');
        }
    }

    /**
     * Collect form data for the current step
     */
    function collectFormData(stepName) {
        const formData = {};

        switch (stepName.toUpperCase()) {
            case 'STEPC':
            case 'C':
                // Collect Step C data
                const primaryRegulators = getSelectedValues('PrimaryRegulators');
                const secondaryRegulators = getSelectedValues('SecondaryRegulators');
                const mandatoryFrameworks = getSelectedValues('MandatoryFrameworks');
                const optionalFrameworks = getSelectedValues('OptionalFrameworks');
                
                if (primaryRegulators.length > 0) formData.PrimaryRegulators = primaryRegulators;
                if (secondaryRegulators.length > 0) formData.SecondaryRegulators = secondaryRegulators;
                if (mandatoryFrameworks.length > 0) formData.MandatoryFrameworks = mandatoryFrameworks;
                if (optionalFrameworks.length > 0) formData.OptionalFrameworks = optionalFrameworks;
                break;

            case 'STEPK':
            case 'K':
                // Collect Step K data
                const selectedBaselines = getSelectedValues('SelectedBaselines');
                const selectedOverlays = getSelectedValues('SelectedOverlays');
                
                if (selectedBaselines.length > 0) formData.SelectedBaselines = selectedBaselines;
                if (selectedOverlays.length > 0) formData.SelectedOverlays = selectedOverlays;
                break;
        }

        return formData;
    }

    /**
     * Get selected values from a multi-select or checkbox group
     */
    function getSelectedValues(name) {
        const values = [];
        
        // Try select[multiple]
        const select = document.querySelector(`select[name="${name}"], select[id="${name}"]`);
        if (select && select.multiple) {
            Array.from(select.selectedOptions).forEach(option => {
                values.push(option.value);
            });
        }
        
        // Try checkboxes
        const checkboxes = document.querySelectorAll(`input[type="checkbox"][name^="${name}"], input[type="checkbox"][id^="${name}"]`);
        checkboxes.forEach(checkbox => {
            if (checkbox.checked) {
                values.push(checkbox.value);
            }
        });
        
        // Try data attributes
        const dataElements = document.querySelectorAll(`[data-${name.toLowerCase()}]`);
        dataElements.forEach(el => {
            const value = el.getAttribute(`data-${name.toLowerCase()}`);
            if (value && !values.includes(value)) {
                values.push(value);
            }
        });

        return values;
    }

    /**
     * Update progress indicator in sidebar
     */
    function updateProgressIndicator(result) {
        if (result.progressPercent !== undefined) {
            const progressBar = document.querySelector('.progress-bar');
            const progressPercent = document.querySelector('.text-primary.fw-bold');
            
            if (progressBar) {
                progressBar.style.width = `${result.progressPercent}%`;
                progressBar.setAttribute('aria-valuenow', result.progressPercent);
            }
            
            if (progressPercent) {
                progressPercent.textContent = `${result.progressPercent}%`;
            }
        }

        // Update step badges
        if (result.completedSections) {
            result.completedSections.forEach(section => {
                const stepNumber = section.charCodeAt(0) - 'A'.charCodeAt(0) + 1;
                const stepElement = document.querySelector(`[data-step="${stepNumber}"]`);
                if (stepElement) {
                    stepElement.classList.add('completed');
                    const badge = stepElement.querySelector('.step-badge');
                    if (badge) {
                        badge.className = 'badge bg-success ms-1 step-badge';
                        badge.innerHTML = '<i class="fas fa-check"></i>';
                    }
                }
            });
        }
    }

    /**
     * Show save indicator
     */
    function showSaveIndicator(success, message = null) {
        // Remove existing indicator
        const existing = document.getElementById('autosave-indicator');
        if (existing) {
            existing.remove();
        }

        // Create indicator
        const indicator = document.createElement('div');
        indicator.id = 'autosave-indicator';
        indicator.className = `alert alert-${success ? 'success' : 'danger'} alert-dismissible fade show position-fixed`;
        indicator.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 250px;';
        indicator.innerHTML = `
            <i class="fas fa-${success ? 'check-circle' : 'exclamation-circle'} me-2"></i>
            ${success ? 'Progress saved' : (message || 'Save failed')}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(indicator);

        // Auto-dismiss after 3 seconds
        setTimeout(() => {
            if (indicator.parentNode) {
                indicator.remove();
            }
        }, 3000);
    }

    /**
     * Get anti-forgery token
     */
    function getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    /**
     * Refresh progress from server
     */
    async function refreshProgress(tenantId) {
        try {
            const response = await fetch(`/OnboardingWizard/GetProgress/${tenantId}`);
            const result = await response.json();

            if (result.success) {
                updateProgressIndicator(result);
            }
        } catch (error) {
            console.error('[ONBOARDING_AUTOSAVE] Progress refresh error:', error);
        }
    }

    /**
     * Initialize progress refresh interval
     */
    function initProgressRefresh(tenantId) {
        if (progressRefreshInterval) {
            clearInterval(progressRefreshInterval);
        }

        progressRefreshInterval = setInterval(() => {
            refreshProgress(tenantId);
        }, PROGRESS_REFRESH_INTERVAL);
    }

    /**
     * Initialize when DOM is ready
     */
    function init() {
        // Get tenant ID and step name from page
        const tenantIdMatch = window.location.pathname.match(/\/OnboardingWizard\/Step[A-L]\/([a-f0-9-]+)/i);
        const stepNameMatch = window.location.pathname.match(/\/OnboardingWizard\/(Step[A-L])/i);
        
        if (tenantIdMatch && stepNameMatch) {
            const tenantId = tenantIdMatch[1];
            const stepName = stepNameMatch[1].replace('Step', '');
            
            // Initialize auto-save for search steps (C and K)
            if (stepName === 'C' || stepName === 'K') {
                initAutoSave(tenantId, stepName);
            }
            
            // Initialize progress refresh for all steps
            initProgressRefresh(tenantId);
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Cleanup on page unload
    window.addEventListener('beforeunload', () => {
        if (autoSaveTimer) clearTimeout(autoSaveTimer);
        if (progressRefreshInterval) clearInterval(progressRefreshInterval);
    });

})();
