using System;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;

static class PetStartup {
    internal const string RunKey=@"Software\Microsoft\Windows\CurrentVersion\Run";
    internal const string ValueName="UsagiDesktopPet";
    internal static string Command(string executable){string full=Path.GetFullPath(executable);if(full.IndexOf('"')>=0||full.IndexOf('\r')>=0||full.IndexOf('\n')>=0)throw new ArgumentException("Invalid executable path.");return "\""+full+"\"";}
    internal static bool Enabled(string executable,string keyPath=RunKey){using(var key=Registry.CurrentUser.OpenSubKey(keyPath,false))return key!=null&&String.Equals(key.GetValue(ValueName) as string,Command(executable),StringComparison.OrdinalIgnoreCase);}
    internal static void SetEnabled(bool enabled,string executable,string keyPath=RunKey){string command=Command(executable);if(command.Length>260)throw new InvalidOperationException("Startup path is too long.");using(var key=Registry.CurrentUser.CreateSubKey(keyPath)){
            if(enabled)key.SetValue(ValueName,command,RegistryValueKind.String);
            else if(String.Equals(key.GetValue(ValueName) as string,command,StringComparison.OrdinalIgnoreCase))key.DeleteValue(ValueName,false);
        }}
}
partial class Usagi {
    ToolStripItem startupMenu;
    void AddStartupMenu(){if(isClone)return;startupMenu=new BubbleMenuItem("시작 시 실행: 끔");startupMenu.Click+=(s,e)=>{try{PetStartup.SetEnabled(!PetStartup.Enabled(Application.ExecutablePath),Application.ExecutablePath);RefreshStartupMenu();}catch(Exception error){startupMenu.Text="시작앱 설정 재시도";startupMenu.ToolTipText="시작앱 설정을 바꾸지 못했어요. "+error.Message;}};menu.Items.Add(startupMenu);RefreshStartupMenu();}
    void RefreshStartupMenu(){if(startupMenu==null)return;try{startupMenu.Text=PetStartup.Enabled(Application.ExecutablePath)?"시작 시 실행: 켜짐":"시작 시 실행: 끔";startupMenu.ToolTipText="눌러서 Windows 로그인 시 자동 실행을 켜거나 끄기";}catch{startupMenu.Text="시작앱 설정 재시도";}}
}
