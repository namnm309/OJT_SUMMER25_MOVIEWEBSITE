
console.log('🔧 Movie Card Debug Script Loaded');

document.addEventListener('DOMContentLoaded', function() {
    console.log('🔧 DOM Content Loaded - Starting movie card debug');
    

    function debugMovieCards() {
        const movieCards = document.querySelectorAll('.recommended-item');
        const overlays = document.querySelectorAll('.recommended-overlay');
        const buttons = document.querySelectorAll('.recommended-view-btn');
        
        console.log(`🔧 Found ${movieCards.length} movie cards`);
        console.log(`🔧 Found ${overlays.length} overlays`);
        console.log(`🔧 Found ${buttons.length} view buttons`);
        

        overlays.forEach((overlay, index) => {
            const styles = getComputedStyle(overlay);
            console.log(`🔧 Overlay ${index}:`, {
                opacity: styles.opacity,
                pointerEvents: styles.pointerEvents,
                zIndex: styles.zIndex,
                position: styles.position
            });
        });
        
        buttons.forEach((button, index) => {
            const styles = getComputedStyle(button);
            console.log(`🔧 Button ${index}:`, {
                pointerEvents: styles.pointerEvents,
                zIndex: styles.zIndex,
                display: styles.display,
                href: button.href
            });
            

            button.addEventListener('click', function(e) {
                console.log(`🔧 Button ${index} clicked!`, e);
                console.log(`🔧 Link href: ${this.href}`);
                

                if (this.href && this.href !== '#') {
                    console.log(`🔧 Navigation should work to: ${this.href}`);
                } else {
                    console.error(`🔧 Invalid href: ${this.href}`);
                    e.preventDefault();
                }
            });
        });
    }
    

    function fixMovieCards() {
        console.log('🔧 Applying movie card fixes...');
        
        const buttons = document.querySelectorAll('.recommended-view-btn');
        buttons.forEach((button, index) => {

            button.style.pointerEvents = 'auto';
            button.style.position = 'relative';
            button.style.zIndex = '1000';
            

            button.style.cursor = 'pointer';
            
            console.log(`🔧 Fixed button ${index}`);
        });
        

        if ('ontouchstart' in window) {
            const overlays = document.querySelectorAll('.recommended-overlay');
            overlays.forEach((overlay, index) => {
                overlay.style.opacity = '0.9';
                overlay.style.pointerEvents = 'auto';
                console.log(`🔧 Made overlay ${index} touch-friendly`);
            });
        }
    }
    

    function enableDebugMode() {
        const urlParams = new URLSearchParams(window.location.search);
        if (urlParams.get('debug') === 'true') {
            console.log('🔧 Debug mode enabled');
            
            document.querySelectorAll('.recommended-overlay').forEach(overlay => {
                overlay.classList.add('debug');
            });
            
            document.querySelectorAll('.recommended-view-btn').forEach(button => {
                button.classList.add('debug');
            });
        }
    }
    

    debugMovieCards();
    fixMovieCards();
    enableDebugMode();
    

    document.addEventListener('click', function(e) {
        if (e.target.closest('.recommended-view-btn')) {
            console.log('🔧 Detected click on movie card button:', e.target);
        }
    });
    
    console.log('🔧 Movie card debug setup complete');
});


window.movieCardDebug = {
    test: function() {
        console.log('🔧 Testing movie card functionality...');
        const firstButton = document.querySelector('.recommended-view-btn');
        if (firstButton) {
            console.log('🔧 First button found:', firstButton);
            console.log('🔧 Button href:', firstButton.href);
            console.log('🔧 Button styles:', getComputedStyle(firstButton));
            

            console.log('🔧 Simulating click...');
            firstButton.click();
        } else {
            console.error('🔧 No movie card buttons found');
        }
    },
    
    fix: function() {
        console.log('🔧 Re-running fixes...');
        document.querySelectorAll('.recommended-view-btn').forEach(button => {
            button.style.pointerEvents = 'auto !important';
            button.style.zIndex = '1000 !important';
        });
    }
}; 