using System;
using System.Diagnostics;
using System.IO;
class UpdateLauncher {
    [STAThread]static void Main(string[] args){string app=null;bool restart=false;string result=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"update-result.txt");
        try{if(args.Length!=4)throw new ArgumentException("Invalid updater arguments.");int pid;if(!Int32.TryParse(args[0],out pid)||pid<=0)throw new ArgumentException("Invalid process.");
            string installer=Path.GetFullPath(args[1]),directory=Path.GetFullPath(args[3]).TrimEnd('\\');app=Path.Combine(directory,"우사기.exe");
            if(!String.Equals(Path.GetDirectoryName(installer),AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),StringComparison.OrdinalIgnoreCase)||!System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(installer),@"^UsagiPet-Setup-\d+\.\d+\.\d+\.exe$")||!File.Exists(app))throw new InvalidDataException("Invalid update paths.");
            restart=true;PetUpdates.VerifyInstaller(installer,args[2]);
            try{using(var parent=Process.GetProcessById(pid))if(!parent.WaitForExit(20000))throw new TimeoutException("Application is still running.");}catch(ArgumentException){}
            restart=true;using(var install=Process.Start(new ProcessStartInfo(installer,"/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /DIR="+PetUpdates.Quote(directory)){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=Path.GetDirectoryName(installer)})){
                install.WaitForExit();if(install.ExitCode!=0&&install.ExitCode!=3010)throw new InvalidOperationException("Installer exit code "+install.ExitCode);}
            File.WriteAllText(result,"OK");
        }catch(Exception error){try{File.WriteAllText(result,"ERROR: "+error.Message);}catch{}}
        finally{if(restart&&app!=null&&File.Exists(app))try{Process.Start(new ProcessStartInfo(app){UseShellExecute=false,WorkingDirectory=Path.GetDirectoryName(app)});}catch{}}
    }
}
