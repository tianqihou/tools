

using Decompiler;
using System;
using System.Windows.Forms;

namespace Tools 
{
	public class Evolution {

		private Evo[] evo = new Evo[EVO_PER_POKE];
		private const int evo_table = 0x006D140;
        public const byte EVO_PER_POKE = 5;
        private DataGridView grid;
        public int rowIndex;

        private Evo[] getEvo(ReadAndWrite rw, int num)
        {
            //Console.WriteLine(Operation.Hex(rw.getStructOffset(typeof(Evo), evo_table, num * EVO_PER_POKE)));
            rw.Seek(rw.getStructOffset(typeof(Evo), evo_table, num * EVO_PER_POKE));
            for(int x = 0; x < EVO_PER_POKE; x++)
            {
                byte[] data = rw.Br.ReadBytes(8);
                evo[x] = (Evo)StructsUtil.ByteToStruct(data, typeof(Evo));
            }
            return evo;
        }

        public Evolution(DataGridView grid)
        {
            grid.Rows.Clear();
            for(int i = 0; i < EVO_PER_POKE; i++)
            {
                grid.Rows.Add();
            }
            this.grid = grid;
        }


        public void setGridEvoInfo(ReadAndWrite rw, int num, string[] pokeNames)
        {
            Evo[] e = getEvo(rw, num);           
            for (int i = 0; i < EVO_PER_POKE; i++)
            {
                Evo e0 = e[i];
                grid.Rows[i].Cells[0].Value = (int)e0.method;
                grid.Rows[i].Cells[1].Value = Operation.Hex(e0.padd0);
                grid.Rows[i].Cells[2].Value = WordsDecoding.PreFixString(pokeNames,e0.poke);
                grid.Rows[i].Cells[3].Value = Operation.Hex(e0.parameter);
                grid.Rows[i].Cells[4].Value = Operation.Hex(e0.padd1);
            }
        }

        public void setPokeIndex(object sender, EventArgs e)
        {
            MyComboBox c = sender as MyComboBox;
            evo[rowIndex].poke = (short)c.SelectedIndex;
            grid.Rows[rowIndex].Cells[2].Value = WordsDecoding.PreFixString((string[])c.DataSource, 
                c.SelectedIndex);
        }

        public void Save(ReadAndWrite rw, int num)
        {
            rw.Seek(rw.getStructOffset(typeof(Evo), evo_table, num * EVO_PER_POKE));
            for(int i = 0; i < EVO_PER_POKE; i++)
            {
                Evo e = evo[i];
                DataGridViewCellCollection cell = grid.Rows[i].Cells;
                e.method = (byte)(int)cell[0].Value;
                e.padd0 = Operation.ParseByte((string)cell[1].Value);
                e.parameter = (ushort)Operation.ParseShort((string)cell[3].Value);
                e.padd1 = Operation.ParseShort((string)cell[4].Value);
                rw.Bw.Write(StructsUtil.StructToByte(e));
            }
        }

    }
}
