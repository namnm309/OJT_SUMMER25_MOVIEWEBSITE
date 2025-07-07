// Book Ticket Dashboard JavaScript
class BookTicketDashboard {
    constructor() {
        this.currentStep = 1;
        this.selectedMovie = null;
        this.selectedShowtime = null;
        this.selectedSeats = [];
        this.customerInfo = null;
        this.totalPrice = 0;
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadMovies();
    }

    bindEvents() {
        // Navigation buttons
        $('#nextBtn').on('click', () => this.nextStep());
        $('#prevBtn').on('click', () => this.prevStep());
        $('#confirmBtn').on('click', () => this.confirmBooking());

        // Movie search
        $('#movieSearch').on('input', (e) => this.searchMovies(e.target.value));

        // Customer search
        $('#searchCustomer').on('click', () => this.searchCustomer());
        $('#customerPhone').on('keypress', (e) => {
            if (e.which === 13) this.searchCustomer();
        });

        // Payment method change
        $('input[name="paymentMethod"]').on('change', () => this.updateOrderSummary());
    }

    async loadMovies() {
        try {
            this.showLoading();
            const response = await fetch('/BookingManagement/BookingTicket/GetMovies');
            const data = await response.json();
            
            if (data.success) {
                this.displayMovies(data.data);
            } else {
                this.showError('Không thể tải danh sách phim');
            }
        } catch (error) {
            console.error('Error loading movies:', error);
            this.showError('Có lỗi xảy ra khi tải danh sách phim');
        } finally {
            this.hideLoading();
        }
    }

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

    async selectMovie(movie) {
        this.selectedMovie = movie;
        
        // Load showtimes
        await this.loadShowtimes(movie.id);
    }

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
        const phoneNumber = $('#customerPhone').val().trim();
        
        if (!phoneNumber) {
            this.showError('Vui lòng nhập số điện thoại');
            return;
        }

        try {
            this.showLoading();
            const response = await fetch('/api/v1/booking-ticket/check-member', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ phoneNumber: phoneNumber })
            });
            const data = await response.json();
            
            if (data.success) {
                this.customerInfo = data.data;
                this.displayCustomerInfo(data.data);
            } else {
                this.showError('Không tìm thấy khách hàng');
                this.customerInfo = null;
                $('#customerInfo').hide();
            }
        } catch (error) {
            console.error('Error searching customer:', error);
            this.showError('Có lỗi xảy ra khi tìm kiếm khách hàng');
        } finally {
            this.hideLoading();
        }
    }

    displayCustomerInfo(customer) {
        const customerInfo = $('#customerInfo');
        customerInfo.html(`
            <div class="customer-details">
                <h6>Thông tin khách hàng</h6>
                <p><strong>Họ tên:</strong> ${customer.fullName}</p>
                <p><strong>Số điện thoại:</strong> ${customer.phoneNumber}</p>
                <p><strong>Email:</strong> ${customer.email}</p>
                <p><strong>Điểm thưởng:</strong> ${customer.points} điểm</p>
                
                <div class="points-usage mt-3">
                    <label for="usePoints">Sử dụng điểm thưởng:</label>
                    <input type="number" class="form-control" id="usePoints" 
                           min="0" max="${customer.points}" value="0"
                           placeholder="Nhập số điểm muốn sử dụng">
                    <small class="text-muted">1 điểm = 1,000 VNĐ</small>
                </div>
            </div>
        `);
        
        customerInfo.show();
        
        // Bind points usage change
        $('#usePoints').on('input', () => this.updateOrderSummary());
        
        this.updateOrderSummary();
    }

    updateOrderSummary() {
        const orderSummary = $('#orderSummary');
        
        if (!this.selectedMovie || !this.selectedShowtime || this.selectedSeats.length === 0) {
            orderSummary.html('<p class="text-muted">Chưa có thông tin đặt vé</p>');
            return;
        }

        const usePoints = parseInt($('#usePoints').val()) || 0;
        const pointsDiscount = usePoints * 1000; // 1 point = 1,000 VND
        const finalTotal = Math.max(0, this.totalPrice - pointsDiscount);

        const html = `
            <div class="order-details">
                <h6>Chi tiết đơn hàng</h6>
                <p><strong>Phim:</strong> ${this.selectedMovie.title}</p>
                <p><strong>Suất chiếu:</strong> ${this.formatTime(this.selectedShowtime.startTime)}</p>
                <p><strong>Ghế:</strong> ${this.selectedSeats.map(s => s.seatCode).join(', ')}</p>
                
                <hr>
                
                <div class="price-breakdown">
                    <div class="price-item">
                        <span>Tổng tiền vé:</span>
                        <span>${this.formatCurrency(this.totalPrice)}</span>
                    </div>
                    ${usePoints > 0 ? `
                        <div class="price-item discount">
                            <span>Giảm giá (${usePoints} điểm):</span>
                            <span>-${this.formatCurrency(pointsDiscount)}</span>
                        </div>
                    ` : ''}
                    <div class="price-total">
                        <strong>
                            <span>Tổng thanh toán:</span>
                            <span>${this.formatCurrency(finalTotal)}</span>
                        </strong>
                    </div>
                </div>
            </div>
        `;

        orderSummary.html(html);
    }

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
        if (!this.validateStep(3)) {
            return;
        }

        const paymentMethod = $('input[name="paymentMethod"]:checked').val();
        const usePoints = parseInt($('#usePoints').val()) || 0;
        
        const bookingData = {
            movieId: this.selectedMovie.id,
            showtimeId: this.selectedShowtime.id,
            seatIds: this.selectedSeats.map(s => s.id),
            customerId: this.customerInfo?.id,
            customerPhone: $('#customerPhone').val(),
            usePoints: usePoints,
            paymentMethod: paymentMethod,
            totalPrice: this.totalPrice
        };

        try {
            this.showLoading();
            
            if (paymentMethod === 'vnpay') {
                await this.processVNPayPayment(bookingData);
            } else {
                await this.processCashPayment(bookingData);
            }
        } catch (error) {
            console.error('Error confirming booking:', error);
            this.showError('Có lỗi xảy ra khi xác nhận đặt vé');
        } finally {
            this.hideLoading();
        }
    }

    async processCashPayment(bookingData) {
        const response = await fetch('/api/v1/booking-ticket/confirm-Admin-booking', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(bookingData)
        });

        const data = await response.json();
        
        if (data.success) {
            this.showSuccess('Đặt vé thành công!');
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        } else {
            this.showError(data.message || 'Có lỗi xảy ra khi đặt vé');
        }
    }

    async processVNPayPayment(bookingData) {
        const response = await fetch('/BookingManagement/BookingTicket/ProcessVNPayPayment', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(bookingData)
        });

        const data = await response.json();
        
        if (data.success) {
            // Redirect to VNPay
            window.open(data.paymentUrl, '_blank');
            this.showSuccess('Đang chuyển hướng đến VNPay...');
        } else {
            this.showError(data.message || 'Có lỗi xảy ra khi xử lý thanh toán');
        }
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
        // You can implement a toast notification here
        alert('Lỗi: ' + message);
    }

    showSuccess(message) {
        // You can implement a toast notification here
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
}

// Initialize when document is ready
$(document).ready(function() {
    new BookTicketDashboard();
});