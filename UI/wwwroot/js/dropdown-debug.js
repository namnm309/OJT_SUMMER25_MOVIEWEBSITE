// Debug script để test dropdown styling và functionality
console.log('🔧 Dropdown Debug Script loaded');

document.addEventListener('DOMContentLoaded', function() {
    console.log('🔧 Dropdown Debug: DOM loaded');
    
    setTimeout(() => {
        testDropdownStyling();
        testDropdownFunctionality();
    }, 1000);
});

function testDropdownStyling() {
    console.log('🎨 Testing dropdown styling...');
    
    const dropdowns = document.querySelectorAll('.pagination-select');
    console.log(`Found ${dropdowns.length} dropdown(s)`);
    
    dropdowns.forEach((dropdown, index) => {
        const styles = window.getComputedStyle(dropdown);
        console.log(`Dropdown ${index + 1} (${dropdown.id}):`, {
            background: styles.background,
            backgroundColor: styles.backgroundColor,
            color: styles.color,
            border: styles.border,
            borderColor: styles.borderColor,
            padding: styles.padding,
            display: styles.display,
            visibility: styles.visibility
        });
        

        const rect = dropdown.getBoundingClientRect();
        console.log(`Dropdown ${index + 1} position:`, {
            width: rect.width,
            height: rect.height,
            top: rect.top,
            left: rect.left,
            visible: rect.width > 0 && rect.height > 0
        });
    });
}

function testDropdownFunctionality() {
    console.log('⚙️ Testing dropdown functionality...');
    
    const testSelectors = [
        'recommendedSort',
        'recommendedGenre', 
        'recommendedPageSize',
        'comingSoonSort',
        'comingSoonGenre',
        'comingSoonPageSize'
    ];
    
    testSelectors.forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            console.log(`✅ ${id}: Found, options = ${element.options.length}`);
            

            element.addEventListener('change', function(e) {
                console.log(`🔄 ${id} changed to: ${e.target.value}`);
            });
            

            Array.from(element.options).forEach((option, index) => {
                console.log(`   Option ${index}: "${option.value}" - "${option.text}"`);
            });
        } else {
            console.log(`❌ ${id}: Not found`);
        }
    });
}


function addTestButtons() {
    if (window.location.search.includes('debug=true')) {
        const testDiv = document.createElement('div');
        testDiv.style.position = 'fixed';
        testDiv.style.top = '10px';
        testDiv.style.right = '10px';
        testDiv.style.zIndex = '99999';
        testDiv.style.background = 'rgba(0,0,0,0.8)';
        testDiv.style.color = 'white';
        testDiv.style.padding = '10px';
        testDiv.style.borderRadius = '5px';
        
        testDiv.innerHTML = `
            <h4>Dropdown Debug</h4>
            <button onclick="testDropdownStyling()" style="margin: 2px; padding: 5px;">Test Styling</button>
            <button onclick="testDropdownFunctionality()" style="margin: 2px; padding: 5px;">Test Function</button>
            <button onclick="fixDropdownStyling()" style="margin: 2px; padding: 5px;">Fix Styling</button>
        `;
        
        document.body.appendChild(testDiv);
    }
}

function fixDropdownStyling() {
    console.log('🔧 Attempting to fix dropdown styling...');
    
    const dropdowns = document.querySelectorAll('.pagination-select');
    dropdowns.forEach((dropdown, index) => {

        dropdown.style.background = 'rgba(40, 40, 80, 0.9)';
        dropdown.style.color = 'white';
        dropdown.style.border = '1px solid rgba(139, 92, 246, 0.5)';
        dropdown.style.borderRadius = '12px';
        dropdown.style.padding = '1rem 1.2rem';
        dropdown.style.fontSize = '0.9rem';
        dropdown.style.fontWeight = '500';
        dropdown.style.cursor = 'pointer';
        
        console.log(`✅ Applied fix to dropdown ${index + 1}`);
    });
}


document.addEventListener('DOMContentLoaded', addTestButtons); 