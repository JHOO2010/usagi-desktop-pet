using System;
static class CharacterSoundCues {
    internal static short[] Balance(string effect,short[] source){if(source.Length==0)return source;double peak=0;foreach(short value in source)peak=Math.Max(peak,Math.Abs((double)value));double energy=0;int active=0;foreach(short value in source)if(Math.Abs((double)value)>peak*.08){energy+=(double)value*value;active++;}double rms=Math.Sqrt(energy/Math.Max(1,active))/32768,normalizedPeak=peak/32768;bool step=effect.StartsWith("walk"),sleep=effect=="sleep";double target=step?.018:sleep?.018:effect=="burst"?.05:.055,ceiling=step?.08:sleep?.09:.28;double gain=Math.Min(1.5,Math.Min(target/Math.Max(.00001,rms),ceiling/Math.Max(.00001,normalizedPeak)));var result=new short[source.Length];for(int i=0;i<source.Length;i++)result[i]=(short)Math.Round(source[i]*gain);return result;}
    // A situation selects an actual reaction or original non-vocal foley; never a universal voice fallback.
    internal static string RelativePath(string character,string effect){
        string reaction=character+"-"+effect;
        if(reaction=="chiikawa-trip"||reaction=="rakko-burst"||reaction=="chiikawa-heart"||reaction=="chiikawa-eat"||reaction=="chiikawa-held"||reaction=="hachiware-hop"||reaction=="momonga-heart"||reaction=="momonga-held"||reaction=="kurimanju-sleep"||reaction=="rakko-trip"||reaction=="shisa-held")return "sounds/reactions/"+reaction+".wav";
        if(effect=="signature"){if(character=="kurimanju")return "sounds/characters/kurimanju-voice.wav";effect=character=="momonga"?"glide":character=="rakko"?"sword":character=="shisa"?"serve":"greet";}
        if(effect=="walk_ura")effect="step-left";else if(effect=="walk_purya")effect="step-right";
        switch(effect){case "bite":case "hop":case "heart":case "burst":case "step-left":case "step-right":case "sleep":case "eat":case "trip":case "held":case "greet":case "glide":case "sip":case "sword":case "serve":return "sounds/actions/"+effect+".wav";default:return null;}
    }
}
