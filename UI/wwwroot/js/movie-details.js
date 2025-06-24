// Movie Details JavaScript
function bookTicket() {
    // Implement booking functionality
    alert('Chức năng đặt vé sẽ được triển khai!');
}

function playTrailer() {
    // Implement trailer functionality
    alert('Chức năng xem trailer sẽ được triển khai!');
}

function toggleFavorite() {
    const btn = event.target.closest('.btn-icon');
    const icon = btn.querySelector('i');
    
    if (icon.classList.contains('far')) {
        icon.classList.remove('far');
        icon.classList.add('fas');
        btn.style.borderColor = '#df2144';
        btn.style.color = '#df2144';
    } else {
        icon.classList.remove('fas');
        icon.classList.add('far');
        btn.style.borderColor = 'rgba(255, 255, 255, 0.3)';
        btn.style.color = 'white';
    }
}

function openImageModal(imageSrc) {
    const modal = document.getElementById('imageModal');
    const modalImage = document.getElementById('modalImage');
    
    modalImage.src = imageSrc;
    modal.style.display = 'block';
    document.body.style.overflow = 'hidden';
}

function closeImageModal() {
    const modal = document.getElementById('imageModal');
    modal.style.display = 'none';
    document.body.style.overflow = 'auto';
}

function viewAllShowtimes() {
    // Implement view all showtimes functionality
    alert('Chức năng xem tất cả lịch chiếu sẽ được triển khai!');
}

// Close modal when clicking outside the image
document.getElementById('imageModal')?.addEventListener('click', function(e) {
    if (e.target === this) {
        closeImageModal();
    }
});

// Close modal with Escape key
document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
        closeImageModal();
    }
});