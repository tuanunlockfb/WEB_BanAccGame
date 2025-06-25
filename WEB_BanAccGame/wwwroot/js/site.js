// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Dark Mode
const initDarkMode = () => {
    const themeToggle = document.getElementById('themeToggle');
    const htmlElement = document.documentElement;
    const icon = themeToggle?.querySelector('i');
    
    // Check saved theme
    const savedTheme = localStorage.getItem('theme') || 'light';
    htmlElement.setAttribute('data-theme', savedTheme);
    updateThemeIcon(savedTheme);
    
    themeToggle?.addEventListener('click', () => {
        const currentTheme = htmlElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        
        htmlElement.setAttribute('data-theme', newTheme);
        localStorage.setItem('theme', newTheme);
        updateThemeIcon(newTheme);
    });
    
    function updateThemeIcon(theme) {
        if (icon) {
            icon.className = theme === 'light' ? 'bi bi-moon-fill' : 'bi bi-sun-fill';
        }
    }
};

// Navbar Scroll Effect
const initNavbarScroll = () => {
    const navbar = document.getElementById('mainNav');
    
    window.addEventListener('scroll', () => {
        if (window.scrollY > 50) {
            navbar?.classList.add('scrolled');
        } else {
            navbar?.classList.remove('scrolled');
        }
    });
};

// Back to Top Button
const initBackToTop = () => {
    const backToTopBtn = document.getElementById('backToTop');
    
    window.addEventListener('scroll', () => {
        if (window.scrollY > 300) {
            backToTopBtn.style.display = 'block';
        } else {
            backToTopBtn.style.display = 'none';
        }
    });
    
    backToTopBtn?.addEventListener('click', () => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
};

// Hide Loader
const hideLoader = () => {
    const loader = document.getElementById('loader');
    setTimeout(() => {
        loader?.classList.add('hidden');
    }, 500);
};

// Update Cart Badge
// Cart badge update - Not needed anymore since we use direct purchase
// function updateCartBadge() {
//     $.get('/ShoppingCart/GetCartItemCount', function(data) {
//         const badge = $('#cart-badge');
//         if (data.count > 0) {
//             badge.text(data.count).show();
//             badge.addClass('animate__animated animate__bounceIn');
//         } else {
//             badge.hide();
//         }
//     });
// }

// Add to Cart with Animation
function addToCart(productId, shouldRedirect = false) {
    $.post('/ShoppingCart/AddToCart', { id: productId }, function(response) {
        // updateCartBadge(); // Not needed with direct purchase
        showToast('success', 'Đã thêm vào giỏ hàng!');
        
        // Nếu cần redirect đến giỏ hàng
        if (shouldRedirect) {
            setTimeout(function() {
                window.location.href = '/ShoppingCart';
            }, 1000);
        }
    }).fail(function(xhr) {
        if (xhr.status === 404) {
            showToast('error', 'Sản phẩm không tồn tại!');
        } else {
            showToast('error', 'Có lỗi xảy ra. Vui lòng thử lại.');
        }
    });
}

// Show Toast Notification
function showToast(type, message) {
    const toastHtml = `
        <div class="toast-container position-fixed top-0 end-0 p-3">
            <div class="toast show animate__animated animate__fadeInRight" role="alert">
                <div class="toast-header bg-${type === 'success' ? 'success' : 'danger'} text-white">
                    <i class="bi bi-${type === 'success' ? 'check-circle' : 'x-circle'} me-2"></i>
                    <strong class="me-auto">${type === 'success' ? 'Thành công' : 'Lỗi'}</strong>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        </div>
    `;
    
    const $toast = $(toastHtml);
    $('body').append($toast);
    
    setTimeout(() => {
        $toast.find('.toast').removeClass('animate__fadeInRight').addClass('animate__fadeOutRight');
        setTimeout(() => $toast.remove(), 500);
    }, 3000);
}

// Add to Wishlist
function toggleWishlist(productId, element) {
    const $btn = $(element);
    const isActive = $btn.hasClass('active');
    
    $.post('/Wishlist/Toggle', { productId: productId }, function(response) {
        if (response.success) {
            if (isActive) {
                $btn.removeClass('active');
                $btn.find('i').removeClass('bi-heart-fill').addClass('bi-heart');
                showToast('success', 'Đã xóa khỏi yêu thích');
            } else {
                $btn.addClass('active');
                $btn.find('i').removeClass('bi-heart').addClass('bi-heart-fill');
                showToast('success', 'Đã thêm vào yêu thích');
            }
        }
    });
}

// Initialize on DOM Ready
$(document).ready(function() {
    // Initialize features
    initDarkMode();
    initNavbarScroll();
    initBackToTop();
    hideLoader();
                    // updateCartBadge(); // Not needed with direct purchase
    
    // Smooth scroll for anchor links
    $('a[href^="#"]').on('click', function(e) {
        e.preventDefault();
        const target = $($(this).attr('href'));
        if (target.length) {
            $('html, body').animate({
                scrollTop: target.offset().top - 80
            }, 500);
        }
    });
    
    // Add to cart buttons
    $(document).on('submit', 'form[action*="AddToCart"]', function(e) {
        // Kiểm tra nếu form có class 'ajax-submit' thì mới dùng AJAX
        if ($(this).hasClass('ajax-submit')) {
            e.preventDefault();
            const productId = $(this).find('input[name="id"]').val();
            addToCart(productId);
        }
        // Nếu không có class 'ajax-submit', cho phép form submit bình thường
    });
    
    // Product image zoom on hover
    $('.product-card').hover(
        function() {
            $(this).find('.card-img-top').css('transform', 'scale(1.1)');
        },
        function() {
            $(this).find('.card-img-top').css('transform', 'scale(1)');
        }
    );
    
    // Animate elements on scroll
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate__animated', 'animate__fadeInUp');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);
    
    document.querySelectorAll('.fade-in').forEach(el => {
        observer.observe(el);
    });
    
    // Search autocomplete (if needed)
    $('input[name="searchString"]').on('input', function() {
        const query = $(this).val();
        if (query.length >= 2) {
            // Implement autocomplete here
        }
    });
    
    // Rating stars
    $('.rating-stars').on('click', '.star', function() {
        const rating = $(this).data('rating');
        const $container = $(this).closest('.rating-stars');
        
        $container.find('.star').each(function() {
            const starRating = $(this).data('rating');
            if (starRating <= rating) {
                $(this).removeClass('bi-star').addClass('bi-star-fill text-warning');
            } else {
                $(this).removeClass('bi-star-fill text-warning').addClass('bi-star');
            }
        });
        
        $container.find('input[name="rating"]').val(rating);
    });
});

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// Countdown timer for flash sales
function startCountdown(endDate, elementId) {
    const countdownElement = document.getElementById(elementId);
    if (!countdownElement) return;
    
    const timer = setInterval(function() {
        const now = new Date().getTime();
        const distance = new Date(endDate).getTime() - now;
        
        if (distance < 0) {
            clearInterval(timer);
            countdownElement.innerHTML = "ĐÃ KẾT THÚC";
            return;
        }
        
        const days = Math.floor(distance / (1000 * 60 * 60 * 24));
        const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((distance % (1000 * 60)) / 1000);
        
        countdownElement.innerHTML = `${days}d ${hours}h ${minutes}m ${seconds}s`;
    }, 1000);
}

// Image preview for uploads
function previewImage(input, previewId) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function(e) {
            $('#' + previewId).attr('src', e.target.result);
        };
        reader.readAsDataURL(input.files[0]);
    }
}
