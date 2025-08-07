/**
 * Demo cho Shared Action Buttons
 * File này minh họa cách sử dụng các helper functions
 */

// Demo 1: Tạo nút đơn lẻ
function demoSingleButtons() {
    console.log('=== Demo Single Buttons ===');
    
    // Tạo nút xem chi tiết
    const viewBtn = createViewButton('viewItem(123)', 'Xem chi tiết phim');
    console.log('View Button:', viewBtn);
    
    // Tạo nút chỉnh sửa
    const editBtn = createEditButton('editItem(123)', 'Chỉnh sửa phim');
    console.log('Edit Button:', editBtn);
    
    // Tạo nút xóa
    const deleteBtn = createDeleteButton('deleteItem(123)', 'Xóa phim');
    console.log('Delete Button:', deleteBtn);
    
    // Tạo nút quản lý
    const manageBtn = createManageButton('manageItem(123)', 'Quản lý ghế');
    console.log('Manage Button:', manageBtn);
}

// Demo 2: Tạo nhóm nút CRUD
function demoCRUDActions() {
    console.log('=== Demo CRUD Actions ===');
    
    const actionConfig = {
        view: {
            onClick: 'viewMovie(123)',
            title: 'Xem chi tiết phim'
        },
        edit: {
            onClick: 'editMovie(123)',
            title: 'Chỉnh sửa phim'
        },
        delete: {
            onClick: 'deleteMovie(123)',
            title: 'Xóa phim'
        }
    };
    
    // Tạo nhóm nút với kiểu icon
    const iconGroup = createCRUDActions(actionConfig, ButtonStyle.ICON);
    console.log('Icon Style Group:', iconGroup);
    
    // Tạo nhóm nút với kiểu action
    const actionGroup = createCRUDActions(actionConfig, ButtonStyle.ACTION);
    console.log('Action Style Group:', actionGroup);
}

// Demo 3: Tạo nút xác nhận xóa
function demoConfirmDelete() {
    console.log('=== Demo Confirm Delete ===');
    
    const confirmDeleteBtn = createConfirmDeleteButton(
        'Phim "Oppenheimer"',
        'deleteMovie',
        'movie-123'
    );
    console.log('Confirm Delete Button:', confirmDeleteBtn);
}

// Demo 4: Quản lý trạng thái nút
function demoButtonStates() {
    console.log('=== Demo Button States ===');
    
    // Tạo một nút để demo
    const button = document.createElement('button');
    button.className = 'btn-icon btn-view';
    button.innerHTML = '<i class="fas fa-eye"></i>';
    
    // Demo loading state
    setButtonLoading(button);
    console.log('Button with loading state:', button.outerHTML);
    
    // Xóa loading state
    removeButtonLoading(button);
    console.log('Button without loading state:', button.outerHTML);
}

// Demo 5: Tạo bảng với action buttons
function demoTableWithActions() {
    console.log('=== Demo Table with Actions ===');
    
    const tableHTML = `
        <table class="table">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Tên</th>
                    <th>Thao tác</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>1</td>
                    <td>Phim A</td>
                    <td>${createCRUDActions({
                        view: { onClick: 'viewMovie(1)', title: 'Xem chi tiết' },
                        edit: { onClick: 'editMovie(1)', title: 'Chỉnh sửa' },
                        delete: { onClick: 'deleteMovie(1)', title: 'Xóa' }
                    }, ButtonStyle.ICON)}</td>
                </tr>
                <tr>
                    <td>2</td>
                    <td>Phim B</td>
                    <td>${createCRUDActions({
                        view: { onClick: 'viewMovie(2)', title: 'Xem chi tiết' },
                        edit: { onClick: 'editMovie(2)', title: 'Chỉnh sửa' },
                        delete: { onClick: 'deleteMovie(2)', title: 'Xóa' }
                    }, ButtonStyle.ACTION)}</td>
                </tr>
            </tbody>
        </table>
    `;
    
    console.log('Table with Actions:', tableHTML);
}

// Demo 6: Tạo action buttons cho phòng chiếu
function demoCinemaRoomActions() {
    console.log('=== Demo Cinema Room Actions ===');
    
    const roomActions = createCRUDActions({
        view: {
            onClick: 'openDetailsModal("room-123")',
            title: 'Xem chi tiết phòng'
        },
        edit: {
            onClick: 'openEditModal("room-123")',
            title: 'Chỉnh sửa phòng'
        },
        manage: {
            onClick: 'openManageSeatsModal("room-123")',
            title: 'Quản lý ghế'
        },
        delete: {
            onClick: 'confirmDelete("room-123", "Phòng A01", "deleteRoom")',
            title: 'Xóa phòng'
        }
    }, ButtonStyle.ICON);
    
    console.log('Cinema Room Actions:', roomActions);
}

// Demo 7: Tạo action buttons cho khuyến mãi
function demoPromotionActions() {
    console.log('=== Demo Promotion Actions ===');
    
    const promotionActions = createCRUDActions({
        view: {
            onClick: 'viewPromotion("promo-123")',
            title: 'Xem chi tiết khuyến mãi'
        },
        edit: {
            onClick: 'editPromotion("promo-123")',
            title: 'Chỉnh sửa khuyến mãi'
        },
        delete: {
            onClick: 'deletePromotion("promo-123")',
            title: 'Xóa khuyến mãi'
        }
    }, ButtonStyle.ACTION);
    
    console.log('Promotion Actions:', promotionActions);
}

// Demo 8: Tạo action buttons với tùy chọn
function demoCustomOptions() {
    console.log('=== Demo Custom Options ===');
    
    const customButton = createActionButton(
        'view',
        'viewItem(123)',
        'Xem chi tiết với icon tùy chỉnh',
        ButtonStyle.ICON,
        {
            icon: 'fas fa-search',
            customClass: 'custom-view-btn',
            id: 'view-btn-123'
        }
    );
    
    console.log('Custom Button:', customButton);
}

// Demo 9: Tạo toggle button
function demoToggleButton() {
    console.log('=== Demo Toggle Button ===');
    
    const toggleBtn = createToggleButton(
        'toggleUserStatus(123)',
        'Bật/Tắt trạng thái người dùng',
        ButtonStyle.ACTION,
        {
            icon: 'fas fa-toggle-on'
        }
    );
    
    console.log('Toggle Button:', toggleBtn);
}

// Demo 10: Tạo action group với mixed styles
function demoMixedStyles() {
    console.log('=== Demo Mixed Styles ===');
    
    const mixedButtons = [
        createViewButton('viewItem(123)', 'Xem chi tiết', ButtonStyle.ICON),
        createEditButton('editItem(123)', 'Chỉnh sửa', ButtonStyle.ACTION),
        createDeleteButton('deleteItem(123)', 'Xóa', ButtonStyle.ICON)
    ];
    
    const mixedGroup = createActionGroup(mixedButtons, ButtonStyle.ICON);
    console.log('Mixed Style Group:', mixedGroup);
}

// Chạy tất cả demo
function runAllDemos() {
    console.log('🚀 Starting Shared Action Buttons Demo...\n');
    
    demoSingleButtons();
    console.log('\n');
    
    demoCRUDActions();
    console.log('\n');
    
    demoConfirmDelete();
    console.log('\n');
    
    demoButtonStates();
    console.log('\n');
    
    demoTableWithActions();
    console.log('\n');
    
    demoCinemaRoomActions();
    console.log('\n');
    
    demoPromotionActions();
    console.log('\n');
    
    demoCustomOptions();
    console.log('\n');
    
    demoToggleButton();
    console.log('\n');
    
    demoMixedStyles();
    console.log('\n');
    
    console.log('✅ Demo completed! Check the console for HTML output.');
}

// Export cho sử dụng
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        runAllDemos,
        demoSingleButtons,
        demoCRUDActions,
        demoConfirmDelete,
        demoButtonStates,
        demoTableWithActions,
        demoCinemaRoomActions,
        demoPromotionActions,
        demoCustomOptions,
        demoToggleButton,
        demoMixedStyles
    };
}

// Tự động chạy demo nếu được gọi trực tiếp
if (typeof window !== 'undefined' && window.location.href.includes('demo')) {
    // Chạy demo sau khi trang load xong
    document.addEventListener('DOMContentLoaded', function() {
        setTimeout(runAllDemos, 1000);
    });
} 