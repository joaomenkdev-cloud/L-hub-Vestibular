-- ============================================================
-- L-HUB VESTIBULAR — SCHEMA SQL COMPLETO (VERSÃO MYSQL)
-- ============================================================
CREATE DATABASE lhub CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- USUÁRIOS
-- ============================================================
CREATE TABLE IF NOT EXISTS usuarios (
    id                INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    numero_inscricao  VARCHAR(50) UNIQUE NOT NULL,
    email             VARCHAR(150) UNIQUE NOT NULL,
    nome              VARCHAR(255) NOT NULL,
    senha_hash        VARCHAR(255) NOT NULL,
    primeiro_acesso   TINYINT(1) DEFAULT 1,
    ultimo_acesso     DATETIME,
    criado_em         DATETIME DEFAULT CURRENT_TIMESTAMP,
    ativo             TINYINT(1) DEFAULT 1
);

-- ============================================================
-- INSCRIÇÕES
-- ============================================================
CREATE TABLE IF NOT EXISTS inscricoes (
    id                INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    numero_inscricao  VARCHAR(50) UNIQUE NOT NULL,
    usuario_id        INT,
    nome              VARCHAR(255) NOT NULL,
    cpf               VARCHAR(14) UNIQUE NOT NULL,
    rg                VARCHAR(20),
    data_nascimento   DATE,
    sexo              VARCHAR(20),
    email             VARCHAR(150) NOT NULL,
    telefone          VARCHAR(20) NOT NULL,
    nome_mae          VARCHAR(255),
    nome_pai          VARCHAR(255),
    cep               VARCHAR(10),
    rua               VARCHAR(255),
    numero_end        VARCHAR(10),
    complemento       VARCHAR(100),
    bairro            VARCHAR(100),
    cidade            VARCHAR(100),
    estado            CHAR(2),
    curso             VARCHAR(100) NOT NULL,
    turno             VARCHAR(50),
    modalidade        VARCHAR(50),
    cotas             TINYINT(1) DEFAULT 0,
    tipo_cota         VARCHAR(100),
    status            VARCHAR(50) DEFAULT 'aguardando_pagamento',
    valor             DECIMAL(10,2) DEFAULT 85.00,
    data_inscricao    DATETIME DEFAULT CURRENT_TIMESTAMP,
    atualizado_em     DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);

-- ============================================================
-- PAGAMENTOS
-- ============================================================
CREATE TABLE IF NOT EXISTS pagamentos (
    id                INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    inscricao_id      INT NOT NULL,
    numero_inscricao  VARCHAR(50) NOT NULL,
    status            VARCHAR(50) DEFAULT 'pendente',
    vencimento        DATE NOT NULL,
    data_pagamento    DATETIME,
    valor             DECIMAL(10,2) DEFAULT 85.00,
    codigo_barras     VARCHAR(100) DEFAULT '34191.79001 01043.510047 91020.150008 1 89370000008500',
    linha_digitavel   VARCHAR(100) DEFAULT '34191790010104351004791020150008189370000008500',
    criado_em         DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (inscricao_id) REFERENCES inscricoes(id)
);

-- ============================================================
-- TOKENS DE SESSÃO
-- ============================================================
CREATE TABLE IF NOT EXISTS tokens (
    id                INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    token             VARCHAR(255) UNIQUE NOT NULL,
    usuario_id        INT NOT NULL,
    numero_inscricao  VARCHAR(50) NOT NULL,
    criado_em         DATETIME DEFAULT CURRENT_TIMESTAMP,
    expira_em         DATETIME,
    ativo             TINYINT(1) DEFAULT 1,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);

-- ============================================================
-- CRONOGRAMA
-- ============================================================
CREATE TABLE IF NOT EXISTS cronograma (
    id           INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    titulo       VARCHAR(255) NOT NULL,
    descricao    TEXT,
    data_inicio  DATE NOT NULL,
    data_fim     DATE NOT NULL,
    ordem        INT DEFAULT 0,
    ativo        TINYINT(1) DEFAULT 1
);

-- ============================================================
-- NOTÍCIAS E EDITAIS
-- ============================================================
CREATE TABLE IF NOT EXISTS noticias (
    id             INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    titulo         VARCHAR(255) NOT NULL,
    slug           VARCHAR(255) UNIQUE,
    corpo          TEXT NOT NULL,
    resumo         TEXT,
    categoria      VARCHAR(50) DEFAULT 'noticia',
    badge_tipo     VARCHAR(50) DEFAULT 'novo',
    publicado      TINYINT(1) DEFAULT 1,
    destaque       TINYINT(1) DEFAULT 0,
    arquivo_url    VARCHAR(255),
    arquivo_nome   VARCHAR(100),
    autor          VARCHAR(100) DEFAULT 'Comissão do Vestibular',
    views          INT DEFAULT 0,
    publicado_em   DATETIME DEFAULT CURRENT_TIMESTAMP,
    atualizado_em  DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ============================================================
-- PROVAS ANTERIORES
-- ============================================================
CREATE TABLE IF NOT EXISTS provas (
    id             INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    titulo         VARCHAR(255) NOT NULL,
    ano            INT NOT NULL,
    periodo        VARCHAR(50) NOT NULL,
    instituicao    VARCHAR(100) DEFAULT 'L-Hub',
    area           VARCHAR(100) DEFAULT 'geral',
    questoes       INT DEFAULT 60,
    duracao_horas  INT DEFAULT 4,
    prova_url      VARCHAR(255),
    gabarito_url   VARCHAR(255),
    fonte          VARCHAR(50),
    fonte_nome     VARCHAR(150),
    downloads      INT DEFAULT 0,
    ativo          TINYINT(1) DEFAULT 1,
    criado_em      DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ============================================================
-- QUESTÕES DOS VESTIBULARES (PARA SIMULADOS)
-- ============================================================
CREATE TABLE IF NOT EXISTS questoes (
    id             INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    prova_id       INT NOT NULL,
    numero_questao INT NOT NULL,
    materia        VARCHAR(50) NOT NULL COMMENT 'Matemática, Português, Física, Química, Biologia, História, Geografia, Inglês',
    enunciado      TEXT NOT NULL,
    alternativa_a  TEXT,
    alternativa_b  TEXT,
    alternativa_c  TEXT,
    alternativa_d  TEXT,
    alternativa_e  TEXT,
    resposta_correta CHAR(1) NOT NULL COMMENT 'A, B, C, D ou E',
    dificuldade    VARCHAR(20) DEFAULT 'medio' COMMENT 'facil, medio, dificil',
    ano            INT NOT NULL,
    instituicao    VARCHAR(100),
    ativo          TINYINT(1) DEFAULT 1,
    criado_em      DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (prova_id) REFERENCES provas(id),
    INDEX idx_materia (materia),
    INDEX idx_dificuldade (dificuldade),
    INDEX idx_ano (ano)
);

-- ============================================================
-- SIMULADOS REALIZADOS (HISTÓRICO)
-- ============================================================
CREATE TABLE IF NOT EXISTS simulados_realizados (
    id                INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    usuario_id        INT NOT NULL,
    materia           VARCHAR(50) NOT NULL,
    total_questoes    INT NOT NULL,
    acertos           INT DEFAULT 0,
    nota              DECIMAL(5,2),
    tempo_gasto_min   INT,
    finalizado        TINYINT(1) DEFAULT 0,
    iniciado_em       DATETIME DEFAULT CURRENT_TIMESTAMP,
    finalizado_em     DATETIME,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
    INDEX idx_usuario (usuario_id),
    INDEX idx_materia (materia)
);

-- ============================================================
-- RESPOSTAS DO SIMULADO (DETALHAMENTO)
-- ============================================================
CREATE TABLE IF NOT EXISTS simulado_respostas (
    id              INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    simulado_id     INT NOT NULL,
    questao_id      INT NOT NULL,
    resposta_usuario CHAR(1),
    correta         TINYINT(1) DEFAULT 0,
    tempo_resposta_seg INT,
    FOREIGN KEY (simulado_id) REFERENCES simulados_realizados(id) ON DELETE CASCADE,
    FOREIGN KEY (questao_id) REFERENCES questoes(id)
);

-- ============================================================
-- LOCAIS DE PROVA E ATRIBUIÇÃO
-- ============================================================
CREATE TABLE IF NOT EXISTS locais_prova (
    id          INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    nome        VARCHAR(255) NOT NULL,
    endereco    VARCHAR(255),
    bairro      VARCHAR(100),
    cidade      VARCHAR(100),
    estado      CHAR(2),
    cep         VARCHAR(10),
    latitude    DECIMAL(10,8),
    longitude   DECIMAL(11,8),
    capacidade  INT,
    ativo       TINYINT(1) DEFAULT 1
);

CREATE TABLE IF NOT EXISTS candidato_local (
    id                INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    numero_inscricao  VARCHAR(50) NOT NULL,
    local_id          INT,
    sala              VARCHAR(50),
    numero_mesa       INT,
    liberado          TINYINT(1) DEFAULT 0,
    liberado_em       DATETIME,
    FOREIGN KEY (numero_inscricao) REFERENCES inscricoes(numero_inscricao),
    FOREIGN KEY (local_id) REFERENCES locais_prova(id)
);

-- ============================================================
-- RESULTADOS E COMUNICADOS
-- ============================================================
CREATE TABLE IF NOT EXISTS resultados (
    id                INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    numero_inscricao  VARCHAR(50) NOT NULL,
    nota_total        DECIMAL(5,2),
    nota_lp           DECIMAL(5,2),
    nota_ch           DECIMAL(5,2),
    nota_cn           DECIMAL(5,2),
    nota_mt           DECIMAL(5,2),
    nota_redacao      DECIMAL(5,2),
    classificacao     INT,
    status            VARCHAR(50) DEFAULT 'aguardando',
    publicado         TINYINT(1) DEFAULT 0,
    publicado_em      DATETIME,
    criado_em         DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (numero_inscricao) REFERENCES inscricoes(numero_inscricao)
);

CREATE TABLE IF NOT EXISTS comunicados_candidato (
    id                INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    numero_inscricao  VARCHAR(50),
    titulo            VARCHAR(255) NOT NULL,
    corpo             TEXT NOT NULL,
    lido              TINYINT(1) DEFAULT 0,
    criado_em         DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (numero_inscricao) REFERENCES inscricoes(numero_inscricao)
);

-- ============================================================
-- DADOS INICIAIS — CRONOGRAMA
-- ============================================================
INSERT IGNORE INTO cronograma (titulo, descricao, data_inicio, data_fim, ordem) VALUES
('Inscrições', 'Período de inscrições para o vestibular', '2026-02-01', '2026-03-15', 1),
('Pagamento', 'Prazo final para pagamento da taxa de inscrição', '2026-02-01', '2026-03-18', 2),
('Divulgação dos Locais', 'Divulgação dos locais e salas de prova', '2026-05-20', '2026-05-20', 3),
('Prova', 'Realização da prova do vestibular', '2026-06-15', '2026-06-15', 4),
('Resultado', 'Divulgação do resultado final', '2026-06-30', '2026-06-30', 5),
('Matrícula', 'Período de matrícula dos aprovados', '2026-07-01', '2026-07-15', 6);

-- ============================================================
-- DADOS EXEMPLO — QUESTÕES DE MATEMÁTICA
-- ============================================================
INSERT IGNORE INTO questoes (prova_id, numero_questao, materia, enunciado, alternativa_a, alternativa_b, alternativa_c, alternativa_d, alternativa_e, resposta_correta, dificuldade, ano, instituicao) VALUES
(1, 1, 'Matemática', 'Qual é o resultado de 2 + 2?', '3', '4', '5', '6', '7', 'B', 'facil', 2025, 'FUVEST'),
(1, 2, 'Matemática', 'Resolva a equação: x² - 5x + 6 = 0', 'x = 1 ou x = 6', 'x = 2 ou x = 3', 'x = -2 ou x = -3', 'x = 0 ou x = 5', 'x = 1 ou x = 5', 'B', 'medio', 2025, 'FUVEST'),
(1, 3, 'Matemática', 'Calcule o valor de log₂(32)', '3', '4', '5', '6', '7', 'C', 'medio', 2025, 'FUVEST'),
(2, 1, 'Português', 'Qual é a classe gramatical da palavra "rapidamente"?', 'Substantivo', 'Adjetivo', 'Advérbio', 'Verbo', 'Pronome', 'C', 'facil', 2024, 'FUVEST'),
(2, 2, 'Português', 'Identifique a figura de linguagem: "Suas palavras foram punhais no meu coração"', 'Metáfora', 'Metonímia', 'Hipérbole', 'Eufemismo', 'Ironia', 'A', 'medio', 2024, 'FUVEST'),
(3, 1, 'Física', 'Um corpo em queda livre no vácuo possui aceleração de aproximadamente:', '5 m/s²', '8 m/s²', '10 m/s²', '12 m/s²', '15 m/s²', 'C', 'facil', 2023, 'FUVEST'),
(3, 2, 'Física', 'A primeira lei de Newton é também conhecida como:', 'Lei da Gravitação Universal', 'Lei da Inércia', 'Lei da Ação e Reação', 'Lei de Ohm', 'Lei de Coulomb', 'B', 'medio', 2023, 'FUVEST'),
(4, 1, 'Química', 'Qual é o símbolo químico do ouro?', 'O', 'Au', 'Ag', 'Go', 'Or', 'B', 'facil', 2025, 'UNICAMP'),
(4, 2, 'Química', 'A fórmula química da água é:', 'H2O', 'HO2', 'H3O', 'O2H', 'OH2', 'A', 'facil', 2025, 'UNICAMP'),
(5, 1, 'Biologia', 'A fotossíntese ocorre principalmente em qual organela?', 'Mitocôndria', 'Núcleo', 'Cloroplasto', 'Ribossomo', 'Retículo', 'C', 'medio', 2024, 'UNICAMP'),
(6, 1, 'História', 'Em que ano foi proclamada a República no Brasil?', '1888', '1889', '1890', '1891', '1892', 'B', 'facil', 2024, 'ENEM'),
(7, 1, 'Geografia', 'Qual é o maior país em extensão territorial do mundo?', 'China', 'Canadá', 'EUA', 'Rússia', 'Brasil', 'D', 'facil', 2024, 'ENEM'),
(8, 1, 'Inglês', 'What is the past tense of "go"?', 'Goed', 'Went', 'Gone', 'Going', 'Goes', 'B', 'facil', 2023, 'ENEM');

-- ============================================================
-- DADOS INICIAIS — NOTÍCIAS E EDITAIS
-- ============================================================
INSERT IGNORE INTO noticias (id, titulo, slug, resumo, corpo, categoria, badge_tipo, destaque, publicado_em) VALUES
(1,
 'Inscrições Abertas para o Vestibular 2026',
 'inscricoes-abertas-vestibular-2026',
 'As inscrições para o processo seletivo 2026 estão abertas até 15 de março. Confira os cursos disponíveis e se inscreva!',
 '<p>O <strong>Vestibular L-Hub 2026</strong> está com inscrições abertas! O período de inscrições vai de <strong>01 de fevereiro</strong> até <strong>15 de março de 2026</strong>.</p><p>Estão disponíveis vagas nos seguintes cursos:</p><ul><li>Ciência da Computação (Matutino/Noturno)</li><li>Engenharia Civil (Integral)</li><li>Direito (Noturno)</li><li>Medicina (Integral)</li><li>Administração (Matutino/Noturno)</li><li>Psicologia (Vespertino/Noturno)</li></ul><p>A taxa de inscrição é de <strong>R$ 85,00</strong>, com vencimento do boleto em 18/03/2026.</p><p>Candidatos com renda familiar de até 1,5 salário mínimo per capita têm direito à isenção da taxa. Saiba mais no edital completo.</p><h3>Como se inscrever</h3><ol><li>Acesse a página de inscrição</li><li>Preencha o formulário com seus dados pessoais</li><li>Escolha o curso e turno desejados</li><li>Realize o pagamento do boleto até 18/03</li></ol>',
 'noticia', 'novo', 1, '2026-02-10 08:00:00'),

(2,
 'Edital Completo do Processo Seletivo 2026',
 'edital-completo-processo-seletivo-2026',
 'Confira todas as regras, critérios de avaliação e informações sobre o vestibular 2026.',
 '<p>A <strong>Comissão do Vestibular L-Hub</strong> divulga o <strong>Edital do Processo Seletivo 2026</strong>, com todas as regras e informações para os candidatos.</p><h3>1. Das Inscrições</h3><p>As inscrições serão realizadas exclusivamente pelo site oficial, no período de 01/02/2026 a 15/03/2026.</p><h3>2. Da Taxa de Inscrição</h3><p>Valor: R$ 85,00 (oitenta e cinco reais), paga via boleto bancário, com vencimento em 18/03/2026.</p><h3>3. Das Provas</h3><p>A prova será aplicada no dia <strong>15/06/2026</strong>, das 09h às 13h, em locais a serem divulgados em 20/05/2026.</p><p>A prova será composta por <strong>60 questões objetivas</strong> das seguintes áreas:</p><ul><li>Linguagens, Códigos e suas Tecnologias: 15 questões</li><li>Ciências Humanas: 15 questões</li><li>Ciências da Natureza: 15 questões</li><li>Matemática: 15 questões</li></ul><h3>4. Do Resultado</h3><p>O resultado será divulgado em 30/06/2026.</p><h3>5. Da Matrícula</h3><p>Candidatos aprovados deverão realizar matrícula entre 01/07/2026 e 15/07/2026, com apresentação de documentos originais.</p>',
 'edital', 'edital', 1, '2026-02-08 10:00:00'),

(3,
 'Prazo para Pagamento Encerra em 18/03',
 'prazo-pagamento-encerra-18-marco',
 'Atenção! O boleto da taxa de inscrição vence em 18/03/2026. Não perca o prazo para garantir sua participação.',
 '<p><strong>Atenção, candidatos!</strong></p><p>O prazo final para pagamento da taxa de inscrição é <strong>18 de março de 2026</strong>. Candidatos que não efetuarem o pagamento até esta data terão suas inscrições canceladas automaticamente.</p><h3>Como emitir a segunda via do boleto</h3><p>Acesse a Área do Candidato com seu número de inscrição e senha temporária para obter o código do boleto.</p><h3>Meios de pagamento aceitos</h3><ul><li>Boleto bancário (em qualquer banco ou lotérica)</li><li>Internet banking</li><li>Aplicativos bancários</li></ul><p><strong>Importante:</strong> O pagamento pode levar até 2 dias úteis para ser processado. Pague com antecedência!</p>',
 'comunicado', 'urgente', 0, '2026-02-12 09:00:00'),

(4,
 'Divulgação dos Locais de Prova em Maio',
 'divulgacao-locais-prova-maio',
 'Os locais e salas de prova serão divulgados a partir de 20 de maio de 2026. Confira como verificar seu local.',
 '<p>A divulgação dos locais de prova, salas e horários será realizada em <strong>20 de maio de 2026</strong>.</p><p>Cada candidato poderá consultar seu local de prova diretamente na Área do Candidato, após o pagamento ser confirmado.</p><h3>O que será informado</h3><ul><li>Nome e endereço completo do local de prova</li><li>Número do bloco e sala</li><li>Horário de abertura dos portões (08:00)</li><li>Horário de fechamento dos portões (08:45)</li><li>Link para mapa de localização</li></ul><h3>Documentos necessários no dia</h3><ul><li>Documento de identidade com foto (RG, CNH ou Passaporte)</li><li>Cartão de confirmação de inscrição (disponível na área do candidato)</li><li>Caneta esferográfica azul ou preta</li></ul>',
 'noticia', 'importante', 0, '2026-02-13 11:00:00');

-- ============================================================
-- DADOS INICIAIS — PROVAS
-- ============================================================
INSERT IGNORE INTO provas (id, titulo, ano, periodo, instituicao, area, questoes, duracao_horas, prova_url, gabarito_url, fonte, fonte_nome, downloads) VALUES
(1, 'FUVEST 2025 — Primeira Fase', 2025, '1ª Fase', 'FUVEST', 'geral', 90, 5, 'https://www.fuvest.br/wp-content/uploads/fuvest-2025-1a-fase-prova.pdf', 'https://www.fuvest.br/wp-content/uploads/fuvest-2025-1a-fase-gabarito.pdf', 'externo', 'FUVEST', 1240),
(2, 'FUVEST 2024 — Primeira Fase', 2024, '1ª Fase', 'FUVEST', 'geral', 90, 5, 'https://www.fuvest.br/wp-content/uploads/fuvest-2024-1a-fase-prova.pdf', 'https://www.fuvest.br/wp-content/uploads/fuvest-2024-1a-fase-gabarito.pdf', 'externo', 'FUVEST', 3850),
(3, 'FUVEST 2023 — Primeira Fase', 2023, '1ª Fase', 'FUVEST', 'geral', 90, 5, 'https://www.fuvest.br/wp-content/uploads/fuvest_2023_1a_fase_prova.pdf', 'https://www.fuvest.br/wp-content/uploads/fuvest_2023_1a_fase_gabarito.pdf', 'externo', 'FUVEST', 5100),
(4, 'UNICAMP 2025 — Primeira Fase', 2025, '1ª Fase', 'UNICAMP', 'geral', 72, 4, 'https://www.comvest.unicamp.br/wp-content/uploads/2024/11/1fase2025_prova.pdf', 'https://www.comvest.unicamp.br/wp-content/uploads/2024/11/1fase2025_gabarito.pdf', 'externo', 'COMVEST', 980),
(5, 'UNICAMP 2024 — Primeira Fase', 2024, '1ª Fase', 'UNICAMP', 'geral', 72, 4, 'https://www.comvest.unicamp.br/wp-content/uploads/2023/11/1fase_2024_prova.pdf', 'https://www.comvest.unicamp.br/wp-content/uploads/2023/11/1fase_2024_gabarito.pdf', 'externo', 'COMVEST', 2700),
(6, 'ENEM 2024 — Caderno Azul (Dia 1)', 2024, '1º Dia', 'ENEM/MEC', 'humanas_linguagens', 90, 5, 'https://download.inep.gov.br/enem/provas_e_gabaritos/2024_PV_impresso_D1_CD1.pdf', 'https://download.inep.gov.br/enem/provas_e_gabaritos/2024_GB_impresso_D1.pdf', 'externo', 'INEP', 8200),
(7, 'ENEM 2024 — Caderno Azul (Dia 2)', 2024, '2º Dia', 'ENEM/MEC', 'exatas_natureza', 90, 5, 'https://download.inep.gov.br/enem/provas_e_gabaritos/2024_PV_impresso_D2_CD1.pdf', 'https://download.inep.gov.br/enem/provas_e_gabaritos/2024_GB_impresso_D2.pdf', 'externo', 'INEP', 7900),
(8, 'ENEM 2023 — Caderno Azul (Dia 1)', 2023, '1º Dia', 'ENEM/MEC', 'humanas_linguagens', 90, 5, 'https://download.inep.gov.br/enem/provas_e_gabaritos/2023_PV_impresso_D1_CD1.pdf', 'https://download.inep.gov.br/enem/provas_e_gabaritos/2023_GB_impresso_D1.pdf', 'externo', 'INEP', 12300),
(9, 'ENEM 2023 — Caderno Azul (Dia 2)', 2023, '2º Dia', 'ENEM/MEC', 'exatas_natureza', 90, 5, 'https://download.inep.gov.br/enem/provas_e_gabaritos/2023_PV_impresso_D2_CD1.pdf', 'https://download.inep.gov.br/enem/provas_e_gabaritos/2023_GB_impresso_D2.pdf', 'externo', 'INEP', 11800),
(10, 'ENEM 2022 — Caderno Azul (Dia 1)', 2022, '1º Dia', 'ENEM/MEC', 'humanas_linguagens', 90, 5, 'https://download.inep.gov.br/enem/provas_e_gabaritos/2022_PV_impresso_D1_CD1.pdf', 'https://download.inep.gov.br/enem/provas_e_gabaritos/2022_GB_impresso_D1.pdf', 'externo', 'INEP', 15600);

-- ============================================================
-- DADOS INICIAIS — LOCAL DE PROVA
-- ============================================================
INSERT IGNORE INTO locais_prova (id, nome, endereco, bairro, cidade, estado, capacidade) VALUES
(1, 'L-Hub Campus Central', 'Av. Paulista, 1000', 'Bela Vista', 'São Paulo', 'SP', 500),
(2, 'L-Hub Campus Norte', 'Rua das Flores, 250', 'Santana', 'São Paulo', 'SP', 300),
(3, 'L-Hub Campus Sul', 'Av. Interlagos, 800', 'Santo Amaro', 'São Paulo', 'SP', 300),
(4, 'Colégio Parceiro Centro', 'Rua da Consolação, 150', 'Consolação', 'São Paulo', 'SP', 200);
