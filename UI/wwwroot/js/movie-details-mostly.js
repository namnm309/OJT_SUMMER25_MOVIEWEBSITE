// Movie Details - Mostly About Style JavaScript

document.addEventListener('DOMContentLoaded', function() {
    initializeTabs();
    initializeScrollEffects();
    initializeInteractions();
});

// Tab functionality
function initializeTabs() {
    const tabButtons = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');
    
    tabButtons.forEach(button => {
        button.addEventListener('click', () => {
            const targetTab = button.getAttribute('data-tab');
            
            // Remove active class from all tabs and contents
            tabButtons.forEach(btn => btn.classList.remove('active'));
            tabContents.forEach(content => content.classList.remove('active'));
            
            // Add active class to clicked tab and corresponding content
            button.classList.add('active');
            document.getElementById(targetTab).classList.add('active');
        });
    });
}

// Scroll effects
function initializeScrollEffects() {
    const nav = document.querySelector('.top-nav');
    
    window.addEventListener('scroll', () => {
        if (window.scrollY > 100) {
            nav.style.background = 'rgba(26, 26, 26, 0.98)';
        } else {
            nav.style.background = 'rgba(26, 26, 26, 0.95)';
        }
    });
}

// Interactive elements
function initializeInteractions() {
    // Feed actions
    document.querySelectorAll('.feed-action').forEach(action => {
        action.addEventListener('click', function() {
            const icon = this.querySelector('i');
            const count = this.querySelector('span');
            
            if (icon.classList.contains('far')) {
                icon.classList.remove('far');
                icon.classList.add('fas');
                this.style.color = 'var(--accent-red)';
                
                if (count) {
                    const currentCount = parseInt(count.textContent);
                    count.textContent = currentCount + 1;
                }
            } else {
                icon.classList.remove('fas');
                icon.classList.add('far');
                this.style.color = '';
                
                if (count) {
                    const currentCount = parseInt(count.textContent);
                    count.textContent = Math.max(0, currentCount - 1);
                }
            }
        });
    });
    
    // Filter buttons
    document.querySelectorAll('.filter-btn').forEach(filter => {
        filter.addEventListener('click', function() {
            document.querySelectorAll('.filter-btn').forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
        });
    });
}

// Movie actions
function toggleWatchlist() {
    const button = event.target.closest('.add-watchlist');
    const icon = button.querySelector('i');
    const text = button.querySelector('span');
    
    if (icon.classList.contains('fa-plus')) {
        icon.classList.remove('fa-plus');
        icon.classList.add('fa-check');
        text.textContent = 'Đã thêm';
        button.style.background = '#28a745';
        showNotification('Đã thêm vào danh sách yêu thích!', 'success');
    } else {
        icon.classList.remove('fa-check');
        icon.classList.add('fa-plus');
        text.textContent = 'Thêm vào danh sách';
        button.style.background = 'var(--accent-red)';
        showNotification('Đã xóa khỏi danh sách yêu thích!', 'info');
    }
}

function playTrailer() {
    // Create trailer modal
    const modal = document.createElement('div');
    modal.className = 'trailer-modal';
    modal.innerHTML = `
        <div class="trailer-backdrop" onclick="closeTrailerModal()"></div>
        <div class="trailer-container">
            <button class="trailer-close" onclick="closeTrailerModal()">
                <i class="fas fa-times"></i>
            </button>
            <div class="trailer-content">
                <iframe 
                    width="100%" 
                    height="100%" 
                    src="https://www.youtube.com/embed/dQw4w9WgXcQ?autoplay=1" 
                    frameborder="0" 
                    allowfullscreen>
                </iframe>
            </div>
        </div>
    `;
    
    // Add styles
    modal.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        z-index: 10000;
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 2rem;
    `;
    
    document.body.appendChild(modal);
    document.body.style.overflow = 'hidden';
    
    showNotification('Đang phát trailer...', 'info');
}

function closeTrailerModal() {
    const modal = document.querySelector('.trailer-modal');
    if (modal) {
        modal.remove();
        document.body.style.overflow = '';
    }
}

// Image modal functions
function openImageModal(imageSrc) {
    const modal = document.getElementById('imageModal');
    const modalImage = document.getElementById('modalImage');
    
    modalImage.src = imageSrc;
    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
    
    // Add animation
    modal.style.opacity = '0';
    modal.style.display = 'flex';
    
    requestAnimationFrame(() => {
        modal.style.transition = 'opacity 0.3s ease';
        modal.style.opacity = '1';
    });
}

function closeImageModal() {
    const modal = document.getElementById('imageModal');
    
    modal.style.opacity = '0';
    
    setTimeout(() => {
        modal.classList.remove('active');
        modal.style.display = 'none';
        document.body.style.overflow = '';
    }, 300);
}

// Search functionality
function initializeSearch() {
    const searchInput = document.querySelector('.search-input');
    
    searchInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            const query = this.value.trim();
            if (query) {
                showNotification(`Đang tìm kiếm: "${query}"`, 'info');
                // Implement search logic here
            }
        }
    });
}

// Notification system
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <i class="fas fa-${getNotificationIcon(type)}"></i>
            <span>${message}</span>
        </div>
    `;
    
    // Add styles
    notification.style.cssText = `
        position: fixed;
        top: 100px;
        right: 20px;
        background: ${getNotificationColor(type)};
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 8px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
        z-index: 10001;
        transform: translateX(100%);
        transition: transform 0.3s ease;
        backdrop-filter: blur(10px);
        max-width: 300px;
    `;
    
    document.body.appendChild(notification);
    
    // Animate in
    requestAnimationFrame(() => {
        notification.style.transform = 'translateX(0)';
    });
    
    // Remove after 3 seconds
    setTimeout(() => {
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            notification.remove();
        }, 300);
    }, 3000);
}

function getNotificationIcon(type) {
    switch (type) {
        case 'success': return 'check-circle';
        case 'error': return 'exclamation-circle';
        case 'warning': return 'exclamation-triangle';
        default: return 'info-circle';
    }
}

function getNotificationColor(type) {
    switch (type) {
        case 'success': return 'linear-gradient(135deg, #28a745, #20c997)';
        case 'error': return 'linear-gradient(135deg, #dc3545, #e74c3c)';
        case 'warning': return 'linear-gradient(135deg, #ffc107, #fd7e14)';
        default: return 'linear-gradient(135deg, #007bff, #6f42c1)';
    }
}

// Keyboard navigation
document.addEventListener('keydown', function(event) {
    if (event.key === 'Escape') {
        closeImageModal();
        closeTrailerModal();
    }
});

// Initialize search when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeSearch();
});

// Add CSS for trailer modal
const style = document.createElement('style');
style.textContent = `
    .trailer-backdrop {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.9);
        backdrop-filter: blur(10px);
    }
    
    .trailer-container {
        position: relative;
        width: 90vw;
        height: 90vh;
        max-width: 1200px;
        max-height: 675px;
        z-index: 2;
    }
    
    .trailer-close {
        position: absolute;
        top: -50px;
        right: 0;
        width: 40px;
        height: 40px;
        background: rgba(255, 255, 255, 0.1);
        border: 1px solid rgba(255, 255, 255, 0.2);
        border-radius: 50%;
        color: white;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.3s ease;
        backdrop-filter: blur(10px);
    }
    
    .trailer-close:hover {
        background: var(--accent-red);
        border-color: var(--accent-red);
    }
    
    .trailer-content {
        width: 100%;
        height: 100%;
        border-radius: 8px;
        overflow: hidden;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.5);
    }
    
    .notification-content {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        font-weight: 500;
    }
`;
document.head.appendChild(style);