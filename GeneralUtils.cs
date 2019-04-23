using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Tools
{

	public struct RomOffset
	{
		private int offset;

		public RomOffset(int offset)
		{
			this.offset = offset;
			Compressed = false;
		}

		public RomOffset(int offset, bool compressed)
		{
			this.offset = offset;
			Compressed = compressed;
		}

		public int Offset {
			get {
				if (offset > ReadAndWrite.ROM)
					offset -= ReadAndWrite.ROM;
				if (offset > 0x2000000 || offset < 0)
					throw new EndOfStreamException(Decompiler.Operation.Hex(offset));
				return offset;
			}
		}

		public bool Compressed { get; private set; }
	}

	public class ReadAndWrite : IDisposable
	{
		public FileStream fs;
		public ReadAndWrite(String path)
		{
			SetPath(path);
		}
		public const int ROM = 0x8000000;

		public ReadAndWrite() { SetPath(MainWindow.rom_path); }

		public void SetPath(String path)
		{
			fs = new FileStream(path, FileMode.Open,
				FileAccess.ReadWrite, FileShare.ReadWrite);
			Br = new BinaryReader(fs);
			Bw = new BinaryWriter(fs);
		}

		public int Position {
			get { return (int)fs.Position; }
		}

		public BinaryReader Br { get; private set; }

		public BinaryWriter Bw { get; private set; }

		public void Seek(int start)
		{
			if (start > ROM)
				start -= ROM;
			if (start > 0x2000000 || start < 0)
				throw new EndOfStreamException(Decompiler.Operation.Hex(start));
			fs.Seek(start, SeekOrigin.Begin);
		}

		public byte[] ReadBytes(int offset, int num, int size)
		{
			Seek(offset + size * num);
			return Br.ReadBytes(size);
		}

		public short[] ReadShort(int offset, int count)
		{
			byte[] data = ReadBytes(offset, 0, count * 2);
			short[] result = new short[count];
			Buffer.BlockCopy(data, 0, result, 0, data.Length);
			return result;
		}

		public Object GetStruct(int offset, int num, Type type)
		{
			int size = Marshal.SizeOf(type);
			byte[] data = ReadBytes(offset, num, size);
			return StructsUtil.ByteToStruct(data, type);
		}

		public T GetStruct<T>(int offset, int num) where T : struct
		{
			int size = Marshal.SizeOf(typeof(T));
			byte[] data = ReadBytes(offset, num, size);
			return (T)StructsUtil.ByteToStruct(data, typeof(T));
		}

		public int getStructOffset(Type type, int pointer, int num)
		{
			int size = Marshal.SizeOf(type);
			pointer = ReadPointer(pointer);
			return size * num + pointer;
		}



		public Bitmap GetBitMap(int img, Color[] pal, int width, int height)
		{
			int length = (height * width) << 1;
			Seek(img);
			byte[] img_data = Br.ReadBytes(length);
			return ImgFunction.ConvertGBAImageToBitmap(img_data, pal, width, height);
		}

		public Bitmap GetIndexedBitmapNotLz(int img, Color[] pal, int width, int height)
		{
			int length = (height * width) << 1;
			Seek(img);
			byte[] img_data = Br.ReadBytes(length);
			return ImgFunction.ConvertGBAImageToBitmapIndexed(img_data, pal, width, height);
		}

		//必须传入写满的数据
		public void WriteBytes(int offset, int num, byte[] data)
		{
			Seek(offset + num * data.Length);
			Bw.Write(data);
		}

		public void Save(int offset, int num, ValueType obj)
		{
			byte[] data = StructsUtil.StructToByte(obj);
			WriteBytes(offset, num, data);
		}

		public int ReadPointer(int offset)
		{
			Seek(offset);
			int k = Br.ReadInt32() - 0x8000000;
			if (k < 0)
				throw new NullReferenceException(Convert.ToString(offset, 16));
			return k;
		}

		public static String TransToCH(byte[] data)
		{
			StringBuilder sb = new StringBuilder();
			int len = data.Length;
			//保留一位，因为至少最后一位是0xFF
			for (int x = 0; x < len - 1; x++) {
				byte i = data[x];
				//中文字符
				if (i >= 1 && i <= 0x1e) {
					if (i >= 7)
						i--;
					if (i >= 0x1b)
						i--;
					sb.Append(WordsDecoding.char_CH[i - 1, data[x + 1]]);

					x++;
				}
				else if (i == 0xFD) {
					sb.Append("[FD," + Convert.ToString(data[x + 1], 16) + "]");
					x++;
				}
				else if (i == 0xFF) {
					break;
				}
				else {
					sb.Append(WordsDecoding.char_EN[i]);
				}
			}
			return sb.ToString();
		}

		public static String TransToCH(BinaryReader br, int offset)
		{
			if (offset > 0x8000000)
				offset -= 0x8000000;
			if (offset > 0x2000000 || offset < 0)
				throw new ArgumentException(Decompiler.Operation.Hex(offset));
			br.BaseStream.Seek(offset, SeekOrigin.Begin);
			StringBuilder sb = new StringBuilder();
			//最多循环150次
			for (int x = 0; x < 150; x++) {
				byte i = br.ReadByte();
				//中文字符
				if (i >= 1 && i <= 0x1e) {
					if (i >= 7)
						i--;
					if (i >= 0x1b)
						i--;
					sb.Append(WordsDecoding.char_CH[i - 1, br.ReadByte()]);
				}
				else if (i == 0xFD) {
					sb.Append("[FD," + br.ReadByte() + "]");
				}
				else if (i == 0xFF) {
					break;
				}
				else {
					char k = WordsDecoding.char_EN[i];
					if (k == '-')
						sb.Append(i);
					else
						sb.Append(k);
				}
			}
			return sb.ToString();
		}


		public static unsafe byte[] Encode(String words, int max_length)
		{
			byte* va = stackalloc byte[128];
			ByteList encoded = new ByteList(va, 128);
			fixed (char* pEncode = WordsDecoding.char_CH) {
				foreach (char word in words) {
					if (word <= 0xFF) {
						encoded.Add((byte)Array.IndexOf(WordsDecoding.char_EN, word));
						continue;
					}
					char* pe = pEncode;
					for (byte x = 1; x < 29; x++) {
						byte y = 0;
						while (true) {
							if (*pe++ == word) {
								if (x >= 6)
									x++;
								if (x >= 0x1a)
									x++;
								encoded.Add(x);
								encoded.Add(y);
								goto Loop;
							}
							if (y == 0xFF)
								break;
							y++;
						}
					}
					Loop:
					continue;
				}
			}
			if (encoded.count > max_length - 1) {
				encoded.count = max_length - 1;
			}
			encoded.Add(0xFF);
			while (encoded.count < max_length)
				encoded.Add(0);
			return encoded.ToArray();
		}

		public String[] GetObjectNames(int pointer, int size, int total)
		{
			String[] src = new String[total];
			int Nameoffset = ReadPointer(pointer);
			fs.Seek(Nameoffset, SeekOrigin.Begin);

			for (int x = 0; x < total; x++) {
				byte[] name = Br.ReadBytes(size);
				src[x] = TransToCH(name);
			}
			return src;
		}

		public short[] GetHmOrToutorList(int offset, int total)
		{
			byte[] tem = ReadBytes(offset, 0, total * 2);
			short[] s = new short[tem.Length / 2];
			Buffer.BlockCopy(tem, 0, s, 0, tem.Length);
			Array.Reverse(s);
			return s;
		}

		//获取技能学习面
		public char[] GetCompatibility(int offset, int size, int total, int pokenum)
		{
			fs.Seek(offset + pokenum * size, SeekOrigin.Begin);
			byte[] data = Br.ReadBytes(size);
			//读取字节数组后反转
			Array.Reverse(data);
			StringBuilder str = new StringBuilder(data.Length * 8);
			foreach (byte d in data)
				str.Append(Convert.ToString(d, 2).PadLeft(8, '0'));
			String result = str.ToString().Substring(size * 8 - total);
			//转成0101形式的2进制字符数组，这样直观一点。
			return result.ToCharArray();
		}


		public void Repoint(int old_offset, int new_offset)
		{
			Seek(old_offset);
			if (new_offset <= 0x8000000)
				new_offset += 0x8000000;
			Bw.Write(new_offset);
		}


		public int FindFreeOffset(int start, int len)
		{
			return FindFreeOffset(start, len, n => n == 0xFF || n == 0);
		}

		public int FindFreeOffset0xFF(int start, int len)
		{
			return FindFreeOffset(start, len, n => n == 0xFF);
		}

		private unsafe int FindFreeOffset(int start, int len, Predicate<byte> match)
		{
			int mod_x4 = start & 3;
			if (mod_x4 != 0) {
				start += 4 - mod_x4;
			}
			Seek(start);
			byte[] data = Br.ReadBytes((int)fs.Length - start);
			int store = len;
			//如果不用指针那个速度真是慢的要死,最后会稍微检查一下下标是否越界
			fixed (byte* pdata = data) {
				byte* pd = pdata;
				while (len > 0) {
					if (match(*pd++))
						len--;
					else {
						len = store;
						mod_x4 = ((int)(pd - pdata)) & 3;//(2*2-1)
						if (mod_x4 != 0) {
							pd += 4 - mod_x4;
						}
					}
				}
				start = (int)(pd - pdata) + start;
				if (start >= 0x2000000)
					throw new EndOfStreamException();
				start -= store;
			}
			data = null;
			return start;
		}

		public int FindFreeOffset0xFF(int len)
		{
			int start = FindFreeOffset0xFF(PokeConfig.ini.start_offset, len);
			PokeConfig.ini.start_offset = start;
			return start;
		}

		public int Save(byte[] data, int offset_to_repoint, bool fill)
		{
			//指针所在地址
			int offset_old = ReadPointer(offset_to_repoint);
			//新地址
			int start = PokeConfig.ini.start_offset;
			int offset_new = FindFreeOffset(start, data.Length);
			WriteBytes(offset_new, 0, data);
			start = Position;
			Repoint(offset_to_repoint, offset_new);
			if (fill) {
				FillWith0xFF(offset_old, data.Length);
			}
			PokeConfig.ini.start_offset = offset_new;
			return offset_new;
		}

		public void FillWith0xFF(int start, int len)
		{
			Seek(start);
			for (int x = 0; x < len / 8; x++)
				Bw.Write(-1L);
			for (int x = 0; x < len % 8; x++)
				Bw.Write((byte)0xFF);
		}

		public byte[] ReadAndClear(int start, int len, bool clear)
		{
			byte[] data = ReadBytes(start, 0, len);
			if (clear)
				FillWith0xFF(start, len);
			return data;
		}

		public void Close()
		{
			Br.Close();
			Bw.Close();
		}

		public void Dispose()
		{
			Br.Dispose();
			Bw.Dispose();
			Close();
			GC.SuppressFinalize(this);
		}
	}

	class PaintUtils
	{
		public Point p1 = Point.Empty;
		public Rectangle r = new Rectangle(Point.Empty, MIN_SIZE);
		Bitmap image;
		List<Color> colors;
		public readonly static Size MIN_SIZE = new Size(16, 16);

		public PaintUtils()
		{
		}

		public String ColorFile { get; private set; }


		public Bitmap Image {
			get { return image; }
			set {
				image = value;
				//DrawColor(color_panel);
			}
		}

		public List<Color> Colors {
			set {

				foreach (Color c in value) {
					if (!colors.Contains(c))
						return;
				}

				colors = value;
				//DrawColor(color_panel);
			}
			get { return colors; }
		}

		public void setColors(Color[] pal)
		{
			for (int x = 0; x < 16; x++)
				colors[x] = pal[x];
			//DrawColor(color_panel);
		}
		//public void DrawColor(Color[] pal) {
		//	Graphics g = Graphics.FromImage(color_panel.BackgroundImage);
		//	Rectangle r = new Rectangle(0, 0, 16, 16);

		//	for (int x = 0; x < 16; r.Y += 16) {
		//		r.X = 0;
		//		g.FillRectangle(new SolidBrush(pal[x++]), r);
		//		r.X = 16;
		//		g.FillRectangle(new SolidBrush(pal[x++]), r);
		//	}
		//}
		public List<Color> GetFromFile(String file)
		{
			FileStream fs = new FileStream(file, FileMode.Open);
			if (fs.Length < 64)
				throw new IOException("文件过短!");
			List<Color> colors = new List<Color>(16);
			BinaryReader br = new BinaryReader(fs);
			for (int x = 0; x < 16; x++) {
				colors.Add(Color.FromArgb(br.ReadInt32()));
			}
			ColorFile = file;
			br.Close();
			return colors;
		}

		public void open()
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				Filter = "调色板文件|*.pal"
			};

			if (dialog.ShowDialog() == DialogResult.OK) {
				Colors = GetFromFile(dialog.FileName);
			}
		}

		public bool SaveColor()
		{
			if (ColorFile is null)
				return false;
			FileStream fs = new FileStream(ColorFile, FileMode.Open);
			BinaryWriter bw = new BinaryWriter(fs);
			foreach (Color c in colors) {
				int k = c.ToArgb();
				k <<= 8;
				k >>= 8;
				bw.Write(k);
			}

			bw.Close();
			return true;
		}

		public void DrawColor(Panel p)
		{
			int a = (Colors.Count | 1) - 1;
			if (a > 44)
				a = 44;
			p.Height = a >> 1 << 4;
			p.BackgroundImage = new Bitmap(p.Width, p.Height, PixelFormat.Format24bppRgb);

			Graphics g = Graphics.FromImage(p.BackgroundImage);
			Rectangle r = new Rectangle(0, 0, 16, 16);

			for (int x = 0; x < a; r.Y += 16) {
				r.X = 0;
				g.FillRectangle(new SolidBrush(Colors[x++]), r);
				r.X = 16;
				g.FillRectangle(new SolidBrush(Colors[x++]), r);
			}
		}

		public void ReColor(Bitmap image, Color old, Color new_color)
		{
			for (int x = 0; x < image.Width; x++) {
				for (int y = 0; y < image.Height; y++) {
					if (image.GetPixel(x, y) == old)
						image.SetPixel(x, y, new_color);
				}
			}
		}

		public void ReColorSelf(Color old, ref Color new_color)
		{
			int index = colors.FindIndex(n => old == n);
			if (index >= 0) {
				ReColor(image, old, new_color);
				colors[index] = new_color;
			}
		}



	}
}
