using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Runtime.InteropServices;using System.Windows.Forms;
// Keep native selection, links and keyboard editing behavior; replace only the system scroll chrome.
class ThemedNotes : UserControl {
    [DllImport("user32.dll")]static extern IntPtr SendMessage(IntPtr window,int message,IntPtr wParam,IntPtr lParam);
    internal readonly RichTextBox Editor;internal readonly NotesRail Rail;bool syncing;
    internal ThemedNotes(){SetStyle(ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;Editor=new RichTextBox{BorderStyle=BorderStyle.None,ReadOnly=true,ScrollBars=RichTextBoxScrollBars.None,WordWrap=true,DetectUrls=true,BackColor=PetTheme.Surface,ForeColor=PetTheme.Ink};Rail=new NotesRail(this);Controls.Add(Editor);Controls.Add(Rail);Editor.VScroll+=(s,e)=>Sync();Editor.TextChanged+=(s,e)=>Sync();Editor.FontChanged+=(s,e)=>Sync();Editor.Resize+=(s,e)=>Sync();}
    internal int FirstLine {get{return Editor.IsHandleCreated?(int)SendMessage(Editor.Handle,0xCE,IntPtr.Zero,IntPtr.Zero):0;}}
    internal int PageLines {get{return Math.Max(1,(int)((Editor.ClientSize.Height-5)/Math.Max(1,Editor.Font.GetHeight())));}}
    internal int LastLine {get{return Math.Max(0,Editor.GetLineFromCharIndex(Editor.TextLength)+1-PageLines);}}
    internal void ScrollTo(int value){value=Math.Max(0,Math.Min(LastLine,value));SendMessage(Editor.Handle,0xB6,IntPtr.Zero,(IntPtr)(value-FirstLine));Sync();}
    internal void Sync(){if(syncing||IsDisposed)return;syncing=true;try{Rail.Visible=LastLine>0;Rail.Invalidate();}finally{syncing=false;}}
    protected override void OnLayout(LayoutEventArgs e){base.OnLayout(e);if(Editor==null)return;Editor.Bounds=new Rectangle(0,0,Math.Max(1,Width-18),Math.Max(1,Height));Rail.Bounds=new Rectangle(Math.Max(0,Width-14),0,14,Height);Sync();}
    protected override void OnFontChanged(EventArgs e){base.OnFontChanged(e);if(Editor!=null)Editor.Font=Font;}
    protected override void OnPaintBackground(PaintEventArgs e){PetTheme.PaintUnder(e.Graphics,this);}
    internal sealed class NotesRail : Control {
        readonly ThemedNotes owner;bool dragging,hover;int grab;
        internal NotesRail(ThemedNotes owner){this.owner=owner;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);TabStop=true;AccessibleRole=AccessibleRole.ScrollBar;AccessibleName="문서 세로 스크롤";Cursor=Cursors.Hand;}
        Rectangle Thumb {get{int height=Math.Max(28,(int)(Height*owner.PageLines/(double)(owner.LastLine+owner.PageLines))),travel=Math.Max(0,Height-height);return new Rectangle(4,(int)(travel*Math.Min(owner.LastLine,owner.FirstLine)/(double)Math.Max(1,owner.LastLine)),6,height);}}
        protected override void OnPaint(PaintEventArgs e){PetTheme.PaintUnder(e.Graphics,this);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var track=new Pen(Color.FromArgb(55,PetTheme.Line),2))e.Graphics.DrawLine(track,7,3,7,Height-3);var thumb=Thumb;using(var shape=SettingArt.Round(thumb,3))using(var brush=new SolidBrush(dragging||hover?PetTheme.Accent:Color.FromArgb(180,PetTheme.Muted)))e.Graphics.FillPath(brush,shape);}
        protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
        protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button!=MouseButtons.Left)return;Focus();var thumb=Thumb;if(e.Y>=thumb.Top&&e.Y<=thumb.Bottom){dragging=true;grab=e.Y-thumb.Top;Capture=true;}else owner.ScrollTo(owner.FirstLine+(e.Y<thumb.Top?-owner.PageLines:owner.PageLines));}
        protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(dragging){int travel=Height-Thumb.Height;if(travel>0)owner.ScrollTo((int)Math.Round((e.Y-grab)*owner.LastLine/(double)travel));}}
        protected override void OnMouseUp(MouseEventArgs e){dragging=false;Capture=false;Invalidate();base.OnMouseUp(e);}
        protected override void OnMouseWheel(MouseEventArgs e){owner.ScrollTo(owner.FirstLine-e.Delta/120*3);base.OnMouseWheel(e);}
        protected override bool IsInputKey(Keys keyData){var key=keyData&Keys.KeyCode;return key==Keys.Left||key==Keys.Right||key==Keys.Up||key==Keys.Down||key==Keys.Home||key==Keys.End||key==Keys.PageUp||key==Keys.PageDown||base.IsInputKey(keyData);}
        protected override void OnKeyDown(KeyEventArgs e){int first=owner.FirstLine;switch(e.KeyCode){case Keys.Up:first--;break;case Keys.Down:first++;break;case Keys.PageUp:first-=owner.PageLines;break;case Keys.PageDown:first+=owner.PageLines;break;case Keys.Home:first=0;break;case Keys.End:first=owner.LastLine;break;default:base.OnKeyDown(e);return;}owner.ScrollTo(first);e.Handled=true;}
    }
}
