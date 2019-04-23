using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Tools
{
    public class PokeImg
    {
        public MyPictureBox front;
        public MyPictureBox shinyfront;
        public MyPictureBox back;
        public MyPictureBox shinyback;

        public byte[] frontdata;
        public byte[] backdata;
        public byte[] normalpallete;
        public byte[] shinypallete;
        public bool isTwoframe;
        public bool preImport;
        public int front_offset;
        public int back_offset;
        public int normal_pal_off;
        public int shiny_pal_off;
        public int start;
        public int index;

        /**
		 * 小图标
		*/
        public int icoinTable;
        public int icoinPalleteIndexTable;
        public Color[][] icoinPalltes;
        public Bitmap icoinImage;

        public PokeImg(MyPictureBox front, MyPictureBox shinyfront,
            MyPictureBox back, MyPictureBox shinyback)
        {
            this.front = front;
            this.shinyfront = shinyfront;
            this.back = back;
            this.shinyback = shinyback;
            icoinImage = new Bitmap(32, 64, PixelFormat.Format24bppRgb);
        }

        public void InIt(ReadAndWrite rw, INI_init ini)
        {
            front_offset = rw.ReadPointer(ini.frontspritetable);
            back_offset = rw.ReadPointer(ini.backspritetable);
            normal_pal_off = rw.ReadPointer(ini.frontpalettetable);
            shiny_pal_off = rw.ReadPointer(ini.shinypalettetable);
            icoinTable = rw.ReadPointer(ini.icointable);
            icoinPalleteIndexTable = rw.ReadPointer(ini.icoinindextable);
            icoinPalltes = new Color[3][];
            rw.Seek(ini.icoinpallete);
            for (byte x = 0; x < 3; x++) {
                byte[] pal = rw.Br.ReadBytes(32);
                icoinPalltes[x] = ImgFunction.ConvertGBAPalTo24Bit(pal);
            }
        }

        public Bitmap GetIcoin(ReadAndWrite rw)
        {
            rw.Seek(icoinPalleteIndexTable + index);
            byte palindex = rw.Br.ReadByte();
            byte[] img = rw.ReadBytes(rw.ReadPointer(icoinTable + (index << 2))
                , 0, 0x400);
            return ImgFunction.ConvertGBAImageToBitmap(img, icoinPalltes[palindex],
                32, 64, icoinImage);
        }

        public byte[] GetData(ReadAndWrite rw, int offset)
        {
            offset = rw.ReadPointer(offset);
            return ImgFunction.LZUncompress(rw.Br, offset).Item1;
        }

        public void LoadFromGBA(ReadAndWrite rw)
        {
            preImport = false;
            int num = index << 3;
            frontdata = GetData(rw, front_offset + num);
            backdata = GetData(rw, back_offset + num);
            normalpallete = GetData(rw, normal_pal_off + num);
            shinypallete = GetData(rw, shiny_pal_off + num);
            byte height = 64;
            if (frontdata.Length == 2048 * 2) {
                height = 128;
                isTwoframe = true;
            }
            else {
                isTwoframe = false;
            }
            Color[] normalpal = ImgFunction.ConvertGBAPalTo24Bit(normalpallete);
            Color[] shinypal = ImgFunction.ConvertGBAPalTo24Bit(shinypallete);
            front.Image = ImgFunction.ConvertGBAImageToBitmap(frontdata, normalpal, 64, height);
            shinyfront.Image = ImgFunction.ConvertGBAImageToBitmap(frontdata, shinypal, 64, height);
            back.Image = ImgFunction.ConvertGBAImageToBitmap(backdata, normalpal, 64, 64);
            shinyback.Image = ImgFunction.ConvertGBAImageToBitmap(backdata, shinypal, 64, 64);
        }

        public void PreImport(Bitmap bm)
        {
            Rectangle r = new Rectangle(0, 0, 64, bm.Height);
			Bitmap bm1 = bm.Clone(r, PixelFormat.Format24bppRgb);
            r.X += 64;
			Bitmap bm2 = bm.Clone(r, PixelFormat.Format24bppRgb);
            r.X += 64;
            r.Height = 64;
			Bitmap bm3 = bm.Clone(r, PixelFormat.Format24bppRgb);
            r.X += 64;
			Bitmap bm4 = bm.Clone(r, PixelFormat.Format24bppRgb);
            PreImport(bm1, bm2,bm3,bm4);
        }

		

        public void PreImport(Bitmap front, Bitmap shinyfront, Bitmap back, Bitmap shinyback)
        {
			this.front.Image = front;
			this.shinyfront.Image = shinyfront;
			this.back.Image = back;
			this.shinyback.Image = shinyback;
            List<Color> normal = new List<Color>(16);
            List<Color> shiny = new List<Color>(16);
            ImgFunction.GetImgColor(front, shinyfront, normal, shiny);
            frontdata = ImgFunction.ConvertNormalImagToGBA(front, normal);
            backdata = ImgFunction.ConvertNormalImagToGBA(back, normal);
            normalpallete = ImgFunction.LZCompress(ImgFunction.ConvertToGBAPal(normal));
            shinypallete = ImgFunction.LZCompress(ImgFunction.ConvertToGBAPal(shiny));
            preImport = true;
        }

        public void SaveAll(ReadAndWrite rw, bool fill)
        {
            if (!preImport)
                return;
            Save(rw, frontdata, front_offset + index * 8, fill);
            Save(rw, backdata, back_offset + index * 8, fill);
            Save(rw, normalpallete, normal_pal_off + index * 8, fill);
            Save(rw, shinypallete, shiny_pal_off + index * 8, fill);
            preImport = false;
        }

        /// <summary>
        /// 保存任意压缩数据，需要提供原始指针所在地址。
        /// </summary>
        /// <param name="rw"></param>
        /// <param name="data">所需要写入的数据</param>
        /// <param name="offset">指针所在地址</param>
        /// <param name="fill">是否将原来的数据填充为0xFF</param>
        public int Save(ReadAndWrite rw, byte[] data, int offset_to_repoint, bool fill)
        {
            //指针所在地址
            int offset_old = rw.ReadPointer(offset_to_repoint);
            //新地址
            int offset_new = rw.FindFreeOffset(start, data.Length);
            rw.WriteBytes(offset_new, 0, data);
            start = rw.Position;
            rw.Repoint(offset_to_repoint, offset_new);
            int length = ImgFunction.LZUncompress(rw.Br, offset_old).Item2;
            if (fill) {
                rw.FillWith0xFF(offset_old, length);
            }
            return offset_new;
        }
    }

    public class PokePos
    {
        public PictureBox playerY;
        public PictureBox enemyY;
        public Point playerPos;
        public Point enemyPos;
        MyPictureBox back_img;
        MyPictureBox front_img;
        MyPictureBox shadow;
        public byte[] pos = new byte[3];
        int enymyyTable = 0;
        int playeryTable = 0;
        int altitudeTable = 0;


        public PokePos(PictureBox playerY, PictureBox enemyY, MyPictureBox shadow)
        {
            this.playerY = playerY;
            this.enemyY = enemyY;
            this.shadow = shadow;
            playerPos = new Point(40, 58);
            enemyPos = new Point(144, 18);
        }

        public void SetPos(ReadAndWrite rw)
        {
            int enymyyTable = 0xa5f54;
            this.enymyyTable = rw.ReadPointer(enymyyTable);
            int playeryTable = 0xa5ebc;
            this.playeryTable = rw.ReadPointer(playeryTable);
            int altitudeTable = 0xa5ff4;
            this.altitudeTable = rw.ReadPointer(altitudeTable);
        }

        public void SetPic(MyPictureBox front, MyPictureBox back)
        {
            front_img = front;
            back_img = back;
        }


        public void GetPos(ReadAndWrite rw, int index)
        {
            rw.Seek(enymyyTable + index * 4 + 1);
            pos[1] = rw.Br.ReadByte();
            rw.Seek(playeryTable + index * 4 + 1);
            pos[0] = rw.Br.ReadByte();
            rw.Seek(altitudeTable + index);
            pos[2] = rw.Br.ReadByte();
        }
        public void SavePos(ReadAndWrite rw, int index)
        {
            rw.Seek(enymyyTable + index * 4 + 1);
            rw.Bw.Write(pos[1]);
            rw.Seek(playeryTable + index * 4 + 1);
            rw.Bw.Write(pos[0]);
            rw.Seek(altitudeTable + index);
            rw.Bw.Write(pos[2]);
        }

        public void RePaint()
        {
            playerPos.Y = 58;
            enemyPos.Y = 18;
            Bitmap front = front_img.Image.Clone(new Rectangle(0, 0, 64, 64),
                PixelFormat.Format24bppRgb);
            front.MakeTransparent(front.GetPixel(0, 0));
            Bitmap back = back_img.Image.Clone(new Rectangle(0, 0, 64, 64),
                PixelFormat.Format24bppRgb);
            back.MakeTransparent(back.GetPixel(0, 0));
            playerY.Image = back;
            enemyY.Image = front;
            Refresh();
        }

        public void Refresh()
        {
            playerPos.Y = (byte)(58 + pos[0]);
            enemyPos.Y = (byte)(18 + pos[1] - pos[2]);
            playerY.Location = playerPos;
            enemyY.Location = enemyPos;
            if (pos[2] > 0)
                shadow.Show();
            else
                shadow.Hide();
        }

        public void ChangePlayerY(object sender, EventArgs e)
        {
            NumericUpDown n = sender as NumericUpDown;
            if (pos[0] == (byte)n.Value)
                return;
            pos[0] = (byte)n.Value;

            Refresh();
        }

        public void ChangeEnemyY(object sender, EventArgs e)
        {
            NumericUpDown n = sender as NumericUpDown;
            if (pos[1] == (byte)n.Value)
                return;
            pos[1] = (byte)n.Value;
            Refresh();
        }

        public void ChangeEnemyAlt(object sender, EventArgs e)
        {
            NumericUpDown n = sender as NumericUpDown;
            if (pos[2] == (byte)n.Value)
                return;
            pos[2] = (byte)n.Value;
            Refresh();
        }
    }

    public class PokeLearnSet
    {
        bool expanded_learnset;
        public byte[] learnlvl_buffer = new byte[50];
        public short[] learnmove_buffer = new short[50];
        public int learn_offset;
        public int offset;
        public DataGridView grid;
        String[] dataSource;
        public PokeLearnSet(String rom_id, int learn_offset, DataGridView grid, String[] dataSource)
        {
            if (rom_id.Equals("rom"))
                expanded_learnset = false;
            else
                expanded_learnset = true;
            this.learn_offset = learn_offset;
            this.grid = grid;
            this.dataSource = dataSource;
        }

        public void SetLearnSet(ReadAndWrite rw, int index)
        {
            offset = rw.ReadPointer(learn_offset + 4 * index);
            BinaryReader br = rw.Br;
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            grid.Rows.Clear();
            if (expanded_learnset) {
                for (int x = 0; x < 50; x++) {
                    short s = br.ReadInt16();
                    learnmove_buffer[x] = s;
                    byte m = br.ReadByte();
                    learnlvl_buffer[x] = m;
                    if (s == 0)
                        break;
                    grid.Rows.Add();
                    grid.Rows[x].Cells[0].Value = m;
                    grid.Rows[x].Cells[1].Value = dataSource[s];
                }
            }
            else {

            }

        }

        public void Save(BinaryWriter bw)
        {
            bw.BaseStream.Seek(offset, SeekOrigin.Begin);
            for (int x = 0; x < 50; x++) {
                bw.Write(learnmove_buffer[x]);
                bw.Write(learnlvl_buffer[x]);
                if (learnmove_buffer[x] == 0)
                    break;
            }
        }

        public void Change(int index, byte lvl, short move)
        {
            learnlvl_buffer[index] = lvl;
            learnmove_buffer[index] = move;
        }

        public bool Export(String fileName)
        {
            if (!expanded_learnset)
                return false;
            StreamWriter sw = new StreamWriter(new FileStream(fileName,
                FileMode.Append));
            sw.Write('{');
            int x = 0;
            while (learnmove_buffer[x] != 0) {
                sw.Write('{');
                sw.Write(learnmove_buffer[x]);
                sw.Write(',');
                sw.Write(learnlvl_buffer[x]);
                sw.Write("}, ");
                x++;
            }
            sw.WriteLine("{MOVE_BLANK, END}};");
            sw.Flush();
            sw.Close();
            return true;
        }

        public void Open(String fileName)
        {
            try {
                System.Diagnostics.Process.Start("notepad++.exe", fileName);
            } catch (Exception) { }
        }
    }

    class NpcType
    {
        const int TABLE_DEAFAULT = 0x509954;//行走图起始
        const int TABLE_EXPANDED = 0x1010e50;//扩充后的行走图
        const int PAL_SPRITE = 0x50bbc8;
        const byte PAL_NUM = 0x24;
        const byte SAMPLE = 76;
        const byte MAX_NUM = 244;
        public Color[][] palletes = new Color[PAL_NUM][];
        public npc_type npc;
        private int num;
        private int table;
        public void LoadPalletes(ReadAndWrite rw)
        {

            byte[] data = new byte[0x20];
            Stream s = rw.Br.BaseStream;
            for (int x = 0; x < PAL_NUM - 1; x++) {
                int off = rw.ReadPointer(PAL_SPRITE + (x << 3));
                int num = s.ReadByte();
                rw.Seek(off);
                s.Read(data, 0, 0x20);
                palletes[num] = ImgFunction.ConvertGBAPalTo24Bit(data);
            }
        }

        private npc_type GetNpc(ReadAndWrite rw, int num)
        {
            if (num > MAX_NUM) {
                num -= MAX_NUM;
                table = TABLE_EXPANDED;
            }
            else {
                table = TABLE_DEAFAULT;
            }
            this.num = num;
            return (npc_type)rw.GetStruct(table, num, typeof(npc_type));
        }

        public void setNpc(ReadAndWrite rw, int num)
        {
            npc = GetNpc(rw, num);
        }

        public Bitmap GetImg(ReadAndWrite rw, int frames)
        {
            if (palletes[0] == null)
                LoadPalletes(rw);
            int off;
            try {
                off = rw.ReadPointer(npc.gfx_table + (frames << 3));
            } catch (Exception) {
                return null;
            }

            Bitmap p = rw.GetIndexedBitmapNotLz(off, palletes[(byte)npc.pal_tag], npc.width, npc.height);
            return p;
        }

        public void ApplyPal(Bitmap bm, int frames)
        {
            if (bm == null) return;
            Color[] c = palletes[frames];
            ColorPalette p = bm.Palette;
            Array.Copy(c, p.Entries, 16);
            bm.Palette = p;
        }

        public void Import(ReadAndWrite rw, Bitmap bm, int frame, int pal, bool clear, bool find)
        {
            int gfx_offset = npc.gfx_table + (frame << 3);
            int img_offset;
            try {
                img_offset = rw.ReadPointer(gfx_offset);
                if (clear) {
                    rw.FillWith0xFF(img_offset, rw.Br.ReadInt16());
                    goto FIND;
                }
                if (clear || find)
                    goto FIND;
                goto NOT_FIND;
            } catch (NullReferenceException) { }
            FIND:
            img_offset = rw.FindFreeOffset0xFF(PokeConfig.ini.start_offset, bm.Width * bm.Height / 2);
            NOT_FIND:
            byte[] img = ImgFunction.ConvertNormalImagToGBA(bm, new List<Color>(palletes[pal]), false);
            rw.WriteBytes(img_offset, 0, img);
            rw.Seek(gfx_offset);
            rw.Bw.Write(img_offset + ReadAndWrite.ROM);
            rw.Bw.Write(bm.Width * bm.Height >> 1);
            npc.width = (short)bm.Width;
            npc.height = (short)bm.Height;
            Save(rw);
        }

        public void Resize(ReadAndWrite rw, int old, int new_, bool clear)
        {
            if (new_ < old) return;
            int start = rw.FindFreeOffset0xFF(new_ * 8);
            rw.Seek(start);
            for(int i = 0; i < new_; i++) {
                rw.Bw.Write(0x1010101010001000);
            }
            byte[] a = rw.ReadAndClear(npc.gfx_table, 8 * old, clear);
            rw.WriteBytes(start, 0, a);
            npc.gfx_table = start + ReadAndWrite.ROM;
            Save(rw);
        }

        public void Save(ReadAndWrite rw)
        {
            rw.Save(table, num, npc);
        }

    }
}
