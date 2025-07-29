// Movie info logic module cho trang SelectSeat
(function (window) {
    let showTimeId = null;
    function getAuthToken() {
        return sessionStorage.getItem('jwtToken') || localStorage.getItem('jwtToken') || localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
    }
    async function loadMovieAndShowtimeDetails() {
        try {
            const showtimeResponse = await fetch(`https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/booking-ticket/showtime/${showTimeId}/details`, {
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
                const movieResponse = await fetch(`https://cinemacity-backend-hhasbzggfafpgbgw.eastasia-01.azurewebsites.net/api/v1/movie/GetById?movieId=${movieIdParam}`, {
                    headers: { 'Authorization': `Bearer ${getAuthToken()}` }
                });
                if (!movieResponse.ok) return;
                const movieDataRaw = await movieResponse.json();
                const payload = movieDataRaw.data || movieDataRaw.value?.data || movieDataRaw;
                if (payload && payload.title) {
                    updateMovieInfo(payload);
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
        let poster = movieInfo.primaryImageUrl;
        if(!poster && Array.isArray(movieInfo.images) && movieInfo.images.length){
            poster = movieInfo.images[0].imageUrl || movieInfo.images[0].url;
        }
        const moviePoster = document.querySelector('.movie-poster');
        if(moviePoster && poster){
            console.log('Set movie poster to', poster);
            if (moviePoster.tagName.toLowerCase() === 'img') {
                moviePoster.src = poster;
                moviePoster.onerror = function(){ this.src='/images/placeholder-movie.jpg'; };
                if(window.$){ $('.movie-poster').attr('src',poster); }
            } else {
                moviePoster.innerHTML = `<img src="${poster}" alt="${movieInfo.title || 'Movie Poster'}" style="width:100%;height:100%;object-fit:cover;border-radius:10px;" onerror="this.src='/images/placeholder-movie.jpg';"/>`;
            }
        } else {
            console.warn('Poster not found in movie info', movieInfo);
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
        const rawDate = showtimeInfo.showDate || showtimeInfo.ShowDate || showtimeInfo.date || null;
        if (rawDate) {
            const showDate = new Date(rawDate);
            const showDateElement = document.getElementById('showDate');
            if (showDateElement) showDateElement.textContent = showDate.toLocaleDateString('vi-VN');
            const showTimeElement = document.getElementById('showTime');
            if (showTimeElement) {
                let timeStr = '';
                if (showtimeInfo.startTime || showtimeInfo.StartTime) {
                    timeStr = (showtimeInfo.startTime || showtimeInfo.StartTime).toString().substring(0,5); // HH:mm
                } else {
                    const hours = showDate.getHours().toString().padStart(2,'0');
                    const minutes = showDate.getMinutes().toString().padStart(2,'0');
                    timeStr = `${hours}:${minutes}`;
                }
                showTimeElement.textContent = timeStr;
            }
            const ticketDateTimeElement = document.getElementById('ticketDateTime');
            if (ticketDateTimeElement) {
                const formattedDate = showDate.toLocaleDateString('vi-VN');
                let timeStr = '';
                if (showtimeInfo.startTime || showtimeInfo.StartTime) {
                    timeStr = (showtimeInfo.startTime || showtimeInfo.StartTime).toString().substring(0,5);
                } else {
                    const hours = showDate.getHours().toString().padStart(2,'0');
                    const minutes = showDate.getMinutes().toString().padStart(2,'0');
                    timeStr = `${hours}:${minutes}`;
                }
                ticketDateTimeElement.textContent = `${formattedDate}, ${timeStr}`;
            }
        }
        const roomName = showtimeInfo.cinemaRoomName || showtimeInfo.roomName || showtimeInfo.RoomName;
        if (roomName) {
            const cinemaRoomElement = document.getElementById('cinemaRoom');
            if (cinemaRoomElement) cinemaRoomElement.textContent = roomName;
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