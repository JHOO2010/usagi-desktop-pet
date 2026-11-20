using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

class BubbleMenu : ContextMenuStrip {
    static readonly System.Windows.Forms.Layout.LayoutEngine bubbleLayout=new BubbleLayout();
    public override System.Windows.Forms.Layout.LayoutEngine LayoutEngine {get{return bubbleLayout;}}
    class BubbleLayout : System.Windows.Forms.Layout.LayoutEngine {public override bool Layout(object container,LayoutEventArgs e){var menu=(BubbleMenu)container;for(int i=0;i<menu.Items.Count;i++){var bounds=new Rectangle(10,9+i*33,Math.Max(1,menu.Width-20),33);var action=menu.Items[i] as BubbleMenuItem;if(action!=null)action.Arrange(bounds);var choice=menu.Items[i] as SizeChoiceItem;if(choice!=null)choice.Arrange(bounds);}return false;}}
    public int BodyWidth=220;
    public System.Drawing.Point? PetAnchor; public int TailX=-1;
    public BubbleMenu(){ShowImageMargin=false;ShowCheckMargin=false;LayoutStyle=ToolStripLayoutStyle.VerticalStackWithOverflow;Padding=new Padding(10,9,10,22);BackColor=Color.FromArgb(255,253,241);ForeColor=Color.FromArgb(89,57,34);Renderer=new BubbleMenuRenderer();DropShadowEnabled=false;}
    protected override void OnOpening(System.ComponentModel.CancelEventArgs e){AutoSize=false;Size=new Size(BodyWidth+20,Items.Count*33+34);base.OnOpening(e);Size=new Size(BodyWidth+20,Items.Count*33+34);PerformLayout();}
    protected override void OnOpened(EventArgs e){if(PetAnchor.HasValue){var point=PetAnchor.Value;var area=Screen.FromPoint(point).WorkingArea;Location=new System.Drawing.Point(Math.Max(area.Left+4,Math.Min(area.Right-Width-4,point.X-Width/2)),Math.Max(area.Top+4,Math.Min(area.Bottom-Height-4,point.Y-Height)));TailX=Math.Max(24,Math.Min(Width-24,point.X-Left));RefreshShape();}base.OnOpened(e);}
    void RefreshShape(){using(var path=Outline(Width,Height,TailX)){var old=Region;Region=new Region(path);if(old!=null)old.Dispose();}Invalidate();}
    internal static GraphicsPath Outline(int width,int height,int tail=-1){var path=new GraphicsPath();float r=18,b=height-14;float tip=tail<0?width/2:tail;
        path.AddArc(1,1,r,r,180,90);path.AddArc(width-r-2,1,r,r,270,90);path.AddArc(width-r-2,b-r,r,r,0,90);
        path.AddLine(width-r/2-2,b,tip+10,b);path.AddLine(tip+10,b,tip,height-2);path.AddLine(tip,height-2,tip-9,b);path.AddLine(tip-9,b,r/2+1,b);path.AddArc(1,b-r,r,r,90,90);path.CloseFigure();return path;
    }
    protected override void OnSizeChanged(EventArgs e){base.OnSizeChanged(e);if(Width<30||Height<30)return;RefreshShape();}
}
class BubbleMenuItem : ToolStripMenuItem {public BubbleMenuItem(string text):base(text){AutoSize=false;}public void Arrange(Rectangle bounds){
    // Menu items subtract the dropdown's internal padding in SetBounds; compensate to keep the requested visible inset.
    if(Owner is ToolStripDropDownMenu)bounds.X+=Owner.Padding.Left;SetBounds(bounds);
}}
class BubbleMenuRenderer : ToolStripProfessionalRenderer {
    static readonly Color Ink=Color.FromArgb(89,57,34),Cream=Color.FromArgb(255,253,241);
    public BubbleMenuRenderer(){RoundedEdges=false;}
    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var path=BubbleMenu.Outline(e.ToolStrip.Width,e.ToolStrip.Height,((BubbleMenu)e.ToolStrip).TailX))using(var brush=new SolidBrush(Cream))e.Graphics.FillPath(brush,path);}
    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var path=BubbleMenu.Outline(e.ToolStrip.Width,e.ToolStrip.Height,((BubbleMenu)e.ToolStrip).TailX))using(var pen=new Pen(Ink,2))e.Graphics.DrawPath(pen,path);}
    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e){if(!e.Item.Selected)return;e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;var rect=new Rectangle(2,2,e.Item.Width-4,e.Item.Height-4);using(var path=new GraphicsPath()){int r=10;path.AddArc(rect.Left,rect.Top,r,r,180,90);path.AddArc(rect.Right-r,rect.Top,r,r,270,90);path.AddArc(rect.Right-r,rect.Bottom-r,r,r,0,90);path.AddArc(rect.Left,rect.Bottom-r,r,r,90,90);path.CloseFigure();using(var brush=new SolidBrush(Color.FromArgb(255,233,179)))e.Graphics.FillPath(brush,path);}}
    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e){using(var brush=new SolidBrush(e.Item.Enabled?Ink:Color.Gray))using(var format=new StringFormat{LineAlignment=StringAlignment.Center})e.Graphics.DrawString(e.Text,e.Item.Font,brush,new RectangleF(10,0,e.Item.Width-26,e.Item.Height),format);}
    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;float x=e.ArrowRectangle.Left+e.ArrowRectangle.Width/2,y=e.ArrowRectangle.Top+e.ArrowRectangle.Height/2;using(var pen=new Pen(Ink,1.8f))e.Graphics.DrawLines(pen,new[]{new PointF(x-2,y-4),new PointF(x+2,y),new PointF(x-2,y+4)});}
    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e){}
}

class SizeChoiceItem : ToolStripItem {
    public void Arrange(Rectangle bounds){SetBounds(bounds);}
    readonly Func<float> current;readonly Action<float> apply;readonly float[] values={.75f,1,1.4f};readonly string[] labels={"작게","보통","크게"};
    public SizeChoiceItem(Func<float> get,Action<float> set){current=get;apply=set;AutoSize=false;Size=new Size(220,33);}
    protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;using(var ink=new SolidBrush(Color.FromArgb(89,57,34)))using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center}){
        g.DrawString("크기",Font,ink,new RectangleF(7,0,44,Height),format);for(int i=0;i<3;i++){var rect=new Rectangle(57+i*51,3,47,Height-6);using(var fill=new SolidBrush(Math.Abs(current()-values[i])<.01?Color.FromArgb(255,224,150):Color.FromArgb(255,247,221)))g.FillRectangle(fill,rect);g.DrawString(labels[i],Font,ink,rect,format);}
    }}
    protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button!=MouseButtons.Left||e.X<57)return;int index=(e.X-57)/51;if(index<3){apply(values[index]);Invalidate();var menu=Owner as ToolStripDropDown;if(menu!=null)menu.Close();}}
}

partial class Usagi : Form {
    class Heart {public double Age,Life,Angle,Radius,BaseSize,Rotation,WorldX,WorldY;public float Scale;public Color Color;}
    readonly System.Collections.Generic.List<Heart> hearts=new System.Collections.Generic.List<Heart>();
    int pendingClicks; double clickDeadline,lastClickTime=-10,lastBurstTime=-10; System.Drawing.Point lastClickPoint;
    enum Mood { Walk, Turn, Look, Sit, Drowsy, Sleep, Stretch, Hop, Dragged, Fall, Nuzzle, Eat, Trip, Recover }
    [StructLayout(LayoutKind.Sequential)] struct Point { public int x,y; public Point(int a,int b){x=a;y=b;} }
    [StructLayout(LayoutKind.Sequential)] struct NativeSize { public int cx,cy; public NativeSize(int a,int b){cx=a;cy=b;} }
    [StructLayout(LayoutKind.Sequential, Pack=1)] struct Blend { public byte op,flags,alpha,format; }
    [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr h);
    [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr h,IntPtr dc);
    [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr dc,IntPtr obj);
    [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr obj);
    [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr dc);
    [DllImport("user32.dll")] static extern bool UpdateLayeredWindow(IntPtr hwnd,IntPtr dst,ref Point pos,ref NativeSize size,IntPtr src,ref Point zero,int key,ref Blend blend,int flags);
    readonly Timer timer=new Timer(); readonly Random rng=new Random(Guid.NewGuid().GetHashCode());
    readonly PetSound sounds=new PetSound();
    readonly System.Diagnostics.Stopwatch frameClock=System.Diagnostics.Stopwatch.StartNew();
    readonly System.Drawing.Text.PrivateFontCollection petFonts=new System.Drawing.Text.PrivateFontCollection();
    Font speechFont;
    Bitmap sprite; readonly Bitmap[] walk=new Bitmap[4],leftWalk=new Bitmap[4],poses=new Bitmap[4];
    readonly System.Collections.Generic.Dictionary<Bitmap,float> headWidths=new System.Collections.Generic.Dictionary<Bitmap,float>();
    Mood mood=Mood.Walk; Mood? settling; bool paused,dragging,pointerDown,moving; int direction=-1,wantedDirection=-1;
    double pressX,pressY,restDuration=2,stretchDuration=1.05;
    double autoHopIn=5.5,walkSoundIn=2.5,sleepSoundIn;
    System.Drawing.Point lastCursor,dragStart; double x,y,t,lastTick,idleTime,stateTime,decisionTime=2.5;
    double speed,targetSpeed=64,walkDistance,hopHeight,hopVelocity,fallVelocity,landing,heartTime,speechTime=2.8,nextSpeechTime=8.5,curiosityCooldown;
    float scale=1,petTop=145; string words="야하!";
    readonly string[] walkPhrases={"야하!","야핫!","우라!","우라라!","푸루야!","푸루루루!","야하~"};
    readonly string[] idlePhrases={"…하아?","후웅?","후우웅~","…우라.","야하.","푸루야!"};
    readonly string[] softPhrases={"후웅~","야하.","푸루야!","야핫!","…우라."};
    readonly string[] hopPhrases={"우라!","우라아!","야핫!","푸루야!","우라라!"};
    readonly System.Collections.Generic.Queue<string> recent=new System.Collections.Generic.Queue<string>();
    NotifyIcon tray; Icon trayIcon; ContextMenuStrip menu;ToolStripItem cleanupMenu;
    protected override CreateParams CreateParams {get{var p=base.CreateParams;p.ExStyle|=0x80000|0x80|0x08000000;return p;}}
    [STAThread] static void Main(){Application.EnableVisualStyles();bool acquired;using(var mutex=new System.Threading.Mutex(true,"UsagiDesktopPet.JH",out acquired)){if(acquired)Application.Run(new Usagi());}}
    public Usagi(bool clone=false,Usagi shared=null){
        isClone=clone;if(clone)preferencesEnabled=false;
        Text="우사기";FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;TopMost=true;Width=480;Height=360;
        string root=AppDomain.CurrentDomain.BaseDirectory;
        petFonts.AddFontFile(System.IO.Path.Combine(root,"온글잎 꾹지체.ttf"));
        speechFont=new Font(petFonts.Families[0],15,FontStyle.Regular);
        if(shared==null){
        using(var original=new Bitmap(System.IO.Path.Combine(root,"usagi.png")))sprite=Crop(original);
        LoadSheet(System.IO.Path.Combine(root,"walk.png"),walk);
        LoadSheet(System.IO.Path.Combine(root,"poses.png"),poses);
        for(int i=0;i<4;i++){leftWalk[i]=new Bitmap(walk[i]);leftWalk[i].RotateFlip(RotateFlipType.RotateNoneFlipX);}
        headWidths[sprite]=MeasureHeadWidth(sprite);foreach(var b in walk)headWidths[b]=MeasureHeadWidth(b);foreach(var b in leftWalk)headWidths[b]=MeasureHeadWidth(b);foreach(var b in poses)headWidths[b]=MeasureHeadWidth(b);
        }else{sprite=shared.sprite;Array.Copy(shared.walk,walk,4);Array.Copy(shared.leftWalk,leftWalk,4);Array.Copy(shared.poses,poses,4);foreach(var pair in shared.headWidths)headWidths[pair.Key]=pair.Value;}
        var area=Screen.PrimaryScreen.WorkingArea;x=area.Right-Width/2-110;y=area.Bottom-Height+8;lastCursor=Cursor.Position;nextSpeechTime=7+rng.NextDouble()*3;LoadPreferences();
        menu=new BubbleMenu{Font=speechFont};Func<string,ToolStripItem> add=text=>{var item=new BubbleMenuItem(text);menu.Items.Add(item);return item;};var pause=add("잠깐 쉬기");pause.Click+=(s,e)=>{paused=!paused;speed=0;moving=false;pause.Text=paused?"다시 산책하기":"잠깐 쉬기";};
        add("폴짝!").Click+=(s,e)=>StartHop();add("쓰다듬기 ♥").Click+=(s,e)=>Pet();add("한마디 해줘!").Click+=(s,e)=>Speak();add("피자만두 주기").Click+=(s,e)=>OfferFood();add("우사기 복제하기").Click+=(s,e)=>DuplicatePets();cleanupMenu=new BubbleMenuItem("우사기 정리");cleanupMenu.Click+=(s,e)=>ClearClones();
        add("이 화면에서 산책하기").Click+=(s,e)=>{var a=Screen.FromPoint(Cursor.Position).WorkingArea;x=a.Left+a.Width/2-Width/2;y=a.Bottom-Height+8;Wake();SavePreferences();};
        menu.Items.Add(new SizeChoiceItem(()=>scale,v=>{scale=v;SavePreferences();}));
        var soundMenu=add("효과음: 이어폰에서만");soundMenu.Click+=(s,e)=>{sounds.Enabled=!sounds.Enabled;soundMenu.Text=sounds.Enabled?"효과음: 이어폰에서만":"효과음: 끔";sounds.Tick(1);SavePreferences();};
        add(isClone?"이 우사기 닫기":"모두 종료").Click+=(s,e)=>Close();foreach(ToolStripItem item in menu.Items){item.AutoSize=false;item.Size=new Size(220,33);}
        menu.Opening+=(s,e)=>{RefreshCloneMenu();((BubbleMenu)menu).PetAnchor=new System.Drawing.Point((int)x+Width/2,(int)y+(int)petTop-5);var output=EndpointProbe.Read();soundMenu.Text=!sounds.Enabled?"효과음: 끔":output!=null&&output.Headphones?"효과음: 켜짐 (이어폰)":"효과음: 이어폰 연결 대기";};
        ContextMenuStrip=menu;
        trayIcon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        tray=new NotifyIcon{Icon=trayIcon,Text="우사기 · 1번 폴짝 / 2번 하트 / 3번 하트 폭발",ContextMenuStrip=menu,Visible=!isClone};tray.DoubleClick+=(s,e)=>{var a=Screen.PrimaryScreen.WorkingArea;x=a.Right-Width/2-110;y=a.Bottom-Height+8;StartHop();};
        MouseDown+=(s,e)=>{if(e.Button==MouseButtons.Right&&(mood==Mood.Sleep||mood==Mood.Drowsy))Wake();if(e.Button==MouseButtons.Left){BeginPointer(Cursor.Position);Capture=true;}};
        MouseMove+=(s,e)=>{MovePointer(Cursor.Position);if(dragging)Render();};
        MouseUp+=(s,e)=>{if(e.Button==MouseButtons.Left){EndPointer(frameClock.Elapsed.TotalSeconds,Cursor.Position);Capture=false;}};
        timer.Interval=16;timer.Tick+=(s,e)=>TickPet();Shown+=(s,e)=>{lastTick=frameClock.Elapsed.TotalSeconds;if(!livePets.Contains(this))livePets.Add(this);if(!isClone)StartDayGreeting(DateTime.Now);timer.Start();};
        FormClosed+=(s,e)=>{if(!isClone)ClearClones();livePets.Remove(this);SavePreferences();timer.Dispose();sounds.Dispose();tray.Visible=false;tray.Dispose();if(trayIcon!=null)trayIcon.Dispose();menu.Dispose();cleanupMenu.Dispose();speechFont.Dispose();petFonts.Dispose();if(!isClone){sprite.Dispose();foreach(var b in walk)b.Dispose();foreach(var b in leftWalk)b.Dispose();foreach(var b in poses)b.Dispose();}};
    }
    static void LoadSheet(string path,Bitmap[] target){using(var sheet=new Bitmap(path))for(int i=0;i<4;i++){int l=i*sheet.Width/4,r=(i+1)*sheet.Width/4;using(var cell=sheet.Clone(new Rectangle(l,0,r-l,sheet.Height),PixelFormat.Format32bppArgb))target[i]=Crop(cell);}}
    // Measure the face band, excluding upright/floppy ears and feet. All poses share one head scale.
    static float MeasureHeadWidth(Bitmap image){int widest=1;for(int y=(int)(image.Height*.45);y<(int)(image.Height*.70);y++){
        int left=image.Width,right=-1;for(int x=0;x<image.Width;x++)if(image.GetPixel(x,y).A>100){left=Math.Min(left,x);right=x;}if(right>=left)widest=Math.Max(widest,right-left+1);
    }return widest;}
    static Bitmap Crop(Bitmap source){int l=source.Width,r=-1,top=source.Height,b=-1;var rect=new Rectangle(0,0,source.Width,source.Height);using(var copy=source.Clone(rect,PixelFormat.Format32bppArgb)){
        var bits=copy.LockBits(rect,ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);try{var row=new byte[source.Width*4];for(int j=0;j<source.Height;j++){Marshal.Copy(IntPtr.Add(bits.Scan0,j*bits.Stride),row,0,row.Length);for(int i=0;i<source.Width;i++)if(row[i*4+3]>30){l=Math.Min(l,i);r=Math.Max(r,i);top=Math.Min(top,j);b=Math.Max(b,j);}}}finally{copy.UnlockBits(bits);}
        return r<l?new Bitmap(source):copy.Clone(new Rectangle(l,top,r-l+1,b-top+1),PixelFormat.Format32bppArgb);
    }}
    void Change(Mood next){if(mood==Mood.Sleep&&next!=Mood.Sleep)sounds.StopSleep();mood=next;settling=null;stateTime=0;if(next==Mood.Sit)restDuration=1.4+rng.NextDouble()*3.1;else if(next==Mood.Look)restDuration=.8+rng.NextDouble()*1.8;if(next==Mood.Stretch)stretchDuration=.85+rng.NextDouble()*.95;if(next!=Mood.Walk&&next!=Mood.Hop)moving=false;}
    void Settle(Mood next){if(speed<1){speed=0;Change(next);}else{settling=next;targetSpeed=0;}}
    void Say(string text){words=text;speechTime=1.5+rng.NextDouble()*1.1;nextSpeechTime=7+rng.NextDouble()*3;recent.Enqueue(text);while(recent.Count>4)recent.Dequeue();}
    string Choose(string[] phrases){var choices=new System.Collections.Generic.List<string>();foreach(var p in phrases)if(!recent.Contains(p))choices.Add(p);if(choices.Count==0)foreach(var p in phrases)if(p!=words)choices.Add(p);return choices[rng.Next(choices.Count)];}
    void AutoSay(string text){if(nextSpeechTime<=0)Say(text);}
    bool autonomousSpeech;
    void Speak(){Say(Choose(mood==Mood.Sleep?new[]{"…","후웅…","…후우웅"}:moving?walkPhrases:idlePhrases));}
    void Wake(){wakeDelay=-1;CancelSocial();idleTime=0;if(mood==Mood.Sleep||mood==Mood.Drowsy){Change(Mood.Stretch);AutoSay("후우웅~");}else Change(Mood.Walk);}
    void Pet(){CancelSocial();idleTime=0;heartTime=0;EmitHearts(false);sounds.Play("heart");Say("푸루야!");}
    void BeginPointer(System.Drawing.Point point){if(mood==Mood.Sleep||mood==Mood.Drowsy)Wake();CancelSocial();scatterActive=false;foodActive=false;pointerDown=true;dragging=false;dragStart=point;pressX=x;pressY=y;idleTime=0;}
    void MovePointer(System.Drawing.Point point){if(!pointerDown)return;double dx=point.X-dragStart.X,dy=point.Y-dragStart.Y;
        if(!dragging&&(Math.Abs(dx)>=Math.Max(6,SystemInformation.DragSize.Width/2)||Math.Abs(dy)>=Math.Max(6,SystemInformation.DragSize.Height/2))){dragging=true;pendingClicks=0;speed=0;hopHeight=0;Change(Mood.Dragged);}
        if(dragging){x=pressX+dx;y=pressY+dy;}
    }
    void EndPointer(double now,System.Drawing.Point point){if(!pointerDown)return;MovePointer(point);bool moved=dragging;pointerDown=false;dragging=false;idleTime=0;
        if(!moved){RegisterClick(now,point);return;}pendingClicks=0;fallVelocity=0;Change(Mood.Fall);if(rng.Next(3)==0)Say(Choose(hopPhrases));
    }
    void RegisterClick(double now,System.Drawing.Point point){
        double interval=SystemInformation.DoubleClickTime/1000.0;int dx=point.X-lastClickPoint.X,dy=point.Y-lastClickPoint.Y;
        bool consecutive=now-lastClickTime<=interval&&Math.Abs(dx)<=SystemInformation.DoubleClickSize.Width&&Math.Abs(dy)<=SystemInformation.DoubleClickSize.Height;
        if(!consecutive){ResolveClicks();pendingClicks=0;}pendingClicks++;lastClickTime=now;lastClickPoint=point;clickDeadline=now+interval;
        if(pendingClicks>=3&&now-lastBurstTime>.22){HeartBurst();lastBurstTime=now;}
    }
    void ResolveClicks(){int count=pendingClicks;pendingClicks=0;if(count==1)StartHop();else if(count==2)Pet();}
    void HeartBurst(){idleTime=0;EmitHearts(true);sounds.Play("burst");Say("야하아!!");}
    void EmitHearts(bool burst){
        int count=burst?22:3;for(int i=0;i<count;i++){
            double angle=burst?-Math.PI+.07+(Math.PI-.14)*i/Math.Max(1,count-1)+(rng.NextDouble()-.5)*.15:-1.3+(rng.NextDouble()-.5)*.8;
            hearts.Add(new Heart{Age=burst?-rng.NextDouble()*.065:-i*.09,Life=burst?.95+rng.NextDouble()*.4:1.05+rng.NextDouble()*.25,Angle=angle,Radius=burst?80+rng.NextDouble()*66:40+rng.NextDouble()*28,BaseSize=burst?10+rng.NextDouble()*10:12+rng.NextDouble()*5,Rotation=(rng.NextDouble()-.5)*65,WorldX=x+Width/2,WorldY=y+Height-55*scale-hopHeight,Scale=scale,Color=i%3==0?Color.FromArgb(245,155,176):i%3==1?Color.FromArgb(231,94,131):Color.FromArgb(250,119,146)});
        }while(hearts.Count>88)hearts.RemoveAt(0);
    }
    void StartHop(){if(paused)return;CancelSocial();scatterActive=false;foodActive=false;idleTime=0;speed=0;hopHeight=0;hopVelocity=220;autoHopIn=6+rng.NextDouble()*9;Change(Mood.Hop);sounds.Play("hop");if(autonomousSpeech)AutoSay("우라!");else Say("우라!");}
    void AutonomousHop(){double idle=idleTime;autonomousSpeech=true;try{StartHop();}finally{autonomousSpeech=false;idleTime=idle;}}
    static double Approach(double value,double goal,double delta){return value<goal?Math.Min(goal,value+delta):Math.Max(goal,value-delta);}
    void TickPet(){double now=frameClock.Elapsed.TotalSeconds,dt=Math.Max(.001,Math.Min(.07,now-lastTick));lastTick=now;if(pendingClicks>0&&now>=clickDeadline&&!pointerDown)ResolveClicks();UpdatePet(dt,Cursor.Position);Render();}
    void UpdatePet(double dt,System.Drawing.Point cursor){
        t+=dt;speechTime=Math.Max(0,speechTime-dt);nextSpeechTime-=dt;heartTime=Math.Max(0,heartTime-dt);curiosityCooldown-=dt;landing=Math.Max(0,landing-dt);
        sounds.Tick(dt);
        for(int i=hearts.Count-1;i>=0;i--){hearts[i].Age+=dt;if(hearts[i].Age>=hearts[i].Life)hearts.RemoveAt(i);}
        int mx=cursor.X-lastCursor.X,my=cursor.Y-lastCursor.Y;
        UpdateDetails(dt,cursor,mx,my);if(mx*mx+my*my>9)idleTime=0;else idleTime+=dt;UpdateSleepWake(dt,cursor,mx*mx+my*my>9);lastCursor=cursor;
        if(paused||menu.Visible){moving=false;return;}
        var area=Screen.FromPoint(new System.Drawing.Point((int)x+Width/2,(int)y+Height-15)).WorkingArea;double floor=area.Bottom-Height+8;
        if(dragging){moving=false;return;}
        if(pointerDown||pendingClicks>0){idleTime=0;return;}
        stateTime+=dt;
        if(UpdateSpecial(dt))return;
        if(UpdateSocial(dt,cursor,mx*mx+my*my>9))return;
        if(mood==Mood.Fall){fallVelocity+=780*dt;y=Math.Min(floor,y+fallVelocity*dt);if(y>=floor){landing=.26;Change(Mood.Sit);}moving=false;return;}
        y=floor;
        if(idleTime>=10&&mood!=Mood.Sleep&&mood!=Mood.Drowsy&&mood!=Mood.Hop){speed=0;Change(Mood.Drowsy);}
        if(mood==Mood.Drowsy&&stateTime>.65){Change(Mood.Sleep);speechTime=0;nextSpeechTime=12+rng.NextDouble()*10;sleepSoundIn=0;}
        if(mood==Mood.Sleep){moving=false;sleepSoundIn-=dt;if(sleepSoundIn<=0){if(!isClone)sounds.PlayAmbient("sleep");sleepSoundIn=9+rng.NextDouble()*8;}return;}
        if(mood==Mood.Stretch&&stateTime>stretchDuration){targetSpeed=rng.Next(46,90);decisionTime=1+rng.NextDouble()*2;Change(Mood.Walk);}
        if((mood==Mood.Sit||mood==Mood.Look)&&stateTime>restDuration){targetSpeed=rng.Next(42,97);decisionTime=1.5+rng.NextDouble()*3;Change(Mood.Walk);}
        if(mood==Mood.Turn){speed*=Math.Exp(-dt/.085);if(stateTime>.24&&speed<1){direction=wantedDirection;Change(Mood.Walk);}}
        if(mood==Mood.Walk&&!settling.HasValue){decisionTime-=dt;if(decisionTime<=0){int action=rng.Next(10);decisionTime=1.8+rng.NextDouble()*3.5;targetSpeed=rng.Next(42,IsNight(DateTime.Now)?75:101);
                if(action<2){Settle(Mood.Sit);}else if(action==2){Settle(Mood.Look);}else if(action==3){AutonomousHop();}else if(action<7){wantedDirection=-direction;Change(Mood.Turn);}}
            double dx=cursor.X-(x+Width/2),dy=cursor.Y-(y+Height-70*scale);if(curiosityCooldown<=0&&dx*dx+dy*dy<65*65&&mx*mx+my*my>9){Settle(Mood.Look);curiosityCooldown=6;AutoSay("하아?");}
        }
        if(mood==Mood.Hop){hopVelocity-=620*dt;hopHeight=Math.Max(0,hopHeight+hopVelocity*dt);if(hopHeight<=0&&hopVelocity<0){landing=.24;hopHeight=0;Change(Mood.Sit);}}
        double oldX=x;if(mood==Mood.Walk){speed+=(targetSpeed-speed)*(1-Math.Exp(-dt/(settling.HasValue?.14:.38)));x+=direction*speed*dt;if(settling.HasValue&&speed<1){var rest=settling.Value;speed=0;Change(rest);}}else if(mood==Mood.Turn)x+=direction*speed*dt;
        double min=area.Left-Width/2+38*scale,max=area.Right-Width/2-38*scale;
        if(x<min||x>max){x=Math.Max(min,Math.Min(max,x));wantedDirection=x<=min?1:-1;speed=0;if(mood==Mood.Walk||mood==Mood.Turn)Change(Mood.Turn);}
        double distance=Math.Abs(x-oldX);moving=distance>.01&&(mood==Mood.Walk||mood==Mood.Turn);if(moving){walkDistance+=distance;LeaveTracks(distance);}
        if(moving&&speed>82&&!foodActive&&tripCooldown<=0){BeginTrip();}
        if(moving&&mood==Mood.Walk&&!settling.HasValue){autoHopIn-=dt;walkSoundIn-=dt;if(autoHopIn<=0){AutonomousHop();}else if(walkSoundIn<=0&&!isClone){bool ura=rng.Next(2)==0;if(sounds.PlayAmbient(ura?"walk_ura":"walk_purya"))AutoSay(ura?"우라!":"푸루야!");walkSoundIn=5+rng.NextDouble()*7;}}
        if(nextSpeechTime<=0&&!dragging&&mood!=Mood.Drowsy)Speak();
    }
    Bitmap DrawFrame(){var frame=new Bitmap(Width,Height,PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(frame)){
        g.SmoothingMode=SmoothingMode.AntiAlias;g.InterpolationMode=InterpolationMode.HighQualityBicubic;
        g.TranslateTransform((float)(x-Math.Floor(x)),(float)(y-Math.Floor(y)));
        double phase=walkDistance/(28*scale)*Math.PI*2;int index=(int)(walkDistance/(7*scale))%4;
        Bitmap pose=moving?(direction>0?walk[index]:leftWalk[index]):sprite;
        float angle=moving?(float)Math.Sin(phase)*.8f:0,bob=moving?(float)(1-Math.Cos(phase*2))*1.0f:0;
        if(mood==Mood.Sit||mood==Mood.Drowsy)pose=poses[0];
        if(mood==Mood.Sleep)pose=poses[1];
        if(mood==Mood.Stretch)pose=poses[2];
        if(mood==Mood.Hop)pose=poses[3];
        if(mood==Mood.Look){pose=direction>0?walk[0]:leftWalk[0];angle=(float)Math.Sin(stateTime*3)*5;}
        if(mood==Mood.Drowsy)angle=(float)Math.Sin(stateTime*4)*5;
        if(mood==Mood.Dragged){pose=poses[3];angle=(float)Math.Sin(t*7)*9;}
        if(mood==Mood.Fall||scatterActive)pose=poses[3];
        float factor=68*scale/headWidths[pose];if(scatterActive){double appear=Math.Min(1,scatterTime/.16);factor*=(float)(.4+.6*(1-Math.Pow(1-appear,3)));angle=(float)(scatterVX/32*Math.Sin(Math.Min(1,scatterTime)*Math.PI));}
        float h=pose.Height*factor,width=pose.Width*factor,bottom=Height-10-bob-(float)hopHeight;
        float squash=landing>0?(float)Math.Sin(landing/.26*Math.PI)*.1f:0;
        if(!paused&&mood==Mood.Sleep){h+=(float)Math.Sin(t*2.2)*1.2f;}
        if(mood==Mood.Nuzzle){pose=poses[1];factor=68*scale/headWidths[pose];h=pose.Height*factor;width=pose.Width*factor;angle=(float)Math.Sin(t*5)*2;}
        if(mood==Mood.Eat){pose=poses[0];factor=68*scale/headWidths[pose];h=pose.Height*factor;width=pose.Width*factor;bob=(float)ChewPulse(stateTime)*.65f*scale;bottom-=bob;angle=stateTime<.65?(float)Math.Sin(stateTime/.65*Math.PI)*3:((float)ChewPulse(stateTime)-.5f)*.65f;}
        if(mood==Mood.Trip){pose=poses[3];factor=68*scale/headWidths[pose];h=pose.Height*factor;width=pose.Width*factor;angle=direction*(float)(Math.Sin(Math.Min(1,stateTime/.5)*Math.PI/2)*68);bottom-=8*scale;}
        if(mood==Mood.Recover){pose=poses[0];factor=68*scale/headWidths[pose];h=pose.Height*factor;width=pose.Width*factor;angle=(float)Math.Sin(stateTime*7)*8;}
        if(mood==Mood.Look)angle+=(float)gazeTilt;
        DrawDetails(g);DrawHearts(g);
        var saved=g.Save();g.TranslateTransform(Width/2,bottom);g.RotateTransform(angle);g.ScaleTransform(1+squash,1-squash);g.DrawImage(pose,-width/2,-h,width,h);if(mood==Mood.Eat){var eating=g.Save();g.TranslateTransform(-Width/2,-bottom);DrawEating(g,bottom);g.Restore(eating);}g.Restore(saved);
        if(mood==Mood.Sleep)DrawSleepText(g,width,h);
        petTop=bottom-h;
        if(speechTime>0&&!menu.Visible)DrawSpeech(g,Math.Max(4,bottom-h-72*scale));
    }return frame;}
    void DrawHearts(Graphics g){foreach(var heart in hearts){if(heart.Age<0)continue;double u=heart.Age/heart.Life;
        double travel=(1-Math.Exp(-u*4.5))/(1-Math.Exp(-4.5));double grow=1-Math.Pow(1-u,3);float size=(float)(heart.BaseSize*heart.Scale*(.2+1.2*grow));
        int alpha=(int)(255*Math.Min(1,u/.07)*Math.Pow(1-u,1.1));if(alpha<=0)continue;
        float hx=(float)(heart.WorldX-x+Math.Cos(heart.Angle)*heart.Radius*heart.Scale*travel),hy=(float)(heart.WorldY-y+Math.Sin(heart.Angle)*heart.Radius*heart.Scale*travel-12*u*heart.Scale);
        var saved=g.Save();g.TranslateTransform(hx,hy);g.RotateTransform((float)(heart.Rotation*(.3+u)));g.ScaleTransform(size,size);
        using(var path=new GraphicsPath()){path.AddBezier(0,.46f,-.12f,.30f,-.55f,.03f,-.48f,-.23f);path.AddBezier(-.48f,-.23f,-.43f,-.51f,-.10f,-.52f,0,-.28f);path.AddBezier(0,-.28f,.10f,-.52f,.43f,-.51f,.48f,-.23f);path.AddBezier(.48f,-.23f,.55f,.03f,.12f,.30f,0,.46f);path.CloseFigure();using(var fill=new SolidBrush(Color.FromArgb(alpha,heart.Color)))g.FillPath(fill,path);}
        g.Restore(saved);
    }}
    void Render(){using(var frame=DrawFrame()){var screen=GetDC(IntPtr.Zero);var dc=CreateCompatibleDC(screen);var bitmap=frame.GetHbitmap(Color.FromArgb(0));var old=SelectObject(dc,bitmap);
        try{var pos=new Point((int)Math.Floor(x),(int)Math.Floor(y));var size=new NativeSize(Width,Height);var zero=new Point(0,0);var blend=new Blend{alpha=255,format=1};UpdateLayeredWindow(Handle,screen,ref pos,ref size,dc,ref zero,0,ref blend,2);}finally{SelectObject(dc,old);DeleteObject(bitmap);DeleteDC(dc);ReleaseDC(IntPtr.Zero,screen);}
    }}
    void DrawSpeech(Graphics g,float top){var saved=g.Save();g.TranslateTransform(Width/2,top);g.ScaleTransform(scale,scale);float left=-68,width=136,height=43;top=0;using(var path=new GraphicsPath()){
        path.AddBezier(left+19,top,left-2,top-3,left-7,top+20,left+3,top+30);path.AddBezier(left+3,top+30,left+7,top+43,left+35,top+height,left+54,top+height-1);
        path.AddLine(left+54,top+height-1,left+66,top+height+12);path.AddLine(left+66,top+height+12,left+72,top+height-1);
        path.AddBezier(left+72,top+height-1,left+width+8,top+height+2,left+width+8,top+12,left+width-8,top+5);path.AddBezier(left+width-8,top+5,left+104,top-7,left+49,top-4,left+19,top);path.CloseFigure();
        using(var fill=new SolidBrush(Color.FromArgb(255,255,253,241)))g.FillPath(fill,path);using(var pen=new Pen(Color.FromArgb(89,57,34),2.1f)){pen.LineJoin=LineJoin.Round;g.DrawPath(pen,path);}
        using(var ink=new SolidBrush(Color.FromArgb(89,57,34)))using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})g.DrawString(words,speechFont,ink,new RectangleF(left,top,width,height),format);
    }g.Restore(saved);}
}
