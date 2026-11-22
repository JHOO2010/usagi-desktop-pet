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
        new CharacterDef("momonga","모몽가",1.12,"살짝 활공하기","귀엽지?","봐줘!","쓰다듬어줘~","흥!"),
        new CharacterDef("kurimanju","쿠리만쥬",.7,"느긋하게 한 모금","하아~","…음.","후우~","좋구나…"),
        new CharacterDef("rakko","랏코",1.05,"검술 연습하기","좋아.","잘했어.","함께 가자.","음!"),
        new CharacterDef("shisa","시사",.94,"꾸벅 인사하기","어서 오세요!","힘낼게요!","감사해요!","헤헤~")
    };
    CharacterDef character=characters[0];CharacterArt characterArt;double characterActionTime,characterActionIn=25;
    sealed class CharacterArt {
        internal Bitmap Sprite;internal Bitmap[] Walk=new Bitmap[4],LeftWalk=new Bitmap[4],Poses=new Bitmap[4],Stumble=new Bitmap[6],LeftStumble=new Bitmap[6],Eating=new Bitmap[6],Held=new Bitmap[6],Special=new Bitmap[3];
        internal readonly Dictionary<Bitmap,float> Heads=new Dictionary<Bitmap,float>();
        internal CharacterArt(string key){string root=AppDomain.CurrentDomain.BaseDirectory;
            if(key=="usagi"){
                using(var source=new Bitmap(Path.Combine(root,"usagi.png")))Sprite=Crop(source);
                LoadSheet(Path.Combine(root,"walk.png"),Walk);LoadSheet(Path.Combine(root,"poses.png"),Poses);LoadSheet(Path.Combine(root,"stumble.png"),Stumble);LoadSheet(Path.Combine(root,"eating.png"),Eating);LoadSheet(Path.Combine(root,"held.png"),Held);
            }else using(var atlas=new Bitmap(Path.Combine(root,"characters",key+".png"))){
                var frames=new Bitmap[30];for(int i=0;i<30;i++){int col=i%6,row=i/6;var rect=Rectangle.FromLTRB(col*atlas.Width/6,row*atlas.Height/5,(col+1)*atlas.Width/6,(row+1)*atlas.Height/5);using(var cell=atlas.Clone(rect,PixelFormat.Format32bppArgb))frames[i]=Crop(cell);}
                Sprite=frames[0];Array.Copy(frames,1,Walk,0,4);Array.Copy(frames,5,Poses,0,4);Array.Copy(frames,9,Stumble,0,6);Array.Copy(frames,15,Eating,0,6);Array.Copy(frames,21,Held,0,6);Array.Copy(frames,27,Special,0,3);
            }
            Heads[Sprite]=key=="usagi"?MeasureHeadWidth(Sprite):MeasureCharacterFace(Sprite);foreach(var group in new[]{Walk,Poses,Stumble,Eating,Held,Special})foreach(var frame in group)if(frame!=null)Heads[frame]=key=="usagi"?MeasureHeadWidth(frame):MeasureCharacterFace(frame);
            for(int i=0;i<4;i++){LeftWalk[i]=new Bitmap(Walk[i]);LeftWalk[i].RotateFlip(RotateFlipType.RotateNoneFlipX);Heads[LeftWalk[i]]=Heads[Walk[i]];}
            for(int i=0;i<6;i++){LeftStumble[i]=new Bitmap(Stumble[i]);LeftStumble[i].RotateFlip(RotateFlipType.RotateNoneFlipX);Heads[LeftStumble[i]]=Heads[Stumble[i]];}
        }
    }
    // Seven immutable packs are shared by every friend, including after switching.
    // They remain valid until process exit; closing one friend never frees another's art.
    static readonly Dictionary<string,CharacterArt> characterCache=new Dictionary<string,CharacterArt>();
    static float MeasureCharacterFace(Bitmap frame){int widest=0;var bits=frame.LockBits(new Rectangle(0,0,frame.Width,frame.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);try{var row=new byte[frame.Width*4];for(int y=0;y<(int)(frame.Height*.75);y++){System.Runtime.InteropServices.Marshal.Copy(IntPtr.Add(bits.Scan0,y*bits.Stride),row,0,row.Length);int left=frame.Width,right=-1;for(int x=0;x<frame.Width;x++){int p=x*4;if(row[p+3]>180&&row[p+2]>=220&&row[p+1]>=195&&row[p]>=140&&row[p+2]>=row[p]+3){left=Math.Min(left,x);right=Math.Max(right,x);}}if(right>=left)widest=Math.Max(widest,right-left+1);}}finally{frame.UnlockBits(bits);}return widest>16?widest*1.045f:MeasureHeadWidth(frame);}
    static CharacterArt GetCharacterArt(string key){CharacterArt art;if(!characterCache.TryGetValue(key,out art)){art=new CharacterArt(key);characterCache.Add(key,art);}return art;}
    static CharacterDef FindCharacter(string key){foreach(var candidate in characters)if(candidate.Key==key)return candidate;return characters[0];}
    void ApplyCharacter(string key,bool react){var next=FindCharacter(key);var art=GetCharacterArt(next.Key);if(react){LeaveRap();LeaveFoodRace();CancelSocial();scatterActive=false;hopHeight=0;speed=0;Change(Mood.Look);}
        character=next;characterArt=art;sounds.CharacterKey=next.Key;sprite=art.Sprite;Array.Copy(art.Walk,walk,4);Array.Copy(art.LeftWalk,leftWalk,4);Array.Copy(art.Poses,poses,4);Array.Copy(art.Stumble,stumble,6);Array.Copy(art.LeftStumble,leftStumble,6);Array.Copy(art.Eating,eating,6);Array.Copy(art.Held,held,6);headWidths.Clear();foreach(var pair in art.Heads)headWidths.Add(pair.Key,pair.Value);
        walkingSpeechHeight=0;characterActionTime=0;characterActionIn=18+rng.NextDouble()*25;Text="먼작귀 펫 · "+next.Name;if(tray!=null){var icon=CreateCharacterIcon();tray.Icon=icon;var previous=trayIcon;trayIcon=icon;if(previous!=null)previous.Dispose();}if(react){Say(next.Phrases[0]);EmitHearts(false);SavePreferences();}RefreshCharacterMenu();
    }
    [System.Runtime.InteropServices.DllImport("user32.dll")]static extern bool DestroyIcon(IntPtr icon);
    Icon CreateCharacterIcon(){using(var image=new Bitmap(64,64))using(var g=Graphics.FromImage(image)){g.Clear(Color.Transparent);g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;float factor=Math.Min(60f/sprite.Width,60f/sprite.Height);float w=sprite.Width*factor,h=sprite.Height*factor;g.DrawImage(sprite,(64-w)/2,(64-h)/2,w,h);IntPtr handle=image.GetHicon();try{using(var icon=Icon.FromHandle(handle))return (Icon)icon.Clone();}finally{DestroyIcon(handle);}}}
    void RefreshCharacterMenu(){if(menu!=null){((BubbleMenu)menu).Heading=character.Name;if(cloneMenuAction!=null)cloneMenuAction.Text="친구 추가…";if(cleanupMenu!=null)cleanupMenu.Text="친구들 정리";}}
    string CharacterSpeech(string text){if(character.Key=="usagi")return text;if(characterActionTime>0)return character.Phrases[0];return character.Phrases[rng.Next(character.Phrases.Length)];}
    void TickCharacter(double dt){if(character.Key=="usagi"||characterActionTime>0)return;characterActionIn-=dt;if(characterActionIn<=0&&!SocialBusy&&!infoOpen&&(mood==Mood.Walk||mood==Mood.Look||mood==Mood.Sit)){BeginCharacterAction();}}
    void BeginCharacterAction(){if(character.Key=="usagi"){StartHop();return;}LeaveRap();LeaveFoodRace();CancelSocial();speed=0;moving=false;scatterActive=false;hopHeight=0;Change(Mood.Look);characterActionTime=.001;characterActionIn=28+rng.NextDouble()*35;AutoSay(character.Phrases[0]);sounds.PlayAmbient("signature");}
    bool UpdateCharacterAction(double dt){if(characterActionTime<=0)return false;if(pointerDown||dragging||paused||foodActive){characterActionTime=0;hopHeight=0;return false;}characterActionTime+=dt;idleTime=0;moving=false;speed=0;
        if(character.Key=="momonga"){double u=Math.Min(1,characterActionTime/2.5);hopHeight=Math.Sin(u*Math.PI)*27;var area=PatrolArea();x=Math.Max(area.Left-Width/2+45*scale,Math.Min(area.Right-Width/2-45*scale,x+direction*40*dt));}
        double duration=character.Key=="kurimanju"?4:2.5;if(characterActionTime>=duration){characterActionTime=0;hopHeight=0;Change(Mood.Look);decisionTime=1;}return true;
    }
    Bitmap CharacterActionPose(){double duration=character.Key=="kurimanju"?4:2.5;double u=characterActionTime/duration;int frame=u<.25?0:u<.75?1:2;return characterArt.Special[frame]??sprite;}
    void AddFriend(string key){if(livePets.Count>=16){Say(character.Phrases[0]);return;}var pet=new Usagi(true,this);pet.ApplyCharacter(key,false);pet.scale=cloneSizes[rng.Next(cloneSizes.Length)];pet.sounds.Enabled=sounds.Enabled;pet.x=x;pet.y=y;pet.scatterActive=true;pet.scatterVX=(rng.Next(2)==0?-1:1)*(140+rng.Next(90));pet.scatterVY=-235;pet.Change(Mood.Fall);livePets.Add(pet);pet.Show();pet.Render();}

    sealed class CharacterChoice : SettingButton {
        internal Bitmap Portrait;internal float Head;internal CharacterChoice(){HideCaption=true;}
        protected override void OnPaint(PaintEventArgs e){string caption=Text;base.OnPaint(e);var g=e.Graphics;g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;float factor=Math.Min(54/Head,57f/Portrait.Height),h=Portrait.Height*factor,w=Portrait.Width*factor;g.DrawImage(Portrait,(Width-w)/2,61-h,w,h);using(var ink=new SolidBrush(SettingArt.Ink))using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})g.DrawString(caption,Font,ink,new RectangleF(0,63,Width,25),format);}
    }
    void ShowCharacterPicker(Form form,bool adding){if(form.Controls.ContainsKey("CharacterPage"))return;var hidden=new List<Control>();foreach(Control control in form.Controls)if(control.Top>=48){hidden.Add(control);control.Visible=false;}
        var page=new Panel{Name="CharacterPage",Location=new Point(26,56),Size=new Size(368,form.Height-86),BackColor=SettingArt.Cream,AutoScroll=true};form.Controls.Add(page);page.BringToFront();Action back=()=>{form.Controls.Remove(page);page.Dispose();foreach(var control in hidden)control.Visible=true;RefreshCharacterMenu();};
        var backButton=new SettingButton{Text="뒤로가기",IconName="back",IconOnly=true,Location=new Point(0,0),Size=new Size(32,32),AccessibleName="뒤로가기"};page.Controls.Add(backButton);backButton.Click+=(s,e)=>back();page.Controls.Add(new Label{Text=adding?"함께 산책할 친구":"이 친구로 바꾸기",Location=new Point(45,5),Size=new Size(300,28),UseCompatibleTextRendering=true});
        for(int i=0;i<characters.Length;i++){var choice=characters[i];var art=GetCharacterArt(choice.Key);var tile=new CharacterChoice{Text=choice.Name,AccessibleName=choice.Name+(adding?" 추가":" 선택"),Portrait=art.Sprite,Head=art.Heads[art.Sprite],Chosen=!adding&&choice==character,Location=new Point(i%3*123,45+i/3*98),Size=new Size(116,90)};page.Controls.Add(tile);tile.Click+=(s,e)=>{if(adding)AddFriend(choice.Key);else ApplyCharacter(choice.Key,true);back();};}
        var signature=new SettingButton{Text="특유 행동 · "+character.Action,Location=new Point(123,70+2*98),Size=new Size(239,40),Visible=!adding};signature.Click+=(s,e)=>{back();form.DialogResult=DialogResult.OK;form.Close();BeginCharacterAction();};page.Controls.Add(signature);backButton.Focus();
    }
    void ShowAddFriend(){using(var form=CreateSettingsForm()){ShowCharacterPicker(form,true);bool previous=infoOpen;infoOpen=true;try{form.ShowDialog(this);}finally{infoOpen=previous;}}}
}
