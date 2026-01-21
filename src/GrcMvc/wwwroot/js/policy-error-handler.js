/**
 * Policy Error Handler
 * Handles policy violation errors from API responses and displays user-friendly messages
 */

(function() {
    'use strict';

    const PolicyErrorHandler = {
        /**
         * Initialize error handler - listens for policy violations
         */
        init: function() {
            // Listen for AJAX errors
            $(document).ajaxError(function(event, xhr, settings, thrownError) {
                PolicyErrorHandler.handleError(xhr);
            });

            // Listen for fetch errors
            window.addEventListener('unhandledrejection', function(event) {
                if (event.reason && event.reason.response) {
                    PolicyErrorHandler.handleError(event.reason.response);
                }
            });
        },

        /**
         * Handle error response
         */
        handleError: function(xhr) {
            if (!xhr || !xhr.responseJSON) {
                return;
            }

            const error = xhr.responseJSON;

            // Check if it's a policy violation
            if (error.errorCode === 'Grc:PolicyViolation' || 
                error.error === 'PolicyViolation' ||
                (error.message && error.message.includes('policy violation'))) {
                
                PolicyErrorHandler.showPolicyViolation({
                    message: error.message || 'A policy violation occurred',
                    ruleId: error.ruleId,
                    remediationHint: error.remediationHint,
                    violations: error.violations || []
                });
            }
        },

        /**
         * Show policy violation dialog
         */
        showPolicyViolation: function(options) {
            const message = options.message || 'A policy violation occurred. Please review the requirements.';
            const ruleId = options.ruleId || null;
            const remediationHint = options.remediationHint || null;
            const violations = options.violations || [];

            // Create modal HTML
            const modalHtml = `
                <div class="modal fade" id="policyViolationModal" tabindex="-1" role="dialog">
                    <div class="modal-dialog modal-dialog-centered" role="document">
                        <div class="modal-content">
                            <div class="modal-header bg-danger text-white">
                                <h5 class="modal-title">
                                    <i class="fas fa-exclamation-triangle"></i> Policy Violation
                                </h5>
                                <button type="button" class="close text-white" data-dismiss="modal">
                                    <span>&times;</span>
                                </button>
                            </div>
                            <div class="modal-body">
                                <div class="alert alert-danger">
                                    <h6 class="alert-heading">${this.escapeHtml(message)}</h6>
                                    ${ruleId ? `<p class="mb-0"><strong>Rule ID:</strong> ${this.escapeHtml(ruleId)}</p>` : ''}
                                </div>
                                ${remediationHint ? `
                                    <div class="alert alert-info">
                                        <h6 class="alert-heading">Remediation Steps</h6>
                                        <p class="mb-0">${this.escapeHtml(remediationHint)}</p>
                                    </div>
                                ` : ''}
                                ${violations.length > 0 ? `
                                    <div class="mt-3">
                                        <h6>Violations:</h6>
                                        <ul class="list-group">
                                            ${violations.map(v => `<li class="list-group-item">${this.escapeHtml(v)}</li>`).join('')}
                                        </ul>
                                    </div>
                                ` : ''}
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            // Remove existing modal if any
            $('#policyViolationModal').remove();

            // Add modal to body
            $('body').append(modalHtml);

            // Show modal
            $('#policyViolationModal').modal('show');

            // Remove modal on close
            $('#policyViolationModal').on('hidden.bs.modal', function() {
                $(this).remove();
            });
        },

        /**
         * Escape HTML to prevent XSS
         */
        escapeHtml: function(text) {
            const map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return text ? text.replace(/[&<>"']/g, m => map[m]) : '';
        },

        /**
         * Handle form submission errors
         */
        handleFormError: function(formElement, error) {
            if (error.errorCode === 'Grc:PolicyViolation') {
                // Clear existing errors
                formElement.find('.text-danger').remove();
                formElement.find('.is-invalid').removeClass('is-invalid');

                // Add error message at top of form
                const errorHtml = `
                    <div class="alert alert-danger alert-dismissible fade show" role="alert">
                        <strong>Policy Violation:</strong> ${this.escapeHtml(error.message || 'A policy violation occurred')}
                        ${error.remediationHint ? `<br><small>${this.escapeHtml(error.remediationHint)}</small>` : ''}
                        <button type="button" class="close" data-dismiss="alert">
                            <span>&times;</span>
                        </button>
                    </div>
                `;
                formElement.prepend(errorHtml);

                // Scroll to top
                $('html, body').animate({ scrollTop: 0 }, 300);
            }
        }
    };

    // Initialize on document ready
    $(document).ready(function() {
        PolicyErrorHandler.init();
    });

    // Export for global use
    window.PolicyErrorHandler = PolicyErrorHandler;
})();
