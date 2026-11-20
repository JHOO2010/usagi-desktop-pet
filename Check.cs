using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
class Check {
 class FakeAudio : IPetAudio {public int Plays,Stops;public uint Device;public bool Finished;public bool Done {get{return Finished;}}public void Play(short[] samples,uint device){Finished=false;Plays++;Device=device;if(samples.Length==0)throw new Exception("Empty sound");}public void Stop(){Stops++;}public void Dispose(){Stop();}}
 static BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Instance;
 static void Set(object o,string n,object v){o.GetType().GetField(n,flags).SetValue(o,v);}
 static object Get(object o,string n){return o.GetType().GetField(n,flags).GetValue(o);}
 static void Call(object o,string n){o.GetType().GetMethod(n,flags).Invoke(o,null);}
 static void Mood(object o,string value){var field=o.GetType().GetField("mood",flags);field.SetValue(o,Enum.Parse(field.FieldType,value));Set(o,"stateTime",0.0);}
 static string State(object o){return Get(o,"mood").ToString();}
 static void Step(object o,double dt,Point cursor){o.GetType().GetMethod("UpdatePet",flags).Invoke(o,new object[]{dt,cursor});}
 static Bitmap Frame(object o){return (Bitmap)o.GetType().GetMethod("DrawFrame",flags).Invoke(o,null);}
 static void Assert(bool value,string message){if(!value)throw new Exception(message);}
 [STAThread] static void Main(){try{Run();Features();}catch(Exception error){Console.WriteLine("FAIL: "+error.Message);Environment.Exit(1);}}
 static void Run(){using(var pet=new Usagi()){
   ((PetSound)Get(pet,"sounds")).Enabled=false;
   var area=Screen.PrimaryScreen.WorkingArea;var cursor=new Point(area.Left+10,area.Top+10);Set(pet,"lastCursor",cursor);Set(pet,"x",(double)area.Left+area.Width/2-110);Set(pet,"y",(double)area.Bottom-pet.Height+8);Set(pet,"direction",1);Set(pet,"decisionTime",100.0);
   double start=(double)Get(pet,"x");Step(pet,.033,cursor);Assert((double)Get(pet,"x")>start,"Walk failed");Assert((double)Get(pet,"speed")<64,"Acceleration missing");Assert(Math.Abs((double)Get(pet,"y")-(area.Bottom-pet.Height+8))<.1,"Taskbar floor wrong");
   double moved=(double)Get(pet,"x")-start;Assert(Math.Abs((double)Get(pet,"walkDistance")-moved)<.001,"Stride disconnected from distance");
   Mood(pet,"Turn");Set(pet,"wantedDirection",-1);for(int i=0;i<12;i++)Step(pet,.033,cursor);Assert((int)Get(pet,"direction")==-1,"Turn did not complete");
   Set(pet,"idleTime",9.8);Step(pet,.033,cursor);Assert(State(pet)!="Sleep"&&State(pet)!="Drowsy","Slept too soon");Set(pet,"idleTime",10.01);Step(pet,.033,cursor);Assert(State(pet)=="Drowsy","Drowsy transition missing");Step(pet,.7,cursor);Assert(State(pet)=="Sleep","Sleep missing");Assert(!(bool)Get(pet,"moving"),"Sleep moved feet");Step(pet,.033,new Point(cursor.X+15,cursor.Y));Assert(State(pet)=="Stretch","Wake should stretch");
   Set(pet,"lastCursor",cursor);Set(pet,"idleTime",0.0);Call(pet,"StartHop");double apex=0;for(int i=0;i<35;i++){Step(pet,.033,cursor);apex=Math.Max(apex,(double)Get(pet,"hopHeight"));}Assert(apex>25,"Hop too low");Assert((double)Get(pet,"hopHeight")==0,"Hop did not land");
   Mood(pet,"Fall");Set(pet,"y",(double)area.Bottom-pet.Height-100);Set(pet,"fallVelocity",0.0);for(int i=0;i<30;i++)Step(pet,.033,cursor);Assert(Math.Abs((double)Get(pet,"y")-(area.Bottom-pet.Height+8))<.1,"Drop landing failed");
   Mood(pet,"Sit");Set(pet,"speechTime",0.0);Set(pet,"heartTime",0.0);Set(pet,"landing",0.0);Set(pet,"moving",false);
   using(var first=Frame(pet)){Set(pet,"t",19.0);using(var second=Frame(pet))for(int j=0;j<first.Height;j++)for(int i=0;i<first.Width;i++)Assert(first.GetPixel(i,j)==second.GetPixel(i,j),"Resting legs changed");}
   string previous=(string)Get(pet,"words");for(int i=0;i<80;i++){Call(pet,"Speak");string current=(string)Get(pet,"words");Assert(current!=previous,"Repeated line");previous=current;}
   Set(pet,"paused",true);start=(double)Get(pet,"x");Step(pet,.1,cursor);Assert((double)Get(pet,"x")==start,"Pause moved position");Set(pet,"paused",false);
   var menu=(ContextMenuStrip)Get(pet,"menu");Assert(menu is BubbleMenu,"Menu style missing");menu.Show(new Point(area.Right-300,area.Bottom-400));menu.Items[0].Select();for(int wait=0;wait<20;wait++){Application.DoEvents();System.Threading.Thread.Sleep(10);}foreach(ToolStripItem item in menu.Items)Assert(item.Bounds.Left==menu.Width-item.Bounds.Right,"Menu horizontal padding is uneven: "+item.Bounds+" menu="+menu.Size);using(var shot=new Bitmap(menu.Width,menu.Height))using(var graphics=Graphics.FromImage(shot)){graphics.CopyFromScreen(menu.Location,Point.Empty,shot.Size);shot.Save("menu-preview.png");}
   var menuAnchor=((BubbleMenu)menu).PetAnchor;Assert(menu.Bottom<=menuAnchor.Value.Y,"Menu covers pet");foreach(ToolStripItem item in menu.Items){var parent=item as ToolStripMenuItem;Assert(parent==null||!parent.HasDropDownItems,"Nested popup remains");}
   var sizeItem=(SizeChoiceItem)menu.Items[6];typeof(SizeChoiceItem).GetMethod("OnMouseUp",flags).Invoke(sizeItem,new object[]{new MouseEventArgs(MouseButtons.Left,1,65,15,0)});Assert(Math.Abs((float)Get(pet,"scale")-.75f)<.01,"Inline size selection failed");Set(pet,"scale",1f);menu.Close();
   using(var scales=new Bitmap(pet.Width*3,pet.Height))using(var graphics=Graphics.FromImage(scales)){graphics.Clear(Color.FromArgb(38,43,51));float[] sizes={.75f,1,1.4f};for(int k=0;k<3;k++){Set(pet,"scale",sizes[k]);Mood(pet,"Walk");Set(pet,"moving",true);Set(pet,"speechTime",3.0);Set(pet,"words","푸루루루!");using(var frame=Frame(pet)){for(int i=0;i<frame.Width;i++)Assert(frame.GetPixel(i,0).A==0,"Scaled speech clipped at top");graphics.DrawImage(frame,k*pet.Width,0);}}scales.Save("scale-preview.png");}Set(pet,"scale",1f);
   using(var sheet=new Bitmap(pet.Width*4,pet.Height*2))using(var g=Graphics.FromImage(sheet)){g.Clear(Color.FromArgb(38,43,51));string[] states={"Walk","Walk","Sit","Look","Drowsy","Sleep","Stretch","Hop"};
     for(int i=0;i<8;i++){Mood(pet,states[i]);Set(pet,"moving",i<2);Set(pet,"direction",i==1?-1:1);Set(pet,"walkDistance",10.0);Set(pet,"hopHeight",i==7?30.0:0.0);Set(pet,"speechTime",0.0);using(var frame=Frame(pet)){Assert(frame.GetPixel(0,0).A==0,"Lost transparency");g.DrawImage(frame,(i%4)*pet.Width,(i/4)*pet.Height);}using(var font=new Font("Malgun Gothic",10))g.DrawString(states[i],font,Brushes.White,(i%4)*pet.Width+70,(i/4)*pet.Height+20);}sheet.Save("animation-preview.png");}
   var particles=(System.Collections.IList)Get(pet,"hearts");particles.Clear();
   var clickMethod=pet.GetType().GetMethod("RegisterClick",flags);double interval=SystemInformation.DoubleClickTime/1000.0;
   Mood(pet,"Look");clickMethod.Invoke(pet,new object[]{10.0,cursor});Assert(State(pet)=="Look","Click must wait for multi-click classification");Call(pet,"ResolveClicks");Assert(State(pet)=="Hop","Single click should hop");
   particles.Clear();clickMethod.Invoke(pet,new object[]{20.0,cursor});clickMethod.Invoke(pet,new object[]{20.0+interval*.2,cursor});Call(pet,"ResolveClicks");Assert(particles.Count==3,"Double click should make a small heart motion");
   particles.Clear();clickMethod.Invoke(pet,new object[]{30.0,cursor});clickMethod.Invoke(pet,new object[]{30.0+interval*.2,cursor});clickMethod.Invoke(pet,new object[]{30.0+interval*.4,cursor});Assert(particles.Count==22,"Triple click should burst hearts immediately");Call(pet,"ResolveClicks");
   using(var timeline=new Bitmap(pet.Width*4,pet.Height))using(var graphics=Graphics.FromImage(timeline)){graphics.Clear(Color.FromArgb(38,43,51));double[] steps={.12,.23,.4,.65};for(int k=0;k<4;k++){Step(pet,steps[k],cursor);using(var frame=Frame(pet))graphics.DrawImage(frame,k*pet.Width,0);}timeline.Save("heart-preview.png");}
   Step(pet,.2,cursor);Assert(particles.Count==0,"Hearts should disappear and release particles");
   var begin=pet.GetType().GetMethod("BeginPointer",flags);var move=pet.GetType().GetMethod("MovePointer",flags);var end=pet.GetType().GetMethod("EndPointer",flags);
   for(int count=1;count<=3;count++){
     particles.Clear();Set(pet,"pendingClicks",0);Set(pet,"lastClickTime",-10.0);Set(pet,"hopHeight",0.0);Set(pet,"speechTime",0.0);Set(pet,"moving",true);Mood(pet,"Walk");Set(pet,"moving",true);Set(pet,"walkDistance",12.0);
     using(var before=Frame(pet)){
       begin.Invoke(pet,new object[]{cursor});move.Invoke(pet,new object[]{new Point(cursor.X+1,cursor.Y+1)});Step(pet,.016,cursor);Assert(State(pet)=="Walk","Mouse press changed pose before real drag");
       using(var pressed=Frame(pet)){for(int j=0;j<before.Height;j++)for(int i=0;i<before.Width;i++)Assert(before.GetPixel(i,j)==pressed.GetPixel(i,j),"Mouse press caused a visual pose jump");}
       end.Invoke(pet,new object[]{50.0+count*10,cursor});
     }
     for(int click=1;click<count;click++){begin.Invoke(pet,new object[]{cursor});Step(pet,.016,cursor);Assert(State(pet)=="Walk","Repeated press showed a preparation pose");end.Invoke(pet,new object[]{50.0+count*10+click*interval*.2,cursor});}
     Assert(State(pet)=="Walk","Click sequence changed pose before classification");Call(pet,"ResolveClicks");
     Assert(State(pet)==(count==1?"Hop":"Walk"),"Only a single click should change to hop pose");Assert(particles.Count==(count==1?0:count==2?3:22),"Click effect count wrong");
   }
   Set(pet,"pendingClicks",0);begin.Invoke(pet,new object[]{cursor});move.Invoke(pet,new object[]{new Point(cursor.X+25,cursor.Y+10)});Assert(State(pet)=="Dragged","Real drag should change pose");end.Invoke(pet,new object[]{100.0,new Point(cursor.X+25,cursor.Y+10)});Assert(State(pet)=="Fall","Dragged release should land");
   ((PetSound)Get(pet,"sounds")).Dispose();var ambientBackend=new FakeAudio();var ambient=new PetSound(()=>new AudioEndpoint{Id="headphone",FormFactor=3,WaveId=2},ambientBackend);Set(pet,"sounds",ambient);
   Set(pet,"pointerDown",false);Set(pet,"pendingClicks",0);Mood(pet,"Walk");Set(pet,"x",(double)area.Left+area.Width/2-pet.Width/2);Set(pet,"y",(double)area.Bottom-pet.Height+8);Set(pet,"idleTime",0.0);Set(pet,"speed",64.0);Set(pet,"decisionTime",100.0);Set(pet,"autoHopIn",100.0);Set(pet,"walkSoundIn",0.0);Set(pet,"curiosityCooldown",100.0);Step(pet,.033,cursor);Assert(ambientBackend.Plays==1,"Walking voice not played");
   int previousPlays=ambientBackend.Plays;Assert(!ambient.PlayAmbient("sleep")&&ambientBackend.Plays==previousPlays,"Ambient sound interrupted an existing voice");
   ambientBackend.Finished=true;ambient.Tick(.1);Mood(pet,"Sleep");Set(pet,"sleepSoundIn",0.0);Step(pet,.033,cursor);Assert(ambientBackend.Plays==previousPlays+1,"Sleeping sound not played");int previousStops=ambientBackend.Stops;Step(pet,.033,new Point(cursor.X+20,cursor.Y));Assert(ambientBackend.Stops>previousStops,"Sleep sound kept playing after waking");
   Mood(pet,"Walk");Set(pet,"autoHopIn",0.0);Set(pet,"lastCursor",cursor);Set(pet,"idleTime",1.0);Set(pet,"speed",64.0);Set(pet,"decisionTime",100.0);Set(pet,"walkSoundIn",100.0);Step(pet,.033,cursor);Assert(State(pet)=="Hop","Independent autonomous jump not triggered");Assert((double)Get(pet,"idleTime")>0,"Autonomous jump reset user inactivity");ambient.Dispose();
 }
 var backend=new FakeAudio();AudioEndpoint endpoint=new AudioEndpoint{Id="speaker",Name="Speakers",FormFactor=1,WaveId=0};using(var sound=new PetSound(()=>endpoint,backend)){
   sound.Play("hop");Assert(backend.Plays==0,"Speaker playback must be blocked");
   endpoint=new AudioEndpoint{Id="headphones",Name="Headphones",FormFactor=3,WaveId=4};sound.Play("heart");Assert(backend.Plays==1&&backend.Device==4,"Sound must bind to the exact headphone device");
   int stops=backend.Stops;endpoint=null;sound.Tick(.1);Assert(backend.Stops>stops,"Unplug should stop sound");
   endpoint=new AudioEndpoint{Id="headphones",FormFactor=5,WaveId=2};sound.Play("burst");stops=backend.Stops;endpoint=new AudioEndpoint{Id="speaker",FormFactor=1,WaveId=0};sound.Tick(.1);Assert(backend.Stops>stops,"Changing output to speaker must stop sound");
   endpoint=new AudioEndpoint{Id="headphones",FormFactor=3,WaveId=uint.MaxValue};int plays=backend.Plays;sound.Play("hop");Assert(backend.Plays==plays,"Unknown mapping must not use default output");
   endpoint=new AudioEndpoint{Id="headphones",FormFactor=3,WaveId=2};sound.Enabled=false;sound.Play("heart");Assert(backend.Plays==plays,"Sound toggle must mute");
 }
 foreach(string effect in new[]{"hop","heart","burst","walk_ura","walk_purya","sleep"}){var samples=PetSound.LoadClip(effect);Assert(samples.Length>1000,"Missing audio clip: "+effect);int peak=0;foreach(short value in samples)peak=Math.Max(peak,Math.Abs((int)value));Assert(peak>100&&peak<16000,"Invalid effect amplitude");Assert(Math.Abs(samples[0])<5&&Math.Abs(samples[samples.Length-1])<10,"Effect edges click");}
 var actual=EndpointProbe.Read();Console.WriteLine("Current audio: "+(actual==null?"unavailable (muted)":actual.Name+"; form="+actual.FormFactor+"; headphone allowed="+actual.Headphones));if(actual!=null){uint id=(uint)typeof(EndpointProbe).GetMethod("FindWaveId",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{actual.Id});Assert(id!=uint.MaxValue,"Native endpoint mapping failed");Console.WriteLine("Native endpoint-to-wave device mapping verified without playing sound.");}
 Console.WriteLine("PASS: motion/clicks, headphone-only gate, device-bound playback selection, simulated unplug/output change, mute, effect sample quality.");
 }
 static void Features(){using(var pet=new Usagi()){
   ((PetSound)Get(pet,"sounds")).Enabled=false;var area=Screen.PrimaryScreen.WorkingArea;var far=new Point(area.Left,area.Top);Set(pet,"lastCursor",far);Set(pet,"x",area.Left+area.Width/2.0-pet.Width/2);Set(pet,"idleTime",0.0);
   Call(pet,"OfferCarrot");double initial=(double)Get(pet,"x");for(int i=0;i<160&&State(pet)!="Eat";i++)Step(pet,.02,far);Assert(State(pet)=="Eat","Did not reach carrot");Assert(((System.Collections.ICollection)Get(pet,"tracks")).Count>0,"Walking tracks missing");Assert(Math.Abs((double)Get(pet,"x")-initial)>50,"Carrot chase did not move");using(var frame=Frame(pet))frame.Save("feature-eating-preview.png");for(int i=0;i<170;i++)Step(pet,.02,far);Assert(!(bool)Get(pet,"carrotActive"),"Carrot never finished");
   Mood(pet,"Sit");Set(pet,"speed",0.0);Set(pet,"idleTime",0.0);using(var frame=Frame(pet)){}int center=(int)(double)Get(pet,"x")+pet.Width/2;int headY=(int)(double)Get(pet,"y")+pet.Height-63;Set(pet,"lastCursor",new Point(center,headY));for(int i=0;i<38;i++)Step(pet,.02,new Point(center+i%2*2,headY));Assert(State(pet)=="Nuzzle","Gentle hover strokes did not trigger affection");using(var frame=Frame(pet))frame.Save("feature-petting-preview.png");
   Call(pet,"BeginTrip");Step(pet,.3,far);Assert(State(pet)=="Trip","Trip stage missing");using(var frame=Frame(pet))frame.Save("feature-trip-preview.png");Step(pet,.4,far);Assert(State(pet)=="Recover","Recovery stage missing");Step(pet,1.2,far);Assert(State(pet)=="Walk","Trip did not resume walking");
   var night=typeof(Usagi).GetMethod("IsNight",BindingFlags.NonPublic|BindingFlags.Static);Assert((bool)night.Invoke(null,new object[]{new DateTime(2026,11,20,23,0,0)})&&!(bool)night.Invoke(null,new object[]{new DateTime(2026,11,20,12,0,0)}),"Day/night classification failed");
   string temp=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"usagi-settings-"+Guid.NewGuid().ToString("N")+".txt");Set(pet,"preferencesPath",temp);Set(pet,"preferencesEnabled",true);Set(pet,"scale",1.4f);Call(pet,"SavePreferences");Set(pet,"scale",.75f);Set(pet,"x",-9999.0);Call(pet,"LoadPreferences");Assert(Math.Abs((float)Get(pet,"scale")-1.4f)<.01&&!( (PetSound)Get(pet,"sounds")).Enabled,"Settings not restored");Assert((double)Get(pet,"x")>area.Left-pet.Width/2,"Position not restored");System.IO.File.WriteAllText(temp,"scale=NaN\nposition=Infinity\nscreen=missing");Call(pet,"LoadPreferences");Assert(!double.IsNaN((double)Get(pet,"x")),"Invalid settings poisoned position");pet.GetType().GetMethod("StartDayGreeting",flags).Invoke(pet,new object[]{new DateTime(2026,11,20,8,0,0)});Assert(State(pet)=="Stretch","Morning greeting missing");Mood(pet,"Walk");pet.GetType().GetMethod("StartDayGreeting",flags).Invoke(pet,new object[]{new DateTime(2026,11,20,9,0,0)});Assert(State(pet)=="Walk","Morning greeting repeated same day");Set(pet,"preferencesEnabled",false);System.IO.File.Delete(temp);
   Console.WriteLine("PASS: carrot chase/eating, hover affection, trip/recovery, day/night, settings round trip and corrupt input.");
 }}

}
