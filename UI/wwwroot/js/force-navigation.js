// Force Navigation Script
console.log('🚀 Force Navigation Script Loaded');

// Add click handlers to movie cards
document.addEventListener('DOMContentLoaded', function() {
    console.log('🚀 Setting up force navigation...');
    
    // Add click to entire movie card
    const movieCards = document.querySelectorAll('.recommended-item');
    movieCards.forEach(function(card, index) {
        console.log('🚀 Setting up card', index);
        
        // Find the button inside this card
        const button = card.querySelector('.recommended-view-btn');
        if (button && button.href) {
            // Add click handler to entire card
            card.style.cursor = 'pointer';
            card.addEventListener('click', function(e) {
                console.log('🚀 Card clicked, navigating to:', button.href);
                e.preventDefault();
                e.stopPropagation();
                window.location.href = button.href;
            });
        }
    });
    
    // Backup: Add direct click to buttons
    const buttons = document.querySelectorAll('.recommended-view-btn');
    buttons.forEach(function(button, index) {
        console.log('🚀 Setting up button', index, button.href);
        
        button.addEventListener('click', function(e) {
            console.log('🚀 Button direct click:', this.href);
            e.preventDefault();
            e.stopPropagation();
            
            if (this.href) {
                window.location.href = this.href;
            }
        });
    });
});

// Global test function
window.forceNavigate = function(movieId) {
    if (!movieId) {
        movieId = '11111111-1111-1111-1111-0000000000a1'; // Default test movie
    }
    
    const url = '/Movies/Details/' + movieId;
    console.log('🚀 Force navigating to:', url);
    window.location.href = url;
}; 