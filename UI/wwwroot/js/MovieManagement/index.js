        const API_BASE = '/api/v1';
        let movies = [];
        

        let currentPage = 1;
        let pageSize = 10;
        let totalMovies = 0;
        let totalPages = 0;
        

        const fallbackMovies = [
            {
                id: 1,
                title: "Avengers: Endgame",
                releaseDate: "2019-04-26",
                duration: 181,
                rating: 8.4,
                status: 2,
                isFeatured: true,
                isRecommended: true,
                primaryImageUrl: "/images/movie-placeholder.jpg"
            },
            {
                id: 2,
                title: "Spider-Man: No Way Home", 
                releaseDate: "2021-12-17",
                duration: 148,
                rating: 8.2,
                status: 2,
                isFeatured: false,
                isRecommended: true,
                primaryImageUrl: "/images/movie-placeholder.jpg"
            },
            {
                id: 3,
                title: "The Batman",
                releaseDate: "2022-03-04", 
                duration: 176,
                rating: 7.8,
                status: 1,
                isFeatured: true,
                isRecommended: false,
                primaryImageUrl: "/images/movie-placeholder.jpg"
            }
        ];


        document.addEventListener('DOMContentLoaded', function() {

            const movieTableBody = document.getElementById('movieTableBody');
            if (!movieTableBody) {
                console.error('movieTableBody element not found');
                showInitializationError();
                return;
            }
            

            loadMoviesPage(currentPage, pageSize);
            
            // Event listener for page size change - thêm debounce
        const pageSizeSelect = document.getElementById('pageSizeSelect');
        let pageSizeChangeTimeout;
        
        pageSizeSelect.addEventListener('change', function() {
            clearTimeout(pageSizeChangeTimeout);
            
            // Hiển thị loading nhỏ bên cạnh dropdown để biết đang xử lý
            const pageSizeContainer = this.closest('.pagination-size');
            const loadingIcon = document.createElement('span');
            loadingIcon.className = 'mini-loading';
            loadingIcon.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
            
            // Thêm icon loading và xóa icon loading cũ nếu có
            const existingLoading = pageSizeContainer.querySelector('.mini-loading');
            if (existingLoading) {
                existingLoading.remove();
            }
            pageSizeContainer.appendChild(loadingIcon);
            
            pageSizeChangeTimeout = setTimeout(() => {
                pageSize = parseInt(this.value);
                currentPage = 1; // Reset to first page
                loadMoviesPage(currentPage, pageSize);
                
                // Xóa icon loading sau 1 giây
                setTimeout(() => {
                    if (loadingIcon.parentNode) {
                        loadingIcon.remove();
                    }
                }, 1000);
            }, 300); // Debounce 300ms
        });
        
        // Event listeners for pagination buttons - thêm visual feedback
        document.getElementById('prevPageBtn').addEventListener('click', function() {
            if (currentPage > 1) {
                this.classList.add('btn-active');
                setTimeout(() => this.classList.remove('btn-active'), 200);
                
                currentPage--;
                loadMoviesPage(currentPage, pageSize);
            }
        });
        
        document.getElementById('nextPageBtn').addEventListener('click', function() {
            if (currentPage < totalPages) {
                this.classList.add('btn-active');
                setTimeout(() => this.classList.remove('btn-active'), 200);
                
                currentPage++;
                loadMoviesPage(currentPage, pageSize);
            }
        });
        

        const searchInput = document.getElementById('movieSearch');
        let searchTimeout;
            
            searchInput.addEventListener('input', function() {
                clearTimeout(searchTimeout);
                searchTimeout = setTimeout(() => {

                    currentPage = 1;
                    loadMoviesPage(currentPage, pageSize, this.value.trim());
                }, 500); // 500ms debounce delay
            });
            

            document.addEventListener('click', function(e) {
                if (e.target.classList.contains('toggle-switch')) {
                    console.log('Toggle switch clicked!', e.target);
                    
                    const movieId = e.target.getAttribute('data-movie-id');
                    const currentState = e.target.getAttribute('data-current-state') === 'true';
                    const type = e.target.getAttribute('data-type');
                    
                    console.log(`Toggle: movieId=${movieId}, currentState=${currentState}, type=${type}`);
                    

                    e.target.style.transform = 'scale(0.95)';
                    setTimeout(() => {
                        e.target.style.transform = 'scale(1)';
                    }, 150);
                    
                    if (type === 'featured') {
                        console.log('Calling toggleFeatured...');
                        toggleFeatured(movieId, !currentState);
                    } else if (type === 'recommended') {
                        console.log('Calling toggleRecommended...');
                        toggleRecommended(movieId, !currentState);
                    }
                }
            });
        });


        function toggleSidebar() {
            const sidebar = document.querySelector('.sidebar');
            const mainContent = document.querySelector('.main-content');
            
            sidebar.classList.toggle('collapsed');
            mainContent.classList.toggle('sidebar-collapsed');
        }


        function logout() {
            if (confirm('Bạn có chắc chắn muốn đăng xuất?')) {
                window.location.href = '/Account/Logout';
            }
        }


        function navigateTo(url) {
            window.location.href = url;
        }

        // Biến để theo dõi lần gọi API mới nhất và tránh spam API
        let currentApiCall = 0;
        let isLoadingData = false;
        let lastAPICallTime = 0;
        const API_COOLDOWN = 500; // 500ms cooldown giữa các lần gọi API


        async function loadMoviesPage(page, pageSize, searchTerm = '') {
            try {
                // Tránh gọi API liên tục, thêm cooldown
                const now = Date.now();
                if (isLoadingData || (now - lastAPICallTime < API_COOLDOWN)) {
                    console.log('Skipping API call - too frequent or already loading');
                    return;
                }
                
                // Đánh dấu đang loading
                isLoadingData = true;
                lastAPICallTime = now;
                
                const tbody = document.getElementById('movieTableBody');
                const thisApiCall = ++currentApiCall; // Mỗi lần gọi API sẽ có một ID duy nhất
                
                // Hiển thị animation loading và giữ lại nội dung cũ với opacity thấp hơn
                const tableContainer = document.querySelector('.movie-table-container');
                
                // Xoá overlay loading cũ nếu có
                const oldOverlay = tableContainer.querySelector('.loading-overlay');
                if (oldOverlay) {
                    oldOverlay.remove();
                }
                
                // Thêm class loading để hiệu ứng mờ
                tableContainer.classList.add('loading');
                
                // Thêm overlay loading indicator
                const loadingOverlay = document.createElement('div');
                loadingOverlay.className = 'loading-overlay';
                loadingOverlay.innerHTML = `
                    <div class="loading-spinner-small">
                        <i class="fas fa-spinner fa-spin"></i>
                    </div>
                `;
                tableContainer.appendChild(loadingOverlay);
                
                console.log(`Loading movies - Page: ${page}, PageSize: ${pageSize}, Search: "${searchTerm}"`);
                
                // Build query URL - Sửa lại để gọi action cục bộ
                let url = `/MovieManagement/Movies/GetMoviesPagination?page=${page}&pageSize=${pageSize}`;
                if (searchTerm) {
                    url += `&searchTerm=${encodeURIComponent(searchTerm)}`;
                }
                
                // Thêm timestamp để tránh cache
                url += `&_=${Date.now()}`;
                
                const response = await fetch(url);
                
                if (response.ok) {
                    const result = await response.json();
                    console.log('API Response:', result);

                    // Log chi tiết cấu trúc API để debug
                    if (result.success && result.data) {
                        console.log('API Response Structure:');
                        if (result.data.data && Array.isArray(result.data.data) && result.data.data.length > 0) {
                            const firstMovie = result.data.data[0];
                            console.log('First movie structure:', firstMovie);
                            console.log('First movie has primaryImageUrl:', !!firstMovie.primaryImageUrl);
                            console.log('First movie has PrimaryImageUrl:', !!firstMovie.PrimaryImageUrl);
                            console.log('First movie has images array:', !!firstMovie.images && Array.isArray(firstMovie.images));
                            console.log('First movie has Images array:', !!firstMovie.Images && Array.isArray(firstMovie.Images));

                            if (firstMovie.images && firstMovie.images.length > 0) {
                                console.log('First image in images array:', firstMovie.images[0]);
                            }
                            if (firstMovie.Images && firstMovie.Images.length > 0) {
                                console.log('First image in Images array:', firstMovie.Images[0]);
                            }
                        }
                    }
                    
                    // Kiểm tra nếu API call này đã bị thay thế bởi call mới hơn
                    if (thisApiCall < currentApiCall) {
                        console.log(`Ignoring stale API response for call #${thisApiCall}`);
                        return;
                    }
                    
                    if (result.success && result.data) {

                        if (result.data.data) {

                            movies = result.data.data || [];
                            totalMovies = result.data.total || 0;
                            currentPage = result.data.page || page;
                            pageSize = result.data.pageSize || pageSize;
                    } else {

                            movies = result.data;
                            totalMovies = movies.length;
                            currentPage = page;
                        }
                        

                        totalPages = Math.max(1, Math.ceil(totalMovies / pageSize));
                        
                        console.log(`Loaded ${movies.length} movies (Page ${currentPage}/${totalPages}, Total: ${totalMovies})`);
                        
                                            try {

                        updatePaginationInfo();
                        renderPageNumbers();
                        updateMovieHeader();
                        renderMovies();
                            

                        const prevBtn = document.getElementById('prevPageBtn');
                        const nextBtn = document.getElementById('nextPageBtn');
                        
                        if (prevBtn) prevBtn.disabled = (currentPage <= 1);
                        if (nextBtn) nextBtn.disabled = (currentPage >= totalPages);
                    } catch (uiError) {
                        console.error('Error updating UI:', uiError);
                        showNotification('Đã xảy ra lỗi khi cập nhật giao diện', 'error');
                    }
                                    } else {
                    console.error('Controller returned error:', result.message);
                    console.log('API error, using fallback demo data');
                    

                    movies = fallbackMovies.slice();
                    totalMovies = fallbackMovies.length;
                    totalPages = Math.ceil(totalMovies / pageSize);
                    currentPage = Math.min(page, totalPages);
                    
                    updatePaginationInfo();
                    renderMovies();
                    
                    showNotification('API không khả dụng. Hiển thị dữ liệu demo.', 'error');
                }
                    
                    // Xóa overlay loading và class loading
                    const tableContainer = document.querySelector('.movie-table-container');
                    const loadingOverlay = tableContainer.querySelector('.loading-overlay');
                    if (loadingOverlay) {
                        loadingOverlay.remove();
                    }
                    tableContainer.classList.remove('loading');
                    

                    isLoadingData = false;
                } else {
                    console.error('Controller Error:', response.status);
                    showEmptyState('Không thể tải dữ liệu phim. Vui lòng thử lại.');
                    
                    // Xóa overlay loading và class loading
                    const tableContainer = document.querySelector('.movie-table-container');
                    const loadingOverlay = tableContainer.querySelector('.loading-overlay');
                    if (loadingOverlay) {
                        loadingOverlay.remove();
                    }
                    tableContainer.classList.remove('loading');
                    

                    isLoadingData = false;
                }
            } catch (error) {
                console.error('Error loading movies:', error);
                

                let errorMessage = 'Không thể tải dữ liệu phim. Vui lòng thử lại.';
                if (error.message && error.message.includes('Failed to fetch')) {
                    errorMessage = 'Không thể kết nối đến server. Vui lòng kiểm tra kết nối mạng.';
                } else if (error.message && error.message.includes('NetworkError')) {
                    errorMessage = 'Lỗi mạng. Vui lòng thử lại sau.';
                }
                
                showNotification(errorMessage + ' (Hiển thị dữ liệu demo)', 'error');
                

                console.log('Using fallback demo data');
                movies = fallbackMovies.slice();
                totalMovies = fallbackMovies.length;
                totalPages = Math.ceil(totalMovies / pageSize);
                currentPage = Math.min(page, totalPages);
                
                updatePaginationInfo();
                renderMovies();
                
                // Xóa overlay loading và class loading
                const tableContainer = document.querySelector('.movie-table-container');
                if (tableContainer) {
                    const loadingOverlay = tableContainer.querySelector('.loading-overlay');
                    if (loadingOverlay) {
                        loadingOverlay.remove();
                    }
                    tableContainer.classList.remove('loading');
                }
                

                isLoadingData = false;
            }
        }


        function updateMovieHeader() {
            const headerTitle = document.getElementById('movieListTitle');
            headerTitle.innerHTML = `<i class="fas fa-film me-2"></i>Danh sách phim (${totalMovies} phim)`;
            

            console.log(`Loaded ${movies.length} movies (total: ${totalMovies}) successfully`);
        }
        
        // Hàm lấy đường dẫn ảnh từ thông tin phim
        // Hàm chuyển đổi URL thường thành URL embed cho trailer
        function getEmbedUrl(url) {
            if (!url) return '';
            
            // Xử lý URL YouTube
            if (url.includes('youtube.com') || url.includes('youtu.be')) {
                // Lấy video ID từ URL YouTube
                let videoId = '';
                
                if (url.includes('youtube.com/watch')) {
                    const urlParams = new URLSearchParams(new URL(url).search);
                    videoId = urlParams.get('v');
                } else if (url.includes('youtu.be/')) {
                    videoId = url.split('youtu.be/')[1].split('?')[0];
                } else if (url.includes('youtube.com/embed/')) {
                    videoId = url.split('youtube.com/embed/')[1].split('?')[0];
                }
                
                if (videoId) {
                    return `https://www.youtube.com/embed/${videoId}?autoplay=0&rel=0`;
                }
            }
            
            // Xử lý URL Cloudinary
            if (url.includes('cloudinary.com')) {
                // Nếu đã là URL embed, trả về nguyên URL
                if (url.includes('/embed')) {
                    return url;
                }
                
                // Chuyển đổi URL thường sang URL embed
                if (url.includes('/video/upload/')) {
                    return url.replace('/video/upload/', '/video/upload/e_volume:0/');
                }
            }
            
            // Xử lý URL Vimeo
            if (url.includes('vimeo.com')) {
                const vimeoId = url.split('vimeo.com/')[1];
                if (vimeoId) {
                    return `https://player.vimeo.com/video/${vimeoId}`;
                }
            }
            
            // Nếu không phải các loại URL trên, trả về URL gốc
            return url;
        }
        
        function getMovieImageUrl(movie) {
            try {
                if (!movie || typeof movie !== 'object') {
                    throw new Error('Invalid movie object');
                }
                
                // Kiểm tra theo thứ tự ưu tiên

                if (movie.primaryImageUrl) {
                    console.log(`Movie ${movie.title || 'Unknown'}: Using primaryImageUrl: ${movie.primaryImageUrl}`);
                    return movie.primaryImageUrl;
                }
            

            if (movie.PrimaryImageUrl) {
                console.log(`Movie ${movie.title}: Using PrimaryImageUrl: ${movie.PrimaryImageUrl}`);
                return movie.PrimaryImageUrl;
            }
            
            // 3. Tìm trong mảng images nếu có isPrimary = true
            if (movie.images && Array.isArray(movie.images) && movie.images.length > 0) {
                // Tìm ảnh có isPrimary = true
                const primaryImage = movie.images.find(img => img.isPrimary === true);
                if (primaryImage && primaryImage.imageUrl) {
                    console.log(`Movie ${movie.title}: Using images array with isPrimary: ${primaryImage.imageUrl}`);
                    return primaryImage.imageUrl;
                }
                
                // Nếu không có isPrimary, lấy ảnh đầu tiên
                if (movie.images[0].imageUrl) {
                    console.log(`Movie ${movie.title}: Using first image: ${movie.images[0].imageUrl}`);
                    return movie.images[0].imageUrl;
                }
            }
            
            // 4. Tìm trong mảng Images (Pascal case) nếu có IsPrimary = true
            if (movie.Images && Array.isArray(movie.Images) && movie.Images.length > 0) {
                // Tìm ảnh có IsPrimary = true
                const primaryImage = movie.Images.find(img => img.IsPrimary === true || img.isPrimary === true);
                if (primaryImage) {
                    const url = primaryImage.ImageUrl || primaryImage.imageUrl;
                    if (url) {
                        console.log(`Movie ${movie.title}: Using Images array with IsPrimary: ${url}`);
                        return url;
                    }
                }
                
                // Nếu không có IsPrimary, lấy ảnh đầu tiên
                const firstImageUrl = movie.Images[0].ImageUrl || movie.Images[0].imageUrl;
                if (firstImageUrl) {
                    console.log(`Movie ${movie.title}: Using first Image (Pascal): ${firstImageUrl}`);
                    return firstImageUrl;
                }
            }
            
            // 5. Kiểm tra imageUrl nếu có
            if (movie.imageUrl) {
                console.log(`Movie ${movie.title}: Using imageUrl: ${movie.imageUrl}`);
                return movie.imageUrl;
            }
            
            // 6. Kiểm tra posterUrl nếu có
            if (movie.posterUrl) {
                console.log(`Movie ${movie.title}: Using posterUrl: ${movie.posterUrl}`);
                return movie.posterUrl;
            }
            
            // 7. Cuối cùng trả về ảnh placeholder
            console.log(`Movie ${movie.title || 'Unknown'}: No image found, using placeholder`);
            } catch (error) {
                console.error('Error getting movie image:', error);
            }
            

            return 'data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI2MCIgaGVpZ2h0PSI4MCIgdmlld0JveD0iMCAwIDYwIDgwIiBmaWxsPSJub25lIj48cmVjdCB3aWR0aD0iNjAiIGhlaWdodD0iODAiIGZpbGw9IiMyYTJhMmEiLz48dGV4dCB4PSI1MCUiIHk9IjUwJSIgZm9udC1mYW1pbHk9IkFyaWFsIiBmb250LXNpemU9IjgiIGZpbGw9IiM2NjYiIHRleHQtYW5jaG9yPSJtaWRkbGUiIGRvbWluYW50LWJhc2VsaW5lPSJtaWRkbGUiPk5vIGltYWdlPC90ZXh0Pjwvc3ZnPg==';
        }
        

        function updatePaginationInfo() {
            const infoElement = document.getElementById('paginationInfo');
            
            if (totalMovies === 0) {
                infoElement.textContent = "Không có phim nào";
                return;
            }
            
            const startItem = (currentPage - 1) * pageSize + 1;
            const endItem = Math.min(currentPage * pageSize, totalMovies);
            
            infoElement.textContent = `Trang ${currentPage}/${totalPages} (Hiển thị ${startItem}-${endItem} của ${totalMovies})`;
        }
        

        function renderPageNumbers() {
            const pageNumbersContainer = document.getElementById('pageNumbers');
            pageNumbersContainer.innerHTML = '';
            
            if (totalPages <= 1) {
                return;
            }
            

            let startPage = Math.max(1, currentPage - 2);
            let endPage = Math.min(totalPages, currentPage + 2);
            

            if (endPage - startPage < 4) {
                if (startPage === 1) {
                    endPage = Math.min(5, totalPages);
                } else {
                    startPage = Math.max(1, endPage - 4);
                }
            }
            

            if (startPage > 1) {
                const firstPageBtn = document.createElement('div');
                firstPageBtn.className = 'page-number';
                firstPageBtn.textContent = '1';
                firstPageBtn.addEventListener('click', () => {
                    currentPage = 1;
                    loadMoviesPage(currentPage, pageSize);
                });
                pageNumbersContainer.appendChild(firstPageBtn);
                
                if (startPage > 2) {
                    const ellipsis = document.createElement('div');
                    ellipsis.className = 'page-ellipsis';
                    ellipsis.textContent = '...';
                    pageNumbersContainer.appendChild(ellipsis);
                }
            }
            

            for (let i = startPage; i <= endPage; i++) {
                const pageBtn = document.createElement('div');
                pageBtn.className = i === currentPage ? 'page-number active' : 'page-number';
                pageBtn.textContent = i;
                pageBtn.addEventListener('click', () => {
                    if (i !== currentPage) {
                        currentPage = i;
                        loadMoviesPage(currentPage, pageSize);
                    }
                });
                pageNumbersContainer.appendChild(pageBtn);
            }
            

            if (endPage < totalPages) {
                if (endPage < totalPages - 1) {
                    const ellipsis = document.createElement('div');
                    ellipsis.className = 'page-ellipsis';
                    ellipsis.textContent = '...';
                    pageNumbersContainer.appendChild(ellipsis);
                }
                
                const lastPageBtn = document.createElement('div');
                lastPageBtn.className = 'page-number';
                lastPageBtn.textContent = totalPages;
                lastPageBtn.addEventListener('click', () => {
                    currentPage = totalPages;
                    loadMoviesPage(currentPage, pageSize);
                });
                pageNumbersContainer.appendChild(lastPageBtn);
            }
        }


        function renderMovies() {
            const tbody = document.getElementById('movieTableBody');
            
            if (!tbody) {
                console.error('Could not find movieTableBody element');
                return;
            }
            
            if (!Array.isArray(movies) || movies.length === 0) {
                showEmptyState();
                return;
            }
            
            try {
                tbody.innerHTML = movies.map(movie => {
                    if (!movie || typeof movie !== 'object') {
                        console.error('Invalid movie object:', movie);
                        return '';
                    }
                    
                    const statusText = getStatusText(movie.status || 0);
                    
                    // Xử lý thể loại để hiển thị tên thể loại thay vì [object Object]
                    let genresText = 'Chưa phân loại';
                    if (Array.isArray(movie.genres) && movie.genres.length > 0) {
                        // Kiểm tra nếu genres là mảng các đối tượng
                        if (typeof movie.genres[0] === 'object' && movie.genres[0] !== null) {
                            genresText = movie.genres.map(g => g.name || g.Name || '').filter(name => name).join(', ');
                        } else {
                            // Nếu là mảng chuỗi
                            genresText = movie.genres.join(', ');
                        }
                    }
                
                    return `
                    <tr data-movie-id="${movie.id}">
                        <td>
                            <img src="${getMovieImageUrl(movie)}" 
                                 alt="${movie.title || 'Poster phim'}" 
                                 class="movie-poster"
                                 loading="lazy"
                                 onerror="this.onerror=null; this.src='data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI2MCIgaGVpZ2h0PSI4MCIgdmlld0JveD0iMCAwIDYwIDgwIiBmaWxsPSJub25lIj48cmVjdCB3aWR0aD0iNjAiIGhlaWdodD0iODAiIGZpbGw9IiMyYTJhMmEiLz48dGV4dCB4PSI1MCUiIHk9IjUwJSIgZm9udC1mYW1pbHk9IkFyaWFsIiBmb250LXNpemU9IjgiIGZpbGw9IiM2NjYiIHRleHQtYW5jaG9yPSJtaWRkbGUiIGRvbWluYW50LWJhc2VsaW5lPSJtaWRkbGUiPk5vIGltYWdlPC90ZXh0Pjwvc3ZnPg=='">
                        </td>
                        <td>
                            <div class="movie-info">
                                <div class="movie-title" title="${movie.title || 'N/A'}">${movie.title || 'N/A'}</div>
                                <div class="movie-studio" title="${movie.productionCompany || 'Chưa có thông tin'}">${movie.productionCompany || 'Chưa có thông tin'}</div>
                            </div>
                        </td>
                        <td>
                            <div class="movie-genres">
                                <span class="genre-tag" title="${genresText}">${genresText}</span>
                            </div>
                        </td>
                        <td style="font-size: 0.85rem;">${movie.releaseDate ? new Date(movie.releaseDate).toLocaleDateString('vi-VN') : 'N/A'}</td>
                        <td style="font-size: 0.85rem;">${movie.runningTime ? movie.runningTime + ' phút' : 'N/A'}</td>
                        <td>
                            <select class="status-dropdown" onchange="changeMovieStatus('${movie.id}', this.value)">
                                <option value="2" ${movie.status === 2 ? 'selected' : ''}>Đang chiếu</option>
                                <option value="1" ${movie.status === 1 ? 'selected' : ''}>Sắp chiếu</option>
                                <option value="3" ${movie.status === 3 ? 'selected' : ''}>Ngừng chiếu</option>
                                <option value="0" ${movie.status === 0 ? 'selected' : ''}>Chưa có</option>
                            </select>
                        </td>
                        <td>
                            <button class="toggle-switch ${(movie.isFeatured === true || movie.isFeatured === 'true') ? 'active' : ''}"
                                    data-movie-id="${movie.id}"
                                    data-current-state="${movie.isFeatured}"
                                    data-type="featured">
                            </button>
                        </td>
                        <td>
                            <button class="toggle-switch ${(movie.isRecommended === true || movie.isRecommended === 'true') ? 'active' : ''}"
                                    data-movie-id="${movie.id}"
                                    data-current-state="${movie.isRecommended}"
                                    data-type="recommended">
                            </button>
                        </td>
                        <td>
                            <input type="number" class="rating-input" 
                                   value="${movie.rating || 0}" 
                                   min="0" max="10" step="0.1"
                                   onchange="updateRating('${movie.id}', this.value)">
                        </td>
                        <td>
                            <div class="action-buttons">
                                <button class="btn-action btn-view" title="Xem chi tiết" onclick="viewMovie('${movie.id}')">
                                    <i class="fas fa-eye"></i>
                                </button>
                                <button class="btn-action btn-edit" title="Chỉnh sửa" onclick="editMovie('${movie.id}')">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button class="btn-action btn-hide" title="Ẩn phim" onclick="hideMovie('${movie.id}')">
                                    <i class="fas fa-eye-slash"></i>
                                </button>
                            </div>
                        </td>
                    </tr>
                `;
                }).join('');
            
                // Thêm sự kiện để xử lý hiệu ứng ripple cho tất cả các nút
                document.querySelectorAll('.btn-action').forEach(button => {
                    button.addEventListener('click', function() {
                        this.classList.add('ripple');
                        setTimeout(() => {
                            this.classList.remove('ripple');
                        }, 1000);
                    });
                });
            } catch (error) {
                console.error("Error rendering movies:", error);
                const tbody = document.getElementById('movieTableBody');
                if (tbody) {
                    tbody.innerHTML = `
                        <tr>
                            <td colspan="10" class="empty-state">
                                <i class="fas fa-exclamation-triangle"></i>
                                <h3>Lỗi hiển thị</h3>
                                <p>Đã có lỗi xảy ra khi xử lý dữ liệu phim. Vui lòng thử lại.</p>
                            </td>
                        </tr>`;
                }
            }
        }


        function getStatusText(status) {
            switch(status) {
                case 2: return 'Đang chiếu';    // NowShowing
                case 1: return 'Sắp chiếu';     // ComingSoon
                case 3: return 'Ngừng chiếu';   // Stopped
                case 0: return 'Chưa có';       // NotAvailable
                default: return 'Không xác định';
            }
        }


        async function changeMovieStatus(movieId, status) {
            try {
                console.log(`changeMovieStatus called: movieId=${movieId}, status=${status}`);
                

                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                
                if (!token) {
                    const statusText = getStatusText(parseInt(status));
                    showNotification(`Demo mode: Cập nhật trạng thái thành "${statusText}"`, 'info');
                    

                    const movie = movies.find(m => m.id === movieId);
                    if (movie) {
                        movie.status = parseInt(status);
                    }
                    return;
                }
                
                const formData = new FormData();
                formData.append('__RequestVerificationToken', token);
                
                const response = await fetch(`/MovieManagement/Movies/ChangeMovieStatus?movieId=${movieId}&status=${status}`, {
                    method: 'PATCH',
                    body: formData
                });

                const result = await response.json();
                if (result.success) {
                    showNotification(result.message, 'success');

                    const movie = movies.find(m => m.id === movieId);
                    if (movie) {
                        movie.status = parseInt(status);
                    }
                } else {
                    showNotification('Lỗi: ' + result.message, 'error');
                }
            } catch (error) {
                console.error('Error changing status:', error);
                const statusText = getStatusText(parseInt(status));
                showNotification(`Demo mode: Cập nhật trạng thái thành "${statusText}"`, 'info');
                

                const movie = movies.find(m => m.id === movieId);
                if (movie) {
                    movie.status = parseInt(status);
                }
            }
        }

        async function toggleFeatured(movieId, isFeatured) {
            try {
                console.log(`toggleFeatured called: movieId=${movieId}, isFeatured=${isFeatured}`);
                

                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                
                if (!token) {
                    console.error('Anti-forgery token not found');
                    showNotification('Demo mode: Cập nhật trạng thái "Phim nổi bật" thành ' + (isFeatured ? 'BẬT' : 'TẮT'), 'info');
                    

                    const movie = movies.find(m => m.id === movieId);
                    if (movie) {
                        movie.isFeatured = isFeatured;
                    }
                    
                    const toggleBtn = document.querySelector(`button[data-movie-id="${movieId}"][data-type="featured"]`);
                    if (toggleBtn) {
                        toggleBtn.className = `toggle-switch ${isFeatured ? 'active' : ''}`;
                        toggleBtn.setAttribute('data-current-state', isFeatured);
                    }
                    return;
                }
                
                const formData = new FormData();
                formData.append('__RequestVerificationToken', token);
                
                const response = await fetch(`/MovieManagement/Movies/SetFeatured?movieId=${movieId}&isFeatured=${isFeatured}`, {
                    method: 'PATCH',
                    body: formData
                });

                const result = await response.json();
                if (result.success) {
                    showNotification(result.message, 'success');

                    const movie = movies.find(m => m.id === movieId);
                    if (movie) {
                        movie.isFeatured = isFeatured;
                    }
                    // Update the toggle button - same as UI cũ
                    const toggleBtn = document.querySelector(`button[data-movie-id="${movieId}"][data-type="featured"]`);
                    if (toggleBtn) {
                        toggleBtn.className = `toggle-switch ${isFeatured ? 'active' : ''}`;
                        toggleBtn.setAttribute('data-current-state', isFeatured);
                    }
                } else {
                    showNotification('Lỗi: ' + result.message, 'error');
                }
            } catch (error) {
                console.error('Error toggling featured:', error);
                showNotification('Demo mode: Toggle "Phim nổi bật" thành ' + (isFeatured ? 'BẬT' : 'TẮT'), 'info');
                

                const movie = movies.find(m => m.id === movieId);
                if (movie) {
                    movie.isFeatured = isFeatured;
                }
                
                const toggleBtn = document.querySelector(`button[data-movie-id="${movieId}"][data-type="featured"]`);
                if (toggleBtn) {
                    toggleBtn.className = `toggle-switch ${isFeatured ? 'active' : ''}`;
                    toggleBtn.setAttribute('data-current-state', isFeatured);
                }
            }
        }

        async function toggleRecommended(movieId, isRecommended) {
            try {
                console.log(`toggleRecommended called: movieId=${movieId}, isRecommended=${isRecommended}`);
                

                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                
                if (!token) {
                    console.error('Anti-forgery token not found');
                    showNotification('Demo mode: Cập nhật trạng thái "Phim đề xuất" thành ' + (isRecommended ? 'BẬT' : 'TẮT'), 'info');
                    

                    const movie = movies.find(m => m.id === movieId);
                    if (movie) {
                        movie.isRecommended = isRecommended;
                    }
                    
                    const toggleBtn = document.querySelector(`button[data-movie-id="${movieId}"][data-type="recommended"]`);
                    if (toggleBtn) {
                        toggleBtn.className = `toggle-switch ${isRecommended ? 'active' : ''}`;
                        toggleBtn.setAttribute('data-current-state', isRecommended);
                    }
                    return;
                }
                
                const formData = new FormData();
                formData.append('__RequestVerificationToken', token);
                
                const response = await fetch(`/MovieManagement/Movies/SetRecommended?movieId=${movieId}&isRecommended=${isRecommended}`, {
                    method: 'PATCH',
                    body: formData
                });

                const result = await response.json();
                if (result.success) {
                    showNotification(result.message, 'success');

                    const movie = movies.find(m => m.id === movieId);
                    if (movie) {
                        movie.isRecommended = isRecommended;
                    }
                    // Update the toggle button - same as UI cũ
                    const toggleBtn = document.querySelector(`button[data-movie-id="${movieId}"][data-type="recommended"]`);
                    if (toggleBtn) {
                        toggleBtn.className = `toggle-switch ${isRecommended ? 'active' : ''}`;
                        toggleBtn.setAttribute('data-current-state', isRecommended);
                    }
                } else {
                    showNotification('Lỗi: ' + result.message, 'error');
                }
            } catch (error) {
                console.error('Error toggling recommended:', error);
                showNotification('Demo mode: Toggle "Phim đề xuất" thành ' + (isRecommended ? 'BẬT' : 'TẮT'), 'info');
                

                const movie = movies.find(m => m.id === movieId);
                if (movie) {
                    movie.isRecommended = isRecommended;
                }
                
                const toggleBtn = document.querySelector(`button[data-movie-id="${movieId}"][data-type="recommended"]`);
                if (toggleBtn) {
                    toggleBtn.className = `toggle-switch ${isRecommended ? 'active' : ''}`;
                    toggleBtn.setAttribute('data-current-state', isRecommended);
                }
            }
        }

        async function updateRating(movieId, rating) {
            try {
                console.log(`updateRating called: movieId=${movieId}, rating=${rating}`);
                

                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                
                if (!token) {
                    showNotification(`Demo mode: Cập nhật rating thành ${rating}/10`, 'info');
                    

                    const movie = movies.find(m => m.id === movieId);
                    if (movie) {
                        movie.rating = parseFloat(rating);
                    }
                    return;
                }
                
                const formData = new FormData();
                formData.append('__RequestVerificationToken', token);
                
                const response = await fetch(`/MovieManagement/Movies/UpdateRating?movieId=${movieId}&rating=${rating}`, {
                    method: 'PATCH',
                    body: formData
                });

                const result = await response.json();
                if (result.success) {
                    showNotification(result.message, 'success');

                    const movie = movies.find(m => m.id === movieId);
                    if (movie) {
                        movie.rating = parseFloat(rating);
                    }
                } else {
                    showNotification('Lỗi: ' + result.message, 'error');
                }
            } catch (error) {
                console.error('Error updating rating:', error);
                showNotification(`Demo mode: Cập nhật rating thành ${rating}/10`, 'info');
                

                const movie = movies.find(m => m.id === movieId);
                if (movie) {
                    movie.rating = parseFloat(rating);
                }
            }
        }

        async function viewMovie(movieId) {
            try {
                const response = await fetch(`/MovieManagement/Movies/GetMovieById?movieId=${movieId}`);
                const result = await response.json();
                
                console.log('View Movie API Response:', result); // Debug log
                
                if (result.success) {

                    const movieData = result.data?.data || result.data;
                    console.log('Movie Data for View:', movieData); // Debug log
                    
                    if (movieData) {
                        showMovieDetailModal(movieData);
                    } else {
                        showNotification('Không có dữ liệu phim', 'error');
                    }
                } else {
                    showNotification('Không thể tải thông tin phim', 'error');
                }
            } catch (error) {
                console.error('Error loading movie details:', error);
                showNotification('Đã xảy ra lỗi khi tải thông tin phim', 'error');
            }
        }

        async function editMovie(movieId) {
            try {
                // Thêm hiệu ứng loading vào nút
                const editButtons = document.querySelectorAll(`.btn-edit[onclick*="${movieId}"]`);
                editButtons.forEach(btn => {
                    // Thêm hiệu ứng ripple
                    btn.classList.add('ripple');
                    
                    // Thêm hiệu ứng loading
                    btn.classList.add('loading');
                    
                    // Vô hiệu hóa nút trong khi loading
                    btn.disabled = true;
                });
                
                const response = await fetch(`/MovieManagement/Movies/GetMovieById?movieId=${movieId}`);
                const result = await response.json();
                
                console.log('Edit Movie API Response:', result); // Debug log
                
                // Sau khi API trả về, xóa hiệu ứng loading
                editButtons.forEach(btn => {
                    setTimeout(() => {
                        btn.classList.remove('ripple');
                        btn.classList.remove('loading');
                        btn.disabled = false;
                    }, 500); // Giữ hiệu ứng thêm 500ms để người dùng thấy rõ
                });
                
                if (result.success) {

                    const movieData = result.data?.data || result.data;
                    console.log('Movie Data for Edit:', movieData); // Debug log
                    
                    if (movieData) {
                        showEditMovieModal(movieData);
                    } else {
                        showNotification('Không có dữ liệu phim để chỉnh sửa', 'error');
                    }
                } else {
                    showNotification('Không thể tải thông tin phim để chỉnh sửa', 'error');
                }
            } catch (error) {
                console.error('Error loading movie for edit:', error);
                showNotification('Đã xảy ra lỗi khi tải thông tin phim', 'error');
                
                // Xóa hiệu ứng loading nếu có lỗi
                const editButtons = document.querySelectorAll(`.btn-edit[onclick*="${movieId}"]`);
                editButtons.forEach(btn => {
                    btn.classList.remove('ripple');
                    btn.classList.remove('loading');
                    btn.disabled = false;
                });
            }
        }

        function deleteMovie(movieId) {
            console.log(`Deleting movie ${movieId}`);

        }
        
        async function hideMovie(movieId) {
            try {
                // Thêm hiệu ứng loading vào nút
                const hideButtons = document.querySelectorAll(`.btn-hide[onclick*="${movieId}"]`);
                hideButtons.forEach(btn => {
                    // Thêm hiệu ứng ripple
                    btn.classList.add('ripple');
                    
                    // Thêm hiệu ứng loading
                    btn.classList.add('loading');
                    
                    // Vô hiệu hóa nút trong khi loading
                    btn.disabled = true;
                });
                
                // Hiển thị xác nhận
                if (!confirm('Bạn có chắc chắn muốn ẩn phim này không? Phim sẽ được chuyển sang trạng thái "Ngừng chiếu".')) {
                    // Nếu người dùng không xác nhận, xóa hiệu ứng loading
                    hideButtons.forEach(btn => {
                        btn.classList.remove('ripple');
                        btn.classList.remove('loading');
                        btn.disabled = false;
                    });
                    return;
                }
                
                // Gọi API để thay đổi trạng thái phim thành "Stopped" (3)
                await changeMovieStatus(movieId, 3);
                
                // Sau khi API trả về, xóa hiệu ứng loading
                hideButtons.forEach(btn => {
                    setTimeout(() => {
                        btn.classList.remove('ripple');
                        btn.classList.remove('loading');
                        btn.disabled = false;
                    }, 500);
                });
                
                // Hiển thị thông báo thành công
                showNotification('Phim đã được ẩn thành công', 'success');
                
                // Cập nhật lại danh sách phim
                loadMoviesPage(currentPage, pageSize);
                
            } catch (error) {
                console.error('Error hiding movie:', error);
                showNotification('Đã xảy ra lỗi khi ẩn phim', 'error');
                
                // Xóa hiệu ứng loading nếu có lỗi
                const hideButtons = document.querySelectorAll(`.btn-hide[onclick*="${movieId}"]`);
                hideButtons.forEach(btn => {
                    btn.classList.remove('ripple');
                    btn.classList.remove('loading');
                    btn.disabled = false;
                });
            }
        }


        function showNotification(message, type = 'info') {

            const notification = document.createElement('div');
            notification.className = `notification notification-${type}`;
            notification.innerHTML = `
                <i class="fas ${type === 'success' ? 'fa-check-circle' : type === 'error' ? 'fa-exclamation-circle' : 'fa-info-circle'}"></i>
                <span>${message}</span>
                <button onclick="this.parentElement.remove()" class="notification-close">
                    <i class="fas fa-times"></i>
                </button>
            `;
            

            document.body.appendChild(notification);
            

            setTimeout(() => {
                if (notification.parentElement) {
                    notification.remove();
                }
            }, 5000);
        }


        function showEmptyState(message = 'Chưa có phim nào trong hệ thống') {
            const tbody = document.getElementById('movieTableBody');
            if (!tbody) {
                console.error('movieTableBody not found for showEmptyState');
                return;
            }
            tbody.innerHTML = `
                <tr>
                    <td colspan="8" class="empty-state">
                        <i class="fas fa-film"></i>
                        <h3>Không có dữ liệu</h3>
                        <p>${message}</p>
                    </td>
                </tr>
            `;
        }


        function showInitializationError() {
            console.error('Failed to initialize MovieManagement - Required elements not found');

            const containers = [
                document.querySelector('.movie-table-container'),
                document.querySelector('.main-content'),
                document.body
            ];
            
            const errorHtml = `
                <div class="alert alert-danger m-3">
                    <h4><i class="fas fa-exclamation-triangle"></i> Lỗi khởi tạo</h4>
                    <p>Không thể tải giao diện quản lý phim. Vui lòng thử tải lại trang.</p>
                    <button class="btn btn-primary" onclick="location.reload()">Tải lại trang</button>
                </div>
            `;
            
            for (const container of containers) {
                if (container) {
                    container.innerHTML = errorHtml;
                    break;
                }
            }
        }

        function openAddMovieModal() {
            const modal = document.getElementById('addMovieModal');
            if (!modal) {
                console.error('Không tìm thấy modal #addMovieModal');
                showNotification('Lỗi: Không thể mở form thêm phim mới', 'error');
                return;
            }
            
            const modalBody = modal.querySelector('.modal-body');
            
            modalBody.innerHTML = `
                <div class="loading-overlay" style="display: flex; justify-content: center; align-items: center; height: 200px;">
                    <div class="loading-spinner-small">
                        <i class="fas fa-spinner fa-spin"></i>
                    </div>
                    <p style="margin-left: 15px; color: white;">Đang tải dữ liệu...</p>
                </div>
            `;
            
            // Hiển thị modal trước khi tải dữ liệu
            modal.style.display = 'block';
            
            // Tải dữ liệu phòng chiếu và thể loại phim
            loadAddMovieData().then(() => {
                // Sau khi tải xong dữ liệu, hiển thị form
                renderAddMovieForm(modalBody);
            }).catch(error => {
                console.error('Lỗi khi tải dữ liệu:', error);
                modalBody.innerHTML = `
                    <div class="alert alert-danger">
                        <h5><i class="fas fa-exclamation-triangle"></i> Lỗi khi tải dữ liệu</h5>
                        <p>Không thể tải dữ liệu phòng chiếu và thể loại phim. Vui lòng thử lại sau.</p>
                        <button class="btn-retry" onclick="openAddMovieModal()">Thử lại</button>
                    </div>
                `;
            });
        }
        
        // Tải dữ liệu phòng chiếu và thể loại phim
        async function loadAddMovieData() {
            try {
                // Tải song song cả hai loại dữ liệu để tối ưu thời gian
                const [genresResponse, roomsResponse, actorsResponse, directorsResponse] = await Promise.all([
                    fetch('/MovieManagement/Movies/GetGenres'),
                    fetch('/MovieManagement/Movies/GetCinemaRooms'),
                    fetch('/MovieManagement/Movies/GetActors'),
                    fetch('/MovieManagement/Movies/GetDirectors')
                ]);
                
                const genresResult = await genresResponse.json();
                const roomsResult = await roomsResponse.json();
                const actorsResult = await actorsResponse.json();
                const directorsResult = await directorsResponse.json();
                
                console.log('Kết quả API thể loại:', genresResult);
                console.log('Kết quả API phòng chiếu:', roomsResult);
                console.log('Kết quả API diễn viên:', actorsResult);
                console.log('Kết quả API đạo diễn:', directorsResult);
                
                // Xử lý dữ liệu thể loại
                if (genresResult.success && genresResult.data) {
                    let genresData = genresResult.data;
                    
                    // Kiểm tra cấu trúc dữ liệu
                    if (genresData.data) {
                        genresData = genresData.data;
                    }
                    
                    if (Array.isArray(genresData)) {
                        window.availableGenres = genresData;
                        console.log('Đã tải', genresData.length, 'thể loại phim');
                    } else {
                        console.error('Dữ liệu thể loại không phải là mảng:', genresData);
                        window.availableGenres = [];
                    }
                } else {
                    console.error('Không thể tải thể loại phim:', genresResult.message || 'Lỗi không xác định');
                    window.availableGenres = [];
                }
                
                // Xử lý dữ liệu phòng chiếu
                if (roomsResult.success && roomsResult.data) {
                    let roomsData = roomsResult.data;
                    
                    // Kiểm tra cấu trúc dữ liệu
                    if (roomsData.data) {
                        roomsData = roomsData.data;
                    }
                    
                    if (Array.isArray(roomsData)) {
                        window.availableRooms = roomsData;
                        console.log('Đã tải', roomsData.length, 'phòng chiếu');
                    } else {
                        console.error('Dữ liệu phòng chiếu không phải là mảng:', roomsData);
                        window.availableRooms = [];
                    }
                } else {
                    console.error('Không thể tải phòng chiếu:', roomsResult.message || 'Lỗi không xác định');
                    window.availableRooms = [];
                }
                
                // Xử lý dữ liệu diễn viên
                if (actorsResult.success && actorsResult.data) {
                    let actorData = actorsResult.data;
                    if (actorData.data) actorData = actorData.data;
                    window.availableActors = Array.isArray(actorData) ? actorData : [];
                } else {
                    window.availableActors = [];
                }
                
                // Xử lý dữ liệu đạo diễn
                if (directorsResult.success && directorsResult.data) {
                    let directorData = directorsResult.data;
                    if (directorData.data) directorData = directorData.data;
                    window.availableDirectors = Array.isArray(directorData) ? directorData : [];
                } else {
                    window.availableDirectors = [];
                }
                
                return {
                    genres: window.availableGenres || [],
                    rooms: window.availableRooms || [],
                    actors: window.availableActors || [],
                    directors: window.availableDirectors || []
                };
            } catch (error) {
                console.error('Lỗi khi tải dữ liệu:', error);
                throw error;
            }
        }
        
        // Hiển thị form thêm phim
        function renderAddMovieForm(modalBody) {
            modalBody.innerHTML = `
                <form id="addMovieForm" onsubmit="createMovie(event)">
                    <div class="form-row">
                        <div class="form-group">
                            <label for="addTitle">Tên phim <span class="required-field">*</span></label>
                            <input type="text" id="addTitle" name="title" required>
                        </div>
                        <div class="form-group">
                            <label for="addReleaseDate">Ngày phát hành <span class="required-field">*</span></label>
                            <input type="date" id="addReleaseDate" name="releaseDate" required>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label for="addEndDate">Ngày kết thúc <span class="required-field">*</span></label>
                            <input type="date" id="addEndDate" name="endDate" required>
                        </div>
                        <div class="form-group">
                            <label for="addProductionCompany">Hãng sản xuất</label>
                            <input type="text" id="addProductionCompany" name="productionCompany">
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label for="addDirectorSelect">Đạo diễn <span class="required-field">*</span>
                                <button type="button" class="btn-add-small" onclick="openAddEntityPrompt('director')"><i class="bi bi-plus-lg"></i></button>
                            </label>
                            <select id="addDirectorSelect" multiple class="multi-select"></select>
                        </div>
                        <div class="form-group">
                            <label for="addActorsSelect">Diễn viên <span class="required-field">*</span>
                                <button type="button" class="btn-add-small" onclick="openAddEntityPrompt('actor')"><i class="bi bi-plus-lg"></i></button>
                            </label>
                            <select id="addActorsSelect" multiple class="multi-select"></select>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label for="addRunningTime">Thời lượng (phút) <span class="required-field">*</span></label>
                            <input type="number" id="addRunningTime" name="runningTime" min="1" required>
                        </div>
                        <div class="form-group">
                            <label for="addVersion">Phiên bản <span class="required-field">*</span></label>
                            <select id="addVersion" name="version" required style="background-color: #2a2a2a; color: white; border: 1px solid rgba(255,255,255,0.2);">
                                <option value="TwoD">2D</option>
                                <option value="ThreeD">3D</option>
                                <option value="FourDX">4DX</option>
                            </select>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label for="addRating">Rating</label>
                            <input type="number" id="addRating" name="rating" value="0" min="0" max="10" step="0.1">
                        </div>
                        <div class="form-group">
                            
                        </div>
                    </div>
                    
                    <div class="form-group full-width">
                        <label for="addContent">Mô tả phim</label>
                        <textarea id="addContent" name="content" rows="4" placeholder="Mô tả nội dung phim..."></textarea>
                    </div>
                    
                    
                    <div class="form-group full-width">
                        <label for="addGenres">Thể loại phim <span class="required-field">*</span></label>
                        <div class="genres-selection">
                            <div id="addGenreCheckboxes" class="genre-checkboxes">
                                
                                <div class="loading-genres">Đang tải thể loại phim...</div>
                            </div>
                        </div>
                        <small class="text-muted">Chọn ít nhất một thể loại</small>
                    </div>
                    
                    
                    <div class="form-group full-width">
                        <label>Lịch chiếu</label>
                        <div class="showtimes-section">
                            <div class="showtimes-header">
                                <button type="button" class="btn-add-showtime" onclick="addNewShowTimeEntry()">
                                    ➕ Thêm lịch chiếu
                                </button>
                            </div>
                            <div id="addShowTimesContainer" class="showtimes-container">
                                
                            </div>
                        </div>
                    </div>
                    
                    <div class="form-group full-width">
                        <label for="addTrailerUrl">Trailer</label>
                        <div class="trailer-upload-container">
                            <div class="upload-method-tabs">
                                <button type="button" class="tab-btn active" onclick="switchAddTrailerTab('url')">URL</button>
                                <button type="button" class="tab-btn" onclick="switchAddTrailerTab('upload')">Upload</button>
                            </div>
                            <div id="addTrailerUrlTab" class="upload-tab active">
                                <input type="url" id="addTrailerUrl" name="trailerUrl" placeholder="https://player.cloudinary.com/embed/?cloud_name=swp39imagel">
                            </div>
                            <div id="addTrailerUploadTab" class="upload-tab">
                                <input type="file" id="addTrailerFileInput" accept="video/*" style="display: none;">
                                <button type="button" class="btn-upload-file" onclick="document.getElementById('addTrailerFileInput').click()">
                                    📤 Chọn file video
                                </button>
                                <div id="addTrailerUploadProgress" class="upload-progress" style="display: none;">
                                    <div class="progress-bar"></div>
                                    <span class="progress-text">Đang upload...</span>
                                </div>
                                <div id="addTrailerUploadResult" class="upload-result"></div>
                            </div>
                        </div>
                        <small class="text-muted">Hỗ trợ MP4, MOV, AVI, MKV, WEBM</small>
                    </div>

                    <div class="form-group full-width">
                        <label>Hình ảnh phim <span class="required-field">*</span></label>
                        
                        <!-- Poster chính -->
                        <div class="poster-upload-section">
                            <h4>Poster chính <span class="required-field">*</span></h4>
                            <div class="image-upload-container">
                                <div class="upload-method-tabs">
                                    <button type="button" class="tab-btn active" onclick="switchAddPosterTab('url')">URL</button>
                                    <button type="button" class="tab-btn" onclick="switchAddPosterTab('upload')">Upload</button>
                                </div>
                                <div id="addPosterUrlTab" class="upload-tab active">
                                    <input type="url" id="addImageUrl" name="imageUrl" placeholder="https://upload.wikimedia.org/wiki">
                                </div>
                                <div id="addPosterUploadTab" class="upload-tab">
                                    <input type="file" id="addPosterFileInput" accept="image/*" style="display: none;">
                                    <button type="button" class="btn-upload-file" onclick="document.getElementById('addPosterFileInput').click()">
                                        📤 Chọn hình ảnh
                                    </button>
                                    <div id="addPosterUploadProgress" class="upload-progress" style="display: none;">
                                        <div class="progress-bar"></div>
                                        <span class="progress-text">Đang upload...</span>
                                    </div>
                                </div>
                                <div class="image-preview">
                                    <img src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='300' height='450' viewBox='0 0 300 450' fill='none'%3E%3Crect width='300' height='450' fill='%232a2a2a'/%3E%3Ctext x='50%25' y='50%25' font-family='Arial' font-size='16' fill='%23666' text-anchor='middle' dominant-baseline='middle'%3EPoster image%3C/text%3E%3C/svg%3E" 
                                         alt="Poster preview" class="poster-preview" id="addPosterPreview"
                                         loading="lazy">
                                    <span class="poster-label">Poster chính</span>
                                    <div class="image-meta">
                                        <input type="text" id="addPosterDescription" name="posterDescription" placeholder="Mô tả hình ảnh..." class="image-description-input">
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Hình ảnh bổ sung -->
                        <div class="additional-images-section">
                            <h4>Hình ảnh bổ sung</h4>
                            <div class="additional-images-upload">
                                <input type="file" id="addAdditionalImagesInput" accept="image/*" multiple style="display: none;">
                                <button type="button" class="btn-upload-multiple" onclick="document.getElementById('addAdditionalImagesInput').click()">
                                    📤 Thêm nhiều hình ảnh
                                </button>
                                <div id="addAdditionalUploadProgress" class="upload-progress" style="display: none;">
                                    <div class="progress-bar"></div>
                                    <span class="progress-text">Đang upload...</span>
                                </div>
                            </div>
                            <div id="addAdditionalImagesContainer" class="additional-images-grid">
                                
                            </div>
                        </div>
                        
                        <small class="text-muted">Hỗ trợ PNG, JPG, JPEG, GIF, WEBP. Có thể upload nhiều hình cùng lúc.</small>
                    </div>
                    
                    <div class="form-row checkbox-row">
                        <div class="form-group">
                            <label class="checkbox-label">
                                <input type="checkbox" id="addIsFeatured" name="isFeatured">
                                <span class="checkmark"></span>
                                Phim nổi bật
                            </label>
                        </div>
                        <div class="form-group">
                            <label class="checkbox-label">
                                <input type="checkbox" id="addIsRecommended" name="isRecommended">
                                <span class="checkmark"></span>
                                Phim đề xuất
                            </label>
                        </div>
                    </div>

                    <div class="form-actions">
                        <button type="button" class="btn-cancel" onclick="closeModal('addMovieModal')">Hủy</button>
                        <button type="submit" class="btn-save" id="createMovieBtn">
                            <span class="btn-text">💾 Thêm phim mới</span>
                            <span class="btn-loading" style="display: none;">
                                <i class="fas fa-spinner fa-spin"></i> Đang thêm phim...
                            </span>
                        </button>
                    </div>
                    <div id="addValidationErrors" class="validation-errors" style="display: none;"></div>
                </form>
            `;
            

            setupAddMovieEventListeners();
            

            if (window.availableGenres && Array.isArray(window.availableGenres)) {
                populateAddGenres(window.availableGenres);
            } else {
                console.warn('Không có dữ liệu thể loại phim');
                document.getElementById('addGenreCheckboxes').innerHTML = '<div class="no-genres">Không có thể loại phim nào</div>';
            }
            

            addNewShowTimeEntry();

            if (window.availableActors) {
                const actorSelect = document.getElementById('addActorsSelect');
                window.availableActors.forEach(a => {
                    const opt = document.createElement('option');
                    opt.value = a.id || a.Id;
                    opt.textContent = a.name || a.Name;
                    actorSelect.appendChild(opt);
                });
            }

            if (window.availableDirectors) {
                const directorSelect = document.getElementById('addDirectorSelect');
                window.availableDirectors.forEach(d => {
                    const opt = document.createElement('option');
                    opt.value = d.id || d.Id;
                    opt.textContent = d.name || d.Name;
                    directorSelect.appendChild(opt);
                });
            }

            // After populating actor & director options in renderAddMovieForm
            if (window.$ && $.fn.select2) {
                $('#addActorsSelect').select2({ placeholder: 'Chọn diễn viên', width: '100%', dropdownParent: $('#addMovieModal') });
                $('#addDirectorSelect').select2({ placeholder: 'Chọn đạo diễn', width: '100%', dropdownParent: $('#addMovieModal') });
            }
        }


        function showMovieDetailModal(movieData) {
            console.log('Showing detail modal with data:', movieData); // Debug log
            
            const modal = document.getElementById('movieDetailModal');
            const modalBody = modal.querySelector('.modal-body');
            

            const title = movieData.title || movieData.Title || 'Không có tiêu đề';
            const director = movieData.director || movieData.Director || 'N/A';
            const actors = movieData.actors || movieData.Actors || 'N/A';
            const productionCompany = movieData.productionCompany || movieData.ProductionCompany || 'N/A';
            const runningTime = movieData.runningTime || movieData.RunningTime || 'N/A';
            const releaseDate = movieData.releaseDate || movieData.ReleaseDate;
            const status = movieData.status || movieData.Status || 0;
            const rating = movieData.rating || movieData.Rating || 0;
            const content = movieData.content || movieData.Content || 'Chưa có mô tả';
            const trailerUrl = movieData.trailerUrl || movieData.TrailerUrl;
            const isFeatured = movieData.isFeatured || movieData.IsFeatured || false;
            const isRecommended = movieData.isRecommended || movieData.IsRecommended || false;
            

            const posterUrl = getMovieImageUrl(movieData);
            

            let genresHtml = '';
            if (movieData.genres && Array.isArray(movieData.genres) && movieData.genres.length > 0) {
                genresHtml = movieData.genres.map(g => 
                    `<span class="movie-genre-pill">${g.name || g.Name || ''}</span>`
                ).join('');
            } else if (movieData.Genres && Array.isArray(movieData.Genres) && movieData.Genres.length > 0) {
                genresHtml = movieData.Genres.map(g => 
                    `<span class="movie-genre-pill">${g.name || g.Name || ''}</span>`
                ).join('');
            } else {
                genresHtml = '<span class="movie-genre-pill">Chưa phân loại</span>';
            }
            

            const ratingValue = parseFloat(rating) || 0;
            const fullStars = Math.floor(ratingValue);
            const halfStar = ratingValue % 1 >= 0.5;
            const emptyStars = 5 - fullStars - (halfStar ? 1 : 0);
            
            let starsHtml = '';
            for (let i = 0; i < fullStars; i++) {
                starsHtml += '<i class="fas fa-star"></i>';
            }
            if (halfStar) {
                starsHtml += '<i class="fas fa-star-half-alt"></i>';
            }
            for (let i = 0; i < emptyStars; i++) {
                starsHtml += '<i class="far fa-star"></i>';
            }
            

            const statusClass = status === 2 ? 'status-active' : (status === 1 ? 'status-coming' : 'status-stopped');
            const statusIcon = status === 2 ? 'fa-play-circle' : (status === 1 ? 'fa-clock' : 'fa-stop-circle');
            
            modalBody.innerHTML = `
                <div class="movie-detail-modern">
                    <div class="movie-detail-backdrop" style="background-image: linear-gradient(180deg, rgba(0,0,0,0) 0%, rgba(18,18,18,1) 100%), url('${posterUrl}');"></div>
                    
                <div class="movie-detail-content">
                    <div class="movie-detail-header">
                            <div class="movie-detail-poster-wrapper">
                        <img src="${posterUrl}" 
                             alt="${title}" class="movie-detail-poster"
                                     loading="lazy"
                                     onerror="this.onerror=null; this.src='data:image/svg+xml,%3Csvg xmlns=\'http://www.w3.org/2000/svg\' width=\'200\' height=\'300\' viewBox=\'0 0 200 300\' fill=\'none\'%3E%3Crect width=\'200\' height=\'300\' fill=\'%232a2a2a\'/%3E%3Ctext x=\'50%25\' y=\'50%25\' font-family=\'Arial\' font-size=\'14\' fill=\'%23666\' text-anchor=\'middle\' dominant-baseline=\'middle\'%3ENo poster available%3C/text%3E%3C/svg%3E'">
                                
                                <div class="movie-status ${statusClass}">
                                    <i class="fas ${statusIcon}"></i> ${getStatusText(status)}
                                </div>
                            </div>
                            
                        <div class="movie-detail-info">
                                <h1 class="movie-title">${title}</h1>
                                
                                <div class="movie-meta">
                                    <div class="movie-year">
                                        <i class="far fa-calendar-alt"></i>
                                        ${releaseDate ? new Date(releaseDate).getFullYear() : 'N/A'}
                                    </div>
                                    <div class="movie-runtime">
                                        <i class="far fa-clock"></i>
                                        ${runningTime} phút
                                </div>
                                    <div class="movie-rating">
                                        <div class="rating-stars">
                                            ${starsHtml}
                            </div>
                                        <div class="rating-number">
                                            ${ratingValue.toFixed(1)}/10
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="movie-genres">
                                    ${genresHtml}
                                </div>
                                
                                <div class="movie-tags">
                                    ${isFeatured ? '<span class="movie-tag featured"><i class="fas fa-certificate"></i> Nổi bật</span>' : ''}
                                    ${isRecommended ? '<span class="movie-tag recommended"><i class="fas fa-thumbs-up"></i> Đề xuất</span>' : ''}
                                </div>
                                
                                <div class="movie-overview">
                        <p>${content}</p>
                    </div>
                            </div>
                        </div>
                        
                        <div class="movie-detail-info-grid">
                            <div class="info-grid-item">
                                <div class="info-label">Đạo diễn</div>
                                <div class="info-value">${director}</div>
                            </div>
                            <div class="info-grid-item">
                                <div class="info-label">Diễn viên</div>
                                <div class="info-value">${actors}</div>
                            </div>
                            <div class="info-grid-item">
                                <div class="info-label">Công ty sản xuất</div>
                                <div class="info-value">${productionCompany}</div>
                            </div>
                            <div class="info-grid-item">
                                <div class="info-label">Ngày phát hành</div>
                                <div class="info-value">${releaseDate ? new Date(releaseDate).toLocaleDateString('vi-VN') : 'N/A'}</div>
                            </div>
                        </div>
                        
                    ${trailerUrl ? `
                        <div class="movie-trailer-section">
                            <h3 class="section-title"><i class="fas fa-film"></i> Trailer</h3>
                            <div class="trailer-container">
                                <div class="trailer-embed-wrapper">
                                    <iframe 
                                        src="${getEmbedUrl(trailerUrl)}" 
                                        frameborder="0" 
                                        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" 
                                        allowfullscreen
                                        class="trailer-iframe">
                                    </iframe>
                                </div>
                            </div>
                    </div>
                    ` : ''}
                    </div>
            </div>
            `;
            
            modal.style.display = 'block';
        }

        function showEditMovieModal(movieData) {
            console.log('Showing edit modal with data:', movieData); // Debug log
            
            const modal = document.getElementById('editMovieModal');
            const modalBody = modal.querySelector('.modal-body');
            

            modal.setAttribute('data-movie-data', JSON.stringify(movieData));
            

            const id = movieData.id || movieData.Id || '';
            const title = movieData.title || movieData.Title || '';
            const director = movieData.director || movieData.Director || '';
            const actors = movieData.actors || movieData.Actors || '';
            const productionCompany = movieData.productionCompany || movieData.ProductionCompany || '';
            const runningTime = movieData.runningTime || movieData.RunningTime || '';
            const releaseDate = movieData.releaseDate || movieData.ReleaseDate;
            const endDate = movieData.endDate || movieData.EndDate;
            const rating = movieData.rating || movieData.Rating || 0;
            const content = movieData.content || movieData.Content || '';
            const trailerUrl = movieData.trailerUrl || movieData.TrailerUrl || '';
            const isFeatured = movieData.isFeatured || movieData.IsFeatured || false;
            const isRecommended = movieData.isRecommended || movieData.IsRecommended || false;
            const version = movieData.version || movieData.Version || '2D';
            

            const posterUrl = getMovieImageUrl(movieData);
            
            modalBody.innerHTML = `
                <form id="editMovieForm" onsubmit="updateMovie(event, '${id}')">
                    <div class="form-row">
                        <div class="form-group">
                            <label for="editTitle">Tên phim <span class="required-field">*</span></label>
                            <input type="text" id="editTitle" name="title" value="${title}" required>
                        </div>
                        <div class="form-group">
                            <label for="editReleaseDate">Ngày phát hành <span class="required-field">*</span></label>
                            <input type="date" id="editReleaseDate" name="releaseDate" 
                                   value="${releaseDate ? releaseDate.split('T')[0] : ''}" required>
                    </div>
                </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label for="editEndDate">Ngày kết thúc <span class="required-field">*</span></label>
                            <input type="date" id="editEndDate" name="endDate" 
                                   value="${endDate ? endDate.split('T')[0] : ''}" required>
                        </div>
                        <div class="form-group">
                            <label for="editProductionCompany">Hãng sản xuất</label>
                            <input type="text" id="editProductionCompany" name="productionCompany" value="${productionCompany}">
                    </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label for="editDirectorInput" style="width:100%">Đạo diễn <span class="required-field">*</span></label>
                            <select id="editDirectorInput" multiple class="multi-select"></select>
                            <button type="button" class="btn-add-small" style="position:absolute; right:6px; top:30px" onclick="openAddEntityPrompt('director')"><i class="bi bi-plus-lg"></i></button>
                        </div>
                        <div class="form-group">
                            <label for="editActorsInput" style="width:100%">Diễn viên <span class="required-field">*</span></label>
                            <select id="editActorsInput" multiple class="multi-select"></select>
                            <button type="button" class="btn-add-small" style="position:absolute; right:6px; top:30px" onclick="openAddEntityPrompt('actor')"><i class="bi bi-plus-lg"></i></button>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label for="editRunningTime">Thời lượng (phút) <span class="required-field">*</span></label>
                            <input type="number" id="editRunningTime" name="runningTime" value="${runningTime}" min="1" required>
                </div>
                        <div class="form-group">
                            <label for="editVersion">Phiên bản <span class="required-field">*</span></label>
                            <select id="editVersion" name="version" required>
                                <option value="TwoD" ${(version === 1 || version === 'TwoD' || version === '2D') ? 'selected' : ''}>2D</option>
                                <option value="ThreeD" ${(version === 2 || version === 'ThreeD' || version === '3D') ? 'selected' : ''}>3D</option>
                                <option value="FourDX" ${(version === 3 || version === 'FourDX' || version === '4DX') ? 'selected' : ''}>4DX</option>
                            </select>
                    </div>
                </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label for="editRating">Rating</label>
                            <input type="number" id="editRating" name="rating" value="${rating}" 
                                   min="0" max="10" step="0.1">
            </div>
                        <div class="form-group">
                            
            </div>
        </div>
                    
                    <div class="form-group full-width">
                        <label for="editContent">Mô tả phim</label>
                        <textarea id="editContent" name="content" rows="4" placeholder="Mô tả nội dung phim...">${content}</textarea>
                </div>
                    
                    
                    <div class="form-group full-width">
                        <label for="editGenres">Thể loại phim <span class="required-field">*</span></label>
                        <div class="genres-selection">
                            <div id="genreCheckboxes" class="genre-checkboxes">
                                
                </div>
                        </div>
                        <small class="text-muted">Chọn ít nhất một thể loại</small>
                    </div>
                    
                    
                    <div class="form-group full-width">
                        <label>Lịch chiếu</label>
                        <div class="showtimes-section">
                            <div class="showtimes-header">
                                <button type="button" class="btn-add-showtime" onclick="addNewShowTimeEntry()">
                                    ➕ Thêm lịch chiếu
                                </button>
                            </div>
                            <div id="showTimesContainer" class="showtimes-container">
                                
                            </div>
                        </div>
                    </div>
                    
                    <div class="form-group full-width">
                        <label for="editTrailerUrl">Trailer</label>
                        <div class="trailer-upload-container">
                            <div class="upload-method-tabs">
                                <button type="button" class="tab-btn active" onclick="switchTrailerTab('url')">URL</button>
                                <button type="button" class="tab-btn" onclick="switchTrailerTab('upload')">Upload</button>
                            </div>
                            <div id="trailerUrlTab" class="upload-tab active">
                                <input type="url" id="editTrailerUrl" name="trailerUrl" value="${trailerUrl}" 
                                       placeholder="https://player.cloudinary.com/embed/?cloud_name=swp39imagel">
                            </div>
                            <div id="trailerUploadTab" class="upload-tab">
                                <input type="file" id="trailerFileInput" accept="video/*" style="display: none;">
                                <button type="button" class="btn-upload-file" onclick="document.getElementById('trailerFileInput').click()">
                                    📤 Chọn file video
                    </button>
                                <div id="trailerUploadProgress" class="upload-progress" style="display: none;">
                                    <div class="progress-bar"></div>
                                    <span class="progress-text">Đang upload...</span>
                </div>
                                <div id="trailerUploadResult" class="upload-result"></div>
            </div>
        </div>
                        <small class="text-muted">Hỗ trợ MP4, MOV, AVI, MKV, WEBM</small>
    </div>

                    <div class="form-group full-width">
                        <label>Hình ảnh phim <span class="required-field">*</span></label>
                        
                        <!-- Poster chính -->
                        <div class="poster-upload-section">
                            <h4>Poster chính <span class="required-field">*</span></h4>
                            <div class="image-upload-container">
                                <div class="upload-method-tabs">
                                    <button type="button" class="tab-btn active" onclick="switchPosterTab('url')">URL</button>
                                    <button type="button" class="tab-btn" onclick="switchPosterTab('upload')">Upload</button>
                                </div>
                                <div id="posterUrlTab" class="upload-tab active">
                                    <input type="url" id="editImageUrl" name="imageUrl" value="${posterUrl}" 
                                           placeholder="https://upload.wikimedia.org/wiki">
                                </div>
                                <div id="posterUploadTab" class="upload-tab">
                                    <input type="file" id="posterFileInput" accept="image/*" style="display: none;">
                                    <button type="button" class="btn-upload-file" onclick="document.getElementById('posterFileInput').click()">
                                        📤 Chọn hình ảnh
                                    </button>
                                    <div id="posterUploadProgress" class="upload-progress" style="display: none;">
                                        <div class="progress-bar"></div>
                                        <span class="progress-text">Đang upload...</span>
                                    </div>
                                </div>
                                <div class="image-preview">
                                    <img src="${posterUrl || 'data:image/svg+xml,%3Csvg xmlns=\'http://www.w3.org/2000/svg\' width=\'300\' height=\'450\' viewBox=\'0 0 300 450\' fill=\'none\'%3E%3Crect width=\'300\' height=\'450\' fill=\'%232a2a2a\'/%3E%3Ctext x=\'50%25\' y=\'50%25\' font-family=\'Arial\' font-size=\'16\' fill=\'%23666\' text-anchor=\'middle\' dominant-baseline=\'middle\'%3EPoster image%3C/text%3E%3C/svg%3E'}" 
                                         alt="Poster preview" class="poster-preview" id="posterPreview"
                                         loading="lazy"
                                         onerror="this.onerror=null; this.src='data:image/svg+xml,%3Csvg xmlns=\'http://www.w3.org/2000/svg\' width=\'300\' height=\'450\' viewBox=\'0 0 300 450\' fill=\'none\'%3E%3Crect width=\'300\' height=\'450\' fill=\'%232a2a2a\'/%3E%3Ctext x=\'50%25\' y=\'50%25\' font-family=\'Arial\' font-size=\'16\' fill=\'%23666\' text-anchor=\'middle\' dominant-baseline=\'middle\'%3EPoster image%3C/text%3E%3C/svg%3E'">
                                    <span class="poster-label">Poster chính</span>
                                    <div class="image-meta">
                                        <input type="text" id="posterDescription" name="posterDescription" placeholder="Mô tả hình ảnh..." class="image-description-input">
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Hình ảnh bổ sung -->
                        <div class="additional-images-section">
                            <h4>Hình ảnh bổ sung</h4>
                            <div class="additional-images-upload">
                                <input type="file" id="additionalImagesInput" accept="image/*" multiple style="display: none;">
                                <button type="button" class="btn-upload-multiple" onclick="document.getElementById('additionalImagesInput').click()">
                                    📤 Thêm nhiều hình ảnh
                                </button>
                                <div id="additionalUploadProgress" class="upload-progress" style="display: none;">
                                    <div class="progress-bar"></div>
                                    <span class="progress-text">Đang upload...</span>
                                </div>
                            </div>
                            <div id="additionalImagesContainer" class="additional-images-grid">
                                
                            </div>
                        </div>
                        
                        <small class="text-muted">Hỗ trợ PNG, JPG, JPEG, GIF, WEBP. Có thể upload nhiều hình cùng lúc.</small>
                    </div>
                    
                    <div class="form-row checkbox-row">
                        <div class="form-group">
                            <label class="checkbox-label">
                                <input type="checkbox" id="editIsFeatured" name="isFeatured" ${isFeatured ? 'checked' : ''}>
                                <span class="checkmark"></span>
                                Phim nổi bật
                            </label>
                        </div>
                        <div class="form-group">
                            <label class="checkbox-label">
                                <input type="checkbox" id="editIsRecommended" name="isRecommended" ${isRecommended ? 'checked' : ''}>
                                <span class="checkmark"></span>
                                Phim đề xuất
                            </label>
                        </div>
                    </div>

                    <div class="form-actions">
                        <button type="button" class="btn-back" onclick="window.location.href='/MovieManagement/Movies'">
                            <i class="fas fa-arrow-left"></i> Quay lại
                        </button>
                        <button type="button" class="btn-cancel" onclick="closeModal('editMovieModal')">Hủy</button>
                        <button type="submit" class="btn-save" id="updateMovieBtn">
                            <span class="btn-text">🔄 Cập nhật phim</span>
                            <span class="btn-loading" style="display: none;">
                                <i class="fas fa-spinner fa-spin"></i> Đang cập nhật...
                            </span>
                        </button>
                    </div>
                    <div id="validationErrors" class="validation-errors" style="display: none;"></div>
                </form>
            `;
            

            setupUploadEventListeners();
            

            loadPosterDescription(movieData);
            

            loadExistingAdditionalImages(movieData);
            

            loadGenresAndRooms(movieData);
            
            modal.style.display = 'block';

            // Inside showEditMovieModal, after loadGenresAndRooms(movieData);
            (async function(){
                try{
                    if(!window.availableActors||!window.availableDirectors){await loadAddMovieData();}
                    const actorSelect=document.getElementById('editActorsInput');
                    const directorSelect=document.getElementById('editDirectorInput');
                    if(actorSelect&&window.availableActors){actorSelect.innerHTML='';window.availableActors.forEach(a=>{const opt=document.createElement('option');opt.value=a.id||a.Id;opt.textContent=a.name||a.Name;actorSelect.appendChild(opt);});}
                    if(directorSelect&&window.availableDirectors){directorSelect.innerHTML='';window.availableDirectors.forEach(d=>{const opt=document.createElement('option');opt.value=d.id||d.Id;opt.textContent=d.name||d.Name;directorSelect.appendChild(opt);});}
                    if(window.$&&$.fn.select2){
                        $(actorSelect).select2({placeholder:'Chọn diễn viên',width:'100%', dropdownParent: $('#editMovieModal')});
                        $(directorSelect).select2({placeholder:'Chọn đạo diễn',width:'100%', dropdownParent: $('#editMovieModal')});

                        // Xác định danh sách ID diễn viên/đạo diễn hiện có của phim
                        let actorIds = movieData.actorIds || (movieData.actorList ? movieData.actorList.map(a=>a.id||a.Id) : []);
                        let directorIds = movieData.directorIds || (movieData.directorList ? movieData.directorList.map(d=>d.id||d.Id) : []);

                        // Đảm bảo option tồn tại trước khi set value
                        actorIds.forEach(aid=>{
                            if(!actorSelect.querySelector(`option[value="${aid}"]`)){
                                // Thêm option tạm nếu thiếu
                                const dataObj = (movieData.actorList||[]).find(x=>(x.id||x.Id)===aid) || {};
                                const opt=document.createElement('option');
                                opt.value=aid;
                                opt.textContent=dataObj.name||dataObj.Name||aid;
                                actorSelect.appendChild(opt);
                            }
                        });
                        directorIds.forEach(did=>{
                            if(!directorSelect.querySelector(`option[value="${did}"]`)){
                                const dataObj = (movieData.directorList||[]).find(x=>(x.id||x.Id)===did) || {};
                                const opt=document.createElement('option');
                                opt.value=did;
                                opt.textContent=dataObj.name||dataObj.Name||did;
                                directorSelect.appendChild(opt);
                            }
                        });

                        if(actorIds && actorIds.length){$(actorSelect).val(actorIds.map(String)).trigger('change');}
                        if(directorIds && directorIds.length){$(directorSelect).val(directorIds.map(String)).trigger('change');}
                    }

                    /* === Load showtimes via API and render === */
                    try{
                        const stRes = await fetch(`/api/v1/showtime/movie/${id}`);
                        const stJson = await stRes.json();
                        if(stJson && stJson.data && Array.isArray(stJson.data) && stJson.data.length){
                            // Chờ availableRooms được load trước khi render
                            const waitForRooms = ()=>new Promise(r=>{
                                const check=()=>{ if(window.availableRooms) r(); else setTimeout(check,100);} ; check(); });
                            await waitForRooms();
                            // Xóa showTimesContainer trước khi nạp mới (tránh nhân đôi)
                            const cont = document.getElementById('showTimesContainer');
                            if(cont) cont.innerHTML='';
                            const showArr = Array.isArray(stJson.data) ? stJson.data : (stJson.data.data||[]);
                            showArr.forEach(st=>{
                                let showDate = st.showDate || st.ShowDate;
                                const roomId = st.cinemaRoomId || st.roomId || st.RoomId;
                                if(showDate){
                                    const dt=new Date(showDate);
                                    if(!isNaN(dt.getTime())){
                                        const year=dt.getFullYear();
                                        const month=String(dt.getMonth()+1).padStart(2,'0');
                                        const day=String(dt.getDate()).padStart(2,'0');
                                        const hours=String(dt.getHours()).padStart(2,'0');
                                        const minutes=String(dt.getMinutes()).padStart(2,'0');
                                        showDate=`${year}-${month}-${day}T${hours}:${minutes}`;
                                    }
                                }
                                addShowTimeEntry(showDate, roomId);
                            });
                            // Nếu không có suất chiếu, thêm 1 dòng trống
                            if(cont && cont.children.length===0) addShowTimeEntry();
                        }
                    }catch(err){
                        console.error('Load showtimes error',err);
                        // Nếu API thất bại, thử dùng dữ liệu showTimes có sẵn trong movieData (nếu có)
                        if(movieData.showTimes || movieData.ShowTimes){
                            const cont=document.getElementById('showTimesContainer');
                            if(cont) cont.innerHTML='';
                            const arr = movieData.showTimes || movieData.ShowTimes;
                            arr.forEach(st=>{
                                let showDate=st.showDate||st.ShowDate;
                                const roomId=st.roomId||st.RoomId;
                                if(showDate){
                                    const dt=new Date(showDate);
                                    if(!isNaN(dt.getTime())){
                                        const y=dt.getFullYear();
                                        const m=String(dt.getMonth()+1).padStart(2,'0');
                                        const d=String(dt.getDate()).padStart(2,'0');
                                        const h=String(dt.getHours()).padStart(2,'0');
                                        const min=String(dt.getMinutes()).padStart(2,'0');
                                        showDate=`${y}-${m}-${d}T${h}:${min}`;
                                    }
                                }
                                addShowTimeEntry(showDate, roomId);
                            });
                        }
                    }                    
                 }catch(e){console.error('Init Select2 error',e);}                 
             })();
        }

        function loadPosterDescription(movieData) {
            const posterDescInput = document.getElementById('posterDescription');
            if (posterDescInput && movieData.images) {
                const images = movieData.images || movieData.Images || [];
                const primaryImage = images.find(img => img.isPrimary || img.IsPrimary);
                if (primaryImage) {
                    const description = primaryImage.description || primaryImage.Description || '';
                    posterDescInput.value = description;
                }
            }
        }

        function loadExistingAdditionalImages(movieData) {
            const container = document.getElementById('additionalImagesContainer');
            container.innerHTML = ''; // Clear existing


            const allImages = [];
            if (movieData.images || movieData.Images) {
                const images = movieData.images || movieData.Images;
                if (Array.isArray(images)) {
                    images.forEach(img => {
                        const isPrimary = img.isPrimary || img.IsPrimary || false;
                        const imageUrl = img.imageUrl || img.ImageUrl || '';
                        const description = img.description || img.Description || '';
                        
                        if (!isPrimary && imageUrl) {
                            allImages.push({
                                url: imageUrl,
                                name: `Hình ảnh ${allImages.length + 1}`,
                                description: description
                            });
                        }
                    });
                }
            }


            if (allImages.length > 0) {
                displayAdditionalImages(allImages, container);
            }
        }


        function setupUploadEventListeners() {

            const imageUrlInput = document.getElementById('editImageUrl');
            const posterPreview = document.getElementById('posterPreview');
            
            if (imageUrlInput) {
                imageUrlInput.addEventListener('input', function() {
                    if (this.value) {
                        posterPreview.src = this.value;
                        posterPreview.onerror = function() {
                            this.src = '/images/placeholder-movie.jpg';
                        };
                    }
                });
            }


            const trailerFileInput = document.getElementById('trailerFileInput');
            if (trailerFileInput) {
                trailerFileInput.addEventListener('change', handleTrailerUpload);
            }


            const posterFileInput = document.getElementById('posterFileInput');
            if (posterFileInput) {
                posterFileInput.addEventListener('change', handlePosterUpload);
            }


            const additionalImagesInput = document.getElementById('additionalImagesInput');
            if (additionalImagesInput) {
                additionalImagesInput.addEventListener('change', handleAdditionalImagesUpload);
            }
        }


        function switchTrailerTab(tab) {
            const urlTab = document.getElementById('trailerUrlTab');
            const uploadTab = document.getElementById('trailerUploadTab');
            const buttons = document.querySelectorAll('.trailer-upload-container .tab-btn');
            
            buttons.forEach(btn => btn.classList.remove('active'));
            
            if (tab === 'url') {
                urlTab.classList.add('active');
                uploadTab.classList.remove('active');
                buttons[0].classList.add('active');
            } else {
                urlTab.classList.remove('active');
                uploadTab.classList.add('active');
                buttons[1].classList.add('active');
            }
        }

        function switchPosterTab(tab) {
            const urlTab = document.getElementById('posterUrlTab');
            const uploadTab = document.getElementById('posterUploadTab');
            const buttons = document.querySelectorAll('.poster-upload-section .tab-btn');
            
            buttons.forEach(btn => btn.classList.remove('active'));
            
            if (tab === 'url') {
                urlTab.classList.add('active');
                uploadTab.classList.remove('active');
                buttons[0].classList.add('active');
            } else {
                urlTab.classList.remove('active');
                uploadTab.classList.add('active');
                buttons[1].classList.add('active');
            }
        }


        async function handleTrailerUpload(event) {
            const file = event.target.files[0];
            if (!file) return;

            const progressDiv = document.getElementById('trailerUploadProgress');
            const progressBar = progressDiv.querySelector('.progress-bar');
            const resultDiv = document.getElementById('trailerUploadResult');
            
            try {

                progressDiv.style.display = 'block';
                progressBar.style.width = '0%';
                resultDiv.innerHTML = '';


                const progressInterval = setInterval(() => {
                    const currentWidth = parseInt(progressBar.style.width) || 0;
                    if (currentWidth < 90) {
                        progressBar.style.width = (currentWidth + 10) + '%';
                    }
                }, 200);


                const formData = new FormData();
                formData.append('file', file);

                const response = await fetch('/MovieManagement/Movies/UploadVideo', {
                    method: 'POST',
                    body: formData
                });

                const result = await response.json();

                clearInterval(progressInterval);
                progressBar.style.width = '100%';

                if (result.success) {

                    const trailerUrlInput = document.getElementById('editTrailerUrl');
                    trailerUrlInput.value = result.videoUrl;
                    
                    resultDiv.innerHTML = `<div class="upload-success">✅ Upload thành công!</div>`;
                    

                    switchTrailerTab('url');
                } else {
                    resultDiv.innerHTML = `<div class="upload-error">❌ ${result.message}</div>`;
                }

                setTimeout(() => {
                    progressDiv.style.display = 'none';
                }, 2000);

            } catch (error) {
                console.error('Error uploading video:', error);
                resultDiv.innerHTML = `<div class="upload-error">❌ Lỗi khi upload video</div>`;
                progressDiv.style.display = 'none';
            }
        }

        async function handlePosterUpload(event) {
            const file = event.target.files[0];
            if (!file) return;

            const progressDiv = document.getElementById('posterUploadProgress');
            const progressBar = progressDiv.querySelector('.progress-bar');
            const posterPreview = document.getElementById('posterPreview');
            
            try {

                progressDiv.style.display = 'block';
                progressBar.style.width = '0%';


                const progressInterval = setInterval(() => {
                    const currentWidth = parseInt(progressBar.style.width) || 0;
                    if (currentWidth < 90) {
                        progressBar.style.width = (currentWidth + 10) + '%';
                    }
                }, 200);


                const formData = new FormData();
                formData.append('file', file);

                const response = await fetch('/MovieManagement/Movies/UploadImage', {
                    method: 'POST',
                    body: formData
                });

                const result = await response.json();

                clearInterval(progressInterval);
                progressBar.style.width = '100%';

                if (result.success) {

                    const imageUrlInput = document.getElementById('editImageUrl');
                    imageUrlInput.value = result.imageUrl;
                    posterPreview.src = result.imageUrl;
                    
                    showNotification('Upload poster thành công!', 'success');
                    

                    switchPosterTab('url');
                } else {
                    showNotification('Lỗi: ' + result.message, 'error');
                }

                setTimeout(() => {
                    progressDiv.style.display = 'none';
                }, 2000);

            } catch (error) {
                console.error('Error uploading image:', error);
                showNotification('Lỗi khi upload hình ảnh', 'error');
                progressDiv.style.display = 'none';
            }
        }

        async function handleAdditionalImagesUpload(event) {
            const files = Array.from(event.target.files);
            if (files.length === 0) return;

            const progressDiv = document.getElementById('additionalUploadProgress');
            const progressBar = progressDiv.querySelector('.progress-bar');
            const container = document.getElementById('additionalImagesContainer');
            
            try {

                progressDiv.style.display = 'block';
                progressBar.style.width = '0%';

                const uploadedImages = [];
                const totalFiles = files.length;

                for (let i = 0; i < files.length; i++) {
                    const file = files[i];
                    

                    const progress = ((i + 1) / totalFiles) * 100;
                    progressBar.style.width = progress + '%';

                    try {
                        const formData = new FormData();
                        formData.append('file', file);

                        const response = await fetch('/MovieManagement/Movies/UploadImage', {
                            method: 'POST',
                            body: formData
                        });

                        const result = await response.json();

                        if (result.success) {
                            uploadedImages.push({
                                url: result.imageUrl,
                                name: file.name
                            });
                        }
                    } catch (error) {
                        console.error(`Error uploading ${file.name}:`, error);
                    }
                }


                displayAdditionalImages(uploadedImages, container);
                
                showNotification(`Đã upload ${uploadedImages.length}/${totalFiles} hình ảnh thành công!`, 'success');

                setTimeout(() => {
                    progressDiv.style.display = 'none';
                }, 2000);

            } catch (error) {
                console.error('Error uploading images:', error);
                showNotification('Lỗi khi upload hình ảnh', 'error');
                progressDiv.style.display = 'none';
            }
        }

        function displayAdditionalImages(images, container) {
            images.forEach((image, index) => {
                const imageDiv = document.createElement('div');
                imageDiv.className = 'additional-image-item';
                imageDiv.innerHTML = `
                    <img src="${image.url}" alt="${image.name}" class="additional-image-preview">
                    <div class="image-overlay">
                        <span class="image-name">${image.name}</span>
                        <button type="button" class="btn-remove-image" onclick="removeAdditionalImage(this)">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                    <div class="image-meta">
                        <input type="text" class="additional-image-description" placeholder="Mô tả hình ảnh..." value="${image.description || ''}">
                    </div>
                `;
                container.appendChild(imageDiv);
            });
        }

        function removeAdditionalImage(button) {
            const imageItem = button.closest('.additional-image-item');
            imageItem.remove();
        }


        async function loadGenresAndRooms(movieData) {
            try {

                const genresResponse = await fetch('/MovieManagement/Movies/GetGenres');
                const genresResult = await genresResponse.json();
                
                if (genresResult.success && genresResult.data && genresResult.data.data) {
                    populateGenres(genresResult.data.data, movieData);
                }
                

                const roomsResponse = await fetch('/MovieManagement/Movies/GetCinemaRooms');
                const roomsResult = await roomsResponse.json();
                
                if (roomsResult.success && roomsResult.data && roomsResult.data.data) {
                    window.availableRooms = roomsResult.data.data;
                    loadExistingShowTimes(movieData);
                }
            } catch (error) {
                console.error('Error loading genres and rooms:', error);
            }
        }

        function populateGenres(genres, movieData) {
            const container = document.getElementById('genreCheckboxes');
            container.innerHTML = '';
            

            const existingGenreIds = [];
            if (movieData.genres || movieData.Genres) {
                const movieGenres = movieData.genres || movieData.Genres;
                if (Array.isArray(movieGenres)) {
                    movieGenres.forEach(genre => {
                        if (genre.id) existingGenreIds.push(genre.id);
                        else if (genre.Id) existingGenreIds.push(genre.Id);
                    });
                }
            }

            genres.forEach(genre => {
                const genreId = genre.id || genre.Id;
                const genreName = genre.name || genre.Name || genre.genreName || genre.GenreName;
                const isChecked = existingGenreIds.includes(genreId);
                
                const genreDiv = document.createElement('div');
                genreDiv.className = 'genre-checkbox-item';
                genreDiv.innerHTML = `
                    <label class="genre-checkbox-label">
                        <input type="checkbox" value="${genreId}" ${isChecked ? 'checked' : ''}>
                        <span class="genre-checkmark"></span>
                        ${genreName}
                    </label>
                `;
                container.appendChild(genreDiv);
            });
        }

        function loadExistingShowTimes(movieData) {
            const container = document.getElementById('showTimesContainer');
            container.innerHTML = '';
            

            if (movieData.showTimes || movieData.ShowTimes) {
                const showTimes = movieData.showTimes || movieData.ShowTimes;
                if (Array.isArray(showTimes)) {
                    showTimes.forEach(showTime => {
                        let showDate = showTime.showDate || showTime.ShowDate;
                        const roomId = showTime.roomId || showTime.RoomId;
                        

                        if (showDate) {
                            try {
                            const date = new Date(showDate);
                                // Kiểm tra xem date có hợp lệ không
                                if (!isNaN(date.getTime())) {

                                    const year = date.getFullYear();
                                    const month = String(date.getMonth() + 1).padStart(2, '0');
                                    const day = String(date.getDate()).padStart(2, '0');
                                    const hours = String(date.getHours()).padStart(2, '0');
                                    const minutes = String(date.getMinutes()).padStart(2, '0');
                                    
                                    showDate = `${year}-${month}-${day}T${hours}:${minutes}`;
                                } else {
                                    console.error('Invalid date:', showDate);
                                    showDate = '';
                                }
                            } catch (error) {
                                console.error('Error parsing date:', showDate, error);
                                showDate = '';
                            }
                        }
                        
                        addShowTimeEntry(showDate, roomId);
                    });
                }
            }
            

            if (container.children.length === 0) {
                addShowTimeEntry();
            }
        }

        function addShowTimeEntry(showDate = '', roomId = '') {
            const container = document.getElementById('showTimesContainer');
            const entryDiv = document.createElement('div');
            entryDiv.className = 'showtime-entry';
            

            let roomOptions = '<option value="">Chọn phòng chiếu</option>';
            if (window.availableRooms) {
                window.availableRooms.forEach(room => {
                    const id = room.id || room.Id;
                    const name = room.name || room.Name || room.roomName || room.RoomName;
                    const isSelected = id === roomId ? 'selected' : '';
                    roomOptions += `<option value="${id}" ${isSelected}>${name}</option>`;
                });
            }
            
            entryDiv.innerHTML = `
                <div class="showtime-fields">
                    <div class="showtime-field">
                        <label>Ngày chiếu</label>
                        <input type="datetime-local" class="showtime-date" value="${showDate}">
                    </div>
                    <div class="showtime-field">
                        <label>Phòng chiếu</label>
                        <div class="room-selection">
                            <select class="showtime-room">
                                ${roomOptions}
                            </select>
                            <button type="button" class="btn-create-room" onclick="openCreateRoomModal(this)">
                                ➕ Tạo phòng mới
                            </button>
                        </div>
                    </div>
                    <div class="showtime-actions">
                        <button type="button" class="btn-remove-showtime" onclick="removeShowTimeEntry(this)">
                            🗑️
                        </button>
                    </div>
                </div>
            `;
            
            container.appendChild(entryDiv);
        }

        function removeShowTimeEntry(button) {
            const entry = button.closest('.showtime-entry');
            entry.remove();
            

            const container = document.getElementById('showTimesContainer');
            if (container.children.length === 0) {
                addShowTimeEntry();
            }
        }

        async function updateMovie(event, movieId) {
            event.preventDefault();
            

            const updateBtn = document.getElementById('updateMovieBtn');
            const btnText = updateBtn.querySelector('.btn-text');
            const btnLoading = updateBtn.querySelector('.btn-loading');
            btnText.style.display = 'none';
            btnLoading.style.display = 'inline-block';
            updateBtn.disabled = true;
            

            const validationErrorsDiv = document.getElementById('validationErrors');
            validationErrorsDiv.innerHTML = '';
            validationErrorsDiv.style.display = 'none';
            
            const form = event.target;
            const formData = new FormData(form);
            

            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            

            // Chuyển đổi phiên bản từ giá trị chuỗi sang số tương ứng với enum MovieVersion (backend)
            let versionValue;
            switch (formData.get('version')) {
                case 'TwoD':
                    versionValue = 1; // 2D
                    break;
                case 'ThreeD':
                    versionValue = 2; // 3D
                    break;
                case 'FourDX':
                    versionValue = 3; // 4DX
                    break;
                default:
                    versionValue = 1; // Mặc định 2D
            }
            

            const images = [];
            

            const posterUrl = formData.get('imageUrl');
            const posterDescription = document.getElementById('posterDescription').value;
            if (posterUrl) {
                images.push({
                    imageUrl: posterUrl,
                    description: posterDescription || "Poster chính",
                    displayOrder: 1,
                    isPrimary: true
                });
            }
            

            const additionalImageItems = document.querySelectorAll('.additional-image-item');
            additionalImageItems.forEach((item, index) => {
                const img = item.querySelector('img');
                const descInput = item.querySelector('.additional-image-description');
                if (img && img.src && !img.src.includes('placeholder')) {
                    images.push({
                        imageUrl: img.src,
                        description: descInput ? descInput.value : `Hình ảnh ${index + 2}`,
                        displayOrder: index + 2,
                        isPrimary: false
                    });
                }
            });


            const genreIds = [];
            const checkedGenres = document.querySelectorAll('#genreCheckboxes input[type="checkbox"]:checked');
            const genreSet = new Set();
            checkedGenres.forEach(checkbox => genreSet.add(checkbox.value));
            genreIds.push(...genreSet);


            const showTimes = [];
            const showTimeEntries = document.querySelectorAll('.showtime-entry');
            showTimeEntries.forEach(entry => {
                const dateInput = entry.querySelector('.showtime-date');
                const roomSelect = entry.querySelector('.showtime-room');
                if (dateInput && roomSelect && dateInput.value && roomSelect.value) {
                    try {
                        // Đảm bảo datetime được chuyển đổi sang UTC
                        const localDate = new Date(dateInput.value);
                        // Tạo UTC date bằng cách chuyển đổi thời gian địa phương sang UTC
                        const utcDate = new Date(
                            Date.UTC(
                                localDate.getFullYear(),
                                localDate.getMonth(),
                                localDate.getDate(),
                                localDate.getHours(),
                                localDate.getMinutes(),
                                localDate.getSeconds()
                            )
                        );
                        const showDateTime = utcDate.toISOString();
                        const key = showDateTime + '_' + roomSelect.value;
                        if(!showTimes.some(st=>st.showDate===showDateTime && st.roomId===roomSelect.value)){
                            showTimes.push({
                                showDate: showDateTime,
                                roomId: roomSelect.value
                            });
                        }
                    } catch (error) {
                        console.error('Error parsing date:', dateInput.value, error);
                        validationErrors.push(`Ngày giờ chiếu không hợp lệ: ${dateInput.value}`);
                    }
                }
            });
            

            const movieData = {
                id: movieId,
                title: formData.get('title') || '',
                // Đảm bảo ngày tháng được chuyển đổi sang UTC
                releaseDate: formData.get('releaseDate') ? new Date(formData.get('releaseDate') + 'T00:00:00Z').toISOString() : '',
                endDate: formData.get('endDate') ? new Date(formData.get('endDate') + 'T00:00:00Z').toISOString() : '',
                directorIds: getSelectValues('editDirectorInput'),
                actorIds: getSelectValues('editActorsInput'),
                productionCompany: formData.get('productionCompany') || '',
                runningTime: parseInt(formData.get('runningTime')) || 0,
                version: versionValue,
                rating: parseFloat(formData.get('rating')) || 0,
                trailerUrl: formData.get('trailerUrl') || '',
                content: formData.get('content') || '',
                isFeatured: formData.get('isFeatured') === 'on',
                isRecommended: formData.get('isRecommended') === 'on',

                genreIds: genreIds,
                showTimes: showTimes,
                images: images
            };


            const validationErrors = [];
            
            if (!movieData.title) {
                validationErrors.push("Tên phim không được để trống");
            }
            
            if (!movieData.releaseDate) {
                validationErrors.push("Ngày phát hành không được để trống");
            }
            
            if (!movieData.endDate) {
                validationErrors.push("Ngày kết thúc không được để trống");
            }
            
            if (new Date(movieData.endDate) <= new Date(movieData.releaseDate)) {
                validationErrors.push("Ngày kết thúc phải sau ngày phát hành");
            }
            
            if (!movieData.runningTime || movieData.runningTime <= 0) {
                validationErrors.push("Thời lượng phim phải lớn hơn 0");
            }
            
            if (!movieData.directorIds || movieData.directorIds.length === 0) {
                validationErrors.push("Vui lòng chọn ít nhất một đạo diễn");
            }

            if (!movieData.actorIds || movieData.actorIds.length === 0) {
                validationErrors.push("Vui lòng chọn ít nhất một diễn viên");
            }

            if (genreIds.length === 0) {
                validationErrors.push("Vui lòng chọn ít nhất một thể loại phim");
            }

            // Bỏ validation bắt buộc lịch chiếu khi cập nhật
            // if (showTimes.length === 0) {
            //     validationErrors.push("Vui lòng thêm ít nhất một lịch chiếu");
            // }
            
            if (images.length === 0) {
                validationErrors.push("Vui lòng thêm ít nhất một hình ảnh cho phim");
            }


            if (validationErrors.length > 0) {
                validationErrorsDiv.innerHTML = `
                    <div class="alert alert-danger">
                        <h5><i class="fas fa-exclamation-triangle"></i> Vui lòng sửa các lỗi sau:</h5>
                        <ul>
                            ${validationErrors.map(error => `<li>${error}</li>`).join('')}
                        </ul>
                    </div>
                `;
                validationErrorsDiv.style.display = 'block';
                

                btnText.style.display = 'inline-block';
                btnLoading.style.display = 'none';
                updateBtn.disabled = false;
                

                validationErrorsDiv.scrollIntoView({ behavior: 'smooth', block: 'start' });
                return;
            }

            console.log('Sending movie data:', movieData); // Debug log

            try {
                // Thêm debug log để kiểm tra dữ liệu trước khi gửi
                console.log('Sending movie data to API:', JSON.stringify(movieData, null, 2));
                
                const response = await fetch('/MovieManagement/Movies/UpdateMovie', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify(movieData)
                });

                // Kiểm tra response status
                if (!response.ok) {
                    console.error('API Error Status:', response.status);
                    const errorText = await response.text();
                    console.error('API Error Response:', errorText);
                    try {
                        // Thử parse JSON từ error response
                        const errorJson = JSON.parse(errorText);
                        if (errorJson && !errorJson.success) {
                            throw new Error(errorJson.message || `API responded with status: ${response.status}`);
                        }
                    } catch (jsonError) {
                        // Nếu không parse được JSON, trả về lỗi gốc
                        throw new Error(`API responded with status: ${response.status}`);
                    }
                }

                const result = await response.json();
                console.log('API Response:', result);
                
                if (result.success) {
                    showNotification(result.message || 'Cập nhật phim thành công!', 'success');
                    closeModal('editMovieModal');

                    setTimeout(() => {
                        loadMoviesPage(currentPage, pageSize);
                    }, 500);
            } else {

                    let errorMessage = 'Có lỗi xảy ra khi cập nhật phim';
                    
                    // Xử lý chi tiết lỗi từ API
                    if (result.message) {
                        errorMessage = result.message;
                    } else if (result.errors) {
                        // Nếu có danh sách lỗi chi tiết
                        const errorDetails = [];
                        for (const key in result.errors) {
                            if (Array.isArray(result.errors[key])) {
                                result.errors[key].forEach(err => errorDetails.push(err));
                            } else {
                                errorDetails.push(result.errors[key]);
                            }
                        }
                        if (errorDetails.length > 0) {
                            errorMessage = errorDetails.join('<br>');
                        }
                    }
                    
                    validationErrorsDiv.innerHTML = `
                        <div class="alert alert-danger">
                            <h5><i class="fas fa-exclamation-triangle"></i> Lỗi từ máy chủ:</h5>
                            <div>${errorMessage}</div>
                        </div>
                    `;
                    validationErrorsDiv.style.display = 'block';
                    console.error('Update error:', result);
                    
                    // Cuộn đến phần thông báo lỗi
                    validationErrorsDiv.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            } catch (error) {
                console.error('Error updating movie:', error);
                validationErrorsDiv.innerHTML = `
                    <div class="alert alert-danger">
                        <h5><i class="fas fa-exclamation-triangle"></i> Lỗi kết nối:</h5>
                        <p>${error.message || 'Đã xảy ra lỗi khi cập nhật phim. Vui lòng thử lại sau.'}</p>
                    </div>
                `;
                validationErrorsDiv.style.display = 'block';
                
                // Cuộn đến phần thông báo lỗi
                validationErrorsDiv.scrollIntoView({ behavior: 'smooth', block: 'start' });
            } finally {

                btnText.style.display = 'inline-block';
                btnLoading.style.display = 'none';
                updateBtn.disabled = false;
            }
        }

        function closeModal(modalId) {
            const modal = document.getElementById(modalId);
            if (modal) {
                modal.style.display = 'none';
                
                // Nếu là modal tạo phòng, xóa callback button
                if (modalId === 'createRoomModal') {
                    delete modal.dataset.callbackButton;
                }
            }
        }


        window.onclick = function(event) {
            if (event.target.classList.contains('modal')) {
                event.target.style.display = 'none';
            }
        }


        document.addEventListener('DOMContentLoaded', function() {

            const dropdownElementList = document.querySelectorAll('.dropdown-toggle');
            const dropdownList = [...dropdownElementList].map(dropdownToggleEl => {
                if (typeof bootstrap !== 'undefined') {
                    return new bootstrap.Dropdown(dropdownToggleEl);
                }
                return null;
            });
        });

        // Hàm hỗ trợ cho chức năng thêm phim mới
        function setupAddMovieEventListeners() {

            const imageUrlInput = document.getElementById('addImageUrl');
            const posterPreview = document.getElementById('addPosterPreview');
            
            if (imageUrlInput) {
                imageUrlInput.addEventListener('input', function() {
                    if (this.value) {
                        posterPreview.src = this.value;
                        posterPreview.onerror = function() {
                            this.src = 'data:image/svg+xml,%3Csvg xmlns=\'http://www.w3.org/2000/svg\' width=\'300\' height=\'450\' fill=\'%232a2a2a\'/%3E%3Ctext x=\'50%25\' y=\'50%25\' font-family=\'Arial\' font-size=\'16\' fill=\'%23666\' text-anchor=\'middle\' dominant-baseline=\'middle\'%3EPoster image%3C/text%3E%3C/svg%3E';
                        };
                    }
                });
            }


            const trailerFileInput = document.getElementById('addTrailerFileInput');
            if (trailerFileInput) {
                trailerFileInput.addEventListener('change', handleAddTrailerUpload);
            }


            const posterFileInput = document.getElementById('addPosterFileInput');
            if (posterFileInput) {
                posterFileInput.addEventListener('change', handleAddPosterUpload);
            }


            const additionalImagesInput = document.getElementById('addAdditionalImagesInput');
            if (additionalImagesInput) {
                additionalImagesInput.addEventListener('change', handleAdditionalImagesUpload);
            }
        }

        function switchAddTrailerTab(tab) {
            const urlTab = document.getElementById('addTrailerUrlTab');
            const uploadTab = document.getElementById('addTrailerUploadTab');
            const buttons = document.querySelectorAll('#addMovieModal .trailer-upload-container .tab-btn');
            
            buttons.forEach(btn => btn.classList.remove('active'));
            
            if (tab === 'url') {
                urlTab.classList.add('active');
                uploadTab.classList.remove('active');
                buttons[0].classList.add('active');
            } else {
                urlTab.classList.remove('active');
                uploadTab.classList.add('active');
                buttons[1].classList.add('active');
            }
        }

        function switchAddPosterTab(tab) {
            const urlTab = document.getElementById('addPosterUrlTab');
            const uploadTab = document.getElementById('addPosterUploadTab');
            const buttons = document.querySelectorAll('#addMovieModal .poster-upload-section .tab-btn');
            
            buttons.forEach(btn => btn.classList.remove('active'));
            
            if (tab === 'url') {
                urlTab.classList.add('active');
                uploadTab.classList.remove('active');
                buttons[0].classList.add('active');
            } else {
                urlTab.classList.remove('active');
                uploadTab.classList.add('active');
                buttons[1].classList.add('active');
            }
        }


        async function handleAddTrailerUpload(event) {
            const file = event.target.files[0];
            if (!file) return;

            const progressDiv = document.getElementById('addTrailerUploadProgress');
            const progressBar = progressDiv.querySelector('.progress-bar');
            const resultDiv = document.getElementById('addTrailerUploadResult');
            
            try {

                progressDiv.style.display = 'block';
                progressBar.style.width = '0%';
                resultDiv.innerHTML = '';


                const progressInterval = setInterval(() => {
                    const currentWidth = parseInt(progressBar.style.width) || 0;
                    if (currentWidth < 90) {
                        progressBar.style.width = (currentWidth + 10) + '%';
                    }
                }, 200);


                const formData = new FormData();
                formData.append('file', file);

                const response = await fetch('/MovieManagement/Movies/UploadVideo', {
                    method: 'POST',
                    body: formData
                });

                const result = await response.json();

                clearInterval(progressInterval);
                progressBar.style.width = '100%';

                if (result.success) {

                    const trailerUrlInput = document.getElementById('addTrailerUrl');
                    trailerUrlInput.value = result.videoUrl;
                    
                    resultDiv.innerHTML = `<div class="upload-success">✅ Upload thành công!</div>`;
                    

                    switchAddTrailerTab('url');
                } else {
                    resultDiv.innerHTML = `<div class="upload-error">❌ ${result.message}</div>`;
                }

                setTimeout(() => {
                    progressDiv.style.display = 'none';
                }, 2000);

            } catch (error) {
                console.error('Error uploading video:', error);
                resultDiv.innerHTML = `<div class="upload-error">❌ Lỗi khi upload video</div>`;
                progressDiv.style.display = 'none';
            }
        }

        async function handleAddPosterUpload(event) {
            const file = event.target.files[0];
            if (!file) return;

            const progressDiv = document.getElementById('addPosterUploadProgress');
            const progressBar = progressDiv.querySelector('.progress-bar');
            const posterPreview = document.getElementById('addPosterPreview');
            
            try {

                progressDiv.style.display = 'block';
                progressBar.style.width = '0%';


                const progressInterval = setInterval(() => {
                    const currentWidth = parseInt(progressBar.style.width) || 0;
                    if (currentWidth < 90) {
                        progressBar.style.width = (currentWidth + 10) + '%';
                    }
                }, 200);


                const formData = new FormData();
                formData.append('file', file);

                const response = await fetch('/MovieManagement/Movies/UploadImage', {
                    method: 'POST',
                    body: formData
                });

                const result = await response.json();

                clearInterval(progressInterval);
                progressBar.style.width = '100%';

                if (result.success) {

                    const imageUrlInput = document.getElementById('addImageUrl');
                    imageUrlInput.value = result.imageUrl;
                    posterPreview.src = result.imageUrl;
                    
                    showNotification('Upload poster thành công!', 'success');
                    

                    switchAddPosterTab('url');
                } else {
                    showNotification('Lỗi: ' + result.message, 'error');
                }

                setTimeout(() => {
                    progressDiv.style.display = 'none';
                }, 2000);

            } catch (error) {
                console.error('Error uploading image:', error);
                showNotification('Lỗi khi upload hình ảnh', 'error');
                progressDiv.style.display = 'none';
            }
        }

        async function handleAddAdditionalImagesUpload(event) {
            const files = Array.from(event.target.files);
            if (files.length === 0) return;

            const progressDiv = document.getElementById('addAdditionalUploadProgress');
            const progressBar = progressDiv.querySelector('.progress-bar');
            const container = document.getElementById('addAdditionalImagesContainer');
            
            try {

                progressDiv.style.display = 'block';
                progressBar.style.width = '0%';

                const uploadedImages = [];
                const totalFiles = files.length;

                for (let i = 0; i < files.length; i++) {
                    const file = files[i];
                    

                    const progress = ((i + 1) / totalFiles) * 100;
                    progressBar.style.width = progress + '%';

                    try {
                        const formData = new FormData();
                        formData.append('file', file);

                        const response = await fetch('/MovieManagement/Movies/UploadImage', {
                            method: 'POST',
                            body: formData
                        });

                        const result = await response.json();

                        if (result.success) {
                            uploadedImages.push({
                                url: result.imageUrl,
                                name: file.name
                            });
                        }
                    } catch (error) {
                        console.error(`Error uploading ${file.name}:`, error);
                    }
                }


                displayAddAdditionalImages(uploadedImages, container);
                
                showNotification(`Đã upload ${uploadedImages.length}/${totalFiles} hình ảnh thành công!`, 'success');

                setTimeout(() => {
                    progressDiv.style.display = 'none';
                }, 2000);

            } catch (error) {
                console.error('Error uploading images:', error);
                showNotification('Lỗi khi upload hình ảnh', 'error');
                progressDiv.style.display = 'none';
            }
        }

        function displayAddAdditionalImages(images, container) {
            images.forEach((image, index) => {
                const imageDiv = document.createElement('div');
                imageDiv.className = 'additional-image-item';
                imageDiv.innerHTML = `
                    <img src="${image.url}" alt="${image.name}" class="additional-image-preview">
                    <div class="image-overlay">
                        <span class="image-name">${image.name}</span>
                        <button type="button" class="btn-remove-image" onclick="removeAddAdditionalImage(this)">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                    <div class="image-meta">
                        <input type="text" class="additional-image-description" placeholder="Mô tả hình ảnh..." value="${image.description || ''}">
                    </div>
                `;
                container.appendChild(imageDiv);
            });
        }

        function removeAddAdditionalImage(button) {
            const imageItem = button.closest('.additional-image-item');
            imageItem.remove();
        }

        function addNewShowTimeEntry() {
            const container = document.getElementById('addShowTimesContainer');
            const entryDiv = document.createElement('div');
            entryDiv.className = 'showtime-entry';
            

            let roomOptions = '<option value="">Chọn phòng chiếu</option>';
            if (window.availableRooms && Array.isArray(window.availableRooms)) {
                window.availableRooms.forEach(room => {
                    const id = room.id || room.Id;
                    const name = room.name || room.Name || room.roomName || room.RoomName;
                    roomOptions += `<option value="${id}">${name}</option>`;
                });
            } else {
                console.warn('Không có dữ liệu phòng chiếu hoặc dữ liệu không đúng định dạng');
            }
            
            entryDiv.innerHTML = `
                <div class="showtime-fields">
                    <div class="showtime-field">
                        <label>Ngày chiếu</label>
                        <input type="datetime-local" class="showtime-date">
                    </div>
                    <div class="showtime-field">
                        <label>Phòng chiếu</label>
                        <div class="room-selection">
                            <select class="showtime-room" style="background-color: #2a2a2a; color: white; border: 1px solid rgba(255,255,255,0.2);">
                                ${roomOptions}
                            </select>
                            <button type="button" class="btn-create-room" onclick="openCreateRoomModal(this)">
                                ➕ Tạo phòng mới
                            </button>
                        </div>
                    </div>
                    <div class="showtime-actions">
                        <button type="button" class="btn-remove-showtime" onclick="removeNewShowTimeEntry(this)">
                            🗑️
                        </button>
                    </div>
                </div>
            `;
            
            container.appendChild(entryDiv);
        }

        function removeNewShowTimeEntry(button) {
            const entry = button.closest('.showtime-entry');
            entry.remove();
            

            const container = document.getElementById('addShowTimesContainer');
            if (container.children.length === 0) {
                addNewShowTimeEntry();
            }
        }

        function openCreateRoomModal(button) {
            const modal = document.getElementById('createRoomModal');
            if (!modal) {
                console.error('Không tìm thấy modal #createRoomModal');
                showNotification('Lỗi: Không thể mở form tạo phòng chiếu mới', 'error');
                return;
            }
            
            // Lưu ID của button để cập nhật select sau khi tạo phòng
            let buttonId = button.id || button.getAttribute('id');
            if (!buttonId) {
                // Tạo ID duy nhất nếu button không có ID
                buttonId = 'create-room-btn-' + Date.now();
                button.id = buttonId;
            }
            modal.dataset.callbackButton = buttonId;
            
            // Reset form
            const form = modal.querySelector('#createRoomForm');
            if (form) {
                form.reset();
                // Set default values
                form.querySelector('#roomName').value = '';
                form.querySelector('#totalSeats').value = '100';
                form.querySelector('#numberOfRows').value = '10';
                form.querySelector('#numberOfColumns').value = '10';
                form.querySelector('#defaultSeatPrice').value = '100000';
                
                // Thêm event listeners để tự động tính toán tổng số ghế
                const rowsInput = form.querySelector('#numberOfRows');
                const colsInput = form.querySelector('#numberOfColumns');
                const totalSeatsInput = form.querySelector('#totalSeats');
                
                const calculateTotalSeats = () => {
                    const rows = parseInt(rowsInput.value) || 0;
                    const cols = parseInt(colsInput.value) || 0;
                    totalSeatsInput.value = rows * cols;
                };
                
                rowsInput.addEventListener('input', calculateTotalSeats);
                colsInput.addEventListener('input', calculateTotalSeats);
            }
            
            modal.style.display = 'block';
        }

        async function createNewRoom(event) {
            event.preventDefault();
            
            const form = event.target;
            const formData = new FormData(form);
            
            // Validate form
            const roomName = formData.get('roomName');
            const totalSeats = parseInt(formData.get('totalSeats'));
            const numberOfRows = parseInt(formData.get('numberOfRows'));
            const numberOfColumns = parseInt(formData.get('numberOfColumns'));
            const defaultSeatPrice = parseFloat(formData.get('defaultSeatPrice'));
            
            if (!roomName || roomName.trim() === '') {
                showNotification('Tên phòng chiếu không được để trống', 'error');
                return;
            }
            
            if (totalSeats !== numberOfRows * numberOfColumns) {
                showNotification(`Số ghế không khớp: ${numberOfRows} hàng x ${numberOfColumns} cột = ${numberOfRows * numberOfColumns}, nhưng TotalSeats = ${totalSeats}`, 'error');
                return;
            }
            
            const roomData = {
                RoomName: roomName.trim(),
                TotalSeats: totalSeats,
                NumberOfRows: numberOfRows,
                NumberOfColumns: numberOfColumns,
                DefaultSeatPrice: defaultSeatPrice
            };
            
            try {
                // Lấy CSRF token
                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                
                console.log('Gửi request tạo phòng chiếu:', {
                    url: '/MovieManagement/Movies/CreateCinemaRoom',
                    data: roomData,
                    token: token ? 'Có token' : 'Không có token'
                });
                
                const result = await fetch('/MovieManagement/Movies/CreateCinemaRoom', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': token || ''
                    },
                    body: JSON.stringify(roomData)
                });
                
                console.log('Response status:', result.status);
                
                if (!result.ok) {
                    // Thử đọc response body để xem lỗi chi tiết
                    let errorMessage = `HTTP ${result.status}: ${result.statusText}`;
                    try {
                        const errorResponse = await result.text();
                        console.log('Error response body:', errorResponse);
                        if (errorResponse) {
                            errorMessage += ` - ${errorResponse}`;
                        }
                    } catch (e) {
                        console.log('Could not read error response body');
                    }
                    throw new Error(errorMessage);
                }
                
                const response = await result.json();
                console.log('Response data:', response);
                
                if (response.success) {
                    showNotification('Tạo phòng chiếu mới thành công!', 'success');
                    
                    // Đóng modal
                    const modal = document.getElementById('createRoomModal');
                    modal.style.display = 'none';
                    
                    // Reload danh sách phòng chiếu
                    await loadAddGenresAndRooms();
                    
                    // Cập nhật tất cả các dropdown phòng chiếu trong form thêm phim
                    updateAllRoomDropdowns(roomName);
                } else {
                    showNotification(response.message || 'Lỗi khi tạo phòng chiếu mới', 'error');
                }
            } catch (error) {
                console.error('Lỗi khi tạo phòng chiếu:', error);
                console.error('Error details:', {
                    message: error.message,
                    stack: error.stack,
                    name: error.name
                });
                showNotification(`Đã xảy ra lỗi khi tạo phòng chiếu mới: ${error.message}`, 'error');
            }
        }

        function updateAllRoomDropdowns(newRoomName) {
            // Tìm tất cả các dropdown phòng chiếu trong form thêm phim
            const roomDropdowns = document.querySelectorAll('#addMovieModal .showtime-room, #editMovieModal .showtime-room');
            
            roomDropdowns.forEach(dropdown => {
                // Lưu giá trị hiện tại
                const currentValue = dropdown.value;
                
                // Xóa tất cả options cũ
                dropdown.innerHTML = '<option value="">Chọn phòng chiếu</option>';
                
                // Thêm lại tất cả phòng chiếu từ window.availableRooms
                if (window.availableRooms && Array.isArray(window.availableRooms)) {
                    window.availableRooms.forEach(room => {
                        const id = room.id || room.Id;
                        const name = room.name || room.Name || room.roomName || room.RoomName;
                        const option = document.createElement('option');
                        option.value = id;
                        option.textContent = name;
                        dropdown.appendChild(option);
                        
                        // Nếu đây là phòng mới tạo, tự động chọn
                        if (name === newRoomName) {
                            option.selected = true;
                        }
                    });
                }
                
                // Nếu không tìm thấy phòng mới, giữ lại giá trị cũ
                if (dropdown.value === '' && currentValue) {
                    dropdown.value = currentValue;
                }
            });
            
            console.log(`Đã cập nhật ${roomDropdowns.length} dropdown phòng chiếu với phòng mới: ${newRoomName}`);
        }

        async function loadAddGenresAndRooms() {
            try {
                console.log('Đang tải thể loại phim và phòng chiếu...');
                

                const genresResponse = await fetch('/MovieManagement/Movies/GetGenres');
                const genresResult = await genresResponse.json();
                
                console.log('Kết quả API thể loại:', genresResult);
                
                if (genresResult.success && genresResult.data) {
                    let genresData = genresResult.data;
                    
                    // Kiểm tra cấu trúc dữ liệu
                    if (genresData.data) {
                        genresData = genresData.data;
                    }
                    
                    if (Array.isArray(genresData)) {
                        populateAddGenres(genresData);
                    } else {
                        console.error('Dữ liệu thể loại không phải là mảng:', genresData);
                    }
                } else {
                    console.error('Không thể tải thể loại phim:', genresResult.message || 'Lỗi không xác định');
                }
                

                const roomsResponse = await fetch('/MovieManagement/Movies/GetCinemaRooms');
                const roomsResult = await roomsResponse.json();
                
                console.log('Kết quả API phòng chiếu:', roomsResult);
                
                if (roomsResult.success && roomsResult.data) {
                    let roomsData = roomsResult.data;
                    
                    // Kiểm tra cấu trúc dữ liệu
                    if (roomsData.data) {
                        roomsData = roomsData.data;
                    }
                    
                    if (Array.isArray(roomsData)) {
                        window.availableRooms = roomsData;
                        console.log('Đã tải', roomsData.length, 'phòng chiếu');
                    } else {
                        console.error('Dữ liệu phòng chiếu không phải là mảng:', roomsData);
                    }
                } else {
                    console.error('Không thể tải phòng chiếu:', roomsResult.message || 'Lỗi không xác định');
                    window.availableRooms = [];
                }
            } catch (error) {
                console.error('Lỗi khi tải thể loại phim và phòng chiếu:', error);
                showNotification('Không thể tải dữ liệu thể loại phim và phòng chiếu. Vui lòng thử lại sau.', 'error');
            }
        }

        function populateAddGenres(genres) {
            console.log('Đang hiển thị', genres.length, 'thể loại phim');
            
            const container = document.getElementById('addGenreCheckboxes');
            if (!container) {
                console.error('Không tìm thấy phần tử #addGenreCheckboxes');
                return;
            }
            
            container.innerHTML = '';
            
            if (!Array.isArray(genres) || genres.length === 0) {
                container.innerHTML = '<div class="no-genres">Không có thể loại phim nào</div>';
                return;
            }
            
            genres.forEach(genre => {
                // Xử lý các trường hợp khác nhau của cấu trúc dữ liệu
                const genreId = genre.id || genre.Id || genre.genreId || genre.GenreId || '';
                const genreName = genre.name || genre.Name || genre.genreName || genre.GenreName || 'Không có tên';
                
                console.log('Thể loại:', { id: genreId, name: genreName });
                
                if (!genreId) {
                    console.warn('Bỏ qua thể loại không có ID:', genre);
                    return;
                }
                
                const genreDiv = document.createElement('div');
                genreDiv.className = 'genre-checkbox-item';
                genreDiv.innerHTML = `
                    <label class="genre-checkbox-label">
                        <input type="checkbox" value="${genreId}" name="genre-${genreId}">
                        <span class="genre-checkmark"></span>
                        ${genreName}
                    </label>
                `;
                container.appendChild(genreDiv);
            });
        }

        // Hàm tạo phim mới
        async function createMovie(event) {
            event.preventDefault();
            

            const createBtn = document.getElementById('createMovieBtn');
            const btnText = createBtn.querySelector('.btn-text');
            const btnLoading = createBtn.querySelector('.btn-loading');
            btnText.style.display = 'none';
            btnLoading.style.display = 'inline-block';
            createBtn.disabled = true;
            

            const validationErrorsDiv = document.getElementById('addValidationErrors');
            validationErrorsDiv.innerHTML = '';
            validationErrorsDiv.style.display = 'none';
            
            const form = event.target;
            const formData = new FormData(form);
            
            try {

                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                if (!token) {
                    console.warn('Không tìm thấy token CSRF');
                    // Thử tìm token ở các vị trí khác
                    const allTokens = document.querySelectorAll('input[name="__RequestVerificationToken"]');
                    console.log('Tổng số token tìm thấy:', allTokens.length);
                    allTokens.forEach((t, i) => console.log(`Token ${i+1}:`, t.value));
                    
                    // Thêm token vào form nếu cần
                    if (allTokens.length > 0 && allTokens[0].value) {
                        const csrfToken = allTokens[0].value;
                        console.log('Sử dụng token từ vị trí khác:', csrfToken);
                    } else {
                        console.error('Không tìm thấy token CSRF trong trang!');
                    }
                } else {
                    console.log('Đã tìm thấy token CSRF:', token);
                }
                

                let versionValue;
                switch (formData.get('version')) {
                    case 'TwoD':
                        versionValue = 1;
                        break;
                    case 'ThreeD':
                        versionValue = 2;
                        break;
                    case 'FourDX':
                        versionValue = 3;
                        break;
                    default:
                        versionValue = 1; // Default to 2D
                }
                

                const images = [];
                

                const posterUrl = formData.get('imageUrl');
                const posterDescription = document.getElementById('addPosterDescription').value;
                if (posterUrl) {
                    images.push({
                        imageUrl: posterUrl,
                        description: posterDescription || "Poster chính",
                        displayOrder: 1,
                        isPrimary: true
                    });
                }
                

                const additionalImageItems = document.querySelectorAll('#addAdditionalImagesContainer .additional-image-item');
                additionalImageItems.forEach((item, index) => {
                    const img = item.querySelector('img');
                    const descInput = item.querySelector('.additional-image-description');
                    if (img && img.src && !img.src.includes('placeholder')) {
                        images.push({
                            imageUrl: img.src,
                            description: descInput ? descInput.value : `Hình ảnh ${index + 2}`,
                            displayOrder: index + 2,
                            isPrimary: false
                        });
                    }
                });


                const genreIds = [];
                const checkedGenres = document.querySelectorAll('#addGenreCheckboxes input[type="checkbox"]:checked');
                
                console.log('Số thể loại đã chọn:', checkedGenres.length);
                
                checkedGenres.forEach(checkbox => {
                    const genreId = checkbox.value;
                    console.log('Thể loại đã chọn:', genreId);
                    
                    if (genreId && genreId.trim() !== '') {
                        genreIds.push(genreId);
                    }
                });


                const showTimes = [];
                const showTimeEntries = document.querySelectorAll('#addShowTimesContainer .showtime-entry');
                
                showTimeEntries.forEach(entry => {
                    const dateInput = entry.querySelector('.showtime-date');
                    const roomSelect = entry.querySelector('.showtime-room');
                    
                    // Chỉ thêm vào mảng nếu cả hai trường đều có giá trị
                    if (dateInput && dateInput.value && roomSelect && roomSelect.value) {
                        showTimes.push({
                            showDate: new Date(dateInput.value).toISOString().split('.')[0] + 'Z',
                            roomId: roomSelect.value
                        });
                    }
                });

                console.log('Lịch chiếu thu thập được:', showTimes);
                
                let hasShowTimeError = false;
                
                showTimeEntries.forEach((entry, index) => {
                    const dateInput = entry.querySelector('.showtime-date');
                    const roomSelect = entry.querySelector('.showtime-room');
                    
                    if (dateInput && roomSelect) {
                        const dateValue = dateInput.value;
                        const roomValue = roomSelect.value;
                        
                        console.log(`Lịch chiếu ${index + 1}:`, { date: dateValue, room: roomValue });
                        
                        if (dateValue && roomValue) {
                            try {
                                // Đảm bảo datetime được chuyển đổi đúng định dạng
                                const localDate = new Date(dateValue);
                                

                                const formattedDate = localDate.toISOString().split('.')[0] + 'Z';
                                
                                showTimes.push({
                                    showDate: formattedDate,
                                    roomId: roomValue
                                });
                            } catch (error) {
                                console.error('Lỗi xử lý ngày giờ:', dateValue, error);
                                hasShowTimeError = true;
                            }
                        } else {
                            if (!dateValue) console.warn(`Lịch chiếu ${index + 1}: Thiếu ngày giờ`);
                            if (!roomValue) console.warn(`Lịch chiếu ${index + 1}: Thiếu phòng chiếu`);
                        }
                    } else {
                        console.warn(`Lịch chiếu ${index + 1}: Thiếu trường dữ liệu`);
                    }
                });
                

                const releaseDate = formData.get('releaseDate');
                const endDate = formData.get('endDate');
                

                const movieData = {
                    title: formData.get('title') || '',
                    releaseDate: releaseDate ? new Date(releaseDate).toISOString().split('.')[0] + 'Z' : '',
                    endDate: endDate ? new Date(endDate).toISOString().split('.')[0] + 'Z' : '',
                    directorIds: $('#addDirectorSelect').val() || [], // Sửa lại cách lấy directorIds
                    actorIds: $('#addActorsSelect').val() || [], // Sửa lại cách lấy actorIds
                    productionCompany: formData.get('productionCompany') || '',
                    runningTime: parseInt(formData.get('runningTime')) || 0,
                    version: versionValue,
                    rating: parseFloat(formData.get('rating')) || 0,
                    trailerUrl: formData.get('trailerUrl') || '',
                    content: formData.get('content') || '',
                    isFeatured: formData.get('isFeatured') === 'on',
                    isRecommended: formData.get('isRecommended') === 'on',
                    genreIds: genreIds,
                    showTimes: showTimes,
                    images: images
                };

                console.log('Dữ liệu phim trước khi gửi:', {
                    ...movieData,
                    directorIds: movieData.directorIds,
                    actorIds: movieData.actorIds,
                    genreIds: movieData.genreIds,
                    showTimes: movieData.showTimes,
                    images: movieData.images
                });

                const validationErrors = [];
                
                if (!movieData.title) {
                    validationErrors.push("Tên phim không được để trống");
                }
                
                if (!movieData.releaseDate) {
                    validationErrors.push("Ngày phát hành không được để trống");
                }
                
                if (!movieData.endDate) {
                    validationErrors.push("Ngày kết thúc không được để trống");
                }
                
                if (new Date(movieData.endDate) <= new Date(movieData.releaseDate)) {
                    validationErrors.push("Ngày kết thúc phải sau ngày phát hành");
                }
                
                if (!movieData.runningTime || movieData.runningTime <= 0) {
                    validationErrors.push("Thời lượng phim phải lớn hơn 0");
                }
                
                if (!movieData.directorIds || movieData.directorIds.length === 0) {
                    validationErrors.push("Vui lòng chọn ít nhất một đạo diễn");
                }

                if (!movieData.actorIds || movieData.actorIds.length === 0) {
                    validationErrors.push("Vui lòng chọn ít nhất một diễn viên");
                }

                if (genreIds.length === 0) {
                    validationErrors.push("Vui lòng chọn ít nhất một thể loại phim");
                }

                // Bỏ validation bắt buộc lịch chiếu khi cập nhật
                // if (showTimes.length === 0) {
                //     validationErrors.push("Vui lòng thêm ít nhất một lịch chiếu");
                // }
                
                if (images.length === 0) {
                    validationErrors.push("Vui lòng thêm ít nhất một hình ảnh cho phim");
                }


                if (validationErrors.length > 0) {
                    validationErrorsDiv.innerHTML = `
                        <div class="alert alert-danger">
                            <h5><i class="fas fa-exclamation-triangle"></i> Vui lòng sửa các lỗi sau:</h5>
                            <ul>
                                ${validationErrors.map(error => `<li>${error}</li>`).join('')}
                            </ul>
                        </div>
                    `;
                    validationErrorsDiv.style.display = 'block';
                    

                    btnText.style.display = 'inline-block';
                    btnLoading.style.display = 'none';
                    createBtn.disabled = false;
                    

                    validationErrorsDiv.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    return;
                }

                console.log('Dữ liệu phim gửi đi:', movieData); // Debug log

                // Thêm debug log để kiểm tra dữ liệu trước khi gửi
                console.log('Dữ liệu JSON gửi đi:', JSON.stringify(movieData, null, 2));
                
                // Gửi dữ liệu trực tiếp theo format API (lowercase)
                console.log('Dữ liệu gửi đến controller:', JSON.stringify(movieData, null, 2));
                
                console.log('Gửi request với dữ liệu:', JSON.stringify(movieData, null, 2));
                
                const response = await fetch('/MovieManagement/Movies/CreateMovieAjax', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token || ''
                    },
                    body: JSON.stringify(movieData)
                });

                console.log('Response status:', response.status);
                console.log('Response headers:', Object.fromEntries(response.headers.entries()));

                // Kiểm tra response status
                if (!response.ok) {
                    console.error('Lỗi API Status:', response.status);
                    const errorText = await response.text();
                    console.error('Phản hồi lỗi API:', errorText);
                    
                    try {
                        // Thử parse JSON từ error response
                        const errorJson = JSON.parse(errorText);
                        if (errorJson && !errorJson.success) {
                            throw new Error(errorJson.message || `API trả về lỗi: ${response.status}`);
                        }
                    } catch (jsonError) {
                        // Nếu không parse được JSON, trả về lỗi gốc
                        throw new Error(`API trả về lỗi: ${response.status}. Chi tiết: ${errorText}`);
                    }
                }

                const result = await response.json();
                console.log('Phản hồi API:', result);
                
                if (result.success) {
                    showNotification(result.message || 'Thêm phim mới thành công!', 'success');
                    closeModal('addMovieModal');

                    setTimeout(() => {
                        loadMoviesPage(currentPage, pageSize);
                    }, 500);
                } else {

                    let errorMessage = 'Có lỗi xảy ra khi thêm phim';
                    
                    // Xử lý chi tiết lỗi từ API
                    if (result.message) {
                        errorMessage = result.message;
                    } else if (result.errors) {
                        // Nếu có danh sách lỗi chi tiết
                        const errorDetails = [];
                        for (const key in result.errors) {
                            if (Array.isArray(result.errors[key])) {
                                result.errors[key].forEach(err => errorDetails.push(err));
                            } else {
                                errorDetails.push(result.errors[key]);
                            }
                        }
                        if (errorDetails.length > 0) {
                            errorMessage = errorDetails.join('<br>');
                        }
                    }
                    
                    validationErrorsDiv.innerHTML = `
                        <div class="alert alert-danger">
                            <h5><i class="fas fa-exclamation-triangle"></i> Lỗi từ máy chủ:</h5>
                            <div>${errorMessage}</div>
                        </div>
                    `;
                    validationErrorsDiv.style.display = 'block';
                    console.error('Lỗi tạo phim:', result);
                    
                    // Cuộn đến phần thông báo lỗi
                    validationErrorsDiv.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            } catch (error) {
                console.error('Lỗi khi tạo phim:', error);
                validationErrorsDiv.innerHTML = `
                    <div class="alert alert-danger">
                        <h5><i class="fas fa-exclamation-triangle"></i> Lỗi kết nối:</h5>
                        <p>${error.message || 'Đã xảy ra lỗi khi thêm phim. Vui lòng thử lại sau.'}</p>
                    </div>
                `;
                validationErrorsDiv.style.display = 'block';
                
                // Cuộn đến phần thông báo lỗi
                validationErrorsDiv.scrollIntoView({ behavior: 'smooth', block: 'start' });
            } finally {

                btnText.style.display = 'inline-block';
                btnLoading.style.display = 'none';
                createBtn.disabled = false;
            }
        }

        // ================== Quick Add Actor / Director ===================
        async function createNewActor() {
            const name = prompt('Nhập tên diễn viên mới:');
            if (!name || name.trim() === '') return;
            try {
                const response = await fetch('/MovieManagement/Movies/CreateActor', {
                    method:'POST',
                    headers:{'Content-Type':'application/json'},
                    body: JSON.stringify({name})
                });
                const result = await response.json();
                if (result.success) {
                    alert('Đã thêm diễn viên!');
                    // reload list
                    window.availableActors.push({id: result.data?.id || result.id || '', name});
                    const sel = document.getElementById('addActorsSelect');
                    if (sel) {
                        const opt=document.createElement('option');
                        opt.value=result.data?.id||result.id||'';
                        opt.textContent=name;
                        sel.appendChild(opt);
                        opt.selected=true;
                    }
                } else alert(result.message || 'Tạo thất bại');
            }catch(err){alert('Lỗi khi gọi API');}
        }

        async function createNewDirector() {
            const name = prompt('Nhập tên đạo diễn mới:');
            if (!name || name.trim() === '') return;
            try {
                const response = await fetch('/MovieManagement/Movies/CreateDirector', {
                    method:'POST',
                    headers:{'Content-Type':'application/json'},
                    body: JSON.stringify({name})
                });
                const result = await response.json();
                if (result.success) {
                    alert('Đã thêm đạo diễn!');
                    window.availableDirectors.push({id: result.data?.id || result.id || '', name});
                    const sel = document.getElementById('addDirectorSelect');
                    if (sel) {
                        const opt=document.createElement('option');
                        opt.value=result.data?.id||result.id||'';
                        opt.textContent=name;
                        sel.appendChild(opt);
                        opt.selected=true;
                    }
                } else alert(result.message || 'Tạo thất bại');
            }catch(err){alert('Lỗi khi gọi API');}
        }

        // inject style for tiny add button
        if(!document.getElementById('btn-add-small-style')){
          const style=document.createElement('style');
          style.id='btn-add-small-style';
          style.innerHTML=`.btn-add-small{margin-left:6px;padding:2px 6px;background:#6f42c1;color:#fff;border:none;border-radius:4px;font-size:0.8rem;cursor:pointer;display:flex;align-items:center;justify-content:center;} .btn-add-small:hover{background:#5a34a3;}`;
          document.head.appendChild(style);
        }

        function openAddEntityPrompt(type){
            const label = type==='actor' ? 'diễn viên' : 'đạo diễn';
            if (window.Swal){
                Swal.fire({
                    title: `Nhập tên ${label}`,
                    input: 'text',
                    inputPlaceholder: `Tên ${label}`,
                    showCancelButton: true,
                    confirmButtonText: 'Lưu',
                    cancelButtonText: 'Hủy',
                    inputValidator: (value)=>{
                        if(!value || !value.trim()) return 'Không được để trống';
                    }
                }).then(result=>{
                    if(result.isConfirmed){
                        createEntity(type, result.value.trim());
                    }
                });
            } else {
                const name = prompt(`Nhập tên ${label}:`);
                if(!name || !name.trim()) return;
                createEntity(type, name.trim());
            }
        }

        async function createEntity(type, name){
            const label = type==='actor' ? 'diễn viên' : 'đạo diễn';
            try{
                const response = await fetch(`/MovieManagement/Movies/Create${type==='actor'?'Actor':'Director'}`,{
                    method:'POST',
                    headers:{'Content-Type':'application/json'},
                    body: JSON.stringify({ name })
                });
                const res = await response.json();
                if(res.success){
                    const id = res.data?.data?.id || res.data?.id || res.data?.Id;
                    if(!id){
                        Swal ? Swal.fire('Lỗi','Không lấy được ID trả về','error') : alert('Tạo thành công nhưng không lấy được ID');
                        return;
                    }
                    const selectIds = type==='actor'? ['addActorsSelect','editActorsSelect'] : ['addDirectorSelect','editDirectorSelect'];
                    selectIds.forEach(sid=>{
                        const sel = document.getElementById(sid);
                        if(!sel) return;
                        let opt = sel.querySelector(`option[value="${id}"]`);
                        if(!opt){
                            opt=document.createElement('option');
                            opt.value=id;
                            opt.textContent=name;
                            sel.appendChild(opt);
                        }
                        opt.selected=true;
                        if(window.$ && $.fn.select2){ $(sel).trigger('change'); }
                    });
                    // Lưu vào cache
                    if(type==='actor') window.availableActors.push({id,name}); else window.availableDirectors.push({id,name});
                    Swal ? Swal.fire('Thành công', `Đã thêm ${label}!`, 'success') : alert(`Đã tạo ${label} mới`);
                } else {
                    Swal ? Swal.fire('Lỗi', res.message || 'Tạo thất bại', 'error') : alert(res.message || 'Tạo thất bại');
                }
            }catch(err){
                console.error(err);
                Swal ? Swal.fire('Lỗi','Không kết nối được máy chủ','error') : alert('Lỗi kết nối máy chủ');
            }
        }

        // Remove dangling global select2 trigger if exists
        // ... existing code ...

        // === Helper to get multi-select values (supports Select2) ===
        function getSelectValues(selectId){
            if(window.$ && $('#'+selectId).length){
                const val = $('#'+selectId).val();
                if(val) return Array.isArray(val) ? val : [val];
            }
            let sel=document.getElementById(selectId);
            // Fallback: nếu không tìm thấy, thử ánh xạ sang id tương ứng của form thêm phim
            if(!sel){
                if(selectId==='editDirectorInput') sel=document.getElementById('addDirectorSelect');
                else if(selectId==='editActorsInput') sel=document.getElementById('addActorsSelect');
            }
            if(!sel) return [];
            return Array.from(sel.selectedOptions).map(o=>o.value);
        }

        // === Tagify helper to get selected IDs ===
        function getTagifyIds(inputId){
          const el=document.getElementById(inputId);
          if(!el||!el.__tagify) return [];
          return el.__tagify.value.map(v=>v.value);
        }

