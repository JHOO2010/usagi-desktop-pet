using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

partial class Usagi {
    [System.Runtime.InteropServices.DllImport("gdi32.dll")]static extern uint GetPixel(IntPtr dc,int x,int y);
    double sleepInkPoll;bool lightSleepInk;
    class Track {public double X,Y,Age;public float Scale;public int Side;}
    readonly System.Collections.Generic.List<Track> tracks=new System.Collections.Generic.List<Track>();
    double trackDistance,tripCooldown=95,strokeTime,strokeCooldown,gazeTilt,wiggleTime,lastWiggle,saveIn=10,foodX;
    int wiggleDirection;bool foodActive;string morningSeen="";
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
    void OfferFood(){CancelSocial();scatterActive=false;
        var area=Screen.FromPoint(new System.Drawing.Point((int)x+Width/2,(int)y+Height-15)).WorkingArea;
        double center=x+Width/2;int side=center<area.Left+area.Width/2?1:-1;
        foodX=Math.Max(area.Left+45,Math.Min(area.Right-45,center+side*145*scale));foodActive=true;paused=false;pendingClicks=0;idleTime=0;hopHeight=0;speed=0;
        Change(Mood.Walk);direction=foodX>center?1:-1;targetSpeed=112;Say("우라!");
    }
    void UpdateDetails(double dt,System.Drawing.Point cursor,int mx,int my){
        if(mood==Mood.Sleep){sleepInkPoll-=dt;if(sleepInkPoll<=0){UpdateSleepInk();sleepInkPoll=.6;}}else sleepInkPoll=0;
        UpdateCloneQueue(dt);
        saveIn-=dt;if(saveIn<=0){SavePreferences();saveIn=10;}
        tripCooldown-=dt;strokeCooldown=Math.Max(0,strokeCooldown-dt);
        for(int i=tracks.Count-1;i>=0;i--){tracks[i].Age+=dt;if(tracks[i].Age>1.1)tracks.RemoveAt(i);}
        double dx=cursor.X-(x+Width/2),localY=cursor.Y-y;
        bool near=Math.Abs(dx)<125*scale&&localY>petTop-30*scale&&localY<Height;
        gazeTilt+=(Math.Max(-7,Math.Min(7,dx/13))-gazeTilt)*(1-Math.Exp(-dt/.18));
        if(scatterActive||pointerDown||dragging||paused||menu.Visible||foodActive||mood==Mood.Hop||mood==Mood.Fall||mood==Mood.Sleep||mood==Mood.Drowsy||mood==Mood.Trip||mood==Mood.Recover)return;
        double travel=Math.Sqrt(mx*mx+my*my);
        bool head=Math.Abs(dx)<40*scale&&localY>petTop+30*scale&&localY<Height-43*scale;
        if(head&&travel>.2&&travel<Math.Max(4,dt*1100)&&strokeCooldown<=0){strokeTime+=dt;if(strokeTime>.5){strokeTime=0;strokeCooldown=4;speed=0;Change(Mood.Nuzzle);EmitHearts(false);Say("후웅~");}}
        else strokeTime=Math.Max(0,strokeTime-dt*.6);
        if(mood==Mood.Nuzzle)return;
        if(near&&travel>1){int sign=mx>0?1:mx<0?-1:0;if(sign!=0&&sign!=wiggleDirection){wiggleDirection=sign;if(t-lastWiggle<.65)wiggleTime+=.5;else wiggleTime=.5;lastWiggle=t;}
            if(curiosityCooldown<=0){Settle(Mood.Look);curiosityCooldown=3;direction=dx<0?-1:1;if(wiggleTime>=1.5){AutoSay("하아?");gazeTilt=dx<0?-12:12;wiggleTime=0;}}
        }
    }
    bool UpdateSpecial(double dt){
        if(UpdateScatter(dt))return true;
        if(foodActive&&mood!=Mood.Eat){idleTime=0;double distance=foodX-(x+Width/2);direction=distance<0?-1:1;targetSpeed=38+74*Math.Min(1,Math.Abs(distance)/(90*scale));decisionTime=100;autoHopIn=100;
            if(Math.Abs(distance)<19*scale){speed=0;moving=false;Change(Mood.Eat);Say("푸루루루~");}}
        if(mood==Mood.Eat){idleTime=0;moving=false;speed=0;if(stateTime>=5.3){foodActive=false;EmitHearts(false);Say("야하!");Change(Mood.Look);}return true;}
        if(mood==Mood.Nuzzle){idleTime=0;moving=false;speed=0;if(stateTime>1.8){Change(Mood.Look);decisionTime=2;}return true;}
        if(mood==Mood.Trip){moving=false;speed=0;if(stateTime>.65){Change(Mood.Recover);AutoSay("…하아?");}return true;}
        if(mood==Mood.Recover){moving=false;if(stateTime>1.1){Change(Mood.Walk);targetSpeed=50;decisionTime=2;}return true;}
        return false;
    }
    void BeginTrip(){speed=0;tripCooldown=90+rng.NextDouble()*100;Change(Mood.Trip);speechTime=0;}
    void LeaveTracks(double distance){trackDistance+=distance;if(trackDistance<11*scale)return;trackDistance=0;
        tracks.Add(new Track{X=x+Width/2-direction*12*scale,Y=y+Height-10,Scale=scale,Side=tracks.Count%2});if(tracks.Count>24)tracks.RemoveAt(0);
    }
    void DrawDetails(Graphics g){
        foreach(var track in tracks){float alpha=(float)(1-track.Age/1.1);using(var ink=new SolidBrush(Color.FromArgb((int)(65*alpha),116,83,57))){float px=(float)(track.X-x),py=(float)(track.Y-y)-track.Side*2*track.Scale;g.FillEllipse(ink,px-3*track.Scale,py-2*track.Scale,6*track.Scale,2.5f*track.Scale);}}
        if(foodActive&&mood!=Mood.Eat)DrawPizzaBun(g,(float)(foodX-x),Height-17,scale,1);

    }
    static readonly System.Collections.Generic.List<Usagi> livePets=new System.Collections.Generic.List<Usagi>();
    bool isClone,scatterActive;int clonesPending,cloneSequence;double cloneDelay,scatterVX,scatterVY,scatterTime;
    static readonly float[] cloneSizes={.63f,1.12f,.82f,1.32f};
    void RefreshCloneMenu(){bool any=false;foreach(var pet in livePets)if(pet.isClone&&!pet.IsDisposed){any=true;break;}
        if(any&&!menu.Items.Contains(cleanupMenu)){cleanupMenu.AutoSize=false;cleanupMenu.Size=new Size(220,33);menu.Items.Insert(6,cleanupMenu);}
        else if(!any&&menu.Items.Contains(cleanupMenu))menu.Items.Remove(cleanupMenu);
    }
    void RefreshMonitorMenu(){if(Screen.AllScreens.Length>1){if(!menu.Items.Contains(monitorMenu)){monitorMenu.AutoSize=false;monitorMenu.Size=new Size(220,33);menu.Items.Insert(menu.Items.Contains(cleanupMenu)?7:6,monitorMenu);}}else if(menu.Items.Contains(monitorMenu))menu.Items.Remove(monitorMenu);}
    void DuplicatePets(){int available=Math.Max(0,16-livePets.Count-clonesPending);clonesPending+=Math.Min(4,available);cloneSequence=0;cloneDelay=0;Say("야하아!!");}
    void ClearClones(){foreach(var pet in livePets.ToArray()){pet.clonesPending=0;if(pet.isClone&&!pet.IsDisposed)pet.Close();}clonesPending=0;}
    void UpdateCloneQueue(double dt){if(clonesPending<=0||IsDisposed)return;cloneDelay-=dt;if(cloneDelay>0)return;
        if(livePets.Count>=16){clonesPending=0;return;}cloneDelay=.11;clonesPending--;
        var pet=new Usagi(true,this);pet.scale=cloneSizes[cloneSequence%4];pet.sounds.Enabled=sounds.Enabled;
        pet.x=x;pet.y=y;pet.idleTime=0;pet.speechTime=0;pet.nextSpeechTime=7+rng.NextDouble()*3;
        int slot=cloneSequence++%4;pet.scatterVX=new[]{-225.0,140.0,-120.0,245.0}[slot];pet.scatterVY=-235-slot%2*45;pet.scatterActive=true;pet.scatterTime=0;pet.Change(Mood.Fall);
        pet.direction=pet.scatterVX>0?1:-1;pet.targetSpeed=44+rng.NextDouble()*45;pet.decisionTime=1+rng.NextDouble()*2;
        livePets.Add(pet);pet.Show();pet.Render();
    }
    bool UpdateScatter(double dt){if(!scatterActive)return false;
        scatterTime+=dt;scatterVY+=640*dt;x+=scatterVX*dt;y+=scatterVY*dt;scatterVX*=Math.Exp(-dt*.6);
        var area=Screen.FromPoint(new System.Drawing.Point((int)x+Width/2,(int)y+Height-15)).WorkingArea;
        double min=area.Left-Width/2+40*scale,max=area.Right-Width/2-40*scale;
        if(x<min||x>max){x=Math.Max(min,Math.Min(max,x));scatterVX=-scatterVX*.65;}
        double floor=area.Bottom-Height+8;
        if(y>=floor&&scatterVY>0){y=floor;scatterActive=false;scatterVX=scatterVY=0;landing=.26;Change(Mood.Sit);decisionTime=.6+rng.NextDouble();}
        moving=false;idleTime=0;return true;
    }
    static double BackgroundLuminance(Color color){return (.2126*color.R+.7152*color.G+.0722*color.B)/255;}
    static bool NeedsLightSleepInk(double luminance,bool previous){return previous?luminance<.6:luminance<.46;}
    void UpdateSleepInk(){
        float factor=68*scale/headWidths[poses[1]],w=poses[1].Width*factor,h=poses[1].Height*factor;
        int sx=(int)(x+Width/2+w/2-4),sy=(int)(y+Height-h-20);double total=0;int count=0;var dc=GetDC(IntPtr.Zero);
        try{foreach(var offset in new[]{new System.Drawing.Point(0,-12),new System.Drawing.Point(20,-12),new System.Drawing.Point(48,0),new System.Drawing.Point(48,15),new System.Drawing.Point(48,30)}){
            var point=new System.Drawing.Point(sx+(int)(offset.X*scale),sy+(int)(offset.Y*scale));if(!SystemInformation.VirtualScreen.Contains(point))continue;
            uint pixel=GetPixel(dc,point.X,point.Y);if(pixel==0xffffffff)continue;total+=BackgroundLuminance(Color.FromArgb((int)(pixel&255),(int)((pixel>>8)&255),(int)((pixel>>16)&255)));count++;
        }}finally{ReleaseDC(IntPtr.Zero,dc);}if(count>0)lightSleepInk=NeedsLightSleepInk(total/count,lightSleepInk);
    }
    void DrawSleepText(Graphics g,float width,float height){
        var point=new PointF(Width/2+width/2-4,Height-height-20-(float)Math.Sin(t*1.5)*3);
        using(var path=new GraphicsPath())using(var format=new StringFormat()){
            path.AddString("z Z",petFonts.Families[0],(int)FontStyle.Regular,13*g.DpiY/72,point,format);
            using(var edge=new Pen(lightSleepInk?Color.FromArgb(155,73,51,36):Color.FromArgb(180,255,253,235),2)){edge.LineJoin=LineJoin.Round;g.DrawPath(edge,path);}
            using(var ink=new SolidBrush(lightSleepInk?Color.FromArgb(255,251,226):Color.FromArgb(89,57,34)))g.FillPath(ink,path);
        }
    }
    // Four deliberate bites, with a brief approach, chew, and swallow for each one.
    static int FoodBites(double time){int bites=0;foreach(double bite in new[]{.95,1.8,2.65,3.5})if(time>=bite)bites++;return bites;}
    static double ChewPulse(double time){if(time<.7||time>4.6)return 0;double phase=(time-.7)% .85;return Math.Pow(Math.Max(0,Math.Sin(phase/.85*Math.PI*4)),2);}
    static float FoodRemaining(double time){double eaten=0;foreach(double bite in new[]{.95,1.8,2.65,3.5}){double u=Math.Max(0,Math.Min(1,(time-bite)/.18));eaten+=.225*u*u*(3-2*u);}eaten+=.1*Math.Max(0,Math.Min(1,(time-3.9)/.25));return (float)Math.Max(0,1-eaten);}
    void DrawEating(Graphics g,float bottom){
        double time=stateTime;float chew=(float)ChewPulse(time);int bites=FoodBites(time);
        float mouthX=Width/2+2*scale,mouthY=bottom-40*scale;
        // The bun follows the same body transform, so it never slides off the mouth.
        float lift=(float)Math.Max(0,Math.Min(1,time/.65));lift=lift*lift*(3-2*lift);
        float cx=(float)(foodX-x)+(mouthX-(float)(foodX-x))*lift;
        if(time<4.15){float remaining=FoodRemaining(time),cut=-20+25*(1-remaining);
            float heldY=mouthY-cut*.9f*scale;float baseY=(Height-17)+(heldY-(Height-17))*lift;
            // The bitten upper edge stays at the lips; the rest advances into the mouth.
            var foodLayer=g.Save();if(lift>.99)g.SetClip(new RectangleF(Width/2-40*scale,mouthY-.5f*scale,80*scale,Height-mouthY+20),CombineMode.Intersect);
            DrawPizzaBun(g,cx-2*scale*lift,baseY-chew*.7f*scale,.9f*scale,remaining,0);g.Restore(foodLayer);
        }
        if(time>.7&&time<4.6){using(var cheek=new SolidBrush(Color.FromArgb((int)(28+chew*38),245,145,148))){g.FillEllipse(cheek,Width/2-24*scale,mouthY-5*scale,(8+chew*2)*scale,5*scale);g.FillEllipse(cheek,Width/2+15*scale,mouthY-5*scale,(8+chew*2)*scale,5*scale);}
        }
        foreach(double bite in new[]{.95,1.8,2.65,3.5}){double age=time-bite;if(age<0||age>.42)continue;
            for(int i=0;i<3;i++){float px=mouthX+(float)((i-1)*18*age)*scale,py=mouthY+(float)(-22*age+90*age*age)*scale;var saved=g.Save();g.TranslateTransform(px,py);g.RotateTransform((float)(age*220+i*35));using(var crumb=new SolidBrush(Color.FromArgb((int)(220*(1-age/.42)),239,150,58)))g.FillRectangle(crumb,-1.2f*scale,-.8f*scale,2.4f*scale,1.6f*scale);g.Restore(saved);}}
        if(time>=4.25&&time<4.65){using(var crumb=new SolidBrush(Color.FromArgb((int)(160*(1-(time-4.25)/.4)),235,191,110)))g.FillEllipse(crumb,mouthX+4*scale,mouthY+8*scale,2*scale,2*scale);}
    }
    static void DrawPizzaBun(Graphics g,float cx,float cy,float size,float remaining){DrawPizzaBun(g,cx,cy,size,remaining,0);}
    static void DrawPizzaBun(Graphics g,float cx,float cy,float size,float remaining,float rotation){if(remaining<=0)return;var saved=g.Save();g.TranslateTransform(cx,cy);g.ScaleTransform(size,size);g.RotateTransform(rotation);
        using(var bun=new GraphicsPath()){
            bun.AddBezier(-12,0,-15,-9,-9,-19,0,-19);bun.AddBezier(0,-19,9,-19,15,-9,12,0);bun.AddBezier(12,0,8,6,-8,6,-12,0);bun.CloseFigure();
            float cut=-20+25*(1-remaining);using(var region=new Region(bun)){
                if(remaining<.99){region.Intersect(new RectangleF(-15,cut,30,25-cut));using(var bite=new GraphicsPath()){bite.AddEllipse(-8,cut-2,8,4);bite.AddEllipse(0,cut-1,8,4);region.Exclude(bite);}}
                g.SetClip(region,CombineMode.Intersect);
                using(var bread=new LinearGradientBrush(new PointF(-8,-18),new PointF(8,5),Color.FromArgb(255,240,189),Color.FromArgb(234,188,106)))g.FillPath(bread,bun);
                using(var outline=new Pen(Color.FromArgb(128,88,45),1.3f))g.DrawPath(outline,bun);
                using(var pleat=new Pen(Color.FromArgb(201,156,86),1.1f)){g.DrawBezier(pleat,-7,-13,-4,-12,-3,-16,0,-17);g.DrawBezier(pleat,7,-13,4,-12,3,-16,0,-17);g.DrawLine(pleat,0,-17,0,-12);}
                if(remaining<.99){using(var filling=new SolidBrush(Color.FromArgb(238,148,50)))g.FillEllipse(filling,-8,cut-1,16,4);using(var cheese=new Pen(Color.FromArgb(255,232,144),1.2f))g.DrawBezier(cheese,-6,cut+1,-2,cut+3,1,cut-1,6,cut+1);using(var tomato=new SolidBrush(Color.FromArgb(197,75,48))){g.FillEllipse(tomato,-4,cut+1,2.5f,1.5f);g.FillEllipse(tomato,3,cut,2,1.5f);}}
            }
        }g.Restore(saved);
    }
}
