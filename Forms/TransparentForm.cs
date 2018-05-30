using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransparentForm.Classes;


namespace TransparentForm
{
    public partial class TransparentForm : Form
    {
        enum Action
        {
            OK,
            Start,
            Stop,
            Saving
        };

        Action action;

        static int screenLeft;
        static int screenTop;
        static int screenWidth;
        static int screenHeight;

        static System.Drawing.Graphics g;

        static Point pointCurrent;
        static Point pointPrev;
        static Point? pointStart;

        static MyCapturePanel capturePanel;
        static Button btnStartCapture;

        static bool isMouseDrag = false;
        static Pen lineColor = Pens.HotPink;

        CaptureScreenToGIF capt;

        public TransparentForm()
        {
            InitializeComponent();
            //setting
            this.ShowInTaskbar = false;

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            this.BackColor = Color.DarkMagenta;
            this.TransparencyKey = this.BackColor;
            this.TopMost = true;
            this.Opacity = 0.5;

            screenLeft = SystemInformation.VirtualScreen.Left;
            screenTop = SystemInformation.VirtualScreen.Top;
            screenWidth = SystemInformation.VirtualScreen.Width;
            screenHeight = SystemInformation.VirtualScreen.Height;

            this.Size = new Size(screenWidth, screenHeight);
            //end of setting
            this.Paint += Form2_Paint;

            pointStart = null;
            //time lable
            timeL.BackColor = Color.LightBlue;
            timeL.Hide();

            //capture panel
            capturePanel = new MyCapturePanel(this)
            {

            };
            capturePanel.BackColor = Color.Green;
            capturePanel.Visible = false;
            capturePanel.btnStartCapture = btnStartCapture;

            //start capture button
            btnStartCapture = new Button();
            btnStartCapture.BackColor = Color.DeepPink;
            btnStartCapture.Click += B_ClickAsync;
            btnStartCapture.Visible = false;
            btnStartCapture.Text = "OK";

            Controls.Add(capturePanel);
            Controls.Add(btnStartCapture);

            action = Action.OK;
        }

        private void CapturePanel_LocationChanged(object sender, EventArgs e)
        {

        }

        private void HideTheForm()
        {
            //clearing the screen
            using (Brush brush = new SolidBrush(this.BackColor))
            using (Graphics _g = this.CreateGraphics())
            {
                _g.FillRectangle(brush,
                  this.DisplayRectangle);
            }
        }

        private void DrawAPlus(Point p, Graphics _g)
        {
            //drawing a Plus at current point
            Point from = new Point(p.X, 0);
            Point to = new Point(p.X, screenHeight);
            _g.DrawLine(lineColor, from, to);
            from = new Point(0, p.Y);
            to = new Point(screenWidth, p.Y);
            _g.DrawLine(lineColor, from, to);
            //
        }

        private void CapturePanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (isMouseDrag)
            {
                isMouseDrag = false;
            }
        }

        private void CapturePanel_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDrag = true;
            pointPrev = e.Location;
        }

        private void CapturePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDrag)
            {
                var location = capturePanel.Location;
                location.Offset(e.Location.X - pointPrev.X, e.Location.Y - pointPrev.Y);
                capturePanel.Location = location;
            }
        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            g = e.Graphics;

            Point pointA = new Point();
            Point pointB = new Point();

            //
            DrawAPlus(pointCurrent, g);
            //

            if (isMouseDrag)
            {
                //drawing start point
                if (pointStart != null && !pointCurrent.Equals((Point)pointStart))
                {
                    var _ptStart = (Point)pointStart;
                    DrawAPlus(_ptStart, g);

                    using (Brush brush = new SolidBrush(this.BackColor))
                    {
                        var varStart = new Point();
                        var varEnd = new Point();
                        if (_ptStart.Y > pointCurrent.Y)
                        {
                            varStart = pointCurrent;
                            varEnd = _ptStart;
                        }
                        else
                        {
                            varStart = _ptStart;
                            varEnd = pointCurrent;
                        }
                        if (varStart.X > varEnd.X)
                        {
                            //swapping
                            int i = varStart.X;
                            varStart.X = varEnd.X;
                            varEnd.X = i;
                        }

                        if (!capturePanel.IsDisposed)
                        {
                            capturePanel.Visible = false;
                            capturePanel.Dispose();
                        }
                        capturePanel = new MyCapturePanel(this)
                        {
                            Location = varStart,
                            Size = new Size(Math.Abs(varEnd.X - varStart.X), Math.Abs(varEnd.Y - varStart.Y)),
                            BackColor = Color.DarkMagenta,
                            btnStartCapture = btnStartCapture
                        };
                        if (!Controls.Contains(capturePanel))
                        {
                            Controls.Add(capturePanel);
                        }

                        capturePanel.Visible = true;


                        //using (Button b = new Button())
                        //{
                        //    Button b = new Button()
                        //    {
                        //        Location = new Point(0, 0),
                        //    };
                        //    if (!capturePanel.Controls.Contains(b))
                        //    {
                        //        capturePanel.Controls.Add(b);
                        //        b.Show();
                        //    }
                        //    b.Click += B_ClickAsync;
                        //}



                        //g.FillRectangle(brush,
                        //    new Rectangle(varStart.X, varStart.Y,
                        //        Math.Abs(varEnd.X - varStart.X), Math.Abs(varEnd.Y - varStart.Y)));
                    }
                }
            }
        }

        private async void B_ClickAsync(object sender, EventArgs e)
        {
            if (action == Action.OK)
            {
                HideTheForm();

                //hiding the move btn of the capture panel
                capturePanel.btnMove.Visible = false;

                ////checking location on window
                //MessageBox.Show(capturePanel.DisplayRectangle.ToString() + "\n" +
                //    capturePanel.ClientRectangle.ToString() + "\n"
                //    + capturePanel.Location);

                //change the action and text
                action = Action.Start;
                btnStartCapture.Text = "Start";
            }
            else if (action == Action.Start)
            {
                //start capturing
                //capture = new CaptureAndSaveToGIF(capturePanel.Location,
                //                    capturePanel.Width,
                //                    capturePanel.Height);
                //capture.Start();
                capt = new CaptureScreenToGIF(true, new Rectangle(capturePanel.Location, new Size(capturePanel.Width, capturePanel.Height)));
                capt.StartCapture();
                //
                action = Action.Stop;
                btnStartCapture.Text = "Stop";
            }
            else if (action == Action.Stop)
            {
                //stop capturing
                //capture.Stop();
                //  capture.Save("Dinh Anh" + ".gif");

                capt.Stop();

                timeL.Show();
                timeL.Text = "Percent...";

                var progress = new Progress<float>();
                progress.ProgressChanged += (s, f) => timeL.Text = $"Percent {f.ToString("P")}...";

                if (capturePanel.Visible)
                {
                    capturePanel.Hide();
                    capturePanel.Dispose();
                    btnStartCapture.Hide();
                    HideTheForm();
                }

                var fileName = Regex.Replace(DateTime.Now.ToString(), "[/:]", "_");
                var extension = ".gif";

                action = Action.Saving;
                btnStartCapture.Text = "Saving";
                btnStartCapture.Enabled = false;


                await capt.SaveAsync(fileName + extension, progress);

                timeL.Text = "Percent 100%";
                timeL.Hide();

                action = Action.OK;
                btnStartCapture.Text = "OK";
                btnStartCapture.Enabled = true;

            }
            else if (action == Action.Saving)
            {

            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //nothing
        }
        private void Form2_Load(object sender, EventArgs e)
        {
        }

        private void Form2_MouseHover(object sender, EventArgs e)
        {

        }

        private void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            pointCurrent = Cursor.Position;
            this.Invalidate();
        }

        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            pointStart = pointCurrent;
            isMouseDrag = true;
            if (!capturePanel.IsDisposed)
            {
                capturePanel.Visible = false;
                capturePanel.Dispose();
            }
            if (btnStartCapture.Visible)
            {
                btnStartCapture.Hide();
            }
        }

        private void Form2_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDrag = false;
            if (pointStart != null)
            {
                pointStart = null;
            }
            //MessageBox.Show(capturePanel.Size.ToString());
            if (!capturePanel.IsDisposed)
            {
                btnStartCapture.Location =
                    new Point(capturePanel.Location.X + capturePanel.Width / 2 - btnStartCapture.Width / 2,
                       // 0);
                       capturePanel.Location.Y + capturePanel.Height + 2); //capturePanel.Width / 2
                btnStartCapture.Visible = true;
            }
        }


    }
}
