
// ========================================
// L-HUB — JAVASCRIPT PREMIUM
// ========================================
const API_BASE = 'http://localhost:5000/api';

document.addEventListener('DOMContentLoaded', () => {
    initCursor();
    initHeader();
    initCountdown();
    initMobileMenu();
    initScrollAnimations();
    initSmoothScroll();
    initStepCards();
    initSimuladoButtons();
    initFilterSimulados();
    initRipple();
    initNavActive();
    initPageTransitions();
    initAuthState();       // ← detecta login e atualiza UI
    initStudyProgress();   // ← progresso de estudos funcional
});

// ========================================
// 1. CUSTOM CURSOR
// ========================================
function initCursor() {
    if (window.innerWidth <= 768) return;
    const dot   = document.getElementById('cursor');
    const ring  = document.getElementById('cursor-ring');
    if (!dot || !ring) return;

    let mouseX = 0, mouseY = 0;
    let ringX  = 0, ringY  = 0;

    document.addEventListener('mousemove', e => {
        mouseX = e.clientX; mouseY = e.clientY;
        dot.style.transform = `translate(${mouseX}px, ${mouseY}px) translate(-50%,-50%)`;
    });

    // Ring follows with lag
    function animateRing() {
        ringX += (mouseX - ringX) * 0.14;
        ringY += (mouseY - ringY) * 0.14;
        ring.style.transform = `translate(${ringX}px, ${ringY}px) translate(-50%,-50%)`;
        requestAnimationFrame(animateRing);
    }
    animateRing();

    // Hover effect on interactive elements
    const hoverEls = 'a, button, .step-card, .simulado-card, .news-card, .filter-btn, .countdown-item';
    document.querySelectorAll(hoverEls).forEach(el => {
        el.addEventListener('mouseenter', () => document.body.classList.add('cursor-hover'));
        el.addEventListener('mouseleave', () => document.body.classList.remove('cursor-hover'));
    });

    // Click feedback
    document.addEventListener('mousedown', () => document.body.classList.add('cursor-click'));
    document.addEventListener('mouseup',   () => document.body.classList.remove('cursor-click'));

    document.addEventListener('mouseleave', () => { dot.style.opacity='0'; ring.style.opacity='0'; });
    document.addEventListener('mouseenter', () => { dot.style.opacity='1'; ring.style.opacity='0.5'; });
}

// ========================================
// 2. HEADER SCROLL
// ========================================
function initHeader() {
    const header = document.querySelector('header');
    if (!header) return;
    const onScroll = () => {
        if (window.scrollY > 50) header.classList.add('scrolled');
        else header.classList.remove('scrolled');
    };
    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
}

// ========================================
// 3. COUNTDOWN
// ========================================
function initCountdown() {
    const examDate = new Date('2026-06-15T09:00:00').getTime();
    const els = {
        days: document.getElementById('days'),
        hours: document.getElementById('hours'),
        minutes: document.getElementById('minutes'),
        seconds: document.getElementById('seconds'),
    };
    if (!els.days) return;

    let prev = {};
    function update() {
        const diff = examDate - Date.now();
        if (diff <= 0) { clearInterval(timer); return; }
        const vals = {
            days:    Math.floor(diff / 86400000),
            hours:   Math.floor((diff % 86400000) / 3600000),
            minutes: Math.floor((diff % 3600000)  / 60000),
            seconds: Math.floor((diff % 60000)    / 1000),
        };
        Object.entries(vals).forEach(([k, v]) => {
            const s = String(v).padStart(2, '0');
            if (prev[k] !== s && els[k]) {
                els[k].style.transform = 'translateY(-8px)';
                els[k].style.opacity   = '0';
                setTimeout(() => {
                    els[k].textContent   = s;
                    els[k].style.transform = 'translateY(0)';
                    els[k].style.opacity   = '1';
                }, 150);
                prev[k] = s;
            }
        });
    }
    Object.values(els).forEach(el => { if (el) el.style.transition = 'all 0.15s ease'; });
    update();
    const timer = setInterval(update, 1000);
}

// ========================================
// 4. MENU MOBILE
// ========================================
function initMobileMenu() {
    const btn  = document.getElementById('menuBtn');
    const menu = document.getElementById('mobileMenu');
    const icon = document.getElementById('menuIcon');
    if (!btn || !menu) return;

    btn.addEventListener('click', () => {
        const open = menu.classList.toggle('show');
        if (icon) icon.innerHTML = open
            ? '<line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line>'
            : '<line x1="3" y1="12" x2="21" y2="12"></line><line x1="3" y1="6" x2="21" y2="6"></line><line x1="3" y1="18" x2="21" y2="18"></line>';
    });
    menu.querySelectorAll('a').forEach(a => a.addEventListener('click', () => {
        menu.classList.remove('show');
        if (icon) icon.innerHTML = '<line x1="3" y1="12" x2="21" y2="12"></line><line x1="3" y1="6" x2="21" y2="6"></line><line x1="3" y1="18" x2="21" y2="18"></line>';
    }));
}

// ========================================
// 5. SCROLL ANIMATIONS — stagger
// ========================================
function initScrollAnimations() {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(e => {
            if (e.isIntersecting) {
                e.target.classList.add('animate-in');
                observer.unobserve(e.target);
            }
        });
    }, { threshold: 0.08, rootMargin: '0px 0px -40px 0px' });

    // Stagger children inside sections
    document.querySelectorAll('.steps-grid, .simulados-grid, .news-list, .footer-grid').forEach(container => {
        Array.from(container.children).forEach((child, i) => {
            child.classList.add('animate-on-scroll');
            child.style.transitionDelay = `${i * 0.08}s`;
            observer.observe(child);
        });
    });

    document.querySelectorAll('.animate-on-scroll').forEach(el => observer.observe(el));
}

// ========================================
// 6. SMOOTH SCROLL
// ========================================
function initSmoothScroll() {
    document.querySelectorAll('a[href^="#"]').forEach(a => {
        a.addEventListener('click', e => {
            const href = a.getAttribute('href');
            if (href === '#' || href.length < 2) return;
            e.preventDefault();
            const target = document.querySelector(href);
            if (target) window.scrollTo({ top: target.offsetTop - 80, behavior: 'smooth' });
        });
    });
}

// ========================================
// 7. STEP CARDS
// ========================================
function initStepCards() {
    document.querySelectorAll('.step-card').forEach(card => {
        card.addEventListener('click', () => {
            card.style.transform = 'scale(0.97) translateY(-4px)';
            setTimeout(() => card.style.transform = '', 200);
        });
    });
}

// ========================================
// 8. SIMULADO BUTTONS
// ========================================
function initSimuladoButtons() {
    document.querySelectorAll('.simulado-btn').forEach(btn => {
        btn.addEventListener('click', e => {
            e.stopPropagation();
            const card    = btn.closest('.simulado-card');
            const materia = card?.querySelector('.simulado-title')?.textContent.trim();
            const orig    = btn.innerHTML;

            btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Carregando...';
            btn.style.opacity = '0.85';

            setTimeout(() => {
                btn.innerHTML = orig;
                btn.style.opacity = '';
                const param = encodeURIComponent(materia || '');
                window.location.href = `simulados.html?materia=${param}`;
            }, 700);
        });
    });
}

// ========================================
// 9. FILTER SIMULADOS
// ========================================
function initFilterSimulados() {
    const btns  = document.querySelectorAll('.filter-btn');
    const cards = document.querySelectorAll('.simulado-card');
    if (!btns.length || !cards.length) return;

    btns.forEach(btn => {
        btn.addEventListener('click', () => {
            const cat = btn.getAttribute('data-category');
            btns.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');

            cards.forEach((card, i) => {
                const match = cat === 'Todas' || card.getAttribute('data-category') === cat;
                if (match) {
                    card.style.display = 'block';
                    setTimeout(() => card.classList.add('show'), 30 + i * 40);
                } else {
                    card.classList.remove('show');
                    setTimeout(() => card.style.display = 'none', 300);
                }
            });
        });
    });
}

// ========================================
// 10. RIPPLE EFFECT
// ========================================
function initRipple() {
    document.querySelectorAll('.btn, .simulado-btn, .filter-btn').forEach(el => {
        el.classList.add('ripple-container');
        el.addEventListener('click', e => {
            const rect = el.getBoundingClientRect();
            const ripple = document.createElement('span');
            ripple.classList.add('ripple');
            const size = Math.max(rect.width, rect.height) * 2;
            ripple.style.cssText = `width:${size}px;height:${size}px;left:${e.clientX-rect.left-size/2}px;top:${e.clientY-rect.top-size/2}px`;
            el.appendChild(ripple);
            setTimeout(() => ripple.remove(), 700);
        });
    });
}

// ========================================
// 11. NAV ACTIVE LINK
// ========================================
function initNavActive() {
    const page = location.pathname.split('/').pop() || 'index.html';
    document.querySelectorAll('.nav-links a, .mobile-menu a').forEach(a => {
        if (a.getAttribute('href') === page) a.classList.add('active');
    });
}

// ========================================
// 12. PAGE TRANSITIONS
// ========================================
function initPageTransitions() {
    const overlay = document.createElement('div');
    overlay.id = 'page-transition';
    overlay.style.cssText = `
        position:fixed;inset:0;z-index:99997;
        background:linear-gradient(135deg,#0f0720,#1a0a35);
        pointer-events:none;opacity:0;transition:opacity 0.35s ease;
    `;
    document.body.appendChild(overlay);

    // Fade in on load
    document.body.style.opacity = '0';
    requestAnimationFrame(() => {
        document.body.style.transition = 'opacity 0.4s ease';
        document.body.style.opacity = '1';
    });

    document.querySelectorAll('a[href]').forEach(a => {
        const href = a.getAttribute('href');
        if (!href || href.startsWith('#') || href.startsWith('mailto') || href.startsWith('tel') || href.startsWith('http')) return;
        // Skip links inside download/action areas
        if (a.closest('.prova-actions, .btn-download')) return;
        a.addEventListener('click', e => {
            e.preventDefault();
            overlay.style.opacity = '0.7';
            setTimeout(() => window.location.href = href, 320);
        });
    });
}

// ========================================
// UTILITIES
// ========================================
function formatCPF(cpf) {
    cpf = cpf.replace(/\D/g, '');
    return cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
}
function getDaysUntil(d) { return Math.floor((new Date(d) - new Date()) / 86400000); }

if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
    window.LHub = { formatCPF, getDaysUntil };
}
// ========================================
// 13. AUTH STATE — nome no header + toast
// ========================================
function initAuthState() {
    const userRaw = sessionStorage.getItem('lhub_user');
    if (!userRaw) return;

    let user;
    try { user = JSON.parse(userRaw); } catch { return; }

    const firstName = (user.nome || '').split(' ')[0];
    if (!firstName) return;

    // Atualiza o botão "Minha Área" com o primeiro nome
    const label = document.getElementById('navUserLabel');
    if (label) label.textContent = firstName;
    const mobileLabel = document.getElementById('mobileNavUserLabel');
    if (mobileLabel) mobileLabel.textContent = firstName;

    // Mostra toast apenas uma vez por sessão por usuário
    const toastKey = 'lhub_welcomed_' + (user.numero_inscricao || user.email || 'demo');
    if (sessionStorage.getItem(toastKey)) return;
    sessionStorage.setItem(toastKey, '1');

    showWelcomeToast(firstName);
}

function showWelcomeToast(firstName) {
    const toast = document.getElementById('welcome-toast');
    if (!toast) return;

    const title = document.getElementById('toast-title');
    const msg   = document.getElementById('toast-msg');
    const close = document.getElementById('toast-close');
    const bar   = document.getElementById('toast-bar');

    if (title) title.textContent = 'Ola, ' + firstName + '! Bem-vindo!';
    if (msg)   msg.textContent   = 'Voce entrou com sucesso. Bons estudos!';

    // Reinicia a animacao da barra de progresso
    if (bar) { bar.style.animation = 'none'; void bar.offsetWidth; bar.style.animation = 'toastProgress 4s linear forwards'; }

    setTimeout(() => {
        toast.classList.add('show');
        toast.classList.remove('hide');
    }, 600);

    let autoClose = setTimeout(() => dismissToast(toast), 4600);

    if (close) {
        close.addEventListener('click', () => {
            clearTimeout(autoClose);
            dismissToast(toast);
        });
    }
}

function dismissToast(toast) {
    toast.classList.add('hide');
    toast.classList.remove('show');
}

// ========================================
// 14. PROGRESSO DE ESTUDOS FUNCIONAL
// ========================================
function initStudyProgress() {
    const subjects = ['Matematica', 'Portugues', 'Ciencias'];
    const storageKey = 'lhub_study_progress';

    let progress = loadProgress(storageKey) || { 'Matematica': 0, 'Portugues': 0, 'Ciencias': 0 };

    const bars   = document.querySelectorAll('.hcard-progress .hcard-bar-fill');
    const labels = document.querySelectorAll('.hcard-progress .hcard-bar-label span:last-child');

    function renderBars() {
        bars.forEach((bar, i) => {
            const val = progress[subjects[i]] || 0;
            setTimeout(() => {
                bar.style.width = val + '%';
                if (labels[i]) labels[i].textContent = val + '%';
            }, 300 + i * 150);
        });
    }

    // Ouve evento de simulado concluido (disparado por simulados.js)
    window.addEventListener('lhub:simulado-concluido', (e) => {
        const { materia, acertos, total } = e.detail || {};
        if (!materia || !total) return;
        const pct = Math.round((acertos / total) * 100);
        const keyMap = { 'matematica': 'Matematica', 'portugues': 'Portugues', 'ciencias': 'Ciencias', 'fisica': 'Matematica', 'quimica': 'Ciencias', 'biologia': 'Ciencias', 'historia': 'Portugues', 'geografia': 'Portugues', 'ingles': 'Portugues' };
        const subj = keyMap[materia.toLowerCase()];
        if (subj) {
            progress[subj] = Math.min(100, Math.round(progress[subj] * 0.7 + pct * 0.3));
            saveProgress(storageKey, progress);
            renderBars();
        }
    });

    // Se usuario logado e sem dados reais, gera valores personalizados (demo)
    const userRaw = sessionStorage.getItem('lhub_user');
    if (userRaw) {
        try {
            const u = JSON.parse(userRaw);
            const hasRealData = Object.values(progress).some(v => v > 0);
            if (!hasRealData) {
                const seed = parseInt((u.numero_inscricao || '12345').replace(/\D/g, '').slice(-5)) || 12345;
                progress['Matematica'] = 40 + (seed % 50);
                progress['Portugues']  = 35 + ((seed * 3) % 55);
                progress['Ciencias']   = 50 + ((seed * 7) % 45);
                saveProgress(storageKey, progress);
            }
        } catch {}
    }

    renderBars();
}

function loadProgress(key) {
    try { const r = sessionStorage.getItem(key); return r ? JSON.parse(r) : null; } catch { return null; }
}
function saveProgress(key, data) {
    try { sessionStorage.setItem(key, JSON.stringify(data)); } catch {}
}
