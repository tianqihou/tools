using System;
using System.Collections.Generic;

namespace Decompiler
{
	public class Operation
	{
        public const string IntString = "{0:X8}";

        public const string ByteString = "{0:X2}";

        public const string ShortString = "{0:X4}";

        public static String Hex(int i)
        {
            return Contact0x(Convert.ToString(i, 16));
        }

        public static String Hex(short i)
        {
            return Contact0x(Convert.ToString(i, 16));
        }

        public static String Hex(byte i)
        {
            return Contact0x(Convert.ToString(i, 16));
        }

        private static String Contact0x(string formated)
        {
            return string.Concat("0x", formated);
        }

        public static int ParseInt(string str)
        {
            if (str.StartsWith("0x"))
                return int.Parse(str.Remove(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            return int.Parse(str);
        }

        public static byte ParseByte(string str)
        {
            if (str.StartsWith("0x"))
                return byte.Parse(str.Remove(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            return byte.Parse(str);
        }

        public static short ParseShort(string str)
        {
            if (str.StartsWith("0x"))
                return short.Parse(str.Remove(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            return short.Parse(str);
        }
    }

	public partial class Move_Ainmation
	{
		String[] macros ={"loadparticle","unloadparticle","launchtemplate",
			"launchtask","pause","waitanimation","","","endanimation",
			"playsound1","pokespritetoBG","pokespritefromBG","setblends",
			"resetblends","call","return","setarg","choosetwoturnanim",
			"jumpifmoveturnEQ","goto","loadBG1","loaddefaultBG","waitforBG",
			"waitfortransparentBG","loadBG2","playsound2","pancontrol",
			"playsoundpanchange","soundcomplex","playsound3","set_BLDCNT",
			"launchsoundtask","waitforsound","jumpifargmatches",
			"anim22","anim23","jumpifnotinbattle","chooseBG","playsoundpanchange2",
			"playsoundpanchange3","leftbankBG_over_partnerBG",
			"bankBG_over_partnerBG","leftopponentbankBG_over_partnerBG",
			"makebankinvisible","makebankvisible","","","stopmusic"};

		//0代表参数个数未知，5代表需要写出template的详细信息
		List<byte[]> args = new List<byte[]>
		{
			new byte[]{2},
			new byte[]{2},
			new byte[]{5,1,0},
			new byte[]{4,1,0},
			new byte[]{1},
			null,null,null,null,new byte[]{2},
			new byte[]{1},
			new byte[]{1},
			new byte[]{2},
			null,new byte[]{4},
			null,new byte[]{1,2},
			new byte[]{4,4},
			new byte[]{1,4},
			new byte[]{4},
			new byte[]{1},
			null,null,null,new byte[]{1},
			new byte[]{2,1},
			new byte[]{1},
			new byte[]{2,1,1,1,1},
			new byte[]{2,1,1,1},
			new byte[]{2,1,1},
			new byte[]{2},
			new byte[]{4,0},
			null,new byte[]{1,2,6},
			new byte[]{1},
			new byte[]{1},
			new byte[]{4},
			new byte[]{1,1,1},
			new byte[]{2,1,1,1,1},
			new byte[]{2,1,1,1,1},
			new byte[]{1},
			null,new byte[]{1},
			new byte[]{1},
			new byte[]{1},
			null,null,null
		};
	}

	public partial class Move_Battle_Script
	{
		String[] macros = {"attackcanceler",	//0
            "accuracycheck",	//1
            "attackstring",	//2
            "ppreduce",	//3
            "critcalc",	//4
            "damagecalc",	//5
            "cmd6",	//6
            "damageadjustment",	//7
            "cmd8",	//8
            "attackanimation",	//9
            "waitanimation",	//A
            "graphicalhpupdate",	//B
            "datahpupdate",	//C
            "critmessage",	//D
            "effectiveness_sound",	//E
            "resultmessage",	//F
            "printstring",	//10
            "printstring2",	//11
            "waitmessage",	//12
            "printfromtable",	//13
            "printfromtable2",	//14
            "seteffectwithchance",	//15
            "seteffectprimary",	//16
            "seteffectsecondary",	//17
            "clearstatus",	//18
            "faintpokemon",	//19
            "cmd1a",	//1A
            "cmd1b",	//1B
            "jumpifstatus",	//1C
            "jumpifsecondarystatus",	//1D
            "jumpifability",	//1E
            "jumpifhalverset",	//1F
            "jumpifstat",	//20
            "jumpifspecialstatus3",	//21
            "jumpiftype",	//22
            "giveexp",	//23
            "gotoandsomething",	//24
            "cmd25",	//25
            "cmd26",	//26
            "jumpifmultihitcontinues",	//27
            "goto_cmd",	//28
            "jumpifbyte",	//29
            "jumpifhalfword",	//2A
            "jumpifword",	//2B
            "jumpifarrayequal",	//2C
            "jumpifarraynotequal",	//2D
            "setbyte",	//2E
            "addbyte",	//2F
            "subtractbyte",	//30
            "copyarray",	//31
            "cmd32",	//32
            "orbyte",	//33
            "orhalfword",	//34
            "orword",	//35
            "bicbyte",	//36
            "bichalfword",	//37
            "bicword",	//38
            "pause_cmd",	//39
            "waitstate",	//3A
            "somethinghealcmd3b",	//3B
            "return_cmd",	//3C
            "end_cmd",	//3D
            "end2",	//3E
            "end3",	//3F
            "cmd40",	//40
            "call",	//41
            "jumpiftype2",	//42
            "jumpifabilitypresent",	//43
            "cmd44",	//44
            "playanimation",	//45
            "cmd46",	//46
            "set_statchange_values",	//47
            "playstatchangeanimation",	//48
            "cmd49",	//49
            "damagecalc2",	//4A
            "cmd4b",	//4B
            "switch1",	//4C
            "switch2",	//4D
            "switch3",	//4E
            "jumpifcannotswitch",	//4F
            "openpartyscreen",	//50
            "switch_handle_order",	//51
            "switchineffects",	//52
            "trainer_slide",	//53
            "cmd54",	//54
            "cmd55",	//55
            "playfaintingcry",	//56
            "cmd57",	//57
            "return_to_ball",	//58
            "checkiflearnmoveinbattle",	//59
            "cmd5a",	//5A
            "cmd5b",	//5B
            "hitanim",	//5C
            "cmd5d",	//5D
            "cmd5e",	//5E
            "cmd5f",	//5F
            "cmd60",	//60
            "cmd61",	//61
            "cmd62",	//62
            "jumptoattack",	//63
            "statusanimation",	//64
            "status2animation",	//65
            "chosenstatusanimation",	//66
            "cmd67",	//67
            "cmd68",	//68
            "cmd69",	//69
            "removeitem",	//6A
            "cmd6b",	//6B
            "cmd6c",	//6C
            "cmd6d",	//6D
            "cmd6e",	//6E
            "cmd6f",	//6F
            "recordability",	//70
            "cmd71",	//71
            "cmd72",	//72
            "cmd73",	//73
            "cmd74",	//74
            "cmd75",	//75
            "cmd76",	//76
            "setprotect",	//77
            "faintifabilitynotdamp",	//78
            "setuserhptozero",	//79
            "jumpwhiletargetvalid",	//7A
            "setdamageasrestorehalfmaxhp",	//7B
            "jumptolastusedattack",	//7C
            "setrain",	//7D
            "setreflect",	//7E
            "setleechseed",	//7F
            "manipulatedamage",	//80
            "setrest",	//81
            "jumpifnotfirstturn",	//82
            "callasm_cmd",	//83
            "jumpifcannotsleep",	//84
            "stockpile",	//85
            "stockpiletobasedamage",	//86
            "stockpiletohprecovery",	//87
            "draindamage",	//88
            "statbuffchange",	//89
            "normalisebuffs",	//8A
            "setbide",	//8B
            "confuseifrepeatingattackends",	//8C
            "setloopcounter",	//8D
            "preparemultihit",	//8E
            "forcerandomswitch",	//8F
            "changetypestoenemyattacktype",	//90
            "givemoney",	//91
            "setlightscreen",	//92
            "koplussomethings",	//93
            "gethalfcurrentenemyhp",	//94
            "setsandstorm",	//95
            "weatherdamage",	//96
            "tryinfatuatetarget",	//97
            "statusiconeupdate",	//98
            "setmisteffect",	//99
            "setincreasedcriticalchance",	//9A
            "transformdataexecution",	//9B
            "setsubstituteeffect",	//9C
            "copyattack",	//9D
            "metronomeeffect",	//9E
            "nightshadedamageeffect",	//9F
            "psywavedamageeffect",	//A0
            "counterdamagecalculator",	//A1
            "mirrorcoatdamagecalculator",	//A2
            "disablelastusedattack",	//A3
            "setencore",	//A4
            "painsplitdamagecalculator",	//A5
            "settypetorandomresistance",	//A6
            "setalwayshitflag",	//A7
            "copymovepermanently",	//A8
            "selectrandommovefromusermoves",	//A9
            "destinybondeffect",	//AA
            "cmdab",	//AB
            "remaininghptopower",	//AC
            "reducepprandom",	//AD
            "clearstatusifnotsoundproofed",	//AE
            "cursetarget",	//AF
            "setspikes",	//B0
            "setforesight",	//B1
            "setperishsong",	//B2
            "rolloutdamagecalculation",	//B3
            "jumpifconfusedandattackmaxed",	//B4
            "furycutterdamagecalculation",	//B5
            "happinesstodamagecalculation",	//B6
            "presentdamagecalculation",	//B7
            "setsafeguard",	//B8
            "magnitudedamagecalculation",	//B9
            "cmdba",	//BA
            "setsunny",	//BB
            "maxattackhalvehp",	//BC
            "copyfoestats",	//BD
            "breakfree",	//BE
            "setcurled",	//BF
            "recoverbasedonsunlight",	//C0
            "hiddenpowerdamagecalculation",	//C1
            "selectnexttarget",	//C2
            "setfutureattack",	//C3
            "beatupcalculation",	//C4
            "hidepreattack",	//C5
            "unhidepostattack",	//C6
            "setminimize",	//C7
            "sethail",	//C8
            "jumpifattackandspecialattackcannotfall",	//C9
            "setforcedtarget",	//CA
            "setcharge",	//CB
            "callterrainattack",	//CC
            "cureifburnedparalysedorpoisoned",	//CD
            "settorment",	//CE
            "jumpifnodamage",	//CF
            "settaunt",	//D0
            "sethelpinghand",	//D1
            "itemswap",	//D2
            "copyability",	//D3
            "cmdd4",	//D4
            "setroots",	//D5
            "doubledamagedealtifdamaged",	//D6
            "setyawn",	//D7
            "setdamagetohealthdifference",	//D8
            "scaledamagebyhealthratio",	//D9
            "abilityswap",	//DA
            "imprisoneffect",	//DB
            "setgrudge",	//DC
            "weightdamagecalculation",	//DD
            "assistattackselect",	//DE
            "setmagiccoat",	//DF
            "setstealstatchange",	//E0
			"cmde1",	//E1
            "swithchoutabilities",	//E2
            "jumpiffainted",	//E3
            "secretpowereffect",	//E4
            "pickupitemcalculation",	//E5
            "castformchangeanim",	//E6
            "checkcastform",	//E7
            "settypebasedhalvers",	//E8
            "seteffectbyweather",	//E9
            "recycleitem",	//EA
            "settypetoterrain",	//EB
            "pursuitwhenswitched",	//EC
            "snatchmove",	//ED
            "removereflectlightscreen",	//EE
            "pokemoncatchfunction",	//EF
            "catchpoke",	//F0
            "capturesomethingf1",	//F1
            "capturesomethingf2",	//F2
            "capturesomethingf3",	//F3
            "removehp",	//F4
            "curestatutfirstword",	//F5
            "cmdf6",	//F6
            "activesidesomething",	//F7
            "trainer_back_slide"    //F8 
        };

		enum ArgType:byte
		{
			u8,
			u16,
			u32,
			address,
			compare,
			bank,
			//结果 MoveOutcome, 
			endorreturn
		}

		List<ArgType[]> args = new List<ArgType[]>()
		{
			null,//0
			new ArgType[]{ArgType.u32,ArgType.u16},//1
			null,//2
			null,//3
			null,//4
			null,//5
			null,//6
			null,//7
			null,//8
			null,//9
			null,//A
			new ArgType[]{ArgType.u8},//B
			new ArgType[]{ArgType.u8},//C
			null,//D
			null,//E
			null,//F
			new ArgType[]{ArgType.u16},//10
			new ArgType[]{ArgType.u16},//11
			new ArgType[]{ArgType.u16},//12
			new ArgType[]{ArgType.u32},//13
			new ArgType[]{ArgType.u32},//14
			null,//15
			null,//16
			null,//17
			new ArgType[]{ArgType.u8},//18
			new ArgType[]{ArgType.u8,ArgType.u8,ArgType.u32},//19
			new ArgType[]{ArgType.u8},//1A
			new ArgType[]{ArgType.u8},//1B
			new ArgType[]{ArgType.bank, ArgType.u32,ArgType.address},//1C
			new ArgType[]{ArgType.bank, ArgType.u32,ArgType.address},//1D
			new ArgType[]{ArgType.bank, ArgType.u8,ArgType.address},//1E
			new ArgType[]{ArgType.bank, ArgType.u16,ArgType.address},//1F
			new ArgType[]{ArgType.bank,ArgType.compare,ArgType.u8,ArgType.u8,ArgType.address},//20
			new ArgType[]{ArgType.bank, ArgType.u32,ArgType.u8,ArgType.address},//21
			new ArgType[]{ArgType.bank, ArgType.u8,ArgType.address},//22
			new ArgType[]{ArgType.bank},//23
			new ArgType[]{ArgType.address},//24
			null,//25
			new ArgType[]{ArgType.u8},//26
			new ArgType[]{ArgType.address},//27
			new ArgType[]{ArgType.address},//28
			new ArgType[]{ArgType.compare, ArgType.u32,ArgType.u8,ArgType.address},//29
			new ArgType[]{ArgType.compare, ArgType.u32,ArgType.u16,ArgType.address},//2A
			new ArgType[]{ArgType.compare, ArgType.u32,ArgType.u32,ArgType.address},//2B
			new ArgType[]{ArgType.u32,ArgType.u32,ArgType.u8,ArgType.address},//2C
			new ArgType[]{ArgType.u32,ArgType.u32,ArgType.u8,ArgType.address},//2D
			new ArgType[]{ArgType.u32,ArgType.u8},//2E
			new ArgType[]{ArgType.u32,ArgType.u8},//2F
			new ArgType[]{ArgType.u32,ArgType.u8},//30
			new ArgType[]{ArgType.u32,ArgType.u32,ArgType.u8},//31
			new ArgType[]{ArgType.u32,ArgType.u32,ArgType.u32,ArgType.u8},//32
			new ArgType[]{ArgType.u32,ArgType.u8},//33
			new ArgType[]{ArgType.u32,ArgType.u16},//34
			new ArgType[]{ArgType.u32,ArgType.u32},//35
			new ArgType[]{ArgType.u32,ArgType.u8},//36
			new ArgType[]{ArgType.u32,ArgType.u16},//37
			new ArgType[]{ArgType.u32,ArgType.u32},//38
			new ArgType[]{ArgType.u16},//39
			null,//3A
			new ArgType[]{ArgType.u8},//3B
			new ArgType[]{ArgType.endorreturn},//3C
			new ArgType[]{ArgType.endorreturn},//3D
			new ArgType[]{ArgType.endorreturn},//3E
			new ArgType[]{ArgType.endorreturn},//3F
			new ArgType[]{ArgType.u32},//40
			new ArgType[]{ArgType.address},//41
			new ArgType[]{ArgType.u8,ArgType.u8,ArgType.address},//42
			new ArgType[]{ArgType.u8,ArgType.u8,ArgType.address},//43
			null,//44
			new ArgType[]{ArgType.u8,ArgType.u8,ArgType.address},//45
			new ArgType[]{ArgType.u8,ArgType.u32,ArgType.u32},//46
			null,//47
			new ArgType[]{ArgType.bank,ArgType.u8,ArgType.u8},//48
			new ArgType[]{ArgType.u8,ArgType.u8,ArgType.u8},//49
			null,//4A
			null,//4B
			new ArgType[]{ArgType.u8},//4C
			new ArgType[]{ArgType.bank,ArgType.u8},//4D
			new ArgType[]{ArgType.bank,ArgType.u8},//4E
			new ArgType[]{ArgType.bank,ArgType.address},//4F
			new ArgType[]{ArgType.bank,ArgType.u32},//50
			new ArgType[]{ArgType.bank,ArgType.u8},//51
			new ArgType[]{ArgType.bank},//52
			new ArgType[]{ArgType.bank},//53
			new ArgType[]{ArgType.u16},//54
			new ArgType[]{ArgType.u32},//55
			new ArgType[]{ArgType.bank},//56
			null,//57
			new ArgType[]{ArgType.bank},//58
			new ArgType[]{ArgType.u32,ArgType.u32,ArgType.u8},//59
			new ArgType[]{ArgType.u32},//5A
			new ArgType[]{ArgType.u32},//5B
			new ArgType[]{ArgType.bank},//5C
			null,//5D
			new ArgType[]{ArgType.bank},//5E
			null,//5F
			new ArgType[]{ArgType.u8},//60
			new ArgType[]{ArgType.bank},//61
			new ArgType[]{ArgType.bank},//62
			new ArgType[]{ArgType.bank},//63
			new ArgType[]{ArgType.bank},//64
			new ArgType[]{ArgType.bank,ArgType.u32},//65
			new ArgType[]{ArgType.bank,ArgType.u8,ArgType.address},//66
			null,//67
			null,//68
			null,//69
			new ArgType[]{ArgType.bank},//6A
			null,//6B
			null,//6C
			null,//6D
			null,//6E
			new ArgType[]{ArgType.bank},//6F
			new ArgType[]{ArgType.bank},//70
			null,//71
			new ArgType[]{ArgType.u32},//72
			new ArgType[]{ArgType.bank},//73
			new ArgType[]{ArgType.bank},//74
			null,//75
			new ArgType[]{ArgType.bank,ArgType.u8},//76
			null,//77
			null,//78
			null,//79
			new ArgType[]{ArgType.address},//7A
			new ArgType[]{ArgType.u32,ArgType.u8},//7B
			null,//7C
			null,//7D
			null,//7E
			null,//7F
			new ArgType[]{ArgType.u8},//80
			new ArgType[]{ArgType.u32},//81
			new ArgType[]{ArgType.address},//82
			new ArgType[]{ArgType.u16},//83
			new ArgType[]{ArgType.address},//84
			new ArgType[]{ArgType.address},//85
			new ArgType[]{ArgType.u32},//86
			new ArgType[]{ArgType.u32},//87
			null,//88
			new ArgType[]{ArgType.u8,ArgType.u32},//89
			null,//8A
			null,//8B
			null,//8C
			new ArgType[]{ArgType.u8},//8D
			null,//8E
			new ArgType[]{ArgType.u32},//8F
			new ArgType[]{ArgType.u32},//90
			null,//91
			null,//92
			new ArgType[]{ArgType.u32},//93
			null,//94
			null,//95
			null,//96
			new ArgType[]{ArgType.u32},//97
			new ArgType[]{ArgType.bank},//98
			null,//99
			null,//9A
			null,//9B
			null,//9C
			new ArgType[]{ArgType.u32},//9D
			null,//9E
			null,//9F
			null,//A0
			new ArgType[]{ArgType.u32},//A1
			new ArgType[]{ArgType.u32},//A2
			new ArgType[]{ArgType.u32},//A3
			new ArgType[]{ArgType.u32},//A4
			new ArgType[]{ArgType.u32},//A5
			new ArgType[]{ArgType.u32},//A6
			null,//A7
			new ArgType[]{ArgType.u32},//A8
			new ArgType[]{ArgType.address},//A9
			null,//AA
			null,//AB
			null,//AC
			new ArgType[]{ArgType.u32},//AD
			null,//AE
			new ArgType[]{ArgType.u32},//AF
			new ArgType[]{ArgType.u32},//B0
			new ArgType[]{ArgType.u32},//B1
			null,//B2
			null,//B3
			new ArgType[]{ArgType.u8,ArgType.u32},//B4
			null,//B5
			null,//B6
			null,//B7
			null,//B8
			null,//B9
			new ArgType[]{ArgType.u32},//BA
			null,//BB
			new ArgType[]{ArgType.u32},//BC
			new ArgType[]{ArgType.u32},//BD
			null,//BE
			null,//BF
			null,//C0
			null,//C1
			null,//C2
			new ArgType[]{ArgType.u32},//C3
			new ArgType[]{ArgType.u32,ArgType.u32},//C4
			null,//C5
			null,//C6
			null,//C7
			null,//C8
			new ArgType[]{ArgType.u32},//C9
			null,//CA
			null,//CB
			null,//CC
			new ArgType[]{ArgType.u32},//CD
			new ArgType[]{ArgType.u32},//CE
			new ArgType[]{ArgType.address},//CF
			new ArgType[]{ArgType.u32},//D0
			new ArgType[]{ArgType.u32},//D1
			new ArgType[]{ArgType.u32},//D2
			new ArgType[]{ArgType.u32},//D3
			new ArgType[]{ArgType.u8,ArgType.u32},//D4
			new ArgType[]{ArgType.u32},//D5
			null,//D6
			new ArgType[]{ArgType.u32},//D7
			new ArgType[]{ArgType.u32},//D8
			null,//D9
			new ArgType[]{ArgType.u32},//DA
			new ArgType[]{ArgType.u32},//DB
			new ArgType[]{ArgType.u32},//DC
			new ArgType[]{ArgType.u32},//DD
			new ArgType[]{ArgType.u32},//DE
			new ArgType[]{ArgType.u32},//DF
			new ArgType[]{ArgType.u32},//E0
			new ArgType[]{ArgType.u32},//E1
			new ArgType[]{ArgType.bank},//E2
			new ArgType[]{ArgType.u8,ArgType.address},//E3
			null,//E4
			null,//E5
			null,//E6
			null,//E7
			new ArgType[]{ArgType.u32},//E8
			null,//E9
			new ArgType[]{ArgType.u32},//EA
			new ArgType[]{ArgType.u32},//EB
			new ArgType[]{ArgType.u32},//EC
			null,//ED
			null,//EE
			null,//EF
			null,//F0
			new ArgType[]{ArgType.u32},//F1
			null,//F2
			new ArgType[]{ArgType.u32},//F3
			null,//F4
			null,//F5
			null,//F6
			null,//F7
			new ArgType[]{ArgType.bank}//F8
		};

		String[] bank = { "bank_target", "bank_attacker", "bank_partner_def" };
		String[] compare = { "Equals", "Not_Equal_To", "Less_Than",
			"Greater_Than", "Has_One_Bit_Common" };
		

		String GetArg(ArgType arg)
		{
			switch (arg)
			{				
				case ArgType.u8:
					return Operation.Hex(br.ReadByte());
				case ArgType.bank:
					byte bank = br.ReadByte();
					if (bank < 3)
						return this.bank[bank];
					else
						return Operation.Hex(bank);		
				case ArgType.u16:
					return Operation.Hex(br.ReadInt16());
				case ArgType.u32:
					return Operation.Hex(br.ReadInt32());				
				case ArgType.compare:
					byte compare = br.ReadByte();
					if (compare < 5)
						return this.compare[compare];
					else
						return Operation.Hex(compare);				
			}
			return null;
		}
	}
	
	public partial class XSE_scripts
	{
		String[] macros = {
			"nop",//0x0
			"nop1",//0x1
			"end",//0x2
			"return",//0x3
			"call",//0x4
			"goto",//0x5
			"if",//0x6
			"if",//0x7
			"gotostd",//0x8
			"callstd",//0x9
			"gotostdif",//0xA
			"callstdif",//0xB
			"jumpram",//0xC
			"killscript",//0xD
			"setbyte",//0xE
			"loadpointer",//0xF
			"setbyte2",//0x10
			"writebytetooffset",//0x11
			"loadbytefrompointer",//0x12
			"setfarbyte",//0x13
			"copyscriptbanks",//0x14
			"copybyte",//0x15
			"setvar",//0x16
			"addvar",//0x17
			"subvar",//0x18
			"copyvar",//0x19
			"copyvarifnotzero",//0x1A
			"comparebanks",//0x1B
			"comparebanktobyte",//0x1C
			"comparebanktofarbyte",//0x1D
			"comparefarbytetobank",//0x1E
			"comparefarbytetobyte",//0x1F
			"comparefarbytes",//0x20
			"compare",//0x21
			"comparevars",//0x22
			"callasm",//0x23
			"cmd24",//0x24
			"special",//0x25
			"special2",//0x26
			"waitstate",//0x27
			"pause",//0x28
			"setflag",//0x29
			"clearflag",//0x2A
			"checkflag",//0x2B
			"cmd2C",//0x2C
			"checkdailyflags",//0x2D
			"resetvars",//0x2E
			"sound",//0x2F
			"checksound",//0x30
			"fanfare",//0x31
			"waitfanfare",//0x32
			"playsong",//0x33
			"playsong2",//0x34
			"fadedefault",//0x35
			"fadesong",//0x36
			"fadeout",//0x37
			"fadein",//0x38
			"warp",//0x39
			"warpmuted",//0x3A
			"warpwalk",//0x3B
			"warphole",//0x3C
			"warpteleport",//0x3D
			"warp3",//0x3E
			"setwarpplace",//0x3F
			"warp4",//0x40
			"warp5",//0x41
			"getplayerpos",//0x42
			"countpokemon",//0x43
			"additem",//0x44
			"removeitem",//0x45
			"checkitemroom",//0x46
			"checkitem",//0x47
			"checkitemtype",//0x48
			"addpcitem",//0x49
			"checkpcitem",//0x4A
			"adddecoration",//0x4B
			"removedecoration",//0x4C
			"testdecoration",//0x4D
			"checkdecoration",//0x4E
			"applymovement",//0x4F
			"applymovementpos",//0x50
			"waitmovement",//0x51
			"waitmovementpos",//0x52
			"hidesprite",//0x53
			"hidespritepos",//0x54
			"showsprite",//0x55
			null,
			"movesprite",//0x57
			null,
			null,
			"faceplayer",//0x5A
			null,
			"trainerbattle",//0x5C
			"repeattrainerbattle",//0x5D
			"endtrainerbattle",//0x5E
			"endtrainerbattle2",//0x5F
			"checktrainerflag",//0x60
			"settrainerflag",//0x61
			"cleartrainerflag",//0x62
			"movesprite2",//0x63
			"moveoffscreen",//0x64
			"spritebehave",//0x65
			"waitmsg",//0x66
			"preparemsg",//0x67
			"closeonkeypress",//0x68
			"lockall",//0x69
			"lock",//0x6A
			"releaseall",//0x6B
			"release",//0x6C
			"waitkeypress",//0x6D
			"yesnobox",//0x6E
			"multichoice",//0x6F
			"multichoice2",//0x70
			"multichoice3",//0x71
			"showbox",//0x72
			"hidebox",//0x73
			"clearbox",//0x74
			"showpokepic",//0x75
			"hidepokepic",//0x76
			"showcontestwinner",//0x77
			"braille",//0x78
			"givepokemon",//0x79
			"giveegg",//0x7A
			"setpkmnpp",//0x7B
			"checkattack",//0x7C
			"bufferpokemon",//0x7D
			"bufferfirstpokemon",//0x7E
			"bufferpartypokemon",//0x7F
			"bufferitem",//0x80
			"bufferdecoration",//0x81
			"bufferattack",//0x82
			"buffernumber",//0x83
			"bufferstd",//0x84
			"bufferstring",//0x85
			"pokemart",//0x86
			"pokemart2",//0x87
			"pokemart3",//0x88
			"pokecasino",//0x89
			"cmd8A",//0x8A
			"choosecontestpokemon",//0x8B
			"startcontest",//0x8C
			"showcontestresults",//0x8D
			"contestlinktransfer",//0x8E
			"random",//0x8F
			"givemoney",//0x90
			"paymoney",//0x91
			"checkmoney",//0x92
			"showmoney",//0x93
			"hidemoney",//0x94
			"updatemoney",//0x95
			"cmd96",//0x96
			"fadescreen",//0x97
			"fadescreendelay",//0x98
			"darken",//0x99
			"lighten",//0x9A
			"preparmsg2",//0x9B
			"doanimation",//0x9C
			"setanimation",//0x9D
			"checkanimation",//0x9E
			"sethealingplace",//0x9F
			"checkgender",//0xA0
			"cry",//0xA1
			"setmaptile",//0xA2
			"resetweather",//0xA3
			"setweather",//0xA4
			"doweather",//0xA5
			"cmdA6",//0xA6
			"setmapfooter",//0xA7
			"spritelevelup",//0xA8
			"restorespritelevel",//0xA9
			"createsprite",//0xAA
			"spriteface2",//0xAB
			"setdooropened",//0xAC
			"setdoorclosed",//0xAD
			"doorchange",//0xAE
			"setdooropened2",//0xAF
			"setdoorclosed2",//0xB0
			"cmdB1",//0xB1
			"cmdB2",//0xB2
			"checkcoins",//0xB3
			"givecoins",//0xB4
			"removecoins",//0xB5
			"setwildbattle",//0xB6
			"dowildbattle",//0xB7
			"setvirtualaddress",//0xB8
			"virtualgoto",//0xB9
			"virtualcall",//0xBA
			"virtualgotoif",//0xBB
			"virtualcallif",//0xBC
			"virtualmsgbox",//0xBD
			"virtualloadpointer",//0xBE
			"virtualbuffer",//0xBF
			"showcoins",//0xC0
			"hidecoins",//0xC1
			"updatecoins",//0xC2
			"cmdC3",//0xC3
			"warp6",//0xC4
			"waitcry",//0xC5
			"bufferboxname",//0xC6
			"textcolor",//0xC7
			"cmdC8",//0xC8
			"cmdC9",//0xC9
			"signmsg",//0xCA
			"normalmsg",//0xCB
			"comparehiddenvar",//0xCC
			"setobedience",//0xCD
			"checkobedience",//0xCE
			"executeram",//0xCF
			"setworldmapflag",//0xD0
			"warpteleport2",//0xD1
			"setcatchlocation",//0xD2
			"braille2",//0xD3
			"bufferitems",//0xD4
			"cmdD5",//0xD5
			"cmdD6",//0xD6
			"warp7",//0xD7
			"cmdD8",//0xD8
			"cmdD9",//0xD9
			"hidebox2",//0xDA
			"preparemsg3",//0xDB
			"fadescreen3",//0xDC
			"buffertrainerclass",//0xDD
			"buffertrainername",//0xDE
			"pokenavcall",//0xDF
			"warp8",//0xE0
			"buffercontesttype",//0xE1
			"bufferitems2"//0xE2			
		};

		List<byte[]> args = new List<byte[]>{
			null,	//0x0
			null,//0x1
			null,//0x2
			null,//0x3
			new byte[]{ 4},//0x4
			new byte[]{ 4},//0x5
			new byte[]{ 1,4},//0x6
			new byte[]{ 1,4},//0x7
			new byte[]{ 1},//0x8
			new byte[]{ 1},//0x9
			new byte[]{ 1,1},//0xA
			new byte[]{ 1,1},//0xB
			null,//0xC
			null,//0xD
			new byte[]{ 1},//0xE
			new byte[]{ 1,4},//0xF
			new byte[]{ 1,1},//0x10
			new byte[]{ 1,4},//0x11
			new byte[]{ 1,4},//0x12
			new byte[]{ 1,4},//0x13
			new byte[]{ 1,1},//0x14
			new byte[]{ 4,4},//0x15
			new byte[]{ 2,2},//0x16
			new byte[]{ 2,2},//0x17
			new byte[]{ 2,2},//0x18
			new byte[]{ 2,2},//0x19
			new byte[]{ 2,2},//0x1A
			new byte[]{ 1,1},//0x1B
			new byte[]{ 1,1},//0x1C
			new byte[]{ 1,4},//0x1D
			new byte[]{ 4,1},//0x1E
			new byte[]{ 4,1},//0x1F
			new byte[]{ 4,4},//0x20
			new byte[]{ 2,2},//0x21
			new byte[]{ 2,2},//0x22
			new byte[]{ 4},//0x23
			new byte[]{ 4},//0x24
			new byte[]{ 2},//0x25
			new byte[]{ 2,2},//0x26
			null,//0x27
			new byte[]{ 2},//0x28
			new byte[]{ 2},//0x29
			new byte[]{ 2},//0x2A
			new byte[]{ 2},//0x2B
			null,//0x2C
			null,//0x2D
			null,//0x2E
			new byte[]{ 2},//0x2F
			null,//0x30
			new byte[]{ 2},//0x31
			null,//0x32
			new byte[]{ 2,1},//0x33
			new byte[]{ 2},//0x34
			null,//0x35
			new byte[]{ 2},//0x36
			new byte[]{ 1},//0x37
			new byte[]{ 1},//0x38
			new byte[]{ 1,1,1,2,2},//0x39
			new byte[]{ 1,1,1,2,2},//0x3A
			new byte[]{ 1,1,1,2,2},//0x3B
			new byte[]{ 1,1},//0x3C
			new byte[]{ 1,1,1,2,2},//0x3D
			new byte[]{ 1,1,1,2,2},//0x3E
			new byte[]{ 1,1,1,2,2},//0x3F
			new byte[]{ 1,1,1,2,2},//0x40
			new byte[]{ 1,1,1,2,2},//0x41
			new byte[]{ 2,2},//0x42
			null,//0x43
			new byte[]{ 2,2},//0x44
			new byte[]{ 2,2},//0x45
			new byte[]{ 2,2},//0x46
			new byte[]{ 2,2},//0x47
			new byte[]{ 2},//0x48
			new byte[]{ 2,2},//0x49
			new byte[]{ 2,2},//0x4A
			new byte[]{ 2},//0x4B
			new byte[]{ 2},//0x4C
			new byte[]{ 2},//0x4D
			new byte[]{ 2},//0x4E
			new byte[]{ 2,4},//0x4F
			new byte[]{ 2,4},//0x50
			new byte[]{ 2},//0x51
			new byte[]{ 2,1,1},//0x52
			new byte[]{ 2},//0x53
			new byte[]{ 2,1,1},//0x54
			new byte[]{ 2},//0x55
			null,
			new byte[]{ 2,2,2},//0x57
			null,null,
			null,//0x5A
			null,
			new byte[]{ 1,2,2},//0x5C			
			null,//0x5D
			null,//0x5E
			null,//0x5F
			new byte[]{ 2},//0x60
			new byte[]{ 2},//0x61
			new byte[]{ 2},//0x62
			new byte[]{ 2,2,2},//0x63
			new byte[]{ 2},//0x64
			new byte[]{ 2,1},//0x65
			null,//0x66
			new byte[]{ 4},//0x67
			null,//0x68
			null,//0x69
			null,//0x6A
			null,//0x6B
			null,//0x6C
			null,//0x6D
			new byte[]{ 1,1},//0x6E
			new byte[]{ 1,1,1,1},//0x6F
			new byte[]{ 1,1,1,1,1},//0x70
			new byte[]{ 1,1,1,1,1},//0x71
			new byte[]{ 1,1,1,1},//0x72
			new byte[]{ 1,1,1,1},//0x73
			new byte[]{ 1,1,1,1},//0x74
			new byte[]{ 2,1,1},//0x75
			null,//0x76
			new byte[]{ 1},//0x77
			new byte[]{ 4},//0x78
			new byte[]{ 2,1,2,4,4,1},//0x79
			new byte[]{ 2},//0x7A
			new byte[]{ 1,1,1},//0x7B
			new byte[]{ 2},//0x7C
			new byte[]{ 1,2},//0x7D
			new byte[]{ 1},//0x7E
			new byte[]{ 1,2},//0x7F
			new byte[]{ 1,2},//0x80
			new byte[]{ 1,2},//0x81
			new byte[]{ 1,2},//0x82
			new byte[]{ 1,2},//0x83
			new byte[]{ 1,2},//0x84
			new byte[]{ 1,2},//0x85
			new byte[]{ 4},//0x86
			new byte[]{ 4},//0x87
			new byte[]{ 4},//0x88
			new byte[]{ 2},//0x89
			new byte[]{ 1,1,1},//0x8A
			null,//0x8B
			null,//0x8C
			null,//0x8D
			null,//0x8E
			new byte[]{ 2},//0x8F
			new byte[]{ 4,1},//0x90
			new byte[]{ 4,1},//0x91
			new byte[]{ 4,1},//0x92
			new byte[]{ 1,1},//0x93
			new byte[]{ 1,1},//0x94
			new byte[]{ 1,1},//0x95
			new byte[]{ 2},//0x96
			new byte[]{ 1},//0x97
			new byte[]{ 1,1},//0x98
			new byte[]{ 2},//0x99
			new byte[]{ 1},//0x9A
			new byte[]{ 4},//0x9B
			new byte[]{ 2},//0x9C
			new byte[]{ 1,2},//0x9D
			new byte[]{ 2},//0x9E
			new byte[]{ 2},//0x9F
			null,//0xA0
			new byte[]{ 2,2},//0xA1
			new byte[]{ 2,2,2,2},//0xA2
			null,//0xA3
			new byte[]{ 2},//0xA4
			null,//0xA5
			new byte[]{ 1},//0xA6
			new byte[]{ 2},//0xA7
			new byte[]{ 2,1,1,1},//0xA8
			new byte[]{ 2,1,1},//0xA9
			new byte[]{ 1,1,2,2,1,1},//0xAA
			new byte[]{ 1,1},//0xAB
			new byte[]{ 2,2},//0xAC
			new byte[]{ 2,2},//0xAD
			null,//0xAE
			new byte[]{ 2,2},//0xAF
			new byte[]{ 2,2},//0xB0
			null,//0xB1
			null,//0xB2
			new byte[]{ 2},//0xB3
			new byte[]{ 2},//0xB4
			new byte[]{ 2},//0xB5
			new byte[]{ 2,1,2},//0xB6
			null,//0xB7
			new byte[]{ 4},//0xB8
			new byte[]{ 4},//0xB9
			new byte[]{ 4},//0xBA
			new byte[]{ 1,4},//0xBB
			new byte[]{ 1,4},//0xBC
			new byte[]{ 4},//0xBD
			new byte[]{ 4},//0xBE
			new byte[]{ 1,4},//0xBF
			new byte[]{ 1,1},//0xC0
			new byte[]{ 1,1},//0xC1
			new byte[]{ 1,1},//0xC2
			new byte[]{ 1},//0xC3
			new byte[]{ 1,1,1,2,2},//0xC4
			null,//0xC5
			new byte[]{ 1,2},//0xC6
			new byte[]{ 1},//0xC7
			new byte[]{ 4},//0xC8
			null,//0xC9
			null,//0xCA
			null,//0xCB
			new byte[]{ 1,4},//0xCC
			new byte[]{	2},//0xCD
			new byte[]{	2},//0xCE
			null,//0xCF
			new byte[]{ 2},//0xD0
			null,//0xD1
			new byte[]{	2,1},//0xD2
			new byte[]{	4},//0xD3
			new byte[]{	2,2},//0xD4
			null,//0xD5
			null,//0xD6
			new byte[]{	1,1,1,2,2},//0xD7
			null,//0xD8
			null,//0xD9
			null,//0xDA
			new byte[]{	4},//0xDB
			new byte[]{	1},//0xDC
			new byte[]{	1,2},//0xDD
			new byte[]{	1,2},//0xDE
			new byte[]{	4},//0xDF
			new byte[]{	1,1,1,2,2},//0xE0
			new byte[]{	1,2},//0xE1
			new byte[]{	1,2,2},//0xE2
		};

		String[] message_type = { "MSG_OBTAIN", "MSG_FIND", "MSG_FACE", "MSG_SIGN",
			"MSG_KEEPOPEN","MSG_YESNO","MSG_NORMAL","0x7","0x8","0x9","MSG_POKENAV" };

		byte[] trainer_arg = new byte[10] {2,3,3,1,3,2,4,3,4,2};
		
	}
}
