// Seat logic module với SignalR hoạt động đúng
(function (window) {
    // Biến toàn cục liên quan seat
    let selectedSeats = [];
    let maxSeats = 8;
    let allSeats = [];
    let heldSeatLogIds = [];
    let seatMapContainer = null;
    let selectedSeatsDisplay = null;
    let totalPriceElement = null;
    let continueBtn = null;
    let showTimeId = null;
    let connection = null;

    // Hàm khởi tạo SignalR connection
    function initSignalRConnection() {
        if (connection) {
            connection.stop();
        }

        connection = new signalR.HubConnectionBuilder()
            .withUrl(`https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/seatHub?showTimeId=${showTimeId}`, {
                accessTokenFactory: () => getAuthToken()
            })
            .withAutomaticReconnect()
            .build();

        // Xử lý khi người khác chọn ghế
        connection.on("SeatSelected", function(seatId, userId) {
            console.log(`Seat ${seatId} selected by user ${userId}`);
            const currentUserId = getCurrentUserId();
            
            // Chỉ cập nhật nếu không phải user hiện tại
            if (userId !== currentUserId) {
                markSeatAsPendingByOthers(seatId, userId);
            }
        });

        // Xử lý khi người khác bỏ chọn ghế
        connection.on("SeatDeselected", function(seatId, userId) {
            console.log(`Seat ${seatId} deselected by user ${userId}`);
            const currentUserId = getCurrentUserId();
            
            // Chỉ cập nhật nếu không phải user hiện tại
            if (userId !== currentUserId) {
                markSeatAsAvailable(seatId);
            }
        });

        // Xử lý kết nối thành công
        connection.start().then(function() {
            console.log("SignalR connected successfully!");
        }).catch(function(err) {
            console.error("SignalR connection error: ", err);
        });

        // Xử lý mất kết nối
        connection.onclose(function(error) {
            console.log("SignalR connection closed", error);
        });

        // Xử lý khi reconnect
        connection.onreconnected(function(connectionId) {
            console.log("SignalR reconnected with connectionId: ", connectionId);
        });
    }

    // Đánh dấu ghế đang được người khác giữ
    function markSeatAsPendingByOthers(seatId, userId) {
        const seatElement = document.querySelector(`[data-seat-id="${seatId}"]`);
        if (seatElement) {
            // Xóa các class cũ
            seatElement.classList.remove('seat-available', 'seat-selected', 'pending-mine');
            
            // Thêm class mới
            seatElement.classList.add('pending-others');
            seatElement.disabled = true;
            seatElement.style.cursor = 'not-allowed';
            
            // Thêm tooltip đã bị xóa để tránh hiển thị tooltip
            // seatElement.title = `Ghế đang được giữ bởi người dùng khác`;
            
            // Cập nhật màu sắc - để CSS xử lý
            seatElement.style.backgroundColor = '';
            seatElement.style.background = '';
            seatElement.style.color = '';
        }
    }

    // Đánh dấu ghế available
    function markSeatAsAvailable(seatId) {
        const seatElement = document.querySelector(`[data-seat-id="${seatId}"]`);
        if (seatElement) {
            // Xóa các class cũ
            seatElement.classList.remove('pending-others', 'seat-selected', 'pending-mine');
            
            // Thêm class available
            seatElement.classList.add('seat-available');
            seatElement.disabled = false;
            seatElement.style.cursor = 'pointer';
            
            // Reset style
            seatElement.style.backgroundColor = '';
            seatElement.style.color = '';
            
            // Cập nhật tooltip đã bị xóa để tránh hiển thị tooltip
            // const seatCode = seatElement.dataset.seatCode;
            // const seatPrice = seatElement.dataset.seatPrice;
            // seatElement.title = `Ghế ${seatCode} - ${formatPrice(seatPrice)} VNĐ`;
        }
    }

    // Gửi thông báo SelectSeat qua SignalR
    async function notifySelectSeat(seatId) {
        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            try {
                await connection.invoke("SelectSeat", showTimeId, seatId, getCurrentUserId());
                console.log(`Notified seat selection: ${seatId}`);
            } catch (error) {
                console.error("Error notifying seat selection:", error);
            }
        }
    }

    // Gửi thông báo DeselectSeat qua SignalR
    async function notifyDeselectSeat(seatId) {
        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            try {
                await connection.invoke("DeselectSeat", showTimeId, seatId, getCurrentUserId());
                console.log(`Notified seat deselection: ${seatId}`);
            } catch (error) {
                console.error("Error notifying seat deselection:", error);
            }
        }
    }

    // Hàm quản lý SeatLogId
    function addSeatLogId(seatLogId) {
        if (seatLogId && !heldSeatLogIds.includes(seatLogId)) {
            heldSeatLogIds.push(seatLogId);
        }
    }

    function removeSeatLogId(seatLogId) {
        const index = heldSeatLogIds.indexOf(seatLogId);
        if (index > -1) heldSeatLogIds.splice(index, 1);
    }

    function getAllHeldSeatLogIds() {
        return [...heldSeatLogIds];
    }

    function clearAllSeatLogIds() {
        heldSeatLogIds = [];
    }

    // Hàm loadSeats
    async function loadSeats() {
        try {
            seatMapContainer.innerHTML = `<div id="loadingSeats" style="text-align: center; padding: 2rem; color: rgba(255,255,255,0.7);"><i class="fas fa-spinner fa-spin"></i> Đang tải ghế...</div>`;
            
            if (!showTimeId) {
                seatMapContainer.innerHTML = `<div style='color:red;text-align:center'>Không tìm thấy showTimeId!</div>`;
                return;
            }

            const response = await fetch(`https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/booking-ticket/available?showTimeId=${showTimeId}`, {
                headers: { 'Authorization': `Bearer ${getAuthToken()}` }
            });

            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

            const data = await response.json();
            if (data.value && data.value.code === 200 && data.value.data && data.value.data.seats) {
                const payload = data.value.data;
                allSeats = payload.seats;
                renderSeats(payload.seats);
                if (payload.roomName) updateRoomName(payload.roomName);
            } else if (data.data && data.data.seats) { // Trường hợp cũ
                allSeats = data.data.seats;
                renderSeats(data.data.seats);
                if (data.data.roomName) updateRoomName(data.data.roomName);
            } else {
                throw new Error(data.message || 'Không có ghế nào trong phòng chiếu này');
            }
        } catch (error) {
            seatMapContainer.innerHTML = `<div style="text-align: center; padding: 2rem; color: #ff6b6b;"><i class="fas fa-exclamation-triangle"></i>${error.message || 'Không thể tải danh sách ghế. Vui lòng thử lại!'}<br><br><button onclick="window.SeatModule.loadSeats()" class="btn btn-primary" style="padding: 0.5rem 1rem;">Thử lại</button></div>`;
        }
    }

    // Render ghế từ dữ liệu API
    function renderSeats(seats) {
        const loadingElement = document.getElementById('loadingSeats');
        if (loadingElement) loadingElement.remove();
        
        updateSeatCount(seats.length);
        
        const seatsByRow = {};
        seats.forEach(seat => {
            const rowKey = seat.rowIndex || seat.row || 1;
            if (!seatsByRow[rowKey]) seatsByRow[rowKey] = [];
            seatsByRow[rowKey].push(seat);
        });

        let html = '';
        const sortedRows = Object.keys(seatsByRow).sort((a, b) => parseInt(a) - parseInt(b));
        
        if (sortedRows.length === 0) {
            html = `<div style="text-align: center; padding: 2rem; color: #ff6b6b;"><i class="fas fa-exclamation-triangle"></i>Không có ghế nào được tìm thấy!<br><br><button onclick="window.SeatModule.loadSeats()" class="btn btn-primary" style="padding: 0.5rem 1rem;">Thử lại</button></div>`;
        } else {
            sortedRows.forEach(rowIndex => {
                const rowSeats = seatsByRow[rowIndex].sort((a, b) => {
                    const colA = a.columnIndex || a.column || 1;
                    const colB = b.columnIndex || b.column || 1;
                    return colA - colB;
                });
                
                const rowLabel = String.fromCharCode(64 + parseInt(rowIndex));
                html += `<div class="seat-row"><span class="row-label">${rowLabel}</span><div class="seats-container">`;
                
                rowSeats.forEach(seat => {
                    const seatClass = getSeatClass(seat);
                    const isDisabled = seat.status ? seat.status.toLowerCase() !== 'available' : (seat.isAvailable === false);
                    const seatCode = seat.seatCode || seat.code || `${rowLabel}${seat.columnIndex || seat.column || '?'}`;
                    const seatPrice = seat.price || 0;
                    
                    html += `<button type="button" class="${seatClass}" data-seat-id="${seat.id}" data-seat-code="${seatCode}" data-seat-price="${seatPrice}" data-seat-type="${seat.seatType || 'regular'}" ${isDisabled ? 'disabled' : ''}></button>`;
                });
                
                html += `</div><span class="row-label">${rowLabel}</span></div>`;
            });
        }
        
        seatMapContainer.innerHTML = html;
        attachSeatEventListeners();
        updateSeatStatistics(seats);
    }

    // Các hàm utility khác (giữ nguyên)
    function updateSeatCount(totalSeats) {
        const seatCountElement = document.querySelector('.seat-count');
        if (seatCountElement) seatCountElement.textContent = `${totalSeats} Seats Total`;
    }

    function updateSeatStatistics(seats) {
        const availableSeats = seats.filter(seat => seat.isAvailable).length;
        const occupiedSeats = seats.filter(seat => !seat.isAvailable).length;
        const totalSeats = seats.length;
        const seatTypes = [...new Set(seats.map(seat => seat.seatType || 'Regular'))];
        
        const seatInfo = document.getElementById('seatInfo');
        if (seatInfo) {
            let seatTypesHtml = '';
            seatTypes.forEach(type => {
                const displayName = getSeatTypeDisplayName(type);
                seatTypesHtml += `<span class="seat-type">${displayName}</span>`;
            });
            seatInfo.innerHTML = `<span class="seat-count">Tổng: ${totalSeats} ghế</span><span class="seat-available">Trống: ${availableSeats}</span><span class="seat-occupied">Đã đặt: ${occupiedSeats}</span><div class="seat-types">${seatTypesHtml}</div>`;
        }
    }

    function getSeatTypeDisplayName(seatType) {
        if (typeof seatType === 'number') {
            const typeMapNum = { 1: 'Thường', 2: 'VIP', 3: 'Couple' };
            return typeMapNum[seatType] || 'Thường';
        }
        if (typeof seatType !== 'string') return seatType || '';
        const typeMap = {
            'regular': 'Thường',
            'normal': 'Thường',
            'vip': 'VIP',
            'comfort': 'Comfort',
            'doublecomfort': 'Double Comfort',
            'double-comfort': 'Double Comfort',
            'couple': 'Couple',
            'fordisabilities': 'Người khuyết tật',
            'for-disabilities': 'Người khuyết tật'
        };
        return typeMap[seatType.toLowerCase()] || seatType;
    }

    function getSeatClass(seat) {
        let baseClass = 'seat';
        
        if (seat.status) {
            switch (seat.status.toLowerCase()) {
                case 'available':
                    return baseClass + ' seat-available';
                case 'pending':
                    return baseClass + ' seat-pending';
                case 'selected':
                case 'occupied':
                    return baseClass + ' seat-unavailable';
            }
        }
        if (seat.hasOwnProperty('isAvailable')) {
            if (seat.isAvailable) {
                // ghế trống
            } else {
                return baseClass + ' seat-unavailable';
            }
        }
        
        let seatType = seat.seatType;
        if (typeof seatType === 'number') {
            const typeMapNum = { 1: 'regular', 2: 'vip', 3: 'couple' };
            seatType = typeMapNum[seatType] || 'regular';
        }
        if (typeof seatType !== 'string') seatType = 'regular';
        
        switch (seatType.toLowerCase()) {
            case 'vip': return baseClass + ' vip available';
            case 'comfort': return baseClass + ' comfort available';
            case 'doublecomfort':
            case 'double-comfort': return baseClass + ' double-comfort available';
            case 'couple': return baseClass + ' couple available';
            case 'fordisabilities':
            case 'for-disabilities': return baseClass + ' for-disabilities available';
            case 'regular':
            case 'normal':
            default: return baseClass + ' regular available';
        }
    }

    function formatPrice(price) {
        return new Intl.NumberFormat('vi-VN').format(price);
    }

    function getCurrentUserId() {
        return sessionStorage.getItem('userId') || localStorage.getItem('userId');
    }

    function getAuthToken() {
        return sessionStorage.getItem('jwtToken') || localStorage.getItem('jwtToken') ||
               localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
    }

    // Hàm hold seat
    async function holdSeat(seatIds) {
        try {
            let seatIdPayload = seatIds;
            if (Array.isArray(seatIds)) seatIdPayload = seatIds[0];
            
            const requestData = { seatId: seatIdPayload, showTimeId: showTimeId };
            
            const response = await fetch('https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/seatsignal/hold', {
                method: 'POST',
                headers: { 
                    'Content-Type': 'application/json', 
                    'Authorization': `Bearer ${getAuthToken()}` 
                },
                body: JSON.stringify(requestData)
            });
            
            const result = await response.json();
            if (response.ok && result.code === 200) {
                if (result.data && result.data.seatLogId) addSeatLogId(result.data.seatLogId);
                return { success: true, data: result.data };
            } else {
                return { success: false, message: result.message || 'Không thể giữ ghế' };
            }
        } catch (error) {
            return { success: false, message: 'Có lỗi xảy ra khi giữ ghế' };
        }
    }

    // Hàm release seat
    async function releaseSeat(seatLogId) {
        try {
            const response = await fetch(`https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/seatsignal/release/${seatLogId}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${getAuthToken()}` }
            });
            
            const result = await response.json();
            if (response.ok && result.code === 200) {
                removeSeatLogId(seatLogId);
                return { success: true };
            } else {
                return { success: false, message: result.message };
            }
        } catch (error) {
            return { success: false, message: 'Có lỗi xảy ra khi bỏ giữ ghế' };
        }
    }

    // Cập nhật hiển thị ghế đã chọn và tổng tiền
    function updateDisplay() {
        if (selectedSeats.length === 0) {
            selectedSeatsDisplay.innerHTML = '<div style="text-align: center; color: rgba(255,255,255,0.6); padding: 1rem;">Chưa chọn ghế nào</div>';
        } else {
            selectedSeatsDisplay.innerHTML = selectedSeats.map(seat => 
                `<div class="selected-seat-item" style="background: rgba(255,255,255,0.2); padding: 0.3rem 0.6rem; border-radius: 5px; font-size: 0.8rem;">${seat.code}</div>`
            ).join('');
        }
        
        const total = selectedSeats.reduce((sum, seat) => sum + seat.price, 0);
        totalPriceElement.textContent = formatPrice(total) + '₫';
        
        const ticketTotalElement = document.getElementById('ticketTotal');
        if (ticketTotalElement) ticketTotalElement.textContent = formatPrice(total) + '₫';
        
        const ticketCountElement = document.getElementById('ticketCount');
        if (ticketCountElement) ticketCountElement.textContent = selectedSeats.length;
        
        const seatCountInHeader = document.querySelector('.seat-count');
        if (seatCountInHeader && allSeats.length > 0) {
            seatCountInHeader.textContent = `${selectedSeats.length}/${allSeats.length} Seats Selected`;
        }
        
        // Cập nhật hiển thị ghế đã chọn trong header nếu có
        const selectedSeatsHeader = document.getElementById('selectedSeatsHeader');
        if (selectedSeatsHeader) {
            if (selectedSeats.length === 0) {
                selectedSeatsHeader.style.display = 'none';
            } else {
                selectedSeatsHeader.style.display = 'block';
                selectedSeatsHeader.innerHTML = selectedSeats.map(seat => 
                    `<span class="selected-seat-badge">${seat.code}</span>`
                ).join('');
            }
        }
        
        continueBtn.disabled = selectedSeats.length === 0;
    }

    // Gắn event listener cho ghế với SignalR integration
    function attachSeatEventListeners() {
        const seats = document.querySelectorAll('.seat');
        seats.forEach(seat => {
            if (seat.classList.contains('seat-unavailable')) {
                seat.style.cursor = 'not-allowed';
                return;
            }
            
            seat.addEventListener('click', async function () {
                const seatId = this.dataset.seatId;
                const seatCode = this.dataset.seatCode;
                const seatPrice = parseFloat(this.dataset.seatPrice);
                const seatType = this.dataset.seatType;
                
                // Kiểm tra nếu ghế đang được người khác giữ
                if (this.classList.contains('pending-others')) {
                    alert('Ghế này đang được người khác giữ. Vui lòng chọn ghế khác!');
                    return;
                }
                
                // Nếu ghế đang được chọn bởi user hiện tại
                if (this.classList.contains('selected') || this.classList.contains('pending-mine')) {
                    const seatData = selectedSeats.find(s => s.id === seatId);
                    if (seatData && seatData.seatLogId) {
                        this.style.opacity = '0.5';
                        this.style.pointerEvents = 'none';
                        
                        const releaseResult = await releaseSeat(seatData.seatLogId);
                        
                        // Gửi thông báo deselect qua SignalR
                        await notifyDeselectSeat(seatId);
                        
                        this.classList.remove('selected', 'pending-mine');
                        selectedSeats = selectedSeats.filter(s => s.id !== seatId);
                        
                        this.style.opacity = '1';
                        this.style.pointerEvents = 'auto';
                    } else {
                        this.classList.remove('selected', 'pending-mine');
                        selectedSeats = selectedSeats.filter(s => s.id !== seatId);
                        
                        // Gửi thông báo deselect qua SignalR
                        await notifyDeselectSeat(seatId);
                    }
                } else {
                    // Chọn ghế mới
                    if (selectedSeats.length < maxSeats) {
                        this.style.opacity = '0.5';
                        this.style.pointerEvents = 'none';
                        
                        const holdResult = await holdSeat([seatId]);
                        if (holdResult.success) {
                            this.classList.add('selected', 'pending-mine');
                            // Đảm bảo không có background inline style
                            this.style.backgroundColor = '';
                            this.style.background = '';
                            
                            selectedSeats.push({ 
                                id: seatId, 
                                code: seatCode, 
                                price: seatPrice, 
                                type: seatType, 
                                seatLogId: holdResult.data?.seatLogId 
                            });
                            
                            // Gửi thông báo select qua SignalR
                            await notifySelectSeat(seatId);
                        } else {
                            alert(holdResult.message);
                        }
                        
                        this.style.opacity = '1';
                        this.style.pointerEvents = 'auto';
                    } else {
                        alert(`Bạn chỉ có thể chọn tối đa ${maxSeats} ghế.`);
                    }
                }
                
                updateDisplay();
                updateContinueButtonState();
            });
        });
    }

    function updateContinueButtonState() {
        if (continueBtn) {
            if (selectedSeats.length > 0) {
                continueBtn.disabled = false;
                continueBtn.textContent = `THANH TOÁN ${selectedSeats.length} GHẾ`;
                continueBtn.style.display = 'block';
                const payBtn = document.getElementById('payVnpayBtn');
                if (payBtn && payBtn.dataset.manualVisible !== 'true') {
                    payBtn.style.display = 'none'; // chỉ hiện sau khi summary thành công
                }
            } else {
                continueBtn.disabled = true;
                continueBtn.textContent = 'CHỌN GHẾ ĐỂ THANH TOÁN';
                continueBtn.style.display = 'block';
                const payBtn = document.getElementById('payVnpayBtn');
                if (payBtn) payBtn.style.display = 'none';
            }
        }
    }

    function updateRoomName(roomName) {
        const roomElement = document.getElementById('cinemaRoom');
        if (roomElement && roomName) {
            roomElement.textContent = roomName;
        }
        
        const sectionTitle = document.querySelector('.section-title');
        if (sectionTitle && roomName) {
            sectionTitle.textContent = `Select Your Seats - ${roomName}`;
        }
    }

    // Hàm khởi tạo module
    function initSeatModule(options) {
        seatMapContainer = options.seatMapContainer;
        selectedSeatsDisplay = options.selectedSeatsDisplay;
        totalPriceElement = options.totalPriceElement;
        continueBtn = options.continueBtn;
        showTimeId = options.showTimeId;
        if (options.roomId) window.SeatModule.roomId = options.roomId;
        
        // Khởi tạo SignalR connection
        initSignalRConnection();
    }

    // Cleanup khi trang bị đóng
    function cleanup() {
        if (connection) {
            connection.stop();
        }
    }

    // Expose các hàm cần thiết ra window.SeatModule
    window.SeatModule = {
        init: initSeatModule,
        loadSeats: loadSeats,
        updateDisplay: updateDisplay,
        updateContinueButtonState: updateContinueButtonState,
        getAllHeldSeatLogIds: getAllHeldSeatLogIds,
        clearAllSeatLogIds: clearAllSeatLogIds,
        selectedSeats: () => selectedSeats,
        allSeats: () => allSeats,
        cleanup: cleanup
    };

    // Khi unload, gửi release cho tất cả ghế đang hold
    window.addEventListener('beforeunload', async function () {
        if (heldSeatLogIds.length === 0) return;
        heldSeatLogIds.forEach(id => {
            if (navigator.sendBeacon) {
                navigator.sendBeacon(`https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/seatsignal/release/${id}`);
            } else {
                fetch(`https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/seatsignal/release/${id}`, { method: 'DELETE', keepalive: true });
            }
        });
        cleanup();
    });
    
})(window);