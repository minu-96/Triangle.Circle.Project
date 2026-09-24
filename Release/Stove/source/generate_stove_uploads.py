"""Render the exact image slot sizes supplied by the STOVE Studio form."""
from pathlib import Path
import sys
sys.path.insert(0,str(Path(__file__).resolve().parent))
from generate_store_art import ROOT, FONT, PAPER, INK, TEAL, GRID, MINT, CORAL, YELLOW, PURPLE, BLUE, COLORS, font, rounded, token, text, board_art
from PIL import Image, ImageDraw

OUT=ROOT/'stove-upload'
for folder in ['background','ko-KR/1x1','ko-KR/16x9','ko-KR/intro','ko-KR/exhibit']:
    (OUT/folder).mkdir(parents=True,exist_ok=True)

def card(w,h,bg=PAPER):
    return Image.new('RGB',(w,h),bg)
def band(d,x,y,label,width=None):
    label='  '+label+'  '
    f=font(max(14,int(d._image.height*.038)))
    box=d.textbbox((0,0),label,font=f)
    ww=box[2]-box[0]+28
    rounded(d,(x,y,x+(width or ww),y+46),19,'#FFFFFF',outline='#E8DFD0',width=2)
    text(d,(x+(width or ww)/2,y+23),label,20,INK)
def save(im,where):
    im.save(OUT/where,'PNG',optimize=True)

# Optional static page background, exact Studio field dimensions (1920x520).
w,h=1920,520
im=card(w,h);d=ImageDraw.Draw(im)
d.ellipse((-230,-430,670,460),fill='#FFF2CF');d.ellipse((1600,310,2200,910),fill='#FDE6DB')
# Product title and copy stay inside the middle safe area.
text(d,(112,129),'작은 도형, 즐거운 생각',32,TEAL,anchor='lm')
text(d,(110,238),'삼각원',112,INK,anchor='lm',stroke=1)
text(d,(116,309),'한 칸씩 채우는 9×9 도형 퍼즐',29,INK,anchor='lm')
rounded(d,(108,365,742,442),24,'#FFFFFF',outline='#E8DFD0',width=2)
text(d,(425,403),'81개 스테이지  ·  4개 챕터  ·  엔드리스',22,'#77746F')
board_art(d,1202,58,404,seed=11,focus=40,filled=True)
for i in range(5):token(d,1212+i*76,463,48,i)
save(im,'background/product-background-1920x520.png')

# Square product title image: a readable icon and wordmark together.
w=h=512;im=card(w,h);d=ImageDraw.Draw(im)
d.ellipse((-88,-116,275,245),fill='#FFF2CF');d.ellipse((364,359,630,625),fill='#FDE6DB')
rounded(d,(68,32,444,408),83,'#FFF8E9')
# 3 × 3 token board from the approved launcher icon language.
bx,by,bs=142,108,228;gap=bs/3
rounded(d,(bx-7,by-7,bx+bs+7,by+bs+7),29,PAPER,GRID,4)
for r in range(3):
 for c in range(3):
  xx=bx+c*gap;yy=by+r*gap
  rounded(d,(xx+3,yy+3,xx+gap-3,yy+gap-3),16,MINT if (r+c)%2 else PAPER)
for r,c,k in [(0,0,0),(0,2,1),(1,1,2),(2,0,3),(2,2,4)]:
 token(d,bx+c*gap+12,by+r*gap+12,gap-24,k)
text(d,(256,459),'삼각원',47,INK)
save(im,'ko-KR/1x1/title-image-512.png')

# 16:9 listing image, exact dimensions shown by STOVE Studio.
w,h=757,426;im=card(w,h);d=ImageDraw.Draw(im)
d.ellipse((-125,-220,232,142),fill='#FFF2CF');d.ellipse((631,300,900,564),fill='#FDE6DB')
text(d,(48,85),'작은 도형, 즐거운 생각',18,TEAL,anchor='lm')
text(d,(48,163),'삼각원',72,INK,anchor='lm',stroke=1)
text(d,(52,214),'한 칸씩 채우는 도형 퍼즐',19,INK,anchor='lm')
rounded(d,(44,257,335,337),18,'#FFFFFF',outline='#E8DFD0',width=2)
text(d,(190,283),'81개 스테이지 · 4개 챕터',15)
text(d,(190,314),'클래식 · 엔드리스',15,'#85847E')
board_art(d,443,36,267,seed=11,focus=40,filled=True)
for i in range(5):token(d,469+i*46,355,31,i)
save(im,'ko-KR/16x9/listing-image-757x426.png')

# Optional image above the one-line text: logo, distinctive tokens, warm ground.
w,h=558,132;im=card(w,h);d=ImageDraw.Draw(im)
d.ellipse((-58,-105,135,105),fill='#FFF2CF')
for i in range(5):token(d,31+i*33,43,32,i)
d.line((224,27,224,105),fill='#E5DDCF',width=2)
text(d,(382,55),'한 칸씩 천천히,',23,INK)
text(d,(382,87),'도형 퍼즐의 즐거움.',20,TEAL)
save(im,'ko-KR/intro/one-line-image-558x132.png')

# Five promotional exhibition cards; they explain the real rules/content rather than pretending to be captured gameplay.
def exhibit(name,kicker,title,subtitle,seed,shapes):
 w,h=757,426;im=card(w,h);d=ImageDraw.Draw(im)
 d.ellipse((-130,-220,228,143),fill='#FFF2CF');d.ellipse((630,300,900,570),fill='#FDE6DB')
 text(d,(49,74),kicker,17,TEAL,anchor='lm')
 text(d,(49,148),title,44,INK,anchor='lm',stroke=0)
 text(d,(52,195),subtitle,19,'#706F6B',anchor='lm')
 # Original illustration panels use the existing colors, rules, and board geometry.
 if name=='01-rule':
  bx,by=444,75; cell=65
  for r in range(3):
   for c in range(3):
    x=bx+c*cell;y=by+r*cell
    rounded(d,(x,y,x+cell-3,y+cell-3),9,MINT if r==1 else '#FFFFFF',outline='#E8DFD0',width=2)
    if r==1 and c<2:token(d,x+13,y+13,38,1)
    if r==1 and c==2:token(d,x+13,y+13,38,1)
  rounded(d,(bx+2*cell+3,by+cell+3,bx+3*cell-6,by+2*cell-6),10,None,outline=CORAL,width=4)
  text(d,(544,322),'세 번째 같은 도형은 놓을 수 없어요',16,INK)
 elif name=='02-classic':
  for j,(t,col) in enumerate([('15',CORAL),('35',TEAL),('55',PURPLE)]):
   x=430+j*94;rounded(d,(x,95,x+78,252),21,'#FFFFFF',outline='#E8DFD0',width=2)
   text(d,(x+39,150),t,34,col);text(d,(x+39,218),'빈칸',15,'#85847E')
  for i in range(5):token(d,457+i*46,290,35,i)
 elif name=='03-stages':
  for j,(label,col) in enumerate([('입문',CORAL),('집중',YELLOW),('균형',TEAL),('종합',PURPLE)]):
   x=430+(j%2)*140;y=81+(j//2)*106
   rounded(d,(x,y,x+119,y+82),19,'#FFFFFF',outline='#E8DFD0',width=2)
   rounded(d,(x+11,y+12,x+46,y+47),11,col)
   text(d,(x+77,y+37),label,20,INK)
   text(d,(x+60,y+65),f'{j+1}장',14,'#85847E')
  board_art(d,471,302,204,seed=8,focus=40,filled=False)
 elif name=='04-play':
  board_art(d,464,62,250,seed=seed,focus=40,filled=True)
  for i in range(5):
   rounded(d,(465+i*49,344,507+i*49,390),12,'#FFFFFF',outline='#E8DFD0',width=2);token(d,471+i*49,350,30,i)
 elif name=='05-endless':
  for i,k in enumerate([0,1,2,3,4]):token(d,465+(i%3)*81,100+(i//3)*77,52,k)
  text(d,(547,315),'∞',105,TEAL)
  text(d,(555,385),'엔드리스',19,INK)
 save(im,'ko-KR/exhibit/'+name+'-757x426.png')

cards=[
 ('01-rule','5가지 도형, 하나의 규칙','같은 도형은 두 개까지','행 · 열 · 3×3 블록을 살펴보세요.',2,[0]),
 ('02-classic','편안한 속도로 즐기는 클래식','빈칸 15 · 35 · 55개','원하는 난이도를 골라 한 판씩.',3,[0]),
 ('03-stages','네 챕터, 81개의 퍼즐','조금씩 달라지는 빈칸 배치','입문부터 종합까지 차근차근 열려요.',4,[0]),
 ('04-play','빈칸을 하나씩 채워보세요','원하는 도형을 골라 놓아보세요','행 · 열 · 블록의 규칙에 맞추면 완성.',11,[0]),
 ('05-endless','모두 풀었다면, 계속해서','엔드리스 보너스 모드','새 퍼즐과 함께 한 판 더.',5,[0])
]
for args in cards:exhibit(*args)
print('Rendered STOVE slots under',OUT)
