/**
 * Onboarding Wizard Helper Functions
 * Tooltips, progress estimation, smart pre-filling, etc.
 */

class WizardHelpers {
    constructor() {
        this.stepTimeEstimates = {
            'A': 5,  // minutes
            'B': 4,
            'C': 6,
            'D': 7,
            'E': 6,
            'F': 5,
            'G': 8,
            'H': 5,
            'I': 6,
            'J': 5,
            'K': 7,
            'L': 4
        };

        this.init();
    }

    init() {
        this.initTooltips();
        this.initProgressEstimation();
        this.initSmartPrefilling();
        this.initMobileResponsive();
    }

    // =========================================================================
    // Tooltips and Help System
    // =========================================================================
    initTooltips() {
        // Initialize Bootstrap tooltips if available
        if (typeof bootstrap !== 'undefined') {
            const tooltipTriggerList = [].slice.call(
                document.querySelectorAll('[data-bs-toggle="tooltip"]')
            );
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        }
    }

    addHelpTooltip(fieldId, helpText, example = null) {
        const field = document.getElementById(fieldId);
        if (!field) return;

        const label = field.closest('.mb-3, .form-group')?.querySelector('label');
        if (!label) return;

        // Create help icon
        const helpIcon = document.createElement('span');
        helpIcon.className = 'field-help-icon';
        helpIcon.innerHTML = '?';
        helpIcon.setAttribute('data-bs-toggle', 'tooltip');
        helpIcon.setAttribute('data-bs-placement', 'right');
        helpIcon.setAttribute('data-bs-html', 'true');

        let tooltipContent = helpText;
        if (example) {
            tooltipContent += `<div class="tooltip-example"><strong>Example:</strong> ${example}</div>`;
        }

        helpIcon.setAttribute('data-bs-title', tooltipContent);
        label.appendChild(helpIcon);

        // Reinitialize tooltip
        if (typeof bootstrap !== 'undefined') {
            new bootstrap.Tooltip(helpIcon);
        }
    }

    // =========================================================================
    // Progress Estimation
    // =========================================================================
    initProgressEstimation() {
        const currentStep = this.getCurrentStep();
        if (!currentStep) return;

        this.updateProgressEstimation(currentStep);
    }

    getCurrentStep() {
        // Extract step from URL or page
        const path = window.location.pathname;
        const match = path.match(/Step([A-L])/);
        return match ? match[1] : null;
    }

    updateProgressEstimation(currentStep) {
        // Calculate remaining time
        const stepIndex = currentStep.charCodeAt(0) - 'A'.charCodeAt(0);
        let totalMinutes = 0;

        for (let i = stepIndex; i < 12; i++) {
            const step = String.fromCharCode('A'.charCodeAt(0) + i);
            totalMinutes += this.stepTimeEstimates[step];
        }

        // Display total time remaining
        this.displayTotalTime(totalMinutes);

        // Add time estimate to sidebar steps
        this.addSidebarTimeEstimates();
    }

    displayTotalTime(minutes) {
        const headerDiv = document.querySelector('.container-fluid > .d-flex.justify-content-between');
        if (!headerDiv) return;

        const timeDiv = document.createElement('div');
        timeDiv.className = 'total-time-remaining';
        timeDiv.innerHTML = `
            <i class="fas fa-clock"></i>
            <div class="time-value">${minutes} min</div>
            <small>Estimated time remaining</small>
        `;

        headerDiv.parentElement.insertBefore(timeDiv, headerDiv.nextSibling);
    }

    addSidebarTimeEstimates() {
        const stepNames = document.querySelectorAll('.wizard-step .step-name');

        stepNames.forEach((stepName, index) => {
            const step = String.fromCharCode('A'.charCodeAt(0) + index);
            const minutes = this.stepTimeEstimates[step];

            if (minutes && !stepName.querySelector('.step-time-estimate')) {
                const timeEstimate = document.createElement('div');
                timeEstimate.className = 'step-time-estimate';
                timeEstimate.innerHTML = `<i class="fas fa-clock"></i> ~${minutes} min`;
                stepName.parentElement.appendChild(timeEstimate);
            }
        });
    }

    // =========================================================================
    // Smart Pre-filling
    // =========================================================================
    initSmartPrefilling() {
        // Pre-fill based on email domain
        const emailDomainInputs = document.querySelectorAll('input[name="CorporateEmailDomains"]');
        if (emailDomainInputs.length > 0) {
            emailDomainInputs[0].addEventListener('blur', (e) => {
                this.suggestFromEmailDomain(e.target.value);
            });
        }

        // Pre-fill based on industry sector
        const sectorSelect = document.getElementById('sectorSelect');
        if (sectorSelect) {
            sectorSelect.addEventListener('change', (e) => {
                this.suggestBusinessLines(e.target.value);
            });
        }

        // Auto-set timezone based on country
        const countrySelect = document.querySelector('select[name="CountryOfIncorporation"]');
        const timezoneSelect = document.querySelector('select[name="DefaultTimezone"]');

        if (countrySelect && timezoneSelect) {
            countrySelect.addEventListener('change', (e) => {
                this.autoSetTimezone(e.target.value, timezoneSelect);
            });
        }
    }

    suggestFromEmailDomain(domain) {
        if (!domain) return;

        // Extract TLD and suggest country
        const tld = domain.split('.').pop().toLowerCase();
        const countryMap = {
            'sa': 'SA',
            'ae': 'AE',
            'qa': 'QA',
            'kw': 'KW',
            'bh': 'BH',
            'om': 'OM',
            'eg': 'EG',
            'jo': 'JO'
        };

        const suggestedCountry = countryMap[tld];
        if (suggestedCountry) {
            const countrySelect = document.querySelector('select[name="CountryOfIncorporation"]');
            if (countrySelect && !countrySelect.value) {
                countrySelect.value = suggestedCountry;
                countrySelect.dispatchEvent(new Event('change'));

                // Show notification
                this.showNotification(`Auto-detected country from email domain: ${suggestedCountry}`, 'info');
            }
        }
    }

    suggestBusinessLines(sector) {
        const businessLineMap = {
            'Banking': ['retail', 'corporate', 'payments', 'lending', 'wealth'],
            'Insurance': ['insurance', 'investment'],
            'Telecom': ['telecom_services'],
            'Technology': ['ict_services']
        };

        const suggested = businessLineMap[sector];
        if (!suggested) return;

        // Auto-check suggested business lines
        suggested.forEach(bl => {
            const checkbox = document.getElementById(`bl_${bl}`);
            if (checkbox && !checkbox.checked) {
                checkbox.checked = true;
                checkbox.closest('.form-check').classList.add('pulse-success');
                setTimeout(() => {
                    checkbox.closest('.form-check').classList.remove('pulse-success');
                }, 1500);
            }
        });

        this.showNotification(`Auto-selected common business lines for ${sector}`, 'success');
    }

    autoSetTimezone(countryCode, timezoneSelect) {
        const timezoneMap = {
            'SA': 'Asia/Riyadh',
            'AE': 'Asia/Dubai',
            'QA': 'Asia/Qatar',
            'KW': 'Asia/Kuwait',
            'EG': 'Africa/Cairo',
            'BH': 'Asia/Bahrain',
            'OM': 'Asia/Muscat'
        };

        const timezone = timezoneMap[countryCode];
        if (timezone && !timezoneSelect.value) {
            timezoneSelect.value = timezone;
        }
    }

    // =========================================================================
    // Mobile Responsive
    // =========================================================================
    initMobileResponsive() {
        // Add mobile toggle button for sidebar
        if (window.innerWidth <= 992) {
            this.createSidebarToggle();
        }

        // Handle window resize
        window.addEventListener('resize', () => {
            if (window.innerWidth <= 992 && !document.querySelector('.sidebar-toggle')) {
                this.createSidebarToggle();
            } else if (window.innerWidth > 992) {
                this.removeSidebarToggle();
            }
        });
    }

    createSidebarToggle() {
        if (document.querySelector('.sidebar-toggle')) return;

        // Create toggle button
        const toggleBtn = document.createElement('button');
        toggleBtn.className = 'sidebar-toggle';
        toggleBtn.innerHTML = '<i class="fas fa-bars"></i>';
        document.body.appendChild(toggleBtn);

        // Create overlay
        const overlay = document.createElement('div');
        overlay.className = 'sidebar-overlay';
        document.body.appendChild(overlay);

        const sidebar = document.querySelector('.wizard-sidebar');

        // Toggle sidebar
        toggleBtn.addEventListener('click', () => {
            sidebar.classList.add('show');
            overlay.classList.add('show');
        });

        overlay.addEventListener('click', () => {
            sidebar.classList.remove('show');
            overlay.classList.remove('show');
        });
    }

    removeSidebarToggle() {
        document.querySelector('.sidebar-toggle')?.remove();
        document.querySelector('.sidebar-overlay')?.remove();
        document.querySelector('.wizard-sidebar')?.classList.remove('show');
    }

    // =========================================================================
    // Notifications
    // =========================================================================
    showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show`;
        notification.style.cssText = 'position: fixed; top: 120px; right: 20px; z-index: 1051; min-width: 300px;';
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(notification);

        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => notification.remove(), 150);
        }, 5000);
    }
}

// Export for use in views
window.WizardHelpers = WizardHelpers;

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    new WizardHelpers();
});
