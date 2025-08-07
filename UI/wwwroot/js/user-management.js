


const API_BASE_URL = '/api';


function extractUserDataFromRow(row) {
    const cells = row.cells;
    const userId = row.getAttribute('data-user-id');
    
    return {
        userId: userId,
        fullName: cells[0].querySelector('.user-name')?.textContent || '',
        email: cells[0].querySelector('.user-email')?.textContent || '',
        role: cells[1].querySelector('.badge')?.textContent || '',
        status: cells[2].querySelector('.status-badge')?.textContent || '',
        createdAt: cells[3]?.textContent || ''
    };
}


async function viewUser(userId) {
    try {
        const response = await fetch(`${API_BASE_URL}/User/${userId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });
        
        const result = await response.json();
        
        if (result.success && result.data) {
            const user = result.data;
            populateViewModal(user);
            
            const modal = new bootstrap.Modal(document.getElementById('viewUserModal'));
            modal.show();
        } else {
            alert('Không thể tải thông tin người dùng: ' + (result.message || 'Lỗi không xác định'));
        }
    } catch (error) {
        console.error('Error viewing user:', error);
        alert('Đã xảy ra lỗi khi tải thông tin người dùng');
    }
}


function populateViewModal(user) {
    document.getElementById('viewUserName').textContent = user.fullName || '';
    document.getElementById('viewUsername').textContent = user.username || '';
    document.getElementById('viewEmail').textContent = user.email || '';
    document.getElementById('viewPhone').textContent = user.phone || 'Chưa cập nhật';
    document.getElementById('viewAddress').textContent = user.address || 'Chưa cập nhật';
    document.getElementById('viewScore').textContent = user.score || '0';
    document.getElementById('viewCreatedAt').textContent = user.createdAt ? 
        new Date(user.createdAt).toLocaleDateString('vi-VN') : '';
    

    const roleBadge = document.getElementById('viewUserRole');
    const roleText = user.role === 'Admin' ? 'Quản trị viên' : 
                    user.role === 'Staff' ? 'Nhân viên' : 'Thành viên';
    const roleClass = user.role === 'Admin' ? 'bg-danger' : 
                     user.role === 'Staff' ? 'bg-warning' : 'bg-success';
    
    roleBadge.textContent = roleText;
    roleBadge.className = `badge ${roleClass}`;
    

    const avatarElement = document.getElementById('viewUserAvatar');
    if (avatarElement && user.fullName) {
        avatarElement.textContent = user.fullName.charAt(0).toUpperCase();
        avatarElement.style.cssText = 'width: 80px; height: 80px; border-radius: 50%; background: linear-gradient(135deg, #667eea, #764ba2); color: white; display: flex; align-items: center; justify-content: center; font-size: 2rem; font-weight: bold; margin: 0 auto;';
    }
}


async function editUser(userId) {
    try {

        const response = await fetch(`${API_BASE_URL}/User/${userId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });
        
        const result = await response.json();
        
        if (result.success && result.data) {
            const user = result.data;
            populateEditModal(user);
            
            const modal = new bootstrap.Modal(document.getElementById('editUserModal'));
            modal.show();
        } else {
            alert('Không thể tải thông tin người dùng để chỉnh sửa: ' + (result.message || 'Lỗi không xác định'));
        }
    } catch (error) {
        console.error('Error loading user for edit:', error);
        alert('Đã xảy ra lỗi khi tải thông tin người dùng');
    }
}


function populateEditModal(user) {
    document.getElementById('editUserId').value = user.userId || '';
    document.getElementById('editUsername').value = user.username || '';
    document.getElementById('editFullName').value = user.fullName || '';
    document.getElementById('editEmail').value = user.email || '';
    document.getElementById('editPhone').value = user.phone || '';
    document.getElementById('editAddress').value = user.address || '';
    document.getElementById('editRole').value = user.role || '';
    document.getElementById('editScore').value = user.score || 0;
    document.getElementById('editPassword').value = ''; // Always empty for security
}


async function updateUser() {
    const userId = document.getElementById('editUserId').value;
    const formData = {
        fullName: document.getElementById('editFullName').value,
        email: document.getElementById('editEmail').value,
        phone: document.getElementById('editPhone').value,
        address: document.getElementById('editAddress').value,
        role: document.getElementById('editRole').value,
        score: parseFloat(document.getElementById('editScore').value) || 0
    };
    

    const password = document.getElementById('editPassword').value;
    if (password) {
        formData.newPassword = password;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/User/${userId}`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData)
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert('Cập nhật người dùng thành công!');
            

            const modal = bootstrap.Modal.getInstance(document.getElementById('editUserModal'));
            modal.hide();
            

            location.reload();
        } else {
            alert('Cập nhật thất bại: ' + (result.message || 'Lỗi không xác định'));
        }
    } catch (error) {
        console.error('Error updating user:', error);
        alert('Đã xảy ra lỗi khi cập nhật người dùng');
    }
}


async function toggleUserStatus(userId, currentStatus) {
    try {

        const userResponse = await fetch(`${API_BASE_URL}/User/${userId}`);
        const userResult = await userResponse.json();
        
        if (!userResult.success) {
            alert('Không thể tải thông tin người dùng');
            return;
        }
        
        const user = userResult.data;
        

        document.getElementById('toggleUserName').textContent = user.fullName;
        document.getElementById('toggleCurrentStatus').textContent = currentStatus ? 'Hoạt động' : 'Không hoạt động';
        document.getElementById('toggleNewStatus').textContent = currentStatus ? 'Không hoạt động' : 'Hoạt động';
        

        document.getElementById('toggleUserId').value = userId;
        

        const modal = new bootstrap.Modal(document.getElementById('toggleStatusModal'));
        modal.show();
        
    } catch (error) {
        console.error('Error preparing status toggle:', error);
        alert('Đã xảy ra lỗi khi chuẩn bị thay đổi trạng thái');
    }
}


async function confirmToggleStatus() {
    const userId = document.getElementById('toggleUserId').value;
    
    try {
        const response = await fetch(`${API_BASE_URL}/User/${userId}/status`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
            }
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert('Thay đổi trạng thái thành công!');
            

            const modal = bootstrap.Modal.getInstance(document.getElementById('toggleStatusModal'));
            modal.hide();
            

            location.reload();
        } else {
            alert('Thay đổi trạng thái thất bại: ' + (result.message || 'Lỗi không xác định'));
        }
    } catch (error) {
        console.error('Error toggling user status:', error);
        alert('Đã xảy ra lỗi khi thay đổi trạng thái người dùng');
    }
}


async function deleteUser(userId) {
    try {

        const userResponse = await fetch(`${API_BASE_URL}/User/${userId}`);
        const userResult = await userResponse.json();
        
        if (!userResult.success) {
            alert('Không thể tải thông tin người dùng');
            return;
        }
        
        const user = userResult.data;
        

        document.getElementById('deleteUserName').textContent = user.fullName;
        document.getElementById('deleteUserEmail').textContent = user.email;
        document.getElementById('deleteUserId').value = userId;
        

        const modal = new bootstrap.Modal(document.getElementById('deleteUserModal'));
        modal.show();
        
    } catch (error) {
        console.error('Error preparing user deletion:', error);
        alert('Đã xảy ra lỗi khi chuẩn bị xóa người dùng');
    }
}


async function confirmDeleteUser() {
    const userId = document.getElementById('deleteUserId').value;
    
    try {
        const response = await fetch(`${API_BASE_URL}/User/${userId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
            }
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert('Xóa người dùng thành công!');
            

            const modal = bootstrap.Modal.getInstance(document.getElementById('deleteUserModal'));
            modal.hide();
            

            location.reload();
        } else {
            alert('Xóa người dùng thất bại: ' + (result.message || 'Lỗi không xác định'));
        }
    } catch (error) {
        console.error('Error deleting user:', error);
        alert('Đã xảy ra lỗi khi xóa người dùng');
    }
}


async function addUser() {
    const formData = {
        Username: document.getElementById('newUsername').value.trim(),
        FullName: document.getElementById('newFullName').value.trim(),
        Email: document.getElementById('newEmail').value.trim(),
        Phone: document.getElementById('newPhone').value.trim(),
        IdentityCard: document.getElementById('newIdentityCard').value.trim(),
        identityCard: document.getElementById('newIdentityCard').value.trim(),
        Address: document.getElementById('newAddress').value.trim(),
        Password: document.getElementById('newPassword').value,
        Role: document.getElementById('newRole').value
    };
    

    if (!formData.Username || !formData.FullName || !formData.Email || !formData.Phone || !formData.IdentityCard || !formData.Address || !formData.Password || !formData.Role) {
        alert('Vui lòng điền đầy đủ các trường bắt buộc!');
        return;
    }
    
    console.log('Create user payload:', formData);
    try {
        const response = await fetch(`${API_BASE_URL}/User`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData)
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert('Thêm người dùng thành công!');
            

            const modal = bootstrap.Modal.getInstance(document.getElementById('addUserModal'));
            modal.hide();
            

            document.getElementById('addUserForm').reset();
            

            location.reload();
        } else {
            alert('Thêm người dùng thất bại: ' + (result.message || 'Lỗi không xác định'));
        }
    } catch (error) {
        console.error('Error adding user:', error);
        alert('Đã xảy ra lỗi khi thêm người dùng');
    }
}


function openAddUserModal() {
    const modal = new bootstrap.Modal(document.getElementById('addUserModal'));
    modal.show();
}


function editUserFromView() {

    const viewModal = bootstrap.Modal.getInstance(document.getElementById('viewUserModal'));
    viewModal.hide();
    

    const userName = document.getElementById('viewUserName').textContent;
    const userEmail = document.getElementById('viewEmail').textContent;
    

    const rows = document.querySelectorAll('#usersTableBody tr');
    for (let row of rows) {
        const rowEmail = row.querySelector('.user-email')?.textContent;
        if (rowEmail === userEmail) {
            const userId = row.getAttribute('data-user-id');
            if (userId) {
                editUser(userId);
                break;
            }
        }
    }
}


document.addEventListener('DOMContentLoaded', function() {
    // ---- Pagination Logic ----
    const tableBody = document.getElementById('usersTableBody');
    if (tableBody) {
        const allRows = Array.from(tableBody.querySelectorAll('tr'));
        const pageSizeSelect = document.getElementById('pageSizeSelect');
        const pageNumbersContainer = document.getElementById('pageNumbers');
        const paginationInfo = document.getElementById('paginationInfo');
        const prevPageBtn = document.getElementById('prevPageBtn');
        const nextPageBtn = document.getElementById('nextPageBtn');

        let currentPage = 1;
        let pageSize = parseInt(pageSizeSelect ? pageSizeSelect.value : 5);

        function totalPages() {
            return Math.max(1, Math.ceil(allRows.length / pageSize));
        }

        function renderPageNumbers() {
            pageNumbersContainer.innerHTML = '';
            const total = totalPages();
            for (let i = 1; i <= total; i++) {
                const btn = document.createElement('button');
                btn.textContent = i;
                btn.className = 'page-number' + (i === currentPage ? ' active' : '');
                btn.addEventListener('click', () => {
                    currentPage = i;
                    renderTable();
                });
                pageNumbersContainer.appendChild(btn);
            }
        }

        function renderTable() {
            // Hide all rows first
            allRows.forEach(r => r.style.display = 'none');
            const start = (currentPage - 1) * pageSize;
            const end = start + pageSize;
            allRows.slice(start, end).forEach(r => r.style.display = '');

            // Update info
            if (paginationInfo) {
                paginationInfo.textContent = `Hiển thị ${start + 1} - ${Math.min(end, allRows.length)} của ${allRows.length} người dùng`;
            }

            // Update buttons
            prevPageBtn.disabled = currentPage === 1;
            nextPageBtn.disabled = currentPage === totalPages();

            // Re-render page numbers for active state
            renderPageNumbers();
        }

        // Initial render
        renderTable();

        // Event listeners
        if (prevPageBtn) {
            prevPageBtn.addEventListener('click', () => {
                if (currentPage > 1) {
                    currentPage--;
                    renderTable();
                }
            });
        }

        if (nextPageBtn) {
            nextPageBtn.addEventListener('click', () => {
                if (currentPage < totalPages()) {
                    currentPage++;
                    renderTable();
                }
            });
        }

        if (pageSizeSelect) {
            pageSizeSelect.addEventListener('change', () => {
                pageSize = parseInt(pageSizeSelect.value);
                currentPage = 1;
                renderTable();
            });
        }
    }
    // ---- End Pagination Logic ----

    setTimeout(function() {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(alert => {
            if (alert.classList.contains('alert-danger') || alert.classList.contains('alert-success')) {
                alert.style.transition = 'opacity 0.5s';
                alert.style.opacity = '0';
                setTimeout(() => alert.remove(), 500);
            }
        });
    }, 5000);
});