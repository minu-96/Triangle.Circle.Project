"""Build a contact sheet and manifest for files prepared for STOVE Studio."""
from pathlib import Path
import json
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "stove-upload"
PAPER = "#F4F0E8"
INK = "#292B2B"
MUTED = "#77746F"

files = [
    ("상품 페이지 배경 · 1920×520", "background/product-background-1920x520.png", 900, 244),
    ("상품 타이틀 · 1:1 · 512×512", "ko-KR/1x1/title-image-512.png", 360, 360),
    ("상품 목록 · 757×426", "ko-KR/16x9/listing-image-757x426.png", 570, 321),
    ("한 줄 소개 · 558×132", "ko-KR/intro/one-line-image-558x132.png", 570, 135),
    ("전시 이미지 01 · 퍼즐 규칙", "ko-KR/exhibit/01-rule-757x426.png", 480, 270),
    ("전시 이미지 02 · 클래식", "ko-KR/exhibit/02-classic-757x426.png", 480, 270),
    ("전시 이미지 03 · 스테이지", "ko-KR/exhibit/03-stages-757x426.png", 480, 270),
    ("전시 이미지 04 · 플레이 예시 그래픽", "ko-KR/exhibit/04-play-757x426.png", 480, 270),
    ("전시 이미지 05 · 엔드리스", "ko-KR/exhibit/05-endless-757x426.png", 480, 270),
    ("제공 화면 01 · 홈", "ko-KR/exhibit-860x483/01-title-home-860x483.png", 480, 270),
    ("제공 화면 02 · 플레이", "ko-KR/exhibit-860x483/02-gameplay-860x483.png", 480, 270),
    ("제공 화면 03 · 스테이지", "ko-KR/exhibit-860x483/03-stage-select-860x483.png", 480, 270),
]
manifest = []
for label, rel, *_ in files:
    with Image.open(OUT / rel) as im:
        manifest.append({"label": label, "path": rel, "width": im.width,
                         "height": im.height, "format": im.format})
(OUT / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n")

try:
    from generate_store_art import FONT
    f_head = ImageFont.truetype(str(FONT), 38)
    f_label = ImageFont.truetype(str(FONT), 20)
except Exception:
    f_head = ImageFont.load_default()
    f_label = ImageFont.load_default()
W, H = 1500, 2100
sheet = Image.new("RGB", (W, H), PAPER)
d = ImageDraw.Draw(sheet)
d.text((54, 36), "STOVE 등록용 이미지 미리보기", font=f_head, fill=INK)
d.text((56, 88), "Studio 입력 항목별 파일 · PNG · 크기는 manifest.json 참조", font=f_label, fill=MUTED)

def thumb(rel, max_w, max_h):
    with Image.open(OUT / rel) as im:
        im = im.convert("RGB")
        im.thumbnail((max_w, max_h), Image.Resampling.LANCZOS)
        return im.copy()

label, rel, tw, th = files[0]
sheet.paste(thumb(rel, tw, th), (56, 142))
d.text((56, 395), label, font=f_label, fill=INK)
identity = [
    (56, files[1], 360, 360),
    (450, files[2], 570, 321),
    (1050, files[3], 390, 100),
]
for x, item, mw, mh in identity:
    label, rel, *_ = item
    sheet.paste(thumb(rel, mw, mh), (x, 452))
    d.text((x, 822), label, font=f_label, fill=INK)
d.text((56, 900), "전시 이미지 · 소개 그래픽 및 제공된 게임 화면", font=f_head, fill=INK)
for idx, item in enumerate(files[4:]):
    label, rel, tw, th = item
    row, col = divmod(idx, 3)
    x, y = 56 + col * 472, 970 + row * 340
    sheet.paste(thumb(rel, 450, 270), (x, y))
    d.text((x, y + 278), label, font=f_label, fill=INK)
sheet.save(OUT / "preview-contact-sheet.png", optimize=True)
print("Wrote stove-upload/manifest.json and preview-contact-sheet.png")
