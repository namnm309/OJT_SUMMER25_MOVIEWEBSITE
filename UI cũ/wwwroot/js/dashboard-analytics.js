// Dashboard Analytics and Statistics
class DashboardAnalytics {
    constructor(options = {}) {
        this.apiBaseUrl = options.apiBaseUrl || '/Dashboard';
        this.chartConfigs = {};
        this.refreshInterval = options.refreshInterval || 300000; // 5 minutes
        this.charts = {};
        
        this.init();
    }

    async init() {
        this.initializeChartLibrary();
        await this.loadDashboardData();
        this.startAutoRefresh();
        this.bindEvents();
    }

    initializeChartLibrary() {
        // Ensure Chart.js is loaded
        if (typeof Chart === 'undefined') {
            console.error('Chart.js library is required for dashboard analytics');
            return;
        }

        // Global Chart.js configuration
        Chart.defaults.font.family = '"Segoe UI", "Roboto", "Arial", sans-serif';
        Chart.defaults.color = '#6B7280';
        Chart.defaults.scale.grid.color = 'rgba(107, 114, 128, 0.1)';
    }

    async loadDashboardData() {
        try {
            const [movieStats, userStats, revenueStats] = await Promise.all([
                this.fetchMovieStatistics(),
                this.fetchUserStatistics(),
                this.fetchRevenueStatistics()
            ]);

            this.updateKPICards(movieStats, userStats, revenueStats);
            this.renderCharts();
        } catch (error) {
            console.error('Error loading dashboard data:', error);
            this.showError('Không thể tải dữ liệu dashboard');
        }
    }

    async fetchMovieStatistics() {
        const response = await fetch(`${this.apiBaseUrl}/GetMovieStatistics`);
        const result = await response.json();
        return result.success ? result.data : null;
    }

    async fetchUserStatistics() {
        const response = await fetch(`${this.apiBaseUrl}/GetUserStatistics`);
        const result = await response.json();
        return result.success ? result.data : null;
    }

    async fetchRevenueStatistics() {
        const response = await fetch(`${this.apiBaseUrl}/GetRevenueStatistics`);
        const result = await response.json();
        return result.success ? result.data : null;
    }

    async fetchChartData(chartType, period = 'month') {
        const response = await fetch(`${this.apiBaseUrl}/GetChartData?chartType=${chartType}&period=${period}`);
        const result = await response.json();
        return result.success ? result.data : null;
    }

    updateKPICards(movieStats, userStats, revenueStats) {
        // Movie KPIs
        this.updateKPICard('total-movies', movieStats?.totalMovies || 0, 'phim');
        this.updateKPICard('now-showing', movieStats?.nowShowingMovies || 0, 'đang chiếu');
        this.updateKPICard('coming-soon', movieStats?.comingSoonMovies || 0, 'sắp chiếu');
        this.updateKPICard('featured-movies', movieStats?.featuredMovies || 0, 'nổi bật');

        // User KPIs
        this.updateKPICard('total-users', userStats?.totalUsers || 0, 'người dùng');
        this.updateKPICard('active-users', userStats?.activeUsers || 0, 'đang hoạt động');
        this.updateKPICard('new-users-month', userStats?.newUsersThisMonth || 0, 'người dùng mới');

        // Revenue KPIs (if available)
        if (revenueStats) {
            this.updateKPICard('total-revenue', this.formatCurrency(revenueStats.totalRevenue), '');
            this.updateKPICard('monthly-revenue', this.formatCurrency(revenueStats.monthlyRevenue), '');
            this.updateKPICard('daily-revenue', this.formatCurrency(revenueStats.dailyRevenue), '');
        }
    }

    updateKPICard(elementId, value, suffix = '') {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = value + (suffix ? ` ${suffix}` : '');
            
            // Add animation effect
            element.style.transform = 'scale(1.05)';
            setTimeout(() => {
                element.style.transform = 'scale(1)';
            }, 200);
        }
    }

    async renderCharts() {
        await this.renderMovieChart();
        await this.renderUserChart();
        await this.renderRevenueChart();
        await this.renderGenreDistributionChart();
    }

    async renderMovieChart() {
        const chartData = await this.fetchChartData('movies', 'month');
        if (!chartData) return;

        const ctx = document.getElementById('movies-chart');
        if (!ctx) return;

        // Destroy existing chart if it exists
        if (this.charts.moviesChart) {
            this.charts.moviesChart.destroy();
        }

        this.charts.moviesChart = new Chart(ctx, {
            type: 'bar',
            data: chartData,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: 'Phim phát hành theo tháng'
                    },
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1
                        }
                    }
                }
            }
        });
    }

    async renderUserChart() {
        const chartData = await this.fetchChartData('users', 'month');
        if (!chartData) return;

        const ctx = document.getElementById('users-chart');
        if (!ctx) return;

        if (this.charts.usersChart) {
            this.charts.usersChart.destroy();
        }

        this.charts.usersChart = new Chart(ctx, {
            type: 'line',
            data: chartData,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: 'Tăng trưởng người dùng'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                },
                elements: {
                    line: {
                        tension: 0.4
                    }
                }
            }
        });
    }

    async renderRevenueChart() {
        const chartData = await this.fetchChartData('revenue', 'month');
        if (!chartData) return;

        const ctx = document.getElementById('revenue-chart');
        if (!ctx) return;

        if (this.charts.revenueChart) {
            this.charts.revenueChart.destroy();
        }

        this.charts.revenueChart = new Chart(ctx, {
            type: 'line',
            data: chartData,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: 'Doanh thu theo tháng'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function(value) {
                                return new Intl.NumberFormat('vi-VN', {
                                    style: 'currency',
                                    currency: 'VND'
                                }).format(value);
                            }
                        }
                    }
                },
                elements: {
                    line: {
                        tension: 0.4
                    }
                }
            }
        });
    }

    async renderGenreDistributionChart() {
        // Mock genre distribution data
        const genreData = {
            labels: ['Hành động', 'Hài kịch', 'Kinh dị', 'Tình cảm', 'Khoa học viễn tưởng', 'Phiêu lưu'],
            datasets: [{
                data: [25, 20, 15, 18, 12, 10],
                backgroundColor: [
                    '#FF6384',
                    '#36A2EB',
                    '#FFCE56',
                    '#4BC0C0',
                    '#9966FF',
                    '#FF9F40'
                ]
            }]
        };

        const ctx = document.getElementById('genre-chart');
        if (!ctx) return;

        if (this.charts.genreChart) {
            this.charts.genreChart.destroy();
        }

        this.charts.genreChart = new Chart(ctx, {
            type: 'doughnut',
            data: genreData,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: 'Phân bố thể loại phim'
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }

    bindEvents() {
        // Chart period selectors
        document.querySelectorAll('.chart-period-selector').forEach(selector => {
            selector.addEventListener('change', (e) => {
                const chartType = e.target.dataset.chartType;
                const period = e.target.value;
                this.refreshChart(chartType, period);
            });
        });

        // Refresh button
        const refreshBtn = document.getElementById('dashboard-refresh');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => {
                this.loadDashboardData();
            });
        }

        // Auto refresh toggle
        const autoRefreshToggle = document.getElementById('auto-refresh-toggle');
        if (autoRefreshToggle) {
            autoRefreshToggle.addEventListener('change', (e) => {
                if (e.target.checked) {
                    this.startAutoRefresh();
                } else {
                    this.stopAutoRefresh();
                }
            });
        }

        // Export data button
        const exportBtn = document.getElementById('export-data');
        if (exportBtn) {
            exportBtn.addEventListener('click', () => {
                this.exportDashboardData();
            });
        }

        // Real-time updates toggle
        const realTimeToggle = document.getElementById('realtime-toggle');
        if (realTimeToggle) {
            realTimeToggle.addEventListener('change', (e) => {
                if (e.target.checked) {
                    this.startRealTimeUpdates();
                } else {
                    this.stopRealTimeUpdates();
                }
            });
        }
    }

    async refreshChart(chartType, period) {
        const chartData = await this.fetchChartData(chartType, period);
        if (!chartData) return;

        const chartMethodMap = {
            'movies': 'renderMovieChart',
            'users': 'renderUserChart',
            'revenue': 'renderRevenueChart'
        };

        const method = chartMethodMap[chartType];
        if (method && typeof this[method] === 'function') {
            await this[method]();
        }
    }

    startAutoRefresh() {
        this.stopAutoRefresh(); // Clear existing interval
        this.refreshIntervalId = setInterval(() => {
            this.loadDashboardData();
        }, this.refreshInterval);
    }

    stopAutoRefresh() {
        if (this.refreshIntervalId) {
            clearInterval(this.refreshIntervalId);
            this.refreshIntervalId = null;
        }
    }

    startRealTimeUpdates() {
        // Implement WebSocket connection for real-time updates
        // This is a placeholder for future implementation
        console.log('Real-time updates started');
    }

    stopRealTimeUpdates() {
        // Stop WebSocket connection
        console.log('Real-time updates stopped');
    }

    async exportDashboardData() {
        try {
            const data = {
                exportDate: new Date().toISOString(),
                movieStats: await this.fetchMovieStatistics(),
                userStats: await this.fetchUserStatistics(),
                revenueStats: await this.fetchRevenueStatistics()
            };

            const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `dashboard-data-${new Date().toISOString().split('T')[0]}.json`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);

            this.showNotification('Xuất dữ liệu thành công', 'success');
        } catch (error) {
            console.error('Error exporting data:', error);
            this.showNotification('Lỗi khi xuất dữ liệu', 'error');
        }
    }

    formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    }

    formatNumber(number) {
        return new Intl.NumberFormat('vi-VN').format(number);
    }

    showError(message) {
        this.showNotification(message, 'error');
    }

    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `alert alert-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'info'} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; max-width: 400px;';
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(notification);
        
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 5000);
    }

    destroy() {
        this.stopAutoRefresh();
        this.stopRealTimeUpdates();
        
        // Destroy all charts
        Object.values(this.charts).forEach(chart => {
            if (chart && typeof chart.destroy === 'function') {
                chart.destroy();
            }
        });
        
        this.charts = {};
    }
}

// Initialize dashboard analytics when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    if (document.querySelector('.dashboard-container')) {
        window.dashboardAnalytics = new DashboardAnalytics();
    }
});

// Clean up on page unload
window.addEventListener('beforeunload', () => {
    if (window.dashboardAnalytics) {
        window.dashboardAnalytics.destroy();
    }
});

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DashboardAnalytics;
} 