from PIL import Image, ImageDraw, ImageFont, ImageFilter
from pathlib import Path
import json, math, shutil
PROJECT=Path(__file__).resolve().parents[3]
ROOT=Path(__file__).resolve().parents[1]
AS=ROOT/'assets'; SRC=ROOT/'source'; SH=ROOT/'screen-designs'
for d in [AS,SRC,SH]: d.mkdir(parents=True,exist_ok=True)
GAME=PROJECT
FONT=str(GAME/'Assets/Resources/Fonts/NanumGothic-Regular.ttf')
PAPER='#FFFBF3';INK='#484B57';TEAL='#08AAA5';GRID='#D2C8B9';MINT='#E7F7F2';CORAL='#FF8875';YELLOW='#FFC342';PURPLE='#B286DE';BLUE='#59ADF0';COLORS=[CORAL,'#44BDB5',YELLOW,PURPLE,BLUE]
def font(size): return ImageFont.truetype(FONT,int(size))
def rounded(d,box,r,fill,outline=None,width=1): d.rounded_rectangle(box,radius=r,fill=fill,outline=outline,width=width)
def token(d,x,y,s,k,outline='#FFFFFF'):
 c=COLORS[k]; pad=s*.18
 if k==0:
  pts=[(x+s/2,y+pad),(x+s-pad,y+s-pad),(x+pad,y+s-pad)];d.polygon(pts,fill=c)
 elif k==1:d.ellipse((x+pad,y+pad,x+s-pad,y+s-pad),fill=c)
 elif k==2: rounded(d,(x+pad,y+pad,x+s-pad,y+s-pad),s*.11,c)
 elif k==3:d.polygon([(x+s/2,y+pad),(x+s-pad,y+s/2),(x+s/2,y+s-pad),(x+pad,y+s/2)],fill=c)
 else:
  pts=[(x+s*.5,y+pad),(x+s-pad,y+s*.39),(x+s*.81,y+s-pad*.68),(x+s*.19,y+s-pad*.68),(x+pad,y+s*.39)]
  d.polygon(pts,fill=c)
def text(d,xy,txt,sz,fill=INK,anchor='mm',stroke=0):d.text(xy,txt,font=font(sz),fill=fill,anchor=anchor,stroke_width=stroke,stroke_fill=fill)
def save(im,name):
 im.convert('RGB').save(AS/name,'PNG',optimize=True)

def board_art(d,x,y,size,seed=7,focus=40,filled=True):
 gap=size/9; rounded(d,(x-8,y-8,x+size+8,y+size+8),size*.026,'#FDFBF6',outline=GRID,width=max(2,int(size*.009)))
 for r in range(9):
  for c in range(9):
   xx=x+c*gap; yy=y+r*gap
   if r==focus//9 or c==focus%9: rounded(d,(xx+1,yy+1,xx+gap-1,yy+gap-1),gap*.07,MINT)
   if c%3==2 and c<8:d.line((xx+gap,yy,xx+gap,yy+gap),fill=GRID,width=max(2,int(size*.007)))
   else:d.line((xx+gap,yy,xx+gap,yy+gap),fill='#E4DCCF',width=1)
   if r%3==2 and r<8:d.line((xx,yy+gap,xx+gap,yy+gap),fill=GRID,width=max(2,int(size*.007)))
   else:d.line((xx,yy+gap,xx+gap,yy+gap),fill='#E4DCCF',width=1)
   i=(r*29+c*17+seed)%11
   if filled and i<5:token(d,xx+gap*.23,yy+gap*.21,gap*.56,(r*2+c+seed)%5)
   elif not filled and (r+c)%4==0:token(d,xx+gap*.23,yy+gap*.21,gap*.56,(r*2+c+seed)%5)
 rounded(d,(x+focus%9*gap+1,y+focus//9*gap+1,x+(focus%9+1)*gap-1,y+(focus//9+1)*gap-1),gap*.1,None,TEAL,max(2,int(size*.01)))

def icon(dim=1024):
 im=Image.new('RGBA',(dim,dim),PAPER);d=ImageDraw.Draw(im)
 # airy, gently framed 3x3 board with five tiny colorful pieces
 rounded(d,(dim*.07,dim*.07,dim*.93,dim*.93),dim*.22,'#FFF5DE')
 x=dim*.175;y=dim*.175;s=dim*.65;gap=s/3
 rounded(d,(x-5,y-5,x+s+5,y+s+5),dim*.075,PAPER,GRID,max(3,int(dim*.012)))
 for r in range(3):
  for c in range(3):
   xx=x+c*gap;yy=y+r*gap;rounded(d,(xx+2,yy+2,xx+gap-2,yy+gap-2),dim*.034,PAPER if (r+c)%2==0 else MINT)
   if (r,c) in [(0,0),(0,2),(1,1),(2,0),(2,2)]:token(d,xx+gap*.2,yy+gap*.2,gap*.6,[(0,0),(0,2),(1,1),(2,0),(2,2)].index((r,c)))
  
 for rr,cc in [(1,0),(0,1),(1,2),(2,1)]:
  if rr%2==cc%2: pass
 return im

# vector masters: logo and app tile stay editable in common design tools.
(SRC/'samgakwon-wordmark.svg').write_text(f'''<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1200 300"><rect width="1200" height="300" rx="80" fill="{PAPER}"/><g transform="translate(60 55)"><path d="M65 15L130 145H0Z" fill="{CORAL}"/><circle cx="225" cy="80" r="62" fill="#44BDB5"/><rect x="325" y="18" width="125" height="125" rx="24" fill="{YELLOW}"/><path d="M535 15L600 80 535 145 470 80Z" fill="{PURPLE}"/><path d="M685 15L750 52 728 145H642L620 52Z" fill="{BLUE}"/></g><text x="800" y="177" text-anchor="middle" dominant-baseline="middle" font-family="Nanum Gothic,sans-serif" font-size="126" font-weight="bold" fill="{INK}">삼각원</text></svg>''',encoding='utf-8')
(SRC/'app-icon.svg').write_text(f'''<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><rect width="512" height="512" rx="112" fill="{PAPER}"/><rect x="55" y="55" width="402" height="402" rx="94" fill="#FFF5DE"/><rect x="103" y="103" width="306" height="306" rx="48" fill="{PAPER}" stroke="{GRID}" stroke-width="8"/><path d="M205 103v306m102-306v306M103 205h306M103 307h306" stroke="#E4DCCF" stroke-width="5"/><rect x="208" y="208" width="96" height="96" rx="14" fill="{MINT}"/><path d="M151 129l32 64h-64z" fill="{CORAL}"/><circle cx="359" cy="154" r="29" fill="#44BDB5"/><rect x="331" y="229" width="57" height="57" rx="10" fill="{YELLOW}"/><path d="M154 326l31 31-31 31-31-31z" fill="{PURPLE}"/><path d="M357 324l30 18-11 47h-39l-11-47z" fill="{BLUE}"/></svg>''',encoding='utf-8')
# Round app icon, transparent icon-free silhouette on brand background.
for dim,name in [(512,'app-icon-512.png'),(1024,'app-icon-1024.png')]:save(icon(dim),name)
# Warm, uncluttered key art, centered safe area and original five-symbol motif.
def hero(w,h,headline=True):
 im=Image.new('RGB',(w,h),PAPER); d=ImageDraw.Draw(im)
 # oversized soft color shapes establish the casual atmosphere
 d.ellipse((-w*.18,-h*.41,w*.33,h*.45),fill='#FFF2CF');d.ellipse((w*.76,h*.64,w*1.15,h*1.25),fill='#FDE6DB')
 for j in range(8):
  x=w*(.04+.13*j);d.line((x,h*.09,x+w*.36,h*.09),fill='#F1ECE3',width=1)
 # Board at right, generous, instantly communicates the game.
 bx=w*.535;by=h*.19;bs=min(w*.38,h*.67);board_art(d,bx,by,bs,11,40,True)
 for j in range(5):token(d,w*.59+j*w*.067,h*.77,h*.042,j)
 if headline:
  text(d,(w*.27,h*.19),'작은 도형, 즐거운 생각',int(h*.027),TEAL)
  text(d,(w*.27,h*.34),'삼각원',int(h*.108),INK,stroke=max(0,int(h*.0012)))
  text(d,(w*.27,h*.44),'한 칸씩 채우는 9×9 도형 퍼즐',int(h*.029),INK)
  rounded(d,(w*.08,h*.64,w*.455,h*.77),h*.036,'#FFFFFF',outline='#E8DFD0',width=2)
  text(d,(w*.267,h*.685),'81개 스테이지  ·  4개 챕터',int(h*.023))
  text(d,(w*.267,h*.735),'클래식  ·  엔드리스',int(h*.022),'#938D83')
 return im
# Source and cropped ratio families.
main=hero(1920,1080)
save(main,'store-key-art-16x9-1920x1080.png')
save(hero(1920,700),'store-feature-banner-1920x700.png')
save(hero(1500,500),'community-banner-1500x500.png')
# 616x353 class store landscape image, master at 1856x1064.
save(hero(1856,1064),'store-landscape-1856x1064.png')
# Social card uses a bolder, readable center lock-up and compact board.
def social(w,h):
 im=Image.new('RGB',(w,h),PAPER);d=ImageDraw.Draw(im);d.ellipse((-w*.1,-h*.7,w*.4,h*.45),fill='#FFF2CF');d.ellipse((w*.82,h*.48,w*1.25,h*1.1),fill='#FDE6DB')
 text(d,(w*.29,h*.27),'작은 도형, 즐거운 생각',int(h*.055),TEAL)
 text(d,(w*.29,h*.49),'삼각원',int(h*.18),INK)
 text(d,(w*.29,h*.68),'81개 스테이지를 한 칸씩.',int(h*.043),INK)
 for j in range(5):token(d,w*.18+j*w*.12,h*.8,h*.075,j)
 return im
save(social(1200,630),'social-card-1200x630.png')
# Raw square display mark on STOVE-sized square/tile, plus square secondary logo.
def square_logo(dim):
 im=icon(dim);d=ImageDraw.Draw(im,'RGBA');rounded(d,(dim*.17,dim*.75,dim*.83,dim*.94),dim*.045,(255,251,243,242));text(d,(dim*.5,dim*.845),'삼각원',dim*.09,INK);return im
save(square_logo(1024),'square-logo-1024.png')
# Square marketing tile composition independent from a launcher icon.
def tile(dim=1024):
 im=Image.new('RGB',(dim,dim),PAPER);d=ImageDraw.Draw(im);d.ellipse((-dim*.32,-dim*.25,dim*.35,dim*.40),fill='#FFF2CF');
 rounded(d,(dim*.10,dim*.10,dim*.9,dim*.9),dim*.1,'#FFF8E9')
 for i in range(5):token(d,dim*.20+i*dim*.12,dim*.25,dim*.105,i)
 board_art(d,dim*.31,dim*.41,dim*.38,4,40,True)
 text(d,(dim*.5,dim*.90),'한 칸씩 채우는 도형 퍼즐',dim*.043,INK)
 return im
save(tile(),'store-square-card-1024.png')
# Actual supplied reference gameplay (from the accepted browser prototype): preserve faithfully, label exact device export.
ref=GAME/'Assets/Resources/samgakwon-assets-v1/previews/gameplay.png'
if ref.exists():
 image=Image.open(ref).convert('RGB'); image.save(AS/'prototype-gameplay-reference-1024x1536.png',optimize=True)
# 16:9 crop-safe mock screenshots accurately use existing authored art as references (never label these as Unity captures).
# Save icon monochrome concept and symbol SVG logo variant.
# Contact sheet for reviewers.
files=['app-icon-512.png','store-key-art-16x9-1920x1080.png','store-feature-banner-1920x700.png','community-banner-1500x500.png','social-card-1200x630.png','store-square-card-1024.png']
thumbs=Image.new('RGB',(1200,1500),'#EEE9E1');d=ImageDraw.Draw(thumbs)
for i,n in enumerate(files):
 im=Image.open(AS/n); im.thumbnail((535,420))
 x=45+(i%2)*575;y=40+(i//2)*475
 rounded(d,(x-12,y-12,x+547,y+430),18,'white'); thumbs.paste(im,(x+(535-im.width)//2,y+(420-im.height)//2));text(d,(x+265,y+450),n[:35],17,INK)
thumbs.save(ROOT/'preview-contact-sheet.png',optimize=True)
manifest={'product':'삼각원','artDirection':'밝은 크림, 민트, 다섯 가지 컬러 도형. 포근하고 짧게 즐기는 퍼즐.', 'palette':{'paper':PAPER,'ink':INK,'mint':MINT,'accent':TEAL,'triangle':CORAL,'circle':'#44BDB5','square':YELLOW,'diamond':PURPLE,'pentagon':BLUE},'assets':[]}
for p in sorted(AS.glob('*.png')):
 im=Image.open(p);manifest['assets'].append({'file':'assets/'+p.name,'width':im.width,'height':im.height,'purpose':p.stem})
manifest['sourceFiles'] = ['source/samgakwon-wordmark.svg', 'source/wordmark-transparent.svg', 'source/app-icon.svg', 'source/generate_store_art.py', 'source/generate_screen_designs.py', 'source/generate_wordmark.py']
manifest['uiDesignConcepts'] = []
for p in sorted(SH.glob('screen-design-*.png')):
 im=Image.open(p); manifest['uiDesignConcepts'].append({'file':'screen-designs/'+p.name,'width':im.width,'height':im.height,'purpose':'Unity UI layout concept; not a release-build screenshot'})
(ROOT/'asset-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
print('\n'.join(f"{a['file']} {a['width']}x{a['height']}" for a in manifest['assets']))
