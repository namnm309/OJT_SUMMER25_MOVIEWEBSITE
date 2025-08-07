

        // Helper function để lấy JWT token
        function getAuthToken() {
            try {
                // Thử lấy từ session storage trước
                let token = sessionStorage.getItem('JWToken') || '';
                if (!token) {
                    // Nếu không có, thử lấy từ cookie
                    const cookies = document.cookie.split(';');
                    const tokenCookie = cookies.find(cookie => cookie.trim().startsWith('JWToken='));
                    if (tokenCookie) {
                        token = tokenCookie.split('=')[1];
                    }
                }
                return token;
            } catch (e) {
                console.warn('Could not get auth token:', e);
                return '';
            }
        }

        // Helper function để tạo headers với auth
        function createAuthHeaders() {
            const headers = {
                'X-Requested-With': 'XMLHttpRequest',
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            };

            const authToken = getAuthToken();
            if (authToken) {
                headers['Authorization'] = `Bearer ${authToken}`;
            }

            return headers;
        }

        let searchTimeout;
        const searchInput = document.getElementById('cinemaSearch');
        
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                const searchTerm = this.value.trim();
                const baseUrl = '@Url.Action("Index")';
                window.location.href = baseUrl + '?search=' + encodeURIComponent(searchTerm);
            }, 500);
        });


        function confirmDelete(roomId, roomName) {
            document.getElementById('roomNameToDelete').textContent = roomName;
            document.getElementById('deleteModal').style.display = 'block';
            
            // Lưu roomId để dùng khi submit
            document.getElementById('deleteModal').setAttribute('data-room-id', roomId);
        }

        async function submitDeleteForm() {
            const roomId = document.getElementById('deleteModal').getAttribute('data-room-id');
            const submitBtn = document.getElementById('deleteBtn');
            const originalText = submitBtn.innerHTML;
            
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Đang xóa...';
            
            try {
                const response = await fetch(`https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/cinemaroom/Delete/${roomId}`, {
                    method: 'DELETE',
                    headers: createAuthHeaders()
                });
                
                const responseText = await response.text();
                let result;
                try {
                    result = JSON.parse(responseText);
                } catch (e) {
                    throw new Error(`Invalid JSON response: ${responseText}`);
                }
                
                if (response.ok && (result.Code === 200 || result.code === 200)) {
                    closeDeleteModal();
                    
                    const successAlert = document.createElement('div');
                    successAlert.className = 'alert alert-success';
                    successAlert.innerHTML = `
                        <i class="fas fa-check-circle me-2"></i>
                        <span>${result.Message || result.message || 'Xóa phòng chiếu thành công!'}</span>
                        <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                            <i class="fas fa-times"></i>
                        </button>
                    `;
                    document.querySelector('.page-header').after(successAlert);
                    
                    setTimeout(() => window.location.reload(), 1500);
                } else {
                    throw new Error(result.error || result.message || result.Message || 'Có lỗi xảy ra khi xóa phòng chiếu');
                }
            } catch (error) {
                console.error('Error deleting room:', error);
                
                const errorAlert = document.createElement('div');
                errorAlert.className = 'alert alert-danger';
                errorAlert.innerHTML = `
                    <i class="fas fa-exclamation-circle me-2"></i>
                    <div>${error.message}</div>
                    <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                        <i class="fas fa-times"></i>
                    </button>
                `;
                document.querySelector('.page-header').after(errorAlert);
                
                setTimeout(() => errorAlert.remove(), 5000);
            } finally {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
        }

        function closeDeleteModal() {
            document.getElementById('deleteModal').style.display = 'none';
        }


        document.getElementById('deleteModal').addEventListener('click', function(e) {
            if (e.target === this) {
                closeDeleteModal();
            }
        });


        setTimeout(function() {
            const alerts = document.querySelectorAll('.alert');
            alerts.forEach(function(alert) {
                alert.style.animation = 'slideDown 0.3s ease reverse';
                setTimeout(() => alert.remove(), 300);
            });
        }, 5000);


        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                this.blur();
            }
        });


        function openCreateModal() {
            document.getElementById('createModal').style.display = 'block';
            setTimeout(() => document.getElementById('roomName').focus(), 100);
            
            // Thêm event listeners cho các input số hàng và số cột
            const rowsInput = document.getElementById('numberOfRows');
            const colsInput = document.getElementById('numberOfColumns');
            
            if (rowsInput) {
                rowsInput.addEventListener('input', calculateTotalSeats);
                rowsInput.addEventListener('change', calculateTotalSeats);
            }
            
            if (colsInput) {
                colsInput.addEventListener('input', calculateTotalSeats);
                colsInput.addEventListener('change', calculateTotalSeats);
            }
        }

        function closeCreateModal() {
            document.getElementById('createModal').style.display = 'none';
            document.getElementById('createRoomForm').reset();
            document.getElementById('layoutPreview').style.display = 'none';
        }

        function calculateTotalSeats() {
            const rows = parseInt(document.getElementById('numberOfRows').value) || 0;
            const cols = parseInt(document.getElementById('numberOfColumns').value) || 0;
            const total = rows * cols;
            
            document.getElementById('totalSeats').value = total;
            
            if (rows > 0 && cols > 0) {
                showLayoutPreview(rows, cols);
            } else {
                document.getElementById('layoutPreview').style.display = 'none';
            }
        }

        function showLayoutPreview(rows, cols) {
            const preview = document.getElementById('layoutPreview');
            const grid = document.getElementById('seatPreviewGrid');
            
            grid.style.gridTemplateColumns = `repeat(${cols}, 1fr)`;
            grid.innerHTML = '';
            
            for (let r = 1; r <= rows; r++) {
                for (let c = 1; c <= cols; c++) {
                    const seat = document.createElement('div');
                    seat.className = 'seat-preview';
                    grid.appendChild(seat);
                }
            }
            
            preview.style.display = 'block';
        }

        async function submitCreateForm(event) {
            event.preventDefault();
            
            const submitBtn = document.getElementById('submitBtn');
            const originalText = submitBtn.innerHTML;
            

            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Đang tạo...';
            
            try {
                // Lấy dữ liệu từ form
                const roomName = event.target.RoomName.value;
                const totalSeats = parseInt(event.target.TotalSeats.value) || 0;
                const numberOfRows = parseInt(event.target.NumberOfRows.value) || 0;
                const numberOfColumns = parseInt(event.target.NumberOfColumns.value) || 0;

                // Validation
                if (!roomName || roomName.trim() === '') {
                    throw new Error('Tên phòng chiếu không được để trống');
                }

                if (numberOfRows <= 0 || numberOfRows > 50) {
                    throw new Error('Số hàng phải từ 1 đến 50');
                }

                if (numberOfColumns <= 0 || numberOfColumns > 50) {
                    throw new Error('Số cột phải từ 1 đến 50');
                }

                if (totalSeats <= 0) {
                    throw new Error('Tổng số ghế phải lớn hơn 0');
                }

                // Kiểm tra xem số ghế có khớp với layout không
                const calculatedSeats = numberOfRows * numberOfColumns;
                if (calculatedSeats !== totalSeats) {
                    throw new Error(`Số ghế không khớp: ${numberOfRows} hàng x ${numberOfColumns} cột = ${calculatedSeats}, nhưng tổng số ghế = ${totalSeats}`);
                }

                // Debug log
                console.log('Form data being sent:');
                console.log('RoomName:', roomName);
                console.log('TotalSeats:', totalSeats);
                console.log('NumberOfRows:', numberOfRows);
                console.log('NumberOfColumns:', numberOfColumns);

                // Tạo object dữ liệu
                const requestData = {
                    RoomName: roomName,
                    TotalSeats: totalSeats,
                    NumberOfRows: numberOfRows,
                    NumberOfColumns: numberOfColumns,
                    DefaultSeatPrice: 100000 // Giá ghế mặc định
                };

                console.log('Request data object:', requestData);
                console.log('JSON string:', JSON.stringify(requestData));

                // Thêm CSRF token vào headers
                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                const headers = {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest',
                    'Accept': 'application/json'
                };
                
                // API call không cần CSRF token
                // if (token) {
                //     headers['RequestVerificationToken'] = token;
                //     console.log('CSRF token found and added to headers');
                // } else {
                //     console.warn('CSRF token not found');
                // }

                const response = await fetch('https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/cinemaroom/Add', {
                    method: 'POST',
                    headers: headers,
                    body: JSON.stringify(requestData)
                });
                
                console.log('Response status:', response.status);
                console.log('Response headers:', response.headers);
                
                const responseText = await response.text();
                console.log('Response text:', responseText);
                
                let result;
                try {
                    result = JSON.parse(responseText);
                } catch (e) {
                    console.error('Failed to parse JSON response:', e);
                    throw new Error(`Invalid JSON response: ${responseText}`);
                }
                
                console.log('Response data:', result);
                console.log('Response Code:', result.Code);
                console.log('Response code:', result.code);
                console.log('Response success:', result.success);
                console.log('Response Message:', result.Message);
                console.log('Response message:', result.message);
                
                if (response.ok) {
                    if (result.Code === 200 || result.code === 200 || result.success) {
                        closeCreateModal();
                        

                        const successAlert = document.createElement('div');
                        successAlert.className = 'alert alert-success';
                        successAlert.innerHTML = `
                            <i class="fas fa-check-circle me-2"></i>
                            <span>${result.Message || result.message || 'Thêm phòng chiếu thành công!'}</span>
                            <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                                <i class="fas fa-times"></i>
                            </button>
                        `;
                        document.querySelector('.page-header').after(successAlert);
                        

                        setTimeout(() => window.location.reload(), 1500);
                    } else {

                        let errorMessage = result.message || result.Message || result.error || 'Có lỗi xảy ra';
                        if (result.errors) {
                            const errorDetails = Object.entries(result.errors)
                                .map(([key, messages]) => `${key}: ${Array.isArray(messages) ? messages.join(', ') : messages}`)
                                .join('<br>');
                            errorMessage += '<br><small>' + errorDetails + '</small>';
                        }
                        throw new Error(errorMessage);
                    }
                } else {
                    // Xử lý error response từ API
                    if (result.error) {
                        throw new Error(result.error);
                    } else {
                        throw new Error(`Server error: ${response.status} - ${response.statusText}`);
                    }
                }
            } catch (error) {
                console.error('Error creating room:', error);
                

                const errorAlert = document.createElement('div');
                errorAlert.className = 'alert alert-danger';
                errorAlert.innerHTML = `
                    <i class="fas fa-exclamation-circle me-2"></i>
                    <div>${error.message || 'Có lỗi xảy ra khi tạo phòng chiếu'}</div>
                    <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                        <i class="fas fa-times"></i>
                    </button>
                `;
                document.querySelector('.modal-body').prepend(errorAlert);
                

                setTimeout(() => errorAlert.remove(), 5000);
            } finally {

                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
        }


        document.getElementById('createModal').addEventListener('click', function(e) {
            if (e.target === this) {
                closeCreateModal();
            }
        });


        async function openDetailsModal(roomId) {
            console.log(`Opening details modal for room ID: ${roomId}`); // Debug log
            const modal = document.getElementById('detailsModal');
            const loading = document.getElementById('detailsLoading');
            const content = document.getElementById('detailsContent');
            

            modal.style.display = 'block';
            loading.style.display = 'flex';
            content.style.display = 'none';
            
            try {

                const detailsUrl = `https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/cinemaroom/ViewSeat?Id=${roomId}`;
                console.log('Fetching from URL:', detailsUrl); // Debug log
                
                const response = await fetch(detailsUrl, {
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest',
                        'Accept': 'application/json'
                    }
                });
                
                console.log('Response status:', response.status); // Debug log
                
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                
                const data = await response.json();
                console.log('API Response:', data); // Debug log
                

                console.log('Checking response format...');
                

                if (data.Code !== undefined) {
                    console.log('Backend format detected (Code, Message, Data)');
                    if (data.Code === 200 && data.Data) {
                        console.log('Room data:', data.Data); // Debug log
                        renderDetailsContent(data.Data);
                    } else {
                        console.error('Error in backend response:', data.Message);
                        throw new Error(data.Message || 'Không thể tải thông tin phòng chiếu');
                    }
                } 

                else if (data.code !== undefined) {
                    console.log('API format detected (code, message, data)');
                    if (data.code === 200 && data.data) {
                        console.log('Room data:', data.data); // Debug log
                        renderDetailsContent(data.data);
                    } else {
                        console.error('Error in API response:', data.message);
                        throw new Error(data.message || 'Không thể tải thông tin phòng chiếu');
                    }
                }

                else {
                    console.log('Raw data format detected');
                    console.log('Room data:', data); // Debug log
                    renderDetailsContent(data);
                }
            } catch (error) {
                console.error('Error loading room details:', error);
                content.innerHTML = `
                    <div class="alert alert-danger">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        <strong>Lỗi:</strong> ${error.message}
                    </div>
                `;
            } finally {
                loading.style.display = 'none';
                content.style.display = 'block';
            }
        }

        function renderDetailsContent(roomData) {
            const content = document.getElementById('detailsContent');
            console.log('Rendering room data:', roomData); // Debug log
            

            if (roomData.data) roomData = roomData.data;
            if (roomData.Data) roomData = roomData.Data;
            

            let seats = [];
            if (Array.isArray(roomData)) {

                seats = roomData;
                console.log('Data is an array, assuming it contains seats');
            } else {
                seats = roomData.seats || roomData.Seats || [];
            }
            console.log('Seats data:', seats); // Debug log
            

            const roomName = roomData.roomName || roomData.RoomName || 'N/A';
            const roomId = roomData.roomId || roomData.RoomId || '';
            const totalSeats = roomData.totalSeats || roomData.TotalSeats || seats.length || 0;
            

            if (!seats || !Array.isArray(seats) || seats.length === 0) {
                content.innerHTML = `
                    <div class="alert alert-warning">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        <strong>Thông báo:</strong> Phòng chiếu này chưa có ghế hoặc dữ liệu ghế không hợp lệ.
                    </div>
                    <div style="background: white; border: 1px solid #e5e7eb; border-radius: 12px; margin-bottom: 1.5rem; overflow: hidden;">
                        <div style="padding: 1rem 1.5rem; border-bottom: 1px solid #f3f4f6; background: #f9fafb;">
                            <h4 style="margin: 0; display: flex; align-items: center; gap: 0.5rem; color: #1f2937;">
                                <i class="fas fa-info-circle"></i>
                                Thông tin phòng chiếu
                            </h4>
                        </div>
                        <div style="padding: 1.5rem;">
                            <div style="background: #f9fafb; padding: 1rem; border-radius: 8px; border: 1px solid #e5e7eb;">
                                <div style="font-size: 0.875rem; color: #6b7280; margin-bottom: 0.25rem;">Tên phòng</div>
                                <div style="font-size: 1.125rem; font-weight: 600; color: #1f2937;">${roomName}</div>
                            </div>
                        </div>
                    </div>
                `;
                return;
            }
            

            // Tính toán số lượng ghế theo loại với debug log
            console.log('Calculating seat counts...');
            console.log('Sample seat data:', seats.slice(0, 3)); // Log 3 ghế đầu tiên để debug
            
            const normalSeats = seats.filter(s => {
                const seatType = s.seatType || s.SeatType;
                console.log(`Seat ${s.seatCode || s.SeatCode}: seatType = ${seatType} (type: ${typeof seatType})`);
                return seatType === 0 || seatType === 'Normal' || seatType === 'normal';
            }).length;
            
            const vipSeats = seats.filter(s => {
                const seatType = s.seatType || s.SeatType;
                return seatType === 1 || seatType === 'VIP' || seatType === 'vip';
            }).length;
            
            const coupleSeats = seats.filter(s => {
                const seatType = s.seatType || s.SeatType;
                return seatType === 2 || seatType === 'Couple' || seatType === 'couple';
            }).length;
            
            console.log(`Seat counts - Normal: ${normalSeats}, VIP: ${vipSeats}, Couple: ${coupleSeats}, Total: ${seats.length}`);
            

            const seatsByRow = seats.reduce((acc, seat) => {
                const row = seat.rowIndex || seat.RowIndex;
                if (!acc[row]) acc[row] = [];
                acc[row].push(seat);
                return acc;
            }, {});
            

            const sortedRows = Object.keys(seatsByRow).sort((a, b) => parseInt(a) - parseInt(b));
            

            let seatLayoutHtml = '';
            sortedRows.forEach(rowNum => {
                const rowSeats = seatsByRow[rowNum].sort((a, b) => (a.columnIndex || a.ColumnIndex) - (b.columnIndex || b.ColumnIndex));
                const rowLetter = String.fromCharCode(65 + parseInt(rowNum) - 1); // A, B, C...
                
                seatLayoutHtml += `
                    <div class="seat-row" style="display: flex; align-items: center; gap: 0.5rem; justify-content: center; margin-bottom: 0.25rem;">
                        <div class="row-label" style="width: 30px; text-align: center; font-weight: 600; color: var(--gray-600);">${rowLetter}</div>
                        ${rowSeats.map(seat => {
                            const seatType = seat.seatType || seat.SeatType;
                            const seatCode = seat.seatCode || seat.SeatCode;
                            
                            // Xác định loại ghế và class CSS
                            let seatClass = 'normal';
                            let seatTypeName = 'Thường';
                            
                            if (seatType === 1 || seatType === 'VIP' || seatType === 'vip') {
                                seatClass = 'vip';
                                seatTypeName = 'VIP';
                            } else if (seatType === 2 || seatType === 'Couple' || seatType === 'couple') {
                                seatClass = 'couple';
                                seatTypeName = 'Ghế đôi';
                            }
                            
                            return `
                                <div class="seat ${seatClass}" 
                                     style="width: 28px; height: 28px; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 0.7rem; font-weight: 500; cursor: pointer; transition: all 0.2s ease;">
                                    ${seatCode ? seatCode.substring(1) : ''}
                                </div>
                            `;
                        }).join('')}
                    </div>
                `;
            });
            
            content.innerHTML = `
                <style>
                    .seat.normal { background: #10b981; color: white; }
                    .seat.vip { background: #f59e0b; color: white; }
                    .seat.couple { background: #ef4444; color: white; }
                    .seat:hover { transform: scale(1.1); box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1); }
                    .alert { padding: 1rem; border-radius: 8px; margin-bottom: 1rem; border: 1px solid; }
                    .alert-danger { background: #fee2e2; color: #dc2626; border-color: #dc2626; }
                    .badge { display: inline-flex; align-items: center; gap: 0.25rem; padding: 0.25rem 0.5rem; border-radius: 4px; font-size: 0.75rem; font-weight: 500; }
                    .badge-success { background: #d1fae5; color: #10b981; }
                </style>
                
                
                <div style="background: white; border: 1px solid #e5e7eb; border-radius: 12px; margin-bottom: 1.5rem; overflow: hidden;">
                    <div style="padding: 1rem 1.5rem; border-bottom: 1px solid #f3f4f6; background: #f9fafb;">
                        <h4 style="margin: 0; display: flex; align-items: center; gap: 0.5rem; color: #1f2937;">
                            <i class="fas fa-info-circle"></i>
                            Thông tin phòng chiếu
                        </h4>
                    </div>
                    <div style="padding: 1.5rem;">
                        <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; margin-bottom: 1.5rem;">
                            <div style="background: #f9fafb; padding: 1rem; border-radius: 8px; border: 1px solid #e5e7eb;">
                                <div style="font-size: 0.875rem; color: #6b7280; margin-bottom: 0.25rem;">Tên phòng</div>
                                <div style="font-size: 1.125rem; font-weight: 600; color: #1f2937;">${roomName}</div>
                            </div>
                            <div style="background: #f9fafb; padding: 1rem; border-radius: 8px; border: 1px solid #e5e7eb;">
                                <div style="font-size: 0.875rem; color: #6b7280; margin-bottom: 0.25rem;">Mã phòng</div>
                                <div style="font-size: 1.125rem; font-weight: 600; color: #1f2937;">${roomId.substring(0, 8).toUpperCase()}</div>
                            </div>
                            <div style="background: #f9fafb; padding: 1rem; border-radius: 8px; border: 1px solid #e5e7eb;">
                                <div style="font-size: 0.875rem; color: #6b7280; margin-bottom: 0.25rem;">Tổng số ghế</div>
                                <div style="font-size: 1.125rem; font-weight: 600; color: #1f2937;">${totalSeats} ghế</div>
                            </div>
                            <div style="background: #f9fafb; padding: 1rem; border-radius: 8px; border: 1px solid #e5e7eb;">
                                <div style="font-size: 0.875rem; color: #6b7280; margin-bottom: 0.25rem;">Trạng thái</div>
                                <div style="font-size: 1.125rem; font-weight: 600; color: #1f2937;">
                                    <span class="badge badge-success">
                                        <i class="fas fa-circle"></i>
                                        Hoạt động
                                    </span>
                                </div>
                            </div>
                        </div>
                        
                        
                        <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(120px, 1fr)); gap: 1rem;">
                            <div style="text-align: center; padding: 1rem; background: #f9fafb; border-radius: 8px; border: 1px solid #e5e7eb;">
                                <div style="font-size: 1.5rem; font-weight: 700; color: #10b981;">${normalSeats}</div>
                                <div style="font-size: 0.875rem; color: #6b7280;">Ghế thường</div>
                                <div style="font-size: 0.75rem; color: #9ca3af; margin-top: 0.25rem;">
                                    ${normalSeats > 0 ? '50,000 VNĐ' : 'Chưa có'}
                                </div>
                            </div>
                            <div style="text-align: center; padding: 1rem; background: #f9fafb; border-radius: 8px; border: 1px solid #e5e7eb;">
                                <div style="font-size: 1.5rem; font-weight: 700; color: #f59e0b;">${vipSeats}</div>
                                <div style="font-size: 0.875rem; color: #6b7280;">Ghế VIP</div>
                                <div style="font-size: 0.75rem; color: #9ca3af; margin-top: 0.25rem;">
                                    ${vipSeats > 0 ? '80,000 VNĐ' : 'Chưa có'}
                                </div>
                            </div>
                            <div style="text-align: center; padding: 1rem; background: #f9fafb; border-radius: 8px; border: 1px solid #e5e7eb;">
                                <div style="font-size: 1.5rem; font-weight: 700; color: #ef4444;">${coupleSeats}</div>
                                <div style="font-size: 0.875rem; color: #6b7280;">Ghế đôi</div>
                                <div style="font-size: 0.75rem; color: #9ca3af; margin-top: 0.25rem;">
                                    ${coupleSeats > 0 ? '120,000 VNĐ' : 'Chưa có'}
                                </div>
                            </div>
                            <div style="text-align: center; padding: 1rem; background: #f9fafb; border-radius: 8px; border: 1px solid #e5e7eb;">
                                <div style="font-size: 1.5rem; font-weight: 700; color: #6366f1;">${seats.length}</div>
                                <div style="font-size: 0.875rem; color: #6b7280;">Tổng cộng</div>
                                <div style="font-size: 0.75rem; color: #9ca3af; margin-top: 0.25rem;">
                                    ${seats.length > 0 ? 'Đã cấu hình' : 'Chưa có ghế'}
                                </div>
                            </div>
                        </div>
                        
                        ${normalSeats === 0 && vipSeats === 0 && coupleSeats === 0 && seats.length > 0 ? `
                            <div style="margin-top: 1rem; padding: 1rem; background: #fef3c7; border: 1px solid #f59e0b; border-radius: 8px;">
                                <div style="display: flex; align-items: center; gap: 0.5rem; color: #92400e;">
                                    <i class="fas fa-exclamation-triangle"></i>
                                    <strong>Lưu ý:</strong> Tất cả ghế đều chưa được phân loại. Vui lòng sử dụng chức năng "Quản lý ghế" để cấu hình loại ghế.
                                </div>
                            </div>
                        ` : ''}
                    </div>
                </div>
                
                
                <div style="background: white; border: 1px solid #e5e7eb; border-radius: 12px; overflow: hidden;">
                    <div style="padding: 1rem 1.5rem; border-bottom: 1px solid #f3f4f6; background: #f9fafb;">
                        <h4 style="margin: 0; display: flex; align-items: center; gap: 0.5rem; color: #1f2937;">
                            <i class="fas fa-chair"></i>
                            Sơ đồ ghế ngồi
                        </h4>
                    </div>
                    <div style="padding: 2rem;">
                        
                        <div style="background: linear-gradient(135deg, #1f2937, #4b5563); color: white; text-align: center; padding: 0.75rem; border-radius: 8px; margin-bottom: 2rem; font-weight: 600; position: relative;">
                            <i class="fas fa-tv" style="margin-right: 0.5rem;"></i>
                            MÀN HÌNH
                        </div>
                        
                        
                        <div style="max-width: 800px; margin: 0 auto;">
                            ${seatLayoutHtml}
                        </div>
                        
                        
                        <div style="display: flex; justify-content: center; gap: 2rem; margin-top: 2rem; flex-wrap: wrap;">
                            <div style="display: flex; align-items: center; gap: 0.5rem; font-size: 0.875rem; color: #6b7280;">
                                <div style="width: 20px; height: 20px; border-radius: 4px; background: #10b981;"></div>
                                <span>Ghế thường</span>
                            </div>
                            <div style="display: flex; align-items: center; gap: 0.5rem; font-size: 0.875rem; color: #6b7280;">
                                <div style="width: 20px; height: 20px; border-radius: 4px; background: #f59e0b;"></div>
                                <span>Ghế VIP</span>
                            </div>
                            <div style="display: flex; align-items: center; gap: 0.5rem; font-size: 0.875rem; color: #6b7280;">
                                <div style="width: 20px; height: 20px; border-radius: 4px; background: #ef4444;"></div>
                                <span>Ghế đôi</span>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        }

        function closeDetailsModal() {
            document.getElementById('detailsModal').style.display = 'none';
        }


        document.getElementById('detailsModal').addEventListener('click', function(e) {
            if (e.target === this) {
                closeDetailsModal();
            }
        });


        async function openEditModal(roomId) {
            console.log(`Opening edit modal for room ID: ${roomId}`); // Debug log
            const modal = document.getElementById('editModal');
            const loading = document.getElementById('editLoading');
            const content = document.getElementById('editContent');
            

            modal.style.display = 'block';
            loading.style.display = 'flex';
            content.style.display = 'none';
            
            try {

                const detailsUrl = `https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/cinemaroom/ViewSeat?Id=${roomId}`;
                console.log('Fetching details from URL:', detailsUrl); // Debug log
                
                const detailsResponse = await fetch(detailsUrl, {
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest',
                        'Accept': 'application/json'
                    }
                });
                
                if (!detailsResponse.ok) {
                    throw new Error(`HTTP ${detailsResponse.status}: ${detailsResponse.statusText}`);
                }
                
                const detailsData = await detailsResponse.json();
                console.log('Room details:', detailsData); // Debug log
                

                let roomData;
                if (detailsData.Code !== undefined && detailsData.Data) {
                    roomData = detailsData.Data;
                } else if (detailsData.code !== undefined && detailsData.data) {
                    roomData = detailsData.data;
                } else {
                    roomData = detailsData;
                }
                
                // Tính toán số hàng và số cột từ danh sách ghế nếu cần
                calculateRoomDimensions(roomData);
                

                renderEditForm(roomId, roomData);
            } catch (error) {
                console.error('Error loading room for edit:', error);
                content.innerHTML = `
                    <div class="alert alert-danger">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        <strong>Lỗi:</strong> ${error.message}
                    </div>
                `;
            } finally {
                loading.style.display = 'none';
                content.style.display = 'block';
            }
        }

        // Hàm tính toán số hàng và số cột từ danh sách ghế
        function calculateRoomDimensions(roomData) {
            // Kiểm tra xem đã có thông tin về số hàng và số cột chưa
            if ((roomData.numberOfRows || roomData.NumberOfRows) && 
                (roomData.numberOfColumns || roomData.NumberOfColumns)) {
                return; // Đã có thông tin, không cần tính toán
            }
            
            // Lấy danh sách ghế
            let seats = [];
            if (roomData.seats) seats = roomData.seats;
            else if (roomData.Seats) seats = roomData.Seats;
            
            if (!seats || !Array.isArray(seats) || seats.length === 0) {
                console.warn('No seats data available to calculate dimensions');
                return;
            }
            
            // Tìm số hàng và số cột lớn nhất
            let maxRow = 0;
            let maxCol = 0;
            
            seats.forEach(seat => {
                const row = seat.rowIndex || seat.RowIndex || 0;
                const col = seat.columnIndex || seat.ColumnIndex || 0;
                
                maxRow = Math.max(maxRow, row);
                maxCol = Math.max(maxCol, col);
            });
            
            // Cập nhật dữ liệu phòng
            roomData.numberOfRows = maxRow;
            roomData.numberOfColumns = maxCol;
            
            console.log(`Calculated room dimensions: ${maxRow} rows x ${maxCol} columns`);
        }

        function renderEditForm(roomId, roomData) {
            const content = document.getElementById('editContent');
            

            if (roomData.data) roomData = roomData.data;
            if (roomData.Data) roomData = roomData.Data;
            

            const roomName = roomData.roomName || roomData.RoomName || '';
            const totalSeats = roomData.totalSeats || roomData.TotalSeats || 0;
            
            // Lấy số hàng và số cột từ dữ liệu
            const numberOfRows = roomData.numberOfRows || roomData.NumberOfRows || 0;
            const numberOfColumns = roomData.numberOfColumns || roomData.NumberOfColumns || 0;
            
            content.innerHTML = `
                <form id="editRoomForm" onsubmit="submitEditForm(event, '${roomId}')">
                    <div class="row g-3">
                        <div class="col-12">
                            <label for="editRoomName" class="form-label">
                                <i class="fas fa-tag me-1"></i>Tên phòng chiếu <span style="color: var(--danger);">*</span>
                            </label>
                            <input type="text" id="editRoomName" name="RoomName" class="form-control-modal" 
                                   value="${roomName}" required maxlength="50">
                        </div>

                        <div class="col-md-4">
                            <label for="editNumberOfRows" class="form-label">
                                <i class="fas fa-arrows-alt-v me-1"></i>Số hàng <span style="color: var(--danger);">*</span>
                            </label>
                            <input type="number" id="editNumberOfRows" name="NumberOfRows" class="form-control-modal" 
                                   value="${numberOfRows}" min="1" max="50" required oninput="calculateEditTotalSeats()">
                            <div class="form-text">
                                <i class="fas fa-info-circle me-1"></i>
                                Số hàng từ 1-50
                            </div>
                        </div>

                        <div class="col-md-4">
                            <label for="editNumberOfColumns" class="form-label">
                                <i class="fas fa-arrows-alt-h me-1"></i>Số cột <span style="color: var(--danger);">*</span>
                            </label>
                            <input type="number" id="editNumberOfColumns" name="NumberOfColumns" class="form-control-modal" 
                                   value="${numberOfColumns}" min="1" max="50" required oninput="calculateEditTotalSeats()">
                            <div class="form-text">
                                <i class="fas fa-info-circle me-1"></i>
                                Số cột từ 1-50
                            </div>
                        </div>

                        <div class="col-md-4">
                            <label for="editTotalSeats" class="form-label">
                                <i class="fas fa-chair me-1"></i>Tổng số ghế
                            </label>
                            <input type="number" id="editTotalSeats" name="TotalSeats" class="form-control-modal" 
                                   value="${totalSeats}" readonly style="background: var(--gray-100);">
                        </div>
                    </div>

                    <hr class="my-4">

                    <div class="d-flex justify-content-end gap-2">
                        <button type="button" onclick="closeEditModal()" class="btn btn-secondary">
                            <i class="fas fa-times me-2"></i>Hủy
                        </button>
                        <button type="submit" class="btn btn-warning" id="editSubmitBtn">
                            <i class="fas fa-save me-2"></i>Cập nhật phòng chiếu
                        </button>
                    </div>
                </form>
            `;
            // Lưu số hàng/cột ban đầu vào form để so sánh khi submit
            const editForm = document.getElementById('editRoomForm');
            editForm.setAttribute('data-original-rows', numberOfRows);
            editForm.setAttribute('data-original-cols', numberOfColumns);
        }

        function calculateEditTotalSeats() {
            const rows = document.getElementById('editNumberOfRows').value;
            const cols = document.getElementById('editNumberOfColumns').value;
            
            if (rows && cols) {
                document.getElementById('editTotalSeats').value = rows * cols;
            }
        }

        async function submitEditForm(event, roomId) {
            event.preventDefault();
            
            const submitBtn = document.getElementById('editSubmitBtn');
            const originalText = submitBtn.innerHTML;
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Đang cập nhật...';
            
            const form = document.getElementById('editRoomForm');
            const formData = new FormData(form);
            const data = {};
            
            // Lấy các giá trị từ form
            const roomName = formData.get('RoomName');
            const numberOfRows = parseInt(formData.get('NumberOfRows'));
            const numberOfColumns = parseInt(formData.get('NumberOfColumns'));
            const totalSeats = numberOfRows * numberOfColumns;
            
            // Kiểm tra dữ liệu
            if (!roomName || !numberOfRows || !numberOfColumns) {
                const errorAlert = document.createElement('div');
                errorAlert.className = 'alert alert-danger';
                errorAlert.innerHTML = `
                    <i class="fas fa-exclamation-circle me-2"></i>
                    <div>Vui lòng điền đầy đủ thông tin bắt buộc</div>
                    <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                        <i class="fas fa-times"></i>
                    </button>
                `;
                document.getElementById('editContent').prepend(errorAlert);
                
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
                return;
            }
            
            // Lấy số hàng/cột ban đầu từ roomData (gắn vào form khi renderEditForm)
            const originalRows = parseInt(form.getAttribute('data-original-rows'));
            const originalCols = parseInt(form.getAttribute('data-original-cols'));
            
            // Tạo dữ liệu để gửi đi
            data.RoomName = roomName;
            data.NumberOfRows = numberOfRows;
            data.NumberOfColumns = numberOfColumns;
            data.TotalSeats = totalSeats;
            // Chỉ gửi RegenerateSeats: true nếu layout thay đổi
            data.RegenerateSeats = (numberOfRows !== originalRows || numberOfColumns !== originalCols);
            
            try {
                const updateUrl = `https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/cinemaroom/Update/${roomId}`;
                console.log('Updating room at URL:', updateUrl, 'with data:', data); // Debug log
                
                const response = await fetch(updateUrl, {
                    method: 'PATCH',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest',
                        'Accept': 'application/json'
                    },
                    body: JSON.stringify(data)
                });
                
                const responseText = await response.text();
                let result;
                try {
                    result = JSON.parse(responseText);
                } catch (e) {
                    throw new Error(`Invalid JSON response: ${responseText}`);
                }
                
                if (response.ok && (result.Code === 200 || result.code === 200)) {
                    closeEditModal();
                    
                    const successAlert = document.createElement('div');
                    successAlert.className = 'alert alert-success';
                    successAlert.innerHTML = `
                        <i class="fas fa-check-circle me-2"></i>
                        <span>${result.Message || result.message || 'Cập nhật phòng chiếu thành công!'}</span>
                        <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                            <i class="fas fa-times"></i>
                        </button>
                    `;
                    document.querySelector('.page-header').after(successAlert);
                    
                    setTimeout(() => window.location.reload(), 1500);
                } else {
                    throw new Error(result.error || result.message || result.Message || 'Có lỗi xảy ra khi cập nhật phòng chiếu');
                }
            } catch (error) {
                console.error('Error updating room:', error);
                

                const errorAlert = document.createElement('div');
                errorAlert.className = 'alert alert-danger';
                errorAlert.innerHTML = `
                    <i class="fas fa-exclamation-circle me-2"></i>
                    <div>${error.message || 'Có lỗi xảy ra khi cập nhật phòng chiếu'}</div>
                    <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                        <i class="fas fa-times"></i>
                    </button>
                `;
                document.getElementById('editContent').prepend(errorAlert);
                

                setTimeout(() => errorAlert.remove(), 5000);
            } finally {

                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
        }

        function closeEditModal() {
            document.getElementById('editModal').style.display = 'none';
        }

        // Quản lý ghế functions
        let currentRoomId = null;
        let currentSeats = [];
        let selectedSeats = []; // Thay đổi từ selectedSeat thành selectedSeats array

        async function openManageSeatsModal(roomId) {
            console.log('Opening manage seats modal for room ID:', roomId);
            currentRoomId = roomId;
            const modal = document.getElementById('manageSeatsModal');
            const loading = document.getElementById('manageSeatsLoading');
            const content = document.getElementById('manageSeatsContent');
            
            modal.style.display = 'block';
            loading.style.display = 'flex';
            content.style.display = 'none';
            
            try {
                // Sử dụng API service giống như modal chi tiết phòng
                const detailsUrl = `https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/cinemaroom/ViewSeat?Id=${roomId}`;
                
                const response = await fetch(detailsUrl, {
                    method: 'GET',
                    headers: createAuthHeaders(),
                    mode: 'cors'
                });
                
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                
                const data = await response.json();
                
                // Xử lý response format giống như modal chi tiết
                let roomData;
                if (data.Code !== undefined && data.Data) {
                    roomData = data.Data;
                } else if (data.code !== undefined && data.data) {
                    roomData = data.data;
                } else {
                    roomData = data;
                }
                
                window.lastSeatsResponse = { data: roomData };
                currentSeats = roomData.seats || roomData.Seats || [];
                console.log('Current seats:', currentSeats);
                console.log('Seats length:', currentSeats.length);
                
                renderSeatsGrid();
                updateRoomName();
                loading.style.display = 'none';
                content.style.display = 'block';
            } catch (error) {
                console.error('Error loading seats:', error);
                let errorMessage = 'Failed to fetch';
                
                if (error.name === 'TypeError' && error.message.includes('fetch')) {
                    errorMessage = 'Không thể kết nối đến server. Vui lòng kiểm tra kết nối mạng.';
                } else if (error.message) {
                    errorMessage = error.message;
                }
                
                loading.innerHTML = `
                    <i class="fas fa-exclamation-triangle" style="color: var(--danger);"></i>
                    <span>${errorMessage}</span>
                    <br><br>
                    <button onclick="openManageSeatsModal('${roomId}')" class="btn btn-primary btn-sm">
                        <i class="fas fa-redo"></i> Thử lại
                    </button>
                `;
            }
        }

        function renderSeatsGrid() {
            const grid = document.getElementById('seatsGrid');
            grid.innerHTML = '';
            
            if (!currentSeats || currentSeats.length === 0) {
                grid.innerHTML = '<p class="text-muted">Không có ghế nào trong phòng này</p>';
                return;
            }
            
            // Tìm số hàng và cột lớn nhất
            const maxRow = Math.max(...currentSeats.map(seat => seat.rowIndex || seat.RowIndex));
            const maxCol = Math.max(...currentSeats.map(seat => seat.columnIndex || seat.ColumnIndex));
            
            grid.style.gridTemplateColumns = `repeat(${maxCol}, 35px)`;
            
            // Tạo grid ghế
            for (let row = 1; row <= maxRow; row++) {
                for (let col = 1; col <= maxCol; col++) {
                    const seat = currentSeats.find(s => 
                        (s.rowIndex || s.RowIndex) === row && 
                        (s.columnIndex || s.ColumnIndex) === col
                    );
                    
                    if (seat) {
                        const seatElement = createSeatElement(seat);
                        grid.appendChild(seatElement);
                    } else {
                        // Tạo placeholder cho vị trí trống
                        const placeholder = document.createElement('div');
                        placeholder.className = 'seat-item disabled';
                        placeholder.textContent = '';
                        grid.appendChild(placeholder);
                    }
                }
            }
        }

        function createSeatElement(seat) {
            const seatElement = document.createElement('div');
            // Xử lý SeatType từ enum (0=Normal, 1=VIP, 2=Couple)
            const seatType = seat.seatType || seat.SeatType;
            const seatTypeClass = typeof seatType === 'number' 
                ? (seatType === 1 ? 'vip' : seatType === 2 ? 'couple' : 'normal')
                : getSeatTypeClass(seatType);
            
            seatElement.className = `seat-item ${seatTypeClass}`;
            seatElement.textContent = seat.seatCode || seat.SeatCode;
            seatElement.setAttribute('data-seat-id', seat.id || seat.Id);
            seatElement.setAttribute('data-seat-code', seat.seatCode || seat.SeatCode);
            seatElement.setAttribute('data-seat-type', seatType);
            seatElement.setAttribute('data-seat-price', seat.priceSeat || seat.PriceSeat || 0);
            seatElement.setAttribute('data-row', seat.rowIndex || seat.RowIndex);
            seatElement.setAttribute('data-column', seat.columnIndex || seat.ColumnIndex);
            
            seatElement.addEventListener('click', () => selectSeat(seatElement, seat));
            
            return seatElement;
        }

        function getSeatTypeClass(seatType) {
            switch (seatType?.toLowerCase()) {
                case 'vip': return 'vip';
                case 'couple': return 'couple';
                default: return 'normal';
            }
        }

        function selectSeat(seatElement, seatData) {
            const seatId = seatData.id || seatData.Id;
            const isSelected = selectedSeats.some(seat => (seat.id || seat.Id) === seatId);
            
            if (isSelected) {
                // Bỏ chọn ghế
                seatElement.classList.remove('selected');
                selectedSeats = selectedSeats.filter(seat => (seat.id || seat.Id) !== seatId);
            } else {
                // Chọn ghế mới
                seatElement.classList.add('selected');
                selectedSeats.push(seatData);
            }
            
            // Cập nhật thông tin ghế
            updateSeatsInfo();
        }

        function updateSeatsInfo() {
            const infoContainer = document.getElementById('selectedSeatInfo');
            
            if (selectedSeats.length === 0) {
                infoContainer.innerHTML = `
                    <p class="text-muted">Chọn một hoặc nhiều ghế để xem thông tin</p>
                    <div class="mt-3">
                        <button onclick="selectAllSeats()" class="btn btn-outline-primary btn-sm me-2">
                            <i class="fas fa-check-square me-1"></i>Chọn tất cả
                        </button>
                        <button onclick="deselectAllSeats()" class="btn btn-outline-secondary btn-sm">
                            <i class="fas fa-square me-1"></i>Bỏ chọn tất cả
                        </button>
                    </div>
                `;
                return;
            }
            
            if (selectedSeats.length === 1) {
                // Hiển thị thông tin chi tiết cho 1 ghế
                const seatData = selectedSeats[0];
                const seatType = seatData.seatType || seatData.SeatType;
                const seatTypeText = getSeatTypeText(seatType);
                const price = (seatData.priceSeat || seatData.PriceSeat || 0).toLocaleString();
                
                infoContainer.innerHTML = `
                    <div class="seat-info-item">
                        <span class="info-label">Mã ghế:</span>
                        <span class="info-value">${seatData.seatCode || seatData.SeatCode}</span>
                    </div>
                    <div class="seat-info-item">
                        <span class="info-label">Loại ghế:</span>
                        <span class="info-value">${seatTypeText}</span>
                    </div>
                    <div class="seat-info-item">
                        <span class="info-label">Giá:</span>
                        <span class="info-value">${price} VNĐ</span>
                    </div>
                    <div class="seat-info-item">
                        <span class="info-label">Vị trí:</span>
                        <span class="info-value">Hàng ${seatData.rowIndex || seatData.RowIndex}, Cột ${seatData.columnIndex || seatData.ColumnIndex}</span>
                    </div>
                    <div class="seat-info-item">
                        <span class="info-label">Trạng thái:</span>
                        <span class="info-value">${seatData.status || seatData.Status || 'Hoạt động'}</span>
                    </div>
                    <div class="mt-3">
                        <button onclick="openEditSeatModal()" class="btn btn-primary btn-sm me-2">
                            <i class="fas fa-edit me-1"></i>Chỉnh sửa
                        </button>
                        <button onclick="openBulkEditModal()" class="btn btn-warning btn-sm">
                            <i class="fas fa-edit me-1"></i>Chỉnh sửa hàng loạt
                        </button>
                    </div>
                `;
            } else {
                // Hiển thị thông tin tổng hợp cho nhiều ghế
                const seatCodes = selectedSeats.map(seat => seat.seatCode || seat.SeatCode).join(', ');
                const totalPrice = selectedSeats.reduce((sum, seat) => sum + (seat.priceSeat || seat.PriceSeat || 0), 0);
                
                infoContainer.innerHTML = `
                    <div class="seat-info-item">
                        <span class="info-label">Số ghế đã chọn:</span>
                        <span class="info-value">${selectedSeats.length} ghế</span>
                    </div>
                    <div class="seat-info-item">
                        <span class="info-label">Mã ghế:</span>
                        <span class="info-value">${seatCodes}</span>
                    </div>
                    <div class="seat-info-item">
                        <span class="info-label">Tổng giá:</span>
                        <span class="info-value">${totalPrice.toLocaleString()} VNĐ</span>
                    </div>
                    <div class="mt-3">
                        <button onclick="openBulkEditModal()" class="btn btn-warning btn-sm">
                            <i class="fas fa-edit me-1"></i>Chỉnh sửa hàng loạt (${selectedSeats.length} ghế)
                        </button>
                    </div>
                `;
            }
        }

        function getSeatTypeText(seatType) {
            // Xử lý cả enum number và string
            if (typeof seatType === 'number') {
                switch (seatType) {
                    case 1: return 'Ghế VIP';
                    case 2: return 'Ghế đôi';
                    default: return 'Ghế thường';
                }
            } else {
                switch (seatType?.toLowerCase()) {
                    case 'vip': return 'Ghế VIP';
                    case 'couple': return 'Ghế đôi';
                    default: return 'Ghế thường';
                }
            }
        }

        function updateRoomName() {
            const roomNameElement = document.getElementById('manageSeatsRoomName');
            if (roomNameElement && currentSeats.length > 0) {
                // Lấy tên phòng từ response data
                const response = window.lastSeatsResponse;
                if (response && response.data) {
                    const roomName = response.data.roomName || response.data.RoomName || 'Phòng chiếu';
                    roomNameElement.textContent = roomName;
                } else {
                    roomNameElement.textContent = 'Phòng chiếu';
                }
            }
        }

        function selectAllSeats() {
            selectedSeats = [...currentSeats];
            document.querySelectorAll('.seat-item').forEach(seatElement => {
                seatElement.classList.add('selected');
            });
            updateSeatsInfo();
        }

        function deselectAllSeats() {
            selectedSeats = [];
            document.querySelectorAll('.seat-item.selected').forEach(seatElement => {
                seatElement.classList.remove('selected');
            });
            updateSeatsInfo();
        }

        function closeManageSeatsModal() {
            document.getElementById('manageSeatsModal').style.display = 'none';
            currentRoomId = null;
            currentSeats = [];
            selectedSeats = [];
        }

        // Modal chỉnh sửa ghế đơn lẻ
        function openEditSeatModal() {
            if (selectedSeats.length !== 1) {
                alert('Vui lòng chọn đúng một ghế để chỉnh sửa');
                return;
            }
            
            const selectedSeat = selectedSeats[0];
            
            const modal = document.getElementById('editSeatModal');
            const seatCode = document.getElementById('editSeatCode');
            const seatType = document.getElementById('editSeatType');
            const seatPrice = document.getElementById('editSeatPrice');
            const seatPosition = document.getElementById('editSeatPosition');
            const seatStatus = document.getElementById('editSeatStatus');
            
            // Điền thông tin ghế
            seatCode.value = selectedSeat.seatCode || selectedSeat.SeatCode;
            // Xử lý SeatType từ enum sang string cho select
            const seatTypeValue = selectedSeat.seatType || selectedSeat.SeatType;
            if (typeof seatTypeValue === 'number') {
                seatType.value = seatTypeValue === 1 ? 'VIP' : seatTypeValue === 2 ? 'Couple' : 'Normal';
            } else {
                seatType.value = seatTypeValue;
            }
            seatPrice.value = selectedSeat.priceSeat || selectedSeat.PriceSeat || 0;
            seatPosition.textContent = `Hàng ${selectedSeat.rowIndex || selectedSeat.RowIndex}, Cột ${selectedSeat.columnIndex || selectedSeat.ColumnIndex}`;
            seatStatus.textContent = selectedSeat.status || selectedSeat.Status || 'Hoạt động';
            
            modal.style.display = 'block';
        }

        function closeEditSeatModal() {
            document.getElementById('editSeatModal').style.display = 'none';
        }

        function updatePriceByType() {
            const seatType = document.getElementById('editSeatType').value;
            const seatPrice = document.getElementById('editSeatPrice');
            
            // Đặt giá mặc định theo loại ghế
            switch (seatType) {
                case 'VIP':
                    seatPrice.value = 80000;
                    break;
                case 'Couple':
                    seatPrice.value = 120000;
                    break;
                default:
                    seatPrice.value = 50000;
                    break;
            }
        }

        function setDefaultPrice() {
            const seatType = document.getElementById('editSeatType').value;
            const seatPrice = document.getElementById('editSeatPrice');
            
            // Đặt giá mặc định theo loại ghế
            switch (seatType) {
                case 'VIP':
                    seatPrice.value = 80000;
                    break;
                case 'Couple':
                    seatPrice.value = 120000;
                    break;
                default:
                    seatPrice.value = 50000;
                    break;
            }
            
            // Hiển thị thông báo nhỏ
            const notification = document.createElement('div');
            notification.className = 'alert alert-info alert-sm';
            notification.innerHTML = `
                <i class="fas fa-info-circle me-1"></i>
                Đã đặt giá mặc định cho ${seatType === 'VIP' ? 'ghế VIP' : seatType === 'Couple' ? 'ghế đôi' : 'ghế thường'}
            `;
            notification.style.position = 'fixed';
            notification.style.top = '20px';
            notification.style.right = '20px';
            notification.style.zIndex = '9999';
            notification.style.maxWidth = '300px';
            notification.style.fontSize = '0.875rem';
            notification.style.padding = '0.5rem 1rem';
            
            document.body.appendChild(notification);
            
            setTimeout(() => {
                notification.remove();
            }, 2000);
        }

        async function submitEditSeatForm(event) {
            event.preventDefault();
            
            if (selectedSeats.length !== 1) {
                alert('Vui lòng chọn đúng một ghế để chỉnh sửa');
                return;
            }
            
            const selectedSeat = selectedSeats[0];
            
            const form = event.target;
            const formData = new FormData(form);
            const submitBtn = document.getElementById('submitEditSeatBtn');
            const originalText = submitBtn.innerHTML;
            
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Đang cập nhật...';
            
            try {
                const seatType = formData.get('SeatType');
                const seatTypeEnum = seatType === 'VIP' ? 1 : seatType === 'Couple' ? 2 : 0;
                
                const seatData = {
                    SeatId: selectedSeat.id || selectedSeat.Id,
                    SeatCode: formData.get('SeatCode'),
                    SeatType: seatTypeEnum,
                    RowIndex: selectedSeat.rowIndex || selectedSeat.RowIndex,
                    ColumnIndex: selectedSeat.columnIndex || selectedSeat.ColumnIndex,
                    PriceSeat: parseFloat(formData.get('PriceSeat')),
                    IsActive: true
                };
                
                const response = await fetch(`/CinemaManagement/CinemaRoom/UpdateSeat/${currentRoomId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify(seatData)
                });
                
                const result = await response.json();
                
                if (result.success) {
                    closeEditSeatModal();
                    
                    // Cập nhật dữ liệu ghế
                    const seatIndex = currentSeats.findIndex(s => (s.id || s.Id) === selectedSeat.id || selectedSeat.Id);
                    if (seatIndex !== -1) {
                        currentSeats[seatIndex] = { ...currentSeats[seatIndex], ...seatData };
                    }
                    
                    // Cập nhật giao diện
                    renderSeatsGrid();
                    updateSeatInfo(currentSeats[seatIndex]);
                    
                    // Hiển thị thông báo thành công
                    const successAlert = document.createElement('div');
                    successAlert.className = 'alert alert-success';
                    successAlert.innerHTML = `
                        <i class="fas fa-check-circle me-2"></i>
                        <span>${result.message}</span>
                        <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                            <i class="fas fa-times"></i>
                        </button>
                    `;
                    document.querySelector('.page-header').after(successAlert);
                    
                    setTimeout(() => successAlert.remove(), 3000);
                } else {
                    throw new Error(result.message || 'Có lỗi xảy ra khi cập nhật ghế');
                }
            } catch (error) {
                console.error('Error updating seat:', error);
                
                const errorAlert = document.createElement('div');
                errorAlert.className = 'alert alert-danger';
                errorAlert.innerHTML = `
                    <i class="fas fa-exclamation-circle me-2"></i>
                    <span>${error.message}</span>
                    <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                        <i class="fas fa-times"></i>
                    </button>
                `;
                document.querySelector('.page-header').after(errorAlert);
                
                setTimeout(() => errorAlert.remove(), 5000);
            } finally {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
        }

        // Modal chỉnh sửa hàng loạt
        function openBulkEditModal() {
            if (selectedSeats.length === 0) {
                alert('Vui lòng chọn ít nhất một ghế để chỉnh sửa');
                return;
            }
            
            const modal = document.getElementById('bulkEditSeatModal');
            const seatCount = document.getElementById('bulkEditSeatCount');
            
            seatCount.textContent = selectedSeats.length;
            modal.style.display = 'block';
        }

        function closeBulkEditSeatModal() {
            document.getElementById('bulkEditSeatModal').style.display = 'none';
        }

        function updateBulkPriceByType() {
            const seatType = document.getElementById('bulkEditSeatType').value;
            const seatPrice = document.getElementById('bulkEditSeatPrice');
            
            if (seatType) {
                // Đặt giá mặc định theo loại ghế
                switch (seatType) {
                    case 'VIP':
                        seatPrice.value = 80000;
                        break;
                    case 'Couple':
                        seatPrice.value = 120000;
                        break;
                    default:
                        seatPrice.value = 50000;
                        break;
                }
            }
        }

        function setBulkDefaultPrice() {
            const seatType = document.getElementById('bulkEditSeatType').value;
            const seatPrice = document.getElementById('bulkEditSeatPrice');
            
            if (!seatType) {
                alert('Vui lòng chọn loại ghế trước khi đặt giá mặc định');
                return;
            }
            
            // Đặt giá mặc định theo loại ghế
            switch (seatType) {
                case 'VIP':
                    seatPrice.value = 80000;
                    break;
                case 'Couple':
                    seatPrice.value = 120000;
                    break;
                default:
                    seatPrice.value = 50000;
                    break;
            }
            
            // Hiển thị thông báo nhỏ
            const notification = document.createElement('div');
            notification.className = 'alert alert-info alert-sm';
            notification.innerHTML = `
                <i class="fas fa-info-circle me-1"></i>
                Đã đặt giá mặc định cho ${seatType === 'VIP' ? 'ghế VIP' : seatType === 'Couple' ? 'ghế đôi' : 'ghế thường'}
            `;
            notification.style.position = 'fixed';
            notification.style.top = '20px';
            notification.style.right = '20px';
            notification.style.zIndex = '9999';
            notification.style.maxWidth = '300px';
            notification.style.fontSize = '0.875rem';
            notification.style.padding = '0.5rem 1rem';
            
            document.body.appendChild(notification);
            
            setTimeout(() => {
                notification.remove();
            }, 2000);
        }

        async function submitBulkEditSeatForm(event) {
            event.preventDefault();
            
            if (selectedSeats.length === 0) {
                alert('Không có ghế nào được chọn');
                return;
            }
            
            const form = event.target;
            const formData = new FormData(form);
            const submitBtn = document.getElementById('submitBulkEditSeatBtn');
            const originalText = submitBtn.innerHTML;
            
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Đang cập nhật...';
            
            try {
                const seatType = formData.get('SeatType');
                const seatPrice = formData.get('PriceSeat');
                
                // Kiểm tra xem có thay đổi gì không
                if (!seatType && !seatPrice) {
                    alert('Vui lòng chọn ít nhất một trường để cập nhật');
                    return;
                }
                
                // Chuẩn bị dữ liệu cập nhật
                const updates = selectedSeats.map(seat => {
                    const update = {
                        SeatId: seat.id || seat.Id
                    };
                    
                    if (seatType) {
                        const seatTypeEnum = seatType === 'VIP' ? 1 : seatType === 'Couple' ? 2 : 0;
                        update.NewSeatType = seatTypeEnum;
                    }
                    
                    if (seatPrice) {
                        update.NewPrice = parseFloat(seatPrice);
                    }
                    
                    return update;
                });
                
                const updateData = {
                    RoomId: currentRoomId,
                    Updates: updates
                };
                
                // Gọi API cập nhật hàng loạt
                const response = await fetch(`/CinemaManagement/CinemaRoom/UpdateSeats`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify(updateData)
                });
                
                const result = await response.json();
                
                if (result.success) {
                    closeBulkEditSeatModal();
                    
                    // Cập nhật dữ liệu ghế
                    selectedSeats.forEach(selectedSeat => {
                        const seatIndex = currentSeats.findIndex(s => (s.id || s.Id) === (selectedSeat.id || selectedSeat.Id));
                        if (seatIndex !== -1) {
                            if (seatType) {
                                const seatTypeEnum = seatType === 'VIP' ? 1 : seatType === 'Couple' ? 2 : 0;
                                currentSeats[seatIndex].seatType = seatTypeEnum;
                                currentSeats[seatIndex].SeatType = seatTypeEnum;
                            }
                            if (seatPrice) {
                                currentSeats[seatIndex].priceSeat = parseFloat(seatPrice);
                                currentSeats[seatIndex].PriceSeat = parseFloat(seatPrice);
                            }
                        }
                    });
                    
                    // Cập nhật giao diện
                    renderSeatsGrid();
                    updateSeatsInfo();
                    
                    // Hiển thị thông báo thành công
                    const successAlert = document.createElement('div');
                    successAlert.className = 'alert alert-success';
                    successAlert.innerHTML = `
                        <i class="fas fa-check-circle me-2"></i>
                        <span>Cập nhật thành công ${selectedSeats.length} ghế!</span>
                        <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                            <i class="fas fa-times"></i>
                        </button>
                    `;
                    document.querySelector('.page-header').after(successAlert);
                    
                    setTimeout(() => successAlert.remove(), 3000);
                } else {
                    throw new Error(result.message || 'Có lỗi xảy ra khi cập nhật ghế');
                }
            } catch (error) {
                console.error('Error updating seats:', error);
                
                const errorAlert = document.createElement('div');
                errorAlert.className = 'alert alert-danger';
                errorAlert.innerHTML = `
                    <i class="fas fa-exclamation-circle me-2"></i>
                    <span>${error.message}</span>
                    <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                        <i class="fas fa-times"></i>
                    </button>
                `;
                document.querySelector('.page-header').after(errorAlert);
                
                setTimeout(() => errorAlert.remove(), 5000);
            } finally {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
        }

        // Event listeners cho modal quản lý ghế
        document.getElementById('manageSeatsModal').addEventListener('click', function(e) {
            if (e.target === this) {
                closeManageSeatsModal();
            }
        });

        document.getElementById('editSeatModal').addEventListener('click', function(e) {
            if (e.target === this) {
                closeEditSeatModal();
            }
        });

        document.getElementById('bulkEditSeatModal').addEventListener('click', function(e) {
            if (e.target === this) {
                closeBulkEditSeatModal();
            }
        });

