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

let isTransitioning = false;

// Cấu hình cho video liên tục
const CONTINUOUS_VIDEO_URL = "https://your-domain.com/videos/hero-continuous.mp4"; // Video dài ghép nối
const MOVIE_SEGMENT_DURATION = 10000; // 10 giây cho mỗi phim
const VIDEO_PLAY_DURATION = 10000; // 10 giây (10000ms)
const VIDEO_PRELOAD_TIME = 2000; // Preload video trước 2 giây
let videoTimer = null;
let preloadTimer = null;
let isAutoPlaying = true; // Flag để bật/tắt auto play


let continuousVideoElement = null;
let movieSyncTimer = null;
let currentVideoTime = 0;
let TOTAL_VIDEO_DURATION = 0; // Will be calculated based on movies length

/*
NEW LOGIC:
- Homepage sẽ tự phát video đầu tiên và loop
- Nếu video không load được → hiển thị background image
- KHÔNG tự động chuyển phim
- Chỉ chuyển phim khi user bấm buttons hoặc dots
- Data lấy từ API (đã có fallback nếu API fail)
*/


document.addEventListener('DOMContentLoaded', function () {
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


    TOTAL_VIDEO_DURATION = movies.length * MOVIE_SEGMENT_DURATION;

    // Khởi tạo continuous video thay vì carousel thông thường
    if (CONTINUOUS_VIDEO_URL && CONTINUOUS_VIDEO_URL !== "https://your-domain.com/videos/hero-continuous.mp4") {
        initializeContinuousVideo();
    } else {

        initializeIndividualVideos();
    }

    updatePaginationDots();

    const userDropdown = document.getElementById('userDropdown');
    const dropdownMenu = userDropdown?.nextElementSibling;

    if (userDropdown && dropdownMenu) {

        userDropdown.addEventListener('click', function (e) {
            e.stopPropagation();
            dropdownMenu.classList.toggle('show');


            const chevron = userDropdown.querySelector('.fa-chevron-down');
            if (chevron) {
                chevron.style.transform = dropdownMenu.classList.contains('show') ? 'rotate(180deg)' : 'rotate(0)';
                chevron.style.transition = 'transform 0.3s ease';
            }
        });


        document.addEventListener('click', function (e) {
            if (!userDropdown.contains(e.target) && !dropdownMenu.contains(e.target)) {
                dropdownMenu.classList.remove('show');
                const chevron = userDropdown.querySelector('.fa-chevron-down');
                if (chevron) {
                    chevron.style.transform = 'rotate(0)';
                }
            }
        });


        dropdownMenu.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    }
});


function initializeCarousel() {
    const heroSection = document.getElementById('heroSection');
    const movieContent = document.getElementById('movieContent');

    // Đảm bảo có sẵn background ngay từ đầu
    showInitialBackground();


    createBackgroundLayers();


    if (!movieContent.style.transition) {
        movieContent.style.transition = 'opacity 0.5s ease-in-out, transform 0.5s ease-in-out';
    }
}

// Hiển thị background ban đầu để tránh màn hình đen
function showInitialBackground() {
    // 🔧 FIX: KHÔNG hiển thị hero-image ban đầu để tránh flash
    const heroImage = document.getElementById('heroImage');
    if (heroImage) {
        heroImage.style.display = 'none';
        heroImage.style.opacity = '0';
    }
}


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
        zIndex: '2' // 🔧 FIX: Tăng z-index để hiển thị ngay
    };

    Object.assign(bgLayer1.style, layerStyles);
    Object.assign(bgLayer2.style, layerStyles);

    // 🔧 FIX: Set background NGAY LẬP TỨC
    const initialMovie = movies[currentMovieIndex];
    const gradient = 'linear-gradient(107deg, rgba(0, 0, 0, 0.00) 36.24%, rgba(14, 14, 14, 0.55) 57.42%, rgba(12, 12, 12, 0.99) 76.93%)';

    bgLayer1.style.background = `${gradient}, url('${initialMovie.background}') lightgray 50% / cover no-repeat`;
    bgLayer1.style.opacity = '1';
    bgLayer2.style.opacity = '0';

    heroSection.insertBefore(bgLayer1, heroSection.firstChild);
    heroSection.insertBefore(bgLayer2, heroSection.firstChild);

    // 🔧 FIX: KHÔNG ẩn hero-image nữa, để nó ẩn sẵn
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

    // 🔧 FIX: Clear video HOÀN TOÀN và clear timer trước khi load video mới
    if (videoTimer) {
        clearTimeout(videoTimer);
        videoTimer = null;
    }
    clearExistingVideos();

    // Update background layers NGAY LẬP TỨC
    updateBackgroundLayers(movie, bgLayer1, bgLayer2, true);


    setTimeout(() => {
        updateMovieContent(movie);


        movieContent.style.opacity = '1';
        movieContent.style.transform = 'translateX(0)';

        // 🔧 FIX: Load video với timeout ngắn hơn
        if (movie.trailerUrl && movie.trailerUrl.trim() !== '') {
            console.log('🎬 [UPDATE] Loading video for:', movie.title);
            loadVideo(movie.trailerUrl, 1000); // Giảm timeout xuống 1 giây
        } else {
            console.log('🎬 [UPDATE] No video URL, showing background only for:', movie.title);
        }


        setTimeout(() => {
            isTransitioning = false;
        }, 500);

    }, 300);
}


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


function clearOldVideosOnly() {
    const heroBackground = document.getElementById('heroBackground');
    const existingVideos = heroBackground.querySelectorAll('.hero-video');

    // Chỉ xóa video cũ (không phải video mới nhất)
    if (existingVideos.length > 1) {

        for (let i = 0; i < existingVideos.length - 1; i++) {
            existingVideos[i].remove();
            console.log('🗑️ [OLD VIDEO CLEAR] Removed old video', i + 1);
        }
    }

    // GIỮ NGUYÊN video-playing class vì vẫn có video
    console.log('🗑️ [OLD VIDEO CLEAR] Kept newest video, video-playing state maintained');
}


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


function loadVideo(videoUrl, timeout = 3000) {
    console.log('🎥 [LOAD] Attempting to load video:', videoUrl, 'with timeout:', timeout + 'ms');

    const onSuccess = () => {
        console.log('🎥 ✅ Video loaded successfully');
    };

    const onError = () => {
        console.log('🎥 ❌ Video failed - background already visible');
    };

    if (isYouTubeUrl(videoUrl)) {
        loadYouTubeVideo(videoUrl, onSuccess, onError, timeout);
    } else if (isCloudinaryEmbedUrl(videoUrl)) {
        loadCloudinaryEmbedVideo(videoUrl, onSuccess, onError, timeout);
    } else if (isCloudinaryDirectUrl(videoUrl) || isDirectVideoUrl(videoUrl)) {
        loadDirectVideo(videoUrl, onSuccess, onError, timeout);
    } else {
        console.warn('❌ [LOAD] Unsupported video URL format:', videoUrl);
        onError();
    }
}


function updateMovieContent(movie) {
    document.getElementById('movieTitle').textContent = movie.title;
    document.getElementById('movieTitleVn').textContent = movie.titleVn;
    document.getElementById('moviePlot').textContent = movie.plot;
    document.getElementById('movieGenre').textContent = movie.genre;
    document.getElementById('duration').textContent = movie.duration;
}


function loadYouTubeVideo(url, onSuccess, onError, timeout = 3000) {
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


        const overlay = document.createElement('div');
        overlay.className = 'hero-video-overlay';

        iframe.onload = () => {
            console.log('YouTube video loaded successfully');
            iframe.classList.add('loaded');

            const heroSection = document.getElementById('heroSection');
            heroSection.classList.add('video-playing');

            // Dừng video sau thời gian cấu hình
            stopVideoAfterDuration(iframe);

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


function loadCloudinaryEmbedVideo(url, onSuccess, onError, timeout = 3000) {
    try {
        console.log('Loading Cloudinary embed video:', url);

        // Tạm thời tắt convert sang direct URL vì không hoạt động







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



        const overlay = document.createElement('div');
        overlay.className = 'hero-video-overlay';

        iframe.onload = () => {
            console.log('🎬 Cloudinary embed video loaded successfully');
            iframe.classList.add('loaded');


            console.log('Before fullscreen - Width:', iframe.width, 'Height:', iframe.height);
            console.log('Before fullscreen - Style:', iframe.getAttribute('style'));

            // FORCE REMOVE Cloudinary inline styles để fullscreen
            forceCloudinaryFullscreen(iframe);


            setTimeout(() => {
                console.log('After fullscreen - Style:', iframe.getAttribute('style'));
                console.log('After fullscreen - Transform:', iframe.style.transform);
            }, 100);

            const heroSection = document.getElementById('heroSection');
            heroSection.classList.add('video-playing');


            setTimeout(() => {
                try {

                    iframe.contentWindow?.postMessage({ action: 'play' }, '*');
                } catch (e) {
                    console.log('Could not trigger autoplay programmatically');
                }
            }, 1000);

            // Dừng video sau thời gian cấu hình
            stopVideoAfterDuration(iframe);

            onSuccess();
        };

        iframe.onerror = () => {
            console.error('Failed to load Cloudinary embed video');
            onError();
        };


        setTimeout(() => {
            if (!iframe.classList.contains('loaded')) {
                console.log('Cloudinary embed video timeout - falling back to background');
                onError();
            }
        }, timeout);

        heroBackground.appendChild(iframe);
        heroBackground.appendChild(overlay);


        // Không đợi onload vì có thể bị cross-origin block
        setTimeout(() => {
            console.log('🚀 Forcing Cloudinary fullscreen immediately...');
            forceCloudinaryFullscreen(iframe);
            iframe.classList.add('loaded');

            const heroSection = document.getElementById('heroSection');
            heroSection.classList.add('video-playing');

            // Dừng video sau thời gian cấu hình (cho immediate force)
            stopVideoAfterDuration(iframe);
        }, 50); // 50ms để iframe render

    } catch (error) {
        console.error('Error loading Cloudinary embed video:', error);
        onError();
    }
}


function tryExtractCloudinaryDirectUrl(embedUrl) {
    try {
        console.log('Parsing Cloudinary embed URL:', embedUrl);
        const urlObj = new URL(embedUrl);
        const cloudName = urlObj.searchParams.get('cloud_name');
        const publicId = urlObj.searchParams.get('public_id');

        console.log('Extracted params - cloud_name:', cloudName, 'public_id:', publicId);

        if (cloudName && publicId) {

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


function addCloudinaryAutoplayParams(url) {
    try {
        const urlObj = new URL(url);


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


function loadDirectVideo(url, onSuccess, onError, timeout = 3000) {
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
            console.log('Direct video loaded successfully');
            video.classList.add('loaded');

            const heroSection = document.getElementById('heroSection');
            heroSection.classList.add('video-playing');

            // Dừng video sau thời gian cấu hình
            stopVideoAfterDuration(video);

            onSuccess();
        });

        video.addEventListener('error', (e) => {
            console.error('Failed to load direct video:', e);
            onError();
        });


        setTimeout(() => {
            if (!video.classList.contains('loaded')) {
                console.log('Direct video timeout - falling back to background');
                onError();
            }
        }, timeout);


        video.load();

        heroBackground.appendChild(video);
        heroBackground.appendChild(overlay);

    } catch (error) {
        console.error('Error loading direct video:', error);
        onError();
    }
}


function forceCloudinaryFullscreen(iframe) {
    console.log('Forcing Cloudinary fullscreen with scale approach...');


    iframe.removeAttribute('width');
    iframe.removeAttribute('height');
    iframe.removeAttribute('style');


    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const videoWidth = 640; // Cloudinary default
    const videoHeight = 360; // Cloudinary default

    const scaleX = viewportWidth / videoWidth;
    const scaleY = viewportHeight / videoHeight;
    const scale = Math.max(scaleX, scaleY); // Use larger scale to cover entirely

    console.log(`Viewport: ${viewportWidth}x${viewportHeight}, Scale: ${scale}`);


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
            console.log('Re-applied scale transform');
        }


        if (iframe.style.aspectRatio && iframe.style.aspectRatio !== 'none') {
            iframe.style.aspectRatio = 'none';
            console.log('Removed aspect-ratio override');
        }

    }, 200); // Check every 200ms for 10 seconds


    setTimeout(() => {
        clearInterval(forceInterval);
        console.log('Stopped Cloudinary fullscreen forcing');
    }, 10000);

    console.log('Cloudinary scale fullscreen applied');
}

// Enhanced function để preload video tiếp theo
function preloadNextVideo() {
    const nextIndex = (currentMovieIndex + 1) % movies.length;
    const nextMovie = movies[nextIndex];

    if (nextMovie.trailerUrl && nextMovie.trailerUrl.trim() !== '') {
        console.log('🔄 Preloading next video:', nextMovie.title);

        // Tạo video element ẩn để preload
        if (isDirectVideoUrl(nextMovie.trailerUrl) || isCloudinaryDirectUrl(nextMovie.trailerUrl)) {
            const preloadVideo = document.createElement('video');
            preloadVideo.src = nextMovie.trailerUrl;
            preloadVideo.muted = true;
            preloadVideo.preload = 'auto';
            preloadVideo.style.display = 'none';
            preloadVideo.setAttribute('data-preload', 'true');
            document.body.appendChild(preloadVideo);

            // Xóa preload video sau khi load xong
            preloadVideo.addEventListener('loadeddata', () => {
                console.log('✅ Next video preloaded:', nextMovie.title);
            });
        }
    }
}

// Enhanced stopVideoAfterDuration với preloading
function stopVideoAfterDuration(videoElement, duration = VIDEO_PLAY_DURATION) {
    // Clear timer cũ nếu có
    if (videoTimer) {
        clearTimeout(videoTimer);
    }
    if (preloadTimer) {
        clearTimeout(preloadTimer);
    }

    console.log(`⏰ Setting video timer for ${duration / 1000} seconds`);

    // Preload video tiếp theo trước khi video hiện tại kết thúc
    preloadTimer = setTimeout(() => {
        if (isAutoPlaying) {
            preloadNextVideo();
        }
    }, duration - VIDEO_PRELOAD_TIME);

    videoTimer = setTimeout(() => {
        console.log(`⏰ Video ended after ${duration / 1000} seconds`);

        if (isAutoPlaying) {
            // Tự động chuyển sang video tiếp theo
            console.log('🎬 Auto transitioning to next video');
            nextMovieSeamless();
        } else {
            // Nếu không auto play, dừng video và hiển thị background
            stopCurrentVideo(videoElement);
        }

    }, duration);
}

// Function chuyển phim mượt mà không có hiệu ứng loading
function nextMovieSeamless() {
    if (isTransitioning || !movies || movies.length === 0) return;

    isTransitioning = true;
    currentMovieIndex = (currentMovieIndex + 1) % movies.length;

    const movie = movies[currentMovieIndex];
    const movieContent = document.getElementById('movieContent');

    // Update nội dung phim ngay lập tức
    updateMovieContent(movie);
    updatePaginationDots();

    // Clear video cũ nhưng giữ video-playing state
    clearExistingVideosKeepState();

    // Load video mới ngay lập tức (đã được preload)
    if (movie.trailerUrl && movie.trailerUrl.trim() !== '') {
        console.log('🎬 Loading preloaded video for:', movie.title);
        loadVideoSeamless(movie.trailerUrl);
    } else {
        console.log('🎬 No video URL, showing background for:', movie.title);
        // Update background nếu không có video
        const heroSection = document.getElementById('heroSection');
        const bgLayer1 = heroSection.querySelector('.bg-layer-1');
        const bgLayer2 = heroSection.querySelector('.bg-layer-2');
        updateBackgroundLayers(movie, bgLayer1, bgLayer2, true);
        heroSection.classList.remove('video-playing');
    }


    setTimeout(() => {
        isTransitioning = false;
    }, 300);
}

// Function load video mượt mà (sử dụng preloaded video)
function loadVideoSeamless(videoUrl) {
    console.log('🎥 Loading seamless video:', videoUrl);

    // Kiểm tra xem có preloaded video không
    const preloadedVideo = document.querySelector(`video[src="${videoUrl}"][data-preload="true"]`);

    if (preloadedVideo) {
        console.log('✅ Using preloaded video');

        // Di chuyển preloaded video vào hero background
        const heroBackground = document.getElementById('heroBackground');
        preloadedVideo.className = 'hero-video';
        preloadedVideo.style.display = 'block';
        preloadedVideo.autoplay = true;
        preloadedVideo.loop = true;
        preloadedVideo.playsInline = true;
        preloadedVideo.controls = false;
        preloadedVideo.style.pointerEvents = 'none';
        preloadedVideo.removeAttribute('data-preload');

        heroBackground.appendChild(preloadedVideo);


        const overlay = document.createElement('div');
        overlay.className = 'hero-video-overlay';
        heroBackground.appendChild(overlay);


        const heroSection = document.getElementById('heroSection');
        heroSection.classList.add('video-playing');


        preloadedVideo.play().then(() => {
            console.log('✅ Seamless video playing');
            stopVideoAfterDuration(preloadedVideo);
        }).catch(e => {
            console.error('❌ Failed to play seamless video:', e);
            loadVideo(videoUrl, 1000); // Fallback to normal loading
        });

    } else {
        // Fallback to normal loading nếu không có preloaded video
        console.log('⚠️ No preloaded video found, using normal loading');
        loadVideo(videoUrl, 1000);
    }
}

// Function để dừng video hiện tại
function stopCurrentVideo(videoElement) {
    if (videoElement) {
        videoElement.classList.add('stopping');

        if (videoElement.tagName === 'VIDEO') {
            videoElement.pause();
            videoElement.currentTime = 0;
        } else if (videoElement.tagName === 'IFRAME') {
            try {
                videoElement.contentWindow?.postMessage({ action: 'pause' }, '*');
            } catch (e) {
                console.log('Could not pause iframe video programmatically');
            }
        }
    }

    // Remove video-playing state để hiển thị background
    const heroSection = document.getElementById('heroSection');
    heroSection.classList.remove('video-playing');
}

// Function để bật/tắt auto play
function toggleAutoPlay() {
    isAutoPlaying = !isAutoPlaying;
    console.log('🔄 Auto play:', isAutoPlaying ? 'ON' : 'OFF');

    if (!isAutoPlaying) {
        // Clear timers nếu tắt auto play
        if (videoTimer) {
            clearTimeout(videoTimer);
            videoTimer = null;
        }
        if (preloadTimer) {
            clearTimeout(preloadTimer);
            preloadTimer = null;
        }
    }
}

// Cleanup function để xóa preloaded videos
function cleanupPreloadedVideos() {
    const preloadedVideos = document.querySelectorAll('video[data-preload="true"]');
    preloadedVideos.forEach(video => {
        video.remove();
        console.log('🗑️ Cleaned up preloaded video');
    });
}


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

        const layer1Opacity = bgLayer1.style.opacity || '0';
        const layer2Opacity = bgLayer2.style.opacity || '0';

        console.log('🔍 Layer opacities - Layer1:', layer1Opacity, 'Layer2:', layer2Opacity);

        const activeLayer = layer1Opacity === '1' ? bgLayer1 : bgLayer2;
        const inactiveLayer = layer1Opacity === '1' ? bgLayer2 : bgLayer1;

        console.log('🔄 Switching - Active layer:', activeLayer === bgLayer1 ? 'Layer1' : 'Layer2',
            'Inactive layer:', inactiveLayer === bgLayer1 ? 'Layer1' : 'Layer2');


        const gradient = 'linear-gradient(107deg, rgba(0, 0, 0, 0.00) 36.24%, rgba(14, 14, 14, 0.55) 57.42%, rgba(12, 12, 12, 0.99) 76.93%)';

        // Sử dụng background image của phim hiện tại
        const backgroundImage = movie.background || 'https://image.tmdb.org/t/p/original/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg';
        const backgroundCSS = `${gradient}, url('${backgroundImage}') lightgray 50% / cover no-repeat`;

        inactiveLayer.style.background = backgroundCSS;

        console.log('🎨 Background updated to:', backgroundImage);
        console.log('🎨 CSS applied:', backgroundCSS.substring(0, 100) + '...');


        console.log('� FORCE immediate background change...');
        bgLayer1.style.opacity = '0';
        bgLayer2.style.opacity = '0';
        inactiveLayer.style.opacity = '1';
        console.log('🔀 FORCED background change completed - Active layer:', inactiveLayer === bgLayer1 ? 'Layer1' : 'Layer2');
    };


    if (movie.background) {
        const img = new Image();
        img.onload = function () {
            console.log('✅ Background image preloaded successfully:', movie.background);
            updateBackground();
        };

        img.onerror = function () {
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


        if (!dot.style.transition) {
            dot.style.transition = 'all 0.3s ease';
        }
    });
}

// Function khởi tạo video liên tục
function initializeContinuousVideo() {
    console.log('🎬 Initializing continuous video playback');

    const heroBackground = document.getElementById('heroBackground');

    // Tạo video element cho video liên tục
    continuousVideoElement = document.createElement('video');
    continuousVideoElement.className = 'hero-video continuous-video';
    continuousVideoElement.src = CONTINUOUS_VIDEO_URL;
    continuousVideoElement.autoplay = true;
    continuousVideoElement.muted = true;
    continuousVideoElement.loop = true;
    continuousVideoElement.playsInline = true;
    continuousVideoElement.controls = false;
    continuousVideoElement.style.pointerEvents = 'none';


    const overlay = document.createElement('div');
    overlay.className = 'hero-video-overlay';


    continuousVideoElement.addEventListener('loadeddata', () => {
        console.log('✅ Continuous video loaded successfully');

        const heroSection = document.getElementById('heroSection');
        heroSection.classList.add('video-playing');

        // Bắt đầu sync nội dung với video
        startMovieContentSync();
    });

    continuousVideoElement.addEventListener('timeupdate', () => {
        currentVideoTime = continuousVideoElement.currentTime * 1000; // Convert to ms
        syncMovieContent();
    });

    continuousVideoElement.addEventListener('error', (e) => {
        console.error('❌ Failed to load continuous video:', e);

        initializeIndividualVideos();
    });

    heroBackground.appendChild(continuousVideoElement);
    heroBackground.appendChild(overlay);


    continuousVideoElement.play().catch(e => {
        console.error('❌ Failed to play continuous video:', e);
    });
}

// Function đồng bộ nội dung phim với timeline video
function syncMovieContent() {
    // Tính toán phim hiện tại dựa trên thời gian video
    const segmentIndex = Math.floor(currentVideoTime / MOVIE_SEGMENT_DURATION);
    const newMovieIndex = segmentIndex % movies.length;

    // Chỉ update khi chuyển sang phim mới
    if (newMovieIndex !== currentMovieIndex) {
        currentMovieIndex = newMovieIndex;
        updateMovieContentOnly();
        updatePaginationDots();

        console.log(`🎬 Synced to movie ${currentMovieIndex + 1}: ${movies[currentMovieIndex].title}`);
    }
}

// Function bắt đầu đồng bộ nội dung
function startMovieContentSync() {

    if (movieSyncTimer) {
        clearInterval(movieSyncTimer);
    }


    movieSyncTimer = setInterval(() => {
        if (continuousVideoElement && !continuousVideoElement.paused) {
            syncMovieContent();
        }
    }, 100);

    console.log('🔄 Movie content sync started');
}

// Function chỉ update nội dung (không load video mới)
function updateMovieContentOnly() {
    const movie = movies[currentMovieIndex];

    // Smooth transition cho nội dung
    const movieContent = document.getElementById('movieContent');
    movieContent.style.opacity = '0.7';

    setTimeout(() => {
        updateMovieContent(movie);
        updateBackgroundForContinuousVideo(movie);

        movieContent.style.opacity = '1';
    }, 200);
}

// Function update background cho video liên tục
function updateBackgroundForContinuousVideo(movie) {
    const heroSection = document.getElementById('heroSection');
    const bgLayer1 = heroSection.querySelector('.bg-layer-1');
    const bgLayer2 = heroSection.querySelector('.bg-layer-2');

    if (bgLayer1 && bgLayer2) {
        // Update background nhẹ nhàng (cho trường hợp video bị lỗi)
        updateBackgroundLayers(movie, bgLayer1, bgLayer2, false);
    }
}

// Function seek đến thời điểm cụ thể trong video
function seekToMovieSegment(movieIndex) {
    if (continuousVideoElement && !isTransitioning) {
        const targetTime = (movieIndex * MOVIE_SEGMENT_DURATION) / 1000; // Convert to seconds

        console.log(`⏭️ Seeking to movie ${movieIndex + 1} at ${targetTime}s`);

        continuousVideoElement.currentTime = targetTime;
        currentMovieIndex = movieIndex;
        updateMovieContentOnly();
        updatePaginationDots();
    }
}

// Fallback function nếu continuous video không hoạt động
function initializeIndividualVideos() {
    console.log('🔄 Falling back to individual video mode');


    if (continuousVideoElement) {
        continuousVideoElement.remove();
        continuousVideoElement = null;
    }


    if (movieSyncTimer) {
        clearInterval(movieSyncTimer);
        movieSyncTimer = null;
    }


    initializeCarousel();
    updateMovieDisplay();
}

// Function cleanup khi rời khỏi trang
function cleanupContinuousVideo() {
    if (movieSyncTimer) {
        clearInterval(movieSyncTimer);
        movieSyncTimer = null;
    }

    if (continuousVideoElement) {
        continuousVideoElement.pause();
        continuousVideoElement = null;
    }
}


function nextMovie() {
    if (isTransitioning || !movies || movies.length === 0) return;

    if (continuousVideoElement) {
        const nextIndex = (currentMovieIndex + 1) % movies.length;
        seekToMovieSegment(nextIndex);
    } else {
        currentMovieIndex = (currentMovieIndex + 1) % movies.length;
        updateMovieDisplay('next');
        updatePaginationDots();
    }
}

function previousMovie() {
    if (isTransitioning || !movies || movies.length === 0) return;

    if (continuousVideoElement) {
        const prevIndex = (currentMovieIndex - 1 + movies.length) % movies.length;
        seekToMovieSegment(prevIndex);
    } else {
        currentMovieIndex = (currentMovieIndex - 1 + movies.length) % movies.length;
        updateMovieDisplay('prev');
        updatePaginationDots();
    }
}


document.addEventListener('click', function (e) {
    if (e.target.classList.contains('dot')) {
        if (isTransitioning) return;

        const dots = Array.from(document.querySelectorAll('.dot'));
        const newIndex = dots.indexOf(e.target);

        if (newIndex !== currentMovieIndex && newIndex >= 0) {
            if (continuousVideoElement) {
                seekToMovieSegment(newIndex);
            } else {
                const direction = newIndex > currentMovieIndex ? 'next' : 'prev';
                currentMovieIndex = newIndex;
                updateMovieDisplay(direction);
                updatePaginationDots();
            }
        }
    }
});


function bookTickets() {
    const currentMovie = movies[currentMovieIndex];
    if (!currentMovie || !currentMovie.id) {
        console.warn('No movie ID available');
        return;
    }
    
    openShowtimeModal(currentMovie.id);
}

function openShowtimeModal(movieId) {
    // Tạo và hiển thị modal
    const modal = createShowtimeModal();
    document.body.appendChild(modal);


    loadMovieDates(movieId);
}

function loadMovieDates(movieId) {
    const datesContainer = document.getElementById('movieDates');
    datesContainer.innerHTML = '<div class="loading-state"><i class="fas fa-spinner fa-spin"></i>Đang tải ngày chiếu...</div>';

    fetch(`https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/booking-ticket/dropdown/movies/${movieId}/dates`)
        .then(response => response.json())
        .then(data => {
            if (data.data && Array.isArray(data.data) && data.data.length > 0) {
                displayMovieDates(data.data, movieId);
            } else {
                datesContainer.innerHTML = '<div class="empty-state">Không có ngày chiếu nào cho phim này.</div>';
            }
        })
        .catch(error => {
            datesContainer.innerHTML = '<div class="error-state">Có lỗi xảy ra khi tải ngày chiếu.</div>';
        });
}

function displayMovieDates(dates, movieId) {
    const datesContainer = document.getElementById('movieDates');
    const datesHtml = dates.map(date =>
        `<button class="date-btn" data-date="${date.code}">
            <div class="date-text">${date.text}</div>
            <div class="day-text">${getDayOfWeek(date.code)}</div>
        </button>`
    ).join('');

    datesContainer.innerHTML = datesHtml;

    // Xử lý sự kiện click cho các nút ngày
    document.querySelectorAll('.date-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            document.querySelectorAll('.date-btn').forEach(b => b.classList.remove('selected'));
            this.classList.add('selected');
            const selectedDate = this.getAttribute('data-date');
            loadMovieTimes(movieId, selectedDate);
        });
    });
}

function loadMovieTimes(movieId, date) {
    const timesContainer = document.getElementById('movieTimes');
    timesContainer.innerHTML = '<div class="loading-state"><i class="fas fa-spinner fa-spin"></i>Đang tải suất chiếu...</div>';
    timesContainer.style.display = 'block';

    fetch(`https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/booking-ticket/dropdown/movies/${movieId}/times?date=${encodeURIComponent(date + ' 10:00:00+07')}`)
        .then(response => response.json())
        .then(data => {
            if (data.data && Array.isArray(data.data) && data.data.length > 0) {
                displayMovieTimes(data.data);
            } else {
                timesContainer.innerHTML = '<div class="empty-state">Không có suất chiếu nào cho ngày này.</div>';
            }
        })
        .catch(error => {
            timesContainer.innerHTML = '<div class="error-state">Có lỗi xảy ra khi tải suất chiếu.</div>';
        });
}

function displayMovieTimes(times) {
    const timesContainer = document.getElementById('movieTimes');
    const timesHtml = times.map(time =>
        `<button class="time-btn" data-showtime-id="${time.id}">
            <div class="time-text">${time.time}</div>
            <div class="room-text">Phòng chiếu</div>
            <div class="seats-text">Còn ghế trống</div>
        </button>`
    ).join('');

    timesContainer.innerHTML = timesHtml;

    // Xử lý sự kiện click cho các nút giờ chiếu
    document.querySelectorAll('.time-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const showtimeId = this.getAttribute('data-showtime-id');
            window.location.href = `/BookingManagement/Booking/SelectSeat?showtimeId=${showtimeId}`;
        });
    });
}

function createShowtimeModal() {
    const modal = document.createElement('div');
    modal.className = 'showtime-modal';
    modal.innerHTML = `
        <div class="modal-content">
            <div class="modal-header">
                <h3>Chọn Suất Chiếu</h3>
                <button class="close-btn">&times;</button>
            </div>
            <div class="modal-body">
                <div id="movieDates" class="dates-container"></div>
                <div id="movieTimes" class="times-container" style="display: none;"></div>
            </div>
        </div>
    `;

    // Xử lý đóng modal
    const closeBtn = modal.querySelector('.close-btn');
    closeBtn.onclick = () => modal.remove();

    return modal;
}

function getDayOfWeek(dateString) {
    const date = new Date(dateString);
    const days = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];
    return days[date.getDay()];
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


function toggleSearch() {
    const searchForm = document.getElementById('searchForm');
    const searchToggle = document.querySelector('.search-toggle');

    searchForm.classList.toggle('expanded');
    searchToggle.classList.toggle('collapsed');


    if (!searchToggle.style.transition) {
        searchToggle.style.transition = 'transform 0.3s ease';
    }
}


function slideMovies(direction) {
    const grid = document.getElementById('moviesGrid');
    const scrollAmount = 300;

    if (direction === 'left') {
        grid.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
    } else {
        grid.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    }
}


document.addEventListener('click', function (e) {
    if (e.target.closest('.toggle-switch')) {
        const toggle = e.target.closest('.toggle-switch');
        toggle.classList.toggle('active');


        const circle = toggle.querySelector('.toggle-circle');
        if (circle && !circle.style.transition) {
            circle.style.transition = 'transform 0.2s ease';
        }
    }
});


document.addEventListener('mouseenter', function (e) {
    if (e.target.closest('.movie-card') && !e.target.closest('.movie-card').classList.contains('featured')) {
        const card = e.target.closest('.movie-card');
        card.style.transition = 'transform 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275)';
        card.style.transform = 'translateY(-8px) scale(1.03)';
    }
}, true);

document.addEventListener('mouseleave', function (e) {
    if (e.target.closest('.movie-card') && !e.target.closest('.movie-card').classList.contains('featured')) {
        const card = e.target.closest('.movie-card');
        card.style.transform = 'translateY(0) scale(1)';
    }
}, true);













document.addEventListener('keydown', function (e) {
    if (e.key === 'ArrowLeft') {
        previousMovie();
    } else if (e.key === 'ArrowRight') {
        nextMovie();
    }
});


let touchStartX = 0;
let touchEndX = 0;

document.getElementById('heroSection').addEventListener('touchstart', function (e) {
    touchStartX = e.changedTouches[0].screenX;
});

document.getElementById('heroSection').addEventListener('touchend', function (e) {
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





class HomepagePagination {
    constructor() {
        console.log('🔧 Initializing HomepagePagination...');
        this.currentPages = {
            recommended: 1,
            comingSoon: 1
        };

        this.init();
    }

    init() {
        console.log('⚙️ Binding pagination events and loading initial data...');
        this.bindRecommendedEvents();
        this.bindComingSoonEvents();

        this.loadRecommendedMovies(false); // false = replace static content
        this.loadComingSoonMovies(false); // false = replace static content
    }

    bindRecommendedEvents() {

        document.getElementById('recommendedSort')?.addEventListener('change', (e) => {
            console.log('🔄 Recommended sort changed to:', e.target.value);
            this.currentPages.recommended = 1;
            this.loadRecommendedMovies(false); // false = replace, not append
        });


        document.getElementById('recommendedGenre')?.addEventListener('change', (e) => {
            console.log('🎭 Recommended genre changed to:', e.target.value);
            this.currentPages.recommended = 1;
            this.loadRecommendedMovies(false); // false = replace, not append
        });


        const recommendedPageSize = document.getElementById('recommendedPageSize');
        if (recommendedPageSize) {
            recommendedPageSize.addEventListener('change', (e) => {
                console.log('📏 Recommended page size changed to:', e.target.value);
                this.currentPages.recommended = 1;
                this.loadRecommendedMovies(false); // false = replace, not append
            });
        } else {
            console.warn('⚠️ recommendedPageSize element not found');
        }


        document.getElementById('loadMoreRecommended')?.addEventListener('click', (e) => {
            this.currentPages.recommended++;
            this.loadRecommendedMovies(true);
        });
    }

    bindComingSoonEvents() {

        document.getElementById('comingSoonSort')?.addEventListener('change', (e) => {
            console.log('🔄 Coming soon sort changed to:', e.target.value);
            this.currentPages.comingSoon = 1;
            this.loadComingSoonMovies(false); // false = replace, not append
        });


        document.getElementById('comingSoonGenre')?.addEventListener('change', (e) => {
            console.log('🎭 Coming soon genre changed to:', e.target.value);
            this.currentPages.comingSoon = 1;
            this.loadComingSoonMovies(false); // false = replace, not append
        });


        const comingSoonPageSize = document.getElementById('comingSoonPageSize');
        if (comingSoonPageSize) {
            comingSoonPageSize.addEventListener('change', (e) => {
                console.log('📏 Coming soon page size changed to:', e.target.value);
                this.currentPages.comingSoon = 1;
                this.loadComingSoonMovies(false); // false = replace, not append
            });
        } else {
            console.warn('⚠️ comingSoonPageSize element not found');
        }


        document.getElementById('loadMoreComingSoon')?.addEventListener('click', (e) => {
            this.currentPages.comingSoon++;
            this.loadComingSoonMovies(true);
        });
    }

    async loadRecommendedMovies(append = false) {
        console.log('🎬 Loading recommended movies...', { append, page: this.currentPages.recommended });
        
        const sortSelect = document.getElementById('recommendedSort');
        const genreSelect = document.getElementById('recommendedGenre');
        const pageSizeSelect = document.getElementById('recommendedPageSize');
        const pagination = document.getElementById('recommendedInfo');

        if (!sortSelect || !genreSelect) {
            console.log('❌ Missing recommended controls:', {
                sort: !!sortSelect,
                genre: !!genreSelect,
                pageSize: !!pageSizeSelect
            });
            return;
        }
        

        if (!pageSizeSelect) {
            console.warn('⚠️ PageSize select not found, using default value');
        }

        const [sortBy, sortOrder] = sortSelect.value.split('-');
        const genre = genreSelect.value;
        const pageSize = parseInt(pageSizeSelect?.value || '6'); // Default to 6 if not found
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

            const apiUrl = `/Home/GetRecommendedMovies?${params}`;
            console.log('📡 API Call:', apiUrl);
            
            const response = await fetch(apiUrl);
            const data = await response.json();

            console.log('📥 API Response:', data);

            if (data.success) {
                console.log(`✅ Loaded ${data.data.length} recommended movies`);
                this.updateRecommendedGrid(data.data, append);
                this.updatePaginationInfo('recommended', data.pagination);
            } else {
                console.error('❌ Failed to load recommended movies:', data.message);
            }
        } catch (error) {
            console.error('Error loading recommended movies:', error);
        } finally {
            pagination.classList.remove('loading');
        }
    }

    async loadComingSoonMovies(append = false) {
        console.log('🔮 Loading coming soon movies...', { append, page: this.currentPages.comingSoon });
        
        const sortSelect = document.getElementById('comingSoonSort');
        const genreSelect = document.getElementById('comingSoonGenre');
        const pageSizeSelect = document.getElementById('comingSoonPageSize');
        const pagination = document.getElementById('comingSoonInfo');

        if (!sortSelect || !genreSelect) {
            console.log('❌ Missing coming soon controls:', {
                sort: !!sortSelect,
                genre: !!genreSelect,
                pageSize: !!pageSizeSelect
            });
            return;
        }
        

        if (!pageSizeSelect) {
            console.warn('⚠️ ComingSoon PageSize select not found, using default value');
        }

        const [sortBy, sortOrder] = sortSelect.value.split('-');
        const genre = genreSelect.value;
        const pageSize = parseInt(pageSizeSelect?.value || '4'); // Default to 4 for coming soon
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
            } else {
                console.error('Failed to load coming soon movies:', data.message);
            }
        } catch (error) {
            console.error('Error loading coming soon movies:', error);
        } finally {
            pagination.classList.remove('loading');
        }
    }

    updateRecommendedGrid(movies, append = false) {

        const grid = document.querySelector('.recommended-grid');
        if (!grid) {
            console.error('Recommended grid not found');
            return;
        }

        console.log(`📝 Updating recommended grid: ${append ? 'append' : 'replace'} with ${movies.length} movies`);

        if (!append) {

            console.log('🧹 Clearing all recommended movies for filter/sort change');
            grid.innerHTML = '';
        } else {

            const existingDynamic = grid.querySelectorAll('.dynamic-item');
            existingDynamic.forEach(item => item.remove());
        }

        movies.forEach(movie => {
            const movieElement = this.createRecommendedMovieElement(movie);
            movieElement.classList.add('dynamic-item'); // Mark as dynamic
            grid.appendChild(movieElement);
        });

        console.log(`✅ Grid updated with ${grid.children.length} total items`);
    }

    updateComingSoonGrid(movies, append = false) {

        const sections = document.querySelectorAll('.recommended-section-new');
        const comingSoonSection = sections[1]; // Second section (0-indexed) is coming soon
        const grid = comingSoonSection?.querySelector('.recommended-grid');
        
        if (!grid) {
            console.error('Coming soon grid not found');
            return;
        }

        console.log(`📝 Updating coming soon grid: ${append ? 'append' : 'replace'} with ${movies.length} movies`);

        if (!append) {

            console.log('🧹 Clearing all coming soon movies for filter/sort change');
            grid.innerHTML = '';
        } else {

            const existingDynamic = grid.querySelectorAll('.dynamic-item');
            existingDynamic.forEach(item => item.remove());
        }

        movies.forEach(movie => {
            const movieElement = this.createComingSoonMovieElement(movie);
            movieElement.classList.add('dynamic-item'); // Mark as dynamic
            grid.appendChild(movieElement);
        });

        console.log(`✅ Coming soon grid updated with ${grid.children.length} total items`);
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
        div.className = 'recommended-item'; // Use same class structure as in HTML
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
                <div class="recommended-release-date">
                    <i class="fas fa-calendar-alt"></i> ${new Date(movie.releaseDate).toLocaleDateString('vi-VN')}
                </div>
                ${movie.rating > 0 ? `
                    <div class="recommended-rating">
                        <i class="fas fa-star"></i> ${movie.rating.toFixed(1)}
                    </div>
                ` : ''}
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

// Cleanup khi rời khỏi trang
window.addEventListener('beforeunload', cleanupContinuousVideo);


document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Homepage DOM loaded, checking pagination elements...');
    

    setTimeout(() => {
        const recommendedInfo = document.getElementById('recommendedInfo');
        const comingSoonInfo = document.getElementById('comingSoonInfo');
        

        const recommendedSort = document.getElementById('recommendedSort');
        const recommendedGenre = document.getElementById('recommendedGenre');
        const recommendedPageSize = document.getElementById('recommendedPageSize');
        const comingSoonSort = document.getElementById('comingSoonSort');
        const comingSoonGenre = document.getElementById('comingSoonGenre');
        const comingSoonPageSize = document.getElementById('comingSoonPageSize');
        
        console.log('📊 Element check results:', {
            recommendedInfo: !!recommendedInfo,
            comingSoonInfo: !!comingSoonInfo,
            recommendedSort: !!recommendedSort,
            recommendedGenre: !!recommendedGenre,
            recommendedPageSize: !!recommendedPageSize,
            comingSoonSort: !!comingSoonSort,
            comingSoonGenre: !!comingSoonGenre,
            comingSoonPageSize: !!comingSoonPageSize,
            recommendedGrid: !!document.querySelector('.recommended-grid'),
            comingSoonList: !!document.querySelector('.coming-soon-list')
        });


        if (recommendedSort) {
            console.log('🔄 Recommended sort options:', Array.from(recommendedSort.options).map(opt => opt.value));
        }
        if (recommendedGenre) {
            console.log('🎭 Recommended genre options:', Array.from(recommendedGenre.options).map(opt => opt.value));
        }
        if (comingSoonSort) {
            console.log('🔄 Coming soon sort options:', Array.from(comingSoonSort.options).map(opt => opt.value));
        }
        if (comingSoonGenre) {
            console.log('🎭 Coming soon genre options:', Array.from(comingSoonGenre.options).map(opt => opt.value));
        }
        
        if (recommendedInfo && comingSoonInfo) {
            window.homepagePagination = new HomepagePagination();
            console.log('✅ Homepage Pagination initialized successfully');
        } else {
            console.warn('⚠️ Pagination elements missing:', {
                recommendedInfo: !!recommendedInfo,
                comingSoonInfo: !!comingSoonInfo
            });
            

            if (recommendedInfo || comingSoonInfo) {
                window.homepagePagination = new HomepagePagination();
                console.log('⚠️ Partial pagination initialized');
            }
        }
    }, 500);
});