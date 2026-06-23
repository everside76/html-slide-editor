# -*- coding: utf-8 -*-
"""HtmlSlideEditor 아이콘 생성: 겹친 페이지(슬라이드) + 빨간 헤더 + 연필(편집)."""
import os
from PIL import Image, ImageDraw

S = 1024  # 고해상도 마스터 (다운샘플로 각 크기 생성)

NAVY  = (32, 36, 44)
RED   = (192, 57, 43)
RED_D = (156, 43, 32)
GOLD  = (210, 162, 64)
GOLD_D= (170, 128, 44)
WHITE = (252, 252, 250)
PAGE2 = (214, 219, 226)
LINE  = (150, 157, 167)
WOOD  = (226, 198, 150)
GRAPH = (60, 64, 72)
ERASE = (236, 156, 150)
FERRU = (210, 214, 220)

def lerp(a, b, t):
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(3))

def diagonal_gradient(size, c1, c2):
    g = Image.new("RGB", (size, size))
    px = g.load()
    for y in range(size):
        for x in range(size):
            t = (x + y) / (2 * (size - 1))
            px[x, y] = lerp(c1, c2, t)
    return g

def rounded_mask(size, box, radius):
    m = Image.new("L", (size, size), 0)
    d = ImageDraw.Draw(m)
    d.rounded_rectangle(box, radius=radius, fill=255)
    return m

# ---- 배경: 둥근 사각 + 대각 그라데이션(네이비→레드) ----
img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
grad = diagonal_gradient(96, NAVY, RED).resize((S, S), Image.BILINEAR).convert("RGBA")
bg_mask = rounded_mask(S, (40, 40, S - 40, S - 40), 200)
img.paste(grad, (0, 0), bg_mask)

d = ImageDraw.Draw(img)

# ---- 뒤 페이지(살짝 비쳐 보이는 두 번째 슬라이드) ----
bw, bh = 392, 506
bx, by = 286, 214
d.rounded_rectangle((bx, by, bx + bw, by + bh), radius=30, fill=PAGE2)

# ---- 앞 페이지 ----
fx, fy = bx + 70, by + 70
d.rounded_rectangle((fx, fy, fx + bw, fy + bh), radius=30, fill=WHITE)

# 빨간 헤더 막대 (보고서 제목줄)
pad = 46
hx0 = fx + pad
hx1 = fx + bw - pad
hy0 = fy + 52
d.rounded_rectangle((hx0, hy0, hx0 + (hx1 - hx0) * 0.62, hy0 + 40), radius=12, fill=RED)
# 헤더 밑줄
d.rectangle((hx0, hy0 + 64, hx1, hy0 + 70), fill=(230, 226, 219))

# 본문 텍스트 라인들
ly = hy0 + 120
widths = [1.0, 0.82, 0.93, 0.7, 0.88, 0.6]
for i, w in enumerate(widths):
    d.rounded_rectangle((hx0, ly, hx0 + (hx1 - hx0) * w, ly + 26), radius=13, fill=LINE)
    ly += 58

# ---- 연필 (별도 레이어에 그린 뒤 회전) ----
PW, PH = 620, 150
pen = Image.new("RGBA", (PW, PH), (0, 0, 0, 0))
pd = ImageDraw.Draw(pen)
body_l, body_r = 150, 520           # 몸통
pd.rounded_rectangle((body_l, 18, body_r, PH - 18), radius=16, fill=GOLD)
pd.rectangle((body_l, 18, body_l + 30, PH - 18), fill=GOLD_D)  # 몸통 음영
# 페룰 + 지우개 (오른쪽 끝)
pd.rectangle((body_r, 22, body_r + 36, PH - 22), fill=FERRU)
pd.rounded_rectangle((body_r + 36, 22, body_r + 96, PH - 22), radius=22, fill=ERASE)
# 깎인 나무 + 흑연 심 (왼쪽 끝)
pd.polygon([(body_l, 18), (body_l, PH - 18), (40, PH // 2)], fill=WOOD)
pd.polygon([(78, PH // 2 - 26), (78, PH // 2 + 26), (40, PH // 2)], fill=GRAPH)

pen = pen.rotate(40, expand=True, resample=Image.BICUBIC)
# 오른쪽 아래에서 위로 향하도록 배치 (편집 중인 느낌)
img.alpha_composite(pen, (int(S * 0.40), int(S * 0.46)))

# ---- 저장 ----
here = os.path.dirname(os.path.abspath(__file__))
master = img.resize((256, 256), Image.LANCZOS)
master.save(os.path.join(here, "icon_preview.png"))
ico_path = os.path.join(os.path.dirname(here), "icon.ico")
master.save(ico_path, sizes=[(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (24, 24), (16, 16)])
print("saved:", ico_path)
print("preview:", os.path.join(here, "icon_preview.png"))
