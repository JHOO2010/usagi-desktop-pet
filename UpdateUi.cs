using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer=System.Windows.Forms.Timer;
partial class Usagi {
    UpdateStatusItem updateMenu;Timer updatePoll;CancellationTokenSource updateCancel;PetRelease availableUpdate;string downloadedUpdate,downloadedHash,notifiedUpdate;bool updateBusy;string updateError="";
    void AddUpdateMenu(){if(isClone)return;updateMenu=new UpdateStatusItem(UpdateAction,ShowPetInfo);menu.Items.Add(updateMenu);}
    DateTime lastUpdateCheck=DateTime.MinValue;string updateCheckStatus="아직 확인하지 않았어요";
    void CheckUpdatesFromMenu(){if(isClone||!SaveEnabled)return;if(!updateBusy&&downloadedUpdate==null&&(DateTime.UtcNow-lastUpdateCheck).TotalSeconds>=60)CheckUpdates();RefreshSettingsBadge();}
    void RefreshSettingsBadge(){if(settingsMenu==null)return;settingsMenu.Text=downloadedUpdate!=null?"설정 · 업데이트 설치":availableUpdate!=null?"설정 · 새 업데이트":updateBusy?"설정 · 업데이트 확인 중":"설정";}
    void StartUpdates(){if(isClone||!SaveEnabled)return;ScheduleReleaseNotes();updateCancel=new CancellationTokenSource();
        string result=Path.Combine(PetUpdates.CacheDirectory,"update-result.txt");try{if(File.Exists(result)){if(File.ReadAllText(result).StartsWith("ERROR")){updateMenu.Text="업데이트 다시 확인";updateMenu.ToolTipText="설치를 완료하지 못했어요. 다시 확인해 주세요.";}File.Delete(result);}}catch(IOException){}catch(UnauthorizedAccessException){}
        updatePoll=new Timer{Interval=12000};updatePoll.Tick+=(s,e)=>{updatePoll.Interval=14400000;CheckUpdates();};updatePoll.Start();
    }
    void StopUpdates(){if(updatePoll!=null)updatePoll.Dispose();if(updateCancel!=null){updateCancel.Cancel();updateCancel.Dispose();}}
    void UpdatePost(Action action){if(IsDisposed||Disposing||!IsHandleCreated)return;try{BeginInvoke(new Action(()=>{if(!IsDisposed&&!Disposing){action();RefreshSettingsBadge();}}));}catch(InvalidOperationException){}}
    void UpdateAction(){if(updateBusy)return;if(downloadedUpdate!=null){InstallUpdate();return;}if(availableUpdate!=null){DownloadUpdate();return;}CheckUpdates();}
    void CheckUpdates(){if(updateBusy||downloadedUpdate!=null)return;if(updateCancel==null)updateCancel=new CancellationTokenSource();var token=updateCancel.Token;lastUpdateCheck=DateTime.UtcNow;updateError="";updateBusy=true;updateMenu.UpdateEnabled=false;updateMenu.Text="업데이트 확인 중…";updateCheckStatus="최신 정보를 확인하고 있어요";
        Task.Factory.StartNew(()=>{try{var release=PetUpdates.FetchLatest(PetUpdates.CurrentVersion,token);UpdatePost(()=>{updateBusy=false;updateMenu.UpdateEnabled=true;availableUpdate=release;updateCheckStatus=DateTime.Now.ToString("HH:mm")+" 확인";if(release==null){updateMenu.Text="최신 버전 · v"+PetUpdates.CurrentVersion.ToString(3);updateMenu.ToolTipText="눌러서 다시 확인";}else{updateMenu.Text=release.Tag+" 업데이트 받기";updateMenu.ToolTipText="GitHub에서 설치파일 내려받기";if(notifiedUpdate!=release.Tag){notifiedUpdate=release.Tag;tray.ShowBalloonTip(5000,"먼작귀 펫 업데이트",release.Tag+" 버전이 나왔어요. 설정 창에서 받을 수 있어요.",ToolTipIcon.Info);}}});}
            catch(Exception error){if(!token.IsCancellationRequested)UpdatePost(()=>{updateError="연결을 확인하고 다시 시도해 주세요.";updateBusy=false;updateMenu.UpdateEnabled=true;updateMenu.Text="업데이트 확인 재시도";updateCheckStatus="확인 실패 · 다시 시도해 주세요";updateMenu.ToolTipText="연결을 확인한 뒤 눌러 주세요.";PetUpdates.LogFailure("update",error);Debug.WriteLine(error.Message);});}});
    }
    void DownloadUpdate(){if(updateBusy||availableUpdate==null)return;var release=availableUpdate;var token=updateCancel.Token;updateError="";updateBusy=true;updateMenu.UpdateEnabled=false;updateMenu.Text="다운로드 중 0%";
        Task.Factory.StartNew(()=>{try{string hash;string file=PetUpdates.Download(release,PetUpdates.CacheDirectory,token,value=>UpdatePost(()=>{updateMenu.Text="다운로드 중 "+value+"%";}),out hash);UpdatePost(()=>{downloadedUpdate=file;downloadedHash=hash;updateBusy=false;updateMenu.UpdateEnabled=true;updateMenu.Text="설치하고 다시 켜기";updateMenu.ToolTipText=release.Tag+" · 다운로드 완료";tray.ShowBalloonTip(4000,"먼작귀 펫 다운로드 완료","설정 창에서 ‘설치하고 다시 켜기’를 눌러 주세요.",ToolTipIcon.Info);});}
            catch(Exception error){if(!token.IsCancellationRequested)UpdatePost(()=>{updateError="다운로드 실패 · 다시 눌러 주세요.";updateBusy=false;updateMenu.UpdateEnabled=true;updateMenu.Text="업데이트 다시 받기";updateMenu.ToolTipText="다운로드를 완료하지 못했어요. 눌러서 다시 시도해 주세요.";PetUpdates.LogFailure("update",error);Debug.WriteLine(error.Message);});}});
    }
    void InstallUpdate(){try{PetUpdates.VerifyInstaller(downloadedUpdate,downloadedHash);string source=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"UsagiUpdate.exe");if(!File.Exists(source))throw new FileNotFoundException("Update launcher missing.");
            string launcher=Path.Combine(PetUpdates.CacheDirectory,"launch-"+Guid.NewGuid().ToString("N")+".exe");File.Copy(source,launcher);SavePreferences();
            var process=Process.Start(new ProcessStartInfo(launcher,Process.GetCurrentProcess().Id+" "+PetUpdates.Quote(downloadedUpdate)+" "+PetUpdates.Quote(downloadedHash)+" "+PetUpdates.Quote(AppDomain.CurrentDomain.BaseDirectory)){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=PetUpdates.CacheDirectory});if(process==null)throw new InvalidOperationException("Could not launch updater.");process.Dispose();Close();
        }catch(Exception error){updateMenu.Text="업데이트 설치 재시도";updateMenu.ToolTipText="설치를 시작하지 못했어요. 다시 눌러 주세요.";PetUpdates.LogFailure("update",error);Debug.WriteLine(error.Message);}}
}
