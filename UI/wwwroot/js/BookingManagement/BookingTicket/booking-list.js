

class BookingListManager {
    constructor() {
        this.currentPage = 1;
        this.pageSize = 10;
        this.currentBookingId = null;
        this.filters = {
            fromDate: null,
            toDate: null,
            movieTitle: '',
            bookingStatus: '',
            customerSearch: '',
            bookingCode: '',
            sortBy: 'BookingDate',
            sortDirection: 'desc'
        };
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadBookingList();
        this.setupDateDefaults();
    }

    setupDateDefaults() {

        const today = new Date();
        const thirtyDaysAgo = new Date(today.getTime() - (30 * 24 * 60 * 60 * 1000));
        
        $('#fromDate').val(this.formatDate(thirtyDaysAgo));
        $('#toDate').val(this.formatDate(today));
    }

    formatDate(date) {
        return date.toISOString().split('T')[0];
    }

    bindEvents() {

        $('#fromDate, #toDate, #statusFilter, #customerSearch, #bookingCodeSearch').on('change input', () => {
            this.updateFilters();
        });


        $('#customerSearch, #bookingCodeSearch').on('keypress', (e) => {
            if (e.which === 13) {
                this.applyFilters();
            }
        });


        $('#cancelBookingModal').on('hidden.bs.modal', () => {
            $('#cancelReason').val('');
            this.currentBookingId = null;
        });

        $('#updateStatusModal').on('hidden.bs.modal', () => {
            this.currentBookingId = null;
        });
    }

    updateFilters() {
        this.filters.fromDate = $('#fromDate').val() || null;
        this.filters.toDate = $('#toDate').val() || null;
        this.filters.bookingStatus = $('#statusFilter').val() || '';
        this.filters.customerSearch = $('#customerSearch').val().trim() || '';
        this.filters.bookingCode = $('#bookingCodeSearch').val().trim() || '';
    }

    async loadBookingList(page = 1) {
        try {
            this.showLoading();
            this.currentPage = page;

            const params = new URLSearchParams({
                page: this.currentPage,
                pageSize: this.pageSize,
                sortBy: this.filters.sortBy,
                sortDirection: this.filters.sortDirection
            });


            if (this.filters.fromDate) params.append('fromDate', this.filters.fromDate);
            if (this.filters.toDate) params.append('toDate', this.filters.toDate);
            if (this.filters.movieTitle) params.append('movieTitle', this.filters.movieTitle);
            if (this.filters.bookingStatus) params.append('bookingStatus', this.filters.bookingStatus);
            if (this.filters.customerSearch) params.append('customerSearch', this.filters.customerSearch);
            if (this.filters.bookingCode) params.append('bookingCode', this.filters.bookingCode);

            const response = await fetch(`/BookingManagement/BookingTicket/GetBookingList?${params}`);
            const result = await response.json();

            if (result.success) {
                this.renderBookingTable(result.data.bookings);
                this.renderPagination(result.data);
                this.updatePaginationInfo(result.data);
            } else {
                this.showError(result.message || 'Không thể tải danh sách đặt vé');
            }
        } catch (error) {
            console.error('Error loading booking list:', error);
            this.showError('Có lỗi xảy ra khi tải danh sách đặt vé');
        } finally {
            this.hideLoading();
        }
    }

    renderBookingTable(bookings) {
        const tbody = $('#bookingTableBody');
        tbody.empty();

        if (!bookings || bookings.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="11" class="text-center py-4">
                        <i class="fas fa-inbox fa-2x text-muted mb-2"></i>
                        <p class="text-muted mb-0">Không có dữ liệu đặt vé</p>
                    </td>
                </tr>
            `);
            return;
        }

        bookings.forEach(booking => {
            const row = this.createBookingRow(booking);
            tbody.append(row);
        });
    }

    createBookingRow(booking) {
        const statusClass = this.getStatusClass(booking.bookingStatus);
        const statusText = this.getStatusText(booking.bookingStatus);
        const showDate = new Date(booking.showDate).toLocaleDateString('vi-VN');
        const showTime = booking.showTime ? this.formatTime(booking.showTime) : 'N/A';
        const bookingDate = new Date(booking.bookingDate).toLocaleDateString('vi-VN');
        const totalAmount = new Intl.NumberFormat('vi-VN').format(booking.totalAmount);

        return `
            <tr>
                <td>
                    <span class="fw-bold text-primary">${booking.bookingCode}</span>
                </td>
                <td>
                    <div>
                        <div class="fw-semibold">${booking.customerName}</div>
                        <small class="text-muted">${booking.customerEmail}</small>
                    </div>
                </td>
                <td>${booking.customerPhone}</td>
                <td>
                    <span class="fw-semibold">${booking.movieTitle}</span>
                </td>
                <td>${booking.cinemaRoom}</td>
                <td>
                    <div>
                        <div>${showDate}</div>
                        <small class="text-muted">${showTime}</small>
                    </div>
                </td>
                <td>
                    <span class="badge bg-light text-dark">${booking.seatNumbers}</span>
                </td>
                <td>
                    <span class="fw-bold text-success">${totalAmount} ₫</span>
                    ${booking.usedPoints > 0 ? `<br><small class="text-info">Đã dùng ${booking.usedPoints} điểm</small>` : ''}
                </td>
                <td>
                    <span class="status-badge ${statusClass}">${statusText}</span>
                </td>
                <td>${bookingDate}</td>
                <td>
                    <div class="d-flex">
                        <button class="action-btn btn-view" onclick="bookingManager.viewBookingDetail('${booking.id}')" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                        ${booking.bookingStatus !== 'Cancelled' ? `
                            <button class="action-btn btn-update" onclick="bookingManager.showUpdateStatusModal('${booking.id}', '${booking.bookingStatus}')" title="Cập nhật trạng thái">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="action-btn btn-cancel" onclick="bookingManager.showCancelModal('${booking.id}')" title="Hủy đặt vé">
                                <i class="fas fa-times"></i>
                            </button>
                            ${booking.paymentMethod && booking.paymentMethod.toLowerCase() === 'vnpay' && booking.bookingStatus === 'Pending' ? `
                                <button class="action-btn btn-pay" onclick="bookingManager.payBooking('${booking.id}', ${booking.totalAmount})" title="Thanh toán VNPay">
                                    <i class="fas fa-credit-card"></i>
                                </button>
                            ` : ''}
                        ` : ''}
                    </div>
                </td>
            </tr>
        `;
    }

    getStatusClass(status) {
        switch (status) {
            case 'Confirmed':
            case 1:
                return 'status-confirmed';
            case 'Pending':
            case 0:
                return 'status-pending';
            case 'Cancelled':
            case 'Canceled':
            case 2:
                return 'status-cancelled';
            case 'Completed':
            case 3:
                return 'status-completed';
            default:
                return 'status-pending';
        }
    }

    getStatusText(status) {
        switch (status) {
            case 'Confirmed':
            case 1:
                return 'Đã xác nhận';
            case 'Pending':
            case 0:
                return 'Chờ xác nhận';
            case 'Cancelled':
            case 'Canceled':
            case 2:
                return 'Đã hủy';
            case 'Completed':
            case 3:
                return 'Đã thanh toán';
            default:
                return status;
        }
    }

    formatTime(timeString) {
        try {

            const parts = timeString.split(':');
            return `${parts[0]}:${parts[1]}`;
        } catch {
            return timeString;
        }
    }

    renderPagination(data) {
        const pagination = $('#pagination');
        pagination.empty();

        if (data.totalPages <= 1) return;


        if (data.currentPage > 1) {
            pagination.append(`
                <li class="page-item">
                    <a class="page-link" href="#" onclick="bookingManager.loadBookingList(${data.currentPage - 1})">Trước</a>
                </li>
            `);
        }


        const startPage = Math.max(1, data.currentPage - 2);
        const endPage = Math.min(data.totalPages, data.currentPage + 2);

        if (startPage > 1) {
            pagination.append('<li class="page-item"><a class="page-link" href="#" onclick="bookingManager.loadBookingList(1)">1</a></li>');
            if (startPage > 2) {
                pagination.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            const activeClass = i === data.currentPage ? 'active' : '';
            pagination.append(`
                <li class="page-item ${activeClass}">
                    <a class="page-link" href="#" onclick="bookingManager.loadBookingList(${i})">${i}</a>
                </li>
            `);
        }

        if (endPage < data.totalPages) {
            if (endPage < data.totalPages - 1) {
                pagination.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
            }
            pagination.append(`<li class="page-item"><a class="page-link" href="#" onclick="bookingManager.loadBookingList(${data.totalPages})">${data.totalPages}</a></li>`);
        }


        if (data.currentPage < data.totalPages) {
            pagination.append(`
                <li class="page-item">
                    <a class="page-link" href="#" onclick="bookingManager.loadBookingList(${data.currentPage + 1})">Sau</a>
                </li>
            `);
        }
    }

    updatePaginationInfo(data) {
        const from = ((data.currentPage - 1) * data.pageSize) + 1;
        const to = Math.min(data.currentPage * data.pageSize, data.totalRecords);
        
        $('#showingFrom').text(from);
        $('#showingTo').text(to);
        $('#totalRecords').text(data.totalRecords);
    }

    async viewBookingDetail(bookingId) {
        try {
            this.showLoading();
            
            const response = await fetch(`/BookingManagement/BookingTicket/GetBookingDetail?bookingId=${bookingId}`);
            const result = await response.json();

            if (result.success) {
                this.renderBookingDetail(result.data);
                $('#bookingDetailModal').modal('show');
            } else {
                this.showError(result.message || 'Không thể tải chi tiết đặt vé');
            }
        } catch (error) {
            console.error('Error loading booking detail:', error);
            this.showError('Có lỗi xảy ra khi tải chi tiết đặt vé');
        } finally {
            this.hideLoading();
        }
    }

    renderBookingDetail(booking) {
        const content = $('#bookingDetailContent');
        const showDate = new Date(booking.showDate).toLocaleDateString('vi-VN');
        const showTime = booking.showTime ? this.formatTime(booking.showTime) : 'N/A';
        const bookingDate = new Date(booking.bookingDate).toLocaleDateString('vi-VN');
        const totalAmount = new Intl.NumberFormat('vi-VN').format(booking.totalAmount);

        content.html(`
            <div class="row">
                <div class="col-md-6">
                    <h6>Thông tin đặt vé</h6>
                    <table class="table table-borderless table-sm">
                        <tr><td><strong>Mã đặt vé:</strong></td><td>${booking.bookingCode}</td></tr>
                        <tr><td><strong>Trạng thái:</strong></td><td><span class="status-badge ${this.getStatusClass(booking.bookingStatus)}">${this.getStatusText(booking.bookingStatus)}</span></td></tr>
                        <tr><td><strong>Ngày đặt:</strong></td><td>${bookingDate}</td></tr>
                        <tr><td><strong>Phương thức thanh toán:</strong></td><td>${booking.paymentMethod}</td></tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <h6>Thông tin khách hàng</h6>
                    <table class="table table-borderless table-sm">
                        <tr><td><strong>Họ tên:</strong></td><td>${booking.customerName}</td></tr>
                        <tr><td><strong>Số điện thoại:</strong></td><td>${booking.customerPhone}</td></tr>
                        <tr><td><strong>Email:</strong></td><td>${booking.customerEmail}</td></tr>
                        ${booking.usedPoints > 0 ? `<tr><td><strong>Điểm đã sử dụng:</strong></td><td>${booking.usedPoints} điểm</td></tr>` : ''}
                    </table>
                </div>
            </div>
            <hr>
            <div class="row">
                <div class="col-md-6">
                    <h6>Thông tin suất chiếu</h6>
                    <table class="table table-borderless table-sm">
                        <tr><td><strong>Phim:</strong></td><td>${booking.movieTitle}</td></tr>
                        <tr><td><strong>Phòng chiếu:</strong></td><td>${booking.cinemaRoom}</td></tr>
                        <tr><td><strong>Ngày chiếu:</strong></td><td>${showDate}</td></tr>
                        <tr><td><strong>Giờ chiếu:</strong></td><td>${showTime}</td></tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <h6>Thông tin ghế và thanh toán</h6>
                    <table class="table table-borderless table-sm">
                        <tr><td><strong>Ghế:</strong></td><td>${booking.seatNumbers}</td></tr>
                        <tr><td><strong>Tổng tiền:</strong></td><td><span class="fw-bold text-success">${totalAmount} ₫</span></td></tr>
                    </table>
                </div>
            </div>
        `);
    }

    showUpdateStatusModal(bookingId, currentStatus) {
        this.currentBookingId = bookingId;
        $('#newStatus').val(currentStatus);
        $('#updateStatusModal').modal('show');
    }

    async confirmUpdateStatus() {
        const newStatus = $('#newStatus').val();
        
        if (!this.currentBookingId || !newStatus) {
            this.showError('Vui lòng chọn trạng thái mới');
            return;
        }

        try {
            this.showLoading();

            const response = await fetch(`/BookingManagement/BookingTicket/UpdateBookingStatus/${this.currentBookingId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ newStatus: newStatus })
            });

            const result = await response.json();

            if (result.success) {
                $('#updateStatusModal').modal('hide');
                this.showSuccess('Cập nhật trạng thái thành công');
                this.loadBookingList(this.currentPage);
            } else {
                this.showError(result.message || 'Không thể cập nhật trạng thái');
            }
        } catch (error) {
            console.error('Error updating booking status:', error);
            this.showError('Có lỗi xảy ra khi cập nhật trạng thái');
        } finally {
            this.hideLoading();
        }
    }

    showCancelModal(bookingId) {
        this.currentBookingId = bookingId;
        $('#cancelBookingModal').modal('show');
    }

    async confirmCancelBooking() {
        const reason = $('#cancelReason').val().trim();
        
        if (!this.currentBookingId || !reason) {
            this.showError('Vui lòng nhập lý do hủy đặt vé');
            return;
        }

        try {
            this.showLoading();

            const response = await fetch(`/BookingManagement/BookingTicket/CancelBooking/${this.currentBookingId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ reason: reason })
            });

            const result = await response.json();

            if (result.success) {
                $('#cancelBookingModal').modal('hide');
                this.showSuccess('Hủy đặt vé thành công');
                this.loadBookingList(this.currentPage);
            } else {
                this.showError(result.message || 'Không thể hủy đặt vé');
            }
        } catch (error) {
            console.error('Error cancelling booking:', error);
            this.showError('Có lỗi xảy ra khi hủy đặt vé');
        } finally {
            this.hideLoading();
        }
    }

    async payBooking(bookingId, amount) {
        try {
            this.showLoading();

            const resp = await fetch('/BookingManagement/BookingTicket/CreateVnpayPayment', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify({
                    bookingId: bookingId,
                    amount: amount,
                    decription: 'Thanh toan VNPay'
                })
            });

            const data = await resp.json();

            if (data.success && data.paymentUrl) {
                window.open(data.paymentUrl, '_blank');
            } else {
                this.showError(data.message || 'Không thể khởi tạo thanh toán VNPay');
            }
        } catch (err) {
            console.error(err);
            this.showError('Có lỗi xảy ra khi tạo thanh toán VNPay');
        } finally {
            this.hideLoading();
        }
    }

    showLoading() {
        $('#loadingOverlay').show();
    }

    hideLoading() {
        $('#loadingOverlay').hide();
    }

    showError(message) {

        alert('Lỗi: ' + message);
    }

    showSuccess(message) {

        alert('Thành công: ' + message);
    }
}


window.applyFilters = function() {
    bookingManager.updateFilters();
    bookingManager.loadBookingList(1);
};

window.confirmUpdateStatus = function() {
    bookingManager.confirmUpdateStatus();
};

window.confirmCancelBooking = function() {
    bookingManager.confirmCancelBooking();
};


$(document).ready(function() {
    window.bookingManager = new BookingListManager();
}); 