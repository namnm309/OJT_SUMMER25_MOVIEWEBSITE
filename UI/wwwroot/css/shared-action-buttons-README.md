# Shared Action Buttons - Hướng dẫn sử dụng

## Tổng quan

Hệ thống Shared Action Buttons được thiết kế để đồng bộ giao diện các nút action (xem chi tiết, chỉnh sửa, xóa, quản lý) trong toàn bộ admin dashboard.

## Các file đã tạo

1. **CSS**: `~/css/shared-action-buttons.css` - Định nghĩa style cho các nút action
2. **JavaScript**: `~/js/shared-action-buttons.js` - Helper functions để tạo nút action
3. **Demo chung**: `~/js/shared-action-buttons-demo.js` - Demo và ví dụ sử dụng chung
4. **Demo Booking List**: `~/js/shared-action-buttons-booking-list-demo.js` - Demo cụ thể cho trang Danh sách đặt vé
5. **README**: `~/css/shared-action-buttons-README.md` - Hướng dẫn sử dụng

## Các kiểu nút action

### 1. Kiểu Icon (btn-icon) - Quản lý phòng chiếu
```html
<div class="action-group">
    <button class="btn-icon btn-view" title="Xem chi tiết">
        <i class="fas fa-eye"></i>
    </button>
    <button class="btn-icon btn-edit" title="Chỉnh sửa">
        <i class="fas fa-edit"></i>
    </button>
    <button class="btn-icon btn-delete" title="Xóa">
        <i class="fas fa-trash"></i>
    </button>
    <button class="btn-icon btn-manage" title="Quản lý">
        <i class="fas fa-chair"></i>
    </button>
</div>
```

### 2. Kiểu Action (btn-action) - Quản lý khuyến mãi
```html
<div class="action-buttons">
    <button class="btn-action btn-view" title="Xem chi tiết">
        <i class="fas fa-eye"></i>
    </button>
    <button class="btn-action btn-edit" title="Chỉnh sửa">
        <i class="fas fa-edit"></i>
    </button>
    <button class="btn-action btn-delete" title="Xóa">
        <i class="fas fa-trash"></i>
    </button>
</div>
```

## Sử dụng JavaScript Helper

### 1. Tạo nút đơn lẻ

```javascript
// Tạo nút xem chi tiết
const viewButton = createViewButton('viewItem(123)', 'Xem chi tiết');

// Tạo nút chỉnh sửa
const editButton = createEditButton('editItem(123)', 'Chỉnh sửa');

// Tạo nút xóa
const deleteButton = createDeleteButton('deleteItem(123)', 'Xóa');

// Tạo nút quản lý
const manageButton = createManageButton('manageItem(123)', 'Quản lý');
```

### 2. Tạo nhóm nút CRUD

```javascript
const actionConfig = {
    view: {
        onClick: 'viewItem(123)',
        title: 'Xem chi tiết'
    },
    edit: {
        onClick: 'editItem(123)',
        title: 'Chỉnh sửa'
    },
    delete: {
        onClick: 'deleteItem(123)',
        title: 'Xóa'
    },
    manage: {
        onClick: 'manageItem(123)',
        title: 'Quản lý'
    }
};

// Tạo nhóm nút với kiểu icon
const actionGroup = createCRUDActions(actionConfig, ButtonStyle.ICON);

// Tạo nhóm nút với kiểu action
const actionGroup2 = createCRUDActions(actionConfig, ButtonStyle.ACTION);
```

### 3. Tạo nút xác nhận xóa

```javascript
const deleteButton = createConfirmDeleteButton(
    'Phòng A01',           // Tên item
    'deleteRoom',          // Tên hàm xóa
    'room-123'             // ID của item
);
```

### 4. Quản lý trạng thái nút

```javascript
// Thêm loading state
setButtonLoading(button);

// Xóa loading state
removeButtonLoading(button);

// Disable tất cả nút trong action group
disableActionGroup(actionGroup);

// Enable tất cả nút trong action group
enableActionGroup(actionGroup);
```

## Các trang đã được cập nhật

1. **Admin Dashboard**: `~/Views/Dashboard/AdminDashboard.cshtml`
2. **Quản lý phòng chiếu**: `~/Areas/CinemaManagement/Views/CinemaRoom/Index.cshtml`
3. **Quản lý khuyến mãi**: `~/Areas/PromotionManagement/Views/Promotions/Index.cshtml`
4. **Quản lý thành viên**: `~/Areas/UserManagement/Views/Members/Index.cshtml`
5. **Quản lý phim**: `~/Areas/MovieManagement/Views/Movies/Index.cshtml`
6. **Quản lý đặt vé**: `~/Areas/BookingManagement/Views/BookingTicket/Index.cshtml`
7. **Danh sách đặt vé**: `~/Areas/BookingManagement/Views/BookingTicket/BookingList.cshtml`
8. **Quản lý lịch chiếu**: `~/Areas/ShowtimeManagement/Views/Showtimes/Index.cshtml`
9. **Layout chung**: `~/Views/Shared/_Layout.cshtml`

## Tính năng

### 1. Responsive Design
- Tự động điều chỉnh kích thước trên mobile
- Ẩn tooltip trên mobile để tránh che nội dung

### 2. Accessibility
- Hỗ trợ keyboard navigation
- Focus states với animation
- ARIA labels thông qua title attribute

### 3. Loading States
- Hiển thị spinner khi nút đang xử lý
- Disable nút trong quá trình xử lý

### 4. Tooltip
- Hiển thị tooltip khi hover
- Tự động ẩn trên mobile

### 5. Dark Mode Support
- Tự động điều chỉnh màu sắc theo theme hệ thống

## Màu sắc

- **Xem chi tiết**: Xanh dương (#3b82f6)
- **Chỉnh sửa**: Vàng cam (#f59e0b)
- **Xóa**: Đỏ (#ef4444)
- **Quản lý**: Xanh dương (#3b82f6)
- **Toggle**: Xanh lá (#10b981)

## Ví dụ sử dụng trong thực tế

### Trong quản lý phòng chiếu
```javascript
function renderActionButtons(roomId, roomName) {
    const actionConfig = {
        view: {
            onClick: `openDetailsModal('${roomId}')`,
            title: 'Xem chi tiết'
        },
        edit: {
            onClick: `openEditModal('${roomId}')`,
            title: 'Chỉnh sửa'
        },
        manage: {
            onClick: `openManageSeatsModal('${roomId}')`,
            title: 'Quản lý ghế'
        },
        delete: {
            onClick: `confirmDelete('${roomId}', '${roomName}', 'deleteRoom')`,
            title: 'Xóa'
        }
    };
    
    return createCRUDActions(actionConfig, ButtonStyle.ICON);
}
```

### Trong quản lý khuyến mãi
```javascript
function renderPromotionActions(promotionId) {
    const actionConfig = {
        view: {
            onClick: `viewPromotion('${promotionId}')`,
            title: 'Xem chi tiết'
        },
        edit: {
            onClick: `editPromotion('${promotionId}')`,
            title: 'Chỉnh sửa'
        },
        delete: {
            onClick: `deletePromotion('${promotionId}')`,
            title: 'Xóa'
        }
    };
    
    return createCRUDActions(actionConfig, ButtonStyle.ACTION);
}
```

## Lưu ý

1. Đảm bảo đã include CSS và JavaScript files trong layout
2. Sử dụng Font Awesome cho icons
3. Có thể tùy chỉnh màu sắc thông qua CSS variables
4. Hỗ trợ SweetAlert2 cho confirm dialogs
5. Tương thích với Bootstrap 5 