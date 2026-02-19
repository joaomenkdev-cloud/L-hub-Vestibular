// ============================================================
//  AREA-CANDIDATO.JS — L-Hub Vestibular
// ============================================================

const API_URL = 'http://localhost:5000/api';
let currentUser = null;

// ── Utilitários ──────────────────────────────────────────────
function formatDate(str) {
    if (!str) return '—';
    const [y,m,d] = str.split('-');
    const months = ['jan','fev','mar','abr','mai','jun','jul','ago','set','out','nov','dez'];
    return `${d} ${months[parseInt(m)-1]} ${y}`;
}

function showAlert(id, type, msg) {
    const el = document.getElementById(id);
    el.className = `alert alert-${type} visible`;
    const sp = el.querySelector('span');
    if (sp) sp.textContent = msg; else el.textContent = msg;
}

function hideAlert(id) { document.getElementById(id).className = 'alert'; }

function setLoading(btnId, textId, spinnerId, loading) {
    document.getElementById(btnId).disabled = loading;
    document.getElementById(textId).style.display  = loading ? 'none' : 'inline';
    document.getElementById(spinnerId).style.display = loading ? 'block' : 'none';
}

// ── Auth ─────────────────────────────────────────────────────
function showLogin() {
    document.getElementById('loginPage').style.display = 'flex';
    document.getElementById('dashboardPage').classList.remove('visible');
}

function showDashboard(userData, inscricaoData) {
    document.getElementById('loginPage').style.display = 'none';
    document.getElementById('dashboardPage').classList.add('visible');
    currentUser = userData;

    // Header
    document.getElementById('nomeUsuario').textContent = userData.nome.split(' ')[0];
    document.getElementById('sbNumero').textContent    = userData.numero_inscricao;
    document.getElementById('sbCurso').textContent     = inscricaoData?.curso || '—';

    const pagSt = inscricaoData?.pagamento?.status || 'pendente';
    const sbPag = document.getElementById('sbPagamento');
    sbPag.textContent = pagSt === 'confirmado' ? 'Confirmado ✓' : 'Pendente';
    sbPag.className   = 'status-bar-value ' + (pagSt === 'confirmado' ? 'ok' : 'pending');

    const statusMap = { aguardando_pagamento:'Aguardando Pagamento', inscrito:'Inscrito', concluido:'Concluído' };
    document.getElementById('sbStatus').textContent = statusMap[inscricaoData?.status] || inscricaoData?.status || '—';

    // Dias para a prova
    const days = Math.max(0, Math.ceil((new Date('2026-06-15') - new Date()) / 86400000));
    document.getElementById('statDias').textContent = days;

    renderDadosInscricao(inscricaoData);
    renderMiniSteps(inscricaoData);
    renderPagamento(inscricaoData);
    renderDocumentos(inscricaoData);
    carregarComunicados();

    setTimeout(initScrollAnimations, 100);
}

async function doLogin() {
    const numero = document.getElementById('inscricaoNum').value.trim();
    const senha  = document.getElementById('loginSenha').value;

    let valid = true;
    if (!numero) { document.getElementById('errInscricao').classList.add('visible'); document.getElementById('inscricaoNum').classList.add('error'); valid=false; }
    else          { document.getElementById('errInscricao').classList.remove('visible'); document.getElementById('inscricaoNum').classList.remove('error'); }
    if (!senha)  { document.getElementById('errSenha').classList.add('visible'); document.getElementById('loginSenha').classList.add('error'); valid=false; }
    else          { document.getElementById('errSenha').classList.remove('visible'); document.getElementById('loginSenha').classList.remove('error'); }
    if (!valid) return;

    hideAlert('loginError');
    setLoading('loginBtn','loginBtnText','loginSpinner',true);

    try {
        const res  = await fetch(`${API_BASE}/auth/login`, {
            method:'POST',
            headers:{'Content-Type':'application/json'},
            body: JSON.stringify({ numero_inscricao: numero, senha }),
            signal: AbortSignal.timeout(8000)
        });
        const data = await res.json();
        if (!res.ok || !data.success) throw new Error(data.message || 'Erro ao fazer login');

        sessionStorage.setItem('lhub_token',    data.token);
        sessionStorage.setItem('lhub_user',     JSON.stringify(data.usuario));
        sessionStorage.setItem('lhub_inscricao',JSON.stringify(data.inscricao));
        showDashboard(data.usuario, data.inscricao);

    } catch (err) {
        // Fallback demo
        if (numero.length >= 5 && senha.length >= 4) {
            const demoUser = { numero_inscricao:numero, nome:'Candidato Demo', email:'demo@lhub.com.br', primeiro_acesso:false };
            const demoInsc = { curso:'Ciência da Computação', status:'aguardando_pagamento', pagamento:{ status:'pendente', vencimento:'2026-03-18' } };
            sessionStorage.setItem('lhub_user',     JSON.stringify(demoUser));
            sessionStorage.setItem('lhub_inscricao',JSON.stringify(demoInsc));
            showDashboard(demoUser, demoInsc);
        } else {
            showAlert('loginError','error', err.message || 'Credenciais inválidas.');
        }
    } finally {
        setLoading('loginBtn','loginBtnText','loginSpinner',false);
    }
}

function doLogout() {
    const token = sessionStorage.getItem('lhub_token');
    if (token) fetch(`${API_BASE}/auth/logout`, { method:'POST', headers:{ Authorization:`Bearer ${token}` } }).catch(()=>{});
    sessionStorage.clear();
    currentUser = null;
    document.getElementById('loginSenha').value  = '';
    document.getElementById('inscricaoNum').value= '';
    showLogin();
}

// ── Renderização ─────────────────────────────────────────────
function renderDadosInscricao(data) {
    const el = document.getElementById('dadosInscricao');
    if (!data) { el.innerHTML='<p style="opacity:0.5;font-size:0.9rem">Dados não disponíveis.</p>'; return; }
    const statusMap = { aguardando_pagamento:'⚠️ Aguardando Pagamento', inscrito:'✅ Inscrito', concluido:'🎓 Concluído' };
    const rows = [
        { label:'Número de Inscrição', value: currentUser?.numero_inscricao || '—' },
        { label:'Nome',                value: currentUser?.nome || '—' },
        { label:'E-mail',              value: currentUser?.email || '—' },
        { label:'Curso',               value: data.curso || '—' },
        { label:'Status',              value: statusMap[data.status] || data.status || '—' }
    ];
    el.innerHTML = rows.map(r=>`
        <div class="info-row">
            <span class="info-row-label">${r.label}</span>
            <span class="info-row-value">${r.value}</span>
        </div>`).join('');
}

function renderMiniSteps(data) {
    const pagOk   = data?.pagamento?.status === 'confirmado';
    const inscrito= data?.status === 'inscrito' || data?.status === 'concluido';
    const steps = [
        { name:'Inscrição',     desc:'Dados preenchidos',           state:'done' },
        { name:'Pagamento',     desc: pagOk ? 'Confirmado' : 'Aguardando pagamento', state: pagOk ? 'done' : 'active' },
        { name:'Local de Prova',desc:'Disponível em 20/05',         state: inscrito ? 'active' : 'pending' },
        { name:'Resultado',     desc:'Disponível em 30/06',         state:'pending' }
    ];
    document.getElementById('miniSteps').innerHTML = steps.map(s=>`
        <div class="mini-step">
            <div class="mini-step-dot ${s.state}"></div>
            <div class="mini-step-text">
                <div class="mini-step-name">${s.name}</div>
                <div class="mini-step-desc">${s.desc}</div>
            </div>
            <span class="mini-step-status ${s.state}">
                ${s.state==='done' ? '✓ Feito' : s.state==='active' ? '● Ativo' : '○ Pendente'}
            </span>
        </div>`).join('');
}

function renderPagamento(data) {
    const el  = document.getElementById('pagamentoSection');
    const pag = data?.pagamento || {};
    const pago= pag.status === 'confirmado';
    el.innerHTML = `
        <div class="payment-status-card ${pago ? 'pago' : ''}">
            <div class="payment-info">
                <h4>${pago ? '✅ Pagamento Confirmado' : '⚠️ Pagamento Pendente'}</h4>
                <p>${pago ? 'Taxa confirmada com sucesso.' : `Vence em ${formatDate(pag.vencimento)||'18 mar 2026'}`}</p>
            </div>
            <div class="payment-amount">R$ 85,00</div>
        </div>
        ${!pago ? `
            <p style="font-size:0.85rem;color:var(--dark);opacity:0.65;margin-bottom:0.75rem">Código de barras:</p>
            <div class="boleto-code">34191.79001 01043.510047 91020.150008 1 89370000008500</div>
            <button class="btn btn-blue btn-full" style="margin-bottom:0.5rem" onclick="copyBoleto()">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
                    <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
                </svg>
                Copiar Código do Boleto
            </button>
            <button class="btn btn-outline btn-full" style="font-size:0.9rem" onclick="downloadBoleto()">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                    <polyline points="7 10 12 15 17 10"></polyline>
                    <line x1="12" y1="15" x2="12" y2="3"></line>
                </svg>
                Baixar Boleto PDF
            </button>` : `<p style="color:#15803d;font-weight:600;font-size:0.85rem">Pago em ${formatDate(pag.data_pagamento)||'—'}</p>`
        }`;
}

function renderDocumentos(data) {
    const el   = document.getElementById('documentosSection');
    const pagOk= data?.pagamento?.status === 'confirmado';
    const docs = [
        { name:'Comprovante de Inscrição', meta:'Gerado automaticamente',          icon:'#dbeafe', stroke:'#1d4ed8', disabled:false, action:"downloadComprovante()" },
        { name:'Boleto de Pagamento',      meta:'Taxa de inscrição - R$ 85,00',    icon:'#fef3c7', stroke:'#d97706', disabled:false, action:"downloadBoleto()" },
        { name:'Cartão de Confirmação',    meta: pagOk ? 'Disponível' : 'Disponível após pagamento', icon: pagOk?'#f0fdf4':'#f3f4f6', stroke: pagOk?'#15803d':'#9ca3af', disabled:!pagOk, action: pagOk?"downloadCartao()":"null" },
        { name:'Local de Prova',           meta:'Disponível em 20/05/2026',        icon:'#f3f4f6', stroke:'#9ca3af', disabled:true, action:"null" }
    ];
    el.innerHTML = docs.map(d=>`
        <div class="doc-item ${d.disabled ? 'disabled' : ''}"
             onclick="${d.action}"
             style="${d.disabled ? 'opacity:0.5;cursor:not-allowed;' : ''}">
            <div class="doc-icon" style="background:${d.icon}">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="${d.stroke}" stroke-width="2">
                    <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                    <polyline points="14 2 14 8 20 8"></polyline>
                </svg>
            </div>
            <div class="doc-info">
                <div class="doc-name">${d.name}</div>
                <div class="doc-meta">${d.meta}</div>
            </div>
            ${!d.disabled ? `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#9ca3af" stroke-width="2">
                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                <polyline points="7 10 12 15 17 10"></polyline>
                <line x1="12" y1="15" x2="12" y2="3"></line>
            </svg>` : ''}
        </div>`).join('');
}

// ── Comunicados da API ────────────────────────────────────────
async function carregarComunicados() {
    try {
        const res  = await fetch(`${API_BASE}/noticias/recentes?limite=3`, { signal: AbortSignal.timeout(4000) });
        const data = await res.json();
        if (!res.ok || !data.success) throw new Error();
        renderComunicados(data.data);
    } catch {
        // já mostrado estático no HTML
    }
}

function renderComunicados(lista) {
    if (!lista || lista.length === 0) return;
    const colors = { urgente:'#ef4444', edital:'var(--primary)', novo:'#22c55e', importante:'#f97316', noticia:'var(--primary)' };
    const el = document.getElementById('comunicadosList');
    if (!el) return;
    el.innerHTML = lista.map(n => {
        const dtStr = n.publicado_em ? n.publicado_em.slice(0,10) : '';
        const cor   = colors[n.badge_tipo] || 'var(--primary)';
        return `<div class="comunicado-item">
            <div class="comunicado-dot" style="background:${cor}"></div>
            <div class="comunicado-content">
                <h5>${n.titulo}</h5>
                <p>${n.resumo || ''}</p>
                <div class="comunicado-date">${formatDate(dtStr)}</div>
            </div>
        </div>`;
    }).join('');
}

// ── Ações de documentos ───────────────────────────────────────
function copyBoleto() {
    navigator.clipboard.writeText('34191790010104351004791020150008189370000008500')
        .then(()=>alert('✓ Código copiado!'))
        .catch(()=>alert('Copie manualmente:\n34191.79001 01043.510047 91020.150008 1 89370000008500'));
}
function downloadBoleto()      { alert('📄 Baixando boleto...\n(Em produção, gera PDF do boleto bancário)'); }
function downloadComprovante() { alert('📄 Baixando comprovante...\n(Em produção, gera PDF com dados da inscrição)'); }
function downloadCartao()      { alert('📄 Baixando cartão de confirmação...\n(Em produção, gera PDF do cartão)'); }

// ── Modal trocar senha ────────────────────────────────────────
function openPasswordModal()  { document.getElementById('passwordModal').classList.add('open'); }
function closePasswordModal() {
    document.getElementById('passwordModal').classList.remove('open');
    ['senhaAtual','senhaNova','senhaConfirm'].forEach(id=>{ document.getElementById(id).value=''; });
    document.getElementById('pwAlert').className = 'alert';
}

async function saveSenha() {
    const atual   = document.getElementById('senhaAtual').value;
    const nova    = document.getElementById('senhaNova').value;
    const confirm = document.getElementById('senhaConfirm').value;

    if (!atual||!nova||!confirm) { showAlert('pwAlert','error','Preencha todos os campos.'); return; }
    if (nova.length<6)           { showAlert('pwAlert','error','Mínimo 6 caracteres.'); return; }
    if (nova!==confirm)          { showAlert('pwAlert','error','As senhas não coincidem.'); return; }

    setLoading('saveSenhaBtn','saveSenhaBtnText','saveSenhaSpinner',true);
    try {
        const res  = await fetch(`${API_BASE}/auth/trocar-senha`, {
            method:'POST',
            headers:{'Content-Type':'application/json'},
            body: JSON.stringify({ numero_inscricao: currentUser?.numero_inscricao, senha_atual:atual, senha_nova:nova }),
            signal: AbortSignal.timeout(5000)
        });
        const data = await res.json();
        if (!res.ok||!data.success) throw new Error(data.message||'Erro');
        showAlert('pwAlert','success','Senha alterada com sucesso!');
        setTimeout(closePasswordModal,1500);
    } catch {
        showAlert('pwAlert','success','Senha alterada! (demo)');
        setTimeout(closePasswordModal,1500);
    } finally {
        setLoading('saveSenhaBtn','saveSenhaBtnText','saveSenhaSpinner',false);
    }
}

// ── Scroll animations ─────────────────────────────────────────
function initScrollAnimations() {
    const obs = new IntersectionObserver(entries=>{
        entries.forEach(e=>{ if(e.isIntersecting) e.target.classList.add('animate-in'); });
    }, { threshold:0.1 });
    document.querySelectorAll('.animate-on-scroll').forEach(el=>obs.observe(el));
}

// ── Menu mobile ───────────────────────────────────────────────
function initMobileMenu() {
    const btn  = document.getElementById('menuBtn');
    const menu = document.getElementById('mobileMenu');
    const icon = document.getElementById('menuIcon');
    if (!btn) return;
    btn.addEventListener('click',()=>{
        menu.classList.toggle('show');
        const open = menu.classList.contains('show');
        icon.innerHTML = open
            ? '<line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line>'
            : '<line x1="3" y1="12" x2="21" y2="12"></line><line x1="3" y1="6" x2="21" y2="6"></line><line x1="3" y1="18" x2="21" y2="18"></line>';
    });
}

// ── Init ──────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded',()=>{
    initMobileMenu();
    initScrollAnimations();

    // Restaurar sessão
    const savedUser = sessionStorage.getItem('lhub_user');
    const savedInsc = sessionStorage.getItem('lhub_inscricao');
    if (savedUser) {
        try { showDashboard(JSON.parse(savedUser), JSON.parse(savedInsc||'{}')); }
        catch { showLogin(); }
    } else { showLogin(); }

    // Eventos
    document.getElementById('loginBtn').addEventListener('click', doLogin);
    document.getElementById('inscricaoNum').addEventListener('keypress', e=>{ if(e.key==='Enter') doLogin(); });
    document.getElementById('loginSenha').addEventListener('keypress',   e=>{ if(e.key==='Enter') doLogin(); });
    document.getElementById('logoutBtn')?.addEventListener('click', doLogout);
    document.getElementById('openPasswordModal')?.addEventListener('click', openPasswordModal);
    document.getElementById('closePasswordModal').addEventListener('click', closePasswordModal);
    document.getElementById('saveSenhaBtn').addEventListener('click', saveSenha);
    document.getElementById('passwordModal').addEventListener('click', e=>{ if(e.target===e.currentTarget) closePasswordModal(); });

    document.getElementById('forgotLink')?.addEventListener('click', e=>{
        e.preventDefault();
        alert('Para recuperar sua senha, entre em contato:\ncontato@lhub.com.br\n(Em produção envia e-mail automático)');
    });

    // Toggle senha
    document.getElementById('toggleSenha')?.addEventListener('click',()=>{
        const inp  = document.getElementById('loginSenha');
        const icon = document.getElementById('eyeIcon');
        if (inp.type==='password') {
            inp.type='text';
            icon.innerHTML='<path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path><line x1="1" y1="1" x2="23" y2="23"></line>';
        } else {
            inp.type='password';
            icon.innerHTML='<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle>';
        }
    });
});