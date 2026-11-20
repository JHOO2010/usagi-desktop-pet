using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

partial class Usagi {
    class Track {public double X,Y,Age;public float Scale;public int Side;}
    readonly System.Collections.Generic.List<Track> tracks=new System.Collections.Generic.List<Track>();
    double trackDistance,tripCooldown=95,strokeTime,strokeCooldown,gazeTilt,wiggleTime,lastWiggle,saveIn=10,carrotX;
    int wiggleDirection;bool carrotActive;string morningSeen="";
    string preferencesPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"UsagiPet","settings.txt");
    bool preferencesEnabled=Path.GetFileName(Application.ExecutablePath)=="우사기.exe";
    bool SaveEnabled {get{return preferencesEnabled;}}
    static bool IsNight(DateTime time){return time.Hour>=22||time.Hour<6;}
    void StartDayGreeting(DateTime now){
        if(!SaveEnabled)return;
        string today=now.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture);
        if(now.Hour>=6&&now.Hour<11&&morningSeen!=today){morningSeen=today;Change(Mood.Stretch);Say("야하~!");SavePreferences();}
        else if(IsNight(now)){Change(Mood.Drowsy);Say("후우웅…");}
    }
    void LoadPreferences(){if(!SaveEnabled)return;try{
        string screen="";double ratio=.8;foreach(string line in File.ReadAllLines(preferencesPath)){
            int equal=line.IndexOf('=');if(equal<0)continue;string key=line.Substring(0,equal),value=line.Substring(equal+1);float size;double fraction;
            if(key=="scale"&&float.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out size)&&size>=.75&&size<=1.4)scale=size;
            if(key=="sound")sounds.Enabled=value!="off";
            if(key=="screen")screen=value;
            if(key=="position"&&double.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out fraction)&&!double.IsNaN(fraction)&&!double.IsInfinity(fraction))ratio=Math.Max(.05,Math.Min(.95,fraction));
            if(key=="morning")morningSeen=value;
        }
        Screen selected=Screen.PrimaryScreen;foreach(var candidate in Screen.AllScreens)if(candidate.DeviceName==screen)selected=candidate;
        var area=selected.WorkingArea;x=area.Left+area.Width*ratio-Width/2;y=area.Bottom-Height+8;
    }catch(IOException){}catch(UnauthorizedAccessException){}}
    void SavePreferences(){if(!SaveEnabled)return;try{
        var screen=Screen.FromPoint(new System.Drawing.Point((int)x+Width/2,(int)y+Height-15));var area=screen.WorkingArea;
        double ratio=Math.Max(.05,Math.Min(.95,(x+Width/2-area.Left)/area.Width));
        Directory.CreateDirectory(Path.GetDirectoryName(preferencesPath));string temp=preferencesPath+".tmp";
        File.WriteAllLines(temp,new[]{"scale="+scale.ToString(CultureInfo.InvariantCulture),"sound="+(sounds.Enabled?"on":"off"),"screen="+screen.DeviceName,"position="+ratio.ToString(CultureInfo.InvariantCulture),"morning="+morningSeen});
        if(File.Exists(preferencesPath))File.Replace(temp,preferencesPath,null);else File.Move(temp,preferencesPath);
    }catch(IOException){}catch(UnauthorizedAccessException){}}
    void OfferCarrot(){
        var area=Screen.FromPoint(new System.Drawing.Point((int)x+Width/2,(int)y+Height-15)).WorkingArea;
        double center=x+Width/2;int side=center<area.Left+area.Width/2?1:-1;
        carrotX=Math.Max(area.Left+45,Math.Min(area.Right-45,center+side*145*scale));carrotActive=true;paused=false;pendingClicks=0;idleTime=0;hopHeight=0;speed=0;
        Change(Mood.Walk);direction=carrotX>center?1:-1;targetSpeed=112;Say("우라!");
    }
    void UpdateDetails(double dt,System.Drawing.Point cursor,int mx,int my){
        saveIn-=dt;if(saveIn<=0){SavePreferences();saveIn=10;}
        tripCooldown-=dt;strokeCooldown=Math.Max(0,strokeCooldown-dt);
        for(int i=tracks.Count-1;i>=0;i--){tracks[i].Age+=dt;if(tracks[i].Age>1.1)tracks.RemoveAt(i);}
        double dx=cursor.X-(x+Width/2),localY=cursor.Y-y;
        bool near=Math.Abs(dx)<125*scale&&localY>petTop-30*scale&&localY<Height;
        gazeTilt+=(Math.Max(-7,Math.Min(7,dx/13))-gazeTilt)*(1-Math.Exp(-dt/.18));
        if(pointerDown||dragging||paused||menu.Visible||carrotActive||mood==Mood.Hop||mood==Mood.Fall||mood==Mood.Sleep||mood==Mood.Drowsy||mood==Mood.Trip||mood==Mood.Recover)return;
        double travel=Math.Sqrt(mx*mx+my*my);
        bool head=Math.Abs(dx)<40*scale&&localY>petTop+30*scale&&localY<Height-43*scale;
        if(head&&travel>.2&&travel<Math.Max(4,dt*1100)&&strokeCooldown<=0){strokeTime+=dt;if(strokeTime>.5){strokeTime=0;strokeCooldown=4;speed=0;Change(Mood.Nuzzle);EmitHearts(false);Say("후웅~");}}
        else strokeTime=Math.Max(0,strokeTime-dt*.6);
        if(mood==Mood.Nuzzle)return;
        if(near&&travel>1){int sign=mx>0?1:mx<0?-1:0;if(sign!=0&&sign!=wiggleDirection){wiggleDirection=sign;if(t-lastWiggle<.65)wiggleTime+=.5;else wiggleTime=.5;lastWiggle=t;}
            if(curiosityCooldown<=0){Settle(Mood.Look);curiosityCooldown=3;direction=dx<0?-1:1;if(wiggleTime>=1.5){Say("하아?");gazeTilt=dx<0?-12:12;wiggleTime=0;}}
        }
    }
    bool UpdateSpecial(double dt){
        if(carrotActive&&mood!=Mood.Eat){idleTime=0;double distance=carrotX-(x+Width/2);direction=distance<0?-1:1;targetSpeed=112;decisionTime=100;autoHopIn=100;
            if(Math.Abs(distance)<19*scale){speed=0;moving=false;Change(Mood.Eat);Say("푸루루루~");}}
        if(mood==Mood.Eat){idleTime=0;moving=false;speed=0;if(stateTime>=3.2){carrotActive=false;EmitHearts(false);Say("야하!");Change(Mood.Look);}return true;}
        if(mood==Mood.Nuzzle){idleTime=0;moving=false;speed=0;if(stateTime>1.8){Change(Mood.Look);decisionTime=2;}return true;}
        if(mood==Mood.Trip){moving=false;speed=0;if(stateTime>.65){Change(Mood.Recover);Say("…하아?");}return true;}
        if(mood==Mood.Recover){moving=false;if(stateTime>1.1){Change(Mood.Walk);targetSpeed=50;decisionTime=2;}return true;}
        return false;
    }
    void BeginTrip(){speed=0;tripCooldown=90+rng.NextDouble()*100;Change(Mood.Trip);speechTime=0;}
    void LeaveTracks(double distance){trackDistance+=distance;if(trackDistance<11*scale)return;trackDistance=0;
        tracks.Add(new Track{X=x+Width/2-direction*12*scale,Y=y+Height-10,Scale=scale,Side=tracks.Count%2});if(tracks.Count>24)tracks.RemoveAt(0);
    }
    void DrawDetails(Graphics g){
        foreach(var track in tracks){float alpha=(float)(1-track.Age/1.1);using(var ink=new SolidBrush(Color.FromArgb((int)(65*alpha),116,83,57))){float px=(float)(track.X-x),py=(float)(track.Y-y)-track.Side*2*track.Scale;g.FillEllipse(ink,px-3*track.Scale,py-2*track.Scale,6*track.Scale,2.5f*track.Scale);}}
        if(carrotActive&&mood!=Mood.Eat)DrawCarrot(g,(float)(carrotX-x),Height-17,scale,1);
        if(mood==Mood.Eat){using(var ink=new SolidBrush(Color.FromArgb(170,225,139,56)))for(int i=0;i<3;i++){float phase=(float)((stateTime*3+i*.3)%1);g.FillEllipse(ink,Width/2+(i-1)*15*scale*phase,Height-34*scale-phase*12*scale,2*scale,2*scale);}}
    }
    static void DrawCarrot(Graphics g,float cx,float cy,float size,float remaining){if(remaining<=0)return;var saved=g.Save();g.TranslateTransform(cx,cy);g.ScaleTransform(size,size);g.RotateTransform(-25);
        using(var carrot=new GraphicsPath()){carrot.AddBezier(-7,-16,-5,-22,6,-22,7,-16);carrot.AddLine(7,-16,0,5);carrot.CloseFigure();using(var orange=new SolidBrush(Color.FromArgb(245,153,61)))g.FillPath(orange,carrot);using(var outline=new Pen(Color.FromArgb(116,74,35),1.4f))g.DrawPath(outline,carrot);}
        using(var leaf=new Pen(Color.FromArgb(83,146,70),3)){leaf.StartCap=leaf.EndCap=LineCap.Round;g.DrawLine(leaf,0,-20,-6,-29);g.DrawLine(leaf,0,-20,1,-31);g.DrawLine(leaf,0,-20,7,-27);}
        if(remaining<.7){using(var cream=new SolidBrush(Color.FromArgb(255,253,241)))g.FillEllipse(cream,-5,1-10*(1-remaining),13,10);}
        g.Restore(saved);
    }
}
