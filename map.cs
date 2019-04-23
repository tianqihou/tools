using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    unsafe class Map : ReadAndWrite
    {
        public const int MAP_FOOTER_POINTER = 0x849CC;
        public const int MAP_BANK_PONTER = 0x84AA4;
        mapheader header;
        footer footer;
        blockset tileset1;//back
        blockset tileset2;//front
        Color[] map_pals;
        long[] buffer = new long[8];

        public Bitmap tileset;
        short[] blockset_data;
        public Bitmap map_tile1;
        public Bitmap map_tile2;
        short[] map_tiledata;
		readonly Bitmap map;

        public void LoadMap(int map_no, int map_bank)
        {
            LoadMapData(map_no, map_bank);
            LoadBlockTiles();
            LoadMapTileData();
            LoadMapTiles();
            LoadMapImg();
        }

        public void LoadMapData(int map_no, int map_bank)
        {
            int offset = ReadPointer(MAP_BANK_PONTER);
            offset = ReadPointer(offset + 4 * map_no);
            offset = ReadPointer(offset + 4 * map_bank);
            header = GetStruct<mapheader>(offset, 0);
            offset = ReadPointer(MAP_FOOTER_POINTER);
            offset = ReadPointer(offset + 4 * header.footer_id);
            footer = GetStruct<footer>(offset, 0);
            tileset1 = GetStruct<blockset>(footer.primary_blockset, 0);
            tileset2 = GetStruct<blockset>(footer.secondary_blockset, 0);
            map_pals = ImgFunction.ConvertGBAPalTo24Bit(ReadBytes(tileset1.pals, 0, 12 * 0x20));
        }
        private void LoadBlockTiles()
        {

            byte[] img1 = ImgFunction.LZUncompress(Br, tileset1.tileset - ROM).Item1;
            byte[] img2 = ImgFunction.LZUncompress(Br, tileset2.tileset - ROM).Item1;
            byte[] img = new byte[img1.Length + img2.Length];
            Buffer.BlockCopy(img1, 0, img, 0, img1.Length);
            Buffer.BlockCopy(img2, 0, img, img1.Length, img2.Length);
            tileset = ImgFunction.ConvertGBAImageToBitmapIndexed(img, map_pals, 128, img.Length * 2 / 128);
        }

        private void LoadMapTileData()
        {
            blockset_data = ReadShort(tileset1.blockset_data, 0x290 * 8);
            map_tiledata = ReadShort(footer.blocksID_movement, footer.width_blocks * footer.height_blocks);
        }

        const int WIDTH = 10 * 8 * 2;

        private void LoadMapTiles()
        {
            map_tile1 = new Bitmap(WIDTH, 0x29 * 16, PixelFormat.Format8bppIndexed);
            ColorPalette entries = map_tile1.Palette;
            for (int i = 0; i < map_pals.Length; i++) {
                entries.Entries[i] = map_pals[i];
            }
            map_tile1.Palette = entries;
            map_tile2 = (Bitmap)map_tile1.Clone();
            BitmapData bd1 = map_tile1.LockBits(new Rectangle(0, 0, map_tile1.Width, map_tile1.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            BitmapData bd2 = map_tile2.LockBits(new Rectangle(0, 0, map_tile1.Width, map_tile1.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            BitmapData bd0 = tileset.LockBits(new Rectangle(0, 0, tileset.Width, tileset.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            for (int y = 0, blockid = 0; y < 0x28; y++) {
                for (int x = 0; x < 10; x++) {
                    SetMapTiles(bd1, bd0, ref blockid, x * 2 + y * 40);
                    SetMapTiles(bd2, bd0, ref blockid, x * 2 + y * 40);
                }
            }
            map_tile1.UnlockBits(bd1);
            map_tile2.UnlockBits(bd2);
            tileset.UnlockBits(bd0);
            map_tile1.MakeTransparent(Color.Black);
            map_tile2.MakeTransparent(Color.Black);
            using (Graphics g = Graphics.FromImage(map_tile1)) {
                g.DrawImage(map_tile2, Point.Empty);
            }
        }
        private unsafe void SetMapTiles(BitmapData bd, BitmapData bd0, ref int bigblockid, int orderid)
        {
            int x = (orderid % (bd.Width >> 3));
            int y = (orderid / (bd.Width >> 3)) * bd.Stride;
            orderid = x + y;
            WriteBitmapData(bd, bd0, ref bigblockid, orderid);
            WriteBitmapData(bd, bd0, ref bigblockid, orderid + 1);
            WriteBitmapData(bd, bd0, ref bigblockid, orderid + bd.Stride);
            WriteBitmapData(bd, bd0, ref bigblockid, orderid + 1 + bd.Stride);
        }

        private void WriteBitmapData(BitmapData bd, BitmapData bd0, ref int bigblockid, int orderid)
        {
            GetBitmapData(bd0, buffer, blockset_data[bigblockid++]);
            long* ptr = (long*)bd.Scan0.ToPointer();

            int stride = bd.Width >> 3;
            for (int i = 0; i < 8; i++) {
                ptr[orderid + i * stride] = buffer[i];
            }
        }

        public static unsafe void GetBitmapData(BitmapData bd, long[] result, int block)
        {
            long* ptr = (long*)bd.Scan0.ToPointer();
            int start = block & 0b1111111111;//后10位id 中间2位是否反转
            int x = (start % (bd.Width >> 3));
            int y = (start / (bd.Width >> 3)) * bd.Stride;
            start = x + y;
            byte palNum = (byte)((block >> 12) * 16);
            long add = palNum << 8 | palNum;
            add = add << 8 | palNum;
            add = add << 8 | palNum;
            add = add << 8 | palNum;
            add = add << 8 | palNum;
            add = add << 8 | palNum;
            add = add << 8 | palNum;
            int stride = bd.Width >> 3;
            for (int i = 0; i < 8; i++) {
                result[i] = ptr[start + i * stride] + add;
            }
            if((block & 0b10000000000) == 0b10000000000) { // x reverse
                for (int i = 0; i < 8; i++) {
                    long val = result[i];
                    byte[] byteval = BitConverter.GetBytes(val);
                    Array.Reverse(byteval);
                    result[i] = BitConverter.ToInt64(byteval, 0);
                }
            }
            if((block & 0b100000000000) == 0b100000000000) { // y reverse
                Array.Reverse(result);
            }
        }

        private void LoadMapImg()
        {

        }

    }
}
