document.addEventListener('DOMContentLoaded', function () {

    //// ===== PAGE LOADER =====
    //var loader = document.getElementById('page-loader');
    //if (loader) {
    //    setTimeout(function () {
    //        loader.style.opacity = '0';
    //        loader.style.transition = 'opacity .5s ease';
    //        setTimeout(function () { loader.style.display = 'none'; }, 500);
    //    }, 1500);
    //}

   
    var nav = document.querySelector('nav.main-nav');
    window.addEventListener('scroll', function () {
        if (window.scrollY > 50) {
            nav.classList.add('scrolled');
        } else {
            nav.classList.remove('scrolled');
        }
    });

    
    var toggle = document.getElementById('navToggle');
    var navLinks = document.getElementById('navLinks');
    if (toggle && navLinks) {
        toggle.addEventListener('click', function () {
            navLinks.classList.toggle('open');
            toggle.classList.toggle('active');
        });
        
        document.addEventListener('click', function (e) {
            if (!nav.contains(e.target)) {
                navLinks.classList.remove('open');
                toggle.classList.remove('active');
            }
        });
    }

    
    var backToTop = document.getElementById('back-to-top');
    if (backToTop) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 400) {
                backToTop.classList.add('visible');
            } else {
                backToTop.classList.remove('visible');
            }
        });
        backToTop.addEventListener('click', function () {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }

    
    var reveals = document.querySelectorAll('.reveal');
    if (reveals.length > 0) {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1 });
        reveals.forEach(function (el) { observer.observe(el); });
    }

   
    document.querySelectorAll('img').forEach(function (img) {
        img.addEventListener('error', function () {
            this.src = 'https://via.placeholder.com/400x300/2d1a08/c9a227?text=𓂀';
            this.style.objectFit = 'contain';
            this.style.padding = '1rem';
        });
    });

    
    window.showToast = function (message, type) {
        var toast = document.getElementById('toast');
        if (!toast) {
            toast = document.createElement('div');
            toast.id = 'toast';
            document.body.appendChild(toast);
        }
        toast.textContent = message;
        toast.className = type === 'gold' ? 'gold' : '';
        toast.classList.add('show');
        setTimeout(function () { toast.classList.remove('show'); }, 3000);
    };

    
    var searchInput = document.querySelector('.nav-search input');
    if (searchInput) {
        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter' && this.value.trim()) {
                window.location.href = '/Home/Search?q=' + encodeURIComponent(this.value.trim());
            }
        });
    }

    
    var stats = document.querySelectorAll('.stat-num');
    if (stats.length > 0) {
        var isArabicPage = document.documentElement.lang === 'ar';
        var arabicToEnglishMap = { '٠': '0', '١': '1', '٢': '2', '٣': '3', '٤': '4', '٥': '5', '٦': '6', '٧': '7', '٨': '8', '٩': '9' };
        var englishDigitsMap = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];

        function normalizeDigits(str) {
            // بيحول أي رقم عربي-هندي (٠-٩) موجود في النص لرقم إنجليزي عادي، عشان الـ parseInt يقدر يقرأه صح
            return str.replace(/[٠-٩]/g, function (d) { return arabicToEnglishMap[d]; });
        }

        function toLocalDigits(str) {
            if (!isArabicPage) return str;
            return str.replace(/[0-9]/g, function (d) { return englishDigitsMap[+d]; });
        }

        var statsObserver = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    var el = entry.target;
                    var normalized = normalizeDigits(el.textContent).replace(/,/g, '');
                    var match = normalized.match(/^(\d+)(.*)$/);
                    var target = match ? parseInt(match[1]) : 0;
                    var suffix = match ? match[2] : '';
                    var current = 0;
                    var step = Math.ceil(target / 50);
                    var timer = setInterval(function () {
                        current += step;
                        if (current >= target) {
                            current = target;
                            clearInterval(timer);
                        }
                        el.textContent = toLocalDigits(current.toLocaleString('en-US') + suffix);
                    }, 30);
                    statsObserver.unobserve(el);
                }
            });
        }, { threshold: 0.5 });
        stats.forEach(function (el) { statsObserver.observe(el); });
    }

   
    var currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.nav-links a').forEach(function (link) {
        var href = link.getAttribute('href') || '';
        if (currentPath.includes(href.toLowerCase()) && href !== '/') {
            link.style.color = '#c9a227';
        }
    });

});