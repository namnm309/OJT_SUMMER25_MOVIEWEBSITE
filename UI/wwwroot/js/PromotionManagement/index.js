
// API Base URL
        const API_BASE = '/api/v1';
        // const API_BASE = 'https://localhost:7049/api/v1';


        let promotions = [];
        
        // Variables for pagination
        let currentPage = 1;
        let pageSize = 10; // Khớp với tuỳ chọn 10/trang mặc định
        let totalPromotions = 0;
        let totalPages = 0;
        
        // Fallback demo data when API is not available
        const fallbackPromotions = [
            {
                id: 1,
                title: "Đây là data ảo => api chưa gọi được",
                startDate: "2024-01-01",
                endDate: "2024-12-31",
                discountPercent: 50,
                description: "Áp dụng cho tất cả các suất chiếu vào thứ 7 và chủ nhật",
                imageUrl: "/images/promotion-placeholder.svg"
            },
            {
                id: 2,
                title: "Combo bỏng nước giảm 30%", 
                startDate: "2024-02-01",
                endDate: "2024-02-29",
                discountPercent: 30,
                description: "Mua combo bỏng nước với giá ưu đãi đặc biệt",
                imageUrl: "/images/promotion-placeholder.svg"
            }
        ];

        // Load promotions on page load
        document.addEventListener('DOMContentLoaded', function() {
            // Check if required elements exist before proceeding
            const promotionTableBody = document.getElementById('promotionTableBody');
            if (!promotionTableBody) {
                console.error('promotionTableBody element not found');
                showInitializationError();
                return;
            }
            
            // Đồng bộ pageSize với dropdown hiện tại
            const pageSizeSelect = document.getElementById('pageSizeSelect');
            pageSize = parseInt(pageSizeSelect.value);

            // Load initial page
            loadPromotionsPage(currentPage, pageSize);
            
            // Event listener for page size change
            let pageSizeChangeTimeout;
            
            pageSizeSelect.addEventListener('change', function() {
                clearTimeout(pageSizeChangeTimeout);
                
                // Show loading indicator
                const pageSizeContainer = this.closest('.pagination-size');
                const loadingIcon = document.createElement('span');
                loadingIcon.className = 'mini-loading';
                loadingIcon.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
                
                const existingLoading = pageSizeContainer.querySelector('.mini-loading');
                if (existingLoading) {
                    existingLoading.remove();
                }
                pageSizeContainer.appendChild(loadingIcon);
                
                pageSizeChangeTimeout = setTimeout(() => {
                    pageSize = parseInt(this.value);
                    currentPage = 1; // Reset to first page
                    loadPromotionsPage(currentPage, pageSize);
                    loadingIcon.remove();
                }, 500); // Debounce for 500ms
            });

            // Search functionality
            const promotionSearch = document.getElementById('promotionSearch');
            let searchTimeout;
            
            promotionSearch.addEventListener('input', function() {
                clearTimeout(searchTimeout);
                searchTimeout = setTimeout(() => {
                    const searchTerm = this.value.trim();
                    filterPromotions(searchTerm);
                }, 300); // Debounce for 300ms
            });
            
            // Pagination event listeners
            document.getElementById('prevPageBtn').addEventListener('click', () => {
                if (currentPage > 1) {
                    loadPromotionsPage(currentPage - 1, pageSize);
                }
            });
            
            document.getElementById('nextPageBtn').addEventListener('click', () => {
                if (currentPage < totalPages) {
                    loadPromotionsPage(currentPage + 1, pageSize);
                }
            });
        });

        // Load promotions with pagination
        async function loadPromotionsPage(page, size) {
            try {
                // Hiển thị loading ngay lập tức, không delay
                showTableLoading();
                
                const response = await fetch(`${API_BASE}/promotions`);

                if (response.ok) {
                    const result = await response.json();
                    console.log('API Response:', result); // Debug log

                    // Handle different response formats
                    let promotionList = [];
                    if (result.data && Array.isArray(result.data)) {
                        promotionList = result.data;
                    } else if (result.data && Array.isArray(result.data.items)) {
                        promotionList = result.data.items;
                    } else if (Array.isArray(result)) {
                        promotionList = result;
                    } else {
                        console.warn('Unexpected API response format:', result);
                        // Không throw error, chỉ log warning
                        promotionList = [];
                    }
                    
                    promotions = promotionList;
                    processPromotionData(promotions, page, size);
                    
                    // Clear any existing error notifications
                    const existingNotifications = document.querySelectorAll('.toast-notification.error');
                    existingNotifications.forEach(notification => notification.remove());
                    
                } else {
                    console.error('API response not ok:', response.status, response.statusText);
                    throw new Error(`API không phản hồi: ${response.status}`);
                }
                
            } catch (error) {
                console.error('Error loading promotions:', error);
                
                // Log more details for debugging
                if (error.message.includes('API không phản hồi')) {
                    console.error('API endpoint might be wrong or server not running');
                }
                
                // Show error message to user
                showNotification('Không thể tải dữ liệu khuyến mãi. Vui lòng thử lại sau.', 'error');
                
                // Fallback data ngay lập tức
                promotions = fallbackPromotions;
                processPromotionData(promotions, page, size);
            }
        }

        // Tách function xử lý data để tái sử dụng và tối ưu
        function processPromotionData(allPromotions, page, size) {
            const now = new Date();
            allPromotions.sort((a, b) => {
                    const aEnd = new Date(a.endDate);
                    const bEnd = new Date(b.endDate);
                    const aExpired = aEnd < now;
                    const bExpired = bEnd < now;

                    if (aExpired && !bExpired) return 1;
                    if (!aExpired && bExpired) return -1;

                    return aEnd - bEnd;
                });

            totalPromotions = allPromotions.length;
            totalPages = Math.ceil(totalPromotions / size);
            currentPage = page;
            
            const startIndex = (page - 1) * size;
            const endIndex = startIndex + size;
            const pagePromotions = allPromotions.slice(startIndex, endIndex);


            
            // Xử lý data tuần tự để tránh conflict
            displayPromotions(pagePromotions);
            updatePaginationInfo();
            updatePaginationControls();
            
            // Hide loading immediately after processing
            hideTableLoading();
        }

        // Display promotions in table
        function displayPromotions(promotionList) {
            const tableBody = document.getElementById('promotionTableBody');
            
            if (promotionList.length === 0) {
                tableBody.innerHTML = `
                    <tr>
                        <td colspan="6" class="empty-state">
                            <i class="fas fa-tags"></i>
                            <p>Không có khuyến mãi nào được tìm thấy</p>
                        </td>
                    </tr>
                `;
                return;
            }
            
            tableBody.innerHTML = promotionList.map(promotion => {
                const startDate = new Date(promotion.startDate);
                const endDate = new Date(promotion.endDate);
                const currentDate = new Date();
                
                let status = 'upcoming';
                let statusText = 'Sắp diễn ra';
                let statusClass = 'status-upcoming';
                
                if (currentDate >= startDate && currentDate <= endDate) {
                    status = 'active';
                    statusText = 'Đang diễn ra';
                    statusClass = 'status-active';
                } else if (currentDate > endDate) {
                    status = 'expired';
                    statusText = 'Đã hết hạn';
                    statusClass = 'status-expired';
                }
                
                return `
                    <tr>
                        <td>
                            <img src="${promotion.imageUrl || '/images/promotion-placeholder.svg'}" 
                                 alt="${promotion.title}" 
                                 class="promotion-image"
                                 onerror="this.src='/images/promotion-placeholder.svg'">
                        </td>
                        <td>
                            <div class="promotion-info">
                                <div class="promotion-title">${promotion.title}</div>
                                <div class="promotion-description">${promotion.description || 'Không có mô tả'}</div>
                            </div>
                        </td>
                        <td>
                            <div style="font-size: 12px;">
                                <strong>Từ:</strong> ${formatDate(startDate)}<br>
                                <strong>Đến:</strong> ${formatDate(endDate)}
                            </div>
                        </td>
                                        <td>
                    <div class="promotion-details">
                        <span class="discount-badge">
                            <i class="fas fa-percentage"></i>
                            ${promotion.discountPercent}%
                        </span>
                        ${(promotion.requiredPoints && promotion.requiredPoints > 0) ? 
                            `<span class="points-badge">
                                <i class="fas fa-coins"></i>
                                ${promotion.requiredPoints} điểm
                            </span>` : 
                            '<span class="free-badge">Miễn phí</span>'
                        }
                    </div>
                </td>
                        <td>
                            <span class="status-badge ${statusClass}">${statusText}</span>
                        </td>
                        <td>
                            <div class="action-buttons">
                                <button class="btn-action btn-view" onclick="viewPromotion('${promotion.id}')" title="Xem chi tiết">
                                    <i class="fas fa-eye"></i>
                                </button>
                                <button class="btn-action btn-edit" onclick="editPromotion('${promotion.id}')" title="Chỉnh sửa">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button class="btn-action btn-delete" onclick="deletePromotion('${promotion.id}')" title="Xóa">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </div>
                        </td>
                    </tr>
                `;
            }).join('');
        }

        // Show loading state
        function showTableLoading() {
            const tableContainer = document.querySelector('.promotion-table-container');
            if (tableContainer) {
                tableContainer.classList.add('loading');
            }
        }

        // Hide loading state  
        function hideTableLoading() {
            const tableContainer = document.querySelector('.promotion-table-container');
            if (tableContainer) {
                tableContainer.classList.remove('loading');
            }
        }

        // Show initialization error
        function showInitializationError() {
            const tableBody = document.getElementById('promotionTableBody');
            if (tableBody) {
                tableBody.innerHTML = `
                    <tr>
                        <td colspan="6" class="empty-state">
                            <i class="fas fa-exclamation-triangle"></i>
                            <p>Có lỗi xảy ra khi khởi tạo trang</p>
                        </td>
                    </tr>
                `;
            }
        }

        // Update pagination info
        function updatePaginationInfo() {
            const start = (currentPage - 1) * pageSize + 1;
            const end = Math.min(currentPage * pageSize, totalPromotions);
            const paginationInfo = document.getElementById('paginationInfo');
            paginationInfo.textContent = `Hiển thị ${start}-${end} trong tổng số ${totalPromotions} khuyến mãi`;
        }

        // Update pagination controls
        function updatePaginationControls() {
            const prevBtn = document.getElementById('prevPageBtn');
            const nextBtn = document.getElementById('nextPageBtn');
            const pageNumbers = document.getElementById('pageNumbers');
            
            prevBtn.disabled = currentPage <= 1;
            nextBtn.disabled = currentPage >= totalPages;
            
            // Generate page numbers
            let pageNumbersHTML = '';
            const maxVisiblePages = 5;
            let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
            let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);
            
            if (endPage - startPage + 1 < maxVisiblePages) {
                startPage = Math.max(1, endPage - maxVisiblePages + 1);
            }
            
            for (let i = startPage; i <= endPage; i++) {
                pageNumbersHTML += `
                    <span class="page-number ${i === currentPage ? 'active' : ''}" 
                          onclick="loadPromotionsPage(${i}, ${pageSize})">
                        ${i}
                    </span>
                `;
            }
            
            pageNumbers.innerHTML = pageNumbersHTML;
        }

        function goToPage(page) {
            const totalPages = Math.ceil(totalPromotions / pageSize);
            if (page < 1 || page > totalPages) return;

            processPromotionData(promotions, page, pageSize);
        }

        // Filter promotions by search term
        function filterPromotions(searchTerm) {
            if (!searchTerm) {
                loadPromotionsPage(1, pageSize);
                return;
            }
            
            const filteredPromotions = promotions.filter(promotion => 
                promotion.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
                (promotion.description && promotion.description.toLowerCase().includes(searchTerm.toLowerCase()))
            );
            
            totalPromotions = filteredPromotions.length;
            totalPages = Math.ceil(totalPromotions / pageSize);
            currentPage = 1;
            
            const startIndex = 0;
            const endIndex = pageSize;
            const pagePromotions = filteredPromotions.slice(startIndex, endIndex);
            
            displayPromotions(pagePromotions);
            updatePaginationInfo();
            updatePaginationControls();
        }

        // Format date for display
        function formatDate(date) {
            return date.toLocaleDateString('vi-VN', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric'
            });
        }

        // Modal functions
        function closeModal(modalId) {
            document.getElementById(modalId).style.display = 'none';
        }

        // Placeholder functions for promotion actions
        function viewPromotion(promotionId) {
            const promotion = promotions.find(p => p.id == promotionId);
            if (!promotion) return;
            
            const modal = document.getElementById('promotionDetailModal');
            const modalBody = modal.querySelector('.modal-body');
            
            const startDate = new Date(promotion.startDate);
            const endDate = new Date(promotion.endDate);
            const currentDate = new Date();
            
            let statusText = 'Sắp diễn ra';
            let statusClass = 'status-upcoming';
            
            if (currentDate >= startDate && currentDate <= endDate) {
                statusText = 'Đang diễn ra';
                statusClass = 'status-active';
            } else if (currentDate > endDate) {
                statusText = 'Đã hết hạn';
                statusClass = 'status-expired';
            }
            
            modalBody.innerHTML = `
                <div class="promotion-detail-modern">
                    <div class="promotion-detail-content">
                        <div class="promotion-detail-header">
                            <div class="promotion-detail-image-wrapper">
                                <img src="${promotion.imageUrl || '/images/promotion-placeholder.svg'}" 
                                     alt="${promotion.title}" 
                                     class="promotion-detail-image"
                                     onerror="this.src='/images/promotion-placeholder.svg'">
                                <div class="promotion-status ${statusClass}">${statusText}</div>
                            </div>
                            <div class="promotion-detail-info">
                                <h2 class="promotion-detail-title">${promotion.title}</h2>
                                <div class="promotion-detail-discount">
                                    <i class="fas fa-percentage"></i>
                                    Giảm ${promotion.discountPercent}%
                                </div>
                                <div class="promotion-detail-dates">
                                    <div class="date-info">
                                        <strong>Từ ngày:</strong> ${formatDate(startDate)}
                                    </div>
                                    <div class="date-info">
                                        <strong>Đến ngày:</strong> ${formatDate(endDate)}
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="promotion-detail-description">
                            <h4><i class="fas fa-info-circle"></i> Mô tả</h4>
                            <p>${promotion.description || 'Không có mô tả'}</p>
                        </div>
                    </div>
                </div>
                <style>
                    .promotion-detail-modern {
                        color: var(--text-dark);
                    }
                    .promotion-detail-header {
                        display: flex;
                        gap: 2rem;
                        margin-bottom: 2rem;
                    }
                    .promotion-detail-image-wrapper {
                        position: relative;
                        flex-shrink: 0;
                    }
                    .promotion-detail-image {
                        width: 200px;
                        height: 300px;
                        object-fit: cover;
                        border-radius: 12px;
                        box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
                    }
                    .promotion-status {
                        position: absolute;
                        bottom: -10px;
                        left: 50%;
                        transform: translateX(-50%);
                        padding: 8px 16px;
                        border-radius: 20px;
                        font-weight: 600;
                        font-size: 0.9rem;
                        white-space: nowrap;
                        box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
                    }
                    .promotion-detail-info {
                        flex: 1;
                    }
                    .promotion-detail-title {
                        font-size: 2rem;
                        font-weight: 700;
                        margin-bottom: 1rem;
                        color: var(--text-dark);
                    }
                    .promotion-detail-discount {
                        display: inline-flex;
                        align-items: center;
                        gap: 0.5rem;
                        background: linear-gradient(135deg, #ff6b6b, #ff8e8e);
                        color: white;
                        padding: 12px 24px;
                        border-radius: 20px;
                        font-weight: 600;
                        font-size: 1.2rem;
                        margin-bottom: 1.5rem;
                    }
                    .promotion-detail-dates {
                        display: flex;
                        flex-direction: column;
                        gap: 0.5rem;
                    }
                    .date-info {
                        color: var(--text-muted);
                        font-size: 14px;
                    }
                    .promotion-detail-description {
                        background: var(--content-secondary);
                        padding: 1.5rem;
                        border-radius: 12px;
                        border: 1px solid var(--content-border);
                    }
                    .promotion-detail-description h4 {
                        color: var(--text-dark);
                        margin-bottom: 1rem;
                        display: flex;
                        align-items: center;
                        gap: 0.5rem;
                    }
                    .promotion-detail-description p {
                        color: var(--text-muted);
                        line-height: 1.6;
                        margin: 0;
                    }
                </style>
            `;
            
            modal.style.display = 'block';
        }

        function editPromotion(promotionId) {
            const promotion = promotions.find(p => p.id == promotionId);
            if (!promotion) return;
            
            const modal = document.getElementById('editPromotionModal');
            const modalBody = modal.querySelector('.modal-body');
            
            const startDate = new Date(promotion.startDate).toISOString().split('T')[0];
            const endDate = new Date(promotion.endDate).toISOString().split('T')[0];
            
            modalBody.innerHTML = `
                <form id="editPromotionForm" onsubmit="submitEditPromotion(event, '${promotion.id}')">
                    <div class="form-row">
                        <div class="form-group">
                            <label for="editTitle">Tên khuyến mãi *</label>
                            <input type="text" id="editTitle" name="title" value="${promotion.title}" required>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label for="editStartDate">Ngày bắt đầu *</label>
                            <input type="date" id="editStartDate" name="startDate" value="${startDate}" required>
                        </div>
                        <div class="form-group">
                            <label for="editEndDate">Ngày kết thúc *</label>
                            <input type="date" id="editEndDate" name="endDate" value="${endDate}" required>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label for="editDiscountPercent">Phần trăm giảm giá (%) *</label>
                            <input type="number" id="editDiscountPercent" name="discountPercent" 
                                   value="${promotion.discountPercent}" min="1" max="100" required>
                        </div>
                                                 <div class="form-group">
                             <label for="editRequiredPoints">Điểm yêu cầu (0 = miễn phí)</label>
                             <input type="number" id="editRequiredPoints" name="requiredPoints" min="0" value="${promotion.requiredPoints || 0}">
                         </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group full-width">
                            <label for="editDescription">Mô tả</label>
                            <textarea id="editDescription" name="description" rows="4">${promotion.description || ''}</textarea>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group full-width">
                            <label for="editImageUrl">URL hình ảnh</label>
                            <input type="url" id="editImageUrl" name="imageUrl" value="${promotion.imageUrl || ''}">
                        </div>
                    </div>
                    <div class="form-actions">
                        <button type="button" class="btn-cancel" onclick="closeModal('editPromotionModal')">Hủy</button>
                        <button type="submit" class="btn-save">Cập nhật khuyến mãi</button>
                    </div>
                </form>
                <style>
                    .form-row {
                        display: flex;
                        gap: 20px;
                        margin-bottom: 20px;
                    }
                    .form-group {
                        flex: 1;
                        display: flex;
                        flex-direction: column;
                        gap: 8px;
                    }
                    .form-group.full-width {
                        width: 100%;
                    }
                    .form-group label {
                        font-size: 0.9rem;
                        font-weight: 500;
                        color: var(--text-dark);
                    }
                    .form-group input,
                    .form-group textarea {
                        padding: 12px 16px;
                        background: var(--content-bg);
                        border: 1px solid var(--content-border);
                        border-radius: 8px;
                        color: var(--text-dark);
                        font-size: 1rem;
                        transition: all 0.3s ease;
                    }
                    .form-group input:focus,
                    .form-group textarea:focus {
                        outline: none;
                        border-color: var(--primary-purple);
                        box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
                    }
                    .form-group textarea {
                        resize: vertical;
                    }
                    .form-actions {
                        display: flex;
                        justify-content: flex-end;
                        gap: 10px;
                        margin-top: 30px;
                        padding-top: 20px;
                        border-top: 1px solid var(--content-border);
                    }
                    .btn-cancel {
                        background: var(--content-secondary);
                        color: var(--text-dark);
                        border: 1px solid var(--content-border);
                        padding: 10px 24px;
                        border-radius: 8px;
                        font-weight: 600;
                        cursor: pointer;
                        transition: all 0.3s ease;
                    }
                    .btn-cancel:hover {
                        background: var(--content-bg);
                    }
                    .btn-save {
                        background: linear-gradient(135deg, var(--primary-purple), var(--primary-purple-dark));
                        color: white;
                        border: none;
                        padding: 10px 24px;
                        border-radius: 8px;
                        font-weight: 600;
                        cursor: pointer;
                        transition: all 0.3s ease;
                        min-width: 160px;
                    }
                    .btn-save:hover:not(:disabled) {
                        transform: translateY(-2px);
                        box-shadow: 0 6px 12px rgba(102, 126, 234, 0.4);
                    }
                    .btn-save:disabled {
                        opacity: 0.7;
                        cursor: not-allowed;
                    }
                </style>
            `;
            
            modal.style.display = 'block';
        }

        async function deletePromotion(promotionId) {
            if (!confirm('Bạn có chắc chắn muốn xóa khuyến mãi này?')) {
                return;
            }
            
            try {
                const response = await fetch(`${API_BASE}/promotions/${promotionId}`, {
                    method: 'DELETE',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    }
                });
                
                if (response.ok) {
                    showNotification('Xóa khuyến mãi thành công!', 'success');
                    loadPromotionsPage(currentPage, pageSize);
                } else {
                    throw new Error('Không thể xóa khuyến mãi');
                }
            } catch (error) {
                console.error('Error deleting promotion:', error);
                showNotification('Có lỗi xảy ra khi xóa khuyến mãi', 'error');
            }
        }

        function openAddPromotionModal() {
            const modal = document.getElementById('addPromotionModal');
            const modalBody = modal.querySelector('.modal-body');
            
            modalBody.innerHTML = `
                <form id="addPromotionForm" onsubmit="submitAddPromotion(event)">
                    <div class="form-row">
                        <div class="form-group">
                            <label for="addTitle">Tên khuyến mãi *</label>
                            <input type="text" id="addTitle" name="title" required>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label for="addStartDate">Ngày bắt đầu *</label>
                            <input type="date" id="addStartDate" name="startDate" required>
                        </div>
                        <div class="form-group">
                            <label for="addEndDate">Ngày kết thúc *</label>
                            <input type="date" id="addEndDate" name="endDate" required>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group">
                            <label for="addDiscountPercent">Phần trăm giảm giá (%) *</label>
                            <input type="number" id="addDiscountPercent" name="discountPercent" 
                                   min="1" max="100" required>
                        </div>
                                                 <div class="form-group">
                             <label for="addRequiredPoints">Điểm yêu cầu (0 = miễn phí)</label>
                             <input type="number" id="addRequiredPoints" name="requiredPoints" min="0" value="0">
                         </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group full-width">
                            <label for="addDescription">Mô tả</label>
                            <textarea id="addDescription" name="description" rows="4"></textarea>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-group full-width">
                            <label for="addImageUrl">URL hình ảnh</label>
                            <input type="url" id="addImageUrl" name="imageUrl">
                        </div>
                    </div>
                    <div class="form-actions">
                        <button type="button" class="btn-cancel" onclick="closeModal('addPromotionModal')">Hủy</button>
                        <button type="submit" class="btn-save">Thêm khuyến mãi</button>
                    </div>
                </form>
                <style>
                    .form-row {
                        display: flex;
                        gap: 20px;
                        margin-bottom: 20px;
                    }
                    .form-group {
                        flex: 1;
                        display: flex;
                        flex-direction: column;
                        gap: 8px;
                    }
                    .form-group.full-width {
                        width: 100%;
                    }
                    .form-group label {
                        font-size: 0.9rem;
                        font-weight: 500;
                        color: var(--text-dark);
                    }
                    .form-group input,
                    .form-group textarea {
                        padding: 12px 16px;
                        background: var(--content-bg);
                        border: 1px solid var(--content-border);
                        border-radius: 8px;
                        color: var(--text-dark);
                        font-size: 1rem;
                        transition: all 0.3s ease;
                    }
                    .form-group input:focus,
                    .form-group textarea:focus {
                        outline: none;
                        border-color: var(--primary-purple);
                        box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
                    }
                    .form-group textarea {
                        resize: vertical;
                    }
                    .form-actions {
                        display: flex;
                        justify-content: flex-end;
                        gap: 10px;
                        margin-top: 30px;
                        padding-top: 20px;
                        border-top: 1px solid var(--content-border);
                    }
                    .btn-cancel {
                        background: var(--content-secondary);
                        color: var(--text-dark);
                        border: 1px solid var(--content-border);
                        padding: 10px 24px;
                        border-radius: 8px;
                        font-weight: 600;
                        cursor: pointer;
                        transition: all 0.3s ease;
                    }
                    .btn-cancel:hover {
                        background: var(--content-bg);
                    }
                    .btn-save {
                        background: linear-gradient(135deg, var(--primary-purple), var(--primary-purple-dark));
                        color: white;
                        border: none;
                        padding: 10px 24px;
                        border-radius: 8px;
                        font-weight: 600;
                        cursor: pointer;
                        transition: all 0.3s ease;
                        min-width: 160px;
                    }
                    .btn-save:hover:not(:disabled) {
                        transform: translateY(-2px);
                        box-shadow: 0 6px 12px rgba(102, 126, 234, 0.4);
                    }
                    .btn-save:disabled {
                        opacity: 0.7;
                        cursor: not-allowed;
                    }
                </style>
            `;
            
            modal.style.display = 'block';
        }

        async function submitAddPromotion(event) {
            event.preventDefault();
            
            const form = event.target;
            const formData = new FormData(form);
            const submitBtn = form.querySelector('.btn-save');
            
            // Disable submit button and show loading
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang thêm...';
            
            try {
                                 const promotionData = {
                     title: formData.get('title'),
                     startDate: formData.get('startDate'),
                     endDate: formData.get('endDate'),
                     discountPercent: parseInt(formData.get('discountPercent')),
                     requiredPoints: parseFloat(formData.get('requiredPoints')) || 0,
                     description: formData.get('description'),
                     imageUrl: formData.get('imageUrl')
                 };
                
                const response = await fetch(`${API_BASE}/promotions`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify(promotionData)
                });
                
                if (response.ok) {
                    showNotification('Thêm khuyến mãi thành công!', 'success');
                    closeModal('addPromotionModal');
                    loadPromotionsPage(currentPage, pageSize);
                } else {
                    throw new Error('Không thể thêm khuyến mãi');
                }
            } catch (error) {
                console.error('Error adding promotion:', error);
                showNotification('Có lỗi xảy ra khi thêm khuyến mãi', 'error');
            } finally {
                // Re-enable submit button
                submitBtn.disabled = false;
                submitBtn.innerHTML = 'Thêm khuyến mãi';
            }
        }

        async function submitEditPromotion(event, promotionId) {
            event.preventDefault();
            
            const form = event.target;
            const formData = new FormData(form);
            const submitBtn = form.querySelector('.btn-save');
            
            // Disable submit button and show loading
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang cập nhật...';
            
            try {
                                 const promotionData = {
                     id: promotionId,
                     title: formData.get('title'),
                     startDate: formData.get('startDate'),
                     endDate: formData.get('endDate'),
                     discountPercent: parseInt(formData.get('discountPercent')),
                     requiredPoints: parseFloat(formData.get('requiredPoints')) || 0,
                     description: formData.get('description'),
                     imageUrl: formData.get('imageUrl')
                 };
                
                const response = await fetch(`${API_BASE}/promotions`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify(promotionData)
                });
                
                if (response.ok) {
                    showNotification('Cập nhật khuyến mãi thành công!', 'success');
                    closeModal('editPromotionModal');
                    loadPromotionsPage(currentPage, pageSize);
                } else {
                    throw new Error('Không thể cập nhật khuyến mãi');
                }
            } catch (error) {
                console.error('Error updating promotion:', error);
                showNotification('Có lỗi xảy ra khi cập nhật khuyến mãi', 'error');
            } finally {
                // Re-enable submit button
                submitBtn.disabled = false;
                submitBtn.innerHTML = 'Cập nhật khuyến mãi';
            }
        }

        // Show notification
        function showNotification(message, type = 'info') {
            // Remove existing notifications
            const existingNotifications = document.querySelectorAll('.notification');
            existingNotifications.forEach(notification => notification.remove());
            
            const notification = document.createElement('div');
            notification.className = `notification notification-${type}`;
            
            let icon = 'fas fa-info-circle';
            if (type === 'success') icon = 'fas fa-check-circle';
            if (type === 'error') icon = 'fas fa-exclamation-circle';
            
            notification.innerHTML = `
                <i class="${icon}"></i>
                <span>${message}</span>
                <button class="notification-close" onclick="this.parentElement.remove()">×</button>
            `;
            
            document.body.appendChild(notification);
            
            // Auto remove after 5 seconds
            setTimeout(() => {
                if (notification.parentElement) {
                    notification.remove();
                }
            }, 5000);
        }

        // Close modal when clicking outside
        window.onclick = function(event) {
            const modals = document.querySelectorAll('.modal');
            modals.forEach(modal => {
                if (event.target === modal) {
                    modal.style.display = 'none';
                }
            });
        }
 