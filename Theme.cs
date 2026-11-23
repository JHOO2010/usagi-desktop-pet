using System;using System.IO;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;using Microsoft.Win32;
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
// Frosted backdrop is captured once before showing, never saved or uploaded.
// Flat blurred material with a small whole-window alpha; no decorative lighting.
class GlassBubbleForm : Form {
    Bitmap blurred,material;
    internal GlassBubbleForm(){Opacity=.96;DoubleBuffered=true;PetTheme.Changed+=ThemeChanged;}
    protected override void OnLoad(EventArgs e){CaptureBackdrop();PetTheme.Apply(this);base.OnLoad(e);}
    internal void CaptureBackdrop(){if(blurred!=null){blurred.Dispose();blurred=null;}try{using(var screen=new Bitmap(Width,Height))using(var g=Graphics.FromImage(screen)){g.Clear(PetTheme.Surface);var clip=Rectangle.Intersect(Bounds,SystemInformation.VirtualScreen);if(clip.Width>0&&clip.Height>0)g.CopyFromScreen(clip.Location,new Point(clip.Left-Left,clip.Top-Top),clip.Size);blurred=new Bitmap(Math.Max(1,Width/24),Math.Max(1,Height/24));using(var b=Graphics.FromImage(blurred)){b.InterpolationMode=InterpolationMode.HighQualityBilinear;b.DrawImage(screen,0,0,blurred.Width,blurred.Height);}}}catch{if(blurred!=null)blurred.Dispose();blurred=null;}RebuildMaterial();}
    void ThemeChanged(object sender,EventArgs e){if(IsDisposed)return;RebuildMaterial();PetTheme.Apply(this);}
    void RebuildMaterial(){if(material!=null)material.Dispose();material=new Bitmap(Math.Max(1,Width),Math.Max(1,Height));using(var g=Graphics.FromImage(material)){g.Clear(PetTheme.Surface);if(blurred!=null){g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawImage(blurred,0,0,Width,Height);using(var tint=new SolidBrush(Color.FromArgb(PetTheme.Dark?190:186,PetTheme.Surface)))g.FillRectangle(tint,0,0,Width,Height);}}Invalidate(true);}
    internal Color SampleMaterial(Point point){if(material==null)return PetTheme.Surface;int r=0,g=0,b=0;for(int dy=-20;dy<=20;dy+=20)for(int dx=-20;dx<=20;dx+=20){Color c=material.GetPixel(Math.Max(0,Math.Min(material.Width-1,point.X+dx)),Math.Max(0,Math.Min(material.Height-1,point.Y+dy)));r+=c.R;g+=c.G;b+=c.B;}return Color.FromArgb(r/9,g/9,b/9);}
    internal void PaintSlice(Graphics g,Rectangle slice){if(material==null){g.Clear(PetTheme.Surface);return;}g.DrawImage(material,new Rectangle(Point.Empty,slice.Size),slice,GraphicsUnit.Pixel);}
    protected override void OnPaintBackground(PaintEventArgs e){PaintSlice(e.Graphics,ClientRectangle);}
    protected override void Dispose(bool disposing){if(disposing){PetTheme.Changed-=ThemeChanged;if(material!=null)material.Dispose();if(blurred!=null)blurred.Dispose();}base.Dispose(disposing);}
}
class GlassPanel : Panel {internal GlassPanel(){DoubleBuffered=true;BackColor=Color.Transparent;}protected override void OnPaintBackground(PaintEventArgs e){PetTheme.PaintUnder(e.Graphics,this);}}

partial class Usagi {double themePoll;void PollTheme(double dt){if(isClone)return;themePoll-=dt;if(themePoll<=0){themePoll=2;PetTheme.RefreshSystem();}}void SaveThemePreference(){bool canSave=SaveEnabled;foreach(var pet in livePets)if(pet.SaveEnabled)canSave=true;if(canSave)PetTheme.Save();}}
