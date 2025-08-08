# ShowtimeManagement (UI Area)

Tài liệu này giải thích **chi tiết** cách trang **Quản lý lịch chiếu** vận hành sau khi refactor, bao gồm kiến trúc, luồng dữ liệu, các điểm mở rộng và gợi ý phát triển.

---
## 1. Kiến trúc tổng thể
```
UI (Razor) ──▶ Controller (ShowtimesController) ──▶ ShowtimeService ──▶ ApiService ──▶ Backend WebAPI / DB
                 ▲                                                       │
                 │<───── JSON Response <──────────────────────────────────┘
``` 
1. **View (`Index.cshtml`)**
   - Gần như thuần HTML/Razor, KHÔNG chứa logic nghiệp vụ.
   - Truyền `Model.Showtimes` cho JavaScript qua biến global `window.initialShowtimesJson`.
   - Gắn file JS ngoài: `~/js/ShowTimeManagement/index.js`.

2. **External JS (`index.js`)**
   - Xử lý UI/UX, gọi API Controller, hiển thị toast, validate…
   - Không còn Razor, dễ tái sử dụng và test.

3. **Controller (`ShowtimesController`)**
   - Cung cấp REST endpoint nội bộ cho FE (JSON):
     | Phương thức | Đường dẫn                       | Chức năng |
     |-------------|---------------------------------|-----------|
     | GET         | `GetMonthlyData`               | Lấy lịch theo tháng (calendar) |
     | GET         | `GetShowtimesPage`             | Phân trang bảng |
     | GET         | `GetMoviesDropdown` / `GetCinemaRoomsDropdown` |
     | GET         | `ProxyCheckConflict`           | Kiểm tra trùng lịch |
     | POST        | `create-new-json`              | Tạo lịch chiếu |
   - Bảo mật (`[Authorize]`) và ánh xạ ViewModel ↔ Service.

4. **Service (`ShowtimeService`)**
   - Đóng gói logic nghiệp vụ UI-side, làm việc với `IApiService`.

5. **ApiService**
   - Wrapper HttpClient dùng chung toàn UI.

---
## 2. Luồng dữ liệu chính
### 2.1 Load trang
1. Razor render, gán `window.initialShowtimesJson`.
2. `index.js` chạy → `loadMonthlyData()` fetch `GetMonthlyData`.
3. Dữ liệu -> `showtimeData` → `generateCalendar()` vẽ lịch.

### 2.2 Calendar
- `generateCalendar()` tạo lưới ngày; `generateShowtimesForDay()` nhúng các suất chiếu.
- Chuyển tháng `changeMonth()` → refetch JSON → vẽ lại.

### 2.3 Table (Bảng)
1. `switchView('table')` → `loadShowtimesTable(page)`.
2. Fetch `GetShowtimesPage` với `pageSize`.
3. Render `<tbody>` qua `renderTableRow()`.
4. `updatePaginationUI()` hiển thị phân trang rút gọn: `1 … 10 11 12 … 1460`.

### 2.4 CRUD
- **Thêm/Sửa**: Modal `_CreateNewShowtimeModal` → `saveNewShowtime()` → POST/PUT `create-new-json`.
- **Xóa**: `deleteShowtime(id)` (hiện gọi thẳng backend `DELETE /api/v1/showtime/{id}` – có thể proxy qua Controller nếu muốn).
- **Check Conflict**: gọi `ProxyCheckConflict` (Controller tính `duration`).

---
## 3. File & Thư mục quan trọng
```
UI/
└─ Areas/
   └─ ShowtimeManagement/
      ├─ Views/Showtimes/Index.cshtml    // View chính
      ├─ Services/ShowtimeService.cs     // FE service
      ├─ Controllers/ShowtimesController.cs
      ├─ Models/ShowtimeViewModels.cs
      └─ README.md   // (file này)
wwwroot/
└─ js/ShowTimeManagement/index.js       // Logic FE
└─ css/ShowTimeManagement/index.css      // Style riêng
```

---
## 4. Pagination rút gọn – Thuật toán
```js
if (totalPages <= 10) renderAll();
else {
  render(1);
  if (currentPage <= 4) {
    render(2..5); renderEllipsis();
  } else if (currentPage >= totalPages-3) {
    renderEllipsis(); render(totalPages-4..totalPages-1);
  } else {
    renderEllipsis(); render(current-1..current+1); renderEllipsis();
  }
  render(totalPages);
}
```
→ đảm bảo thanh phân trang không tràn ngang.

---
## 5. Mở rộng / Bảo trì
1. **Chuyển sang SPA**: Có thể giữ nguyên Controller JSON, chỉ thay UI bằng React/Vue; endpoint đã sẵn REST.
2. **Logic backend**: Muốn xử lý trực tiếp DB, chỉ cần sửa `ShowtimeService` không chạm FE.
3. **Unit test**: Mock `IApiService` để test `ShowtimeService`; test JS bằng Jest/Playwright.
4. **Phân quyền**: Controller đã `[Authorize(Roles="Admin,Staff")]`; thêm Claims dễ dàng.

---
## 6. FAQ
- **Vì sao còn một vài call tới `/api/v1/showtime` trực tiếp?**
  • Xóa nhanh 1 bản ghi dùng DELETE trực tiếp để tránh proxy; có thể chuyển sang Controller nếu cần log/authorize.
- **Tại sao vẫn dùng inline script nhỏ?**
  • Chỉ để truyền JSON ban đầu; tránh phải fetch lần đầu, và không vi phạm separation-of-concerns.
- **Có thể lazy-load lịch từng tuần?**
  • Hoàn toàn được: thêm `GetWeeklyData`, sửa `generateCalendar()` để gọi khi click ngày.

---
_© Cinema City admin panel – ShowtimeManagement module._ 