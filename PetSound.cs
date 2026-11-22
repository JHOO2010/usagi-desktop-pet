using System;
using System.Runtime.InteropServices;
using System.IO;

class AudioEndpoint {
    public string Id,Name;public int FormFactor;public uint WaveId;
    public bool Headphones {get{return FormFactor==3||FormFactor==5;}}
}
interface IPetAudioPosition {double? PositionSeconds {get;}}
interface IPetAudio : IDisposable {void Play(short[] samples,uint device);void Stop();bool Done {get;}}
class PetSound : IDisposable {
    readonly Func<AudioEndpoint> probe;readonly IPetAudio output;string playingDevice,playingEffect;double poll;
    public bool Enabled=true;internal string CharacterKey="usagi";
    public PetSound():this(EndpointProbe.Read,new WaveAudio()){}
    internal PetSound(Func<AudioEndpoint> source,IPetAudio backend){probe=source;output=backend;}
    public bool Play(string effect){try{return PlaySamples(effect,LoadCharacterClip(effect));}catch{return false;}}
    short[] LoadCharacterClip(string effect){if(CharacterKey=="usagi"||effect=="sleep")return LoadClip(effect);string file=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"sounds","characters",CharacterKey+"-voice.wav");string key=CharacterKey+"/"+effect;short[] samples;if(clips.TryGetValue(key,out samples))return samples;if(!File.Exists(file))return new short[0];samples=RapTrack.Load(file).Samples;if(effect.StartsWith("walk")){var quiet=new short[samples.Length];for(int i=0;i<samples.Length;i++)quiet[i]=(short)(samples[i]*.35);samples=quiet;}clips[key]=samples;return samples;}
    bool PlaySamples(string effect,short[] samples){if(!Enabled||samples==null||samples.Length==0)return false;if(playingEffect=="rap"&&playingDevice!=null&&!output.Done&&effect!="rap")return false;try{var device=probe();if(device==null||!device.Headphones||device.WaveId==uint.MaxValue)return false;var quiet=new short[samples.Length];for(int i=0;i<samples.Length;i++)quiet[i]=(short)(samples[i]*.5);output.Stop();output.Play(quiet,device.WaveId);if(output.Done){playingDevice=null;playingEffect=null;return false;}playingDevice=device.Id;playingEffect=effect;poll=0;return true;}catch{output.Stop();playingDevice=null;playingEffect=null;return false;}}
    public bool PlayRap(short[] samples){return samples!=null&&samples.Length<=44100*90&&PlaySamples("rap",samples);}
    public bool RapPlaying {get{return playingEffect=="rap"&&playingDevice!=null&&!output.Done;}}
    public double? RapPosition {get{var position=output as IPetAudioPosition;return RapPlaying&&position!=null?position.PositionSeconds:null;}}
    public void StopAll(){output.Stop();playingDevice=null;playingEffect=null;}
    public void StopRap(){if(playingEffect=="rap")StopAll();}
    public bool PlayAmbient(string effect){if(playingDevice!=null&&!output.Done)return false;return Play(effect);}
    public void StopSleep(){if(playingEffect!="sleep")return;output.Stop();playingDevice=null;playingEffect=null;}
    public void Tick(double dt){if(playingDevice==null)return;poll-=dt;if(poll>0)return;poll=.06;
        try{var device=probe();if(!Enabled||device==null||!device.Headphones||device.Id!=playingDevice||output.Done){output.Stop();playingDevice=null;}}catch{output.Stop();playingDevice=null;}
    }
    public void Dispose(){output.Dispose();}
    static readonly System.Collections.Generic.Dictionary<string,short[]> clips=new System.Collections.Generic.Dictionary<string,short[]>();
    internal static short[] LoadClip(string effect){short[] cached;if(clips.TryGetValue(effect,out cached))return cached;
        string name=effect=="hop"?"ura.wav":effect=="heart"?"purya.wav":effect=="burst"?"yaha.wav":effect=="walk_ura"?"walk-ura.wav":effect=="walk_purya"?"walk-purya.wav":effect=="sleep"?"sleep.wav":null;if(name==null)return new short[0];
        string path=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"sounds",name);if(!File.Exists(path))return new short[0];
        using(var stream=File.OpenRead(path))using(var reader=new BinaryReader(stream)){
            if(new string(reader.ReadChars(4))!="RIFF")return new short[0];reader.ReadUInt32();if(new string(reader.ReadChars(4))!="WAVE")return new short[0];bool format=false;
            while(stream.Position+8<=stream.Length){string chunk=new string(reader.ReadChars(4));uint size=reader.ReadUInt32();long next=stream.Position+size+(size%2);if(next>stream.Length)break;
                if(chunk=="fmt "&&size>=16){ushort tag=reader.ReadUInt16(),channels=reader.ReadUInt16();uint rate=reader.ReadUInt32();reader.ReadUInt32();reader.ReadUInt16();ushort bits=reader.ReadUInt16();format=tag==1&&channels==1&&rate==44100&&bits==16;}
                if(chunk=="data"&&format&&size<=44100*2*8){var bytes=reader.ReadBytes((int)size);cached=new short[bytes.Length/2];Buffer.BlockCopy(bytes,0,cached,0,cached.Length*2);clips[effect]=cached;return cached;}stream.Position=next;
            }
        }return new short[0];
    }
}

static class EndpointProbe {
    [ComImport,Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]class Enumerator {}
    [ComImport,Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]interface IEnumerator {
        void EnumAudioEndpoints(int flow,uint mask,out IntPtr devices);void GetDefaultAudioEndpoint(int flow,int role,out IDevice device);
    }
    [ComImport,Guid("D666063F-1587-4E43-81F1-B948E807363F"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]interface IDevice {
        void Activate(ref Guid iid,uint context,IntPtr parameters,out IntPtr result);void OpenPropertyStore(uint access,out IStore store);void GetId([MarshalAs(UnmanagedType.LPWStr)]out string id);void GetState(out uint state);
    }
    [StructLayout(LayoutKind.Sequential)]struct Key {public Guid id;public uint pid;public Key(string guid,uint property){id=new Guid(guid);pid=property;}}
    [StructLayout(LayoutKind.Explicit,Size=24)]struct Value {[FieldOffset(0)]public ushort vt;[FieldOffset(8)]public uint number;[FieldOffset(8)]public IntPtr pointer;}
    [ComImport,Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]interface IStore {
        void GetCount(out uint count);void GetAt(uint index,out Key key);void GetValue(ref Key key,out Value value);
    }
    [DllImport("ole32.dll")]static extern int PropVariantClear(ref Value value);
    [DllImport("winmm.dll")]static extern uint waveOutGetNumDevs();
    [DllImport("winmm.dll")]static extern uint waveOutMessage(IntPtr device,uint message,IntPtr first,IntPtr second);
    public static AudioEndpoint Read(){IEnumerator enumerator=null;IDevice device=null;IStore store=null;
        try{enumerator=(IEnumerator)new Enumerator();enumerator.GetDefaultAudioEndpoint(0,1,out device);uint state;device.GetState(out state);if(state!=1)return null;
            string id;device.GetId(out id);device.OpenPropertyStore(0,out store);
            var factor=new Key("1da5d803-d492-4edd-8c23-e0c0ffee7f0e",0);Value value;store.GetValue(ref factor,out value);int form;try{form=value.vt==19?(int)value.number:-1;}finally{PropVariantClear(ref value);}
            var friendly=new Key("a45c254e-df1c-4efd-8020-67d146a850e0",14);store.GetValue(ref friendly,out value);string name;try{name=value.vt==31?Marshal.PtrToStringUni(value.pointer):"";}finally{PropVariantClear(ref value);}
            return new AudioEndpoint{Id=id,Name=name,FormFactor=form,WaveId=form==3||form==5?FindWaveId(id):uint.MaxValue};
        }catch{return null;}finally{if(store!=null)Marshal.ReleaseComObject(store);if(device!=null)Marshal.ReleaseComObject(device);if(enumerator!=null)Marshal.ReleaseComObject(enumerator);}
    }
    static uint FindWaveId(string endpoint){IntPtr size=Marshal.AllocHGlobal(IntPtr.Size);try{for(uint i=0;i<waveOutGetNumDevs();i++){
        Marshal.WriteIntPtr(size,IntPtr.Zero);if(waveOutMessage(new IntPtr(i),0x0812,size,IntPtr.Zero)!=0)continue;long bytes=Marshal.ReadIntPtr(size).ToInt64();if(bytes<2||bytes>16384)continue;
        IntPtr text=Marshal.AllocHGlobal((int)bytes);try{if(waveOutMessage(new IntPtr(i),0x0811,text,new IntPtr(bytes))==0&&string.Equals(Marshal.PtrToStringUni(text),endpoint,StringComparison.OrdinalIgnoreCase))return i;}finally{Marshal.FreeHGlobal(text);}
    }return uint.MaxValue;}finally{Marshal.FreeHGlobal(size);}}
}

class WaveAudio : IPetAudio,IPetAudioPosition {
    [StructLayout(LayoutKind.Sequential,Pack=2)]struct Format {public ushort tag,channels;public uint rate,bytes;public ushort align,bits,extra;}
    [StructLayout(LayoutKind.Sequential)]struct Header {public IntPtr data;public uint length,recorded;public UIntPtr user;public uint flags,loops;public IntPtr next;public UIntPtr reserved;}
    [DllImport("winmm.dll")]static extern uint waveOutOpen(out IntPtr handle,uint device,ref Format format,IntPtr callback,IntPtr instance,uint flags);
    [DllImport("winmm.dll")]static extern uint waveOutPrepareHeader(IntPtr handle,IntPtr header,uint size);
    [DllImport("winmm.dll")]static extern uint waveOutWrite(IntPtr handle,IntPtr header,uint size);
    [DllImport("winmm.dll")]static extern uint waveOutReset(IntPtr handle);
    [DllImport("winmm.dll")]static extern uint waveOutUnprepareHeader(IntPtr handle,IntPtr header,uint size);
    [DllImport("winmm.dll")]static extern uint waveOutClose(IntPtr handle);
    [StructLayout(LayoutKind.Explicit,Size=12)]struct WaveTime {[FieldOffset(0)]public uint Type;[FieldOffset(4)]public uint Value;}
    [DllImport("winmm.dll")]static extern uint waveOutGetPosition(IntPtr handle,ref WaveTime time,uint size);
    IntPtr output,header,data;bool prepared;uint currentBytes=88200;double nextPositionQuery,cachedPosition;readonly System.Diagnostics.Stopwatch positionClock=new System.Diagnostics.Stopwatch();
    public double? PositionSeconds {get{if(output==IntPtr.Zero)return null;double now=positionClock.Elapsed.TotalSeconds;if(now<nextPositionQuery)return cachedPosition;nextPositionQuery=now+.008;var time=new WaveTime{Type=2};if(waveOutGetPosition(output,ref time,12)!=0)return null;cachedPosition=time.Type==2?time.Value/44100.0:time.Type==1?time.Value/1000.0:time.Type==4?time.Value/(double)currentBytes:now;return cachedPosition;}}
    internal static bool SupportsFormat(uint device){var format=new Format{tag=1,channels=1,rate=44100,bytes=88200,align=2,bits=16};IntPtr handle;uint result=waveOutOpen(out handle,device,ref format,IntPtr.Zero,IntPtr.Zero,1);if(handle!=IntPtr.Zero)waveOutClose(handle);return result==0;}

    public bool Done {get{return header==IntPtr.Zero||(((Header)Marshal.PtrToStructure(header,typeof(Header))).flags&1)!=0;}}
    public void Play(short[] samples,uint device){Stop();if(device==uint.MaxValue)return;var format=new Format{tag=1,channels=1,rate=44100,bytes=88200,align=2,bits=16};
        if(waveOutOpen(out output,device,ref format,IntPtr.Zero,IntPtr.Zero,0)!=0){format.channels=2;format.bytes=176400;format.align=4;if(waveOutOpen(out output,device,ref format,IntPtr.Zero,IntPtr.Zero,0)!=0){output=IntPtr.Zero;return;}var stereo=new short[samples.Length*2];for(int i=0;i<samples.Length;i++)stereo[i*2]=stereo[i*2+1]=samples[i];samples=stereo;}currentBytes=format.bytes;cachedPosition=nextPositionQuery=0;positionClock.Restart();
        try{data=Marshal.AllocHGlobal(samples.Length*2);Marshal.Copy(samples,0,data,samples.Length);header=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Header)));Marshal.StructureToPtr(new Header{data=data,length=(uint)(samples.Length*2)},header,false);
            uint size=(uint)Marshal.SizeOf(typeof(Header));if(waveOutPrepareHeader(output,header,size)!=0)throw new InvalidOperationException();prepared=true;if(waveOutWrite(output,header,size)!=0)throw new InvalidOperationException();
        }catch{Stop();}
    }
    public void Stop(){if(output!=IntPtr.Zero){waveOutReset(output);if(prepared&&header!=IntPtr.Zero)waveOutUnprepareHeader(output,header,(uint)Marshal.SizeOf(typeof(Header)));waveOutClose(output);output=IntPtr.Zero;}prepared=false;if(header!=IntPtr.Zero){Marshal.FreeHGlobal(header);header=IntPtr.Zero;}if(data!=IntPtr.Zero){Marshal.FreeHGlobal(data);data=IntPtr.Zero;}}
    public void Dispose(){Stop();}
}
