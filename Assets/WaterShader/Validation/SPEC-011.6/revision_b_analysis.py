"""Summarize completed Revision B GPU captures without changing source assets."""
from pathlib import Path
import json
import numpy as np
from PIL import Image, ImageDraw, ImageFont

root = Path(__file__).resolve().parent
font = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 20)
names = ['G_StraightGateMotion', 'Straight_Rerun'] + [f'RevisionB_Trial{i}' for i in range(2, 7)]
rows, metrics = [], {}
for name in names:
    folder = root / '.frames' / name
    if not (folder / 'Capture.txt').exists():
        continue
    paths = sorted(folder.glob('*.png'))
    row = Image.new('RGB', (960, 210), '#101921')
    ImageDraw.Draw(row).text((12, 5), name, font=font, fill='white')
    row.paste(Image.open(paths[0]).crop((0, 145, 960, 325)), (0, 30))
    rows.append(row)
    frames = [np.asarray(Image.open(p))[157:322, 50:910, 0].astype(float) for p in paths]
    delta = [float(np.abs(b-a).mean()) for a,b in zip(frames, frames[1:])]
    fps = 30 if name in ['G_StraightGateMotion', 'Straight_Rerun'] else 20
    a,b = frames[0],frames[fps]
    corr = [np.corrcoef(a[:, :-s].ravel(), b[:, s:].ravel())[0,1] for s in range(1,250)]
    metrics[name] = {'frames':len(frames), 'seconds':len(frames)/fps,
        'translation_px_per_second':int(np.argmax(corr)+1), 'translation_correlation':float(max(corr)),
        'adjacent_mae_mean':float(np.mean(delta)), 'adjacent_mae_max':max(delta),
        'entry_middle_exit_coverage_red_gt_50':[
            float(np.mean([np.mean(part>50) for f in frames for part in [np.array_split(f,3,axis=1)[i]]])) for i in range(3)]}
contact = Image.new('RGB',(960,210*len(rows)))
for i,row in enumerate(rows):
    contact.paste(row,(0,210*i))
contact.save(root/'RevisionB_Trials.jpg')
(root/'RevisionB_Measurements.json').write_text(json.dumps(metrics,indent=2),encoding='utf-8')
print(json.dumps(metrics,indent=2))
