// Movie data for carousel - lấy từ server hoặc dùng dữ liệu mặc định
// Bỏ hardcoded array, chỉ sử dụng data từ API
const movies = window.heroMovies || [];

let currentMovieIndex = 0;
let movieInterval;
let isTransitioning = false;

// Initialize page
document.addEventListener('DOMContentLoaded', function() {
    // Kiểm tra nếu có data từ server
    if (!movies || movies.length === 0) {
        console.warn('Không có dữ liệu movies từ server');
        return;
    }
    
    initializeCarousel();
    updateMovieDisplay();
    startMovieCarousel();
    updatePaginationDots();
    const userDropdown = document.getElementById('userDropdown');
    const dropdownMenu = userDropdown?.nextElementSibling;
    
    if (userDropdown && dropdownMenu) {
        // Toggle dropdown on click
        userDropdown.addEventListener('click', function(e) {
            e.stopPropagation();
            dropdownMenu.classList.toggle('show');
            
            // Toggle chevron rotation
            const chevron = userDropdown.querySelector('.fa-chevron-down');
            if (chevron) {
                chevron.style.transform = dropdownMenu.classList.contains('show') ? 'rotate(180deg)' : 'rotate(0)';
                chevron.style.transition = 'transform 0.3s ease';
            }
        });
        
        // Close dropdown when clicking outside
        document.addEventListener('click', function(e) {
            if (!userDropdown.contains(e.target) && !dropdownMenu.contains(e.target)) {
                dropdownMenu.classList.remove('show');
                const chevron = userDropdown.querySelector('.fa-chevron-down');
                if (chevron) {
                    chevron.style.transform = 'rotate(0)';
                }
            }
        });
        
        // Prevent dropdown from closing when clicking inside
        dropdownMenu.addEventListener('click', function(e) {
            e.stopPropagation();
        });
    }
});

// Initialize carousel with smooth transitions
function initializeCarousel() {
    const heroSection = document.getElementById('heroSection');
    const movieContent = document.getElementById('movieContent');
    
    // Create background layers for smooth transitions
    createBackgroundLayers();
    
    // Add transition styles if not already present
    if (!movieContent.style.transition) {
        movieContent.style.transition = 'opacity 0.5s ease-in-out, transform 0.5s ease-in-out';
    }
}

// Create two background layers for crossfade effect
function createBackgroundLayers() {
    const heroSection = document.getElementById('heroSection');
    
    // Remove existing background layers if any
    const existingLayers = heroSection.querySelectorAll('.bg-layer');
    existingLayers.forEach(layer => layer.remove());
    
    // Create two background layers
    const bgLayer1 = document.createElement('div');
    const bgLayer2 = document.createElement('div');
    
    bgLayer1.className = 'bg-layer bg-layer-1';
    bgLayer2.className = 'bg-layer bg-layer-2';
    
    // Set initial styles
    const layerStyles = {
        position: 'absolute',
        top: '0',
        left: '0',
        width: '100%',
        height: '100%',
        backgroundSize: 'cover',
        backgroundPosition: '50%',
        backgroundRepeat: 'no-repeat',
        transition: 'opacity 1s cubic-bezier(0.4, 0, 0.2, 1)',
        zIndex: '1'
    };
    
    Object.assign(bgLayer1.style, layerStyles);
    Object.assign(bgLayer2.style, layerStyles);
    
    // Set initial background
    const initialMovie = movies[currentMovieIndex];
    const gradient = 'linear-gradient(107deg, rgba(0, 0, 0, 0.00) 36.24%, rgba(14, 14, 14, 0.55) 57.42%, rgba(12, 12, 12, 0.99) 76.93%)';
    
    bgLayer1.style.background = `${gradient}, url('${initialMovie.background}') lightgray 50% / cover no-repeat`;
    bgLayer1.style.opacity = '1';
    bgLayer2.style.opacity = '0';
    
    // Insert layers at the beginning of hero section
    heroSection.insertBefore(bgLayer1, heroSection.firstChild);
    heroSection.insertBefore(bgLayer2, heroSection.firstChild);
    
    // Ensure movie content is above background layers
    const movieContent = document.getElementById('movieContent');
    if (movieContent) {
        movieContent.style.position = 'relative';
        movieContent.style.zIndex = '10';
    }
    
    // Ensure movie controls are above background layers
    const movieControls = document.querySelector('.movie-controls');
    if (movieControls) {
        movieControls.style.position = 'relative';
        movieControls.style.zIndex = '10';
    }
}

// Enhanced movie display with smooth background transitions
function updateMovieDisplay(direction = 'next') {
    if (isTransitioning) return;
    isTransitioning = true;
    
    const movie = movies[currentMovieIndex];
    const heroSection = document.getElementById('heroSection');
    const movieContent = document.getElementById('movieContent');
    const bgLayer1 = heroSection.querySelector('.bg-layer-1');
    const bgLayer2 = heroSection.querySelector('.bg-layer-2');
    
    if (!bgLayer1 || !bgLayer2) {
        createBackgroundLayers();
        return;
    }
    
    // Start content fade out animation
    movieContent.style.opacity = '0';
    movieContent.style.transform = direction === 'next' ? 'translateX(-30px)' : 'translateX(30px)';
    
    // Preload new background image
    const img = new Image();
    img.onload = function() {
        // Determine which layer is currently visible
        const activeLayer = bgLayer1.style.opacity === '1' ? bgLayer1 : bgLayer2;
        const inactiveLayer = bgLayer1.style.opacity === '1' ? bgLayer2 : bgLayer1;
        
        // Set new background on inactive layer
        const gradient = 'linear-gradient(107deg, rgba(0, 0, 0, 0.00) 36.24%, rgba(14, 14, 14, 0.55) 57.42%, rgba(12, 12, 12, 0.99) 76.93%)';
        inactiveLayer.style.background = `${gradient}, url('${movie.background}') lightgray 50% / cover no-repeat`;
        
        // Start crossfade animation
        setTimeout(() => {
            activeLayer.style.opacity = '0';
            inactiveLayer.style.opacity = '1';
        }, 100);
    };
    
    img.onerror = function() {
        // Fallback if image fails to load
        console.warn('Failed to load background image:', movie.background);
        setTimeout(() => {
            isTransitioning = false;
        }, 500);
    };
    
    img.src = movie.background;
    
    // Update content after fade out
    setTimeout(() => {
        // Update text content
        document.getElementById('movieTitle').textContent = movie.title;
        document.getElementById('movieTitleVn').textContent = movie.titleVn;
        document.getElementById('moviePlot').textContent = movie.plot;
        document.getElementById('movieGenre').textContent = movie.genre;
        document.getElementById('duration').textContent = movie.duration;
        
        // Fade in with slide animation
        movieContent.style.transform = direction === 'next' ? 'translateX(30px)' : 'translateX(-30px)';
        
        // Trigger reflow to ensure transform is applied
        movieContent.offsetHeight;
        
        // Fade in
        movieContent.style.opacity = '1';
        movieContent.style.transform = 'translateX(0)';
        
        // Reset transition flag after all animations complete
        setTimeout(() => {
            isTransitioning = false;
        }, 800);
        
    }, 250);
}

function updatePaginationDots() {
    const dots = document.querySelectorAll('.dot');
    dots.forEach((dot, index) => {
        dot.classList.toggle('active', index === currentMovieIndex);
        
        // Add smooth transition to dots
        if (!dot.style.transition) {
            dot.style.transition = 'all 0.3s ease';
        }
    });
}

function nextMovie() {
    if (isTransitioning) return;
    currentMovieIndex = (currentMovieIndex + 1) % movies.length;
    updateMovieDisplay('next');
    updatePaginationDots();
    resetCarouselTimer();
}

function previousMovie() {
    if (isTransitioning) return;
    currentMovieIndex = (currentMovieIndex - 1 + movies.length) % movies.length;
    updateMovieDisplay('prev');
    updatePaginationDots();
    resetCarouselTimer();
}

function startMovieCarousel() {
    movieInterval = setInterval(nextMovie, 6000); // Increased to 6 seconds for better UX
}

function resetCarouselTimer() {
    clearInterval(movieInterval);
    startMovieCarousel();
}

// Enhanced dot click with smooth transition
document.addEventListener('click', function(e) {
    if (e.target.classList.contains('dot')) {
        if (isTransitioning) return;
        
        const dots = Array.from(document.querySelectorAll('.dot'));
        const newIndex = dots.indexOf(e.target);
        
        if (newIndex !== currentMovieIndex) {
            const direction = newIndex > currentMovieIndex ? 'next' : 'prev';
            currentMovieIndex = newIndex;
            updateMovieDisplay(direction);
            updatePaginationDots();
            resetCarouselTimer();
        }
    }
});

// Action button functions
function bookTickets() {
    const currentMovie = movies[currentMovieIndex];
    if (currentMovie && currentMovie.id) {
        window.location.href = window.movieUrls.moviesIndex + '?movieId=' + currentMovie.id;
    } else {
        window.location.href = window.movieUrls.moviesIndex;
    }
}

function showMovieInfo() {
    const currentMovie = movies[currentMovieIndex];
    if (currentMovie && currentMovie.id) {
        window.location.href = window.movieUrls.movieDetails + '/' + currentMovie.id;
    } else {
        // Fallback nếu không có ID
        window.location.href = window.movieUrls.moviesIndex;
    }
}

// Search section toggle with smooth animation
function toggleSearch() {
    const searchForm = document.getElementById('searchForm');
    const searchToggle = document.querySelector('.search-toggle');
    
    searchForm.classList.toggle('expanded');
    searchToggle.classList.toggle('collapsed');
    
    // Add smooth rotation to toggle icon
    if (!searchToggle.style.transition) {
        searchToggle.style.transition = 'transform 0.3s ease';
    }
}

// Enhanced movie recommendations slide
function slideMovies(direction) {
    const grid = document.getElementById('moviesGrid');
    const scrollAmount = 300;
    
    if (direction === 'left') {
        grid.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
    } else {
        grid.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    }
}

// Toggle switches with animation
document.addEventListener('click', function(e) {
    if (e.target.closest('.toggle-switch')) {
        const toggle = e.target.closest('.toggle-switch');
        toggle.classList.toggle('active');
        
        // Add smooth transition if not present
        const circle = toggle.querySelector('.toggle-circle');
        if (circle && !circle.style.transition) {
            circle.style.transition = 'transform 0.2s ease';
        }
    }
});

// Enhanced movie card hover effects
document.addEventListener('mouseenter', function(e) {
    if (e.target.closest('.movie-card') && !e.target.closest('.movie-card').classList.contains('featured')) {
        const card = e.target.closest('.movie-card');
        card.style.transition = 'transform 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275)';
        card.style.transform = 'translateY(-8px) scale(1.03)';
    }
}, true);

document.addEventListener('mouseleave', function(e) {
    if (e.target.closest('.movie-card') && !e.target.closest('.movie-card').classList.contains('featured')) {
        const card = e.target.closest('.movie-card');
        card.style.transform = 'translateY(0) scale(1)';
    }
}, true);

// Pause carousel on hover with smooth transition
document.getElementById('heroSection').addEventListener('mouseenter', function() {
    clearInterval(movieInterval);
});

document.getElementById('heroSection').addEventListener('mouseleave', function() {
    if (!isTransitioning) {
        startMovieCarousel();
    }
});

// Add keyboard navigation
document.addEventListener('keydown', function(e) {
    if (e.key === 'ArrowLeft') {
        previousMovie();
    } else if (e.key === 'ArrowRight') {
        nextMovie();
    }
});

// Add touch/swipe support for mobile
let touchStartX = 0;
let touchEndX = 0;

document.getElementById('heroSection').addEventListener('touchstart', function(e) {
    touchStartX = e.changedTouches[0].screenX;
});

document.getElementById('heroSection').addEventListener('touchend', function(e) {
    touchEndX = e.changedTouches[0].screenX;
    handleSwipe();
});

function handleSwipe() {
    const swipeThreshold = 50;
    const diff = touchStartX - touchEndX;
    
    if (Math.abs(diff) > swipeThreshold) {
        if (diff > 0) {
            nextMovie(); // Swipe left - next movie
        } else {
            previousMovie(); // Swipe right - previous movie
        }
    }
}


// Thêm vào cuối file homepage.js
async function testApiConnection() {
    try {
        const response = await fetch('/api/v1/movie/View');
        const data = await response.json();
        console.log('API Response:', data);
        return response.status;
    } catch (error) {
        console.error('API Error:', error);
        return 'Error';
    }
}

// Gọi function này khi trang load
document.addEventListener('DOMContentLoaded', function () {
    testApiConnection();
});

// Tạo video layers thay vì background layers
function createVideoLayers() {
    const heroSection = document.getElementById('heroSection');
    
    // Remove existing layers
    const existingLayers = heroSection.querySelectorAll('.video-layer, .bg-layer');
    existingLayers.forEach(layer => layer.remove());
    
    // Create two video layers for crossfade
    const videoLayer1 = document.createElement('div');
    const videoLayer2 = document.createElement('div');
    
    videoLayer1.className = 'video-layer video-layer-1';
    videoLayer2.className = 'video-layer video-layer-2';
    
    // Create overlay gradients
    const overlay1 = document.createElement('div');
    const overlay2 = document.createElement('div');
    overlay1.className = 'video-overlay';
    overlay2.className = 'video-overlay';
    
    videoLayer1.appendChild(overlay1);
    videoLayer2.appendChild(overlay2);
    
    // Set initial state
    videoLayer1.style.opacity = '1';
    videoLayer2.style.opacity = '0';
    
    // Load initial video
    const initialMovie = movies[currentMovieIndex];
    if (initialMovie && initialMovie.trailerUrl) {
        loadVideoInLayer(videoLayer1, initialMovie.trailerUrl);
    }
    
    // Insert layers
    heroSection.insertBefore(videoLayer1, heroSection.firstChild);
    heroSection.insertBefore(videoLayer2, heroSection.firstChild);
}

// Convert YouTube URL to embed format với autoplay
function getYouTubeEmbedUrl(url) {
    const videoId = extractYouTubeVideoId(url);
    if (videoId) {
        return `https://www.youtube.com/embed/${videoId}?autoplay=1&mute=1&loop=1&playlist=${videoId}&controls=0&showinfo=0&rel=0&iv_load_policy=3&modestbranding=1&start=0&end=30`;
    }
    return null;
}

// Extract YouTube video ID
function extractYouTubeVideoId(url) {
    const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|\&v=)([^#\&\?]*).*/;
    const match = url.match(regExp);
    return (match && match[2].length === 11) ? match[2] : null;
}

// Load video into layer
function loadVideoInLayer(layer, trailerUrl) {
    // Remove existing video/iframe
    const existingMedia = layer.querySelector('video, iframe');
    if (existingMedia) {
        existingMedia.remove();
    }
    
    if (trailerUrl.includes('youtube.com') || trailerUrl.includes('youtu.be')) {
        // Create YouTube iframe
        const iframe = document.createElement('iframe');
        const videoId = extractYouTubeVideoId(trailerUrl);
        iframe.src = `https://www.youtube.com/embed/${videoId}?autoplay=1&mute=1&loop=1&playlist=${videoId}&controls=0&showinfo=0&rel=0&iv_load_policy=3&modestbranding=1`;
        iframe.allow = 'autoplay; encrypted-media';
        iframe.style.position = 'absolute';
        iframe.style.top = '0';
        iframe.style.left = '0';
        
        // Insert before overlay
        const overlay = layer.querySelector('.video-overlay');
        layer.insertBefore(iframe, overlay);
    } else {
        // Create video element for direct URLs
        const video = document.createElement('video');
        video.src = trailerUrl;
        video.muted = true;
        video.loop = true;
        video.autoplay = true;
        video.playsInline = true;
        video.preload = 'metadata';
        
        // Insert before overlay
        const overlay = layer.querySelector('.video-overlay');
        layer.insertBefore(video, overlay);
        
        // Handle video load
        video.addEventListener('loadeddata', () => {
            video.currentTime = 0; // Start from beginning
        });
    }
}

// Enhanced movie display with video transitions
function updateMovieDisplay(direction = 'next') {
    if (isTransitioning) return;
    isTransitioning = true;
    
    const movie = movies[currentMovieIndex];
    const heroSection = document.getElementById('heroSection');
    const movieContent = document.getElementById('movieContent');
    const videoLayer1 = heroSection.querySelector('.video-layer-1');
    const videoLayer2 = heroSection.querySelector('.video-layer-2');
    
    if (!videoLayer1 || !videoLayer2) {
        createVideoLayers();
        return;
    }
    
    // Start content fade out
    movieContent.style.opacity = '0';
    movieContent.style.transform = direction === 'next' ? 'translateX(-30px)' : 'translateX(30px)';
    
    // Determine active and inactive layers
    const activeLayer = videoLayer1.style.opacity === '1' ? videoLayer1 : videoLayer2;
    const inactiveLayer = videoLayer1.style.opacity === '1' ? videoLayer2 : videoLayer1;
    
    // Load new video on inactive layer
    if (movie && movie.trailerUrl) {
        loadVideoInLayer(inactiveLayer, movie.trailerUrl);
    }
    
    // Start crossfade after short delay
    setTimeout(() => {
        activeLayer.style.opacity = '0';
        inactiveLayer.style.opacity = '1';
    }, 100);
    
    // Update movie content
    setTimeout(() => {
        updateMovieContent(movie);
        
        // Fade content back in
        setTimeout(() => {
            movieContent.style.opacity = '1';
            movieContent.style.transform = 'translateX(0)';
            isTransitioning = false;
        }, 100);
    }, 300);
}

// Update movie content function
function updateMovieContent(movie) {
    if (!movie) return;
    
    document.getElementById('movieTitle').textContent = movie.title || '';
    document.getElementById('movieTitleVn').textContent = movie.titleVn || '';
    document.getElementById('moviePlot').textContent = movie.plot || '';
    document.getElementById('movieGenre').textContent = movie.genre || '';
    document.getElementById('duration').textContent = movie.duration || '';
}

// Update initialization
function initializeCarousel() {
    const heroSection = document.getElementById('heroSection');
    const movieContent = document.getElementById('movieContent');
    
    // Create video layers instead of background layers
    createVideoLayers();
    
    // Add transition styles
    if (!movieContent.style.transition) {
        movieContent.style.transition = 'opacity 0.5s ease-in-out, transform 0.5s ease-in-out';
    }
}

// Banner movie variables
let bannerMovies = [];
let currentBannerIndex = 0;
let bannerInterval;
let isBannerTransitioning = false;

// Initialize banner movies
function initializeBannerMovies() {
    // Get featured movies data from the model
    if (typeof featuredMoviesData !== 'undefined' && featuredMoviesData.length > 0) {
        bannerMovies = featuredMoviesData;
        startBannerCarousel();
        
        // Add click handlers for banner dots
        document.addEventListener('click', function(e) {
            if (e.target.closest('#bannerPaginationDots .dot')) {
                if (isBannerTransitioning) return;
                
                const dots = Array.from(document.querySelectorAll('#bannerPaginationDots .dot'));
                const newIndex = dots.indexOf(e.target);
                
                if (newIndex !== currentBannerIndex && newIndex !== -1) {
                    const direction = newIndex > currentBannerIndex ? 'next' : 'prev';
                    currentBannerIndex = newIndex;
                    updateBannerDisplay(direction);
                    updateBannerPaginationDots();
                    resetBannerCarouselTimer();
                }
            }
        });
    }
}

function nextBannerMovie() {
    if (isBannerTransitioning || bannerMovies.length === 0) return;
    currentBannerIndex = (currentBannerIndex + 1) % bannerMovies.length;
    updateBannerDisplay('next');
    updateBannerPaginationDots();
    resetBannerCarouselTimer();
}

function previousBannerMovie() {
    if (isBannerTransitioning || bannerMovies.length === 0) return;
    currentBannerIndex = (currentBannerIndex - 1 + bannerMovies.length) % bannerMovies.length;
    updateBannerDisplay('prev');
    updateBannerPaginationDots();
    resetBannerCarouselTimer();
}

function updateBannerDisplay(direction = 'next') {
    if (isBannerTransitioning || bannerMovies.length === 0) return;
    isBannerTransitioning = true;
    
    const movie = bannerMovies[currentBannerIndex];
    const bannerSection = document.getElementById('featuredBanner');
    const bannerInfo = document.querySelector('.banner-info');
    
    if (!bannerSection || !bannerInfo) {
        isBannerTransitioning = false;
        return;
    }
    
    // Start content fade out animation
    bannerInfo.style.opacity = '0';
    bannerInfo.style.transform = direction === 'next' ? 'translateX(-30px)' : 'translateX(30px)';
    
    // Update background
    const backgroundUrl = movie.primaryImageUrl || movie.imageUrl || (movie.images && movie.images.length > 0 ? movie.images[0].imageUrl : '');
    if (backgroundUrl) {
        bannerSection.style.backgroundImage = `url('${backgroundUrl}')`;
    }
    
    // Update content after fade out
    setTimeout(() => {
        // Update text content
        const titleElement = document.getElementById('bannerTitle');
        const titleVnElement = document.getElementById('bannerTitleVn');
        const plotElement = document.getElementById('bannerPlot');
        const genreElement = document.getElementById('bannerGenre');
        const durationElement = document.getElementById('bannerDuration');
        const detailsBtnElement = document.getElementById('bannerDetailsBtn');
        const posterImgElement = document.getElementById('bannerPosterImg');
        
        if (titleElement) titleElement.textContent = movie.title || '';
        if (titleVnElement) titleVnElement.textContent = movie.productionCompany || 'Nhà sản xuất';
        if (plotElement) plotElement.textContent = movie.content || 'Chưa có thông tin nội dung.';
        if (genreElement) genreElement.textContent = movie.genres && movie.genres.length > 0 ? movie.genres.join(', ') : 'Chưa phân loại';
        if (durationElement) durationElement.textContent = `${movie.runningTime || 0} phút`;
        if (detailsBtnElement) detailsBtnElement.href = `/Movies/Details/${movie.id}`;
        
        // Update poster image
        if (posterImgElement && backgroundUrl) {
            posterImgElement.src = backgroundUrl;
            posterImgElement.alt = movie.title || '';
        }
        
        // Fade in with slide animation
        bannerInfo.style.transform = direction === 'next' ? 'translateX(30px)' : 'translateX(-30px)';
        
        // Trigger reflow
        bannerInfo.offsetHeight;
        
        // Fade in
        bannerInfo.style.opacity = '1';
        bannerInfo.style.transform = 'translateX(0)';
        
        // Reset transition flag
        setTimeout(() => {
            isBannerTransitioning = false;
        }, 500);
        
    }, 250);
}

function updateBannerPaginationDots() {
    const dots = document.querySelectorAll('#bannerPaginationDots .dot');
    dots.forEach((dot, index) => {
        dot.classList.toggle('active', index === currentBannerIndex);
    });
}

function startBannerCarousel() {
    if (bannerMovies.length > 1) {
        bannerInterval = setInterval(nextBannerMovie, 8000); // 8 seconds for banner
    }
}

function resetBannerCarouselTimer() {
    clearInterval(bannerInterval);
    startBannerCarousel();
}

// Initialize when page loads
document.addEventListener('DOMContentLoaded', function() {
    testApiConnection();
    initializeBannerMovies();
});