﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;

namespace RH.HeadShop.Controls.TrackBar
{

    #region Declaration

    /// <summary>
    /// Represents the method that will handle a change in value.
    /// </summary>
    public delegate void ValueChangedHandler(object sender, decimal value);

    public enum MACBorderStyle
    {
        /// <summary>
        /// No border.
        /// </summary>
        None,
        /// <summary>
        /// A dashed border.
        /// </summary>
        Dashed, //from ButtonBorderStyle Enumeration
        /// <summary>
        /// A dotted-line border.
        /// </summary>
        Dotted, //from ButtonBorderStyle Enumeration
        /// <summary>
        /// A sunken border.
        /// </summary>
        Inset, //from ButtonBorderStyle Enumeration
        /// <summary>
        /// A raised border.
        /// </summary>
        Outset, //from ButtonBorderStyle Enumeration
        /// <summary>
        /// A solid border.
        /// </summary>
        Solid, //from ButtonBorderStyle Enumeration

        /// <summary>
        /// The border is drawn outside the specified rectangle, preserving the dimensions of the rectangle for drawing.
        /// </summary>
        Adjust, //from Border3DStyle Enumeration
        /// <summary>
        /// The inner and outer edges of the border have a raised appearance.
        /// </summary>
        Bump, //from Border3DStyle Enumeration
        /// <summary>
        /// The inner and outer edges of the border have an etched appearance.
        /// </summary>
        Etched, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has no three-dimensional effects.
        /// </summary>
        Flat, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has raised inner and outer edges.
        /// </summary>
        Raised, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has a raised inner edge and no outer edge.
        /// </summary>
        RaisedInner, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has a raised outer edge and no inner edge.
        /// </summary>
        RaisedOuter, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has sunken inner and outer edges.
        /// </summary>
        Sunken, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has a sunken inner edge and no outer edge.
        /// </summary>
        SunkenInner, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has a sunken outer edge and no inner edge.
        /// </summary>
        SunkenOuter //from Border3DStyle Enumeration
    }

    #endregion

    [Designer(typeof(TrackBarDesigner))]
    [DefaultProperty("Maximum")]
    [DefaultEvent("ValueChanged")]
    public class TrackBarEx : Control
    {

        #region Private Members

        private int value;
        private int minimum;
        private int maximum = 10;

        private int largeChange = 2;
        private int smallChange = 1;

        private Orientation orientation = Orientation.Horizontal;

        private MACBorderStyle borderStyle = MACBorderStyle.None;
        private Color borderColor = SystemColors.ActiveBorder;

        private Size trackerSize = new Size(10, 20);
        private int indentWidth = 6;
        private int indentHeight = 6;

        private int tickHeight = 2;
        private int tickFrequency = 1;
        private Color tickColor = Color.Black;
        private TickStyle tickStyle = TickStyle.BottomRight;
        private TickStyle textTickStyle = TickStyle.BottomRight;

        private int trackLineHeight = 3;
        private Color trackLineColor = SystemColors.Control;

        private Color trackerColor = SystemColors.Control;
        public RectangleF TrackerRect = RectangleF.Empty;

        private bool autoSize = true;

        private bool leftButtonDown;
        private float mouseStartPos = -1;

        /// <summary>
        /// Occurs when the property Value has been changed.
        /// </summary>
        public event ValueChangedHandler ValueChanged;
        /// <summary>
        /// Occurs when either a mouse or keyboard action moves the slider.
        /// </summary>
        public event EventHandler Scroll;

        #endregion

        public TrackBarEx()
        {
            MouseDown += OnMouseDownSlider;
            MouseUp += OnMouseUpSlider;
            MouseMove += OnMouseMoveSlider;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.DoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);

            Font = new Font("Verdana", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ForeColor = Color.FromArgb(123, 125, 123);
            BackColor = Color.Transparent;

            tickColor = Color.FromArgb(148, 146, 148);
            tickHeight = 4;

            trackerColor = Color.FromArgb(24, 130, 198);
            trackerSize = new Size(16, 16);
            indentWidth = 6;
            indentHeight = 6;

            trackLineColor = Color.FromArgb(90, 93, 90);
            trackLineHeight = 3;

            borderStyle = MACBorderStyle.None;
            borderColor = SystemColors.ActiveBorder;

            autoSize = true;
            Height = FitSize.Height;
        }

        #region Public Properties

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (autoSize)
            {
                // Calculate the Position for children controls
                if (orientation == Orientation.Horizontal)
                {
                    Height = FitSize.Height;
                }
                else
                {
                    Width = FitSize.Width;
                }
                //=================================================
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether the height or width of the track bar is being automatically sized.
        /// </summary>
        /// <remarks>You can set the AutoSize property to true to cause the track bar to adjust either its height or width, depending on orientation, to ensure that the control uses only the required amount of space.</remarks>
        /// <value>true if the track bar is being automatically sized; otherwise, false. The default is true.</value>
        [Category("Behavior")]
        [Description("Gets or sets the height of track line.")]
        [DefaultValue(true)]
        public override bool AutoSize
        {
            get { return autoSize; }

            set
            {
                if (autoSize != value)
                {
                    autoSize = value;
                    if (autoSize)
                        Size = FitSize;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value to be added to or subtracted from the <see cref="Value"/> property when the slider is moved a large distance.
        /// </summary>
        /// <remarks>
        /// When the user presses the PAGE UP or PAGE DOWN key or clicks the track bar on either side of the slider, the <see cref="Value"/> 
        /// property changes according to the value set in the <see cref="LargeChange"/> property. 
        /// You might consider setting the <see cref="LargeChange"/> value to a percentage of the <see cref="Control.Height"/> (for a vertically oriented track bar) or 
        /// <see cref="Control.Width"/> (for a horizontally oriented track bar) values. This keeps the distance your track bar moves proportionate to its size.
        /// </remarks>
        /// <value>A numeric value. The default value is 2.</value>
        [Category("Behavior")]
        [Description("Gets or sets a value to be added to or subtracted from the Value property when the slider is moved a large distance.")]
        [DefaultValue(2)]
        public int LargeChange
        {
            get { return largeChange; }

            set
            {
                largeChange = value;
                if (largeChange < 1)
                    largeChange = 1;
            }
        }

        /// <summary>
        /// Gets or sets a value to be added to or subtracted from the <see cref="Value"/> property when the slider is moved a small distance.
        /// </summary>
        /// <remarks>
        /// When the user presses one of the arrow keys, the <see cref="Value"/> property changes according to the value set in the SmallChange property.
        /// You might consider setting the <see cref="SmallChange"/> value to a percentage of the <see cref="Control.Height"/> (for a vertically oriented track bar) or 
        /// <see cref="Control.Width"/> (for a horizontally oriented track bar) values. This keeps the distance your track bar moves proportionate to its size.
        /// </remarks>
        /// <value>A numeric value. The default value is 1.</value>
        [Category("Behavior")]
        [Description("Gets or sets a value to be added to or subtracted from the Value property when the slider is moved a small distance.")]
        [DefaultValue(1)]
        public int SmallChange
        {
            get { return smallChange; }

            set
            {
                smallChange = value;
                if (smallChange < 1)
                    smallChange = 1;
            }
        }

        /// <summary>
        /// Gets or sets the height of track line.
        /// </summary>
        /// <value>The default value is 4.</value>
        [Category("Appearance")]
        [Description("Gets or sets the height of track line.")]
        [DefaultValue(4)]
        public int TrackLineHeight
        {
            get { return trackLineHeight; }

            set
            {
                if (trackLineHeight != value)
                {
                    trackLineHeight = value;
                    if (trackLineHeight < 1)
                        trackLineHeight = 1;

                    if (trackLineHeight > trackerSize.Height)
                        trackLineHeight = trackerSize.Height;

                    Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets the tick's <see cref="Color"/> of the control.
        /// </summary>
        [Category("Appearance")]
        [Description("Gets or sets the tick's color of the control.")]
        public Color TickColor
        {
            get { return tickColor; }

            set
            {
                if (tickColor != value)
                {
                    tickColor = value;
                    Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets a value that specifies the delta between ticks drawn on the control.
        /// </summary>
        /// <remarks>
        /// For a <see cref="TrackBarEx"/> with a large range of values between the <see cref="Minimum"/> and the 
        /// <see cref="Maximum"/>, it might be impractical to draw all the ticks for values on the control. 
        /// For example, if you have a control with a range of 100, passing in a value of 
        /// five here causes the control to draw 20 ticks. In this case, each tick 
        /// represents five units in the range of values.
        /// </remarks>
        /// <value>The numeric value representing the delta between ticks. The default is 1.</value>
        [Category("Appearance")]
        [Description("Gets or sets a value that specifies the delta between ticks drawn on the control.")]
        [DefaultValue(1)]
        public int TickFrequency
        {
            get { return tickFrequency; }

            set
            {
                if (tickFrequency != value)
                {
                    tickFrequency = value;
                    if (tickFrequency < 1)
                        tickFrequency = 1;
                    Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets the height of tick.
        /// </summary>
        /// <value>The height of tick in pixels. The default value is 2.</value>
        [Category("Appearance")]
        [Description("Gets or sets the height of tick.")]
        [DefaultValue(6)]
        public int TickHeight
        {
            get { return tickHeight; }

            set
            {
                if (tickHeight != value)
                {
                    tickHeight = value;

                    if (tickHeight < 1)
                        tickHeight = 1;

                    if (autoSize)
                        Size = FitSize;

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the height of indent (or Padding-Y).
        /// </summary>
        /// <value>The height of indent in pixels. The default value is 6.</value>
        [Category("Appearance")]
        [Description("Gets or sets the height of indent.")]
        [DefaultValue(2)]
        public int IndentHeight
        {
            get { return indentHeight; }

            set
            {
                if (indentHeight != value)
                {
                    indentHeight = value;
                    if (indentHeight < 0)
                        indentHeight = 0;

                    if (autoSize)
                        Size = FitSize;

                    Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets the width of indent (or Padding-Y).
        /// </summary>
        /// <value>The width of indent in pixels. The default value is 6.</value>
        [Category("Appearance")]
        [Description("Gets or sets the width of indent.")]
        [DefaultValue(6)]
        public int IndentWidth
        {
            get { return indentWidth; }

            set
            {
                if (indentWidth != value)
                {
                    indentWidth = value;
                    if (indentWidth < 0)
                        indentWidth = 0;

                    if (autoSize)
                        Size = FitSize;

                    Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets the tracker's size. 
        /// The tracker's width must be greater or equal to tracker's height.
        /// </summary>
        /// <value>The <see cref="Size"/> object that represents the height and width of the tracker in pixels.</value>
        [Category("Appearance")]
        [Description("Gets or sets the tracker's size.")]
        public Size TrackerSize
        {
            get { return trackerSize; }

            set
            {
                if (trackerSize != value)
                {
                    trackerSize = value;
                    if (trackerSize.Width > trackerSize.Height)
                        trackerSize.Height = trackerSize.Width;

                    if (autoSize)
                        Size = FitSize;

                    Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets the text tick style of the trackbar.
        /// There are 4 styles for selection: None, TopLeft, BottomRight, Both. 
        /// </summary>
        /// <remarks>You can use the <see cref="Control.Font"/>, <see cref="Control.ForeColor"/>
        /// properties to customize the tick text.</remarks>
        /// <value>One of the <see cref="TickStyle"/> values. The default is <b>BottomRight</b>.</value>
        [Category("Appearance")]
        [Description("Gets or sets the text tick style.")]
        [DefaultValue(TickStyle.BottomRight)]
        public TickStyle TextTickStyle
        {
            get { return textTickStyle; }

            set
            {
                if (textTickStyle != value)
                {
                    textTickStyle = value;

                    if (autoSize)
                        Size = FitSize;

                    Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets the tick style of the trackbar.
        /// There are 4 styles for selection: None, TopLeft, BottomRight, Both. 
        /// </summary>
        /// <remarks>You can use the <see cref="TickColor"/>, <see cref="TickFrequency"/>, 
        /// <see cref="TickHeight"/> properties to customize the trackbar's ticks.</remarks>
        /// <value>One of the <see cref="TickStyle"/> values. The default is <b>BottomRight</b>.</value>
        [Category("Appearance")]
        [Description("Gets or sets the tick style.")]
        [DefaultValue(TickStyle.BottomRight)]
        public TickStyle TickStyle
        {
            get { return tickStyle; }

            set
            {
                if (tickStyle != value)
                {
                    tickStyle = value;

                    if (autoSize)
                        Size = FitSize;

                    Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or set tracker's color.
        /// </summary>
        /// <value>
        /// <remarks>You can change size of tracker by <see cref="TrackerSize"/> property.</remarks>
        /// A <see cref="Color"/> that represents the color of the tracker. 
        /// </value>
        [Description("Gets or set tracker's color.")]
        [Category("Appearance")]
        public Color TrackerColor
        {
            get
            {
                return trackerColor;
            }
            set
            {
                if (trackerColor != value)
                {
                    trackerColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets a numeric value that represents the current position of the slider on the track bar.
        /// </summary>
        /// <remarks>The Value property contains the number that represents the current position of the slider on the track bar.</remarks>
        /// <value>A numeric value that is within the <see cref="Minimum"/> and <see cref="Maximum"/> range. 
        /// The default value is 0.</value>
        [Description("The current value for the TrackBar, in the range specified by the Minimum and Maximum properties.")]
        [Category("Behavior")]
        public int Value
        {
            get
            {
                return value;
            }
            set
            {
                if (this.value != value)
                {
                    if (value < minimum)
                        this.value = minimum;
                    else
                        if (value > maximum)
                            this.value = maximum;
                        else
                            this.value = value;

                    OnValueChanged(this.value);

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the lower limit of the range this <see cref="TrackBarEx"/> is working with.
        /// </summary>
        /// <remarks>You can use the <see cref="SetRange"/> method to set both the <see cref="Maximum"/> and <see cref="Minimum"/> properties at the same time.</remarks>
        /// <value>The minimum value for the <see cref="TrackBarEx"/>. The default value is 0.</value>
        [Description("The lower bound of the range this TrackBar is working with.")]
        [Category("Behavior")]
        public int Minimum
        {
            get
            {
                return minimum;
            }
            set
            {
                minimum = value;

                if (minimum > maximum)
                    maximum = minimum;
                if (minimum > this.value)
                    this.value = minimum;

                if (autoSize)
                    Size = FitSize;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the upper limit of the range this <see cref="TrackBarEx"/> is working with.
        /// </summary>
        /// <remarks>You can use the <see cref="SetRange"/> method to set both the <see cref="Maximum"/> and <see cref="Minimum"/> properties at the same time.</remarks>
        /// <value>The maximum value for the <see cref="TrackBarEx"/>. The default value is 10.</value>
        [Description("The uppper bound of the range this TrackBar is working with.")]
        [Category("Behavior")]
        public int Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                maximum = value;

                if (maximum < this.value)
                    this.value = maximum;
                if (maximum < minimum)
                    minimum = maximum;

                if (autoSize)
                    Size = FitSize;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the horizontal or vertical orientation of the track bar.
        /// </summary>
        /// <remarks>
        /// When the <b>Orientation</b> property is set to <b>Orientation.Horizontal</b>, 
        /// the slider moves from left to right as the <see cref="Value"/> increases. 
        /// When the <b>Orientation</b> property is set to <b>Orientation.Vertical</b>, the slider moves 
        /// from bottom to top as the <see cref="Value"/> increases.
        /// </remarks>
        /// <value>One of the <see cref="Orientation"/> values. The default value is <b>Horizontal</b>.</value>
        [Description("Gets or sets a value indicating the horizontal or vertical orientation of the track bar.")]
        [Category("Behavior")]
        [DefaultValue(Orientation.Horizontal)]
        public Orientation Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                if (value != orientation)
                {
                    orientation = value;
                    if (orientation == Orientation.Horizontal)
                    {
                        if (Width < Height)
                        {
                            var temp = Width;
                            Width = Height;
                            Height = temp;
                        }
                    }
                    else //Vertical 
                    {
                        if (Width > Height)
                        {
                            var temp = Width;
                            Width = Height;
                            Height = temp;
                        }
                    }
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the border type of the trackbar control.
        /// </summary>
        /// <value>A <see cref="MACBorderStyle"/> that represents the border type of the trackbar control. 
        /// The default is <b>MACBorderStyle.None</b>.</value>
        [Description("Gets or sets the border type of the trackbar control.")]
        [Category("Appearance"), DefaultValue(typeof(MACBorderStyle), "None")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public MACBorderStyle BorderStyle
        {
            get
            {
                return borderStyle;
            }
            set
            {
                if (borderStyle != value)
                {
                    borderStyle = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the border color of the control.
        /// </summary>
        /// <value>A <see cref="Color"/> object that represents the border color of the control.</value>
        [Category("Appearance")]
        [Description("Gets or sets the border color of the control.")]
        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                if (value != borderColor)
                {
                    borderColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the track line.
        /// </summary>
        /// <value>A <see cref="Color"/> object that represents the color of the track line.</value>
        [Category("Appearance")]
        [Description("Gets or sets the color of the track line.")]
        public Color TrackLineColor
        {
            get { return trackLineColor; }
            set
            {
                if (value != trackLineColor)
                {
                    trackLineColor = value;
                    Invalidate();
                }
            }
        }


        #endregion

        #region Private Properties

        /// <summary>
        /// Gets the Size of area need for drawing.
        /// </summary>
        [Description("Gets the Size of area need for drawing.")]
        [Browsable(false)]
        private Size FitSize
        {
            get
            {
                Size fitSize;
                float textAreaSize;

                // Create a Graphics object for the Control.
                var g = CreateGraphics();

                float currentUsedPos;

                if (orientation == Orientation.Horizontal)
                {
                    currentUsedPos = indentHeight;
                    //==========================================================================

                    // Get Height of Text Area
                    textAreaSize = g.MeasureString(maximum.ToString(CultureInfo.InvariantCulture), Font).Height;

                    if (textTickStyle == TickStyle.TopLeft || textTickStyle == TickStyle.Both)
                        currentUsedPos += textAreaSize;

                    if (tickStyle == TickStyle.TopLeft || tickStyle == TickStyle.Both)
                        currentUsedPos += tickHeight + 1;

                    currentUsedPos += trackerSize.Height;

                    if (tickStyle == TickStyle.BottomRight || tickStyle == TickStyle.Both)
                    {
                        currentUsedPos += 1;
                        currentUsedPos += tickHeight;
                    }

                    if (textTickStyle == TickStyle.BottomRight || textTickStyle == TickStyle.Both)
                        currentUsedPos += textAreaSize;

                    currentUsedPos += indentHeight;

                    fitSize = new Size(ClientRectangle.Width, (int)currentUsedPos);
                }
                else //orientation == Orientation.Vertical
                {
                    currentUsedPos = indentWidth;
                    //==========================================================================

                    // Get Width of Text Area
                    textAreaSize = g.MeasureString(maximum.ToString(CultureInfo.InvariantCulture), Font).Width;

                    if (textTickStyle == TickStyle.TopLeft || textTickStyle == TickStyle.Both)
                        currentUsedPos += textAreaSize;

                    if (tickStyle == TickStyle.TopLeft || tickStyle == TickStyle.Both)
                        currentUsedPos += tickHeight + 1;

                    currentUsedPos += trackerSize.Height;

                    if (tickStyle == TickStyle.BottomRight || tickStyle == TickStyle.Both)
                    {
                        currentUsedPos += 1;
                        currentUsedPos += tickHeight;
                    }

                    if (textTickStyle == TickStyle.BottomRight || textTickStyle == TickStyle.Both)
                        currentUsedPos += textAreaSize;

                    currentUsedPos += indentWidth;

                    fitSize = new Size((int)currentUsedPos, ClientRectangle.Height);

                }

                // Clean up the Graphics object.
                g.Dispose();

                return fitSize;
            }
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="value">The new value</param>
        public virtual void OnValueChanged(int value)
        {
            // Any attached event handlers?
            if (ValueChanged != null)
                ValueChanged(this, value);

        }

        /// <summary>
        /// Raises the Scroll event.
        /// </summary>
        public virtual void OnScroll()
        {
            // Any attached event handlers?
            if (Scroll != null)
                Scroll(this, new EventArgs());
        }


        /// <summary>
        /// Call the Increment() method to increase the value displayed by an integer you specify 
        /// </summary>
        /// <param name="value"></param>
        public void Increment(int value)
        {
            if (this.value < maximum)
            {
                this.value += value;
                if (this.value > maximum)
                    this.value = maximum;
            }
            else
                this.value = maximum;

            OnValueChanged(this.value);
            Invalidate();
        }

        /// <summary>
        /// Call the Decrement() method to decrease the value displayed by an integer you specify 
        /// </summary>
        /// <param name="value"> The value to decrement</param>
        public void Decrement(int value)
        {
            if (this.value > minimum)
            {
                this.value -= value;
                if (this.value < minimum)
                    this.value = minimum;
            }
            else
                this.value = minimum;

            OnValueChanged(this.value);
            Invalidate();
        }

        /// <summary>
        /// Sets the minimum and maximum values for a TrackBar.
        /// </summary>
        /// <param name="minValue">The lower limit of the range of the track bar.</param>
        /// <param name="maxValue">The upper limit of the range of the track bar.</param>
        public void SetRange(int minValue, int maxValue)
        {
            minimum = minValue;

            if (minimum > value)
                value = minimum;

            maximum = maxValue;

            if (maximum < value)
                value = maximum;
            if (maximum < minimum)
                minimum = maximum;

            Invalidate();
        }

        /// <summary>
        /// Reset the appearance properties.
        /// </summary>
        public void ResetAppearance()
        {
            Font = new Font("Verdana", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ForeColor = Color.FromArgb(123, 125, 123);
            BackColor = Color.Transparent;

            tickColor = Color.FromArgb(148, 146, 148);
            tickHeight = 4;

            trackerColor = Color.FromArgb(24, 130, 198);
            trackerSize = new Size(16, 16);
            //TrackerRect.Size = trackerSize;

            indentWidth = 6;
            indentHeight = 6;

            trackLineColor = Color.FromArgb(90, 93, 90);
            trackLineHeight = 3;

            borderStyle = MACBorderStyle.None;
            borderColor = SystemColors.ActiveBorder;

            //==========================================================================

            if (autoSize)
                Size = FitSize;
            Invalidate();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// The OnCreateControl method is called when the control is first created.
        /// </summary>
        protected override void OnCreateControl()
        {
        }
        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }
        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }
        protected override void OnClick(EventArgs e)
        {
            Focus();
            Invalidate();
            base.OnClick(e);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var blResult = true;

            // Specified WM_KEYDOWN enumeration value.
            const int WM_KEYDOWN = 0x0100;

            // Specified WM_SYSKEYDOWN enumeration value.
            const int WM_SYSKEYDOWN = 0x0104;


            if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
            {
                switch (keyData)
                {
                    case Keys.Left:
                    case Keys.Down:
                        Decrement(smallChange);
                        break;
                    case Keys.Right:
                    case Keys.Up:
                        Increment(smallChange);
                        break;

                    case Keys.PageUp:
                        Increment(largeChange);
                        break;
                    case Keys.PageDown:
                        Decrement(largeChange);
                        break;

                    case Keys.Home:
                        Value = maximum;
                        break;
                    case Keys.End:
                        Value = minimum;
                        break;

                    default:
                        blResult = base.ProcessCmdKey(ref msg, keyData);
                        break;
                }
            }

            return blResult;
        }

        #endregion

        #region Painting Methods

        protected override void OnPaint(PaintEventArgs e)
        {
            RectangleF drawRect;
            float textAreaSize;

            var workingRect = Rectangle.Inflate(ClientRectangle, -indentWidth, -indentHeight);
            float currentUsedPos;

            //==========================================================================
            // Draw the background of the ProgressBar control.
            //==========================================================================
            Brush brush = new SolidBrush(BackColor);
            RectangleF rectTemp = ClientRectangle;
            e.Graphics.FillRectangle(brush, rectTemp);
            brush.Dispose();
            //==========================================================================

            //==========================================================================
            if (orientation == Orientation.Horizontal)
            {
                currentUsedPos = indentHeight;
                //==========================================================================

                // Get Height of Text Area
                textAreaSize = e.Graphics.MeasureString(maximum.ToString(CultureInfo.InvariantCulture), Font).Height;

                if (textTickStyle == TickStyle.TopLeft || textTickStyle == TickStyle.Both)
                {
                    //==========================================================================
                    // Draw the 1st Text Line.
                    //==========================================================================
                    drawRect = new RectangleF(workingRect.Left, currentUsedPos, workingRect.Width, textAreaSize);
                    drawRect.Inflate(-trackerSize.Width * 0.5f, 0);
                    currentUsedPos += textAreaSize;

                    DrawTickTextLine(e.Graphics, drawRect, tickFrequency, minimum, maximum, ForeColor, Font);
                    //==========================================================================
                }

                if (tickStyle == TickStyle.TopLeft || tickStyle == TickStyle.Both)
                {
                    //==========================================================================
                    // Draw the 1st Tick Line.
                    //==========================================================================
                    drawRect = new RectangleF(workingRect.Left, currentUsedPos, workingRect.Width, tickHeight);
                    drawRect.Inflate(-trackerSize.Width * 0.5f, 0);
                    currentUsedPos += tickHeight + 1;

                    DrawTickLine(e.Graphics, drawRect, tickFrequency, minimum, maximum, tickColor);
                    //==========================================================================
                }

                //==========================================================================
                // Caculate the Tracker's rectangle
                //==========================================================================
                float currentTrackerPos;
                if (maximum == minimum)
                    currentTrackerPos = workingRect.Left;
                else
                    currentTrackerPos = (workingRect.Width - trackerSize.Width) * (value - minimum) / (maximum - minimum) + workingRect.Left;
                TrackerRect = new RectangleF(currentTrackerPos, currentUsedPos, trackerSize.Width, trackerSize.Height);// Remember this for drawing the Tracker later
                //TrackerRect.Inflate(0,-1);

                //==========================================================================
                // Draw the Track Line
                //==========================================================================
                drawRect = new RectangleF(workingRect.Left, currentUsedPos + trackerSize.Height * 0.5f - trackLineHeight * 0.5f, workingRect.Width, trackLineHeight);
                DrawTrackLine(e.Graphics, drawRect);
                currentUsedPos += trackerSize.Height;


                //==========================================================================

                if (tickStyle == TickStyle.BottomRight || tickStyle == TickStyle.Both)
                {
                    //==========================================================================
                    // Draw the 2st Tick Line.
                    //==========================================================================
                    currentUsedPos += 1;
                    drawRect = new RectangleF(workingRect.Left, currentUsedPos, workingRect.Width, tickHeight);
                    drawRect.Inflate(-trackerSize.Width * 0.5f, 0);
                    currentUsedPos += tickHeight;

                    DrawTickLine(e.Graphics, drawRect, tickFrequency, minimum, maximum, tickColor);
                    //==========================================================================
                }

                if (textTickStyle == TickStyle.BottomRight || textTickStyle == TickStyle.Both)
                {
                    //==========================================================================
                    // Draw the 2st Text Line.
                    //==========================================================================
                    // Get Height of Text Area
                    drawRect = new RectangleF(workingRect.Left, currentUsedPos, workingRect.Width, textAreaSize);
                    drawRect.Inflate(-trackerSize.Width * 0.5f, 0);

                    DrawTickTextLine(e.Graphics, drawRect, tickFrequency, minimum, maximum, ForeColor, Font);
                    //==========================================================================
                }
            }
            else //orientation == Orientation.Vertical
            {
                currentUsedPos = indentWidth;
                //==========================================================================

                // Get Width of Text Area
                textAreaSize = e.Graphics.MeasureString(maximum.ToString(CultureInfo.InvariantCulture), Font).Width;

                if (textTickStyle == TickStyle.TopLeft || textTickStyle == TickStyle.Both)
                {
                    //==========================================================================
                    // Draw the 1st Text Line.
                    //==========================================================================
                    // Get Height of Text Area
                    drawRect = new RectangleF(currentUsedPos, workingRect.Top, textAreaSize, workingRect.Height);
                    drawRect.Inflate(0, -trackerSize.Width * 0.5f);
                    currentUsedPos += textAreaSize;

                    DrawTickTextLine(e.Graphics, drawRect, tickFrequency, minimum, maximum, ForeColor, Font);
                    //==========================================================================
                }

                if (tickStyle == TickStyle.TopLeft || tickStyle == TickStyle.Both)
                {
                    //==========================================================================
                    // Draw the 1st Tick Line.
                    //==========================================================================
                    drawRect = new RectangleF(currentUsedPos, workingRect.Top, tickHeight, workingRect.Height);
                    drawRect.Inflate(0, -trackerSize.Width / 22f);
                    currentUsedPos += tickHeight + 1;

                    DrawTickLine(e.Graphics, drawRect, tickFrequency, minimum, maximum, tickColor);
                    //==========================================================================
                }

                //==========================================================================
                // Caculate the Tracker's rectangle
                //==========================================================================
                float currentTrackerPos;
                if (maximum == minimum)
                    currentTrackerPos = workingRect.Top;
                else
                    currentTrackerPos = (workingRect.Height - trackerSize.Width) * (value - minimum) / (float)(maximum - minimum);

                TrackerRect = new RectangleF(currentUsedPos, workingRect.Bottom - currentTrackerPos - trackerSize.Width, trackerSize.Height, trackerSize.Width);// Remember this for drawing the Tracker later

                //==========================================================================
                // Draw the Track Line
                //==========================================================================
                drawRect = new RectangleF(currentUsedPos + trackerSize.Height * 0.5f - trackLineHeight * 0.5f, workingRect.Top, trackLineHeight, workingRect.Height);
                DrawTrackLine(e.Graphics, drawRect);
                currentUsedPos += trackerSize.Height;
                //==========================================================================

                if (tickStyle == TickStyle.BottomRight || tickStyle == TickStyle.Both)
                {
                    //==========================================================================
                    // Draw the 2st Tick Line.
                    //==========================================================================
                    currentUsedPos += 1;
                    drawRect = new RectangleF(currentUsedPos, workingRect.Top, tickHeight, workingRect.Height);
                    drawRect.Inflate(0, -trackerSize.Width * 0.5f);
                    currentUsedPos += tickHeight;

                    DrawTickLine(e.Graphics, drawRect, tickFrequency, minimum, maximum, tickColor);
                    //==========================================================================
                }

                if (textTickStyle == TickStyle.BottomRight || textTickStyle == TickStyle.Both)
                {
                    //==========================================================================
                    // Draw the 2st Text Line.
                    //==========================================================================
                    // Get Height of Text Area
                    drawRect = new RectangleF(currentUsedPos, workingRect.Top, textAreaSize, workingRect.Height);
                    drawRect.Inflate(0, -trackerSize.Width * 0.5f);

                    DrawTickTextLine(e.Graphics, drawRect, tickFrequency, minimum, maximum, ForeColor, Font);
                    //==========================================================================
                }
            }

            //==========================================================================
            // Check for special values of Max, Min & Value
            if (maximum == minimum)
            {
                // Draw border only and exit;
                DrawBorder(e.Graphics);
                return;
            }
            //==========================================================================

            //==========================================================================
            // Draw the Tracker
            //==========================================================================
            DrawTracker(e.Graphics, TrackerRect);
            //==========================================================================

            // Draw border
            DrawBorder(e.Graphics);
            //==========================================================================

            // Draws a focus rectangle
            //if(this.Focused && this.BackColor != Color.Transparent)
            if (Focused)
                ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -2, -2));
            //==========================================================================
        }

        private void DrawTrackLine(Graphics g, RectangleF drawRect)
        {
            TrackBarDrawingHelper.DrawAquaPillSingleLayer(g, drawRect, trackLineColor, orientation);
        }

        private void DrawTracker(Graphics g, RectangleF trackerRect)
        {
            TrackBarDrawingHelper.DrawAquaPill(g, trackerRect, trackerColor, orientation);
        }

        private void DrawTickTextLine(Graphics g, RectangleF drawRect, int tickFrequency, int minimum, int maximum, Color foreColor, Font font)
        {

            //Check input value
            if (maximum == minimum)
                return;

            //Caculate tick number
            var tickCount = (maximum - minimum) / tickFrequency;
            if ((maximum - minimum) % tickFrequency == 0)
                tickCount -= 1;

            //Prepare for drawing Text
            //===============================================================
            var stringFormat = new StringFormat {
                FormatFlags = StringFormatFlags.NoWrap,
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                HotkeyPrefix = HotkeyPrefix.Show
            };

            Brush brush = new SolidBrush(foreColor);
            string text;
            float tickFrequencySize;
            //===============================================================

            if (this.orientation == Orientation.Horizontal)
            {
                // Calculate tick's setting
                tickFrequencySize = drawRect.Width * tickFrequency / (maximum - minimum);

                //===============================================================

                // Draw each tick text
                for (var i = 0; i <= tickCount; i++)
                {
                    text = Convert.ToString(this.minimum + tickFrequency * i, 10);
                    g.DrawString(text, font, brush, drawRect.Left + tickFrequencySize * i, drawRect.Top + drawRect.Height * 0.5f, stringFormat);

                }
                // Draw last tick text at Maximum
                text = Convert.ToString(this.maximum, 10);
                g.DrawString(text, font, brush, drawRect.Right, drawRect.Top + drawRect.Height * 0.5f, stringFormat);

                //===============================================================
            }
            else //Orientation.Vertical
            {
                // Calculate tick's setting
                tickFrequencySize = drawRect.Height * tickFrequency / (maximum - minimum);
                //===============================================================

                // Draw each tick text
                for (var i = 0; i <= tickCount; i++)
                {
                    text = Convert.ToString(this.minimum + tickFrequency * i, 10);
                    g.DrawString(text, font, brush, drawRect.Left + drawRect.Width * 0.5f, drawRect.Bottom - tickFrequencySize * i, stringFormat);
                }
                // Draw last tick text at Maximum
                text = Convert.ToString(this.maximum, 10);
                g.DrawString(text, font, brush, drawRect.Left + drawRect.Width * 0.5f, drawRect.Top, stringFormat);
                //===============================================================

            }
        }

        private void DrawTickLine(Graphics g, RectangleF drawRect, int tickFrequency, int minimum, int maximum, Color tickColor)
        {
            //Check input value
            if (maximum == minimum)
                return;

            //Create the Pen for drawing Ticks
            var pen = new Pen(tickColor, 1);
            float tickFrequencySize;

            //Caculate tick number
            var tickCount = (maximum - minimum) / tickFrequency;
            if ((maximum - minimum) % tickFrequency == 0)
                tickCount -= 1;

            if (orientation == Orientation.Horizontal)
            {
                // Calculate tick's setting
                tickFrequencySize = drawRect.Width * tickFrequency / (maximum - minimum);

                //===============================================================

                // Draw each tick
                for (var i = 0; i <= tickCount; i++)
                {
                    g.DrawLine(pen, drawRect.Left + tickFrequencySize * i, drawRect.Top, drawRect.Left + tickFrequencySize * i, drawRect.Bottom);
                }
                // Draw last tick at Maximum
                g.DrawLine(pen, drawRect.Right, drawRect.Top, drawRect.Right, drawRect.Bottom);
                //===============================================================
            }
            else //Orientation.Vertical
            {
                // Calculate tick's setting
                tickFrequencySize = drawRect.Height * tickFrequency / (maximum - minimum);
                //===============================================================

                // Draw each tick
                for (var i = 0; i <= tickCount; i++)
                {
                    g.DrawLine(pen, drawRect.Left, drawRect.Bottom - tickFrequencySize * i, drawRect.Right, drawRect.Bottom - tickFrequencySize * i);
                }
                // Draw last tick at Maximum
                g.DrawLine(pen, drawRect.Left, drawRect.Top, drawRect.Right, drawRect.Top);
                //===============================================================
            }
        }

        private void DrawBorder(Graphics g)
        {

            switch (borderStyle)
            {
                case MACBorderStyle.Dashed: //from ButtonBorderStyle Enumeration
                    ControlPaint.DrawBorder(g, ClientRectangle, borderColor, ButtonBorderStyle.Dashed);
                    break;
                case MACBorderStyle.Dotted: //from ButtonBorderStyle Enumeration
                    ControlPaint.DrawBorder(g, ClientRectangle, borderColor, ButtonBorderStyle.Dotted);
                    break;
                case MACBorderStyle.Inset: //from ButtonBorderStyle Enumeration
                    ControlPaint.DrawBorder(g, ClientRectangle, borderColor, ButtonBorderStyle.Inset);
                    break;
                case MACBorderStyle.Outset: //from ButtonBorderStyle Enumeration
                    ControlPaint.DrawBorder(g, ClientRectangle, borderColor, ButtonBorderStyle.Outset);
                    break;
                case MACBorderStyle.Solid: //from ButtonBorderStyle Enumeration
                    ControlPaint.DrawBorder(g, ClientRectangle, borderColor, ButtonBorderStyle.Solid);
                    break;

                case MACBorderStyle.Adjust: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, ClientRectangle, Border3DStyle.Adjust);
                    break;
                case MACBorderStyle.Bump: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, ClientRectangle, Border3DStyle.Bump);
                    break;
                case MACBorderStyle.Etched: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, ClientRectangle, Border3DStyle.Etched);
                    break;
                case MACBorderStyle.Flat: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, ClientRectangle, Border3DStyle.Flat);
                    break;
                case MACBorderStyle.Raised: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, ClientRectangle, Border3DStyle.Raised);
                    break;
                case MACBorderStyle.RaisedInner: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, ClientRectangle, Border3DStyle.RaisedInner);
                    break;
                case MACBorderStyle.RaisedOuter: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, ClientRectangle, Border3DStyle.RaisedOuter);
                    break;
                case MACBorderStyle.Sunken: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, ClientRectangle, Border3DStyle.Sunken);
                    break;
                case MACBorderStyle.SunkenInner: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, ClientRectangle, Border3DStyle.SunkenInner);
                    break;
                case MACBorderStyle.SunkenOuter: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, ClientRectangle, Border3DStyle.SunkenOuter);
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void OnMouseDownSlider(object sender, MouseEventArgs e)
        {
            var offsetValue = 0;

            var currentPoint = new PointF(e.X, e.Y);
            if (TrackerRect.Contains(currentPoint))
            {
                if (!leftButtonDown)
                {
                    leftButtonDown = true;
                    Capture = true;
                    switch (orientation)
                    {
                        case Orientation.Horizontal:
                            mouseStartPos = currentPoint.X - TrackerRect.X;
                            break;

                        case Orientation.Vertical:
                            mouseStartPos = currentPoint.Y - TrackerRect.Y;
                            break;
                    }
                }
            }
            else
            {
                switch (orientation)
                {
                    case Orientation.Horizontal:
                        if (currentPoint.X + trackerSize.Width * 0.5f >= Width - indentWidth)
                            offsetValue = maximum - minimum;
                        else if (currentPoint.X - trackerSize.Width * 0.5f <= indentWidth)
                            offsetValue = 0;
                        else
                            offsetValue = (int)(((currentPoint.X - indentWidth - trackerSize.Width * 0.5f) * (maximum - minimum)) / (Width - 2 * indentWidth - trackerSize.Width) + 0.5);

                        break;

                    case Orientation.Vertical:
                        if (currentPoint.Y + trackerSize.Width * 0.5f >= Height - indentHeight)
                            offsetValue = 0;
                        else if (currentPoint.Y - trackerSize.Width * 0.5f <= indentHeight)
                            offsetValue = maximum - minimum;
                        else
                            offsetValue = (int)(((Height - currentPoint.Y - indentHeight - trackerSize.Width * 0.5f) * (maximum - minimum)) / (Height - 2 * indentHeight - trackerSize.Width) + 0.5);

                        break;
                }

                var oldValue = value;
                value = minimum + offsetValue;
                Invalidate();

                if (oldValue != value)
                {
                    OnScroll();
                    OnValueChanged(value);
                }
            }

        }
        private void OnMouseUpSlider(object sender, MouseEventArgs e)
        {
            leftButtonDown = false;
            Capture = false;

        }
        private void OnMouseMoveSlider(object sender, MouseEventArgs e)
        {
            var offsetValue = 0;

            var currentPoint = new PointF(e.X, e.Y);

            if (leftButtonDown)
            {
                try
                {
                    switch (orientation)
                    {
                        case Orientation.Horizontal:
                            if ((currentPoint.X + trackerSize.Width - mouseStartPos) >= Width - indentWidth)
                                offsetValue = maximum - minimum;
                            else if (currentPoint.X - mouseStartPos <= indentWidth)
                                offsetValue = 0;
                            else
                                offsetValue = (int)(((currentPoint.X - mouseStartPos - indentWidth) * (maximum - minimum)) / (Width - 2 * indentWidth - trackerSize.Width) + 0.5);

                            break;

                        case Orientation.Vertical:
                            if (currentPoint.Y + trackerSize.Width * 0.5f >= Height - indentHeight)
                                offsetValue = 0;
                            else if (currentPoint.Y + trackerSize.Width * 0.5f <= indentHeight)
                                offsetValue = maximum - minimum;
                            else
                                offsetValue = (int)(((Height - currentPoint.Y + trackerSize.Width * 0.5f - mouseStartPos - indentHeight) * (maximum - minimum)) / (Height - 2 * indentHeight) + 0.5);

                            break;
                    }

                }
                finally
                {
                    var oldValue = value;
                    Value = minimum + offsetValue;
                    Invalidate();

                    if (oldValue != value)
                    {
                        OnScroll();
                        OnValueChanged(value);
                    }
                }
            }

        }

        #endregion

    }
}