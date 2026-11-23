using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
static class SettingArt {
    internal static Color Cream {get{return PetTheme.Surface;}}internal static Color Ink {get{return PetTheme.Ink;}}internal static Color Muted {get{return PetTheme.Muted;}}internal static Color Line {get{return PetTheme.Line;}}internal static Color Accent {get{return PetTheme.Accent;}}
    internal static GraphicsPath Round(Rectangle r,int radius){var p=new GraphicsPath();int d=radius*2;p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
    internal static void Icon(Graphics g,string name,int x,int y){var state=g.Save();g.TranslateTransform(x,y);g.SmoothingMode=SmoothingMode.AntiAlias;using(var p=new Pen(Ink,1.4f)){p.StartCap=p.EndCap=LineCap.Round;p.LineJoin=LineJoin.Round;
        if(name=="theme"){g.DrawArc(p,3,3,14,14,80,270);g.DrawArc(p,8,1,10,11,80,190);}
        else if(name=="back"){g.DrawLine(p,4,10,17,10);g.DrawLine(p,4,10,10,4);g.DrawLine(p,4,10,10,16);}
        else if(name=="close"){g.DrawLine(p,5,5,15,15);g.DrawLine(p,15,5,5,15);}
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
class SettingButton : Control, IButtonControl {
    public DialogResult DialogResult {get;set;}
    public void NotifyDefault(bool value){}
    public void PerformClick(){if(Enabled)OnClick(EventArgs.Empty);}
    protected override void OnClick(EventArgs e){base.OnClick(e);if(DialogResult!=DialogResult.None){var form=FindForm();if(form!=null){form.DialogResult=DialogResult;form.Close();}}}
    protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Space||e.KeyCode==Keys.Enter){PerformClick();e.Handled=true;}}
    protected override void OnPaintBackground(PaintEventArgs e){PetTheme.PaintUnder(e.Graphics,this);}
    internal bool Chosen,IconOnly,HideCaption;internal string IconName;bool hover;float hoverAmount;Timer hoverTimer;
    internal SettingButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.Selectable|ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;TabStop=true;AccessibleRole=AccessibleRole.PushButton;Cursor=Cursors.Hand;}
    protected override void OnMouseEnter(EventArgs e){hover=true;AnimateHover();base.OnMouseEnter(e);}
    protected override void OnMouseLeave(EventArgs e){hover=false;AnimateHover();base.OnMouseLeave(e);}
    void AnimateHover(){if(hoverTimer==null){hoverTimer=new Timer{Interval=15};hoverTimer.Tick+=(s,e)=>{hoverAmount=Math.Max(0,Math.Min(1,hoverAmount+(hover?.18f:-.18f)));Invalidate();if(hoverAmount==0||hoverAmount==1)hoverTimer.Stop();};}hoverTimer.Start();}
    protected override void Dispose(bool disposing){if(disposing&&hoverTimer!=null)hoverTimer.Dispose();base.Dispose(disposing);}
    static Color Blend(Color a,Color b,float u){return Color.FromArgb((int)(a.A+(b.A-a.A)*u),(int)(a.R+(b.R-a.R)*u),(int)(a.G+(b.G-a.G)*u),(int)(a.B+(b.B-a.B)*u));}
    protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;PetTheme.PaintUnder(g,this);g.SmoothingMode=SmoothingMode.AntiAlias;g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;using(var path=SettingArt.Round(new Rectangle(0,0,Width-1,Height-1),7))using(var fill=new SolidBrush(Chosen?SettingArt.Accent:Blend(PetTheme.Button,PetTheme.Hover,hoverAmount)))g.FillPath(fill,path);
        using(var rim=new Pen(Chosen?PetTheme.Accent:Color.FromArgb(65,PetTheme.Line),1))using(var outline=SettingArt.Round(new Rectangle(0,0,Width-1,Height-1),7))g.DrawPath(rim,outline);
        if(IconName!=null)SettingArt.Icon(g,IconName,IconOnly?(Width-21)/2:10,(Height-21)/2);
        if(!IconOnly&&!HideCaption)using(var brush=new SolidBrush(Enabled?(Chosen?PetTheme.SelectionInk:SettingArt.Ink):SettingArt.Muted))using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center,Trimming=StringTrimming.EllipsisCharacter,FormatFlags=StringFormatFlags.NoWrap})g.DrawString(Text,Font,brush,new RectangleF(IconName==null?3:28,0,Width-(IconName==null?6:32),Height),format);
        if(Focused&&ShowFocusCues)using(var focus=new Pen(PetTheme.Ink,1.3f))using(var ring=SettingArt.Round(new Rectangle(3,3,Width-7,Height-7),5))g.DrawPath(focus,ring);
    }
}
class SettingToggle : SettingButton {
    bool state;internal SettingToggle(){AccessibleRole=AccessibleRole.CheckButton;}
    internal void SetState(bool on){if(state==on&&Text.Length>0)return;state=on;Text=on?"켜짐":"끔";AccessibleDescription=Text;Invalidate();}
    protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;PetTheme.PaintUnder(g,this);g.SmoothingMode=SmoothingMode.AntiAlias;g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;using(var b=new SolidBrush(Enabled?(Chosen?PetTheme.SelectionInk:SettingArt.Ink):SettingArt.Muted))using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center,FormatFlags=StringFormatFlags.NoWrap})g.DrawString(state?"켬":"끔",Font,b,new RectangleF(0,0,27,Height),format);using(var path=SettingArt.Round(new Rectangle(29,5,42,22),11))using(var fill=new SolidBrush(state?SettingArt.Accent:SettingArt.Line))g.FillPath(fill,path);using(var fill=new SolidBrush(state?Color.White:PetTheme.Muted))g.FillEllipse(fill,state?51:32,8,16,16);if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(g,new Rectangle(1,1,Width-2,Height-2),PetTheme.Ink,PetTheme.Surface);}
}
partial class Usagi {
    void PrepareSettingsMenu(){foreach(ToolStripItem item in new System.Collections.Generic.List<ToolStripItem>(MenuItems()))if(item is SizeChoiceItem||item==soundMenu||item==startupMenu||item==updateMenu||item==rapChangeMenu){menu.Items.Remove(item);if(item is SizeChoiceItem)item.Dispose();}settingsReady=true;}
    System.Collections.Generic.IEnumerable<ToolStripItem> MenuItems(){foreach(ToolStripItem item in menu.Items)yield return item;}
    Form CreateSettingsForm(){PetTheme.RefreshSystem();var form=new GlassBubbleForm{Text="먼작귀 펫 설정",Font=menuFont,FormBorderStyle=FormBorderStyle.None,ShowInTaskbar=false,TopMost=true,StartPosition=FormStartPosition.Manual,ClientSize=new Size(400,430),BackColor=SettingArt.Cream,ForeColor=SettingArt.Ink};
        var anchor=new Point((int)Math.Round(x)+Width/2,(int)Math.Round(y+petTop)-5);var area=Screen.FromPoint(anchor).WorkingArea;
        form.Location=new Point(Math.Max(area.Left+4,Math.Min(area.Right-form.Width-4,anchor.X-form.Width/2)),Math.Max(area.Top+4,Math.Min(area.Bottom-form.Height-4,anchor.Y-form.Height)));
        int tail=Math.Max(24,Math.Min(form.Width-24,anchor.X-form.Left));using(var path=BubbleMenu.Outline(form.Width,form.Height,tail))form.Region=new Region(path);
        form.Paint+=(s,e)=>{var g=e.Graphics;g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;using(var path=BubbleMenu.Outline(form.Width,form.Height,tail))using(var pen=new Pen(PetTheme.Border,1.2f))g.DrawPath(pen,path);using(var line=new Pen(SettingArt.Line))g.DrawLine(line,24,326,376,326);};
        var titleFont=new Font(menuFont.FontFamily,15);var smallFont=new Font(menuFont.FontFamily,9);form.Disposed+=(s,e)=>{titleFont.Dispose();smallFont.Dispose();};
        Action<string,int,int,int,Font,Color> text=(value,left,top,width,font,color)=>form.Controls.Add(new Label{Text=value,Location=new Point(left,top),Size=new Size(width,24),Font=font,Tag=color==SettingArt.Muted?"muted":null,ForeColor=color,UseCompatibleTextRendering=true});
        text("먼작귀 펫",66,23,270,titleFont,SettingArt.Ink);
        var logo=new PictureBox{Name="CharacterLogo",Location=new Point(24,17),Size=new Size(34,34),SizeMode=PictureBoxSizeMode.CenterImage,BackColor=Color.Transparent};form.Controls.Add(logo);
        Icon settingsIcon=null;Action refreshIdentity=()=>{var selector=form.Controls["CharacterSelector"];if(selector!=null)selector.Text="캐릭터 · "+character.Name;var oldImage=logo.Image;logo.Image=CreateCharacterLogo(32,false);logo.AccessibleName=character.Name+" 로고";if(oldImage!=null)oldImage.Dispose();var previous=settingsIcon;settingsIcon=CreateCharacterIcon();form.Icon=settingsIcon;form.Text="먼작귀 펫 설정 · "+character.Name;if(previous!=null)previous.Dispose();};
        EventHandler identityChanged=(s,e)=>refreshIdentity();CharacterChanged+=identityChanged;refreshIdentity();form.Disposed+=(s,e)=>{CharacterChanged-=identityChanged;if(logo.Image!=null)logo.Image.Dispose();if(settingsIcon!=null)settingsIcon.Dispose();};
        var characterSelect=new SettingButton{Name="CharacterSelector",Text="캐릭터 · "+character.Name,Location=new Point(26,55),Size=new Size(230,30)};form.Controls.Add(characterSelect);characterSelect.Click+=(s,e)=>ShowCharacterPicker(form,false);
        var addFriend=new SettingButton{Text="친구 추가",Location=new Point(264,55),Size=new Size(110,30)};form.Controls.Add(addFriend);addFriend.Click+=(s,e)=>ShowCharacterPicker(form,true);
        Action<string,string,int> label=(icon,value,top)=>{var row=new Label{Text=value,Location=new Point(26,top),Size=new Size(145,30),Padding=new Padding(30,4,0,0),UseCompatibleTextRendering=true};row.Paint+=(s,e)=>SettingArt.Icon(e.Graphics,icon,1,5);form.Controls.Add(row);};
        Func<string,int,int,int,SettingButton> button=(value,left,top,width)=>{var control=new SettingButton{Text=value,Location=new Point(left,top),Size=new Size(width,32)};form.Controls.Add(control);return control;};
        label("theme","화면 모드",100);var themeButtons=new SettingButton[3];var themeModes=new[]{"light","dark","system"};var themeNames=new[]{"라이트","다크","시스템"};Action refreshTheme=()=>{for(int i=0;i<3;i++){themeButtons[i].Chosen=PetTheme.Mode==themeModes[i];themeButtons[i].AccessibleDescription=themeButtons[i].Chosen?"선택됨":"선택 안 됨";themeButtons[i].Invalidate();}};for(int i=0;i<3;i++){int index=i;themeButtons[i]=button(themeNames[i],180+i*64,98,60);themeButtons[i].Name="Theme-"+themeModes[i];themeButtons[i].AccessibleName=themeNames[i]+" 모드";themeButtons[i].Click+=(s,e)=>{PetTheme.Set(themeModes[index]);SaveThemePreference();refreshTheme();};}EventHandler themeChanged=(s,e)=>refreshTheme();PetTheme.Changed+=themeChanged;form.Disposed+=(s,e)=>PetTheme.Changed-=themeChanged;refreshTheme();
        label("size","크기",142);var sizes=new[]{.75f,1f,1.4f};var names=new[]{"작게","보통","크게"};var sizeButtons=new SettingButton[3];Action refreshSizes=()=>{for(int i=0;i<3;i++){sizeButtons[i].Chosen=Math.Abs(scale-sizes[i])<.01;sizeButtons[i].Invalidate();}};
        for(int i=0;i<3;i++){int index=i;sizeButtons[i]=button(names[i],180+i*64,140,60);sizeButtons[i].Click+=(s,e)=>{scale=sizes[index];SavePreferences();refreshSizes();};}refreshSizes();
        label("sound","효과음",184);var sound=new SettingToggle{Location=new Point(296,182),Size=new Size(72,32),AccessibleName="효과음"};form.Controls.Add(sound);sound.Click+=(s,e)=>{sounds.Enabled=!sounds.Enabled;sounds.Tick(1);SavePreferences();};
        var soundHelp=new Label{Location=new Point(56,214),Size=new Size(310,22),Font=smallFont,Tag="muted",ForeColor=SettingArt.Muted,UseCompatibleTextRendering=true};form.Controls.Add(soundHelp);
        label("startup","시작 시 실행",240);var startup=new SettingToggle{Location=new Point(296,238),Size=new Size(72,32),Enabled=!isClone,AccessibleName="Windows 시작 시 실행"};form.Controls.Add(startup);startup.Click+=(s,e)=>startupMenu.PerformClick();
        startup.AccessibleDescription=isClone?"시작앱은 원본에서 설정":"Windows 로그인 시 함께 실행";
        label("music","우사기 랩",282);button("음원 바꾸기…",218,280,156).Click+=(s,e)=>ChooseRapFile(false);
        label("update","업데이트",342);var changes=new LinkLabel{Text="변경 내용",Location=new Point(56,374),Size=new Size(120,18),Font=smallFont,LinkColor=SettingArt.Ink,ActiveLinkColor=Color.Black,UseCompatibleTextRendering=true};form.Controls.Add(changes);changes.LinkClicked+=(s,e)=>ShowReleaseNotesPage(form,availableUpdate==null?"v"+PetUpdates.CurrentVersion.ToString(3):availableUpdate.Tag,availableUpdate==null?CurrentReleaseNotes():availableUpdate.Notes);
        var checkedAt=new Label{Location=new Point(190,374),Size=new Size(184,18),Font=smallFont,Tag="muted",ForeColor=SettingArt.Muted,UseCompatibleTextRendering=true};form.Controls.Add(checkedAt);
        var update=button("업데이트 확인",190,340,184);update.Enabled=!isClone;if(isClone)update.Text="원본에서 업데이트";update.Click+=(s,e)=>UpdateAction();
        var about=new LinkLabel{Text="정보 보기",LinkColor=Color.Black,ActiveLinkColor=Color.Black,VisitedLinkColor=Color.Black,LinkBehavior=LinkBehavior.AlwaysUnderline,TextAlign=ContentAlignment.MiddleCenter,Location=new Point(150,392),Size=new Size(100,18),UseCompatibleTextRendering=true};form.Controls.Add(about);about.LinkClicked+=(s,e)=>ShowSettingsInfo(form);
        if(Screen.AllScreens.Length>1){var monitor=button("이 화면에서 산책",24,390,28);monitor.Size=new Size(28,24);monitor.IconName="monitor";monitor.IconOnly=true;monitor.AccessibleName="이 화면에서 산책하기";monitor.Click+=(s,e)=>{var a=Screen.FromControl(form).WorkingArea;patrolScreen=Screen.FromControl(form);x=a.Left+a.Width/2-Width/2;y=a.Bottom-Height+8;Wake();SavePreferences();form.DialogResult=DialogResult.OK;form.Close();};}
        var close=button("닫기",340,22,32);close.IconName="close";close.IconOnly=true;close.AccessibleName="설정 닫기";close.DialogResult=DialogResult.Cancel;form.CancelButton=close;
        PetTheme.Apply(form);var poll=new Timer{Interval=300};Action refresh=()=>{PetTheme.RefreshSystem();characterSelect.Text="캐릭터 · "+character.Name;var endpoint=EndpointProbe.Read();sound.SetState(sounds.Enabled);soundHelp.Text=!sounds.Enabled?"지금은 조용히 산책해요":endpoint!=null&&endpoint.Headphones?"연결된 이어폰으로만 들려요":"이어폰을 연결하면 소리가 나요";if(startupMenu!=null){RefreshStartupMenu();startup.SetState(startupMenu.Text.EndsWith("켜짐"));}if(updateMenu!=null){update.Text=updateMenu.Text;update.Enabled=updateMenu.UpdateEnabled;checkedAt.Text=updateCheckStatus;}};
        poll.Tick+=(s,e)=>refresh();form.Shown+=(s,e)=>{refresh();poll.Start();};form.Disposed+=(s,e)=>poll.Dispose();return form;
    }
    void ShowSettingsInfo(Form form){
        if(form.Controls.ContainsKey("InfoPage"))return;
        var hidden=new System.Collections.Generic.List<Control>();
        foreach(Control control in form.Controls)if(control.Top>=48){hidden.Add(control);control.Visible=false;}
        var page=new GlassPanel{Name="InfoPage",Location=new Point(24,68),Size=new Size(352,form.Height-94),BackColor=SettingArt.Cream,AutoScroll=true};form.Controls.Add(page);page.BringToFront();
        var back=new SettingButton{Text="뒤로가기",IconName="back",IconOnly=true,AccessibleName="뒤로가기",Location=new Point(0,0),Size=new Size(32,32)};page.Controls.Add(back);
        back.Click+=(s,e)=>{form.Controls.Remove(page);page.Dispose();foreach(var control in hidden)control.Visible=true;foreach(var control in hidden)if(control is LinkLabel){control.Focus();break;}};
        page.Controls.Add(new Label{Text="먼작귀 펫",Location=new Point(0,46),Size=new Size(344,26),UseCompatibleTextRendering=true});
        page.Controls.Add(new Label{Text="v"+PetUpdates.CurrentVersion.ToString(3)+" · 만든 사람 JHOO",Location=new Point(0,74),Size=new Size(344,24),Tag="muted",ForeColor=SettingArt.Muted,UseCompatibleTextRendering=true});
        AddSettingsInfoLink(page,"포트폴리오","https://jhoo-diblisher.pages.dev/",108);
        AddSettingsInfoLink(page,"GitHub 저장소","https://github.com/JHOO2010/usagi-desktop-pet",142);
        page.Controls.Add(new Label{Text="비공식 팬 프로젝트로 공식 서비스와 관계없습니다.\n캐릭터·이미지·음성·글꼴의 권리는 각 권리자에게 있습니다. 제3자 자료의 사용·재배포에는 각 권리자의 조건이 적용됩니다.\n\n앱 코드에는 별도 오픈소스 라이선스를 부여하지 않았습니다.\n메뉴 글꼴: 나눔스퀘어라운드 · NAVER · SIL OFL 1.1",Location=new Point(0,180),Size=new Size(344,180),UseCompatibleTextRendering=true});PetTheme.Apply(page);back.Focus();
    }
    static void AddSettingsInfoLink(Control page,string text,string url,int top){var link=new LinkLabel{Text=text,Location=new Point(0,top),Size=new Size(344,26),LinkColor=Color.Black,ActiveLinkColor=Color.Black,UseCompatibleTextRendering=true};link.LinkClicked+=(s,e)=>{try{System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url){UseShellExecute=true});}catch(Exception){MessageBox.Show(page,"링크를 열지 못했어요.\n"+url,"정보");}};page.Controls.Add(link);}
    void ShowPetSettings(){using(var form=CreateSettingsForm()){bool previous=infoOpen;infoOpen=true;try{form.ShowDialog(this);}finally{infoOpen=previous;}}}
}
