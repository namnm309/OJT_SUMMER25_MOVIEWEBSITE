// Payment logic module cho trang SelectSeat - FIXED VERSION
(function (window) {
    let continueBtn = null;
    let showTimeId = null;
    let lastBookingId = null;
    let selectedPromotion = null;
    // Lưu tổng tiền gốc (chưa khuyến mãi) để tính toán FE
    let originalTotal = 0;
    
    function getAuthToken() {
        return sessionStorage.getItem('jwtToken') || localStorage.getItem('jwtToken') || localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
    }
    
    function getAllHeldSeatLogIds() {
        return window.SeatModule && window.SeatModule.getAllHeldSeatLogIds ? window.SeatModule.getAllHeldSeatLogIds() : [];
    }
    
    function getSelectedSeats() {
        return window.SeatModule && window.SeatModule.selectedSeats ? window.SeatModule.selectedSeats() : [];
    }
    
    // Gọi API summary với tất cả SeatLogId đã hold
    async function getSeatSummary() {
        try {
            const allSeatLogIds = getAllHeldSeatLogIds();
            if (!allSeatLogIds || !Array.isArray(allSeatLogIds) || allSeatLogIds.length === 0) {
                alert('Không có ghế nào được giữ!');
                return;
            }
            
            console.log('seatLogIds gửi lên:', allSeatLogIds);
            const body = {
                seatLogId: allSeatLogIds,
                promotionId: selectedPromotion ? (selectedPromotion.promotionId || selectedPromotion.id) : null
            };
            
            const response = await fetch('https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/seatsignal/summary', {
                method: 'POST',
                headers: { 
                    'Authorization': `Bearer ${getAuthToken()}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(body)
            });
            
            const result = await response.json();
            console.log('Kết quả summary:', result);
            return result;
        } catch (error) {
            console.error('Error in getSeatSummary:', error);
            return { code: 400, message: 'Có lỗi xảy ra khi lấy thông tin tóm tắt ghế' };
        }
    }
    
    // Hàm validate và chuyển sang thanh toán
    async function validateAndContinue() {
        console.log('validateAndContinue called');
        const summaryResult = await getSeatSummary();
        console.log('summaryResult:', summaryResult);
        
        let bookingId = null;
        let amount = null;

        if (
            summaryResult &&
            summaryResult.value &&
            summaryResult.value.code === 200 &&
            summaryResult.value.data
        ) {
            const data = summaryResult.value.data;
            bookingId = data.bookingId;
            amount = data.finalPrice || data.totalAmount || data.amount || data.totalPrice || data.total;
        }

        if (bookingId) {
            try {
                sessionStorage.setItem('lastBookingId', bookingId);
                if (summaryResult.value.data.bookingCode) {
                    sessionStorage.setItem('lastBookingCode', summaryResult.value.data.bookingCode);
                }
            } catch (e) {
                console.warn('Cannot save booking info', e);
            }

            // Cập nhật UI tạm thời
            if (summaryResult && summaryResult.value && summaryResult.value.data) {
                applySummaryData(summaryResult.value.data);
            }
            if (typeof disableContinueBtn === 'function') disableContinueBtn();

            // Gọi VNPay ngay
            await callVnpayPayment(bookingId, amount);
        } else {
            alert(
                (summaryResult && summaryResult.value && summaryResult.value.message) ||
                summaryResult.message ||
                'Không thể lấy thông tin ghế để thanh toán'
            );
        }
    }
    
    // FIXED: Hàm xử lý VNPAY với error handling tốt hơn
    async function callVnpayPayment(bookingId, amount = null) {
        try {
            console.log('Calling VNPay API with bookingId:', bookingId, 'amount:', amount);
            
            // Nếu có voucher, lưu vào session để redeem sau khi thanh toán thành công
            if (selectedPromotion && (selectedPromotion.userPromotionId || selectedPromotion.id)) {
                sessionStorage.setItem('promoToRedeem', selectedPromotion.userPromotionId || selectedPromotion.id);
            }
            
            // Nếu không có amount, lấy từ summary API
            if (!amount) {
                const summaryResult = await getSeatSummary();
                if (summaryResult && summaryResult.value && summaryResult.value.data) {
                    amount = summaryResult.value.data.finalPrice ||
                            summaryResult.value.data.totalAmount || 
                            summaryResult.value.data.amount || 
                            summaryResult.value.data.totalPrice ||
                            summaryResult.value.data.total;
                }
            }
            
            if (!amount) {
                throw new Error('Không thể lấy thông tin số tiền thanh toán');
            }
            
            // Đảm bảo amount là số nguyên (VNPay yêu cầu)
            const finalAmount = Math.round(Number(amount));
            
            if (finalAmount < 5000 || finalAmount >= 1000000000) {
                throw new Error(`Số tiền không hợp lệ: ${finalAmount}. Phải từ 5,000 đến dưới 1 tỷ đồng`);
            }
            
            const requestBody = {
                bookingId: bookingId,
                amount: finalAmount, // Thêm amount vào request
                Decription: "Thanh toán vé xem phim"
            };
            
            console.log('Request body:', requestBody);
            
            const response = await fetch('https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/payment/vnpay', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${getAuthToken()}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestBody)
            });
            
            console.log('Response status:', response.status);
            console.log('Response headers:', response.headers);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            // Lấy content-type để xác định cách parse
            const contentType = response.headers.get('content-type');
            let result;
            
            if (contentType && contentType.includes('application/json')) {
                result = await response.json();
                console.log('JSON result:', result);
            } else {
                result = await response.text();
                console.log('Text result:', result);
            }
            
            // Xử lý các trường hợp response khác nhau
            let paymentUrl = null;
            
            if (typeof result === 'string') {
                if (result.startsWith('http')) {
                    paymentUrl = result;
                } else if (result.startsWith('/')) {
                    // Nếu là relative path, ghép với domain VNPay
                    paymentUrl = 'https://sandbox.vnpayment.vn' + result;
                } else {
                    // Thử parse làm JSON nếu có thể
                    try {
                        const parsed = JSON.parse(result);
                        paymentUrl = extractPaymentUrl(parsed);
                    } catch (e) {
                        console.error('Cannot parse result as JSON:', result);
                    }
                }
            } else if (typeof result === 'object') {
                paymentUrl = extractPaymentUrl(result);
            }
            
            if (paymentUrl) {
                console.log('Redirecting to:', paymentUrl);
                // Thử nhiều cách redirect
                if (window.top) {
                    window.top.location.href = paymentUrl;
                } else {
                    window.location.href = paymentUrl;
                }
                
                // Fallback nếu redirect không hoạt động
                setTimeout(() => {
                    if (window.location.href !== paymentUrl) {
                        window.open(paymentUrl, '_self');
                    }
                }, 1000);
                
            } else {
                console.error('Không lấy được payment URL từ response:', result);
                alert('Không thể lấy link thanh toán. Vui lòng thử lại!');
            }
            
        } catch (error) {
            console.error('Error calling VNPay API:', error);
            alert('Có lỗi xảy ra khi tạo thanh toán: ' + error.message);
        }
    }
    
    // Hàm extract payment URL từ object response
    function extractPaymentUrl(obj) {
        if (!obj) return null;
        
        // Thử các trường có thể chứa payment URL
        const possibleFields = [
            'paymentUrl', 'payment_url', 'url', 
            'redirectUrl', 'redirect_url',
            'vnpayUrl', 'vnpay_url',
            'data.paymentUrl', 'data.payment_url', 'data.url',
            'result.paymentUrl', 'result.payment_url', 'result.url'
        ];
        
        for (let field of possibleFields) {
            let value = obj;
            const parts = field.split('.');
            
            for (let part of parts) {
                if (value && typeof value === 'object' && value[part] !== undefined) {
                    value = value[part];
                } else {
                    value = null;
                    break;
                }
            }
            
            if (value && typeof value === 'string' && 
                (value.startsWith('http') || value.startsWith('/'))) {
                return value;
            }
        }
        
        return null;
    }
    
    function initPaymentModule(options) {
        continueBtn = options.continueBtn;
        showTimeId = options.showTimeId;
        
        // Init promotion handlers
        initPromotionHandlers();

        // Init cancel buttons
        const cancelSeatsBtn = document.getElementById('cancelSeatsBtn');
        const cancelVoucherBtn = document.getElementById('cancelVoucherBtn');
        if (cancelSeatsBtn) {
            cancelSeatsBtn.addEventListener('click', async () => {
                await cancelAllSeats();
            });
        }
        if (cancelVoucherBtn) {
            cancelVoucherBtn.addEventListener('click', () => {
                cancelVoucher();
            });
        }
    }
    
    // FIXED: Event handler cho confirm seats
    if (document.getElementById('confirmSeatsBtn')) {
        document.getElementById('confirmSeatsBtn').onclick = async function () {
            const summaryResult = await getSeatSummary();
            let bookingId = null;
            let amount = null;

            // Trường hợp response được wrap trong property "value" (ASP.NET IActionResult)
            if (
                summaryResult &&
                summaryResult.value &&
                summaryResult.value.code === 200 &&
                summaryResult.value.data
            ) {
                const data = summaryResult.value.data;
                bookingId = data.bookingId;
                amount = data.finalPrice || data.totalAmount || data.amount || data.totalPrice || data.total;
            }
            // Trường hợp response ở dạng { success, data, ... }
            else if (summaryResult && summaryResult.success && summaryResult.data) {
                bookingId = summaryResult.data.bookingId;
                amount = summaryResult.data.finalPrice || summaryResult.data.totalAmount || summaryResult.data.amount || summaryResult.data.totalPrice || summaryResult.data.total;
            }

            if (bookingId) {
                lastBookingId = bookingId;
                // Gọi VNPay trực tiếp, không cần nút riêng
                await callVnpayPayment(lastBookingId, amount);
            } else {
                alert('Không thể đặt ghế!');
            }
        };
    }
    
    function showVnpayButton(bookingId) {
        lastBookingId = bookingId;
        console.log('Set lastBookingId:', lastBookingId);
        
        const payBtn = document.getElementById('payVnpayBtn');
        if (payBtn) {
            payBtn.style.display = 'inline-block';
            payBtn.dataset.manualVisible = 'true';
            
            // Remove existing event listeners
            payBtn.onclick = null;
            
            payBtn.onclick = async function() {
                if (!lastBookingId) {
                    alert('Bạn chưa xác nhận đặt ghế!');
                    return;
                }
                
                // Disable button để tránh double click
                this.disabled = true;
                this.textContent = 'Đang xử lý...';
                
                try {
                    // Lấy amount từ summary trước khi gọi VNPay
                    const summaryResult = await getSeatSummary();
                    let amount = null;
                    
                    if (summaryResult && summaryResult.value && summaryResult.value.data) {
                        amount = summaryResult.value.data.finalPrice ||
                                summaryResult.value.data.totalAmount || 
                                summaryResult.value.data.amount || 
                                summaryResult.value.data.totalPrice ||
                                summaryResult.value.data.total;
                    }
                    
                    console.log('Amount from summary:', amount);
                    await callVnpayPayment(lastBookingId, amount);
                } catch (error) {
                    console.error('Payment error:', error);
                    alert('Có lỗi xảy ra trong quá trình thanh toán: ' + error.message);
                } finally {
                    // Re-enable button
                    this.disabled = false;
                    this.textContent = 'Thanh toán';
                }
            };
        } else {
            console.error('Không tìm thấy nút payVnpayBtn trên DOM!');
        }
    }

    // Thêm vào trong IIFE của Payment.js

    // Thêm vào hàm initPaymentModule
    function initPaymentModule(options) {
        continueBtn = options.continueBtn;
        showTimeId = options.showTimeId;
        
        // Init promotion handlers
        initPromotionHandlers();
    }

    // Thêm các hàm xử lý promotion
    function initPromotionHandlers() {
        const showBtn = document.getElementById('showPromotionsBtn');
        const popup = document.getElementById('promotionsPopup');
        const closeBtn = document.querySelector('.close-popup');
        
        if (showBtn) {
            showBtn.addEventListener('click', async () => {
                await loadPromotions();
                popup.style.display = 'flex';
            });
        }
        
        if (closeBtn) {
            closeBtn.addEventListener('click', () => {
                popup.style.display = 'none';
            });
        }
        
        // Click outside to close
        popup.addEventListener('click', (e) => {
            if (e.target === popup) {
                popup.style.display = 'none';
            }
        });
    }

    async function loadPromotions() {
        try {
            const response = await fetch('https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/promotions/my', {
                headers: {
                    'Authorization': `Bearer ${getAuthToken()}`
                }
            });
            
            if (!response.ok) throw new Error('Failed to load promotions');
            
            const result = await response.json();
            if (result && result.data) {
                displayPromotions(result.data);
            }
        } catch (error) {
            console.error('Error loading promotions:', error);
            document.querySelector('.promotions-list').innerHTML = 
                '<div class="error-message">Không thể tải danh sách khuyến mãi</div>';
        }
    }

    function displayPromotions(promotions) {
        const listElement = document.querySelector('.promotions-list');
        // lọc bỏ voucher đã đổi
        promotions = (promotions || []).filter(p=> !p.isRedeemed && !p.isRedeemed === false ? true : !p.isRedeemed);

        if (!promotions || promotions.length === 0) {
            listElement.innerHTML = '<div class="no-promotions">Bạn chưa có mã khuyến mãi nào</div>';
            return;
        }
        
        listElement.innerHTML = promotions.map(promo => {
            const expiryDate = new Date(promo.endDate || promo.expiryDate);
            const daysLeft = Math.ceil((expiryDate - new Date()) / (1000 * 60 * 60 * 24));
            const promoUniqueId = promo.promotionId || promo.id || promo.userPromotionId;
            const isSelected = selectedPromotion && (selectedPromotion.promotionId || selectedPromotion.id) === promoUniqueId;
            
            return `
                <div class="promotion-item ${isSelected ? 'selected' : ''}" 
                     data-promotion-id="${promoUniqueId}">
                    <div class="promotion-info">
                        <div class="promotion-discount">Giảm ${promo.discountPercent}%</div>
                        <div class="promotion-expiry">Còn ${daysLeft} ngày</div>
                    </div>
                    <button class="btn-select-promotion">
                        ${isSelected ? 'Đã chọn' : 'Chọn'}
                    </button>
                </div>
            `;
        }).join('');
        
        // Add click handlers
        listElement.querySelectorAll('.promotion-item').forEach(item => {
            item.addEventListener('click', async () => {
                const promoId = item.dataset.promotionId;
                const promo = promotions.find(p => (p.promotionId || p.id || p.userPromotionId) === promoId);
                
                if (selectedPromotion && (selectedPromotion.promotionId || selectedPromotion.id) === promoId) {
                    // Deselect
                    selectedPromotion = null;
                    item.classList.remove('selected');
                } else {
                    // Select new
                    selectedPromotion = promo;
                    document.querySelectorAll('.promotion-item').forEach(i => i.classList.remove('selected'));
                    item.classList.add('selected');
                }
                
                // Chỉ cập nhật UI, KHÔNG gọi API summary
                updateSelectedPromotionUI();
                
                // Close popup
                document.getElementById('promotionsPopup').style.display = 'none';
            });
        });
    }

    async function cancelAllSeats() {
        try {
            const seatLogIds = getAllHeldSeatLogIds();
            if (!seatLogIds || seatLogIds.length === 0) {
                alert('Bạn chưa giữ ghế nào!');
                return;
            }
            const confirms = confirm('Bạn chắc chắn muốn hủy tất cả ghế đã chọn?');
            if (!confirms) return;

            await Promise.all(
                seatLogIds.map(id => fetch(`https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/seatsignal/release/${id}`, {
                    method: 'DELETE',
                    headers: { 'Authorization': `Bearer ${getAuthToken()}` }
                }))
            );

            // Làm sạch dữ liệu ở SeatModule và UI
            if (window.SeatModule && window.SeatModule.clearAllSeatLogIds) {
                window.SeatModule.clearAllSeatLogIds();
            }
            if (window.SeatModule && window.SeatModule.loadSeats) {
                await window.SeatModule.loadSeats();
                window.SeatModule.updateDisplay();
            }

            lastBookingId = null;
            continueBtn.disabled = true;
            continueBtn.textContent = 'CHỌN GHẾ ĐỂ THANH TOÁN';
            const payBtn = document.getElementById('payVnpayBtn');
            if (payBtn) { payBtn.style.display = 'none'; payBtn.dataset.manualVisible='false'; }
            alert('Đã hủy giữ ghế.');
        } catch (e) {
            console.error('cancelAllSeats error:', e);
            alert('Có lỗi khi hủy ghế!');
        }
    }

    function cancelVoucher() {
        if (!selectedPromotion) return;
        selectedPromotion = null;
        updateSelectedPromotionUI();
        const cancelVoucherBtn = document.getElementById('cancelVoucherBtn');
        if (cancelVoucherBtn) cancelVoucherBtn.style.display = 'none';
    }

    function updateSelectedPromotionUI() {
        const btn = document.getElementById('showPromotionsBtn');
        if (selectedPromotion) {
            btn.innerHTML = `<i class="fas fa-ticket-alt"></i> Giảm ${selectedPromotion.discountPercent}%`;
            const cancelVoucherBtn = document.getElementById('cancelVoucherBtn');
            if (cancelVoucherBtn) cancelVoucherBtn.style.display = 'inline-block';
            // Lưu promotionId để dùng khi thanh toán
            sessionStorage.setItem('selectedPromotionId', selectedPromotion.promotionId || selectedPromotion.id || '');
        } else {
            btn.innerHTML = `<i class="fas fa-ticket-alt"></i> Chọn mã`;
            const cancelVoucherBtn = document.getElementById('cancelVoucherBtn');
            if (cancelVoucherBtn) cancelVoucherBtn.style.display = 'none';
            sessionStorage.removeItem('selectedPromotionId');
        }
        // Cập nhật lại tổng hiển thị ở FE
        recalculateTotalDisplay();
    }

    // Tính lại số tiền hiển thị ở FE theo khuyến mãi (không gọi API)
    function recalculateTotalDisplay() {
        // Hàm parse số tiền đang hiển thị ("100.000đ" => 100000)
        function parseDisplayedTotal() {
            const el = document.getElementById('totalPrice') || document.getElementById('ticketTotal');
            if (!el) return 0;
            const text = (el.textContent || '').replace(/[^0-9]/g, '');
            return Number(text) || 0;
        }

        // Nếu originalTotal chưa được set, lấy từ DOM
        if (!originalTotal || originalTotal === 0) {
            originalTotal = parseDisplayedTotal();
        }

        let total = originalTotal;
        if (selectedPromotion && selectedPromotion.discountPercent) {
            total = Math.max(0, Math.round(originalTotal * (1 - selectedPromotion.discountPercent / 100)));
        }
        const formatted = Number(total).toLocaleString('vi-VN') + '₫';
        const totalPriceEl = document.getElementById('totalPrice');
        const ticketTotalEl = document.getElementById('ticketTotal');
        if (totalPriceEl) totalPriceEl.textContent = formatted;
        if (ticketTotalEl) ticketTotalEl.textContent = formatted;
    }

    // Hàm áp dụng dữ liệu tổng giá từ server vào FE và cập nhật originalTotal
    function applySummaryData(data) {
        if (!data) return;
        // Ưu tiên các trường tổng tiền từ server
        const totalServer = data.totalAmount || data.totalPrice || data.total || data.finalPrice || 0;
        // Chỉ cập nhật originalTotal khi chưa áp dụng khuyến mãi nào
        if (!selectedPromotion) {
            originalTotal = Number(totalServer) || 0;
        }
        // Sau khi có originalTotal, hiển thị theo khuyến mãi hiện tại (nếu có)
        recalculateTotalDisplay();
    }

    async function updateSummaryWithPromotion() {
        try {
            const summaryResult = await getSeatSummary();
            if (
                summaryResult &&
                summaryResult.value &&
                summaryResult.value.code === 200 &&
                summaryResult.value.data
            ) {
                applySummaryData(summaryResult.value.data);
                // đồng bộ giá trong SeatModule (nếu cần)
            }
        } catch (e) {
            console.error('updateSummaryWithPromotion error:', e);
        }
    }

    // Export module
    window.PaymentModule = {
        init: initPaymentModule,
        validateAndContinue: validateAndContinue,
        callVnpayPayment: callVnpayPayment
    };

})(window);