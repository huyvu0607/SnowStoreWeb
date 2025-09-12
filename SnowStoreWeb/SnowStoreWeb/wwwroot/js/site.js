// SnowStore JavaScript Functions
$(document).ready(function () {
    // Initialize all features
    initializeNavigation();
    initializeSearch();
    initializeAnimations();
    initializeTooltips();
    initializeScrollEffects();
});

// Navigation Functions
function initializeNavigation() {
    // Add active class to current page nav item
    updateActiveNavItem();

    // Smooth scrolling for anchor links
    $('a[href*="#"]:not([href="#"])').click(function () {
        if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && location.hostname == this.hostname) {
            var target = $(this.hash);
            target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
            if (target.length) {
                $('html, body').animate({
                    scrollTop: target.offset().top - 100
                }, 1000);
                return false;
            }
        }
    });

    // Mobile menu auto-close
    $('.navbar-nav .nav-link').on('click', function () {
        if ($(window).width() < 992) {
            $('.navbar-collapse').collapse('hide');
        }
    });
}

function updateActiveNavItem() {
    var currentPath = window.location.pathname.toLowerCase();
    var currentController = '';
    var currentAction = '';

    // Parse the current path to get controller and action
    var pathParts = currentPath.split('/').filter(part => part !== '');

    if (pathParts.length === 0) {
        // Root path - this is Home/Index
        currentController = 'home';
        currentAction = 'index';
    } else if (pathParts.length === 1) {
        // Only controller specified - default action is Index
        currentController = pathParts[0].toLowerCase();
        currentAction = 'index';
    } else {
        // Both controller and action specified
        currentController = pathParts[0].toLowerCase();
        currentAction = pathParts[1].toLowerCase();
    }

    // Remove all active classes first
    $('.nav-link-custom').removeClass('active');

    // Check each navigation link
    $('.nav-link-custom').each(function () {
        var $link = $(this);
        var href = $link.attr('href');

        if (!href) return;

        // Parse the link's controller and action from asp-controller and asp-action attributes
        // Since we can't access these directly in client-side, we'll parse from href
        var linkController = '';
        var linkAction = '';

        if (href === '/' || href.toLowerCase() === '/home' || href.toLowerCase() === '/home/index') {
            linkController = 'home';
            linkAction = 'index';
        } else {
            var hrefParts = href.split('/').filter(part => part !== '');
            if (hrefParts.length >= 1) {
                linkController = hrefParts[0].toLowerCase();
                linkAction = hrefParts.length >= 2 ? hrefParts[1].toLowerCase() : 'index';
            }
        }

        // Check for exact match (controller + action)
        if (currentController === linkController && currentAction === linkAction) {
            $link.addClass('active');
        }
        // For controller-only pages (like Product), match just the controller
        else if (currentController === linkController &&
            (linkAction === 'index' || currentAction === 'index')) {
            $link.addClass('active');
        }
    });
}
})
}
// Alternative method using data attributes (recommended)
function updateActiveNavItemWithDataAttributes() {
    // Remove all active classes first
    $('.nav-link-custom').removeClass('active');

    // Get current controller and action from the page
    // You can set these as data attributes on the body tag in your layout
    var currentController = $('body').data('controller') || '';
    var currentAction = $('body').data('action') || '';

    if (!currentController) {
        // Fallback to parsing URL
        var pathParts = window.location.pathname.split('/').filter(part => part !== '');
        currentController = pathParts.length > 0 ? pathParts[0] : 'Home';
        currentAction = pathParts.length > 1 ? pathParts[1] : 'Index';
    }

    // Find matching navigation link
    $('.nav-link-custom').each(function () {
        var $link = $(this);
        var linkController = $link.data('controller') || '';
        var linkAction = $link.data('action') || 'Index';

        if (currentController.toLowerCase() === linkController.toLowerCase()) {
            // For index actions or when actions match
            if (linkAction.toLowerCase() === 'index' ||
                currentAction.toLowerCase() === linkAction.toLowerCase()) {
                $link.addClass('active');
            }
        }
    });
}
// Search Functions
function initializeSearch() {
    const searchInput = $('.search-input');
    const searchBtn = $('.search-btn');

    // Search input enhancements
    searchInput.on('focus', function () {
        $(this).parent().addClass('search-focus');
    });

    searchInput.on('blur', function () {
        $(this).parent().removeClass('search-focus');
    });

    // Auto-suggest functionality (if you have an API endpoint)
    searchInput.on('input', debounce(function () {
        const query = $(this).val().trim();
        if (query.length >= 2) {
            showSearchSuggestions(query);
        } else {
            hideSearchSuggestions();
        }
    }, 300));

    // Search form submission
    $('.search-form').on('submit', function (e) {
        const query = searchInput.val().trim();
        if (!query) {
            e.preventDefault();
            searchInput.focus();
            showNotification('Vui lòng nhập từ khóa tìm kiếm', 'warning');
            return false;
        }

        // Add loading state
        searchBtn.html('<div class="loading-snow"></div>');
        searchBtn.prop('disabled', true);
    });

    // Quick search shortcuts
    $(document).keydown(function (e) {
        // Ctrl+K or Cmd+K to focus search
        if ((e.ctrlKey || e.metaKey) && e.keyCode === 75) {
            e.preventDefault();
            searchInput.focus();
        }

        // Escape to clear search
        if (e.keyCode === 27) {
            searchInput.val('').blur();
            hideSearchSuggestions();
        }
    });
}

function showSearchSuggestions(query) {
    // This would typically make an AJAX call to your search API
    // For now, we'll just show a placeholder
    const suggestions = [
        'Áo khoác mùa đông',
        'Giày thể thao',
        'Túi xách nữ',
        'Đồng hồ nam',
        'Phụ kiện thời trang'
    ].filter(item => item.toLowerCase().includes(query.toLowerCase()));

    if (suggestions.length > 0) {
        let suggestionHtml = '<div class="search-suggestions">';
        suggestions.forEach(suggestion => {
            suggestionHtml += `<div class="suggestion-item" data-query="${suggestion}">${suggestion}</div>`;
        });
        suggestionHtml += '</div>';

        $('.search-form').append(suggestionHtml);

        // Handle suggestion clicks
        $('.suggestion-item').on('click', function () {
            const selectedQuery = $(this).data('query');
            $('.search-input').val(selectedQuery);
            $('.search-form').submit();
        });
    }
}

function hideSearchSuggestions() {
    $('.search-suggestions').fadeOut(200, function () {
        $(this).remove();
    });
}

// Animation Functions
function initializeAnimations() {
    // Navbar scroll effect
    $(window).scroll(function () {
        const scrollTop = $(window).scrollTop();
        if (scrollTop > 50) {
            $('.snow-navbar').addClass('scrolled');
        } else {
            $('.snow-navbar').removeClass('scrolled');
        }
    });

    // Fade in animations for cards
    if (typeof IntersectionObserver !== 'undefined') {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver(function (entries) {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');
                }
            });
        }, observerOptions);

        // Observe all cards
        document.querySelectorAll('.card-snow, .product-card, .feature-card').forEach(card => {
            observer.observe(card);
        });
    }

    // Button hover effects
    $('.btn-snow').hover(
        function () {
            $(this).addClass('btn-hover');
        },
        function () {
            $(this).removeClass('btn-hover');
        }
    );
}

function initializeScrollEffects() {
    // Back to top button
    const backToTop = $('<button class="back-to-top btn-snow"><i class="fas fa-chevron-up"></i></button>');
    $('body').append(backToTop);

    $(window).scroll(function () {
        if ($(window).scrollTop() > 300) {
            backToTop.addClass('show');
        } else {
            backToTop.removeClass('show');
        }
    });

    backToTop.click(function () {
        $('html, body').animate({ scrollTop: 0 }, 800);
    });

    // Parallax effect for hero sections
    $('.hero-section, .banner-section').each(function () {
        const $this = $(this);
        $(window).scroll(function () {
            const scrolled = $(window).scrollTop();
            const rate = scrolled * -0.5;
            $this.css('transform', 'translateY(' + rate + 'px)');
        });
    });
}

function initializeTooltips() {
    // Initialize Bootstrap tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Custom tooltips for navigation
    $('.nav-link-custom').each(function () {
        const $this = $(this);
        const text = $this.text().trim();
        $this.attr('title', text);
    });
}

// Utility Functions
function debounce(func, wait, immediate) {
    var timeout;
    return function () {
        var context = this, args = arguments;
        var later = function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        var callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func.apply(context, args);
    };
}

function showNotification(message, type = 'info', duration = 3000) {
    const notification = $(`
        <div class="snow-notification ${type}">
            <div class="notification-content">
                <i class="fas ${getNotificationIcon(type)}"></i>
                <span>${message}</span>
                <button class="notification-close">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        </div>
    `);

    $('body').append(notification);

    // Show notification
    setTimeout(() => notification.addClass('show'), 100);

    // Auto hide
    setTimeout(() => {
        notification.removeClass('show');
        setTimeout(() => notification.remove(), 300);
    }, duration);

    // Manual close
    notification.find('.notification-close').click(function () {
        notification.removeClass('show');
        setTimeout(() => notification.remove(), 300);
    });
}

function getNotificationIcon(type) {
    switch (type) {
        case 'success': return 'fa-check-circle';
        case 'error': return 'fa-exclamation-circle';
        case 'warning': return 'fa-exclamation-triangle';
        default: return 'fa-info-circle';
    }
}

// Product Functions
function addToCart(productId, quantity = 1) {
    // Show loading state
    const btn = $(`.add-to-cart[data-product-id="${productId}"]`);
    const originalHtml = btn.html();
    btn.html('<div class="loading-snow"></div> Đang thêm...').prop('disabled', true);

    // AJAX call to add product to cart
    $.ajax({
        url: '/Cart/Add',
        method: 'POST',
        data: {
            productId: productId,
            quantity: quantity
        },
        success: function (response) {
            if (response.success) {
                showNotification('Đã thêm sản phẩm vào giỏ hàng!', 'success');
                updateCartCount(response.cartCount);

                // Add visual feedback
                btn.removeClass('btn-snow').addClass('btn-success');
                btn.html('<i class="fas fa-check"></i> Đã thêm');

                setTimeout(() => {
                    btn.removeClass('btn-success').addClass('btn-snow');
                    btn.html(originalHtml).prop('disabled', false);
                }, 2000);
            } else {
                showNotification(response.message || 'Có lỗi xảy ra!', 'error');
                btn.html(originalHtml).prop('disabled', false);
            }
        },
        error: function () {
            showNotification('Không thể kết nối đến server!', 'error');
            btn.html(originalHtml).prop('disabled', false);
        }
    });
}

function updateCartCount(count) {
    const cartBadge = $('.cart-badge');
    if (cartBadge.length) {
        cartBadge.text(count);
        cartBadge.addClass('bounce');
        setTimeout(() => cartBadge.removeClass('bounce'), 600);
    }
}

function addToWishlist(productId) {
    const btn = $(`.wishlist-btn[data-product-id="${productId}"]`);
    const icon = btn.find('i');

    $.ajax({
        url: '/Wishlist/Toggle',
        method: 'POST',
        data: { productId: productId },
        success: function (response) {
            if (response.success) {
                if (response.added) {
                    icon.removeClass('far').addClass('fas text-danger');
                    showNotification('Đã thêm vào danh sách yêu thích!', 'success');
                } else {
                    icon.removeClass('fas text-danger').addClass('far');
                    showNotification('Đã xóa khỏi danh sách yêu thích!', 'info');
                }
            }
        },
        error: function () {
            showNotification('Có lỗi xảy ra!', 'error');
        }
    });
}

// Form Validation
function validateForm(formSelector) {
    const form = $(formSelector);
    let isValid = true;

    // Clear previous validation
    form.find('.is-invalid').removeClass('is-invalid');
    form.find('.invalid-feedback').remove();

    // Validate required fields
    form.find('[required]').each(function () {
        const field = $(this);
        const value = field.val().trim();

        if (!value) {
            field.addClass('is-invalid');
            field.after('<div class="invalid-feedback">Trường này là bắt buộc</div>');
            isValid = false;
        }
    });

    // Validate email fields
    form.find('input[type="email"]').each(function () {
        const field = $(this);
        const value = field.val().trim();

        if (value && !isValidEmail(value)) {
            field.addClass('is-invalid');
            field.after('<div class="invalid-feedback">Email không hợp lệ</div>');
            isValid = false;
        }
    });

    // Validate password confirmation
    const password = form.find('input[name="Password"]');
    const confirmPassword = form.find('input[name="ConfirmPassword"]');

    if (password.length && confirmPassword.length) {
        if (password.val() !== confirmPassword.val()) {
            confirmPassword.addClass('is-invalid');
            confirmPassword.after('<div class="invalid-feedback">Mật khẩu xác nhận không khớp</div>');
            isValid = false;
        }
    }

    return isValid;
}

function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Image Functions
function initializeImageLazyLoading() {
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    imageObserver.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    }
}

function initializeImageZoom() {
    $('.product-image').on('mousemove', function (e) {
        const $this = $(this);
        const offset = $this.offset();
        const x = (e.pageX - offset.left) / $this.width() * 100;
        const y = (e.pageY - offset.top) / $this.height() * 100;

        $this.css('transform-origin', `${x}% ${y}%`);
        $this.addClass('zoomed');
    });

    $('.product-image').on('mouseleave', function () {
        $(this).removeClass('zoomed');
    });
}

// Local Storage Functions
function saveToLocalStorage(key, data) {
    try {
        localStorage.setItem(`snowstore_${key}`, JSON.stringify(data));
    } catch (e) {
        console.warn('Cannot save to localStorage:', e);
    }
}

function getFromLocalStorage(key) {
    try {
        const data = localStorage.getItem(`snowstore_${key}`);
        return data ? JSON.parse(data) : null;
    } catch (e) {
        console.warn('Cannot read from localStorage:', e);
        return null;
    }
}

function removeFromLocalStorage(key) {
    try {
        localStorage.removeItem(`snowstore_${key}`);
    } catch (e) {
        console.warn('Cannot remove from localStorage:', e);
    }
}

// Recently Viewed Products
function addToRecentlyViewed(productId) {
    let recentlyViewed = getFromLocalStorage('recently_viewed') || [];

    // Remove if already exists
    recentlyViewed = recentlyViewed.filter(id => id !== productId);

    // Add to beginning
    recentlyViewed.unshift(productId);

    // Keep only last 10
    recentlyViewed = recentlyViewed.slice(0, 10);

    saveToLocalStorage('recently_viewed', recentlyViewed);
}

// Theme Functions
function toggleTheme() {
    const currentTheme = $('body').hasClass('dark-theme') ? 'dark' : 'light';
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

    $('body').toggleClass('dark-theme');
    saveToLocalStorage('theme', newTheme);

    // Update theme toggle button
    const themeBtn = $('.theme-toggle');
    const icon = themeBtn.find('i');

    if (newTheme === 'dark') {
        icon.removeClass('fa-moon').addClass('fa-sun');
    } else {
        icon.removeClass('fa-sun').addClass('fa-moon');
    }
}

function loadSavedTheme() {
    const savedTheme = getFromLocalStorage('theme');
    if (savedTheme === 'dark') {
        $('body').addClass('dark-theme');
    }
}

$(document).ready(function () {
    updateActiveNavItem();

    // Update active state when using AJAX navigation or SPA routing
    $(window).on('popstate', function () {
        updateActiveNavItem();
    });

    // Update active state when clicking navigation links
    $('.nav-link-custom').on('click', function () {
        // Remove active from all links
        $('.nav-link-custom').removeClass('active');
        // Add active to clicked link
        $(this).addClass('active');
    });
});
// Initialize everything when document is ready
$(document).ready(function () {
    loadSavedTheme();
    initializeImageLazyLoading();
    initializeImageZoom();

    // Add click handlers for dynamic elements
    $(document).on('click', '.add-to-cart', function (e) {
        e.preventDefault();
        const productId = $(this).data('product-id');
        const quantity = $(this).data('quantity') || 1;
        addToCart(productId, quantity);
    });

    $(document).on('click', '.wishlist-btn', function (e) {
        e.preventDefault();
        const productId = $(this).data('product-id');
        addToWishlist(productId);
    });

    $(document).on('click', '.theme-toggle', function (e) {
        e.preventDefault();
        toggleTheme();
    });

    // Form validation
    $('form').on('submit', function (e) {
        if ($(this).hasClass('validate')) {
            if (!validateForm(this)) {
                e.preventDefault();
                return false;
            }
        }
    });
});

// Export functions for use in other scripts
window.SnowStore = {
    showNotification,
    addToCart,
    addToWishlist,
    validateForm,
    saveToLocalStorage,
    getFromLocalStorage,
    addToRecentlyViewed,
    toggleTheme
};