using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace cocosUiEditor
{
    [Serializable()]
    public enum NodeKind
    {
        Sprite = 0,
        Menu,
        Button,
        Label,
        BMFLabel,
        LoadingBar,
        CheckBox,
        Slider,
        Scale9Sprite,
        TextField
    }
    [Serializable()]
    public enum PosKind
    {
        Vec2,
        Offset,
        Docking
    }
    [Serializable()]
    public enum RSC
    {
        Local,
        PLIST
    }
    [Serializable()]
    public enum Direction
    {
        LEFT,
        RIGHT
        //Horizoontal,
        //Vertical
    }
    //[Serializable()]
    //[TypeConverterAttribute(typeof(System.ComponentModel.ExpandableObjectConverter))]
    //public class Anchor
    //{
    //    private float _X = 0;
    //    private float _Y = 0;
    //    public Anchor() { }
    //    public Anchor(float x, float y)
    //    {
    //        _X = x;
    //        _Y = y;
    //    }
    //    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    //    public Anchor anc
    //    {
    //        get { return this; }
    //        set {
    //            this._X = value.X;
    //            this._Y = value.Y;
    //        }
    //    }
    //    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    //    public float X
    //    {
    //        get { return _X; }
    //        set { this._X = value; }
    //    }
    //    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    //    public float Y
    //    {
    //        get { return _Y; }
    //        set { this._Y = value; }
    //    }
    //    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    //    public override string ToString()
    //    {
    //        return string.Format("{0:F2}, {1:F2}", _X, _Y);
    //    }
    //}
    //[Serializable()]
    //[TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
    ////[TypeConverter(typeof(NodeRectTypeConverter))]
    //public class NodeRect
    //{
    //    public float left = 0;
    //    public float right = 0;
    //    public float top = 0;
    //    public float bottom = 0;
    //    public NodeRect() { }
    //    public NodeRect(float left, float top, float right, float bottom)
    //    {
    //        this.left = left;
    //        this.top = top;
    //        this.right = right;
    //        this.bottom = bottom;
    //    }
    //    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    //    //public float left
    //    //{
    //    //    get { return Left;}
    //    //    set { Left = value; }
    //    //}
    //    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    //    //public float right
    //    //{
    //    //    get { return Right; }
    //    //    set { Right = value; }
    //    //}
    //    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    //    //public float top
    //    //{
    //    //    get { return Top; }
    //    //    set { Top = value; }
    //    //}
    //    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    //    //public float bottom
    //    //{
    //    //    get { return Bottom; }
    //    //    set { Bottom = value; }
    //    //}
    //    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    //    public override string ToString()
    //    {
    //        return string.Format("{0},{1}, {2},{3}", (int)left, (int)top, (int)right, (int)bottom);
    //    }
    //}
    [Serializable()]
    [DefaultPropertyAttribute("Sprite")]
    public class CocosNode
    {
        //private Form1 parentForm = null;
        private string assetPath = "";
        private string safeFilename = "";
        //public string CheckedsafeFilename = "";
        public Bitmap image = null;
        private float X = 0, Y = 0;//절대좌표
        private float relativeX = 0, relativeY = 0;//비례좌표
        private float Left = 0, Right = 0, Top = 0, Bottom = 0;//화면 테두리로부터의 좌표
        private int Kind = 0;
        private int Tag = 0;
        private int Zorder = 0;
        private string Name = "";
        private int Opacity = 255;
        private float Scale = 1.0f;
        private float anchorX = 0.5f, anchorY = 0.5f;
        private Color sColor;
        private int Rotate;
        private bool FlipX, FlipY;
        private RSC RscFrom;
        //private Anchor _anchor = new Anchor(0.5f, 0.5f);

        //Label or Button Title
        private string fontpath;
        private Font labelFont;
        private string content;
        private Color tColor;
        private int fSize;

        //scale9sprite
        private Rectangle CapInsets = new Rectangle(0,0,0,0);
        private Size ContentSize;

        //ImageMenu, Button, Slider Node
        private string pressAsset = "";
        private string disableAsset = "";

        //CheckBox
        private string checkedAsset = "";
        public Bitmap checkedAssetImage = null;
        private string checkedDisableAsset = "";
        private bool baseCheck = false;

        //Slider
        private string barAsset = "";
        public Bitmap barImage = null;
        private string ballAsset = "";
        public Bitmap ballImage = null;
        private int barPercent = 0;
        private Direction bardir = Direction.LEFT;
        //Slider's back = image
        //LoadingBar's bar = image

        //Scale9Sprite
        public Bitmap insectImage = null;

        //TextField
        private bool isPassword = false;
        private int maxLength = 50;

        private PosKind _nodepostype;
        private NodeKind _nodekind;

        public CocosNode()
        {
        }

        public CocosNode(string filepath, int kind)
        {
            Kind = kind;//Sprite
            assetPath = filepath;
            if (kind != 3 && kind != 4)
                image = new Bitmap(filepath);
        }

        public CocosNode(string filepath)
        {
            assetPath = filepath;
            image = new Bitmap(filepath);
            ContentSize = image.Size;

            //NodeKind.TextField = false;
        }

        public void renewBitmap(string filepath)
        {
            image.Dispose();
            image = new Bitmap(filepath);
            assetPath = filepath;
            ContentSize = image.Size;
        }
        public void bitmapInsect()
        {
            if (insectImage != null)
                insectImage.Dispose();
            insectImage = new Bitmap(ContentSize.Width, ContentSize.Height);
            Graphics g = Graphics.FromImage(insectImage);

            int innerWidth = ContentSize.Width - CapInsets.X - (image.Width - (CapInsets.X + CapInsets.Width));
            int innerHeight = ContentSize.Height - CapInsets.Y - (image.Height - (CapInsets.Y + CapInsets.Height));

            Rectangle lefttop = new Rectangle(0, 0, CapInsets.X, CapInsets.Y);
            Rectangle righttop = new Rectangle(CapInsets.X + CapInsets.Width, 0, image.Width - (CapInsets.X + CapInsets.Width), CapInsets.Y);
            Rectangle leftbottom = new Rectangle(0, CapInsets.Y + CapInsets.Height, CapInsets.X, image.Height - (CapInsets.Y + CapInsets.Height));
            Rectangle rightbottom = new Rectangle(CapInsets.X + CapInsets.Width, CapInsets.Y + CapInsets.Height, image.Width - (CapInsets.X + CapInsets.Width), image.Height - (CapInsets.Y + CapInsets.Height));

            g.DrawImage(image, new Rectangle(0, 0, lefttop.Width, lefttop.Height), lefttop, GraphicsUnit.Pixel);
            g.DrawImage(image, new Rectangle(lefttop.Width + innerWidth, 0, righttop.Width, righttop.Height), righttop, GraphicsUnit.Pixel);
            g.DrawImage(image, new Rectangle(0, lefttop.Height + innerHeight, leftbottom.Width, leftbottom.Height), leftbottom, GraphicsUnit.Pixel);
            g.DrawImage(image, new Rectangle(lefttop.Width + innerWidth, lefttop.Height + innerHeight, rightbottom.Width, rightbottom.Height), rightbottom, GraphicsUnit.Pixel);

            g.DrawImage(image, new Rectangle(lefttop.Width, 0, innerWidth, lefttop.Height), new Rectangle(lefttop.Width, 0, CapInsets.Width, lefttop.Height), GraphicsUnit.Pixel);
            g.DrawImage(image, new Rectangle(leftbottom.Width, lefttop.Height + innerHeight, innerWidth, leftbottom.Height), new Rectangle(leftbottom.Width, leftbottom.Y, CapInsets.Width, leftbottom.Height), GraphicsUnit.Pixel);
            g.DrawImage(image, new Rectangle(0, lefttop.Height, lefttop.Width, innerHeight), new Rectangle(0, lefttop.Height, lefttop.Width, CapInsets.Height), GraphicsUnit.Pixel);
            g.DrawImage(image, new Rectangle(lefttop.Width + innerWidth, righttop.Height, righttop.Width, innerHeight), new Rectangle(righttop.X, righttop.Height, righttop.Width, CapInsets.Height), GraphicsUnit.Pixel);
            g.DrawImage(image, new Rectangle(lefttop.Width, lefttop.Height, innerWidth, innerHeight), CapInsets, GraphicsUnit.Pixel);

            g.Dispose();
        }
        public void bitmapRotate(int rotate)
        {
            image.Dispose();
            image = new Bitmap(assetPath);//정위치의 이미지를 리셋

            if (rotate % 360 == 0)
                return;

            double width = image.Width;
            double height = image.Height;

            double rad = rotate * Math.PI / 180d;//rotate is degree
            double cos = Math.Abs(Math.Cos(rad));
            double sin = Math.Abs(Math.Sin(rad));

            width = Math.Round(image.Width * cos + image.Height * sin);
            height = Math.Round(image.Width * sin + image.Height * cos);

            Bitmap bmp = new Bitmap((int)width, (int)height);
            bmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);
            gfx.RotateTransform((float)rotate);
            gfx.TranslateTransform(-(float)image.Width / 2, -(float)image.Height / 2);
            //gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            gfx.DrawImage(image, new Point(0,0));
            gfx.Dispose();
            image.Dispose();
            image = bmp;
        }
        public void bitmapFlipX()
        {
            image.RotateFlip(RotateFlipType.RotateNoneFlipX);
        }
        public void bitmapFlipY()
        {
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }
        //public void setParent(Form1 form)
        //{
        //    parentForm = form;
        //}

        //[Category("UIE"),
        //Description("Properties for UIE")]
        //public Form1 hParent
        //{
        //    get { return parentForm; }
        //    set { parentForm = value; }
        //}
        [Category("UIE"), DisplayName("Kind"),
        Description("Type of cocos Node(UI)")]
        public NodeKind nKind
        {
            get { return _nodekind; }
            set { _nodekind = value; }
        }
        [Category("UIE"), DisplayName("Asset Path"),
            Editor(typeof(System.Windows.Forms.Design.FileNameEditor),
          typeof(System.Drawing.Design.UITypeEditor)),
        Description("Asset Full Path")]
        public string hAssetPath
        {
            get { return assetPath; }
            set {
                if (assetPath != value)
                {
                    assetPath = value;
                    this.renewBitmap(assetPath);
                }
                if (this.image != null)
                {
                    int aa = assetPath.LastIndexOf("Resources\\");
                    hSafeFilename = assetPath.Substring(aa + 10, assetPath.Length - aa - 10);
                }
            }
        }
        [Category("UIE"), DisplayName("File Name"), ReadOnly(true),
        Description("Asset FileName")]
        public string hSafeFilename
        {
            get { return safeFilename; }
            set { safeFilename = value; }
        }

        [Category("Node"), DisplayName("Tag"),
        Description("Node Tag as int")]
        public int hTag
        {
            get { return Tag; }
            set { Tag = value; }
        }

        [Category("Node"), DisplayName("Name"),
        Description("Node Name as string")]
        public string hName
        {
            get { return Name; }
            set { Name = value; }
        }
        [Category("Node"), DisplayName("Position"),
        Description("Node position as Vec2")]
        public Point hPosition
        {
            get { return new Point((int)X, (int)Y); }
            set {
                X = value.X; Y = value.Y;
            }
        }
        //[Category("Node"),
        //Description("Node position from Display Edge"),
        //DisplayName("Docking")]//, DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        //public NodeRect hDocking
        //{
        //    get { return new NodeRect(Left, Top, Right, Bottom); }
        //    set {
        //        Left = value.left;
        //        Top = value.top;
        //        Right = value.right;
        //        Bottom = value.bottom;
        //    }
        //}
        [Category("Node"),
        Description("Node position from Display Edge"),
        DisplayName("Docking Left")]
        public int hLeft
        {
            get { return (int)Left; }
            set
            {
                Left = value;
            }
        }
        [Category("Node"),
        Description("Node position from Display Edge"),
        DisplayName("Docking Right")]
        public int hRight
        {
            get { return (int)Right; }
            set
            {
                Right = value;
            }
        }
        [Category("Node"),
        Description("Node position from Display Edge"),
        DisplayName("Docking Top")]
        public int hTop
        {
            get { return (int)Top; }
            set
            {
                Top = value;
            }
        }
        [Category("Node"),
        Description("Node position from Display Edge"),
        DisplayName("Docking Bottom")]
        public int hBottom
        {
            get { return (int)Bottom; }
            set
            {
                Bottom = value;
            }
        }

        //[Category("Node"),
        //Description("Node Anchor"), 
        //DisplayName("Anchor")]//, DesignerSerializationVisibility(DesignerSerializationVisibility.Content), TypeConverter(typeof(ExpandableObjectConverter))]
        //public PointF hAnchor
        //{
        //    get { return new PointF(anchorX, anchorY); }
        //    set {
        //        anchorX = value.X;
        //        anchorY = value.Y;
        //    }
        //}
        [Category("Node"),
        Description("Node Anchor X"),
        DisplayName("Anchor X")]
        public float hAnchorX
        {
            get { return anchorX; }
            set
            {
                anchorX = value;
            }
        }
        [Category("Node"),
        Description("Node Anchor Y"),
        DisplayName("Anchor Y")]
        public float hAnchorY
        {
            get { return anchorY; }
            set
            {
                anchorY = value;
            }
        }
        [Category("Node"), DisplayName("Opacity"),
        Description("Node Opacity (don't apply on GUIEditor)")]
        public int hOpacity
        {
            get { return Opacity; }
            set { Opacity = value; }
        }
        [Category("Node"), DisplayName("Color"),
        Description("Node Color3B (don't apply on GUIEditor)")]
        public Color hColor
        {
            get { return sColor; }
            set { sColor = value; }
        }
        [Category("Node"), DisplayName("Scale"),
        Description("Node Scale")]
        public float hScale
        {
            get { return Scale; }
            set { Scale = value; }
        }
        [Category("Node"), DisplayName("Z Order"),
        Description("Node local Z order")]
        public int hZorder
        {
            get { return Zorder; }
            set { Zorder = value; }
        }

        //[Category("Node"), DisplayName("Offset"),
        //Description("Node position as offset type")]
        //public Anchor hOffset
        //{
        //    get { return new Anchor(relativeX, relativeY); }
        //    set {
        //        relativeX = value.X; relativeY = value.Y;
        //    }
        //}
        [Category("Node"),
        Description("Nomarlized Position"),
        DisplayName("Nomarlized X")]
        public float hOffsetX
        {
            get { return relativeX; }
            set
            {
                relativeX = value;
            }
        }
        [Category("Node"),
        Description("Nomarlized Position"),
        DisplayName("Nomarlized Y")]
        public float hOffsetY
        {
            get { return relativeY; }
            set
            {
                relativeY = value;
            }
        }
        [Category("Node"), DisplayName("Position Mode"),
        Description("Type of Position")]
        public PosKind hPosmode
        {
            get { return _nodepostype; }
            set { _nodepostype = value; }
        }
        [Category("Node"), DisplayName("Rotation"),
        Description("Type of Position")]
        public int hRotate
        {
            get { return Rotate; }
            set { Rotate = value; this.bitmapRotate(Rotate); }
        }
        [Category("Node"), DisplayName("Flip X"),
        Description("Type of Position")]
        public bool hFlipX
        {
            get { return FlipX; }
            set { FlipX = value; this.bitmapFlipX(); }
        }
        [Category("Node"), DisplayName("Flip Y"),
        Description("Type of Position")]
        public bool hFlipY
        {
            get { return FlipY; }
            set { FlipY = value; this.bitmapFlipY(); }
        }
        [Category("Node"), DisplayName("Resource Type"),
        Description("Local or PLIST. It will effects only when Export.")]
        public RSC hRscFrom
        {
            get { return RscFrom; }
            set { RscFrom = value; }
        }

        [Category("Scale9Sprite"), DisplayName("CapInsets"),
        Description("Define Edge area")]
        public Rectangle hCapInsets
        {
            get { return CapInsets; }
            set { CapInsets = value; }
        }
        [Category("Scale9Sprite"), DisplayName("ContentSize"),
        Description("Outsize of Box")]
        public Size hContentSize
        {
            get { return ContentSize; }
            set { ContentSize = value; }
        }

        //ImageMenu, Button, Slider Node
        [Category("Selectable"), DisplayName("Pressed"),
            Editor(typeof(System.Windows.Forms.Design.FileNameEditor),
          typeof(System.Drawing.Design.UITypeEditor)),
        Description("Image when Pressed of Button, Image Menu, Slider, CheckBox")]
        public string hpressAsset
        {
            get { return pressAsset; }
            set
            {
                pressAsset = value;
            }
        }
        [Category("Selectable"), DisplayName("Disable"),
            Editor(typeof(System.Windows.Forms.Design.FileNameEditor),
          typeof(System.Drawing.Design.UITypeEditor)),
        Description("Image for selectable UI Button, Image Menu, Slider, CheckBox but disable")]
        public string hdisableAsset
        {
            get { return disableAsset; }
            set
            {
                disableAsset = value;
            }
        }

        //CheckBox
        [Category("CheckBox"), DisplayName("Checked"),
            Editor(typeof(System.Windows.Forms.Design.FileNameEditor),
          typeof(System.Drawing.Design.UITypeEditor)),
        Description("CheckBox image when Checked active")]
        public string hcheckedAsset
        {
            get { return checkedAsset; }
            set
            {
                checkedAsset = value;
                if(baseCheck)
                {
                    if (checkedAssetImage != null)
                        checkedAssetImage.Dispose();
                    checkedAssetImage = new Bitmap(checkedAsset);
                }
            }
        }
        [Category("CheckBox"), DisplayName("Checked Disable"),
            Editor(typeof(System.Windows.Forms.Design.FileNameEditor),
          typeof(System.Drawing.Design.UITypeEditor)),
        Description("CheckBox image when Checked active but disable")]
        public string hcheckedDisableAsset
        {
            get { return checkedDisableAsset; }
            set { checkedDisableAsset = value; }
        }
        [Category("CheckBox"), DisplayName("Init"),
        Description("CheckBox image when Checked status but disable")]
        public bool hbaseCheck
        {
            get { return baseCheck; }
            set
            {
                baseCheck = value;
                if (baseCheck)
                {
                    if (checkedAssetImage != null)
                        checkedAssetImage.Dispose();
                    checkedAssetImage = new Bitmap(checkedAsset);
                }
            }
        }

        //Slider
        [Category("Slider"), DisplayName("Slide Ball"),
            Editor(typeof(System.Windows.Forms.Design.FileNameEditor),
          typeof(System.Drawing.Design.UITypeEditor)),
        Description("Use for Slider's controller Ball image")]
        public string hballAsset
        {
            get { return ballAsset; }
            set
            {
                ballAsset = value;
                if (ballImage != null)
                    ballImage.Dispose();
                ballImage = new Bitmap(ballAsset);
            }
        }
        [Category("Slider"), DisplayName("Slide Bar"),
            Editor(typeof(System.Windows.Forms.Design.FileNameEditor),
          typeof(System.Drawing.Design.UITypeEditor)),
        Description("Use for Slider's Bar image")]
        public string hbarAsset
        {
            get { return barAsset; }
            set
            {
                barAsset = value;
                if (barImage != null)
                    barImage.Dispose();
                barImage = new Bitmap(barAsset);
            }
        }
        //barPercent
        [Category("Slider && LoadingBar"), DisplayName("Percent"),
        Description("Initial Value for Bar of Slider and LoadingBar (0~100)")]
        public int hbarPercent
        {
            get { return barPercent; }
            set
            {
                barPercent = value;
                if (barPercent < 0)
                    barPercent = 0;
                if (barPercent > 100)
                    barPercent = 100;
            }
        }
        [Category("Slider && LoadingBar"), DisplayName("Direction"),
        Description("Bar's Direction as Horizontal or Vertical")]
        public Direction hbarDir
        {
            get { return bardir; }
            set { bardir = value; }
        }

        [Category("Label"), DisplayName("Font"),
        Description("only for Label")]
        public Font lFontprop
        {
            get { return labelFont; }
            set
            {
                labelFont = value;
                fontpath = labelFont.Name;
                fSize = (int)labelFont.Size;
            }
        }
        [Category("Label"), DisplayName("Font Name"),
        Description("Font name or Font path. for Labels(Label, BMFont Label, Button Title and TextField)")]
        public string lFont
        {
            get { return fontpath; }
            set { fontpath = value; }
        }
        [Category("Label"), DisplayName("Text"),
        Description("initial Content of Labels")]
        public string lText
        {
            get { return content; }
            set { content = value; }
        }
        [Category("Label"), DisplayName("Color"),
        Description("Color of Labels")]
        public Color lColor
        {
            get { return tColor; }
            set { tColor = value; }
        }
        [Category("Label"), DisplayName("Font Size"),
        Description("Font size of Labels")]
        public int lFontSize
        {
            get { return fSize; }
            set { fSize = value; }
        }

        //TextField
        //private bool isPassword = false;
        //private int maxLength = 50;
        [Category("TextField"), DisplayName("is Password"),
        Description("TextField use for password input")]
        public bool hisPassword
        {
            get { return isPassword; }
            set { isPassword = value; }
        }
        [Category("TextField"), DisplayName("max Length"),
        Description("TextField's max length")]
        public int hmaxLength
        {
            get { return maxLength; }
            set { maxLength = value; }
        }


        public Object Clone()
        {
            return this.MemberwiseClone();
        }

    }
}
