/**
 * Demo sử dụng Shared Action Buttons trong trang Danh sách đặt vé
 * File này minh họa cách áp dụng shared action buttons vào BookingList
 */

// Demo: Cách tạo nút action cho một booking row
function createBookingActionButtons(booking) {
    const actionConfig = {
        view: {
            onClick: `viewBookingDetail('${booking.id}')`,
            title: 'Xem chi tiết đặt vé'
        }
    };

    // Chỉ hiển thị nút edit và delete nếu booking chưa bị hủy
    if (booking.bookingStatus !== 'Cancelled') {
        actionConfig.edit = {
            onClick: `showUpdateStatusModal('${booking.id}', '${booking.bookingStatus}')`,
            title: 'Cập nhật trạng thái'
        };
        
        actionConfig.delete = {
            onClick: `showCancelModal('${booking.id}')`,
            title: 'Hủy đặt vé'
        };
    }

    // Sử dụng kiểu btn-action (giống quản lý khuyến mãi)
    return createCRUDActions(actionConfig, ButtonStyle.ACTION);
}

// Demo: Cách render một booking row với shared action buttons
function renderBookingRowWithSharedButtons(booking) {
    const statusClass = getStatusClass(booking.bookingStatus);
    const statusText = getStatusText(booking.bookingStatus);
    const showDate = new Date(booking.showDate).toLocaleDateString("vi-VN");
    const showTime = booking.showTime ? formatTime(booking.showTime) : "N/A";
    const bookingDate = new Date(booking.bookingDate).toLocaleDateString("vi-VN");
    const totalAmount = new Intl.NumberFormat("vi-VN").format(booking.totalAmount);

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
                ${booking.usedPoints > 0 
                    ? `<br><small class="text-info">Đã dùng ${booking.usedPoints} điểm</small>`
                    : ""
                }
            </td>
            <td>
                <span class="status-badge ${statusClass}">${statusText}</span>
            </td>
            <td>${bookingDate}</td>
            <td>
                ${createBookingActionButtons(booking)}
            </td>
        </tr>
    `;
}

// Demo: Cách tạo nút xác nhận hủy đặt vé
function createCancelBookingButton(bookingId, customerName) {
    return createConfirmDeleteButton(
        `đặt vé của ${customerName}`,  // Tên item
        'confirmCancelBooking',        // Tên hàm xóa
        bookingId                       // ID của item
    );
}

// Demo: Cách tạo nút với loading state
function createBookingActionWithLoading(bookingId, actionType) {
    const button = createViewButton(
        `viewBookingDetailWithLoading('${bookingId}')`,
        'Xem chi tiết',
        ButtonStyle.ACTION,
        { id: `btn-view-${bookingId}` }
    );
    
    // Thêm loading state khi click
    $(document).on('click', `#btn-view-${bookingId}`, function() {
        setButtonLoading(this);
        // Simulate loading
        setTimeout(() => {
            removeButtonLoading(this);
        }, 2000);
    });
    
    return button;
}

// Demo: Cách tạo action group với custom styling
function createCustomBookingActions(booking) {
    const buttons = [];
    
    // Nút xem chi tiết
    buttons.push(createViewButton(
        `viewBookingDetail('${booking.id}')`,
        'Xem chi tiết',
        ButtonStyle.ACTION,
        { customClass: 'custom-view-btn' }
    ));
    
    // Nút in vé (nếu đã xác nhận)
    if (booking.bookingStatus === 'Confirmed') {
        buttons.push(createActionButton(
            'print',
            `printTicket('${booking.id}')`,
            'In vé',
            ButtonStyle.ACTION,
            { 
                icon: 'fas fa-print',
                customClass: 'btn-print'
            }
        ));
    }
    
    // Nút hủy (nếu chưa hủy)
    if (booking.bookingStatus !== 'Cancelled') {
        buttons.push(createDeleteButton(
            `showCancelModal('${booking.id}')`,
            'Hủy đặt vé',
            ButtonStyle.ACTION
        ));
    }
    
    return createActionGroup(buttons, ButtonStyle.ACTION);
}

// Demo: Cách sử dụng trong BookingListManager class
class BookingListManagerWithSharedButtons {
    constructor() {
        this.currentPage = 1;
        this.pageSize = 10;
        this.totalPages = 1;
        this.totalRecords = 0;
        this.currentBookingId = null;
        this.filters = {
            fromDate: null,
            toDate: null,
            movieTitle: "",
            bookingStatus: "",
            customerSearch: "",
            bookingCode: "",
            sortBy: "BookingDate",
            sortDirection: "desc",
        };
        this.allBookings = [];
    }

    // Sử dụng shared action buttons
    createBookingRow(booking) {
        const statusClass = this.getStatusClass(booking.bookingStatus);
        const statusText = this.getStatusText(booking.bookingStatus);
        const showDate = new Date(booking.showDate).toLocaleDateString("vi-VN");
        const showTime = booking.showTime ? this.formatTime(booking.showTime) : "N/A";
        const bookingDate = new Date(booking.bookingDate).toLocaleDateString("vi-VN");
        const totalAmount = new Intl.NumberFormat("vi-VN").format(booking.totalAmount);

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
                    ${booking.usedPoints > 0 
                        ? `<br><small class="text-info">Đã dùng ${booking.usedPoints} điểm</small>`
                        : ""
                    }
                </td>
                <td>
                    <span class="status-badge ${statusClass}">${statusText}</span>
                </td>
                <td>${bookingDate}</td>
                <td>
                    ${this.createBookingActionButtons(booking)}
                </td>
            </tr>
        `;
    }

    // Tạo action buttons sử dụng shared helpers
    createBookingActionButtons(booking) {
        const actionConfig = {
            view: {
                onClick: `this.viewBookingDetail('${booking.id}')`,
                title: 'Xem chi tiết đặt vé'
            }
        };

        // Chỉ hiển thị nút edit và delete nếu booking chưa bị hủy
        if (booking.bookingStatus !== 'Cancelled') {
            actionConfig.edit = {
                onClick: `this.showUpdateStatusModal('${booking.id}', '${booking.bookingStatus}')`,
                title: 'Cập nhật trạng thái'
            };
            
            actionConfig.delete = {
                onClick: `this.showCancelModal('${booking.id}')`,
                title: 'Hủy đặt vé'
            };
        }

        return createCRUDActions(actionConfig, ButtonStyle.ACTION);
    }

    // Các method khác...
    getStatusClass(status) {
        switch (status) {
            case "Completed":
                return "status-confirmed";
            case "Pending":
                return "status-pending";
            case "Cancelled":
                return "status-cancelled";
            default:
                return "status-pending";
        }
    }

    getStatusText(status) {
        switch (status) {
            case "Completed":
                return "Đã xác nhận";
            case "Pending":
                return "Chờ xác nhận";
            case "Cancelled":
                return "Đã hủy";
            default:
                return "Chờ xác nhận";
        }
    }

    formatTime(timeString) {
        if (!timeString) return "N/A";
        return timeString.substring(0, 5);
    }

    viewBookingDetail(bookingId) {
        console.log(`Xem chi tiết booking: ${bookingId}`);
        // Implementation...
    }

    showUpdateStatusModal(bookingId, currentStatus) {
        console.log(`Cập nhật trạng thái booking: ${bookingId}, status: ${currentStatus}`);
        // Implementation...
    }

    showCancelModal(bookingId) {
        console.log(`Hủy booking: ${bookingId}`);
        // Implementation...
    }
}

// Export cho sử dụng
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        createBookingActionButtons,
        renderBookingRowWithSharedButtons,
        createCancelBookingButton,
        createBookingActionWithLoading,
        createCustomBookingActions,
        BookingListManagerWithSharedButtons
    };
} 