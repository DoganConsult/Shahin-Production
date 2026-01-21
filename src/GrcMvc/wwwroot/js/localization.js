/**
 * JavaScript Localization Helper
 * Provides client-side localization support for GRC system
 */

(function() {
    'use strict';

    // Get current culture from meta tag or cookie
    function getCurrentCulture() {
        const metaTag = document.querySelector('meta[name="culture"]');
        if (metaTag) {
            return metaTag.getAttribute('content') || 'ar';
        }
        
        // Fallback to cookie
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            if (name === 'GrcMvc.Culture') {
                if (value) {
                    try {
                        // Cookie format: c=ar|uic=ar (from CookieRequestCultureProvider)
                        const decoded = decodeURIComponent(value);
                        const match = decoded.match(/c=([^|]+)/);
                        if (match && match[1]) {
                            return match[1];
                        }
                        // Fallback: try to extract culture directly
                        return decoded.replace(/"/g, '').substring(0, 2);
                    } catch (e) {
                        console.warn('Error parsing culture cookie:', e);
                    }
                }
                return 'ar';
            }
        }
        
        return 'ar'; // Default
    }

    // Localization dictionary (populated from server)
    let localizationDict = {};

    /**
     * Initialize localization
     * @param {Object} translations - Dictionary of translations
     */
    window.initLocalization = function(translations) {
        if (translations && typeof translations === 'object') {
            localizationDict = translations;
        }
    };

    /**
     * Get localized string
     * @param {string} key - Resource key
     * @param {Array} args - Optional format arguments
     * @returns {string} Localized string
     */
    window.L = function(key, ...args) {
        const translation = localizationDict[key] || key;
        
        if (args.length > 0) {
            // Simple format replacement: {0}, {1}, etc.
            return translation.replace(/\{(\d+)\}/g, (match, index) => {
                const argIndex = parseInt(index, 10);
                return args[argIndex] !== undefined ? args[argIndex] : match;
            });
        }
        
        return translation;
    };

    /**
     * Get current culture code
     * @returns {string} Culture code (e.g., 'ar', 'en')
     */
    window.getCulture = function() {
        return getCurrentCulture();
    };

    /**
     * Check if current culture is RTL
     * @returns {boolean} True if RTL language
     */
    window.isRTL = function() {
        return getCurrentCulture() === 'ar';
    };

    /**
     * Format date according to current culture
     * @param {Date|string} date - Date to format
     * @param {Object} options - Intl.DateTimeFormat options
     * @returns {string} Formatted date
     */
    window.formatDate = function(date, options = {}) {
        const dateObj = date instanceof Date ? date : new Date(date);
        const culture = getCurrentCulture();
        const locale = culture === 'ar' ? 'ar-SA' : 'en-US';
        
        const defaultOptions = {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        };
        
        return new Intl.DateTimeFormat(locale, { ...defaultOptions, ...options }).format(dateObj);
    };

    /**
     * Format number according to current culture
     * @param {number} number - Number to format
     * @param {Object} options - Intl.NumberFormat options
     * @returns {string} Formatted number
     */
    window.formatNumber = function(number, options = {}) {
        const culture = getCurrentCulture();
        const locale = culture === 'ar' ? 'ar-SA' : 'en-US';
        
        return new Intl.NumberFormat(locale, options).format(number);
    };

    // Auto-initialize from data attributes
    document.addEventListener('DOMContentLoaded', function() {
        // Check for embedded translations in script tag
        const translationsScript = document.getElementById('localization-data');
        if (translationsScript) {
            try {
                const translations = JSON.parse(translationsScript.textContent);
                window.initLocalization(translations);
            } catch (e) {
                console.warn('Failed to parse localization data:', e);
            }
        }

        // Set culture meta tag if not exists
        if (!document.querySelector('meta[name="culture"]')) {
            const meta = document.createElement('meta');
            meta.name = 'culture';
            meta.content = getCurrentCulture();
            document.head.appendChild(meta);
        }
    });

})();
