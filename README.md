# 🎓 L-Hub Vestibular

Plataforma web completa para gestão de processo seletivo e simulados vestibulares. Inclui área do candidato, simulados interativos, importação de provas (ENEM, FUVEST, UNICAMP) via PDF, e painel administrativo — tudo rodando com back-end em **ASP.NET Core (.NET 10)** + **SQLite** e front-end estático em HTML/CSS/JS com integração opcional ao **Firebase**.

---

## 📋 Pré-requisitos

Antes de qualquer coisa, instale as seguintes ferramentas:

| Ferramenta | Versão mínima | Link |
|---|---|---|
| **.NET SDK** | 10.0 | https://dotnet.microsoft.com/download |
| **Node.js** | 18+ (opcional, para Firebase Emulator) | https://nodejs.org |
| **Firebase CLI** | Qualquer recente (opcional) | `npm install -g firebase-tools` |
| **Git** | Qualquer | https://git-scm.com |

Verifique as instalações:
```bash
dotnet --version   # deve mostrar 10.x.x
node --version     # deve mostrar 18.x ou superior (se for usar Firebase)
firebase --version # se for usar emulador
```

---

## ⚙️ Configuração das Variáveis de Ambiente (OBRIGATÓRIO)

O projeto usa **variáveis de ambiente** para não expor credenciais no código. Você precisa configurar duas coisas antes de rodar:

### 1. Firebase Config (Front-end)

Crie o arquivo `js/firebase-config.js` com base no template abaixo. Substitua com os dados do **seu** projeto Firebase:

```js
// js/firebase-config.js
import { initializeApp }                          from 'https://www.gstatic.com/firebasejs/10.12.2/firebase-app.js';
import { getAuth, connectAuthEmulator }           from 'https://www.gstatic.com/firebasejs/10.12.2/firebase-auth.js';
import { getFirestore, connectFirestoreEmulator } from 'https://www.gstatic.com/firebasejs/10.12.2/firebase-firestore.js';

const FIREBASE_CONFIG = {
  apiKey:            "SUA_API_KEY",
  authDomain:        "SEU_PROJECT_ID.firebaseapp.com",
  projectId:         "SEU_PROJECT_ID",
  storageBucket:     "SEU_PROJECT_ID.firebasestorage.app",
  messagingSenderId: "SEU_SENDER_ID",
  appId:             "SEU_APP_ID",
  measurementId:     "SEU_MEASUREMENT_ID"   // opcional
};

const app  = initializeApp(FIREBASE_CONFIG);
const auth = getAuth(app);
const db   = getFirestore(app);

// Conecta ao emulador local automaticamente em desenvolvimento
if (location.hostname === 'localhost' || location.hostname === '127.0.0.1') {
  connectAuthEmulator(auth, 'http://127.0.0.1:9199', { disableWarnings: true });
  connectFirestoreEmulator(db, '127.0.0.1', 8282);
}

export { app, auth, db };
```

> 💡 **Onde achar essas informações?** Acesse o [Firebase Console](https://console.firebase.google.com) → seu projeto → ⚙️ Configurações do projeto → "Seus apps" → SDK setup.

### 2. Firebase Project ID (`.firebaserc`)

Crie o arquivo `.firebaserc` na raiz do projeto:

```json
{
  "projects": {
    "default": "SEU_PROJECT_ID"
  }
}
```

---

## 🚀 Rodando o Projeto Localmente

### Passo 1 — Clone o repositório

```bash
git clone https://github.com/SEU_USUARIO/l-hub-vestibular.git
cd l-hub-vestibular
```

### Passo 2 — Configure os arquivos de credenciais

Copie os templates e preencha com seus dados:

```bash
# Copiar templates
cp js/firebase-config.example.js js/firebase-config.js
cp .firebaserc.example .firebaserc
```

Edite os arquivos copiados com as suas credenciais Firebase (veja seção acima).

### Passo 3 — Inicie o Back-end

```bash
cd back-end
dotnet run
```

O servidor sobe em `http://localhost:5000`.

Você verá algo assim no terminal:
```
╔══════════════════════════════════════════════╗
║       🎓  L-Hub API  v3.1.0                 ║
╠══════════════════════════════════════════════╣
║  URL:    http://localhost:5000               ║
║  BD:     SQLite (lhub.db)                    ║
║  Docs:   http://localhost:5000/swagger       ║
╚══════════════════════════════════════════════╝
```

### Passo 4 — (Opcional) Inicie o Firebase Emulator

Em outro terminal, a partir da raiz do projeto:

```bash
firebase emulators:start
```

Portas utilizadas pelo emulador:
- **Firestore:** `8282`
- **Auth:** `9199`
- **Emulator UI:** `4000`

### Passo 5 — Acesse no navegador

Abra: **http://localhost:5000**

O back-end serve o front-end estático automaticamente. A rota `/` redireciona para `/html/index.html`.

---

## 🗂️ Estrutura do Projeto

```
l-hub-vestibular/
├── back-end/
│   ├── program.cs              # Entry point da API ASP.NET Core
│   ├── Controllers.cs          # Endpoints REST (alunos, simulados, auth…)
│   ├── ImportacaoController.cs # Importação de provas via PDF/URL
│   ├── PdfImportService.cs     # Extração de texto de PDFs (PdfPig)
│   ├── Appdbcontext.cs         # Entity Framework Core + SQLite
│   ├── models.cs               # Modelos de dados
│   ├── Authservice.cs          # Autenticação com hash SHA-256 + tokens
│   ├── Inscricaoservice.cs     # Lógica de inscrição de candidatos
│   ├── Dbseeder.cs             # Seed de dados iniciais no banco
│   └── Lhubvestibular.CSPROJ   # Dependências .NET
├── html/                       # Páginas HTML do front-end
│   ├── index.html
│   ├── inscricao.html
│   ├── area-candidato.html
│   ├── simulados.html
│   ├── provas.html
│   └── ...
├── js/                         # Scripts JavaScript
│   ├── firebase-config.js      # ⚠️ NÃO versionar — usar template
│   ├── area-candidato.js
│   ├── inscricao.js
│   ├── provas.js
│   └── java.js
├── css/                        # Estilos
├── firebase.json               # Configuração do Firebase Hosting/Emulator
├── .firebaserc                 # ⚠️ NÃO versionar — usar template
├── firestore.rules             # Regras de segurança do Firestore
├── firestore.indexes.json      # Índices do Firestore
└── README.md
```

---

## 📡 Endpoints principais da API

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/swagger` | Documentação interativa da API |
| `POST` | `/api/auth/login` | Login do candidato |
| `POST` | `/api/auth/logout` | Logout |
| `POST` | `/api/auth/trocar-senha` | Troca de senha |
| `GET` | `/api/candidatos` | Listar candidatos (admin) |
| `POST` | `/api/candidatos` | Cadastrar candidato |
| `GET` | `/api/simulados` | Listar simulados |
| `POST` | `/api/simulados/{id}/responder` | Enviar respostas de simulado |
| `POST` | `/api/admin/importacao/todas` | Importar todas as provas pré-configuradas |
| `POST` | `/api/admin/importacao/url` | Importar prova por URL de PDF |

A documentação completa com todos os parâmetros está disponível em `/swagger` quando o servidor está rodando.

---

## 📥 Importando Provas (ENEM, FUVEST, UNICAMP)

A plataforma suporta importação automática de questões via PDF.

**Importar um PDF por URL:**
```bash
curl -X POST http://localhost:5000/api/admin/importacao/url \
  -H "Content-Type: application/json" \
  -d '{"url": "https://exemplo.com/prova.pdf", "tipo": "ENEM", "ano": 2023}'
```

**Importar todas as provas pré-configuradas:**
```bash
curl -X POST http://localhost:5000/api/admin/importacao/todas
```

---

## 🔐 Segurança e Credenciais

**Nunca suba para o GitHub:**
- `js/firebase-config.js` (contém API Key do Firebase)
- `.firebaserc` (contém seu Project ID)
- `back-end/lhub.db` (banco de dados com dados reais)
- `back-end/lhub.db-shm`
- `back-end/lhub.db-wal`

Todos esses arquivos estão listados no `.gitignore` deste repositório. Use os arquivos `.example` como templates.

---

## 🧪 Banco de Dados

O projeto usa **SQLite** — nenhuma instalação de banco de dados é necessária. O arquivo `lhub.db` é criado automaticamente na primeira execução dentro da pasta `back-end/`.

O seed inicial (`Dbseeder.cs`) popula o banco com dados de exemplo para testes.

Para resetar o banco, basta apagar o arquivo `lhub.db` e reiniciar o servidor.

---

## 📦 Dependências do Back-end

Gerenciadas automaticamente pelo `dotnet restore`:

- **Microsoft.EntityFrameworkCore.Sqlite** `9.0.0` — ORM + SQLite
- **Swashbuckle.AspNetCore** `6.5.0` — Swagger/OpenAPI
- **PdfPig** `0.1.9` — Extração de texto de PDFs

---

## 🌐 Deploy

### Firebase Hosting (Front-end)

```bash
firebase login
firebase deploy --only hosting
```

### Back-end em produção

O back-end pode ser publicado em qualquer servidor com .NET 10. Exemplo com `dotnet publish`:

```bash
cd back-end
dotnet publish -c Release -o ./publish
```

Em produção, configure a variável de ambiente `ASPNETCORE_ENVIRONMENT=Production` para desativar o Swagger.

---

## 🤝 Contribuindo

1. Fork o repositório
2. Crie uma branch: `git checkout -b feature/minha-feature`
3. Faça commit das mudanças: `git commit -m 'feat: minha feature'`
4. Push: `git push origin feature/minha-feature`
5. Abra um Pull Request

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para detalhes.
