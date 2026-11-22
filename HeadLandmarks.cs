using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
partial class Usagi {
    sealed class FaceLandmark {internal float Left,Right;internal float Width {get{return Right-Left;}}internal float Center {get{return (Left+Right)/2;}}}
    static readonly Dictionary<string,FaceLandmark> faceLandmarks=ReadFaceLandmarks();
    static readonly Dictionary<Bitmap,FaceLandmark> frameLandmarks=new Dictionary<Bitmap,FaceLandmark>();
    static Dictionary<string,FaceLandmark> ReadFaceLandmarks(){var result=new Dictionary<string,FaceLandmark>();string file=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"characters","face-landmarks.tsv");if(!File.Exists(file))return result;foreach(string line in File.ReadAllLines(file)){if(line.StartsWith("#")||String.IsNullOrWhiteSpace(line))continue;var p=line.Split('\t');if(p.Length!=5)throw new InvalidDataException("Invalid face landmarks");float left=float.Parse(p[3],CultureInfo.InvariantCulture),right=float.Parse(p[4],CultureInfo.InvariantCulture);if(left<0||right<=left||right>256)throw new InvalidDataException("Invalid face endpoints");result.Add(p[0]+"/"+p[1]+"/"+p[2],new FaceLandmark{Left=left,Right=right});}return result;}
    static void RegisterFaceLandmark(string strip,int index,Bitmap frame){string key=new DirectoryInfo(Path.GetDirectoryName(strip)).Name+"/"+Path.GetFileNameWithoutExtension(strip)+"/"+index;FaceLandmark point;if(!faceLandmarks.TryGetValue(key,out point)||point.Right>frame.Width+1)throw new InvalidDataException("Missing or incompatible measured face endpoints: "+key);frameLandmarks.Add(frame,point);}
    static FaceLandmark MeasuredFace(Bitmap frame){FaceLandmark point;if(!frameLandmarks.TryGetValue(frame,out point))throw new InvalidDataException("Frame has no reviewed face endpoints");return point;}
}
