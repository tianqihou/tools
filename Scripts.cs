using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace Decompiler
{
    public partial class Move_Ainmation
    {
        BinaryReader br;
        int particle_offset;
		StreamWriter ai;

        public Move_Ainmation(BinaryReader br, StreamWriter sw)
        {
            this.br = br;
            br.BaseStream.Seek(0xA3DD0, SeekOrigin.Begin);
            particle_offset = br.ReadInt32() - 0x8000000;
            ai= sw;
        }

        byte[] needed_retrans = { 0x11, 0xe, 0x24};

        List<int> Temple_offsets = new List<int>();

        int GetParticle(int particle_id)
        {
            particle_id -= 0x2710;
            br.BaseStream.Seek(particle_offset + (particle_id << 3), SeekOrigin.Begin);
            return br.ReadInt32() - 0x8000000;
        }

        String GetTemple(int start)
        {
            if (start > 0x8000000)
                start -= 0x8000000;
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            StringBuilder sb = new StringBuilder();
            sb.Append("objtemplate 0x");
            sb.Append(Convert.ToString(br.ReadInt16(), 16) + " 0x");
            sb.Append(Convert.ToString(br.ReadInt16(), 16) + " 0x");
            sb.Append(Convert.ToString(br.ReadInt32(), 16) + " 0x");
            sb.Append(Convert.ToString(br.ReadInt32(), 16) + " 0x");
            sb.Append(Convert.ToString(br.ReadInt32(), 16) + " 0x");
			sb.Append(Convert.ToString(br.ReadInt32(), 16) + " 0x");
			sb.Append(Convert.ToString(br.ReadInt32(), 16) + "\n");
            return sb.ToString();
        }
        
        public void GetMoveAniMation(int start)
        {           
            if (start > 0x8000000)
                start -= 0x8000000;
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            List<int> call_or_goto = new List<int>();
            while (true)                
            {
                String str = null;
                byte num = br.ReadByte();
				if (num > 0x2f)
					return;
                ai.Write("\t" + macros[num]);
                if (num == 0)
                {
                    int particle_id = br.ReadInt16();
                    long position = br.BaseStream.Position;
                    ai.Write(" 0x" + Convert.ToString(particle_id, 16));
                    particle_id = GetParticle(particle_id);
                    ai.Write("  @" + Operation.Hex(particle_id) + '\n');
                    br.BaseStream.Seek(position,SeekOrigin.Begin);
                    continue;
                }
                if(num==0x8 || num == 0xf)
                {
                    break;
                }
				if (num == 0x13)
				{
					int a = ConvertBytesToInt(4);
					ai.Write(" " + Operation.Hex(a));
					if (!call_or_goto.Contains(a))
						call_or_goto.Add(a);
					break;
				}
					
				if (args[num] == null)
                {
                    ai.WriteLine();
                    continue;
                }
                foreach(byte arg in args[num])
                {
                    if (arg == 0)
                    {
                        byte data = br.ReadByte();
                        ai.Write(' ' + Operation.Hex(data));
                        for(int x = 0; x < data; x++)
                        {
                            ai.Write(' ' + Operation.Hex(br.ReadInt16()));
                        }
                    }
                    else if (arg == 5)
                    {
                        int template = br.ReadInt32();
                        ai.Write(" " + Operation.Hex(template));
                        if (Temple_offsets.Contains(template))
                            continue;
                        else
                        {
                            Temple_offsets.Add(template);
                            long postion = br.BaseStream.Position;
                            str = "\t@" + GetTemple(template);
                            br.BaseStream.Seek(postion, SeekOrigin.Begin);
                        }
                        
                    }
					else if (arg == 6)
					{
						int a = ConvertBytesToInt(4);
						ai.Write(" " + Operation.Hex(a));
						if ( !call_or_goto.Contains(a))
							call_or_goto.Add(a);
					}

                    else
                    {
                        int a = ConvertBytesToInt(arg);
                        ai.Write(" " + Operation.Hex(a));
                        if (needed_retrans.Contains(num) && !call_or_goto.Contains(a))
                            call_or_goto.Add(a);
						
                    }
                }
                ai.WriteLine();
                if(!String.IsNullOrEmpty(str))
                   ai.Write(str);
            }
            foreach(int a in call_or_goto)
            {
                ai.Write("\n" + Operation.Hex(a) + ":\n");
                GetMoveAniMation(a);
            }
            
        }

        public int ConvertBytesToInt(int arg)
        {
            switch (arg)
            {
                case 1:
                    return br.ReadByte();
                case 2:
                    return br.ReadInt16();
                case 4:
                    return br.ReadInt32();
            }
            return 0;
        }

        int GetAnimationOffset(int move_num)
        {
            br.BaseStream.Seek(0xA3A44, SeekOrigin.Begin);
            br.BaseStream.Seek(br.ReadInt32() - 0x8000000+(move_num<<2)
                , SeekOrigin.Begin);
            return br.ReadInt32() -0x8000000;
        }

        public void Start(int move_num, String move_name)
        {
            int offset = GetAnimationOffset(move_num);
            ai.WriteLine(".text\n.thumb\n\n" +
				".include \"defines/anim_macros.s\"\n\n" +
				".align 1\n" + move_name + ":");
            GetMoveAniMation(offset);
        }       
    }

    public partial class Move_Battle_Script
    {
        int move_offset;
        int script_offset;
        BinaryReader br;
		StreamWriter sw;
		public Move_Battle_Script(BinaryReader br, StreamWriter sw, int move_pointer)
        {
            this.br = br;
            br.BaseStream.Seek(move_pointer, SeekOrigin.Begin);
            move_offset = br.ReadInt32() - 0x8000000;
            br.BaseStream.Seek(0x50288, SeekOrigin.Begin);
            script_offset = br.ReadInt32() - 0x8000000;			
			this.sw = sw;
		}

        int Find_Script_offset(int move)
        {
			br.BaseStream.Seek(move_offset + move * 12, SeekOrigin.Begin);
			byte script= br.ReadByte();
			sw.WriteLine("@" + script);
			br.BaseStream.Seek(script_offset+(script<<2), SeekOrigin.Begin);
            return br.ReadInt32() - 0x8000000;
        }
		List<int> address = new List<int>();
		void GetBattleScript(int offset)
		{
			if (offset > 0x8000000)
				offset -= 0x8000000;
			if (offset > 0x2000000 || offset < 0)
				return;
			br.BaseStream.Seek(offset, SeekOrigin.Begin);
			while (true)
			{
				byte num = br.ReadByte();
				if (num > 0xf8)
					return;
				sw.Write("\t" + macros[num]);

				if (args[num] == null)
				{
					sw.WriteLine();					
					continue;
				}
					
				if (num>=0x3c && num<=0x3f)
					break;
				if(num == 0x28)
				{
					int hex = br.ReadInt32();
					if (hex > 0x8000000)
					{
						if (hex != 0x82D8A4E)
						{
							sw.Write(" " + Operation.Hex(hex));
						}
						else
						{
							sw.Write(" " + "ENDTURN");
						}
                        if (!address.Contains(hex)) {
                            address.Add(hex);
                        }
					}						
					break;
				}
				else if (num==0x83)
				{
					short callasm=br.ReadInt16();
					sw.Write(" " + Operation.Hex(callasm));
					switch (callasm)
					{
						case 31:
						case 38:
						case 59:
						case 91:
							int hex = br.ReadInt32();
							address.Add(hex);
							sw.WriteLine(" " + Operation.Hex(hex));
							break;
						case 40:
							callasm = br.ReadByte();
							sw.WriteLine(" " + Operation.Hex(callasm));
							break;
					}
					continue;
				}

				foreach (ArgType arg in args[num])
				{
					if (arg == ArgType.address)
					{
						int hex = br.ReadInt32();
						if (hex > 0x8000000 && !address.Contains(hex))
							address.Add(hex);
						sw.Write(" "+ Operation.Hex(hex))	;
					}
					else
					{
						String str = GetArg(arg);
						sw.Write(" " + str);
					}					
				}
				sw.WriteLine();
			}			
		}

		public void Start(int move, String move_name, string off)
		{
			int offset;
			if (!String.IsNullOrWhiteSpace(off)) {
				if (!off.StartsWith("0x"))
					off = "0x" + off;
				offset = Tools.PokeConfig.ParseInt(off);
			}
			else {
				offset = Find_Script_offset(move);
			}

			sw.WriteLine(".text\n.thumb\n.align 1\n\n" 
				+ move_name + ":");
			GetBattleScript(offset);
			for (int a = 0; a < address.Count; a++)
			{
				sw.Write("\n" + Operation.Hex(address[a]) + ":\n");
				GetBattleScript(address[a]);
			}
		}       
    }

	public partial class XSE_scripts
	{
		BinaryReader br;
		StreamWriter sw;

		public XSE_scripts(BinaryReader br, StreamWriter sw)
		{
			this.br = br;
			this.sw = sw;
		}
		List<int> need_get = new List<int>();
		List<int> message = new List<int>();
		void GetXSE(int offset)
		{
			if (offset > 0x8000000)
				offset -= 0x8000000;
			if (offset > 0x2000000 || offset < 0)
				return;
			br.BaseStream.Seek(offset, SeekOrigin.Begin);
			byte toTermine = 0;
			while (true)
			{
				sw.Write("\t");
				byte command = br.ReadByte();
				if (command > 0xe2)
					return;
				if (command == 0xF)
				{
					long off = 5;
					br.BaseStream.Seek(off, SeekOrigin.Current);
					if (br.ReadByte() == 9)
					{
						off = -5;
						br.BaseStream.Seek(off, SeekOrigin.Current);
						int message = br.ReadInt32();
						this.message.Add(message);
						br.BaseStream.Seek(1, SeekOrigin.Current);
						int message_type = br.ReadByte();
						sw.WriteLine("msgbox " + Operation.Hex(message) +
							" " + this.message_type[message_type]);
						continue;
					}
					else
					{
						off = -6;
						br.BaseStream.Seek(off, SeekOrigin.Current);
					}						
				}
				else if (command == 0x1A)
				{
					long off = br.BaseStream.Position;
					int a = br.ReadInt32();
					byte type = br.ReadByte();
					if (type == 9)
					{
						byte b = br.ReadByte();
						if (b == 7)
							sw.Write("giveitem ");
						else if (b == 8)
							sw.Write("registernav ");
						sw.WriteLine(Operation.Hex(a>>16));
						continue;
					}
					else if (type == 0x1a)
					{
						int a1 = br.ReadInt32();
						type = br.ReadByte();
						if (type == 9)
						{
							sw.WriteLine("giveitem " + Operation.Hex(a>>16)
								+" "+Operation.Hex(a1>>16)
								+" "+message_type[br.ReadByte()]);
							continue;
						}
						else if (type == 0x1a)
						{
							int a2 = br.ReadInt32();
							if (br.ReadUInt16() == 0x909)
							{
								sw.WriteLine("giveitem " + Operation.Hex(a >> 16)
								+ " " + Operation.Hex(a1 >> 16)
								+ " " + Operation.Hex(a2 >> 16));
								continue;
							}
						}
					}							
					br.BaseStream.Seek(off, SeekOrigin.Begin);					
				}
				else if (command == 0x5C)
				{
					sw.Write("trainerbattle ");
					byte a = br.ReadByte();
					short b = br.ReadInt16();
					br.BaseStream.Seek(2, SeekOrigin.Current);
					sw.Write(Operation.Hex(a)+" "+Operation.Hex(b));
					for (int c = 0; c < trainer_arg[a]; c++)
					{
						int k = br.ReadInt32();
						sw.Write(" " + Operation.Hex(k));
						if (c < 2)
							message.Add(k);
						else if (c==2)
						{
							if(a==7)
								message.Add(k);
							else if(!need_get.Contains(k))
								need_get.Add(k);
						}
						else if (!need_get.Contains(k))
							need_get.Add(k);
					}
						
					sw.WriteLine();
					continue;
				}

				sw.Write(macros[command]);
				if(command == 0) {
					toTermine++;
					sw.WriteLine();
					if (toTermine == 5)
						break;
					goto E;
				}
				if (command < 4)
				{
					sw.WriteLine();
					break;
				}
				E:
				if (args[command] == null)
				{
					sw.WriteLine();
					continue;
				}
					
				foreach (byte arg in args[command])
				{
					switch (arg)
					{
						case 1:
							sw.Write(" " + Operation.Hex(br.ReadByte()));
							break;
						case 2:
							sw.Write(" " + Operation.Hex(br.ReadInt16()));
							break;
						case 4:
							int a = br.ReadInt32();
							if(command==6)
								sw.Write(" goto " + Operation.Hex(a));
							else if(command==7)
								sw.Write(" call " + Operation.Hex(a));
							else
								sw.Write(" " + Operation.Hex(a));
							if (command >= 4 && command <= 7 && !need_get.Contains(a))
								need_get.Add(a);
							break;
					}
					
				}
				sw.WriteLine();
				if (command == 5)
					break;
			}
		}

		public void Start(int offset)
		{
			need_get.Add(offset);
			for (int a = 0; a < need_get.Count; a++)
			{
				sw.WriteLine("#org " + Operation.Hex(need_get[a]));
				GetXSE(need_get[a]);
			}
			for (int a = 0; a < message.Count; a++)
			{
				sw.WriteLine("#org " + Operation.Hex(message[a]));
				sw.WriteLine(Tools.ReadAndWrite.TransToCH(br, message[a]));
			}
		}
	}
}
