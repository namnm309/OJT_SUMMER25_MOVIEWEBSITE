class BookTicketDashboard {
  constructor() {
    this.currentStep = 1;
    this.selectedMovie = null;
    this.selectedShowtime = null;
    this.selectedSeats = [];
    this.customerInfo = null;
    this.totalPrice = 0;
    this.movies = [];
    this.promotions = [];
    this.selectedPromotion = null;
    // Base URL backend API
    this.API_BASE_BE = "https://localhost:7049";

    this.init();
  }

  // tạo các method sử dụng
  init() {
    this.bindEvents();
    this.loadMovies();
    this.loadPromotions();
    this.updateStepDisplay(); // Đảm bảo hiển thị đúng bước ban đầu
  }

  // gán click, keypress, change events cho các nút và input
  bindEvents() {
    $("#nextBtn").on("click", () => this.nextStep());
    $("#prevBtn").on("click", () => this.prevStep());
    $("#confirmBtn").on("click", () => this.confirmBooking());

    $("#movieSearch").on("input", (e) => this.searchMovies(e.target.value));

    const searchButton = $("#searchCustomer");
    const phoneInput = $("#customerPhone");

    if (searchButton.length > 0) {
      searchButton.on("click", () => {
        this.searchCustomer();
      });
    }

    if (phoneInput.length > 0) {
      phoneInput.on("keypress", (e) => {
        if (e.which === 13) {
          this.searchCustomer();
        }
      });
    }

    $("#createCustomer").on("click", () => this.showCreateCustomerModal());
    $("#saveCustomer").on("click", () => this.createCustomer());

    $('input[name="paymentMethod"]').on("change", () =>
      this.updateOrderSummary()
    );

    // Add event for promotion selection
    $("#promotionSelect").on("change", (e) => {
      const selectedId = $(e.target).val();
      this.selectedPromotion = this.promotions.find(
        (p) => p.id && p.id.toString() === selectedId
      );
      // You may want to update order summary or apply promotion logic here
      // For now, just updateOrderSummary if needed
      this.updateOrderSummary();
    });
  }

  async loadMovies() {
    try {
      this.showLoading();
      const response = await fetch(
        "/BookingManagement/BookingTicket/GetMovies"
      );
      const data = await response.json();

      if (data.success) {
        this.movies = data.data; // Store data for later use
        this.displayMovies(data.data);
      } else {
        this.showError("Không thể tải danh sách phim");
      }
    } catch (error) {
      this.showError("Có lỗi xảy ra khi tải danh sách phim");
    } finally {
      this.hideLoading();
    }
  }

  //dropdown moivie để chọn
  displayMovies(movies) {
    const movieSelect = $("#movieSelect");
    const selectedMovieInfo = $("#selectedMovieInfo");

    movieSelect.find("option:not(:first)").remove();
    selectedMovieInfo.hide();

    if (!movies || movies.length === 0) {
      movieSelect.append(
        "<option disabled>Hiện tại không có phim đang chiếu</option>"
      );
      return;
    }

    movies.forEach((movie) => {
      const option = $(`<option value="${movie.id}">${movie.title}</option>`);
      option.data("movie", movie);
      movieSelect.append(option);
    });

    movieSelect.off("change").on("change", (e) => {
      const selectedOption = $(e.target).find("option:selected");
      const movie = selectedOption.data("movie");

      if (movie) {
        this.selectMovie(movie);
        this.displaySelectedMovieInfo(movie);
      } else {
        // Reset trạng thái khi không chọn phim
        this.selectedMovie = null;
        this.selectedShowtime = null;
        this.selectedSeats = [];
        selectedMovieInfo.hide();
        $("#showtimeSelection").html(
          '<p class="text-muted">Vui lòng chọn phim trước</p>'
        );
        this.updateStepDisplay(); // Cập nhật thanh tiến trình
      }
    });
  }

  // chọn dropdown xong thì show poster, tên, thể loại, thời lượng phim
  displaySelectedMovieInfo(movie) {
    const selectedMovieInfo = $("#selectedMovieInfo");
    selectedMovieInfo.html(`
            <div class="selected-movie-card">
                <div class="row">
                    <div class="col-md-4">
                        <img src="${
                          movie.primaryImageUrl || "/images/default-movie.jpg"
                        }" alt="${movie.title}" class="img-fluid rounded">
                    </div>
                    <div class="col-md-8">
                        <h6 class="movie-title">${movie.title}</h6>
                        <p class="movie-genre text-muted">${
                          movie.genre || "Chưa phân loại"
                        }</p>
                        <p class="movie-duration text-muted">${
                          movie.duration || 0
                        } phút</p>
                    </div>
                </div>
            </div>
        `);
    selectedMovieInfo.show();
  }

  // lưu phim đã chọn và tải lịch chiếu
  async selectMovie(movie) {
    this.selectedMovie = movie;
    this.selectedShowtime = null; // Reset suất chiếu khi chọn phim mới
    this.selectedSeats = []; // Reset ghế đã chọn

    await this.loadShowtimes(movie.id);
    this.updateStepDisplay(); // Cập nhật thanh tiến trình
  }

  //lấy ngày chiếu và giờ chiếu của phim

  async loadShowtimes(movieId) {
    try {
      this.showLoading();

      const datesResponse = await fetch(
        `/BookingManagement/BookingTicket/GetShowDates?movieId=${movieId}`
      );
      const datesData = await datesResponse.json();

      if (datesData.success && datesData.data && datesData.data.length > 0) {
        // 1. Lấy ngày hôm nay và loại bỏ phần giờ để so sánh chính xác
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        // 2. ✅ Tạo biến selectDate bằng cách lọc mảng datesData.data
        const selectDate = datesData.data.filter((dateItem) => {
          const itemDate = new Date(dateItem.code);
          return itemDate >= today;
        });

        const showDatesWithTimes = [];

        for (const dateItem of selectDate) {
          try {
            const timesResponse = await fetch(
              `/BookingManagement/BookingTicket/GetShowTimes?movieId=${movieId}&showDate=${encodeURIComponent(
                dateItem.code
              )}`
            );
            const timesData = await timesResponse.json();

            if (timesData.success) {
              const showtimes = Array.isArray(timesData.data)
                ? timesData.data
                : timesData.data?.showtimes || [];

              if (showtimes.length > 0) {
                showDatesWithTimes.push({
                  date: dateItem.text,
                  dateCode: dateItem.code,
                  showtimes: showtimes.map((time) => ({
                    id: time.id,
                    startTime: time.time || time.startTime,
                  })),
                });
              } else {
                showDatesWithTimes.push({
                  date: dateItem.text,
                  dateCode: dateItem.code,
                  showtimes: [],
                });
              }
            } else {
              showDatesWithTimes.push({
                date: dateItem.text,
                dateCode: dateItem.code,
                showtimes: [],
              });
            }
          } catch (timeError) {
            showDatesWithTimes.push({
              date: dateItem.text,
              dateCode: dateItem.code,
              showtimes: [],
            });
          }
        }

        this.displayShowtimes(showDatesWithTimes);
      } else {
        this.displayShowtimes([]);
      }
    } catch (error) {
      this.displayShowtimes([]);
      this.showError("Không thể tải lịch chiếu. Vui lòng thử lại.");
    } finally {
      this.hideLoading();
    }
  }

  // tạo các nút giờ chiếu theo từng ngày
  displayShowtimes(showDates) {
    const showtimeSelection = $("#showtimeSelection");
    if (!showtimeSelection.length) {
      return;
    }
    showtimeSelection.empty();

    if (!showDates || showDates.length === 0) {
      showtimeSelection.html('<p class="text-muted">Không có lịch chiếu</p>');
      return;
    }

    showDates.forEach((dateGroup) => {
      const fullDate = this.formatFullDate(dateGroup.dateCode);

      const dateSection = $(`
                <div class="showtime-date">
                    <h6>${fullDate}</h6>
                    <div class="showtime-list"></div>
                </div>
            `);

      const showtimeList = dateSection.find(".showtime-list");

      if (
        dateGroup.showtimes &&
        Array.isArray(dateGroup.showtimes) &&
        dateGroup.showtimes.length > 0
      ) {
        dateGroup.showtimes.forEach((showtime) => {
          const showtimeBtn = $(`
                        <button class="btn btn-outline-primary btn-sm showtime-btn" data-showtime-id="${showtime.id}">
                            ${showtime.startTime}
                        </button>
                    `);

          showtimeBtn.on("click", () =>
            this.selectShowtime({
              id: showtime.id,
              startTime: showtime.startTime,
            })
          );
          showtimeList.append(showtimeBtn);
        });
      } else {
        showtimeList.html('<p class="text-muted">Không có suất chiếu</p>');
      }

      showtimeSelection.append(dateSection);
    });
  }

  // lưu suất chiếu và => step 2 chọn ghế
  selectShowtime(showtime) {
    this.selectedShowtime = showtime;
    this.selectedSeats = []; // Reset ghế đã chọn khi chọn suất chiếu mới

    $(".showtime-btn").removeClass("active");
    $(`.showtime-btn[data-showtime-id="${showtime.id}"]`).addClass("active");

    this.updateStepDisplay(); // Cập nhật thanh tiến trình ngay lập tức

    setTimeout(() => {
      this.currentStep = 2;
      this.updateStepDisplay();
      this.loadSeats(showtime.id);
    }, 500); // Small delay for better UX
  }

  // lấy danh sách ghế của suất chiếu và show sơ đồ ghế

  async loadSeats(showtimeId) {
    try {
      const url = `/BookingManagement/BookingTicket/GetSeats?showTimeId=${showtimeId}`;

      const response = await fetch(url);

      const data = await response.json();

      if (data.success) {
        this.displaySeats(data.data);
      } else {
        this.showError("Không thể tải sơ đồ ghế: " + data.message);
      }
    } catch (error) {
      this.showError("Có lỗi xảy ra khi tải sơ đồ ghế");
    }
  }

  // show sơ đồ ghế theo hàng, hiển thị trạng thái ghế , khó siu cấp
  displaySeats(seats) {
    const seatMap = $("#seatMap");
    seatMap.empty();

    // Kiểm tra nếu có nested structure (RoomName + Seats)
    let seatData = seats;
    if (seats && seats.seats) {
      seatData = seats.seats;
    } else if (seats && seats.data && seats.data.seats) {
      seatData = seats.data.seats;
    } else if (Array.isArray(seats)) {
      seatData = seats;
    }

    if (!seatData || seatData.length === 0) {
      seatMap.html('<p class="text-muted">Không có dữ liệu ghế</p>');
      return;
    }

    const seatRows = {};
    seatData.forEach((seat, index) => {
      const rowName = seat.seatCode ? seat.seatCode.charAt(0) : "A";
      if (!seatRows[rowName]) {
        seatRows[rowName] = [];
      }

      const transformedSeat = {
        id: seat.id,
        rowName: rowName,
        seatNumber: seat.seatCode ? seat.seatCode.slice(1) : "1", // Extract number part
        seatCode: seat.seatCode,
        seatType: seat.seatType,
        isBooked: !seat.isAvailable, // Backend: isAvailable, Frontend: isBooked
        price: seat.price || 0,
      };

      seatRows[rowName].push(transformedSeat);
    });

    Object.keys(seatRows)
      .sort()
      .forEach((rowName) => {
        const row = $(
          `<div class="seat-row"><span class="row-label">${rowName}</span></div>`
        );

        seatRows[rowName]
          .sort((a, b) => parseInt(a.seatNumber) - parseInt(b.seatNumber))
          .forEach((seat) => {
            const seatElement = $(`
                    <div class="seat ${
                      seat.isBooked ? "occupied" : "available"
                    } ${seat.seatType === "VIP" ? "vip" : ""}" 
                         data-seat-id="${seat.id}" 
                         data-seat-name="${seat.seatCode}"
                         data-seat-price="${seat.price}">
                        ${seat.seatNumber}
                    </div>
                `);

            if (!seat.isBooked) {
              seatElement.on("click", () => this.toggleSeat(seat, seatElement));
            }

            row.append(seatElement);
          });

        seatMap.append(row);
      });
  }

  // thêm/xóa ghế khỏi danh sách đã chọn
  toggleSeat(seat, seatElement) {
    const seatId = seat.id;
    const seatIndex = this.selectedSeats.findIndex((s) => s.id === seatId);

    if (seatIndex > -1) {
      this.selectedSeats.splice(seatIndex, 1);
      seatElement.removeClass("selected");
    } else {
      this.selectedSeats.push(seat);
      seatElement.addClass("selected");

      // Tự động chuyển sang bước 2 ngay khi chọn ghế đầu tiên
      if (this.currentStep === 1 && this.selectedSeats.length === 1) {
        console.log(
          "Chuyển từ bước 1 sang bước 2 khi chọn ghế:",
          seat.seatCode
        );
        this.currentStep = 2;
      }
    }

    this.updateSelectedSeats();
    this.updateStepDisplay(); // Cập nhật thanh tiến trình khi thay đổi ghế
  }

  // hiển thị danh sách ghế đã chọn và tính tổng tiền
  updateSelectedSeats() {
    const selectedSeatsContainer = $("#selectedSeats");

    if (this.selectedSeats.length === 0) {
      selectedSeatsContainer.html(
        '<p class="text-muted">Chưa có ghế nào được chọn</p>'
      );
      $("#nextBtn").prop("disabled", true);
      return;
    }

    let html = '<div class="selected-seats-list">';
    let totalPrice = 0;

    this.selectedSeats.forEach((seat) => {
      html += `
                <div class="selected-seat-item">
                    <span class="seat-name">${seat.seatCode}</span>
                    <span class="seat-price">${this.formatCurrency(
                      seat.price
                    )}</span>
                </div>
            `;
      totalPrice += seat.price;
    });

    html += `
            <div class="selected-seats-total">
                <strong>Tổng: ${this.formatCurrency(totalPrice)}</strong>
            </div>
        </div>`;

    selectedSeatsContainer.html(html);
    this.totalPrice = totalPrice;
    $("#nextBtn").prop("disabled", false);
  }

  async searchCustomer() {
    // Kiểm tra input
    const searchInput = $("#customerPhone");
    const searchTerm = searchInput.val().trim();

    // Xóa thông báo cũ
    this.clearSearchMessage();

    if (!searchTerm) {
      this.showSearchError("Vui lòng nhập số điện thoại hoặc email");
      return;
    }

    try {
      this.showLoading();

      // Gọi API thông qua UI controller
      const response = await fetch(
        `/BookTicket/SearchCustomer?searchTerm=${encodeURIComponent(
          searchTerm
        )}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Accept: "application/json",
          },
        }
      );

      if (response.ok) {
        const result = await response.json();
        console.log("thong tinh ne", result);
        //data:
        //email:"namnguyen@example.com"
        //fullName:"Nguyễn Văn Nammmmm"
        //id:"aa6ea0df-33ca-4914-b265-eaf9880ccfd1"
        //lastBookingDate:"2025-07-19"
        //phoneNumber:"0912345678"
        //points: 475
        //totalBookings:30
        if (result.success) {
          this.hideLoading();
          this.customerInfo = result.data;
          this.displayCustomerInfo(result.data);
        } else {
          this.hideLoading();
          this.showSearchError(result.message || "Không tìm thấy khách hàng");
        }
      } else {
        const errorData = await response.json();
        this.hideLoading();
        this.showSearchError(errorData.message || "Không tìm thấy khách hàng");
      }
    } catch (error) {
      this.hideLoading();
      this.showSearchError("Có lỗi xảy ra khi tìm kiếm khách hàng");
    }
  }

  //Ẩn và xóa nội dung thông báo tìm kiếm
  clearSearchMessage() {
    $("#searchMessage")
      .hide()
      .removeClass("alert alert-success alert-danger")
      .empty();
  }

  showSearchError(message) {
    const searchTerm = $("#customerPhone").val().trim();

    // Nếu thông báo lỗi là "không tìm thấy" và có search term, hiển thị nút tạo mới
    if (message.toLowerCase().includes("không tìm thấy") && searchTerm) {
      $("#searchMessage")
        .removeClass("alert-success")
        .addClass("alert alert-warning")
        .html(
          `
                    <div class="d-flex justify-content-between align-items-center">
                        <span><i class="fas fa-exclamation-triangle me-2"></i>${message}</span>
                        <button type="button" class="btn btn-sm btn-success" id="createNewCustomerBtn">
                            <i class="fas fa-user-plus me-1"></i>Tạo khách hàng mới
                        </button>
                    </div>
                `
        )
        .show();

      // Bind event cho nút tạo mới
      $("#createNewCustomerBtn").on("click", () =>
        this.showCreateCustomerModal()
      );
    } else {
      $("#searchMessage")
        .removeClass("alert-success")
        .addClass("alert alert-danger")
        .html(`<i class="fas fa-exclamation-circle me-2"></i>${message}`)
        .show();
    }
  }

  showSearchSuccess(message) {
    $("#searchMessage")
      .removeClass("alert-danger")
      .addClass("alert alert-success")
      .html(`<i class="fas fa-check-circle me-2"></i>${message}`)
      .show();
  }

  displayCustomerInfo(customer) {
    const customerInfoContainer = $("#customerInfo");

    if (!customer) {
      customerInfoContainer.hide();
      return;
    }

    // Xóa thông báo tìm kiếm khi hiển thị thông tin thành công
    this.clearSearchMessage();

    // Kiểm tra các trường dữ liệu
    const lastBookingDate = customer.lastBookingDate
      ? this.formatDate(customer.lastBookingDate)
      : "Chưa có";

    // Nếu không có container, tạo mới
    if (customerInfoContainer.length === 0) {
      $(".booking-container").append(`
                <div id="customerInfo" class="customer-info-section mt-3">
                    
                </div>
            `);
    }

    // Tạo HTML chi tiết
    const customerInfoHtml = `
            <div class="card">
                <div class="card-header">
                    <h5><i class="fas fa-user me-2"></i>Thông tin khách hàng</h5>
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-3 text-center">
                            <div class="customer-avatar">
                                ${customer.fullName.charAt(0).toUpperCase()}
                            </div>
                        </div>
                        <div class="col-md-9">
                            <h4>${customer.fullName}</h4>
                            <p><strong>Email:</strong> ${
                              customer.email || "Chưa cập nhật"
                            }</p>
                            <p><strong>Số điện thoại:</strong> ${
                              customer.phoneNumber
                            }</p>
                            <div class="customer-stats row">
                                <div class="col-4">
                                    <strong>Điểm tích lũy</strong>
                                    <p>${customer.points} điểm</p>
                                </div>
                                <div class="col-4">
                                    <strong>Tổng số vé</strong>
                                    <p>${customer.totalBookings} vé</p>
                                </div>
                                <div class="col-4">
                                    <strong>Lần đặt cuối</strong>
                                    <p>${lastBookingDate}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;

    // Hiển thị thông tin
    customerInfoContainer.html(customerInfoHtml).show();

    // Lưu thông tin khách hàng
    this.customerInfo = customer;

    // Cập nhật các phần liên quan
    this.updatePointsUsage();
    this.updateOrderSummary();
  }

  updatePointsUsage() {
    if (!this.customerInfo) {
      $("#usePoints").prop("disabled", true);
      return;
    }

    const usePointsInput = $("#usePoints");
    const maxPoints = this.customerInfo.points;

    // Kích hoạt/vô hiệu hóa input điểm
    usePointsInput.prop("disabled", maxPoints === 0);

    // Cập nhật max points
    usePointsInput.attr("max", maxPoints);
    usePointsInput.attr("placeholder", `Tối đa ${maxPoints} điểm`);

    // Xử lý khi thay đổi điểm
    usePointsInput.on("input", () => {
      const usedPoints = parseInt(usePointsInput.val() || 0);

      // Kiểm tra điểm không vượt quá số điểm hiện có
      if (usedPoints > maxPoints) {
        usePointsInput.val(maxPoints);
      }

      // Kiểm tra điểm có hợp lệ không
      if (isNaN(usedPoints) || usedPoints < 0) {
        usePointsInput.val(0);
      }

      // Cập nhật tổng giá
      this.updateOrderSummary();
    });
  }

  updateOrderSummary() {
    const orderSummary = $("#orderSummary");
    const selectedSeatsElement = $("#selectedSeats");
    const usePointsInput = $("#usePoints");

    // Kiểm tra xem đã chọn ghế chưa
    if (this.selectedSeats.length === 0) {
      orderSummary.html('<p class="text-muted">Chưa có ghế nào được chọn</p>');
      return;
    }

    // Tính tổng giá vé
    const seatPrices = this.selectedSeats.map((seat) => seat.price);
    const totalPrice = seatPrices.reduce((a, b) => a + b, 0);

    // Lấy số điểm sử dụng
    const usedPoints = Math.min(
      parseInt(usePointsInput.val() || 0),
      this.customerInfo ? this.customerInfo.points : 0
    );

    // Tính giảm giá từ điểm
    const pointDiscount = usedPoints * 1000;
    let finalPrice = Math.max(0, totalPrice - pointDiscount);

    // Áp dụng khuyến mãi nếu có
    let promotionDiscount = 0;
    let discountPercent = 0;
    if (this.selectedPromotion && this.selectedPromotion.discountPercent > 0) {
      discountPercent = this.selectedPromotion.discountPercent;
      promotionDiscount = Math.round((finalPrice * discountPercent) / 100);
      finalPrice = Math.max(0, finalPrice - promotionDiscount);
    }

    // Hiển thị chi tiết đơn hàng
    let html = `<div class="order-summary-details">
            <div class="summary-item">
                <span>Tổng giá vé</span>
                <span>${totalPrice.toLocaleString()} VNĐ</span>
            </div>
            <div class="summary-item">
                <span>Điểm sử dụng</span>
                <span>${usedPoints} điểm (-${pointDiscount.toLocaleString()} VNĐ)</span>
            </div>`;
    if (promotionDiscount > 0) {
      html += `<div class="summary-item">
                <span>Khuyến mãi (${discountPercent}%)</span>
                <span>- ${promotionDiscount.toLocaleString()} VNĐ</span>
            </div>`;
    }
    html += `<div class="summary-item total">
                <span>Tổng thanh toán</span>
                <span>${finalPrice.toLocaleString()} VNĐ</span>
            </div>
        </div>`;
    orderSummary.html(html);
  }

  //Validate và chuyển sang bước tiếp theo (1→2→3)
  nextStep() {
    if (this.currentStep < 3) {
      // Nếu đang ở bước 1 và đã chọn ghế, tự động chuyển sang bước 2
      if (this.currentStep === 1 && this.selectedSeats.length > 0) {
        this.currentStep = 2;
        this.updateStepDisplay();
        return;
      }

      if (!this.validateStep(this.currentStep)) {
        return;
      }

      this.currentStep++;
      this.updateStepDisplay();

      if (this.currentStep === 2) {
        this.loadSeats(this.selectedShowtime.id);
      } else if (this.currentStep === 3) {
        this.updateOrderSummary();
      }
    }
  }

  prevStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
      this.updateStepDisplay();
    }
  }

  updateStepDisplay() {
    console.log("updateStepDisplay được gọi, currentStep:", this.currentStep);
    console.log("selectedSeats.length:", this.selectedSeats.length);
    console.log("selectedSeats:", this.selectedSeats);

    $(".step").removeClass("active completed");

    // Step 1: Chuyển sang màu xanh khi đã chọn phim VÀ suất chiếu
    if (this.selectedMovie && this.selectedShowtime) {
      $(`.step[data-step="1"]`).addClass("completed");
    } else if (this.currentStep === 1) {
      $(`.step[data-step="1"]`).addClass("active");
    }

    // Step 2: Chỉ chuyển sang màu xanh khi đã chọn ít nhất một ghế
    // Không có else if để tránh chuyển xanh khi chưa chọn ghế
    if (this.selectedSeats.length > 0) {
      console.log("Thêm class completed cho Step 2 vì đã chọn ghế");
      $(`.step[data-step="2"]`).addClass("completed");
    } else {
      console.log("Step 2 không có class nào vì chưa chọn ghế");
    }

    // Step 3: Chuyển sang màu xanh lá khi đã hoàn thành bước 2 và chuyển sang bước 3
    if (this.currentStep === 3) {
      console.log("Thêm class completed cho Step 3 vì đã chuyển sang bước 3");
      $(`.step[data-step="3"]`).addClass("completed");
    }

    $(".step-panel").removeClass("active");
    $(`#step${this.currentStep}`).addClass("active");

    $("#prevBtn").toggle(this.currentStep > 1);
    $("#nextBtn").toggle(this.currentStep < 3);
    $("#confirmBtn").toggle(this.currentStep === 3);

    console.log("Đã cập nhật step display, bước hiện tại:", this.currentStep);
  }

  validateStep(step) {
    switch (step) {
      case 1:
        // Nếu đã chọn ghế, cho phép chuyển sang bước 2
        if (this.selectedSeats.length > 0) {
          return true;
        }
        // Nếu chưa chọn ghế, kiểm tra phim và showtime
        if (!this.selectedMovie || !this.selectedShowtime) {
          this.showError("Vui lòng chọn phim và suất chiếu");
          return false;
        }
        break;
      case 2:
        if (this.selectedSeats.length === 0) {
          this.showError("Vui lòng chọn ít nhất một ghế");
          return false;
        }
        break;
    }
    return true;
  }

  async confirmBooking() {
    const paymentMethod = $('input[name="paymentMethod"]:checked').val();
    const usedPoints = parseInt($("#usePoints").val()) || 0;

    if (!this.validateStep(4)) {
      this.showError("Vui lòng kiểm tra lại thông tin đặt vé");
      return;
    }

    try {
      this.showLoading();

      const seatIdsParam = this.selectedSeats.map((seat) => seat.id).join(",");
      const detailResponse = await fetch(
        `/BookTicket/GetBookingConfirmationDetail?showTimeId=${this.selectedShowtime.id}&seatIds=${seatIdsParam}&memberId=${this.customerInfo.id}`,
        {
          method: "GET",
          headers: {
            Accept: "application/json",
          },
        }
      );

      const detailResult = await detailResponse.json();

      if (!detailResult.success) {
        this.hideLoading();
        this.showError(
          detailResult.message || "Không thể tải thông tin đặt vé"
        );
        return;
      }

      this.showBookingConfirmationModal(detailResult.data, usedPoints);
    } catch (error) {
      this.hideLoading();
      this.showError("Có lỗi xảy ra khi xác nhận đặt vé. Vui lòng thử lại.");
    }
  }

  showBookingConfirmationModal(bookingDetail, requestedPoints) {
    // Tính lại tổng tiền đã trừ điểm và khuyến mãi ở FE
    const seatPrices = this.selectedSeats.map((seat) => seat.price);
    const totalPrice = seatPrices.reduce((a, b) => a + b, 0);
    const usedPoints = requestedPoints || 0;
    const pointDiscount = usedPoints * 1000;
    let finalPrice = Math.max(0, totalPrice - pointDiscount);
    let promotionDiscount = 0;
    let discountPercent = 0;
    if (this.selectedPromotion && this.selectedPromotion.discountPercent > 0) {
      discountPercent = this.selectedPromotion.discountPercent;
      promotionDiscount = Math.round((finalPrice * discountPercent) / 100);
      finalPrice = Math.max(0, finalPrice - promotionDiscount);
    }
    const modalHtml = `
            <div class="modal fade" id="bookingConfirmModal" tabindex="-1" role="dialog">
                <div class="modal-dialog modal-lg" role="document">
                    <div class="modal-content">
                                    <div class="modal-header">
                <h5 class="modal-title">Xác Nhận Đặt Vé</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
                        <div class="modal-body">
                            
                            <div class="row mb-3">
                                <div class="col-md-6">
                                    <h6>Thông Tin Đặt Vé</h6>
                                    <p><strong>Phim:</strong> ${
                                      bookingDetail.movieName
                                    }</p>
                                    <p><strong>Phòng chiếu:</strong> ${
                                      bookingDetail.screen
                                    }</p>
                                    <p><strong>Ngày:</strong> ${
                                      bookingDetail.date
                                    }</p>
                                    <p><strong>Giờ:</strong> ${
                                      bookingDetail.time
                                    }</p>
                                    <p><strong>Ghế:</strong> ${
                                      bookingDetail.seat
                                    }</p>
                                    <p><strong>Tổng giá vé:</strong> ${this.formatCurrency(
                                      totalPrice
                                    )}</p>
                                    <p><strong>Điểm sử dụng:</strong> ${usedPoints} điểm (-${this.formatCurrency(
      pointDiscount
    )})</p>
                                    ${
                                      promotionDiscount > 0
                                        ? `<p><strong>Khuyến mãi (${discountPercent}%):</strong> -${this.formatCurrency(
                                            promotionDiscount
                                          )}</p>`
                                        : ""
                                    }
                                    <p><strong>Tổng thanh toán:</strong> <span style="color:red;font-weight:bold">${this.formatCurrency(
                                      finalPrice
                                    )}</span></p>
                                </div>
                                <div class="col-md-6">
                                    <h6>Thông Tin Khách Hàng</h6>
                                    <p><strong>Mã thành viên:</strong> ${
                                      bookingDetail.memberId
                                    }</p>
                                    <p><strong>Họ tên:</strong> ${
                                      bookingDetail.fullName
                                    }</p>
                                    <p><strong>Điểm tích lũy:</strong> ${
                                      bookingDetail.memberScore
                                    }</p>
                                    <p><strong>CCCD/CMND:</strong> ${
                                      bookingDetail.identityCard
                                    }</p>
                                    <p><strong>Số điện thoại:</strong> ${
                                      bookingDetail.phoneNumber
                                    }</p>
                                </div>
                            </div>

                            
                            ${
                              bookingDetail.canConvertScore
                                ? `
                                <div class="card mb-3">
                                    <div class="card-header">
                                        <h6>Tùy Chọn Chuyển Đổi Điểm</h6>
                                    </div>
                                    <div class="card-body">
                                        <div class="form-check">
                                            <input class="form-check-input" type="radio" name="scoreConversion" id="noConversion" value="false" checked>
                                            <label class="form-check-label" for="noConversion">
                                                Không sử dụng điểm
                                            </label>
                                        </div>
                                        <div class="form-check">
                                            <input class="form-check-input" type="radio" name="scoreConversion" id="useConversion" value="true">
                                            <label class="form-check-label" for="useConversion">
                                                Chuyển đổi điểm thành vé (${bookingDetail.scorePerTicket} điểm = 1 vé)
                                            </label>
                                        </div>
                                        <div id="conversionOptions" style="display: none; margin-top: 15px;">
                                            <label for="ticketsToConvert">Số vé muốn chuyển đổi:</label>
                                            <input type="number" id="ticketsToConvert" class="form-control" min="0" max="${bookingDetail.maxTicketsFromScore}" value="0">
                                            <small class="text-muted">Tối đa: ${bookingDetail.maxTicketsFromScore} vé</small>
                                            <div id="conversionSummary" class="mt-2"></div>
                                        </div>
                                    </div>
                                </div>
                            `
                                : '<div class="alert alert-info">Không đủ điểm để chuyển đổi vé</div>'
                            }

                            
                            <div class="form-group">
                                <label>Phương thức thanh toán:</label>
                                <div class="form-check">
                                    <input class="form-check-input" type="radio" name="modalPaymentMethod" id="modalCash" value="cash" checked>
                                    <label class="form-check-label" for="modalCash">Tiền mặt</label>
                                </div>
                                <div class="form-check">
                                    <input class="form-check-input" type="radio" name="modalPaymentMethod" id="modalVnpay" value="vnpay">
                                    <label class="form-check-label" for="modalVnpay">VNPay</label>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                            <button type="button" class="btn btn-primary" id="finalConfirmBtn">Xác Nhận Đặt Vé</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

    $("#bookingConfirmModal").remove();
    $("body").append(modalHtml);

    this.bindScoreConversionEvents(bookingDetail);

    const modal = new bootstrap.Modal(
      document.getElementById("bookingConfirmModal")
    );
    modal.show();
    this.hideLoading();
  }

  bindScoreConversionEvents(bookingDetail) {
    $('input[name="scoreConversion"]').change(function () {
      const useConversion = $(this).val() === "true";
      $("#conversionOptions").toggle(useConversion);
      if (!useConversion) {
        $("#ticketsToConvert").val(0);
        $("#conversionSummary").html("");
      }
    });

    $("#ticketsToConvert").on("input", function () {
      const tickets = parseInt($(this).val()) || 0;
      const maxTickets = bookingDetail.maxTicketsFromScore;

      if (tickets > maxTickets) {
        $(this).val(maxTickets);
        tickets = maxTickets;
      }

      if (tickets > 0) {
        const pointsNeeded = tickets * bookingDetail.scorePerTicket;
        const remainingPoints = bookingDetail.memberScore - pointsNeeded;

        if (remainingPoints < 0) {
          $("#conversionSummary").html(
            '<div class="text-danger">Not enough score to convert into ticket</div>'
          );
          $("#finalConfirmBtn").prop("disabled", true);
        } else {
          $("#conversionSummary").html(`
                        <div class="text-success">
                            <p>Sẽ sử dụng: ${pointsNeeded} điểm</p>
                            <p>Điểm còn lại: ${remainingPoints}</p>
                        </div>
                    `);
          $("#finalConfirmBtn").prop("disabled", false);
        }
      } else {
        $("#conversionSummary").html("");
        $("#finalConfirmBtn").prop("disabled", false);
      }
    });

    $("#finalConfirmBtn")
      .off("click")
      .on("click", () => {
        this.executeFinalBooking(bookingDetail);
      });
  }

  async executeFinalBooking(bookingDetail) {
    try {
      this.showLoading();

      const useScoreConversion =
        $('input[name="scoreConversion"]:checked').val() === "true";
      const ticketsToConvert = useScoreConversion
        ? parseInt($("#ticketsToConvert").val()) || 0
        : 0;
      const paymentMethod = $('input[name="modalPaymentMethod"]:checked').val();

      const bookingData = {
        ShowTimeId: this.selectedShowtime.id,
        SeatIds: this.selectedSeats.map((seat) => seat.id),
        MemberId: this.customerInfo.id.toString(),
        UseScoreConversion: useScoreConversion,
        TicketsToConvert: ticketsToConvert,
        PaymentMethod: paymentMethod,
        StaffId: "admin", // Could be from session
        Notes: "",
        // Gửi kèm promotionId nếu có chọn
        PromotionId: this.selectedPromotion ? this.selectedPromotion.id : "",
      };

      console.log("bookingData id khuyen mai", bookingData);
      const response = await fetch(
        "/BookingManagement/BookingTicket/ConfirmBookingWithScore",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Accept: "application/json",
          },
          body: JSON.stringify(bookingData),
        }
      );

      const result = await response.json();
      this.hideLoading();
      console.log("result", result);

      if (result.success && result.data) {
        const confirmModal = bootstrap.Modal.getInstance(
          document.getElementById("bookingConfirmModal")
        );
        confirmModal.hide();

        if (
          result.data.paymentMethod &&
          result.data.paymentMethod.toLowerCase() === "vnpay"
        ) {
          let bookingIdToPay = result.data.bookingId || result.data.id || null;
          if (!bookingIdToPay && result.data.bookingCode) {
            const bookingCode = result.data.bookingCode;
            try {
              const idResp = await fetch(
                `/BookingManagement/BookingTicket/GetBookingIdByCode?bookingCode=${bookingCode}`
              );
              if (idResp.ok) {
                const idJson = await idResp.json();
                bookingIdToPay = idJson.bookingId;
              }
            } catch {}
          }

          if (!bookingIdToPay) {
            this.showError("Không lấy được BookingId");
            return;
          }
          // Gọi API tạo thanh toán VNPay
          try {
            const createResp = await fetch(
              "/BookingManagement/BookingTicket/CreateVnpayPayment",
              {
                method: "POST",
                headers: {
                  "Content-Type": "application/json",
                  Accept: "application/json",
                },
                body: JSON.stringify({
                  bookingId: bookingIdToPay,
                  amount: result.data.total,
                  decription: "Thanh toan VNPay",
                }),
              }
            );

            const createData = await createResp.json();
            console.log("createData:", createData);

            if (createData.success && createData.paymentUrl) {
              window.location.href = createData.paymentUrl;
              return;
            } else {
              this.showError(
                createData.message || "Không thể khởi tạo thanh toán VNPay"
              );
            }
          } catch (payErr) {
            this.showError("Có lỗi khi tạo thanh toán VNPay");
          }
        }

        // Nếu không phải VNPay, hiển thị modal thành công như cũ
        this.displayBookingSuccess(result.data);

        this.resetBookingForm();
        this.currentStep = 1;
        this.updateStepDisplay();
      } else {
        this.showError(result.message || "Đặt vé không thành công");
      }
    } catch (error) {
      this.hideLoading();
      this.showError("Có lỗi xảy ra khi đặt vé. Vui lòng thử lại.");
    }
  }

  displayBookingSuccess(bookingData) {
    const successModal = `
            <div class="modal fade" id="bookingSuccessModal" tabindex="-1" role="dialog">
                <div class="modal-dialog modal-lg" role="document">
                    <div class="modal-content">
                        <div class="modal-header bg-success text-white">
                            <h5 class="modal-title">Đặt Vé Thành Công!</h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <div class="text-center mb-3">
                                <h4 class="text-success">${
                                  bookingData.message
                                }</h4>
                                <h5>Mã đặt vé: <strong>${
                                  bookingData.bookingCode
                                }</strong></h5>
                            </div>
                            
                            <div class="row">
                                <div class="col-md-6">
                                    <h6>Chi Tiết Đặt Vé</h6>
                                    <p><strong>Phim:</strong> ${
                                      bookingData.movieTitle
                                    }</p>
                                    <p><strong>Phòng:</strong> ${
                                      bookingData.cinemaRoom
                                    }</p>
                                    <p><strong>Ngày:</strong> ${
                                      bookingData.showDate
                                    }</p>
                                    <p><strong>Giờ:</strong> ${
                                      bookingData.showTime
                                    }</p>
                                    <p><strong>Ghế:</strong> ${bookingData.seats
                                      .map((s) => s.seatCode)
                                      .join(", ")}</p>
                                </div>
                                <div class="col-md-6">
                                    <h6>Thanh Toán</h6>
                                    <p><strong>Tạm tính:</strong> ${this.formatCurrency(
                                      bookingData.subTotal
                                    )}</p>
                                    ${
                                      bookingData.scoreUsed
                                        ? `
                                        <p><strong>Giảm giá (${
                                          bookingData.ticketsConvertedFromScore
                                        } vé):</strong> -${this.formatCurrency(
                                            bookingData.scoreDiscount
                                          )}</p>
                                        <p><strong>Điểm đã sử dụng:</strong> ${
                                          bookingData.scoreDeducted
                                        }</p>
                                        <p><strong>Điểm còn lại:</strong> ${
                                          bookingData.remainingScore
                                        }</p>
                                    `
                                        : ""
                                    }
                                    <p><strong>Tổng cộng:</strong> ${this.formatCurrency(
                                      bookingData.total
                                    )}</p>
                                    <p><strong>Thanh toán:</strong> ${this.getPaymentMethodText(
                                      bookingData.paymentMethod
                                    )}</p>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-primary" id="closeSuccessModal" data-bs-dismiss="modal">Đóng</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

    $("#bookingSuccessModal").remove();

    $("body").append(successModal);

    const successModalInstance = new bootstrap.Modal(
      document.getElementById("bookingSuccessModal")
    );
    successModalInstance.show();

    $("#bookingSuccessModal").on(
      "click",
      '[data-bs-dismiss="modal"], #closeSuccessModal',
      function () {
        successModalInstance.hide();
      }
    );

    $("#bookingSuccessModal").on("click", function (e) {
      if (e.target === this) {
        successModalInstance.hide();
      }
    });

    $(document).on("keydown.successModal", function (e) {
      if (e.keyCode === 27) {
        // Escape key
        successModalInstance.hide();
        $(document).off("keydown.successModal");
      }
    });

    $("#bookingSuccessModal").on("hidden.bs.modal", function () {
      $(this).remove();
      $(document).off("keydown.successModal");
    });
  }

  resetBookingForm() {
    $("#movieSelect").val("");
    $("#selectedMovieInfo").hide();
    this.selectedMovie = null;

    $("#showtimeSelection").html(
      '<p class="text-muted">Vui lòng chọn phim trước</p>'
    );
    this.selectedShowtime = null;

    this.selectedSeats = [];
    $("#seatMap").empty();
    this.updateSelectedSeats();

    $("#customerPhone").val("");
    $("#customerInfo").hide();
    this.customerInfo = null;
    this.clearSearchMessage();

    $("#usePoints").val(0).prop("disabled", true);

    $('input[name="paymentMethod"][value="cash"]').prop("checked", true);

    this.updateOrderSummary();

    $(".alert").hide();

    // Reset về bước 1 và cập nhật thanh tiến trình
    this.currentStep = 1;
    this.updateStepDisplay();
  }

  searchMovies(searchTerm) {
    const movieCards = $(".movie-card");

    if (!searchTerm) {
      movieCards.show();
      return;
    }

    movieCards.each(function () {
      const title = $(this).find(".movie-title").text().toLowerCase();
      const genre = $(this).find(".movie-genre").text().toLowerCase();

      if (
        title.includes(searchTerm.toLowerCase()) ||
        genre.includes(searchTerm.toLowerCase())
      ) {
        $(this).show();
      } else {
        $(this).hide();
      }
    });
  }

  formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString("vi-VN", {
      weekday: "long",
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  }

  formatTime(timeString) {
    const time = new Date(timeString);
    return time.toLocaleTimeString("vi-VN", {
      hour: "2-digit",
      minute: "2-digit",
    });
  }

  formatCurrency(amount) {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(amount);
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

  // Thêm phương thức mới để định dạng giờ
  formatTimeFromString(timeString) {
    // Giả sử timeString ở định dạng "HH:mm"
    return timeString;
  }

  // Phương thức mới để format ngày đầy đủ
  formatFullDate(dateString) {
    // dateString có dạng 'YYYY-MM-DD'
    const [year, month, day] = dateString.split("-");
    const daysOfWeek = [
      "Chủ Nhật",
      "Thứ Hai",
      "Thứ Ba",
      "Thứ Tư",
      "Thứ Năm",
      "Thứ Sáu",
      "Thứ Bảy",
    ];

    // Tạo đối tượng Date để lấy thứ
    const date = new Date(dateString);
    const dayOfWeek = daysOfWeek[date.getDay()];

    return `${dayOfWeek}, ${day}/${month}/${year}`;
  }

  // Modal tạo khách hàng mới
  showCreateCustomerModal() {
    $("#createCustomerForm")[0].reset();

    const searchTerm = $("#customerPhone").val().trim();
    if (searchTerm) {
      $("#newCustomerPhone").val(searchTerm);
    }

    const modal = new bootstrap.Modal(
      document.getElementById("createCustomerModal")
    );
    modal.show();
  }

  async createCustomer() {
    try {
      const formData = {
        fullName: $("#newCustomerFullName").val(),
        email: $("#newCustomerEmail").val(),
        phoneNumber: $("#newCustomerPhone").val(),
        identityCard: $("#newCustomerIdentity").val(),
      };

      const response = await fetch("/BookTicket/CreateCustomer", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(formData),
      });

      const result = await response.json();

      if (result.success) {
        const modal = bootstrap.Modal.getInstance(
          document.getElementById("createCustomerModal")
        );
        modal.hide();
        this.showSuccess("Tạo khách hàng thành công");

        $("#customerPhone").val(formData.phoneNumber);
        await this.searchCustomer();
      } else {
        alert("Lỗi: " + (result.message || "Không thể tạo khách hàng"));
      }
    } catch (error) {
      alert("Có lỗi xảy ra khi tạo khách hàng");
    }
  }

  // Helper method để convert payment method
  getPaymentMethodText(paymentMethod) {
    switch (paymentMethod?.toLowerCase()) {
      case "cash":
        return "Tiền mặt";
      case "vnpay":
        return "VNPay";
      default:
        return "Tiền mặt";
    }
  }

  async loadPromotions() {
    try {
      this.showLoading();
      const response = await fetch(
        "/BookingManagement/BookingTicket/GetPromotions",
        {
          method: "GET",
          headers: {
            Accept: "application/json",
          },
        }
      );
      const data = await response.json();
      console.log("dataaa", data);

      if (data.success && Array.isArray(data.data)) {
        // Lọc khuyến mãi đang diễn ra
        const now = new Date();
        const validPromotions = data.data.filter((promo) => {
          if (!promo.startDate || !promo.endDate) return false;
          const start = new Date(promo.startDate);
          const end = new Date(promo.endDate);
          // Ngày hiện tại nằm trong khoảng [start, end]
          return now >= start && now <= end;
        });
        this.promotions = validPromotions;
        this.displayPromotions(validPromotions);
      } else {
        this.promotions = [];
        this.displayPromotions([]);
      }
    } catch (error) {
      this.promotions = [];
      this.displayPromotions([]);
    } finally {
      this.hideLoading();
    }
  }

  displayPromotions(promotions) {
    const promotionSelect = $("#promotionSelect");
    if (!promotionSelect.length) return;
    // Remove all except the first option ("Không áp dụng")
    promotionSelect.find("option:not(:first)").remove();
    if (!promotions || promotions.length === 0) {
      promotionSelect.append("<option disabled>Không có khuyến mãi</option>");
      return;
    }
    promotions.forEach((promo) => {
      // Use promo.id and promo.name (or .title) for display
      const name = promo.name || promo.title || `Khuyến mãi #${promo.id}`;
      const option = $(`<option></option>`) // jQuery for safety
        .val(promo.id)
        .text(name)
        .data("promotion", promo);
      promotionSelect.append(option);
    });
  }
}

$(document).ready(function () {
  const dashboard = new BookTicketDashboard();

  // Thêm event listener cho nút in vé
  $(".print-ticket").on("click", function () {
    const modalContent = $("#bookingConfirmationModal .modal-body").clone();

    // Tạo cửa sổ in
    const printWindow = window.open("", "_blank", "width=600,height=800");

    // Tạo nội dung in
    printWindow.document.write(`
            <html>
                <head>
                    <title>Vé Xem Phim</title>
                    <style>
                        body { 
                            font-family: Arial, sans-serif; 
                            max-width: 500px; 
                            margin: 0 auto; 
                            padding: 20px; 
                            text-align: center;
                        }
                        .ticket-header {
                            background-color: #007bff;
                            color: white;
                            padding: 10px;
                            margin-bottom: 20px;
                        }
                        .ticket-details {
                            background-color: #f8f9fa;
                            border: 1px solid #e9ecef;
                            border-radius: 8px;
                            padding: 20px;
                        }
                        .ticket-details .detail-item {
                            display: flex;
                            justify-content: space-between;
                            margin-bottom: 10px;
                            padding-bottom: 10px;
                            border-bottom: 1px solid #e9ecef;
                        }
                        .ticket-details .detail-item:last-child {
                            border-bottom: none;
                        }
                        .ticket-details strong {
                            color: #6c757d;
                        }
                        .ticket-details .total {
                            font-weight: bold;
                            color: #007bff;
                        }
                        .qr-code {
                            margin-top: 20px;
                        }
                    </style>
                </head>
                <body>
                    <div class="ticket-header">
                        <h2>VÉ XEM PHIM</h2>
                    </div>
                    <div class="ticket-details">
                        ${modalContent.html()}
                    </div>
                    <div class="qr-code">
                        <img src="/images/cinema-logo.png" alt="Cinema Logo" style="max-width: 100px;">
                    </div>
                </body>
            </html>
        `);

    // In vé
    printWindow.document.close();
    printWindow.print();
  });
});
