#!/usr/bin/env python3
"""
CleanCodeJN.GenericApis — demo GIF generator
Run: python create_demo_gif.py
Output: demo.gif
"""

from PIL import Image, ImageDraw, ImageFont
import re, os

# ── dimensions ────────────────────────────────────────────────────────────────
W, H = 900, 520

# ── colors ────────────────────────────────────────────────────────────────────
BG          = (30, 30, 30)
TITLE_BG    = (37, 37, 38)
TAB_ACTIVE  = (30, 30, 30)
TAB_IDLE    = (45, 45, 48)
STATUS_BG   = (0, 122, 204)
BORDER      = (60, 60, 60)
GUTTER_BG   = (35, 35, 35)

# syntax (VS Code Dark+)
KW  = (86,  156, 214)   # keyword      blue
STR = (206, 145, 120)   # string       orange
CMT = (106, 153,  85)   # comment      green
TYP = ( 78, 201, 176)   # type         teal
MTH = (220, 220, 170)   # method/prop  yellow
NUM = (181, 206, 168)   # number       lime
PLN = (212, 212, 212)   # plain        light-gray
DIM = (100, 100, 100)   # dim          dark-gray
LNO = ( 80,  80,  80)   # line-number

# terminal
TBG  = ( 12,  12,  12)
TPMT = ( 57, 181,  74)   # prompt  green
TCMD = (255, 255, 255)   # command white
TOUT = (178, 178, 178)   # output  gray
TLOG = ( 97, 214, 214)   # log     cyan
TOK  = ( 57, 181,  74)   # ok      green

# chat
CHAT_BG   = ( 25,  25,  35)
USER_BG   = (  0,  84, 166)
BOT_BG    = ( 50,  50,  60)
TOOL_BG   = ( 50,  38,   8)
TOOL_BD   = (200, 150,  30)
CHAT_PLN  = (230, 230, 230)
CHAT_DIM  = (120, 120, 150)
SIDE_BG   = ( 18,  18,  28)
SIDE_TXT  = ( 60,  80, 130)

# ── fonts ─────────────────────────────────────────────────────────────────────
FONT_PATH = 'C:/Windows/Fonts/consola.ttf'
BOLD_PATH = 'C:/Windows/Fonts/consolab.ttf'

def lf(size):
    return ImageFont.truetype(FONT_PATH, size)

CF  = lf(13)   # code
UF  = lf(14)   # UI
SMF = lf(11)   # small
HDF = lf(20)   # headline overlay

# ── geometry ──────────────────────────────────────────────────────────────────
CHROME_H = 36
TAB_H    = 28
STATUS_H = 22
LNO_W    = 46
CODE_X   = LNO_W + 10
TOP      = CHROME_H + TAB_H
BOT      = H - STATUS_H

# ── helpers ───────────────────────────────────────────────────────────────────
def tw(draw, text, font):
    return int(draw.textlength(text, font=font))

def lh(font):
    return font.getbbox('Mg')[3] + 4

def draw_spans(draw, x, y, spans, font):
    for text, color in spans:
        draw.text((x, y), text, fill=color, font=font)
        x += tw(draw, text, font)

def rr(draw, xy, r, fill, outline=None, width=1):
    draw.rounded_rectangle(xy, radius=r, fill=fill, outline=outline, width=width)

def wrap(draw, text, font, max_w):
    words = text.split()
    lines, cur = [], ''
    for w in words:
        test = (cur + ' ' + w).strip()
        if tw(draw, test, font) <= max_w:
            cur = test
        else:
            if cur: lines.append(cur)
            cur = w
    if cur: lines.append(cur)
    return lines or ['']

# ── syntax highlighter ────────────────────────────────────────────────────────
KEYWORDS = {
    'public','private','protected','internal','static','sealed','abstract',
    'override','virtual','new','class','interface','enum','struct','record',
    'using','namespace','return','void','var','int','string','bool','object',
    'List','Task','async','await','null','true','false','this','base','typeof',
    'get','set','init','readonly','const','where','if','else','for','foreach',
    'in','out','ref',
}

def hl(line):
    """Return list of (text, color) spans for a C# line."""
    stripped = line.lstrip()
    if stripped.startswith('//'):
        return [(line, CMT)]

    tokens = re.split(r'(\s+|"[^"]*"|\'[^\']*\'|[<>(){}\[\],.:;=\+\-\*/!@#&|?])', line)
    result = []
    for tok in tokens:
        if not tok:
            continue
        if tok.startswith('"') or tok.startswith("'"):
            result.append((tok, STR))
        elif tok.strip() == '':
            result.append((tok, PLN))
        elif tok in KEYWORDS:
            result.append((tok, KW))
        elif re.match(r'^[A-Z][A-Za-z0-9_]*(<.*>)?$', tok):
            result.append((tok, TYP))
        elif re.match(r'^\d+$', tok):
            result.append((tok, NUM))
        elif tok in '<>(){}[],.:;=+-*/!@#&|?':
            result.append((tok, PLN))
        else:
            result.append((tok, PLN))
    return result if result else [(line, PLN)]

# ── chrome ────────────────────────────────────────────────────────────────────
def editor_chrome(draw, filename, status_extra='C#  UTF-8'):
    # title bar
    draw.rectangle([0, 0, W, CHROME_H], fill=TITLE_BG)
    title = f'{filename}  —  CleanCodeJN.GenericApis Demo'
    draw.text((W//2 - tw(draw, title, SMF)//2, 10), title, fill=(160,160,160), font=SMF)
    for i, c in enumerate([(255,95,86),(255,189,46),(39,201,63)]):
        draw.ellipse([12+i*20, 11, 24+i*20, 23], fill=c)
    # tab
    draw.rectangle([0, CHROME_H, W, CHROME_H+TAB_H], fill=TAB_IDLE)
    tab_w = max(160, tw(draw, filename, SMF) + 36)
    draw.rectangle([0, CHROME_H, tab_w, CHROME_H+TAB_H], fill=TAB_ACTIVE)
    draw.line([0, CHROME_H+TAB_H-1, tab_w, CHROME_H+TAB_H-1], fill=STATUS_BG, width=2)
    draw.text((12, CHROME_H + 7), filename, fill=PLN, font=SMF)
    draw.text((tab_w-16, CHROME_H+7), 'x', fill=DIM, font=SMF)
    # status bar
    draw.rectangle([0, H-STATUS_H, W, H], fill=STATUS_BG)
    draw.text((10, H-STATUS_H+4), f'  {status_extra}', fill=(255,255,255), font=SMF)

def terminal_chrome(draw, title='Windows PowerShell'):
    draw.rectangle([0, 0, W, CHROME_H], fill=(16,16,16))
    draw.text((W//2 - tw(draw, title, SMF)//2, 10), title, fill=(160,160,160), font=SMF)
    for i, c in enumerate([(255,95,86),(255,189,46),(39,201,63)]):
        draw.ellipse([12+i*20, 11, 24+i*20, 23], fill=c)
    draw.rectangle([0, H-STATUS_H, W, H], fill=(16,16,16))
    draw.text((10, H-STATUS_H+4), '  Step 1 of 7: Setup', fill=DIM, font=SMF)

# ── scene renderers ───────────────────────────────────────────────────────────
def _draw_overlay(d, text, x_min=0, y_offset=0):
    """Floating marketing card, right-center of the frame."""
    if not text:
        return
    lines = text.split('\n')
    px, py = 22, 14
    max_w  = max(tw(d, l, HDF) for l in lines)
    bw     = max_w + px * 2
    bh     = len(lines) * lh(HDF) + py * 2
    bx     = max(x_min + 16, W - bw - 32)
    by     = H // 2 + 10 + y_offset
    # subtle drop-shadow
    rr(d, [bx + 3, by + 3, bx + bw + 3, by + bh + 3], 14, (2, 5, 12))
    # card body
    rr(d, [bx, by, bx + bw, by + bh], 14, (10, 18, 38), outline=(0, 195, 162), width=2)
    for i, line in enumerate(lines):
        d.text((bx + px, by + py + i * lh(HDF)), line, fill=(228, 242, 255), font=HDF)

def render_code(code_lines, n, filename, step_label, step_n, headline='', overlay_y=0):
    """
    code_lines: list of str (raw C# code)
    n: how many lines to reveal
    """
    img = Image.new('RGB', (W, H), BG)
    d = ImageDraw.Draw(img)
    editor_chrome(d, filename, f'C#  UTF-8   Step {step_n}/9: {step_label}')
    d.rectangle([0, TOP, W, BOT], fill=BG)
    d.rectangle([0, TOP, LNO_W, BOT], fill=GUTTER_BG)
    d.line([LNO_W, TOP, LNO_W, BOT], fill=BORDER)
    row_h = lh(CF)
    y0 = TOP + 8
    for i, raw in enumerate(code_lines[:n]):
        y = y0 + i * row_h
        if y + row_h > BOT - 4: break
        d.text((6, y), f'{i+1:3d}', fill=LNO, font=CF)
        draw_spans(d, CODE_X, y, hl(raw), CF)
    if headline:
        _draw_overlay(d, headline, y_offset=overlay_y)
    return img

def render_terminal(lines, n, step_label, step_n):
    """lines: list of (text, color)"""
    img = Image.new('RGB', (W, H), TBG)
    d = ImageDraw.Draw(img)
    draw.rectangle = d.rectangle
    terminal_chrome(d)
    # fix status
    d.rectangle([0, H-STATUS_H, W, H], fill=(16,16,16))
    d.text((10, H-STATUS_H+4), f'  Step {step_n}/9: {step_label}', fill=DIM, font=SMF)
    row_h = lh(UF)
    y = CHROME_H + 18
    for text, color in lines[:n]:
        if isinstance(text, list):  # multi-span line
            draw_spans(d, 22, y, text, UF)
        else:
            d.text((22, y), text, fill=color, font=UF)
        y += row_h
    return img

def render_chat(messages, step_label, step_n, headline=''):
    # colors matching real /ai page
    MAIN_BG     = (10,  21,  32)
    NAV_BG      = ( 8,  16,  26)
    HDR_BG      = (12,  22,  38)
    INP_BG      = (12,  26,  44)
    CHIP_TC     = ( 0, 178, 152)   # teal chip text/border
    CHIP_BG_C   = ( 8,  38,  46)   # chip fill
    MSG_USR     = ( 0,  82, 162)   # user bubble
    MSG_BOT     = (14,  38,  62)   # bot bubble
    TOOL_BD_C   = ( 0, 130, 115)
    TOOL_BG_C   = (10,  38,  45)

    SIDEBAR_W  = 192
    HEADER_H   = 38
    INPUT_H    = 52

    img = Image.new('RGB', (W, H), MAIN_BG)
    d   = ImageDraw.Draw(img)

    # ── header ─────────────────────────────────────────────────────────────
    d.rectangle([0, 0, W, HEADER_H], fill=HDR_BG)
    d.ellipse([10, 8, 30, 28], fill=(22, 148, 74))
    d.text((14, 13), 'CJ', fill=(255, 255, 255), font=SMF)
    d.text((38, 11), 'CleanCodeJN AI Assistant', fill=(210, 225, 240), font=UF)

    # ── sidebar ────────────────────────────────────────────────────────────
    d.rectangle([0, HEADER_H, SIDEBAR_W, H - INPUT_H], fill=NAV_BG)
    d.text((8, HEADER_H + 7), 'Available Tools', fill=(135, 165, 200), font=SMF)
    d.line([0, HEADER_H + 23, SIDEBAR_W, HEADER_H + 23], fill=(18, 40, 62))

    tools = [
        'get_api_v1_customers',
        'get_api_v1_customers_paged',
        'get_api_v1_customers_filtered',
        'get_api_v1_customers_id',
        'put_api_v1_customers',
        'post_api_v1_customers',
        'patch_api_v1_customers_id',
        'delete_api_v1_customers_id',
        'get_api_v1_customers_cached',
        'post_api_v1_customers_request',
        'describe_cached_customer_command',
        'describe_delete_customer_command',
    ]
    chip_h   = 16
    chip_gap = 4
    max_cw   = SIDEBAR_W - 14
    ty       = HEADER_H + 27
    for t in tools:
        if ty + chip_h > H - INPUT_H - 4:
            break
        label = t if tw(d, t, SMF) + 18 <= max_cw else t[:21] + '..'
        cw2 = min(max_cw, tw(d, label, SMF) + 16)
        rr(d, [6, ty, 6 + cw2, ty + chip_h], 8, CHIP_BG_C, outline=CHIP_TC, width=1)
        d.text((11, ty + 3), label, fill=CHIP_TC, font=SMF)
        ty += chip_h + chip_gap

    d.line([SIDEBAR_W, HEADER_H, SIDEBAR_W, H - INPUT_H], fill=(20, 45, 70))

    # ── input bar ──────────────────────────────────────────────────────────
    d.rectangle([0, H - INPUT_H, W, H], fill=NAV_BG)
    ix  = SIDEBAR_W + 12
    iw  = W - ix - 14
    iy1 = H - INPUT_H + 9
    iy2 = H - 9
    rr(d, [ix, iy1, ix + iw, iy2], 18, INP_BG, outline=(30, 60, 95))
    d.text((ix + 16, iy1 + 7), 'Ask anything...', fill=(55, 90, 135), font=UF)
    sbx = ix + iw - 32
    sby = iy1 + 4
    rr(d, [sbx, sby, sbx + 22, sby + 22], 11, (0, 110, 90))
    d.text((sbx + 5, sby + 5), '>', fill=(200, 240, 225), font=SMF)
    d.text((W - 240, H - 15), f'Step {step_n}/9: {step_label}', fill=(32, 62, 95), font=SMF)

    # ── main content ───────────────────────────────────────────────────────
    mx       = SIDEBAR_W + 2
    mw       = W - mx - 4
    cy_start = HEADER_H + 8
    cy_end   = H - INPUT_H - 4

    if not messages:
        # welcome / empty state
        cx = mx + mw // 2
        logo_sz = 76
        ll = cx - logo_sz // 2
        lt = cy_start + 28
        rr(d, [ll, lt, ll + logo_sz, lt + logo_sz], 14, (14, 30, 52))
        m = 8
        d.ellipse([ll + m, lt + m, ll + logo_sz - m, lt + logo_sz - m], fill=(22, 145, 72))
        lbl = 'CJ'
        d.text((ll + logo_sz//2 - tw(d, lbl, UF)//2, lt + logo_sz//2 - lh(UF)//2), lbl, fill=(255,255,255), font=UF)

        sub1 = 'Clean Code JN'
        d.text((cx - tw(d, sub1, SMF)//2, lt + logo_sz + 5), sub1, fill=(155, 190, 220), font=SMF)
        sub2 = 'by Jorg Nieveler'
        d.text((cx - tw(d, sub2, SMF)//2, lt + logo_sz + 5 + lh(SMF)), sub2, fill=(75, 110, 150), font=SMF)

        heading = 'CleanCodeJN AI Assistant'
        hy = lt + logo_sz + 12 + lh(SMF) * 2 + 6
        d.text((cx - tw(d, heading, UF)//2, hy), heading, fill=(200, 220, 240), font=UF)

        desc = 'This assistant connects directly to your API and can query, create, update and delete data — just by chatting.'
        dy = hy + lh(UF) + 8
        for ln in wrap(d, desc, SMF, mw - 60):
            d.text((cx - tw(d, ln, SMF)//2, dy), ln, fill=(88, 130, 175), font=SMF)
            dy += lh(SMF) + 2

        actions = ['List all customers', '+ Create a new invoice', 'Show a summary table']
        total_w = sum(tw(d, a, SMF) + 24 for a in actions) + 12 * (len(actions) - 1)
        ax = cx - total_w // 2
        for a in actions:
            aw = tw(d, a, SMF) + 24
            rr(d, [ax, dy + 12, ax + aw, dy + 36], 12, (10, 28, 52), outline=(25, 65, 110))
            d.text((ax + 12, dy + 18), a, fill=(95, 155, 215), font=SMF)
            ax += aw + 12
    else:
        # chat messages
        cy = cy_start + 6
        amw = mw - 20
        for msg in messages:
            if cy >= cy_end - 20:
                break
            role, text = msg['role'], msg['text']
            if role == 'user':
                ls = wrap(d, text, UF, amw - 60)
                bh = len(ls) * lh(UF) + 16
                bw = min(amw, max(tw(d, l, UF) for l in ls) + 28)
                bx = mx + 12 + amw - bw
                rr(d, [bx, cy, bx+bw, cy+bh], 10, MSG_USR)
                for j, ln in enumerate(ls):
                    d.text((bx+12, cy+8+j*lh(UF)), ln, fill=(240,245,255), font=UF)
                cy += bh + 8
            elif role == 'tool':
                lbl2 = f'tool call: {text}'
                ctw = tw(d, lbl2, SMF) + 22
                rr(d, [mx+8, cy, mx+8+ctw, cy+22], 5, TOOL_BG_C, outline=TOOL_BD_C)
                d.text((mx+16, cy+4), lbl2, fill=CHIP_TC, font=SMF)
                cy += 28
            elif role == 'thinking':
                d.text((mx+10, cy+2), text, fill=(50, 90, 140), font=SMF)
                cy += lh(SMF) + 6
            elif role == 'assistant':
                ls = wrap(d, text, UF, amw - 60)
                bh = len(ls) * lh(UF) + 16
                bw = min(amw, max(tw(d, l, UF) for l in ls) + 28)
                rr(d, [mx+8, cy, mx+8+bw, cy+bh], 10, MSG_BOT)
                for j, ln in enumerate(ls):
                    d.text((mx+20, cy+8+j*lh(UF)), ln, fill=(195,218,240), font=UF)
                cy += bh + 8

    if headline:
        _draw_overlay(d, headline, x_min=SIDEBAR_W + 2)

    return img

# ── scene data ─────────────────────────────────────────────────────────────────
TERMINAL_SETUP = [
    ([('PS C:\\> ', TPMT), ('dotnet new web -n MyApi', TCMD)], ''),
    ("The template 'ASP.NET Core Empty' was created successfully.", TOUT),
    ('', PLN),
    ([('PS C:\\> ', TPMT), ('cd MyApi', TCMD)], ''),
    ([('PS C:\\MyApi> ', TPMT), ('dotnet add package CleanCodeJN.GenericApis', TCMD)], ''),
    ('  Determining packages to restore...', TOUT),
    ('  Installed CleanCodeJN.GenericApis 6.2.3   [OK]', TOK),
]

CUSTOMER_CS = [
    'using CleanCodeJN.GenericApis.Abstractions.Contracts;',
    '',
    'public class Customer : IEntity<int>',
    '{',
    '    public int Id { get; set; }',
    '    public string Name { get; set; }',
    '    public string Email { get; set; }',
    '}',
]

DTOS_CS = [
    '// CustomerGetDto.cs',
    'public class CustomerGetDto : IDto',
    '{',
    '    public int Id { get; set; }',
    '    public string Name { get; set; }',
    '    public string Email { get; set; }',
    '}',
    '',
    '// CustomerPostDto.cs',
    'public class CustomerPostDto : IDto',
    '{',
    '    public string Name { get; set; }',
    '    public string Email { get; set; }',
    '}',
]

CUSTOMERS_API_CS = [
    'public class CustomersApi : IApi',
    '{',
    '    public List<string> Tags => ["Customers"];',
    '    public string Route => "api/v1/Customers";',
    '',
    '    public List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods =>',
    '    [',
    '        app => app.MapGet<Customer, CustomerGetDto, int>(Route, Tags),',
    '        app => app.MapGetById<Customer, CustomerGetDto, int>(Route, Tags),',
    '        app => app.MapPost<Customer, CustomerPostDto, CustomerGetDto>(Route, Tags),',
    '        app => app.MapPut<Customer, CustomerPutDto, CustomerGetDto>(Route, Tags),',
    '        app => app.MapPatch<Customer, CustomerGetDto, int>(Route, Tags),',
    '        app => app.MapDelete<Customer, CustomerGetDto, int>(Route, Tags),',
    '    ];',
    '}',
]

COMMAND_CS = [
    '/// <summary>',
    '/// Handles the deletion of a customer integration by',
    '/// executing a series of related requests.',
    '/// </summary>',
    'public class DeleteCustomerIntegrationCommand(',
    '    ICommandExecutionContext executionContext)',
    '    : IntegrationCommand<DeleteCustomerIntegrationRequest,',
    '                         Customer>(executionContext)',
    '{',
    '    /// <inheritdoc/>',
    '    public override async Task<BaseResponse<Customer>> Handle(',
    '        DeleteCustomerIntegrationRequest request,',
    '        CancellationToken cancellationToken) =>',
    '        await ExecutionContext',
    '            .LoadCustomersInParallelRequest(request.Id)',
    '            .LoadInvoiceByIdRequest()',
    '            .CustomerGetByIdRequest(request.Id)',
    '            .InvoiceGetFirstByIdRequest()',
    '            .DeleteCustomerByIdRequest()',
    '            .Execute<Customer>(cancellationToken);',
    '}',
]

PROGRAM_CS = [
    'builder.Services.AddCleanCodeJN<MyDbContext>(options =>',
    '{',
    '    options.ApplicationAssemblies = [typeof(Program).Assembly];',
    '    options.AiProxyOptions = new AiProxyOptions',
    '    {',
    '        LlmApiKey   = config["Anthropic:ApiKey"],',
    '        SelfBaseUrl = "https://localhost:7001",',
    '    };',
    '});',
    '',
    'var app = builder.Build();',
    '',
    'app.UseCleanCodeJNWithMinimalApis();',
    'app.UseCleanCodeJNWithMcp();',
    'app.UseCleanCodeJNWithAiChat();',
    'app.Run();',
]

TERMINAL_RUN = [
    ([('PS C:\\MyApi> ', TPMT), ('dotnet run', TCMD)], ''),
    ('', PLN),
    ([('info: ', TLOG), ('Now listening on https://localhost:7001', TOUT)], ''),
    ([('info: ', TLOG), ('MCP server ready at POST /mcp          [OK]', TOK)], ''),
    ([('info: ', TLOG), ('AI chat ready at POST /ai/chat         [OK]', TOK)], ''),
    ([('info: ', TLOG), ('Application started. Press Ctrl+C to shut down.', TOUT)], ''),
]

CHAT_MESSAGES_STEPS = [
    [],   # welcome / empty state
    [
        {'role':'user', 'text':'Create a customer named John Doe with email john@acme.com'}
    ],
    [
        {'role':'user', 'text':'Create a customer named John Doe with email john@acme.com'},
        {'role':'thinking', 'text':'Claude is thinking...'},
    ],
    [
        {'role':'user', 'text':'Create a customer named John Doe with email john@acme.com'},
        {'role':'tool', 'text':'post_api_v1_customers'},
    ],
    [
        {'role':'user', 'text':'Create a customer named John Doe with email john@acme.com'},
        {'role':'tool', 'text':'post_api_v1_customers'},
        {'role':'assistant', 'text':'Done! Customer "John Doe" was created successfully with ID 42.'},
    ],
]

# ── docs scene ────────────────────────────────────────────────────────────────
DOCS_STEPS = [
    'LoadCustomersInParallelRequest',
    'LoadInvoiceByIdRequest',
    'CustomerGetByIdRequest',
    'InvoiceGetFirstByIdRequest',
    'DeleteCustomerByIdRequest',
]

def render_docs_scene(n, headline=''):
    """
    n=0: header + summary + remarks, no steps
    n=1..5: reveal step n (collapsed)
    n=6: all steps, step 3 expanded (▼) with XML doc content
    """
    PANEL = W // 2 - 10   # left panel right edge

    img = Image.new('RGB', (W, H), BG)
    d   = ImageDraw.Draw(img)
    editor_chrome(d, 'Documentation — /docs', 'HTML   Step 9/9: Docs')
    d.rectangle([0, TOP, W, BOT], fill=BG)
    d.line([PANEL + 5, TOP, PANEL + 5, BOT], fill=BORDER)

    lx = 14
    lw = PANEL - lx - 10
    cy = TOP + 10

    # ── title + namespace ──────────────────────────────────────────────────
    d.text((lx, cy), 'DeleteCustomerIntegrationCommand', fill=TYP, font=UF)
    cy += lh(UF) + 2
    d.text((lx, cy), 'CleanCodeJN.GenericApis.Sample.Business', fill=DIM, font=SMF)
    cy += lh(SMF) + 8
    d.line([lx, cy, PANEL - 10, cy], fill=BORDER)
    cy += 8

    # ── summary (always shown) ─────────────────────────────────────────────
    d.text((lx, cy), '/// Summary', fill=CMT, font=CF)
    cy += lh(CF) + 2
    for ln in wrap(d, 'Handles the deletion of a customer integration by executing a series of related requests.', SMF, lw):
        d.text((lx + 10, cy), ln, fill=PLN, font=SMF)
        cy += lh(SMF)
    cy += 6

    # ── remarks (hidden when n==6 to reclaim vertical space) ──────────────
    if n < 6:
        d.text((lx, cy), '/// Remarks', fill=CMT, font=CF)
        cy += lh(CF) + 2
        for ln in wrap(d, 'Implements the IOSP principle: orchestrates atomic operations through a fluent ExecutionContext pipeline.', SMF, lw):
            d.text((lx + 10, cy), ln, fill=PLN, font=SMF)
            cy += lh(SMF)
        cy += 8

    d.line([lx, cy, PANEL - 10, cy], fill=BORDER)
    cy += 8

    # ── Steps heading ──────────────────────────────────────────────────────
    d.text((lx, cy), 'Steps', fill=MTH, font=CF)
    cy += lh(CF) + 6

    step_h   = 24
    step_gap = 4
    n_steps  = min(n, 5)

    # Expanded step XML content (step 3 = index 2 = CustomerGetByIdRequest)
    EXP_SUMMARY = 'Configures the execution context to retrieve a customer by their unique identifier.'
    EXP_REMARKS = 'This method adds a request to the execution context to fetch a customer by their ID, including related invoices.'
    EXP_PARAM   = 'customerId: The unique identifier of the customer to retrieve.'

    for i, sname in enumerate(DOCS_STEPS[:n_steps]):
        sx2 = PANEL - 10

        if n == 6 and i == 2:
            # expanded step: build content line list
            exp_content = (
                ['/// Summary']
                + wrap(d, EXP_SUMMARY, SMF, lw - 24)
                + ['']
                + ['/// Remarks']
                + wrap(d, EXP_REMARKS, SMF, lw - 24)
                + ['', '/// Parameters', EXP_PARAM]
            )
            exp_h = step_h + len(exp_content) * lh(SMF) + 10
            rr(d, [lx, cy, sx2, cy + exp_h], 6, (14, 28, 52), outline=KW, width=2)
            d.text((lx + 8,  cy + 5), '▼', fill=TYP, font=SMF)
            d.text((lx + 22, cy + 5), sname, fill=PLN, font=SMF)
            ey = cy + step_h + 2
            for line in exp_content:
                if line.startswith('///'):
                    d.text((lx + 14, ey), line, fill=CMT, font=SMF)
                elif line == '':
                    pass
                elif line.startswith('customerId'):
                    parts = line.split(':', 1)
                    px2 = lx + 14
                    d.text((px2, ey), parts[0] + ':', fill=PLN, font=SMF)
                    px2 += tw(d, parts[0] + ':', SMF) + 4
                    d.text((px2, ey), parts[1].strip(), fill=DIM, font=SMF)
                else:
                    d.text((lx + 14, ey), line, fill=PLN, font=SMF)
                ey += lh(SMF)
            cy += exp_h + step_gap
        else:
            rr(d, [lx, cy, sx2, cy + step_h], 6, GUTTER_BG, outline=BORDER)
            d.text((lx + 8,  cy + 5), '▶', fill=TYP, font=SMF)
            d.text((lx + 22, cy + 5), sname, fill=PLN, font=SMF)
            cy += step_h + step_gap

    # ── Right panel: flow chart ────────────────────────────────────────────
    rx = PANEL + 16
    rw = W - rx - 14
    ry = TOP + 10

    d.text((rx, ry), 'Flow', fill=MTH, font=CF)
    ry += lh(CF) + 10

    box_w   = min(rw - 4, 224)
    box_h   = 28
    box_gap = 16
    box_x   = rx + (rw - box_w) // 2

    for i, sname in enumerate(DOCS_STEPS[:n_steps]):
        by1 = ry
        rr(d, [box_x, by1, box_x + box_w, by1 + box_h], 5, (14, 28, 52), outline=KW, width=2)
        lbl = sname if tw(d, sname, SMF) <= box_w - 16 else sname[:25] + '..'
        d.text((box_x + (box_w - tw(d, lbl, SMF)) // 2, by1 + 7), lbl, fill=PLN, font=SMF)
        ry += box_h
        if i < n_steps - 1:
            ax = box_x + box_w // 2
            d.line([ax, ry + 1, ax, ry + box_gap - 4], fill=DIM, width=1)
            d.polygon([(ax - 4, ry + box_gap - 5), (ax + 4, ry + box_gap - 5), (ax, ry + box_gap)], fill=DIM)
            ry += box_gap

    if headline:
        _draw_overlay(d, headline, y_offset=140)

    return img


# ── frame builder ─────────────────────────────────────────────────────────────
def build_frames():
    frames = []   # list of (PIL.Image, delay_ms)

    def add(img, delay):
        frames.append((img.convert('P', palette=Image.ADAPTIVE, colors=128), delay))

    H1 = 'Build APIs you can talk to.'
    H2 = 'One setup. Everything wired.'
    H3 = 'Define your model. Get a full API.'
    H4 = 'No mapping. No config.'
    H5 = 'No controllers. No boilerplate.'
    H6 = 'From CRUD to real workflows.'
    H7 = "It's running. Let's talk."
    H8 = 'Talk to your API.'
    H9 = 'Automatic Documentation out of\nyour XML Comments and Workflows'

    # ─ Scene 1: Terminal setup ─────────────────────────────────────────────────
    lines = list(TERMINAL_SETUP)
    for n in range(1, len(lines)+1):
        add(render_terminal(lines, n, 'Install package', 1, H1), 200)
    add(render_terminal(lines, len(lines), 'Install package', 1, H1), 2500)

    # ─ Scene 2: Program.cs ────────────────────────────────────────────────────
    for n in range(1, len(PROGRAM_CS)+1):
        add(render_code(PROGRAM_CS, n, 'Program.cs', 'Setup', 2, H2), 120)
    add(render_code(PROGRAM_CS, len(PROGRAM_CS), 'Program.cs', 'Setup', 2, H2), 3500)

    # ─ Scene 3: Customer.cs ───────────────────────────────────────────────────
    for n in range(1, len(CUSTOMER_CS)+1):
        add(render_code(CUSTOMER_CS, n, 'Customer.cs', 'Entity', 3, H3), 150)
    add(render_code(CUSTOMER_CS, len(CUSTOMER_CS), 'Customer.cs', 'Entity', 3, H3), 3000)

    # ─ Scene 4: DTOs ──────────────────────────────────────────────────────────
    for n in range(1, len(DTOS_CS)+1):
        add(render_code(DTOS_CS, n, 'CustomerDtos.cs', 'DTOs', 4, H4), 120)
    add(render_code(DTOS_CS, len(DTOS_CS), 'CustomerDtos.cs', 'DTOs', 4, H4), 3000)

    # ─ Scene 5: CustomersApi.cs ───────────────────────────────────────────────
    for n in range(1, len(CUSTOMERS_API_CS)+1):
        add(render_code(CUSTOMERS_API_CS, n, 'CustomersApi.cs', 'Minimal API CRUD', 5, H5, overlay_y=90), 100)
    add(render_code(CUSTOMERS_API_CS, len(CUSTOMERS_API_CS), 'CustomersApi.cs', 'Minimal API CRUD', 5, H5, overlay_y=90), 3500)

    # ─ Scene 6: IOSP Command ──────────────────────────────────────────────────
    for n in range(1, len(COMMAND_CS)+1):
        add(render_code(COMMAND_CS, n, 'DeleteCustomerIntegrationCommand.cs', 'IOSP', 6, H6), 80)
    add(render_code(COMMAND_CS, len(COMMAND_CS), 'DeleteCustomerIntegrationCommand.cs', 'IOSP', 6, H6), 3500)

    # ─ Scene 7: Terminal run ──────────────────────────────────────────────────
    run_lines = list(TERMINAL_RUN)
    for n in range(1, len(run_lines)+1):
        add(render_terminal(run_lines, n, 'dotnet run', 7, H7), 250)
    add(render_terminal(run_lines, len(run_lines), 'dotnet run', 7, H7), 2500)

    # ─ Scene 8: Chat ──────────────────────────────────────────────────────────
    delays = [2000, 800, 900, 800, 3000]
    for step, msgs in enumerate(CHAT_MESSAGES_STEPS):
        add(render_chat(msgs, 'AI Chat: create customer via MCP', 8, H8), delays[step])

    # ─ Scene 9: Documentation ─────────────────────────────────────────────────
    add(render_docs_scene(0, H9), 1200)
    for n in range(1, 6):
        add(render_docs_scene(n, H9), 500)
    add(render_docs_scene(5, H9), 1500)   # all steps collapsed, brief hold
    add(render_docs_scene(6, H9), 5000)   # step 3 expanded, long hold

    return frames

# ── terminal render (fixed) ───────────────────────────────────────────────────
def render_terminal(lines, n, step_label, step_n, headline=''):
    img = Image.new('RGB', (W, H), TBG)
    d = ImageDraw.Draw(img)
    # chrome
    d.rectangle([0, 0, W, CHROME_H], fill=(16, 16, 16))
    title = 'Windows PowerShell'
    d.text((W//2 - tw(d, title, SMF)//2, 10), title, fill=(160,160,160), font=SMF)
    for i, c in enumerate([(255,95,86),(255,189,46),(39,201,63)]):
        d.ellipse([12+i*20, 11, 24+i*20, 23], fill=c)
    d.rectangle([0, H-STATUS_H, W, H], fill=(16, 16, 16))
    d.text((10, H-STATUS_H+4), f'  Step {step_n}/9: {step_label}', fill=DIM, font=SMF)
    row_h = lh(UF)
    y = CHROME_H + 20
    for entry in lines[:n]:
        if isinstance(entry, tuple) and len(entry) == 2:
            text_or_spans, color = entry
            if isinstance(text_or_spans, list):
                draw_spans(d, 22, y, text_or_spans, UF)
            elif color == '':
                # multi-span stored as (list, '')
                draw_spans(d, 22, y, text_or_spans, UF)
            else:
                d.text((22, y), text_or_spans, fill=color, font=UF)
        y += row_h
    if headline:
        _draw_overlay(d, headline)
    return img

# ── main ──────────────────────────────────────────────────────────────────────
if __name__ == '__main__':
    print('Generating frames...')
    frames = build_frames()
    print(f'  {len(frames)} frames generated')

    images  = [f for f, _ in frames]
    delays  = [d for _, d in frames]

    out = os.path.join(os.path.dirname(__file__), 'demo.gif')
    images[0].save(
        out,
        save_all=True,
        append_images=images[1:],
        duration=delays,
        loop=0,
        optimize=True,
    )
    size_kb = os.path.getsize(out) // 1024
    print(f'  Saved: {out}  ({size_kb} KB, {len(frames)} frames)')
