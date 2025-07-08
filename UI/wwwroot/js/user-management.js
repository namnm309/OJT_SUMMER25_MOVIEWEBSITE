// User Management JavaScript Functions

// Base API URL - Update this if backend port changes
const API_BASE_URL = 'https://localhost:7049';

// Extract user data from table row
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

// 1. VIEW USER DETAILS
async function viewUser(userId) {
    try {
        const response = await fetch(`${API_BASE_URL}/api/User/${userId}`, {
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

// Populate view modal with user data
function populateViewModal(user) {
    document.getElementById('viewUserName').textContent = user.fullName || '';
    document.getElementById('viewUsername').textContent = user.username || '';
    document.getElementById('viewEmail').textContent = user.email || '';
    document.getElementById('viewPhone').textContent = user.phone || 'Chưa cập nhật';
    document.getElementById('viewAddress').textContent = user.address || 'Chưa cập nhật';
    document.getElementById('viewScore').textContent = user.score || '0';
    document.getElementById('viewCreatedAt').textContent = user.createdAt ? 
        new Date(user.createdAt).toLocaleDateString('vi-VN') : '';
    
    // Set role badge
    const roleBadge = document.getElementById('viewUserRole');
    const roleText = user.role === 'Admin' ? 'Quản trị viên' : 
                    user.role === 'Staff' ? 'Nhân viên' : 'Thành viên';
    const roleClass = user.role === 'Admin' ? 'bg-danger' : 
                     user.role === 'Staff' ? 'bg-warning' : 'bg-success';
    
    roleBadge.textContent = roleText;
    roleBadge.className = `badge ${roleClass}`;
    
    // Style avatar (if exists)
    const avatarElement = document.getElementById('viewUserAvatar');
    if (avatarElement && user.fullName) {
        avatarElement.textContent = user.fullName.charAt(0).toUpperCase();
        avatarElement.style.cssText = 'width: 80px; height: 80px; border-radius: 50%; background: linear-gradient(135deg, #667eea, #764ba2); color: white; display: flex; align-items: center; justify-content: center; font-size: 2rem; font-weight: bold; margin: 0 auto;';
    }
}

// 2. EDIT USER
async function editUser(userId) {
    try {
        // First get user details
        const response = await fetch(`${API_BASE_URL}/api/User/${userId}`, {
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

// Populate edit modal with user data
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

// Update user (called from edit modal)
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
    
    // Add password if provided
    const password = document.getElementById('editPassword').value;
    if (password) {
        formData.newPassword = password;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/User/${userId}`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData)
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert('Cập nhật người dùng thành công!');
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('editUserModal'));
            modal.hide();
            
            // Refresh page
            location.reload();
        } else {
            alert('Cập nhật thất bại: ' + (result.message || 'Lỗi không xác định'));
        }
    } catch (error) {
        console.error('Error updating user:', error);
        alert('Đã xảy ra lỗi khi cập nhật người dùng');
    }
}

// 3. TOGGLE USER STATUS
async function toggleUserStatus(userId, currentStatus) {
    try {
        // First get user details to show in confirmation
        const userResponse = await fetch(`${API_BASE_URL}/api/User/${userId}`);
        const userResult = await userResponse.json();
        
        if (!userResult.success) {
            alert('Không thể tải thông tin người dùng');
            return;
        }
        
        const user = userResult.data;
        
        // Populate confirmation modal
        document.getElementById('toggleUserName').textContent = user.fullName;
        document.getElementById('toggleCurrentStatus').textContent = currentStatus ? 'Hoạt động' : 'Không hoạt động';
        document.getElementById('toggleNewStatus').textContent = currentStatus ? 'Không hoạt động' : 'Hoạt động';
        
        // Store userId for confirmation
        document.getElementById('toggleUserId').value = userId;
        
        // Show confirmation modal
        const modal = new bootstrap.Modal(document.getElementById('toggleStatusModal'));
        modal.show();
        
    } catch (error) {
        console.error('Error preparing status toggle:', error);
        alert('Đã xảy ra lỗi khi chuẩn bị thay đổi trạng thái');
    }
}

// Confirm toggle user status
async function confirmToggleStatus() {
    const userId = document.getElementById('toggleUserId').value;
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/User/${userId}/status`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
            }
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert('Thay đổi trạng thái thành công!');
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('toggleStatusModal'));
            modal.hide();
            
            // Refresh page
            location.reload();
        } else {
            alert('Thay đổi trạng thái thất bại: ' + (result.message || 'Lỗi không xác định'));
        }
    } catch (error) {
        console.error('Error toggling user status:', error);
        alert('Đã xảy ra lỗi khi thay đổi trạng thái người dùng');
    }
}

// 4. DELETE USER
async function deleteUser(userId) {
    try {
        // First get user details to show in confirmation
        const userResponse = await fetch(`${API_BASE_URL}/api/User/${userId}`);
        const userResult = await userResponse.json();
        
        if (!userResult.success) {
            alert('Không thể tải thông tin người dùng');
            return;
        }
        
        const user = userResult.data;
        
        // Populate confirmation modal
        document.getElementById('deleteUserName').textContent = user.fullName;
        document.getElementById('deleteUserEmail').textContent = user.email;
        document.getElementById('deleteUserId').value = userId;
        
        // Show confirmation modal
        const modal = new bootstrap.Modal(document.getElementById('deleteUserModal'));
        modal.show();
        
    } catch (error) {
        console.error('Error preparing user deletion:', error);
        alert('Đã xảy ra lỗi khi chuẩn bị xóa người dùng');
    }
}

// Confirm delete user
async function confirmDeleteUser() {
    const userId = document.getElementById('deleteUserId').value;
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/User/${userId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
            }
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert('Xóa người dùng thành công!');
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('deleteUserModal'));
            modal.hide();
            
            // Refresh page
            location.reload();
        } else {
            alert('Xóa người dùng thất bại: ' + (result.message || 'Lỗi không xác định'));
        }
    } catch (error) {
        console.error('Error deleting user:', error);
        alert('Đã xảy ra lỗi khi xóa người dùng');
    }
}

// 5. ADD NEW USER
async function addUser() {
    const formData = {
        username: document.getElementById('newUsername').value,
        fullName: document.getElementById('newFullName').value,
        email: document.getElementById('newEmail').value,
        phone: document.getElementById('newPhone').value,
        password: document.getElementById('newPassword').value,
        role: document.getElementById('newRole').value
    };
    
    // Basic validation
    if (!formData.username || !formData.fullName || !formData.email || !formData.password || !formData.role) {
        alert('Vui lòng điền đầy đủ các trường bắt buộc!');
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/User`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData)
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert('Thêm người dùng thành công!');
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('addUserModal'));
            modal.hide();
            
            // Reset form
            document.getElementById('addUserForm').reset();
            
            // Refresh page
            location.reload();
        } else {
            alert('Thêm người dùng thất bại: ' + (result.message || 'Lỗi không xác định'));
        }
    } catch (error) {
        console.error('Error adding user:', error);
        alert('Đã xảy ra lỗi khi thêm người dùng');
    }
}

// Helper function to open add user modal
function openAddUserModal() {
    const modal = new bootstrap.Modal(document.getElementById('addUserModal'));
    modal.show();
}

// Edit user from view modal
function editUserFromView() {
    // Close view modal
    const viewModal = bootstrap.Modal.getInstance(document.getElementById('viewUserModal'));
    viewModal.hide();
    
    // Get user ID from view modal and open edit modal
    const userName = document.getElementById('viewUserName').textContent;
    const userEmail = document.getElementById('viewEmail').textContent;
    
    // Find the user row to get the userId
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

// Event handlers setup
document.addEventListener('DOMContentLoaded', function() {
    // Auto-hide alerts after 5 seconds
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