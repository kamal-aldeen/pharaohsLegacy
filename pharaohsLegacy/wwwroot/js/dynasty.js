// Dynasty Filter
document.querySelectorAll('.dyn-filter-btn').forEach(btn => {
    btn.addEventListener('click', () => {
        document.querySelectorAll('.dyn-filter-btn').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');

        const era = btn.dataset.era;
        document.querySelectorAll('.dyn-era-block').forEach(block => {
            if (era === 'all' || block.dataset.eraGroup === era) {
                block.style.display = '';
            } else {
                block.style.display = 'none';
            }
        });
    });
});

// Scroll reveal for cards
const observer = new IntersectionObserver((entries) => {
    entries.forEach(e => {
        if (e.isIntersecting) {
            e.target.style.opacity = '1';
            e.target.style.transform = 'translateY(0)';
        }
    });
}, { threshold: 0.1 });

document.querySelectorAll('.dyn-card, .dyn-det-card, .dyn-related-card').forEach(card => {
    card.style.opacity = '0';
    card.style.transform = 'translateY(20px)';
    card.style.transition = 'opacity .4s ease, transform .4s ease';
    observer.observe(card);
});