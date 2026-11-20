using System;
using System.Drawing;
using System.Windows.Forms;
partial class Usagi {
    Screen patrolScreen;
    Rectangle PatrolArea(){if(patrolScreen==null)patrolScreen=Screen.FromPoint(new System.Drawing.Point((int)x+Width/2,(int)y+Height-15));foreach(var screen in Screen.AllScreens)if(screen.DeviceName==patrolScreen.DeviceName){patrolScreen=screen;return screen.WorkingArea;}patrolScreen=Screen.PrimaryScreen;return patrolScreen.WorkingArea;}
    double WrapMargin {get{return 90*scale;}}
    static double WrapCenter(double center,double left,double right,double margin){double start=left-margin,period=right-left+margin*2;if(center>right+margin||center<start)return start+((center-start)%period+period)%period;return center;}
    void WrapPatrol(Rectangle area){double center=x+Width/2,wrapped=WrapCenter(center,area.Left,area.Right,WrapMargin);if(Math.Abs(wrapped-center)<.001)return;x=wrapped-Width/2;tracks.Clear();hearts.Clear();speechTime=0;}
    bool AtPatrolEdge(Rectangle area){double center=x+Width/2;return center<area.Left+80*scale||center>area.Right-80*scale;}
}
