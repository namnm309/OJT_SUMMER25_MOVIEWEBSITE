// Advanced Search and Filter Functionality
class AdvancedMovieSearch {
    constructor(options = {}) {
        this.apiUrl = options.apiUrl || '/Movies/GetMoviesByFilter';
        this.searchUrl = options.searchUrl || '/Movies/SearchMovies';
        this.genresUrl = options.genresUrl || '/Movies/GetGenres';
        this.containerId = options.containerId || 'movies-container';
        this.paginationId = options.paginationId || 'pagination-container';
        this.filtersFormId = options.filtersFormId || 'filters-form';
        
        // Filter state
        this.currentFilters = {
            keyword: '',
            status: 'all',
            genre: '',
            fromDate: null,
            toDate: null,
            minRating: null,
            maxRating: null,
            minDuration: null,
            maxDuration: null,
            sortBy: 'releaseDate',
            sortOrder: 'desc',
            page: 1,
            pageSize: 12
        };
        
        this.init();
    }

    async init() {
        this.bindEvents();
        await this.loadGenres();
        await this.loadMovies();
    }

    bindEvents() {
        // Search input with debounce
        const searchInput = document.getElementById('search-keyword');
        if (searchInput) {
            this.debounce(searchInput, 'input', () => {
                this.currentFilters.keyword = searchInput.value;
                this.currentFilters.page = 1;
                this.loadMovies();
            }, 500);
        }

        // Filter form submission
        const filtersForm = document.getElementById(this.filtersFormId);
        if (filtersForm) {
            filtersForm.addEventListener('change', (e) => {
                this.handleFilterChange(e);
            });
        }

        // Sort dropdown
        const sortSelect = document.getElementById('sort-select');
        if (sortSelect) {
            sortSelect.addEventListener('change', (e) => {
                const [sortBy, sortOrder] = e.target.value.split('-');
                this.currentFilters.sortBy = sortBy;
                this.currentFilters.sortOrder = sortOrder;
                this.currentFilters.page = 1;
                this.loadMovies();
            });
        }

        // Page size selector
        const pageSizeSelect = document.getElementById('page-size-select');
        if (pageSizeSelect) {
            pageSizeSelect.addEventListener('change', (e) => {
                this.currentFilters.pageSize = parseInt(e.target.value);
                this.currentFilters.page = 1;
                this.loadMovies();
            });
        }

        // Clear filters button
        const clearFiltersBtn = document.getElementById('clear-filters');
        if (clearFiltersBtn) {
            clearFiltersBtn.addEventListener('click', () => {
                this.clearAllFilters();
            });
        }

        // Advanced filters toggle
        const advancedToggle = document.getElementById('advanced-filters-toggle');
        if (advancedToggle) {
            advancedToggle.addEventListener('click', () => {
                this.toggleAdvancedFilters();
            });
        }
    }

    handleFilterChange(e) {
        const { name, value, type, checked } = e.target;
        
        if (type === 'checkbox') {
            this.currentFilters[name] = checked;
        } else if (type === 'date') {
            this.currentFilters[name] = value ? new Date(value) : null;
        } else if (type === 'number') {
            this.currentFilters[name] = value ? parseFloat(value) : null;
        } else {
            this.currentFilters[name] = value;
        }
        
        this.currentFilters.page = 1;
        this.loadMovies();
    }

    async loadGenres() {
        try {
            const response = await fetch(this.genresUrl);
            const result = await response.json();
            
            if (result.success && result.data) {
                this.renderGenreOptions(result.data);
            }
        } catch (error) {
            console.error('Error loading genres:', error);
        }
    }

    renderGenreOptions(genres) {
        const genreSelect = document.getElementById('genre-filter');
        if (!genreSelect) return;

        genreSelect.innerHTML = '<option value="">Tất cả thể loại</option>';
        genres.forEach(genre => {
            const option = document.createElement('option');
            option.value = genre.name;
            option.textContent = genre.name;
            genreSelect.appendChild(option);
        });
    }

    async loadMovies() {
        try {
            this.showLoading(true);
            
            const queryParams = new URLSearchParams();
            Object.keys(this.currentFilters).forEach(key => {
                if (this.currentFilters[key] !== null && this.currentFilters[key] !== '') {
                    if (this.currentFilters[key] instanceof Date) {
                        queryParams.append(key, this.currentFilters[key].toISOString().split('T')[0]);
                    } else {
                        queryParams.append(key, this.currentFilters[key]);
                    }
                }
            });

            const url = this.currentFilters.keyword ? 
                `${this.searchUrl}?${queryParams}` : 
                `${this.apiUrl}?${queryParams}`;

            const response = await fetch(url);
            const result = await response.json();
            
            if (result.success) {
                this.renderMovies(result.data);
                this.renderPagination(result.pagination || {});
                this.updateResultsInfo(result);
            } else {
                this.showError(result.message);
            }
        } catch (error) {
            console.error('Error loading movies:', error);
            this.showError('Có lỗi xảy ra khi tải danh sách phim');
        } finally {
            this.showLoading(false);
        }
    }

    renderMovies(movies) {
        const container = document.getElementById(this.containerId);
        if (!container) return;

        if (!movies || movies.length === 0) {
            container.innerHTML = `
                <div class="col-12">
                    <div class="alert alert-info text-center">
                        <i class="fas fa-search me-2"></i>
                        Không tìm thấy phim nào phù hợp với bộ lọc của bạn.
                    </div>
                </div>
            `;
            return;
        }

        container.innerHTML = movies.map(movie => this.createMovieCard(movie)).join('');
    }

    createMovieCard(movie) {
        const statusBadge = this.getStatusBadge(movie.status);
        const ratingStars = this.generateStars(movie.rating);
        
        return `
            <div class="col-lg-3 col-md-4 col-sm-6 mb-4">
                <div class="card movie-card h-100 shadow-sm">
                    <div class="position-relative">
                        <img src="${movie.imageUrl || movie.primaryImageUrl || '/images/default-movie.jpg'}" 
                             class="card-img-top movie-poster" alt="${movie.title}"
                             style="height: 300px; object-fit: cover;">
                        ${statusBadge}
                        ${movie.isFeatured ? '<span class="badge bg-warning position-absolute top-0 start-0 m-2">Nổi bật</span>' : ''}
                        ${movie.isRecommended ? '<span class="badge bg-success position-absolute top-0 end-0 m-2">Đề xuất</span>' : ''}
                    </div>
                    <div class="card-body d-flex flex-column">
                        <h5 class="card-title movie-title" title="${movie.title}">
                            ${this.truncateText(movie.title, 50)}
                        </h5>
                        <div class="movie-info mb-2">
                            <small class="text-muted">
                                <i class="fas fa-calendar me-1"></i>
                                ${new Date(movie.releaseDate).toLocaleDateString('vi-VN')}
                            </small>
                            <br>
                            <small class="text-muted">
                                <i class="fas fa-clock me-1"></i>
                                ${movie.runningTime} phút
                            </small>
                        </div>
                        <div class="movie-rating mb-2">
                            ${ratingStars}
                            <span class="rating-text ms-1">${movie.rating.toFixed(1)}/10</span>
                        </div>
                        <div class="movie-genres mb-2">
                            ${movie.genres ? movie.genres.slice(0, 2).map(g => 
                                `<span class="badge bg-secondary me-1">${g.name}</span>`
                            ).join('') : ''}
                        </div>
                        <p class="card-text movie-description flex-grow-1">
                            ${this.truncateText(movie.content || 'Không có mô tả', 100)}
                        </p>
                        <div class="mt-auto">
                            <a href="/Movies/Details/${movie.id}" class="btn btn-primary btn-sm w-100">
                                <i class="fas fa-info-circle me-1"></i>
                                Chi tiết
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    getStatusBadge(status) {
        const statusMap = {
            0: { text: 'Ngừng chiếu', class: 'bg-secondary' },
            1: { text: 'Sắp chiếu', class: 'bg-warning' },
            2: { text: 'Đang chiếu', class: 'bg-success' },
            3: { text: 'Hết suất', class: 'bg-danger' }
        };
        
        const statusInfo = statusMap[status] || statusMap[0];
        return `<span class="badge ${statusInfo.class} position-absolute bottom-0 end-0 m-2">${statusInfo.text}</span>`;
    }

    generateStars(rating) {
        const fullStars = Math.floor(rating / 2);
        const halfStar = (rating % 2) >= 1;
        const emptyStars = 5 - fullStars - (halfStar ? 1 : 0);
        
        let stars = '';
        for (let i = 0; i < fullStars; i++) {
            stars += '<i class="fas fa-star text-warning"></i>';
        }
        if (halfStar) {
            stars += '<i class="fas fa-star-half-alt text-warning"></i>';
        }
        for (let i = 0; i < emptyStars; i++) {
            stars += '<i class="far fa-star text-warning"></i>';
        }
        
        return stars;
    }

    renderPagination(pagination) {
        const container = document.getElementById(this.paginationId);
        if (!container || !pagination.totalPages) return;

        const { currentPage, totalPages, hasNextPage, hasPreviousPage } = pagination;
        
        let paginationHtml = `
            <nav aria-label="Movie pagination">
                <ul class="pagination justify-content-center">
                    <li class="page-item ${!hasPreviousPage ? 'disabled' : ''}">
                        <a class="page-link" href="#" data-page="${currentPage - 1}">
                            <i class="fas fa-chevron-left"></i>
                        </a>
                    </li>
        `;

        // Generate page numbers
        const startPage = Math.max(1, currentPage - 2);
        const endPage = Math.min(totalPages, currentPage + 2);

        if (startPage > 1) {
            paginationHtml += `<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>`;
            if (startPage > 2) {
                paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            paginationHtml += `
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                </li>
            `;
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
            paginationHtml += `<li class="page-item"><a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a></li>`;
        }

        paginationHtml += `
                    <li class="page-item ${!hasNextPage ? 'disabled' : ''}">
                        <a class="page-link" href="#" data-page="${currentPage + 1}">
                            <i class="fas fa-chevron-right"></i>
                        </a>
                    </li>
                </ul>
            </nav>
        `;

        container.innerHTML = paginationHtml;

        // Bind pagination events
        container.querySelectorAll('.page-link[data-page]').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const page = parseInt(e.target.closest('[data-page]').dataset.page);
                if (page && page !== currentPage) {
                    this.currentFilters.page = page;
                    this.loadMovies();
                    this.scrollToTop();
                }
            });
        });
    }

    updateResultsInfo(result) {
        const infoElement = document.getElementById('results-info');
        if (!infoElement) return;

        const { pagination, totalCount } = result;
        const total = pagination?.totalItems || totalCount || 0;
        const currentPage = pagination?.currentPage || 1;
        const pageSize = pagination?.pageSize || this.currentFilters.pageSize;
        
        const startItem = ((currentPage - 1) * pageSize) + 1;
        const endItem = Math.min(currentPage * pageSize, total);

        infoElement.innerHTML = `
            Hiển thị ${startItem}-${endItem} trong tổng số ${total} phim
        `;
    }

    clearAllFilters() {
        // Reset filters to default
        this.currentFilters = {
            keyword: '',
            status: 'all',
            genre: '',
            fromDate: null,
            toDate: null,
            minRating: null,
            maxRating: null,
            minDuration: null,
            maxDuration: null,
            sortBy: 'releaseDate',
            sortOrder: 'desc',
            page: 1,
            pageSize: 12
        };

        // Reset form inputs
        const form = document.getElementById(this.filtersFormId);
        if (form) {
            form.reset();
        }

        // Reload movies
        this.loadMovies();
    }

    toggleAdvancedFilters() {
        const advancedPanel = document.getElementById('advanced-filters-panel');
        const toggleIcon = document.querySelector('#advanced-filters-toggle i');
        
        if (advancedPanel) {
            advancedPanel.classList.toggle('show');
            if (toggleIcon) {
                toggleIcon.classList.toggle('fa-chevron-down');
                toggleIcon.classList.toggle('fa-chevron-up');
            }
        }
    }

    showLoading(show) {
        const container = document.getElementById(this.containerId);
        const loadingElement = document.getElementById('loading-indicator');
        
        if (show) {
            if (container) container.style.opacity = '0.6';
            if (loadingElement) loadingElement.style.display = 'block';
        } else {
            if (container) container.style.opacity = '1';
            if (loadingElement) loadingElement.style.display = 'none';
        }
    }

    showError(message) {
        const container = document.getElementById(this.containerId);
        if (container) {
            container.innerHTML = `
                <div class="col-12">
                    <div class="alert alert-danger">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        ${message}
                    </div>
                </div>
            `;
        }
    }

    scrollToTop() {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    truncateText(text, maxLength) {
        if (text.length <= maxLength) return text;
        return text.substr(0, maxLength) + '...';
    }

    debounce(element, event, callback, delay) {
        let timeoutId;
        element.addEventListener(event, (e) => {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(() => callback(e), delay);
        });
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('movies-container')) {
        window.movieSearch = new AdvancedMovieSearch();
    }
});

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AdvancedMovieSearch;
} 