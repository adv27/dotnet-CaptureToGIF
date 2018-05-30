using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TransparentForm
{
    class MyCapturePanel : Panel
    {
        private TransparentForm parent;
        public Button btnMove;
        public Button btnStartCapture { get; set; }
        private Color colorBorder = Color.HotPink;
        private Point pointPrev;
        private bool isMouseDownOnMoveBtn = false;

        public MyCapturePanel(Form parent)
        {
            InitializeComponent();
            this.parent = (TransparentForm)parent;
            this.Paint += PaintBorder;
        }

        private void PaintBorder(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics,
                this.ClientRectangle,
               this.colorBorder,
               ButtonBorderStyle.Solid);
        }

        private void InitializeComponent()
        {
            this.btnMove = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnMove
            // 
            this.btnMove.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.btnMove.Image = global::TransparentForm.Properties.Resources.abc;
            this.btnMove.Location = new System.Drawing.Point(0, 0);
            this.btnMove.Name = "btnMove";
            this.btnMove.Size = new System.Drawing.Size(18, 18);
            this.btnMove.TabIndex = 0;
            this.btnMove.UseVisualStyleBackColor = true;
            this.btnMove.MouseMove += MoveBtn_MouseMove;
            this.btnMove.MouseDown += MoveBtn_MouseDown;
            this.btnMove.MouseUp += MoveBtn_MouseUp;
            // 
            // MyCapturePanel
            // 
            this.Controls.Add(this.btnMove);
            this.ResumeLayout(false);

        }

        private void MoveBtn_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDownOnMoveBtn)
            {
                var location = this.Location;
                location.Offset(e.Location.X - pointPrev.X, e.Location.Y - pointPrev.Y);
                this.Location = location;


                btnStartCapture.Location =
                    new Point(this.Location.X + this.Width / 2 - btnStartCapture.Width / 2,
                       // 0);
                       this.Location.Y + this.Height + 2); //capturePanel.Width / 2
                btnStartCapture.Visible = true;
            }
        }

        private void MoveBtn_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDownOnMoveBtn = false;
        }

        private void MoveBtn_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDownOnMoveBtn = true;
            pointPrev = e.Location;
        }
    }
}
