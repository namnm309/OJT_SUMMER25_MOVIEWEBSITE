

               // Sử dụng HTTPS để trùng với backend và tránh lỗi mixed-scheme/CORS
const apiBaseUrl = 'https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net';

               let isEditMode = false;
               let editingShowtimeId = null;

               function searchShowtimes() {
                   const searchTerm = document.getElementById('showtimeSearch').value.toLowerCase();
                   const tableBody = document.getElementById('showtimeTableBody');
                   const rows = tableBody.querySelectorAll('tr');

                   rows.forEach(row => {
                       const movieTitle = row.querySelector('.movie-details h6')?.textContent.toLowerCase() || '';
                       const roomName = row.querySelectorAll('td')[2]?.textContent.toLowerCase() || '';

                       if (movieTitle.includes(searchTerm) || roomName.includes(searchTerm)) {
                           row.style.display = '';
                       } else {
                           row.style.display = 'none';
                       }
                   });

                   showMovieSuggestions(searchTerm);
               }


               function toggleSelectAll() {
                   const selectAllCheckbox = document.getElementById('selectAll');
                   const checkboxes = document.querySelectorAll('tbody input[type="checkbox"]');

                   checkboxes.forEach(checkbox => {
                       checkbox.checked = selectAllCheckbox.checked;
                   });

                   updateBulkActionButtons();
               }


               function updateBulkActionButtons() {
                   const checkedBoxes = document.querySelectorAll('tbody input[type="checkbox"]:checked');
                   const bulkDeleteBtn = document.getElementById('bulkDeleteBtn');
                   const bulkUpdateBtn = document.getElementById('bulkUpdateBtn');

                   const hasSelected = checkedBoxes.length > 0;
                   bulkDeleteBtn.disabled = !hasSelected;
                   bulkUpdateBtn.disabled = !hasSelected;
               }


               async function openCreateShowtimeModal() {
                   try {
                       // Reset trạng thái về chế độ tạo mới
                       isEditMode = false;
                       editingShowtimeId = null;
                       // Khôi phục tiêu đề & nút lưu
                       document.getElementById('createNewShowtimeModalLabel').innerHTML = '<i class="fas fa-plus-circle text-primary me-2"></i> Thêm lịch chiếu mới';
                       const saveBtn = document.getElementById('saveNewShowtimeBtn');
                       if (saveBtn) saveBtn.innerHTML = '<i class="fas fa-save me-1"></i> Lưu lịch chiếu';

                       await loadMoviesAndRooms();

                       //

                       const modal = new bootstrap.Modal(document.getElementById('createNewShowtimeModal'));
                       modal.show();
                   } catch (error) {
                       console.error('Error opening create showtime modal:', error);
                       showNotification('Lỗi khi mở form thêm lịch chiếu', 'danger');
                   }
               }

               async function loadMoviesAndRooms() {
                   try {

                       const moviesResponse = await fetch(`${apiBaseUrl}/api/v1/booking-ticket/dropdown/movies`);
                       const moviesResult = await moviesResponse.json();

                       // Một số API trả về { data: [...] } hoặc { Data: [...] } hoặc trả thẳng mảng
                       const moviesData = moviesResult.data || moviesResult.Data || moviesResult;

                       if (Array.isArray(moviesData) && moviesData.length) {
                           const movieSelect = document.getElementById('movieSelect');
                           movieSelect.innerHTML = '<option value="">-- Chọn phim --</option>';
                           moviesData.forEach(movie => {
                               // movie có thể có 'id' / 'Id', 'title' / 'Title'
                               const movieId = movie.id || movie.Id;
                               const movieTitle = movie.title || movie.Title;
                               movieSelect.innerHTML += `<option value="${movieId}" data-duration="${movie.duration || movie.Duration || 0}">${movieTitle}</option>`;
                           });
                       } else {
                           showNotification('Không tìm thấy dữ liệu phim', 'danger');
                       }


                       const roomsResponse = await fetch(`${apiBaseUrl}/api/v1/cinemaroom/ViewRoom`);
                       const roomsResult = await roomsResponse.json();

                       const roomsData = roomsResult.data || roomsResult;

                       if (Array.isArray(roomsData)) {
                           const roomSelect = document.getElementById('cinemaRoomSelect');
                           roomSelect.innerHTML = '<option value="">-- Chọn phòng chiếu --</option>';
                           roomsData.forEach(room => {
                               roomSelect.innerHTML += `<option value="${room.id}">${room.roomName}</option>`;
                           });
                       } else {
                           showNotification('Lỗi khi tải dữ liệu phòng chiếu', 'danger');
                       }
                   } catch (error) {
                       console.error('Error loading movies and rooms:', error);
                       showNotification('Lỗi khi tải dữ liệu phim và phòng chiếu', 'danger');
                   }
               }

               async function checkScheduleConflict() {
                   const movieId = document.getElementById('movieSelect').value;
                   const cinemaRoomId = document.getElementById('cinemaRoomSelect').value;
                   const showDate = document.getElementById('showDate').value;
                   const startTime = document.getElementById('startTime').value;
                   const endTime = document.getElementById('endTime').value;

                   if (!movieId || !cinemaRoomId || !showDate || !startTime || !endTime) {
                       showNotification('Vui lòng điền đầy đủ thông tin trước khi kiểm tra xung đột', 'warning');
                       return;
                   }

                   try {
                       const response = await fetch(`${apiBaseUrl}/api/v1/showtime/CheckConflict?movieId=${movieId}&cinemaRoomId=${cinemaRoomId}&showDate=${showDate}&startTime=${startTime}&endTime=${endTime}`);
                       const result = await response.json();

                       if (result.success) {
                           if (result.data.hasConflict) {
                               showNotification('Có xung đột lịch chiếu! Vui lòng chọn thời gian khác.', 'danger');
                           } else {
                               showNotification('Không có xung đột lịch chiếu. Có thể tạo lịch chiếu này.', 'success');
                           }
                       } else {
                           showNotification('Lỗi khi kiểm tra xung đột: ' + result.message, 'danger');
                       }
                   } catch (error) {
                       console.error('Error checking schedule conflict:', error);
                       showNotification('Lỗi khi kiểm tra xung đột lịch chiếu', 'danger');
                   }
               }

               async function saveNewShowtime() {
            // Nếu đang ở chế độ chỉnh sửa => gọi API PUT, ngược lại POST
            const isEdit = isEditMode && editingShowtimeId;
            
            // 0. Tính endTime tự động dựa vào duration
            calculateEndTime();

            // 1. Lấy rawDate từ picker (MM/DD/YYYY hoặc YYYY-MM-DD) và chuyển sang ISO (YYYY-MM-DD)
            const rawDate = document.getElementById('showDate').value;
            let showDateIso = rawDate;
            if (rawDate.includes('/')) {
                const [mm, dd, yyyy] = rawDate.split('/');
                showDateIso = `${yyyy}-${mm.padStart(2,'0')}-${dd.padStart(2,'0')}`;
            }

            // 2. Chuẩn hóa giờ bắt đầu/kết thúc
            const normalizeTime = t => t && (t.length === 5 ? `${t}:00` : t);
            const startTime = normalizeTime(document.getElementById('startTime').value);
            const endTime   = normalizeTime(document.getElementById('endTime').value);

            // 3. Kiểm tra bắt buộc
            const movieId       = document.getElementById('movieSelect').value;
            const cinemaRoomId  = document.getElementById('cinemaRoomSelect').value;
            if (!movieId || !cinemaRoomId || !showDateIso || !startTime || !endTime) {
                showNotification('Vui lòng điền đầy đủ thông tin', 'warning');
                return;
            }

            // BẬT kiểm tra xung đột lịch chiếu trước khi lưu
            const duration = (() => {
                const movieSelect = document.getElementById('movieSelect');
                return parseInt(movieSelect.selectedOptions[0]?.dataset.duration || '0');
            })();
            let conflictUrl = `${apiBaseUrl}/api/v1/showtime/CheckConflict?cinemaRoomId=${cinemaRoomId}&showDate=${showDateIso}&startTime=${startTime}&endTime=${endTime}&movieId=${movieId}`;
            if (isEdit) {
                conflictUrl += `&excludeId=${editingShowtimeId}`;
            }
            try {
                const conflictResp = await fetch(conflictUrl);
                const conflictResult = await conflictResp.json();
                if (conflictResult.hasConflict === true || (conflictResult.data && conflictResult.data.hasConflict)) {
                    showNotification('Lịch chiếu bị trùng với suất chiếu khác trong phòng này!', 'danger');
                    return;
                }
            } catch (err) {
                showNotification('Không kiểm tra được xung đột lịch chiếu. Vui lòng thử lại.', 'danger');
                return;
            }

            // 5. Chuẩn bị payload
            const payload = {
                movieId: movieId,
                cinemaRoomId: cinemaRoomId,
                showDate: showDateIso,
                startTime: startTime,
                price: parseFloat(document.getElementById('price').value) || 0,
                isActive: document.getElementById('isActive').checked
            };
            // Chỉ thêm endTime cho create, không cho update
            if (!isEdit) {
                payload.endTime = endTime; // Chỉ cho create-new DTO
            }
            if (isEdit) {
                payload.id = editingShowtimeId; // ShowtimeUpdateDto yêu cầu Id
            }

            const url    = isEdit ? `${apiBaseUrl}/api/v1/showtime/${editingShowtimeId}`
                                   : `${apiBaseUrl}/api/v1/showtime/create-new`;
            const method = isEdit ? 'PUT' : 'POST';

            try {
                const resp = await fetch(url, {
                    method: method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                console.log('API Response Status:', resp.status);
                console.log('API Response Headers:', resp.headers);
                const text = await resp.text();
                console.log('API Response Text:', text);
                let result; try { result = JSON.parse(text); } catch { result = { message: text }; }
                console.log('Parsed Result:', result);
                const success = resp.ok || result.code === 200 || result.success === true;
                console.log('Success Flag:', success);

                if (success) {
                    showNotification(isEdit ? 'Cập nhật lịch chiếu thành công!' : 'Tạo lịch chiếu mới thành công!', 'success');

                    // Ẩn modal và reset form
                    const modalEl = document.getElementById('createNewShowtimeModal');
                    bootstrap.Modal.getInstance(modalEl)?.hide();
                    document.getElementById('createNewShowtimeForm').reset();
                    
                    // Reset edit mode
                    isEditMode = false;
                    editingShowtimeId = null;

                    setTimeout(()=> location.reload(), 1500);
                } else {
                    console.log('API Error - Status:', resp.status, 'Message:', result.message);
                    // Hiển thị lỗi chi tiết từ backend
                    let errorMessage = 'Có lỗi xảy ra';
                    if (result.message) {
                        errorMessage = result.message;
                    } else if (result.error) {
                        errorMessage = result.error;
                    } else if (result.errors && Array.isArray(result.errors)) {
                        errorMessage = result.errors.join(', ');
                    } else if (typeof result === 'string') {
                        errorMessage = result;
                    }
                    
                    // Thêm thông tin HTTP status nếu có
                    if (resp.status !== 200) {
                        errorMessage += ` (HTTP ${resp.status})`;
                    }
                    
                    showNotification(errorMessage, 'danger');
                }
            } catch (err) {
                console.error('Error save showtime', err);
                showNotification('Lỗi kết nối: ' + err.message, 'danger');
            }
        }


               function openFilterModal() {
                   showNotification('Chức năng lọc đang được phát triển', 'info');
               }

               function exportShowtimes() {
                   showNotification('Đang xuất dữ liệu...', 'info');
               }


               function viewShowtime(id) {
                   showNotification(`Xem chi tiết lịch chiếu ID: ${id}`, 'info');
               }

               function editShowtime(id) {
                   // Chuyển sang chế độ chỉnh sửa
                   isEditMode = true;
                   editingShowtimeId = id;
                   
                   // Đảm bảo dropdown phim/phòng đã có dữ liệu
                   loadMoviesAndRooms().then(async () => {
                       try {
                           const resp = await fetch(`${apiBaseUrl}/api/v1/showtime/${id}`);
                           const result = await resp.json();
                           const data = result.data || result.Data || result;
                           if (!data) {
                               showNotification('Không tìm thấy dữ liệu lịch chiếu', 'danger');
                               return;
                           }

                           // Gán giá trị vào form
                           document.getElementById('movieSelect').value        = data.movieId;
                           document.getElementById('cinemaRoomSelect').value   = data.cinemaRoomId;
                           document.getElementById('showDate').value           = (data.showDate || '').split('T')[0];
                           document.getElementById('startTime').value          = (data.startTime || '').substring(0,5);
                           document.getElementById('price').value              = data.price || 0;
                           document.getElementById('isActive').checked         = data.isActive !== false;

                           calculateEndTime();
                           
                           // Hiển thị giờ kết thúc hiện tại nếu có
                           if (data.endTime) {
                               const endTimeDisplay = document.getElementById('endTimeDisplay');
                               if (endTimeDisplay) {
                                   const endTimeStr = (data.endTime || '').substring(0,5);
                                   endTimeDisplay.value = endTimeStr;
                               }
                           }

                           // Cập nhật tiêu đề & nút lưu
                           document.getElementById('createNewShowtimeModalLabel').innerHTML = '<i class="fas fa-edit text-warning me-2"></i> Chỉnh sửa lịch chiếu';
                           const saveBtn = document.getElementById('saveNewShowtimeBtn');
                           if (saveBtn) saveBtn.innerHTML = '<i class="fas fa-save me-1"></i> Cập nhật lịch chiếu';

                           const modal = new bootstrap.Modal(document.getElementById('createNewShowtimeModal'));
                           modal.show();
                       } catch (err) {
                           console.error('Error load showtime detail', err);
                           showNotification('Lỗi khi tải thông tin lịch chiếu', 'danger');
                       }
                   });
               }

               function deleteShowtime(id) {
                   if (confirm('Bạn có chắc chắn muốn xóa lịch chiếu này?')) {
                       // Gọi API xóa lịch chiếu
                       fetch(`${apiBaseUrl}/api/v1/showtime/${id}`, {
                           method: 'DELETE',
                           headers: {
                               'Content-Type': 'application/json'
                           }
                       })
                       .then(response => response.json())
                       .then(result => {
                           if (result.success || result.code === 200) {
                               showNotification('Xóa lịch chiếu thành công!', 'success');
                               setTimeout(() => location.reload(), 1500);
                           } else {
                               // Hiển thị lỗi chi tiết từ backend
                               let errorMessage = 'Lỗi khi xóa lịch chiếu';
                               if (result.message) {
                                   errorMessage += ': ' + result.message;
                               } else if (result.error) {
                                   errorMessage += ': ' + result.error;
                               } else if (result.errors && Array.isArray(result.errors)) {
                                   errorMessage += ': ' + result.errors.join(', ');
                               }
                               showNotification(errorMessage, 'danger');
                           }
                       })
                       .catch(error => {
                           console.error('Error deleting showtime:', error);
                           showNotification('Lỗi kết nối khi xóa: ' + error.message, 'danger');
                       });
                   }
               }

               function bulkDeleteShowtimes() {
                   const checkedBoxes = document.querySelectorAll('tbody input[type="checkbox"]:checked');
                   if (checkedBoxes.length === 0) {
                       showNotification('Vui lòng chọn ít nhất một lịch chiếu để xóa', 'warning');
                       return;
                   }

                   if (confirm(`Bạn có chắc chắn muốn xóa ${checkedBoxes.length} lịch chiếu đã chọn?`)) {
                       showNotification(`Đã xóa ${checkedBoxes.length} lịch chiếu`, 'success');
                       checkedBoxes.forEach(checkbox => {
                           checkbox.closest('tr').remove();
                       });
                       updateBulkActionButtons();
                   }
               }

               function bulkUpdateStatus() {
                   const checkedBoxes = document.querySelectorAll('tbody input[type="checkbox"]:checked');
                   if (checkedBoxes.length === 0) {
                       showNotification('Vui lòng chọn ít nhất một lịch chiếu để cập nhật', 'warning');
                       return;
                   }

                   showNotification(`Đã cập nhật trạng thái cho ${checkedBoxes.length} lịch chiếu`, 'success');
               }


               function showNotification(message, type = 'info') {
                   const notification = document.createElement('div');
                   notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
                   notification.style.cssText = `
                       top: 20px;
                       right: 20px;
                       z-index: 10000;
                       min-width: 300px;
                       box-shadow: var(--shadow-lg);
                       border: none;
                       border-radius: 12px;
                   `;

                   const icon = type === 'success' ? 'check-circle' :
                               type === 'warning' ? 'exclamation-triangle' :
                               type === 'danger' ? 'times-circle' : 'info-circle';

                   notification.innerHTML = `
                       <div class="d-flex align-items-center">
                           <i class="fas fa-${icon} me-2"></i>
                           ${message}
                       </div>
                       <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                   `;

                   document.body.appendChild(notification);

                   setTimeout(() => {
                       notification.remove();
                   }, 5000);
               }


               let currentDate = new Date();
               let currentShowtimeId = null;
               let showtimeData = window.initialShowtimesJson || [];

               function switchView(viewType) {
                   const calendarView = document.getElementById('calendarView');
                   const tableView = document.getElementById('tableView');
                   const toggleBtns = document.querySelectorAll('.view-toggle-btn');

                   toggleBtns.forEach(btn => btn.classList.remove('active'));

                   if (viewType === 'calendar') {
                       calendarView.classList.add('active');
                       tableView.classList.remove('active');
                       document.querySelector('[onclick="switchView(\'calendar\')"]').classList.add('active');
                       // Load dữ liệu tháng hiện tại sau đó render lịch
                       loadMonthlyData().then(generateCalendar);
                           } else {
                       calendarView.classList.remove('active');
                       tableView.classList.add('active');
                       document.querySelector('[onclick="switchView(\'table\')"]').classList.add('active');
                       loadShowtimesTable(1);
                   }
               }

               async function changeMonth(direction) {
                   currentDate.setMonth(currentDate.getMonth() + direction);
                   await loadMonthlyData();
                   generateCalendar();
               }

               async function loadMonthlyData() {
                   try {
                       const month = currentDate.getMonth() + 1;
                       const year = currentDate.getFullYear();

                       showLoadingOverlay(true);

                       const response = await fetch(`/ShowtimeManagement/Showtimes/GetMonthlyData?month=${month}&year=${year}`);
                       const result = await response.json();

                       if (result.success) {
                           showtimeData = result.data || [];
                       } else {
                           console.error('Failed to load monthly data:', result.message);
                           showNotification('Không thể tải dữ liệu lịch chiếu', 'error');
                           showtimeData = [];
                       }
                   } catch (error) {
                       console.error('Error loading monthly data:', error);
                       showNotification('Lỗi khi tải dữ liệu', 'error');
                       showtimeData = [];
                   } finally {
                       showLoadingOverlay(false);
                   }
               }

               function showLoadingOverlay(show) {
                   const container = document.querySelector('.calendar-container');
                   let overlay = container.querySelector('.loading-overlay');

                   if (show) {
                       if (!overlay) {
                           overlay = document.createElement('div');
                           overlay.className = 'loading-overlay';
                           overlay.innerHTML = '<i class="fas fa-spinner fa-spin loading-spinner-small"></i>';
                           container.appendChild(overlay);
                       }
                       overlay.style.display = 'flex';
                   } else {
                       if (overlay) {
                           overlay.style.display = 'none';
                       }
                   }
               }

               function generateCalendar() {
                   const year = currentDate.getFullYear();
                   const month = currentDate.getMonth();
                   const firstDay = new Date(year, month, 1);
                   const lastDay = new Date(year, month + 1, 0);
                   const firstDayOfWeek = (firstDay.getDay() + 6) % 7; // Convert to Monday = 0


                   const monthNames = ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6',
                                     'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'];
                   document.getElementById('calendarTitle').textContent = `${monthNames[month]}/${year}`;

                   const calendarGrid = document.getElementById('calendarGrid');


                   const existingDays = calendarGrid.querySelectorAll('.calendar-day');
                   existingDays.forEach(day => day.remove());


                   let dayCount = 1;
                   for (let week = 0; week < 6; week++) {
                       for (let day = 0; day < 7; day++) {
                           const dayElement = document.createElement('div');
                           dayElement.className = 'calendar-day';

                           const dayIndex = week * 7 + day;

                           if (dayIndex < firstDayOfWeek) {

                               const prevMonthDay = new Date(year, month, -(firstDayOfWeek - dayIndex - 1));
                               dayElement.classList.add('other-month');
                               dayElement.innerHTML = `<div class="calendar-day-number">${prevMonthDay.getDate()}</div>`;
                           } else if (dayCount <= lastDay.getDate()) {

                               const currentDay = new Date(year, month, dayCount);
                               const isToday = currentDay.toDateString() === new Date().toDateString();

                               if (isToday) {
                                   dayElement.classList.add('today');
                               }

                               dayElement.innerHTML = `
                                   <div class="calendar-day-number">${dayCount}</div>
                                   <div class="calendar-showtimes" id="day-${year}-${month}-${dayCount}">
                                       ${generateShowtimesForDay(currentDay)}
                                   </div>
                               `;
                               dayCount++;
                           } else {

                               const nextMonthDay = dayCount - lastDay.getDate();
                               dayElement.classList.add('other-month');
                               dayElement.innerHTML = `<div class="calendar-day-number">${nextMonthDay}</div>`;
                               dayCount++;
                           }

                           calendarGrid.appendChild(dayElement);
                       }

                       if (dayCount > lastDay.getDate() && week < 5) {
                           break;
                       }
                   }
                   // Khởi tạo tooltip cho các item vừa render
                   initializeShowtimeTooltips();
               }

               function generateShowtimesForDay(date) {
                    // Lấy ngày theo giờ local, format "YYYY-MM-DD"
                    const y = date.getFullYear();
                    const m = String(date.getMonth() + 1).padStart(2, '0');
                    const d = String(date.getDate()).padStart(2, '0');
                    const dateStr = `${y}-${m}-${d}`;   // "YYYY-MM-DD"

                    // 1. Lọc showtimes trùng ngày
                    let dayShowtimes = showtimeData.filter(st => {
                        // st.showDate format: "YYYY-MM-DDTHH:mm:ss"
                        return (st.showDate || '').startsWith(dateStr);
                    });

                    // 2. Sort theo startTime tăng dần
                    dayShowtimes.sort((a, b) => a.startTime.localeCompare(b.startTime));

                    // 3. Render ra HTML
                    return dayShowtimes.map(showtime => {
                        const posterUrl = showtime.moviePoster?.trim()
                            ? showtime.moviePoster
                            : '/images/default-movie-poster.jpg';

                        return `
                            <div class="calendar-showtime-item" onclick="showShowtimeDetail('${showtime.id}')"
                                 data-movie-id="${showtime.movieId || ''}"
                                 data-movie-title="${(showtime.movieTitle || '').toLowerCase()}"
                                 data-bs-toggle="tooltip" data-bs-html="true"
                                 data-poster="${posterUrl}"
                                 title="<img src='${posterUrl}' width='140' height='200' style='object-fit:cover;border-radius:6px;'>">
                                <span class="calendar-showtime-title">${showtime.movieTitle}</span>
                                <span class="calendar-showtime-time">${formatTime(showtime.startTime)}</span>
                            </div>
                        `;
                    }).join('');
                }


               function formatTime(timeSpan) {

                   return timeSpan.substring(0, 5);
               }

               function showShowtimeDetail(showtimeId) {
                   const showtime = showtimeData.find(st => st.id === showtimeId);
                   if (!showtime) return;

                   currentShowtimeId = showtimeId;

                   const modalBody = document.getElementById('showtimeDetailBody');
                   modalBody.innerHTML = `
                       <div class="showtime-detail-header">
                           <img src="${showtime.moviePoster || '/images/no-poster.jpg'}"
                                alt="${showtime.movieTitle}"
                                class="showtime-detail-poster"
                                onerror="this.src='/images/no-poster.jpg'">
                           <div class="showtime-detail-info">
                               <h3>${showtime.movieTitle}</h3>
                               <p class="text-muted">Thời lượng: ${showtime.movieDuration} phút</p>
                               <div class="showtime-detail-meta">
                                   <div class="showtime-detail-item">
                                       <i class="fas fa-calendar"></i>
                                       <div>
                                           <strong>Ngày chiếu:</strong><br>
                                           ${new Date(showtime.showDate).toLocaleDateString('vi-VN')}
                                       </div>
                                   </div>
                                   <div class="showtime-detail-item">
                                       <i class="fas fa-clock"></i>
                                       <div>
                                           <strong>Giờ chiếu:</strong><br>
                                           ${formatTime(showtime.startTime)}
                                       </div>
                                   </div>
                                   <div class="showtime-detail-item">
                                       <i class="fas fa-door-open"></i>
                                       <div>
                                           <strong>Phòng chiếu:</strong><br>
                                           ${showtime.cinemaRoomName}
                                       </div>
                                   </div>
                                   <div class="showtime-detail-item">
                                       <i class="fas fa-money-bill"></i>
                                       <div>
                                           <strong>Giá vé:</strong><br>
                                           ${showtime.price.toLocaleString('vi-VN')} VNĐ
                                       </div>
                                   </div>
                                   <div class="showtime-detail-item">
                                       <i class="fas fa-users"></i>
                                       <div>
                                           <strong>Tình trạng vé:</strong><br>
                                           ${showtime.bookedSeats}/${showtime.totalSeats} vé đã đặt
                                       </div>
                                   </div>
                                   <div class="showtime-detail-item">
                                       <i class="fas fa-info-circle"></i>
                                       <div>
                                           <strong>Trạng thái:</strong><br>
                                           ${getStatusText(showtime.showDate)}
                                       </div>
                                   </div>
                               </div>
                           </div>
                       </div>
                   `;

                   const modal = new bootstrap.Modal(document.getElementById('showtimeDetailModal'));
                   modal.show();
               }

               function getStatusText(showDate) {
                   const today = new Date();
                   const showtimeDate = new Date(showDate);

                   if (showtimeDate.toDateString() === today.toDateString()) {
                       return 'Hôm nay';
                   } else if (showtimeDate < today) {
                       return 'Đã qua';
                   } else {
                       return 'Sắp tới';
                   }
               }

               function editCurrentShowtime() {
                   if (currentShowtimeId) {
                       editShowtime(currentShowtimeId);
                       bootstrap.Modal.getInstance(document.getElementById('showtimeDetailModal')).hide();
                   }
               }

               function deleteCurrentShowtime() {
                   if (currentShowtimeId) {
                       deleteShowtime(currentShowtimeId);
                       bootstrap.Modal.getInstance(document.getElementById('showtimeDetailModal')).hide();
                   }
               }

               // Tính giờ kết thúc dựa vào thời lượng phim & giờ bắt đầu
               function calculateEndTime() {
                   const movieSelect = document.getElementById('movieSelect');
                   const startTimeInput = document.getElementById('startTime');
                   const endTimeInput = document.getElementById('endTime');
                   const endTimeDisplay = document.getElementById('endTimeDisplay');

                   if (!movieSelect || !startTimeInput || !endTimeInput) return;

                   const duration = parseInt(movieSelect.selectedOptions[0]?.dataset.duration || '0');
                   const startVal = startTimeInput.value;
                   if (!startVal) return;
                   if (!duration) {
                       showNotification('Phim chưa có thời lượng nên không tính được giờ kết thúc', 'warning');
                       return;
                   }

                   const [h, m] = startVal.split(':').map(Number);
                   if (isNaN(h) || isNaN(m)) return;

                   const startDate = new Date();
                   startDate.setHours(h, m, 0, 0);
                   const endDate = new Date(startDate.getTime() + duration * 60000);

                   const endHours = String(endDate.getHours()).padStart(2, '0');
                   const endMinutes = String(endDate.getMinutes()).padStart(2, '0');
                   endTimeInput.value = `${endHours}:${endMinutes}`;
                   
                   // Cập nhật hiển thị giờ kết thúc
                   if (endTimeDisplay) {
                       endTimeDisplay.value = `${endHours}:${endMinutes}`;
                   }
               }

               // Lắng nghe thay đổi để cập nhật EndTime
               document.addEventListener('DOMContentLoaded', () => {
                   document.getElementById('movieSelect')?.addEventListener('change', calculateEndTime);
                   document.getElementById('startTime')?.addEventListener('change', calculateEndTime);
               });

            //   async function autoCheckConflict() {
            //    return false; // Luôn coi như không trùng lịch
            //}


               function clearCalendarHighlight() {
                   document.querySelectorAll('.calendar-showtime-item.selected-movie').forEach(el => el.classList.remove('selected-movie'));
               }

               function highlightMovieInCalendar(movieId) {
                   if (!movieId) return;
                   document.querySelectorAll(`.calendar-showtime-item[data-movie-id='${movieId}']`).forEach(el => el.classList.add('selected-movie'));
               }

               async function showMovieSuggestions(term) {
                   const container = document.getElementById('movieSearchResult');
                   if (!container) return;

                   if (!term) { hideMoviePreview(); clearCalendarHighlight(); return; }

                   container.innerHTML = '<div class="text-center"><i class="fas fa-spinner fa-spin"></i></div>';
                   container.style.display = 'block';

                   try {
                       const resp = await fetch(`${apiBaseUrl}/api/v1/movie/Search?keyword=${encodeURIComponent(term)}`);
                       const result = await resp.json();
                       const movies = result.data || result || [];

                       if (!Array.isArray(movies) || !movies.length) {
                           container.innerHTML = '<span class="text-muted" style="font-size:12px;">Không tìm thấy phim</span>';
                           clearCalendarHighlight();
                           return;
                       }

                       // Chọn phim phù hợp nhất: ưu tiên khớp tên chính xác, kế tiếp là bắt đầu bằng từ khoá, sau đó lấy đầu tiên
                       const termLower = term.toLowerCase();
                       let chosen = movies.find(m => (m.title || '').toLowerCase() === termLower);
                       if (!chosen) {
                           chosen = movies.find(m => (m.title || '').toLowerCase().startsWith(termLower));
                       }
                       if (!chosen) {
                           chosen = movies[0];
                       }
                       selectMovieFromSearch(chosen.id, true);
                   } catch(err) {
                       console.error('Error searching movie', err);
                       container.innerHTML = '<span class="text-danger">Lỗi tìm kiếm phim</span>';
                   }
               }

               async function selectMovieFromSearch(movieId, auto = false) {
                   hideMoviePreview();
                   clearCalendarHighlight();
                   highlightMovieInCalendar(movieId);

                   const container = document.getElementById('movieSearchResult');
                   if (!container) return;

                   container.dataset.currentMovieId = movieId;

                   container.innerHTML = '<div class="text-center"><i class="fas fa-spinner fa-spin"></i></div>';

                   try {
                       const resp = await fetch(`${apiBaseUrl}/api/v1/movie/GetById?movieId=${movieId}`);
                       const result = await resp.json();
                       const movie = result.data || result;

                       const poster = movie.primaryImageUrl || (Array.isArray(movie.images)? movie.images.find(i => i.isPrimary)?.imageUrl : null) || '/images/no-poster.jpg';
                       const title = movie.title || 'Không rõ tên';

                       // Lấy showtimes theo showtimeData của tháng hiện tại
                       const times = showtimeData
                           .filter(st => st.movieId === movieId)
                           .sort((a,b)=> new Date(a.showDate) - new Date(b.showDate) || a.startTime.localeCompare(b.startTime));

                       const timesHtml = times.length ?
                           '<ul style="list-style:none;padding-left:0;margin:6px 0 0 0;max-height:140px;overflow-y:auto;">' +
                           times.slice(0,20).map(t => `<li style="font-size:12px;">${new Date(t.showDate).toLocaleDateString('vi-VN')} ${formatTime(t.startTime)} - ${t.cinemaRoomName}</li>`).join('') +
                           '</ul>' : '<p class="text-muted mb-0" style="font-size:12px;">Không có lịch chiếu</p>';

                       container.innerHTML = `
                           <div class="d-flex gap-2 align-items-start movie-preview" style="display:flex;">
                               <img src="${poster}" alt="${title}" class="preview-poster" onerror="this.src='/images/no-poster.jpg'">
                               <div style="flex:1;">
                                   <strong>${title}</strong>
                                   ${timesHtml}
                               </div>
                           </div>`;
                       container.style.display = 'block';
                   } catch (err) {
                       console.error('Error preview movie', err);
                       container.innerHTML = '<span class="text-danger">Không thể tải thông tin phim</span>';
                   }
               }

               function hideMoviePreview() {
                   const container = document.getElementById('movieSearchResult');
                   if (container) container.style.display = 'none';
               }

               /*================ TABLE VIEW - PAGINATION ================*/
               let tablePageSize = 10;
               let currentTablePage = 1;
               let totalTablePages = 1;
               let totalItemsCount = 0;

               async function loadShowtimesTable(page = 1) {
                   const tbody = document.getElementById('showtimeTableBody');
                   if (!tbody) return;
                   tbody.innerHTML = `<tr><td colspan="10" class="text-center py-4"><i class="fas fa-spinner fa-spin"></i></td></tr>`;

                   try {
                       tablePageSize = parseInt(document.getElementById('pageSizeSelect')?.value || 10);
                       const resp = await fetch(`${apiBaseUrl}/api/v1/showtime?page=${page}&pageSize=${tablePageSize}`);
                       const result = await resp.json();
                       const data = result.data || result.Data || result;
                       const items = data.items || data.Items || data;
                       totalTablePages = data.totalPages || data.TotalPages || 1;
                       totalItemsCount = data.totalItems || data.TotalItems || 0;
                       currentTablePage = data.currentPage || data.CurrentPage || page;

                       if (!Array.isArray(items) || !items.length) {
                           tbody.innerHTML = `<tr><td colspan="10" class="text-center py-4 text-muted">Không có dữ liệu</td></tr>`;
                       } else {
                           tbody.innerHTML = items.map(renderTableRow).join('');
                       }

                       updatePaginationUI();
                   } catch(err) {
                       console.error('Error load table', err);
                       tbody.innerHTML = `<tr><td colspan="10" class="text-center text-danger">Lỗi tải dữ liệu</td></tr>`;
                   }
               }

               function renderTableRow(item) {
                   const poster = item.moviePoster || '/images/no-poster.jpg';
                   const occupancyRate = item.totalSeats ? (item.bookedSeats / item.totalSeats) * 100 : 0;
                   const occupancyText = occupancyRate >= 90 ? 'Hết vé' : occupancyRate >= 70 ? 'Sắp hết vé' : 'Còn vé';
                   const statusText = new Date(item.showDate).toDateString() === new Date().toDateString() ? 'Hôm nay' : (new Date(item.showDate) < new Date() ? 'Đã qua' : 'Sắp tới');

                   return `
                       <tr>
                           <td><input type="checkbox" value="${item.id}" onchange="updateBulkActionButtons()"></td>
                           <td>
                               <div class="d-flex gap-2 align-items-center movie-details">
                                   <img src="${poster}" alt="${item.movieTitle}" class="movie-poster" style="width:40px;height:60px;object-fit:cover;border-radius:4px;">
                                   <div>
                                       <h6 class="mb-0">${item.movieTitle}</h6>
                                       <small>${item.movieDuration} phút</small>
                                   </div>
                               </div>
                           </td>
                           <td>${item.cinemaRoomName}</td>
                           <td>${new Date(item.showDate).toLocaleDateString('vi-VN')}</td>
                           <td>${formatTime(item.startTime)}</td>
                           <td>${formatTime(item.endTime)}</td>
                           <td><span class="text-success fw-bold">${(item.price || 0).toLocaleString('vi-VN')} VNĐ</span></td>
                           <td>${statusText}</td>
                           <td>${item.bookedSeats}/${item.totalSeats} vé (${Math.round(occupancyRate)}%)</td>
                           <td><!-- actions placeholder --></td>
                       </tr>`;
               }

               function updatePaginationUI() {
                   // Info text
                   const infoEl = document.getElementById('paginationInfo');
                   if (infoEl) {
                       const startItem = totalItemsCount === 0 ? 0 : (currentTablePage - 1) * tablePageSize + 1;
                       const endItem = Math.min(currentTablePage * tablePageSize, totalItemsCount);
                       infoEl.textContent = `Trang ${currentTablePage}/${totalTablePages} (Hiển thị ${startItem}-${endItem} của ${totalItemsCount})`;
                   }

                   // Prev/next buttons
                   document.getElementById('prevPageBtn')?.toggleAttribute('disabled', currentTablePage <= 1);
                   document.getElementById('nextPageBtn')?.toggleAttribute('disabled', currentTablePage >= totalTablePages);

                   // Page numbers - condensed with ellipsis
                   const pagesWrapper = document.getElementById('pageNumbers');
                   if (!pagesWrapper) return;
                   pagesWrapper.innerHTML = '';

                   const createPageBtn = (num, isActive = false) => {
                       const el = document.createElement('div');
                       el.className = 'page-number' + (isActive ? ' active' : '');
                       el.textContent = num;
                       el.onclick = () => loadShowtimesTable(num);
                       return el;
                   };

                   const createEllipsis = () => {
                       const span = document.createElement('span');
                       span.textContent = '...';
                       span.style.padding = '8px 12px';
                       span.style.color = 'var(--text-muted)';
                       return span;
                   };

                   if (totalTablePages <= 10) {
                       for (let i = 1; i <= totalTablePages; i++) {
                           pagesWrapper.appendChild(createPageBtn(i, i === currentTablePage));
                       }
                   } else {
                       pagesWrapper.appendChild(createPageBtn(1, currentTablePage === 1));

                       if (currentTablePage <= 4) {
                           for (let i = 2; i <= 5; i++) {
                               pagesWrapper.appendChild(createPageBtn(i, i === currentTablePage));
                           }
                           pagesWrapper.appendChild(createEllipsis());
                       } else if (currentTablePage >= totalTablePages - 3) {
                           pagesWrapper.appendChild(createEllipsis());
                           for (let i = totalTablePages - 4; i < totalTablePages; i++) {
                               pagesWrapper.appendChild(createPageBtn(i, i === currentTablePage));
                           }
                       } else {
                           pagesWrapper.appendChild(createEllipsis());
                           for (let i = currentTablePage - 1; i <= currentTablePage + 1; i++) {
                               pagesWrapper.appendChild(createPageBtn(i, i === currentTablePage));
                           }
                           pagesWrapper.appendChild(createEllipsis());
                       }

                       pagesWrapper.appendChild(createPageBtn(totalTablePages, currentTablePage === totalTablePages));
                   }
               }

               function changePageSize() {
                   currentTablePage = 1;
                   loadShowtimesTable(1);
               }

        /*============== END TABLE PAGINATION =============*/


               document.addEventListener('DOMContentLoaded', function() {
                   console.log('Showtime Management Dashboard loaded');
                   // Load dữ liệu tháng hiện tại sau đó render lịch
                   loadMonthlyData().then(generateCalendar);

                   // Gắn submit handler cho form tạo lịch chiếu mới
                   const form = document.getElementById('createNewShowtimeForm');
                   if (form) {
                       form.addEventListener('submit', function (e) {
                           e.preventDefault(); // Ngăn reload trang
                           form.classList.add('was-validated');
                           saveNewShowtime(); // Gọi hàm lưu
                       });
                   }
               });

               function initializeShowtimeTooltips() {
                   const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
                   tooltipTriggerList.map(el => new bootstrap.Tooltip(el));
               }

    