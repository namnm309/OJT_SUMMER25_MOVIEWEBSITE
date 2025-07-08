// Book Ticket Dashboard JavaScript
class BookTicketDashboard {
    constructor() {
        this.currentStep = 1;
        this.selectedMovie = null;
        this.selectedShowtime = null;
        this.selectedSeats = [];
        this.customerInfo = null;
        this.totalPrice = 0;
        this.movies = [];
        
        this.init();
    }

    // tạo các method sử dụng  
    init() {
        this.bindEvents();
        this.loadMovies();
    }

    // gán click, keypress, change events cho các nút và input
    bindEvents() {
        // Navigation buttons
        $('#nextBtn').on('click', () => this.nextStep());
        $('#prevBtn').on('click', () => this.prevStep());
        $('#confirmBtn').on('click', () => this.confirmBooking());

        // Movie search
        $('#movieSearch').on('input', (e) => this.searchMovies(e.target.value));

        // Customer search
        const searchButton = $('#searchCustomer');
        const phoneInput = $('#customerPhone');

        if (searchButton.length > 0) {
            searchButton.on('click', () => {
                this.searchCustomer();
            });
        }

        if (phoneInput.length > 0) {
            phoneInput.on('keypress', (e) => {
                if (e.which === 13) {
                    this.searchCustomer();
                }
            });
        }

        // Create customer modal events
        $('#createCustomer').on('click', () => this.showCreateCustomerModal());
        $('#saveCustomer').on('click', () => this.createCustomer());

        // Payment method change
        $('input[name="paymentMethod"]').on('change', () => this.updateOrderSummary());
    }

    // api /BookingManagement/BookingTicket/GetMovies
    async loadMovies() {
        try {
            this.showLoading();
            const response = await fetch('/BookingManagement/BookingTicket/GetMovies');
            const data = await response.json();
            
            if (data.success) {
                this.movies = data.data; // Store data for later use
                this.displayMovies(data.data);
            } else {
                this.showError('Không thể tải danh sách phim');
            }
        } catch (error) {
            this.showError('Có lỗi xảy ra khi tải danh sách phim');
        } finally {
            this.hideLoading();
        }
    }

    //dropdown moivie để chọn 
    displayMovies(movies) {
        const movieSelect = $('#movieSelect');
        const selectedMovieInfo = $('#selectedMovieInfo');
        
        // Clear existing options except the first one
        movieSelect.find('option:not(:first)').remove();
        selectedMovieInfo.hide();

        if (!movies || movies.length === 0) {
            movieSelect.append('<option disabled>Hiện tại không có phim đang chiếu</option>');
            return;
        }

        // Populate dropdown with movies
        movies.forEach(movie => {
            const option = $(`<option value="${movie.id}">${movie.title}</option>`);
            option.data('movie', movie);
            movieSelect.append(option);
        });

        // Handle movie selection
        movieSelect.off('change').on('change', (e) => {
            const selectedOption = $(e.target).find('option:selected');
            const movie = selectedOption.data('movie');
            
            if (movie) {
                this.selectMovie(movie);
                this.displaySelectedMovieInfo(movie);
            } else {
                selectedMovieInfo.hide();
                $('#showtimeSelection').html('<p class="text-muted">Vui lòng chọn phim trước</p>');
            }
        });
    }

    // chọn dropdown xong thì show poster, tên, thể loại, thời lượng phim
    displaySelectedMovieInfo(movie) {
        const selectedMovieInfo = $('#selectedMovieInfo');
        selectedMovieInfo.html(`
            <div class="selected-movie-card">
                <div class="row">
                    <div class="col-md-4">
                        <img src="${movie.primaryImageUrl || '/images/default-movie.jpg'}" alt="${movie.title}" class="img-fluid rounded">
                    </div>
                    <div class="col-md-8">
                        <h6 class="movie-title">${movie.title}</h6>
                        <p class="movie-genre text-muted">${movie.genre || 'Chưa phân loại'}</p>
                        <p class="movie-duration text-muted">${movie.duration || 0} phút</p>
                    </div>
                </div>
            </div>
        `);
        selectedMovieInfo.show();
    }

    // lưu phim đã chọn và tải lịch chiếu
    async selectMovie(movie) {
        this.selectedMovie = movie;
        
        // Load showtimes
        await this.loadShowtimes(movie.id);
    }

    //lấy ngày chiếu và giờ chiếu của phim 
    // api /BookingManagement/BookingTicket/GetShowDates?movieId=${movieId}` 
    // api /BookingManagement/BookingTicket/GetShowTimes?movieId=${movieId}&showDate=${encodeURIComponent(dateItem.code)}`
    async loadShowtimes(movieId) {
        try {
            this.showLoading();
            // First get available dates for the movie
            const datesResponse = await fetch(`/BookingManagement/BookingTicket/GetShowDates?movieId=${movieId}`);
            const datesData = await datesResponse.json();
            
            if (datesData.success && datesData.data && datesData.data.length > 0) {
                // For each date, get the showtimes
                const showDatesWithTimes = [];
                
                for (const dateItem of datesData.data) {
                    try {
                        const timesResponse = await fetch(`/BookingManagement/BookingTicket/GetShowTimes?movieId=${movieId}&showDate=${encodeURIComponent(dateItem.code)}`);
                        const timesData = await timesResponse.json();

                        if (timesData.success) {
                            const showtimes = Array.isArray(timesData.data) 
                                ? timesData.data 
                                : (timesData.data?.showtimes || []);
                            
                            if (showtimes.length > 0) {
                                showDatesWithTimes.push({
                                    date: dateItem.text,
                                    dateCode: dateItem.code,
                                    showtimes: showtimes.map(time => ({
                                        id: time.id,
                                        startTime: time.time || time.startTime
                                    }))
                                });
                            } else {
                                showDatesWithTimes.push({
                                    date: dateItem.text,
                                    dateCode: dateItem.code,
                                    showtimes: []
                                });
                            }
                        } else {
                            showDatesWithTimes.push({
                                date: dateItem.text,
                                dateCode: dateItem.code,
                                showtimes: []
                            });
                        }
                    } catch (timeError) {
                        showDatesWithTimes.push({
                            date: dateItem.text,
                            dateCode: dateItem.code,
                            showtimes: []
                        });
                    }
                }
                
                this.displayShowtimes(showDatesWithTimes);
            } else {
                this.displayShowtimes([]);
            }
        } catch (error) {
            this.displayShowtimes([]);
            this.showError('Không thể tải lịch chiếu. Vui lòng thử lại.');
        } finally {
            this.hideLoading();
        }
    }

    // tạo các nút giờ chiếu theo từng ngày
    displayShowtimes(showDates) {
        const showtimeSelection = $('#showtimeSelection');
        if (!showtimeSelection.length) {
            return;
        }
        showtimeSelection.empty();

        if (!showDates || showDates.length === 0) {
            showtimeSelection.html('<p class="text-muted">Không có lịch chiếu</p>');
            return;
        }

        showDates.forEach(dateGroup => {
            const fullDate = this.formatFullDate(dateGroup.dateCode);
            
            const dateSection = $(`
                <div class="showtime-date">
                    <h6>${fullDate}</h6>
                    <div class="showtime-list"></div>
                </div>
            `);

            const showtimeList = dateSection.find('.showtime-list');
            
            if (dateGroup.showtimes && Array.isArray(dateGroup.showtimes) && dateGroup.showtimes.length > 0) {
                dateGroup.showtimes.forEach(showtime => {
                    const showtimeBtn = $(`
                        <button class="btn btn-outline-primary btn-sm showtime-btn" data-showtime-id="${showtime.id}">
                            ${showtime.startTime}
                        </button>
                    `);

                    showtimeBtn.on('click', () => this.selectShowtime({
                        id: showtime.id,
                        startTime: showtime.startTime
                    }));
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
        
        // Update UI
        $('.showtime-btn').removeClass('active');
        $(`.showtime-btn[data-showtime-id="${showtime.id}"]`).addClass('active');
        
        // Auto advance to step 2 and load seats
        setTimeout(() => {
            this.currentStep = 2;
            this.updateStepDisplay();
            this.loadSeats(showtime.id);
        }, 500); // Small delay for better UX
    }

    // lấy danh sách ghế của suất chiếu và show sơ đồ ghế
    // api /BookingManagement/BookingTicket/GetSeats?showTimeId=${showtimeId}`
    async loadSeats(showtimeId) {
        try {
            const url = `/BookingManagement/BookingTicket/GetSeats?showTimeId=${showtimeId}`;
            
            const response = await fetch(url);
            
            const data = await response.json();
            
            if (data.success) {
                this.displaySeats(data.data);
            } else {
                this.showError('Không thể tải sơ đồ ghế: ' + data.message);
            }
        } catch (error) {
            this.showError('Có lỗi xảy ra khi tải sơ đồ ghế');
        }
    }

    // show sơ đồ ghế theo hàng, hiển thị trạng thái ghế , khó siu cấp
    displaySeats(seats) {
        const seatMap = $('#seatMap');
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

        // Group seats by row (extract row from seatCode)
        const seatRows = {};
        seatData.forEach((seat, index) => {
            // Extract row letter from seatCode (e.g., "A1" -> "A")
            const rowName = seat.seatCode ? seat.seatCode.charAt(0) : 'A';
            if (!seatRows[rowName]) {
                seatRows[rowName] = [];
            }
            
            // Transform backend data to frontend format
            const transformedSeat = {
                id: seat.id,
                rowName: rowName,
                seatNumber: seat.seatCode ? seat.seatCode.slice(1) : '1', // Extract number part
                seatCode: seat.seatCode,
                seatType: seat.seatType,
                isBooked: !seat.isAvailable, // Backend: isAvailable, Frontend: isBooked
                price: seat.price || 0
            };
            
            seatRows[rowName].push(transformedSeat);
        });

        // Display seats
        Object.keys(seatRows).sort().forEach(rowName => {
            const row = $(`<div class="seat-row"><span class="row-label">${rowName}</span></div>`);
            
            seatRows[rowName].sort((a, b) => parseInt(a.seatNumber) - parseInt(b.seatNumber)).forEach(seat => {
                const seatElement = $(`
                    <div class="seat ${seat.isBooked ? 'occupied' : 'available'} ${seat.seatType === 'VIP' ? 'vip' : ''}" 
                         data-seat-id="${seat.id}" 
                         data-seat-name="${seat.seatCode}"
                         data-seat-price="${seat.price}">
                        ${seat.seatNumber}
                    </div>
                `);

                if (!seat.isBooked) {
                    seatElement.on('click', () => this.toggleSeat(seat, seatElement));
                }

                row.append(seatElement);
            });

            seatMap.append(row);
        });
    }
 
    // thêm/xóa ghế khỏi danh sách đã chọn
    toggleSeat(seat, seatElement) {
        const seatId = seat.id;
        const seatIndex = this.selectedSeats.findIndex(s => s.id === seatId);

        if (seatIndex > -1) {
            // Deselect seat
            this.selectedSeats.splice(seatIndex, 1);
            seatElement.removeClass('selected');
        } else {
            // Select seat
            this.selectedSeats.push(seat);
            seatElement.addClass('selected');
        }

        this.updateSelectedSeats();
    }
    
    // hiển thị danh sách ghế đã chọn và tính tổng tiền
    updateSelectedSeats() {
        const selectedSeatsContainer = $('#selectedSeats');
        
        if (this.selectedSeats.length === 0) {
            selectedSeatsContainer.html('<p class="text-muted">Chưa có ghế nào được chọn</p>');
            $('#nextBtn').prop('disabled', true);
            return;
        }

        let html = '<div class="selected-seats-list">';
        let totalPrice = 0;

        this.selectedSeats.forEach(seat => {
            html += `
                <div class="selected-seat-item">
                    <span class="seat-name">${seat.seatCode}</span>
                    <span class="seat-price">${this.formatCurrency(seat.price)}</span>
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
        $('#nextBtn').prop('disabled', false);
    }

    async searchCustomer() {
        // Kiểm tra input
        const searchInput = $('#customerPhone');
        const searchTerm = searchInput.val().trim();
        
        // Xóa thông báo cũ
        this.clearSearchMessage();
        
        if (!searchTerm) {
            this.showSearchError('Vui lòng nhập số điện thoại hoặc email');
            return;
        }

        try {
            this.showLoading();

            // Gọi API thông qua UI controller
            const response = await fetch(`/BookTicket/SearchCustomer?searchTerm=${encodeURIComponent(searchTerm)}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                }
            });

            if (response.ok) {
                const result = await response.json();
                
                if (result.success) {
                    this.hideLoading();
                    this.customerInfo = result.data;
                    this.displayCustomerInfo(result.data);
                } else {
                    this.hideLoading();
                    this.showSearchError(result.message || 'Không tìm thấy khách hàng');
                }
            } else {
                const errorData = await response.json();
                this.hideLoading();
                this.showSearchError(errorData.message || 'Không tìm thấy khách hàng');
            }
        } catch (error) {
            this.hideLoading();
            this.showSearchError('Có lỗi xảy ra khi tìm kiếm khách hàng');
        }
    }
    
    //Ẩn và xóa nội dung thông báo tìm kiếm
    clearSearchMessage() {
        $('#searchMessage').hide().removeClass('alert alert-success alert-danger').empty();
    }

    showSearchError(message) {
        const searchTerm = $('#customerPhone').val().trim();
        
        // Nếu thông báo lỗi là "không tìm thấy" và có search term, hiển thị nút tạo mới
        if (message.toLowerCase().includes('không tìm thấy') && searchTerm) {
            $('#searchMessage')
                .removeClass('alert-success')
                .addClass('alert alert-warning')
                .html(`
                    <div class="d-flex justify-content-between align-items-center">
                        <span><i class="fas fa-exclamation-triangle me-2"></i>${message}</span>
                        <button type="button" class="btn btn-sm btn-success" id="createNewCustomerBtn">
                            <i class="fas fa-user-plus me-1"></i>Tạo khách hàng mới
                        </button>
                    </div>
                `)
                .show();
                
            // Bind event cho nút tạo mới
            $('#createNewCustomerBtn').on('click', () => this.showCreateCustomerModal());
        } else {
            $('#searchMessage')
                .removeClass('alert-success')
                .addClass('alert alert-danger')
                .html(`<i class="fas fa-exclamation-circle me-2"></i>${message}`)
                .show();
        }
    }

    showSearchSuccess(message) {
        $('#searchMessage')
            .removeClass('alert-danger')
            .addClass('alert alert-success')
            .html(`<i class="fas fa-check-circle me-2"></i>${message}`)
            .show();
    }

    displayCustomerInfo(customer) {
        const customerInfoContainer = $('#customerInfo');
        
        if (!customer) {
            customerInfoContainer.hide();
            return;
        }

        // Xóa thông báo tìm kiếm khi hiển thị thông tin thành công
        this.clearSearchMessage();

        // Kiểm tra các trường dữ liệu
        const lastBookingDate = customer.lastBookingDate 
            ? this.formatDate(customer.lastBookingDate) 
            : 'Chưa có';

        // Nếu không có container, tạo mới
        if (customerInfoContainer.length === 0) {
            $('.booking-container').append(`
                <div id="customerInfo" class="customer-info-section mt-3">
                    <!-- Customer info will be inserted here -->
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
                            <p><strong>Email:</strong> ${customer.email || 'Chưa cập nhật'}</p>
                            <p><strong>Số điện thoại:</strong> ${customer.phoneNumber}</p>
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
            $('#usePoints').prop('disabled', true);
            return;
        }

        const usePointsInput = $('#usePoints');
        const maxPoints = this.customerInfo.points;
        
        // Kích hoạt/vô hiệu hóa input điểm
        usePointsInput.prop('disabled', maxPoints === 0);
        
        // Cập nhật max points
        usePointsInput.attr('max', maxPoints);
        usePointsInput.attr('placeholder', `Tối đa ${maxPoints} điểm`);
        
        // Xử lý khi thay đổi điểm
        usePointsInput.on('input', () => {
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
        const orderSummary = $('#orderSummary');
        const selectedSeatsElement = $('#selectedSeats');
        const usePointsInput = $('#usePoints');

        // Kiểm tra xem đã chọn ghế chưa
        if (this.selectedSeats.length === 0) {
            orderSummary.html('<p class="text-muted">Chưa có ghế nào được chọn</p>');
            return;
        }

        // Tính tổng giá vé
        const seatPrices = this.selectedSeats.map(seat => seat.price);
        const totalPrice = seatPrices.reduce((a, b) => a + b, 0);

        // Lấy số điểm sử dụng
        const usedPoints = Math.min(
            parseInt(usePointsInput.val() || 0), 
            this.customerInfo ? this.customerInfo.points : 0
        );
        
        // Tính giảm giá từ điểm
        const pointDiscount = usedPoints * 1000;
        const finalPrice = Math.max(0, totalPrice - pointDiscount);

        // Hiển thị chi tiết đơn hàng
        orderSummary.html(`
            <div class="order-summary-details">
                <div class="summary-item">
                    <span>Tổng giá vé</span>
                    <span>${totalPrice.toLocaleString()} VNĐ</span>
                </div>
                <div class="summary-item">
                    <span>Điểm sử dụng</span>
                    <span>${usedPoints} điểm (-${pointDiscount.toLocaleString()} VNĐ)</span>
                </div>
                <div class="summary-item total">
                    <span>Tổng thanh toán</span>
                    <span>${finalPrice.toLocaleString()} VNĐ</span>
                </div>
            </div>
        `);
    }

    //Validate và chuyển sang bước tiếp theo (1→2→3)
    nextStep() {
        if (this.currentStep < 3) {
            // Validate current step
            if (!this.validateStep(this.currentStep)) {
                return;
            }

            this.currentStep++;
            this.updateStepDisplay();
            
            // Load data for next step
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
        // Update step indicators
        $('.step').removeClass('active completed');
        for (let i = 1; i <= 3; i++) {
            if (i < this.currentStep) {
                $(`.step[data-step="${i}"]`).addClass('completed');
            } else if (i === this.currentStep) {
                $(`.step[data-step="${i}"]`).addClass('active');
            }
        }

        // Update step panels
        $('.step-panel').removeClass('active');
        $(`#step${this.currentStep}`).addClass('active');

        // Update navigation buttons
        $('#prevBtn').toggle(this.currentStep > 1);
        $('#nextBtn').toggle(this.currentStep < 3);
        $('#confirmBtn').toggle(this.currentStep === 3);
    }

    validateStep(step) {
        switch (step) {
            case 1:
                if (!this.selectedMovie || !this.selectedShowtime) {
                    this.showError('Vui lòng chọn phim và suất chiếu');
                    return false;
                }
                break;
            case 2:
                if (this.selectedSeats.length === 0) {
                    this.showError('Vui lòng chọn ít nhất một ghế');
                    return false;
                }
                break;
        }
        return true;
    }

    async confirmBooking() {
        const paymentMethod = $('input[name="paymentMethod"]:checked').val();
        const usedPoints = parseInt($('#usePoints').val()) || 0;

        if (!this.validateStep(4)) {
            this.showError('Vui lòng kiểm tra lại thông tin đặt vé');
            return;
        }

        try {
            this.showLoading();

            // Step 1: Get booking confirmation details (AC-01)
            const seatIdsParam = this.selectedSeats.map(seat => seat.id).join(',');
            const detailResponse = await fetch(`/BookTicket/GetBookingConfirmationDetail?showTimeId=${this.selectedShowtime.id}&seatIds=${seatIdsParam}&memberId=${this.customerInfo.id}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            });

            const detailResult = await detailResponse.json();
            
            if (!detailResult.success) {
                this.hideLoading();
                this.showError(detailResult.message || 'Không thể tải thông tin đặt vé');
                return;
            }

            // Step 2: Show confirmation modal with score conversion options (AC-02)
            this.showBookingConfirmationModal(detailResult.data, usedPoints);

        } catch (error) {
            this.hideLoading();
            this.showError('Có lỗi xảy ra khi xác nhận đặt vé. Vui lòng thử lại.');
        }
    }

    showBookingConfirmationModal(bookingDetail, requestedPoints) {
        // Build confirmation modal with all booking details (AC-01)
        const modalHtml = `
            <div class="modal fade" id="bookingConfirmModal" tabindex="-1" role="dialog">
                <div class="modal-dialog modal-lg" role="document">
                    <div class="modal-content">
                                    <div class="modal-header">
                <h5 class="modal-title">Xác Nhận Đặt Vé</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
                        <div class="modal-body">
                            <!-- Booking Information (Read-only) -->
                            <div class="row mb-3">
                                <div class="col-md-6">
                                    <h6>Thông Tin Đặt Vé</h6>
                                    <p><strong>Phim:</strong> ${bookingDetail.movieName}</p>
                                    <p><strong>Phòng chiếu:</strong> ${bookingDetail.screen}</p>
                                    <p><strong>Ngày:</strong> ${bookingDetail.date}</p>
                                    <p><strong>Giờ:</strong> ${bookingDetail.time}</p>
                                    <p><strong>Ghế:</strong> ${bookingDetail.seat}</p>
                                    <p><strong>Tổng tiền:</strong> ${this.formatCurrency(bookingDetail.total)}</p>
                                </div>
                                <div class="col-md-6">
                                    <h6>Thông Tin Khách Hàng</h6>
                                    <p><strong>Mã thành viên:</strong> ${bookingDetail.memberId}</p>
                                    <p><strong>Họ tên:</strong> ${bookingDetail.fullName}</p>
                                    <p><strong>Điểm tích lũy:</strong> ${bookingDetail.memberScore}</p>
                                    <p><strong>CCCD/CMND:</strong> ${bookingDetail.identityCard}</p>
                                    <p><strong>Số điện thoại:</strong> ${bookingDetail.phoneNumber}</p>
                                </div>
                            </div>

                            <!-- Score Conversion Options (AC-02) -->
                            ${bookingDetail.canConvertScore ? `
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
                            ` : '<div class="alert alert-info">Không đủ điểm để chuyển đổi vé</div>'}

                            <!-- Payment Method -->
                            <div class="form-group">
                                <label>Phương thức thanh toán:</label>
                                <div class="form-check">
                                    <input class="form-check-input" type="radio" name="modalPaymentMethod" id="modalCash" value="cash" checked>
                                    <label class="form-check-label" for="modalCash">Tiền mặt</label>
                                </div>
                                <div class="form-check">
                                    <input class="form-check-input" type="radio" name="modalPaymentMethod" id="modalCard" value="card">
                                    <label class="form-check-label" for="modalCard">Thẻ</label>
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

        // Remove existing modal and add new one
        $('#bookingConfirmModal').remove();
        $('body').append(modalHtml);

        // Bind events for score conversion
        this.bindScoreConversionEvents(bookingDetail);

        // Show modal - Bootstrap 5 way
        const modal = new bootstrap.Modal(document.getElementById('bookingConfirmModal'));
        modal.show();
        this.hideLoading();
    }

    bindScoreConversionEvents(bookingDetail) {
        // Toggle conversion options
        $('input[name="scoreConversion"]').change(function() {
            const useConversion = $(this).val() === 'true';
            $('#conversionOptions').toggle(useConversion);
            if (!useConversion) {
                $('#ticketsToConvert').val(0);
                $('#conversionSummary').html('');
            }
        });

        // Update conversion summary
        $('#ticketsToConvert').on('input', function() {
            const tickets = parseInt($(this).val()) || 0;
            const maxTickets = bookingDetail.maxTicketsFromScore;
            
            if (tickets > maxTickets) {
                $(this).val(maxTickets);
                tickets = maxTickets;
            }

            if (tickets > 0) {
                const pointsNeeded = tickets * bookingDetail.scorePerTicket;
                const remainingPoints = bookingDetail.memberScore - pointsNeeded;
                
                // AC-03: Check if score is sufficient
                if (remainingPoints < 0) {
                    $('#conversionSummary').html('<div class="text-danger">Not enough score to convert into ticket</div>');
                    $('#finalConfirmBtn').prop('disabled', true);
                } else {
                    $('#conversionSummary').html(`
                        <div class="text-success">
                            <p>Sẽ sử dụng: ${pointsNeeded} điểm</p>
                            <p>Điểm còn lại: ${remainingPoints}</p>
                        </div>
                    `);
                    $('#finalConfirmBtn').prop('disabled', false);
                }
            } else {
                $('#conversionSummary').html('');
                $('#finalConfirmBtn').prop('disabled', false);
            }
        });

        // Final confirm button
        $('#finalConfirmBtn').off('click').on('click', () => {
            this.executeFinalBooking(bookingDetail);
        });
    }

    async executeFinalBooking(bookingDetail) {
        try {
            this.showLoading();

            const useScoreConversion = $('input[name="scoreConversion"]:checked').val() === 'true';
            const ticketsToConvert = useScoreConversion ? parseInt($('#ticketsToConvert').val()) || 0 : 0;
            const paymentMethod = $('input[name="modalPaymentMethod"]:checked').val();

            // Prepare data for new API (AC-05) - Use PascalCase for C# model binding
            const bookingData = {
                ShowTimeId: this.selectedShowtime.id,
                SeatIds: this.selectedSeats.map(seat => seat.id),
                MemberId: this.customerInfo.id.toString(),
                UseScoreConversion: useScoreConversion,
                TicketsToConvert: ticketsToConvert,
                PaymentMethod: paymentMethod,
                StaffId: 'admin', // Could be from session
                Notes: ''
            };

            const response = await fetch('/BookTicket/ConfirmBookingWithScore', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(bookingData)
            });

            const result = await response.json();
            this.hideLoading();

            if (result.success && result.data) {
                // Hide confirmation modal - Bootstrap 5 way
                const confirmModal = bootstrap.Modal.getInstance(document.getElementById('bookingConfirmModal'));
                confirmModal.hide();
                
                // Display success result
                this.displayBookingSuccess(result.data);
                
                // Reset form
                this.resetBookingForm();
                this.currentStep = 1;
                this.updateStepDisplay();
            } else {
                // AC-03: Display error if score insufficient or other errors
                this.showError(result.message || 'Đặt vé không thành công');
            }
        } catch (error) {
            this.hideLoading();
            this.showError('Có lỗi xảy ra khi đặt vé. Vui lòng thử lại.');
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
                                <h4 class="text-success">${bookingData.message}</h4>
                                <h5>Mã đặt vé: <strong>${bookingData.bookingCode}</strong></h5>
                            </div>
                            
                            <div class="row">
                                <div class="col-md-6">
                                    <h6>Chi Tiết Đặt Vé</h6>
                                    <p><strong>Phim:</strong> ${bookingData.movieTitle}</p>
                                    <p><strong>Phòng:</strong> ${bookingData.cinemaRoom}</p>
                                    <p><strong>Ngày:</strong> ${bookingData.showDate}</p>
                                    <p><strong>Giờ:</strong> ${bookingData.showTime}</p>
                                    <p><strong>Ghế:</strong> ${bookingData.seats.map(s => s.seatCode).join(', ')}</p>
                                </div>
                                <div class="col-md-6">
                                    <h6>Thanh Toán</h6>
                                    <p><strong>Tạm tính:</strong> ${this.formatCurrency(bookingData.subTotal)}</p>
                                    ${bookingData.scoreUsed ? `
                                        <p><strong>Giảm giá (${bookingData.ticketsConvertedFromScore} vé):</strong> -${this.formatCurrency(bookingData.scoreDiscount)}</p>
                                        <p><strong>Điểm đã sử dụng:</strong> ${bookingData.scoreDeducted}</p>
                                        <p><strong>Điểm còn lại:</strong> ${bookingData.remainingScore}</p>
                                    ` : ''}
                                    <p><strong>Tổng cộng:</strong> ${this.formatCurrency(bookingData.total)}</p>
                                    <p><strong>Thanh toán:</strong> ${this.getPaymentMethodText(bookingData.paymentMethod)}</p>
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

        // Remove existing modal if any
        $('#bookingSuccessModal').remove();
        
        // Add modal to body
        $('body').append(successModal);
        
        // Show modal - Bootstrap 5 way
        const successModalInstance = new bootstrap.Modal(document.getElementById('bookingSuccessModal'));
        successModalInstance.show();
        
        // Add manual close event handlers for better compatibility
        $('#bookingSuccessModal').on('click', '[data-bs-dismiss="modal"], #closeSuccessModal', function() {
            successModalInstance.hide();
        });
        
        // Close on backdrop click
        $('#bookingSuccessModal').on('click', function(e) {
            if (e.target === this) {
                successModalInstance.hide();
            }
        });
        
        // Close on escape key
        $(document).on('keydown.successModal', function(e) {
            if (e.keyCode === 27) { // Escape key
                successModalInstance.hide();
                $(document).off('keydown.successModal');
            }
        });
        
        // Clean up when modal is hidden
        $('#bookingSuccessModal').on('hidden.bs.modal', function() {
            $(this).remove();
            $(document).off('keydown.successModal');
        });
    }

    resetBookingForm() {
        // Reset movie selection
        $('#movieSelect').val('');
        $('#selectedMovieInfo').hide();
        this.selectedMovie = null;
        
        // Reset showtime
        $('#showtimeSelection').html('<p class="text-muted">Vui lòng chọn phim trước</p>');
        this.selectedShowtime = null;
        
        // Reset seats
        this.selectedSeats = [];
        $('#seatMap').empty();
        this.updateSelectedSeats();

        // Reset customer info
        $('#customerPhone').val('');
        $('#customerInfo').hide();
        this.customerInfo = null;
        this.clearSearchMessage();

        // Reset points
        $('#usePoints').val(0).prop('disabled', true);
        
        // Reset payment method
        $('input[name="paymentMethod"][value="cash"]').prop('checked', true);
        
        // Reset order summary
        this.updateOrderSummary();
        
        // Clear any error messages
        $('.alert').hide();
    }

    searchMovies(searchTerm) {
        const movieCards = $('.movie-card');
        
        if (!searchTerm) {
            movieCards.show();
            return;
        }

        movieCards.each(function() {
            const title = $(this).find('.movie-title').text().toLowerCase();
            const genre = $(this).find('.movie-genre').text().toLowerCase();
            
            if (title.includes(searchTerm.toLowerCase()) || genre.includes(searchTerm.toLowerCase())) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    }

    // Utility methods
    formatDate(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    }

    formatTime(timeString) {
        const time = new Date(timeString);
        return time.toLocaleTimeString('vi-VN', {
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    }

    showLoading() {
        $('#loadingOverlay').show();
    }

    hideLoading() {
        $('#loadingOverlay').hide();
    }

    showError(message) {
        // For general errors only (not search errors)
        alert('Lỗi: ' + message);
    }

    showSuccess(message) {
        // For general success messages
        alert('Thành công: ' + message);
    }

    // Thêm phương thức mới để định dạng giờ
    formatTimeFromString(timeString) {
        // Giả sử timeString ở định dạng "HH:mm"
        return timeString;
    }

    // Phương thức mới để format ngày đầy đủ
    formatFullDate(dateString) {
        // dateString có dạng 'YYYY-MM-DD'
        const [year, month, day] = dateString.split('-');
        const daysOfWeek = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
        
        // Tạo đối tượng Date để lấy thứ
        const date = new Date(dateString);
        const dayOfWeek = daysOfWeek[date.getDay()];

        return `${dayOfWeek}, ${day}/${month}/${year}`;
    }

    // Modal tạo khách hàng mới
    showCreateCustomerModal() {
        // Clear form
        $('#createCustomerForm')[0].reset();
        
        // Auto fill phone number from search input
        const searchTerm = $('#customerPhone').val().trim();
        if (searchTerm) {
            $('#newCustomerPhone').val(searchTerm);
        }
        
        // Show modal - Bootstrap 5 way
        const modal = new bootstrap.Modal(document.getElementById('createCustomerModal'));
        modal.show();
    }

    async createCustomer() {
        try {
            const formData = {
                fullName: $('#createFullName').val(),
                email: $('#createEmail').val(),
                phoneNumber: $('#createPhone').val(),
                identityCard: $('#createIdentityCard').val()
            };

            const response = await fetch('/BookTicket/CreateCustomer', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            });

            const result = await response.json();

            if (result.success) {
                // Hide modal - Bootstrap 5 way
                const modal = bootstrap.Modal.getInstance(document.getElementById('createCustomerModal'));
                modal.hide();
                this.showSuccess('Tạo khách hàng thành công');
                
                // Auto search the new customer
                $('#customerPhone').val(formData.phoneNumber);
                await this.searchCustomer();
            } else {
                alert('Lỗi: ' + (result.message || 'Không thể tạo khách hàng'));
            }
        } catch (error) {
            alert('Có lỗi xảy ra khi tạo khách hàng');
        }
    }

    // Helper method để convert payment method
    getPaymentMethodText(paymentMethod) {
        switch (paymentMethod?.toLowerCase()) {
            case 'cash':
                return 'Tiền mặt';
            case 'vnpay':
                return 'VNPay';
            default:
                return 'Tiền mặt';
        }
    }
}

// Initialize when document is ready
$(document).ready(function() {
    const dashboard = new BookTicketDashboard();

    // Thêm event listener cho nút in vé
    $('.print-ticket').on('click', function() {
        const modalContent = $('#bookingConfirmationModal .modal-body').clone();
        
        // Tạo cửa sổ in
        const printWindow = window.open('', '_blank', 'width=600,height=800');
        
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