class BookingListManager {
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
    this.allBookings = []; // Thêm biến lưu toàn bộ danh sách

    this.API_BASE_BE = "/api/v1";
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
      } else {
        this.showError("Không thể tải danh sách đặt vé");
      }
    } catch (error) {
      console.error("Error loading booking list:", error);
      this.showError("Lỗi khi tải danh sách đặt vé");
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
                    <div class="action-buttons">
                        <button class="btn-action btn-view" onclick="bookingListManager.viewBookingDetail('${
                          booking.id
                        }')" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                        ${
                          booking.bookingStatus !== "Cancelled"
                            ? `
                            <button class="btn-action btn-edit" onclick="bookingListManager.showUpdateStatusModal('${booking.id}', '${booking.bookingStatus}')" title="Cập nhật trạng thái">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn-action btn-delete" onclick="bookingListManager.showCancelModal('${booking.id}')" title="Hủy đặt vé">
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

    // Lọc dữ liệu dựa trên filters
    let filteredBookings = this.allBookings.filter((booking) => {
      // Lọc theo ngày
      if (this.filters.fromDate) {
        const bookingDate = new Date(booking.bookingDate);
        const fromDate = new Date(this.filters.fromDate);
        if (bookingDate < fromDate) return false;
      }

      if (this.filters.toDate) {
        const bookingDate = new Date(booking.bookingDate);
        const toDate = new Date(this.filters.toDate);
        toDate.setHours(23, 59, 59, 999); // Đến cuối ngày
        if (bookingDate > toDate) return false;
      }

      // Lọc theo trạng thái
      if (
        this.filters.bookingStatus &&
        booking.bookingStatus !== this.filters.bookingStatus
      ) {
        return false;
      }

      // Lọc theo tìm kiếm khách hàng
      if (this.filters.customerSearch) {
        const searchTerm = this.filters.customerSearch.toLowerCase();
        const customerName = booking.customerName?.toLowerCase() || "";
        const customerPhone = booking.customerPhone?.toLowerCase() || "";
        const customerEmail = booking.customerEmail?.toLowerCase() || "";

        if (
          !customerName.includes(searchTerm) &&
          !customerPhone.includes(searchTerm) &&
          !customerEmail.includes(searchTerm)
        ) {
          return false;
        }
      }

      // Lọc theo mã đặt vé
      if (
        this.filters.bookingCode &&
        !booking.bookingCode
          ?.toLowerCase()
          .includes(this.filters.bookingCode.toLowerCase())
      ) {
        return false;
      }

      return true;
    });

    // Sắp xếp
    filteredBookings.sort((a, b) => {
      let aValue, bValue;

      switch (this.filters.sortBy) {
        case "BookingDate":
          aValue = new Date(a.bookingDate);
          bValue = new Date(b.bookingDate);
          break;
        case "CustomerName":
          aValue = a.customerName || "";
          bValue = b.customerName || "";
          break;
        case "TotalAmount":
          aValue = a.totalAmount || 0;
          bValue = b.totalAmount || 0;
          break;
        default:
          aValue = new Date(a.bookingDate);
          bValue = new Date(b.bookingDate);
      }

      if (this.filters.sortDirection === "desc") {
        return aValue > bValue ? -1 : aValue < bValue ? 1 : 0;
      } else {
        return aValue < bValue ? -1 : aValue > bValue ? 1 : 0;
      }
    });

    // Phân trang
    const totalRecords = filteredBookings.length;
    const totalPages = Math.ceil(totalRecords / this.pageSize);

    // Đảm bảo currentPage không vượt quá totalPages
    if (this.currentPage > totalPages && totalPages > 0) {
      this.currentPage = totalPages;
    }

    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    const pagedBookings = filteredBookings.slice(start, end);

    // Render
    this.renderBookingTable(pagedBookings);
    this.renderPagination({
      totalRecords: totalRecords,
      currentPage: this.currentPage,
      pageSize: this.pageSize,
      totalPages: totalPages,
    });
  }

  renderPagination(data) {
    this.totalPages = data.totalPages;
    this.totalRecords = data.totalRecords;

    // Cập nhật thông tin phân trang
    this.updatePaginationInfo(data);

    // Cập nhật nút prev/next
    document
      .getElementById("prevPageBtn")
      ?.toggleAttribute("disabled", this.currentPage <= 1);
    document
      .getElementById("nextPageBtn")
      ?.toggleAttribute("disabled", this.currentPage >= this.totalPages);

    // Render page numbers
    const pagesWrapper = document.getElementById("pageNumbers");
    if (!pagesWrapper) return;
    pagesWrapper.innerHTML = "";

    const createPageBtn = (num, isActive = false) => {
      const el = document.createElement("div");
      el.className = "page-number" + (isActive ? " active" : "");
      el.textContent = num;
      el.onclick = () => this.loadPage(num);
      return el;
    };

    const createEllipsis = () => {
      const span = document.createElement("span");
      span.textContent = "...";
      span.style.padding = "8px 12px";
      span.style.color = "var(--text-muted)";
      return span;
    };

    if (this.totalPages <= 10) {
      for (let i = 1; i <= this.totalPages; i++) {
        pagesWrapper.appendChild(createPageBtn(i, i === this.currentPage));
      }
    } else {
      pagesWrapper.appendChild(createPageBtn(1, this.currentPage === 1));

      if (this.currentPage <= 4) {
        for (let i = 2; i <= 5; i++) {
          pagesWrapper.appendChild(createPageBtn(i, i === this.currentPage));
        }
        pagesWrapper.appendChild(createEllipsis());
      } else if (this.currentPage >= this.totalPages - 3) {
        pagesWrapper.appendChild(createEllipsis());
        for (let i = this.totalPages - 4; i < this.totalPages; i++) {
          pagesWrapper.appendChild(createPageBtn(i, i === this.currentPage));
        }
      } else {
        pagesWrapper.appendChild(createEllipsis());
        for (let i = this.currentPage - 1; i <= this.currentPage + 1; i++) {
          pagesWrapper.appendChild(createPageBtn(i, i === this.currentPage));
        }
        pagesWrapper.appendChild(createEllipsis());
      }

      pagesWrapper.appendChild(
        createPageBtn(this.totalPages, this.currentPage === this.totalPages)
      );
    }
  }

  updatePaginationInfo(data) {
    const startItem =
      data.totalRecords === 0 ? 0 : (data.currentPage - 1) * data.pageSize + 1;
    const endItem = Math.min(
      data.currentPage * data.pageSize,
      data.totalRecords
    );

    const infoElement = document.getElementById("paginationInfo");
    if (infoElement) {
      if (data.totalRecords === 0) {
        infoElement.textContent = "Không có dữ liệu";
      } else {
        infoElement.textContent = `Trang ${data.currentPage}/${data.totalPages} (Hiển thị ${startItem}-${endItem} của ${data.totalRecords})`;
      }
    }
  }

  loadPage(page) {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.applyFilters();
    }
  }

  changePageSize() {
    const newPageSize = parseInt(
      document.getElementById("pageSizeSelect").value
    );
    this.pageSize = newPageSize;
    this.currentPage = 1;
    this.applyFilters();
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
        `${this.API_BASE_BE}/booking-ticket/booking/${this.currentBookingId}/status`,
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
        `${this.API_BASE_BE}/booking-ticket/booking/${this.currentBookingId}/status`,
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
