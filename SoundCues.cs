using System;
static class CharacterSoundCues {
    // A situation selects an actual reaction or original non-vocal foley; never a universal voice fallback.
    internal static string RelativePath(string character,string effect){
        string reaction=character+"-"+effect;
        if(reaction=="chiikawa-heart"||reaction=="chiikawa-eat"||reaction=="chiikawa-held"||reaction=="hachiware-hop"||reaction=="momonga-heart"||reaction=="momonga-held"||reaction=="kurimanju-sleep"||reaction=="rakko-trip"||reaction=="shisa-held")return "sounds/reactions/"+reaction+".wav";
        if(effect=="signature"){if(character=="kurimanju")return "sounds/characters/kurimanju-voice.wav";effect=character=="momonga"?"glide":character=="rakko"?"sword":character=="shisa"?"serve":"greet";}
        if(effect=="walk_ura")effect="step-left";else if(effect=="walk_purya")effect="step-right";
        switch(effect){case "hop":case "heart":case "burst":case "step-left":case "step-right":case "sleep":case "eat":case "trip":case "held":case "greet":case "glide":case "sip":case "sword":case "serve":return "sounds/actions/"+effect+".wav";default:return null;}
    }
}
