/**
 * Help System JavaScript
 * Provides tooltip initialization, glossary functionality, and help system features
 */

const HelpSystem = {
    /**
     * Initialize all help system features
     */
    init: function() {
        this.initTooltips();
        this.initGlossary();
        this.initContextualHelp();
    },

    /**
     * Initialize Bootstrap tooltips
     */
    initTooltips: function() {
        // Initialize all tooltips on the page
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    },

    /**
     * Initialize glossary term links
     */
    initGlossary: function() {
        // Handle glossary term clicks
        document.querySelectorAll('.glossary-term').forEach(link => {
            link.addEventListener('click', function(e) {
                e.preventDefault();
                const term = this.dataset.term;
                if (term) {
                    HelpSystem.showGlossaryTerm(term);
                }
            });
        });
    },

    /**
     * Show glossary term in modal
     */
    showGlossaryTerm: async function(term) {
        try {
            const response = await fetch(`/Help/GetGlossaryTerm?term=${encodeURIComponent(term)}`);
            const data = await response.json();

            if (data.error) {
                alert('Term not found: ' + term);
                return;
            }

            // Show modal with term definition
            const modal = document.getElementById('glossaryModal');
            if (modal) {
                document.getElementById('glossaryTermTitle').textContent = data.term;
                document.getElementById('glossaryTermDefinition').textContent = data.definition;
                document.getElementById('glossaryTermCategory').textContent = data.category || '';
                
                const modalInstance = new bootstrap.Modal(modal);
                modalInstance.show();
            } else {
                // Fallback: show in alert
                alert(`${data.term}: ${data.definition}`);
            }
        } catch (error) {
            console.error('Error loading glossary term:', error);
            alert('Error loading term definition');
        }
    },

    /**
     * Initialize contextual help
     */
    initContextualHelp: function() {
        // Add help icons to elements with data-help attribute
        document.querySelectorAll('[data-help]').forEach(element => {
            const helpText = element.dataset.help;
            if (helpText && !element.querySelector('.help-icon')) {
                const icon = document.createElement('i');
                icon.className = 'bi bi-question-circle text-info ms-1 help-icon';
                icon.setAttribute('data-bs-toggle', 'tooltip');
                icon.setAttribute('data-bs-placement', 'top');
                icon.setAttribute('title', helpText);
                element.appendChild(icon);
                
                // Initialize tooltip for new icon
                new bootstrap.Tooltip(icon);
            }
        });
    },

    /**
     * Show empty state help
     */
    showEmptyStateHelp: function(message, actionText, actionUrl) {
        // This can be called from pages with no data
    }
};

// Auto-initialize on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        HelpSystem.init();
    });
} else {
    HelpSystem.init();
}
