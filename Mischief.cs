using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
partial class Usagi {
    class FoodDrop {public double X;public Rectangle Area;public Usagi Winner,Renderer;}
    static readonly System.Collections.Generic.List<FoodDrop> foodDrops=new System.Collections.Generic.List<FoodDrop>();
    FoodDrop racingFood;
    double heldTime,scareDelay=-1,scareCooldown,accessoryIn=12,accessoryLeft;int accessoryKind;
    static int HeldFrame(double seconds){if(seconds<.25)return 0;if(seconds<2.5)return 1+(int)((seconds-.25)/.14)%2;if(seconds<2.9)return 3;return 4+(int)((seconds-2.9)/.55)%2;}
    void UpdateMischief(double dt){
        if(dragging)heldTime+=dt;else heldTime=0;
        scareCooldown=Math.Max(0,scareCooldown-dt);
        if(scareDelay>=0){if(SocialBusy||mood==Mood.Sleep||mood==Mood.Drowsy)scareDelay=-1;else{scareDelay-=dt;if(scareDelay<=0){scareDelay=-1;AutonomousHop();}}}
        if(accessoryLeft>0){accessoryLeft=Math.Max(0,accessoryLeft-dt);if(accessoryLeft==0){accessoryKind=0;accessoryIn=35+rng.NextDouble()*40;}}
        else{accessoryIn-=dt;if(accessoryIn<=0&&!SocialBusy&&(mood==Mood.Walk||mood==Mood.Look||mood==Mood.Sit)){accessoryKind=rng.Next(1,3);accessoryLeft=14+rng.NextDouble()*16;}}
    }
    void SurpriseNeighbors(){int order=0;foreach(var peer in livePets)if(SameFloor(peer)&&!peer.SocialBusy&&peer.mood!=Mood.Sleep&&peer.mood!=Mood.Drowsy&&peer.scareCooldown<=0&&Math.Abs(peer.x+peer.Width/2-(x+Width/2))<190){peer.scareDelay=.12+order++*.07+peer.rng.NextDouble()*.12;peer.scareCooldown=8;}}
    void PlaceSharedFood(){LeaveFoodRace();var area=PatrolArea();foreach(var old in foodDrops.ToArray())if(old.Area==area)CancelFoodDrop(old);
        double center=x+Width/2;int side=center<area.Left+area.Width/2?1:-1;
        var drop=new FoodDrop{X=Math.Max(area.Left+45,Math.Min(area.Right-45,center+side*145*scale)),Area=area,Renderer=this};foodDrops.Add(drop);
        var racers=new System.Collections.Generic.List<Usagi>(livePets);if(!racers.Contains(this))racers.Add(this);
        foreach(var pet in racers){if(pet!=this&&(!SameFloor(pet)||pet.SocialBusy||pet.mood==Mood.Sleep||pet.mood==Mood.Drowsy||Math.Abs(pet.x+pet.Width/2-drop.X)>480))continue;
            pet.CancelSocial();pet.racingFood=drop;pet.foodX=drop.X;pet.foodActive=true;pet.scatterActive=false;pet.paused=false;pet.pendingClicks=0;pet.idleTime=0;pet.hopHeight=0;pet.speed=0;pet.Change(Mood.Walk);pet.direction=drop.X>pet.x+pet.Width/2?1:-1;pet.targetSpeed=112;
        }Say("우라!");
    }
    bool ClaimFood(){var drop=racingFood;if(drop==null)return true;if(drop.Winner!=null)return drop.Winner==this;
        drop.Winner=this;foreach(var peer in livePets)if(peer!=this&&peer.racingFood==drop){peer.racingFood=null;peer.foodActive=false;peer.speed=0;peer.Change(Mood.Look);peer.decisionTime=1;peer.socialBreak=2;}return true;
    }
    static void CancelFoodDrop(FoodDrop drop){foodDrops.Remove(drop);foreach(var peer in livePets)if(peer.racingFood==drop){peer.racingFood=null;peer.foodActive=false;if(peer.mood==Mood.Eat){peer.speed=0;peer.Change(Mood.Look);}}}
    void FinishFood(){if(racingFood!=null){foodDrops.Remove(racingFood);racingFood=null;}foodActive=false;}
    void LeaveFoodRace(){var drop=racingFood;if(drop==null)return;racingFood=null;foodActive=false;if(drop.Winner==this){if(mood==Mood.Eat){speed=0;Change(Mood.Look);}CancelFoodDrop(drop);return;}bool others=false;foreach(var peer in livePets)if(peer!=this&&!peer.IsDisposed&&peer.racingFood==drop){others=true;if(drop.Renderer==this)drop.Renderer=peer;break;}if(!others)foodDrops.Remove(drop);}
    void DrawSharedFood(Graphics g){foreach(var drop in foodDrops){if(drop.Winner!=null||drop.Area!=PatrolArea())continue;if(drop.Renderer==null||drop.Renderer.IsDisposed||drop.Renderer.dragging){foreach(var peer in livePets)if(!peer.IsDisposed&&!peer.dragging&&peer.racingFood==drop){drop.Renderer=peer;break;}}
            if(drop.Renderer==this)DrawPizzaBun(g,(float)(drop.X-x),(float)(drop.Area.Bottom-9-y),scale,1);
        }}
    void DrawAccessory(Graphics g,float h){if(accessoryKind==0||accessoryLeft<=0||foodActive||!(mood==Mood.Walk||mood==Mood.Sit||mood==Mood.Look))return;var saved=g.Save();
        if(accessoryKind==1){g.TranslateTransform(0,-h*.43f);g.ScaleTransform(scale,scale);
            using(var lens=new SolidBrush(Color.FromArgb(242,40,48,54)))using(var frame=new Pen(Color.FromArgb(67,49,36),1.5f))using(var shine=new Pen(Color.FromArgb(160,192,212,217),1.1f)){
                g.FillEllipse(lens,-24,-6,21,13);g.FillEllipse(lens,3,-6,21,13);g.DrawEllipse(frame,-24,-6,21,13);g.DrawEllipse(frame,3,-6,21,13);g.DrawBezier(frame,-3,-2,-1,-4,1,-4,3,-2);g.DrawLine(frame,-24,-3,-30,-6);g.DrawLine(frame,24,-3,30,-6);g.DrawLine(shine,-19,-3,-12,0);g.DrawLine(shine,8,-3,15,0);
            }
        }else{g.TranslateTransform(8*scale,-h*.65f);g.ScaleTransform(scale,scale);g.RotateTransform(15+(float)Math.Sin(t*2)*4);
            using(var leaf=new GraphicsPath()){leaf.AddBezier(-2,2,-20,-5,-10,-20,10,-17);leaf.AddBezier(10,-17,12,-7,7,0,-2,2);leaf.CloseFigure();using(var green=new SolidBrush(Color.FromArgb(112,168,79)))g.FillPath(green,leaf);using(var edge=new Pen(Color.FromArgb(68,110,48),1.25f))g.DrawPath(edge,leaf);using(var vein=new Pen(Color.FromArgb(182,204,119),1))g.DrawLine(vein,-5,5,6,-13);}
        }g.Restore(saved);
    }
}
