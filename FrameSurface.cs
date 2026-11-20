using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
sealed class LayeredSurface : IDisposable {
    [StructLayout(LayoutKind.Sequential)]struct XY {public int X,Y;public XY(int x,int y){X=x;Y=y;}}
    [StructLayout(LayoutKind.Sequential,Pack=1)]struct Blend {public byte Op,Flags,Alpha,Format;}
    [StructLayout(LayoutKind.Sequential)]struct BitmapInfo {public uint Size;public int Width,Height;public ushort Planes,Bits;public uint Compression,ImageSize;public int Xppm,Yppm;public uint Used,Important,Color;}
    [DllImport("user32.dll")]static extern IntPtr GetDC(IntPtr window);
    [DllImport("user32.dll")]static extern int ReleaseDC(IntPtr window,IntPtr dc);
    [DllImport("gdi32.dll")]static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")]static extern IntPtr CreateDIBSection(IntPtr dc,ref BitmapInfo info,uint usage,out IntPtr bits,IntPtr section,uint offset);
    [DllImport("gdi32.dll")]static extern IntPtr SelectObject(IntPtr dc,IntPtr obj);
    [DllImport("gdi32.dll")]static extern bool DeleteObject(IntPtr obj);
    [DllImport("gdi32.dll")]static extern bool DeleteDC(IntPtr dc);
    [DllImport("user32.dll")]static extern bool UpdateLayeredWindow(IntPtr window,IntPtr screen,ref XY position,ref XY size,IntPtr source,ref XY origin,int key,ref Blend blend,int flags);
    internal bool LastPresentSucceeded {get;private set;}
    IntPtr memory,bitmap,previous;Bitmap frame;Size size;
    internal void Draw(IntPtr window,Point position,Size requested,Action<Bitmap> paint){IntPtr screen=GetDC(IntPtr.Zero);try{if(frame==null||size!=requested){Dispose();size=requested;memory=CreateCompatibleDC(screen);var info=new BitmapInfo{Size=40,Width=size.Width,Height=-size.Height,Planes=1,Bits=32};IntPtr pixels;bitmap=CreateDIBSection(screen,ref info,0,out pixels,IntPtr.Zero,0);if(memory==IntPtr.Zero||bitmap==IntPtr.Zero||pixels==IntPtr.Zero){Dispose();throw new InvalidOperationException("Could not create animation surface.");}previous=SelectObject(memory,bitmap);frame=new Bitmap(size.Width,size.Height,size.Width*4,PixelFormat.Format32bppPArgb,pixels);}
            paint(frame);var point=new XY(position.X,position.Y);var dimensions=new XY(size.Width,size.Height);var origin=new XY(0,0);var blend=new Blend{Alpha=255,Format=1};LastPresentSucceeded=UpdateLayeredWindow(window,screen,ref point,ref dimensions,memory,ref origin,0,ref blend,2);
        }finally{ReleaseDC(IntPtr.Zero,screen);}}
    public void Dispose(){if(frame!=null){frame.Dispose();frame=null;}if(memory!=IntPtr.Zero&&previous!=IntPtr.Zero)SelectObject(memory,previous);if(bitmap!=IntPtr.Zero)DeleteObject(bitmap);if(memory!=IntPtr.Zero)DeleteDC(memory);memory=bitmap=previous=IntPtr.Zero;}
}
