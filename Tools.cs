using System;
using Decompiler;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Drawing.Imaging;

namespace Tools
{
    delegate void Save_Delegate();

    public partial class MainWindow : Form
    {
        public static String rom_path;
        Poke_basestats BaseStats;
        INI_init ini;
        PokeConfig cp;
        Wild_Poke[] Wild_Poke_info;
        int[] Wild_Poke_offset = new int[4];
        Item item_info;
        Move_Info moveinfo;
        PokeLearnSet pls;
        int moveinfo_offset;
        PokeImg pokeimg;
        //开拓区道具编号
        short[] item_num;
        Save_Delegate[] saves;
        const String filter = "图片|*.png;*.bmp;*.jpg;*.dib";
        NpcType npc;
        Evolution evolution;
        public MainWindow()
        {
            InitializeComponent();
            wildmaptype.DataSource = WordsDecoding.wildmaptype;
            dataGrid_wildPoke.RowsDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            evoMethod.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            evoMethod.DisplayMember = "desc";
            evoMethod.ValueMember = "method";
            evoMethod.DataSource = WordsDecoding.evoMethods;
            evolution = new Evolution(gridEvo);
            evoPokeNames.SelectedIndexChanged += new EventHandler(evolution.setPokeIndex);
            pokeimg = new PokeImg(front, shinyfront, back, shinyback);
            pokePos = new PokePos(pictureBox3, pictureBox4, shadow);
            saves = new Save_Delegate[] {
            new Save_Delegate(Save_basestats) ,
            new Save_Delegate(Save_wildpokeinfo) ,
            new Save_Delegate(Save_iteminfo) ,
            new Save_Delegate(Save_TMSinfo) ,
            new Save_Delegate(Save_learn) ,
            new Save_Delegate(Save_trainer) ,
            new Save_Delegate(Save_bf) ,
            new Save_Delegate(Save_Pic),
                new Save_Delegate(SaveEvo),
                new Save_Delegate(SaveNpc)
            };
            npc = new NpcType();
        }

        //初始化所有数据
        void choose_MenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "gba文件（*.gba）|*.gba"
            };
            if (dialog.ShowDialog() == DialogResult.OK) {
                init(dialog.FileName);
            }
        }

        public void init(String file_name)
        {
            if (file_name.EndsWith(".gba") && File.Exists(file_name)) {
                cp = new PokeConfig(@"rom.ini");
                LoadEveryThing(file_name);
                cp.WriteValue("last_open", "last_open", file_name);
                SetAction();
            }
        }

        public void SetAction()
        {
            pokeName.SelectedIndexChanged +=
                new EventHandler(pokeName_SelectedIndexChanged);
            listBox_map.SelectedIndexChanged +=
                new EventHandler(listBox_map_SelectedIndexChanged);
            button1.Click += new EventHandler(button1_Click);
            wildmaptype.SelectedIndexChanged +=
                new EventHandler(wildmaptype_SelectedIndexChanged);
            comboBox_item_no.SelectedIndexChanged +=
                new EventHandler(comboBox_item_no_SelectedIndexChanged);
            button2.Click += new EventHandler(button2_Click);
            learn_move.SelectedIndexChanged +=
                new EventHandler(learn_move_SelectedIndexChanged);
            UpDown.ValueChanged +=
                new EventHandler(UpDown_ValueChanged);
            bf_nb.ValueChanged +=
                new EventHandler(nb_ValueChanged);
            trainer_num.SelectedIndexChanged +=
                new EventHandler(trainer_num_ValueChanged);
            bf_items.SelectedIndexChanged +=
                new EventHandler(bf_items_SelectedIndexChanged);
            PlayerY.ValueChanged +=
                new EventHandler(pokePos.ChangePlayerY);
            Altitude.ValueChanged +=
                new EventHandler(pokePos.ChangeEnemyAlt);
            EnemyY.ValueChanged +=
                new EventHandler(pokePos.ChangeEnemyY);
        }

        public void LoadEveryThing(String path)
        {

            pokeName.SelectedIndexChanged -=
                new EventHandler(pokeName_SelectedIndexChanged);
            listBox_map.SelectedIndexChanged -=
                new EventHandler(listBox_map_SelectedIndexChanged);
            button1.Click -= new EventHandler(button1_Click);
            wildmaptype.SelectedIndexChanged -=
                new EventHandler(wildmaptype_SelectedIndexChanged);
            comboBox_item_no.SelectedIndexChanged -=
                new EventHandler(comboBox_item_no_SelectedIndexChanged);
            button2.Click -= new EventHandler(button2_Click);
            learn_move.SelectedIndexChanged -=
                new EventHandler(learn_move_SelectedIndexChanged);
            UpDown.ValueChanged -=
                new EventHandler(UpDown_ValueChanged);
            bf_nb.ValueChanged -=
                new EventHandler(nb_ValueChanged);
            trainer_num.SelectedIndexChanged -=
                new EventHandler(trainer_num_ValueChanged);
            bf_items.SelectedIndexChanged -=
                new EventHandler(bf_items_SelectedIndexChanged);
            PlayerY.ValueChanged -=
                new EventHandler(pokePos.ChangePlayerY);
            Altitude.ValueChanged -=
                new EventHandler(pokePos.ChangeEnemyAlt);
            EnemyY.ValueChanged -=
                new EventHandler(pokePos.ChangeEnemyY);

            rom_path = path;
            pokeName.Enabled = true;
            PictureItem.Cursor = Cursors.Hand;
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            cp.Validate(rw);
            cp.Init_ini(out ini);
            ini.traineroffset = rw.ReadPointer(ini.traineroffset);

            //初始化精灵名字
            String[] str_PokeName = rw.GetObjectNames(ini.pokenames,
                    WordsDecoding.SIZE.pokename, ini.pokenum);
            pokeName.DataSource = str_PokeName;
            evoPokeNames.DataSource = str_PokeName.Clone();
            //初始化野怪的精灵名字
            comboBox_poke.DataSource = str_PokeName.Clone();
            //开拓区精灵初始化
            String[] str_PokeName3 = new string[ini.pokenum];
            str_PokeName.CopyTo(str_PokeName3, 0);
            bf_poke.DataSource = str_PokeName3;
            //训练师精灵
            String[] str_PokeName5 = new string[ini.pokenum];
            str_PokeName.CopyTo(str_PokeName5, 0);
            trainer_poke.DataSource = str_PokeName5;
            //全国图鉴编号初始化
            ini.national_dex = rw.ReadPointer(ini.national_dex);
            /*
			String[] str_PokeName4 = new string[803];
			rw.Br.BaseStream.Seek(ini.national_dex, SeekOrigin.Begin);
			for (int x = 1; x < 1200; x++)
			{
				short y = rw.Br.ReadInt16();
				if (y <= 802 && String.IsNullOrEmpty(str_PokeName4[y]))
				{
					str_PokeName4[y] = str_PokeName[x];
				}
			}
			national_index.DataSource = str_PokeName4;
			*/
            //初始化特性1
            String[] str_ability = rw.GetObjectNames(ini.abilities,
                WordsDecoding.SIZE.abilityname, ini.ablitynum);
            ability1.DataSource = str_ability;
            //初始化特性2
            ability2.DataSource = str_ability.Clone();
            ability3.DataSource = str_ability.Clone();
            //初始化道具1
            String[] str_item1 = rw.GetObjectNames(ini.items,
                WordsDecoding.SIZE.iteminfo, ini.itemnum);
            item1.DataSource = str_item1;
            //初始化道具2
            item2.DataSource = str_item1.Clone();
            //初始化道具3
            comboBox_item_no.DataSource = str_item1.Clone();
            //开拓区道具列表            
            bf_items.DataSource = str_item1.Clone();

            //训练师道具列表
            trainer_item1.DataSource = str_item1.Clone(); ;
            trainer_item2.DataSource = str_item1.Clone(); ;
            trainer_item3.DataSource = str_item1.Clone(); ;
            trainer_item4.DataSource = str_item1.Clone(); ;
            poke_item.DataSource = str_item1.Clone(); ;
            //训练师分组  0x30FCD4
            String[] str_trainerclass = rw.GetObjectNames(
                0x183B4, 13, 67);
            str_trainerclass[66] = "大魔王";
            trainer_class.DataSource = str_trainerclass;
            //初始化技能名
            String[] str_move = rw.GetObjectNames(ini.moves,
                WordsDecoding.SIZE.movename, ini.movenum);
            move_name.DataSource = str_move;
            learn_move.DataSource = str_move.Clone();
            //开拓区技能初始化
            bf_move1.DataSource = str_move.Clone();
            bf_move2.DataSource = str_move.Clone();
            bf_move3.DataSource = str_move.Clone();
            bf_move4.DataSource = str_move.Clone();
            //训练师精灵技能
            trainer_move1.DataSource = str_move.Clone();
            trainer_move2.DataSource = str_move.Clone();
            trainer_move3.DataSource = str_move.Clone();
            trainer_move4.DataSource = str_move.Clone();
            //开拓区性格
            bf_nature.DataSource = WordsDecoding.nature;
            //初始化TM和TOUTOR

            SetTMList(rw, str_move);
            //训练师编号,原来是 0x357
            for (int x = 0; x < ini.trainertatal; x++) {
                trainer_num.Items.Add("0x" + Convert.ToString(x, 16));
            }

            //其他
            type.DataSource = WordsDecoding.type;
            type1.DataSource = WordsDecoding.type1;
            type2.DataSource = WordsDecoding.type2;
            egg_group1.DataSource = WordsDecoding.egg_group1;
            egg_group2.DataSource = WordsDecoding.egg_group2;
            exp_yield.DataSource = WordsDecoding.exp_yield;
            dex_colour.DataSource = WordsDecoding.dex_color;
            //设置训练师数量的最大值
            UpDown.Maximum = new decimal(new int[] { ini.trainernum, 0, 0, 0 });
            //获取地图名
            listBox_map.DataSource = cp.Keys("map");
            //初始化ini地址
            ini.trainerimg = rw.ReadPointer(ini.trainerimg);
            ini.trainerpal = rw.ReadPointer(ini.trainerpal);

            //图片
            pokeimg.InIt(rw, ini);
            pokeimg.start = ini.start_offset;
            //位置
            pokePos.SetPos(rw);
            pokePos.SetPic(shinyfront, back);
            //learnSet
            pls = new PokeLearnSet(cp.section,
                rw.ReadPointer(ini.learnset), dataGrid_learn, str_move);
            //关闭流
            trainer_num.SelectedIndex = 0x291;

            rw.Close();
            pokeName_SelectedIndexChanged(null, null);

            //绑定action
            SetItemList();
            add.Enabled = true;
            t_poke_num.Enabled = true;
            Btn_Imoprt.Enabled = true;
            change_item.Enabled = true;
            nb_ValueChanged(null, null);

        }

        void SetTMList(ReadAndWrite rw, String[] str_move)
        {
            try {
                short[] tms = rw.GetHmOrToutorList(rw.ReadPointer(ini.tmhmlist),
                ini.tmhmnum);
                short[] toutors = rw.GetHmOrToutorList(
                    rw.ReadPointer(ini.toutorlist), ini.toutornum);
                TM.Items.Clear();
                TOUTOR.Items.Clear();
                foreach (short x in tms) {
                    TM.Items.Add(str_move[x], false);
                }

                foreach (short x in toutors) {
                    TOUTOR.Items.Add(str_move[x], false);
                }
            } catch (Exception) {
                return;
            }
        }

        //开拓区道具列表
        void SetItemList()
        {
            bf_itemlist.Items.Clear();
            FileStream fs = new FileStream(rom_path, FileMode.Open
                , FileAccess.ReadWrite, FileShare.ReadWrite);
            bf_itemlist.SelectedIndexChanged -=
                new EventHandler(bf_itemlist_SelectedIndexChanged);
            String[] source = (String[])bf_items.DataSource;
            item_num = new short[0x49];
            fs.Seek(0x5cecb0, SeekOrigin.Begin);
            byte[] buffer = new byte[146];
            fs.Read(buffer, 0, 146);
            Buffer.BlockCopy(buffer, 0, item_num, 0, 146);
            for (int x = 0; x < 0x49; x++) {
                bf_itemlist.Items.Add(source[item_num[x]]);
            }
            fs.Close();
            bf_itemlist.SelectedIndexChanged +=
                new EventHandler(bf_itemlist_SelectedIndexChanged);
        }

        void SetSize(bool isTwoframe)
        {
            Size size = front.Size;
            if (isTwoframe) {
                size.Height = 128;
            }
            else {
                size.Height = 64;
            }
            front.Size = size;
            shinyfront.Size = size;
        }
        PokePos pokePos;
        //精灵名字下拉框值发生改变时，加载基础信息
        void pokeName_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (ReadAndWrite rw = new ReadAndWrite(rom_path)) {
                byte[] tem = rw.ReadBytes(rw.ReadPointer(ini.pokebasestats),
                    pokeName.SelectedIndex, WordsDecoding.SIZE.basestats);
                BaseStats =
                (Poke_basestats)StructsUtil.ByteToStruct(tem, BaseStats.GetType());
                base_hp.Text = BaseStats.base_hp.ToString();
                base_def.Text = BaseStats.base_def.ToString();
                base_atk.Text = BaseStats.base_atk.ToString();
                base_spd.Text = BaseStats.base_spd.ToString();
                base_spatk.Text = BaseStats.base_spatk.ToString();
                base_spdef.Text = BaseStats.base_spdef.ToString();
                catch_rate.Text = BaseStats.catch_rate.ToString();
                gender_ratio.Text = BaseStats.gender_ratio.ToString();
                hatching.Text = BaseStats.hatching.ToString();
                friendship.Text = BaseStats.friendship.ToString();
                exp_curve.Text = BaseStats.exp_yield.ToString();
                safari_flee_rate.Text = BaseStats.safari_flee_rate.ToString();
                item1.SelectedIndex = BaseStats.item1;
                item2.SelectedIndex = BaseStats.item2;
                type1.SelectedIndex = BaseStats.type1;
                type2.SelectedIndex = BaseStats.type2;
                egg_group1.SelectedIndex = BaseStats.egg_group1;
                egg_group2.SelectedIndex = BaseStats.egg_group2;
                ability1.SelectedIndex = BaseStats.ability1;
                ability2.SelectedIndex = BaseStats.ability2;
                ability3.SelectedIndex = BaseStats.pad1;
                exp_yield.SelectedIndex = BaseStats.exp_curve;
                dex_colour.SelectedIndex = BaseStats.dex_colour;
                ushort evs = BaseStats.evs;
                evs_hp.Text = (evs & 3).ToString();
                evs_atk.Text = ((evs >> 2) & 3).ToString();
                evs_def.Text = ((evs >> 4) & 3).ToString();
                evs_spd.Text = ((evs >> 6) & 3).ToString();
                evs_spatk.Text = ((evs >> 8) & 3).ToString();
                evs_spdef.Text = ((evs >> 10) & 3).ToString();
                SetCompatibility(rw);
                //evolution
                evolution.setGridEvoInfo(rw, pokeName.SelectedIndex, (string[])pokeName.DataSource);
                //加载图片
                /*
                int img = rw.ReadPointer(ini.frontspritetable);
                img = rw.ReadPointer((pokeName.SelectedIndex<<3)+img);
                int palette = rw.ReadPointer(ini.frontpalettetable);
                palette = rw.ReadPointer( (pokeName.SelectedIndex<<3)+palette);
                ImgFunction imgfun = ImgFunction;
                Bitmap bm = new Bitmap(148, 64);
                //bm.MakeTransparent(Color.FromArgb(0xFF, 0xff, 0xff));
                Graphics g = Graphics.FromImage(bm);
                Bitmap bm1 = imgfun.GetImg(rw.Br, img, palette, 64, 64);                
                g.DrawImage(bm1, new Point(0, 0));
                img = rw.ReadPointer(ini.backspritetable);
                img = rw.ReadPointer( (pokeName.SelectedIndex<<3)+img);
                palette = rw.ReadPointer(ini.shinypalettetable);
                palette = rw.ReadPointer( (pokeName.SelectedIndex<<3)+palette);
                g.DrawImage(imgfun.GetImg(rw.Br, img, palette, 64, 64
                    ), new Point(79, 0));
                pokeImg.Image = bm;
                g.Dispose();
				*/
                pokeimg.index = pokeName.SelectedIndex;
                pokeimg.LoadFromGBA(rw);
                SetSize(pokeimg.isTwoframe);
                panel4.Refresh();
                pokePos.GetPos(rw, pokeName.SelectedIndex);
                EnemyY.Value = pokePos.pos[1];
                PlayerY.Value = pokePos.pos[0];
                Altitude.Value = pokePos.pos[2];
                pokePos.RePaint();
                //icoin
                iconbox.Image = pokeimg.GetIcoin(rw);
                //dex
                rw.Br.BaseStream.Seek(ini.national_dex + ((pokeName.SelectedIndex - 1) << 1)
                    , SeekOrigin.Begin);
                short dex = rw.Br.ReadInt16();
                national_index.Text = dex.ToString();
                textBox2.Text = pokeName.Text;
                //加载升级技能
                if ("rom".Equals(cp.section))
                    return;
                pls.SetLearnSet(rw, pokeName.SelectedIndex);
            }
        }


        //保存
        void button1_Click(object sender, EventArgs e)
        {
            saves[DataEditor.SelectedIndex]();
        }

        void Save_Pic()
        {
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            pokePos.SavePos(rw, pokeimg.index);
            try {
                pokeimg.SaveAll(rw, checkBox3.Checked);

            } catch (Exception es) {
                MessageBox.Show(es.ToString());
            } finally { rw.Close(); }
        }

        void SaveEvo()
        {
            using (ReadAndWrite rw = new ReadAndWrite(rom_path)) {
                evolution.Save(rw, pokeName.SelectedIndex);
            };
        }

        void Save_bf()
        {
            bf_struct.poke = (short)bf_poke.SelectedIndex;
            bf_struct.move1 = (short)bf_move1.SelectedIndex;
            bf_struct.move2 = (short)bf_move2.SelectedIndex;
            bf_struct.move3 = (short)bf_move3.SelectedIndex;
            bf_struct.move4 = (short)bf_move4.SelectedIndex;
            bf_struct.item = (byte)bf_itemlist.SelectedIndex;
            bf_struct.nature = (byte)bf_nature.SelectedIndex;
            int ev = 0;
            for (int x = 0; x < 6; x++) {
                if (bf_ev.GetItemChecked(x))
                    ev |= (1 << (7 - x));
            }
            bf_struct.ev = (byte)ev;
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            rw.Save(0x5D97BC, (int)bf_nb.Value, bf_struct);

            rw.Close();
            nb_ValueChanged(null, null);
        }

        void Save_learn()
        {
            FileStream fs = new FileStream(rom_path, FileMode.Open
                , FileAccess.ReadWrite, FileShare.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fs);
            pls.Save(bw);
            //保存技能信息
            moveinfo.script_id = byte.Parse(script_id.Text);
            moveinfo.base_power = byte.Parse(base_power.Text);
            moveinfo.accuracy = byte.Parse(accuracy.Text);
            moveinfo.pp = byte.Parse(pp.Text);
            moveinfo.effect_chance = byte.Parse(effect_chance.Text);
            moveinfo.priority = sbyte.Parse(priority.Text);
            moveinfo.arg1 = byte.Parse(arg1.Text);
            moveinfo.arg2 = byte.Parse(arg2.Text);
            moveinfo.split = (byte)split.SelectedIndex;
            moveinfo.type = (byte)type.SelectedIndex;
            moveinfo.target = WordsDecoding.target[target.SelectedIndex];
            int z = 0;
            for (int y = 0; y < 7; y++) {
                if (move_flag.GetItemChecked(y))
                    z = z | (1 << y);
            }
            moveinfo.move_flags = (byte)z;
            byte[] data = StructsUtil.StructToByte(moveinfo);
            bw.Seek(moveinfo_offset + 12 * learn_move.SelectedIndex, SeekOrigin.Begin);
            bw.Write(data);
            bw.Close();
        }

        void Save_TMSinfo()
        {
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            byte[] data = GetChangedCompability(TM, ini.tmhmsize);
            int offset = rw.ReadPointer(ini.tmhmcompabilities);
            rw.WriteBytes(offset,
                pokeName.SelectedIndex, data);

            offset = rw.ReadPointer(ini.toutorcompabilities);
            data = GetChangedCompability(TOUTOR, ini.toutorsize);
            rw.WriteBytes(offset,
                pokeName.SelectedIndex, data);
            rw.Close();
        }

        byte[] GetChangedCompability(CheckedListBox clb, int Size)
        {
            String bin = "";
            for (int x = 0; x < clb.Items.Count; x++) {
                if (clb.GetItemChecked(x))
                    bin += "1";
                else
                    bin += "0";
            }
            bin = bin.PadLeft(Size * 8, '0');
            byte[] data = new byte[Size];
            for (int x = 0; x < Size; x++) {
                data[x] = Convert.ToByte(bin.Substring(x * 8, 8), 2);
            }
            Array.Reverse(data);
            return data;
        }

        void Save_iteminfo()
        {
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            try {
                item_info.battle_usage = uint.Parse(
                    textBox_battle_usage.Text,
                    NumberStyles.AllowHexSpecifier);
                item_info.battle_usage_code = uint.Parse(
                    textBox_battle_usage_code.Text,
                    NumberStyles.AllowHexSpecifier);
                item_info.desc_pointer = uint.Parse(
                    textBox_desc_pointer.Text,
                    NumberStyles.AllowHexSpecifier);
                item_info.extra_param = uint.Parse(
                    textBox_extra_param.Text, NumberStyles.AllowHexSpecifier);
                item_info.field_usage_code = uint.Parse(
                    textBox_field_usage_code.Text,
                    NumberStyles.AllowHexSpecifier);
                item_info.held_effect = byte.Parse(textBox_held_effect.Text);
                item_info.held_effect_quality = byte.Parse(
                    textBox_held_effect_quality.Text);
                item_info.index = ushort.Parse(textBox_index.Text);
                item_info.pocket_no = byte.Parse(textBox_poket.Text);
                item_info.price = ushort.Parse(textBox_price.Text);
                item_info.type_of_item = byte.Parse(textBox_type.Text);
                byte[] name = ReadAndWrite.Encode(textBox_name.Text, 13);
                Buffer.BlockCopy(name, 0, item_info.name, 0, name.Length);
                if (item_info.battle_usage_code != 0)
                    item_info.battle_usage_code |= 1;
                if (item_info.field_usage_code != 0)
                    item_info.field_usage_code |= 1;
                rw.Save(rw.ReadPointer(ini.items),
                    comboBox_item_no.SelectedIndex, item_info);
            } catch (Exception es) {
                MessageBox.Show(es.ToString());
                return;
            } finally {
                rw.Close();
            }
        }

        void Save_wildpokeinfo()
        {
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            int offset = Wild_Poke_offset[wildmaptype.SelectedIndex];
            for (int x = 0; x < Wild_Poke_info.Length; x++) {
                rw.Save(offset, x, Wild_Poke_info[x]);
            }
            rw.Close();
            wildmaptype_SelectedIndexChanged(null, null);
        }

        void Save_basestats()
        {
            using (ReadAndWrite rw = new ReadAndWrite(rom_path)) {
                BaseStats.base_hp = byte.Parse(base_hp.Text);
                BaseStats.base_atk = byte.Parse(base_atk.Text);
                BaseStats.base_def = byte.Parse(base_def.Text);
                BaseStats.base_spd = byte.Parse(base_spd.Text);
                BaseStats.base_spatk = byte.Parse(base_spatk.Text);
                BaseStats.base_spdef = byte.Parse(base_spdef.Text);
                BaseStats.catch_rate = byte.Parse(catch_rate.Text);
                BaseStats.exp_yield = byte.Parse(exp_curve.Text);
                BaseStats.gender_ratio = byte.Parse(gender_ratio.Text);
                BaseStats.hatching = byte.Parse(hatching.Text);
                BaseStats.friendship = byte.Parse(friendship.Text);
                BaseStats.safari_flee_rate = byte.Parse(safari_flee_rate.Text);
                BaseStats.type1 = (byte)type1.SelectedIndex;
                BaseStats.type2 = (byte)type2.SelectedIndex;

                BaseStats.egg_group1 = (byte)egg_group1.SelectedIndex;
                BaseStats.egg_group2 = (byte)egg_group2.SelectedIndex;
                BaseStats.ability1 = (byte)ability1.SelectedIndex;
                BaseStats.ability2 = (byte)ability2.SelectedIndex;
                BaseStats.pad1 = (byte)ability3.SelectedIndex;
                BaseStats.dex_colour = (byte)dex_colour.SelectedIndex;
                BaseStats.exp_curve = (byte)exp_yield.SelectedIndex;
                BaseStats.item1 = (ushort)item1.SelectedIndex;
                BaseStats.item2 = (ushort)item2.SelectedIndex;
                int evs = 0;
                evs = evs | (byte.Parse(evs_spdef.Text) > 3 ?
                    3 : byte.Parse(evs_spdef.Text));
                evs = (evs << 2) | (byte.Parse(evs_spatk.Text) > 3 ?
                    3 : byte.Parse(evs_spatk.Text));
                evs = (evs << 2) | (byte.Parse(evs_spd.Text) > 3 ?
                    3 : byte.Parse(evs_spd.Text));
                evs = (evs << 2) | (byte.Parse(evs_def.Text) > 3 ?
                    3 : byte.Parse(evs_def.Text));
                evs = (evs << 2) | (byte.Parse(evs_atk.Text) > 3 ?
                    3 : byte.Parse(evs_atk.Text));
                evs = (evs << 2) | (byte.Parse(evs_hp.Text) > 3 ?
                    3 : byte.Parse(evs_hp.Text));
                BaseStats.evs = (ushort)evs;
                rw.Save(rw.ReadPointer(ini.pokebasestats), pokeName.SelectedIndex, BaseStats);
                rw.Bw.BaseStream.Seek(ini.national_dex + ((pokeName.SelectedIndex - 1) << 1)
                    , SeekOrigin.Begin);
                rw.Bw.Write(short.Parse(national_index.Text));
                
                pokeName_SelectedIndexChanged(null, null);
            }
        }
        //添加野怪
        void listBox_map_SelectedIndexChanged(object sender, EventArgs e)
        {
            String value = listBox_map.Text;
            String[] offsets = cp.Get("map", value).Split(',');
            if (offsets.Length != 4)
                throw new IndexOutOfRangeException("地图地址未正确添加！");
            for (int x = 0; x < 4; x++) {
                Wild_Poke_offset[x] = int.Parse(offsets[x].Substring(2),
                    NumberStyles.AllowHexSpecifier);
            }
            wildmaptype_SelectedIndexChanged(sender, e);
        }

        //根据选中的地图类型添加野怪到表格里
        void wildmaptype_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGrid_wildPoke.Rows.Clear();
            int offset = Wild_Poke_offset[wildmaptype.SelectedIndex];
            if (offset == 0)
                return;
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            String[] chance = WordsDecoding.percents[wildmaptype.SelectedIndex];
            int length = chance.Length;
            Wild_Poke_info = new Wild_Poke[length];
            Type type = Wild_Poke_info[0].GetType();
            for (int x = 0; x < length; x++) {
                byte[] tem = rw.ReadBytes(offset, x, 4);
                Wild_Poke_info[x] =
                    (Wild_Poke)StructsUtil.ByteToStruct(tem, type);
            }
            rw.Close();
            String[] strs = (String[])comboBox_poke.DataSource;
            foreach (String str in chance) {
                int index = dataGrid_wildPoke.Rows.Add();
                dataGrid_wildPoke.Rows[index].Cells[0].Value =
                    Wild_Poke_info[index].minLvl;
                dataGrid_wildPoke.Rows[index].Cells[1].Value =
                    Wild_Poke_info[index].maxLvl;
                dataGrid_wildPoke.Rows[index].Cells[2].Value =
                    WordsDecoding.PreFixString(strs, Wild_Poke_info[index].spieces);
                dataGrid_wildPoke.Rows[index].Cells[3].Value = str;
            }
            //列（1）、行（0）
            dataGrid_wildPoke.CurrentCell = dataGrid_wildPoke[1, 0];
        }

        void dataGrid_wildPoke_CellEnter(object sender,
            DataGridViewCellEventArgs e)
        {
            if (dataGrid_wildPoke.Rows.Count < 5)
                return;
            String[] str = (String[])comboBox_poke.DataSource;
            try {
                textBox_minlvl.Text =
                dataGrid_wildPoke.Rows[e.RowIndex].Cells[0].Value.ToString();
                textBox_maxlvl.Text =
                dataGrid_wildPoke.Rows[e.RowIndex].Cells[1].Value.ToString();
                comboBox_poke.SelectedIndex = Wild_Poke_info[e.RowIndex].spieces;
            } catch (Exception es) {
                MessageBox.Show(es.ToString());
            }
        }

        void button2_Click(object sender, EventArgs e)
        {
            String[] strs = (String[])comboBox_poke.DataSource;
            int index = dataGrid_wildPoke.CurrentRow.Index;
            try {
                Wild_Poke_info[index].minLvl =
                    byte.Parse(textBox_minlvl.Text);
                Wild_Poke_info[index].maxLvl =
                    byte.Parse(textBox_maxlvl.Text);
                Wild_Poke_info[index].spieces =
                    (ushort)comboBox_poke.SelectedIndex;
            } catch (Exception es) {
                MessageBox.Show(es.ToString());
            }
            dataGrid_wildPoke.Rows[index].Cells[0].Value =
                Wild_Poke_info[index].minLvl;
            dataGrid_wildPoke.Rows[index].Cells[1].Value =
                Wild_Poke_info[index].maxLvl;
            dataGrid_wildPoke.Rows[index].Cells[2].Value =
                strs[Wild_Poke_info[index].spieces];
        }
        //道具信息
        void comboBox_item_no_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            byte[] tem = rw.ReadBytes(rw.ReadPointer(ini.items),
                comboBox_item_no.SelectedIndex, WordsDecoding.SIZE.iteminfo);
            item_info = (Item)StructsUtil.ByteToStruct(tem, item_info.GetType());
            textBox_battle_usage.Text =
                Convert.ToString(item_info.battle_usage, 16);
            textBox_battle_usage_code.Text =
                Convert.ToString(item_info.battle_usage_code, 16);
            textBox_desc_pointer.Text =
                Convert.ToString(item_info.desc_pointer, 16);
            textBox_extra_param.Text =
                Convert.ToString(item_info.extra_param, 16);
            textBox_field_usage_code.Text =
                Convert.ToString(item_info.field_usage_code, 16);
            textBox_held_effect.Text = item_info.held_effect.ToString();
            textBox_held_effect_quality.Text =
                item_info.held_effect_quality.ToString();
            textBox_index.Text = item_info.index.ToString();
            textBox_name.Text = ReadAndWrite.TransToCH(item_info.name);
            textBox_poket.Text = item_info.pocket_no.ToString();
            textBox_price.Text = item_info.price.ToString();
            textBox_type.Text = item_info.type_of_item.ToString();
            //描述
            byte[] buffer = rw.ReadBytes(
               (int)item_info.desc_pointer - 0x8000000, 0, 100);
            item_desc.Text = ReadAndWrite.TransToCH(buffer);

            //加载图片
            Bitmap bm = GetItemImg(rw);
            rw.Close();
            PictureItem.Image = bm;
        }

        Bitmap GetItemImg(ReadAndWrite rw)
        {
            int offset = ini.itemimages;
            offset = rw.ReadPointer(offset);
            int offset_img = rw.ReadPointer(
                offset + (comboBox_item_no.SelectedIndex << 3));
            int offset_pal = rw.ReadPointer(
                offset + (comboBox_item_no.SelectedIndex << 3) + 4);
            item_desc.Text += "\n图片地址：" + Operation.Hex(offset_img) +
                "\n调色板地址：" + Operation.Hex(offset_pal);
            Bitmap bm = ImgFunction.GetImg(rw.Br, offset_img,
                offset_pal, 24, 24);
            return bm;
        }

        void Change_item_img(Bitmap bm, bool clear)
        {
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            int old_offset = rw.ReadPointer(ini.itemimages);
            Tuple<byte[], byte[]> data = ImgFunction.ConvertNormalImagToGBA(bm);
            int old_pointer = old_offset + (comboBox_item_no.SelectedIndex << 3);
            pokeimg.Save(rw, data.Item1, old_pointer, clear);
            pokeimg.Save(rw, data.Item2, old_pointer + 4, clear);
            rw.Close();
            comboBox_item_no_SelectedIndexChanged(null, null);
        }

        void SetCompatibility(ReadAndWrite rw)
        {
            if (TM.Items.Count == 0 || TOUTOR.Items.Count == 0)
                return;
            //显示TM和HM的学习面
            try {
                int offset = rw.ReadPointer(ini.tmhmcompabilities);
                char[] compatibility = rw.GetCompatibility(offset,
                    ini.tmhmsize, ini.tmhmnum, pokeName.SelectedIndex);
                for (int i = 0; i < TM.Items.Count; i++) {
                    if (compatibility[i] == '1')
                        TM.SetItemChecked(i, true);
                    else
                        TM.SetItemChecked(i, false);
                }

                //定点教学
                offset = rw.ReadPointer(ini.toutorcompabilities);
                compatibility = rw.GetCompatibility(offset,
                    ini.toutorsize, ini.toutornum, pokeName.SelectedIndex);
                for (int i = 0; i < TOUTOR.Items.Count; i++) {
                    if (compatibility[i] == '1')
                        TOUTOR.SetItemChecked(i, true);
                    else
                        TOUTOR.SetItemChecked(i, false);
                }
            } catch (Exception es) {
                MessageBox.Show(es.ToString());
            }

        }

        void TM_SelectedIndexChanged(object sender, EventArgs e)
        {
            move_name.Text = TM.GetItemText(
                TM.Items[TM.SelectedIndex]);
        }

        void TOUTOR_SelectedIndexChanged(object sender, EventArgs e)
        {
            move_name.Text = TOUTOR.GetItemText(
                TOUTOR.Items[TOUTOR.SelectedIndex]);
        }

        void dataGrid_learn_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGrid_learn.Rows.Count < 3)
                return;
            learn_lvl.Text =
                dataGrid_learn.Rows[e.RowIndex].Cells[0].Value.ToString();
            learn_move.Text =
                dataGrid_learn.Rows[e.RowIndex].Cells[1].Value.ToString();
        }

        void button3_Click(object sender, EventArgs e)
        {
            if (dataGrid_learn.Rows.Count < 1)
                return;
            int index = dataGrid_learn.CurrentRow.Index;
            dataGrid_learn.Rows[index].Cells[0].Value = learn_lvl.Text;
            dataGrid_learn.Rows[index].Cells[1].Value = learn_move.Text;
            byte lvl = byte.Parse(learn_lvl.Text);
            if (lvl > 100)
                lvl = 100;
            pls.Change(index, lvl, (short)learn_move.SelectedIndex);
        }

        void learn_move_SelectedIndexChanged(object sender, EventArgs e)
        {
            Set_Move_Info(learn_move.SelectedIndex);
        }

        void Set_Move_Info(int index)
        {
            if (index == 0)
                return;
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            moveinfo_offset = rw.ReadPointer(ini.movedesc);
            moveinfo_offset =
                rw.ReadPointer(moveinfo_offset + 4 * index - 4);
            move_desc.Text = ReadAndWrite.TransToCH(rw.ReadBytes(moveinfo_offset, 0, 50));
            moveinfo_offset = rw.ReadPointer(ini.moveinfo);
            byte[] data = rw.ReadBytes(moveinfo_offset, index, 12);
            moveinfo = (Move_Info)StructsUtil.ByteToStruct(data,
                moveinfo.GetType());
            script_id.Text = moveinfo.script_id.ToString();
            base_power.Text = moveinfo.base_power.ToString();
            accuracy.Text = moveinfo.accuracy.ToString();
            pp.Text = moveinfo.pp.ToString();
            effect_chance.Text = moveinfo.effect_chance.ToString();
            priority.Text = moveinfo.priority.ToString();
            arg1.Text = moveinfo.arg1.ToString();
            arg2.Text = moveinfo.arg2.ToString();
            type.SelectedIndex = moveinfo.type;
            split.SelectedIndex = moveinfo.split;
            for (int x = 0; x < 7; x++) {
                if (WordsDecoding.target[x] == moveinfo.target) {
                    target.SelectedIndex = x;
                    break;
                }
            }
            int y = moveinfo.move_flags;
            for (int x = 0; x < 7; x++) {
                if ((y & 1) == 1) {
                    move_flag.SetItemChecked(x, true);
                }
                else {
                    move_flag.SetItemChecked(x, false);
                }
                y >>= 1;
            }
            rw.Close();
        }
        Tuple<byte[], byte[]> new_img_data;

        void pictureBox_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = filter
            };

            if (dialog.ShowDialog() == DialogResult.OK) {
                Bitmap b = new Bitmap(dialog.FileName);
                if (b.Width < 64 || b.Height < 64) {
                    MessageBox.Show("图片大小不符合");
                    return;
                }
                Bitmap m = b.Clone(new Rectangle(0, 0, 64, 64),
                    b.PixelFormat);
                pictureBox2.Image = m;
                new_img_data = ImgFunction.ConvertNormalImagToGBA(m);
                StringBuilder sb = new StringBuilder("图片大小：0x");
                sb.Append(Convert.ToString(new_img_data.Item1.Length, 16));
                sb.Append("\n调色板大小：0x");
                sb.Append(Convert.ToString(new_img_data.Item2.Length, 16));
                sb.Append("\n起始搜索地址：0x");
                sb.Append(Convert.ToString(ini.start_offset, 16));
                import_info.Text = sb.ToString();
            }
        }
        //加载训练师图片
        void UpDown_ValueChanged(object sender, EventArgs e)
        {
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            try {
                int index = (int)UpDown.Value;
                int Img_offset = rw.ReadPointer(ini.trainerimg + (index << 3));
                int pal_offset = rw.ReadPointer(ini.trainerpal + (index << 3));
                Bitmap bm = ImgFunction.GetImg(rw.Br, Img_offset, pal_offset, 64, 64);
                pictureBox1.Image = bm;
            } catch (Exception es) {
                MessageBox.Show("出问题了，但是仍然可以继续：" + es.ToString());
            } finally {
                rw.Close();
            }

        }

        void Btn_Imoprt_Click(object sender, EventArgs e)
        {
            if (new_img_data == null)
                return;
            ReadAndWrite rw = null;
            try {
                rw = new ReadAndWrite(rom_path);
                int index = ((int)UpDown.Value) << 3;
                //图片，读取图片地址，填充FF，寻找新空位，写入新指针，写入新数据
                int offset = ini.trainerimg + index;
                int start_offset = pokeimg.Save(rw, new_img_data.Item1, offset, (checkBox2.Checked && offset > 0));
                StringBuilder text = new StringBuilder("导入成功！\n图片地址：0x");
                text.Append(Convert.ToString(start_offset, 16)).Append("\n调色板地址：0x");
                offset = ini.trainerpal + index;
                start_offset = pokeimg.Save(rw, new_img_data.Item2, offset, (checkBox2.Checked && offset > 0));
                text.Append(Convert.ToString(start_offset, 16));
                ini.start_offset = rw.Position;
                import_info.Text = text.ToString();
            } catch (Exception es) {
                MessageBox.Show(es.ToString());
            } finally {
                rw.Close();
            }
            UpDown_ValueChanged(null, null);
        }

        //开拓区:0x5D97BC 道具列表:5cecb0 0x49个
        Battle_Frontier bf_struct;
        void nb_ValueChanged(object sender, EventArgs e)
        {
            bf_move1.SelectedIndexChanged -=
                new EventHandler(bf_move1_SelectedIndexChanged);
            bf_move2.SelectedIndexChanged -=
                new EventHandler(bf_move1_SelectedIndexChanged);
            bf_move3.SelectedIndexChanged -=
                new EventHandler(bf_move1_SelectedIndexChanged);
            bf_move4.SelectedIndexChanged -=
                new EventHandler(bf_move1_SelectedIndexChanged);
            //int offset = ((int)bf_nb.Value << 4) + 0x5D97BC;
            ReadAndWrite rw = null;
            try {
                rw = new ReadAndWrite(rom_path);
                byte[] bf_data = rw.ReadBytes(0x5D97BC, (int)bf_nb.Value, 16);
                bf_struct = (Battle_Frontier)StructsUtil.ByteToStruct(
                    bf_data, bf_struct.GetType());
                bf_poke.SelectedIndex = bf_struct.poke;
                bf_move1.SelectedIndex = bf_struct.move1;
                bf_move2.SelectedIndex = bf_struct.move2;
                bf_move3.SelectedIndex = bf_struct.move3;
                bf_move4.SelectedIndex = bf_struct.move4;
                bf_nature.SelectedIndex = bf_struct.nature;
                for (int x = 0; x < 6; x++) {
                    if ((bf_struct.ev >> (7 - x) & 1) == 1) {
                        bf_ev.SetItemChecked(x, true);
                    }
                    else {
                        bf_ev.SetItemChecked(x, false);
                    }
                }
            } catch (Exception es) {
                MessageBox.Show(es.ToString());
            } finally {
                rw.Close();
            }
            bf_move1.SelectedIndexChanged +=
                new EventHandler(bf_move1_SelectedIndexChanged);
            bf_move2.SelectedIndexChanged +=
                new EventHandler(bf_move1_SelectedIndexChanged);
            bf_move3.SelectedIndexChanged +=
                new EventHandler(bf_move1_SelectedIndexChanged);
            bf_move4.SelectedIndexChanged +=
                new EventHandler(bf_move1_SelectedIndexChanged);
            bf_itemlist.SelectedIndex = bf_struct.item;
        }

        //修改开拓区道具
        void change_item_Click(object sender, EventArgs e)
        {
            item_num[bf_itemlist.SelectedIndex] =
                (short)bf_items.SelectedIndex;
            FileStream fs = new FileStream(rom_path, FileMode.Open
                , FileAccess.ReadWrite, FileShare.ReadWrite);
            byte[] item = new byte[146];
            Buffer.BlockCopy(item_num, 0, item, 0, 0x49 * 2);
            fs.Seek(0x5cecb0, SeekOrigin.Begin);
            fs.Write(item, 0, 146);
            fs.Close();
            SetItemList();
            bf_itemlist.SelectedIndex = bf_struct.item;
        }
        //更新训练师信息
        Trainer_Data trainers;
        void trainer_num_ValueChanged(object sender, EventArgs e)
        {
            ReadAndWrite rw = null;
            try {
                rw = new ReadAndWrite(rom_path);
                byte[] data = rw.ReadBytes(ini.traineroffset
                    , trainer_num.SelectedIndex, 40);
                //获取训练师的数据
                trainers = (Trainer_Data)StructsUtil.ByteToStruct(data,
                    trainers.GetType());
                custom_move.Enabled = trainers.poke_number == 0;
                trainer_name.Text = ReadAndWrite.TransToCH(trainers.name);
                trainer_class.SelectedIndex = trainers.trainer_class;
                music.Value = trainers.music_gender & 127;
                custom_move.Checked = (trainers.moves_item & 1) == 1;
                if ((trainers.moves_item & 2) == 2) {
                    custom_item.Checked = true;
                    trainer_item1.SelectedIndex = trainers.items[0];
                    trainer_item2.SelectedIndex = trainers.items[1];
                    trainer_item3.SelectedIndex = trainers.items[2];
                    trainer_item4.SelectedIndex = trainers.items[3];
                }
                else {
                    custom_item.Checked = false;
                    trainer_item1.SelectedIndex = trainers.items[0];
                    trainer_item2.SelectedIndex = trainers.items[1];
                    trainer_item3.SelectedIndex = trainers.items[2];
                    trainer_item4.SelectedIndex = trainers.items[3];
                }
                male.Checked = (trainers.music_gender >> 7 & 1) == 0;
                female.Checked = !male.Checked;
                double_battle.Checked = trainers.double_battle == 1;
            } catch (Exception es) {
                MessageBox.Show(es.ToString());
            } finally {
                rw.Close();
            }
            UpDown.Value = trainers.sprite;
            groupBox11.Text = "精灵信息： " + trainers.poke_number + "个"
                + " 地址：" + Operation.Hex(trainers.poke_data);
            t_poke_num.Value = 1;
            t_poke_num_ValueChanged(null, null);
        }
        //保存训练师信息
        void Save_trainer()
        {
            trainers.music_gender =
                (byte)((int)music.Value | (male.Checked ? 0 : 128));
            trainers.sprite = (byte)UpDown.Value;
            trainers.trainer_class = (byte)trainer_class.SelectedIndex;
            trainers.items[0] = (short)trainer_item1.SelectedIndex;
            trainers.items[1] = (short)trainer_item2.SelectedIndex;
            trainers.items[2] = (short)trainer_item3.SelectedIndex;
            trainers.items[3] = (short)trainer_item4.SelectedIndex;
            trainers.moves_item = (byte)((custom_move.Checked ? 1 : 0)
                | (custom_item.Checked ? 2 : 0));
            trainers.ai_scripts = 0xff;
            trainers.double_battle = double_battle.Checked ? (byte)1 : (byte)0;
            byte[] name = ReadAndWrite.Encode(trainer_name.Text, 10);
            Buffer.BlockCopy(name, 0, trainers.name, 0, name.Length);
            FileStream fs = new FileStream(rom_path, FileMode.Open
                , FileAccess.ReadWrite, FileShare.ReadWrite);
            fs.Seek(ini.traineroffset +
                trainer_num.SelectedIndex * 40, SeekOrigin.Begin);
            fs.Write(StructsUtil.StructToByte(trainers), 0, 40);
            fs.Close();
            trainer_num_ValueChanged(null, null);
        }

        void custom_item_CheckedChanged(object sender, EventArgs e)
        {
            groupBox10.Enabled = custom_item.Checked;
        }

        void bf_move1_SelectedIndexChanged(object sender, EventArgs e)
        {
            MyComboBox mb = sender as MyComboBox;
            Set_Move_Info(mb.SelectedIndex);
            label8.Text = new StringBuilder("属性：").Append(type.Text).Append("\n")
                .Append("威力：").Append(base_power.Text).Append("\n").Append(
                "物特：").Append(split.Text).Append("\n").Append(
                "描述：").Append(move_desc.Text).Append("\n").ToString();
        }

        void bf_itemlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            bf_items.SelectedIndex = item_num[bf_itemlist.SelectedIndex];
        }


        /// <summary>  
        /// 开拓区道具下拉框修改时，显示道具信息提示文本 
        /// </summary>
        void bf_items_SelectedIndexChanged(object sender, EventArgs e)
        {
            int item = bf_items.SelectedIndex;
            ReadAndWrite fs = new ReadAndWrite(rom_path);
            int offset = fs.ReadPointer(ini.items);
            offset = fs.ReadPointer(offset +
                WordsDecoding.SIZE.iteminfo * item + 20);
            byte[] data = new byte[100];
            fs.fs.Seek(offset, SeekOrigin.Begin);
            fs.Br.Read(data, 0, 100);
            label12.Text = ReadAndWrite.TransToCH(data).Replace("\n", "");
            fs.Close();
        }

        /// <summary>  
        /// 导入道具图片  
        /// </summary>
        void PictureItem_Click(object sender, EventArgs e)
        {
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            int offset = rw.ReadPointer(ini.itemimages);
            offset = rw.ReadPointer(offset +
                (comboBox_item_no.SelectedIndex << 3));
            rw.Close();
            bool clear = false;
            if (offset != 0xDAB058)
                clear = true;
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = filter
            };

            if (dialog.ShowDialog() == DialogResult.OK) {
                Bitmap bm = new Bitmap(dialog.FileName);
                if (bm.Width == 24 && bm.Height == 24)
                    Change_item_img(bm, clear);
                else
                    bm.Dispose();
            }
        }

        void button4_Click(object sender, EventArgs e)
        {
            if (rom_path == null)
                return;
            FileStream fs = new FileStream(rom_path, FileMode.Open
                , FileAccess.ReadWrite, FileShare.ReadWrite);
            BinaryReader br = new BinaryReader(fs);
            StreamWriter sw = new StreamWriter(
                new FileStream("ani.s", FileMode.Create));
            try {
                new Move_Ainmation(br, sw).Start(learn_move.SelectedIndex,
                    learn_move.Text);
                Process.Start("notepad++.exe", "ani.s");
            } catch (System.ComponentModel.Win32Exception) { } catch (Exception es) {
                MessageBox.Show(es.ToString());
            } finally {
                br.Close();
                sw.Close();
            }

        }

        void button5_Click(object sender, EventArgs e)
        {
            if (rom_path == null)
                return;
            FileStream fs = new FileStream(rom_path, FileMode.Open
                , FileAccess.ReadWrite, FileShare.ReadWrite);
            BinaryReader br = new BinaryReader(fs);
            StreamWriter sw = new StreamWriter(new FileStream("battle_script.s",
                FileMode.Create));
            try {
                new Move_Battle_Script(br, sw,
                    ini.moveinfo).Start(learn_move.SelectedIndex,
                    learn_move.Text, script_offset.Text);
            } catch (Exception es) {
                MessageBox.Show(es.ToString());
            } finally {
                br.Close();
                sw.Close();
            }
            pls.Open("battle_script.s");
        }

        private void custom_move_CheckedChanged(object sender, EventArgs e)
        {
            panel3.Enabled = custom_move.Checked;
        }
        TrainerPokeData tp;
        /// <summary>
        /// 训练师精灵编号改变回调
        /// </summary>
        private void t_poke_num_ValueChanged(object sender, EventArgs e)
        {
            if (t_poke_num.Value > trainers.poke_number) {
                trainer_ev.Enabled = panel3.Enabled =
                poke_item.Enabled = trainer_lv.Enabled =
                trainer_poke.Enabled = false;
                trainer_ev.Value = 0;
                poke_item.SelectedIndex = 0;
                trainer_poke.SelectedIndex = 0;
                trainer_lv.Value = 1;
                add.Text = "增加";
                return;
            }
            trainer_ev.Enabled =
            poke_item.Enabled = trainer_lv.Enabled =
            trainer_poke.Enabled = true;
            add.Text = "保存";
            int size = custom_move.Checked ? 16 : 8;
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            byte[] data = rw.ReadBytes(trainers.poke_data,
                (int)t_poke_num.Value - 1, size);
            rw.Close();
            tp = (TrainerPokeData)StructsUtil.ByteToStruct(data,
                typeof(TrainerPokeData));
            trainer_poke.SelectedIndex = tp.poke_id;
            poke_item.SelectedIndex = tp.item_id;
            trainer_lv.Value = tp.level;
            trainer_ev.Value = tp.evs_id;
            if (size == 16) {
                short[] moves = new short[4];
                Buffer.BlockCopy(data, 8, moves, 0, 8);
                trainer_move1.SelectedIndex = moves[0];
                trainer_move2.SelectedIndex = moves[1];
                trainer_move3.SelectedIndex = moves[2];
                trainer_move4.SelectedIndex = moves[3];
                panel3.Enabled = true;
            }

        }
        /// <summary>
        /// 保存训练师精灵信息
        /// </summary>
        /// <param name="index">精灵编号</param>
        void Save_trainer_poke(int index)
        {
            BinaryWriter bw = null;
            try {
                tp.evs_id = (byte)trainer_ev.Value;
                tp.level = (byte)trainer_lv.Value;
                tp.item_id = (short)poke_item.SelectedIndex;
                tp.poke_id = (short)trainer_poke.SelectedIndex;
                byte[] data = StructsUtil.StructToByte(tp);
                int size = custom_move.Checked ? 4 : 3;
                FileStream fs = new FileStream(rom_path, FileMode.Open
                    , FileAccess.ReadWrite, FileShare.ReadWrite);
                bw = new BinaryWriter(fs);
                fs.Seek(trainers.poke_data - 0x8000000 + ((index - 1) << size),
                    SeekOrigin.Begin);
                fs.Write(data, 0, 8);
                if (custom_move.Checked) {
                    bw.Write((short)trainer_move1.SelectedIndex);
                    bw.Write((short)trainer_move2.SelectedIndex);
                    bw.Write((short)trainer_move3.SelectedIndex);
                    bw.Write((short)trainer_move4.SelectedIndex);
                }
            } catch (Exception es) {
                MessageBox.Show(es.ToString());
            } finally {
                bw.Close();
            }

        }
        /// <summary>
        /// 增加或者修改训练师
        /// </summary>
        private void add_Click(object sender, EventArgs e)
        {
            if (t_poke_num.Value <= trainers.poke_number)
                Save_trainer_poke((int)t_poke_num.Value);
            else
                IncreaseTrainerPoke((byte)t_poke_num.Value);
        }

        int new_poke_data = 0x100fe00;
        /// <summary>
        /// 增加训练师精灵
        /// </summary>
        void IncreaseTrainerPoke(byte num)
        {
            int start = new_poke_data;
            int size = custom_move.Checked ? 4 : 3;
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            start = rw.FindFreeOffset0xFF(0x100fe00, num << size);
            StringBuilder sb = new StringBuilder();
            sb.Append("地址：").Append(Operation.Hex(start)).Append("\n新增数量： ")
                .Append(num).Append("个\n").Append("确定新增吗？");
            MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
            DialogResult dr = MessageBox.Show(
                sb.ToString(), "增加精灵", messButton);
            if (dr == DialogResult.OK)//如果点击“确定”按钮
            {
                byte[] data = rw.ReadBytes(trainers.poke_data, 0,
                    trainers.poke_number << size);
                rw.FillWith0xFF(trainers.poke_data, trainers.poke_number << size);
                rw.WriteBytes(start, 0, data);
                for (int x = 0; x < num - trainers.poke_number; x++) {
                    rw.Bw.Write(new byte[2 << size]);
                }
                new_poke_data = rw.Position;
                rw.Close();
                trainers.poke_number = num;
                trainers.poke_data = start + 0x8000000;
                Save_trainer();
                MessageBox.Show("增加成功！");
            }
            else
                rw.Close();
        }

        void panel3_EnabledChanged(object sender, EventArgs e)
        {
            if (!panel3.Enabled) {
                trainer_move1.SelectedIndex = 0;
                trainer_move2.SelectedIndex = 0;
                trainer_move3.SelectedIndex = 0;
                trainer_move4.SelectedIndex = 0;
            }
        }
        /// <summary>
        /// 导出XSE文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void button6_Click(object sender, EventArgs e)
        {
            if (rom_path == null)
                return;
            String offset = script_offset.Text;
            if (String.IsNullOrWhiteSpace(offset))
                return;
            if (!offset.StartsWith("0x"))
                offset = "0x" + offset;
            FileStream fs = new FileStream(rom_path, FileMode.Open
                , FileAccess.ReadWrite, FileShare.ReadWrite);
            BinaryReader br = new BinaryReader(fs);
            StreamWriter sw = new StreamWriter(new FileStream("xse_script.s",
                FileMode.Create));
            try {
                new XSE_scripts(br, sw).Start(PokeConfig.ParseInt(offset));
            } catch (Exception es) {
                MessageBox.Show(es.ToString());
            } finally {
                br.Close();
                sw.Close();
            }
            pls.Open("xse_script.s");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = filter,
			};
            if (dialog.ShowDialog() == DialogResult.OK) {			
					Bitmap bm = new Bitmap(dialog.FileName);
					if (bm.Height != 64 && bm.Height != 128)
						return;
					SetSize(bm.Height == 128);
					pokeimg.PreImport(bm);
					pokePos.RePaint();
					bm.Dispose();               
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int height = front.Image.Height;
            Bitmap b = new Bitmap(256, front.Image.Height);
            Graphics g = Graphics.FromImage(b);
            g.DrawImage(front.Image, new Point(0, 0));
            g.DrawImage(shinyfront.Image, new Point(64, 0));
            g.DrawImage(back.Image, new Point(128, 0));
            g.DrawImage(shinyback.Image, new Point(192, 0));
            b.Save(pokeName.SelectedIndex + ".png", ImageFormat.Png);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (pls.Export("Learnsets.s"))
                pls.Open("Learnsets.s");
            else
                MessageBox.Show("false");
        }

        int k(int a)
        {
            a >>= 3;
            a <<= 3;
            return a;
        }

        private void pictureBox5_MouseClick(object sender, MouseEventArgs e)
        {
            
            Point p = e.Location;
            p.X = p.X >> 4 << 4;
            p.Y = p.Y >> 4 << 4;
            ref Rectangle r = ref pu.r;
            r.Location = p;
            r.Height = (int)numberH.Value;
            r.Width = (int)numberW.Value;
            numberX.Value = p.X;
            numberY.Value = p.Y;
        }

        void DrawImg(ref Rectangle r)
        {
            if (r != Rectangle.Empty && img_in.Image != null) {
                Bitmap bm = img_in.Image.Clone(r, PixelFormat.Format24bppRgb);
                img_in.Refresh();
                Graphics g = img_in.CreateGraphics();
                g.DrawRectangle(Pens.Blue, r);
                pictureBox6.Image = bm;
                pictureBox6.Size = bm.Size;
            }
        }

        Bitmap open_file()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = filter
            };

            if (dialog.ShowDialog() == DialogResult.OK) {
                Bitmap bm = new Bitmap(dialog.FileName);
                return bm;
                //return bm.Clone(new Rectangle(0,0,bm.Width, bm.Height), PixelFormat.Format24bppRgb);
            }
            return null;
        }

        PaintUtils pu;
        Graphics img_out_graphics;

        private void tileset_import_Click(object sender, EventArgs e)
        {
            Bitmap bm = open_file();
            if (bm != null) {
                pu = new PaintUtils();
                img_out.Image = bm;
                img_out_graphics = Graphics.FromImage(bm);
                img_out.Height = bm.Height;
                img_out.Width = bm.Width;
            }

        }

        private void img_out_MouseClick(object sender, MouseEventArgs e)
        {
            if (pictureBox6.Image is null || img_out.Image is null) {
                return;
            }   
            Point p = e.Location;
            p.X = k(p.X);
            p.Y = k(p.Y);
            Rectangle r = new Rectangle(p, pictureBox6.Size);
            img_out_graphics.DrawImage(pictureBox6.Image, r);
            img_out.Refresh();
        }

        private void tileset_save_Click(object sender, EventArgs e)
        {
            if (img_out.Image is null)
                return;
            img_out.Image.Save("tileset.bmp", ImageFormat.Bmp);
            pu.SaveColor();
        }

        private void sucai_Click(object sender, EventArgs e)
        {
            if (img_out.Image == null) {
                img_out.Image = new Bitmap(8*16, 256);
                img_out_graphics = Graphics.FromImage(img_out.Image);
            }
                
            Bitmap bm = open_file();
            if (bm != null) {
                pu = new PaintUtils();
                if (bm.PixelFormat == PixelFormat.Format8bppIndexed)
                    bm = bm.Clone(new Rectangle(0, 0, bm.Width, bm.Height), PixelFormat.Format32bppArgb);
                //pu.ReColor(bm, bm.GetPixel(0, 0), pu.Colors[0]);
                //Bitmap new_ = ImgFunction.GetbyBitMap(bm, pu.Colors);
                img_in.Image = bm;
                img_in.Height = bm.Height;
                img_in.Width = bm.Width;
            }
        }

        private void pal_import_Click(object sender, EventArgs e)
        {
            pu.open();
            if (img_in.Image is null)
                return;
            img_in.Image.Dispose();
            img_in.Image = null;
            img_in.Refresh();
        }

        //private void color_panel_MouseClick(object sender, MouseEventArgs e)
        //{
        //    if (color_panel.BackgroundImage is null)
        //        return;
        //    ColorDialog c = new ColorDialog();
        //    DialogResult result = c.ShowDialog();
        //    if (result == DialogResult.OK) {
        //        Color new_ = c.Color;
        //        Color old = ((Bitmap)color_panel.BackgroundImage).
        //            GetPixel(e.X, e.Y);
        //        pu.ReColorSelf(old, ref new_);
        //        pu.ReColor((Bitmap)color_panel.BackgroundImage
        //            , old, new_);
        //        if (img_in.Image != null)
        //            pu.ReColor(img_in.Image, old, new_);
        //        color_panel.Refresh();
        //        img_in.Refresh();
        //        img_out.Refresh();
        //    }
        //}

        private void npc_num_ValueChanged(object sender, EventArgs e)
        {
            ow.Image = null;
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            int num = (int)npc_num.Value;
            npc.setNpc(rw, num);
            npc_pal_num.Value = (byte)npc.npc.pal_tag;
            if (gfx.Value == 0)
                ow.Image = npc.GetImg(rw, 0);
            gfx.Value = 0;
            ow.Size = ow.Image.Size;

            rw.Close();
        }

        private void npc_pal_num_ValueChanged(object sender, EventArgs e)
        {
            npc.ApplyPal(ow.Image, (int)npc_pal_num.Value);
            ow.Refresh();
        }

        private void gfx_ValueChanged(object sender, EventArgs e)
        {
            ReadAndWrite rw = new ReadAndWrite(rom_path);
            Bitmap b = npc.GetImg(rw, (int)gfx.Value);
            if (b != null) ow.Image = b;
            rw.Close();
        }

        private void gridEvo_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2) {
                evoPokeNames.Enabled = true;
                evolution.rowIndex = e.RowIndex;
            }
            else {
                evoPokeNames.Enabled = false;
            }
        }

        private void ow_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                Bitmap i = open_file();
                if (i == null) {
                    return;
                }
                i = ImgFunction.ConvertGBAImageToBitmapIndexed(i);
                PictureBox box = (PictureBox)sender;
                box.Image = i;
                box.Width = i.Width;
                box.Height = i.Height;
                using (ReadAndWrite rw = new ReadAndWrite()) {
                    npc.Import(rw, i, (int)gfx.Value, (int)npc_pal_num.Value, checkBox1.Checked,
                        checkBox4.Checked);
                }
            }else if(e.Button == MouseButtons.Right) {
                SaveFileDialog sd = new SaveFileDialog() {
                    Filter = "png|*.png;*.PNG"
                };
                if(sd.ShowDialog() == DialogResult.OK) {
                    PictureBox box = (PictureBox)sender;
                    box.Image.Save(sd.FileName, ImageFormat.Png);
                }
            }
                
            
        }

        private void tileMapImportClick(object sender, EventArgs e)
        {
            raw8_Click(sender, e);
        }

        private void raw8_Click(object sender, EventArgs e)
        {
            if(sender == raw8) {
                MessageBox.Show("暂时不支持");
                return;
            }
            Bitmap i = open_file();
            if (i == null) return;
            FolderBrowserDialog fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK) {
                ImgFunction.toRaw(i, fd.SelectedPath);
                MessageBox.Show("finish");
            }
        }


        private void button10_Click(object sender, EventArgs e)
        {
            using (FrameWindow f = new FrameWindow()) {
                if (DialogResult.OK == f.ShowDialog()) {
                    using (ReadAndWrite rw = new ReadAndWrite(rom_path)) {
                        npc.Resize(rw, (int)f.numericUpDown1.Value, (int)f.numericUpDown2.Value,
                            f.checkBox1.Checked);
                    }
                }
            }

        }

        void SaveNpc()
        {
            using (ReadAndWrite rw = new ReadAndWrite(rom_path)) {
                ref npc_type n = ref npc.npc;
                n.pal_tag = (short)(n.pal_tag & 0xFF00 | (int)npc_pal_num.Value);
                int field_c = n.field_C & 0xF;
                if (field_c == 10) n.field_C = (byte)(n.field_C & 0xF0 | 2);
                npc.Save(rw);
            }
        }

        private void numberX_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown a = (NumericUpDown)sender;
            string tag = (string)a.Tag;
            int value = (int)a.Value;
            switch (tag[0]) {
                case '1':
                    pu.r.X = value;
                    break;
                case '2':
                    pu.r.Y = value;
                    break;
                case '3':
                    pu.r.Width = value;
                    break;
                case '4':
                    pu.r.Height = value;
                    break;
                default:
                    return;
            }
            DrawImg(ref pu.r);
        }

		private void button11_Click(object sender, EventArgs e)
		{
			object[] datas = new object[4];
			OpenFileDialog dialog = new OpenFileDialog
			{
				Filter = filter,
			};
			for (int i = 0; i < 4; i++) {
				if (dialog.ShowDialog() == DialogResult.OK) {
					datas[i] = dialog.FileName;
				}
				else {
					return;
				}
			}
			for(int i = 0; i < 4; i++) {
				Bitmap bm = new Bitmap((string)datas[i]);
				if (bm.Height != 64 && bm.Height != 128) {
					MessageBox.Show("第" + i + "张图片大小不是64*64或者128*64, 请修正后再导入");
					return;
				}
				if (bm.PixelFormat != PixelFormat.Format24bppRgb || bm.PixelFormat != PixelFormat.Format32bppArgb)
					bm = bm.Clone(new Rectangle(0,0,bm.Width, bm.Height), PixelFormat.Format24bppRgb);
				datas[i] = bm;
			}
			SetSize(((Bitmap)datas[0]).Height == 128);
			pokeimg.PreImport((Bitmap)datas[0], (Bitmap)datas[1], (Bitmap)datas[2], (Bitmap)datas[3]);
			pokePos.RePaint();
		}

		private void textBox2_TextChanged(object sender, EventArgs e)
		{
			using(ReadAndWrite rw = new ReadAndWrite()) {
				byte[] data = ReadAndWrite.Encode(textBox2.Text, 11);
				((string[])pokeName.DataSource)[pokeName.SelectedIndex] = textBox2.Text;
				rw.WriteBytes(rw.ReadPointer(ini.pokenames), pokeName.SelectedIndex, data);
			}
			
		}
	}
}

