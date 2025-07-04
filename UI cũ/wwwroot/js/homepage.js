// Dữ liệu phim cho carousel
const movies = window.heroMoviesData || window.heroMovies ||154 [
    {
        id: null,
        title: "Oppenheimer 2023",
        titleVn: "Cha đẻ bom nguyên tử",
        plot: "Phim Oppenheimer kể về cuộc đời của J. Robert Oppenheimer, nhà vật lý lý thuyết người Mỹ được mệnh danh là 'cha đẻ của bom nguyên tử' vì vai trò của ông trong Dự án Manhattan - chương trình nghiên cứu và phát triển vũ khí hạt nhân đầu tiên của thế giới trong Thế chiến II.",
        genre: "Lịch sử, Tiểu sử, Chính kịch",
        duration: "3 Giờ",
        background: "https://image.tmdb.org/t/p/original/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg",
        trailerUrl: ""
    },
    {
        id: null,
        title: "Avatar: The Way of Water",
        titleVn: "Avatar: Dòng Chảy Của Nước",
        plot: "Jake Sully sống cùng gia đình mới của mình trên hành tinh Pandora. Khi một mối đe dọa quen thuộc trở lại để hoàn thành những gì đã bắt đầu trước đây, Jake phải làm việc với Neytiri và quân đội của chủng tộc Na'vi để bảo vệ hành tinh của họ.",
        genre: "Hành động, Phiêu lưu, Khoa học viễn tưởng",
        duration: "3 Giờ 12 phút",
        background: "https://image.tmdb.org/t/p/original/t6HIqrRAclMCA60NsSmeqe9RmNV.jpg",
        trailerUrl: ""
    },
    {
        id: null,
        title: "Top Gun: Maverick",
        titleVn: "Phi Công Siêu Đẳng: Maverick",
        plot: "Sau hơn ba thập kỷ phục vụ với tư cách là một trong những phi công hàng đầu của Hải quân, Pete 'Maverick' Mitchell đang ở nơi anh thuộc về, thúc đẩy ranh giới với tư cách là một phi công thử nghiệm dũng cảm và né tránh thăng chức trong cấp bậc sẽ khiến anh ta không được bay.",
        genre: "Hành động, Chính kịch",
        duration: "2 Giờ 11 phút",
        background: "https://image.tmdb.org/t/p/original/62HCnUTziyWcpDaBO2i1DX17ljH.jpg",
        trailerUrl: ""
    },
    {
        id: null,
        title: "Killers of the Flower Moon",
        titleVn: "Kẻ Giết Người Dưới Trăng Hoa",
        plot: "Dựa trên cuốn sách bán chạy nhất của David Grann, câu chuyện về vụ giết người hàng loạt các thành viên bộ lạc Osage giàu có ở Oklahoma vào những năm 1920 và cuộc điều tra FBI sau đó dẫn đến việc thành lập FBI.",
        genre: "Tội phạm, Chính kịch, Lịch sử",
        duration: "3 Giờ 26 phút",
        background: "https://image.tmdb.org/t/p/original/dKqa850uvbNSCaQCV4Im1XlzEtQ.jpg",
        trailerUrl: ""
    },
    {
        id: null,
        title: "Mission: Impossible - Dead Reckoning",
        titleVn: "Nhiệm Vụ Bất Khả Thi: Nghiệp Báo Phần Một",
        plot: "Ethan Hunt và đội IMF phải đuổi theo một vũ khí cực kỳ nguy hiểm trước khi nó rơi vào tay kẻ xấu. Với số phận của thế giới treo trên một sợi chỉ, cuộc đua sinh tử đưa Ethan và đội của anh ta vòng quanh thế giới.",
        genre: "Hành động, Phiêu lưu, Ly kỳ",
        duration: "2 Giờ 43 phút",
        background: "https://image.tmdb.org/t/p/original/NNxYkU70HPurnNCSiCjYAmacwm.jpg",
        trailerUrl: ""
    }
];

let currentMovieIndex = 0;
let isTransitioning = false;

// Logic mới: Tự phát video đầu tiên, không tự động chuyển phim, chỉ chuyển khi user click

// Khởi tạo trang
document.addEventListener('DOMContentLoaded', function() {
    // Kiểm tra dữ liệu phim
    if (!movies || movies.length === 0) {
        const heroImage = document.getElementById('heroImage');
        if (heroImage) {
            heroImage.style.backgroundImage = "url('https://image.tmdb.org/t/p/original/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg')";
            heroImage.style.display = 'block';
            heroImage.style.opacity = '1';
            heroImage.classList.add('visible');
        }
        return;
    }
    
    initializeCarousel();
    updateMovieDisplay();
    updatePaginationDots();
    
    const userDropdown = document.getElementById('userDropdown');
    const dropdownMenu = userDropdown?.nextElementSibling;
    
    if (userDropdown && dropdownMenu) {
        userDropdown.addEventListener('click', function(e) {
            e.stopPropagation();
            dropdownMenu.classList.toggle('show');
            
            const chevron = userDropdown.querySelector('.fa-chevron-down');
            if (chevron) {
                chevron.style.transform = dropdownMenu.classList.contains('show') ? 'rotate(180deg)' : 'rotate(0)';
                chevron.style.transition = 'transform 0.3s ease';
            }
        });
        
        document.addEventListener('click', function(e) {
            if (!userDropdown.contains(e.target) && !dropdownMenu.contains(e.target)) {
                dropdownMenu.classList.remove('show');
                const chevron = userDropdown.querySelector('.fa-chevron-down');
                if (chevron) {
                    chevron.style.transform = 'rotate(0)';
                }
            }
        });
        
        dropdownMenu.addEventListener('click', function(e) {
            e.stopPropagation();
        });
    }
});

// Khởi tạo carousel với hiệu ứng mượt
function initializeCarousel() {
    const heroSection = document.getElementById('heroSection');
    const movieContent = document.getElementById('movieContent');
    
    showInitialBackground();
    createBackgroundLayers();
    
    if (!movieContent.style.transition) {
        movieContent.style.transition = 'opacity 0.5s ease-in-out, transform 0.5s ease-in-out';
    }
}

// Hiển thị background ban đầu
function showInitialBackground() {
    const heroImage = document.getElementById('heroImage');
    if (heroImage && movies.length > 0) {
        const initialMovie = movies[currentMovieIndex];
        heroImage.style.backgroundImage = `url('${initialMovie.background}')`;
        heroImage.style.display = 'block';
        heroImage.style.opacity = '0.8';
        heroImage.classList.add('visible');
    }
}

// Tạo hai layer background để hiệu ứng crossfade
function createBackgroundLayers() {
    const heroSection = document.getElementById('heroSection');
    
    const existingLayers = heroSection.querySelectorAll('.bg-layer');
    existingLayers.forEach(layer => layer.remove());
    
    const bgLayer1 = document.createElement('div');
    const bgLayer2 = document.createElement('div');
    
    bgLayer1.className = 'bg-layer bg-layer-1';
    bgLayer2.className = 'bg-layer bg-layer-2';
    
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
    
    const initialMovie = movies[currentMovieIndex];
    const gradient = 'linear-gradient(107deg, rgba(0, 0, 0, 0.00) 36.24%, rgba(14, 14, 14, 0.55) 57.42%, rgba(12, 12, 12, 0.99) 76.93%)';
    
    bgLayer1.style.background = `${gradient}, url('${initialMovie.background}') lightgray 50% / cover no-repeat`;
    bgLayer1.style.opacity = '1';
    bgLayer2.style.opacity = '0';
    
    heroSection.insertBefore(bgLayer1, heroSection.firstChild);
    heroSection.insertBefore(bgLayer2, heroSection.firstChild);
    
    const heroImage = document.getElementById('heroImage');
    if (heroImage) {
        heroImage.style.opacity = '0';
        heroImage.style.display = 'none';
    }
    
    const movieContent = document.getElementById('movieContent');
    if (movieContent) {
        movieContent.style.position = 'relative';
        movieContent.style.zIndex = '10';
        movieContent.style.opacity = '1';
        movieContent.style.transform = 'translateX(0)';
    }
    
    const movieControls = document.querySelector('.movie-controls');
    if (movieControls) {
        movieControls.style.position = 'relative';
        movieControls.style.zIndex = '10';
    }
}

// Cập nhật hiển thị phim với hỗ trợ video
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
    
    movieContent.style.opacity = '0';
    movieContent.style.transform = direction === 'next' ? 'translateX(-30px)' : 'translateX(30px)';
    
    if (movie.trailerUrl && movie.trailerUrl.trim()) {
        clearExistingVideosKeepState();
        
        loadVideo(movie.trailerUrl, () => {
            updateBackgroundLayers(movie, bgLayer1, bgLayer2, false);
        }, () => {
            const heroSection = document.getElementById('heroSection');
            heroSection.classList.remove('video-playing');
            updateBackgroundLayers(movie, bgLayer1, bgLayer2, true);
        });
    } else {
        clearExistingVideos();
        updateBackgroundLayers(movie, bgLayer1, bgLayer2, true);
    }
    
    // Cập nhật nội dung sau hiệu ứng fade
    setTimeout(() => {
        document.getElementById('movieTitle').textContent = movie.title;
        document.getElementById('movieTitleVn').textContent = movie.titleVn;
        document.getElementById('moviePlot').textContent = movie.plot;
        document.getElementById('movieGenre').textContent = movie.genre;
        document.getElementById('duration').textContent = movie.duration;
        
        movieContent.style.transform = direction === 'next' ? 'translateX(30px)' : 'translateX(-30px)';
        movieContent.offsetHeight;
        
        movieContent.style.opacity = '1';
        movieContent.style.transform = 'translateX(0)';
        
        setTimeout(() => {
            isTransitioning = false;
        }, 800);
        
    }, 250);
}

// Xóa tất cả video và chuyển về background
function clearExistingVideos() {
    const heroBackground = document.getElementById('heroBackground');
    const existingVideos = heroBackground.querySelectorAll('.hero-video');
    existingVideos.forEach(video => video.remove());
    
    const heroSection = document.getElementById('heroSection');
    heroSection.classList.remove('video-playing');
    
    const bgLayer1 = heroSection.querySelector('.bg-layer-1');
    const bgLayer2 = heroSection.querySelector('.bg-layer-2');
    
    if (bgLayer1 && bgLayer2) {
        if (bgLayer1.style.opacity === '0' && bgLayer2.style.opacity === '0') {
            if (bgLayer1.style.background) {
                bgLayer1.style.opacity = '1';
            } else if (bgLayer2.style.background) {
                bgLayer2.style.opacity = '1';
            }
        }
    }
}

// Xóa video nhưng giữ trạng thái video-playing
function clearExistingVideosKeepState() {
    const heroBackground = document.getElementById('heroBackground');
    const existingVideos = heroBackground.querySelectorAll('.hero-video');
    existingVideos.forEach(video => video.remove());
}

// Chỉ xóa video cũ, giữ video mới nhất
function clearOldVideosOnly() {
    const heroBackground = document.getElementById('heroBackground');
    const existingVideos = heroBackground.querySelectorAll('.hero-video');
    
    if (existingVideos.length > 1) {
        for (let i = 0; i < existingVideos.length - 1; i++) {
            existingVideos[i].remove();
        }
    }
}

// Kiểm tra các loại URL video
function isYouTubeUrl(url) {
    return url.includes('youtube.com') || url.includes('youtu.be');
}

function isCloudinaryEmbedUrl(url) {
    return url.includes('player.cloudinary.com/embed');
}

function isCloudinaryDirectUrl(url) {
    return url.includes('cloudinary.com') && !url.includes('player.cloudinary.com/embed');
}

function isDirectVideoUrl(url) {
    return /\.(mp4|webm|ogg|mov)(\?|$)/i.test(url);
}

// Tải video từ URL
function loadVideo(videoUrl, onSuccess, onError) {
    if (isYouTubeUrl(videoUrl)) {
        loadYouTubeVideo(videoUrl, onSuccess, onError);
    } else if (isCloudinaryEmbedUrl(videoUrl)) {
        loadCloudinaryEmbedVideo(videoUrl, onSuccess, onError);
    } else if (isCloudinaryDirectUrl(videoUrl) || isDirectVideoUrl(videoUrl)) {
        loadDirectVideo(videoUrl, onSuccess, onError);
    } else {
        onError();
    }
}

// Tải video YouTube
function loadYouTubeVideo(url, onSuccess, onError) {
    try {
        const videoId = extractYouTubeVideoId(url);
        if (!videoId) {
            onError();
            return;
        }
        
        const heroBackground = document.getElementById('heroBackground');
        const iframe = document.createElement('iframe');
        
        iframe.className = 'hero-video';
        iframe.src = `https://www.youtube.com/embed/${videoId}?autoplay=1&mute=1&loop=1&playlist=${videoId}&controls=0&showinfo=0&rel=0&iv_load_policy=3&modestbranding=1&disablekb=1&fs=0`;
        iframe.setAttribute('frameborder', '0');
        iframe.setAttribute('allow', 'autoplay; encrypted-media');
        iframe.style.pointerEvents = 'none';
        
        const overlay = document.createElement('div');
        overlay.className = 'hero-video-overlay';
        
        iframe.onload = () => {
            iframe.classList.add('loaded');
            
            const heroSection = document.getElementById('heroSection');
            heroSection.classList.add('video-playing');
            
            onSuccess();
        };
        
        iframe.onerror = () => {
            onError();
        };
        
        heroBackground.appendChild(iframe);
        heroBackground.appendChild(overlay);
        
    } catch (error) {
        onError();
    }
}

// Tải Cloudinary embed player
function loadCloudinaryEmbedVideo(url, onSuccess, onError) {
    try {
        const heroBackground = document.getElementById('heroBackground');
        const iframe = document.createElement('iframe');
        
        iframe.className = 'hero-video';
        
        const autoplayUrl = addCloudinaryAutoplayParams(url);
        iframe.src = autoplayUrl;
        
        iframe.setAttribute('frameborder', '0');
        iframe.setAttribute('allowfullscreen', 'true');
        iframe.setAttribute('allow', 'autoplay; encrypted-media; fullscreen; picture-in-picture');
        
        const overlay = document.createElement('div');
        overlay.className = 'hero-video-overlay';
        
        iframe.onload = () => {
            iframe.classList.add('loaded');
            
            forceCloudinaryFullscreen(iframe);
            
            const heroSection = document.getElementById('heroSection');
            heroSection.classList.add('video-playing');
            
            setTimeout(() => {
                try {
                    iframe.contentWindow?.postMessage({ action: 'play' }, '*');
                } catch (e) {
                    // Không thể trigger autoplay
                }
            }, 1000);
            
            onSuccess();
        };
        
        iframe.onerror = () => {
            onError();
        };
        
        setTimeout(() => {
            if (!iframe.classList.contains('loaded')) {
                iframe.classList.add('loaded');
                
                forceCloudinaryFullscreen(iframe);
                
                const heroSection = document.getElementById('heroSection');
                heroSection.classList.add('video-playing');
                
                onSuccess();
            }
        }, 3000);
        
        heroBackground.appendChild(iframe);
        heroBackground.appendChild(overlay);
        
        setTimeout(() => {
            forceCloudinaryFullscreen(iframe);
            iframe.classList.add('loaded');
            
            const heroSection = document.getElementById('heroSection');
            heroSection.classList.add('video-playing');
        }, 50);
        
    } catch (error) {
        onError();
    }
}

// Trích xuất URL video trực tiếp từ Cloudinary embed URL
function tryExtractCloudinaryDirectUrl(embedUrl) {
    try {
        const urlObj = new URL(embedUrl);
        const cloudName = urlObj.searchParams.get('cloud_name');
        const publicId = urlObj.searchParams.get('public_id');
        
        if (cloudName && publicId) {
            const directUrl = `https://res.cloudinary.com/${cloudName}/video/upload/${publicId}.mp4`;
            return directUrl;
        }
    } catch (e) {
        // Lỗi parse URL
    }
    return null;
}

// Thêm tham số autoplay vào Cloudinary URL
function addCloudinaryAutoplayParams(url) {
    try {
        const urlObj = new URL(url);
        
        urlObj.searchParams.set('autoplay', 'true');
        urlObj.searchParams.set('muted', 'true');
        urlObj.searchParams.set('loop', 'true');
        urlObj.searchParams.set('controls', 'false');
        
        return urlObj.toString();
    } catch (e) {
        return url;
    }
}

// Tải video trực tiếp
function loadDirectVideo(url, onSuccess, onError) {
    try {
        const heroBackground = document.getElementById('heroBackground');
        const video = document.createElement('video');
        
        video.className = 'hero-video';
        video.src = url;
        video.autoplay = true;
        video.muted = true;
        video.loop = true;
        video.playsInline = true;
        video.controls = false;
        video.style.pointerEvents = 'none';
        
        const overlay = document.createElement('div');
        overlay.className = 'hero-video-overlay';
        
        video.addEventListener('loadeddata', () => {
            video.classList.add('loaded');
            
            const heroSection = document.getElementById('heroSection');
            heroSection.classList.add('video-playing');
            
            onSuccess();
        });
        
        video.addEventListener('error', (e) => {
            onError();
        });
        
        video.load();
        
        heroBackground.appendChild(video);
        heroBackground.appendChild(overlay);
        
    } catch (error) {
        onError();
    }
}

// Ép iframe Cloudinary fullscreen bằng scale
function forceCloudinaryFullscreen(iframe) {
    iframe.removeAttribute('width');
    iframe.removeAttribute('height');
    iframe.removeAttribute('style');
    
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const videoWidth = 640;
    const videoHeight = 360;
    
    const scaleX = viewportWidth / videoWidth;
    const scaleY = viewportHeight / videoHeight;
    const scale = Math.max(scaleX, scaleY);
    
    Object.assign(iframe.style, {
        position: 'absolute',
        top: '50%',
        left: '50%',
        width: '640px',
        height: '360px',
        transform: `translate(-50%, -50%) scale(${scale})`,
        transformOrigin: 'center center',
        border: 'none',
        outline: 'none',
        zIndex: '2',
        pointerEvents: 'none',
        aspectRatio: 'none !important',
        objectFit: 'cover',
        margin: '0',
        padding: '0',
        overflow: 'hidden'
    });
    
    const styleSheet = document.createElement('style');
    styleSheet.textContent = `
        iframe.hero-video[src*="cloudinary"] {
            aspect-ratio: none !important;
            width: 640px !important;
            height: 360px !important;
            transform: translate(-50%, -50%) scale(${scale}) !important;
            position: absolute !important;
            top: 50% !important;
            left: 50% !important;
            z-index: 2 !important;
            pointer-events: none !important;
            border: none !important;
            outline: none !important;
            margin: 0 !important;
            padding: 0 !important;
        }
    `;
    document.head.appendChild(styleSheet);
    
    const forceInterval = setInterval(() => {
        if (!document.contains(iframe)) {
            clearInterval(forceInterval);
            return;
        }
        
        const currentTransform = iframe.style.transform;
        if (!currentTransform.includes('scale')) {
            iframe.style.transform = `translate(-50%, -50%) scale(${scale})`;
        }
        
        if (iframe.style.aspectRatio && iframe.style.aspectRatio !== 'none') {
            iframe.style.aspectRatio = 'none';
        }
        
    }, 200);
    
    setTimeout(() => {
        clearInterval(forceInterval);
    }, 10000);
}

// Trích xuất YouTube video ID
function extractYouTubeVideoId(url) {
    const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|&v=)([^#&?]*).*/;
    const match = url.match(regExp);
    return (match && match[2].length === 11) ? match[2] : null;
}

// Cập nhật background layers
function updateBackgroundLayers(movie, bgLayer1, bgLayer2, showBackground) {
    const updateBackground = () => {
        const layer1Opacity = bgLayer1.style.opacity || '0';
        const layer2Opacity = bgLayer2.style.opacity || '0';
        
        const activeLayer = layer1Opacity === '1' ? bgLayer1 : bgLayer2;
        const inactiveLayer = layer1Opacity === '1' ? bgLayer2 : bgLayer1;
        
        const gradient = 'linear-gradient(107deg, rgba(0, 0, 0, 0.00) 36.24%, rgba(14, 14, 14, 0.55) 57.42%, rgba(12, 12, 12, 0.99) 76.93%)';
        
        const backgroundImage = movie.background || 'https://image.tmdb.org/t/p/original/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg';
        const backgroundCSS = `${gradient}, url('${backgroundImage}') lightgray 50% / cover no-repeat`;
        
        inactiveLayer.style.background = backgroundCSS;
        
        bgLayer1.style.opacity = '0';
        bgLayer2.style.opacity = '0';
        inactiveLayer.style.opacity = '1';
    };

    if (movie.background) {
        const img = new Image();
        img.onload = function() {
            updateBackground();
        };
        
        img.onerror = function() {
            updateBackground();
        };
        
        img.src = movie.background;
    } else {
        updateBackground();
    }
}

function updatePaginationDots() {
    const dots = document.querySelectorAll('.dot');
    dots.forEach((dot, index) => {
        dot.classList.toggle('active', index === currentMovieIndex);
        
        if (!dot.style.transition) {
            dot.style.transition = 'all 0.3s ease';
        }
    });
}

function nextMovie() {
    if (isTransitioning || !movies || movies.length === 0) return;
    currentMovieIndex = (currentMovieIndex + 1) % movies.length;
    updateMovieDisplay('next');
    updatePaginationDots();
}

function previousMovie() {
    if (isTransitioning || !movies || movies.length === 0) return;
    currentMovieIndex = (currentMovieIndex - 1 + movies.length) % movies.length;
    updateMovieDisplay('prev');
    updatePaginationDots();
}

// Click dot để chuyển phim
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
        }
    }
});

// Hàm đặt vé
function bookTickets() {
    const isAuthenticated = document.querySelector('.user-profile') !== null;
    
    if (!isAuthenticated) {
        if (confirm('Bạn cần đăng nhập để đặt vé. Chuyển đến trang đăng nhập?')) {
            window.location.href = '/Account/Login';
        }
        return;
    }
    
    window.location.href = '/BookingManagement/Booking/SelectMovie';
}

function showMovieInfo() {
    if (!movies || movies.length === 0) {
        window.location.href = window.movieUrls?.moviesIndex || '/Movies';
        return;
    }
    
    if (!window.movieUrls) {
        window.location.href = '/Movies';
        return;
    }
    
    const currentMovie = movies[currentMovieIndex];
    
    if (currentMovie && currentMovie.id) {
        const detailsUrl = window.movieUrls.movieDetails + '/' + currentMovie.id;
        window.location.href = detailsUrl;
    } else {
        window.location.href = window.movieUrls.moviesIndex;
    }
}

// Toggle tìm kiếm
function toggleSearch() {
    const searchForm = document.getElementById('searchForm');
    const searchToggle = document.querySelector('.search-toggle');
    
    searchForm.classList.toggle('expanded');
    searchToggle.classList.toggle('collapsed');
    
    if (!searchToggle.style.transition) {
        searchToggle.style.transition = 'transform 0.3s ease';
    }
}

// Slide phim được đề xuất
function slideMovies(direction) {
    const grid = document.getElementById('moviesGrid');
    const scrollAmount = 300;
    
    if (direction === 'left') {
        grid.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
    } else {
        grid.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    }
}

// Toggle switches
document.addEventListener('click', function(e) {
    if (e.target.closest('.toggle-switch')) {
        const toggle = e.target.closest('.toggle-switch');
        toggle.classList.toggle('active');
        
        const circle = toggle.querySelector('.toggle-circle');
        if (circle && !circle.style.transition) {
            circle.style.transition = 'transform 0.2s ease';
        }
    }
});

// Hiệu ứng hover card phim
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

// Điều khiển bằng bàn phím
document.addEventListener('keydown', function(e) {
    if (e.key === 'ArrowLeft') {
        previousMovie();
    } else if (e.key === 'ArrowRight') {
        nextMovie();
    }
});

// Hỗ trợ vuốt trên mobile
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
            nextMovie();
        } else {
            previousMovie();
        }
    }
}

// Phân trang homepage
class HomepagePagination {
    constructor() {
        this.currentPages = {
            recommended: 1,
            comingSoon: 1
        };
        
        this.init();
    }
    
    init() {
        this.bindRecommendedEvents();
        this.bindComingSoonEvents();
        this.updatePaginationInfo();
    }
    
    bindRecommendedEvents() {
        document.getElementById('recommendedSort')?.addEventListener('change', (e) => {
            this.currentPages.recommended = 1;
            this.loadRecommendedMovies();
        });
        
        document.getElementById('recommendedGenre')?.addEventListener('change', (e) => {
            this.currentPages.recommended = 1;
            this.loadRecommendedMovies();
        });
        
        document.getElementById('recommendedPageSize')?.addEventListener('change', (e) => {
            this.currentPages.recommended = 1;
            this.loadRecommendedMovies();
        });
        
        document.getElementById('loadMoreRecommended')?.addEventListener('click', (e) => {
            this.currentPages.recommended++;
            this.loadRecommendedMovies(true);
        });
    }
    
    bindComingSoonEvents() {
        document.getElementById('comingSoonSort')?.addEventListener('change', (e) => {
            this.currentPages.comingSoon = 1;
            this.loadComingSoonMovies();
        });
        
        document.getElementById('comingSoonGenre')?.addEventListener('change', (e) => {
            this.currentPages.comingSoon = 1;
            this.loadComingSoonMovies();
        });
        
        document.getElementById('comingSoonPageSize')?.addEventListener('change', (e) => {
            this.currentPages.comingSoon = 1;
            this.loadComingSoonMovies();
        });
        
        document.getElementById('loadMoreComingSoon')?.addEventListener('click', (e) => {
            this.currentPages.comingSoon++;
            this.loadComingSoonMovies(true);
        });
    }
    
    async loadRecommendedMovies(append = false) {
        const sortSelect = document.getElementById('recommendedSort');
        const genreSelect = document.getElementById('recommendedGenre');
        const pageSizeSelect = document.getElementById('recommendedPageSize');
        const pagination = document.getElementById('recommendedPagination');
        
        if (!sortSelect || !genreSelect || !pageSizeSelect) return;
        
        const [sortBy, sortOrder] = sortSelect.value.split('-');
        const genre = genreSelect.value;
        const pageSize = parseInt(pageSizeSelect.value);
        const page = this.currentPages.recommended;
        
        pagination.classList.add('loading');
        
        try {
            const params = new URLSearchParams({
                page: page,
                pageSize: pageSize,
                sortBy: sortBy,
                sortOrder: sortOrder
            });
            
            if (genre && genre !== 'all') {
                params.append('genre', genre);
            }
            
            const response = await fetch(`/Home/GetRecommendedMovies?${params}`);
            const data = await response.json();
            
            if (data.success) {
                this.updateRecommendedGrid(data.data, append);
                this.updatePaginationInfo('recommended', data.pagination);
            }
        } catch (error) {
            // Lỗi tải phim đề xuất
        } finally {
            pagination.classList.remove('loading');
        }
    }
    
    async loadComingSoonMovies(append = false) {
        const sortSelect = document.getElementById('comingSoonSort');
        const genreSelect = document.getElementById('comingSoonGenre');
        const pageSizeSelect = document.getElementById('comingSoonPageSize');
        const pagination = document.getElementById('comingSoonPagination');
        
        if (!sortSelect || !genreSelect || !pageSizeSelect) return;
        
        const [sortBy, sortOrder] = sortSelect.value.split('-');
        const genre = genreSelect.value;
        const pageSize = parseInt(pageSizeSelect.value);
        const page = this.currentPages.comingSoon;
        
        pagination.classList.add('loading');
        
        try {
            const params = new URLSearchParams({
                page: page,
                pageSize: pageSize,
                sortBy: sortBy,
                sortOrder: sortOrder
            });
            
            if (genre && genre !== 'all') {
                params.append('genre', genre);
            }
            
            const response = await fetch(`/Home/GetComingSoonMovies?${params}`);
            const data = await response.json();
            
            if (data.success) {
                this.updateComingSoonGrid(data.data, append);
                this.updatePaginationInfo('comingSoon', data.pagination);
            }
        } catch (error) {
            // Lỗi tải phim sắp ra mắt
        } finally {
            pagination.classList.remove('loading');
        }
    }
    
    updateRecommendedGrid(movies, append = false) {
        const grid = document.querySelector('.recommended-grid');
        if (!grid) return;
        
        if (!append) {
            grid.innerHTML = '';
        }
        
        movies.forEach(movie => {
            const movieElement = this.createRecommendedMovieElement(movie);
            grid.appendChild(movieElement);
        });
    }
    
    updateComingSoonGrid(movies, append = false) {
        const list = document.querySelector('.coming-soon-list');
        if (!list) return;
        
        if (!append) {
            list.innerHTML = '';
        }
        
        movies.forEach(movie => {
            const movieElement = this.createComingSoonMovieElement(movie);
            list.appendChild(movieElement);
        });
    }
    
    createRecommendedMovieElement(movie) {
        const div = document.createElement('div');
        div.className = 'recommended-item';
        div.innerHTML = `
            <div class="recommended-poster">
                <img src="${movie.primaryImageUrl || movie.imageUrl || 'https://via.placeholder.com/300x450/1a1a1a/ffffff?text=No+Image'}" 
                     alt="${movie.title}" loading="lazy" 
                     onerror="this.src='https://via.placeholder.com/300x450/1a1a1a/ffffff?text=No+Image'">
                <div class="recommended-overlay">
                    <a href="/Movies/Details/${movie.id}" class="recommended-view-btn">
                        <i class="fas fa-info-circle"></i>
                        Xem Chi Tiết
                    </a>
                </div>
                <div class="recommended-rating">
                    <i class="fas fa-star"></i> ${movie.rating.toFixed(1)}
                </div>
            </div>
            <div class="recommended-info">
                <span class="recommended-genre">
                    ${movie.genres && movie.genres.length > 0 ? movie.genres[0].name : 'Chưa phân loại'}
                </span>
                <h3 class="recommended-title">${movie.title}</h3>
                <div class="recommended-meta">
                    <div class="recommended-duration">
                        <i class="fas fa-clock"></i>
                        <span>${movie.runningTime} phút</span>
                    </div>
                    <span>${new Date(movie.releaseDate).getFullYear()}</span>
                </div>
            </div>
        `;
        return div;
    }
    
    createComingSoonMovieElement(movie) {
        const div = document.createElement('div');
        div.className = 'coming-soon-item';
        div.innerHTML = `
            <div class="coming-soon-poster">
                <img src="${movie.primaryImageUrl || movie.imageUrl || 'https://via.placeholder.com/300x450/1a1a1a/ffffff?text=No+Image'}" 
                     alt="${movie.title}" loading="lazy"
                     onerror="this.src='https://via.placeholder.com/300x450/1a1a1a/ffffff?text=No+Image'">
                <div class="coming-soon-overlay">
                    <a href="/Movies/Details/${movie.id}" class="coming-soon-view-btn">
                        <i class="fas fa-info-circle"></i>
                        Xem Chi Tiết
                    </a>
                </div>
                <div class="coming-soon-release">${new Date(movie.releaseDate).toLocaleDateString('vi-VN')}</div>
                ${movie.rating > 0 ? `
                    <div class="coming-soon-rating">
                        <i class="fas fa-star"></i> ${movie.rating.toFixed(1)}
                    </div>
                ` : ''}
            </div>
            <div class="coming-soon-info">
                <h3 class="coming-soon-title">${movie.title}</h3>
                <div class="coming-soon-meta">
                    <span>${movie.genres && movie.genres.length > 0 ? movie.genres[0].name : 'Chưa phân loại'}</span>
                    <span>${movie.runningTime} phút</span>
                </div>
            </div>
        `;
        return div;
    }
    
    updatePaginationInfo(section = null, pagination = null) {
        if (section && pagination) {
            const infoElement = document.getElementById(`${section}Info`);
            const loadMoreBtn = document.getElementById(`loadMore${section.charAt(0).toUpperCase() + section.slice(1)}`);
            
            if (infoElement) {
                const infoContent = infoElement.querySelector('.info-content span');
                if (infoContent) {
                    infoContent.textContent = `Trang ${pagination.currentPage} / ${pagination.totalPages} (${pagination.totalItems} phim)`;
                } else {
                    infoElement.textContent = `Trang ${pagination.currentPage} / ${pagination.totalPages} (${pagination.totalItems} phim)`;
                }
            }
            
            if (loadMoreBtn) {
                loadMoreBtn.style.display = pagination.hasNextPage ? 'flex' : 'none';
            }
        }
    }
}

// Khởi tạo phân trang khi DOM sẵn sàng
document.addEventListener('DOMContentLoaded', function() {
    setTimeout(() => {
        if (document.getElementById('recommendedPagination') && 
            document.getElementById('comingSoonPagination')) {
            new HomepagePagination();
        }
    }, 500);
});