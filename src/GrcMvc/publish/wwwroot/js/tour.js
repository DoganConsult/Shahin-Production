/**
 * Welcome Tour JavaScript
 * Provides interactive step-by-step tour for first-time users
 */

const Tour = {
    currentStep: 0,
    steps: [],
    isActive: false,

    /**
     * Start welcome tour
     */
    startWelcomeTour: function() {
        if (this.isActive) return;

        this.steps = this.getTourSteps();
        if (this.steps.length === 0) return;

        this.isActive = true;
        this.currentStep = 0;
        this.showStep(0);
    },

    /**
     * Get tour steps based on current page
     */
    getTourSteps: function() {
        const path = window.location.pathname.toLowerCase();
        const steps = [];

        if (path === '/' || path.includes('dashboard')) {
            steps.push(
                {
                    element: '.navbar',
                    title: 'Navigation Menu',
                    content: 'Use the navigation menu to access different sections of the GRC system.',
                    position: 'bottom'
                },
                {
                    element: '.help-menu',
                    title: 'Help & Support',
                    content: 'Click here anytime for help, FAQ, glossary, or to contact support.',
                    position: 'bottom'
                },
                {
                    element: '#supportChatButton',
                    title: 'Live Chat',
                    content: 'Use the chat button for instant support from our team.',
                    position: 'top'
                }
            );
        } else if (path.includes('onboarding')) {
            steps.push(
                {
                    element: '.progress-indicator',
                    title: 'Onboarding Progress',
                    content: 'Track your progress through the 12-step onboarding wizard.',
                    position: 'bottom'
                },
                {
                    element: '.help-hint',
                    title: 'Need Help?',
                    content: 'Look for (?) icons for tooltips and glossary links on GRC terms.',
                    position: 'top'
                }
            );
        }

        return steps;
    },

    /**
     * Show tour step
     */
    showStep: function(stepIndex) {
        if (stepIndex >= this.steps.length) {
            this.endTour();
            return;
        }

        const step = this.steps[stepIndex];
        const element = document.querySelector(step.element);

        if (!element) {
            // Skip if element not found
            this.showStep(stepIndex + 1);
            return;
        }

        // Highlight element
        this.highlightElement(element);

        // Show tooltip/overlay
        this.showTooltip(element, step, stepIndex);
    },

    /**
     * Highlight element
     */
    highlightElement: function(element) {
        // Add highlight class
        element.classList.add('tour-highlight');
        
        // Scroll into view
        element.scrollIntoView({ behavior: 'smooth', block: 'center' });
    },

    /**
     * Show tooltip for step
     */
    showTooltip: function(element, step, stepIndex) {
        // Remove existing tooltip
        const existing = document.getElementById('tourTooltip');
        if (existing) existing.remove();

        // Create tooltip
        const tooltip = document.createElement('div');
        tooltip.id = 'tourTooltip';
        tooltip.className = 'tour-tooltip';
        tooltip.innerHTML = `
            <div class="tour-tooltip-header">
                <h5>${step.title}</h5>
                <button class="btn-close" onclick="Tour.endTour()"></button>
            </div>
            <div class="tour-tooltip-body">
                <p>${step.content}</p>
                <div class="tour-tooltip-footer">
                    <span>Step ${stepIndex + 1} of ${this.steps.length}</span>
                    <div>
                        ${stepIndex > 0 ? '<button class="btn btn-sm btn-outline-secondary" onclick="Tour.previousStep()">Previous</button>' : ''}
                        <button class="btn btn-sm btn-primary ms-2" onclick="Tour.nextStep()">
                            ${stepIndex === this.steps.length - 1 ? 'Finish' : 'Next'}
                        </button>
                    </div>
                </div>
            </div>
        `;

        document.body.appendChild(tooltip);

        // Position tooltip
        this.positionTooltip(tooltip, element, step.position);
    },

    /**
     * Position tooltip relative to element
     */
    positionTooltip: function(tooltip, element, position) {
        const rect = element.getBoundingClientRect();
        const tooltipRect = tooltip.getBoundingClientRect();

        let top = 0;
        let left = 0;

        switch (position) {
            case 'top':
                top = rect.top - tooltipRect.height - 10;
                left = rect.left + (rect.width / 2) - (tooltipRect.width / 2);
                break;
            case 'bottom':
                top = rect.bottom + 10;
                left = rect.left + (rect.width / 2) - (tooltipRect.width / 2);
                break;
            case 'left':
                top = rect.top + (rect.height / 2) - (tooltipRect.height / 2);
                left = rect.left - tooltipRect.width - 10;
                break;
            case 'right':
                top = rect.top + (rect.height / 2) - (tooltipRect.height / 2);
                left = rect.right + 10;
                break;
        }

        tooltip.style.top = top + 'px';
        tooltip.style.left = left + 'px';
    },

    /**
     * Next step
     */
    nextStep: function() {
        // Remove highlight from current element
        const currentElement = document.querySelector('.tour-highlight');
        if (currentElement) {
            currentElement.classList.remove('tour-highlight');
        }

        this.currentStep++;
        this.showStep(this.currentStep);
    },

    /**
     * Previous step
     */
    previousStep: function() {
        // Remove highlight
        const currentElement = document.querySelector('.tour-highlight');
        if (currentElement) {
            currentElement.classList.remove('tour-highlight');
        }

        this.currentStep--;
        if (this.currentStep < 0) this.currentStep = 0;
        this.showStep(this.currentStep);
    },

    /**
     * End tour
     */
    endTour: function() {
        // Remove highlight
        document.querySelectorAll('.tour-highlight').forEach(el => {
            el.classList.remove('tour-highlight');
        });

        // Remove tooltip
        const tooltip = document.getElementById('tourTooltip');
        if (tooltip) tooltip.remove();

        // Mark tour as completed
        this.markTourCompleted();

        this.isActive = false;
        this.currentStep = 0;
    },

    /**
     * Mark tour as completed
     */
    markTourCompleted: function() {
        // Save to localStorage or send to server
        localStorage.setItem('grcTourCompleted', 'true');
        
        // Optionally send to server
        fetch('/api/user/preferences', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ tourCompleted: true })
        }).catch(err => console.log('Could not save tour preference'));
    },

    /**
     * Check if tour should be shown
     */
    shouldShowTour: function() {
        // Check localStorage
        if (localStorage.getItem('grcTourCompleted') === 'true') {
            return false;
        }

        // Check server preference if available
        return true;
    }
};

// Auto-start if ViewBag.ShowWelcomeTour is true
if (typeof window.showWelcomeTour !== 'undefined' && window.showWelcomeTour === true) {
    if (Tour.shouldShowTour()) {
        Tour.startWelcomeTour();
    }
}
