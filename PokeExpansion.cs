using System;
using System.Collections.Generic;
using System.IO;


namespace Tools
{
    class PokeExpansion : ReadAndWrite
    {
        //pointer, size, old_slot
        Dictionary<String, int[]> info = new Dictionary<String, int[]> {
            {"基础值", new int[]{ 0x0001BC, 0x1C, 411} },
            {"正面图", new int[]{ 0x000128, 8, 440} },
            {"背面图", new int[]{ 0x00012C, 8, 440} },
            {"普通色板", new int[]{ 0x000130, 8, 440} },
            {"闪光色板", new int[]{ 0x000134, 8, 440} },
            {"小图标", new int[]{ 0x000138, 4, 440} },
            {"图标色板", new int[]{ 0x00013C, 1, 440} },
            {"精灵名字", new int[]{ 0x000144, 11, 411} },
            {"tmhm学习面", new int[]{ 0x06E060, 8, 411} },
            {"定点学习面", new int[]{ 0x1B2390, 4, 411} },
            {"图鉴", new int[]{ 0x0BFA20, 0x20, 387} },
            {"进化表", new int[]{ 0x06D140, 40, 411} },
            {"敌方Y轴", new int[]{ 0x0A5F54, 4, 440} },
            {"友方Y轴", new int[]{ 0x0A5EBC, 4, 440} },
            {"升级技能", new int[]{0x06E3B4, 4, 411} },
            {"front_animation_table", new int[]{ 0x06EE7C, 1, 410} },
            {"anim_delay_table", new int[]{ 0x06EDDC, 1, 410} },
            {"footprint_table", new int[]{ 0x0C0DBC, 4, 411} },
            {"叫声1", new int[]{ 0x0A35EC, 0xc, 388} },
            {"叫声2", new int[]{ 0x0A35DC, 0xc, 388} },
            {"阴影", new int[]{ 0x0A5FF4, 1,411} },
            {"auxialary_cry_table", new int[]{ 0x06D534, 2, 136} },
            {"全国图鉴", new int[]{ 0x06D4BC, 2, 410} },
            {"hoenn_to_national_table", new int[]{ 0x06D494, 2,410} },
            {"hoenn_dex_table", new int[]{ 0x06D3FC, 2, 410} },
            {"back_anim_table", new int[]{ 0x17F488, 1, 411} },
            {"帧数", new int[]{ 0x05E7BC, 4, 440} },
        };

        private BinaryReader br;
        private BinaryWriter bw;
        int new_poke;
        int new_dex;

        public PokeExpansion(String path,int new_poke, int new_dex):base(path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            bw = new BinaryWriter(fs);
            br = new BinaryReader(fs);
            this.new_dex = new_dex;
            this.new_poke = new_poke;
        }
     
        public void RepointPokesEm(int NewNumberOfPokes, int NewDexSize, int RamOffset, int StartOffset)
        {
            WordsDecoding wd = new WordsDecoding();
            //插入数据
            bw.BaseStream.Seek(0x5cdc00, SeekOrigin.Begin);
            bw.Write(wd.SaveSizeEm, 0, wd.SaveSizeEm.Length);
            bw.BaseStream.Seek(StartOffset, SeekOrigin.Begin);
            bw.Write(wd.SaveBlockEm, 0, wd.SaveBlockEm.Length);

            //设置saveblock
            bw.BaseStream.Seek(0x152E98, SeekOrigin.Begin);
            bw.Write(0x47004800);
            //开始地址转成指针先加0x8000000
            bw.Write(StartOffset + 1 + 0x8000000);
            bw.BaseStream.Seek(0x15284E, SeekOrigin.Begin);
            bw.Write(new byte[] { 0x38, 0x47 });
            bw.BaseStream.Seek(0x15288C, SeekOrigin.Begin);
            bw.Write(StartOffset + 61 + 0x8000000);
            bw.BaseStream.Seek(0x0D9CC6, SeekOrigin.Begin);
            bw.Write(new byte[] { 0x38, 0x47 });
            bw.BaseStream.Seek(0x0D9D04, SeekOrigin.Begin);
            bw.Write(StartOffset + 61 + 0x8000000);
            bw.BaseStream.Seek(0x0DA284, SeekOrigin.Begin);
            bw.Write(0x47004800);
            bw.Write(StartOffset + 1 + 0x8000000);

            //扩容图鉴。重定义seenflag和caughtflag
            //内存地址不需要加
            //安全地址1：0203D800 安全地址2：0x0203CF64
            if (NewDexSize % 8 != 0)
                NewDexSize += (8 - NewDexSize % 8);
            byte NeededFlagBytes = (byte)(NewDexSize / 8);
            int SeenFlagOffset = RamOffset;
            int CaughtFlagOffset = SeenFlagOffset + NeededFlagBytes;

            bw.BaseStream.Seek(0xC06EC, SeekOrigin.Begin);
            bw.Write(SeenFlagOffset);
            bw.BaseStream.Seek(0xC06AC, SeekOrigin.Begin);
            bw.Write(0);

            bw.BaseStream.Seek(0xC0744, SeekOrigin.Begin);
            bw.Write(CaughtFlagOffset);
            bw.BaseStream.Seek(0xC06FE, SeekOrigin.Begin);
            bw.Write(new byte[6] { 0, 0, 0, 0, 0, 0 });
            bw.BaseStream.Seek(0xC0710, SeekOrigin.Begin);
            bw.Write(new byte[2] { 0x15, 0xE0 });
            bw.BaseStream.Seek(0xC0720, SeekOrigin.Begin);
            bw.Write(new byte[2] { 0xd, 0xE0 });
            bw.BaseStream.Seek(0xC07C8, SeekOrigin.Begin);
            bw.Write(SeenFlagOffset);
            bw.BaseStream.Seek(0xC079E, SeekOrigin.Begin);
            bw.Write(0);
            bw.BaseStream.Seek(0xC07AC, SeekOrigin.Begin);
            bw.Write(new byte[2] { 0x1c, 0xE0 });
            bw.BaseStream.Seek(0xC07F0, SeekOrigin.Begin);
            bw.Write(CaughtFlagOffset);
            bw.BaseStream.Seek(0xC07DA, SeekOrigin.Begin);
            bw.Write(0);
            bw.BaseStream.Seek(0x843BC, SeekOrigin.Begin);
            bw.Write(SeenFlagOffset);
            bw.BaseStream.Seek(0x8439A, SeekOrigin.Begin);
            bw.Write(0x1c20);
            bw.BaseStream.Seek(0x843A0, SeekOrigin.Begin);
            bw.Write(new byte[2] { NeededFlagBytes, 0x22 });
            bw.BaseStream.Seek(0x843A6, SeekOrigin.Begin);
            bw.Write(new byte[2] { 0x20, 0x1c });
            bw.BaseStream.Seek(0x843A8, SeekOrigin.Begin);
            bw.Write(new byte[2] { NeededFlagBytes, 0x30 });
            bw.BaseStream.Seek(0x843AC, SeekOrigin.Begin);
            bw.Write(new byte[2] { NeededFlagBytes, 0x22 });
        }

        public int Align4Offset(int offset)
        {
            if (offset % 4 != 0)
                offset += (4 - offset % 4);
            return offset;
        }

        public void Clear(int offset, int bytes)
        {
            bw.BaseStream.Seek(offset, SeekOrigin.Begin);
            byte ff = 0xff;
            for(int x = 0; x < bytes; x++)
            {
                bw.Write(ff);
            }
        }

        public void Replace(int new_pointer, int old_pointer)
        {
            int[] offsets = new int[0x7a000];
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            byte[] bytes = br.ReadBytes(0x1E8000);
            Buffer.BlockCopy(bytes, 0, offsets, 0, 0x1E8000);
            for (int x = 0; x< 0x7a000;x++)
            {
                if(offsets[x]== old_pointer)
                {
                    offsets[x] = new_pointer;
                }
            }
            Buffer.BlockCopy(offsets, 0, bytes, 0, 0x7a000);
            bw.BaseStream.Seek(0, SeekOrigin.Begin);
            bw.Write(bytes);
        }

        public int ReadAsPointer(int offset)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            byte[] b = br.ReadBytes(4);
            int k = BitConverter.ToInt32(b, 0);
            return k;
        }

        public void Repoint()
        {

        }

    }
}
