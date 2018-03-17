﻿using System.Drawing;
using System.Drawing.Drawing2D;

namespace ImageCompressionJMPEG
{
    /// <summary>
    /// Cusotmized slider for volume control based on Slider.cs
    /// </summary>
    class CustomSlider : Slider
    {
        SolidBrush brush;
        SolidBrush brushTwo;
        GraphicsPath p = new GraphicsPath();
        int height;
        int yPos;
        int xPos;
        float width;
        int radius;

        /// <summary>
        /// Constructor for custom slider
        /// </summary>
        /// <param name="height">Height of slider</param>
        /// <param name="width">width of slider</param>
        /// <param name="radius">Radius of rounded corners</param>
        /// <param name="brush">Color brush</param>
        /// <param name="brushTwo">Second color brush</param>
        public CustomSlider(int height, int width, int radius, SolidBrush brush, SolidBrush brushTwo)
        {
            this.brush = brush;
            this.brushTwo = brushTwo;
            this.height = height;
            this.Width1 = width - 15;
            this.radius = height / 2;
            this.Value = 0;
        }

        public float Width1 { get => width; set => width = value; }

        /// <summary>
        /// Draws the background bar for the custom slider
        /// </summary>
        /// <param name="graphics">Graphic object</param>
        protected override void DrawBar(Graphics graphics)
        {
            this.yPos = 7 - height / 2;
            this.xPos = 7;
            GraphicsPath path = RoundedRectangle.Create(xPos, yPos, (int)Width1, height, radius);
            graphics.FillPath(brush, path);
            path = RoundedRectangle.Create(xPos, yPos, (int)((Width1 + 5) * this.Percent), height, radius);
            graphics.FillPath(brushTwo, path);
        }
    }
}
