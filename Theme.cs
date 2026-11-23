using System;using System.IO;using System.Drawing.Imaging;using System.Runtime.InteropServices;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;using Microsoft.Win32;
static class PetTheme {
    static PetTheme(){LoadFrom(PreferencePath);}
    internal static void LoadFrom(string path){try{Set(File.ReadAllText(path).Trim());}catch(IOException){Set("system");}catch(UnauthorizedAccessException){Set("system");}}
    internal static string PreferencePath {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"UsagiPet","theme.txt");}}
    internal static void Save(){SaveTo(PreferencePath);}
    internal static void SaveTo(string path){try{Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";File.WriteAllText(temp,Mode);if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);}catch(IOException){}catch(UnauthorizedAccessException){}}
    internal static string Mode="system";internal static bool Dark;internal static event EventHandler Changed;
    internal static Color Surface {get{return Dark?Color.FromArgb(38,36,34):Color.FromArgb(248,246,241);}}
    internal static Color Ink {get{return Dark?Color.FromArgb(244,237,225):Color.FromArgb(64,53,43);}}
    internal static Color LinkInk {get{return Dark?Ink:Color.Black;}}
    internal static Color Muted {get{return Dark?Color.FromArgb(205,195,181):Color.FromArgb(90,76,61);}}
    internal static Color Line {get{return Dark?Color.FromArgb(83,76,67):Color.FromArgb(218,207,189);}}
    internal static Color SelectionInk {get{return Color.FromArgb(72,52,29);}}
    internal static Color Accent {get{return Dark?Color.FromArgb(199,169,126):Color.FromArgb(237,216,183);}}
    internal static Color Button {get{return Dark?Color.FromArgb(100,82,75,65):Color.FromArgb(125,255,255,255);}}
    internal static Color Hover {get{return Dark?Color.FromArgb(160,114,98,77):Color.FromArgb(205,244,235,219);}}
    internal static Color Border {get{return Dark?Color.FromArgb(135,185,168,145):Color.FromArgb(220,255,255,255);}}
    internal static bool SystemDark(){try{using(var key=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))return key!=null&&Convert.ToInt32(key.GetValue("AppsUseLightTheme",1))==0;}catch{return false;}}
    internal static void Set(string mode){if(mode!="light"&&mode!="dark"&&mode!="system")mode="system";bool dark=mode=="dark"||mode=="system"&&SystemDark();if(Mode==mode&&Dark==dark)return;Mode=mode;Dark=dark;if(Changed!=null)Changed(null,EventArgs.Empty);}
    internal static void RefreshSystem(){if(Mode=="system")Set("system");}
    internal static void Apply(Control control){bool opaque=control is RichTextBox;control.ForeColor=Equals(control.Tag,"muted")?Muted:Ink;if(control is Form||opaque)control.BackColor=Surface;else if(control is Label||control is PictureBox||control is Panel)control.BackColor=Color.Transparent;var link=control as LinkLabel;if(link!=null){link.LinkColor=link.VisitedLinkColor=Equals(control.Tag,"muted")?Muted:LinkInk;link.ActiveLinkColor=Ink;}foreach(Control child in control.Controls)Apply(child);control.Invalidate();}
    internal static void PaintUnder(Graphics g,Control control){var root=control.FindForm() as GlassBubbleForm;if(root==null){g.Clear(Surface);return;}Point point=Point.Empty;Control cursor=control;while(cursor!=root&&cursor!=null){point.Offset(cursor.Location);cursor=cursor.Parent;}root.PaintSlice(g,new Rectangle(point,control.Size));}
}
// The live backdrop is reconstructed in memory while the popup is open.
// Blurred backdrop with a translucent tint; no unblurred whole-window overlay.
class GlassBubbleForm : Form {
    Bitmap blurred,material,original,shapeMask;int tail=-1; readonly Timer backdropTimer=new Timer{Interval=100}; bool backdropPending;
    internal GlassBubbleForm(){Opacity=1;DoubleBuffered=true;PetTheme.Changed+=ThemeChanged;backdropTimer.Tick+=(s,e)=>RefreshBackdrop();}
    protected override void OnLoad(EventArgs e){CaptureBackdrop();PetTheme.Apply(this);base.OnLoad(e);}
    protected override void OnShown(EventArgs e){base.OnShown(e);backdropTimer.Start();RefreshBackdrop();}
    void RefreshBackdrop(){if(backdropPending||!Visible||IsDisposed||!LiveBackdrop.Enter())return;backdropPending=true;var bounds=Bounds;var window=Handle;System.Threading.ThreadPool.QueueUserWorkItem(delegate{Bitmap image=null;try{image=LiveBackdrop.Capture(window,bounds);if(IsDisposed||!IsHandleCreated){image.Dispose();return;}var ready=image;image=null;try{BeginInvoke((Action)delegate{backdropPending=false;if(IsDisposed||!Visible||Bounds!=bounds){ready.Dispose();return;}SetBackdrop(ready);});}catch(InvalidOperationException){ready.Dispose();}}catch{if(image!=null)image.Dispose();try{if(IsHandleCreated)BeginInvoke((Action)delegate{backdropPending=false;});}catch(InvalidOperationException){}}finally{LiveBackdrop.Leave();}});}
    internal void ConfigureShape(int tip){tail=tip;BuildMask();}
    void BuildMask(){if(shapeMask!=null)shapeMask.Dispose();shapeMask=new Bitmap(Width,Height,PixelFormat.Format32bppArgb);using(var large=new Bitmap(Width*4,Height*4,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(large)){g.Clear(Color.Transparent);g.SmoothingMode=SmoothingMode.AntiAlias;g.ScaleTransform(4,4);using(var shape=BubbleMenu.Outline(Width,Height,tail))g.FillPath(Brushes.White,shape);using(var down=Graphics.FromImage(shapeMask))down.DrawImage(large,new Rectangle(0,0,Width,Height),0,0,large.Width,large.Height,GraphicsUnit.Pixel);}var region=new Region();region.MakeEmpty();var bits=shapeMask.LockBits(new Rectangle(0,0,Width,Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);try{var row=new byte[Width*4];for(int y=0;y<Height;y++){Marshal.Copy(IntPtr.Add(bits.Scan0,y*bits.Stride),row,0,row.Length);int start=-1;for(int x=0;x<=Width;x++){bool on=x<Width&&row[x*4+3]>0;if(on&&start<0)start=x;if(!on&&start>=0){region.Union(new Rectangle(start,y,x-start,1));start=-1;}}}}finally{shapeMask.UnlockBits(bits);}var old=Region;Region=region;if(old!=null)old.Dispose();}
    internal void CaptureBackdrop(){if(blurred!=null){blurred.Dispose();blurred=null;}if(original!=null){original.Dispose();original=null;}try{original=new Bitmap(Width,Height,PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(original)){g.Clear(PetTheme.Surface);var clip=Rectangle.Intersect(Bounds,SystemInformation.VirtualScreen);if(clip.Width>0&&clip.Height>0)g.CopyFromScreen(clip.Location,new Point(clip.Left-Left,clip.Top-Top),clip.Size);}SetBackdrop(original);}catch{if(blurred!=null)blurred.Dispose();blurred=null;}RebuildMaterial();}
    void SetBackdrop(Bitmap image){if(original!=image){if(original!=null)original.Dispose();original=image;}if(blurred!=null)blurred.Dispose();blurred=new Bitmap(Math.Max(1,Width/24),Math.Max(1,Height/24));using(var b=Graphics.FromImage(blurred))using(var attributes=new ImageAttributes()){attributes.SetWrapMode(WrapMode.TileFlipXY);b.InterpolationMode=InterpolationMode.HighQualityBilinear;b.DrawImage(original,new Rectangle(0,0,blurred.Width,blurred.Height),0,0,original.Width,original.Height,GraphicsUnit.Pixel,attributes);}RebuildMaterial();}
    void ThemeChanged(object sender,EventArgs e){if(IsDisposed)return;RebuildMaterial();PetTheme.Apply(this);}
    void RebuildMaterial(){if(material!=null)material.Dispose();material=new Bitmap(Math.Max(1,Width),Math.Max(1,Height),PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(material)){g.Clear(PetTheme.Surface);if(blurred!=null){using(var attributes=new ImageAttributes()){attributes.SetWrapMode(WrapMode.TileFlipXY);g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawImage(blurred,new Rectangle(0,0,Width,Height),0,0,blurred.Width,blurred.Height,GraphicsUnit.Pixel,attributes);}using(var tint=new SolidBrush(Color.FromArgb(PetTheme.Dark?145:140,PetTheme.Surface)))g.FillRectangle(tint,0,0,Width,Height);}}if(shapeMask==null)BuildMask();BlendEdge();Invalidate(true);}
    void BlendEdge(){if(original==null||shapeMask==null)return;var rectangle=new Rectangle(0,0,Width,Height);var front=material.LockBits(rectangle,ImageLockMode.ReadWrite,PixelFormat.Format32bppArgb);var back=original.LockBits(rectangle,ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);var mask=shapeMask.LockBits(rectangle,ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);try{var a=new byte[Width*4];var b=new byte[a.Length];var m=new byte[a.Length];for(int y=0;y<Height;y++){Marshal.Copy(IntPtr.Add(front.Scan0,y*front.Stride),a,0,a.Length);Marshal.Copy(IntPtr.Add(back.Scan0,y*back.Stride),b,0,b.Length);Marshal.Copy(IntPtr.Add(mask.Scan0,y*mask.Stride),m,0,m.Length);for(int x=0;x<Width;x++){int offset=x*4,coverage=m[offset+3];for(int c=0;c<3;c++)a[offset+c]=(byte)((a[offset+c]*coverage+b[offset+c]*(255-coverage)+127)/255);a[offset+3]=255;}Marshal.Copy(a,0,IntPtr.Add(front.Scan0,y*front.Stride),a.Length);}}finally{material.UnlockBits(front);original.UnlockBits(back);shapeMask.UnlockBits(mask);}}
    internal Color SampleMaterial(Point point){if(material==null)return PetTheme.Surface;int r=0,g=0,b=0;for(int dy=-20;dy<=20;dy+=20)for(int dx=-20;dx<=20;dx+=20){Color c=material.GetPixel(Math.Max(0,Math.Min(material.Width-1,point.X+dx)),Math.Max(0,Math.Min(material.Height-1,point.Y+dy)));r+=c.R;g+=c.G;b+=c.B;}return Color.FromArgb(r/9,g/9,b/9);}
    internal void PaintSlice(Graphics g,Rectangle slice){if(material==null){g.Clear(PetTheme.Surface);return;}g.DrawImage(material,new Rectangle(Point.Empty,slice.Size),slice,GraphicsUnit.Pixel);}
    protected override void OnPaintBackground(PaintEventArgs e){PaintSlice(e.Graphics,ClientRectangle);}
    protected override void Dispose(bool disposing){if(disposing){backdropTimer.Stop();backdropTimer.Dispose();PetTheme.Changed-=ThemeChanged;if(material!=null)material.Dispose();if(blurred!=null)blurred.Dispose();if(original!=null)original.Dispose();if(shapeMask!=null)shapeMask.Dispose();}base.Dispose(disposing);}
}
class GlassPanel : Panel {internal GlassPanel(){DoubleBuffered=true;BackColor=Color.Transparent;}protected override void OnPaintBackground(PaintEventArgs e){PetTheme.PaintUnder(e.Graphics,this);}}

partial class Usagi {double themePoll;void PollTheme(double dt){if(isClone)return;themePoll-=dt;if(themePoll<=0){themePoll=2;PetTheme.RefreshSystem();}}void SaveThemePreference(){bool canSave=SaveEnabled;foreach(var pet in livePets)if(pet.SaveEnabled)canSave=true;if(canSave)PetTheme.Save();}}

// Reconstruct only windows behind the popup. Never hide the popup or capture it recursively.
static class LiveBackdrop {
    [StructLayout(LayoutKind.Sequential)]struct Rect{public int L,T,R,B;public Rectangle Box{get{return Rectangle.FromLTRB(L,T,R,B);}}}
    [DllImport("user32.dll")]static extern IntPtr GetShellWindow();
    [DllImport("user32.dll")]static extern IntPtr GetWindow(IntPtr window,uint command);
    [DllImport("user32.dll")]static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")]static extern bool IsIconic(IntPtr window);
    [DllImport("user32.dll")]static extern bool GetWindowRect(IntPtr window,out Rect rect);
    [DllImport("user32.dll")]static extern bool PrintWindow(IntPtr window,IntPtr dc,uint flags);
    [DllImport("dwmapi.dll")]static extern int DwmGetWindowAttribute(IntPtr window,int attribute,out int value,int size);
    [DllImport("gdi32.dll")]static extern bool SetViewportOrgEx(IntPtr dc,int x,int y,IntPtr previous);
    [DllImport("gdi32.dll")]static extern int IntersectClipRect(IntPtr dc,int left,int top,int right,int bottom);
    [DllImport("gdi32.dll")]static extern int SaveDC(IntPtr dc);
    [DllImport("gdi32.dll")]static extern bool RestoreDC(IntPtr dc,int saved);
    static int busy;
    internal static bool Enter(){return System.Threading.Interlocked.CompareExchange(ref busy,1,0)==0;}
    internal static void Leave(){System.Threading.Interlocked.Exchange(ref busy,0);}
    internal static Bitmap Capture(IntPtr popup,Rectangle bounds){
        var windows=new System.Collections.Generic.List<IntPtr>();var shell=GetShellWindow();
        int visited=0;for(IntPtr window=GetWindow(popup,2);window!=IntPtr.Zero&&visited++<512;window=GetWindow(window,2)){
            if(window==shell||!IsWindowVisible(window)||IsIconic(window))continue;
            int cloaked;DwmGetWindowAttribute(window,14,out cloaked,4);if(cloaked!=0)continue;
            Rect rect;if(GetWindowRect(window,out rect)&&rect.Box.IntersectsWith(bounds))windows.Add(window);
        }
        var image=new Bitmap(bounds.Width,bounds.Height,PixelFormat.Format32bppRgb);
        try{using(var graphics=Graphics.FromImage(image)){graphics.Clear(PetTheme.Surface);var dc=graphics.GetHdc();try{
            Paint(shell,dc,bounds);for(int i=windows.Count-1;i>=0;i--)Paint(windows[i],dc,bounds);
        }finally{graphics.ReleaseHdc(dc);}}return image;}catch{image.Dispose();throw;}
    }
    static void Paint(IntPtr window,IntPtr dc,Rectangle bounds){Rect rect;if(window==IntPtr.Zero||!GetWindowRect(window,out rect))return;var clip=Rectangle.Intersect(bounds,rect.Box);if(clip.IsEmpty)return;int saved=SaveDC(dc);try{SetViewportOrgEx(dc,rect.L-bounds.Left,rect.T-bounds.Top,IntPtr.Zero);IntersectClipRect(dc,clip.Left-rect.L,clip.Top-rect.T,clip.Right-rect.L,clip.Bottom-rect.T);PrintWindow(window,dc,2);}finally{RestoreDC(dc,saved);}}
}
