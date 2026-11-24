using System;using System.Drawing;
static class MonitorCheck {
    static void Assert(bool value,string message){if(!value)throw new Exception(message);}
    internal static void Run(){
        var bounds=new[]{new Rectangle(0,0,1920,1080),new Rectangle(1920,0,2560,1440)};
        var aligned=new[]{new Rectangle(0,0,1920,1032),new Rectangle(1920,0,2560,1032)};
        double mapped;bool portal;
        Assert(MonitorLayout.CrossWalk(bounds,aligned,0,1,1921,90,out mapped,out portal)==1&&!portal&&mapped==1921,"Side-by-side walk did not preserve seam position");
        Assert(MonitorLayout.CrossWalk(bounds,aligned,1,-1,1918,90,out mapped,out portal)==0&&!portal&&mapped==1918,"Reverse walk did not cross back");
        Assert(MonitorLayout.CrossWalk(bounds,aligned,1,1,4573,90,out mapped,out portal)==0&&portal&&mapped==-87,"Outer edge did not wrap to the other monitor with overshoot");
        Assert(MonitorLayout.CrossFlight(bounds,0,1921,500)==1&&MonitorLayout.CrossFlight(bounds,1,1919,500)==0,"Thrown pet did not pass the shared vertical seam");
        Assert(MonitorLayout.Passage(bounds,0,1,-20)==-1,"Nonexistent monitor space became an open boundary");
        var stepped=new[]{new Rectangle(0,0,1920,1032),new Rectangle(1920,0,2560,1080)};
        double before=MonitorLayout.Floor(bounds,stepped,0,1919.99),after=MonitorLayout.Floor(bounds,stepped,1,1920.01);
        Assert(Math.Abs(before-after)<.1,"Different taskbar heights caused a floor jump at the seam");
        var high=new[]{new Rectangle(0,0,1920,1032),new Rectangle(1920,0,2560,1400)};
        Assert(MonitorLayout.CrossWalk(bounds,high,0,1,1921,90,out mapped,out portal)==-1,"Large height change moved before the exit animation");
        Assert(MonitorLayout.CrossWalk(bounds,high,0,1,2015,90,out mapped,out portal)==1&&portal&&mapped==1835,"Large height change did not map the walking entry");
        var vertical=new[]{new Rectangle(-100,0,1920,1080),new Rectangle(-100,-1080,1920,1080)};
        Assert(MonitorLayout.CrossFlight(vertical,0,400,-1)==1&&MonitorLayout.CrossFlight(vertical,1,400,1)==0,"Vertical monitor throw could not move up and down");
        Assert(MonitorLayout.CrossWalk(vertical,vertical,0,1,1915,90,out mapped,out portal)==1&&portal,"Stacked monitors were unreachable by walking");
        var negative=new[]{new Rectangle(-1920,-100,1920,1080),new Rectangle(0,-100,1920,1080)};
        Assert(MonitorLayout.CrossFlight(negative,1,-2,300)==0&&MonitorLayout.CrossFlight(negative,0,2,300)==1,"Negative monitor coordinates failed");
        var gap=new[]{new Rectangle(0,0,1920,1080),new Rectangle(2100,0,1920,1080)};
        Assert(MonitorLayout.Passage(gap,0,1,500)==-1&&MonitorLayout.CrossFlight(gap,0,1921,500)==-1,"Throw crossed an empty display gap");
        Assert(MonitorLayout.CrossWalk(gap,gap,0,1,2011,90,out mapped,out portal)==1&&portal,"Gapped monitors were unreachable by walking");
        var one=new[]{new Rectangle(0,0,1920,1080)};Assert(MonitorLayout.WalkNeighbor(one,one,0,1)==-1&&MonitorLayout.CrossFlight(one,0,1921,500)==-1,"Single monitor behavior changed");
        var mirror=new[]{one[0],one[0]};Assert(MonitorLayout.WalkNeighbor(mirror,mirror,0,1)==-1,"Mirrored duplicate formed a transfer loop");
        var three=new[]{new Rectangle(-1280,0,1280,1080),new Rectangle(0,0,1920,1080),new Rectangle(1920,0,1920,1080)};
        Assert(MonitorLayout.WalkNeighbor(three,three,0,1)==1&&MonitorLayout.WalkNeighbor(three,three,1,1)==2,"Walking skipped the adjacent monitor");
        Console.WriteLine("PASS: dual-monitor walk/throw routing, reverse crossing, taskbar floor continuity, different heights, stacked and negative layouts, gaps, duplicates and single-monitor fallback.");
    }
}
