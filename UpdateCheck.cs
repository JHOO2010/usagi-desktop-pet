using System;
using System.IO;
using System.Text;
class UpdateCheck {
    static void Assert(bool value,string message){if(!value)throw new Exception(message);}
    static void Reject(Action action,string message){bool rejected=false;try{action();}catch(InvalidDataException){rejected=true;}Assert(rejected,message);}
    internal static void Run(){
        string name="UsagiPet-Setup-9.8.7.exe",url="https://github.com/"+PetUpdates.Repo+"/releases/download/v9.8.7/";
        string json="{\"tag_name\":\"v9.8.7\",\"draft\":false,\"prerelease\":false,\"assets\":[{\"name\":\""+name+"\",\"state\":\"uploaded\",\"size\":4096,\"browser_download_url\":\""+url+name+"\"},{\"name\":\""+name+".sha256\",\"state\":\"uploaded\",\"size\":100,\"browser_download_url\":\""+url+name+".sha256\"}]}";
        var release=PetUpdates.ParseRelease(json,new Version(9,8,6));Assert(release!=null&&release.Version==new Version(9,8,7),"New update not detected");
        Assert(PetUpdates.ParseRelease(json,new Version(9,8,7))==null&&PetUpdates.ParseRelease(json,new Version(10,0,0))==null,"Equal/older release offered");
        Assert(PetUpdates.ParseRelease(json.Replace("\"prerelease\":false","\"prerelease\":true"),new Version(1,0,0))==null,"Preview release offered");
        Assert(PetUpdates.ParseRelease(json.Replace("\"draft\":false","\"draft\":true"),new Version(1,0,0))==null,"Draft release offered");
        Reject(()=>PetUpdates.ParseRelease(json.Replace("https://github.com/","https://example.com/"),new Version(1,0,0)),"Untrusted download host accepted");
        Reject(()=>PetUpdates.ParseRelease(json.Replace(name+".sha256","missing.sha256"),new Version(1,0,0)),"Missing checksum accepted");
        Assert(PetUpdates.ParseVersion("v1.2.3-beta")==null&&PetUpdates.ParseVersion("../../1.2.3")==null,"Unsafe version parsed");
        string directory=Path.Combine(Path.GetTempPath(),"usagi-update-check-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);string file=Path.Combine(directory,name);
        try{byte[] bytes=new byte[4096];bytes[0]=77;bytes[1]=90;File.WriteAllBytes(file,bytes);string hash=PetUpdates.HashFile(file);Assert(PetUpdates.ParseChecksum(hash+"  "+name+"\n",name)==hash,"Valid checksum rejected");PetUpdates.VerifyInstaller(file,hash);
            Reject(()=>PetUpdates.ParseChecksum(hash+"  other.exe",name),"Wrong checksum filename accepted");
            bytes[3]=1;File.WriteAllBytes(file,bytes);Reject(()=>PetUpdates.VerifyInstaller(file,hash),"Tampered installer accepted");bytes[0]=0;File.WriteAllBytes(file,bytes);Reject(()=>PetUpdates.VerifyInstaller(file,PetUpdates.HashFile(file)),"Non-PE payload accepted");
        }finally{if(File.Exists(file))File.Delete(file);Directory.Delete(directory);}
        Console.WriteLine("PASS: update versions, draft/preview filtering, pinned asset URLs, mandatory checksum, tamper and invalid executable rejection.");
    }
}
