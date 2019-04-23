using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Tools
{
    public class MyPictureBox : PictureBox
    {
        public new Bitmap Image {
            set {
                base.Image = value;
            }
            get {
                return (Bitmap)base.Image;
            }
        }
    }

    public class MyComboBox : ComboBox
    {
        public MyComboBox()
        {
            MaxDropDownItems = 20;
            IntegralHeight = false;
            DrawMode = DrawMode.OwnerDrawVariable;
        }
        public override int SelectedIndex {
            get {
                return base.SelectedIndex;
            }
            set {
                if (value < Items.Count)
                    base.SelectedIndex = value;
                else
                {
                    base.SelectedIndex = 0;
                }

            }
        }

        /// <summary>
        /// 绘制下拉框的内容
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            if (DesignMode)
            {
                return;
            }
            Rectangle boundsRect = e.Bounds;//获取绘制项边界的矩形
            int pad = 4;
            int X = (int)e.Graphics.MeasureString("ZZZ", Font).Width + pad;
            int width = Width - X;
            pad += X;
            //e.Graphics.FillRectangle(Brushes.Bisque, e.Bounds);
            //if ((e.State & DrawItemState.Focus) == 0)
            //{
            //设置鼠标悬浮ComboBox的item的背景色
            //	e.Graphics.FillRectangle(Brushes.Pink, e.Bounds);
            //}
            e.DrawBackground();

            using (Pen linePen = new Pen(SystemColors.GrayText))
            {
                using (SolidBrush brush = new SolidBrush(ForeColor))
                {
                    if (Items.Count == 0)
                    {
                        e.Graphics.DrawString(Convert.ToString(Items[e.Index]), Font, brush, boundsRect);
                    }
                    else
                    {
                        string item = Items[e.Index].ToString();
                        boundsRect.X = 0;//列的左边位置
                        boundsRect.Width = X;
                        e.Graphics.FillRectangle(Brushes.LightPink, boundsRect);
                        e.Graphics.DrawString(Convert.ToString(e.Index, 16).ToUpper().PadLeft(3, '0'), Font, brush, boundsRect);
                        e.Graphics.DrawLine(linePen, boundsRect.Right, boundsRect.Top, boundsRect.Right, boundsRect.Bottom);

                        boundsRect.X = pad;
                        boundsRect.Width = width;
                        e.Graphics.DrawString(item, Font, brush, boundsRect);
                    }
                }
            }
            e.DrawFocusRectangle();
        }
    }


   

}
