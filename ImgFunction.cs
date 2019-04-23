using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Tools
{
    public delegate void writeRaw(FileStream fs, byte v, long[] data);

    unsafe struct ByteList
    {
        public readonly byte* data;
        public readonly int length;
        public int count;

        public ByteList(byte* data, int length)
        {
            this.data = data;
            this.length = length;
            count = 0;
        }

        public void Add(byte i)
        {
            if (count == length) throwsIndexOutOfRange();
            data[count++] = i;
        }

        public void throwsIndexOutOfRange() { throw new IndexOutOfRangeException(); }
       

        public void AddRange(ref ByteList list)
        {
            if (list.count == 0)
                return;
            if (count + list.count > length) throwsIndexOutOfRange();
            for (int i = 0; i < list.count; i++)
            {
                data[count++] = list.data[i];
            }
        }

        public void Clear() { count = 0; data[0] = 0; }

        public byte[] ToArray()
        {
            byte[] data = new byte[count];
            Marshal.Copy(new IntPtr(this.data), data, 0, count);
            return data;
        }
    }

    struct IntegerString
    {
        int value;
        public int count;

        public byte Value { get { return (byte)value; } }

        public void reset()
        {
            value = 0; count = 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is IntegerString i)
            {
                return value == i.value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public bool isEmpty() { return count == 0; }

        public static bool operator ==(IntegerString a, int b) => a.value == b;
        public static bool operator !=(IntegerString a, int b) => a.value != b;
        public static IntegerString operator +(IntegerString a, int b)
        {
            if (a.count == 8)
                throw new IndexOutOfRangeException();
            a.value = (a.value << 1 | b);
            a.count++;
            return a;
        }

    }

    public struct SubArray
    {
        public readonly byte[] array;
        public readonly int from;
        public readonly int Length;

        public SubArray(byte[] array, int from, int to)
        {
            if (to > array.Length)
                to = array.Length;
            if (from > array.Length) {
                Length = 0;
                this.array = null;
                this.from = 0;
            }
            else {
                this.from = from;
                this.array = array;
                Length = to - from;
            }
        }

        public SubArray(SubArray a, int from, int to)
        {
            array = a.array;
            this.from = a.from + from;
            Length = to - from;
            if (this.from + Length > array.Length)
                Length = array.Length - this.from;

        }

        public byte this[int index] {
            get {
                if (index >= Length)
                    throw new IndexOutOfRangeException();
                return array[from + index];
            }
        }

        public override String ToString()
        {
            System.Text.StringBuilder b = new System.Text.StringBuilder();
            b.Append('[');
            for (int i = 0; ; i++) {
                b.Append(this[i]);
                if (i == Length - 1)
                    return b.Append(']').ToString();
                b.Append(", ");
            }
        }
    }

    public static unsafe class ImgFunction
    {
        public static Tuple<byte[], int> LZUncompress(BinaryReader rom, int offset)
        {
            rom.BaseStream.Seek(offset, SeekOrigin.Begin);

            int data_length = rom.ReadInt32();

            if ((data_length & 0xFF) != 0x10)
                throw new ArgumentException(Decompiler.Operation.Hex(offset));
            data_length = data_length >> 8;
            int x = 0;
            byte[] uncompressed = new byte[data_length];
            while (true)
            {
                byte bit_field = rom.ReadByte();

                for (int y = 7; y >= 0; y--)
                {
                    if (((bit_field >> y) & 1) == 1)
                    {
                        int r5 = rom.ReadByte();
                        int store = r5;
                        int r6 = 3;
                        int r3 = (r5 >> 4) + r6;
                        r6 = store;
                        r5 = r6 & 0xF;
                        int r12 = r5 << 8;
                        r6 = rom.ReadByte();
                        r5 = r6 | r12;
                        r12 = r5 + 1;
                        for (int n = 0; n < r3; n++)
                        {
                            try
                            {
                                r5 = uncompressed[x - r12];
                            }
                            catch (Exception)
                            {
                                throw new ArgumentException(Decompiler.Operation.Hex(offset));
                            }
                            uncompressed[x] = (byte)r5;
                            x++;
                            if (x == data_length)
                            {
                                int size = (int)rom.BaseStream.Position;
                                return new Tuple<byte[], int>(uncompressed, size - offset);
                            }
                        }
                    }
                    else
                    {
                        uncompressed[x] = rom.ReadByte();
                        x++;
                        if (x == data_length)
                        {
                            int size = (int)rom.BaseStream.Position;
                            return new Tuple<byte[], int>(uncompressed, size - offset);
                        }
                    }
                }
            }
        }

        //public static unsafe byte[] LZCompress(byte[] data)
        //{
        //    byte* bytess = stackalloc byte[4096];
        //    ByteList compressed = new ByteList(bytess, 4096);
        //    int len = data.Length;
        //    compressed.Add(0x10);
        //    compressed.Add((byte)len);
        //    compressed.Add((byte)(len >> 8));
        //    compressed.Add((byte)(len >> 16));
        //    int index = 0;
        //    int w = 4095;
        //    //byte[] window = null;
        //    byte[] lookahead = null;
        //    IntegerString bits = new IntegerString();
        //    byte* bytes = stackalloc byte[100];
        //    ByteList currCompSet = new ByteList(bytes, 100);
        //    while (true)
        //    {
        //        bits.reset();
        //        currCompSet.Clear();
        //        int check = 0;
        //        for (int n = 0; n < 8; n++)
        //        {
        //            int start, end;
        //            if (index < w)
        //            {
        //                start = 0; end = index;
        //                //window = SubBytes(data, 0, index);
        //            }
        //            else
        //            {
        //                start = index - w; end = index;
        //                //window = SubBytes(data, index - w, index);
        //            }

        //            lookahead = SubBytes(data, index, len);
        //            if (lookahead.Length == 0)
        //            {
        //                if (!bits.isEmpty())
        //                {
        //                    while (bits.count < 8)
        //                    {
        //                        bits += 0;
        //                        currCompSet.Add(0);
        //                    }
        //                    compressed.Add(bits.Value);
        //                    compressed.AddRange(ref currCompSet);
        //                }
        //                break;
        //            }
        //            //byte[] a = SubBytes(lookahead, 0, 3);
        //            //check = Find(window, a);
        //            check = Find(data, start, end, lookahead, 0, 3);
        //            if (check == -1)
        //            {
        //                index++;
        //                bits += 0;
        //                currCompSet.Add(lookahead[0]);
        //            }
        //            else
        //            {
        //                bits += 1;
        //                int length = 2;
        //                int store_length = 0;
        //                int store_check = 0;
        //                while (check != -1 && length < 18)
        //                {
        //                    store_length = length;
        //                    length++;
        //                    store_check = check;
        //                    //check = Find(window, SubBytes(lookahead, 0, length));
        //                    check = Find(data, start, end, lookahead, 0, length);
        //                }
        //                index += store_length;
        //                store_length -= 3;
        //                //int position = window.Length - store_check - 1;
        //                int position = end - start - store_check - 1;
        //                store_length = store_length << 12;
        //                int pos_and_len = store_length | position;
        //                currCompSet.Add((byte)(pos_and_len >> 8));
        //                currCompSet.Add((byte)pos_and_len);
        //            }
        //        }
        //        if (lookahead.Length == 0)
        //        {
        //            break;
        //        }
        //        compressed.Add(bits.Value);
        //        compressed.AddRange(ref currCompSet);
        //    }
        //    byte[] new_data = compressed.ToArray();
        //    return new_data;
        //}

        public static byte[] LZCompress(byte[] data)
        {
            List<byte> compressed = new List<byte>(5000);
            int *moveLength = stackalloc int[MAXSIZE];
            ImgFunction.moveLength = moveLength;
            int len = data.Length;
            len = ((len << 8) | 0x10);
            compressed.AddRange(BitConverter.GetBytes(len));
            len = data.Length;
            int index = 0;
            int w = 4095;
            SubArray window;//= new SubArray();
            SubArray lookahead = new SubArray();
            List<byte> currCompSet = new List<byte>(100);
            while (true) {
                IntegerString bits = new IntegerString();
                int check = 0;
                currCompSet.Clear();
                for (int n = 0; n < 8; n++) {
                    if (index < w) {
                        window = new SubArray(data, 0, index);
                    }
                    else {
                        window = new SubArray(data, index - w, index);
                    }

                    lookahead = new SubArray(data, index, len);
                    if (lookahead.Length == 0) {
                        if (!bits.isEmpty()) {
                            while (bits.count < 8) {
                                bits += 0;
                                currCompSet.Add(0);
                            }
                            compressed.Add(bits.Value);
                            compressed.AddRange(currCompSet);
                        }
                        break;
                    }
                    SubArray a = new SubArray(lookahead, 0, 3);
                    check = Sunday(ref window,ref a);
                    if (check == -1) {
                        index++;
                        bits += 0;
                        currCompSet.Add(lookahead[0]);
                    }
                    else {
                        bits += 1;
                        int length = 2;
                        int store_length = 0;
                        int store_check = 0;
                        while (check != -1 && length < 18) {
                            store_length = length;
                            length++;
                            store_check = check;
                            SubArray tmp = new SubArray(lookahead, 0, length);
                            check = Sunday(ref window,ref tmp);
                        }
                        index += store_length;
                        store_length -= 3;
                        int position = window.Length - store_check - 1;
                        store_length = store_length << 12;
                        int pos_and_len = store_length | position;
                        currCompSet.Add((byte)(pos_and_len >> 8));
                        currCompSet.Add((byte)pos_and_len);
                    }
                }
                if (lookahead.Length == 0) {
                    break;
                }
                compressed.Add(bits.Value);
                compressed.AddRange(currCompSet);
            }
            byte[] new_data = compressed.ToArray();
            compressed = null;
            return new_data;
        }
        const int MAXSIZE = 256;
        private static int* moveLength = null;
        private static unsafe void GetMoveLength(ref SubArray T, int* moveLength)
        {
            int tLen = T.Length;
            for (int i = 0; i < MAXSIZE; ++i)
                moveLength[i] = tLen + 1;
            for (int i = 0; i < tLen && T[i] != 0; ++i)
                moveLength[T[i]] = tLen - i;
        }


        public static unsafe int Sunday(ref SubArray S,ref SubArray T)
        {
            if (S.Length < T.Length)
                return -1;
            if (S.Length == T.Length) {
                for (int k = 0; k < S.Length; k++) {
                    if (S[k] != T[k]) return -1;
                }
                return 0;
            }
            //int* moveLength = stackalloc int[MAXSIZE];
            GetMoveLength(ref T, moveLength);
            int tLen = T.Length;
            int sLen = S.Length;
            int i = 0;
            while (i < sLen) {
                int j = 0;
                while (j < tLen && i + j < sLen && S[i + j] == T[j]) {
                    ++j;
                }
                if (j >= tLen) return i;
                if (i + tLen >= sLen)
                    return -1;
                i += moveLength[S[i + tLen]];
            }
            return -1;
        }



        //public static byte[] SubBytes(byte[] data, int start, int end)
        //{
        //    if (start > data.Length - 1 || end <= start)
        //        throw new IndexOutOfRangeException();
        //    if (end > data.Length)
        //        end = data.Length;
        //    if (end - start == data.Length)
        //        return data;
        //    byte[] tem = new byte[end - start];
        //    Buffer.BlockCopy(data, start, tem, 0, end - start);
        //    return tem;
        //}

        ////KMP算法,对于任意序列都有用,s=source,t=target
        //public static int Find(byte[] s, byte[] t)
        //{
        //    return Find(s, 0, s.Length, t, 0, t.Length);
        //}

        //public static int Find(byte[] s, byte[] t, int start, int end)
        //{
        //    return Find(s, 0, s.Length, t, start, end);
        //}

        //public static unsafe int Find(byte[] s, int s_start, int s_end, byte[] t, int t_start, int t_end)
        //{
        //    t_end = Math.Min(t_end, t.Length);
        //    s_end = Math.Min(s_end, s.Length);
        //    if (s.Length == 0 || t.Length == 0 || s.Length < t.Length)
        //        return -1;
        //    int len = t_end - t_start;
        //    int* next = stackalloc int[len];
        //    InitNext(t, t_start, t_end, next);
        //    //int[] next = InitNext(t, t_start, t_end);
        //    int i = s_start, j;
        //    while (i < s_end)
        //    {
        //        for (j = t_start; j < t_end && i < s_end;)
        //        {
        //            if (s[i] == t[j])
        //            {
        //                i++;
        //                j++;
        //            }
        //            else
        //            {
        //                i = j > 0 ? i - next[j - 1 - t_start] : i + 1;
        //                break;
        //            }
        //        }
        //        if (j == t_end)
        //            return i - len;
        //    }
        //    return -1;
        //}

        //static unsafe void InitNext(byte[] t, int start, int end, int* ret)
        //{
        //    //if (start >= end) throw new IndexOutOfRangeException();
        //    //int[] ret = new int[end - start];
        //    for (int i = 0; i < end - start; i++)
        //        ret[i] = 0;
        //    for (int i = start + 1; i < end; i++)
        //    {
        //        int temp = start;
        //        int j = start;

        //        while (i < end && j < end && t[i] == t[j])
        //        {
        //            ret[i] = ++temp;
        //            i++;
        //            j++;
        //        }
        //    }
        //}

        //static int[] InitNext(byte[] t, int start, int end)
        //{
        //    if (start >= end) throw new IndexOutOfRangeException();
        //    int[] ret = new int[end - start];
        //    //for (int i = 0; i < t.Length; i++)
        //    //ret[i] = 0;
        //    for (int i = start + 1; i < end; i++)
        //    {
        //        int temp = start;
        //        int j = start;

        //        while (i < end && j < end && t[i] == t[j])
        //        {
        //            ret[i] = ++temp;
        //            i++;
        //            j++;
        //        }
        //    }
        //    return ret;
        //}

        //static int[] InitNext(byte[] t)
        //{
        //    int[] ret = new int[t.Length];
        //    //for (int i = 0; i < t.Length; i++)
        //    //ret[i] = 0;
        //    for (int i = 1; i < t.Length; i++)
        //    {
        //        int temp = 0;
        //        int j = 0;

        //        while (i < t.Length && j < t.Length && t[i] == t[j])
        //        {
        //            ret[i] = ++temp;
        //            i++;
        //            j++;
        //        }
        //    }
        //    return ret;
        //}

        public static Color[] ConvertGBAPalTo24Bit(byte[] palette)
        {
            int length = palette.Length >> 1;
            Color[] new_palette = new Color[length];
            byte mask = 0x1F;
            for (int n = 0; n < length; n++)
            {
                short color = (short)((palette[2 * n + 1] << 8) | palette[2 * n]);

                new_palette[n] = Color.FromArgb(
                    //red
                    (byte)((color & mask) << 3),
                    //green
                    (byte)(((color >> 5) & mask) << 3),
                    //blue
                    (byte)(((color >> 10) & mask) << 3));
            }
            return new_palette;
        }


        public static unsafe Bitmap ConvertGBAImageToBitmap(byte[] img, Color[] palette,
            int width, int height)
        {
            Bitmap bm = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            return ConvertGBAImageToBitmap(img, palette, width, height, bm);
        }

        public static Bitmap ConvertGBAImageToBitmapIndexed(byte[] img, byte[] pal,
            int width, int height)
        {
            Color[] palette = ConvertGBAPalTo24Bit(pal);
            return ConvertGBAImageToBitmapIndexed(img, palette, width, height);
        }

        public static unsafe Bitmap ConvertGBAImageToBitmapIndexed(Bitmap img)
        {
            List<Color> palette = new List<Color>(32);
            GetImgColor(img, palette);
            Bitmap p = new Bitmap(img.Width, img.Height, PixelFormat.Format8bppIndexed);
            BitmapData bd = p.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);
            ColorPalette c = p.Palette;
            for (int x = 0; x < palette.Count; x++)
            {
                c.Entries[x] = palette[x];
            }
            p.Palette = c;
            byte* ptr = (byte*)bd.Scan0.ToPointer();
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    *ptr++ = (byte)palette.IndexOf(img.GetPixel(j, i));
                }
            }
            p.UnlockBits(bd);
            return p;
        }

        //返回8位索引模式的位图
        public static unsafe Bitmap ConvertGBAImageToBitmapIndexed(byte[] img,
            Color[] palette, int width, int height)
        {

            Bitmap p = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bd = p.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format8bppIndexed);
            ColorPalette c = p.Palette;
            for (int x = 0; x < palette.Length; x++)
            {
                c.Entries[x] = palette[x];
            }
            p.Palette = c;
            byte[] data = new byte[img.Length * 2];
            for (int x = 0; x < img.Length; x++)
            {
                byte pixe = img[x];
                data[2 * x] = (byte)(pixe & 0xF);
                data[x * 2 + 1] = (byte)(pixe >> 4);
            }

            byte* ptr = (byte*)bd.Scan0.ToPointer();
            int k = 0;

            for (int y = 0; y < (height >> 3); y++)
            {
                int yrow = y << 3;
                //横着的方向也除以8 横着填充
                for (int x = 0; x < (width >> 3); x++)
                {
                    //竖着
                    int xrow = x << 3;
                    for (int a = yrow; a < yrow + 8; a++)
                    {
                        //每行扫描的个数。相当于像素的行宽
                        int stride = a * bd.Stride;
                        //横着 继续一横一横的填充
                        for (int b = xrow; b < xrow + 8; b++)
                        {
                            ptr[stride + b] = data[k];
                            k++;
                        }
                    }
                }
            }
            p.UnlockBits(bd);
            return p;
        }



        public static unsafe Bitmap ConvertGBAImageToBitmap(byte[] img,
            Color[] palette, int width, int height, Bitmap bm)
        {
            int length = img.Length;
            byte* indexed_image = stackalloc byte[length * 2];
            byte* pi = indexed_image;
            for (int x = 0; x < length; x++)
            {
                byte pixe = img[x];
                *pi++ = (byte)(pixe & 0xF);
                *pi++ = (byte)(pixe >> 4);
            }
            pi = indexed_image;
            int xrow = 0, yrow = 0;
            //GBA的图像被分成了x*y=8*8的块储存的,
            //所以填充bitmap的时候就以每8*8为小单位来填充
            //一行一行的填充
            //竖着的方向分成/8
            for (int x = 0; x < (height >> 3); x++)
            {
                //横着的方向也除以8
                for (int y = 0; y < (width >> 3); y++)
                {
                    //竖着
                    xrow = x << 3;
                    yrow = y << 3;
                    for (int a = xrow; a < xrow + 8; a++)
                    {
                        //横着
                        for (int b = yrow; b < yrow + 8; b++)
                        {
                            int k = *pi++;
                            bm.SetPixel(b, a, palette[k]);
                        }
                    }
                }
            }
            return bm;
        }

        public static Bitmap GetImg(BinaryReader rom, int imgoffset,
            int paletteOffset, int width, int height)
        {
            byte[] img = LZUncompress(rom, imgoffset).Item1;
            byte[] palette_data = LZUncompress(rom, paletteOffset).Item1;
            //Color[] palette = ConvertGBAPalTo24Bit(palette_data);
            return ConvertGBAImageToBitmapIndexed(img, palette_data, width, height);
        }

        /// <summary>
        /// 导出压缩后的图片和调色板
        /// </summary>
        /// <param name="bm"></param>
        public static unsafe Tuple<byte[], byte[]> ConvertNormalImagToGBA(Bitmap bm)
        {

            List<Color> colors = new List<Color>();

            int xrow = 0, yrow = 0;
            byte[] img_data = new byte[(bm.Width * bm.Height) >> 1];
            int color1 = -1, color2 = -1;
            fixed (byte* pImg = img_data)
            {
                byte* pI = pImg;
                for (int x = 0; x < bm.Height >> 3; x++)
                {
                    //横着的方向也除以8
                    for (int y = 0; y < bm.Width >> 3; y++)
                    {
                        //竖着
                        xrow = x << 3;
                        yrow = y << 3;
                        for (int a = xrow; a < xrow + 8; a++)
                        {
                            //横着
                            for (int b = yrow; b < yrow + 8; b++)
                            {
                                if (color1 == -1)
                                {
                                    Color c = bm.GetPixel(b, a);
                                    color1 = colors.FindIndex(n => c == n);
                                    if (color1 == -1)
                                    {
                                        color1 = colors.Count;
                                        colors.Add(c);
                                    }
                                }
                                else
                                {
                                    Color c = bm.GetPixel(b, a);
                                    color2 = colors.FindIndex(n => c == n);
                                    if (color2 == -1)
                                    {
                                        color2 = colors.Count;
                                        colors.Add(c);
                                    }
                                    *pI++ = (byte)((color2 << 4) | color1);
                                    color1 = -1;
                                }
                            }
                        }
                    }
                }
            }
            while (colors.Count < 16)
            {
                colors.Add(Color.White);
            }
            byte[] pal = ConvertToGBAPal(colors);
            img_data = LZCompress(img_data);

            pal = LZCompress(pal);
            return new Tuple<byte[], byte[]>(img_data, pal);
        }
        public static unsafe byte[] ConvertNormalImagToGBA(Bitmap bm, List<Color> colors)
        {
            return ConvertNormalImagToGBA(bm, colors, true);
        }

        public static unsafe byte[] ConvertNormalImagToGBA(Bitmap bm, List<Color> colors, bool compress)
        {
            int xrow = 0, yrow = 0;
            byte[] img_data = new byte[(bm.Width * bm.Height) >> 1];
            int color1 = -1, color2 = -1;
            fixed (byte* pImg = img_data)
            {
                byte* pI = pImg;
                for (int x = 0; x < bm.Height >> 3; x++)
                {
                    //横着的方向也除以8
                    for (int y = 0; y < bm.Width >> 3; y++)
                    {
                        //竖着
                        xrow = x << 3;
                        yrow = y << 3;
                        for (int a = xrow; a < xrow + 8; a++)
                        {
                            //横着
                            for (int b = yrow; b < yrow + 8; b++)
                            {
                                if (color1 == -1)
                                {
                                    Color c = bm.GetPixel(b, a);
                                    color1 = colors.FindIndex(n => c == n);
                                    if (color1 == -1)
                                    {
                                        color1 = BestMatch(c, colors);
                                    }
                                }
                                else
                                {
                                    Color c = bm.GetPixel(b, a);
                                    color2 = colors.FindIndex(n => c == n);
                                    if (color2 == -1)
                                    {
                                        color2 = BestMatch(c, colors);
                                    }
                                    *pI++ = (byte)((color2 << 4) | color1);
                                    color1 = -1;
                                }
                            }
                        }
                    }
                }
            }
            if (compress)
                img_data = LZCompress(img_data);
            return img_data;
        }

        public static Bitmap GetbyBitMap(Bitmap bm, List<Color> colors)
        {
            Bitmap new_bm = new Bitmap(bm.Width, bm.Height, PixelFormat.Format24bppRgb);
            Dictionary<Color, Color> cd = new Dictionary<Color, Color>();
            Color color;
            for (int x = 0; x < bm.Width; x++)
            {
                for (int y = 0; y < bm.Height; y++)
                {
                    Color old_color = bm.GetPixel(x, y);

                    if (!cd.ContainsKey(old_color))
                    {
                        int index = BestMatch(old_color, colors);
                        cd[old_color] = color = colors[index];
                    }
                    else { color = cd[old_color]; }
                    new_bm.SetPixel(x, y, color);
                }
            }
            return new_bm;
        }

        public static byte[] ConvertToGBAPal(List<Color> c)
        {
            while (c.Count < 16)
            {
                c.Add(Color.White);
            }
            byte[] pal = new byte[32];
            int count = 0;
            while (count < 32)
            {
                Color n = c[count >> 1];
                int red = n.R;
                int green = n.G;
                int blue = n.B;
                red = red >> 3;
                green = ((green >> 3) << 5);
                blue = ((blue >> 3) << 10);
                int color = (blue | green | red);
                pal[count++] = (byte)color;
                pal[count++] = (byte)(color >> 8);
            }
            return pal;
        }

        public static double Distance(Color c1, Color c2)
        {
            double a = Math.Pow(c1.R - c2.R, 2) +
                Math.Pow(c1.G - c2.G, 2) +
                Math.Pow(c1.B - c2.B, 2);
            a = Math.Pow(a, 0.5);
            return a;
        }

        public static byte BestMatch(Color sample, List<Color> colors)
        {
            double min_len = long.MaxValue;
            byte index = 0;
            byte best = 0;
            foreach (Color color in colors)
            {
                double len = Distance(sample, color);
                if (min_len > len)
                {
                    min_len = len;
                    best = index;
                }
                index++;
            }
            return best;
        }

        public static void GetImgColor(Bitmap bm, List<Color> colors)
        {
            colors.Clear();
            int xrow = 0, yrow = 0;
            for (int x = 0; x < bm.Height >> 3; x++)
            {
                for (int y = 0; y < bm.Width >> 3; y++)
                {
                    xrow = x << 3;
                    yrow = y << 3;
                    for (int a = xrow; a < xrow + 8; a++)
                    {
                        for (int b = yrow; b < yrow + 8; b++)
                        {
                            Color c = bm.GetPixel(b, a);

                            if (!colors.Contains(c))
                            {
                                colors.Add(c);
                            }

                        }
                    }

                }
            }
        }

        /// <summary>
        /// 根据普通图片，返回闪光色盘和普通色板。
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="shiny"></param>
        /// <param name="colors"></param>
        /// <param name="shinyColors"></param>
        public static void GetImgColor(Bitmap normal, Bitmap shiny, List<Color> colors, List<Color> shinyColors)
        {
            int xrow = 0, yrow = 0;
            for (int x = 0; x < normal.Height >> 3; x++)
            {
                for (int y = 0; y < normal.Width >> 3; y++)
                {
                    xrow = x << 3;
                    yrow = y << 3;
                    for (int a = xrow; a < xrow + 8; a++)
                    {
                        for (int b = yrow; b < yrow + 8; b++)
                        {
                            Color c = normal.GetPixel(b, a);

                            if (!colors.Contains(c))
                            {
                                colors.Add(c);
                                Color d = shiny.GetPixel(b, a);
                                if (!shinyColors.Contains(d))
                                    shinyColors.Add(d);
                                else
                                    shinyColors.Add(c);
                                if (colors.Count == 16)
                                    return;
                            }

                        }
                    }

                }
            }
        }

        public static Bitmap FromRaw(Bitmap bm, string raw_path, PixelFormat pixel)
        {
            Bitmap bitmap = new Bitmap(512, 512);
            Graphics e = Graphics.FromImage(bitmap);
            FileStream fs = new FileStream(raw_path, FileMode.Open);
            Rectangle rectangle = new Rectangle(0, 0, 8, 8);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    int index = fs.ReadByte();
                    rectangle.X = (index % (bm.Width >> 3)) << 3;
                    rectangle.Y = (index / (bm.Width >> 3)) << 3;
                    Bitmap b = bm.Clone(rectangle, bitmap.PixelFormat);
                    rectangle.X = x << 3;
                    rectangle.Y = y << 3;
                    e.DrawImage(b, rectangle);
                }
            }
            return bitmap;
        }

        //
        /*
        public unsafe static void toRaw(Bitmap bm, string directory, writeRaw _4)//true = 4,false=8
        {
            if (bm.PixelFormat != PixelFormat.Format8bppIndexed)
                bm = ConvertGBAImageToBitmapIndexed(bm);
            Dictionary<ByteArray, byte> bitmaps = new Dictionary<ByteArray, byte>(256);
            BitmapData bd = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            long* ptr = (long*)bd.Scan0.ToPointer();
            ByteArray data = new ByteArray(new long[8]);
            byte count = 0;
            FileStream fs = new FileStream(string.Concat(directory, "/raw1.raw"), FileMode.Create);
            Bitmap result = new Bitmap(8, 128 * 16, PixelFormat.Format8bppIndexed)
            {
                Palette = bm.Palette
            };
            for(int y = 0;y < bm.Height >> 3; y++)
            {
                for (int x = 0, width = (bm.Width >> 3); x < width; x++)
                {
                    int block = y * width * 8 + x;
                    for (int i = 0; i < 8; i++)
                    {
                        data[i] = ptr[block + i * width];
                    }
                    if (!bitmaps.TryGetValue(data, out byte v))
                    {
                        bitmaps[(ByteArray)data.Clone()] = count;
                        v = count;
                        WriteData(result, data.data, count);
                        count++;
                    }
                    //fs.WriteByte(v);
                    //_4(fs,v,data.data);
                }
            }
            bm.UnlockBits(bd);
            bm.Dispose();
            fs.Close();
            count = (count & 7) == 0 ? count : (byte)(count + 8 - (count & 7));
            bm = result.Clone(new Rectangle(0, 0, 8, 8 * count), PixelFormat.Format8bppIndexed);
            result.Dispose();
            bm.Save(string.Concat(directory, "/tile1.png"), ImageFormat.Png);
            bm.Dispose();
        }
        */
        public unsafe static void toRaw(Bitmap bm, string directory)//true = 4,false=8
        {
            if (bm.PixelFormat != PixelFormat.Format8bppIndexed)
                bm = ConvertGBAImageToBitmapIndexed(bm);
            Dictionary<ByteArray, ushort> bitmaps = new Dictionary<ByteArray, ushort>(0x400);
            BitmapData bd = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            long* ptr = (long*)bd.Scan0.ToPointer();
            ByteArray data = new ByteArray8();
            ushort count = 0;
            Bitmap result = new Bitmap(8 * 8, 128 * 8, PixelFormat.Format8bppIndexed)
            {
                Palette = bm.Palette
            };
            BinaryWriter bw = new BinaryWriter(new FileStream(directory + "/raw1.raw", FileMode.Create));
            for (int y = 0; y < bm.Height >> 3; y++) {
                for (int x = 0, width = (bm.Width >> 3); x < width; x++) {
                    int block = y * width * 8 + x;
                    for (int i = 0; i < 8; i++) {
                        data[i] = ptr[block + i * width];
                    }
                    if (!bitmaps.TryGetValue(data, out ushort v)) {
                        bitmaps[(ByteArray)data.Clone()] = count;
                        v = count;
                        data.WriteToTileset(result, count);
                        count++;
                    }
                    v = (ushort)(v | (data.data[0] >> 4 << 12));
                    if (ByteArray8.x_reverse)
                        v |= ByteArray.X_MASK;
                    if (ByteArray8.y_reverse)
                        v |= ByteArray.Y_MASK;
                    bw.Write(v);
                }
                bw.Flush();
            }
            bm.UnlockBits(bd);
            bw.Close();
            count = (count % 8) == 0 ? count : (ushort)(count + 8 - (count % 8));
            bm = result.Clone(new Rectangle(0, 0, result.Width, count), PixelFormat.Format8bppIndexed);
            bm.Save(string.Concat(directory, "/tile1.png"), ImageFormat.Png);
        }


        public static BitmapData LockBits(this Bitmap bm)
        {
            return bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadWrite,
                bm.PixelFormat);
        }

        private static void WriteRaw4(FileStream fs,byte v, long[] data)
        {
            //fixme 低10位raw index,中间2位是否反转，高4位调色板编号，最多1024块
            fs.WriteByte(v);//raw屏幕大小240x160
            //是否反转尚未实现
            fs.WriteByte((byte)(data[0] >> 60 << 4));//调色板索引
        }

        private static void WriteRaw8(FileStream fs,byte v, long[] data){
            fs.WriteByte(v);//直接写入raw编号。占用一个字节，最多256块,比4更节省空间
        }

        public static writeRaw writeRaw4 = new writeRaw(WriteRaw4);
        public static writeRaw writeRaw8 = new writeRaw(WriteRaw8);
        /*
        public static unsafe void WriteData(Bitmap bm, long[] data, int block)
        {
            int x = (block % (bm.Width >> 3)) << 3;
            int y = (block / (bm.Width >> 3)) << 3;
            BitmapData bd = bm.LockBits(new Rectangle(x, y, 8, 8),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            long* ptr = (long*)bd.Scan0.ToPointer();
            for (int a = 0, stride = bm.Width >> 3; a < 8; a++)
            {
                ptr[a * stride] = data[a];
            }
            bm.UnlockBits(bd);
        }
        */
        
    }

    public unsafe class ByteArray : ICloneable
    {
        public byte[] data;
        protected int hash;

        public const ushort X_MASK = 0b10000000000;
        public const ushort Y_MASK = 0b100000000000;
        public ByteArray(int size)
        {
            data = new byte[size * 8];
            ByteArray8.x_reverse = false;
            ByteArray8.y_reverse = true;
        }
        public int Length {
            get { return data.Length / 8; }
        }

        public unsafe long this[int k] {

            get {
                if (k > Length) throw new IndexOutOfRangeException();
                return BitConverter.ToInt64(data, k * 8);
            }
            set {
                if (k > Length) throw new IndexOutOfRangeException();
                fixed (void* a = data) {
                    ((long*)a)[k] = value;
                }
            }
        }

        


        public override int GetHashCode()
        {
            if (this.hash == 0) {
                long hash = 0;
                for (int var3 = 0; var3 < Length; ++var3) {
                    hash = 31 * hash + this[var3];
                }
                this.hash = (int)hash;
            }
            return this.hash;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj is ByteArray value && data.Length == value.data.Length) {
                for (int i = 0; i < Length; i++) {
                    if (value[i] != this[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        public object Clone()
        {
            ByteArray a = (ByteArray)MemberwiseClone();
            a.data = (byte[])data.Clone();
            return a;
        }

        public unsafe void WriteToTileset(Bitmap bm, int block)
        {
            //if (block >= 0x200) return;
            int x = (block % (bm.Width >> 3)) << 3;
            int y = (block / (bm.Width >> 3)) << 3;
            BitmapData bd = bm.LockBits(new Rectangle(x, y, 8, 8),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            long* ptr = (long*)bd.Scan0.ToPointer();
            for (int a = 0, stride = bm.Width >> 3; a < 8; a++) {
                ptr[a * stride] = this[a];//- PalIndex((byte)(data.data[a * 8] >> 4));
            }
            bm.UnlockBits(bd);
        }
    }

    public class ByteArray8 : ByteArray
    {
        public static bool x_reverse;
        public static bool y_reverse;
        public ByteArray8() : base(8) { }


        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            x_reverse = false;
            y_reverse = false;
            if (obj is ByteArray8 value && data.Length == value.data.Length) {
                if (Equ(value)) return true;
                XEqu(value);
                YEqu(value);
                return x_reverse || y_reverse;
            }
            return false;
        }

        private bool Equ(ByteArray8 value)
        {
            for (int i = 0; i < data.Length; i += 8) {
                if ((BitConverter.ToInt64(data, i) & 0x0F0F0F0F0F0F0F0F) !=
                    (BitConverter.ToInt64(value.data, i) & 0x0F0F0F0F0F0F0F0F))
                    return false;
            }
            return true;
        }

        private void XEqu(ByteArray8 value)
        {
            for (int i = 0; i < data.Length; i += 8) {
                for (int j = 0; j < 8; j++) {
                    if ((data[i + j] & 15) != (value.data[i + 7 - j] & 15))
                        return;
                }
            }
            x_reverse = true;
        }

        private void YEqu(ByteArray8 value)
        {
            if (x_reverse) {
                for (int i = 0; i < data.Length; i += 8) {
                    for (int j = 0; j < 8; j++) {
                        if ((data[i + j] & 15) != (value.data[56 - i + 7 - j] & 15))
                            return;
                    }
                }
            }
            else {
                for (int i = 0; i < data.Length; i += 8) {
                    for (int j = 0; j < 8; j++) {
                        if ((data[i + j] & 15) != (value.data[56 - i + j] & 15))
                            return;
                    }
                }
            }
            y_reverse = true;
        }


        public override int GetHashCode()
        {
            if (this.hash == 0) {
                int hash = 0;
                for (int var3 = 0; var3 < data.Length; ++var3) {
                    hash = 31 * hash + data[var3];
                }
                this.hash = hash;
            }
            return this.hash;
        }
    }
}
