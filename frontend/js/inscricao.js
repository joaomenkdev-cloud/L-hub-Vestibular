// ========================================
// INSCRIÇÃO - JAVASCRIPT
// ========================================
// Em todos os arquivos .js
const API_URL = 'http://localhost:5000/api';

// Exemplo de chamada:
fetch(`${API_URL}/noticias`)
  .then(r => r.json())
  .then(data => console.log(data));
  
let currentStep = 1;
const totalSteps = 4;

document.addEventListener('DOMContentLoaded', function() {
    console.log('📝 Sistema de inscrição inicializado');
    
    // Inicializar máscaras de input
    initInputMasks();
    
    // Inicializar validações
    initValidations();
    
    // Buscar CEP automático
    initCEP();
    
    // Mostrar/ocultar campo de cotas
    initCotasToggle();
    
    // Submissão do formulário
    initFormSubmission();
});

// ========================================
// NAVEGAÇÃO ENTRE STEPS
// ========================================
function nextStep() {
    if (validateCurrentStep()) {
        if (currentStep < totalSteps) {
            // Desativar step atual
            document.querySelector(`.form-step[data-step="${currentStep}"]`).classList.remove('active');
            document.querySelector(`.progress-step[data-step="${currentStep}"]`).classList.remove('active');
            document.querySelector(`.progress-step[data-step="${currentStep}"]`).classList.add('completed');
            
            // Ativar próximo step
            currentStep++;
            document.querySelector(`.form-step[data-step="${currentStep}"]`).classList.add('active');
            document.querySelector(`.progress-step[data-step="${currentStep}"]`).classList.add('active');
            
            // Se for o último step, mostrar confirmação
            if (currentStep === 4) {
                showConfirmation();
            }
            
            // Scroll para o topo
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    }
}

function prevStep() {
    if (currentStep > 1) {
        // Desativar step atual
        document.querySelector(`.form-step[data-step="${currentStep}"]`).classList.remove('active');
        document.querySelector(`.progress-step[data-step="${currentStep}"]`).classList.remove('active');
        
        // Ativar step anterior
        currentStep--;
        document.querySelector(`.form-step[data-step="${currentStep}"]`).classList.add('active');
        document.querySelector(`.progress-step[data-step="${currentStep}"]`).classList.add('active');
        document.querySelector(`.progress-step[data-step="${currentStep}"]`).classList.remove('completed');
        
        // Scroll para o topo
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
}

// ========================================
// VALIDAÇÃO DE STEPS
// ========================================
function validateCurrentStep() {
    const currentStepElement = document.querySelector(`.form-step[data-step="${currentStep}"]`);
    const requiredInputs = currentStepElement.querySelectorAll('[required]');
    let isValid = true;
    
    requiredInputs.forEach(input => {
        if (!input.value.trim()) {
            isValid = false;
            input.style.borderColor = '#ef4444';
            
            // Remover o destaque após 2 segundos
            setTimeout(() => {
                input.style.borderColor = '';
            }, 2000);
        }
    });
    
    if (!isValid) {
        showToast('Por favor, preencha todos os campos obrigatórios', 'error');
    }
    
    return isValid;
}

// ========================================
// MÁSCARAS DE INPUT
// ========================================
function initInputMasks() {
    // CPF
    const cpfInput = document.getElementById('cpf');
    if (cpfInput) {
        cpfInput.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\D/g, '');
            value = value.replace(/(\d{3})(\d)/, '$1.$2');
            value = value.replace(/(\d{3})(\d)/, '$1.$2');
            value = value.replace(/(\d{3})(\d{1,2})$/, '$1-$2');
            e.target.value = value;
        });
    }
    
    // Telefone
    const telefoneInput = document.getElementById('telefone');
    if (telefoneInput) {
        telefoneInput.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\D/g, '');
            if (value.length <= 10) {
                value = value.replace(/(\d{2})(\d)/, '($1) $2');
                value = value.replace(/(\d{4})(\d)/, '$1-$2');
            } else {
                value = value.replace(/(\d{2})(\d)/, '($1) $2');
                value = value.replace(/(\d{5})(\d)/, '$1-$2');
            }
            e.target.value = value;
        });
    }
    
    // CEP
    const cepInput = document.getElementById('cep');
    if (cepInput) {
        cepInput.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\D/g, '');
            value = value.replace(/(\d{5})(\d)/, '$1-$2');
            e.target.value = value;
        });
    }
}

// ========================================
// VALIDAÇÕES
// ========================================
function initValidations() {
    // Validar CPF
    const cpfInput = document.getElementById('cpf');
    if (cpfInput) {
        cpfInput.addEventListener('blur', function() {
            const cpf = this.value.replace(/\D/g, '');
            if (cpf.length === 11) {
                if (!validarCPF(cpf)) {
                    showToast('CPF inválido', 'error');
                    this.style.borderColor = '#ef4444';
                } else {
                    this.style.borderColor = '#22c55e';
                }
            }
        });
    }
    
    // Validar e-mail
    const emailInput = document.getElementById('email');
    if (emailInput) {
        emailInput.addEventListener('blur', function() {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(this.value)) {
                showToast('E-mail inválido', 'error');
                this.style.borderColor = '#ef4444';
            } else {
                this.style.borderColor = '#22c55e';
            }
        });
    }
}

function validarCPF(cpf) {
    // Algoritmo de validação de CPF
    if (cpf.length !== 11 || /^(\d)\1+$/.test(cpf)) return false;
    
    let soma = 0;
    let resto;
    
    for (let i = 1; i <= 9; i++) {
        soma += parseInt(cpf.substring(i-1, i)) * (11 - i);
    }
    
    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(cpf.substring(9, 10))) return false;
    
    soma = 0;
    for (let i = 1; i <= 10; i++) {
        soma += parseInt(cpf.substring(i-1, i)) * (12 - i);
    }
    
    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(cpf.substring(10, 11))) return false;
    
    return true;
}

// ========================================
// BUSCAR CEP
// ========================================
function initCEP() {
    const cepInput = document.getElementById('cep');
    if (cepInput) {
        cepInput.addEventListener('blur', async function() {
            const cep = this.value.replace(/\D/g, '');
            
            if (cep.length === 8) {
                showToast('Buscando CEP...', 'info');
                
                try {
                    const response = await fetch(`https://viacep.com.br/ws/${cep}/json/`);
                    const data = await response.json();
                    
                    if (!data.erro) {
                        document.getElementById('rua').value = data.logradouro;
                        document.getElementById('bairro').value = data.bairro;
                        document.getElementById('cidade').value = data.localidade;
                        document.getElementById('estado').value = data.uf;
                        
                        showToast('CEP encontrado!', 'success');
                        document.getElementById('numero').focus();
                    } else {
                        showToast('CEP não encontrado', 'error');
                    }
                } catch (error) {
                    console.error('Erro ao buscar CEP:', error);
                    showToast('Erro ao buscar CEP', 'error');
                }
            }
        });
    }
}

// ========================================
// TOGGLE DE COTAS
// ========================================
function initCotasToggle() {
    const cotasCheckbox = document.getElementById('cotas');
    const tipoCotasDiv = document.getElementById('tipoCotas');
    
    if (cotasCheckbox && tipoCotasDiv) {
        cotasCheckbox.addEventListener('change', function() {
            if (this.checked) {
                tipoCotasDiv.style.display = 'block';
                document.getElementById('tipoCota').required = true;
            } else {
                tipoCotasDiv.style.display = 'none';
                document.getElementById('tipoCota').required = false;
                document.getElementById('tipoCota').value = '';
            }
        });
    }
}

// ========================================
// CONFIRMAÇÃO
// ========================================
function showConfirmation() {
    const form = document.getElementById('inscriptionForm');
    const formData = new FormData(form);
    const confirmationBox = document.getElementById('confirmationData');
    
    let html = '';
    
    // Dados Pessoais
    html += `
        <div class="confirmation-section">
            <h3>📋 Dados Pessoais</h3>
            <div class="confirmation-row">
                <span class="confirmation-label">Nome:</span>
                <span class="confirmation-value">${formData.get('nome')}</span>
            </div>
            <div class="confirmation-row">
                <span class="confirmation-label">CPF:</span>
                <span class="confirmation-value">${formData.get('cpf')}</span>
            </div>
            <div class="confirmation-row">
                <span class="confirmation-label">E-mail:</span>
                <span class="confirmation-value">${formData.get('email')}</span>
            </div>
            <div class="confirmation-row">
                <span class="confirmation-label">Telefone:</span>
                <span class="confirmation-value">${formData.get('telefone')}</span>
            </div>
        </div>
    `;
    
    // Endereço
    html += `
        <div class="confirmation-section">
            <h3>📍 Endereço</h3>
            <div class="confirmation-row">
                <span class="confirmation-label">CEP:</span>
                <span class="confirmation-value">${formData.get('cep')}</span>
            </div>
            <div class="confirmation-row">
                <span class="confirmation-label">Rua:</span>
                <span class="confirmation-value">${formData.get('rua')}, ${formData.get('numero')}</span>
            </div>
            <div class="confirmation-row">
                <span class="confirmation-label">Bairro:</span>
                <span class="confirmation-value">${formData.get('bairro')}</span>
            </div>
            <div class="confirmation-row">
                <span class="confirmation-label">Cidade/UF:</span>
                <span class="confirmation-value">${formData.get('cidade')} - ${formData.get('estado')}</span>
            </div>
        </div>
    `;
    
    // Curso
    html += `
        <div class="confirmation-section">
            <h3>🎓 Curso Escolhido</h3>
            <div class="confirmation-row">
                <span class="confirmation-label">Curso:</span>
                <span class="confirmation-value">${formData.get('curso')}</span>
            </div>
            <div class="confirmation-row">
                <span class="confirmation-label">Turno:</span>
                <span class="confirmation-value">${formData.get('turno')}</span>
            </div>
            <div class="confirmation-row">
                <span class="confirmation-label">Modalidade:</span>
                <span class="confirmation-value">${formData.get('modalidade')}</span>
            </div>
            ${formData.get('cotas') ? `
            <div class="confirmation-row">
                <span class="confirmation-label">Cota:</span>
                <span class="confirmation-value">${formData.get('tipoCota')}</span>
            </div>
            ` : ''}
        </div>
    `;
    
    confirmationBox.innerHTML = html;
}

// ========================================
// SUBMISSÃO DO FORMULÁRIO
// ========================================
function initFormSubmission() {
    const form = document.getElementById('inscriptionForm');
    
    form.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        // Validar termos
        const termosCheckbox = document.getElementById('termos');
        if (!termosCheckbox.checked) {
            showToast('Você precisa aceitar os termos de uso', 'error');
            return;
        }
        
        // Coletar dados do formulário
        const formData = new FormData(form);
        const data = Object.fromEntries(formData.entries());
        
        // Adicionar loading ao botão
        const submitBtn = document.querySelector('.btn-submit');
        submitBtn.classList.add('loading');
        submitBtn.textContent = 'Processando...';
        
        try {
            // Enviar para o backend
            const response = await fetch('http://localhost:5000/api/inscricao', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(data)
            });
            
            const result = await response.json();
            
            if (response.ok) {
                // Sucesso!
                showSuccessModal(result.numero_inscricao);
            } else {
                // Erro
                showToast(result.message || 'Erro ao processar inscrição', 'error');
            }
        } catch (error) {
            console.error('Erro ao enviar inscrição:', error);
            showToast('Erro ao conectar com o servidor', 'error');
        } finally {
            // Remover loading
            submitBtn.classList.remove('loading');
            submitBtn.innerHTML = `
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="20 6 9 17 4 12"></polyline>
                </svg>
                Finalizar Inscrição
            `;
        }
    });
}

// ========================================
// MODAL DE SUCESSO
// ========================================
function showSuccessModal(numeroInscricao) {
    const modal = document.getElementById('successModal');
    const numeroElement = document.getElementById('inscricaoNumero');
    
    numeroElement.textContent = numeroInscricao;
    modal.classList.add('show');
}

function closeModal() {
    const modal = document.getElementById('successModal');
    modal.classList.remove('show');
}

// ========================================
// TOAST NOTIFICATIONS
// ========================================
function showToast(message, type = 'info') {
    // Remover toast anterior se existir
    const existingToast = document.querySelector('.toast');
    if (existingToast) {
        existingToast.remove();
    }
    
    // Criar novo toast
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    
    // Adicionar estilos inline
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
    
    // Cores baseadas no tipo
    const colors = {
        success: '#22c55e',
        error: '#ef4444',
        warning: '#f59e0b',
        info: '#3b82f6'
    };
    
    toast.style.background = colors[type] || colors.info;
    
    // Adicionar ao DOM
    document.body.appendChild(toast);
    
    // Remover após 3 segundos
    setTimeout(() => {
        toast.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Adicionar animações de toast
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

console.log('✅ Sistema de inscrição pronto!');