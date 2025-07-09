// Pagination Debug Script
console.log('🔧 Pagination Debug Script Loaded');

// Add debug functions to window
window.paginationDebug = {
    testRecommended: function() {
        console.log('🔧 Testing recommended movies pagination...');
        const button = document.getElementById('loadMoreRecommended');
        if (button) {
            console.log('🔧 Load more button found, clicking...');
            button.click();
        } else {
            console.error('🔧 Load more button not found');
        }
    },
    
    testComingSoon: function() {
        console.log('🔧 Testing coming soon movies pagination...');
        const button = document.getElementById('loadMoreComingSoon');
        if (button) {
            console.log('🔧 Load more button found, clicking...');
            button.click();
        } else {
            console.error('🔧 Load more button not found');
        }
    },
    
    checkElements: function() {
        console.log('🔧 Checking pagination elements...');
        
        const elements = {
            recommendedSort: document.getElementById('recommendedSort'),
            recommendedGenre: document.getElementById('recommendedGenre'),
            recommendedPageSize: document.getElementById('recommendedPageSize'),
            recommendedInfo: document.getElementById('recommendedInfo'),
            loadMoreRecommended: document.getElementById('loadMoreRecommended'),
            
            comingSoonSort: document.getElementById('comingSoonSort'),
            comingSoonGenre: document.getElementById('comingSoonGenre'),
            comingSoonPageSize: document.getElementById('comingSoonPageSize'),
            comingSoonInfo: document.getElementById('comingSoonInfo'),
            loadMoreComingSoon: document.getElementById('loadMoreComingSoon')
        };
        
        console.table(Object.fromEntries(
            Object.entries(elements).map(([key, elem]) => [key, !!elem])
        ));
        
        return elements;
    },
    
    enableDebugMode: function() {
        console.log('🔧 Enabling debug mode...');
        document.body.classList.add('debug-pagination');
        
        // Make load more buttons more visible
        const buttons = document.querySelectorAll('.btn-load-more');
        buttons.forEach(btn => {
            btn.style.background = 'lime';
            btn.style.color = 'black';
            btn.style.border = '2px solid red';
        });
    },
    
    simulateApiResponse: function(section = 'recommended') {
        console.log(`🔧 Simulating API response for ${section}...`);
        
        const mockData = {
            success: true,
            data: [
                {
                    id: 'mock-1',
                    title: 'Mock Movie 1',
                    rating: 8.5,
                    runningTime: 120,
                    releaseDate: '2023-01-01',
                    primaryImageUrl: 'https://via.placeholder.com/300x450/ff0000/ffffff?text=Mock+1',
                    genres: [{ name: 'Hành động' }]
                },
                {
                    id: 'mock-2',
                    title: 'Mock Movie 2',
                    rating: 7.8,
                    runningTime: 110,
                    releaseDate: '2023-02-01',
                    primaryImageUrl: 'https://via.placeholder.com/300x450/00ff00/ffffff?text=Mock+2',
                    genres: [{ name: 'Tình cảm' }]
                }
            ],
            pagination: {
                currentPage: 2,
                totalPages: 5,
                totalItems: 25,
                hasNextPage: true
            }
        };
        
        // Manually trigger grid update
        if (window.homepagePagination) {
            if (section === 'recommended') {
                window.homepagePagination.updateRecommendedGrid(mockData.data, true);
                window.homepagePagination.updatePaginationInfo('recommended', mockData.pagination);
            } else {
                window.homepagePagination.updateComingSoonGrid(mockData.data, true);
                window.homepagePagination.updatePaginationInfo('comingSoon', mockData.pagination);
            }
        }
    }
};

// Auto-check elements when script loads
document.addEventListener('DOMContentLoaded', function() {
    setTimeout(() => {
        console.log('🔧 Running automatic pagination check...');
        window.paginationDebug.checkElements();
    }, 1000);
}); 