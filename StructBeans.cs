using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Tools
{
	
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Poke_basestats
    {
        public byte base_hp;
        public byte base_atk;
        public byte base_def;
        public byte base_spd;
        public byte base_spatk;
        public byte base_spdef;
        public byte type1;
        public byte type2;
        public byte catch_rate;
        public byte exp_yield;
        /*
        short evs_hp : 2;
        short evs_atk : 2;
        short evs_def : 2;
        short evs_spd : 2;
        short evs_spatk : 2;
        short evs_spdef : 2;
        short unused : 4;
        */
        public ushort evs;
        public ushort item1;
        public ushort item2;
        public byte gender_ratio;
        public byte hatching;
        public byte friendship;
        public byte exp_curve;
        public byte egg_group1;
        public byte egg_group2;
        public byte ability1;
        public byte ability2;
        public byte safari_flee_rate;
        public byte dex_colour;
        public byte pad1;
        public byte pad2;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Wild_Poke
    {
        public byte minLvl;
        public byte maxLvl;
        public ushort spieces;
    }

    public class INI_init
    {
        public int start_offset;
        public int pokenum;
        public int pokebasestats;
        public int pokenames;
        public int items;
        public int itemnum;
        public int itemimages;
        public int abilities;
        public int ablitynum;
        public int tmhmcompabilities;
        public int tmhmnum;
        public int tmhmlist;
        public int toutorcompabilities;
        public int toutorlist;
        public int toutornum;
        public int moves;
        public int movenum;
        public byte tmhmsize;
        public byte toutorsize;
        public int frontspritetable;
        public int backspritetable;
        public int frontpalettetable;
        public int shinypalettetable;
        public int learnset;
        public int movedesc;
        public int moveinfo;
        public int trainerimg;
        public int trainerpal;
        public int trainernum;
		public int trainertatal;
        public int traineroffset;
        public int trainernumber;
        public int national_dex;
		public int icointable;
		public int icoinpallete;
		public int icoinindextable;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Item
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public byte[] name;
        public ushort index;
        public ushort price;
        public byte held_effect;
        public byte held_effect_quality;
        public uint desc_pointer;
        public ushort mystery_value;
        public byte pocket_no;
        public byte type_of_item;
        public uint field_usage_code;
        public uint battle_usage;
        public uint battle_usage_code;
        public uint extra_param;
    };
	
	public struct Move_Info
	{
        public byte script_id;
        public byte base_power;
        public byte type;
        public byte accuracy;
        public byte pp;
        public byte effect_chance;
        public byte target;
        public sbyte priority;
        public byte move_flags;
        public byte arg1;
        public byte split;
        public byte arg2;
	};

    public struct Battle_Frontier
    {
        public short poke;
        public short move1;
        public short move2;
        public short move3;
        public short move4;
        public byte item;
        public byte ev;
        public byte nature;
        public byte pad1;
        public short pad2;
    }
    //trainer_classname 0x30FCD4
	//奖金的指针在 0x4E6A8
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Trainer_Data
    {
        //u8 custom_moves : 1;
        //u8 custom_item : 1;
        public byte moves_item;//1:1
        public byte trainer_class;
        //u8 music : 7;
        //u8 gender : 1;
        public byte music_gender;//7:1
        public byte sprite;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] name;
        public byte field_E;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public short[] items;
        public byte double_battle;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] padd1;
        public int ai_scripts;//32位
        public byte poke_number;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] padd2;
        public int poke_data;
    };

	public struct TrainerPokeData
	{
		public byte evs_id;
		public byte filler;
		public byte level;
		public byte filler2;
		public short poke_id;
		public short item_id;
	};
	
	public struct Dex_Info
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
		public byte[] name;
		public ushort height;
		public ushort weight;
		public int text_pointer;
		public short filler1;
		public short poke_scale;
		public short poke_pos;
		public short role_scale;
		public short role_pos;
		public short filler2;
	}


	public struct npc_type
	{
		public short tiles_tag;
		public short pal_tag;
		public short pal_tag2;
		public short field_6;
		public short width; 
		public short height; 
		public byte field_C;
		public byte field_D;
		public byte field_E;
		public byte field_F;
		public int oam;
		public int formation;
		public int image_anims;
		public int gfx_table;//(size = x*y/2, pointer)
		public int rotscale_anims;
	}

	public  struct Evo {
		public byte method;
		public byte padd0;
		public ushort parameter;
		public short poke;
		public short padd1;
    }

    public class EvoMethods {
        public int method;
        public String desc;

        public int Method { get { return method; } }

        public String Desc { get { return desc; } }
    }
    public struct mapheader
    {
        public int map_footer;
        public int events;//2026698
        public int scripts;
        public int connections;
        public short music_id;
        public short footer_id;
        public byte name;//0x5a1480
        public byte light;
        public byte weather;
        public byte maptype;
        public byte field_18;
        public byte can_dig;
        public byte show_name;
        public byte battletype;
    };


    struct blockset
    {
        public byte compressed;
        public byte pal_mode_flag;
        public byte field2;
        public byte field3;
        public int tileset;
        public int pals;
        public int blockset_data;//每个2字节，一共8个16字节，后0x3ff blockid，前12位调色板id
        public int bg_bytes;
        public int anim_routine;
    };


    public struct footer
    {
        public int width_blocks;
        public int height_blocks;
        public int border_data;
        public int blocksID_movement;//后10位blockid 前6位行为字节
        public int primary_blockset;
        public int secondary_blockset;
    };

    public struct events
    {
        public byte npc_num;
        public byte entry_num;
        public byte script_num;
        public byte sign_num;
        public int rom_npc;//人物
        public int entry_scripts;//出入口
        public int script_scripts;//脚本
        public int sign_scripts;//标志
    }

    public struct entry_scripts
    {

    }
    public struct script_scripts
    {

    }
    public struct sign_scripts
    {

    }

    public struct rom_npc
    {
        public byte localID; //0
        public byte spriteID; //1
        public short field2; //2
        public short x_pos; //4
        public short y_pos; //6
        public byte height; //8
        public byte behaviour;//9
        public short behaviour_property;// xA
        public byte is_trainer; //xC
        public byte fieldD; //xD
        public short radius_plantID; //xE
        public int script; //x10
        public short flag; //x14
        public short field16; //x16
    }

    /// <summary>
    /// 用于结构体和字节数组的互换
    /// </summary>
    public static unsafe class StructsUtil
    {
		/// <summary>
		/// 字节数组转结构体
		/// </summary>
		/// <param name="bytes">字节数组</param>
		/// <param name="type">所需要转换的结构体的类型</param>
		/// <returns>object</returns>
        public static object ByteToStruct(byte[] bytes,Type type)
        {
            int size = Marshal.SizeOf(type);
            if (size > bytes.Length) {
                return null;
            }
            fixed (byte* b = bytes){ 
                return  Marshal.PtrToStructure(new IntPtr(b), type);
            }
        }
		/// <summary>
		/// 结构体转字节数组
		/// </summary>
		/// <param name="structure">需要转换的结构体</param>
		/// <returns>字节数组</returns>
        public static byte[] StructToByte(ValueType structure)
        {
            int size = Marshal.SizeOf(structure);
            byte[] bytes = new byte[size];
            fixed(byte* b = bytes) {
                IntPtr ptr = new IntPtr(b);
                Marshal.StructureToPtr(structure, ptr, false);
                return bytes;
            }
        }

    }
}
