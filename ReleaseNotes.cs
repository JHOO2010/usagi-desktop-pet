using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
partial class Usagi {
    string CurrentReleaseNotes(){try{return File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"release-notes.md"));}catch(IOException){return "변경 내용은 GitHub 릴리스에서 확인할 수 있어요.";}catch(UnauthorizedAccessException){return "변경 내용을 읽지 못했어요.";}}
    internal static string ReleaseNotesText(string markdown){string value=markdown??"";if(value.Length>16000)value=value.Substring(0,16000);value=Regex.Replace(value,@"^\s*#{1,6}\s*","",RegexOptions.Multiline).Replace("**","").Replace("__","");value=Regex.Replace(value,@"\[([^\]]+)\]\((https://[^)]+)\)","$1 ($2)");return Regex.Replace(value,@"^\s*-\s+","• ",RegexOptions.Multiline).Trim();}
    void ShowReleaseNotesPage(Form form,string version,string notes){if(form.Controls.ContainsKey("ReleasePage"))return;var hidden=new System.Collections.Generic.List<Control>();foreach(Control control in form.Controls)if(control.Top>=48){hidden.Add(control);control.Visible=false;}
        var page=new Panel{Name="ReleasePage",Location=new Point(26,56),Size=new Size(368,form.Height-88),BackColor=SettingArt.Cream};form.Controls.Add(page);page.BringToFront();
        var back=new SettingButton{Text="뒤로가기",AccessibleName="뒤로가기",IconName="back",IconOnly=true,Location=new Point(0,0),Size=new Size(32,32)};page.Controls.Add(back);back.Click+=(s,e)=>{form.Controls.Remove(page);page.Dispose();foreach(var control in hidden)control.Visible=true;};
        page.Controls.Add(new Label{Text=version+" 변경 내용",Location=new Point(45,5),Size=new Size(310,28),UseCompatibleTextRendering=true});
        var body=new RichTextBox{Text=ReleaseNotesText(notes),ReadOnly=true,BorderStyle=BorderStyle.None,BackColor=SettingArt.Cream,ForeColor=SettingArt.Ink,Font=menuFont,Location=new Point(0,48),Size=new Size(368,page.Height-48),ScrollBars=RichTextBoxScrollBars.Vertical,DetectUrls=true,TabStop=true};page.Controls.Add(body);body.LinkClicked+=(s,e)=>{Uri link;if(Uri.TryCreate(e.LinkText,UriKind.Absolute,out link)&&link.Scheme=="https")System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(link.AbsoluteUri){UseShellExecute=true});};back.Focus();
    }
    static bool ShouldShowReleaseNotes(string seen,string current){return !String.Equals((seen??"").Trim(),current,StringComparison.Ordinal);}
    void ScheduleReleaseNotes(){string seenPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"UsagiPet","last-seen-version.txt"),current=PetUpdates.CurrentVersion.ToString(3),seen="";try{if(File.Exists(seenPath))seen=File.ReadAllText(seenPath);}catch(IOException){}catch(UnauthorizedAccessException){}if(!ShouldShowReleaseNotes(seen,current))return;
        var timer=new Timer{Interval=1500};FormClosed+=(s,e)=>timer.Dispose();timer.Tick+=(s,e)=>{if(infoOpen||menu.Visible||pointerDown||dragging)return;timer.Stop();timer.Dispose();if(IsDisposed)return;using(var form=CreateSettingsForm()){ShowReleaseNotesPage(form,"v"+current,CurrentReleaseNotes());form.Shown+=(a,b)=>{try{Directory.CreateDirectory(Path.GetDirectoryName(seenPath));File.WriteAllText(seenPath,current);}catch(IOException){}catch(UnauthorizedAccessException){}};bool previous=infoOpen;infoOpen=true;try{form.ShowDialog(this);}finally{infoOpen=previous;}}};timer.Start();
    }
}
