using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ImageCompressionJMPEG
{
    /// <summary>
    /// Hold info of minimum number of data before calling a thread
    /// and number of threads to be used
    /// </summary>
    public struct ThreadSetting
    {
        public static int THREAD_THRESHOLD = 8;
        public static int threadNum = 4;
    }

    public partial class ImageCompressoin : Form
    {
        /// <summary>
        /// For overriding movement of window
        /// </summary>
        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;
        /// <summary>
        /// Custom GUI offset
        /// </summary>
        private const int CLOSE_FORM_HORZ_OFFSET = 30;
        private const int PANEL_VERT_OFFSET = 25;
        private const int PICTUREBOX_OFFSET = 25;
        private const int MAX_FORM_HORZ_OFFSET = 60;
        private const int MIN_FORM_HORZ_OFFSET = 90;
        private const int MOTIONVECTOR_HORZ_OFFSET = 0;
        private const int MOTIONVECTOR_PANEL_HEIGHT = 30;
        private const int LEFT_OFFSET = 14;
        private const int LABEL_SIZE = 24;
        private const int BUTTON_GAP = 3;
        private const int NUM_DISPLAY_FRAME = 6;
        private const float BUTTON_FONT_SIZE = 8f;
        private const int VIDEO_PANEL_HEIGHT = 30;
        private const int VIDEO_BUTTON_SIZE = 30;
        private const float VIDEO_FONT_SIZE = 8.25f;
        private const int COMPRESSION_RATIO_CAPTION_WIDTH = 160;
        private const int COMPRESSION_RATIO_WIDTH = 40;
        /// <summary>
        /// Main GUI variables
        /// </summary>
        private PictureBox pictureBoxOne;
        private PictureBox pictureBoxTwo;
        private PictureBox pictureBoxThree;
        private PictureBox pictureBoxGrayscaleLeft;
        private PictureBox pictureBoxGrayscaleRight;
        private bool addToOne = true;
        private Panel panel;
        private Panel motionVectorInfoPanel;
        private CustomButton closeForm;
        private CustomButton minForm;
        private CustomButton maxForm;
        private Label currentSearchAreaRange;
        private CustomButton addSearchAreaRange;
        private CustomButton subtractSearchAreaRange;
        private CustomButton jpegView;
        private CustomButton mpegView;
        private CustomButton grayscaleView;
        private Bitmap compressedBitmap;
        private Bitmap[] inputFrames;
        private byte[] compressedByteArrayJPEG;
        private byte[] compressedByteArrayMPEG;
        private JPEGInfo jpegInfo;
        private MPEGInfo mpegInfo;
        private Color themeColor;
        private Color themeBackgroundColor;
        private Color themeBackgroundColorTwo;
        private TextBox title;
        private Vector[] motionVectors;
        private bool drawMV = false;
        /// <summary>
        /// custom slider panel for play frame
        /// </summary>
        private Panel playFrameSliderPanel;
        private CustomSlider playFrameSlider;
        private float playFrameSliderValue;
        /// <summary>
        /// video control panel
        /// </summary>
        private Panel videoControlPanel;
        private CustomButton playBegin;
        private CustomButton playEnd;
        private CustomButton playPause;
        private System.Windows.Forms.Timer playTimer;
        private int currentFrame = -1;
        private bool playing = false;
        /// <summary>
        /// Compressoin ratio panel for jpeg
        /// </summary>
        private Panel compressionRatioPanelJPEG;
        private Label compressionRatioCaptionJPEG;
        private Label compressionRatioJPEG;
        private int compressedRatioJPEG;
        private CustomSlider compressionRatioSliderJPEG;
        private int compressionRatioValueJPEG = 100;
        private System.Windows.Forms.Timer compressionRateTimerJPEG;
        /// <summary>
        /// Compression ratio panel for mpeg
        /// </summary>
        private Panel compressionRatioPanelMPEG;
        private Label compressionRatioCaptionMPEG;
        private Label compressionRatioMPEG;
        private int compressedRatioMPEG;
        private CustomSlider compressionRatioSliderMPEG;
        private int compressionRatioValueMPEG = 100;
        private System.Windows.Forms.Timer compressionRateTimerMPEG;

        public ImageCompressoin()
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
        /// Initializes custom GUI
        /// </summary>
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
            // pictureBoxThree shown when mpeg view active
            //
            pictureBoxThree = new PictureBox();
            pictureBoxThree.Name = "pictureBoxThree";
            pictureBoxThree.TabStop = false;
            pictureBoxThree.BackColor = themeBackgroundColorTwo;
            pictureBoxThree.SizeMode = PictureBoxSizeMode.StretchImage;
            //
            // pictureBoxGrayScaleLeft
            //
            pictureBoxGrayscaleLeft = new PictureBox();
            pictureBoxGrayscaleLeft.Location = new System.Drawing.Point(PICTUREBOX_OFFSET, 27 + PICTUREBOX_OFFSET);
            pictureBoxGrayscaleLeft.Name = "pictureBoxGrayscaleLeft";
            pictureBoxGrayscaleLeft.TabStop = false;
            pictureBoxGrayscaleLeft.BackColor = themeBackgroundColorTwo;
            pictureBoxGrayscaleLeft.SizeMode = PictureBoxSizeMode.StretchImage;
            //
            // pictureBoxGrayscaleRight
            //
            pictureBoxGrayscaleRight = new PictureBox();
            pictureBoxGrayscaleRight.Name = "pictureBoxGrayscaleRight";
            pictureBoxGrayscaleRight.TabStop = false;
            pictureBoxGrayscaleRight.BackColor = themeBackgroundColorTwo;
            pictureBoxGrayscaleRight.SizeMode = PictureBoxSizeMode.StretchImage;
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
            // context menu for pictureBoxThree
            //
            ContextMenu cmThree = new ContextMenu();
            MenuItem mnuAddImageThree = new MenuItem("Add image");
            MenuItem mnuRemoveImageThree = new MenuItem("Remove image");
            mnuAddImageThree.Click += new EventHandler(addImageThree);
            mnuRemoveImageThree.Click += new EventHandler(removeImageThree);
            cmThree.MenuItems.Add(mnuAddImageThree);
            cmThree.MenuItems.Add(mnuRemoveImageThree);
            pictureBoxThree.ContextMenu = cmThree;
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
            minForm.Font = new System.Drawing.Font("Microsoft Sans Serif",
                BUTTON_FONT_SIZE, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            title.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, 
                System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            motionVectorInfoPanel.BackColor = themeBackgroundColor;
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
            currentSearchAreaRange.Font = new System.Drawing.Font("Microsoft Sans Serif", 
                BUTTON_FONT_SIZE, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            addSearchAreaRange.Font = new System.Drawing.Font("Microsoft Sans Serif", 
                BUTTON_FONT_SIZE, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            addSearchAreaRange.Location = new System.Drawing.Point(PICTUREBOX_OFFSET + LABEL_SIZE, BUTTON_GAP);
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
            subtractSearchAreaRange.Font = new System.Drawing.Font("Microsoft Sans Serif", 
                BUTTON_FONT_SIZE, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, (byte)0);
            subtractSearchAreaRange.Location = new System.Drawing.Point(PICTUREBOX_OFFSET + LABEL_SIZE * 2, BUTTON_GAP);
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
            //
            // jpegView
            //
            jpegView = new CustomButton();
            jpegView.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            jpegView.FlatAppearance.BorderSize = 0;
            jpegView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            jpegView.Font = new System.Drawing.Font("Microsoft Sans Serif", BUTTON_FONT_SIZE, 
                System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, (byte)0);
            jpegView.Name = "jpegView";
            jpegView.Size = new System.Drawing.Size(LABEL_SIZE, LABEL_SIZE);
            jpegView.TabStop = false;
            jpegView.Text = "\u004A";
            jpegView.ForeColor = themeColor;
            jpegView.UseMnemonic = false;
            jpegView.UseVisualStyleBackColor = true;
            jpegView.BackColor = themeBackgroundColorTwo;
            jpegView.Click += new System.EventHandler(this.jpegView_Click);
            motionVectorInfoPanel.Controls.Add(jpegView);
            //
            // mpegView
            //
            mpegView = new CustomButton();
            mpegView.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            mpegView.FlatAppearance.BorderSize = 0;
            mpegView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            mpegView.Font = new System.Drawing.Font("Microsoft Sans Serif", BUTTON_FONT_SIZE, 
                System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            mpegView.Name = "mpegView";
            mpegView.Size = new System.Drawing.Size(LABEL_SIZE, LABEL_SIZE);
            mpegView.TabStop = false;
            mpegView.Text = "\u004D";
            mpegView.ForeColor = themeColor;
            mpegView.UseMnemonic = false;
            mpegView.UseVisualStyleBackColor = true;
            mpegView.BackColor = themeBackgroundColorTwo;
            mpegView.Click += new System.EventHandler(this.mpegView_Click);
            motionVectorInfoPanel.Controls.Add(mpegView);
            //
            // grayscaleView
            //
            grayscaleView = new CustomButton();
            grayscaleView.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            grayscaleView.FlatAppearance.BorderSize = 0;
            grayscaleView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            grayscaleView.Font = new System.Drawing.Font("Microsoft Sans Serif", BUTTON_FONT_SIZE, 
                System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            grayscaleView.Name = "mpegView";
            grayscaleView.Size = new System.Drawing.Size(LABEL_SIZE, LABEL_SIZE);
            grayscaleView.TabStop = false;
            grayscaleView.Text = "\u0047";
            grayscaleView.ForeColor = themeColor;
            grayscaleView.UseMnemonic = false;
            grayscaleView.UseVisualStyleBackColor = true;
            grayscaleView.BackColor = themeBackgroundColorTwo;
            grayscaleView.Click += new System.EventHandler(this.grayscaleView_Click);
            motionVectorInfoPanel.Controls.Add(grayscaleView);
            //
            // playFrameSliderPanel
            //
            playFrameSliderPanel = new Panel();
            playFrameSliderPanel.BackColor = themeBackgroundColor;
            //
            // playFramdeSlider
            //
            playFrameSlider = new CustomSlider(5, this.Width - PICTUREBOX_OFFSET * 3, PICTUREBOX_OFFSET,
                new SolidBrush(Color.FromArgb(50, 100, 100, 100)), new SolidBrush(themeColor));
            playFrameSlider.Location = new Point(5, 5);
            playFrameSlider.Name = "play frame slider";
            playFrameSlider.BackColor = Color.Black;
            Image knobImage = Image.FromFile("..\\..\\img\\circleGrey.png");
            Image knobHoverImage = Image.FromFile("..\\..\\img\\circleLightGreen.png");
            playFrameSlider.KnobImage = new Bitmap(knobImage);
            playFrameSlider.KnobHoverImage = new Bitmap(knobHoverImage);
            playFrameSlider.MouseUp += new MouseEventHandler(this.playFrameSlider_MouseUp);
            playFrameSlider.MouseMove += new MouseEventHandler(this.playFrameSlider_MouseMove);
            playFrameSliderPanel.Controls.Add(playFrameSlider);
            //
            // videoControlPanel
            //
            videoControlPanel = new Panel();
            videoControlPanel.BackColor = themeBackgroundColor;
            //
            // playBegin
            //
            playBegin = new CustomButton();
            playBegin.Size = new Size(VIDEO_BUTTON_SIZE, VIDEO_BUTTON_SIZE);
            playBegin.Location = new Point(0, 0);
            playBegin.BackgroundImageLayout = ImageLayout.Zoom;
            playBegin.FlatAppearance.BorderSize = 0;
            playBegin.FlatStyle = FlatStyle.Flat;
            playBegin.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            playBegin.Name = "playBegin";
            playBegin.Text = "\u25B6";
            playBegin.ForeColor = themeColor;
            playBegin.UseMnemonic = false;
            playBegin.UseVisualStyleBackColor = true;
            playBegin.BackColor = themeBackgroundColor;
            playBegin.Click += new EventHandler(this.playBegin_Click);
            videoControlPanel.Controls.Add(playBegin);
            //
            // playEnd
            //
            playEnd = new CustomButton();
            playEnd.Size = new Size(VIDEO_BUTTON_SIZE, VIDEO_BUTTON_SIZE);
            playEnd.Location = new Point(VIDEO_BUTTON_SIZE, 0);
            playEnd.BackgroundImageLayout = ImageLayout.Zoom;
            playEnd.FlatAppearance.BorderSize = 0;
            playEnd.FlatStyle = FlatStyle.Flat;
            playEnd.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            playEnd.Name = "playEnd";
            playEnd.Text = "\u23F9";
            playEnd.ForeColor = themeColor;
            playEnd.UseMnemonic = false;
            playEnd.UseVisualStyleBackColor = true;
            playEnd.BackColor = themeBackgroundColor;
            playEnd.Click += new EventHandler(this.playEnd_Click);
            videoControlPanel.Controls.Add(playEnd);
            //
            // playPause
            //
            playPause = new CustomButton();
            playPause.Size = new Size(VIDEO_BUTTON_SIZE, VIDEO_BUTTON_SIZE);
            playPause.Location = new Point(0, 0);
            playPause.BackgroundImageLayout = ImageLayout.Zoom;
            playPause.FlatAppearance.BorderSize = 0;
            playPause.FlatStyle = FlatStyle.Flat;
            playPause.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            playPause.Name = "playPause";
            playPause.Text = "❚❚";
            playPause.ForeColor = themeColor;
            playPause.UseMnemonic = false;
            playPause.UseVisualStyleBackColor = true;
            playPause.BackColor = themeBackgroundColor;
            playPause.Click += new EventHandler(this.playPause_Click);
            //
            // compressionRatioPanelJPEG
            //
            compressionRatioPanelJPEG = new Panel();
            compressionRatioPanelJPEG.BackColor = themeBackgroundColor;
            compressionRatioPanelJPEG.Location = new Point(PICTUREBOX_OFFSET + LABEL_SIZE * 4, 0);
            motionVectorInfoPanel.Controls.Add(compressionRatioPanelJPEG);
            //
            // compressionRatioCaptionJPEG
            //
            compressionRatioCaptionJPEG = new Label();
            compressionRatioCaptionJPEG.BorderStyle = System.Windows.Forms.BorderStyle.None;
            compressionRatioCaptionJPEG.Location = new System.Drawing.Point(0, 0);
            compressionRatioCaptionJPEG.Size = new System.Drawing.Size(COMPRESSION_RATIO_CAPTION_WIDTH, PICTUREBOX_OFFSET);
            compressionRatioCaptionJPEG.Font = new System.Drawing.Font("Microsoft Sans Serif",
                BUTTON_FONT_SIZE, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            compressionRatioCaptionJPEG.TextAlign = ContentAlignment.MiddleCenter;
            compressionRatioCaptionJPEG.Text = "JPEG Compression Ratio: ";
            compressionRatioCaptionJPEG.ForeColor = themeColor;
            compressionRatioCaptionJPEG.BackColor = themeBackgroundColorTwo;
            compressionRatioPanelJPEG.Controls.Add(compressionRatioCaptionJPEG);
            //
            // compressionRatioJPEG
            //
            compressionRatioJPEG = new Label();
            compressionRatioJPEG.BorderStyle = System.Windows.Forms.BorderStyle.None;
            compressionRatioJPEG.Location = new System.Drawing.Point(COMPRESSION_RATIO_CAPTION_WIDTH, 0);
            compressionRatioJPEG.Size = new System.Drawing.Size(COMPRESSION_RATIO_WIDTH, PICTUREBOX_OFFSET);
            compressionRatioJPEG.Font = new System.Drawing.Font("Microsoft Sans Serif",
                BUTTON_FONT_SIZE, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            compressionRatioJPEG.TextAlign = ContentAlignment.MiddleCenter;
            compressionRatioJPEG.Text = compressionRatioValueJPEG + "%";
            compressionRatioJPEG.ForeColor = Color.Cyan;
            compressionRatioJPEG.BackColor = themeBackgroundColorTwo;
            compressionRatioPanelJPEG.Controls.Add(compressionRatioJPEG);
            //
            // compressionRatioSliderJPEG
            //
            compressionRatioSliderJPEG = new CustomSlider(5, this.Width - (PICTUREBOX_OFFSET + LABEL_SIZE * 4) * 2 - COMPRESSION_RATIO_CAPTION_WIDTH - COMPRESSION_RATIO_WIDTH - 15, PICTUREBOX_OFFSET,
                new SolidBrush(themeColor), new SolidBrush(Color.Cyan));
            compressionRatioSliderJPEG.Location = new Point(COMPRESSION_RATIO_CAPTION_WIDTH + COMPRESSION_RATIO_WIDTH, 5);
            compressionRatioSliderJPEG.Name = "compression ratio slider jpeg";
            compressionRatioSliderJPEG.BackColor = Color.Black;
            compressionRatioPanelJPEG.Controls.Add(compressionRatioSliderJPEG);
            compressionRatioSliderJPEG.Value = compressionRatioValueJPEG;
            //
            // compressionRatioPanelMPEG
            //
            compressionRatioPanelMPEG = new Panel();
            compressionRatioPanelMPEG.BackColor = themeBackgroundColor;
            compressionRatioPanelMPEG.Location = new Point(PICTUREBOX_OFFSET + LABEL_SIZE * 4, 0);
            //
            // compressionRatioCaptionMPEG
            //
            compressionRatioCaptionMPEG = new Label();
            compressionRatioCaptionMPEG.BorderStyle = System.Windows.Forms.BorderStyle.None;
            compressionRatioCaptionMPEG.Location = new System.Drawing.Point(0, 0);
            compressionRatioCaptionMPEG.Size = new System.Drawing.Size(COMPRESSION_RATIO_CAPTION_WIDTH, PICTUREBOX_OFFSET);
            compressionRatioCaptionMPEG.Font = new System.Drawing.Font("Microsoft Sans Serif",
                BUTTON_FONT_SIZE, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            compressionRatioCaptionMPEG.TextAlign = ContentAlignment.MiddleCenter;
            compressionRatioCaptionMPEG.Text = "MPEG Compression Ratio: ";
            compressionRatioCaptionMPEG.ForeColor = themeColor;
            compressionRatioCaptionMPEG.BackColor = themeBackgroundColorTwo;
            compressionRatioPanelMPEG.Controls.Add(compressionRatioCaptionMPEG);
            //
            // compressionRatioMPEG
            //
            compressionRatioMPEG = new Label();
            compressionRatioMPEG.BorderStyle = System.Windows.Forms.BorderStyle.None;
            compressionRatioMPEG.Location = new System.Drawing.Point(COMPRESSION_RATIO_CAPTION_WIDTH, 0);
            compressionRatioMPEG.Size = new System.Drawing.Size(COMPRESSION_RATIO_WIDTH, PICTUREBOX_OFFSET);
            compressionRatioMPEG.Font = new System.Drawing.Font("Microsoft Sans Serif",
                BUTTON_FONT_SIZE, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            compressionRatioMPEG.TextAlign = ContentAlignment.MiddleCenter;
            compressionRatioMPEG.Text = compressionRatioValueJPEG + "%";
            compressionRatioMPEG.ForeColor = Color.Cyan;
            compressionRatioMPEG.BackColor = themeBackgroundColorTwo;
            compressionRatioPanelMPEG.Controls.Add(compressionRatioMPEG);
            //
            // compressionRatioSliderMPEG
            //
            compressionRatioSliderMPEG = new CustomSlider(5, this.Width - (PICTUREBOX_OFFSET + LABEL_SIZE * 4) * 2 - COMPRESSION_RATIO_CAPTION_WIDTH - COMPRESSION_RATIO_WIDTH - 15, PICTUREBOX_OFFSET,
                new SolidBrush(themeColor), new SolidBrush(Color.Cyan));
            compressionRatioSliderMPEG.Location = new Point(COMPRESSION_RATIO_CAPTION_WIDTH + COMPRESSION_RATIO_WIDTH, 5);
            compressionRatioSliderMPEG.Name = "compression ratio slider mpeg";
            compressionRatioSliderMPEG.BackColor = Color.Black;
            compressionRatioPanelMPEG.Controls.Add(compressionRatioSliderMPEG);
            compressionRatioSliderMPEG.Value = compressionRatioValueMPEG;
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
                pictureBoxThree.Size = new Size(w - PICTUREBOX_OFFSET * 2, h - 54 - PICTUREBOX_OFFSET * 2 - VIDEO_PANEL_HEIGHT);
                pictureBoxThree.Location = new Point(PICTUREBOX_OFFSET, 27 + PICTUREBOX_OFFSET);
                pictureBoxGrayscaleLeft.Size = new Size(w / 2 - PICTUREBOX_OFFSET * 3 / 2, h - 54 - PICTUREBOX_OFFSET * 2);
                pictureBoxGrayscaleRight.Size = new Size(w / 2 - PICTUREBOX_OFFSET * 3 / 2, h - 54 - PICTUREBOX_OFFSET * 2);
                pictureBoxGrayscaleRight.Location = new Point(w / 2 + PICTUREBOX_OFFSET / 2, 27 + PICTUREBOX_OFFSET);
                motionVectorInfoPanel.Width = w;
                grayscaleView.Location = new System.Drawing.Point(motionVectorInfoPanel.Size.Width - LABEL_SIZE * 3 - PICTUREBOX_OFFSET, BUTTON_GAP);
                jpegView.Location = new Point(motionVectorInfoPanel.Size.Width - LABEL_SIZE * 2 - PICTUREBOX_OFFSET, BUTTON_GAP);
                mpegView.Location = new System.Drawing.Point(motionVectorInfoPanel.Size.Width - LABEL_SIZE - PICTUREBOX_OFFSET, BUTTON_GAP);
                playFrameSliderPanel.Size = new Size(w - PICTUREBOX_OFFSET * 2, PICTUREBOX_OFFSET);
                playFrameSliderPanel.Location = new Point(PICTUREBOX_OFFSET, 27 + PICTUREBOX_OFFSET + pictureBoxThree.Height);
                playFrameSlider.Size = new Size(w - PICTUREBOX_OFFSET * 2 - 10, 15);
                playFrameSlider.Width1 = w - PICTUREBOX_OFFSET * 3;
                videoControlPanel.Size = new Size(w - PICTUREBOX_OFFSET * 2, VIDEO_PANEL_HEIGHT);
                videoControlPanel.Location = new Point(PICTUREBOX_OFFSET, 27 + PICTUREBOX_OFFSET * 2 + pictureBoxThree.Height);
                compressionRatioPanelJPEG.Size = new Size(w - (PICTUREBOX_OFFSET + LABEL_SIZE * 4) * 2, PICTUREBOX_OFFSET);
                compressionRatioSliderJPEG.Size = new Size(compressionRatioPanelJPEG.Size.Width - COMPRESSION_RATIO_CAPTION_WIDTH - COMPRESSION_RATIO_WIDTH, PICTUREBOX_OFFSET);
                compressionRatioSliderJPEG.Width1 = compressionRatioSliderJPEG.Size.Width - 20;
                compressionRatioPanelMPEG.Size = new Size(w - (PICTUREBOX_OFFSET + LABEL_SIZE * 4) * 2, PICTUREBOX_OFFSET);
                compressionRatioSliderMPEG.Size = new Size(compressionRatioPanelMPEG.Size.Width - COMPRESSION_RATIO_CAPTION_WIDTH - COMPRESSION_RATIO_WIDTH, PICTUREBOX_OFFSET);
                compressionRatioSliderMPEG.Width1 = compressionRatioSliderMPEG.Size.Width - 20;
            }
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
        /// Play frames when clicked
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void playBegin_Click(object sender, EventArgs e)
        {
            if (inputFrames != null)
            {
                playing = true;
                videoControlPanel.Controls.Add(playPause);
                videoControlPanel.Controls.Remove(playBegin);
                if (playTimer == null)
                {
                    playTimer = new System.Windows.Forms.Timer();
                    playTimer.Tick += new EventHandler(playFrames);
                    playTimer.Interval = 33;
                }
                playTimer.Start();
            }
        }

        /// <summary>
        /// Pause play frame timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playPause_Click(object sender, EventArgs e)
        {
            if (playing)
            {
                videoControlPanel.Controls.Remove(playPause);
                videoControlPanel.Controls.Add(playBegin);
                if (playTimer != null)
                {
                    playTimer.Stop();
                }
                playing = false;
            }
        }

        /// <summary>
        /// End play frame timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playEnd_Click(object sender, EventArgs e)
        {
            if (playTimer != null && inputFrames != null)
            {
                playPause_Click(sender, e);
                if (inputFrames.Length != 0)
                {
                    pictureBoxThree.Image = inputFrames[0];
                }
                currentFrame = -1;
                playFrameSlider.Value = 0;
                playFrameSlider.Refresh();
            }
        }

        /// <summary>
        /// Replace current frame with next frame if there is one
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void playFrames(object sender, EventArgs e)
        {
            if (currentFrame + 1 < inputFrames.Length)
            {
                pictureBoxThree.Image = inputFrames[++currentFrame];
                playFrameSlider.Value = (int)(((float)currentFrame / (inputFrames.Length - 1)) * 100);
                playFrameSlider.Refresh();
            }
            else
            {
                currentFrame = -1;
                if (playTimer != null)
                {
                    playTimer.Stop();
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openImage();
        }

        /// <summary>
        /// Handle OpenFileDialog cases
        /// </summary>
        private void openImage()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Open Image";
                dialog.Filter = "images|*.JPG; *.PNG; *.GJF; *.bmp; *.CJPG; *.CMPEG; * .CMPEG";
                dialog.Multiselect = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (Compression.jView && Path.GetExtension(dialog.FileNames[0]).Equals(".CJPG"))
                    {
                        if (addToOne)
                        {
                            pictureBoxOne.Image = null;
                            pictureBoxOne.Image = Compression.JPEGDecompression(SaveAndLoad.loadByteArrayJPEG(File.ReadAllBytes(dialog.FileNames[0])));
                        }
                        else
                        {
                            pictureBoxTwo.Image = null;
                            pictureBoxTwo.Image = Compression.JPEGDecompression(SaveAndLoad.loadByteArrayJPEG(File.ReadAllBytes(dialog.FileNames[0])));
                        }
                    }
                    else if (Compression.mView && Path.GetExtension(dialog.FileNames[0]).Equals(".CMPEG"))
                    {
                        pictureBoxThree.Image = null;
                        inputFrames = Compression.MPEGDecompression(SaveAndLoad.loadByteArrayMPEG(File.ReadAllBytes(dialog.FileNames[0])));
                        pictureBoxThree.Image = inputFrames[0];
                    }
                    else if (Compression.jView && !Path.GetExtension(dialog.FileNames[0]).Equals(".CMPEG"))
                    {
                        if (addToOne)
                        {
                            pictureBoxOne.Image = null;
                            pictureBoxOne.Image = new Bitmap(dialog.FileNames[0]);
                        }
                        else
                        {
                            pictureBoxTwo.Image = null;
                            pictureBoxTwo.Image = new Bitmap(dialog.FileNames[0]);
                        }
                    }
                    else
                    {
                        inputFrames = new Bitmap[dialog.FileNames.Length];
                        for (int i = 0; i < dialog.FileNames.Length; i++)
                        {
                            inputFrames[i] = new Bitmap(dialog.FileNames[i]);
                        }
                        pictureBoxThree.Image = null;
                        pictureBoxThree.Image = new Bitmap(dialog.FileNames[0]);
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
            resetRatioJPEG();
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
            resetRatioJPEG();
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
            resetRatioJPEG();
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
            resetRatioJPEG();
            Refresh();
        }

        private void addImageThree(object sender, EventArgs e)
        {
            openImage();
            resetRatioMPEG();
        }

        private void removeImageThree(object sender, EventArgs e)
        {
            pictureBoxThree.Image = null;
            resetRatioMPEG();
        }

        /// <summary>
        /// JPEG the image on the left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jPEGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap(pictureBoxOne.Image);
            Bitmap grayscaleBitmap;
            Bitmap grayscaleBitmapTwo;
            jpegInfo = Compression.JPEGCompression(bitmap);
            compressedBitmap = Compression.JPEGDecompression(jpegInfo);
            pictureBoxTwo.Image = new Bitmap(compressedBitmap);
            Compression.Grayscale(bitmap, out grayscaleBitmap, out grayscaleBitmapTwo);
            updateGrayscale(grayscaleBitmap, grayscaleBitmapTwo);
            compressedByteArrayJPEG = SaveAndLoad.saveIntoByteArray(jpegInfo);
            updateCompressionRatioJPEG(bitmap, compressedByteArrayJPEG);
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
                    if (Path.GetExtension(saveFileDialog.FileName).Equals(".CJPG"))
                    {
                        BinaryWriter wr = new BinaryWriter(f);
                        if (jpegInfo.originalHeight != 0)
                        {
                            compressedByteArrayJPEG = SaveAndLoad.saveIntoByteArray(jpegInfo);
                            wr.Write(compressedByteArrayJPEG);
                        }
                        wr.Close();
                        f.Close();
                    }
                    if (Path.GetExtension(saveFileDialog.FileName).Equals(".CMPEG"))
                    {
                        BinaryWriter wr = new BinaryWriter(f);
                        if (mpegInfo.originalHeight != 0)
                        {
                            compressedByteArrayJPEG = SaveAndLoad.saveIntoByteArray(mpegInfo);
                            wr.Write(compressedByteArrayJPEG);
                        }
                        wr.Close();
                        f.Close();
                    }
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
                        if (y * heightScaler - (y + motionVectors[index].y) * heightScaler == 0 &&
                            x * widthScaler - (x + motionVectors[index].x) * widthScaler == 0)
                        {
                            e.Graphics.DrawEllipse(pen, x * widthScaler, y * heightScaler, 3, 3);
                        }
                        else
                        {
                            e.Graphics.DrawLine(pen, x * widthScaler, y * heightScaler,
                               (x + motionVectors[index].x) * widthScaler, (y + motionVectors[index].y) * heightScaler);
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
                Bitmap grayscaleBitmap;
                Bitmap grayscaleBitmapTwo;
                JPEGInfo iFrame = Compression.JPEGCompression(bitmapOne);
                Compression.Grayscale(bitmapOne, out grayscaleBitmap, out grayscaleBitmapTwo);
                compressedBitmap = Compression.JPEGDecompression(iFrame);
                pictureBoxOne.Image = new Bitmap(compressedBitmap);
                Bitmap bitmapTwo = new Bitmap(pictureBoxTwo.Image);
                PFrame mPEGPReg = Compression.MPEGMotionVector(compressedBitmap, bitmapTwo);
                motionVectors = mPEGPReg.MotionVectorsY;
                drawMV = true;
                pictureBoxTwo.Image = null;
                pictureBoxTwo.Image = new Bitmap(Compression.displayBitmap);
                pictureBoxTwo.Refresh();
                updateGrayscale(grayscaleBitmap, grayscaleBitmapTwo);
            }
        }

        private void updateGrayscale(Bitmap grayscaleBitmap, Bitmap grayscaleBitmapTwo)
        {
            pictureBoxGrayscaleLeft.Image = grayscaleBitmap;
            pictureBoxGrayscaleRight.Image = grayscaleBitmapTwo;
        }

        /// <summary>
        /// MPEG two currently loaded frame
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void mPEGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (inputFrames.Length > 0)
            {
                mpegInfo = Compression.MPEGCompression(inputFrames);
                inputFrames = Compression.MPEGDecompression(mpegInfo);
                pictureBoxThree.Image = new Bitmap(inputFrames[0]);
                compressedByteArrayMPEG = SaveAndLoad.saveIntoByteArray(mpegInfo);
                updateCompresionRatioMPEG(inputFrames, compressedByteArrayMPEG);
            }
        }

        /// <summary>
        /// Updates the compression ratio value
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="compressedByteArray"></param>
        private void updateCompressionRatioJPEG(Bitmap bitmap, byte[] compressedByteArray)
        {
            int originalSize = bitmap.Width * bitmap.Height * 3;
            int compressedSize = compressedByteArray.Length;
            compressedRatioJPEG = (int)((compressedSize / (double)originalSize) * 100);
            compressionRateTimerJPEG = new System.Windows.Forms.Timer();
            compressionRateTimerJPEG.Tick += new EventHandler(updateRatioJPEG);
            compressionRateTimerJPEG.Interval = 1;
            compressionRateTimerJPEG.Start();
        }

        /// <summary>
        /// Update ratio value event jpeg
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void updateRatioJPEG(object sender, EventArgs e)
        {
            if (compressedRatioJPEG < compressionRatioValueJPEG)
            {
                compressionRatioValueJPEG--;
                compressionRatioSliderJPEG.Value = compressionRatioValueJPEG;
                compressionRatioJPEG.Text = compressionRatioValueJPEG + "%";
                compressionRateTimerJPEG.Interval++;
            }
            else
            {
                compressionRateTimerJPEG.Stop();
            }
        }

        /// <summary>
        /// resets the compression ratio for JPEG
        /// </summary>
        private void resetRatioJPEG()
        {
            compressionRatioValueJPEG = 100;
            compressionRatioSliderJPEG.Value = compressionRatioValueJPEG;
            compressionRatioJPEG.Text = compressionRatioValueJPEG + "%";
        }

        /// <summary>
        /// update the compression ratio value for mpeg
        /// </summary>
        /// <param name="inputFrames">input frames</param>
        /// <param name="compressedByteArray">compressed frames</param>
        private void updateCompresionRatioMPEG(Bitmap[] inputFrames, byte[] compressedByteArray)
        {
            int originalSize = inputFrames[0].Width * inputFrames[0].Height * 3 * inputFrames.Length;
            int compressedSize = compressedByteArray.Length;
            compressedRatioMPEG = (int)((compressedSize / (double)originalSize) * 100);
            compressionRateTimerMPEG = new System.Windows.Forms.Timer();
            compressionRateTimerMPEG.Tick += new EventHandler(updateRatioMPEG);
            compressionRateTimerMPEG.Interval = 1;
            compressionRateTimerMPEG.Start();
        }

        /// <summary>
        /// update ratio value event mpeg
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void updateRatioMPEG(object sender, EventArgs e)
        {
            if (compressedRatioMPEG < compressionRatioValueMPEG)
            {
                compressionRatioValueMPEG--;
                compressionRatioSliderMPEG.Value = compressionRatioValueMPEG;
                compressionRatioMPEG.Text = compressionRatioValueMPEG + "%";
                compressionRateTimerMPEG.Interval++;
            }
            else
            {
                compressionRateTimerMPEG.Stop();
            }
        }

        /// <summary>
        /// Reset the compression ratio for mpeg
        /// </summary>
        private void resetRatioMPEG()
        {
            compressionRatioValueMPEG = 100;
            compressionRatioSliderMPEG.Value = compressionRatioValueMPEG;
            compressionRatioMPEG.Text = compressionRatioValueMPEG + "%";
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

        /// <summary>
        /// Switch to jpeg view
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void jpegView_Click(object sender, EventArgs e)
        {
            panel.Controls.Add(pictureBoxOne);
            panel.Controls.Add(pictureBoxTwo);
            panel.Controls.Remove(pictureBoxThree);
            panel.Controls.Remove(pictureBoxGrayscaleLeft);
            panel.Controls.Remove(pictureBoxGrayscaleRight);
            motionVectorInfoPanel.Controls.Add(compressionRatioPanelJPEG);
            motionVectorInfoPanel.Controls.Remove(compressionRatioPanelMPEG);
            removeVideoPanel();
            Compression.mView = false;
            Compression.jView = true;
            Compression.gView = false;
        }

        /// <summary>
        /// Switch to mpeg view
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void mpegView_Click(object sender, EventArgs e)
        {
            panel.Controls.Remove(pictureBoxOne);
            panel.Controls.Remove(pictureBoxTwo);
            panel.Controls.Add(pictureBoxThree);
            panel.Controls.Remove(pictureBoxGrayscaleLeft);
            panel.Controls.Remove(pictureBoxGrayscaleRight);
            addVideoPanel();
            motionVectorInfoPanel.Controls.Add(compressionRatioPanelMPEG);
            motionVectorInfoPanel.Controls.Remove(compressionRatioPanelJPEG);
            Compression.mView = true;
            Compression.jView = false;
            Compression.gView = false;
        }

        /// <summary>
        /// Grayscale view
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void grayscaleView_Click(object sender, EventArgs e)
        {
            panel.Controls.Remove(pictureBoxOne);
            panel.Controls.Remove(pictureBoxTwo);
            panel.Controls.Remove(pictureBoxThree);
            panel.Controls.Add(pictureBoxGrayscaleLeft);
            panel.Controls.Add(pictureBoxGrayscaleRight);
            removeVideoPanel();
            motionVectorInfoPanel.Controls.Add(compressionRatioPanelJPEG);
            motionVectorInfoPanel.Controls.Remove(compressionRatioPanelMPEG);
            Compression.mView = false;
            Compression.jView = false;
            Compression.gView = true;
        }

        private void addVideoPanel()
        {
            panel.Controls.Add(playFrameSliderPanel);
            panel.Controls.Add(videoControlPanel);
        }

        private void removeVideoPanel()
        {
            panel.Controls.Remove(playFrameSliderPanel);
            panel.Controls.Remove(videoControlPanel);
        }

        /// <summary>
        /// Handle customSlider mouse up event
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void playFrameSlider_MouseUp(object sender, EventArgs e)
        {
            playFrameSliderValue = playFrameSlider.Value;
        }

        /// <summary>
        /// Handle customSlider mouse move event
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event</param>
        private void playFrameSlider_MouseMove(object sender, EventArgs e)
        {
            playFrameSliderValue = playFrameSlider.Value;
        }

        /// <summary>
        /// Custom tool strip renderer
        /// </summary>
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

    }
}
