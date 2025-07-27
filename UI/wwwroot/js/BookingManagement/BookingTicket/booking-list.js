class BookingListManager {
  constructor() {
    this.currentPage = 1;
    this.pageSize = 5;
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
    this.allBookings = []; // Thêm biến lưu toàn bộ danh sách

    this.API_BASE_BE = "https://localhost:7049";
    this.init();
  }

  init() {
    this.bindEvents();
    this.loadBookingList();
    this.setupDateDefaults();
  }

  setupDateDefaults() {
    const today = new Date();
    const thirtyDaysAgo = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000);

    $("#fromDate").val(this.formatDate(thirtyDaysAgo));
    $("#toDate").val(this.formatDate(today));
  }

  formatDate(date) {
    return date.toISOString().split("T")[0];
  }

  bindEvents() {
    $(
      "#fromDate, #toDate, #statusFilter, #customerSearch, #bookingCodeSearch"
    ).on("change input", () => {
      this.updateFilters();
    });

    $("#customerSearch, #bookingCodeSearch").on("keypress", (e) => {
      if (e.which === 13) {
        this.applyFilters();
      }
    });

    $("#cancelBookingModal").on("hidden.bs.modal", () => {
      $("#cancelReason").val("");
      this.currentBookingId = null;
    });

    $("#updateStatusModal").on("hidden.bs.modal", () => {
      this.currentBookingId = null;
    });
  }

  updateFilters() {
    this.filters.fromDate = $("#fromDate").val() || null;
    this.filters.toDate = $("#toDate").val() || null;
    this.filters.bookingStatus = $("#statusFilter").val() || "";
    this.filters.customerSearch = $("#customerSearch").val().trim() || "";
    this.filters.bookingCode = $("#bookingCodeSearch").val().trim() || "";
  }

  async loadBookingList() {
    try {
      this.showLoading();
      // Lấy toàn bộ danh sách (pageSize lớn)
      const response = await fetch(
        `/BookingManagement/BookingTicket/GetBookingList?page=1&pageSize=1000`
      );
      const result = await response.json();

      if (result.success) {
        this.allBookings = result.data.bookings; // Lưu lại toàn bộ danh sách
        this.currentPage = 1;
        // Hiển thị tất cả vé, không lọc mặc định
        const start = (this.currentPage - 1) * this.pageSize;
        const end = start + this.pageSize;
        const paged = this.allBookings.slice(start, end);
        this.renderBookingTable(paged);
        this.renderPagination({
          totalRecords: this.allBookings.length,
          currentPage: this.currentPage,
          pageSize: this.pageSize,
          totalPages: Math.ceil(this.allBookings.length / this.pageSize),
        });
        this.updatePaginationInfo({
          totalRecords: this.allBookings.length,
          currentPage: this.currentPage,
          pageSize: this.pageSize,
          totalPages: Math.ceil(this.allBookings.length / this.pageSize),
        });
      } else {
        this.showError(result.message || "Không thể tải danh sách đặt vé");
      }
    } catch (error) {
      console.error("Error loading booking list:", error);
      this.showError("Có lỗi xảy ra khi tải danh sách đặt vé");
    } finally {
      this.hideLoading();
    }
  }

  renderBookingTable(bookings) {
    const tbody = $("#bookingTableBody");
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

    bookings.forEach((booking) => {
      const row = this.createBookingRow(booking);
      tbody.append(row);
    });
  }

  createBookingRow(booking) {
    const statusClass = this.getStatusClass(booking.bookingStatus);
    const statusText = this.getStatusText(booking.bookingStatus);
    const showDate = new Date(booking.showDate).toLocaleDateString("vi-VN");
    const showTime = booking.showTime
      ? this.formatTime(booking.showTime)
      : "N/A";
    const bookingDate = new Date(booking.bookingDate).toLocaleDateString(
      "vi-VN"
    );
    const totalAmount = new Intl.NumberFormat("vi-VN").format(
      booking.totalAmount
    );

    return `
            <tr>
                <td>
                    <span class="fw-bold text-primary">${
                      booking.bookingCode
                    }</span>
                </td>
                <td>
                    <div>
                        <div class="fw-semibold">${booking.customerName}</div>
                        <small class="text-muted">${
                          booking.customerEmail
                        }</small>
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
                    <span class="badge bg-light text-dark">${
                      booking.seatNumbers
                    }</span>
                </td>
                <td>
                    <span class="fw-bold text-success">${totalAmount} ₫</span>
                    ${
                      booking.usedPoints > 0
                        ? `<br><small class="text-info">Đã dùng ${booking.usedPoints} điểm</small>`
                        : ""
                    }
                </td>
                <td>
                    <span class="status-badge ${statusClass}">${statusText}</span>
                </td>
                <td>${bookingDate}</td>
                <td>
                    <div class="d-flex">
                        <button class="action-btn btn-view" onclick="bookingManager.viewBookingDetail('${
                          booking.id
                        }')" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                        ${
                          booking.bookingStatus !== "Cancelled"
                            ? `
                            <button class="action-btn btn-update" onclick="bookingManager.showUpdateStatusModal('${booking.id}', '${booking.bookingStatus}')" title="Cập nhật trạng thái">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="action-btn btn-cancel" onclick="bookingManager.showCancelModal('${booking.id}')" title="Hủy đặt vé">
                                <i class="fas fa-times"></i>
                            </button>
                        `
                            : ""
                        }
                    </div>
                </td>
            </tr>
        `;
  }

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
        return status;
    }
  }

  formatTime(timeString) {
    try {
      const parts = timeString.split(":");
      return `${parts[0]}:${parts[1]}`;
    } catch {
      return timeString;
    }
  }

  applyFilters() {
    this.updateFilters();
    let filtered = this.allBookings;

    // Lọc theo từng trường
    if (this.filters.fromDate) {
      filtered = filtered.filter(
        (b) => new Date(b.bookingDate) >= new Date(this.filters.fromDate)
      );
    }
    if (this.filters.toDate) {
      filtered = filtered.filter(
        (b) => new Date(b.bookingDate) <= new Date(this.filters.toDate)
      );
    }
    if (this.filters.bookingStatus) {
      filtered = filtered.filter(
        (b) => b.bookingStatus === this.filters.bookingStatus
      );
    }
    if (this.filters.customerSearch) {
      const search = this.filters.customerSearch.toLowerCase();
      filtered = filtered.filter(
        (b) =>
          b.customerName.toLowerCase().includes(search) ||
          b.customerPhone.includes(search) ||
          b.customerEmail.toLowerCase().includes(search)
      );
    }
    if (this.filters.bookingCode) {
      const code = this.filters.bookingCode.toLowerCase();
      filtered = filtered.filter((b) =>
        b.bookingCode.toLowerCase().includes(code)
      );
    }

    // Sắp xếp nếu cần
    if (this.filters.sortBy) {
      const sortKey = this.filters.sortBy;
      const direction = this.filters.sortDirection === "asc" ? 1 : -1;
      filtered = filtered.slice().sort((a, b) => {
        if (a[sortKey] < b[sortKey]) return -1 * direction;
        if (a[sortKey] > b[sortKey]) return 1 * direction;
        return 0;
      });
    }

    // Phân trang phía client
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    const paged = filtered.slice(start, end);

    this.renderBookingTable(paged);
    // Cập nhật lại phân trang nếu cần
    this.renderPagination({
      totalRecords: filtered.length,
      currentPage: this.currentPage,
      pageSize: this.pageSize,
      totalPages: Math.ceil(filtered.length / this.pageSize),
    });
    this.updatePaginationInfo({
      totalRecords: filtered.length,
      currentPage: this.currentPage,
      pageSize: this.pageSize,
      totalPages: Math.ceil(filtered.length / this.pageSize),
    });
  }

  renderPagination(data) {
    const pagination = $("#pagination");
    pagination.empty();
    console.log("Phân trang:", data);
    if (data.totalPages <= 1) return;
    for (let i = 1; i <= data.totalPages; i++) {
      const active = i === this.currentPage ? "active" : "";
      pagination.append(
        `<li class="page-item ${active}"><a class="page-link" href="#">${i}</a></li>`
      );
    }
    // Gán lại sự kiện click
    pagination
      .find("a")
      .off("click")
      .on("click", (e) => {
        e.preventDefault();
        const page = parseInt($(e.target).text());
        if (!isNaN(page)) {
          this.currentPage = page;
          this.applyFilters();
        }
      });
  }

  updatePaginationInfo(data) {
    const from = (data.currentPage - 1) * data.pageSize + 1;
    const to = Math.min(data.currentPage * data.pageSize, data.totalRecords);

    $("#showingFrom").text(from);
    $("#showingTo").text(to);
    $("#totalRecords").text(data.totalRecords);
  }

  viewBookingDetail(bookingId) {
    const booking = this.allBookings.find((b) => b.id === bookingId);
    if (booking) {
      this.renderBookingDetail(booking);
      $("#bookingDetailModal").modal("show");
    } else {
      this.showError("Không tìm thấy thông tin đặt vé");
    }
  }

  renderBookingDetail(booking) {
    const content = $("#bookingDetailContent");
    const showDate = new Date(booking.showDate).toLocaleDateString("vi-VN");
    const showTime = booking.showTime
      ? this.formatTime(booking.showTime)
      : "N/A";
    const bookingDate = new Date(booking.bookingDate).toLocaleDateString(
      "vi-VN"
    );
    const totalAmount = new Intl.NumberFormat("vi-VN").format(
      booking.totalAmount
    );

    content.html(`
            <div class="row">
                <div class="col-md-6">
                    <h6>Thông tin đặt vé</h6>
                    <table class="table table-borderless table-sm">
                        <tr><td><strong>Mã đặt vé:</strong></td><td>${
                          booking.bookingCode
                        }</td></tr>
                        <tr><td><strong>Trạng thái:</strong></td><td><span class="status-badge ${this.getStatusClass(
                          booking.bookingStatus
                        )}">${this.getStatusText(
      booking.bookingStatus
    )}</span></td></tr>
                        <tr><td><strong>Ngày đặt:</strong></td><td>${bookingDate}</td></tr>
                        <tr><td><strong>Phương thức thanh toán:</strong></td><td>${
                          booking.paymentMethod
                        }</td></tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <h6>Thông tin khách hàng</h6>
                    <table class="table table-borderless table-sm">
                        <tr><td><strong>Họ tên:</strong></td><td>${
                          booking.customerName
                        }</td></tr>
                        <tr><td><strong>Số điện thoại:</strong></td><td>${
                          booking.customerPhone
                        }</td></tr>
                        <tr><td><strong>Email:</strong></td><td>${
                          booking.customerEmail
                        }</td></tr>
                        ${
                          booking.usedPoints > 0
                            ? `<tr><td><strong>Điểm đã sử dụng:</strong></td><td>${booking.usedPoints} điểm</td></tr>`
                            : ""
                        }
                    </table>
                </div>
            </div>
            <hr>
            <div class="row">
                <div class="col-md-6">
                    <h6>Thông tin suất chiếu</h6>
                    <table class="table table-borderless table-sm">
                        <tr><td><strong>Phim:</strong></td><td>${
                          booking.movieTitle
                        }</td></tr>
                        <tr><td><strong>Phòng chiếu:</strong></td><td>${
                          booking.cinemaRoom
                        }</td></tr>
                        <tr><td><strong>Ngày chiếu:</strong></td><td>${showDate}</td></tr>
                        <tr><td><strong>Giờ chiếu:</strong></td><td>${showTime}</td></tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <h6>Thông tin ghế và thanh toán</h6>
                    <table class="table table-borderless table-sm">
                        <tr><td><strong>Ghế:</strong></td><td>${
                          booking.seatNumbers
                        }</td></tr>
                        <tr><td><strong>Tổng tiền:</strong></td><td><span class="fw-bold text-success">${totalAmount} ₫</span></td></tr>
                    </table>
                </div>
            </div>
        `);
  }

  showUpdateStatusModal(bookingId, currentStatus) {
    this.currentBookingId = bookingId;
    $("#newStatus").val(currentStatus);
    $("#updateStatusModal").modal("show");
  }

  async confirmUpdateStatus() {
    const newStatus = $("#newStatus").val();

    if (!this.currentBookingId || !newStatus) {
      this.showError("Vui lòng chọn trạng thái mới");
      return;
    }

    try {
      this.showLoading();

      const response = await fetch(
        `${this.API_BASE_BE}/api/v1/booking-ticket/booking/${this.currentBookingId}/status`,
        {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ newStatus: newStatus }),
        }
      );
      console.log("respone", response);
      const result = await response.json();
      console.log("ket qua", result);

      if (result.code === 200) {
        $("#updateStatusModal").modal("hide");
        this.loadBookingList(); // Reload to apply new status to the table
      } else {
        this.showError(result.message || "Không thể cập nhật trạng thái");
      }
    } catch (error) {
      console.error("Error updating booking status:", error);
      this.showError("Có lỗi xảy ra khi cập nhật trạng thái");
    } finally {
      this.hideLoading();
    }
  }

  showCancelModal(bookingId) {
    this.currentBookingId = bookingId;
    $("#cancelBookingModal").modal("show");
  }

  async confirmCancelBooking() {
    if (!this.currentBookingId) {
      this.showError("Không tìm thấy mã đặt vé");
      return;
    }
    try {
      this.showLoading();
      const response = await fetch(
        `${this.API_BASE_BE}/api/v1/booking-ticket/booking/${this.currentBookingId}/status`,
        {
          method: "PUT",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ newStatus: "Cancelled" }),
        }
      );
      const result = await response.json();
      if (result.code === 200) {
        $("#cancelBookingModal").modal("hide");
        this.loadBookingList();
      } else {
        this.showError(result.message || "Không thể hủy đặt vé");
      }
    } catch (error) {
      this.showError("Có lỗi xảy ra khi hủy đặt vé");
    } finally {
      this.hideLoading();
    }
  }

  showLoading() {
    $("#loadingOverlay").show();
  }

  hideLoading() {
    $("#loadingOverlay").hide();
  }

  showError(message) {
    alert("Lỗi: " + message);
  }

  showSuccess(message) {
    alert("Thành công: " + message);
  }
}

window.applyFilters = function () {
  bookingManager.updateFilters();
  bookingManager.applyFilters();
};

window.confirmUpdateStatus = function () {
  bookingManager.confirmUpdateStatus();
};

window.confirmCancelBooking = function () {
  bookingManager.confirmCancelBooking();
};

$(document).ready(function () {
  window.bookingManager = new BookingListManager();
});
