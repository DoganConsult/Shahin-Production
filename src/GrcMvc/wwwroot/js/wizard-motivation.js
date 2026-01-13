/**
 * Wizard Motivation & Smart Guidance System
 * Provides gamification, progress celebrations, smart tips, and dynamic guidance
 */

class WizardMotivation {
    constructor(options = {}) {
        this.stepNumber = options.stepNumber || 1;
        this.totalSteps = options.totalSteps || 12;
        this.tenantId = options.tenantId || '';
        this.stepName = options.stepName || 'StepA';

        // Configuration
        this.config = {
            enableCelebrations: true,
            enableTips: true,
            enableProgress: true,
            enablePreview: true,
            tipDelay: 3000,
            celebrationDuration: 2500
        };

        // Step metadata with tips and unlock messages
        this.stepMetadata = {
            StepA: {
                title: 'Organization Identity',
                icon: 'fa-building',
                color: 'primary',
                estimatedTime: '3-5 min',
                unlocks: ['Workspace configuration', 'Jurisdiction rules', 'Bilingual support'],
                tips: [
                    'Your legal name will appear on compliance reports and audit documents.',
                    'Operating in multiple countries? We\'ll suggest relevant regional frameworks.',
                    'The industry sector helps us recommend the right compliance frameworks automatically.'
                ],
                outputs: ['Tenant Profile', 'Default Workspace', 'Localization Settings']
            },
            StepB: {
                title: 'Assurance Objective',
                icon: 'fa-bullseye',
                color: 'info',
                estimatedTime: '2-3 min',
                unlocks: ['Assessment prioritization', 'Dashboard KPIs', 'Reporting setup'],
                tips: [
                    'Select your primary driver to help us prioritize your compliance activities.',
                    'Pain points help us customize your dashboard focus areas.',
                    'Your desired maturity level sets the improvement tracking goals.'
                ],
                outputs: ['Compliance Goals', 'KPI Configuration', 'Priority Matrix']
            },
            StepC: {
                title: 'Regulatory Applicability',
                icon: 'fa-gavel',
                color: 'danger',
                estimatedTime: '3-4 min',
                unlocks: ['Control baseline selection', 'Audit scope definition', 'Framework mapping'],
                tips: [
                    'Select your regulators and we\'ll automatically suggest mandatory frameworks.',
                    'Existing certifications can reduce duplicate compliance efforts.',
                    'Benchmarking frameworks help you align with international best practices.'
                ],
                outputs: ['Framework Selection', 'Regulatory Map', 'Control Baseline']
            },
            StepD: {
                title: 'Scope Definition',
                icon: 'fa-crosshairs',
                color: 'warning',
                estimatedTime: '4-6 min',
                unlocks: ['Assessment boundary', 'Control applicability filtering', 'Entity mapping'],
                tips: [
                    'Define your scope precisely to ensure controls are relevant.',
                    'System criticality helps prioritize assessments and RTO/RPO.',
                    'Document exclusions for audit trail and regulatory justification.'
                ],
                outputs: ['Scope Matrix', 'Entity Register', 'Criticality Tiers']
            },
            StepE: {
                title: 'Data & Risk Profile',
                icon: 'fa-shield-alt',
                color: 'danger',
                estimatedTime: '3-4 min',
                unlocks: ['Evidence requirements', 'PDPL/PCI-DSS rules', 'Vendor risk setup'],
                tips: [
                    'Data types determine which privacy frameworks apply (PDPL, GDPR).',
                    'Payment card data triggers PCI-DSS requirements automatically.',
                    'Third-party processors are key for vendor risk management.'
                ],
                outputs: ['Risk Heat Map', 'Data Classification', 'Vendor Risk Baseline']
            },
            StepF: {
                title: 'Technology Landscape',
                icon: 'fa-server',
                color: 'info',
                estimatedTime: '4-5 min',
                unlocks: ['Integration configuration', 'Evidence storage', 'SSO setup'],
                tips: [
                    'Connecting your identity provider enables seamless SSO.',
                    'Your SIEM integration can automate security monitoring evidence.',
                    'Cloud providers inform cloud-specific security controls.'
                ],
                outputs: ['Integration Blueprint', 'Evidence Sources', 'Automation Points']
            },
            StepG: {
                title: 'Control Ownership',
                icon: 'fa-user-shield',
                color: 'success',
                estimatedTime: '3-4 min',
                unlocks: ['RACI mapping', 'Approval workflows', 'Escalation paths'],
                tips: [
                    'Choose centralized for smaller teams, federated for large organizations.',
                    'Exception approvers ensure risk acceptance is properly authorized.',
                    'Clear ownership prevents controls from falling through the cracks.'
                ],
                outputs: ['RACI Matrix', 'Approval Workflow', 'Governance Structure']
            },
            StepH: {
                title: 'Teams & Access',
                icon: 'fa-users',
                color: 'primary',
                estimatedTime: '4-5 min',
                unlocks: ['User workspace assignment', 'Role provisioning', 'Notifications'],
                tips: [
                    'Team structure enables collaboration on compliance tasks.',
                    'RACI mapping ensures clear responsibilities per control.',
                    'Notification preferences keep the right people informed.'
                ],
                outputs: ['Team Workspaces', 'Role Assignments', 'Notification Config']
            },
            StepI: {
                title: 'Workflow & Cadence',
                icon: 'fa-calendar-alt',
                color: 'warning',
                estimatedTime: '3-4 min',
                unlocks: ['Calendar reminders', 'Deadline tracking', 'Escalation rules'],
                tips: [
                    'Evidence frequency determines your compliance calendar.',
                    'SLAs ensure timely evidence submission and remediation.',
                    'Regular reviews keep your security posture current.'
                ],
                outputs: ['Compliance Calendar', 'SLA Configuration', 'Reminder System']
            },
            StepJ: {
                title: 'Evidence Standards',
                icon: 'fa-file-alt',
                color: 'info',
                estimatedTime: '2-3 min',
                unlocks: ['Evidence naming', 'Storage policies', 'Retention rules'],
                tips: [
                    'Naming conventions ensure consistent evidence organization.',
                    'Retention periods meet regulatory requirements.',
                    'Access rules protect sensitive evidence appropriately.'
                ],
                outputs: ['Evidence Pack Template', 'Naming Convention', 'Retention Policy']
            },
            StepK: {
                title: 'Baseline & Overlays',
                icon: 'fa-layer-group',
                color: 'success',
                estimatedTime: '2-3 min',
                unlocks: ['Final control set', 'Custom controls', 'Overlay application'],
                tips: [
                    'Review the suggested baseline for accuracy.',
                    'Overlays add industry or jurisdiction-specific controls.',
                    'Custom controls capture internal policy requirements.'
                ],
                outputs: ['Final Control Library', 'Overlay Configuration', 'Custom Controls']
            },
            StepL: {
                title: 'Go-Live & Metrics',
                icon: 'fa-rocket',
                color: 'danger',
                estimatedTime: '3-4 min',
                unlocks: ['Dashboard metrics', 'Improvement tracking', 'Success KPIs'],
                tips: [
                    'Success metrics help demonstrate compliance program value.',
                    'Baseline measurements enable ROI tracking.',
                    'Target improvements drive continuous enhancement.'
                ],
                outputs: ['Success Dashboard', 'KPI Tracker', 'Improvement Goals']
            }
        };

        this.init();
    }

    init() {
        this.createMotivationElements();
        this.attachEventListeners();
        this.showEntryAnimation();

        // Delayed tip display
        if (this.config.enableTips) {
            setTimeout(() => this.showSmartTip(), this.config.tipDelay);
        }
    }

    createMotivationElements() {
        // Create celebration overlay
        if (!document.getElementById('celebration-overlay')) {
            const celebrationOverlay = document.createElement('div');
            celebrationOverlay.id = 'celebration-overlay';
            celebrationOverlay.className = 'celebration-overlay';
            celebrationOverlay.innerHTML = `
                <div class="celebration-content">
                    <div class="celebration-icon"><i class="fas fa-check-circle"></i></div>
                    <h3 class="celebration-title">Step Complete!</h3>
                    <p class="celebration-message"></p>
                    <div class="celebration-unlocks"></div>
                </div>
            `;
            document.body.appendChild(celebrationOverlay);
        }

        // Create smart tip container
        if (!document.getElementById('smart-tip-container')) {
            const tipContainer = document.createElement('div');
            tipContainer.id = 'smart-tip-container';
            tipContainer.className = 'smart-tip-container';
            tipContainer.innerHTML = `
                <div class="smart-tip">
                    <div class="smart-tip-header">
                        <i class="fas fa-lightbulb"></i>
                        <span>Smart Tip</span>
                        <button class="smart-tip-close"><i class="fas fa-times"></i></button>
                    </div>
                    <div class="smart-tip-content"></div>
                </div>
            `;
            document.body.appendChild(tipContainer);

            tipContainer.querySelector('.smart-tip-close').addEventListener('click', () => {
                this.hideTip();
            });
        }

        // Create progress celebration particles
        if (!document.getElementById('particles-container')) {
            const particles = document.createElement('div');
            particles.id = 'particles-container';
            particles.className = 'particles-container';
            document.body.appendChild(particles);
        }
    }

    attachEventListeners() {
        // Track form field completion for micro-celebrations
        document.querySelectorAll('.form-control, .form-select').forEach(field => {
            field.addEventListener('blur', (e) => {
                if (e.target.value) {
                    this.showFieldComplete(e.target);
                }
            });
        });

        // Track card completion
        document.querySelectorAll('.card').forEach((card, index) => {
            const requiredFields = card.querySelectorAll('[required]');
            if (requiredFields.length > 0) {
                requiredFields.forEach(field => {
                    field.addEventListener('change', () => {
                        this.checkCardCompletion(card, index);
                    });
                });
            }
        });

        // Form submission celebration
        const form = document.querySelector('form');
        if (form) {
            form.addEventListener('submit', (e) => {
                // Only show celebration for valid form
                if (form.checkValidity()) {
                    this.showStepCompleteCelebration();
                }
            });
        }
    }

    showEntryAnimation() {
        const cards = document.querySelectorAll('.card');
        cards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(30px)';

            setTimeout(() => {
                card.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 100 + (index * 150));
        });
    }

    showFieldComplete(field) {
        const wrapper = field.closest('.mb-3') || field.parentElement;

        // Add subtle success indication
        const existingIndicator = wrapper.querySelector('.field-success-indicator');
        if (!existingIndicator) {
            const indicator = document.createElement('span');
            indicator.className = 'field-success-indicator';
            indicator.innerHTML = '<i class="fas fa-check text-success"></i>';
            wrapper.appendChild(indicator);

            setTimeout(() => {
                indicator.classList.add('show');
            }, 10);
        }
    }

    checkCardCompletion(card, cardIndex) {
        const requiredFields = card.querySelectorAll('[required]');
        let allComplete = true;

        requiredFields.forEach(field => {
            if (!field.value) allComplete = false;
        });

        if (allComplete) {
            this.showCardCompleteCelebration(card, cardIndex);
        }
    }

    showCardCompleteCelebration(card, cardIndex) {
        const cardHeader = card.querySelector('.card-header');

        // Only celebrate once
        if (cardHeader.classList.contains('celebrated')) return;
        cardHeader.classList.add('celebrated');

        // Add completion badge
        if (!cardHeader.querySelector('.card-complete-badge')) {
            const badge = document.createElement('span');
            badge.className = 'card-complete-badge';
            badge.innerHTML = '<i class="fas fa-check-circle"></i> Complete';
            cardHeader.appendChild(badge);

            setTimeout(() => badge.classList.add('show'), 10);
        }

        // Subtle pulse animation
        card.classList.add('card-complete-pulse');
        setTimeout(() => card.classList.remove('card-complete-pulse'), 600);
    }

    showStepCompleteCelebration() {
        if (!this.config.enableCelebrations) return;

        const metadata = this.stepMetadata[this.stepName] || {};
        const overlay = document.getElementById('celebration-overlay');

        if (overlay) {
            // Update content
            overlay.querySelector('.celebration-title').textContent =
                `${metadata.title || 'Step'} Complete!`;
            overlay.querySelector('.celebration-message').textContent =
                `Great job! You've completed step ${this.stepNumber} of ${this.totalSteps}`;

            // Show unlocks
            const unlocksContainer = overlay.querySelector('.celebration-unlocks');
            if (metadata.unlocks && metadata.unlocks.length > 0) {
                unlocksContainer.innerHTML = `
                    <p class="unlock-title">You unlocked:</p>
                    <div class="unlock-items">
                        ${metadata.unlocks.map(u => `
                            <span class="unlock-item">
                                <i class="fas fa-unlock"></i> ${u}
                            </span>
                        `).join('')}
                    </div>
                `;
            }

            // Show overlay
            overlay.classList.add('show');

            // Show particles
            this.showParticles();

            // Hide after duration
            setTimeout(() => {
                overlay.classList.remove('show');
            }, this.config.celebrationDuration);
        }
    }

    showParticles() {
        const container = document.getElementById('particles-container');
        if (!container) return;

        const colors = ['#10B981', '#3B82F6', '#F59E0B', '#8B5CF6', '#EC4899'];

        for (let i = 0; i < 50; i++) {
            const particle = document.createElement('div');
            particle.className = 'particle';
            particle.style.background = colors[Math.floor(Math.random() * colors.length)];
            particle.style.left = Math.random() * 100 + 'vw';
            particle.style.animationDelay = Math.random() * 0.5 + 's';
            particle.style.animationDuration = (Math.random() * 1 + 1) + 's';
            container.appendChild(particle);

            setTimeout(() => particle.remove(), 2000);
        }
    }

    showSmartTip() {
        const metadata = this.stepMetadata[this.stepName];
        if (!metadata || !metadata.tips || metadata.tips.length === 0) return;

        const container = document.getElementById('smart-tip-container');
        if (!container) return;

        // Get random tip
        const tip = metadata.tips[Math.floor(Math.random() * metadata.tips.length)];

        container.querySelector('.smart-tip-content').textContent = tip;
        container.classList.add('show');

        // Auto-hide after 10 seconds
        setTimeout(() => this.hideTip(), 10000);
    }

    hideTip() {
        const container = document.getElementById('smart-tip-container');
        if (container) {
            container.classList.remove('show');
        }
    }

    // Get step metadata for external use
    getStepMetadata() {
        return this.stepMetadata[this.stepName] || {};
    }

    // Update progress in sidebar
    updateProgress(percent) {
        const progressBar = document.querySelector('.wizard-sidebar .progress-bar');
        if (progressBar) {
            progressBar.style.width = percent + '%';
            progressBar.setAttribute('aria-valuenow', percent);
        }

        const progressText = document.querySelector('.wizard-sidebar .text-primary.fw-bold');
        if (progressText) {
            progressText.textContent = percent + '%';
        }
    }
}

/**
 * Live Preview Panel Component
 * Shows what will be generated based on current inputs
 */
class LivePreviewPanel {
    constructor(options = {}) {
        this.containerId = options.containerId || 'live-preview-panel';
        this.stepName = options.stepName || 'StepA';
        this.previewData = {};

        this.previewGenerators = {
            StepA: this.generateStepAPreview.bind(this),
            StepB: this.generateStepBPreview.bind(this),
            StepC: this.generateStepCPreview.bind(this),
            StepD: this.generateStepDPreview.bind(this),
            StepE: this.generateStepEPreview.bind(this),
            StepF: this.generateStepFPreview.bind(this)
        };

        this.init();
    }

    init() {
        this.createPreviewPanel();
        this.attachInputListeners();
        this.updatePreview();
    }

    createPreviewPanel() {
        const existingPanel = document.getElementById(this.containerId);
        if (existingPanel) return;

        const panel = document.createElement('div');
        panel.id = this.containerId;
        panel.className = 'live-preview-panel';
        panel.innerHTML = `
            <div class="preview-header">
                <i class="fas fa-eye"></i>
                <span>Live Preview</span>
                <button class="preview-toggle" title="Collapse">
                    <i class="fas fa-chevron-right"></i>
                </button>
            </div>
            <div class="preview-body">
                <div class="preview-section">
                    <h6><i class="fas fa-cog"></i> What will be generated:</h6>
                    <div class="preview-outputs"></div>
                </div>
                <div class="preview-section">
                    <h6><i class="fas fa-chart-line"></i> Impact:</h6>
                    <div class="preview-impact"></div>
                </div>
            </div>
        `;

        // Add to main content area
        const mainContent = document.querySelector('.wizard-main-content');
        if (mainContent) {
            mainContent.appendChild(panel);
        }

        // Toggle functionality
        panel.querySelector('.preview-toggle').addEventListener('click', () => {
            panel.classList.toggle('collapsed');
            const icon = panel.querySelector('.preview-toggle i');
            icon.classList.toggle('fa-chevron-right');
            icon.classList.toggle('fa-chevron-left');
        });
    }

    attachInputListeners() {
        document.querySelectorAll('.form-control, .form-select, .form-check-input').forEach(input => {
            input.addEventListener('change', () => this.updatePreview());
            if (input.type === 'text' || input.type === 'textarea') {
                input.addEventListener('input', this.debounce(() => this.updatePreview(), 300));
            }
        });
    }

    updatePreview() {
        const generator = this.previewGenerators[this.stepName];
        if (generator) {
            const preview = generator();
            this.renderPreview(preview);
        }
    }

    renderPreview(preview) {
        const outputsContainer = document.querySelector('.preview-outputs');
        const impactContainer = document.querySelector('.preview-impact');

        if (outputsContainer && preview.outputs) {
            outputsContainer.innerHTML = preview.outputs.map(o => `
                <div class="preview-item">
                    <i class="fas fa-check-circle text-success"></i>
                    <span>${o}</span>
                </div>
            `).join('');
        }

        if (impactContainer && preview.impact) {
            impactContainer.innerHTML = preview.impact.map(i => `
                <div class="preview-item ${i.type || ''}">
                    <i class="fas ${i.icon || 'fa-arrow-right'}"></i>
                    <span>${i.text}</span>
                </div>
            `).join('');
        }
    }

    generateStepAPreview() {
        const outputs = ['Tenant Profile'];
        const impact = [];

        const orgName = document.querySelector('[name="OrganizationLegalNameEn"]')?.value;
        const country = document.querySelector('[name="CountryOfIncorporation"]')?.value;
        const orgType = document.querySelector('[name="OrganizationType"]')?.value;
        const sector = document.querySelector('[name="IndustrySector"]')?.value;

        if (orgName) {
            outputs.push(`Workspace: "${orgName}"`);
        }

        if (country) {
            const countryName = document.querySelector(`[name="CountryOfIncorporation"] option[value="${country}"]`)?.textContent;
            impact.push({
                icon: 'fa-globe',
                text: `Jurisdiction: ${countryName}`,
                type: 'info'
            });

            // Suggest frameworks based on country
            if (country === 'SA') {
                impact.push({
                    icon: 'fa-gavel',
                    text: 'Likely frameworks: NCA-ECC, PDPL',
                    type: 'success'
                });
            } else if (country === 'AE') {
                impact.push({
                    icon: 'fa-gavel',
                    text: 'Likely frameworks: NESA, CBUAE',
                    type: 'success'
                });
            }
        }

        if (orgType === 'regulated_fi' || sector === 'Banking') {
            outputs.push('Financial sector controls enabled');
            impact.push({
                icon: 'fa-university',
                text: 'Banking/FI controls will be included',
                type: 'success'
            });
        }

        if (orgType === 'healthcare' || sector === 'Healthcare') {
            outputs.push('Healthcare compliance module');
            impact.push({
                icon: 'fa-hospital',
                text: 'Healthcare controls will be included',
                type: 'success'
            });
        }

        const operatingCountries = document.querySelectorAll('.operating-country:checked');
        if (operatingCountries.length > 1) {
            outputs.push('Multi-jurisdiction support');
            impact.push({
                icon: 'fa-globe-americas',
                text: `${operatingCountries.length} jurisdictions configured`,
                type: 'info'
            });
        }

        return { outputs, impact };
    }

    generateStepBPreview() {
        const outputs = ['Compliance Goals'];
        const impact = [];

        const driver = document.querySelector('[name="PrimaryDriver"]')?.value;
        const maturity = document.querySelector('[name="DesiredMaturityLevel"]')?.value;

        if (driver) {
            outputs.push('Priority Matrix');
            impact.push({
                icon: 'fa-bullseye',
                text: `Focus: ${driver.replace(/_/g, ' ')}`,
                type: 'info'
            });
        }

        if (maturity) {
            outputs.push('Maturity Tracking');
            impact.push({
                icon: 'fa-chart-line',
                text: `Target maturity: ${maturity}`,
                type: 'success'
            });
        }

        return { outputs, impact };
    }

    generateStepCPreview() {
        const outputs = ['Framework Selection'];
        const impact = [];

        const frameworks = document.querySelectorAll('[name="MandatoryFrameworks"]:checked');
        if (frameworks.length > 0) {
            outputs.push(`${frameworks.length} frameworks selected`);
            impact.push({
                icon: 'fa-list-check',
                text: `~${frameworks.length * 50-100} controls estimated`,
                type: 'info'
            });
        }

        return { outputs, impact };
    }

    generateStepDPreview() {
        const outputs = ['Scope Matrix'];
        const impact = [];

        // Count entities, systems, etc.
        const entities = document.querySelectorAll('.entity-row').length;
        const systems = document.querySelectorAll('.system-row').length;

        if (entities > 0) outputs.push(`${entities} entities in scope`);
        if (systems > 0) outputs.push(`${systems} systems in scope`);

        return { outputs, impact };
    }

    generateStepEPreview() {
        const outputs = ['Risk Profile'];
        const impact = [];

        const dataTypes = document.querySelectorAll('[name="DataTypes"]:checked');
        dataTypes.forEach(dt => {
            if (dt.value === 'PII') {
                impact.push({ icon: 'fa-user-shield', text: 'PDPL compliance required', type: 'warning' });
            }
            if (dt.value === 'PaymentCard') {
                impact.push({ icon: 'fa-credit-card', text: 'PCI-DSS compliance required', type: 'warning' });
            }
            if (dt.value === 'Health') {
                impact.push({ icon: 'fa-hospital', text: 'Healthcare compliance required', type: 'warning' });
            }
        });

        return { outputs, impact };
    }

    generateStepFPreview() {
        const outputs = ['Integration Blueprint'];
        const impact = [];

        const idp = document.querySelector('[name="IdentityProvider"]')?.value;
        if (idp && idp !== 'none') {
            outputs.push('SSO Configuration');
            impact.push({
                icon: 'fa-key',
                text: `SSO via ${idp}`,
                type: 'success'
            });
        }

        const siem = document.querySelector('[name="SiemPlatform"]')?.value;
        if (siem) {
            outputs.push('Security Monitoring Integration');
            impact.push({
                icon: 'fa-shield-alt',
                text: 'Automated evidence from SIEM',
                type: 'success'
            });
        }

        return { outputs, impact };
    }

    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
}

/**
 * Next Best Action Panel
 * Shows contextual guidance on what to do next
 */
class NextBestActionPanel {
    constructor(options = {}) {
        this.stepNumber = options.stepNumber || 1;
        this.stepName = options.stepName || 'StepA';

        this.init();
    }

    init() {
        this.createPanel();
        this.updateActions();
    }

    createPanel() {
        const existingPanel = document.getElementById('nba-panel');
        if (existingPanel) return;

        const panel = document.createElement('div');
        panel.id = 'nba-panel';
        panel.className = 'nba-panel';
        panel.innerHTML = `
            <div class="nba-header">
                <i class="fas fa-compass"></i>
                <span>Next Best Actions</span>
            </div>
            <div class="nba-actions"></div>
        `;

        // Add after sidebar
        const sidebar = document.querySelector('.wizard-sidebar');
        if (sidebar) {
            sidebar.appendChild(panel);
        }
    }

    updateActions() {
        const actionsContainer = document.querySelector('.nba-actions');
        if (!actionsContainer) return;

        const actions = this.getContextualActions();

        actionsContainer.innerHTML = actions.map((action, index) => `
            <div class="nba-action" data-action="${index}">
                <div class="nba-action-icon ${action.priority || ''}">
                    <i class="fas ${action.icon}"></i>
                </div>
                <div class="nba-action-content">
                    <div class="nba-action-title">${action.title}</div>
                    <div class="nba-action-time">${action.time}</div>
                </div>
            </div>
        `).join('');
    }

    getContextualActions() {
        const form = document.querySelector('form');
        const requiredFields = form ? form.querySelectorAll('[required]') : [];
        let emptyRequired = 0;

        requiredFields.forEach(field => {
            if (!field.value) emptyRequired++;
        });

        const actions = [];

        if (emptyRequired > 0) {
            actions.push({
                icon: 'fa-edit',
                title: `Complete ${emptyRequired} required fields`,
                time: '~2 min',
                priority: 'high'
            });
        }

        actions.push({
            icon: 'fa-arrow-right',
            title: 'Continue to next step',
            time: '~1 min',
            priority: ''
        });

        actions.push({
            icon: 'fa-save',
            title: 'Save & continue later',
            time: 'Anytime',
            priority: ''
        });

        return actions;
    }
}

/**
 * Why We Ask Tooltips
 * Adds helpful context to form fields
 */
class WhyWeAskTooltips {
    constructor() {
        this.tooltipData = {
            'OrganizationLegalNameEn': 'Your legal name appears on official compliance reports, audit documents, and regulatory submissions.',
            'OrganizationLegalNameAr': 'Arabic name enables bilingual reports required by some GCC regulators.',
            'CountryOfIncorporation': 'Determines which regulatory frameworks apply to your organization.',
            'OrganizationType': 'Helps us recommend the right compliance frameworks and control depth.',
            'IndustrySector': 'Industry-specific regulations and best practices will be suggested.',
            'PrimaryHqLocation': 'Used for timezone defaults and location-based compliance requirements.',
            'DefaultTimezone': 'Ensures deadlines and reminders are set in your local time.',
            'OperatingCountries': 'Multi-jurisdiction operations may require additional compliance frameworks.',
            'PrimaryLanguage': 'Sets the default language for notifications and reports.',
            'HasDataResidencyRequirement': 'Some regulations require data to remain within specific geographic boundaries.',
            'CorporateEmailDomains': 'Used to validate user registrations and enable SSO configurations.'
        };

        this.init();
    }

    init() {
        Object.keys(this.tooltipData).forEach(fieldName => {
            this.addTooltip(fieldName);
        });
    }

    addTooltip(fieldName) {
        const field = document.querySelector(`[name="${fieldName}"]`);
        if (!field) return;

        const label = field.closest('.mb-3')?.querySelector('label') ||
                     field.closest('.col-md-6')?.querySelector('label') ||
                     field.closest('.col-md-4')?.querySelector('label');

        if (!label || label.querySelector('.why-icon')) return;

        const icon = document.createElement('span');
        icon.className = 'why-icon';
        icon.innerHTML = '<i class="fas fa-question-circle"></i>';
        icon.setAttribute('title', this.tooltipData[fieldName]);
        icon.setAttribute('data-bs-toggle', 'tooltip');
        icon.setAttribute('data-bs-placement', 'top');

        label.appendChild(icon);

        // Initialize Bootstrap tooltip if available
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            new bootstrap.Tooltip(icon);
        }
    }
}

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { WizardMotivation, LivePreviewPanel, NextBestActionPanel, WhyWeAskTooltips };
}
