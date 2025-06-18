// Movie data for carousel - lấy từ server hoặc dùng dữ liệu mặc định
const movies = window.heroMovies || [
    {
        title: "Oppenheimer 2023",
        titleVn: "Cha đẻ bom nguyên tử",
        plot: "Phim Oppenheimer kể về cuộc đời của J. Robert Oppenheimer, nhà vật lý lý thuyết người Mỹ được mệnh danh là 'cha đẻ của bom nguyên tử' vì vai trò của ông trong Dự án Manhattan - chương trình nghiên cứu và phát triển vũ khí hạt nhân đầu tiên của thế giới trong Thế chiến II.",
        genre: "Lịch sử, Tiểu sử, Chính kịch",
        duration: "3 Giờ",
        background: "https://image.tmdb.org/t/p/original/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg"
    },
    {
        title: "Avatar: The Way of Water",
        titleVn: "Avatar: Dòng Chảy Của Nước",
        plot: "Jake Sully sống cùng gia đình mới của mình trên hành tinh Pandora. Khi một mối đe dọa quen thuộc trở lại để hoàn thành những gì đã bắt đầu trước đây, Jake phải làm việc với Neytiri và quân đội của chủng tộc Na'vi để bảo vệ hành tinh của họ.",
        genre: "Hành động, Phiêu lưu, Khoa học viễn tưởng",
        duration: "3 Giờ 12 phút",
        background: "https://image.tmdb.org/t/p/original/t6HIqrRAclMCA60NsSmeqe9RmNV.jpg"
    },
    {
        title: "Top Gun: Maverick",
        titleVn: "Phi Công Siêu Đẳng: Maverick",
        plot: "Sau hơn ba thập kỷ phục vụ với tư cách là một trong những phi công hàng đầu của Hải quân, Pete 'Maverick' Mitchell đang ở nơi anh thuộc về, thúc đẩy ranh giới với tư cách là một phi công thử nghiệm dũng cảm và né tránh thăng chức trong cấp bậc sẽ khiến anh ta không được bay.",
        genre: "Hành động, Chính kịch",
        duration: "2 Giờ 11 phút",
        background: "https://image.tmdb.org/t/p/original/62HCnUTziyWcpDaBO2i1DX17ljH.jpg"
    },
    {
        title: "Killers of the Flower Moon",
        titleVn: "Kẻ Giết Người Dưới Trăng Hoa",
        plot: "Dựa trên cuốn sách bán chạy nhất của David Grann, câu chuyện về vụ giết người hàng loạt các thành viên bộ lạc Osage giàu có ở Oklahoma vào những năm 1920 và cuộc điều tra FBI sau đó dẫn đến việc thành lập FBI.",
        genre: "Tội phạm, Chính kịch, Lịch sử",
        duration: "3 Giờ 26 phút",
        background: "https://image.tmdb.org/t/p/original/dKqa850uvbNSCaQCV4Im1XlzEtQ.jpg"
    },
    {
        title: "Mission: Impossible - Dead Reckoning",
        titleVn: "Nhiệm Vụ Bất Khả Thi: Nghiệp Báo Phần Một",
        plot: "Ethan Hunt và đội IMF phải đuổi theo một vũ khí cực kỳ nguy hiểm trước khi nó rơi vào tay kẻ xấu. Với số phận của thế giới treo trên một sợi chỉ, cuộc đua sinh tử đưa Ethan và đội của anh ta vòng quanh thế giới.",
        genre: "Hành động, Phiêu lưu, Ly kỳ",
        duration: "2 Giờ 43 phút",
        background: "https://image.tmdb.org/t/p/original/NNxYkU70HPurnNCSiCjYAmacwm.jpg"
    }
];

let currentMovieIndex = 0;
let movieInterval;
let isTransitioning = false;

// Initialize page
document.addEventListener('DOMContentLoaded', function() {
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