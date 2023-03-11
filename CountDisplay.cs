using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipNCollect
{
    public partial class CountDisplay : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );

        System.Timers.Timer time = null;
        public CountDisplay(string infoHeader, string infoSubHeader)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            headerLabel.Text = infoHeader;
            infoLabel.Text = infoSubHeader;

            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width - 40,
                                      workingArea.Bottom - Size.Height - 40);


            this.Click += CountDisplay_Click;
            this.mainPanel.Click += CountDisplay_Click;

            System.Timers.Timer time = new System.Timers.Timer();
            time.Elapsed += Time_Elapsed;
            time.Interval = 3000;
            time.Start();
        }

        private void Time_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            time?.Stop();
            if (InvokeRequired && !this.IsDisposed)
            {
                this.Invoke(new MethodInvoker(delegate {
                    this.Close();
                }));
                return;
            }
        }

        private void CountDisplay_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle,
                                                                       Color.Black,
                                                                       Color.Purple,
                                                                       260F))
            {
                e.Graphics.FillRectangle(brush, this.DisplayRectangle);
            }
        }

        private void CountDisplay_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }
    }
}
