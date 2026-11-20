using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

class UpdateStatusItem : ToolStripItem {
    readonly Action update,info;bool infoFocus;
    internal bool UpdateEnabled=true;
    internal Rectangle InfoBounds {get{return new Rectangle(Width-70,3,68,Height-6);}}
    public UpdateStatusItem(Action update,Action info){this.update=update;this.info=info;Text="업데이트 확인";AutoSize=false;Size=new Size(220,33);AccessibleName="업데이트 확인 / 정보 보기";}
    public void Arrange(Rectangle bounds){SetBounds(bounds);}
    protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;g.SmoothingMode=SmoothingMode.AntiAlias;
        using(var font=new Font(Font.FontFamily,9f))using(var ink=new SolidBrush(UpdateEnabled?Color.FromArgb(89,57,34):Color.Gray))using(var format=new StringFormat{LineAlignment=StringAlignment.Center,Trimming=StringTrimming.EllipsisCharacter,FormatFlags=StringFormatFlags.NoWrap})g.DrawString(Text,font,ink,new RectangleF(7,0,Width-80,Height),format);
        var r=InfoBounds;using(var fill=new SolidBrush(infoFocus?Color.FromArgb(246,222,174):Color.FromArgb(248,243,232)))g.FillRectangle(fill,r);
        using(var font=new Font(Font.FontFamily,9f))using(var ink=new SolidBrush(Color.FromArgb(89,57,34)))using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})g.DrawString("정보 보기",font,ink,r,format);
    }
    protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);infoFocus=InfoBounds.Contains(e.Location);Invalidate();}
    protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);infoFocus=false;Invalidate();}
    protected override void OnMouseUp(MouseEventArgs e){infoFocus=InfoBounds.Contains(e.Location);base.OnMouseUp(e);}
    protected override void OnClick(EventArgs e){Activate(infoFocus);}
    protected override bool ProcessDialogKey(Keys keyData){if(keyData==Keys.Right||keyData==Keys.Left){infoFocus=keyData==Keys.Right;Invalidate();return true;}return base.ProcessDialogKey(keyData);}
    void Activate(bool showInfo){var dropdown=Owner as ToolStripDropDown;if(dropdown!=null)dropdown.Close();if(showInfo)info();else if(UpdateEnabled)update();}
}

partial class Usagi {
    bool infoOpen;
    void ShowPetInfo(){using(var dialog=new Form{Text="우사기 정보",FormBorderStyle=FormBorderStyle.None,StartPosition=FormStartPosition.Manual,ClientSize=new Size(390,380),BackColor=Color.FromArgb(255,253,241),ForeColor=Color.FromArgb(89,57,34),ShowInTaskbar=false,TopMost=true})
        using(var body=new Font(menuFont.FontFamily,10f))using(var title=new Font(menuFont.FontFamily,15f)){
        var anchor=new Point((int)Math.Round(x)+Width/2,(int)Math.Round(y+petTop)-5);
        var area=Screen.FromPoint(anchor).WorkingArea;
        dialog.ClientSize=new Size(Math.Min(390,area.Width-8),Math.Min(380,area.Height-8));
        dialog.Location=new Point(Math.Max(area.Left+4,Math.Min(area.Right-dialog.Width-4,anchor.X-dialog.Width/2)),Math.Max(area.Top+4,Math.Min(area.Bottom-dialog.Height-4,anchor.Y-dialog.Height)));
        int tail=Math.Max(24,Math.Min(dialog.Width-24,anchor.X-dialog.Left));
        dialog.Font=body;using(var outline=BubbleMenu.Outline(dialog.Width,dialog.Height,tail))dialog.Region=new Region(outline);
        dialog.Paint+=(s,e)=>{e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var outline=BubbleMenu.Outline(dialog.Width,dialog.Height,tail))using(var pen=new Pen(dialog.ForeColor,2))e.Graphics.DrawPath(pen,outline);};
        dialog.Controls.Add(new Label{Text="우사기 산책 펫",Font=title,Location=new Point(24,22),Size=new Size(340,35)});
        dialog.Controls.Add(new Label{Text="v"+PetUpdates.CurrentVersion.ToString(3)+" · 만든 사람 JHOO",Location=new Point(26,65),Size=new Size(340,25)});
        AddInfoLink(dialog,"포트폴리오 ↗","https://jhoo-diblisher.pages.dev/",105);
        AddInfoLink(dialog,"GitHub 저장소 ↗","https://github.com/JHOO2010/usagi-desktop-pet",140);
        dialog.Controls.Add(new Label{Text="비공식 팬 프로젝트로 공식 서비스와 관계없습니다.\n캐릭터·이미지·음성·글꼴의 권리는 각 권리자에게 있습니다. 제3자 자료의 사용·재배포에는 각 권리자의 조건이 적용됩니다.\n앱 코드에는 별도 오픈소스 라이선스를 부여하지 않았습니다.\n메뉴 글꼴: 나눔스퀘어라운드 · NAVER · SIL OFL 1.1",Location=new Point(26,185),Size=new Size(338,136)});
        var close=new Button{Text="닫기",DialogResult=DialogResult.OK,FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(255,233,179),Location=new Point(285,318),Size=new Size(78,32)};dialog.Controls.Add(close);dialog.AcceptButton=close;dialog.CancelButton=close;infoOpen=true;try{dialog.ShowDialog(this);}finally{infoOpen=false;}
    }}
    static void AddInfoLink(Form dialog,string text,string url,int top){var link=new LinkLabel{Text=text,Location=new Point(26,top),Size=new Size(338,28),LinkColor=Color.FromArgb(133,77,33),ActiveLinkColor=Color.FromArgb(190,110,44)};link.LinkClicked+=(s,e)=>{try{Process.Start(new ProcessStartInfo(url){UseShellExecute=true});}catch(Exception){MessageBox.Show(dialog,"링크를 열지 못했어요.\n"+url,"정보 보기");}};dialog.Controls.Add(link);}
}
