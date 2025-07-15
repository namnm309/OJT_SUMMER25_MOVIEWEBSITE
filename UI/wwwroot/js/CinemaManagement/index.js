

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
            const deleteUrl = '@Url.Action("Delete")';
            document.getElementById('deleteForm').action = deleteUrl + '/' + roomId;
            document.getElementById('deleteModal').style.display = 'block';
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
                const formData = new FormData();
                formData.append('RoomName', event.target.RoomName.value);
                formData.append('TotalSeats', event.target.TotalSeats.value);
                formData.append('NumberOfRows', event.target.NumberOfRows.value);
                formData.append('NumberOfColumns', event.target.NumberOfColumns.value);

                const response = await fetch('@Url.Action("Create")', {
                    method: 'POST',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: formData
                });
                
                if (response.ok) {
                    const result = await response.json();
                    if (result.success) {
                        closeCreateModal();
                        

                        const successAlert = document.createElement('div');
                        successAlert.className = 'alert alert-success';
                        successAlert.innerHTML = `
                            <i class="fas fa-check-circle me-2"></i>
                            <span>Thêm phòng chiếu thành công!</span>
                            <button type="button" class="alert-close" onclick="this.parentElement.remove()">
                                <i class="fas fa-times"></i>
                            </button>
                        `;
                        document.querySelector('.page-header').after(successAlert);
                        

                        setTimeout(() => window.location.reload(), 1500);
                    } else {

                        let errorMessage = result.message || 'Có lỗi xảy ra';
                        if (result.errors) {
                            const errorDetails = Object.entries(result.errors)
                                .map(([key, messages]) => `${key}: ${Array.isArray(messages) ? messages.join(', ') : messages}`)
                                .join('<br>');
                            errorMessage += '<br><small>' + errorDetails + '</small>';
                        }
                        throw new Error(errorMessage);
                    }
                } else {
                    const errorText = await response.text();
                    console.error('Server response:', errorText);
                    throw new Error(`Server error: ${response.status} - ${response.statusText}`);
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

                const detailsUrl = '@Url.Action("Details", "CinemaRoom", new { area = "CinemaManagement" })';
                const fullUrl = `${detailsUrl}?id=${roomId}`;
                console.log('Fetching from URL:', fullUrl); // Debug log
                
                const response = await fetch(fullUrl, {
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
            

            const normalSeats = seats.filter(s => (s.seatType || s.SeatType) === 0).length;
            const vipSeats = seats.filter(s => (s.seatType || s.SeatType) === 1).length;
            const coupleSeats = seats.filter(s => (s.seatType || s.SeatType) === 2).length;
            

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
                            const seatClass = seatType === 1 ? 'vip' : seatType === 2 ? 'couple' : 'normal';
                            const seatTypeName = seatType === 1 ? 'VIP' : seatType === 2 ? 'Ghế đôi' : 'Thường';
                            return `
                                <div class="seat ${seatClass}" 
                                     style="width: 28px; height: 28px; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 0.7rem; font-weight: 500; cursor: pointer; transition: all 0.2s ease;"
                                     title="${seatCode} - ${seatTypeName}">
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
                                <div style="font-size: 1.5rem; font-weight: 700; color: #6366f1;">${normalSeats}</div>
                                <div style="font-size: 0.875rem; color: #6b7280;">Ghế thường</div>
                            </div>
                            <div style="text-align: center; padding: 1rem; background: #f9fafb; border-radius: 8px; border: 1px solid #e5e7eb;">
                                <div style="font-size: 1.5rem; font-weight: 700; color: #6366f1;">${vipSeats}</div>
                                <div style="font-size: 0.875rem; color: #6b7280;">Ghế VIP</div>
                            </div>
                            <div style="text-align: center; padding: 1rem; background: #f9fafb; border-radius: 8px; border: 1px solid #e5e7eb;">
                                <div style="font-size: 1.5rem; font-weight: 700; color: #6366f1;">${coupleSeats}</div>
                                <div style="font-size: 0.875rem; color: #6b7280;">Ghế đôi</div>
                            </div>
                            <div style="text-align: center; padding: 1rem; background: #f9fafb; border-radius: 8px; border: 1px solid #e5e7eb;">
                                <div style="font-size: 1.5rem; font-weight: 700; color: #6366f1;">${seats.length}</div>
                                <div style="font-size: 0.875rem; color: #6b7280;">Tổng cộng</div>
                            </div>
                        </div>
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

                const detailsUrl = '@Url.Action("Details", "CinemaRoom", new { area = "CinemaManagement" })';
                const fullDetailsUrl = `${detailsUrl}?id=${roomId}`;
                console.log('Fetching details from URL:', fullDetailsUrl); // Debug log
                
                const detailsResponse = await fetch(fullDetailsUrl, {
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
            
            // Tạo dữ liệu để gửi đi
            data.RoomName = roomName;
            data.NumberOfRows = numberOfRows;
            data.NumberOfColumns = numberOfColumns;
            data.TotalSeats = totalSeats;
            
            try {
                const updateUrl = '@Url.Action("Edit", "CinemaRoom", new { area = "CinemaManagement" })';
                const fullUpdateUrl = `${updateUrl}?id=${roomId}`;
                console.log('Updating room at URL:', fullUpdateUrl, 'with data:', data); // Debug log
                
                const response = await fetch(fullUpdateUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify(data)
                });
                
                if (!response.ok) {
                    const errorText = await response.text();
                    console.error('Server response:', errorText);
                    throw new Error(`Server error: ${response.status} - ${response.statusText}`);
                }
                
                const result = await response.json();
                if (result.success) {

                    window.location.reload();
                } else {
                    throw new Error(result.message || 'Có lỗi xảy ra khi cập nhật phòng chiếu');
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
 
