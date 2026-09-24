from pathlib import Path
import sys
sys.path.insert(0,str(Path(__file__).resolve().parents[1]/'source'))
from generate_store_art import ROOT,SRC,FONT,COLORS,token
from PIL import Image,ImageDraw,ImageFont
im=Image.new('RGBA',(1500,360),(0,0,0,0));d=ImageDraw.Draw(im)
for i in range(5): token(d,66+i*119,115,96,i)
d.text((780,180),'삼각원',font=ImageFont.truetype(FONT,175),fill='#484B57',anchor='lm')
im.save(ROOT/'assets/wordmark-transparent-1500x360.png',optimize=True)
svg='''<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1500 360"><g transform="translate(58 108)"><path d="M48 4L96 100H0Z" fill="#FF8875"/><circle cx="169" cy="52" r="46" fill="#44BDB5"/><rect x="242" y="6" width="92" height="92" rx="17" fill="#FFC342"/><path d="M406 4l49 48-49 48-49-48z" fill="#B286DE"/><path d="M515 4l49 28-17 68h-63l-17-68z" fill="#59ADF0"/></g><text x="780" y="220" font-family="Nanum Gothic,sans-serif" font-size="175" font-weight="bold" fill="#484B57">삼각원</text></svg>'''
(SRC/'wordmark-transparent.svg').write_text(svg,encoding='utf-8')
