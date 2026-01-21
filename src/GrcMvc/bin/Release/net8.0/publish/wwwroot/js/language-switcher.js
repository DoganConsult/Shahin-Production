// Language Switcher Helper
// Note: Server-side CookieRequestCultureProvider handles cookie setting via URL parameters
// This script primarily reads the current language and applies RTL/LTR direction
(function() {
    'use strict';

    // Get current language from cookie or URL parameter or default to Arabic
    function getCurrentLanguage() {
        // First, check URL parameters (most recent)
        const urlParams = new URLSearchParams(window.location.search);
        const urlCulture = urlParams.get('culture') || urlParams.get('ui-culture');
        if (urlCulture) {
            return urlCulture;
        }

        // Then check cookie (persisted preference)
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            if (name === 'GrcMvc.Culture') {
                try {
                    // Cookie format: c=ar|uic=ar
                    const match = value.match(/c=([^|]+)/);
                    if (match && match[1]) {
                        return match[1];
                    }
                } catch (e) {
                    console.warn('Error parsing culture cookie:', e);
                }
            }
        }

        // Check HTML lang attribute (set by server)
        const htmlLang = document.documentElement.getAttribute('lang');
        if (htmlLang) {
            return htmlLang;
        }

        // Default to Arabic
        return 'ar';
    }

    // Apply RTL/LTR direction based on language
    function applyDirection(culture) {
        if (!culture) {
            culture = 'ar'; // Default to Arabic
        }

        const isRtl = culture === 'ar';
        const dir = isRtl ? 'rtl' : 'ltr';
        
        // Update HTML attributes
        document.documentElement.setAttribute('dir', dir);
        document.documentElement.setAttribute('lang', culture);
        
        // Update Bootstrap CSS dynamically (only if not already set by server)
        // Server-side _Layout.cshtml already handles this, but this ensures it works
        // even if server-side detection fails
        const bootstrapLinks = document.querySelectorAll('link[href*="bootstrap"]');
        bootstrapLinks.forEach(function(link) {
            const href = link.getAttribute('href');
            if (href) {
                if (isRtl && href.includes('bootstrap.min.css') && !href.includes('rtl')) {
                    // Switch to RTL version
                    link.setAttribute('href', href.replace('bootstrap.min.css', 'bootstrap.rtl.min.css'));
                } else if (!isRtl && href.includes('bootstrap.rtl.min.css')) {
                    // Switch to regular version
                    link.setAttribute('href', href.replace('bootstrap.rtl.min.css', 'bootstrap.min.css'));
                }
            }
        });
    }

    // Initialize on page load
    function initialize() {
        try {
            const currentLang = getCurrentLanguage();
            applyDirection(currentLang);
        } catch (e) {
            console.error('Error initializing language switcher:', e);
            // Fallback to Arabic/RTL
            applyDirection('ar');
        }
    }

    // Run on DOMContentLoaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        // DOM already loaded
        initialize();
    }

    // Handle language switch links (let server handle cookie, just apply direction immediately)
    document.addEventListener('click', function(e) {
        const langLink = e.target.closest('a[href*="culture="]');
        if (langLink) {
            try {
                const url = new URL(langLink.href, window.location.origin);
                const culture = url.searchParams.get('culture') || url.searchParams.get('ui-culture');
                if (culture) {
                    // Apply direction immediately for better UX
                    // Server will set cookie on page reload
                    applyDirection(culture);
                }
            } catch (e) {
                console.warn('Error handling language switch:', e);
            }
        }
    });

    // Export functions for use in other scripts
    window.GrcLanguage = {
        getCurrentLanguage: getCurrentLanguage,
        applyDirection: applyDirection,
        // Note: setLanguage removed - server handles cookie setting via URL parameters
        // If needed, use: window.location.href = '/?culture=ar&ui-culture=ar'
    };
})();
