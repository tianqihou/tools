using System;
using myDLL;
using System.Globalization;

namespace Tools {
	public class PokeConfig : ConfigParser {
		public String section;
		public PokeConfig(String path) : base(path) { }
		public static INI_init ini;
		static bool Compare(byte[] a, byte[] b) {
			for (int x = 0; x < 10; x++) {
				if (a[x] != b[x])
					return false;
			}
			return true;
		}

		public void Validate(ReadAndWrite rw) {
            /*
			rw.Seek(0xAC);
			byte[] data = rw.Br.ReadBytes(10);
			if (Compare(data, WordsDecoding.ULTRA))
				section = "802";
			else if (Compare(data, WordsDecoding.BPEE))
				section = "rom";
			else
				throw new Exception("unknown rom_id");
             */
            section = "802";
        }

		public String Get(String section, String key) {
			return Get(section, key, "");
		}

		public int GetAsInt(String key) {
			String value = Get(key).ToLower();
			if (value.Length == 0)
				throw new ArgumentException("未获取到"+key+", 请使用最新的ini");
			return ParseInt(value);
		}

		public String Get(String key) {
			return Get(section, key, "");
		}

		public static int ParseInt(String value) {
			if (value.StartsWith("0x"))
				return int.Parse(value.Substring(2), NumberStyles.AllowHexSpecifier);
			return int.Parse(value);
		}

		public static byte GetSize(int total) {
			byte size = 4;
			if (total > 32 && total <= 64)
				size = 8;
			else if (total <= 96)
				size = 12;
			else if (total <= 128)
				size = 16;
			else
				throw new ArgumentException("128");
			return size;
		}

		public void Init_ini(out INI_init ini) {
            PokeConfig.ini = new INI_init {
                start_offset = GetAsInt("起始空位"),
				abilities = GetAsInt("特性名"),
				ablitynum = GetAsInt("特性总量"),
				pokenum = GetAsInt("精灵总量"),
				pokebasestats = GetAsInt("基础值"),
				pokenames = GetAsInt("精灵名字"),
				items = GetAsInt("道具"),
				itemnum = GetAsInt("道具总量"),
				itemimages = GetAsInt("道具图片"),
				tmhmcompabilities = GetAsInt("TM学习面"),
				tmhmnum = GetAsInt("TM总量"),
				toutornum = GetAsInt("TOUTOR总量"),
				toutorsize = GetSize(GetAsInt("TOUTOR总量")),
				tmhmsize = GetSize(GetAsInt("TM总量")),
				tmhmlist = GetAsInt("TMLIST"),
				toutorcompabilities = GetAsInt("TOUTOR学习面"),
				toutorlist = GetAsInt("TOUTORLIST"),
				movenum = GetAsInt("技能总量"),
				moves = GetAsInt("技能名"),
				frontspritetable = GetAsInt("正面图"),
				backspritetable = GetAsInt("背面图"),
				frontpalettetable = GetAsInt("普通色板"),
				shinypalettetable = GetAsInt("闪光色板"),
				learnset = GetAsInt("升级技能"),
				movedesc = GetAsInt("技能描述"),
				moveinfo = GetAsInt("技能表"),
				trainerimg = GetAsInt("训练师图片"),
				trainerpal = GetAsInt("训练师调色板"),
				trainernum = GetAsInt("图片数量"),
				trainertatal = GetAsInt("训练师数量"),
				traineroffset = GetAsInt("训练师地址"),
				national_dex = GetAsInt("全国图鉴"),
				icoinindextable = GetAsInt("图标色板标号"),
				icoinpallete = GetAsInt("图标色板"),
				icointable = GetAsInt("图标"),
			};
            ini = PokeConfig.ini;
		}
	}

}