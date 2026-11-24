using System;using System.Drawing;using System.Reflection;using System.Windows.Forms;
static class ThrowCheck {
    static readonly BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static object Get(Usagi pet,string key){return typeof(Usagi).GetField(key,F).GetValue(pet);}
    static void Set(Usagi pet,string key,object value){typeof(Usagi).GetField(key,F).SetValue(pet,value);}
    static object Call(Usagi pet,string key,params object[] args){return typeof(Usagi).GetMethod(key,F).Invoke(pet,args);}
    static void Assert(bool value,string message){if(!value)throw new Exception(message);}
    static void Launch(Usagi pet,double vx,double vy){var area=Screen.PrimaryScreen.WorkingArea;Set(pet,"patrolScreen",Screen.PrimaryScreen);Set(pet,"x",(double)area.Left+area.Width/2-pet.Width/2);Set(pet,"y",(double)area.Bottom-pet.Height+8-160);Call(pet,"Change",Enum.Parse(typeof(Usagi).GetNestedType("Mood",BindingFlags.NonPublic),"Fall"));Set(pet,"throwActive",true);Set(pet,"throwVX",vx);Set(pet,"throwVY",vy);Set(pet,"throwAge",0.0);Set(pet,"throwBounces",0);}
    internal static void Run(){using(var pet=new Usagi()){
        Call(pet,"BeginPointer",new Point(100,100));Call(pet,"MovePointer",new Point(190,80));double now=((System.Diagnostics.Stopwatch)Get(pet,"frameClock")).Elapsed.TotalSeconds;Call(pet,"ResetThrowGesture",new Point(100,100),now-.09);Call(pet,"RecordThrowSample",new Point(140,90),now-.05);Call(pet,"RecordThrowSample",new Point(190,80),now-.001);Call(pet,"EndPointer",now,new Point(190,80));Assert((bool)Get(pet,"throwActive")&&!(bool)Get(pet,"dragging")&&!(bool)Get(pet,"pointerDown"),"Mouse release did not transfer drag into inertia");
        Call(pet,"ResetThrowGesture",new Point(0,0),10.0);Call(pet,"RecordThrowSample",new Point(40,-15),10.04);Call(pet,"RecordThrowSample",new Point(90,-40),10.09);
        Assert((bool)Call(pet,"StartThrowFromGesture",10.09),"Fast drag did not launch");Assert((double)Get(pet,"throwVX")>700&&(double)Get(pet,"throwVY")<0,"Release direction or speed lost");
        Call(pet,"ResetThrowGesture",new Point(0,0),11.0);Assert(!(bool)Get(pet,"throwActive"),"Catching did not cancel inertia");Call(pet,"RecordThrowSample",new Point(5,1),11.1);Assert(!(bool)Call(pet,"StartThrowFromGesture",11.1),"Slow drop became a throw");
        Call(pet,"ResetThrowGesture",new Point(0,0),12.0);Call(pet,"RecordThrowSample",new Point(80,0),12.05);Assert(!(bool)Call(pet,"StartThrowFromGesture",12.3),"Paused hand retained stale fling speed");
        Launch(pet,800,500);var area=Screen.PrimaryScreen.WorkingArea;double floor=area.Bottom-pet.Height+8;Set(pet,"y",floor-1);Call(pet,"UpdateThrow",.04);Assert((double)Get(pet,"throwVY")<0&&(int)Get(pet,"throwBounces")==1,"Floor impact did not give one small bounce");
        for(int i=0;i<144*6&&(bool)Get(pet,"throwActive");i++)Call(pet,"UpdateThrow",1.0/144);Assert(!(bool)Get(pet,"throwActive")&&Math.Abs((double)Get(pet,"y")-floor)<.01,"Thrown pet failed to settle on the taskbar floor");
        Launch(pet,900,0);double right=area.Right-pet.Width/2-64;Set(pet,"x",right-1);Call(pet,"UpdateThrow",.04);Assert((double)Get(pet,"throwVX")<0&&(double)Get(pet,"x")<=right,"Throw escaped the monitor boundary");
        Launch(pet,0,-1200);Set(pet,"y",(double)area.Top-(float)Get(pet,"petTop")+9);Call(pet,"UpdateThrow",.05);Assert((double)Get(pet,"throwVY")>0,"Upward throw escaped through the screen ceiling");
        using(var frame=(Bitmap)Call(pet,"DrawFrame"))Assert(frame.Width==pet.Width,"Throw pose failed to render");
        foreach(var key in new[]{"chiikawa","hachiware","momonga","kurimanju","rakko","shisa"}){Call(pet,"ApplyCharacter",key,false);Launch(pet,500,-220);Call(pet,"UpdateThrow",.03);using(var frame=(Bitmap)Call(pet,"DrawFrame"))Assert(frame.Width==pet.Width,"Character throw pose failed: "+key);}
    }
    using(var sixty=new Usagi())using(var high=new Usagi()){Launch(sixty,320,-200);Launch(high,320,-200);for(int i=0;i<30;i++)Call(sixty,"UpdateThrow",1.0/60);for(int i=0;i<72;i++)Call(high,"UpdateThrow",1.0/144);Assert(Math.Abs((double)Get(sixty,"x")-(double)Get(high,"x"))<.1&&Math.Abs((double)Get(sixty,"y")-(double)Get(high,"y"))<.1,"Throw motion changed with refresh rate");}
    Console.WriteLine("PASS: drag velocity, gentle release, stale sample rejection, midair catch, bounce, friction, screen bounds and 60/144Hz throw consistency.");}
}
