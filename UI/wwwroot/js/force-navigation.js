
console.log('🚀 Force Navigation Script Loaded');


document.addEventListener('DOMContentLoaded', function() {
    console.log('🚀 Setting up force navigation...');
    

    const movieCards = document.querySelectorAll('.recommended-item');
    movieCards.forEach(function(card, index) {
        console.log('🚀 Setting up card', index);
        

        const button = card.querySelector('.recommended-view-btn');
        if (button && button.href) {

            card.style.cursor = 'pointer';
            card.addEventListener('click', function(e) {
                console.log('🚀 Card clicked, navigating to:', button.href);
                e.preventDefault();
                e.stopPropagation();
                window.location.href = button.href;
            });
        }
    });
    

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


window.forceNavigate = function(movieId) {
    if (!movieId) {
        movieId = '11111111-1111-1111-1111-0000000000a1'; // Default test movie
    }
    
    const url = '/Movies/Details/' + movieId;
    console.log('🚀 Force navigating to:', url);
    window.location.href = url;
}; 