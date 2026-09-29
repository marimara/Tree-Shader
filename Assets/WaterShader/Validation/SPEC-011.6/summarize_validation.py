"""Encode completed GPU frame sequences and compare translation/mark coverage."""
from pathlib import Path
import json
import subprocess
import numpy as np
from PIL import Image, ImageDraw, ImageFont

root = Path(__file__).resolve().parent
ffmpeg = Path('C:/Program Files/Krita (x64)/bin/ffmpeg.exe')
rates = {'F_BaselineStraight': 30, 'G_StraightTrial1': 30,
         'G_StraightGateMotion': 30, 'H_Curved': 20, 'I_Obstacle': 20,
         'I_OldObstacle': 15, 'J_Scale1': 20, 'J_NonUniform': 20,
         'K_Calm': 10, 'K_Fast': 20, 'L_LongRun': 20,
         'M_StrengthGradient': 20, 'N_Uniform': 20,
         **{f'Stage{i}': 15 for i in range(1, 6)}}

def run(*args):
    subprocess.run([str(ffmpeg), '-v', 'error', *map(str, args)], check=True)

for name, rate in rates.items():
    folder = root / '.frames' / name
    if not folder.exists():
        continue
    # All executions of this script are made after checking capture completion.
    run('-framerate', rate, '-i', folder / '%05d.png', '-c:v', 'libopenh264',
        '-b:v', '4M', '-pix_fmt', 'yuv420p', '-an', '-y', root / f'{name}.mp4')

def gray(name, i):
    return np.asarray(Image.open(root/'.frames'/name/f'{i:05}.png'))[157:322, 50:910, 0].astype(float)

metrics = {}
for name in ['F_BaselineStraight', 'G_StraightGateMotion']:
    a, b = gray(name, 0), gray(name, 30)
    scores = [np.corrcoef(a[:, :-s].ravel(), b[:, s:].ravel())[0,1] for s in range(1, 200)]
    masks = a > 50  # Red base is near zero; fixed comparison threshold, not shader threshold.
    runs = []
    for x in range(masks.shape[1]):
        edges = np.diff(np.r_[False, masks[:, x], False].astype(int))
        runs.extend((np.flatnonzero(edges == -1) - np.flatnonzero(edges == 1)).tolist())
    metrics[name] = {'one_second_displacement_px': int(np.argmax(scores)+1),
                     'one_second_translation_correlation': float(max(scores)),
                     'visible_coverage_red_gt_50': float(masks.mean()),
                     'vertical_mark_run_px_median': float(np.median(runs)),
                     'vertical_mark_run_px_p90': float(np.percentile(runs,90))}

name = 'G_StraightGateMotion'
count = len(list((root/'.frames'/name).glob('*.png')))
changes = [float(np.mean(np.abs(gray(name,i)-gray(name,i-1)))) for i in range(1,count)]
metrics[name]['adjacent_frame_MAE_mean'] = float(np.mean(changes))
metrics[name]['adjacent_frame_MAE_max'] = max(changes)
metrics[name]['frames'] = count
(root/'Measurements.json').write_text(json.dumps(metrics, indent=2), encoding='utf-8')

# Labels are rendered independently, so ffmpeg does not depend on fontconfig.
font = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 24)
labels = ['BEFORE - old dual phase', 'AFTER - coherent authored marks', 'REFERENCE - surface pattern']
for i, label in enumerate(labels):
    im = Image.new('RGB', (640, 44), '#101921')
    ImageDraw.Draw(im).text((16,8), label, font=font, fill='white')
    im.save(root/f'Label{i}.png')

reference = root.parents[1]/'References/REF_Style_WaterfallRiver.mp4'
filters = ('[0:v]crop=872:174:44:153,scale=640:128,pad=640:360:0:116:color=0x101921[a];'
           '[1:v]crop=872:174:44:153,scale=640:128,pad=640:360:0:116:color=0x101921[b];'
           '[2:v]crop=650:360:100:650,scale=640:354,pad=640:360:0:3:color=0x101921[c];'
           '[3:v][a]vstack[aa];[4:v][b]vstack[bb];[5:v][c]vstack[cc];'
           '[aa][bb][cc]hstack=inputs=3[out]')
run('-i',root/'F_BaselineStraight.mp4','-i',root/'G_StraightGateMotion.mp4','-i',reference,
    '-loop',1,'-i',root/'Label0.png','-loop',1,'-i',root/'Label1.png','-loop',1,'-i',root/'Label2.png',
    '-filter_complex',filters,'-map','[out]','-t',12,'-r',30,'-c:v','libopenh264','-b:v','6M',
    '-pix_fmt','yuv420p','-an','-y',root/'Before_After_Reference.mp4')
print(json.dumps(metrics, indent=2))

