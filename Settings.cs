using System;
using System.Drawing;
using System.Windows.Forms;
partial class Usagi {
    void PrepareSettingsMenu(){foreach(ToolStripItem item in new System.Collections.Generic.List<ToolStripItem>(MenuItems()))if(item is SizeChoiceItem||item==soundMenu||item==startupMenu||item==updateMenu||item==rapChangeMenu){menu.Items.Remove(item);if(item is SizeChoiceItem)item.Dispose();}settingsReady=true;}
    System.Collections.Generic.IEnumerable<ToolStripItem> MenuItems(){foreach(ToolStripItem item in menu.Items)yield return item;}
    Form CreateSettingsForm(){var form=new Form{Text="우사기 설정",Font=menuFont,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,ShowInTaskbar=false,TopMost=true,StartPosition=FormStartPosition.Manual,ClientSize=new Size(430,440),BackColor=Color.FromArgb(255,253,246),ForeColor=Color.FromArgb(99,72,51)};
        var area=Screen.FromPoint(new Point((int)x+Width/2,(int)y+Height/2)).WorkingArea;form.Location=new Point(Math.Max(area.Left+8,Math.Min(area.Right-form.Width-8,(int)x+Width/2-form.Width/2)),Math.Max(area.Top+8,Math.Min(area.Bottom-form.Height-8,(int)(y+petTop)-form.Height-12)));
        var titleFont=new Font(menuFont.FontFamily,16);form.Disposed+=(s,e)=>titleFont.Dispose();form.Controls.Add(new Label{Text="우사기 설정",Font=titleFont,AutoSize=true,Location=new Point(24,22),UseCompatibleTextRendering=true});
        Action<string,int> label=(text,top)=>form.Controls.Add(new Label{Text=text,Location=new Point(26,top+7),Size=new Size(135,24),UseCompatibleTextRendering=true});
        Func<string,int,int,int,Button> button=(text,left,top,width)=>{var b=new Button{Text=text,Location=new Point(left,top),Size=new Size(width,32),FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(248,243,232),UseCompatibleTextRendering=true};b.FlatAppearance.BorderColor=Color.FromArgb(231,220,201);form.Controls.Add(b);return b;};
        label("크기",78);var sizes=new[]{.75f,1f,1.4f};var names=new[]{"작게","보통","크게"};var sizeButtons=new Button[3];Action refreshSizes=()=>{for(int i=0;i<3;i++)sizeButtons[i].BackColor=Math.Abs(scale-sizes[i])<.01?Color.FromArgb(246,222,174):Color.FromArgb(248,243,232);};
        for(int i=0;i<3;i++){int index=i;sizeButtons[i]=button(names[i],176+i*74,78,68);sizeButtons[i].Click+=(s,e)=>{scale=sizes[index];SavePreferences();refreshSizes();};}refreshSizes();
        label("효과음",125);var sound=button("",176,125,216);sound.Click+=(s,e)=>{sounds.Enabled=!sounds.Enabled;sounds.Tick(1);SavePreferences();};
        label("시작 시 실행",172);var startup=button("",176,172,216);startup.Enabled=!isClone;if(isClone)startup.Text="원본에서 설정";startup.Click+=(s,e)=>startupMenu.PerformClick();
        label("랩 음원",219);button("음원 바꾸기…",176,219,216).Click+=(s,e)=>ChooseRapFile(false);
        var line=new Label{BackColor=Color.FromArgb(231,220,201),Location=new Point(26,272),Size=new Size(366,1)};form.Controls.Add(line);
        var version=new Label{Text="v"+PetUpdates.CurrentVersion.ToString(3),Location=new Point(26,292),Size=new Size(135,26),UseCompatibleTextRendering=true};form.Controls.Add(version);
        var update=button("업데이트 확인",176,285,216);update.Enabled=!isClone;if(isClone)update.Text="원본에서 업데이트";update.Click+=(s,e)=>UpdateAction();
        button("정보 보기",26,337,112).Click+=(s,e)=>ShowPetInfo();
        if(Screen.AllScreens.Length>1)button("이 화면에서 산책",150,337,242).Click+=(s,e)=>{var a=Screen.FromControl(form).WorkingArea;patrolScreen=Screen.FromControl(form);x=a.Left+a.Width/2-Width/2;y=a.Bottom-Height+8;Wake();SavePreferences();};
        var close=button("닫기",302,390,90);close.DialogResult=DialogResult.OK;form.CancelButton=close;
        var poll=new Timer{Interval=200};Action refresh=()=>{var endpoint=EndpointProbe.Read();sound.Text=!sounds.Enabled?"끔":endpoint!=null&&endpoint.Headphones?"켜짐 · 이어폰":"켜짐 · 이어폰 연결 대기";if(startupMenu!=null){RefreshStartupMenu();startup.Text=startupMenu.Text.EndsWith("켜짐")?"켜짐":"끔";}if(updateMenu!=null){update.Text=updateMenu.Text;update.Enabled=updateMenu.UpdateEnabled;}};
        poll.Tick+=(s,e)=>refresh();form.Shown+=(s,e)=>{refresh();poll.Start();};form.FormClosed+=(s,e)=>{poll.Dispose();};form.Disposed+=(s,e)=>{poll.Dispose();};return form;
    }
    void ShowPetSettings(){using(var form=CreateSettingsForm()){bool previous=infoOpen;infoOpen=true;try{form.ShowDialog(this);}finally{infoOpen=previous;}}}
}
