"""Generate original non-vocal cartoon foley; never synthesize character voices."""
import math, random, wave, struct
from pathlib import Path
RATE=44100
ROOT=Path(__file__).resolve().parents[1]/'sounds'/'actions'
ROOT.mkdir(parents=True,exist_ok=True)
def render(name,duration,events):
    samples=[0.0]*int(duration*RATE)
    rng=random.Random(name)
    for start,length,f0,f1,volume,noise in events:
        phase=0.0; filtered=0.0
        for j in range(int(length*RATE)):
            i=int(start*RATE)+j
            if i>=len(samples): break
            u=j/(length*RATE); phase+=2*math.pi*(f0+(f1-f0)*u)/RATE
            filtered=.65*filtered+.35*rng.uniform(-1,1)
            env=math.sin(math.pi*min(1,u*12)) if u<1/12 else (1-u)**2
            samples[i]+=volume*env*((1-noise)*math.sin(phase)+noise*filtered)
    with wave.open(str(ROOT/(name+'.wav')),'wb') as w:
        w.setparams((1,2,RATE,0,'NONE','not compressed'))
        w.writeframes(b''.join(struct.pack('<h',int(max(-.8,min(.8,v))*32767)) for v in samples))
render('hop',.24,[(.015,.20,230,700,.13,.04)])
render('heart',.65,[(.01,.28,660,660,.08,0),(.15,.35,880,880,.065,0),(.29,.30,1108,1108,.05,0)])
render('burst',.82,[(i*.075,.24,400+i*135,800+i*110,.09,0.04) for i in range(7)])
render('step-left',.075,[(.005,.060,155,85,.055,.75)])
render('step-right',.075,[(.005,.060,175,105,.05,.75)])
render('sleep',1.4,[(.05,.62,70,58,.032,1),(.78,.56,58,65,.024,1)])
render('bite',.15,[(.005,.07,500,230,.055,.94),(.06,.06,340,190,.035,.92)])
render('trip',.28,[(.01,.18,125,50,.12,.65),(.10,.15,200,90,.025,.9)])
render('held',.42,[(.01,.15,180,220,.04,.97),(.23,.15,220,180,.04,.97)])
render('greet',.43,[(.01,.18,520,660,.065,.02),(.17,.22,780,780,.055,.02)])
render('glide',.65,[(.01,.55,180,520,.05,.90)])
render('sip',.58,[(.01,.30,260,450,.045,.88),(.35,.15,1200,950,.035,.05)])
render('sword',.4,[(.01,.22,450,1350,.05,.94),(.19,.18,1450,1350,.065,.05)])
render('serve',.45,[(.01,.16,950,900,.04,.03),(.17,.20,1350,1250,.04,.03)])
render('eat',2.75,[(.1+i*.29,.105,420,180,.055,.93) for i in range(8)])
