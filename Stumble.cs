using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
partial class Usagi {
    double stumbleVelocity;
    static float SmoothBlend(double time,double from,double to){double u=Math.Max(0,Math.Min(1,(time-from)/(to-from)));return (float)(u*u*(3-2*u));}
    static void StumbleFrames(bool recovery,double time,out int a,out int b,out float mix){
        if(!recovery){if(time<.17){a=0;b=1;mix=SmoothBlend(time,.07,.17);}else if(time<.34){a=1;b=2;mix=SmoothBlend(time,.23,.34);}else{a=b=2;mix=0;}}
        else if(time<.18){a=2;b=3;mix=SmoothBlend(time,0,.18);}else if(time<.65){a=b=3;mix=0;}else if(time<.92){a=3;b=4;mix=SmoothBlend(time,.65,.92);}else if(time<1.2){a=4;b=5;mix=SmoothBlend(time,.97,1.2);}else{a=b=5;mix=0;}
    }
    static void EatingFrames(double time,out int a,out int b,out float mix){
        double[] thresholds={.45,.95,1.8,3.5,4.15};a=0;while(a<5&&time>=thresholds[a])a++;b=a;mix=0;
        if(a>0&&time<thresholds[a-1]+.11){b=a;a--;mix=SmoothBlend(time,thresholds[a],thresholds[a]+.11);}
    }
    void MoveStumble(double dt){if(stateTime>.34)return;var area=Screen.FromPoint(new System.Drawing.Point((int)x+Width/2,(int)y+Height-15)).WorkingArea;
        x=Math.Max(area.Left-Width/2+55*scale,Math.Min(area.Right-Width/2-55*scale,x+stumbleVelocity*dt));stumbleVelocity*=Math.Exp(-dt/.12);
    }
    void DrawSpriteBlend(Graphics g,Bitmap[] frames,int a,int b,float mix){DrawSpriteLayer(g,frames[mix>=.5f?b:a],1);}
    void DrawSpriteLayer(Graphics g,Bitmap pose,float alpha){if(alpha<=0)return;float factor=68*scale/headWidths[pose],w=pose.Width*factor,h=pose.Height*factor;
        if(alpha>=.999f){g.DrawImage(pose,-w/2,-h,w,h);return;}
        using(var attr=new ImageAttributes()){var matrix=new ColorMatrix();matrix.Matrix33=alpha;attr.SetColorMatrix(matrix);var points=new[]{new PointF(-w/2,-h),new PointF(w/2,-h),new PointF(-w/2,0)};g.DrawImage(pose,points,new RectangleF(0,0,pose.Width,pose.Height),GraphicsUnit.Pixel,attr);}
    }
    void DrawStumble(Graphics g){int a,b;float mix;StumbleFrames(mood==Mood.Recover,stateTime,out a,out b,out mix);DrawSpriteBlend(g,direction>0?stumble:leftStumble,a,b,mix);}
    void DrawEatingSprites(Graphics g){int a,b;float mix;EatingFrames(stateTime,out a,out b,out mix);DrawSpriteBlend(g,eating,a,b,mix);}
}
