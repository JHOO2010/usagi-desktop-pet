using System;
using System.Drawing;
using System.Windows.Forms;
partial class Usagi {
    class FoodDrop {public double X;public Rectangle Area;public Usagi Winner,Renderer;public string Kind;}
    static readonly System.Collections.Generic.List<FoodDrop> foodDrops=new System.Collections.Generic.List<FoodDrop>();
    FoodDrop racingFood;
    double heldTime,scareDelay=-1,scareCooldown;
    static int HeldFrame(double seconds){if(seconds<.25)return 0;if(seconds<2.5)return 1+(int)((seconds-.25)/.14)%2;if(seconds<2.9)return 3;return 4+(int)((seconds-2.9)/.55)%2;}
    void UpdateMischief(double dt){
        if(dragging)heldTime+=dt;else heldTime=0;
        scareCooldown=Math.Max(0,scareCooldown-dt);
        if(scareDelay>=0){if(SocialBusy||mood==Mood.Sleep||mood==Mood.Drowsy)scareDelay=-1;else{scareDelay-=dt;if(scareDelay<=0){scareDelay=-1;AutonomousHop();}}}

    }
    void SurpriseNeighbors(){int order=0;foreach(var peer in livePets)if(SameFloor(peer)&&!peer.SocialBusy&&peer.mood!=Mood.Sleep&&peer.mood!=Mood.Drowsy&&peer.scareCooldown<=0&&Math.Abs(peer.x+peer.Width/2-(x+Width/2))<190){peer.scareDelay=.12+order++*.07+peer.rng.NextDouble()*.12;peer.scareCooldown=8;}}
    void PlaceSharedFood(){LeaveRap();LeaveFoodRace();var area=PatrolArea();foreach(var old in foodDrops.ToArray())if(old.Area==area)CancelFoodDrop(old);
        double center=x+Width/2;int side=center<area.Left+area.Width/2?1:-1;
        var drop=new FoodDrop{X=Math.Max(area.Left+45,Math.Min(area.Right-45,center+side*145*scale)),Area=area,Renderer=this,Kind=CharacterFoodKey};foodDrops.Add(drop);
        var racers=new System.Collections.Generic.List<Usagi>(livePets);if(!racers.Contains(this))racers.Add(this);
        foreach(var pet in racers){if(pet!=this&&(pet.CharacterFoodKey!=drop.Kind||!SameFloor(pet)||pet.SocialBusy||pet.mood==Mood.Sleep||pet.mood==Mood.Drowsy||Math.Abs(pet.x+pet.Width/2-drop.X)>480))continue;
            pet.CancelSocial();pet.racingFood=drop;pet.foodX=drop.X;pet.foodActive=true;pet.scatterActive=false;pet.paused=false;pet.pendingClicks=0;pet.idleTime=0;pet.hopHeight=0;pet.speed=0;pet.Change(Mood.Walk);pet.direction=drop.X>pet.x+pet.Width/2?1:-1;pet.targetSpeed=112;
        }SayFor("food","우라!");
    }
    bool ClaimFood(){var drop=racingFood;if(drop==null)return true;if(drop.Winner!=null)return drop.Winner==this;
        drop.Winner=this;foreach(var peer in livePets)if(peer!=this&&peer.racingFood==drop){peer.racingFood=null;peer.foodActive=false;peer.speed=0;peer.Change(Mood.Look);peer.decisionTime=1;peer.socialBreak=2;}return true;
    }
    static void CancelFoodDrop(FoodDrop drop){foodDrops.Remove(drop);foreach(var peer in livePets)if(peer.racingFood==drop){peer.racingFood=null;peer.foodActive=false;if(peer.mood==Mood.Eat){peer.speed=0;peer.Change(Mood.Look);}}}
    void FinishFood(){if(racingFood!=null){foodDrops.Remove(racingFood);racingFood=null;}foodActive=false;}
    void LeaveFoodRace(){var drop=racingFood;if(drop==null)return;racingFood=null;foodActive=false;if(drop.Winner==this){if(mood==Mood.Eat){speed=0;Change(Mood.Look);}CancelFoodDrop(drop);return;}bool others=false;foreach(var peer in livePets)if(peer!=this&&!peer.IsDisposed&&peer.racingFood==drop){others=true;if(drop.Renderer==this)drop.Renderer=peer;break;}if(!others)foodDrops.Remove(drop);}
    void DrawSharedFood(Graphics g){foreach(var drop in foodDrops){if(drop.Winner!=null||drop.Area!=PatrolArea())continue;if(drop.Renderer==null||drop.Renderer.IsDisposed||drop.Renderer.dragging){foreach(var peer in livePets)if(!peer.IsDisposed&&!peer.dragging&&peer.racingFood==drop){drop.Renderer=peer;break;}}
            if(drop.Renderer==this)DrawCharacterFood(g,drop.Kind,(float)(drop.X-x),(float)(drop.Area.Bottom-9-y),scale);
        }}
}
