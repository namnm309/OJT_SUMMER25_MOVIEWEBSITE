

class DashboardSidebar {
    constructor() {
        this.init();
    }

    init() {
        this.setupDropdownHandlers();
        this.setupActiveStates();
        this.setupAuthorizationIndicator();
        this.setupMobileToggle();
    }

    setupDropdownHandlers() {
        const dropdownToggles = document.querySelectorAll('.dropdown-toggle');
        
        dropdownToggles.forEach(toggle => {
            toggle.addEventListener('click', (e) => {
                // Chỉ ngăn điều hướng nếu href là '#'
                if (toggle.getAttribute('href') === '#') {
                    e.preventDefault();
                }
                this.toggleDropdown(toggle);
            });
        });
    }

    toggleDropdown(toggle) {
        const dropdown = toggle.nextElementSibling;
        const isExpanded = toggle.classList.contains('expanded');
        

        this.closeAllDropdowns(toggle);
        

        if (isExpanded) {
            this.closeDropdown(toggle, dropdown);
        } else {
            this.openDropdown(toggle, dropdown);
        }
    }

    openDropdown(toggle, dropdown) {
        toggle.classList.add('expanded');
        dropdown.classList.add('show');
        

        dropdown.style.maxHeight = dropdown.scrollHeight + 'px';
    }

    closeDropdown(toggle, dropdown) {
        toggle.classList.remove('expanded');
        dropdown.classList.remove('show');
        

        dropdown.style.maxHeight = '0px';
    }

    closeAllDropdowns(currentToggle = null) {
        const dropdownToggles = document.querySelectorAll('.dropdown-toggle');
        
        dropdownToggles.forEach(toggle => {
            if (toggle !== currentToggle) {
                const dropdown = toggle.nextElementSibling;
                if (dropdown) {
                    this.closeDropdown(toggle, dropdown);
                }
            }
        });
    }

    setupActiveStates() {

        const activeDropdownLinks = document.querySelectorAll('.new-nav-dropdown .new-nav-link.active');
        
        activeDropdownLinks.forEach(link => {
            const dropdown = link.closest('.new-nav-dropdown');
            const toggle = dropdown?.previousElementSibling;
            
            if (dropdown && toggle) {
                this.openDropdown(toggle, dropdown);
            }
        });


        this.setupHoverEffects();
    }

    setupHoverEffects() {
        const navLinks = document.querySelectorAll('.new-nav-link');
        
        navLinks.forEach(link => {
            link.addEventListener('mouseenter', () => {
                if (!link.classList.contains('active')) {
                    link.style.transform = 'translateX(8px)';
                }
            });

            link.addEventListener('mouseleave', () => {
                if (!link.classList.contains('active')) {
                    link.style.transform = 'translateX(0)';
                }
            });
        });
    }

    setupAuthorizationIndicator() {
        const authIndicator = document.querySelector('.auth-indicator');
        
        if (authIndicator) {
            this.checkAuthorizationStatus(authIndicator);
            

            setInterval(() => {
                this.checkAuthorizationStatus(authIndicator);
            }, 30000);
        }
    }

    checkAuthorizationStatus(indicator) {

        indicator.style.background = '#10b981';
        indicator.style.boxShadow = '0 0 0 2px rgba(16, 185, 129, 0.3)';
        indicator.title = 'Đã kết nối';
    }

    hasValidCookieSession() {

        const cookies = document.cookie.split(';');
        
        for (let cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            

            if (name === '.AspNetCore.Identity.Application' || 
                name === '.AspNetCore.Antiforgery' ||
                name.startsWith('.AspNetCore.Session')) {
                
                if (value && value.length > 10) {
                    return true;
                }
            }
        }
        
        return false;
    }

    handleExpiredSession() {


        console.log('Session may have expired - check auth status');
    }

    setupMobileToggle() {

        if (window.innerWidth <= 768 && !document.querySelector('.mobile-sidebar-toggle')) {
            this.createMobileToggle();
        }


        window.addEventListener('resize', () => {
            if (window.innerWidth <= 768) {
                this.enableMobileMode();
            } else {
                this.disableMobileMode();
            }
        });
    }

    createMobileToggle() {
        const toggleButton = document.createElement('button');
        toggleButton.className = 'mobile-sidebar-toggle';
        toggleButton.innerHTML = '<i class="fas fa-bars"></i>';
        toggleButton.style.cssText = `
            position: fixed;
            top: 20px;
            left: 20px;
            z-index: 1002;
            background: var(--primary-purple);
            border: none;
            border-radius: 8px;
            padding: 12px;
            color: white;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
            transition: all 0.3s ease;
        `;

        toggleButton.addEventListener('click', () => {
            this.toggleMobileSidebar();
        });

        document.body.appendChild(toggleButton);
    }

    toggleMobileSidebar() {
        const sidebar = document.querySelector('.new-dashboard-sidebar');
        const isVisible = sidebar.classList.contains('show');

        if (isVisible) {
            sidebar.classList.remove('show');
        } else {
            sidebar.classList.add('show');
        }
    }

    enableMobileMode() {
        const sidebar = document.querySelector('.new-dashboard-sidebar');
        if (sidebar) {
            sidebar.classList.add('mobile-mode');
        }
    }

    disableMobileMode() {
        const sidebar = document.querySelector('.new-dashboard-sidebar');
        if (sidebar) {
            sidebar.classList.remove('mobile-mode', 'show');
        }
    }


    refreshAuthStatus() {
        const authIndicator = document.querySelector('.auth-indicator');
        if (authIndicator) {
            this.checkAuthorizationStatus(authIndicator);
        }
    }

    closeAllDropdowns() {
        this.closeAllDropdowns();
    }
}


document.addEventListener('DOMContentLoaded', () => {
    window.dashboardSidebar = new DashboardSidebar();
});


if (typeof module !== 'undefined' && module.exports) {
    module.exports = DashboardSidebar;
} 