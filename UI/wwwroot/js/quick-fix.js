// Quick Fix for Movie Card Click Issue
console.log('🔧 Quick Fix Script Loaded');

// Wait for DOM to load
window.addEventListener('load', function() {
    console.log('🔧 Window loaded - Applying quick fixes');
    
    // Find all movie card buttons
    const buttons = document.querySelectorAll('.recommended-view-btn');
    console.log('🔧 Found buttons:', buttons.length);
    
    buttons.forEach(function(button, index) {
        console.log('🔧 Processing button', index, button);
        
        // Force styles
        button.style.pointerEvents = 'auto';
        button.style.position = 'relative';
        button.style.zIndex = '9999';
        button.style.cursor = 'pointer';
        
        // Add click handler
        button.addEventListener('click', function(e) {
            console.log('🔧 Button clicked!', this.href);
            
            // Ensure navigation works
            if (this.href && this.href !== window.location.href) {
                console.log('🔧 Navigating to:', this.href);
                window.location.href = this.href;
            } else {
                console.error('🔧 Invalid href:', this.href);
            }
        });
        
        // Test click programmatically
        console.log('🔧 Button', index, 'setup complete. href:', button.href);
    });
    
    // Force overlay fixes
    const overlays = document.querySelectorAll('.recommended-overlay');
    overlays.forEach(function(overlay, index) {
        overlay.style.pointerEvents = 'none';
        console.log('🔧 Fixed overlay', index);
    });
    
    // Add hover handlers to enable overlay
    const movieItems = document.querySelectorAll('.recommended-item');
    movieItems.forEach(function(item, index) {
        item.addEventListener('mouseenter', function() {
            const overlay = this.querySelector('.recommended-overlay');
            if (overlay) {
                overlay.style.pointerEvents = 'auto';
                overlay.style.opacity = '1';
            }
        });
        
        item.addEventListener('mouseleave', function() {
            const overlay = this.querySelector('.recommended-overlay');
            if (overlay) {
                overlay.style.pointerEvents = 'none';
                overlay.style.opacity = '0';
            }
        });
    });
    
    console.log('🔧 Quick fix complete');
});

// Global test function
window.testMovieCard = function() {
    console.log('🔧 Testing first movie card...');
    const firstButton = document.querySelector('.recommended-view-btn');
    if (firstButton) {
        console.log('🔧 First button found:', firstButton.href);
        console.log('🔧 Clicking button...');
        firstButton.click();
    } else {
        console.error('🔧 No buttons found');
    }
}; 