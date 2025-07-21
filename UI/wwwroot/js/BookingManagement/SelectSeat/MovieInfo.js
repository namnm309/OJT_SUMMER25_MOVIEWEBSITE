// Movie info logic module cho trang SelectSeat
(function (window) {
    let showTimeId = null;
    function getAuthToken() {
        return sessionStorage.getItem('jwtToken') || localStorage.getItem('jwtToken') || localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
    }
    async function loadMovieAndShowtimeDetails() {
        try {
            const showtimeResponse = await fetch(`https://localhost:7049/api/v1/booking-ticket/showtime/${showTimeId}/details`, {
                headers: { 'Authorization': `Bearer ${getAuthToken()}` }
            });
            if (!showtimeResponse.ok) throw new Error('Không thể lấy thông tin suất chiếu');
            const showtimeData = await showtimeResponse.json();
            if (showtimeData.code === 200 && showtimeData.data) {
                const movieId = showtimeData.data.movieId;
                const roomId = showtimeData.data.roomId;
                // Khởi tạo SeatModule nếu chưa có
                if (roomId && (!window.SeatModule.roomId || window.SeatModule.roomId !== roomId)) {
                    window.SeatModule.init({
                        seatMapContainer: document.getElementById('seatMap'),
                        selectedSeatsDisplay: document.getElementById('selectedSeatsDisplay'),
                        totalPriceElement: document.getElementById('totalPrice'),
                        continueBtn: document.getElementById('continueBtn'),
                        showTimeId: showTimeId,
                        roomId: roomId
                    });
                    window.SeatModule.loadSeats();
                    window.SeatModule.updateDisplay();
                }
                const movieIdParam = typeof movieId === 'string' ? movieId : movieId.toString();
                const movieResponse = await fetch(`https://localhost:7049/api/v1/movie/GetById?movieId=${movieIdParam}`, {
                    headers: { 'Authorization': `Bearer ${getAuthToken()}` }
                });
                if (!movieResponse.ok) return;
                const movieData = await movieResponse.json();
                if (movieData.code === 200 && movieData.data) {
                    updateMovieInfo(movieData.data);
                    updateShowtimeInfo(showtimeData.data);
                }
            }
        } catch (error) {
            // Có thể log hoặc hiển thị lỗi nếu cần
        }
    }
    function updateMovieInfo(movieInfo) {
        const movieTitles = document.querySelectorAll('.movie-title');
        movieTitles.forEach(title => { if (movieInfo.title) title.textContent = movieInfo.title; });
        const moviePoster = document.querySelector('.movie-poster');
        if (moviePoster && movieInfo.primaryImageUrl) {
            moviePoster.innerHTML = `<img src="${movieInfo.primaryImageUrl}" alt="${movieInfo.title || 'Movie Poster'}" style="width: 100%; height: 100%; object-fit: cover; border-radius: 10px;" onerror="this.src='/images/placeholder-movie.jpg';" />`;
        }
        const movieDescription = document.getElementById('movieDescription');
        if (movieDescription && movieInfo.content) movieDescription.textContent = movieInfo.content;
        const movieMeta = document.getElementById('movieMeta');
        if (movieMeta && movieInfo) {
            let metaHtml = '';
            if (movieInfo.genres && movieInfo.genres.length > 0) {
                const genreNames = movieInfo.genres.map(g => g.name || g).join(', ');
                metaHtml += `<span class="meta-item">🎬 ${genreNames}</span>`;
            } else {
                metaHtml += `<span class="meta-item">🎬 Phim điện ảnh</span>`;
            }
            if (movieInfo.runningTime) {
                metaHtml += `<span class="meta-item">⏱️ ${movieInfo.runningTime} phút</span>`;
            } else {
                metaHtml += `<span class="meta-item">⏱️ Chưa cập nhật</span>`;
            }
            if (movieInfo.rating) {
                metaHtml += `<span class="meta-item">⭐ ${movieInfo.rating}/10</span>`;
            } else {
                metaHtml += `<span class="meta-item">⭐ Chưa đánh giá</span>`;
            }
            if (movieInfo.director) {
                metaHtml += `<span class="meta-item">🎭 ${movieInfo.director}</span>`;
            }
            movieMeta.innerHTML = metaHtml;
        }
    }
    function updateShowtimeInfo(showtimeInfo) {
        if (showtimeInfo.showDate) {
            const showDate = new Date(showtimeInfo.showDate);
            const showDateElement = document.getElementById('showDate');
            if (showDateElement) showDateElement.textContent = showDate.toLocaleDateString('vi-VN');
            const showTimeElement = document.getElementById('showTime');
            if (showTimeElement) {
                const hours = showDate.getUTCHours().toString().padStart(2, '0');
                const minutes = showDate.getUTCMinutes().toString().padStart(2, '0');
                showTimeElement.textContent = `${hours}:${minutes}`;
            }
            const ticketDateTimeElement = document.getElementById('ticketDateTime');
            if (ticketDateTimeElement) {
                const formattedDate = showDate.toLocaleDateString('vi-VN');
                const formattedTime = showDate.getUTCHours().toString().padStart(2, '0') + ':' + showDate.getUTCMinutes().toString().padStart(2, '0');
                ticketDateTimeElement.textContent = `${formattedDate}, ${formattedTime}`;
            }
        }
        if (showtimeInfo.cinemaRoomName) {
            const cinemaRoomElement = document.getElementById('cinemaRoom');
            if (cinemaRoomElement) cinemaRoomElement.textContent = showtimeInfo.cinemaRoomName;
        }
        const ticketTypeElement = document.getElementById('ticketType');
        if (ticketTypeElement) {
            const ticketType = showtimeInfo.version || 'Vé thường';
            ticketTypeElement.textContent = ticketType;
        }
    }
    function initMovieInfoModule(options) {
        showTimeId = options.showTimeId;
    }
    window.MovieInfoModule = {
        init: initMovieInfoModule,
        loadMovieAndShowtimeDetails: loadMovieAndShowtimeDetails
    };
})(window); 