using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;

class PetRelease {
    public Version Version;public string Tag,Filename,DownloadUrl,ChecksumUrl,Digest;public long Size;
}
static class PetUpdates {
    internal const string Repo="JHOO2010/usagi-desktop-pet";
    internal const string LatestUrl="https://api.github.com/repos/"+Repo+"/releases/latest";
    internal static string CacheDirectory {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"UsagiPet","updates");}}
    internal static Version ParseVersion(string text){if(text==null||!Regex.IsMatch(text,@"^v?\d+\.\d+\.\d+$"))return null;Version value;return Version.TryParse(text.TrimStart('v'),out value)?value:null;}
    internal static Version CurrentVersion {get{var v=System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;return new Version(v.Major,v.Minor,Math.Max(0,v.Build));}}
    internal static PetRelease ParseRelease(string json,Version current){
        var root=new JavaScriptSerializer{MaxJsonLength=2097152}.DeserializeObject(json) as Dictionary<string,object>;if(root==null)throw new InvalidDataException("Invalid release response.");
        if(!root.ContainsKey("draft")||!root.ContainsKey("prerelease")||!(root["draft"] is bool)||!(root["prerelease"] is bool))throw new InvalidDataException("Release flags missing.");
        if((bool)root["draft"]||(bool)root["prerelease"])return null;
        string tag=root.ContainsKey("tag_name")?root["tag_name"] as string:null;var version=ParseVersion(tag);if(version==null||tag!="v"+version.ToString(3))throw new InvalidDataException("Invalid version.");if(version<=current)return null;
        string filename="UsagiPet-Setup-"+version.ToString(3)+".exe",download=null,checksum=null,digest=null;long size=0;
        object raw;var assets=root.TryGetValue("assets",out raw)?raw as object[]:null;if(assets==null)throw new InvalidDataException("Release assets missing.");
        foreach(var item in assets){var asset=item as Dictionary<string,object>;if(asset==null||!asset.ContainsKey("name")||!asset.ContainsKey("browser_download_url")||!asset.ContainsKey("state")||!Equals(asset["state"],"uploaded"))continue;
            string name=asset["name"] as string,url=asset["browser_download_url"] as string;if(name!=filename&&name!=filename+".sha256")continue;
            string expected="https://github.com/"+Repo+"/releases/download/"+tag+"/"+name;if(url!=expected)throw new InvalidDataException("Unexpected asset source.");
            if(name==filename){download=url;size=Convert.ToInt64(asset["size"],CultureInfo.InvariantCulture);if(asset.ContainsKey("digest"))digest=asset["digest"] as string;}else checksum=url;
        }
        if(download==null||checksum==null||size<1024||size>104857600)throw new InvalidDataException("Installer or checksum missing.");
        return new PetRelease{Version=version,Tag=tag,Filename=filename,DownloadUrl=download,ChecksumUrl=checksum,Size=size,Digest=digest};
    }
    static bool TrustedHost(Uri uri){return uri.Scheme=="https"&&(uri.Host=="api.github.com"||uri.Host=="github.com"||uri.Host=="release-assets.githubusercontent.com"||uri.Host=="objects.githubusercontent.com");}
    static void ReadUrl(string url,Stream target,long limit,CancellationToken token,Action<int> progress){
        var uri=new Uri(url);if(!TrustedHost(uri))throw new InvalidDataException("Unexpected update host.");ServicePointManager.SecurityProtocol|=(SecurityProtocolType)3072;
        var request=(HttpWebRequest)WebRequest.Create(uri);request.UserAgent="UsagiDesktopPet/"+CurrentVersion.ToString(3);request.Accept="application/vnd.github+json";request.Timeout=20000;request.ReadWriteTimeout=15000;
        using(token.Register(()=>request.Abort()))using(var response=(HttpWebResponse)request.GetResponse()){
            if(!TrustedHost(response.ResponseUri)||response.ContentLength>limit)throw new InvalidDataException("Unexpected download response.");
            using(var input=response.GetResponseStream()){byte[] buffer=new byte[65536];long total=0;int count,last=-1;while((count=input.Read(buffer,0,buffer.Length))>0){token.ThrowIfCancellationRequested();total+=count;if(total>limit)throw new InvalidDataException("Download too large.");target.Write(buffer,0,count);if(progress!=null&&response.ContentLength>0){int value=(int)Math.Min(99,total*100/response.ContentLength);if(value!=last){last=value;progress(value);}}}}
        }
    }
    static string ReadText(string url,int limit,CancellationToken token){using(var stream=new MemoryStream()){ReadUrl(url,stream,limit,token,null);return Encoding.UTF8.GetString(stream.ToArray());}}
    internal static PetRelease FetchLatest(Version current,CancellationToken token){return ParseRelease(ReadText(LatestUrl,2097152,token),current);}
    internal static string ParseChecksum(string text,string filename){var pieces=text.Trim().Split((char[])null,StringSplitOptions.RemoveEmptyEntries);if(pieces.Length!=2||!Regex.IsMatch(pieces[0],"^[0-9a-fA-F]{64}$")||pieces[1].TrimStart('*')!=filename)throw new InvalidDataException("Invalid installer checksum.");return pieces[0].ToLowerInvariant();}
    internal static string HashFile(string path){using(var sha=SHA256.Create())using(var file=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(file)).Replace("-","").ToLowerInvariant();}
    internal static void VerifyInstaller(string path,string expected){if(!Regex.IsMatch(expected??"","^[0-9a-fA-F]{64}$")||!String.Equals(HashFile(path),expected,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Installer checksum mismatch.");using(var file=File.OpenRead(path))if(file.ReadByte()!=77||file.ReadByte()!=90)throw new InvalidDataException("Installer format mismatch.");}
    internal static string Download(PetRelease release,string directory,CancellationToken token,Action<int> progress,out string hash){
        hash=ParseChecksum(ReadText(release.ChecksumUrl,4096,token),release.Filename);
        if(!String.IsNullOrEmpty(release.Digest)&&!String.Equals(release.Digest,"sha256:"+hash,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Release checksum mismatch.");
        Directory.CreateDirectory(directory);string final=Path.Combine(directory,release.Filename),partial=final+".partial";
        if(File.Exists(final)){try{VerifyInstaller(final,hash);if(new FileInfo(final).Length==release.Size){if(progress!=null)progress(100);return final;}}catch(InvalidDataException){}}
        try{using(var file=new FileStream(partial,FileMode.Create,FileAccess.Write,FileShare.None))ReadUrl(release.DownloadUrl,file,release.Size,token,progress);token.ThrowIfCancellationRequested();if(new FileInfo(partial).Length!=release.Size)throw new InvalidDataException("Incomplete installer.");VerifyInstaller(partial,hash);if(File.Exists(final))File.Replace(partial,final,null);else File.Move(partial,final);if(progress!=null)progress(100);return final;}
        finally{if(File.Exists(partial))File.Delete(partial);}
    }
    internal static string Quote(string value){if(value==null||value.IndexOf('"')>=0||value.IndexOf('\r')>=0||value.IndexOf('\n')>=0)throw new ArgumentException("Invalid argument.");return "\""+value.TrimEnd('\\')+"\"";}
}
