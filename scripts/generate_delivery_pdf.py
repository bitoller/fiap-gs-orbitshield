from __future__ import annotations

from pathlib import Path
import random

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import cm
from reportlab.platypus import (
    BaseDocTemplate,
    Frame,
    NextPageTemplate,
    PageBreak,
    PageTemplate,
    Paragraph,
    Spacer,
    Table,
    TableStyle,
)


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "orbit-shield-final-report.pdf"

DEEP_SPACE = colors.HexColor("#0B0E14")
PANEL = colors.HexColor("#10131A")
CYAN = colors.HexColor("#00AFC4")
CYAN_LIGHT = colors.HexColor("#00F0FF")
AMBER = colors.HexColor("#FFB800")
RED = colors.HexColor("#FF3B30")
INK = colors.HexColor("#1C2430")
MUTED = colors.HexColor("#5E6B78")
LINE = colors.HexColor("#D7DEE8")
SOFT = colors.HexColor("#F3F7FA")


def load_integrantes() -> str:
    text = (ROOT / "docs" / "entrega.txt").read_text(encoding="utf-8").strip()
    lines = [line.strip() for line in text.splitlines() if line.strip() and not line.lower().startswith("vídeo")]
    return "<br/>".join(lines)


def make_styles():
    base = getSampleStyleSheet()
    return {
        "Title": ParagraphStyle(
            "Title",
            parent=base["Title"],
            fontName="Helvetica-Bold",
            fontSize=30,
            leading=34,
            textColor=colors.white,
            alignment=TA_CENTER,
            spaceAfter=12,
        ),
        "Subtitle": ParagraphStyle(
            "Subtitle",
            parent=base["Normal"],
            fontName="Helvetica",
            fontSize=12,
            leading=16,
            textColor=colors.HexColor("#D7F9FF"),
            alignment=TA_CENTER,
        ),
        "H1": ParagraphStyle(
            "H1",
            parent=base["Heading1"],
            fontName="Helvetica-Bold",
            fontSize=17,
            leading=22,
            textColor=DEEP_SPACE,
            spaceBefore=16,
            spaceAfter=8,
        ),
        "H2": ParagraphStyle(
            "H2",
            parent=base["Heading2"],
            fontName="Helvetica-Bold",
            fontSize=12,
            leading=15,
            textColor=CYAN,
            spaceBefore=10,
            spaceAfter=5,
        ),
        "Body": ParagraphStyle(
            "Body",
            parent=base["BodyText"],
            fontName="Helvetica",
            fontSize=9.4,
            leading=13.2,
            textColor=INK,
            spaceAfter=5,
        ),
        "Small": ParagraphStyle(
            "Small",
            parent=base["BodyText"],
            fontName="Helvetica",
            fontSize=8,
            leading=10.5,
            textColor=MUTED,
        ),
        "Bullet": ParagraphStyle(
            "Bullet",
            parent=base["BodyText"],
            fontName="Helvetica",
            fontSize=9.2,
            leading=12.5,
            leftIndent=12,
            firstLineIndent=-7,
            textColor=INK,
            spaceAfter=3,
        ),
        "Code": ParagraphStyle(
            "Code",
            parent=base["BodyText"],
            fontName="Courier",
            fontSize=8,
            leading=10.5,
            textColor=colors.HexColor("#13202B"),
            backColor=colors.HexColor("#EEF6F8"),
            borderPadding=5,
            spaceBefore=3,
            spaceAfter=6,
        ),
        "Callout": ParagraphStyle(
            "Callout",
            parent=base["BodyText"],
            fontName="Helvetica-Bold",
            fontSize=9.2,
            leading=12.5,
            textColor=DEEP_SPACE,
            backColor=colors.HexColor("#E8FBFF"),
            borderColor=CYAN,
            borderWidth=0.8,
            borderPadding=7,
            spaceBefore=5,
            spaceAfter=8,
        ),
        "Cell": ParagraphStyle(
            "Cell",
            parent=base["BodyText"],
            fontName="Helvetica",
            fontSize=7.4,
            leading=9.2,
            textColor=INK,
        ),
        "HeaderCell": ParagraphStyle(
            "HeaderCell",
            parent=base["BodyText"],
            fontName="Helvetica-Bold",
            fontSize=7.3,
            leading=9,
            textColor=colors.white,
        ),
        "CoverSmall": ParagraphStyle(
            "CoverSmall",
            parent=base["Normal"],
            fontName="Helvetica-Bold",
            fontSize=9,
            leading=12,
            textColor=CYAN_LIGHT,
            alignment=TA_CENTER,
        ),
    }


S = make_styles()


def p(text: str, style: str = "Body"):
    return Paragraph(text, S[style])


def bullets(items: list[str]):
    return [p(f"- {item}", "Bullet") for item in items]


def section(title: str):
    return [p(title, "H1")]


def h2(title: str):
    return [p(title, "H2")]


def table(data, widths=None, header=True):
    wrapped = []
    for row_index, row in enumerate(data):
        wrapped_row = []
        for cell in row:
            style = "HeaderCell" if header and row_index == 0 else "Cell"
            wrapped_row.append(Paragraph(str(cell), S[style]))
        wrapped.append(wrapped_row)
    t = Table(wrapped, colWidths=widths, hAlign="LEFT", repeatRows=1 if header else 0)
    style = [
        ("FONT", (0, 0), (-1, -1), "Helvetica"),
        ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
        ("GRID", (0, 0), (-1, -1), 0.35, LINE),
        ("LEFTPADDING", (0, 0), (-1, -1), 5),
        ("RIGHTPADDING", (0, 0), (-1, -1), 5),
        ("TOPPADDING", (0, 0), (-1, -1), 5),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 5),
    ]
    if header:
        style += [
            ("BACKGROUND", (0, 0), (-1, 0), DEEP_SPACE),
        ]
    for row in range(1 if header else 0, len(data)):
        if row % 2 == 0:
            style.append(("BACKGROUND", (0, row), (-1, row), SOFT))
    t.setStyle(TableStyle(style))
    return t


def cover(canvas, doc):
    width, height = A4
    canvas.saveState()
    canvas.setFillColor(DEEP_SPACE)
    canvas.rect(0, 0, width, height, fill=1, stroke=0)
    random.seed(42)
    for _ in range(130):
        x = random.uniform(0.5 * cm, width - 0.5 * cm)
        y = random.uniform(2 * cm, height - 1 * cm)
        r = random.choice([0.35, 0.45, 0.65])
        canvas.setFillColor(colors.Color(0.75, 0.95, 1, alpha=random.uniform(0.25, 0.75)))
        canvas.circle(x, y, r, fill=1, stroke=0)
    canvas.setStrokeColor(CYAN_LIGHT)
    canvas.setLineWidth(1.2)
    canvas.circle(width / 2, height / 2 + 2 * cm, 5.4 * cm, stroke=1, fill=0)
    canvas.setStrokeColor(colors.Color(0, 0.94, 1, alpha=0.28))
    canvas.ellipse(width / 2 - 7.4 * cm, height / 2 + 0.4 * cm, width / 2 + 7.4 * cm, height / 2 + 4.2 * cm, stroke=1, fill=0)
    canvas.setFillColor(CYAN_LIGHT)
    canvas.circle(width / 2 + 4.5 * cm, height / 2 + 3.6 * cm, 0.11 * cm, fill=1, stroke=0)
    canvas.setFillColor(AMBER)
    canvas.circle(width / 2 - 4.2 * cm, height / 2 + 0.5 * cm, 0.07 * cm, fill=1, stroke=0)
    canvas.restoreState()


def body_page(canvas, doc):
    width, height = A4
    canvas.saveState()
    canvas.setFillColor(DEEP_SPACE)
    canvas.rect(0, height - 1.05 * cm, width, 1.05 * cm, fill=1, stroke=0)
    canvas.setFillColor(CYAN_LIGHT)
    canvas.setFont("Helvetica-Bold", 8)
    canvas.drawString(1.6 * cm, height - 0.68 * cm, "ORBIT SHIELD | RELATÓRIO FINAL")
    canvas.setFillColor(colors.white)
    canvas.setFont("Helvetica", 8)
    canvas.drawRightString(width - 1.6 * cm, height - 0.68 * cm, f"Página {doc.page}")
    canvas.setStrokeColor(CYAN)
    canvas.setLineWidth(0.6)
    canvas.line(1.6 * cm, 1.15 * cm, width - 1.6 * cm, 1.15 * cm)
    canvas.setFillColor(MUTED)
    canvas.setFont("Helvetica", 7.5)
    canvas.drawString(1.6 * cm, 0.75 * cm, "MVP acadêmico - Backend, Oracle, Android, Segurança e IoT")
    canvas.restoreState()


def build_story() -> list:
    integrantes = load_integrantes()
    story = []
    story.append(Spacer(1, 6.2 * cm))
    story.append(p("ORBIT SHIELD", "Title"))
    story.append(p("Gestão de Tráfego Orbital Autônomo", "Subtitle"))
    story.append(Spacer(1, 0.35 * cm))
    story.append(p("Relatório final de entregas não-código", "CoverSmall"))
    story.append(Spacer(1, 0.85 * cm))
    story.append(p(integrantes, "Subtitle"))
    story.append(Spacer(1, 6 * cm))
    story.append(p("FIAP | Global Solution | Indústria Espacial", "CoverSmall"))
    story.append(NextPageTemplate("body"))
    story.append(PageBreak())

    story += section("Resumo Executivo")
    story.append(p("Orbit Shield é um MVP acadêmico de gestão autônoma de tráfego orbital. A solução conecta backend, banco Oracle, aplicativo Android nativo e simulação IoT no Wokwi para provar a ideia de um satélite que reduz atraso de reação ao calcular risco localmente."))
    story.append(p("O backend atua como Mission Control: persiste dados, autentica usuários, expõe API RESTful, busca TLE público, propaga órbita com SGP4 e injeta cenários de detritos. O ESP32 representa o computador embarcado do satélite, calcula aproximação localmente e executa manobra visual. O app Android é o painel do engenheiro.", "Callout"))
    story.append(table([
        ["Área", "Entrega não-código", "Arquivo/Evidência"],
        ["Banco", "Diagrama ER, modelo relacional, SQL e consultas", "docs/database-er-diagram.md; database/oracle/*.sql"],
        ["API", "Swagger, endpoints e arquitetura em camadas", "docs/api-endpoints.md; README.md"],
        ["Testes", "Plano, cenários executados e logs", "docs/backend-test-plan.md; docs/iot-test-plan.md"],
        ["Mobile", "Descrição de telas, arquitetura e integração", "docs/android-setup.md"],
        ["Segurança", "Login, BCrypt, JWT, perfis e práticas", "README.md; docs/api-endpoints.md"],
        ["IoT", "Lógica dos sensores, circuito e autonomia", "docs/orbital-autonomy.md; docs/iot-test-plan.md"],
    ], [2.4 * cm, 6.2 * cm, 7.1 * cm]))

    story += section("Arquitetura Distribuída")
    story.append(p("A arquitetura foi separada em três partes integradas: Mission Control, Satélite IoT e Painel do Engenheiro. Essa separação demonstra SOA/REST, persistência, segurança, mobile e dispositivos simulados em uma prova de conceito colaborativa."))
    story.append(table([
        ["Componente", "Responsabilidade", "Tecnologia"],
        ["Backend", "API REST, autenticação, regras de missão, persistência, geração de cenários orbitais", ".NET 8, C#, EF Core, Oracle, Swagger"],
        ["Banco", "Integridade relacional, histórico de sensores, usuários, TLEs e manobras", "Oracle, tabelas OS_*"],
        ["IoT", "Computador embarcado simulado, sensores, LCD, LED e servo", "ESP32 C++ no Wokwi"],
        ["Mobile", "Painel nativo do engenheiro com telemetria, alertas e logs", "Kotlin, Jetpack Compose, Retrofit, StateFlow"],
    ], [2.5 * cm, 8.0 * cm, 5.2 * cm]))

    story += section("1. Banco de Dados")
    story += h2("Modelo e integridade")
    story.append(p("O modelo relacional usa prefixo OS_ e contém mais que as 4 entidades mínimas exigidas. As tabelas possuem chaves primárias, estrangeiras, constraints de domínio, unicidade e validações numéricas."))
    story.append(table([
        ["Tabela", "Finalidade"],
        ["OS_USERS", "Usuários, hash de senha e perfil de acesso."],
        ["OS_SATELLITES", "Satélites monitorados e telemetria base."],
        ["OS_DEBRIS_OBJECTS", "Objetos de detrito orbital."],
        ["OS_ORBITAL_ELEMENTS", "TLEs e elementos orbitais."],
        ["OS_CONJUNCTION_EVENTS", "Eventos de aproximação e risco."],
        ["OS_MANEUVER_LOGS", "Manobras executadas pelo ESP32."],
        ["OS_SENSOR_READINGS", "Leituras de sensores simulados."],
    ], [4.1 * cm, 11.6 * cm]))
    story += bullets([
        "Diagrama ER: docs/database-er-diagram.md",
        "DDL Oracle: database/oracle/01_create_tables_current_user.sql",
        "Consultas SQL: database/oracle/02_sample_queries.sql",
    ])

    story += section("2. API Backend")
    story.append(p("A API foi implementada em C#/.NET 8 com organização em camadas. Os controllers chamam serviços de aplicação, que acessam repositórios e persistem dados em Oracle."))
    story.append(table([
        ["Camada", "Pasta", "Função"],
        ["Controller", "src/OrbitShield.Api", "Entrada HTTP, Swagger e autenticação."],
        ["Service", "src/OrbitShield.Application", "Regras de negócio, autenticação, cenários orbitais."],
        ["Repository", "src/OrbitShield.Infrastructure", "Persistência Oracle com EF Core."],
        ["Domain", "src/OrbitShield.Domain", "Entidades de domínio."],
    ], [3.0 * cm, 5.0 * cm, 7.7 * cm]))
    story += h2("Endpoints representativos")
    story += bullets([
        "POST /api/auth/register e POST /api/auth/login",
        "GET/POST/PUT/DELETE /api/satellites",
        "GET /api/satellite/telemetry",
        "POST /api/satellite/sensor-readings",
        "POST /api/satellite/maneuver",
        "GET /api/orbital-scenarios/satellites/1/environment",
        "POST /api/orbital-scenarios/satellites/1/throw-random-debris",
    ])

    story += section("3. Testes e Logs")
    story.append(p("O projeto inclui plano de testes com 7 cenários, superando o mínimo de 5. As evidências foram documentadas com respostas JSON, status HTTP e logs de integração."))
    story.append(table([
        ["Evidência", "Onde encontrar", "O que comprova"],
        ["Plano backend", "docs/backend-test-plan.md", "Telemetria, emergência, sensores, manobras, autenticação e JWT."],
        ["Evidência IoT", "docs/iot-test-plan.md", "ESP32 conecta, envia sensores, calcula risco e registra manobra."],
        ["Logs Docker", "docker logs orbitshield-api --tail 80", "Requisições, persistência e execução da API."],
        ["Logs túnel", "docker logs orbitshield-tunnel --tail 50", "URL pública usada por Wokwi/celular."],
        ["Logs Wokwi", "Serial Monitor", "Wi-Fi, POST sensor, GET environment e POST maneuver."],
        ["Build Android", "scripts/build-android.ps1", "BUILD SUCCESSFUL e APK gerado."],
    ], [3.1 * cm, 5.5 * cm, 7.1 * cm]))
    story.append(p("Logs são saídas de execução. Eles servem como evidência porque mostram o sistema funcionando: status 201 para sensores, status 200 para ambiente orbital, decisão autônoma do ESP32 e registro de manobra.", "Callout"))

    story += section("4. Mobile Android")
    story.append(p("O app nativo Android foi criado com Kotlin e Jetpack Compose. Ele segue MVVM/Clean Architecture e consome a API real via Retrofit, sem dados mockados na UI."))
    story.append(table([
        ["Tela", "Objetivo"],
        ["Login", "Autenticar engenheiro com JWT."],
        ["Dashboard", "Mostrar telemetria, risco orbital, sensores e status."],
        ["Maneuver Logs", "Exibir manobras gravadas no backend."],
        ["Global / Scenario Control", "Disparar debris aleatório e presets reais."],
        ["Settings", "Mostrar sessão, API ativa e saúde do sistema."],
    ], [4.0 * cm, 11.7 * cm]))
    story += bullets([
        "Projeto: android/OrbitShieldEngineer",
        "Guia: docs/android-setup.md",
        "Build validado com Gradle Wrapper.",
        "URL do emulador: http://10.0.2.2:5184",
    ])

    story += section("5. Segurança")
    story.append(p("A solução inclui autenticação, autorização e práticas de segurança compatíveis com um MVP acadêmico robusto."))
    story += bullets([
        "Senha com hash BCrypt, nunca salva em texto puro.",
        "JWT Bearer para autenticação.",
        "Perfis Admin, Engineer e SatelliteDevice.",
        "Endpoints administrativos protegidos por role.",
        "DTOs e validações de entrada.",
        "EF Core com consultas parametrizadas para reduzir SQL Injection.",
        "Segredos locais excluídos do Git por .gitignore e exemplos seguros em .env.example.",
    ])

    story += section("6. IoT e Autonomia Orbital")
    story.append(p("O ESP32 no Wokwi representa o computador embarcado de um satélite. O backend fornece ambiente orbital; o satélite simulado decide localmente se precisa manobrar."))
    story.append(table([
        ["Componente", "Papel na simulação"],
        ["ESP32", "Computador embarcado que executa a lógica autônoma."],
        ["DHT22", "Telemetria térmica enviada ao backend."],
        ["Potenciômetro", "Thrust/propulsão simulada para cálculo de manobra."],
        ["LCD I2C", "Estado visual da missão para a banca."],
        ["LED vermelho", "Alerta de risco."],
        ["Servo motor", "Atuador visual de manobra evasiva."],
    ], [4.1 * cm, 11.6 * cm]))
    story += h2("Cálculo embarcado")
    story.append(p("tca = -dot(r, v) / |v|²<br/>closest = r + v * tca<br/>missDistance = |closest|<br/>risk = missDistance <= safeDistance", "Code"))
    story.append(p("O debris aleatório varia diâmetro, massa estimada, seção transversal, densidade, distância de passagem, velocidade relativa e energia de impacto. A decisão visual fica estável durante o evento para não parecer que múltiplos objetos foram lançados, e volta para SAFE PASS quando o objeto passa.", "Callout"))

    story += section("7. Execução Para Demonstração")
    story.append(p("Comando principal:", "H2"))
    story.append(p("powershell -ExecutionPolicy Bypass -File .\\scripts\\start-demo.ps1", "Code"))
    story += bullets([
        "Sobe Oracle, backend e túnel público no Docker.",
        "Aguarda a API ficar pronta.",
        "Prepara usuário demo e reseta a missão para SAFE PASS.",
        "Mostra URL do Swagger, Android Emulator e Wokwi.",
    ])
    story.append(p("Credenciais Android:", "H2"))
    story.append(p("API URL: http://10.0.2.2:5184<br/>email: orbit.demo.2026@orbitshield.local<br/>password: OrbitShield123!", "Code"))
    story.append(p("Fluxo sugerido: iniciar Docker, abrir Swagger, abrir Android, confirmar SAFE PASS, abrir Wokwi, disparar Throw Random Debris, observar dashboard, LCD, LED, servo e logs de manobra.", "Callout"))

    story += section("Conclusão")
    story.append(p("O Orbit Shield atende aos requisitos do desafio: arquitetura distribuída, banco relacional, API RESTful, autenticação, controle de acesso, app mobile nativo, simulação IoT e evidências de teste. A prova de conceito demonstra autonomia orbital em um cenário acadêmico realista, usando ferramentas gratuitas e executáveis localmente."))
    return story


def main():
    doc = BaseDocTemplate(
        str(OUT),
        pagesize=A4,
        leftMargin=1.55 * cm,
        rightMargin=1.55 * cm,
        topMargin=1.45 * cm,
        bottomMargin=1.45 * cm,
        title="Orbit Shield - Relatório Final",
        author="Orbit Shield Team",
    )
    frame = Frame(doc.leftMargin, doc.bottomMargin, doc.width, doc.height, id="normal")
    doc.addPageTemplates([
        PageTemplate(id="cover", frames=frame, onPage=cover),
        PageTemplate(id="body", frames=frame, onPage=body_page),
    ])
    story = build_story()
    story.insert(1, Paragraph("", S["Body"]))
    story.append(Spacer(1, 0.1 * cm))
    doc.build(story)
    print(OUT)


if __name__ == "__main__":
    main()
