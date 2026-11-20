using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;

static class DisplayRefresh {
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]struct Mode {
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)]public string Device;
        public ushort SpecVersion,DriverVersion,Size,Extra;public uint Fields;
        public int X,Y;public uint Orientation,FixedOutput;public short Color,Duplex,YResolution,TTOption,Collate;
        [MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)]public string Form;
        public ushort LogPixels;public uint Bits,Width,Height,Flags,Frequency,ICMMethod,ICMIntent,MediaType,DitherType,Reserved1,Reserved2,PanningWidth,PanningHeight;
    }
    [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern bool EnumDisplaySettingsW(string device,int mode,ref Mode settings);
    internal static int Normalize(uint hz){return hz>=20&&hz<=1000?(int)hz:60;}
    internal static int Read(string device){var mode=new Mode{Size=(ushort)Marshal.SizeOf(typeof(Mode)),Device="",Form=""};return EnumDisplaySettingsW(device,-1,ref mode)?Normalize(mode.Frequency):60;}
}
sealed class FrameTicket : IDisposable {
    internal readonly Control Owner;internal readonly Action Draw;internal double Next;internal int Hertz;internal bool Ready;internal volatile bool Active=true;
    internal FrameTicket(Control owner,Action draw,int hz){Owner=owner;Draw=draw;Hertz=hz;Next=FramePump.Now;}
    public void Dispose(){if(!Active)return;Active=false;FramePump.Remove(this);}
    internal void SetRate(int hz){FramePump.SetRate(this,hz);}
}
static class FramePump {
    static readonly object sync=new object();static readonly List<FrameTicket> entries=new List<FrameTicket>();static readonly AutoResetEvent changed=new AutoResetEvent(false);static Thread thread;static int batchPending;
    internal static double Now {get{return Stopwatch.GetTimestamp()/(double)Stopwatch.Frequency;}}
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr CreateWaitableTimerExW(IntPtr attributes,string name,uint flags,uint access);
    [DllImport("kernel32.dll",SetLastError=true)]static extern bool SetWaitableTimer(IntPtr timer,ref long due,int period,IntPtr routine,IntPtr argument,bool resume);
    sealed class NativeWait : WaitHandle {internal NativeWait(IntPtr handle){SafeWaitHandle=new SafeWaitHandle(handle,true);}}
    internal static double Advance(double due,double now,int hz){double period=1.0/hz;return now<due?due:due+(Math.Floor((now-due)/period)+1)*period;}
    internal static FrameTicket Register(Control owner,Action draw,int hz){var ticket=new FrameTicket(owner,draw,hz);lock(sync){entries.Add(ticket);if(thread==null){thread=new Thread(Run){IsBackground=true,Name="Usagi display frame scheduler"};thread.Start();}}changed.Set();return ticket;}
    internal static void Remove(FrameTicket ticket){lock(sync)entries.Remove(ticket);changed.Set();}
    internal static void SetRate(FrameTicket ticket,int hz){lock(sync){if(ticket.Hertz==hz)return;ticket.Hertz=hz;ticket.Next=Now;}changed.Set();}
    static void Dispatch(List<FrameTicket> due){List<FrameTicket> tickets;lock(sync){foreach(var ticket in due)if(ticket.Active)ticket.Ready=true;if(Interlocked.CompareExchange(ref batchPending,1,0)!=0)return;tickets=entries.FindAll(ticket=>ticket.Active&&ticket.Ready);foreach(var ticket in tickets)ticket.Ready=false;if(tickets.Count==0){Interlocked.Exchange(ref batchPending,0);return;}}
        // Keep the entire UI batch pending until every pet has drawn. Per-pet
        // callbacks can continuously refill WinForms' marshalled callback queue.
        var owner=tickets[0].Owner;
        try{owner.BeginInvoke(new Action(()=>{try{foreach(var ticket in tickets)if(ticket.Active&&!ticket.Owner.IsDisposed&&!ticket.Owner.Disposing)ticket.Draw();}finally{Interlocked.Exchange(ref batchPending,0);}}));}catch(InvalidOperationException){Interlocked.Exchange(ref batchPending,0);}
    }
    static void Run(){IntPtr native=CreateWaitableTimerExW(IntPtr.Zero,null,2,0x100002);if(native==IntPtr.Zero)native=CreateWaitableTimerExW(IntPtr.Zero,null,0,0x100002);
        using(var waiter=native==IntPtr.Zero?null:new NativeWait(native)){
            WaitHandle[] waits=waiter==null?null:new WaitHandle[]{changed,waiter};
            for(;;){var due=new List<FrameTicket>();double next=Double.MaxValue;lock(sync){double now=Now;foreach(var ticket in entries){if(!ticket.Active)continue;if(now>=ticket.Next){ticket.Next=Advance(ticket.Next,now,ticket.Hertz);due.Add(ticket);}next=Math.Min(next,ticket.Next);}}
                Dispatch(due);
                if(next==Double.MaxValue){changed.WaitOne();continue;}double delay=Math.Max(.0001,next-Now);
                long relative=-(long)Math.Max(1,delay*10000000);if(waiter!=null&&SetWaitableTimer(native,ref relative,0,IntPtr.Zero,IntPtr.Zero,false))WaitHandle.WaitAny(waits);else changed.WaitOne(Math.Max(1,(int)Math.Ceiling(delay*1000)));
            }
        }
    }
}
partial class Usagi {
    FrameTicket frameTicket;int targetFrameRate=60;string frameDevice;double refreshRateCheck;LayeredSurface frameSurface;
    void StartAnimation(){RefreshFrameRate(true);lastTick=frameClock.Elapsed.TotalSeconds;if(frameTicket==null)frameTicket=FramePump.Register(this,TickPet,targetFrameRate);}
    void StopAnimation(){if(frameTicket!=null){frameTicket.Dispose();frameTicket=null;}}
    void RefreshFrameRate(bool force){PatrolArea();double now=frameClock.Elapsed.TotalSeconds;string device=patrolScreen.DeviceName;if(!force&&device==frameDevice&&now<refreshRateCheck)return;
        targetFrameRate=DisplayRefresh.Read(device);frameDevice=device;refreshRateCheck=now+2;if(frameTicket!=null)frameTicket.SetRate(targetFrameRate);if(tray!=null)tray.Text="우사기 · 화면 맞춤 "+targetFrameRate+"Hz · 1번 폴짝 / 2번 하트";
    }
}
