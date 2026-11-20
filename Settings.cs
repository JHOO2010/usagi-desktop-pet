using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
static class SettingArt {
    internal static readonly Color Cream=Color.FromArgb(255,253,246),Ink=Color.FromArgb(99,72,51),Muted=Color.FromArgb(151,131,110),Line=Color.FromArgb(231,220,201),Accent=Color.FromArgb(246,222,174);
    internal static GraphicsPath Round(Rectangle r,int radius){var p=new GraphicsPath();int d=radius*2;p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
    internal static void Icon(Graphics g,string name,int x,int y){var state=g.Save();g.TranslateTransform(x,y);g.SmoothingMode=SmoothingMode.AntiAlias;using(var p=new Pen(Ink,1.4f)){p.StartCap=p.EndCap=LineCap.Round;p.LineJoin=LineJoin.Round;
        if(name=="close"){g.DrawLine(p,5,5,15,15);g.DrawLine(p,15,5,5,15);}
        else if(name=="size"){g.DrawRectangle(p,3,9,8,8);g.DrawLine(p,6,4,17,4);g.DrawLine(p,17,4,17,14);g.DrawLine(p,13,8,17,4);}
        else if(name=="sound"){g.DrawLines(p,new[]{new Point(3,8),new Point(7,8),new Point(12,4),new Point(12,17),new Point(7,13),new Point(3,13),new Point(3,8)});g.DrawArc(p,10,5,8,11,-60,120);}
        else if(name=="startup"){g.DrawArc(p,4,4,13,13,-50,280);g.DrawLine(p,10,1,10,9);}
        else if(name=="music"){g.DrawLine(p,8,4,8,15);g.DrawLine(p,8,4,17,2);g.DrawLine(p,17,2,17,13);g.DrawEllipse(p,3,13,5,4);g.DrawEllipse(p,12,11,5,4);}
        else if(name=="update"){g.DrawArc(p,3,3,14,14,35,280);g.DrawLines(p,new[]{new Point(14,2),new Point(17,6),new Point(12,6)});}
        else if(name=="monitor"){g.DrawRectangle(p,2,3,17,12);g.DrawLine(p,10,15,10,19);g.DrawLine(p,6,19,14,19);}
        else if(name=="settings"){g.DrawLine(p,3,5,18,5);g.DrawLine(p,3,11,18,11);g.DrawLine(p,3,17,18,17);using(var fill=new SolidBrush(Cream)){g.FillEllipse(fill,6,2,5,6);g.FillEllipse(fill,12,8,5,6);g.FillEllipse(fill,5,14,5,6);}g.DrawEllipse(p,6,2,5,6);g.DrawEllipse(p,12,8,5,6);g.DrawEllipse(p,5,14,5,6);}
        else {g.DrawEllipse(p,2,2,17,17);g.DrawLine(p,10,9,10,14);g.DrawEllipse(p,9.5f,5,1,1);}
    }g.Restore(state);}
}
class SettingButton : Button {
    internal bool Chosen,IconOnly;internal string IconName;bool hover;
    internal SettingButton(){FlatStyle=FlatStyle.Flat;SetStyle(ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer,true);Cursor=Cursors.Hand;}
    protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}
    protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
    protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;using(var path=SettingArt.Round(new Rectangle(0,0,Width-1,Height-1),7))using(var fill=new SolidBrush(Chosen?SettingArt.Accent:hover?Color.FromArgb(250,235,204):Color.FromArgb(248,243,232)))g.FillPath(fill,path);
        if(IconName!=null)SettingArt.Icon(g,IconName,IconOnly?(Width-21)/2:10,(Height-21)/2);
        if(!IconOnly)using(var brush=new SolidBrush(Enabled?SettingArt.Ink:SettingArt.Muted))using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center,Trimming=StringTrimming.EllipsisCharacter,FormatFlags=StringFormatFlags.NoWrap})g.DrawString(Text,Font,brush,new RectangleF(IconName==null?3:28,0,Width-(IconName==null?6:32),Height),format);
        if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(3,3,Width-6,Height-6));
    }
}
class SettingToggle : Button {
    bool state;internal SettingToggle(){Cursor=Cursors.Hand;SetStyle(ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer,true);}
    internal void SetState(bool on){if(state==on&&Text.Length>0)return;state=on;Text=on?"켜짐":"끔";AccessibleDescription=Text;Invalidate();}
    protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;using(var b=new SolidBrush(Enabled?SettingArt.Ink:SettingArt.Muted))g.DrawString(state?"켬":"끔",Font,b,0,8);using(var path=SettingArt.Round(new Rectangle(29,5,42,22),11))using(var fill=new SolidBrush(state?SettingArt.Accent:SettingArt.Line))g.FillPath(fill,path);using(var fill=new SolidBrush(SettingArt.Cream))g.FillEllipse(fill,state?51:32,8,16,16);if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(1,1,Width-2,Height-2));}
}
partial class Usagi {
    void PrepareSettingsMenu(){foreach(ToolStripItem item in new System.Collections.Generic.List<ToolStripItem>(MenuItems()))if(item is SizeChoiceItem||item==soundMenu||item==startupMenu||item==updateMenu||item==rapChangeMenu){menu.Items.Remove(item);if(item is SizeChoiceItem)item.Dispose();}settingsReady=true;}
    System.Collections.Generic.IEnumerable<ToolStripItem> MenuItems(){foreach(ToolStripItem item in menu.Items)yield return item;}
    Form CreateSettingsForm(){var form=new Form{Text="우사기 설정",Font=menuFont,FormBorderStyle=FormBorderStyle.None,ShowInTaskbar=false,TopMost=true,StartPosition=FormStartPosition.Manual,ClientSize=new Size(420,438),BackColor=SettingArt.Cream,ForeColor=SettingArt.Ink};
        var anchor=new Point((int)Math.Round(x)+Width/2,(int)Math.Round(y+petTop)-5);var area=Screen.FromPoint(anchor).WorkingArea;
        form.Location=new Point(Math.Max(area.Left+4,Math.Min(area.Right-form.Width-4,anchor.X-form.Width/2)),Math.Max(area.Top+4,Math.Min(area.Bottom-form.Height-4,anchor.Y-form.Height)));
        int tail=Math.Max(24,Math.Min(form.Width-24,anchor.X-form.Left));using(var path=BubbleMenu.Outline(form.Width,form.Height,tail))form.Region=new Region(path);
        form.Paint+=(s,e)=>{var g=e.Graphics;g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;using(var path=BubbleMenu.Outline(form.Width,form.Height,tail))using(var pen=new Pen(Color.FromArgb(164,135,104),1.4f))g.DrawPath(pen,path);SettingArt.Icon(g,"settings",27,26);using(var line=new Pen(SettingArt.Line))g.DrawLine(line,26,294,394,294);};
        var titleFont=new Font(menuFont.FontFamily,15);var smallFont=new Font(menuFont.FontFamily,9);form.Disposed+=(s,e)=>{titleFont.Dispose();smallFont.Dispose();};
        Action<string,int,int,int,Font,Color> text=(value,left,top,width,font,color)=>form.Controls.Add(new Label{Text=value,Location=new Point(left,top),Size=new Size(width,24),Font=font,ForeColor=color,UseCompatibleTextRendering=true});
        text("우사기 설정",56,23,280,titleFont,SettingArt.Ink);text("바꾸면 바로 적용돼요",27,56,300,smallFont,SettingArt.Muted);
        Action<string,string,int> label=(icon,value,top)=>{var row=new Label{Text=value,Location=new Point(26,top),Size=new Size(145,30),Padding=new Padding(30,4,0,0),UseCompatibleTextRendering=true};row.Paint+=(s,e)=>SettingArt.Icon(e.Graphics,icon,1,5);form.Controls.Add(row);};
        Func<string,int,int,int,SettingButton> button=(value,left,top,width)=>{var control=new SettingButton{Text=value,Location=new Point(left,top),Size=new Size(width,32)};form.Controls.Add(control);return control;};
        label("size","크기",92);var sizes=new[]{.75f,1f,1.4f};var names=new[]{"작게","보통","크게"};var sizeButtons=new SettingButton[3];Action refreshSizes=()=>{for(int i=0;i<3;i++){sizeButtons[i].Chosen=Math.Abs(scale-sizes[i])<.01;sizeButtons[i].Invalidate();}};
        for(int i=0;i<3;i++){int index=i;sizeButtons[i]=button(names[i],184+i*70,90,65);sizeButtons[i].Click+=(s,e)=>{scale=sizes[index];SavePreferences();refreshSizes();};}refreshSizes();
        label("sound","효과음",142);var sound=new SettingToggle{Location=new Point(320,140),Size=new Size(72,32),AccessibleName="효과음"};form.Controls.Add(sound);sound.Click+=(s,e)=>{sounds.Enabled=!sounds.Enabled;sounds.Tick(1);SavePreferences();};
        var soundHelp=new Label{Location=new Point(56,172),Size=new Size(310,22),Font=smallFont,ForeColor=SettingArt.Muted,UseCompatibleTextRendering=true};form.Controls.Add(soundHelp);
        label("startup","시작 시 실행",204);var startup=new SettingToggle{Location=new Point(320,202),Size=new Size(72,32),Enabled=!isClone,AccessibleName="Windows 시작 시 실행"};form.Controls.Add(startup);startup.Click+=(s,e)=>startupMenu.PerformClick();
        text(isClone?"시작앱은 원본 우사기에서 설정해요":"Windows에 로그인하면 함께 나와요",56,234,330,smallFont,SettingArt.Muted);
        label("music","랩 음원",263);button("음원 바꾸기…",222,260,170).Click+=(s,e)=>ChooseRapFile(false);
        label("update","업데이트",310);text("v"+PetUpdates.CurrentVersion.ToString(3),56,340,130,smallFont,SettingArt.Muted);
        var update=button("업데이트 확인",192,310,200);update.Enabled=!isClone;if(isClone)update.Text="원본에서 업데이트";update.Click+=(s,e)=>UpdateAction();
        var about=button("정보 보기",26,382,122);about.IconName="info";about.Click+=(s,e)=>ShowPetInfo();
        if(Screen.AllScreens.Length>1){var monitor=button("이 화면에서 산책",178,382,214);monitor.IconName="monitor";monitor.Click+=(s,e)=>{var a=Screen.FromControl(form).WorkingArea;patrolScreen=Screen.FromControl(form);x=a.Left+a.Width/2-Width/2;y=a.Bottom-Height+8;Wake();SavePreferences();form.DialogResult=DialogResult.OK;form.Close();};}
        var close=button("닫기",360,22,32);close.IconName="close";close.IconOnly=true;close.AccessibleName="설정 닫기";close.DialogResult=DialogResult.Cancel;form.CancelButton=close;
        var poll=new Timer{Interval=300};Action refresh=()=>{var endpoint=EndpointProbe.Read();sound.SetState(sounds.Enabled);soundHelp.Text=!sounds.Enabled?"지금은 조용히 산책해요":endpoint!=null&&endpoint.Headphones?"연결된 이어폰으로만 들려요":"이어폰을 연결하면 소리가 나요";if(startupMenu!=null){RefreshStartupMenu();startup.SetState(startupMenu.Text.EndsWith("켜짐"));}if(updateMenu!=null){update.Text=updateMenu.Text;update.Enabled=updateMenu.UpdateEnabled;}};
        poll.Tick+=(s,e)=>refresh();form.Shown+=(s,e)=>{refresh();poll.Start();};form.Disposed+=(s,e)=>poll.Dispose();return form;
    }
    void ShowPetSettings(){using(var form=CreateSettingsForm()){bool previous=infoOpen;infoOpen=true;try{form.ShowDialog(this);}finally{infoOpen=previous;}}}
}
