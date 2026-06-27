(function () {
    'use strict';

    // ── Theme toggle ──────────────────────────────────────────────────────────
    var THEME_KEY = 'ica_theme';
    var htmlRoot = document.getElementById('htmlRoot');
    var themeToggle = document.getElementById('themeToggle');
    var themeIcon = document.getElementById('themeIcon');

    function applyTheme(theme) {
        if (!htmlRoot) return;
        htmlRoot.setAttribute('data-bs-theme', theme);
        if (themeIcon) {
            themeIcon.className = theme === 'dark'
                ? 'bi bi-sun-fill'
                : 'bi bi-moon-stars-fill';
        }
    }

    var savedTheme = localStorage.getItem(THEME_KEY) || 'light';
    applyTheme(savedTheme);

    if (themeToggle) {
        themeToggle.addEventListener('click', function () {
            var current = htmlRoot ? htmlRoot.getAttribute('data-bs-theme') : 'light';
            var next = current === 'dark' ? 'light' : 'dark';
            localStorage.setItem(THEME_KEY, next);
            applyTheme(next);
        });
    }

    // ── Password visibility toggle ────────────────────────────────────────────
    window.togglePassword = function (inputId, btn) {
        var input = document.getElementById(inputId);
        if (!input) return;
        var icon = btn.querySelector('i');
        if (input.type === 'password') {
            input.type = 'text';
            if (icon) { icon.className = 'bi bi-eye-slash'; }
        } else {
            input.type = 'password';
            if (icon) { icon.className = 'bi bi-eye'; }
        }
    };

    // ── Color swatch ──────────────────────────────────────────────────────────
    window.setProjectColor = function (color, inputId) {
        var input = document.getElementById(inputId);
        if (input) input.value = color;
    };

    // ── Auto-dismiss alerts ────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('.alert.alert-success').forEach(function (alert) {
            setTimeout(function () {
                var bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
                if (bsAlert) bsAlert.close();
            }, 4000);
        });
    });
})();
