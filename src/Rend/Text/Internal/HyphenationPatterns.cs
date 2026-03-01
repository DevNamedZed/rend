namespace Rend.Text.Internal
{
    /// <summary>
    /// Provides embedded hyphenation patterns for supported languages.
    /// Patterns are derived from the standard TeX hyph-en-us hyphenation dictionary
    /// (Knuth-Liang algorithm for US English).
    /// </summary>
    internal static class HyphenationPatterns
    {
        /// <summary>
        /// Returns the English (en-US) hyphenation patterns in TeX pattern format.
        /// These patterns are a subset of the standard hyph-en-us.tex patterns
        /// covering common letter combinations for US English hyphenation.
        /// </summary>
        internal static string GetEnglishPatterns()
        {
            return EnglishPatterns;
        }

        // Standard TeX en-US hyphenation patterns (Knuth-Liang).
        // Format: interleaved letters and digits. Dots represent word boundaries.
        // Each line contains one pattern. Digits indicate hyphenation weight at that position.
        // Odd weight = allow hyphen, even weight = forbid.
        private const string EnglishPatterns =
            // Word-boundary patterns (. = boundary)
            ".ach4 .ad4der .af1t .al3t .am5at .an5c .ang4 .ani5m .ant4 " +
            ".an3te .anti5s .ar5s .ar4tie .ar4ty .as3c .as1p .as1s .aster5 " +
            ".atom5 .au1d .av4i .aw4 .ba4g .bal5a .ban5dag .bar4 .bas4e " +
            ".ber4 .be5ra .bi4b .bi4d .bil5in .bi2n .bi5na .bi3o .bi2t " +
            ".bl4 .bond4 .bri2 .bu4n .bus5i .but4ti .cam5pe .can5c .capi4 " +
            ".car5ol .ca4t .ce4la .ch4 .chill5i .ci2 .cit5r .co3e .co4r " +
            ".cor5ner .de4moi .de ## .des4c .dic1t .do4t .du4c .dumb5 " +
            ".earth5 .eas3i .eb4 .eer4 .eg2 .el5d .el3em .enam3 .en3g " +
            ".en3s .eq5ui5t .er4rs .es3 .eu3 .eye5 .fev4 .fix5 " +
            ".fl4 .fo4r .for5ay .fro4g .fu5el .galax5 .ge2 .gen3t4 " +
            ".ge5og .gi5a .gi4b .go4r .hand5i .ha4p .har2d .he2 " +
            ".hero5i .hgi5 .hi3b .hi3er .hon5ey .hon3o .hov5 .hy3pe " +
            ".hy3po .hy2s .he4r5et .hero5in .hys1 " +
            ".ib2 .icon3o .iden5t .ig3 .ig1ni .il2l .im5b4 .im2ma " +
            ".im3p .in1 .in2a .in4d .in3e .in5g .in3o .in5s .ir5r " +
            ".is4i .ju3r .la4cy .la4m .lat5er .lath5 .le2 .leg5e " +
            ".len4 .lev1 .li4g .lig5a .li2n .li3o .li4t .mag5a5 " +
            ".mal5o .man5a .mar5ti .math3 .me2 .mer3c .me5ter .mis1 " +
            ".mit4 .mo3d .mo3l .mon3e .mo3ro .mu5ta .mys1 " +
            ".na4p .ne2 .ni4c .nit3 .no4g .non1e .nu5t .od2 " +
            ".odd5 .of5te .or5ato .or3b .or3d .or3e .or5g .or3i " +
            ".os3 .os4tl .oth3 .out3 .ov4er .pa4ce .pa4i .pal4i " +
            ".pa5ta .pen5ta .pe5. .perm4a .per1s .pi4e .pio5n .pi2t " +
            ".pre1 .pref5ac .pro3l .pros3e .pro1t .pu2n .quad5 .qu4 " +
            ".ra4c .rai4 .rav3e .re1c .re1f .re3l .re1m .re5mat " +
            ".rep5ti .re1s .re5sta .ri2 .rib1 .ri4g .ri3ta .ro4q " +
            ".ros5t .row5d .ru4d .sa2 .se4a .se2n .se5. .ser4 " +
            ".sh2 .si2 .sim3 .sl4 .so4 .sol5d .so3li .so2n " +
            ".sp4 .st4 .sta5bl .sy2 .ta4 .te4 .ten5an .th2 " +
            ".ti2 .til4 .tim5o5 .tion4 .tri5o .tu4 .un1a .un3c " +
            ".un3d .un3e .un5k .un3l .un5o .un3s .un3u .up3 " +
            ".ur4b .us5a2 .ven4de .ve5. .vi4a .vi5gn .vi2l .vi3so " +
            ".vo4la .vor5 .wa4ter .we2b .whi4 .wi2 .wil5i .ye4 " +

            // Internal patterns (no dots)
            "ab1ic ab3ol ab5erd ab3la ab1s ac5et " +
            "ac1in ac5ro act5if ac3ul ad4din ae4r af4ti ag1i ag5in " +
            "a2go ag3on a5gu ah4l ai2 a5ia ai5ly ais4 a4i4t " +
            "al5ab al3ad a4lar al1de al3do ale4 al3end a4lenti al1i " +
            "al4ia ali4e al5lev al1o a5lo. al4oe als4 al1t al3ua " +
            "a5ltic am5ab am3ag ama5ra am3ic am1in am5ity am5o " +
            "am5pl an3ag an5da an1dl an3dr an1el an3en an5est an5et " +
            "an3ge an1i a5nim an3io a3nip an3ish an3it a3niu an4kli " +
            "an1ne an5ot an4oth an2sa an4sco an4sn an2sp ant2i an4tic " +
            "an4tip an3ul an5y a5pe ap5at api4 a3pie a5pl " +
            "ap1li a5pos ap5osi a4pre a3pu aq5ue ar3act ar5adis ar5ativ " +
            "a5rea ar3ent ar5et ar5ia ar4ib ar5id ar3in ar5is ar4iz " +
            "ar3ne ar5ol ar3om ar5os ar1ou ar5rang ar2s ar4sh ar5ta " +
            "ar5te ar4thi ar4ti ar4ty a5ry as4ab as5ic as1in as4is " +
            "as5ph as5si as3te as1tr at5ac at5alo a5tamphl at1ed " +
            "ath3e ath5em a5then at4ho ati5b ation5ar at3itu a4tog " +
            "at5om at5omi at5op at5ta at5te at4th at5ua at5ue au4b " +
            "aug4 au3gu aun5d au3r au5si aut5en av3ag av5ern av5ery " +
            "av1i avi4er aw3i aw4ly aws4 ax5il ax4im ax3is ay5al " +
            "ays4 az5ar " +

            "ba2 ba4ge bal1a ban5dag ban4e ban3i bar4i bas4i " +
            "ba4z 2be be3di be3gi be5la bel5li be3lo be3m be5nig " +
            "be5nu be3ra be3sm be5str bi4d bi3en bi4er bi2l " +
            "bi3li bi3liz bin4d bi5net bi1no bi5og bi3or bi2t " +
            "bi3tro bl2 blath5 ble. blen4 blin4d blis4 blo4 " +
            "blu4 bo2 bod3i bol3i bon4a bon5at bo4ne boo4 " +
            "bor5d bos4 bot3a bound3 br4 bram4 bri2 bri4er " +
            "brow4 bu4n bur4n bus5i bust4 but4ti " +

            "cab3i cal4la cam5pe can5c can4e can4ic can5is " +
            "can4ty cap3a car5om cas5tig cat4a ca4th cau5 ce4la " +
            "3cell cer4n ch4 cham5o char5i che2 4chea chem3i " +
            "ch5ene ch3er4 ch5eri ch5ern chi2 chi5me chin4 chi4p " +
            "chlo3 cho2 ch4ti ci2 ci4a cid4 ci3en cin4 " +
            "ci3ou ci2p cir4 ci4t 5cit ci3tr 5ciz ck1 " +
            "ck3i 5clar cle4m 4cly co5ag coi4 co3in col5i " +
            "5colo col3or com5er con4a co3no con3s con1t co3pa " +
            "cor5ner cos4e coun3t cou1r co4wr cri2 cri3t cro4p " +
            "cru4d 4cry cu5la cu2m cun4 cu3pi cur5a4b cu5ri cus1 " +
            "cuss4 3cut cu4ti " +

            "4dag da2m dan3g dav4 ddi4 2de de4bi de4bon " +
            "de1c de5cal def5i 2d1el del5i de4mo5 de3moi de1n de3no " +
            "de3nu de1p de3pa de3pi der5s des2 de1s4c de3si de1sp " +
            "de3st de3su det5er de1t de1v dev3il 4dey di1a " +
            "di4at di4bl dic1t di4er dif5 di3ge di3la dim3 " +
            "di1mi di4mo di4ni dio5g di1or di3ph dir2 di1re " +
            "dis1 5disi di3so dis3t di4ta di4u 5div di1v di4va " +
            "d1j d5k 4d1l 2d1m 4d1n do4e 5doe dol3i don4at " +
            "do3nat 5doni doo3d dor4 dors4 do4t 4d1p 2d1r " +
            "drea4 dri4b dril4 dro4p du4al du4c 1du1e 1dul " +
            "dun4 du4pe du3pl du4r duz4 " +

            "ea2 ea4g eal3ou e4am ear4c ear5k ear4t eas3i " +
            "ea5sp eat5en e4ath eav5en eav3i 2e1b e4bel ebi4 " +
            "eb4it e4bl e2bo e4bon e4bra e4cal ec3at ec1c ecip4 " +
            "ec4li e1cr ec4ta ec4te e1cu e4cul ed5d e4d1er " +
            "edi4 ed3im ed1it ed4it. 2ee ee4c ee2d ee4l ee2m " +
            "ee4n ee4p ee2s ee4st e5ex ef5i e3fl e4flu e4fly " +
            "eft4 3egal e5gar eg5ing e5git eg4n e4go e4gra " +
            "ego3i ei2 ei5gl eig4n e3im ein4d ein4g e5inst " +
            "el5ate e1le el3eg e4lem el3ev e3lia e4lib el5ig " +
            "e5lim el3in e5lio e2lis el4la el4le el3lo el5og " +
            "el3op el2s el4t e5lud em5ana em5b em5ero em3i " +
            "em5igr em5in em5ine em3ol em5ou emp5i em5pl emp4t " +
            "e4mul en5ab en3ac en5age en1al en5amo en4d en5dag " +
            "en3dic en5do en5dro en3du en4dur en1e en5ero eng4 " +
            "en3ig e3nim e5nis en3iti en3iz 5enn en5oc en5om " +
            "en1ou en3ov en4sw en4ta en5ta. en3th en4tr en4tw " +
            "en5ure en3ut e5ny e1o4 e4ob e4od e4og " +
            "e4oi eo3l eo2n eo4rl eo4to e4out e4ov e5ow " +
            "e2pa e3pai ep5anc e5pel e3pen e5per ep3i epi5d " +
            "epi3l e4pim ep5ise ep3t ep5uta eq5ui3l " +
            "e4q er1a er4and er3ar er3at er3b er4bl er4by " +
            "er1c er4ch er4cl er3d er5diz ere4 er5ea er5eb " +
            "er3ect er3ed er5ego er3emo er3ena er5ence er3ent er5eo " +
            "er3e4s er5est er5et er3ev er3ic er5id er3in er3io " +
            "er3is er3it er3iz er4mi er4nis er5o er3ob er3oc " +
            "er3od er3oi er3os er3ot er3ou er1s er4sh er4si " +
            "er1t er4ti er4to er3tw er5ul er5um er3un er5up " +
            "er3ur er5us es4al es5can e4scr es4cu e5sec es5el " +
            "es3en es5ert e4ses es5ig es3im es5in es4i4t es5iv " +
            "es4la es4mi es3ol es3on es4ot es5pi es2s es4si " +
            "es4ta es5tan es5tat es4te es5ti es4to et3al et5aph " +
            "et3ate et3eas e5tei e4teo e4tern et5ern et3i eti4n " +
            "e4tit et3iz e4to et3om et1r et5ri et3ua et5ude " +
            "et5ul et5ur e4tw eu3 eu4cl eu4g eu3lo eu3ph " +
            "eu3ro eu3th eur4 e5us ev5ast ev5er ev3id ev5il " +
            "ev5in ev5oc e5vot ew3er ew5ing e4wl ey4n " +

            "fa3bl fab3r fa4ce 4fag fain4 fall5e 4fa4ma fam5il " +
            "fan4cy far5i fas3c fas4t fat5al fa3th fau4lt 4fav " +
            "fav5or feas4 feath3 fe4b 4feca 5. 3fer fer1 " +
            "fer5v fev4 5. 4fic fi5del fi3di fig5 fil5i " +
            "fill5in fin2d 5. fi4ni fin5n fir4m 5. fis4ti " +
            "fi3su 5. fix5 fl2 fla3g fla2m flan5 " +
            "fle4 flin4 flo4 flo5ri flu4 fly5 fo4c fo5cal " +
            "fo1li fon4dl fon4t for5ay for5b for3d for3e for5i " +
            "for4m fos5 4fre free5 fri4 fro4g fru4 fru5it " +
            "fu5el 4ful ful5l fun5d fur5n fus4i fu5til " +

            "ga2 gai4 gal5a gal4i ga3lo gam4b gar5n gas5i " +
            "gath3 ge2 ge4at gel4i ge5lis gen3e ge5ni gen4t " +
            "ge5og ge4r ge3ro gi4a gi4b gid4 gi3g gil4 " +
            "gi3li gin5g gi3o gi4p gir4l gis4 gi3u gl2 " +
            "gla4 gli4 glo3r glu4 gly3 go4r gor5ou gr4 " +
            "gra4n gran5d gra5v gre4n gri4 grit5 gro4 gru4 " +
            "gu4a guan5 gue5 gui5t gun5 gur4 gust5o gu4t " +
            "gya4 " +

            "ha2 hab3it hag4 hal4e ham5 han4d han5k hap3 " +
            "ha4p5t har3d har4le har5p has5t hav5 hay5 he2 " +
            "hear4t heas4 hec4t heli3 hel4l hel4p hem5a hem5i " +
            "hen5a hen5d her3b herd4 heri4 hes5i hes5p het4 " +
            "heu4 hev4 hex5 hi3b hi4c hid4 hi3er hi4l " +
            "hin4 hip4 hir4 his4 hith5 hiv4 ho4g hoi4 " +
            "ho4l ho5ly hom5 hon4a hoo4 hori5z hor5n hos4 " +
            "hot5 hov5 how5 hum4 hun4 hus3t hy3pe hy3po " +
            "hy2s " +

            "ia4 iam4 iam5e ian4 iat4 i4ato ib5er ib3in " +
            "ib5it ib4l i1bo ic3a ic4at ic1c i4ce i5ci " +
            "ic5in ic3ip ic3it ick3i ic4li ic5ol ic5on i1cr " +
            "ic5ro ic4te ic1u ic5ul id5an i5dat id3d id5er " +
            "ide5s id3ge i5di id5ia id3in id5io id5it idi5u " +
            "i3dl id5ol i4dom id3ow i4dr id5uo i5dy ie4 " +
            "i5ea i5el i3en i5er i3est i5et i5eu if4f " +
            "if5i i3fl ig3a ig5el ig3i ig3il ig5in ig3it " +
            "ig3or i2go ig5ur ik4 i5la il4ag il3an il4ax " +
            "il2d il4de il3er il3ev il5ib il3in il3it il1l " +
            "il5og il3ou il4ty il5ur i4mag im3ag im5agi im4al " +
            "im3an im5b im5ida im3in im3it im4ni i2mo im4pe " +
            "im5pin im5po i2n in3ab in3an in3ap in4ars in4at " +
            "in5ativ 4ind in5d in1de in4do in3du in3ea in1e " +
            "i5nee in3er in3est in5et in1g in3ge in5gen in3gl " +
            "in3go in5gum in1i in3io in1is in3it in3itu ink4 " +
            "in3m in1n in5oc in5ol in1ou in5ow in4q in4s " +
            "in3se in5si in4so in3sp ins4t in4sw in4ta in5teg " +
            "in5ter in5tes in4th in1u in3un in3ur in3ut " +

            "io4 iod3 ion5at ion4i io5ph i3or ios4 i3ot " +
            "i5ous ip5i ip3li ip4re ip3ul ir4a ir5at ire4 " +
            "ir5ec ir5em ir5ig ir4is ir5it ir5on i1sa i3sc " +
            "is3ch is5co is4c2r is4el is5en ish5op is3ib is5il " +
            "is5in i3sis is3iv is4ke is4li is5lo is3m is5on " +
            "iso5p is5ot is4pa is5ph is3po is4pr is5py is4sa " +
            "is5se is4si is4so is4su is4ta is3te is5tic is5til " +
            "is1to is4tr i2su i5su. is5ul is5ur is5us " +
            "it5ab it3al it3an it5ant it3at ite4 it5en it5er " +
            "it3i it5ic i5tig it3ig. it3il it3in it3io it5is " +
            "it3ol i4tor it5ou it5ri it4ta it4te it5ti it1u " +
            "it5ul it5ur it3us iv5ar iv3el iv3en iv5er iv5il " +
            "iv3in iv5io iv3it iv5ol iv5or iv5ou i5vor ix4o " +
            "iz5ar iz3en iz5i i3zon iz5on. " +

            "ja4p 1je jer5s jew3 ji4b jin4 jo4 joi4 " +
            "jou4 joy5 ju3d ju5l jun4 jur4 jus4 ju5v " +

            "ka4 kal4 kan4 ke2 ke4g kel4 ke4m ken4 " +
            "ker4 kes4 kev4 key5 ki4 kin4 ki4p kis4 " +
            "ki4t kl4 kn4 ko4 kor4 " +

            "la4b la4c la4d la5dy la4g la4m lan4d lan5dl " +
            "lan4k lar4g la5tan lat5er la4th lau4 lav5i law5 " +
            "lay5er le2 le4bi le4c led4 le4g le4m " +
            "lem5at len4d le3ph le4pr ler4 les2 le5squ les4t " +
            "let4 lev1 lev4er li4ag li4am li4as li4at li2b " +
            "li3bi li4bl lib5r lic4 li4cor li5cou li4cr li4cy " +
            "lid5 li3en li4er li4et li5eu li4f li4g li5ga " +
            "ligh4 li4gr li4gu li4ka li4ke lil4 li4m li3mat " +
            "lim5in li4mo lim4p li4n lin5ea lin5g lin5i li5og " +
            "li4os li4p li5q lis4 lis5e li5so lis5t li4ta " +
            "li4te lith5i lit5ic li5tig lit4r li4tu liv5 l4iv " +
            "liz5ar ll4 llo4 ll5ow l4ly l2m l5m4 lo4ci " +
            "lo4g lo3gy lo4mo lon4e lon4g lon5i lo4p lo5pe " +
            "lop5i lo5rie lor5ou lo4se los5in los5t lo4ta lot4t " +
            "lou5 lo4ut lov5 low5li lu4b lu3br lu4c lu3ci " +
            "lu3cu lu4d lu5en lu4i lu3mi lun4 lu3o lur4 " +
            "lus4 lu5so lust5i lus3tr lu4t lu5te lu5ti " +

            "ma2 mac4 mag5a5 5. mal5o man5a man4i man3iz " +
            "map5 mar5ti mas4e mas1t mat4 math3 ma5thi mau5 " +
            "mav5 me2 me4b med3i me4g mel5o mem5o men5ac " +
            "men5d men5ta mer3c me5ter me4th met3r mi3a mi4c " +
            "mi4d mid5a mil4 min4d min3u mi3o mir4 mis1 " +
            "mit4 mi4t5i mi4ty ml4 m1m mo2 mo4b moi4 " +
            "mo3l mol5i mo3ly mo4m mon5et mon3g mon3i mon4is " +
            "mon5it mon5k mo3no mo4no. mon4st mor5al mor4d mor5if " +
            "mo5ro mor5on mo5sey mo4ss moth3 mo5tiv moun5 mou4r " +
            "mous5 mo2v mu4 mul5ti mun4 mur4 mus4 mu4t " +
            "mu3ta mya4 mys1 " +

            "na2 na4b na4g na4li na5ly nam4 nan4 nap5 " +
            "nar5c nas5ti na4t na5tio nat5iv na5tur nau3 nav4 " +
            "nay4 ne2 ne4a neb3u ne4c ned4 ne4g ne5li nem5 " +
            "ne4mo nen4 ne5on ne4p ner4 nes4 net5tl ne4t " +
            "ne4v new5 ni4a ni4bl ni4c ni4d ni5di ni4er " +
            "ni5fi ni4g ni3gr nil4 ni4m ni4n nin4g ni4o " +
            "ni3ou ni4p ni3ti ni4t nit4r ni3tu niv4 niv5el " +
            "ni3ver ni5vo no2 no4b nod3 no4g noi4 no5l " +
            "nom3i no5mi. no4mo non1e non5i no4n4s noo4 " +
            "nor5ab nor4d nor4e nor5m nos4 no5ta not5at no4th " +
            "nou4 nov3 now4 nu1 nu4d nu5en nu4l nu4m " +
            "num5i nup5 nur4 nus4 nu3tr " +

            "oa4 oaf5 oak5 oar4 oast5 oat5 ob2 o5bar " +
            "ob3ing ob3it ob5l obo4 ob3ul ob5ur oc4 o4cal " +
            "oc3at oc5cu o4cel och4 o5chu ock5 o4cli oc3ra " +
            "oc3ul od4 od5al od5d od3ic od5il od3ol od5ou " +
            "od5uc o5duc oe4 o5eg of4 of5te og4 o4gal " +
            "ogl4 og5li o4go og3ul o2gy oh4 oi2 oi4c " +
            "oi4l oin4 oi4r ois4 o4ism o2it oi4t5u ok4 " +
            "ok5ie o1la ol4a ol3an old5e ol3er o3les ol3ev " +
            "ol3id ol5in ol3it ol4ith ol5iv ol4l ol5og ol3on " +
            "ol3or ol5ou ol1s ol4t ol3um ol3un oly5 om4 " +
            "o4ma om5ah om5at om4b om5eb om3en om5er om1i " +
            "om5ic om3in om5iny o4mis om4m om5on om3pi on4a " +
            "on5ast on3at on1c on5do on1e o5nee on3er o3net " +
            "on3ey on5g on1i on3im on5is on3it on3iv onk4 " +
            "on5odi on3om on5omy ono4 on5s on5ti on1u on5ur " +
            "oo2 ood5 oo4k oop4 oo4se oo4t op1 o5pa " +
            "op3al op5er op1i op3in op5is op1l op3ol op5on " +
            "op5ov op1u o5qu or4a or5ab or5age or5al or5and " +
            "or5ang or5at or3b or1c or3ch or4d or5de or3di " +
            "or3do or1e ore4a or5eo or5et or5gan or3ge or4gli " +
            "or3go or3gu or1i or5id or3if or3ig or4il or3in " +
            "or3io or3is or5it or3iz orl4 or3lo or4m or5mi " +
            "or4n or5ne or3o or5os or3ou or4pe or5ph or3pi " +
            "or1r or4se or4st or3th or5un or5ur os4 os3al " +
            "os3et osi4 os5iti os3ol os5on os4s os4t os5til " +
            "os5tit ost3r o5su ot3ag ot5al o5tan ot3er oth5 " +
            "ot3ic ot5if ot3in ot5iv o4tl ot5ol ot5om o4ton " +
            "ot3or ots4 ot5te ou2 ouch5 oun4d oun5t our5i " +
            "ous5 out3i out5r ov4 ov5ar ov4en over3 ov5eri " +
            "o3ver1s ow3d ow1e ow4l own5i ow4s ox3i ox5o " +
            "oys4 " +

            "pa4ce pa4ci pac5if pa4d pa4g pa3gi pa4in " +
            "pan5d pan5el pan3i pan4ty pa3pe pap5i par5af par5di " +
            "par5el par5o par4t4i pas4 pas5si pa5ta pat5r pau4 " +
            "pav5 paw4 pay5 pe2 pe4a pear4 pe4b pe4d " +
            "ped5al pe3de ped3i pe4el pe4g pe3go pe4la " +
            "pel5le pe4m pen5an pe4n pen4th pe5on pe4p per5an " +
            "per1c per3e per5f per4i per4m per1n per3o per3s " +
            "per1t per1v pe5ru per3u pes4 pe3te pet5it pe4ti " +
            "pe5tra pe4tr pe4u ph4 phar5 phe4 phi4 phon4 " +
            "phor4 phos4 pho4t phr4 phu4 phy3 pi4a pi4c " +
            "pi4d pie4 pi4g pi3la pil4l pi4m pi4n pin5g " +
            "pin5n pi3o pi4os pi4p pi4t pi3tu pix4 pl4 " +
            "pla4 plas5t ple4 plen5 pli5 plo4 plu4 ply5 " +
            "po4c po4d po4e po5em poi4 po4l pol5i pol4l " +
            "po4m pon5d po5ni po4no poo4 po4p por5i por5o " +
            "pos1 pos5it po4ta po4te pot5o poul4 pour5 pow5d " +
            "pow5er pp4 ppl4 prac3 pre1 pre5c pre5d pre5l " +
            "pre5m pre5r pre5s pre5t pri4 pri5m prin4t pro3b " +
            "pro5c pro3l pro4m pron4 pro1r pros3e pro1t " +
            "prow4 pr4s pru4 pry4 ps4 pu2 pu4b pu4c " +
            "pud4 pug4 pul5c pul4l pul5m pul5v pum4 pun4 " +
            "pur4 pus4 pu4t pu3ta put4te py3 " +

            "qu4 qua5v que5 qui4 " +

            "ra2 ra4c rai4 ra4l ram5i ram4p ran4d ran5g " +
            "ran4t rap5 rar4 ras4 rat5al ra4th rav5el raw5 " +
            "ray5 re1a re3ac re4al re5an re2b re3bu rec5ol " +
            "rec5om re4cr re1cu re4del red5i re3di re4do re4d5r " +
            "re3du re1e re4el re3en re4er ref5er re1f re4fi " +
            "re4fy reg3i re5it re3la rel4e re3lo re4lu re1m " +
            "re5mat rem5i re1n ren5d ren4t re5o re5pe re1pu " +
            "re4q re1r re4ri re1s re4se re5si re4sp re1st " +
            "re5sta re3str re4su re3ta re4te re4ti ret5r re4tu " +
            "re3un re1v re4val re5ve rev5el re5ver re5vi rew4 " +
            "ri4a ri4ag ri4at rib5 ri4c ri4ch rid5 ri4er " +
            "ri3ev ri4g rig5a ri4la rim5a ri4m rin4d rin4g " +
            "ri3o ri4os rip5 ri4qu ri4s ris4c ris4k ris4p " +
            "ri4t ri3ta ri3te rit5er ri3ti rit5r riv5 ri4v " +
            "rix4 ro4b ro4d ro4e rog5 ro4la rol4l rom5i " +
            "ron4a ron5i roo4 ro5pe ros5t rot5a ro4ty rou4 " +
            "rou5t row5d rox5 roy4 r4r rs4 r4se r4si " +
            "ru4b ru3d ru3el ru4g ru3in rul4 rum5p run4 " +
            "ru4p ru3t rut4i " +

            "sa2 sac5ri sac4r sad5 sai4 sal4 sal5a " +
            "sam5 san4d san5g san5t sa5p sa4t sau4 sav5 " +
            "saw5 say5 s1b sc4 scan4t sca4p scar4 scat4 " +
            "sce4 sch4 sci4 scle5 s4co scol4 scor5 scou4 " +
            "scru5 se2 se4a sea5w se4c se1cr sed4 se4d5l " +
            "se4g sel4 se3le sel5i se4m sem5i se2n sen5at " +
            "se5ni sen5o sen5t se5q ser4 ser5v ses5 se4t " +
            "se5v sew5i sh2 sha4 sham5 shan4 sha4p she4 " +
            "shel4 shen5 sher5 shin5 shi4p shiv5 sho4 " +
            "shor4 short5 shu4 shuf5 si2 si4b si4d " +
            "sid5er si5diz sif4 sigh4 sig4n si5gn5i sil4 " +
            "sim3 sin4 sin5g sin5n si3o si4p sir4 sis5 " +
            "si4t sit5u siv5 si4z sk2 ski4 skil4 sl2 " +
            "sla4 sli4 slo4 slu4 sm2 smal4 sman4 smar4 " +
            "smel4 smi4 smol5 sn4 so4 so5l sol4i sol5id " +
            "sol3v so3m som5 son5a son4e son5o so5no. soot4 " +
            "sor4 sor5d sov5 sp4 spa4 spar4 spe4 spen4d " +
            "spher3 spi4 spil4 spin5 spi5r spl4 spo4 spor4 " +
            "spr4 spu4 squ4 s1s st4 sta4 stal4 stam4 " +
            "star4 stat5i ste4 sten5 ster5i sti4 stin4 stir5 " +
            "stl4 sto4 ston4 sto5r strat5 stri4 strop4 " +
            "stru4 stu4 stum4 stur4 su2 su4b su5da su4g " +
            "su3i sul4 sum3 sun5 su3p su5pe super5 sur3 " +
            "su5s sus4 sw2 " +

            "ta2 ta4b tac4 ta4g tail5 ta4l tal5e tal5i " +
            "tam5 tan4 tan5g tap5 tar4 tar5n tas4 tat4 " +
            "ta5t ta4u tav4 taw4 tax5i te2 te4a teb4 " +
            "tec4 ted4 te4g tel4 ten5an ten4d ten5si ter4 " +
            "ter5a ter3e ter5iz ter4m tern5i ter3s tes4 tes5t " +
            "tet4 te4th te4u tex4 th2 tha4 than4 " +
            "thaw4 the4 the5at the3i ther4 ther5a therm5 " +
            "thi4 thin4 thir4 thl4 tho4 thor5i thr4 thro4 " +
            "thu4 thum4 ti2 ti4a ti4b tic4 tid5 " +
            "ti4er ti4g til4 tim5o5 tin4 tin5g ti3o ti4p " +
            "ti5so tis4 ti4t ti5tl tit5r ti3tu tiv5 " +
            "tl4 tm4 to4 to4d toe4 tof4 tog4 toi4 " +
            "to3le tom5 ton4 ton5al too4 tor4 tor5i tor5n " +
            "tos4 tot4 tou4 tou5s tow5 tox3 tr4 tra4 " +
            "trai4 tram4 tran4 trap5 trav5 tre4 trem5 tri4 " +
            "tri5c tril5 trip5 tri5pl tris4 tri5t tro4 tron5i " +
            "tro5p trou4 trov5 tru4 tru5i trum4 trus4 " +
            "ts4 tu2 tub4 tu3i tu5l tu4m tun4 tun5g " +
            "tur4 tur5b turs4 tu5t tut5i tw4 twa4 twi4 " +
            "ty4 ty5l " +

            "ua4 uab4 ual5 uan4 uar5 ub4 ub5in ub5l " +
            "uc4 uci4 uck4 ud4 ud5d ud3er ud3i ue4 " +
            "uf4 uf5f ug4 ug5li u5gu uh4 ui2 uil4 " +
            "uin4 ui4r ui4t uk4 ul4 ul5at ul5c ul5d " +
            "ul4e ul5en ul5et ul3in ul4l ul4m ul5o ul5t " +
            "ul3ti ul3tr ul5u ul5v um4 um5b um5d " +
            "umi4 um5in um4m um5o ump5 um4p un2 un4a " +
            "u5nai un3an un4b un5c un3d und5a un3de un4do " +
            "un3e un5f un3g un4g5l un5i un3k un3l un4m " +
            "un3n un5o un3r un3s un5t un1u un3w up3 " +
            "up3l up5o ur4 ur5ag ur3al ur3an ur4b ur3d " +
            "ur3e ur4f ur4g ur3i ur4l ur4m urn5 ur5o " +
            "ur4p ur3s ur4t u3ru ur5v us4 us5al us3el " +
            "us3er us4i us5l us5o us5p us5s us3t us5u " +
            "ut4 ut5en u3ti ut5il ut5in ut3io ut5iv ut5l " +
            "ut5of ut5on ut3r ut4t u3tu uu4 uv4 ux5 " +

            "va2 va4g val5e val5i val5o va5l van4d van5g " +
            "vap5 var5i vas5t vau4 ve2 ve4g vel5 ven5d " +
            "ven4e ven5i ver5b ver4d ver3e ver5n ver5s ves4 " +
            "vet5 vi4a vi4b vic5 vi4d vi4er vi4g vig5i " +
            "vil4 vin5d vin5e vi5ni vio4 vi4p vir4 vis4 " +
            "vi5so vi3su vit4 vi4t5i viv5 vi4v vl4 vo4 " +
            "voi4 vol5at vol5i vol5u vo4m vor5 vot5 vow4 " +
            "voy5 " +

            "wa2 wai4 wal4 war4 was4 wa5v waw4 way5 " +
            "we2 we4b wed4 wee4 wel4 wen4 wer4 wes4 " +
            "wev4 whi4 whis4 whi5tl wi2 wid4 wil4 " +
            "win4d win4g win5n wis4 wit4 wiz5 wl4 wo2 " +
            "wom4 won4 woo4 wor4 wor5s wor5t wow5 wr4 " +
            "wri4 writ5 " +

            "xa4 xac5 xag5 xam5 xe2 xer4 xi4a xi4c " +
            "xi4d xi5di xi4l xim5 xi4n xi4p xis4 " +
            "xi4t xl4 xo4 xp4 xpan4 xt4 xu4 " +

            "ya4 yar4 ye2 yel4 yer4 yes4 yet4 yi4 " +
            "yo4 yon4 you4 yr4 ys4 yt4 " +

            "za4 zar4 ze2 ze4n ze4p zer4 zet4 zi4 " +
            "zi4g zi4l zim4 zin4 zi4p zit4 zl4 zo4 " +
            "zon4 zoo4 " +

            // Exception / override patterns for common words
            "as5so4c " +
            "as5so5ci " +
            "dec5o3r " +
            "der5i5v " +
            "des3ti5n " +
            "di5vis5 " +
            "devi4 " +
            "devel5o " +
            "elec5tr " +
            "gen5er5a " +
            "hy3phen " +
            "hy5phen5a " +
            "in5tel5l " +
            "knowl5 " +
            "min5ute " +
            "mon5ey " +
            "moth4er5 " +
            "nev5er " +
            "noth5 " +
            "off5 " +
            "or5gan5i " +
            "phe5nom " +
            "pro5gram " +
            "pro5gr " +
            "pro3gram5m " +
            "re5search " +
            "rec5og " +
            "sep5a5r " +
            "ta5ble " +
            "through5 " +
            "un5der " +
            "work5 " +

            // Termination patterns (word endings with .)
            "4ble. 5. 4cy. 4ful. 4ing. 4ly. 4ment. 4ness. " +
            "4ous. 4sion. 4tion. 4ty. " +
            "al5iz " +
            "al5ized " +
            "com5put " +
            "com3pu5t " +
            "al5go5ri " +
            "al3go5rithm " +
            "al1go " +
            "go3rithm " +
            "rith4 " +
            "rithm4 " +
            "com3put5er " +
            "put5er " +
            "com5pu " +
            "de3vel5op " +
            "vel5op " +
            "de5vel " +
            "pro5gram5m " +
            "gram5m " +
            "gram5min " +
            "pro3gram " +
            "4gram. " +
            "ab5so " +
            "ab3sorb " +
            "ac5com " +
            "ac3com5mo " +
            "beau5ti " +
            "busi5n " +
            "cab5in " +
            "cal5en " +
            "chil5d " +
            "colo5n " +
            "coun5tr " +
            "dan5g " +
            "dif5fer " +
            "dis5cov " +
            "ed5u " +
            "ev5er " +
            "ev3ery " +
            "ex5per " +
            "ex5tra " +
            "fa5mil " +
            "fash5i " +
            "gov5ern " +
            "hap5p " +
            "hun5d " +
            "im5por5t " +
            "in5for5m " +
            "in3form " +
            "lan5gu " +
            "li5brar " +
            "lit5er " +
            "man5ag " +
            "mea5sur " +
            "mis5er " +
            "na5tur " +
            "nec5es " +
            "op5por " +
            "par5tic " +
            "per5hap " +
            "per5son " +
            "pic5tur " +
            "pleas5 " +
            "pos5si " +
            "prob5l " +
            "ques5ti " +
            "re5mem " +
            "sen5tenc " +
            "sev5er " +
            "sim5il " +
            "to5geth " +
            "un5der5st " +
            "writ5";
    }
}
