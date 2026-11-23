using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

partial class Usagi {
    sealed class CharacterDef {
        internal string Key,Name,Action;internal double Pace;internal string[] Phrases;
        internal CharacterDef(string key,string name,double pace,string action,params string[] phrases){Key=key;Name=name;Pace=pace;Action=action;Phrases=phrases;}
    }
    static readonly CharacterDef[] characters={
        new CharacterDef("usagi","우사기",1,"폴짝!","야하!","우라!","푸루야!"),
        new CharacterDef("chiikawa","치이카와",.82,"수줍게 인사하기","와아…","응!","에헤헤","…에?"),
        new CharacterDef("hachiware","하치와레",.98,"흥얼거리기","같이 가자!","괜찮아!","에헤헤~","해냈어!"),
        new CharacterDef("momonga","모몽가",1.12,"살짝 활공하기","귀엽다고 해!","이 몸을 봐라!","쓰다듬어라!","흥!"),
        new CharacterDef("kurimanju","쿠리만쥬",.7,"느긋하게 한 모금","하아~","크으…","후우~","…하아."),
        new CharacterDef("rakko","랏코",1.05,"검술 연습하기","좋아.","잘했어.","함께 가자.","음!"),
        new CharacterDef("shisa","시사",.94,"꾸벅 인사하기","어서 오세요!","힘낼게요!","감사해요!","헤헤~")
    };
    event EventHandler CharacterChanged;CharacterDef character=characters[0];CharacterArt characterArt;double characterActionTime,characterActionIn=25;
    sealed class CharacterArt {
        internal Bitmap Sprite;internal Bitmap[] Walk=new Bitmap[4],LeftWalk=new Bitmap[4],Poses=new Bitmap[4],Stumble=new Bitmap[6],LeftStumble=new Bitmap[6],Eating=new Bitmap[6],Held=new Bitmap[6],Special=new Bitmap[4],SitFrames=new Bitmap[4],SleepFrames=new Bitmap[4],StretchFrames=new Bitmap[4],JumpFrames=new Bitmap[4];internal bool HasActionStrips;
        internal readonly Dictionary<Bitmap,float> Heads=new Dictionary<Bitmap,float>(),Centers=new Dictionary<Bitmap,float>();internal float BodyUnits,HeadUnits;
        internal CharacterArt(string key){string root=AppDomain.CurrentDomain.BaseDirectory;
            if(key=="usagi"){
                using(var source=new Bitmap(Path.Combine(root,"usagi.png")))Sprite=Crop(source);
                LoadSheet(Path.Combine(root,"walk.png"),Walk);LoadSheet(Path.Combine(root,"poses.png"),Poses);LoadSheet(Path.Combine(root,"stumble.png"),Stumble);LoadSheet(Path.Combine(root,"eating.png"),Eating);LoadSheet(Path.Combine(root,"held.png"),Held);
            }else if(CompleteActionPack(Path.Combine(root,"characters",key))){
                string folder=Path.Combine(root,"characters",key);LoadActionStrip(Path.Combine(folder,"walk.png"),Walk);LoadActionStrip(Path.Combine(folder,"sit.png"),SitFrames);LoadActionStrip(Path.Combine(folder,"sleep.png"),SleepFrames);LoadActionStrip(Path.Combine(folder,"stretch.png"),StretchFrames);LoadActionStrip(Path.Combine(folder,"jump.png"),JumpFrames);LoadActionStrip(Path.Combine(folder,"special.png"),Special);
                Sprite=SitFrames[0];Poses[0]=SitFrames[2];Poses[1]=SleepFrames[0];Poses[2]=StretchFrames[2];Poses[3]=JumpFrames[2];
                var fall=new Bitmap[4];LoadActionStrip(Path.Combine(folder,"stumble.png"),fall);int[] fallMap={0,1,2,2,3,3};for(int i=0;i<6;i++)Stumble[i]=fall[fallMap[i]];Stumble[5]=Sprite;
                var eat=new Bitmap[4];LoadActionStrip(Path.Combine(folder,"eat.png"),eat);int[] eatMap={0,0,1,2,3,3};for(int i=0;i<6;i++)Eating[i]=eat[eatMap[i]];
                var hold=new Bitmap[4];LoadActionStrip(Path.Combine(folder,"held.png"),hold);int[] holdMap={0,0,1,2,2,3};for(int i=0;i<6;i++)Held[i]=hold[holdMap[i]];HasActionStrips=true;
            }else using(var atlas=new Bitmap(Path.Combine(root,"characters",key+".png"))){
                var frames=new Bitmap[30];for(int i=0;i<30;i++){int col=i%6,row=i/6;var rect=Rectangle.FromLTRB(col*atlas.Width/6,row*atlas.Height/5,(col+1)*atlas.Width/6,(row+1)*atlas.Height/5);using(var cell=atlas.Clone(rect,PixelFormat.Format32bppArgb))frames[i]=CropCharacterCell(cell);}
                Sprite=frames[0];Array.Copy(frames,1,Walk,0,4);Array.Copy(frames,5,Poses,0,4);Array.Copy(frames,9,Stumble,0,6);Array.Copy(frames,15,Eating,0,6);Array.Copy(frames,21,Held,0,6);Array.Copy(frames,27,Special,0,3);
            }
            // Explicitly reviewed face contour endpoints; no pale-pixel or largest-region size estimate.
            var reference=MeasureBody(Sprite,true);float core=StandingBodyHeight(Sprite,reference,key=="momonga");float baseHead=key=="usagi"?MeasureHeadWidth(Sprite):MeasuredFace(Sprite).Width;BodyUnits=core*68/baseHead;HeadUnits=68;
            Heads[Sprite]=baseHead;Centers[Sprite]=key=="usagi"?Sprite.Width/2f:MeasuredFace(Sprite).Center;
            foreach(var group in new[]{Walk,Poses,Stumble,Eating,Held,Special,SitFrames,SleepFrames,StretchFrames,JumpFrames})foreach(var frame in group)if(frame!=null&&!Heads.ContainsKey(frame)){Heads[frame]=key=="usagi"?MeasureHeadWidth(frame):MeasuredFace(frame).Width;Centers[frame]=key=="usagi"?frame.Width/2f:MeasuredFace(frame).Center;}
            for(int i=0;i<4;i++){LeftWalk[i]=new Bitmap(Walk[i]);LeftWalk[i].RotateFlip(RotateFlipType.RotateNoneFlipX);Heads[LeftWalk[i]]=Heads[Walk[i]];Centers[LeftWalk[i]]=Walk[i].Width-Centers[Walk[i]];}
            for(int i=0;i<6;i++){LeftStumble[i]=new Bitmap(Stumble[i]);LeftStumble[i].RotateFlip(RotateFlipType.RotateNoneFlipX);Heads[LeftStumble[i]]=Heads[Stumble[i]];Centers[LeftStumble[i]]=Stumble[i].Width-Centers[Stumble[i]];}
        }
    }
    // Seven immutable packs are shared by every friend, including after switching.
    // They remain valid until process exit; closing one friend never frees another's art.
    static readonly Dictionary<string,CharacterArt> characterCache=new Dictionary<string,CharacterArt>();
    static readonly string[] actionNames={"walk","sit","sleep","stretch","jump","eat","stumble","held","special"};
    static bool CompleteActionPack(string folder){foreach(string action in actionNames)if(!File.Exists(Path.Combine(folder,action+".png")))return false;return true;}
    static void LoadActionStrip(string path,Bitmap[] frames){using(var strip=new Bitmap(path))for(int i=0;i<4;i++){var bounds=Rectangle.FromLTRB(i*strip.Width/4,0,(i+1)*strip.Width/4,strip.Height);using(var cell=strip.Clone(bounds,PixelFormat.Format32bppArgb))frames[i]=CropCharacterCell(cell);RegisterFaceLandmark(path,i,frames[i]);}}
    static CharacterArt GetCharacterArt(string key){CharacterArt art;if(!characterCache.TryGetValue(key,out art)){art=new CharacterArt(key);characterCache.Add(key,art);}return art;}
    static CharacterDef FindCharacter(string key){foreach(var candidate in characters)if(candidate.Key==key)return candidate;return characters[0];}
    void ApplyCharacter(string key,bool react){var next=FindCharacter(key);var art=GetCharacterArt(next.Key);if(react){LeaveRap();LeaveFoodRace();CancelSocial();scatterActive=false;hopHeight=0;speed=0;Change(Mood.Look);}
        character=next;characterArt=art;sounds.CharacterKey=next.Key;sprite=art.Sprite;Array.Copy(art.Walk,walk,4);Array.Copy(art.LeftWalk,leftWalk,4);Array.Copy(art.Poses,poses,4);Array.Copy(art.Stumble,stumble,6);Array.Copy(art.LeftStumble,leftStumble,6);Array.Copy(art.Eating,eating,6);Array.Copy(art.Held,held,6);headWidths.Clear();foreach(var pair in art.Heads)headWidths.Add(pair.Key,pair.Value);
        walkingSpeechHeight=0;characterActionTime=0;characterActionIn=18+rng.NextDouble()*25;Text="먼작귀 펫 · "+next.Name;if(tray!=null){var icon=CreateCharacterIcon();tray.Icon=icon;Icon=icon;tray.Text="먼작귀 펫 · "+character.Name+" · "+targetFrameRate+"Hz · 1번 폴짝 / 2번 하트";var previous=trayIcon;trayIcon=icon;if(previous!=null)previous.Dispose();}if(react){SayFor("greet",next.Phrases[0]);EmitHearts(false);SavePreferences();}RefreshCharacterMenu();if(CharacterChanged!=null)CharacterChanged(this,EventArgs.Empty);
    }
    [System.Runtime.InteropServices.DllImport("user32.dll")]static extern bool DestroyIcon(IntPtr icon);
    Bitmap CreateCharacterLogo(int size,bool faceOnly){var result=new Bitmap(size,size);using(var g=Graphics.FromImage(result)){g.Clear(Color.Transparent);g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;float ratio=character.Key=="usagi"?.76f:character.Key=="rakko"?.61f:character.Key=="shisa"?.70f:.68f;var source=new RectangleF(0,0,sprite.Width,faceOnly?sprite.Height*ratio:sprite.Height);float factor=Math.Min((size-2)/source.Width,(size-2)/source.Height),w=source.Width*factor,h=source.Height*factor;g.DrawImage(sprite,new RectangleF((size-w)/2,(size-h)/2,w,h),source,GraphicsUnit.Pixel);}return result;}
    Icon CreateCharacterIcon(){using(var image=CreateCharacterLogo(64,true)){IntPtr handle=image.GetHicon();try{using(var icon=Icon.FromHandle(handle))return (Icon)icon.Clone();}finally{DestroyIcon(handle);}}}
    void RefreshCharacterMenu(){if(menu!=null){((BubbleMenu)menu).Heading=character.Name;if(cloneMenuAction!=null)cloneMenuAction.Text="친구 추가…";if(cleanupMenu!=null)cleanupMenu.Text="친구들 정리";if(foodMenuAction!=null)foodMenuAction.Text=CharacterFoodName+" 주기";RefreshRapMenu();}}
    string CharacterSpeech(string text){return SpeechFor(characterActionTime>0?"signature":mood==Mood.Sleep||mood==Mood.Drowsy?"sleep":mood==Mood.Dragged?"held":moving?"walk":"idle",text);}
    void TickCharacter(double dt){if(character.Key=="usagi"||characterActionTime>0)return;characterActionIn-=dt;if(characterActionIn<=0&&!SocialBusy&&!infoOpen&&(mood==Mood.Walk||mood==Mood.Look||mood==Mood.Sit)){BeginCharacterAction();}}
    void BeginCharacterAction(){if(character.Key=="usagi"){StartHop();return;}LeaveRap();LeaveFoodRace();CancelSocial();paused=false;pendingClicks=0;idleTime=0;speed=0;moving=false;scatterActive=false;hopHeight=0;Change(Mood.Look);characterActionTime=.001;characterActionIn=28+rng.NextDouble()*35;AutoSayFor("signature",character.Phrases[0]);sounds.PlayAmbient("signature");}
    bool UpdateCharacterAction(double dt){if(characterActionTime<=0)return false;if(pointerDown||dragging||paused||foodActive){characterActionTime=0;hopHeight=0;return false;}characterActionTime+=dt;idleTime=0;moving=false;speed=0;
        if(character.Key=="momonga"){double u=Math.Max(0,Math.Min(1,(characterActionTime-.2)/2.1)),glide=Math.Sin(u*Math.PI);hopHeight=Math.Pow(glide,1.6)*27;var area=PatrolArea();x=Math.Max(area.Left-Width/2+45*scale,Math.Min(area.Right-Width/2-45*scale,x+direction*60*glide*dt));}
        double duration=character.Key=="kurimanju"?4:2.5;if(characterActionTime>=duration){characterActionTime=0;hopHeight=0;Change(Mood.Look);decisionTime=1;}return true;
    }
    Bitmap CharacterActionPose(){double duration=character.Key=="kurimanju"?4:2.5;double u=characterActionTime/duration;int frame=characterArt.HasActionStrips?(u<.18?0:u<.38?1:u<.78?2:3):(u<.25?0:u<.75?1:2);return characterArt.Special[frame]??sprite;}
    void CharacterActionMotion(out float angle,out float bob,out float squash){double seconds=characterActionTime,duration=character.Key=="kurimanju"?4:2.5,u=Math.Min(1,seconds/duration),envelope=Math.Sin(u*Math.PI);angle=0;bob=0;squash=0;
        if(character.Key=="chiikawa"){angle=(float)(Math.Sin(seconds*2.6)*envelope*2.5);}
        else if(character.Key=="hachiware"){angle=(float)(Math.Sin(seconds*4.8)*envelope*3);bob=(float)(Math.Abs(Math.Sin(seconds*4.8))*envelope*1.5);}
        else if(character.Key=="momonga"){angle=(float)(direction*envelope*-4);squash=(float)(.04*(Math.Exp(-Math.Pow((seconds-.14)/.1,2))+Math.Exp(-Math.Pow((seconds-2.35)/.12,2))));}
        else if(character.Key=="kurimanju"){angle=(float)(Math.Sin(u*Math.PI)*-4);bob=(float)(Math.Sin(seconds*1.5)*.35);}
        else if(character.Key=="rakko"){angle=seconds<.4?SmoothBlend(seconds,0,.4)*7:seconds<.72?7-SmoothBlend(seconds,.4,.72)*15:-8*(1-SmoothBlend(seconds,.72,1.4));}
        else if(character.Key=="shisa"){bob=(float)(Math.Sin(u*Math.PI)*2.4);squash=(float)(envelope*.018);}
    }
    void AddFriend(string key){if(livePets.Count>=16){Say(character.Phrases[0]);return;}var pet=new Usagi(true,this);pet.ApplyCharacter(key,false);pet.scale=scale;pet.sounds.Mode=sounds.Mode;pet.x=x;pet.y=y;pet.scatterActive=true;pet.scatterVX=(rng.Next(2)==0?-1:1)*(140+rng.Next(90));pet.scatterVY=-235;pet.Change(Mood.Fall);livePets.Add(pet);pet.tray.Visible=true;pet.Show();pet.Render();}

    sealed class CharacterChoice : SettingButton {
        internal Bitmap Portrait;internal float Head;internal CharacterChoice(){HideCaption=true;}
        protected override void OnPaint(PaintEventArgs e){string caption=Text;base.OnPaint(e);var g=e.Graphics;g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;if(Portrait!=null){float factor=Math.Min(54/Head,57f/Portrait.Height),h=Portrait.Height*factor,w=Portrait.Width*factor;g.DrawImage(Portrait,(Width-w)/2,61-h,w,h);}else using(var placeholder=new SolidBrush(Color.FromArgb(35,PetTheme.Muted)))g.FillEllipse(placeholder,Width/2-19,17,38,38);using(var ink=new SolidBrush(Chosen?PetTheme.SelectionInk:SettingArt.Ink))using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})g.DrawString(caption,Font,ink,new RectangleF(0,63,Width,25),format);}
    }
    sealed class PickerPortrait {internal Bitmap Image;internal float Head;}
    static readonly Dictionary<string,PickerPortrait> pickerPortraits=new Dictionary<string,PickerPortrait>();
    static PickerPortrait GetPickerPortrait(string key){lock(pickerPortraits){PickerPortrait portrait;if(pickerPortraits.TryGetValue(key,out portrait))return portrait;if(key=="usagi"){var art=GetCharacterArt(key);portrait=new PickerPortrait{Image=art.Sprite,Head=art.Heads[art.Sprite]};}else{string path=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"characters",key,"sit.png");using(var strip=new Bitmap(path))using(var cell=strip.Clone(new Rectangle(0,0,strip.Width/4,strip.Height),PixelFormat.Format32bppArgb))portrait=new PickerPortrait{Image=CropCharacterCell(cell),Head=faceLandmarks[key+"/sit/0"].Width};}pickerPortraits.Add(key,portrait);return portrait;}}
    void ShowCharacterPicker(Form form,bool adding){if(form.Controls.ContainsKey("CharacterPage"))return;IntPtr previewHandle=form.Handle;var hidden=new List<Control>();foreach(Control control in form.Controls)if(control.Top>=48){hidden.Add(control);control.Visible=false;}
        var page=new GlassPanel{Name="CharacterPage",Location=new Point(24,72),Size=new Size(352,form.Height-104),BackColor=Color.Transparent,AutoScroll=true};form.Controls.Add(page);page.BringToFront();Action back=()=>{form.Controls.Remove(page);page.Dispose();foreach(var control in hidden)control.Visible=true;RefreshCharacterMenu();};
        var backButton=new SettingButton{Text="뒤로가기",IconName="back",IconOnly=true,Location=new Point(0,0),Size=new Size(32,32),AccessibleName="뒤로가기"};page.Controls.Add(backButton);backButton.Click+=(s,e)=>back();page.Controls.Add(new Label{Text=adding?"함께 산책할 친구":"이 친구로 바꾸기",Location=new Point(45,5),Size=new Size(300,28),UseCompatibleTextRendering=true});
        for(int i=0;i<characters.Length;i++){var choice=characters[i];var tile=new CharacterChoice{Text=choice.Name,AccessibleName=choice.Name+(adding?" 추가":" 선택"),Chosen=!adding&&choice==character,Location=new Point(i%3*117,45+i/3*94),Size=new Size(110,90)};page.Controls.Add(tile);tile.Click+=(s,e)=>{if(adding)AddFriend(choice.Key);else ApplyCharacter(choice.Key,true);back();};if(choice.Key=="usagi"){var art=GetCharacterArt(choice.Key);tile.Portrait=art.Sprite;tile.Head=art.Heads[art.Sprite];}else System.Threading.Tasks.Task.Factory.StartNew(()=>{try{var portrait=GetPickerPortrait(choice.Key);if(form.IsDisposed||page.IsDisposed)return;form.BeginInvoke(new Action(()=>{if(tile.IsDisposed)return;tile.Portrait=portrait.Image;tile.Head=portrait.Head;tile.Invalidate();}));}catch(InvalidOperationException){}catch(IOException){}});}
        PetTheme.Apply(page);backButton.Focus();
    }
    void ShowAddFriend(){using(var form=CreateSettingsForm()){ShowCharacterPicker(form,true);bool previous=infoOpen;infoOpen=true;try{form.ShowDialog(this);}finally{infoOpen=previous;}}}
}
