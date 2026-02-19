// ========================================
// PROVAS ANTERIORES - JAVASCRIPT
// ========================================

const API_URL = 'http://localhost:5000/api';
let provasData = [];
let filteredProvas = [];
let currentProva = null;

document.addEventListener('DOMContentLoaded', function() {
    console.log('📚 Página de Provas inicializada');
    
    // Carregar provas
    loadProvas();
    
    // Inicializar menu mobile (do arquivo java.js)
    if (typeof initMobileMenu === 'function') {
        initMobileMenu();
    }
});

// ========================================
// CARREGAR PROVAS DA API
// ========================================
async function loadProvas() {
    const loadingEl = document.getElementById('loadingProvas');
    const gridEl = document.getElementById('provasGrid');
    const emptyEl = document.getElementById('emptyState');
    
    // Mostrar loading
    loadingEl.style.display = 'block';
    gridEl.innerHTML = '';
    emptyEl.style.display = 'none';
    
    try {
        const response = await fetch(`${API_URL}/provas`);
        
        if (!response.ok) {
            throw new Error('Erro ao carregar provas');
        }
        
        const data = await response.json();
        provasData = data;
        filteredProvas = data;
        
        // Renderizar provas
        renderProvas(filteredProvas);
        
        // Atualizar contador
        document.getElementById('totalProvas').textContent = data.length;
        
    } catch (error) {
        console.error('Erro:', error);
        
        // Mostrar empty state
        emptyEl.style.display = 'block';
        emptyEl.querySelector('h3').textContent = 'Erro ao carregar provas';
        emptyEl.querySelector('p').textContent = 'Tente novamente mais tarde';
    } finally {
        loadingEl.style.display = 'none';
    }
}

// ========================================
// RENDERIZAR PROVAS
// ========================================
function renderProvas(provas) {
    const gridEl = document.getElementById('provasGrid');
    const emptyEl = document.getElementById('emptyState');
    
    if (provas.length === 0) {
        gridEl.innerHTML = '';
        emptyEl.style.display = 'block';
        return;
    }
    
    emptyEl.style.display = 'none';
    
    gridEl.innerHTML = provas.map(prova => `
        <div class="prova-card" data-id="${prova.id}" data-ano="${prova.ano}" data-periodo="${prova.periodo}" data-area="${prova.area || 'geral'}">
            <div class="prova-header">
                <div class="prova-year">${prova.ano}</div>
                <div class="prova-badge">${prova.periodo}º Sem</div>
            </div>
            
            <h3 class="prova-title">Vestibular ${prova.ano}</h3>
            <p class="prova-subtitle">${prova.periodo}º Semestre - ${prova.nome || 'Prova Geral'}</p>
            
            <div class="prova-info">
                <div class="prova-info-item">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"></path>
                        <path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"></path>
                    </svg>
                    ${prova.questoes || 60} questões
                </div>
                <div class="prova-info-item">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <circle cx="12" cy="12" r="10"></circle>
                        <polyline points="12 6 12 12 16 14"></polyline>
                    </svg>
                    ${prova.duracao || 4} horas
                </div>
                <div class="prova-info-item">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                        <polyline points="7 10 12 15 17 10"></polyline>
                        <line x1="12" y1="15" x2="12" y2="3"></line>
                    </svg>
                    ${prova.downloads || 0} downloads
                </div>
            </div>
            
            <div class="prova-actions">
                <button class="btn-icon btn-view" onclick="viewProva(${prova.id})">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                        <circle cx="12" cy="12" r="3"></circle>
                    </svg>
                    Ver
                </button>
                <button class="btn-icon btn-download" onclick="downloadProvaFile(${prova.id}, 'prova')">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                        <polyline points="7 10 12 15 17 10"></polyline>
                        <line x1="12" y1="15" x2="12" y2="3"></line>
                    </svg>
                    Baixar
                </button>
            </div>
        </div>
    `).join('');
}

// ========================================
// FILTROS
// ========================================
function filterProvas() {
    const anoFilter = document.getElementById('filterAno').value;
    const periodoFilter = document.getElementById('filterPeriodo').value;
    const cursoFilter = document.getElementById('filterCurso').value;
    
    filteredProvas = provasData.filter(prova => {
        const matchAno = !anoFilter || prova.ano == anoFilter;
        const matchPeriodo = !periodoFilter || prova.periodo == periodoFilter;
        const matchCurso = !cursoFilter || prova.area == cursoFilter;
        
        return matchAno && matchPeriodo && matchCurso;
    });
    
    renderProvas(filteredProvas);
    
    // Atualizar contador
    document.getElementById('totalProvas').textContent = filteredProvas.length;
}

function clearFilters() {
    document.getElementById('filterAno').value = '';
    document.getElementById('filterPeriodo').value = '';
    document.getElementById('filterCurso').value = '';
    
    filterProvas();
}

// ========================================
// VISUALIZAR PROVA
// ========================================
function viewProva(id) {
    const prova = provasData.find(p => p.id === id);
    if (!prova) return;
    
    currentProva = prova;
    
    const modal = document.getElementById('provaModal');
    const modalTitle = document.getElementById('modalTitle');
    const modalBody = document.getElementById('modalBody');
    
    modalTitle.textContent = `Vestibular ${prova.ano} - ${prova.periodo}º Semestre`;
    
    modalBody.innerHTML = `
        <div style="margin-bottom: 1.5rem;">
            <h3 style="color: var(--primary-dark); margin-bottom: 0.5rem;">Informações da Prova</h3>
            <div style="display: grid; gap: 1rem; background: #f9fafb; padding: 1rem; border-radius: 0.75rem;">
                <div style="display: flex; justify-content: space-between;">
                    <span style="color: #6b7280; font-weight: 600;">Ano:</span>
                    <span style="color: var(--primary-dark); font-weight: 700;">${prova.ano}</span>
                </div>
                <div style="display: flex; justify-content: space-between;">
                    <span style="color: #6b7280; font-weight: 600;">Período:</span>
                    <span style="color: var(--primary-dark); font-weight: 700;">${prova.periodo}º Semestre</span>
                </div>
                <div style="display: flex; justify-content: space-between;">
                    <span style="color: #6b7280; font-weight: 600;">Questões:</span>
                    <span style="color: var(--primary-dark); font-weight: 700;">${prova.questoes || 60}</span>
                </div>
                <div style="display: flex; justify-content: space-between;">
                    <span style="color: #6b7280; font-weight: 600;">Duração:</span>
                    <span style="color: var(--primary-dark); font-weight: 700;">${prova.duracao || 4} horas</span>
                </div>
            </div>
        </div>
        
        <div style="margin-bottom: 1.5rem;">
            <h3 style="color: var(--primary-dark); margin-bottom: 1rem;">Arquivos Disponíveis</h3>
            <div style="display: flex; flex-direction: column; gap: 0.75rem;">
                <button class="btn btn-primary" onclick="downloadProvaFile(${prova.id}, 'prova')" style="width: 100%; justify-content: center;">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                        <polyline points="14 2 14 8 20 8"></polyline>
                        <line x1="16" y1="13" x2="8" y2="13"></line>
                        <line x1="16" y1="17" x2="8" y2="17"></line>
                    </svg>
                    Baixar Prova (PDF)
                </button>
                <button class="btn btn-secondary" onclick="downloadProvaFile(${prova.id}, 'gabarito')" style="width: 100%; justify-content: center;">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <polyline points="20 6 9 17 4 12"></polyline>
                    </svg>
                    Baixar Gabarito (PDF)
                </button>
            </div>
        </div>
        
        <div style="background: #fef3c7; padding: 1rem; border-radius: 0.75rem; border-left: 4px solid #f59e0b;">
            <p style="color: #92400e; font-weight: 600; margin: 0;">
                💡 Dica: Faça a prova simulando as condições reais do vestibular para ter uma experiência mais próxima do dia da prova.
            </p>
        </div>
    `;
    
    modal.classList.add('show');
}

function closeProvaModal() {
    const modal = document.getElementById('provaModal');
    modal.classList.remove('show');
    currentProva = null;
}

// ========================================
// DOWNLOAD
// ========================================
async function downloadProvaFile(id, tipo) {
    const prova = provasData.find(p => p.id === id);
    if (!prova) return;
    
    try {
        // Incrementar contador de downloads
        const totalEl = document.getElementById('totalDownloads');
        totalEl.textContent = parseInt(totalEl.textContent) + 1;
        
        // Simular download (em produção, baixar arquivo real)
        const url = tipo === 'prova' ? prova.url : prova.gabarito_url;
        
        showToast(`Download iniciado: ${tipo === 'prova' ? 'Prova' : 'Gabarito'} ${prova.ano}`, 'success');
        
        // Em produção, fazer download real:
        // window.open(url, '_blank');
        
        console.log(`Downloading: ${url}`);
        
    } catch (error) {
        console.error('Erro no download:', error);
        showToast('Erro ao baixar arquivo', 'error');
    }
}

function downloadProva() {
    if (currentProva) {
        downloadProvaFile(currentProva.id, 'prova');
    }
}

// ========================================
// TOAST NOTIFICATIONS
// ========================================
function showToast(message, type = 'info') {
    const existingToast = document.querySelector('.toast');
    if (existingToast) {
        existingToast.remove();
    }
    
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    
    Object.assign(toast.style, {
        position: 'fixed',
        top: '100px',
        right: '20px',
        padding: '1rem 1.5rem',
        borderRadius: '0.75rem',
        color: 'white',
        fontWeight: '600',
        zIndex: '10000',
        animation: 'slideInRight 0.3s ease',
        boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)'
    });
    
    const colors = {
        success: '#22c55e',
        error: '#ef4444',
        warning: '#f59e0b',
        info: '#3b82f6'
    };
    
    toast.style.background = colors[type] || colors.info;
    
    document.body.appendChild(toast);
    
    setTimeout(() => {
        toast.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Adicionar animações
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(400px);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(400px);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);

console.log('✅ Sistema de provas pronto!');