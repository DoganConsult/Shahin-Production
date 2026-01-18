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

        // Detect reduce-motion preference for accessibility
        this.reduceMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches ?? false;
        this.prefersReducedMotion = this.reduceMotion; // alias for backward compatibility

        // Configuration (enterprise-safe defaults + allow overrides from options.config)
        const defaults = {
            mode: 'enterprise',
            enableCelebrations: true,      // toast, not fullscreen overlay
            celebrationStyle: 'toast',
            enableParticles: false,        // off in GRC contexts
            enableTips: true,
            enableProgress: true,
            enablePreview: true,
            entryAnimation: !this.reduceMotion,
            tipDelay: 2500,
            celebrationDuration: 2200,
            // Motion spec (predictable, short)
            transitionMs: 180,
            enterStaggerMs: 80
        };

        this.config = { ...defaults, ...(options.config || {}) };

        // Override motion settings when reduce-motion is preferred
        if (this.reduceMotion) {
            this.config.enableParticles = false;
            this.config.transitionMs = 0;
            this.config.enterStaggerMs = 0;
            this.config.celebrationDuration = 1800;
        }

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

        // Only show entry animation if enabled
        if (this.config.entryAnimation) {
            this.showEntryAnimation();
        }

        // Delayed tip display
        if (this.config.enableTips) {
            setTimeout(() => this.showSmartTip(), this.config.tipDelay);
        }
    }

    createMotivationElements() {
        // Celebration overlay (enterprise toast, not fullscreen)
        if (!document.getElementById('celebration-overlay')) {
            const overlay = document.createElement('div');
            overlay.id = 'celebration-overlay';
            overlay.className = 'celebration-overlay';
            overlay.innerHTML = `
                <div class="celebration-toast" role="status" aria-live="polite">
                    <div class="celebration-icon" aria-hidden="true">
                        <i class="fas fa-check-circle"></i>
                    </div>
                    <div class="celebration-body">
                        <div class="celebration-title">Configuration updated</div>
                        <div class="celebration-message"></div>
                        <div class="celebration-unlocks"></div>
                    </div>
                    <button type="button" class="celebration-close" aria-label="Dismiss">
                        <i class="fas fa-times" aria-hidden="true"></i>
                    </button>
                </div>
            `;
            document.body.appendChild(overlay);

            overlay.querySelector('.celebration-close').addEventListener('click', () => {
                overlay.classList.remove('show');
            });
        }

        // Smart tip container (keep, but enterprise styling handled in CSS)
        if (!document.getElementById('smart-tip-container')) {
            const tipContainer = document.createElement('div');
            tipContainer.id = 'smart-tip-container';
            tipContainer.className = 'smart-tip-container';
            tipContainer.innerHTML = `
                <div class="smart-tip" role="note" aria-live="polite">
                    <div class="smart-tip-header">
                        <i class="fas fa-lightbulb"></i>
                        <span>Tip</span>
                        <button class="smart-tip-close" aria-label="Dismiss tip"><i class="fas fa-times"></i></button>
                    </div>
                    <div class="smart-tip-content"></div>
                </div>
            `;
            document.body.appendChild(tipContainer);

            tipContainer.querySelector('.smart-tip-close').addEventListener('click', () => {
                this.hideTip();
            });
        }
    }

    attachEventListeners() {
        // Track form field completion for micro-celebrations
        // Only show indicator when: field is required OR changed from empty to non-empty
        document.querySelectorAll('.form-control, .form-select').forEach(field => {
            // Store initial value to detect meaningful changes
            field._wzPrevValue = field.value || '';

            field.addEventListener('focus', (e) => {
                // Capture value on focus to detect changes
                e.target._wzPrevValue = e.target.value || '';
            });

            field.addEventListener('blur', (e) => {
                const target = e.target;
                const prevValue = target._wzPrevValue || '';
                const currentValue = target.value || '';
                const isRequired = target.hasAttribute('required');

                // Only celebrate if:
                // 1. Field is required and now has a value, OR
                // 2. Field changed from empty to non-empty (first-time fill)
                const isFirstFill = !prevValue && currentValue;
                const isRequiredFilled = isRequired && currentValue;

                if (isFirstFill || isRequiredFilled) {
                    this.showFieldComplete(target);
                }

                // Update prev value for next check
                target._wzPrevValue = currentValue;
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
        // Respect user's reduce-motion preference
        if (this.reduceMotion) return;

        const cards = document.querySelectorAll('.card');
        cards.forEach((card, index) => {
            // Start hidden and slightly below
            card.style.opacity = '0';
            card.style.transform = 'translateY(12px)';

            // Staggered reveal with configurable timing
            const delay = 60 + (index * this.config.enterStaggerMs);
            setTimeout(() => {
                card.style.transition =
                    `opacity ${this.config.transitionMs}ms ease-out, transform ${this.config.transitionMs}ms ease-out`;
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, delay);
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

        // Calm highlight instead of pulse (enterprise-safe)
        card.classList.add('card-complete-highlight');
        setTimeout(() => card.classList.remove('card-complete-highlight'), 900);
    }

    showStepCompleteCelebration() {
        if (!this.config.enableCelebrations) return;

        const metadata = this.stepMetadata[this.stepName] || {};
        const overlay = document.getElementById('celebration-overlay');
        if (!overlay) return;

        // Update toast content
        overlay.querySelector('.celebration-title').textContent = 'Configuration updated';
        overlay.querySelector('.celebration-message').textContent =
            `Step ${this.stepNumber} of ${this.totalSteps} saved successfully.`;

        // Show unlocked features (up to 3)
        const unlocksContainer = overlay.querySelector('.celebration-unlocks');
        if (metadata.unlocks?.length) {
            unlocksContainer.innerHTML = `
                <div class="unlock-title">Enabled:</div>
                <div class="unlock-items">
                    ${metadata.unlocks.slice(0, 3).map(u => `
                        <span class="unlock-item">
                            <i class="fas fa-check" aria-hidden="true"></i> ${u}
                        </span>
                    `).join('')}
                </div>
            `;
        } else {
            unlocksContainer.innerHTML = '';
        }

        overlay.classList.add('show');
        setTimeout(() => overlay.classList.remove('show'), this.config.celebrationDuration);
    }

    showParticles() {
        // Enterprise UX: no confetti/particles in regulated GRC tools
        // Guard against both config and reduce-motion preference
        if (!this.config.enableParticles || this.reduceMotion) return;
        // Intentionally a no-op for professional compliance software
    }

    showSmartTip() {
        const metadata = this.stepMetadata[this.stepName];
        if (!metadata || !metadata.tips || metadata.tips.length === 0) return;

        // Show tip only once per step per tenant (prevents nagging)
        const key = `wz_tip_shown_${this.tenantId}_${this.stepName}`;
        if (localStorage.getItem(key) === '1') return;

        const container = document.getElementById('smart-tip-container');
        if (!container) return;

        // Get random tip
        const tip = metadata.tips[Math.floor(Math.random() * metadata.tips.length)];

        container.querySelector('.smart-tip-content').textContent = tip;
        container.classList.add('show');

        // Mark as shown
        localStorage.setItem(key, '1');

        // Auto-hide after 9 seconds
        setTimeout(() => this.hideTip(), 9000);
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
            StepF: this.generateStepFPreview.bind(this),
            StepG: this.generateStepGPreview.bind(this),
            StepH: this.generateStepHPreview.bind(this),
            StepI: this.generateStepIPreview.bind(this),
            StepJ: this.generateStepJPreview.bind(this),
            StepK: this.generateStepKPreview.bind(this),
            StepL: this.generateStepLPreview.bind(this)
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
        panel.className = 'live-preview-panel is-entering';
        panel.setAttribute('role', 'complementary');
        panel.setAttribute('aria-label', 'Live preview panel');
        panel.innerHTML = `
            <div class="preview-header">
                <i class="fas fa-eye" aria-hidden="true"></i>
                <span>Live Preview</span>
                <button class="preview-toggle" type="button" title="Collapse" aria-label="Collapse Live Preview" aria-expanded="true">
                    <i class="fas fa-chevron-right" aria-hidden="true"></i>
                </button>
            </div>
            <div class="preview-body">
                <div class="preview-section">
                    <h6><i class="fas fa-cog" aria-hidden="true"></i> What will be generated:</h6>
                    <div class="preview-outputs"></div>
                </div>
                <div class="preview-section">
                    <h6><i class="fas fa-chart-line" aria-hidden="true"></i> Impact:</h6>
                    <div class="preview-impact"></div>
                </div>
            </div>
        `;

        // Attach to body to avoid parent overflow/stacking issues
        document.body.appendChild(panel);

        // One-time enter: keep it subtle and skip for reduce motion
        const reduceMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches;
        if (!reduceMotion) {
            requestAnimationFrame(() => {
                panel.classList.add('is-entered');
            });
            // Clean up class after transition ends
            panel.addEventListener('transitionend', () => {
                panel.classList.remove('is-entering', 'is-entered');
            }, { once: true });
        } else {
            panel.classList.remove('is-entering');
        }

        // Toggle functionality
        const toggleBtn = panel.querySelector('.preview-toggle');
        toggleBtn.addEventListener('click', () => {
            const isCollapsed = panel.classList.toggle('collapsed');

            const icon = panel.querySelector('.preview-toggle i');
            icon.classList.toggle('fa-chevron-right', !isCollapsed);
            icon.classList.toggle('fa-chevron-left', isCollapsed);

            toggleBtn.setAttribute('aria-expanded', String(!isCollapsed));
            toggleBtn.setAttribute('aria-label', isCollapsed ? 'Expand Live Preview' : 'Collapse Live Preview');
            toggleBtn.title = isCollapsed ? 'Expand' : 'Collapse';
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

        const reduceMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches;

        if (outputsContainer && preview.outputs) {
            if (!reduceMotion) outputsContainer.classList.add('is-updating');
            outputsContainer.innerHTML = preview.outputs.map(o => `
                <div class="preview-item">
                    <i class="fas fa-check-circle text-success" aria-hidden="true"></i>
                    <span>${o}</span>
                </div>
            `).join('');
            if (!reduceMotion) setTimeout(() => outputsContainer.classList.remove('is-updating'), 120);
        }

        if (impactContainer && preview.impact) {
            if (!reduceMotion) impactContainer.classList.add('is-updating');
            impactContainer.innerHTML = preview.impact.map(i => `
                <div class="preview-item ${i.type || ''}">
                    <i class="fas ${i.icon || 'fa-arrow-right'}" aria-hidden="true"></i>
                    <span>${i.text}</span>
                </div>
            `).join('');
            if (!reduceMotion) setTimeout(() => impactContainer.classList.remove('is-updating'), 120);
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
            // Fixed: proper range calculation (was math bug: 50-100 evaluated incorrectly)
            const low = frameworks.length * 50;
            const high = frameworks.length * 100;
            impact.push({
                icon: 'fa-list-check',
                text: `Estimated control set: ${low}–${high} controls`,
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

    generateStepGPreview() {
        const outputs = ['Team Structure'];
        const impact = [];

        const departments = document.querySelectorAll('[name="Departments"]:checked');
        if (departments.length > 0) {
            outputs.push(`${departments.length} departments configured`);
            impact.push({
                icon: 'fa-sitemap',
                text: `RACI matrix for ${departments.length} teams`,
                type: 'info'
            });
        }

        const riskOwner = document.querySelector('[name="RiskOwnerName"]')?.value;
        if (riskOwner) {
            impact.push({
                icon: 'fa-user-tie',
                text: `Risk Owner: ${riskOwner}`,
                type: 'success'
            });
        }

        return { outputs, impact };
    }

    generateStepHPreview() {
        const outputs = ['Access Controls'];
        const impact = [];

        const users = document.querySelectorAll('.user-row').length;
        if (users > 0) {
            outputs.push(`${users} users to onboard`);
        }

        const roles = document.querySelectorAll('[name="UserRoles"]:checked');
        if (roles.length > 0) {
            impact.push({
                icon: 'fa-user-shield',
                text: `${roles.length} role types configured`,
                type: 'info'
            });
        }

        const mfaEnabled = document.querySelector('[name="RequireMFA"]:checked');
        if (mfaEnabled) {
            impact.push({
                icon: 'fa-lock',
                text: 'MFA enforcement enabled',
                type: 'success'
            });
        }

        return { outputs, impact };
    }

    generateStepIPreview() {
        const outputs = ['Notification Settings'];
        const impact = [];

        const channels = document.querySelectorAll('[name="NotificationChannels"]:checked');
        if (channels.length > 0) {
            const channelNames = Array.from(channels).map(c => c.value).join(', ');
            outputs.push(`Channels: ${channelNames}`);
            impact.push({
                icon: 'fa-bell',
                text: `${channels.length} notification channels active`,
                type: 'info'
            });
        }

        const escalation = document.querySelector('[name="EscalationPolicy"]')?.value;
        if (escalation) {
            impact.push({
                icon: 'fa-arrow-up',
                text: 'Escalation policy configured',
                type: 'success'
            });
        }

        return { outputs, impact };
    }

    generateStepJPreview() {
        const outputs = ['Scheduling Configuration'];
        const impact = [];

        const frequency = document.querySelector('[name="AssessmentFrequency"]')?.value;
        if (frequency) {
            outputs.push(`Assessment cycle: ${frequency}`);
            impact.push({
                icon: 'fa-calendar-check',
                text: `${frequency} assessment schedule`,
                type: 'info'
            });
        }

        const reviewCycle = document.querySelector('[name="ControlReviewCycle"]')?.value;
        if (reviewCycle) {
            impact.push({
                icon: 'fa-sync',
                text: `Control reviews: ${reviewCycle}`,
                type: 'info'
            });
        }

        return { outputs, impact };
    }

    generateStepKPreview() {
        const outputs = ['Advanced Settings'];
        const impact = [];

        const autoEvidence = document.querySelector('[name="AutoEvidenceCollection"]:checked');
        if (autoEvidence) {
            outputs.push('Automated evidence collection');
            impact.push({
                icon: 'fa-robot',
                text: 'Evidence automation enabled',
                type: 'success'
            });
        }

        const aiAssist = document.querySelector('[name="AIAssistEnabled"]:checked');
        if (aiAssist) {
            impact.push({
                icon: 'fa-brain',
                text: 'AI-assisted compliance enabled',
                type: 'success'
            });
        }

        return { outputs, impact };
    }

    generateStepLPreview() {
        const outputs = ['Success Dashboard', 'KPI Tracker', 'Improvement Goals'];
        const impact = [];

        // Q91: top 3 success metrics
        const metrics = Array.from(document.querySelectorAll('.success-metric:checked'))
            .map(x => x.value);

        if (metrics.length) {
            const metricLabels = metrics.map(m => m.replace(/_/g, ' ')).join(', ');
            impact.push({
                icon: 'fa-trophy',
                text: `Tracking: ${metricLabels}`,
                type: 'info'
            });
        } else {
            impact.push({
                icon: 'fa-circle-exclamation',
                text: 'Select 3 success metrics to enable KPI tracking.',
                type: 'warning'
            });
        }

        // Q92-94 baseline values
        const auditHrs = document.querySelector('[name="BaselineAuditPrepHoursPerMonth"]')?.value;
        const closureDays = document.querySelector('[name="BaselineRemediationClosureDays"]')?.value;
        const overdueControls = document.querySelector('[name="BaselineOverdueControlsPerMonth"]')?.value;

        if (auditHrs) {
            impact.push({ icon: 'fa-clock', text: `Baseline audit prep: ${auditHrs} hrs/month`, type: 'success' });
        }
        if (closureDays) {
            impact.push({ icon: 'fa-calendar', text: `Baseline remediation closure: ${closureDays} days`, type: 'success' });
        }
        if (overdueControls) {
            impact.push({ icon: 'fa-list-check', text: `Baseline overdue controls: ${overdueControls}/month`, type: 'success' });
        }

        // Q96 pilot scope
        const pilot = Array.from(document.querySelectorAll('[name="PilotScope"]:checked'))
            .map(x => x.value);

        if (pilot.length) {
            outputs.push(`Pilot scope: ${pilot.length} domains`);
            const pilotSummary = pilot.slice(0, 3).join(', ') + (pilot.length > 3 ? '…' : '');
            impact.push({
                icon: 'fa-flask',
                text: `Pilot domains: ${pilotSummary}`,
                type: 'info'
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

        // Standard required fields check
        if (emptyRequired > 0) {
            actions.push({
                icon: 'fa-edit',
                title: `Complete ${emptyRequired} required fields`,
                time: '~2 min',
                priority: 'high'
            });
        }

        // Step-specific contextual actions
        if (this.stepName === 'StepL') {
            // Q91: Success metrics (need exactly 3)
            const metricsChecked = document.querySelectorAll('.success-metric:checked').length;
            if (metricsChecked !== 3) {
                actions.push({
                    icon: 'fa-trophy',
                    title: `Select 3 success metrics (${metricsChecked}/3 selected)`,
                    time: '~1 min',
                    priority: 'high'
                });
            }

            // Q92-94: Baseline values (recommended but not required)
            const baselineAny =
                !!document.querySelector('[name="BaselineAuditPrepHoursPerMonth"]')?.value ||
                !!document.querySelector('[name="BaselineRemediationClosureDays"]')?.value ||
                !!document.querySelector('[name="BaselineOverdueControlsPerMonth"]')?.value;

            if (!baselineAny) {
                actions.push({
                    icon: 'fa-chart-line',
                    title: 'Add baseline values to measure 90-day impact',
                    time: '~1 min',
                    priority: ''
                });
            }

            // Q96: Pilot scope (recommended)
            const pilotSelected = document.querySelectorAll('[name="PilotScope"]:checked').length;
            if (pilotSelected === 0) {
                actions.push({
                    icon: 'fa-flask',
                    title: 'Select pilot domains for initial implementation',
                    time: '~1 min',
                    priority: ''
                });
            }

            // Final step: show complete action
            if (metricsChecked === 3) {
                actions.push({
                    icon: 'fa-check-double',
                    title: 'Complete onboarding & go to dashboard',
                    time: '~1 min',
                    priority: 'high'
                });
            }
        } else {
            // Default actions for other steps
            actions.push({
                icon: 'fa-arrow-right',
                title: 'Continue to next step',
                time: '~1 min',
                priority: ''
            });
        }

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
            // StepA - Organization Identity
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
            'CorporateEmailDomains': 'Used to validate user registrations and enable SSO configurations.',

            // StepB - Assurance Objective
            'PrimaryDriver': 'Determines how we prioritize your compliance activities and dashboard focus.',
            'CompliancePainPoints': 'Helps us customize automation and recommendations for your specific challenges.',
            'DesiredMaturityLevel': 'Sets the target for tracking improvement over time.',
            'CurrentMaturityLevel': 'Establishes baseline to measure your compliance growth journey.',

            // StepC - Regulatory Applicability
            'PrimaryRegulator': 'Your primary regulator determines mandatory frameworks and reporting requirements.',
            'MandatoryFrameworks': 'Frameworks required by regulators form the core of your control baseline.',
            'ExistingCertifications': 'Existing certifications can reduce duplicate compliance efforts through control inheritance.',
            'BenchmarkingFrameworks': 'Voluntary frameworks help align with international best practices.',

            // StepD - Scope Definition
            'InScopeEntities': 'Entities within scope define the organizational boundary for compliance assessments.',
            'InScopeSystems': 'Systems in scope determine which technical controls apply and evidence sources needed.',
            'ScopeExclusions': 'Documenting exclusions provides audit trail for regulatory justification.',
            'SystemCriticality': 'Criticality tiers inform RTO/RPO requirements and assessment prioritization.',

            // StepE - Data & Risk Profile
            'DataTypes': 'Data classifications trigger specific privacy and security framework requirements.',
            'ProcessesPaymentCards': 'Payment card processing mandates PCI-DSS compliance controls.',
            'ProcessesHealthData': 'Health data triggers HIPAA or local health privacy regulations.',
            'ThirdPartyProcessors': 'Third-party processors are key inputs for vendor risk management.',
            'DataResidencyRequirements': 'Data residency affects cloud provider selection and storage controls.',

            // StepF - Technology Landscape
            'IdentityProvider': 'Connecting your identity provider enables SSO and user provisioning automation.',
            'SiemPlatform': 'SIEM integration enables automated evidence collection for monitoring controls.',
            'CloudProviders': 'Cloud providers inform cloud-specific security controls and shared responsibility model.',
            'TicketingSystem': 'Ticketing integration automates remediation tracking and evidence collection.',
            'CodeRepository': 'Code repository integration enables secure development lifecycle evidence.',

            // StepG - Control Ownership
            'GovernanceModel': 'Governance model determines how controls are assigned and managed across teams.',
            'ControlOwnershipApproach': 'Centralized vs federated ownership affects workflow and escalation paths.',
            'ExceptionApprovers': 'Exception approvers ensure risk acceptance is properly authorized and documented.',
            'EscalationPath': 'Clear escalation paths prevent controls from stalling in remediation.',

            // StepH - Teams & Access
            'Departments': 'Departments define the organizational structure for RACI mapping.',
            'TeamMembers': 'Team members receive role-based access to relevant controls and evidence tasks.',
            'UserRoles': 'Role types determine permissions and dashboard views for each user.',
            'RequireMFA': 'MFA enforcement ensures compliance portal access meets security standards.',

            // StepI - Workflow & Cadence
            'NotificationChannels': 'Notification channels determine how users receive alerts and reminders.',
            'EscalationPolicy': 'Escalation policy ensures overdue items get appropriate management attention.',
            'ReminderFrequency': 'Reminder frequency balances user engagement with notification fatigue.',
            'WorkflowApprovals': 'Workflow approvals ensure evidence and exceptions are properly reviewed.',

            // StepJ - Evidence Standards
            'AssessmentFrequency': 'Assessment frequency determines your compliance calendar and workload distribution.',
            'ControlReviewCycle': 'Control review cycles ensure your security posture stays current.',
            'EvidenceNamingConvention': 'Naming conventions ensure consistent evidence organization for auditors.',
            'EvidenceRetentionPeriod': 'Retention periods must meet regulatory requirements for audit evidence.',
            'EvidenceAccessRules': 'Access rules protect sensitive evidence while enabling necessary reviews.',

            // StepK - Baseline & Overlays
            'ControlBaseline': 'The control baseline forms the foundation of your compliance program.',
            'ControlOverlays': 'Overlays add industry or jurisdiction-specific controls to your baseline.',
            'CustomControls': 'Custom controls capture internal policy requirements not covered by frameworks.',
            'AutoEvidenceCollection': 'Automated evidence reduces manual effort and improves collection consistency.',
            'AIAssistEnabled': 'AI assistance helps with control mapping, gap analysis, and remediation guidance.',

            // StepL - Go-Live & Success Metrics (Q91-Q96)
            'SuccessMetricsTop3': 'Defines executive KPIs used to demonstrate measurable program outcomes after go-live.',
            'BaselineAuditPrepHoursPerMonth': 'Creates a baseline to quantify time savings and audit readiness improvements.',
            'BaselineRemediationClosureDays': 'Enables tracking remediation SLA improvement over 90 days.',
            'BaselineOverdueControlsPerMonth': 'Measures control operational maturity and backlog reduction.',
            'PilotScope': 'Determines which control domains are prioritized in the first implementation wave.'
        };

        this.init();
    }

    init() {
        // Add tooltips by field name
        Object.keys(this.tooltipData).forEach(fieldName => {
            this.addTooltip(fieldName);
        });

        // Support tooltips on arbitrary elements via data-why attribute
        // Useful for section headers, checkbox groups, etc.
        document.querySelectorAll('[data-why]').forEach(el => {
            const key = el.getAttribute('data-why');
            const text = this.tooltipData[key];
            if (!text || el.querySelector('.why-icon')) return;

            const icon = document.createElement('span');
            icon.className = 'why-icon';
            icon.innerHTML = '<i class="fas fa-question-circle"></i>';
            icon.setAttribute('title', text);
            icon.setAttribute('data-bs-toggle', 'tooltip');
            icon.setAttribute('data-bs-placement', 'top');
            el.appendChild(icon);

            if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
                new bootstrap.Tooltip(icon);
            }
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
