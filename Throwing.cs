using System;
using System.Drawing;
using System.Collections.Generic;

partial class Usagi {
    struct DragSample {internal Point Point;internal double Time;}
    readonly List<DragSample> throwSamples=new List<DragSample>();
    bool throwActive;double throwVX,throwVY,throwAge,throwLean;int throwBounces;

    void ResetThrowGesture(Point point,double now){monitorEntry=false;throwActive=false;throwVX=throwVY=throwLean=0;throwSamples.Clear();RecordThrowSample(point,now);}
    void RecordThrowSample(Point point,double now){
        if(throwSamples.Count>0&&now<throwSamples[throwSamples.Count-1].Time)throwSamples.Clear();
        while(throwSamples.Count>0&&(now-throwSamples[0].Time>.11||throwSamples.Count>=64))throwSamples.RemoveAt(0);
        if(throwSamples.Count>0&&now-throwSamples[throwSamples.Count-1].Time<.001)return;
        throwSamples.Add(new DragSample{Point=point,Time=now});
    }
    bool StartThrowFromGesture(double now){
        if(throwSamples.Count<2||now-throwSamples[throwSamples.Count-1].Time>.08)return false;
        while(throwSamples.Count>0&&now-throwSamples[0].Time>.12)throwSamples.RemoveAt(0);
        if(throwSamples.Count<2)return false;
        var first=throwSamples[0];var last=throwSamples[throwSamples.Count-1];
        if(last.Time-first.Time<.012||Math.Abs(last.Point.X-first.Point.X)+Math.Abs(last.Point.Y-first.Point.Y)<12)return false;
        double mt=0,mx=0,my=0;foreach(var sample in throwSamples){mt+=sample.Time-first.Time;mx+=sample.Point.X;my+=sample.Point.Y;}
        mt/=throwSamples.Count;mx/=throwSamples.Count;my/=throwSamples.Count;
        double variance=0,vx=0,vy=0;foreach(var sample in throwSamples){double time=sample.Time-first.Time-mt;variance+=time*time;vx+=time*(sample.Point.X-mx);vy+=time*(sample.Point.Y-my);}
        if(variance<.000001)return false;vx/=variance;vy/=variance;
        double magnitude=Math.Sqrt(vx*vx+vy*vy);if(magnitude<220)return false;
        double limit=Math.Min(1,1500/magnitude);throwVX=vx*limit;throwVY=vy*limit;
        throwActive=true;throwAge=0;throwBounces=0;hopHeight=0;speed=0;moving=false;idleTime=0;fallVelocity=0;y=Math.Min(y,PatrolArea().Bottom-Height+8);
        if(Math.Abs(throwVX)>20)direction=throwVX>0?1:-1;Change(Mood.Fall);return true;
    }
    bool UpdateThrow(double dt){
        if(!throwActive){throwLean*=Math.Exp(-dt/.075);if(Math.Abs(throwLean)<.03)throwLean=0;return false;}
        throwAge+=dt;var area=PatrolArea();double floor=area.Bottom-Height+8;var screens=System.Windows.Forms.Screen.AllScreens;var bounds=MonitorBounds(screens,false);int current=MonitorIndex(screens);
        double inset=(64+Math.Abs(throwLean)*.7)*scale;
        moving=false;idleTime=0;
        int steps=Math.Max(1,(int)Math.Ceiling(dt*240));double h=dt/steps;
        for(int i=0;i<steps;i++){
            area=screens[current].WorkingArea;floor=area.Bottom-Height+8;
            double left=area.Left-Width/2+inset,right=area.Right-Width/2-inset,ceiling=Math.Min(floor,area.Top-petTop+8);
            double centerX=x+Width/2,centerY=y+Height-55*scale;
            bool openLeft=MonitorLayout.Passage(bounds,current,0,centerY)>=0,openRight=MonitorLayout.Passage(bounds,current,1,centerY)>=0;
            bool openTop=MonitorLayout.Passage(bounds,current,2,centerX)>=0,openBottom=MonitorLayout.Passage(bounds,current,3,centerX)>=0;
            bool grounded=!openBottom&&y>=floor-.001&&Math.Abs(throwVY)<.001;
            double damping=Math.Exp(-.38*h);x+=throwVX*(1-damping)/.38;throwVX*=damping;
            if(!grounded){y+=throwVY*h+550*h*h;throwVY+=1100*h;}
            else throwVX=Approach(throwVX,0,800*h);
            int next=MonitorLayout.CrossFlight(bounds,current,x+Width/2,y+Height-55*scale);
            if(next>=0){current=next;EnterMonitor(screens[next],false);continue;}
            if(x<left&&!openLeft){x=left;throwVX=Math.Abs(throwVX)*.42;}else if(x>right&&!openRight){x=right;throwVX=-Math.Abs(throwVX)*.42;}
            if(y<ceiling&&!openTop){y=ceiling;throwVY=Math.Abs(throwVY)*.35;}
            if(y>=floor&&throwVY>=0&&!openBottom){
                y=floor;if(!grounded)landing=.22;
                if(throwVY>180&&throwBounces<1){throwVY=-Math.Min(220,throwVY*.28);throwVX*=.7;throwBounces++;}
                else throwVY=0;
            }
            double target=Math.Max(-26,Math.Min(26,throwVX/32));if(!openBottom&&y>=floor-.001&&throwVY==0)target*=.45;
            throwLean+=(target-throwLean)*(1-Math.Exp(-h/.075));
        }
        floor=PatrolArea().Bottom-Height+8;
        if(Math.Abs(throwVX)>20)direction=throwVX>0?1:-1;
        if(y>=floor-.001&&throwVY==0&&(Math.Abs(throwVX)<12||throwAge>5)){
            throwActive=false;throwVX=throwVY=0;y=floor;speed=0;landing=.22;tripCooldown=Math.Max(tripCooldown,3);Change(Mood.Sit);SavePreferences();
        }
        return true;
    }
}
