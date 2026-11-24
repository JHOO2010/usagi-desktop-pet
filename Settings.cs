using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
static class SettingArt {
    internal static Color Cream {get{return PetTheme.Surface;}}internal static Color Ink {get{return PetTheme.Ink;}}internal static Color Muted {get{return PetTheme.Muted;}}internal static Color Line {get{return PetTheme.Line;}}internal static Color Accent {get{return PetTheme.Accent;}}
    internal static GraphicsPath Round(Rectangle r,int radius){var p=new GraphicsPath();int d=radius*2;p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
    internal static void Icon(Graphics g,string name,int x,int y){var state=g.Save();g.TranslateTransform(x,y);g.SmoothingMode=SmoothingMode.AntiAlias;using(var p=new Pen(Ink,1.4f)){p.StartCap=p.EndCap=LineCap.Round;p.LineJoin=LineJoin.Round;
        if(name=="theme"){g.DrawEllipse(p,7,7,6,6);for(int i=0;i<8;i++){double a=i*Math.PI/4;g.DrawLine(p,10+(float)Math.Cos(a)*7,10+(float)Math.Sin(a)*7,10+(float)Math.Cos(a)*9,10+(float)Math.Sin(a)*9);}}
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
    bool chosen;float selectionAmount;internal bool Chosen {get{return chosen;}set{if(chosen==value)return;chosen=value;if(!IsHandleCreated||!Visible){selectionAmount=value?1:0;Invalidate();}else AnimateHover();}}internal bool IconOnly,HideCaption;internal string IconName;bool hover;float hoverAmount;Timer hoverTimer;
    internal SettingButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.Selectable|ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;TabStop=true;AccessibleRole=AccessibleRole.PushButton;Cursor=Cursors.Hand;}
    protected override void OnMouseEnter(EventArgs e){hover=true;AnimateHover();base.OnMouseEnter(e);}
    protected override void OnMouseLeave(EventArgs e){hover=false;AnimateHover();base.OnMouseLeave(e);}
    void AnimateHover(){if(hoverTimer==null){hoverTimer=new Timer{Interval=15};hoverTimer.Tick+=(s,e)=>{hoverAmount=Math.Max(0,Math.Min(1,hoverAmount+(hover?.18f:-.18f)));float target=chosen?1:0;selectionAmount+=(target-selectionAmount)*.34f;if(Math.Abs(target-selectionAmount)<.005f)selectionAmount=target;Invalidate();if((hoverAmount==0||hoverAmount==1)&&selectionAmount==target)hoverTimer.Stop();};}hoverTimer.Start();}
    protected override void Dispose(bool disposing){if(disposing&&hoverTimer!=null)hoverTimer.Dispose();base.Dispose(disposing);}
    protected static Color Blend(Color a,Color b,float u){return Color.FromArgb((int)(a.A+(b.A-a.A)*u),(int)(a.R+(b.R-a.R)*u),(int)(a.G+(b.G-a.G)*u),(int)(a.B+(b.B-a.B)*u));}
    protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;PetTheme.PaintUnder(g,this);g.SmoothingMode=SmoothingMode.AntiAlias;g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;using(var path=SettingArt.Round(new Rectangle(0,0,Width-1,Height-1),11))using(var fill=new SolidBrush(Blend(Blend(PetTheme.Button,PetTheme.Hover,hoverAmount),SettingArt.Accent,selectionAmount)))g.FillPath(fill,path);
        using(var rim=new Pen(Blend(Color.FromArgb(65,PetTheme.Line),PetTheme.Accent,selectionAmount),1))using(var outline=SettingArt.Round(new Rectangle(0,0,Width-1,Height-1),11))g.DrawPath(rim,outline);
        if(IconName!=null)SettingArt.Icon(g,IconName,IconOnly?(Width-21)/2:10,(Height-21)/2);
        if(!IconOnly&&!HideCaption)using(var brush=new SolidBrush(Enabled?Blend(SettingArt.Ink,PetTheme.SelectionInk,selectionAmount):SettingArt.Muted))using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center,Trimming=StringTrimming.EllipsisCharacter,FormatFlags=StringFormatFlags.NoWrap})g.DrawString(Text,Font,brush,new RectangleF(IconName==null?3:28,0,Width-(IconName==null?6:32),Height),format);
        if(Focused&&ShowFocusCues)using(var focus=new Pen(PetTheme.Ink,1.3f))using(var ring=SettingArt.Round(new Rectangle(3,3,Width-7,Height-7),9))g.DrawPath(focus,ring);
    }
}
class SettingToggle : SettingButton {
    protected override void Dispose(bool disposing){if(disposing&&slide!=null)slide.Dispose();base.Dispose(disposing);}
    bool state;float amount;Timer slide;internal SettingToggle(){AccessibleRole=AccessibleRole.CheckButton;}
    internal void SetState(bool on){if(state==on&&Text.Length>0)return;state=on;Text=on?"켜짐":"끔";AccessibleDescription=Text;if(!IsHandleCreated||!Visible)amount=on?1:0;else{if(slide==null){slide=new Timer{Interval=15};slide.Tick+=(s,e)=>{float target=state?1:0;amount+=(target-amount)*.34f;if(Math.Abs(target-amount)<.005f){amount=target;slide.Stop();}Invalidate();};}slide.Start();}Invalidate();}
    protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;PetTheme.PaintUnder(g,this);g.SmoothingMode=SmoothingMode.AntiAlias;g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;TextRenderer.DrawText(g,state?"켬":"끔",Font,new Rectangle(0,0,27,Height),Enabled?SettingArt.Ink:SettingArt.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.NoPadding);using(var path=SettingArt.Round(new Rectangle(29,5,42,22),11))using(var fill=new SolidBrush(Color.FromArgb((int)(SettingArt.Line.R+(SettingArt.Accent.R-SettingArt.Line.R)*amount),(int)(SettingArt.Line.G+(SettingArt.Accent.G-SettingArt.Line.G)*amount),(int)(SettingArt.Line.B+(SettingArt.Accent.B-SettingArt.Line.B)*amount))))g.FillPath(fill,path);using(var fill=new SolidBrush(Blend(PetTheme.Muted,Color.White,amount)))g.FillEllipse(fill,32+19*amount,8,16,16);if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(g,new Rectangle(1,1,Width-2,Height-2),PetTheme.Ink,PetTheme.Surface);}
}
partial class Usagi {
    void PrepareSettingsMenu(){foreach(ToolStripItem item in new System.Collections.Generic.List<ToolStripItem>(MenuItems()))if(item is SizeChoiceItem||item==soundMenu||item==startupMenu||item==updateMenu){menu.Items.Remove(item);if(item is SizeChoiceItem)item.Dispose();}settingsReady=true;}
    System.Collections.Generic.IEnumerable<ToolStripItem> MenuItems(){foreach(ToolStripItem item in menu.Items)yield return item;}
    Form CreateSettingsForm(){PetTheme.RefreshSystem();var form=new GlassBubbleForm{Text="먼작귀 펫 설정",Font=menuFont,FormBorderStyle=FormBorderStyle.None,ShowInTaskbar=false,TopMost=true,StartPosition=FormStartPosition.Manual,ClientSize=new Size(400,430),BackColor=SettingArt.Cream,ForeColor=SettingArt.Ink};
        var anchor=new Point((int)Math.Round(x)+Width/2,(int)Math.Round(y+petTop)-5);var area=Screen.FromPoint(anchor).WorkingArea;
        form.Location=new Point(Math.Max(area.Left+4,Math.Min(area.Right-form.Width-4,anchor.X-form.Width/2)),Math.Max(area.Top+4,Math.Min(area.Bottom-form.Height-4,anchor.Y-form.Height)));
        int tail=Math.Max(34,Math.Min(form.Width-34,anchor.X-form.Left));form.ConfigureShape(tail);
        form.Paint+=(s,e)=>{var g=e.Graphics;g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;using(var line=new Pen(SettingArt.Line))g.DrawLine(line,24,308,376,308);};
        var titleFont=new Font(menuFont.FontFamily,15);var smallFont=new Font(menuFont.FontFamily,9);form.Disposed+=(s,e)=>{titleFont.Dispose();smallFont.Dispose();};
        Action<string,int,int,int,Font,Color> text=(value,left,top,width,font,color)=>form.Controls.Add(new Label{Text=value,Location=new Point(left,top),Size=new Size(width,24),Font=font,Tag=color==SettingArt.Muted?"muted":null,ForeColor=color,UseCompatibleTextRendering=true});
        text("먼작귀 펫",66,29,270,titleFont,SettingArt.Ink);
        var logo=new PictureBox{Name="CharacterLogo",Location=new Point(24,24),Size=new Size(34,34),SizeMode=PictureBoxSizeMode.CenterImage,BackColor=Color.Transparent};form.Controls.Add(logo);
        Icon settingsIcon=null;Action refreshIdentity=()=>{var selector=form.Controls["CharacterSelector"];if(selector!=null)selector.Text="캐릭터 바꾸기";var oldImage=logo.Image;logo.Image=CreateCharacterLogo(32,false);logo.AccessibleName=character.Name+" 로고";if(oldImage!=null)oldImage.Dispose();var previous=settingsIcon;settingsIcon=CreateCharacterIcon();form.Icon=settingsIcon;form.Text="먼작귀 펫 설정 · "+character.Name;if(previous!=null)previous.Dispose();};
        EventHandler identityChanged=(s,e)=>refreshIdentity();CharacterChanged+=identityChanged;refreshIdentity();form.Disposed+=(s,e)=>{CharacterChanged-=identityChanged;if(logo.Image!=null)logo.Image.Dispose();if(settingsIcon!=null)settingsIcon.Dispose();};
        var characterSelect=new SettingButton{Name="CharacterSelector",Text="캐릭터 바꾸기",Location=new Point(26,80),Size=new Size(230,32)};form.Controls.Add(characterSelect);characterSelect.Click+=(s,e)=>ShowCharacterPicker(form,false);
        var addFriend=new SettingButton{Text="친구 추가",Location=new Point(264,80),Size=new Size(110,32)};form.Controls.Add(addFriend);addFriend.Click+=(s,e)=>ShowCharacterPicker(form,true);
        Action<string,string,int> label=(icon,value,top)=>{var row=new Label{Text=value,Location=new Point(26,top-2),Size=new Size(145,32),Padding=new Padding(30,0,0,0),TextAlign=ContentAlignment.MiddleLeft,UseCompatibleTextRendering=false};row.Paint+=(s,e)=>SettingArt.Icon(e.Graphics,icon,1,(row.Height-21)/2);form.Controls.Add(row);};
        Func<string,int,int,int,SettingButton> button=(value,left,top,width)=>{var control=new SettingButton{Text=value,Location=new Point(left,top),Size=new Size(width,32)};form.Controls.Add(control);return control;};
        label("theme","화면 모드",136);var themeButtons=new SettingButton[3];var themeModes=new[]{"light","dark","system"};var themeNames=new[]{"라이트","다크","시스템"};Action refreshTheme=()=>{for(int i=0;i<3;i++){themeButtons[i].Chosen=PetTheme.Mode==themeModes[i];themeButtons[i].AccessibleDescription=themeButtons[i].Chosen?"선택됨":"선택 안 됨";themeButtons[i].Invalidate();}};for(int i=0;i<3;i++){int index=i;themeButtons[i]=button(themeNames[i],186+i*64,134,60);themeButtons[i].Name="Theme-"+themeModes[i];themeButtons[i].AccessibleName=themeNames[i]+" 모드";themeButtons[i].Click+=(s,e)=>{PetTheme.Set(themeModes[index]);SaveThemePreference();refreshTheme();};}EventHandler themeChanged=(s,e)=>refreshTheme();PetTheme.Changed+=themeChanged;form.Disposed+=(s,e)=>PetTheme.Changed-=themeChanged;refreshTheme();
        label("size","크기",178);var sizes=new[]{.75f,1f,1.4f};var names=new[]{"작게","보통","크게"};var sizeButtons=new SettingButton[3];Action refreshSizes=()=>{for(int i=0;i<3;i++){sizeButtons[i].Chosen=Math.Abs(scale-sizes[i])<.01;sizeButtons[i].Invalidate();}};
        for(int i=0;i<3;i++){int index=i;sizeButtons[i]=button(names[i],186+i*64,176,60);sizeButtons[i].Click+=(s,e)=>{scale=sizes[index];SavePreferences();refreshSizes();};}refreshSizes();
        label("sound","효과음",220);var sound=new SoundModeControl{Name="SoundMode",Location=new Point(186,218),Size=new Size(188,32)};form.Controls.Add(sound);sound.SelectionChanged+=(s,e)=>{sounds.Mode=new[]{"off","headphones","all"}[sound.Index];SavePreferences();};
        label("startup","시작 시 실행",262);var startup=new SettingToggle{Location=new Point(302,260),Size=new Size(72,32),Enabled=!isClone,AccessibleName="Windows 시작 시 실행"};form.Controls.Add(startup);startup.Click+=(s,e)=>{startupMenu.PerformClick();RefreshStartupMenu();startup.SetState(startupMenu.Text.EndsWith("켜짐"));};
        startup.AccessibleDescription=isClone?"시작앱은 원본에서 설정":"Windows 로그인 시 함께 실행";
        var updateTitle=new LinkLabel{Name="UpdateTitle",Text="업데이트",LinkBehavior=LinkBehavior.AlwaysUnderline,Location=new Point(56,322),Size=new Size(110,32),TextAlign=ContentAlignment.MiddleLeft,UseCompatibleTextRendering=false,AccessibleName="업데이트 화면 열기"};form.Controls.Add(updateTitle);updateTitle.LinkClicked+=(s,e)=>ShowUpdatePage(form);var updateIcon=new Label{Location=new Point(26,322),Size=new Size(24,32)};updateIcon.Paint+=(s,e)=>SettingArt.Icon(e.Graphics,"update",1,(updateIcon.Height-21)/2);form.Controls.Add(updateIcon);
        var update=button("업데이트 확인",190,322,184);update.Enabled=!isClone;if(isClone)update.Text="원본에서 업데이트";update.Click+=(s,e)=>UpdateAction();
        var about=new LinkLabel{Tag="footer",Text="정보 보기",LinkColor=Color.Black,ActiveLinkColor=Color.Black,VisitedLinkColor=Color.Black,LinkBehavior=LinkBehavior.AlwaysUnderline,TextAlign=ContentAlignment.MiddleCenter,Location=new Point(150,384),Size=new Size(100,18),UseCompatibleTextRendering=true};form.Controls.Add(about);about.LinkClicked+=(s,e)=>ShowSettingsInfo(form);
        if(Screen.AllScreens.Length>1){var monitor=button("이 화면에서 산책",24,390,28);monitor.Size=new Size(28,24);monitor.IconName="monitor";monitor.IconOnly=true;monitor.AccessibleName="이 화면에서 산책하기";monitor.Click+=(s,e)=>{var a=Screen.FromControl(form).WorkingArea;patrolScreen=Screen.FromControl(form);x=a.Left+a.Width/2-Width/2;y=a.Bottom-Height+8;Wake();SavePreferences();form.DialogResult=DialogResult.OK;form.Close();};}
        var close=button("닫기",340,26,32);close.IconName="close";close.IconOnly=true;close.AccessibleName="설정 닫기";close.DialogResult=DialogResult.Cancel;form.CancelButton=close;
        PetTheme.Apply(form);var poll=new Timer{Interval=300};Action refresh=()=>{PetTheme.RefreshSystem();characterSelect.Text="캐릭터 바꾸기";sound.SetIndex(sounds.Mode=="off"?0:sounds.Mode=="all"?2:1);if(startupMenu!=null){RefreshStartupMenu();startup.SetState(startupMenu.Text.EndsWith("켜짐"));}if(updateMenu!=null){update.Text=updateMenu.Text;update.Enabled=updateMenu.UpdateEnabled;}};
        poll.Tick+=(s,e)=>refresh();form.Shown+=(s,e)=>{refresh();poll.Start();};form.Disposed+=(s,e)=>poll.Dispose();return form;
    }
    void ShowSettingsInfo(Form form){
        if(form.Controls.ContainsKey("InfoPage"))return;
        var hidden=new System.Collections.Generic.List<Control>();
        foreach(Control control in form.Controls)if(control.Top>=48){hidden.Add(control);control.Visible=false;}
        var page=new GlassPanel{Name="InfoPage",Location=new Point(24,72),Size=new Size(352,form.Height-104),BackColor=Color.Transparent,AutoScroll=false};form.Controls.Add(page);page.BringToFront();
        var back=new SettingButton{Text="뒤로가기",IconName="back",IconOnly=true,AccessibleName="뒤로가기",Location=new Point(0,0),Size=new Size(32,32)};page.Controls.Add(back);
        back.Click+=(s,e)=>{form.Controls.Remove(page);page.Dispose();foreach(var control in hidden)control.Visible=true;foreach(var control in hidden)if(control is LinkLabel){control.Focus();break;}};
        var infoFont=new Font(menuFont.FontFamily,9.25f);var brandFont=new Font(menuFont.FontFamily,17);var metaFont=new Font(menuFont.FontFamily,9);page.Disposed+=(s,e)=>{infoFont.Dispose();brandFont.Dispose();metaFont.Dispose();};
        page.Controls.Add(new Label{Text="앱 정보",Location=new Point(45,0),Size=new Size(250,32),TextAlign=ContentAlignment.MiddleLeft,UseCompatibleTextRendering=false});
        page.Controls.Add(new Label{Text="먼작귀 펫",Location=new Point(0,50),Size=new Size(240,34),Font=brandFont,TextAlign=ContentAlignment.MiddleLeft,UseCompatibleTextRendering=false});
        page.Controls.Add(new Label{Text="v"+PetUpdates.CurrentVersion.ToString(3),Location=new Point(250,54),Size=new Size(102,28),Font=metaFont,Tag="muted",TextAlign=ContentAlignment.MiddleRight,UseCompatibleTextRendering=false});
        page.Controls.Add(new Label{Text="작업 표시줄 위의 작은 친구들",Location=new Point(0,88),Size=new Size(352,22),Tag="muted",UseCompatibleTextRendering=false});
        page.Controls.Add(new Label{Text="만든 사람",Location=new Point(0,119),Size=new Size(64,22),Font=metaFont,Tag="muted",TextAlign=ContentAlignment.MiddleLeft,UseCompatibleTextRendering=false});
        page.Controls.Add(new Label{Text="JHOO",Location=new Point(68,119),Size=new Size(160,22),TextAlign=ContentAlignment.MiddleLeft,UseCompatibleTextRendering=false});
        AddSettingsInfoLink(page,"포트폴리오 ↗","https://jhoo-diblisher.pages.dev/",152);
        AddSettingsInfoLink(page,"GitHub 저장소 ↗","https://github.com/JHOO2010/usagi-desktop-pet",152);var repoLink=page.Controls[page.Controls.Count-1];repoLink.Left=188;repoLink.Width=164;page.Controls[page.Controls.Count-2].Width=160;
        page.Paint+=(s,e)=>{using(var line=new Pen(SettingArt.Line))e.Graphics.DrawLine(line,0,190,352,190);};
        page.Controls.Add(new InfoCopy{Name="RightsNotice",Text="비공식 팬 프로젝트 · 공식 서비스와 무관합니다.\n캐릭터·이미지·음성·글꼴의 권리는\n각 권리자에게 있으며, 사용·재배포 시\n해당 권리자의 조건을 따릅니다.",Location=new Point(0,203),Size=new Size(352,68),Font=infoFont,UseCompatibleTextRendering=false});
        page.Controls.Add(new InfoCopy{LineSpacing=15,Name="LicenseNotice",Text="앱 코드  별도 라이선스 미부여\n메뉴 글꼴  나눔스퀘어라운드 · NAVER\nSIL Open Font License 1.1",Location=new Point(0,279),Size=new Size(352,45),Font=metaFont,Tag="muted",UseCompatibleTextRendering=false});
        PetTheme.Apply(page);back.Focus();
    }
    static void AddSettingsInfoLink(Control page,string text,string url,int top){var link=new LinkLabel{Text=text,Location=new Point(0,top),Size=new Size(344,26),LinkColor=Color.Black,ActiveLinkColor=Color.Black,UseCompatibleTextRendering=true};link.LinkClicked+=(s,e)=>{try{System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url){UseShellExecute=true});}catch(Exception){MessageBox.Show(page,"링크를 열지 못했어요.\n"+url,"정보");}};page.Controls.Add(link);}
    void ShowPetSettings(){using(var form=CreateSettingsForm()){bool previous=infoOpen;infoOpen=true;try{form.ShowDialog(this);}finally{infoOpen=previous;}}}
}

sealed class InfoCopy : Label {internal int LineSpacing=17;protected override void OnPaint(PaintEventArgs e){int y=0;foreach(string line in Text.Split('\n')){TextRenderer.DrawText(e.Graphics,line,Font,new Rectangle(0,y,Width,LineSpacing),ForeColor,TextFormatFlags.NoPadding|TextFormatFlags.SingleLine);y+=LineSpacing;}}}
