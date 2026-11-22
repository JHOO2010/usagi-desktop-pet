using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
partial class Usagi {
    sealed class BodyGeometry {internal Rectangle Bounds;internal float FaceWidth,Center,CoreHeight;}
    static BodyGeometry MeasureBody(Bitmap image,bool warm){int width=image.Width,height=image.Height,total=width*height;var mask=new bool[total];var bits=image.LockBits(new Rectangle(0,0,width,height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
        try{var row=new byte[width*4];for(int y=0;y<height;y++){System.Runtime.InteropServices.Marshal.Copy(IntPtr.Add(bits.Scan0,y*bits.Stride),row,0,row.Length);for(int x=0;x<width;x++){int p=x*4;mask[y*width+x]=row[p+3]>(warm?180:40)&&(!warm||row[p+2]>=218&&row[p+1]>=192&&row[p]>=138&&row[p+2]>=row[p]+3);}}}finally{image.UnlockBits(bits);}
        var queue=new int[total];int[] largest=null;var groups=new List<KeyValuePair<int,Rectangle>>();int best=0;
        for(int start=0;start<total;start++){if(!mask[start])continue;int take=0,end=1;queue[0]=start;mask[start]=false;int l=width,r=0,t=height,b=0;
            while(take<end){int pixel=queue[take++],x=pixel%width,y=pixel/width;l=Math.Min(l,x);r=Math.Max(r,x);t=Math.Min(t,y);b=Math.Max(b,y);
                if(x>0&&mask[pixel-1]){mask[pixel-1]=false;queue[end++]=pixel-1;}if(x+1<width&&mask[pixel+1]){mask[pixel+1]=false;queue[end++]=pixel+1;}if(y>0&&mask[pixel-width]){mask[pixel-width]=false;queue[end++]=pixel-width;}if(y+1<height&&mask[pixel+width]){mask[pixel+width]=false;queue[end++]=pixel+width;}}
            groups.Add(new KeyValuePair<int,Rectangle>(end,Rectangle.FromLTRB(l,t,r+1,b+1)));if(end>best){best=end;largest=new int[end];Array.Copy(queue,largest,end);}
        }
        if(largest==null)return new BodyGeometry{Bounds=new Rectangle(0,0,width,height),FaceWidth=width,Center=width/2f,CoreHeight=height};
        Rectangle bounds=Rectangle.Empty;foreach(var group in groups)if(group.Key>=best*.08)bounds=bounds.IsEmpty?group.Value:Rectangle.Union(bounds,group.Value);
        var counts=new int[height];var left=new int[height];var right=new int[height];for(int y=0;y<height;y++)left[y]=width;
        foreach(int pixel in largest){int y=pixel/width,x=pixel%width;counts[y]++;left[y]=Math.Min(left[y],x);right[y]=Math.Max(right[y],x);}
        int max=0,faceRow=0;for(int y=0;y<height;y++)if(counts[y]>max){max=counts[y];faceRow=y;}
        int coreTop=faceRow;for(int y=0;y<height;y++)if(counts[y]>=max*.65){coreTop=y;break;}
        return new BodyGeometry{Bounds=bounds,FaceWidth=right[faceRow]-left[faceRow]+1,Center=(right[faceRow]+left[faceRow]+1)/2f,CoreHeight=height-coreTop};
    }
    static Bitmap CropCharacterCell(Bitmap cell){var body=MeasureBody(cell,false);return cell.Clone(body.Bounds,PixelFormat.Format32bppArgb);}
    static float StandingBodyHeight(Bitmap image,BodyGeometry face,bool omitBlueTail){var tops=new List<int>();int half=Math.Max(2,(int)(face.FaceWidth*.1)),center=(int)face.Center;var bits=image.LockBits(new Rectangle(0,0,image.Width,image.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);try{var row=new byte[image.Width*4];var found=new bool[image.Width];for(int y=0;y<image.Height;y++){System.Runtime.InteropServices.Marshal.Copy(IntPtr.Add(bits.Scan0,y*bits.Stride),row,0,row.Length);for(int x=Math.Max(0,center-half);x<Math.Min(image.Width,center+half);x++){int p=x*4;if(!found[x]&&row[p+3]>180&&(!omitBlueTail||row[p+2]>=220&&row[p+1]>=195)){found[x]=true;tops.Add(y);}}}}finally{image.UnlockBits(bits);}if(tops.Count==0)return face.CoreHeight;tops.Sort();return image.Height-tops[tops.Count/2];}
    float PoseLeft(Bitmap pose,float renderedWidth){float center;return characterArt.Centers.TryGetValue(pose,out center)?-center*renderedWidth/pose.Width:-renderedWidth/2;}
    void DrawAlignedPose(Graphics g,Bitmap pose,float width,float height){g.DrawImage(pose,PoseLeft(pose,width),-height,width,height);}
}
