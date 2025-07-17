function showTab(tabName, button) {
            // Hide all tab contents
            const tabContents = document.querySelectorAll('.tab-content');
            tabContents.forEach(content => {
        content.classList.remove('active');
            });

    // Remove active class from all tab buttons
    const tabButtons = document.querySelectorAll('.tab-btn');
            tabButtons.forEach(btn => {
        btn.classList.remove('active');
            });

    // Show selected tab content
    document.getElementById(tabName).classList.add('active');

    // Add active class to clicked button
    button.classList.add('active');
        }

    // Initialize page
    document.addEventListener('DOMContentLoaded', function() {
        console.log('✅ User Profile Loaded Successfully');

            // Add smooth scrolling for better UX
            document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth'
                });
            }
        });
            });
        });
