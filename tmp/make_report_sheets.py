from PIL import Image, ImageDraw
from pathlib import Path
import sys
folder=Path(sys.argv[1] if len(sys.argv)>1 else 'tmp/report_render2')
fs=sorted(folder.glob('page-*.png'),key=lambda x:int(x.stem.split('-')[1]))
print(len(fs))
for j in range((len(fs)+19)//20):
 out=Image.new('RGB',(700,840),'white'); draw=ImageDraw.Draw(out)
 for i,f in enumerate(fs[j*20:(j+1)*20]):
  out.paste(Image.open(f).convert('RGB').resize((140,198)),((i%5)*140,(i//5)*210))
  draw.text(((i%5)*140+4,(i//5)*210+198),str(j*20+i+1),fill='black')
 out.save(folder/f'sheet-{j+1}.png')
