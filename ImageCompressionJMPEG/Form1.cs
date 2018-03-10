using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageCompressionJMPEG
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// For overriding movement of window
        /// </summary>
        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;
        private const int CLOSE_FORM_HORZ_OFFSET = 30;
        private const int PANEL_VERT_OFFSET = 25;
        private const int PICTUREBOX_OFFSET = 25;
        private const int MAX_FORM_HORZ_OFFSET = 60;
        private const int MIN_FORM_HORZ_OFFSET = 90;
        private const int MOTIONVECTOR_HORZ_OFFSET = 0;
        private const int MOTIONVECTOR_PANEL_HEIGHT = 30;
        private const int LEFT_OFFSET = 14;
        private const int LABEL_SIZE = 19;
        private const int BUTTON_GAP = 3;
        private PictureBox pictureBoxOne;
        private PictureBox pictureBoxTwo;
        private bool addToOne = true;
        private Panel panel;
        Panel motionVectorInfoPanel;
        private CustomButton closeForm;
        private CustomButton minForm;
        private CustomButton maxForm;
        private Label currentSearchAreaRange;
        private CustomButton addSearchAreaRange;
        private CustomButton subtractSearchAreaRange;
        private Bitmap compressedBitmap;
        private byte[] compressedByteArray;
        private Color themeColor;
        private Color themeBackgroundColor;
        private Color themeBackgroundColorTwo;
        private TextBox title;
        private Vector[] motionVectors;
        private bool drawMV = false;

        public Form1()
        {
            this.themeBackgroundColor = Color.FromArgb(175, 0, 0, 0);
            this.themeBackgroundColorTwo = Color.FromArgb(100, 0, 0, 0);
            this.themeColor = Color.FromArgb(200, 144, 238, 144);
            InitializeComponent();
            initializeCustom();
            customizeMenuStrip(menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }

        /// <summary>
        /// Override WndProc to allow for resizing
        /// </summary>
        /// <param name="m">Message for WndProc</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)(HT_CAPTION);
            }
        }

        /// <summary>
        /// Redraw form when resized
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void ImageCompressor_Resize(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                Control control = (Control)sender;
                int w = control.Size.Width;
                int h = control.Size.Height;
                closeForm.Location = new Point(w - CLOSE_FORM_HORZ_OFFSET, 0);
                panel.Size = new Size(w, h - PANEL_VERT_OFFSET);
                maxForm.Location = new Point(w - MAX_FORM_HORZ_OFFSET, 0);
                minForm.Location = new Point(w - MIN_FORM_HORZ_OFFSET, 0);
                pictureBoxOne.Size = new Size(w / 2 - PICTUREBOX_OFFSET * 3 / 2, h - 54 - PICTUREBOX_OFFSET * 2);
                pictureBoxTwo.Size = new Size(w / 2 - PICTUREBOX_OFFSET * 3 / 2, h - 54 - PICTUREBOX_OFFSET * 2);
                pictureBoxTwo.Location = new Point(w / 2 + PICTUREBOX_OFFSET / 2, 27 + PICTUREBOX_OFFSET);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openImage();
        }

        private void openImage()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Open Image";
                dialog.Filter = "images|*.JPG; *.PNG; *.GJF; *.bmp; *.CJPG; *.CMPEG; * .CMPEG";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (Path.GetExtension(dialog.FileName).Equals(".CJPG"))
                    {
                        if (addToOne)
                        {
                            pictureBoxOne.Image = null;
                            pictureBoxOne.Image = Compression.JPEGDecompression(File.ReadAllBytes(dialog.FileName));
                        }
                        else
                        {
                            pictureBoxTwo.Image = null;
                            pictureBoxTwo.Image = Compression.JPEGDecompression(File.ReadAllBytes(dialog.FileName));
                        }
                    }
                    else if (Path.GetExtension(dialog.FileName).Equals(".CMPEG"))
                    {
                        pictureBoxOne.Image = null;
                        pictureBoxOne.Image = Compression.MPEGDecompression(File.ReadAllBytes(dialog.FileName));
                    }
                    else
                    {
                        if (addToOne)
                        {
                            pictureBoxOne.Image = null;
                            pictureBoxOne.Image = new Bitmap(dialog.FileName);
                        }
                        else
                        {
                            pictureBoxTwo.Image = null;
                            pictureBoxTwo.Image = new Bitmap(dialog.FileName);
                        }
                    }
                    Refresh();
                }
                dialog.Dispose();
            }
        }

        /// <summary>
        /// Add an image to pictureBoxOne
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e"></param>
        private void addImageOne(object sender, EventArgs e)
        {
            addToOne = true;
            openImage();
        }

        /// <summary>
        /// Remove an image from pictureBoxOne
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeImageOne(object sender, EventArgs e)
        {
            pictureBoxOne.Image = null;
            Refresh();
        }

        /// <summary>
        /// Add an image to pictureBoxTwo
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e"></param>
        private void addImageTwo(object sender, EventArgs e)
        {
            addToOne = false;
            openImage();
        }

        /// <summary>
        /// Remove an image from pictureBoxOne
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeImageTwo(object sender, EventArgs e)
        {
            pictureBoxTwo.Image = null;
            Refresh();
        }

        private void initializeCustom()
        {
            //
            // Control
            //
            Width = 1000;
            Height = 600;
            // 
            // pictureBoxOne shown on the left
            //
            pictureBoxOne = new PictureBox();
            pictureBoxOne.Location = new System.Drawing.Point(PICTUREBOX_OFFSET, 27 + PICTUREBOX_OFFSET);
            pictureBoxOne.Name = "pictureBoxOne";
            pictureBoxOne.TabStop = false;
            pictureBoxOne.BackColor = themeBackgroundColorTwo;
            pictureBoxOne.SizeMode = PictureBoxSizeMode.StretchImage;
            // 
            // pictureBoxTwo shown on the right
            //
            pictureBoxTwo = new PictureBox();
            pictureBoxTwo.Name = "pictureBoxTwo";
            pictureBoxTwo.TabStop = false;
            pictureBoxTwo.BackColor = themeBackgroundColorTwo;
            pictureBoxTwo.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxTwo.Paint += new PaintEventHandler(pictureBoxTwo_Paint);
            //
            // conext menu for pictureBoxOne
            //
            ContextMenu cm = new ContextMenu();
            MenuItem mnuAddImage = new MenuItem("Add image");
            MenuItem mnuRemoveImage = new MenuItem("Remove image");
            mnuAddImage.Click += new EventHandler(addImageOne);
            mnuRemoveImage.Click += new EventHandler(removeImageOne);
            cm.MenuItems.Add(mnuAddImage);
            cm.MenuItems.Add(mnuRemoveImage);
            pictureBoxOne.ContextMenu = cm;
            //
            // context menu for pictureBoxTwo
            //
            ContextMenu cmTwo = new ContextMenu();
            MenuItem mnuAddImageTwo = new MenuItem("Add image");
            MenuItem mnuRemoveImageTwo = new MenuItem("Remove image");
            mnuAddImageTwo.Click += new EventHandler(addImageTwo);
            mnuRemoveImageTwo.Click += new EventHandler(removeImageTwo);
            cmTwo.MenuItems.Add(mnuAddImageTwo);
            cmTwo.MenuItems.Add(mnuRemoveImageTwo);
            pictureBoxTwo.ContextMenu = cmTwo;
            // 
            // panel1
            // 
            panel = new Panel();
            panel.BackColor = System.Drawing.Color.Transparent;
            panel.Controls.Add(this.menuStrip1);
            panel.Location = new System.Drawing.Point(0, 25);
            panel.Name = "panel1";
            panel.Size = new System.Drawing.Size(this.Width, this.Height - 25);
            panel.Controls.Add(pictureBoxOne);
            panel.Controls.Add(pictureBoxTwo);
            // 
            // closeForm
            // 
            closeForm = new CustomButton();
            closeForm.ForeColor = themeColor;
            closeForm.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            closeForm.FlatAppearance.BorderSize = 0;
            closeForm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            closeForm.Location = new System.Drawing.Point(this.Width - 45, 0);
            closeForm.Name = "closeForm";
            closeForm.Size = new System.Drawing.Size(30, 25);
            closeForm.TabIndex = 6;
            closeForm.Text = "X";
            closeForm.UseVisualStyleBackColor = true;
            closeForm.Click += new System.EventHandler(closeForm_Click);
            // 
            // maxForm
            // 
            maxForm = new CustomButton();
            maxForm.ForeColor = themeColor;
            maxForm.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            maxForm.FlatAppearance.BorderSize = 0;
            maxForm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            maxForm.Location = new System.Drawing.Point(this.Width - 75, 0);
            maxForm.Name = "maxForm";
            maxForm.Size = new System.Drawing.Size(30, 25);
            maxForm.TabIndex = 5;
            maxForm.TabStop = false;
            maxForm.Text = "⎕";
            maxForm.UseMnemonic = false;
            maxForm.UseVisualStyleBackColor = true;
            maxForm.Click += new System.EventHandler(maxForm_Click);
            // 
            // minForm
            // 
            minForm = new CustomButton();
            minForm.ForeColor = themeColor;
            minForm.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            minForm.FlatAppearance.BorderSize = 0;
            minForm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            minForm.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            minForm.Location = new System.Drawing.Point(this.Width-105, 0);
            minForm.Name = "minForm";
            minForm.Size = new System.Drawing.Size(30, 25);
            minForm.TabIndex = 4;
            minForm.TabStop = false;
            minForm.Text = "_";
            minForm.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            minForm.UseMnemonic = false;
            minForm.UseVisualStyleBackColor = true;
            minForm.Click += new System.EventHandler(minForm_Click);
            //
            // title
            //
            title = new TextBox();
            title.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            title.BorderStyle = System.Windows.Forms.BorderStyle.None;
            title.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            title.Location = new System.Drawing.Point(LEFT_OFFSET, 4);
            title.Name = "Title";
            title.Size = new System.Drawing.Size(163, 16);
            title.ReadOnly = true;
            title.Enabled = false;
            title.TabStop = false;
            title.ForeColor = themeColor;
            title.Text = "ImageCompressiorV1";
            //
            // ImageCompressor
            //
            Resize += new System.EventHandler(this.ImageCompressor_Resize);
            Controls.Add(panel);
            Controls.Add(maxForm);
            Controls.Add(title);
            Controls.Add(minForm);
            Controls.Add(closeForm);
            BackColor = Color.FromArgb(35, 35, 35);
            //
            // motionVectorInfoPanel
            //
            motionVectorInfoPanel = new Panel();
//            motionVectorInfoPanel.BackColor = themeBackgroundColor;
            panel.Controls.Add(motionVectorInfoPanel);
            motionVectorInfoPanel.Location = new Point(0, 27);
            motionVectorInfoPanel.Name = "motionVectorInfoPanel";
            motionVectorInfoPanel.Size = new Size(panel.Width, PICTUREBOX_OFFSET);
            //
            // currentSearchAreaRange
            //
            currentSearchAreaRange = new Label();
            currentSearchAreaRange.BorderStyle = System.Windows.Forms.BorderStyle.None;
            currentSearchAreaRange.Location = new System.Drawing.Point(PICTUREBOX_OFFSET, BUTTON_GAP);
            currentSearchAreaRange.Size = new System.Drawing.Size(LABEL_SIZE, LABEL_SIZE);
            currentSearchAreaRange.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            currentSearchAreaRange.TextAlign = ContentAlignment.MiddleCenter;
            currentSearchAreaRange.Text = "15";
            currentSearchAreaRange.ForeColor = themeColor;
            currentSearchAreaRange.BackColor = themeBackgroundColorTwo;
            motionVectorInfoPanel.Controls.Add(currentSearchAreaRange);
            //
            // addSearchAreaRange
            //
            addSearchAreaRange = new CustomButton();
            addSearchAreaRange.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            addSearchAreaRange.FlatAppearance.BorderSize = 0;
            addSearchAreaRange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            addSearchAreaRange.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            addSearchAreaRange.Location = new System.Drawing.Point(PICTUREBOX_OFFSET + LABEL_SIZE + BUTTON_GAP, BUTTON_GAP);
            addSearchAreaRange.Name = "addSearchAreaRange";
            addSearchAreaRange.Size = new System.Drawing.Size(LABEL_SIZE, LABEL_SIZE);
            addSearchAreaRange.TabStop = false;
            addSearchAreaRange.Text = "➕";
            addSearchAreaRange.ForeColor = themeColor;
            addSearchAreaRange.UseMnemonic = false;
            addSearchAreaRange.UseVisualStyleBackColor = true;
            addSearchAreaRange.BackColor = themeBackgroundColorTwo;
            addSearchAreaRange.Click += new System.EventHandler(this.addSearchAreaRange_Click);
            motionVectorInfoPanel.Controls.Add(addSearchAreaRange);
            //
            // subtractSearchAreaRange
            //
            subtractSearchAreaRange = new CustomButton();
            subtractSearchAreaRange.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            subtractSearchAreaRange.FlatAppearance.BorderSize = 0;
            subtractSearchAreaRange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            subtractSearchAreaRange.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            subtractSearchAreaRange.Location = new System.Drawing.Point(PICTUREBOX_OFFSET + LABEL_SIZE * 2 + BUTTON_GAP * 2, BUTTON_GAP);
            subtractSearchAreaRange.Name = "subtractSearchAreaRange";
            subtractSearchAreaRange.Size = new System.Drawing.Size(LABEL_SIZE, LABEL_SIZE);
            subtractSearchAreaRange.TabStop = false;
            subtractSearchAreaRange.Text = "➖";
            subtractSearchAreaRange.ForeColor = themeColor;
            subtractSearchAreaRange.UseMnemonic = false;
            subtractSearchAreaRange.UseVisualStyleBackColor = true;
            subtractSearchAreaRange.BackColor = themeBackgroundColorTwo;
            subtractSearchAreaRange.Click += new System.EventHandler(this.subtractSearchAreaRange_Click);
            motionVectorInfoPanel.Controls.Add(subtractSearchAreaRange);
        }

        /// <summary>
        /// JPEG the image on the left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jPEGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap(pictureBoxOne.Image);
            compressedBitmap = Compression.JPEGCompression(bitmap, pictureBoxOne.Image.Width, pictureBoxOne.Image.Height);
            pictureBoxTwo.Image = new Bitmap(compressedBitmap);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.Filter = "*.CJPG|* .CJPG|*.CMPEG|* .CMPEG";
            DialogResult dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                Stream f = saveFileDialog.OpenFile();
                if (f != null)
                {
                    BinaryWriter wr = new BinaryWriter(f);
                    compressedByteArray = Compression.getCompressedByteArray();
                    if (compressedByteArray != null)
                    {
                        wr.Write(compressedByteArray);
//                        File.WriteAllBytes(saveFileDialog.FileName, compressedByteArray);
                    }
                    wr.Close();
                    f.Close();
                }
            }
        }

        /// <summary>
        /// Closes the form when closeForm button is clicked
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void closeForm_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Minimizes the form when minForm button is clicked
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void minForm_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// Maximize the form when maxForm button is clicked
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void maxForm_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }
        private class MyRenderer : ToolStripProfessionalRenderer
        {
            public MyRenderer(Color themeBackgroundColor) : base(new MyColors(themeBackgroundColor)) { }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                var toolStripMenuItem = e.Item as ToolStripMenuItem;
                if (toolStripMenuItem != null)
                {
                    e.ArrowColor = Color.FromArgb(200, 144, 238, 144);
                }
                base.OnRenderArrow(e);
            }
        }

        /// <summary>
        /// Class that override specific form item color
        /// </summary>
        private class MyColors : ProfessionalColorTable
        {
            private Color themeBackgroundColor;

            public MyColors(Color themeBackgroundColor)
            {
                this.themeBackgroundColor = themeBackgroundColor;
            }

            public override Color MenuItemSelected
            {
                get { return themeBackgroundColor; }
            }
            public override Color ButtonSelectedGradientMiddle
            {
                get { return Color.Transparent; }
            }

            public override Color ButtonSelectedHighlight
            {
                get { return Color.Transparent; }
            }

            public override Color ButtonCheckedGradientBegin
            {
                get { return themeBackgroundColor; }
            }
            public override Color ButtonCheckedGradientEnd
            {
                get { return themeBackgroundColor; }
            }
            public override Color ButtonSelectedBorder
            {
                get { return Color.FromArgb(200, 144, 238, 144); }
            }
            public override Color ToolStripDropDownBackground
            {
                get { return themeBackgroundColor; }
            }
            public override Color CheckSelectedBackground
            {
                get { return themeBackgroundColor; }
            }
            public override Color MenuItemSelectedGradientBegin
            {
                get { return themeBackgroundColor; }
            }
            public override Color MenuItemSelectedGradientEnd
            {
                get { return themeBackgroundColor; }
            }
            public override Color MenuItemBorder
            {
                get { return Color.Black; }
            }
            public override Color MenuItemPressedGradientBegin
            {
                get { return Color.Transparent; }
            }
            public override Color CheckBackground
            {
                get { return themeBackgroundColor; }
            }
            public override Color CheckPressedBackground
            {
                get { return themeBackgroundColor; }
            }
            public override Color ImageMarginGradientBegin
            {
                get { return Color.Transparent; }
            }
            public override Color ImageMarginGradientMiddle
            {
                get { return Color.Transparent; }
            }
            public override Color ImageMarginGradientEnd
            {
                get { return Color.Transparent; }
            }
            public override Color MenuItemPressedGradientEnd
            {
                get { return Color.Transparent; }
            }
        }

        /// <summary>
        /// Custom button that override Button
        /// </summary>
        public class CustomButton : Button
        {
            protected override bool ShowFocusCues
            {
                get
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Customize menuStrip's color
        /// </summary>
        /// <param name="menuStrip">Menustrip object</param>
        private void customizeMenuStrip(MenuStrip menuStrip)
        {
            menuStrip.Renderer = new MyRenderer(themeBackgroundColor);
            menuStrip.BackColor = Color.Transparent;
            menuStrip.ForeColor = themeColor;
            openToolStripMenuItem.ForeColor = themeColor;
            saveToolStripMenuItem.ForeColor = themeColor;
            jPEGToolStripMenuItem.ForeColor = themeColor;
            mPEGToolStripMenuItem.ForeColor = themeColor;
            videoToolStripMenuItem.ForeColor = themeColor;
            mVToolStripMenuItem.ForeColor = themeColor;
        }

        /// <summary>
        /// Event handler for pictureBoxTwo's paint for drawing motion vectors
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void pictureBoxTwo_Paint(object sender, PaintEventArgs e)
        {
            if (motionVectors != null && pictureBoxOne.Image != null && pictureBoxTwo.Image != null && drawMV)
            {
                float heightScaler = (float)pictureBoxTwo.Size.Height / (float)pictureBoxOne.Image.Height;
                float widthScaler = (float)pictureBoxTwo.Size.Width / (float)pictureBoxOne.Image.Width;
                Pen pen = new Pen(Color.Red, 3);
                pen.StartCap = System.Drawing.Drawing2D.LineCap.RoundAnchor;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                SolidBrush brush = new SolidBrush(Color.Red);
                Pen pen2 = new Pen(Color.Red, 3);
                int index = 0;
                for (int y = 0; y < pictureBoxOne.Image.Height; y += 16)
                {
                    for (int x = 0; x < pictureBoxOne.Image.Width; x += 16)
                    {
                        if (x * heightScaler - (x + motionVectors[index].x) * heightScaler == 0 &&
                            y * widthScaler - (y + motionVectors[index].y) * widthScaler == 0)
                        {
                            e.Graphics.DrawEllipse(pen, y * widthScaler, x * heightScaler, 3, 3);
                        }
                        else
                        {
                            e.Graphics.DrawLine(pen, y * widthScaler, x * heightScaler,
                               (y + motionVectors[index].y) * widthScaler, (x + motionVectors[index].x) * heightScaler);
                        }
                        index++;
                    }
                }
            }
        }

        /// <summary>
        /// Draw motion vectors for display assuming both frame has same size
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void mVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBoxOne.Image != null && pictureBoxTwo.Image != null)
            {
                Bitmap bitmapOne = new Bitmap(pictureBoxOne.Image);
                compressedBitmap = Compression.JPEGCompression(bitmapOne, pictureBoxOne.Image.Width, pictureBoxOne.Image.Height);
                pictureBoxOne.Image = new Bitmap(compressedBitmap);
                Bitmap bitmapTwo = new Bitmap(pictureBoxTwo.Image);
                MPEGPrep mPEGPReg = Compression.MPEGMotionVector(compressedBitmap, bitmapTwo);
                motionVectors = mPEGPReg.MotionVectorsY;
                drawMV = true;
                pictureBoxTwo.Image = null;
                pictureBoxTwo.Image = new Bitmap(Compression.displayBitmap);
                pictureBoxTwo.Refresh();
            }
        }

        /// <summary>
        /// MPEG two currently loaded frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mPEGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap bitmapOne = new Bitmap(pictureBoxOne.Image);
            compressedBitmap = Compression.JPEGCompression(bitmapOne, pictureBoxOne.Image.Width, pictureBoxOne.Image.Height);
            pictureBoxOne.Image = new Bitmap(compressedBitmap);
            Bitmap bitmapTwo = new Bitmap(pictureBoxTwo.Image);

        }

        /// <summary>
        /// Add search range by one limited by upper search range
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void addSearchAreaRange_Click(object sender, EventArgs e)
        {
            Compression.SearchArea = Int32.Parse(currentSearchAreaRange.Text) + 1;
            if (Compression.SearchArea > Compression.UPPER_SEARCH_RANGE)
            {
                Compression.SearchArea = Compression.UPPER_SEARCH_RANGE;
            }
            else
            {
                currentSearchAreaRange.Text = "" + Compression.SearchArea;
            }
        }

        /// <summary>
        /// Subtract search range by one limited by lower search range
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void subtractSearchAreaRange_Click(object sender, EventArgs e)
        {
            Compression.SearchArea = Int32.Parse(currentSearchAreaRange.Text) - 1;
            if (Compression.SearchArea < Compression.LOWER_SEARCH_RANGE)
            {
                Compression.SearchArea = Compression.LOWER_SEARCH_RANGE;
            }
            else
            {
                currentSearchAreaRange.Text = "" + Compression.SearchArea;
            }
        }
    }
}
