from PIL import Image,ImageDraw
from pathlib import Path
import sys
sys.path.insert(0,str(Path(__file__).resolve().parent))
import generate_store_art as a
ROOT=a.ROOT;SH=ROOT/'screen-designs'

def canvas(title,sub='조금씩 채워지는 즐거움'):
 im=Image.new('RGB',(680,1000),a.PAPER);d=ImageDraw.Draw(im)
 for i,k in enumerate(range(3)):a.token(d,30+i*26,28,22,i)
 a.text(d,(145,39),'삼각원',20)
 a.text(d,(610,39),'⚙',20)
 a.text(d,(48,106),title,32,anchor='lm');a.text(d,(48,163),sub,15,'#938D83',anchor='lm')
 return im,d

def button(d,box,label,subtitle=None,mark=None,active=False):
 x1,y1,x2,y2=box;a.rounded(d,box,18,'#E7F7F2' if active else '#FFFFFF',outline='#E8DFD0',width=2)
 if mark is not None:a.token(d,x1+23,(y1+y2)/2-18,36,mark)
 xx=x1+82 if mark is not None else (x1+x2)/2
 a.text(d,(xx,y1+(y2-y1)*.39),label,22,anchor='lm' if mark is not None else 'mm')
 if subtitle:a.text(d,(xx,y1+(y2-y1)*.70),subtitle,14,'#938D83',anchor='lm' if mark is not None else 'mm')

def footer(im,d):
 d.line((48,943,632,943),fill='#E9E1D5',width=2)
 a.text(d,(340,970),'삼각원  ·  화면 디자인 시안',13,'#A7A199')

def save(im,name):im.resize((1020,1500),Image.Resampling.LANCZOS).save(SH/name,optimize=True)
# Main menu concept
im,d=canvas('')
for i in range(5):a.token(d,170+i*73,165,55,i)
a.text(d,(340,294),'삼각원',58);a.text(d,(340,342),'작은 도형, 즐거운 생각',20,'#938D83')
button(d,(85,417,595,481),'이어하기','이어서 한 칸 채우기',None,True)
for i,(title,sub,k) in enumerate([('클래식','세 가지 난이도로 가볍게',0),('스테이지','4개 챕터 · 81개의 도전',1),('엔드리스','81단계 완성 후 해금',4)]):button(d,(85,506+i*100,595,587+i*100),title,sub,k)
a.text(d,(340,875),'잠깐의 여유에 한 판씩.',16,'#938D83');footer(im,d);save(im,'screen-design-home.png')
# Classic modes
im,d=canvas('클래식','빈칸 수만 달라져요 · 무작위로 만나는 퍼즐')
for i,(t,n) in enumerate([('쉬움',15),('보통',35),('어려움',55)]):
 y=293+i*157;button(d,(62,y,618,y+123),t,f'빈칸 {n}개 · 나만의 속도로',i,True if i==0 else False)
 a.text(d,(340,805),'최고 기록은 이 기기에 저장됩니다.',15,'#938D83')
footer(im,d);save(im,'screen-design-classic.png')
# Stages/chapter selector
im,d=canvas('스테이지','다른 배치 전략으로 즐기는 81개의 퍼즐')
for i,name in enumerate(['입문','집중','균형','종합']):button(d,(44+i*153,223,184+i*153,275),name,None,i,active=i==0)
a.text(d,(56,320),'1장 · 입문',27,anchor='lm');a.text(d,(56,361),'무작위로 흩어진 빈칸을 하나씩 채워보세요.',15,'#938D83',anchor='lm')
for i in range(20):
 c=i%5;r=i//5;x=47+c*122;y=420+r*96
 state=i<4; rounded=a.rounded
 rounded(d,(x,y,x+106,y+72),15,'#BCE7DC' if state else '#FFAD98' if i==4 else '#F2EEE7',outline='#E8DFD0')
 a.text(d,(x+53,y+36),('✓' if state else str(i+1)),21 if not state else 19)
a.text(d,(340,843),'1–20 무작위  ·  21–40 중심부  ·  41–60 블록 균형  ·  61–81 종합',13,'#938D83')
footer(im,d);save(im,'screen-design-stages.png')
# Actual UI design target, based on running prototype composition (label remains explicit as a design frame).
im,d=canvas('스테이지 24','2장 · 집중')
a.text(d,(591,113),'03:24',24)
a.board_art(d,75,197,530,11,40,True)
a.text(d,(340,762),'행 · 열 · 블록마다 같은 도형은 최대 2개',16)
for i in range(5):
 x=78+i*105;y=804;a.rounded(d,(x,y,x+78,y+74),16,'#E7F7F2' if i==1 else '#FFFFFF',outline='#D2C8B9',width=2);a.token(d,x+17,y+6,44,i);a.text(d,(x+39,y+59),str(i+1),12,'#938D83')
button(d,(161,887,331,934),'지우기');button(d,(349,887,519,934),'힌트',None,None,True)
footer(im,d);save(im,'screen-design-gameplay.png')
# Clear and personal-best design
im,d=canvas('','')
a.board_art(d,105,160,470,11,40,True)
a.rounded(d,(93,350,587,725),30,'#FFF5CF',outline='#E7DCBD',width=2)
a.text(d,(340,415),'퍼즐 완성!',32)
a.text(d,(340,504),'03:24',48)
a.text(d,(340,565),'행 · 열 · 블록의 규칙을 모두 맞췄어요.',16,'#817B70')
button(d,(142,625,538,678),'다음 스테이지',None,None,True)
button(d,(142,689,538,735),'스테이지 선택')
footer(im,d);save(im,'screen-design-clear.png')
print('Created 5 portrait screen design frames at 1020×1500: '+', '.join(p.name for p in SH.glob('screen-design-*.png')))
