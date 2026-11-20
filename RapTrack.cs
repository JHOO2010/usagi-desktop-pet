using System;
using System.IO;
using System.Text;
sealed class RapTrack {
    internal const int MaxSeconds=90;
    internal readonly short[] Samples;internal readonly float[] Levels;internal readonly double Bpm;
    internal double Duration {get{return Samples.Length/44100.0;}}
    internal RapTrack(short[] samples){Samples=samples;Levels=new float[(samples.Length+881)/882];float peak=.01f;for(int n=0;n<Levels.Length;n++){double energy=0;int end=Math.Min(samples.Length,(n+1)*882);for(int i=n*882;i<end;i++){double value=samples[i]/32768.0;energy+=value*value;}Levels[n]=(float)Math.Sqrt(energy/Math.Max(1,end-n*882));peak=Math.Max(peak,Levels[n]);}for(int n=0;n<Levels.Length;n++)Levels[n]/=peak;
        double best=0;int lagBest=25;if(Levels.Length>100)for(int lag=18;lag<=40;lag++){double score=0;for(int i=lag;i<Levels.Length;i++)score+=Math.Max(0,Levels[i]-.35f)*Math.Max(0,Levels[i-lag]-.35f);score/=Levels.Length-lag;if(score>best){best=score;lagBest=lag;}}Bpm=3000.0/lagBest;
    }
    internal float Level(double time){if(Levels.Length==0)return 0;double index=Math.Max(0,time*50);int a=Math.Min(Levels.Length-1,(int)index),b=Math.Min(Levels.Length-1,a+1);return Levels[a]+(Levels[b]-Levels[a])*(float)(index-Math.Floor(index));}
    internal static RapTrack Load(string path){using(var stream=File.OpenRead(path))using(var reader=new BinaryReader(stream)){
        if(stream.Length<12||stream.Length>83886080||new string(reader.ReadChars(4))!="RIFF")throw new InvalidDataException("WAV 음원을 선택해 주세요.");reader.ReadUInt32();if(new string(reader.ReadChars(4))!="WAVE")throw new InvalidDataException("WAV 음원을 선택해 주세요.");
        int tag=0,channels=0,rate=0,bits=0,block=0;byte[] data=null;
        while(stream.Position+8<=stream.Length){string chunk=new string(reader.ReadChars(4));uint length=reader.ReadUInt32();long end=stream.Position+length;if(end>stream.Length)throw new InvalidDataException("음원 파일이 손상되었어요.");
            if(chunk=="fmt "&&length>=16){tag=reader.ReadUInt16();channels=reader.ReadUInt16();rate=reader.ReadInt32();reader.ReadInt32();block=reader.ReadUInt16();bits=reader.ReadUInt16();if(tag==65534&&length>=40){reader.ReadUInt16();reader.ReadUInt16();reader.ReadUInt32();tag=reader.ReadUInt16();}}
            else if(chunk=="data")data=reader.ReadBytes((int)length);
            stream.Position=Math.Min(stream.Length,end+(length%2));
        }
        if(data==null||channels<1||channels>2||rate<8000||rate>192000||!((tag==1&&(bits==16||bits==24||bits==32))||(tag==3&&bits==32))||block!=channels*bits/8||data.Length%block!=0)throw new InvalidDataException("PCM WAV(16/24/32비트) 또는 float WAV 음원을 선택해 주세요.");
        int frames=data.Length/block;if(frames<rate/4||frames/(double)rate>MaxSeconds)throw new InvalidDataException("0.25~90초 길이의 음원을 선택해 주세요.");
        int count=(int)Math.Round(frames*44100.0/rate);var result=new short[count];for(int i=0;i<count;i++){double p=Math.Min(frames-1,i*(double)rate/44100.0);int a=(int)p,b=Math.Min(frames-1,a+1);double first=ReadSample(data,a,channels,bits,tag),second=ReadSample(data,b,channels,bits,tag);double value=first+(second-first)*(p-a);result[i]=(short)Math.Max(short.MinValue,Math.Min(short.MaxValue,Math.Round(value*32767)));}
        int fade=Math.Min(220,result.Length/4);for(int i=0;i<fade;i++){result[i]=(short)(result[i]*i/(double)fade);int j=result.Length-1-i;result[j]=(short)(result[j]*i/(double)fade);}return new RapTrack(result);
    }}
    static double ReadSample(byte[] data,int frame,int channels,int bits,int tag){double sum=0;for(int c=0;c<channels;c++){int p=frame*channels*bits/8+c*bits/8;double v;if(tag==3){v=BitConverter.ToSingle(data,p);if(Double.IsNaN(v)||Double.IsInfinity(v))v=0;}else if(bits==16)v=BitConverter.ToInt16(data,p)/32768.0;else if(bits==24){int raw=data[p]|data[p+1]<<8|data[p+2]<<16;if((raw&0x800000)!=0)raw|=unchecked((int)0xff000000);v=raw/8388608.0;}else v=BitConverter.ToInt32(data,p)/2147483648.0;sum+=Math.Max(-1,Math.Min(1,v));}return sum/channels;}
    internal void Save(string path){Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".partial";try{using(var writer=new BinaryWriter(File.Create(temp))){writer.Write(Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+Samples.Length*2);writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(44100);writer.Write(88200);writer.Write((short)2);writer.Write((short)16);writer.Write(Encoding.ASCII.GetBytes("data"));writer.Write(Samples.Length*2);foreach(short sample in Samples)writer.Write(sample);}if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);}finally{if(File.Exists(temp))File.Delete(temp);}}
}
