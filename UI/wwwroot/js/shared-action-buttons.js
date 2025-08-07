/**
 * Shared Action Buttons Helper
 * Hỗ trợ tạo các nút action một cách nhất quán trong admin dashboard
 */

// Enum cho các loại nút action
const ActionButtonType = {
    VIEW: 'view',
    EDIT: 'edit',
    DELETE: 'delete',
    MANAGE: 'manage',
    TOGGLE: 'toggle'
};

// Enum cho kiểu nút
const ButtonStyle = {
    ICON: 'icon',      // Kiểu btn-icon (quản lý phòng chiếu)
    ACTION: 'action'   // Kiểu btn-action (quản lý khuyến mãi)
};

/**
 * Tạo nút action
 * @param {string} type - Loại nút (view, edit, delete, manage, toggle)
 * @param {string} onClick - Hàm onclick
 * @param {string} title - Tooltip
 * @param {string} style - Kiểu nút (icon hoặc action)
 * @param {object} options - Các tùy chọn khác
 * @returns {string} HTML của nút
 */
function createActionButton(type, onClick, title, style = ButtonStyle.ICON, options = {}) {
    const {
        disabled = false,
        loading = false,
        customClass = '',
        icon = null,
        id = null
    } = options;

    // Xác định icon mặc định
    const defaultIcons = {
        [ActionButtonType.VIEW]: 'fas fa-eye',
        [ActionButtonType.EDIT]: 'fas fa-edit',
        [ActionButtonType.DELETE]: 'fas fa-trash',
        [ActionButtonType.MANAGE]: 'fas fa-chair',
        [ActionButtonType.TOGGLE]: 'fas fa-toggle-on'
    };

    const buttonIcon = icon || defaultIcons[type];
    const buttonClass = style === ButtonStyle.ICON ? 'btn-icon' : 'btn-action';
    const disabledAttr = disabled ? 'disabled' : '';
    const loadingClass = loading ? 'loading' : '';
    const idAttr = id ? `id="${id}"` : '';

    return `
        <button 
            ${idAttr}
            onclick="${onClick}" 
            class="${buttonClass} btn-${type} ${customClass} ${loadingClass}"
            title="${title}"
            ${disabledAttr}>
            <i class="${buttonIcon}"></i>
        </button>
    `;
}

/**
 * Tạo nhóm các nút action
 * @param {Array} buttons - Mảng các nút action
 * @param {string} style - Kiểu nút (icon hoặc action)
 * @returns {string} HTML của nhóm nút
 */
function createActionGroup(buttons, style = ButtonStyle.ICON) {
    const containerClass = style === ButtonStyle.ICON ? 'action-group' : 'action-buttons';
    
    return `
        <div class="${containerClass}">
            ${buttons.join('')}
        </div>
    `;
}

/**
 * Tạo nút xem chi tiết
 * @param {string} onClick - Hàm onclick
 * @param {string} title - Tooltip (mặc định: "Xem chi tiết")
 * @param {string} style - Kiểu nút
 * @param {object} options - Các tùy chọn khác
 * @returns {string} HTML của nút
 */
function createViewButton(onClick, title = 'Xem chi tiết', style = ButtonStyle.ICON, options = {}) {
    return createActionButton(ActionButtonType.VIEW, onClick, title, style, options);
}

/**
 * Tạo nút chỉnh sửa
 * @param {string} onClick - Hàm onclick
 * @param {string} title - Tooltip (mặc định: "Chỉnh sửa")
 * @param {string} style - Kiểu nút
 * @param {object} options - Các tùy chọn khác
 * @returns {string} HTML của nút
 */
function createEditButton(onClick, title = 'Chỉnh sửa', style = ButtonStyle.ICON, options = {}) {
    return createActionButton(ActionButtonType.EDIT, onClick, title, style, options);
}

/**
 * Tạo nút xóa
 * @param {string} onClick - Hàm onclick
 * @param {string} title - Tooltip (mặc định: "Xóa")
 * @param {string} style - Kiểu nút
 * @param {object} options - Các tùy chọn khác
 * @returns {string} HTML của nút
 */
function createDeleteButton(onClick, title = 'Xóa', style = ButtonStyle.ICON, options = {}) {
    return createActionButton(ActionButtonType.DELETE, onClick, title, style, options);
}

/**
 * Tạo nút quản lý
 * @param {string} onClick - Hàm onclick
 * @param {string} title - Tooltip (mặc định: "Quản lý")
 * @param {string} style - Kiểu nút
 * @param {object} options - Các tùy chọn khác
 * @returns {string} HTML của nút
 */
function createManageButton(onClick, title = 'Quản lý', style = ButtonStyle.ICON, options = {}) {
    return createActionButton(ActionButtonType.MANAGE, onClick, title, style, options);
}

/**
 * Tạo nút toggle
 * @param {string} onClick - Hàm onclick
 * @param {string} title - Tooltip (mặc định: "Bật/Tắt")
 * @param {string} style - Kiểu nút
 * @param {object} options - Các tùy chọn khác
 * @returns {string} HTML của nút
 */
function createToggleButton(onClick, title = 'Bật/Tắt', style = ButtonStyle.ACTION, options = {}) {
    return createActionButton(ActionButtonType.TOGGLE, onClick, title, style, options);
}

/**
 * Tạo bộ nút CRUD chuẩn (View, Edit, Delete)
 * @param {object} config - Cấu hình các nút
 * @param {string} style - Kiểu nút
 * @returns {string} HTML của nhóm nút
 */
function createCRUDActions(config, style = ButtonStyle.ICON) {
    const buttons = [];
    
    if (config.view) {
        buttons.push(createViewButton(config.view.onClick, config.view.title, style, config.view.options));
    }
    
    if (config.edit) {
        buttons.push(createEditButton(config.edit.onClick, config.edit.title, style, config.edit.options));
    }
    
    if (config.delete) {
        buttons.push(createDeleteButton(config.delete.onClick, config.delete.title, style, config.delete.options));
    }
    
    if (config.manage) {
        buttons.push(createManageButton(config.manage.onClick, config.manage.title, style, config.manage.options));
    }
    
    return createActionGroup(buttons, style);
}

/**
 * Tạo nút xác nhận xóa với SweetAlert2
 * @param {string} itemName - Tên item cần xóa
 * @param {string} deleteFunction - Tên hàm xóa
 * @param {string} itemId - ID của item
 * @returns {string} HTML của nút
 */
function createConfirmDeleteButton(itemName, deleteFunction, itemId) {
    const confirmMessage = `Bạn có chắc chắn muốn xóa ${itemName}?`;
    const onClick = `confirmDelete('${itemId}', '${itemName}', '${deleteFunction}')`;
    
    return createDeleteButton(onClick, 'Xóa', ButtonStyle.ICON);
}

/**
 * Hàm xác nhận xóa chung
 * @param {string} id - ID của item
 * @param {string} name - Tên của item
 * @param {string} deleteFunction - Tên hàm xóa
 */
function confirmDelete(id, name, deleteFunction) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'Xác nhận xóa',
            text: `Bạn có chắc chắn muốn xóa "${name}"?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#ef4444',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                // Gọi hàm xóa
                if (typeof window[deleteFunction] === 'function') {
                    window[deleteFunction](id);
                } else {
                    console.error(`Function ${deleteFunction} not found`);
                }
            }
        });
    } else {
        // Fallback nếu không có SweetAlert2
        if (confirm(`Bạn có chắc chắn muốn xóa "${name}"?`)) {
            if (typeof window[deleteFunction] === 'function') {
                window[deleteFunction](id);
            }
        }
    }
}

/**
 * Thêm loading state cho nút
 * @param {HTMLElement} button - Element nút
 */
function setButtonLoading(button) {
    button.classList.add('loading');
    button.disabled = true;
}

/**
 * Xóa loading state cho nút
 * @param {HTMLElement} button - Element nút
 */
function removeButtonLoading(button) {
    button.classList.remove('loading');
    button.disabled = false;
}

/**
 * Disable tất cả nút trong action group
 * @param {HTMLElement} actionGroup - Element action group
 */
function disableActionGroup(actionGroup) {
    const buttons = actionGroup.querySelectorAll('button');
    buttons.forEach(button => {
        button.disabled = true;
    });
}

/**
 * Enable tất cả nút trong action group
 * @param {HTMLElement} actionGroup - Element action group
 */
function enableActionGroup(actionGroup) {
    const buttons = actionGroup.querySelectorAll('button');
    buttons.forEach(button => {
        button.disabled = false;
    });
}

// Export cho sử dụng trong các module khác
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        ActionButtonType,
        ButtonStyle,
        createActionButton,
        createActionGroup,
        createViewButton,
        createEditButton,
        createDeleteButton,
        createManageButton,
        createToggleButton,
        createCRUDActions,
        createConfirmDeleteButton,
        confirmDelete,
        setButtonLoading,
        removeButtonLoading,
        disableActionGroup,
        enableActionGroup
    };
} 