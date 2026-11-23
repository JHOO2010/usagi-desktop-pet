using System;
using System.Collections.Generic;
using System.Drawing;
partial class Usagi {
    // Korean fan adaptations of the documented speaking styles, never verbatim episode scripts.
    // Sources and the distinction between a favorite and an app food choice: docs/CHARACTERS.md.
    static readonly Dictionary<string,string[]> characterLines=BuildCharacterLines();
    static Dictionary<string,string[]> BuildCharacterLines(){var map=new Dictionary<string,string[]>();foreach(string row in new[]{
        "chiikawa|idle|와아…|응…|에헤…",
        "chiikawa|walk|응!|와아!|으쌰…",
        "chiikawa|greet|아…!|응!|와아~",
        "chiikawa|pet|에헤헤…|와아…|헤헤…",
        "chiikawa|happy|와아아!|에헤헤!",
        "chiikawa|hop|얍!|와앗!",
        "chiikawa|curious|에…?|…응?",
        "chiikawa|surprise|앗…!|아와와…",
        "chiikawa|held|에엣…!|아…!",
        "chiikawa|food|와아…!|우와…!",
        "chiikawa|eating|함…|냠…",
        "chiikawa|full|에헤헤…!|와아~!",
        "chiikawa|sleep|후우…|…",
        "chiikawa|wake|으음…|응…?",
        "chiikawa|signature|아…!|에헤…",
        "hachiware|idle|오늘도 잘해 보자!|어떻게든 될 거야!|좋은 느낌이야!",
        "hachiware|walk|같이 걸을까?|이쪽도 가 보자!|조금만 더 가 보자!",
        "hachiware|greet|안녕! 잘 지냈어?|만나서 반가워!",
        "hachiware|pet|에헤헤, 고마워!|기분 좋다~",
        "hachiware|happy|이렇게 좋은 일이!|정말 기쁘다!",
        "hachiware|hop|한번 뛰어 볼까!|폴짝 가는 거야!",
        "hachiware|curious|그런 거라는 거?!|어라? 그게 뭐야?",
        "hachiware|surprise|우와앗!|깜짝 놀랐어!",
        "hachiware|held|어라? 올라간 거야?!|우와, 높다!",
        "hachiware|food|케첩 파스타다!|같이 먹으면 좋겠다!",
        "hachiware|eating|잘 먹을게!|따끈할 때 먹자!",
        "hachiware|full|맛있었어! 고마워!|배부르다~",
        "hachiware|sleep|조금 쉬었다 갈까…|잘 자…",
        "hachiware|wake|잘 잤다~|이제 다시 가 볼까?",
        "hachiware|signature|흥흥~♪|노래가 절로 나오네!",
        "momonga|idle|이 몸을 봐라!|귀엽다고 해!|더 칭찬해라!",
        "momonga|walk|따라와라!|비켜라~!|이 몸이 지나간다!",
        "momonga|greet|자, 이 몸을 봐라!|귀여운 내가 왔다!",
        "momonga|pet|더 쓰다듬어라!|그렇지, 더!",
        "momonga|happy|더 귀여워해라!|마음에 드는군!",
        "momonga|hop|잘 보고 있어라!|어떠냐!",
        "momonga|curious|이건 뭐냐!|어디 가는 거냐!",
        "momonga|surprise|뭐냐앗!|깜짝 놀랐잖냐!",
        "momonga|held|내려놔라!|놔라~!",
        "momonga|food|그 만두를 내놔라!|내 몫이다!",
        "momonga|eating|내가 먹겠다!|음~!",
        "momonga|full|하나 더 내놔라!|마음에 드는군!",
        "momonga|sleep|조용히 해라…|졸리다…",
        "momonga|wake|누가 깨운 거냐…|흐아암…",
        "momonga|signature|날 봐라~!|귀엽게 날아 주마!",
        "kurimanju|idle|후우…|…하아.|크으…",
        "kurimanju|walk|후…|음…",
        "kurimanju|greet|…음.|후우…",
        "kurimanju|pet|후우~|음~…",
        "kurimanju|happy|크으~!|하아~!",
        "kurimanju|hop|홋…!|흣!",
        "kurimanju|curious|…응?|음…?",
        "kurimanju|surprise|흣…!|…헛!",
        "kurimanju|held|으음…!|…흠!",
        "kurimanju|food|…오.|음…!",
        "kurimanju|eating|음…|…",
        "kurimanju|full|크으~|하아아~",
        "kurimanju|sleep|후우…|…",
        "kurimanju|wake|으음…|…후우.",
        "kurimanju|signature|하아아~|크으~",
        "rakko|idle|천천히 해도 좋다.|잘하고 있다.|준비는 됐나?",
        "rakko|walk|함께 가자.|주위를 살피도록.|좋은 걸음이다.",
        "rakko|greet|왔구나.|오늘도 힘내자.",
        "rakko|pet|고맙다.|…기분 좋군.",
        "rakko|happy|훌륭하다!|잘해 냈군!",
        "rakko|hop|자, 간다!|좋아!",
        "rakko|curious|무슨 일이지?|살펴볼까.",
        "rakko|surprise|엇!|…괜찮나?",
        "rakko|held|잠깐…!|조심해라.",
        "rakko|food|…달콤한 냄새군.|잠시 쉬어 갈까.",
        "rakko|eating|잘 먹겠다.|…음.",
        "rakko|full|…달군.|좋은 휴식이었다.",
        "rakko|sleep|잠시 쉬도록 하지.|푹 쉬어라…",
        "rakko|wake|충분히 쉬었나?|다시 출발하자.",
        "rakko|signature|기본부터 차근차근.|움직임을 잘 봐라.",
        "shisa|idle|오늘도 열심히 할게요!|공부도 해야 해요~|힘내 볼게요~",
        "shisa|walk|같이 가요~|차근차근 갈게요!|좋은 하루예요~",
        "shisa|greet|안녕하세요~!|어서 오세요~!",
        "shisa|pet|감사해요~!|기분이 좋아요~",
        "shisa|happy|정말 기뻐요~!|열심히 하길 잘했어요!",
        "shisa|hop|영차~!|한번 해 볼게요!",
        "shisa|curious|무슨 일인가요~?|그게 뭔가요?",
        "shisa|surprise|앗, 깜짝이야!|괜찮으세요~?",
        "shisa|held|조심해 주세요~!|와아, 높아요!",
        "shisa|food|사타판빈이에요~!|같이 드실래요~?",
        "shisa|eating|잘 먹겠습니다~!|냠냠~",
        "shisa|full|맛있었어요~!|감사히 잘 먹었어요!",
        "shisa|sleep|조금 쉬어 갈게요…|안녕히 주무세요~",
        "shisa|wake|잘 잤어요~|오늘도 힘낼게요!",
        "shisa|signature|잘 부탁드려요~!|스승님처럼 열심히!"
    }){var p=row.Split('|');var lines=new string[p.Length-2];Array.Copy(p,2,lines,0,lines.Length);map.Add(p[0]+"/"+p[1],lines);}return map;}
    string SpeechFor(string context,string usagiText){if(character.Key=="usagi")return usagiText;string[] lines;if(!characterLines.TryGetValue(character.Key+"/"+context,out lines))lines=character.Phrases;return Choose(lines);}
    void SayFor(string context,string usagiText){DisplaySpeech(SpeechFor(context,usagiText));}
    void AutoSayFor(string context,string usagiText){if(nextSpeechTime<=0)SayFor(context,usagiText);}
    string CharacterFoodKey {get{switch(character.Key){case "chiikawa":return "pudding";case "hachiware":return "pasta";case "momonga":return "meatbun";case "kurimanju":return "salmon";case "rakko":return "creampuff";case "shisa":return "satapanbin";default:return "pizzabun";}}}
    string CharacterFoodName {get{switch(CharacterFoodKey){case "pudding":return "푸딩";case "pasta":return "케첩 파스타";case "meatbun":return "고기만두";case "salmon":return "연어 육포";case "creampuff":return "슈크림";case "satapanbin":return "사타판빈";default:return "피자만두";}}}
    // Tiny runtime props follow the same food identity as each four-frame eating strip.
    static void DrawCharacterFood(Graphics g,string key,float x,float y,float size){if(key=="pizzabun"){DrawPizzaBun(g,x,y,size,1);return;}var saved=g.Save();g.TranslateTransform(x,y);g.ScaleTransform(size,size);using(var edge=new Pen(Color.FromArgb(101,67,42),1.25f))using(var cream=new SolidBrush(Color.FromArgb(255,249,225)))using(var gold=new SolidBrush(Color.FromArgb(230,170,76)))using(var brown=new SolidBrush(Color.FromArgb(156,81,43))){
        if(key=="pudding"){g.FillEllipse(cream,-17,-4,34,7);g.DrawEllipse(edge,-17,-4,34,7);var points=new[]{new PointF(-9,-19),new PointF(9,-19),new PointF(12,-2),new PointF(-12,-2)};g.FillPolygon(gold,points);g.DrawPolygon(edge,points);g.FillEllipse(brown,-9,-21,18,5);g.DrawEllipse(edge,-9,-21,18,5);}
        else if(key=="pasta"){g.FillPie(cream,-18,-15,36,21,0,180);g.DrawArc(edge,-18,-15,36,21,0,180);g.FillEllipse(cream,-18,-17,36,10);using(var noodles=new Pen(Color.FromArgb(213,87,43),2.3f)){for(int i=0;i<4;i++)g.DrawBezier(noodles,-13,-13+i*1.4f,-8,-21+i*2,7,-6-i,13,-14+i); }g.DrawEllipse(edge,-18,-17,36,10);}
        else if(key=="salmon"){var p=new[]{new PointF(-16,-6),new PointF(-12,-13),new PointF(15,-10),new PointF(17,-3)};g.FillPolygon(brown,p);g.DrawPolygon(edge,p);using(var grain=new Pen(Color.FromArgb(235,153,100),1)){for(int i=-9;i<14;i+=5)g.DrawLine(grain,i,-11,i+2,-5);}}
        else if(key=="meatbun"){g.FillEllipse(cream,-14,-21,28,24);g.DrawEllipse(edge,-14,-21,28,24);g.DrawLine(edge,-7,-16,0,-20);g.DrawLine(edge,7,-16,0,-20);g.DrawLine(edge,0,-20,0,-14);}
        else if(key=="satapanbin"){g.FillEllipse(brown,-13,-23,26,26);g.DrawEllipse(edge,-13,-23,26,26);using(var purple=new SolidBrush(Color.FromArgb(157,103,162)))g.FillEllipse(purple,-5,-16,15,11);g.DrawArc(edge,-5,-16,15,11,15,220);}
        else {g.FillEllipse(gold,-15,-23,30,26);g.DrawEllipse(edge,-15,-23,30,26);g.FillEllipse(cream,-13,-12,26,8);g.DrawArc(edge,-13,-12,26,8,5,170);g.DrawArc(edge,-8,-20,12,8,180,150);}
    }g.Restore(saved);}
}
