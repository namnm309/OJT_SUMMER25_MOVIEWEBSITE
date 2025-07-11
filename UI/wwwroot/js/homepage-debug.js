


window.homepageDebug = {

    checkInitialization() {
        console.log('📊 Pagination Status:', {
            initialized: !!window.homepagePagination,
            currentPages: window.homepagePagination?.currentPages
        });
        
        return !!window.homepagePagination;
    },


    checkElements() {
        const elements = {
            recommendedInfo: document.getElementById('recommendedInfo'),
            comingSoonInfo: document.getElementById('comingSoonInfo'),
            recommendedSort: document.getElementById('recommendedSort'),
            recommendedGenre: document.getElementById('recommendedGenre'),
            recommendedPageSize: document.getElementById('recommendedPageSize'),
            comingSoonSort: document.getElementById('comingSoonSort'),
            comingSoonGenre: document.getElementById('comingSoonGenre'),
            comingSoonPageSize: document.getElementById('comingSoonPageSize'),
            recommendedGrid: document.querySelector('.recommended-grid'),
            comingSoonGrid: document.querySelectorAll('.recommended-section-new')[1]?.querySelector('.recommended-grid'),
            loadMoreRecommended: document.getElementById('loadMoreRecommended'),
            loadMoreComingSoon: document.getElementById('loadMoreComingSoon')
        };

        console.log('🔍 Element Check:', elements);
        
        const missing = Object.entries(elements)
            .filter(([key, element]) => !element)
            .map(([key]) => key);
            
        if (missing.length > 0) {
            console.warn('❌ Missing elements:', missing);
        } else {
            console.log('✅ All elements found!');
        }
        
        return elements;
    },


    async testRecommendedAPI() {
        console.log('🧪 Testing Recommended Movies API...');
        
        try {
            const params = new URLSearchParams({
                page: 1,
                pageSize: 6,
                sortBy: 'releaseDate',
                sortOrder: 'desc'
            });
            
            const response = await fetch(`/Home/GetRecommendedMovies?${params}`);
            const data = await response.json();
            
            console.log('📥 API Response:', data);
            return data;
        } catch (error) {
            console.error('❌ API Error:', error);
            return null;
        }
    },


    async testComingSoonAPI() {
        console.log('🧪 Testing Coming Soon Movies API...');
        
        try {
            const params = new URLSearchParams({
                page: 1,
                pageSize: 6,
                sortBy: 'releaseDate',
                sortOrder: 'asc'
            });
            
            const response = await fetch(`/Home/GetComingSoonMovies?${params}`);
            const data = await response.json();
            
            console.log('📥 API Response:', data);
            return data;
        } catch (error) {
            console.error('❌ API Error:', error);
            return null;
        }
    },


    forceReloadRecommended() {
        if (window.homepagePagination) {
            console.log('🔄 Force reloading recommended movies...');
            window.homepagePagination.loadRecommendedMovies();
        } else {
            console.warn('❌ Pagination not initialized');
        }
    },


    forceReloadComingSoon() {
        if (window.homepagePagination) {
            console.log('🔄 Force reloading coming soon movies...');
            window.homepagePagination.loadComingSoonMovies();
        } else {
            console.warn('❌ Pagination not initialized');
        }
    },


    testFilters() {
        console.log('🎯 Testing all filter changes...');
        
        const recommendedSort = document.getElementById('recommendedSort');
        const recommendedGenre = document.getElementById('recommendedGenre');
        const recommendedPageSize = document.getElementById('recommendedPageSize');
        const comingSoonSort = document.getElementById('comingSoonSort');
        const comingSoonGenre = document.getElementById('comingSoonGenre');
        const comingSoonPageSize = document.getElementById('comingSoonPageSize');
        

        if (recommendedSort) {
            console.log('🔄 Testing recommended sort change...');
            recommendedSort.dispatchEvent(new Event('change'));
        }
        
        if (recommendedGenre) {
            console.log('🎭 Testing recommended genre change...');
            recommendedGenre.dispatchEvent(new Event('change'));
        }
        
        if (recommendedPageSize) {
            console.log('📏 Testing recommended page size change...');
            recommendedPageSize.dispatchEvent(new Event('change'));
        }
        

        if (comingSoonSort) {
            console.log('🔄 Testing coming soon sort change...');
            comingSoonSort.dispatchEvent(new Event('change'));
        }
        
        if (comingSoonGenre) {
            console.log('🎭 Testing coming soon genre change...');
            comingSoonGenre.dispatchEvent(new Event('change'));
        }
        
        if (comingSoonPageSize) {
            console.log('📏 Testing coming soon page size change...');
            comingSoonPageSize.dispatchEvent(new Event('change'));
        }
    },


    testSpecificFilter(section, filterType) {
        const elementId = `${section}${filterType.charAt(0).toUpperCase() + filterType.slice(1)}`;
        const element = document.getElementById(elementId);
        
        if (element) {
            console.log(`🎯 Testing ${section} ${filterType}...`);
            element.dispatchEvent(new Event('change'));
            return true;
        } else {
            console.warn(`❌ Element ${elementId} not found`);
            return false;
        }
    },


    async testFilterFunctionality() {
        console.log('🧪 Testing filter functionality with UI changes...');
        
        if (!window.homepagePagination) {
            console.warn('❌ Pagination not initialized');
            return false;
        }


        const recommendedSort = document.getElementById('recommendedSort');
        if (recommendedSort) {
            console.log('🔄 Testing recommended sort: title-asc');
            recommendedSort.value = 'title-asc';
            recommendedSort.dispatchEvent(new Event('change'));
            await new Promise(resolve => setTimeout(resolve, 2000)); // Wait for API response
        }


        const recommendedGenre = document.getElementById('recommendedGenre');
        if (recommendedGenre) {
            console.log('🎭 Testing recommended genre: hành động');
            recommendedGenre.value = 'hành động';
            recommendedGenre.dispatchEvent(new Event('change'));
            await new Promise(resolve => setTimeout(resolve, 2000)); // Wait for API response
        }


        const comingSoonSort = document.getElementById('comingSoonSort');
        if (comingSoonSort) {
            console.log('🔄 Testing coming soon sort: title-asc');
            comingSoonSort.value = 'title-asc';
            comingSoonSort.dispatchEvent(new Event('change'));
            await new Promise(resolve => setTimeout(resolve, 2000)); // Wait for API response
        }

        console.log('✅ Filter functionality test completed');
        return true;
    },


    checkGridContent() {
        const recommendedGrid = document.querySelector('.recommended-grid');
        const comingSoonSections = document.querySelectorAll('.recommended-section-new');
        const comingSoonGrid = comingSoonSections[1]?.querySelector('.recommended-grid');

        const results = {
            recommendedGrid: {
                found: !!recommendedGrid,
                totalItems: recommendedGrid?.children.length || 0,
                dynamicItems: recommendedGrid?.querySelectorAll('.dynamic-item').length || 0,
                staticItems: recommendedGrid?.querySelectorAll('.recommended-item:not(.dynamic-item)').length || 0
            },
            comingSoonGrid: {
                found: !!comingSoonGrid,
                totalItems: comingSoonGrid?.children.length || 0,
                dynamicItems: comingSoonGrid?.querySelectorAll('.dynamic-item').length || 0,
                staticItems: comingSoonGrid?.querySelectorAll('.recommended-item:not(.dynamic-item)').length || 0
            }
        };

        console.log('📊 Grid Content Analysis:', results);
        return results;
    },


    async runAllTests() {
        console.log('🧪 Running comprehensive homepage tests...');
        
        const results = {
            initialization: this.checkInitialization(),
            elements: this.checkElements(),
            gridContent: this.checkGridContent(),
            recommendedAPI: await this.testRecommendedAPI(),
            comingSoonAPI: await this.testComingSoonAPI()
        };
        
        console.log('📋 Test Results Summary:', results);
        return results;
    }
};


document.addEventListener('DOMContentLoaded', function() {
    setTimeout(() => {
        console.log('🔧 Homepage Debug loaded. Use homepageDebug.runAllTests() to start debugging.');
    }, 1000);
});

console.log('🔧 Homepage Debug Helper loaded. Available functions:', Object.keys(window.homepageDebug));


window.testFilter = (section, type, value) => {
    const elementId = `${section}${type.charAt(0).toUpperCase() + type.slice(1)}`;
    const element = document.getElementById(elementId);
    
    if (element) {
        console.log(`🎯 Setting ${section} ${type} to: ${value}`);
        element.value = value;
        element.dispatchEvent(new Event('change'));
        return true;
    } else {
        console.warn(`❌ Element ${elementId} not found`);
        return false;
    }
};

window.clearGrid = (section = 'recommended') => {
    let grid;
    if (section === 'recommended') {
        grid = document.querySelector('.recommended-grid');
    } else {
        const sections = document.querySelectorAll('.recommended-section-new');
        grid = sections[1]?.querySelector('.recommended-grid');
    }
    
    if (grid) {
        console.log(`🧹 Manually clearing ${section} grid`);
        grid.innerHTML = '';
        return true;
    } else {
        console.warn(`❌ ${section} grid not found`);
        return false;
    }
};


console.log(`
🚀 Quick Test Commands:
- testFilter('recommended', 'sort', 'title-asc')
- testFilter('recommended', 'genre', 'hành động') 
- testFilter('comingSoon', 'sort', 'title-asc')
- clearGrid('recommended')
- homepageDebug.runAllTests()
- homepageDebug.testFilterFunctionality()
- homepageDebug.checkGridContent()
`); 