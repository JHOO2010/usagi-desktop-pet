using System;
using System.Drawing;
using System.Windows.Forms;

partial class Usagi {
    double greetLeft,greetReplyDelay=-1,greetCooldown=3,socialBreak,playLeft,playCooldown=7,followLeft,followCooldown=5,napSeekTime;
    Usagi napFriend;bool following;
    bool SocialBusy {get{return IsDisposed||paused||menu.Visible||pointerDown||dragging||pendingClicks>0||foodActive||scatterActive||mood==Mood.Hop||mood==Mood.Fall||mood==Mood.Eat||mood==Mood.Nuzzle||mood==Mood.Trip||mood==Mood.Recover||mood==Mood.Stretch;}}
    bool SameFloor(Usagi other){return other!=this&&!other.IsDisposed&&Math.Abs((y+Height)-(other.y+other.Height))<8;}
    void CancelSocial(){greetLeft=0;greetReplyDelay=-1;playLeft=0;following=false;followLeft=0;napFriend=null;napSeekTime=0;socialBreak=3;}
    bool UpdateSocial(double dt,System.Drawing.Point cursor,bool mouseMoved){
        greetCooldown-=dt;playCooldown-=dt;followCooldown-=dt;socialBreak=Math.Max(0,socialBreak-dt);
        if(SocialBusy){CancelSocial();return false;}
        if(mouseMoved&&napFriend!=null){napFriend=null;napSeekTime=0;socialBreak=2;}
        if(greetLeft>0){greetLeft-=dt;moving=false;speed=0;
            if(greetReplyDelay>=0){greetReplyDelay-=dt;if(greetReplyDelay<0)Say("야하~!");}
            if(greetLeft<=0){Change(Mood.Walk);targetSpeed=58;decisionTime=2;}return true;}
        if(mood==Mood.Sleep)return false;
        if(napFriend!=null){
            if(napFriend.IsDisposed||napFriend.mood!=Mood.Sleep||!SameFloor(napFriend)||napSeekTime>7){napFriend=null;napSeekTime=0;}
            else{napSeekTime+=dt;double target=NapPosition(napFriend);if(SocialMove(dt,target,72,5)){speed=0;moving=false;Change(Mood.Sleep);sleepSoundIn=3;speechTime=0;napFriend=null;napSeekTime=0;}return true;}
        }
        if(idleTime>=9.5&&socialBreak<=0){foreach(var peer in livePets)if(SameFloor(peer)&&peer.mood==Mood.Sleep&&!peer.SocialBusy){napFriend=peer;napSeekTime=0;return true;}}
        if(mood==Mood.Drowsy||idleTime>=9.5)return false;
        if(playLeft>0){playLeft-=dt;following=false;
            if(playLeft>1.25){double goal=cursor.X-(cursor.X>x+Width/2?22:-22)*scale;SocialMove(dt,goal,90,4);if(Math.Abs(cursor.X-(x+Width/2))<28*scale){playLeft=1.25;AutoSay("우라!");}}
            else{double away=(x+Width/2)<cursor.X?-1:1;SocialMove(dt,x+Width/2+away*95,105,2);}
            if(playLeft<=0){Change(Mood.Look);socialBreak=2;decisionTime=1;}return true;
        }
        if(socialBreak>0)return false;
        double dx=cursor.X-(x+Width/2),dy=cursor.Y-(y+Height-55*scale);
        if(playCooldown<=0&&Math.Abs(dx)>32*scale&&Math.Abs(dx)<155*scale&&Math.Abs(dy)<55*scale&&mouseMoved){playLeft=3.2;playCooldown=16+rng.NextDouble()*10;AutoSay("하아?");return true;}
        if(greetCooldown<=0){foreach(var peer in livePets){double distance=peer.x+peer.Width/2-(x+Width/2);
            if(SameFloor(peer)&&!peer.SocialBusy&&peer.mood!=Mood.Sleep&&peer.mood!=Mood.Drowsy&&peer.greetLeft<=0&&peer.playLeft<=0&&peer.greetCooldown<=0&&direction!=peer.direction&&(moving||peer.moving)&&Math.Abs(distance)<44*(scale+peer.scale)&&Math.Abs(distance)>12){
                BeginGreeting(peer);return true;
            }
        }}
        if(following){Usagi leader=FindLeader();if(leader==null||followLeft<=0||leader.mood==Mood.Sleep){following=false;followCooldown=6;return false;}
            followLeft-=dt;double target=FollowPosition(leader);SocialMove(dt,target,Math.Min(104,Math.Abs(leader.speed)+35),5);decisionTime=2;return true;}
        if(followCooldown<=0){Usagi leader=FindLeader();if(leader!=null&&leader.moving){following=true;followLeft=10+rng.NextDouble()*7;followCooldown=16;return true;}}
        return false;
    }
    void BeginGreeting(Usagi peer){
        greetLeft=peer.greetLeft=1.5;greetCooldown=peer.greetCooldown=16;following=peer.following=false;speed=peer.speed=0;moving=peer.moving=false;
        direction=peer.x>x?1:-1;peer.direction=-direction;Change(Mood.Look);peer.Change(Mood.Look);Say("야하!");peer.greetReplyDelay=.28;
    }
    Usagi FindLeader(){Usagi leader=null;foreach(var peer in livePets)if(SameFloor(peer)&&!peer.SocialBusy&&peer.scale>scale+.05f&&(peer.mood==Mood.Walk||peer.mood==Mood.Turn||peer.mood==Mood.Look||peer.mood==Mood.Sit)&&(leader==null||peer.scale>leader.scale))leader=peer;return leader;}
    double FollowPosition(Usagi leader){double gap=0;float previous=leader.scale;var line=new System.Collections.Generic.List<Usagi>();foreach(var peer in livePets)if((peer==this||SameFloor(peer))&&peer!=leader&&peer.scale<leader.scale)line.Add(peer);line.Sort((a,b)=>{float bs=b.scale,asize=a.scale;int size=bs.CompareTo(asize);return size!=0?size:livePets.IndexOf(a).CompareTo(livePets.IndexOf(b));});
        foreach(var peer in line){gap+=35*(previous+peer.scale)+12;previous=peer.scale;if(peer==this)break;}return leader.x+leader.Width/2-leader.direction*gap;
    }
    double NapPosition(Usagi sleeper){int rank=0;foreach(var peer in livePets){if(peer==this)break;if(SameFloor(peer)&&(peer.napFriend==sleeper||peer.mood==Mood.Sleep))rank++;}int side=rank%2==0?-1:1;double spacing=(rank/2+1)*(35*(scale+sleeper.scale)+9);return sleeper.x+sleeper.Width/2+side*spacing;}
    bool SocialMove(double dt,double target,double maxSpeed,double tolerance){
        var area=Screen.FromPoint(new System.Drawing.Point((int)x+Width/2,(int)y+Height-15)).WorkingArea;target=Math.Max(area.Left+40*scale,Math.Min(area.Right-40*scale,target));double distance=target-(x+Width/2);
        settling=null;double old=x;if(Math.Abs(distance)<=tolerance){speed=0;moving=false;if(mood!=Mood.Look)Change(Mood.Look);return true;}
        int wanted=distance>0?1:-1;if(direction!=wanted&&speed>4){speed*=Math.Exp(-dt/.1);}else{direction=wanted;double goal=Math.Min(maxSpeed,Math.Abs(distance)*3);speed+=(goal-speed)*(1-Math.Exp(-dt/.2));x+=direction*Math.Min(Math.Abs(distance),speed*dt);}
        if(mood!=Mood.Walk)Change(Mood.Walk);moving=Math.Abs(x-old)>.01;walkDistance+=Math.Abs(x-old);if(moving)LeaveTracks(Math.Abs(x-old));return false;
    }
}
