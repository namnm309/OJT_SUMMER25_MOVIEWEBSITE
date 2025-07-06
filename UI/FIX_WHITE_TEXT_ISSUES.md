# Sửa lỗi hiển thị chữ màu trắng trong trang đặt vé

## Vấn đề đã phát hiện
Trong trang đặt vé (BookingTicket), có nhiều phần tử text bị hiển thị màu trắng trên nền trắng, khiến người dùng không thể đọc được nội dung.

## Nguyên nhân
- Các file CSS đang sử dụng quá nhiều style `color: white` và `color: #fff`
- Conflict giữa các file CSS khác nhau (new-dashboard.css, book-ticket.css, v.v.)
- Thiếu override cho các text elements trong content area

## Giải pháp đã thực hiện

### 1. Tạo file CSS fix chuyên biệt
**File:** `UI/wwwroot/css/book-ticket-fix.css`
- Override tất cả text colors trong main content area thành màu tối
- Fix breadcrumb colors
- Fix page title và subtitle colors
- Fix booking steps text colors
- Fix card headers và content colors
- Fix form labels và input placeholders
- Fix seat legend và selected seats text
- Fix customer info và payment methods text
- Đảm bảo buttons có contrast tốt

### 2. Cập nhật file Index.cshtml
**File:** `UI/Areas/BookingManagement/Views/BookingTicket/Index.cshtml`
- Thêm link đến file `book-ticket-fix.css` sau file `book-ticket.css`
- Đảm bảo CSS fix được load cuối cùng để override các style cũ

### 3. Cập nhật file new-dashboard.css
**File:** `UI/wwwroot/css/new-dashboard.css`
- Thêm `color: var(--text-dark)` cho `.new-dashboard-main`
- Thêm breadcrumb fixes với colors phù hợp
- Đảm bảo main content area có text color mặc định là dark

## Các màu sắc được sử dụng
- **Text chính:** `#1e293b` (dark slate)
- **Text phụ:** `#64748b` (slate)
- **Text muted:** `#94a3b8` (light slate)
- **Primary color:** `#667eea` (purple)
- **Links:** `#667eea` (purple)

## Kiểm tra sau khi fix
Sau khi áp dụng các thay đổi này, tất cả text trong trang đặt vé sẽ:
- Hiển thị màu tối trên nền trắng
- Có contrast tốt, dễ đọc
- Breadcrumb có màu purple cho links, dark cho active item
- Form elements có placeholder và label rõ ràng
- Buttons giữ nguyên màu nền và text trắng để có contrast tốt

## Lưu ý
- File `book-ticket-fix.css` sử dụng `!important` để đảm bảo override các style cũ
- Các thay đổi chỉ ảnh hưởng đến trang đặt vé, không ảnh hưởng đến các trang khác
- Nếu có thêm elements mới, có thể cần bổ sung thêm styles vào file fix

## Files đã thay đổi
1. `UI/wwwroot/css/book-ticket-fix.css` (tạo mới)
2. `UI/Areas/BookingManagement/Views/BookingTicket/Index.cshtml` (cập nhật)
3. `UI/wwwroot/css/new-dashboard.css` (cập nhật)
4. `UI/FIX_WHITE_TEXT_ISSUES.md` (tạo mới - file này)

## Cách test
1. Chạy ứng dụng
2. Đăng nhập với quyền Admin hoặc Staff
3. Truy cập trang đặt vé: `/BookingManagement/BookingTicket`
4. Kiểm tra tất cả text đều hiển thị rõ ràng, không bị trắng
5. Kiểm tra breadcrumb, page title, form labels, buttons đều có màu phù hợp