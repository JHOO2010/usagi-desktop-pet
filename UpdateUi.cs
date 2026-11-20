using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer=System.Windows.Forms.Timer;
partial class Usagi {
    ToolStripItem updateMenu;Timer updatePoll;CancellationTokenSource updateCancel;PetRelease availableUpdate;string downloadedUpdate,downloadedHash,notifiedUpdate;bool updateBusy;
    void AddUpdateMenu(){if(isClone)return;updateMenu=new BubbleMenuItem("업데이트 확인"){Size=new System.Drawing.Size(220,33)};updateMenu.Click+=(s,e)=>UpdateAction();menu.Items.Add(updateMenu);}
    void StartUpdates(){if(isClone||!SaveEnabled)return;updateCancel=new CancellationTokenSource();
        string result=Path.Combine(PetUpdates.CacheDirectory,"update-result.txt");try{if(File.Exists(result)){if(File.ReadAllText(result).StartsWith("ERROR")){updateMenu.Text="업데이트 다시 확인";updateMenu.ToolTipText="설치를 완료하지 못했어요. 다시 확인해 주세요.";}File.Delete(result);}}catch(IOException){}catch(UnauthorizedAccessException){}
        updatePoll=new Timer{Interval=12000};updatePoll.Tick+=(s,e)=>{updatePoll.Interval=14400000;CheckUpdates();};updatePoll.Start();
    }
    void StopUpdates(){if(updatePoll!=null)updatePoll.Dispose();if(updateCancel!=null){updateCancel.Cancel();updateCancel.Dispose();}}
    void UpdatePost(Action action){if(IsDisposed||Disposing||!IsHandleCreated)return;try{BeginInvoke(new Action(()=>{if(!IsDisposed&&!Disposing)action();}));}catch(InvalidOperationException){}}
    void UpdateAction(){if(updateBusy)return;if(downloadedUpdate!=null){InstallUpdate();return;}if(availableUpdate!=null){DownloadUpdate();return;}CheckUpdates();}
    void CheckUpdates(){if(updateBusy||downloadedUpdate!=null)return;if(updateCancel==null)updateCancel=new CancellationTokenSource();var token=updateCancel.Token;updateBusy=true;updateMenu.Enabled=false;updateMenu.Text="업데이트 확인 중…";
        Task.Factory.StartNew(()=>{try{var release=PetUpdates.FetchLatest(PetUpdates.CurrentVersion,token);UpdatePost(()=>{updateBusy=false;updateMenu.Enabled=true;availableUpdate=release;if(release==null){updateMenu.Text="최신 버전 · v"+PetUpdates.CurrentVersion.ToString(3);updateMenu.ToolTipText="눌러서 다시 확인";}else{updateMenu.Text=release.Tag+" 업데이트 받기";updateMenu.ToolTipText="GitHub에서 설치파일 내려받기";if(notifiedUpdate!=release.Tag){notifiedUpdate=release.Tag;tray.ShowBalloonTip(5000,"우사기 업데이트",release.Tag+" 버전이 나왔어요. 우클릭 메뉴에서 받을 수 있어요.",ToolTipIcon.Info);}}});}
            catch(Exception error){if(!token.IsCancellationRequested)UpdatePost(()=>{updateBusy=false;updateMenu.Enabled=true;updateMenu.Text="업데이트 확인 재시도";updateMenu.ToolTipText="연결을 확인한 뒤 눌러 주세요.";Debug.WriteLine(error.Message);});}});
    }
    void DownloadUpdate(){if(updateBusy||availableUpdate==null)return;var release=availableUpdate;var token=updateCancel.Token;updateBusy=true;updateMenu.Enabled=false;updateMenu.Text="다운로드 중 0%";
        Task.Factory.StartNew(()=>{try{string hash;string file=PetUpdates.Download(release,PetUpdates.CacheDirectory,token,value=>UpdatePost(()=>{updateMenu.Text="다운로드 중 "+value+"%";}),out hash);UpdatePost(()=>{downloadedUpdate=file;downloadedHash=hash;updateBusy=false;updateMenu.Enabled=true;updateMenu.Text="설치하고 다시 켜기";updateMenu.ToolTipText=release.Tag+" · 다운로드 완료";tray.ShowBalloonTip(4000,"우사기 다운로드 완료","우클릭 메뉴에서 ‘설치하고 다시 켜기’를 눌러 주세요.",ToolTipIcon.Info);});}
            catch(Exception error){if(!token.IsCancellationRequested)UpdatePost(()=>{updateBusy=false;updateMenu.Enabled=true;updateMenu.Text="업데이트 다시 받기";updateMenu.ToolTipText="다운로드를 완료하지 못했어요. 눌러서 다시 시도해 주세요.";Debug.WriteLine(error.Message);});}});
    }
    void InstallUpdate(){try{PetUpdates.VerifyInstaller(downloadedUpdate,downloadedHash);string source=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"UsagiUpdate.exe");if(!File.Exists(source))throw new FileNotFoundException("Update launcher missing.");
            string launcher=Path.Combine(PetUpdates.CacheDirectory,"launch-"+Guid.NewGuid().ToString("N")+".exe");File.Copy(source,launcher);SavePreferences();
            var process=Process.Start(new ProcessStartInfo(launcher,Process.GetCurrentProcess().Id+" "+PetUpdates.Quote(downloadedUpdate)+" "+PetUpdates.Quote(downloadedHash)+" "+PetUpdates.Quote(AppDomain.CurrentDomain.BaseDirectory)){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=PetUpdates.CacheDirectory});if(process==null)throw new InvalidOperationException("Could not launch updater.");process.Dispose();Close();
        }catch(Exception error){updateMenu.Text="업데이트 설치 재시도";updateMenu.ToolTipText="설치를 시작하지 못했어요. 다시 눌러 주세요.";Debug.WriteLine(error.Message);}}
}
