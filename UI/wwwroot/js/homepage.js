// Movie data for carousel - lấy từ server hoặc dùng dữ liệu mặc định
const movies = window.heroMoviesData || window.heroMovies || [
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
// let movieInterval; // REMOVED: No auto-transition
let isTransitioning = false;

/*
NEW LOGIC:
- Homepage sẽ tự phát video đầu tiên và loop
- Nếu video không load được → hiển thị background image
- KHÔNG tự động chuyển phim
- Chỉ chuyển phim khi user bấm buttons hoặc dots
- Data lấy từ API (đã có fallback nếu API fail)
*/

// Initialize page
document.addEventListener('DOMContentLoaded', function() {
    console.log('Movies data loaded:', movies);
    
    // Kiểm tra nếu không có data movies
    if (!movies || movies.length === 0) {
        console.warn('No movies data available, using fallback');
        // Hiển thị hero image fallback nếu không có movies data
        const heroImage = document.getElementById('heroImage');
        if (heroImage) {
            heroImage.style.backgroundImage = "url('https://image.tmdb.org/t/p/original/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg')";
            heroImage.style.display = 'block';
            heroImage.style.opacity = '1';
            heroImage.classList.add('visible');
        }
        return; // Không khởi tạo carousel nếu không có data
    }
    
    initializeCarousel();
    updateMovieDisplay();
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
    
    // Đảm bảo có sẵn background ngay từ đầu
    showInitialBackground();
    
    // Create background layers for smooth transitions
    createBackgroundLayers();
    
    // Add transition styles if not already present
    if (!movieContent.style.transition) {
        movieContent.style.transition = 'opacity 0.5s ease-in-out, transform 0.5s ease-in-out';
    }
}

// Hiển thị background ban đầu để tránh màn hình đen
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
    
    // Ẩn hero-image vì đã có background layers
    const heroImage = document.getElementById('heroImage');
    if (heroImage) {
        heroImage.style.opacity = '0';
        heroImage.style.display = 'none';
    }
    
    // Ensure movie content is above background layers
    const movieContent = document.getElementById('movieContent');
    if (movieContent) {
        movieContent.style.position = 'relative';
        movieContent.style.zIndex = '10';
        movieContent.style.opacity = '1';
        movieContent.style.transform = 'translateX(0)';
    }
    
    // Ensure movie controls are above background layers
    const movieControls = document.querySelector('.movie-controls');
    if (movieControls) {
        movieControls.style.position = 'relative';
        movieControls.style.zIndex = '10';
    }
}

// Enhanced movie display with video support
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
    
    // 🎥 VIDEO-FIRST APPROACH: Clear và load ngay, giữ video-playing state
    
    if (movie.trailerUrl && movie.trailerUrl.trim()) {
        console.log('🎥 [VIDEO-FIRST] Loading video for:', movie.title);
        
        // Clear video cũ NHƯNG GIỮ video-playing class để không flash background
        clearExistingVideosKeepState();
        
        // Load video mới ngay lập tức
        loadVideo(movie.trailerUrl, () => {
            // ✅ Video loaded successfully
            console.log('🎥 [VIDEO-FIRST] ✅ Video loaded successfully');
            // Update background trong background (không visible vì có video)
            updateBackgroundLayers(movie, bgLayer1, bgLayer2, false);
            
        }, () => {
            // ❌ Video failed to load
            console.log('🎥 [VIDEO-FIRST] ❌ Video failed - falling back to background');
            // Remove video-playing class và hiển thị background
            const heroSection = document.getElementById('heroSection');
            heroSection.classList.remove('video-playing');
            updateBackgroundLayers(movie, bgLayer1, bgLayer2, true);
        });
    } else {
        // Không có video URL - clear video và hiển thị background
        console.log('🎥 [VIDEO-FIRST] No video URL - showing background only');
        clearExistingVideos();
        updateBackgroundLayers(movie, bgLayer1, bgLayer2, true);
    }
    
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

// Clear existing videos (full clear - for fallback to background)
function clearExistingVideos() {
    const heroBackground = document.getElementById('heroBackground');
    const existingVideos = heroBackground.querySelectorAll('.hero-video');
    existingVideos.forEach(video => video.remove());
    
    const heroSection = document.getElementById('heroSection');
    heroSection.classList.remove('video-playing');
    
    console.log('🗑️ [VIDEO CLEAR] Removed', existingVideos.length, 'videos, video-playing class removed');
    console.log('🗑️ [VIDEO CLEAR] Hero section classes:', heroSection.className);
    
    // Đảm bảo background layers hiển thị lại
    const bgLayer1 = heroSection.querySelector('.bg-layer-1');
    const bgLayer2 = heroSection.querySelector('.bg-layer-2');
    
    if (bgLayer1 && bgLayer2) {
        // Nếu có ít nhất 1 layer đang hiển thị thì OK
        if (bgLayer1.style.opacity === '0' && bgLayer2.style.opacity === '0') {
            // Cả 2 layer đều bị ẩn, hiển thị layer có background
            if (bgLayer1.style.background) {
                bgLayer1.style.opacity = '1';
                console.log('🗑️ [VIDEO CLEAR] Restored bgLayer1 opacity to 1');
            } else if (bgLayer2.style.background) {
                bgLayer2.style.opacity = '1';
                console.log('🗑️ [VIDEO CLEAR] Restored bgLayer2 opacity to 1');
            }
        }
    }
    
    console.log('🗑️ [VIDEO CLEAR] Background layers restored, should be visible now');
}

// Clear videos but KEEP video-playing state (prevents background flash)
function clearExistingVideosKeepState() {
    const heroBackground = document.getElementById('heroBackground');
    const existingVideos = heroBackground.querySelectorAll('.hero-video');
    existingVideos.forEach(video => video.remove());
    
    // GIỮ NGUYÊN video-playing class để background không hiện lên
    const heroSection = document.getElementById('heroSection');
    // KHÔNG remove video-playing class
    
    console.log('🗑️ [VIDEO CLEAR KEEP STATE] Removed', existingVideos.length, 'videos, kept video-playing class');
    console.log('🗑️ [VIDEO CLEAR KEEP STATE] Hero section classes:', heroSection.className);
}

// Clear only old videos (keep new video, keep video-playing state)  
function clearOldVideosOnly() {
    const heroBackground = document.getElementById('heroBackground');
    const existingVideos = heroBackground.querySelectorAll('.hero-video');
    
    // Chỉ xóa video cũ (không phải video mới nhất)
    if (existingVideos.length > 1) {
        // Remove all except the last one (newest video)
        for (let i = 0; i < existingVideos.length - 1; i++) {
            existingVideos[i].remove();
            console.log('🗑️ [OLD VIDEO CLEAR] Removed old video', i + 1);
        }
    }
    
    // GIỮ NGUYÊN video-playing class vì vẫn có video
    console.log('🗑️ [OLD VIDEO CLEAR] Kept newest video, video-playing state maintained');
}

// Check if URL is YouTube
function isYouTubeUrl(url) {
    return url.includes('youtube.com') || url.includes('youtu.be');
}

// Check if URL is Cloudinary embed player
function isCloudinaryEmbedUrl(url) {
    return url.includes('player.cloudinary.com/embed');
}

// Check if URL is Cloudinary direct video
function isCloudinaryDirectUrl(url) {
    return url.includes('cloudinary.com') && !url.includes('player.cloudinary.com/embed');
}

// Check if URL is direct video file
function isDirectVideoUrl(url) {
    return /\.(mp4|webm|ogg|mov)(\?|$)/i.test(url);
}

// Load video from URL (YouTube, Cloudinary embed, or direct video)
function loadVideo(videoUrl, onSuccess, onError) {
    console.log('🎥 [LOAD] Attempting to load video:', videoUrl);
    
    if (isYouTubeUrl(videoUrl)) {
        loadYouTubeVideo(videoUrl, onSuccess, onError);
    } else if (isCloudinaryEmbedUrl(videoUrl)) {
        loadCloudinaryEmbedVideo(videoUrl, onSuccess, onError);
    } else if (isCloudinaryDirectUrl(videoUrl) || isDirectVideoUrl(videoUrl)) {
        loadDirectVideo(videoUrl, onSuccess, onError);
    } else {
        console.warn('❌ [LOAD] Unsupported video URL format:', videoUrl);
        onError();
    }
}

// Load YouTube video
function loadYouTubeVideo(url, onSuccess, onError) {
    try {
        const videoId = extractYouTubeVideoId(url);
        if (!videoId) {
            console.error('Could not extract YouTube video ID from:', url);
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
        
        // Add video overlay for text readability
        const overlay = document.createElement('div');
        overlay.className = 'hero-video-overlay';
        
        iframe.onload = () => {
            console.log('YouTube video loaded successfully');
            iframe.classList.add('loaded');
            
            const heroSection = document.getElementById('heroSection');
            heroSection.classList.add('video-playing');
            
            onSuccess();
        };
        
        iframe.onerror = () => {
            console.error('Failed to load YouTube video');
            onError();
        };
        
        heroBackground.appendChild(iframe);
        heroBackground.appendChild(overlay);
        
    } catch (error) {
        console.error('Error loading YouTube video:', error);
        onError();
    }
}

// Load Cloudinary embed player
function loadCloudinaryEmbedVideo(url, onSuccess, onError) {
    try {
        console.log('Loading Cloudinary embed video:', url);
        
        // Tạm thời tắt convert sang direct URL vì không hoạt động
        // const directVideoUrl = tryExtractCloudinaryDirectUrl(url);
        // if (directVideoUrl) {
        //     console.log('Converting to direct video URL:', directVideoUrl);
        //     loadDirectVideo(directVideoUrl, onSuccess, onError);
        //     return;
        // }
        
        const heroBackground = document.getElementById('heroBackground');
        const iframe = document.createElement('iframe');
        
        iframe.className = 'hero-video';
        
        // Thêm autoplay parameters vào Cloudinary URL
        const autoplayUrl = addCloudinaryAutoplayParams(url);
        iframe.src = autoplayUrl;
        
        iframe.setAttribute('frameborder', '0');
        iframe.setAttribute('allowfullscreen', 'true');
        iframe.setAttribute('allow', 'autoplay; encrypted-media; fullscreen; picture-in-picture');
        
        // KHÔNG set style inline để tránh conflict với CSS
        // iframe.style.pointerEvents = 'none';
        
        // Add video overlay for text readability
        const overlay = document.createElement('div');
        overlay.className = 'hero-video-overlay';
        
        iframe.onload = () => {
            console.log('🎬 Cloudinary embed video loaded successfully');
            iframe.classList.add('loaded');
            
            // Debug: Log iframe attributes before forcing fullscreen
            console.log('Before fullscreen - Width:', iframe.width, 'Height:', iframe.height);
            console.log('Before fullscreen - Style:', iframe.getAttribute('style'));
            
            // FORCE REMOVE Cloudinary inline styles để fullscreen
            forceCloudinaryFullscreen(iframe);
            
            // Debug: Log iframe attributes after forcing fullscreen
            setTimeout(() => {
                console.log('After fullscreen - Style:', iframe.getAttribute('style'));
                console.log('After fullscreen - Transform:', iframe.style.transform);
            }, 100);
            
            const heroSection = document.getElementById('heroSection');
            heroSection.classList.add('video-playing');
            
            // Try to trigger autoplay programmatically after a short delay
            setTimeout(() => {
                try {
                    // Send message to iframe to start playing
                    iframe.contentWindow?.postMessage({ action: 'play' }, '*');
                } catch (e) {
                    console.log('Could not trigger autoplay programmatically');
                }
            }, 1000);
            
            onSuccess();
        };
        
        iframe.onerror = () => {
            console.error('Failed to load Cloudinary embed video');
            onError();
        };
        
        // Set timeout as fallback for onload event
        setTimeout(() => {
            if (!iframe.classList.contains('loaded')) {
                console.log('Cloudinary embed video assumed loaded (timeout)');
                iframe.classList.add('loaded');
                
                // FORCE fullscreen even with timeout fallback
                forceCloudinaryFullscreen(iframe);
                
                const heroSection = document.getElementById('heroSection');
                heroSection.classList.add('video-playing');
                
                onSuccess();
            }
        }, 3000);
        
        heroBackground.appendChild(iframe);
        heroBackground.appendChild(overlay);
        
        // 🔧 FORCE fullscreen NGAY sau khi append iframe
        // Không đợi onload vì có thể bị cross-origin block
        setTimeout(() => {
            console.log('🚀 Forcing Cloudinary fullscreen immediately...');
            forceCloudinaryFullscreen(iframe);
            iframe.classList.add('loaded');
            
            const heroSection = document.getElementById('heroSection');
            heroSection.classList.add('video-playing');
        }, 50); // 50ms để iframe render
        
    } catch (error) {
        console.error('Error loading Cloudinary embed video:', error);
        onError();
    }
}

// Try to extract direct video URL from Cloudinary embed URL
function tryExtractCloudinaryDirectUrl(embedUrl) {
    try {
        console.log('Parsing Cloudinary embed URL:', embedUrl);
        const urlObj = new URL(embedUrl);
        const cloudName = urlObj.searchParams.get('cloud_name');
        const publicId = urlObj.searchParams.get('public_id');
        
        console.log('Extracted params - cloud_name:', cloudName, 'public_id:', publicId);
        
        if (cloudName && publicId) {
            // Convert to direct video URL
            const directUrl = `https://res.cloudinary.com/${cloudName}/video/upload/${publicId}.mp4`;
            console.log('Generated direct video URL:', directUrl);
            return directUrl;
        } else {
            console.log('Missing required parameters for direct URL conversion');
        }
    } catch (e) {
        console.error('Error parsing Cloudinary embed URL:', e);
    }
    return null;
}

// Add autoplay parameters to Cloudinary URL
function addCloudinaryAutoplayParams(url) {
    try {
        const urlObj = new URL(url);
        
        // Add autoplay parameters
        urlObj.searchParams.set('autoplay', 'true');
        urlObj.searchParams.set('muted', 'true');
        urlObj.searchParams.set('loop', 'true');
        urlObj.searchParams.set('controls', 'false');
        
        return urlObj.toString();
    } catch (e) {
        console.warn('Could not parse Cloudinary URL, using original:', url);
        return url;
    }
}

// Load direct video (Cloudinary, etc.)
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
        
        // Add video overlay for text readability
        const overlay = document.createElement('div');
        overlay.className = 'hero-video-overlay';
        
        video.addEventListener('loadeddata', () => {
            console.log('Direct video loaded successfully');
            video.classList.add('loaded');
            
            const heroSection = document.getElementById('heroSection');
            heroSection.classList.add('video-playing');
            
            onSuccess();
        });
        
        video.addEventListener('error', (e) => {
            console.error('Failed to load direct video:', e);
            onError();
        });
        
        // Start loading
        video.load();
        
        heroBackground.appendChild(video);
        heroBackground.appendChild(overlay);
        
    } catch (error) {
        console.error('Error loading direct video:', error);
        onError();
    }
}

// Force Cloudinary iframe to fullscreen using scale approach
function forceCloudinaryFullscreen(iframe) {
    console.log('Forcing Cloudinary fullscreen with scale approach...');
    
    // Remove all existing attributes
    iframe.removeAttribute('width');
    iframe.removeAttribute('height');
    iframe.removeAttribute('style');
    
    // Calculate scale needed to fill viewport
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const videoWidth = 640; // Cloudinary default
    const videoHeight = 360; // Cloudinary default
    
    const scaleX = viewportWidth / videoWidth;
    const scaleY = viewportHeight / videoHeight;
    const scale = Math.max(scaleX, scaleY); // Use larger scale to cover entirely
    
    console.log(`Viewport: ${viewportWidth}x${viewportHeight}, Scale: ${scale}`);
    
    // Apply aggressive fullscreen styles
    Object.assign(iframe.style, {
        position: 'absolute',
        top: '50%',
        left: '50%',
        width: '640px', // Keep original size for scaling calculation
        height: '360px',
        transform: `translate(-50%, -50%) scale(${scale})`,
        transformOrigin: 'center center',
        border: 'none',
        outline: 'none',
        zIndex: '2',
        pointerEvents: 'none',
        // Force override any aspect ratio
        aspectRatio: 'none !important',
        objectFit: 'cover',
        // Ensure no margins/padding
        margin: '0',
        padding: '0',
        // Prevent any overflow
        overflow: 'hidden'
    });
    
    // Add !important rules via CSS text
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
    
    // Continuously force remove any Cloudinary overrides
    const forceInterval = setInterval(() => {
        // Check if iframe still exists
        if (!document.contains(iframe)) {
            clearInterval(forceInterval);
            return;
        }
        
        // Re-apply transform if it gets reset
        const currentTransform = iframe.style.transform;
        if (!currentTransform.includes('scale')) {
            iframe.style.transform = `translate(-50%, -50%) scale(${scale})`;
            console.log('Re-applied scale transform');
        }
        
        // Remove any aspect-ratio that gets re-added
        if (iframe.style.aspectRatio && iframe.style.aspectRatio !== 'none') {
            iframe.style.aspectRatio = 'none';
            console.log('Removed aspect-ratio override');
        }
        
    }, 200); // Check every 200ms for 10 seconds
    
    // Stop forcing after 10 seconds
    setTimeout(() => {
        clearInterval(forceInterval);
        console.log('Stopped Cloudinary fullscreen forcing');
    }, 10000);
    
    console.log('Cloudinary scale fullscreen applied');
}

// Extract YouTube video ID from various URL formats
function extractYouTubeVideoId(url) {
    const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|&v=)([^#&?]*).*/;
    const match = url.match(regExp);
    return (match && match[2].length === 11) ? match[2] : null;
}

// Update background layers - LUÔN update background cho mỗi phim
function updateBackgroundLayers(movie, bgLayer1, bgLayer2, showBackground) {
    console.log('🎬 [BACKGROUND UPDATE] Starting for movie:', movie.title, 'Background URL:', movie.background);
    console.log('🎬 [LAYER CHECK] Layer1 exists:', !!bgLayer1, 'Layer2 exists:', !!bgLayer2);
    
    // Luôn update background image, bất kể có video hay không
    const updateBackground = () => {
        // Determine which layer is currently visible
        const layer1Opacity = bgLayer1.style.opacity || '0';
        const layer2Opacity = bgLayer2.style.opacity || '0';
        
        console.log('🔍 Layer opacities - Layer1:', layer1Opacity, 'Layer2:', layer2Opacity);
        
        const activeLayer = layer1Opacity === '1' ? bgLayer1 : bgLayer2;
        const inactiveLayer = layer1Opacity === '1' ? bgLayer2 : bgLayer1;
        
        console.log('🔄 Switching - Active layer:', activeLayer === bgLayer1 ? 'Layer1' : 'Layer2', 
                    'Inactive layer:', inactiveLayer === bgLayer1 ? 'Layer1' : 'Layer2');
        
        // Set new background on inactive layer
        const gradient = 'linear-gradient(107deg, rgba(0, 0, 0, 0.00) 36.24%, rgba(14, 14, 14, 0.55) 57.42%, rgba(12, 12, 12, 0.99) 76.93%)';
        
        // Sử dụng background image của phim hiện tại
        const backgroundImage = movie.background || 'https://image.tmdb.org/t/p/original/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg';
        const backgroundCSS = `${gradient}, url('${backgroundImage}') lightgray 50% / cover no-repeat`;
        
        inactiveLayer.style.background = backgroundCSS;
        
        console.log('🎨 Background updated to:', backgroundImage);
        console.log('🎨 CSS applied:', backgroundCSS.substring(0, 100) + '...');
        
        // FORCE IMMEDIATE background change (no crossfade for now)
        console.log('🔀 FORCE immediate background change...');
        bgLayer1.style.opacity = '0';
        bgLayer2.style.opacity = '0';
        inactiveLayer.style.opacity = '1';
        console.log('🔀 FORCED background change completed - Active layer:', inactiveLayer === bgLayer1 ? 'Layer1' : 'Layer2');
    };

    // Preload new background image
    if (movie.background) {
        const img = new Image();
        img.onload = function() {
            console.log('✅ Background image preloaded successfully:', movie.background);
            updateBackground();
        };
        
        img.onerror = function() {
            console.warn('❌ Failed to load background image, using fallback:', movie.background);
            // Vẫn update background với fallback image
            updateBackground();
        };
        
        console.log('🔄 Preloading background image:', movie.background);
        img.src = movie.background;
    } else {
        // Nếu không có background, sử dụng fallback ngay lập tức
        console.warn('⚠️ No background URL, using fallback');
        updateBackground();
    }
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
        }
    }
});

// Action button functions
function bookTickets() {
    console.log('🎫 [BOOK TICKETS] Function called');
    
    if (!movies || movies.length === 0) {
        console.warn('🎫 No movies data available, redirecting to movies page');
        window.location.href = window.movieUrls?.moviesIndex || '/Movies';
        return;
    }
    
    if (!window.movieUrls) {
        console.warn('🎫 No movie URLs configured, using fallback');
        window.location.href = '/Movies';
        return;
    }
    
    const currentMovie = movies[currentMovieIndex];
    console.log('🎫 Current movie:', currentMovie?.title, 'ID:', currentMovie?.id);
    
    if (currentMovie && currentMovie.id) {
        // Chuyển đến trang chi tiết phim để đặt vé
        const detailsUrl = window.movieUrls.movieDetails + '/' + currentMovie.id;
        console.log('🎫 Redirecting to movie details:', detailsUrl);
        window.location.href = detailsUrl;
    } else {
        // Fallback - đi đến trang tất cả phim
        console.log('🎫 No movie ID, redirecting to movies index');
        window.location.href = window.movieUrls.moviesIndex;
    }
}

function showMovieInfo() {
    console.log('ℹ️ [MOVIE INFO] Function called');
    
    if (!movies || movies.length === 0) {
        console.warn('ℹ️ No movies data available, redirecting to movies page');
        window.location.href = window.movieUrls?.moviesIndex || '/Movies';
        return;
    }
    
    if (!window.movieUrls) {
        console.warn('ℹ️ No movie URLs configured, using fallback');
        window.location.href = '/Movies';
        return;
    }
    
    const currentMovie = movies[currentMovieIndex];
    console.log('ℹ️ Current movie:', currentMovie?.title, 'ID:', currentMovie?.id);
    
    if (currentMovie && currentMovie.id) {
        // Chuyển đến trang chi tiết phim
        const detailsUrl = window.movieUrls.movieDetails + '/' + currentMovie.id;
        console.log('ℹ️ Redirecting to movie details:', detailsUrl);
        window.location.href = detailsUrl;
    } else {
        // Fallback - đi đến trang tất cả phim
        console.log('ℹ️ No movie ID, redirecting to movies index');
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

// NO AUTO-CAROUSEL: Remove hover events since we don't have auto-transition
// document.getElementById('heroSection').addEventListener('mouseenter', function() {
//     clearInterval(movieInterval);
// });

// document.getElementById('heroSection').addEventListener('mouseleave', function() {
//     if (!isTransitioning) {
//         startMovieCarousel();
//     }
// });

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