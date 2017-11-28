using Emgu.CV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iRecon
{
    public partial class emImageViewer : Form
    {
        public emImageViewer(IImage image, long score = 0)
        {
            InitializeComponent();

            this.Text = "Score: " + score.ToString();

            if (image != null)
            {
                imgBox.Image = image.Bitmap;

                Size size = image.Size;
                size.Width += 12;
                size.Height += 42;
                if (!Size.Equals(size)) Size = size;
            }
        }
    }
}
