// Modern Movie Details JavaScript

// Smooth scrolling for internal links
document.addEventListener('DOMContentLoaded', function() {
    // Initialize page animations
    initializeAnimations();
    
    // Initialize interactive elements
    initializeInteractivity();
    
    // Initialize lazy loading for images
    initializeLazyLoading();
});

// Page animations
function initializeAnimations() {
    // Animate cards on scroll
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);
    
    // Observe all cards
    document.querySelectorAll('.content-card, .sidebar-card').forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        card.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(card);
    });
}

// Interactive elements
function initializeInteractivity() {
    // Add ripple effect to buttons
    document.querySelectorAll('.btn-primary-modern, .btn-secondary-modern, .quick-action-btn').forEach(button => {
        button.addEventListener('click', createRipple);
    });
    
    // Add hover effects to gallery items
    document.querySelectorAll('.gallery-item').forEach(item => {
        item.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-8px) scale(1.02)';
        });
        
        item.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0) scale(1)';
        });
    });
}

// Lazy loading for images
function initializeLazyLoading() {
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                observer.unobserve(img);
            }
        });
    });
    
    document.querySelectorAll('img[data-src]').forEach(img => {
        imageObserver.observe(img);
    });
}

// Ripple effect
function createRipple(event) {
    const button = event.currentTarget;
    const ripple = document.createElement('span');
    const rect = button.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = event.clientX - rect.left - size / 2;
    const y = event.clientY - rect.top - size / 2;
    
    ripple.style.width = ripple.style.height = size + 'px';
    ripple.style.left = x + 'px';
    ripple.style.top = y + 'px';
    ripple.classList.add('ripple');
    
    button.appendChild(ripple);
    
    setTimeout(() => {
        ripple.remove();
    }, 600);
}

// Movie actions
function bookTicket() {
    // Add booking animation
    const button = event.target.closest('.btn-primary-modern');
    button.style.transform = 'scale(0.95)';
    
    setTimeout(() => {
        button.style.transform = 'scale(1)';
        // Redirect to booking page or show booking modal
        showNotification('Chuyển hướng đến trang đặt vé...', 'success');
    }, 150);
}

function playTrailer() {
    // Show trailer modal or redirect
    showNotification('Đang tải trailer...', 'info');
    // Implementation for trailer playback
}

function toggleFavorite() {
    const button = event.target.closest('.btn-icon-modern');
    const icon = button.querySelector('i');
    
    if (icon.classList.contains('far')) {
        icon.classList.remove('far');
        icon.classList.add('fas');
        button.style.color = '#ff6b35';
        showNotification('Đã thêm vào danh sách yêu thích', 'success');
    } else {
        icon.classList.remove('fas');
        icon.classList.add('far');
        button.style.color = '';
        showNotification('Đã xóa khỏi danh sách yêu thích', 'info');
    }
}

function shareMovie() {
    if (navigator.share) {
        navigator.share({
            title: document.querySelector('.movie-title').textContent,
            text: 'Xem phim hay tại Cinema City',
            url: window.location.href
        });
    } else {
        // Fallback: copy to clipboard
        navigator.clipboard.writeText(window.location.href);
        showNotification('Đã sao chép link phim', 'success');
    }
}

function addToWishlist() {
    showNotification('Đã thêm vào danh sách mong muốn', 'success');
}

function downloadInfo() {
    showNotification('Đang tải thông tin phim...', 'info');
}

function viewAllShowtimes() {
    showNotification('Chuyển hướng đến trang lịch chiếu...', 'info');
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
        top: 20px;
        right: 20px;
        background: ${getNotificationColor(type)};
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 12px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
        z-index: 10000;
        transform: translateX(100%);
        transition: transform 0.3s ease;
        backdrop-filter: blur(10px);
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
    }
});

// Add CSS for ripple effect
const style = document.createElement('style');
style.textContent = `
    .ripple {
        position: absolute;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.3);
        transform: scale(0);
        animation: ripple-animation 0.6s linear;
        pointer-events: none;
    }
    
    @keyframes ripple-animation {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
    
    .notification-content {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        font-weight: 500;
    }
`;
document.head.appendChild(style);