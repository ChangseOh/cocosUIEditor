using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Drawing.Drawing2D;

namespace cocosUiEditor
{
    public partial class Form1 : Form
    {
        Random rand = new Random();

        int donateLink = 0;
        List<CocosNode> cocosNodes;
        Bitmap backBuffer;
        Bitmap loopeBuffer;
        string ProjectName;
        int CanvasWidth, CanvasHeight;
        float CanvasScale = 1.0f;
        int CanvasScaleInt = 100;

        bool isChanged = false;

        bool showCross = true;
        bool showBorder = true;
        bool showAllBorder = false;

        Point clickAdj;
        Point clickStart;
        string mainPath = "";
        string fileName = "";

        public Form1()
        {
            InitializeComponent();

            TypeDescriptor.AddAttributes(typeof(PointF), new TypeConverterAttribute(typeof(ExpandableObjectConverter)));


            cocosNodes = new List<CocosNode>();
            cocosNodes.Clear();

            ProjectName = "Default";
            CanvasWidth = 480;
            CanvasHeight = 800;
            CanvasScale = 1.0f;

            Timer roofer = new Timer();
            roofer.Interval = 16;
            roofer.Tick += new EventHandler(tick);
            roofer.Start();

            Timer donateRenew = new Timer();
            donateRenew.Interval = 180000;
            donateRenew.Tick += new EventHandler(donateTick);
            donateRenew.Start();

            this.ResizeEnd += new EventHandler(Form1_CreateBackBuffer);
            this.Load += new EventHandler(Form1_CreateBackBuffer);
            this.Paint += new PaintEventHandler(Form1_Paint);

            this.FormClosing += systemQuit;
            //this.FormClosed += quitToolStripMenuItem_Click;
            //this.KeyDown += new KeyEventHandler(Form1_KeyDown);

            pictureBox1.MouseDown += new MouseEventHandler(Controll_MouseDown);
            pictureBox1.MouseMove += new MouseEventHandler(Controll_MouseMove);
            pictureBox1.MouseUp += new MouseEventHandler(Controll_MouseUp);

            //backBuffer = new Bitmap(CanvasWidth, CanvasHeight);
            //pictureBox1.Width = (int)(CanvasWidth * CanvasScaleInt / 100);
            //pictureBox1.Height = (int)(CanvasHeight * CanvasScaleInt / 100);
            loopeBuffer = new Bitmap(pictureBox3.Width, pictureBox3.Height);

            DoubleBuffered = true;

            label3.Text = ProjectName;

            donateTick(this, EventArgs.Empty);

            MessageBox.Show("Welcome cocos GUI Editor!!\n(version 0.5.1)");
        }
        void donateTick(object sender, EventArgs e)
        {
            donateLink = rand.Next() % 2;
            switch (donateLink)
            {
                case 0:
                    pictureBox2.Load("http://www.g-gameplay.com/donate_oxfighter.jpg");
                    break;
                case 1:
                    pictureBox2.Load("http://www.g-gameplay.com/donate_moekeeper.jpg");
                    break;
                case 2:
                    pictureBox2.Load("http://www.g-gameplay.com/donate_moekeeper.jpg");
                    break;
                case 3:
                    pictureBox2.Load("http://www.g-gameplay.com/donate_moekeeper.jpg");
                    break;
            }
        }
        void tick(object sender, EventArgs e)
        {
            Draw();
        }
        void Draw()
        {
            if (backBuffer != null)
            {
                using (var g = Graphics.FromImage(backBuffer))
                {
                    //g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    List<int> zorders = new List<int>();
                    zorders.Clear();
                    foreach (CocosNode a in cocosNodes)
                    {
                        int zorder = a.hZorder;
                        if (zorders.IndexOf(zorder) == -1)
                            zorders.Add(zorder);
                    }
                    zorders.Sort();

                    g.Clear(Color.White);
                    foreach (int z in zorders)
                    {
                        foreach (CocosNode a in cocosNodes)
                        {
                            bool isDraw = true;
                            if (a.nKind != NodeKind.Label && a.nKind != NodeKind.BMFLabel && a.image == null)
                                isDraw = false;
                            if (isDraw && a.hZorder == z)
                            {
                                float _x = 0;
                                float _y = 0;
                                if (a.nKind == NodeKind.Scale9Sprite && a.insectImage != null)
                                {
                                    _x = a.hPosition.X - a.insectImage.Width * a.hAnchorX * a.hScale;
                                    _y = a.hPosition.Y - a.insectImage.Height * a.hAnchorY * a.hScale;
                                    g.DrawImage(a.insectImage, new RectangleF(_x, _y, a.insectImage.Width * a.hScale, a.insectImage.Height * a.hScale));

                                }
                                else if (a.nKind != NodeKind.Label && a.nKind != NodeKind.BMFLabel && a.nKind != NodeKind.TextField)
                                {
                                    _x = a.hPosition.X - a.image.Width * a.hAnchorX * a.hScale;
                                    _y = a.hPosition.Y - a.image.Height * a.hAnchorY * a.hScale;
                                    if (a.nKind == NodeKind.LoadingBar && a.hbarPercent > 0 && a.hbarPercent < 100)
                                    {
                                        if (a.hbarDir == Direction.LEFT)
                                            g.DrawImage(a.image, new RectangleF(_x, _y, a.image.Width * a.hScale * a.hbarPercent / 100, a.image.Height * a.hScale), new RectangleF(0, 0, a.image.Width * a.hScale * a.hbarPercent / 100, a.image.Height * a.hScale), GraphicsUnit.Pixel);//left to right bar
                                        else
                                        {
                                            g.DrawImage(a.image, new RectangleF(_x + a.image.Width * a.hScale * (100 - a.hbarPercent) / 100, _y, a.image.Width * a.hScale * a.hbarPercent / 100, a.image.Height * a.hScale), new RectangleF(a.image.Width * a.hScale * (100 - a.hbarPercent) / 100, 0, a.image.Width * a.hScale * a.hbarPercent / 100, a.image.Height * a.hScale), GraphicsUnit.Pixel);//right to left bar
                                        }
                                    }
                                    else if (a.nKind == NodeKind.CheckBox)
                                    {
                                        g.DrawImage(a.image, new RectangleF(_x, _y, a.image.Width * a.hScale, a.image.Height * a.hScale));
                                        if (a.hbaseCheck && a.checkedAssetImage != null)
                                            g.DrawImage(a.checkedAssetImage, new RectangleF(_x, _y, a.checkedAssetImage.Width * a.hScale, a.checkedAssetImage.Height * a.hScale));
                                    }
                                    else
                                        g.DrawImage(a.image, new RectangleF(_x, _y, a.image.Width * a.hScale, a.image.Height * a.hScale));

                                    if (a.nKind == NodeKind.CheckBox && a.hbaseCheck && a.checkedAssetImage != null)
                                    {
                                        g.DrawImage(a.checkedAssetImage, new RectangleF(_x, _y, a.checkedAssetImage.Width * a.hScale, a.checkedAssetImage.Height * a.hScale));
                                    }
                                    if (a.nKind == NodeKind.Slider && a.hbarPercent > 0 && a.barImage != null)
                                    {
                                        //image = back
                                        //draw bar with percent
                                        float adjX = (a.image.Size.Width - a.barImage.Size.Width) / 2 * a.hScale;
                                        float adjY = (a.image.Size.Height - a.barImage.Size.Height) / 2 * a.hScale;
                                        //if (a.hbarDir == Direction.Horizoontal)
                                        {
                                            g.DrawImage(a.barImage, new RectangleF(_x + adjX, _y + adjY, a.barImage.Width * a.hScale * a.hbarPercent / 100, a.barImage.Height * a.hScale), new RectangleF(0, 0, a.barImage.Width * a.hbarPercent / 100, a.barImage.Height), GraphicsUnit.Pixel);//horizontal bar
                                            if (a.ballImage != null)
                                            {
                                                g.DrawImage(a.ballImage, new RectangleF(_x + a.barImage.Width * a.hScale * a.hbarPercent / 100 - a.ballImage.Width * a.hScale / 2, _y - a.ballImage.Height * a.hScale / 2 + a.image.Size.Height * a.hScale / 2, a.ballImage.Width * a.hScale, a.ballImage.Height * a.hScale));
                                            }
                                        }
                                        //else
                                        //{
                                        //    g.DrawImage(a.barImage, new RectangleF(_x + adjX, _y + adjY + a.barImage.Height * a.hScale * (100 - a.hbarPercent) / 100, a.barImage.Width * a.hScale, a.barImage.Height * a.hScale * a.hbarPercent / 100), new RectangleF(0, a.barImage.Height * (100 - a.hbarPercent) / 100, a.barImage.Width, a.barImage.Height * a.hbarPercent / 100), GraphicsUnit.Pixel);//vertical bar
                                        //    if (a.ballImage != null)
                                        //    {
                                        //        g.DrawImage(a.ballImage, new RectangleF(_x - a.ballImage.Width * a.hScale / 2 + a.image.Size.Width * a.hScale / 2, _y + a.barImage.Height * a.hScale * (100 - a.hbarPercent) / 100 - a.ballImage.Height * a.hScale / 2, a.ballImage.Width * a.hScale, a.ballImage.Height * a.hScale));
                                        //    }
                                        //}
                                    }
                                }
                                if (a.nKind == NodeKind.Label || a.nKind == NodeKind.BMFLabel || a.nKind == NodeKind.Button || a.nKind == NodeKind.TextField)
                                {
                                    string fontname = a.lFont;
                                    int fontsize = a.lFontSize;
                                    if (fontsize < 5)
                                    {
                                        fontsize = 5;
                                        a.lFontSize = 5;
                                    }
                                    Color fontcolor = a.lColor;// Color.FromName(a.lColor);
                                    if (fontname == "")
                                        fontname = "Arial";
                                    if (fontcolor.A == 0)
                                        fontcolor = Color.Black;
                                    Font font = new Font(fontname, fontsize * a.hScale);
                                    SizeF textsize = g.MeasureString(a.lText, font);
                                    _x = a.hPosition.X - textsize.Width * a.hAnchorX * a.hScale;
                                    _y = a.hPosition.Y - textsize.Height * a.hAnchorY * a.hScale;

                                    SolidBrush brush = new SolidBrush(fontcolor);
                                    g.DrawString(a.lText, font, brush, new PointF(_x, _y));
                                    brush.Dispose();
                                }
                            }
                        }
                    }
                    //make loope
                    {
                        Point _point = PointToClient(new Point(Control.MousePosition.X, Control.MousePosition.Y));
                        int _x = (_point.X - panel3.AutoScrollPosition.X) * 100 / CanvasScaleInt;
                        int _y = (_point.Y - panel3.AutoScrollPosition.Y) * 100 / CanvasScaleInt;

                        RectangleF rect = new RectangleF(0, 0, CanvasWidth * CanvasScaleInt / 100, CanvasHeight * CanvasScaleInt / 100);//pictureBox1.Image.GetBounds(ref gunit);

                        if (rect.Contains(_point))
                        {
                            Graphics loope = Graphics.FromImage(loopeBuffer);
                            //loope.SmoothingMode = SmoothingMode.None;
                            loope.InterpolationMode = InterpolationMode.NearestNeighbor;
                            loope.DrawImage(backBuffer, new RectangleF(0, 0, pictureBox3.Width, pictureBox3.Height), new RectangleF(_x - 5, _y - 2, 10, 5), GraphicsUnit.Pixel);
                            pictureBox3.CreateGraphics().DrawImage(loopeBuffer, new Point(0, 0));

                            //loope.Dispose();
                        }
                    }

                    //Draw BorderLine
                    foreach (int z in zorders)
                    {
                        foreach (CocosNode a in cocosNodes)
                        {
                            bool isDraw = true;
                            if (a.nKind != NodeKind.Label && a.nKind != NodeKind.BMFLabel && a.image == null)
                                isDraw = false;

                            if (isDraw && a.hZorder == z)
                            {
                                if (a.nKind == NodeKind.Scale9Sprite && a.insectImage != null)
                                {
                                    if (a == selectedNode && showBorder)
                                    {
                                        drawBorder(g, new PointF(a.hPosition.X, a.hPosition.Y), new PointF(a.hAnchorX, a.hAnchorY), Color.Red, a.insectImage.Size, a.hScale);
                                    }
                                    else if (showAllBorder)
                                    {
                                        drawBorder(g, new PointF(a.hPosition.X, a.hPosition.Y), new PointF(a.hAnchorX, a.hAnchorY), Color.Blue, a.insectImage.Size, a.hScale);
                                    }
                                }
                                else if (a.nKind == NodeKind.TextField)
                                {
                                    //Draw Border and Anchor
                                    if (a == selectedNode && showBorder)
                                    {
                                        drawBorder(g, new PointF(a.hPosition.X, a.hPosition.Y), new PointF(a.hAnchorX, a.hAnchorY), Color.Red, a.hContentSize, 1);
                                    }
                                    else if (showAllBorder)
                                    {
                                        drawBorder(g, new PointF(a.hPosition.X, a.hPosition.Y), new PointF(a.hAnchorX, a.hAnchorY), Color.Blue, a.hContentSize, 1);
                                    }
                                }
                                else if (a.nKind == NodeKind.Menu)
                                {
                                    //Draw Border and Anchor
                                    if (a.lText != null && a.lText.Length > 0)
                                    {
                                        string fontname = a.lFont;
                                        int fontsize = a.lFontSize;
                                        if (fontsize < 5)
                                        {
                                            fontsize = 5;
                                            a.lFontSize = 5;
                                        }
                                        if (fontname == "")
                                            fontname = "Arial";
                                        Font font = new Font(fontname, fontsize * a.hScale);
                                        SizeF textsize = g.MeasureString(a.lText, font);

                                        if (a == selectedNode && showBorder)
                                        {
                                            drawBorder(g, new PointF(a.hPosition.X, a.hPosition.Y), new PointF(a.hAnchorX, a.hAnchorY), Color.Red, textsize, a.hScale);
                                        }
                                        else if (showAllBorder)
                                        {
                                            drawBorder(g, new PointF(a.hPosition.X, a.hPosition.Y), new PointF(a.hAnchorX, a.hAnchorY), Color.Blue, textsize, a.hScale);
                                        }
                                    }
                                    else
                                    {
                                        if (a == selectedNode && showBorder)
                                        {
                                            drawBorder(g, new PointF(a.hPosition.X, a.hPosition.Y), new PointF(a.hAnchorX, a.hAnchorY), Color.Red, a.image.Size, a.hScale);
                                        }
                                        else if (showAllBorder)
                                        {
                                            drawBorder(g, new PointF(a.hPosition.X, a.hPosition.Y), new PointF(a.hAnchorX, a.hAnchorY), Color.Blue, a.image.Size, a.hScale);
                                        }
                                    }
                                }
                                else if (a.nKind != NodeKind.Label && a.nKind != NodeKind.BMFLabel)
                                {
                                    //Draw Border and Anchor
                                    if (a == selectedNode && showBorder)
                                    {
                                        drawBorder(g, new PointF(a.hPosition.X, a.hPosition.Y), new PointF(a.hAnchorX, a.hAnchorY), Color.Red, a.image.Size, a.hScale);
                                    }
                                    else if (showAllBorder)
                                    {
                                        drawBorder(g, new PointF(a.hPosition.X, a.hPosition.Y), new PointF(a.hAnchorX, a.hAnchorY), Color.Blue, a.image.Size, a.hScale);
                                    }
                                }
                                if (a.nKind == NodeKind.Label || a.nKind == NodeKind.BMFLabel || a.nKind == NodeKind.Button)
                                {
                                    string fontname = a.lFont;
                                    int fontsize = a.lFontSize;
                                    if (fontsize < 5)
                                    {
                                        fontsize = 5;
                                        a.lFontSize = 5;
                                    }
                                    if (fontname == "")
                                        fontname = "Arial";
                                    Font font = new Font(fontname, fontsize * a.hScale);
                                    SizeF textsize = g.MeasureString(a.lText, font);

                                    if (a.nKind != NodeKind.Button)
                                    {
                                        if (a == selectedNode && showBorder)
                                        {
                                            drawBorder(g, new PointF(a.hPosition.X, a.hPosition.Y), new PointF(a.hAnchorX, a.hAnchorY), Color.Red, textsize, a.hScale);
                                        }
                                        else if (showAllBorder)
                                        {
                                            drawBorder(g, new PointF(a.hPosition.X, a.hPosition.Y), new PointF(a.hAnchorX, a.hAnchorY), Color.Blue, textsize, a.hScale);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //Draw Cross Wire
                    if (showCross)
                    {
                        Point _point = PointToClient(new Point(Control.MousePosition.X, Control.MousePosition.Y));

                        if (ModifierKeys == Keys.Shift && mouseIsDown)
                        {
                            int _adjX = Math.Abs(clickStart.X - _point.X);
                            int _adjY = Math.Abs(clickStart.Y - _point.Y);

                            if (_adjX > _adjY)
                                _point.Y = clickStart.Y;
                            if (_adjX < _adjY)
                                _point.X = clickStart.X;
                        }

                        int _x = (_point.X - panel3.AutoScrollPosition.X) * 100 / CanvasScaleInt;
                        int _y = (_point.Y - panel3.AutoScrollPosition.Y) * 100 / CanvasScaleInt;

                        RectangleF rect = new RectangleF(0, 0, CanvasWidth * CanvasScaleInt / 100, CanvasHeight * CanvasScaleInt / 100);//pictureBox1.Image.GetBounds(ref gunit);

                        if (rect.Contains(_point))//new Point(_x, _y)))
                        {
                            SolidBrush brush = new SolidBrush(Color.Green);
                            Pen pen = new Pen(brush);
                            g.DrawLine(pen, _x, 0, _x, _y - 4);
                            g.DrawLine(pen, _x, _y + 4, _x, CanvasHeight);

                            g.DrawLine(pen, 0, _y, _x - 4, _y);
                            g.DrawLine(pen, _x + 4, _y, CanvasWidth, _y);

                            pen.Dispose();
                            brush.Dispose();

                            label6.Text = string.Format("Cursor\n{0:F0} x {1:F0}", _x, _y);
                        }
                    }
                }
                Invalidate();
            }
        }
        void drawBorder(Graphics g, PointF loc, PointF anchor, Color color, SizeF size, float scale)
        {
            float _x = loc.X - size.Width * anchor.X * scale;
            float _y = loc.Y - size.Height * anchor.Y * scale;

            SolidBrush brush = new SolidBrush(color);
            g.DrawRectangle(new Pen(brush), _x, _y, size.Width * scale - 1, size.Height * scale - 1);
            brush.Dispose();

            SolidBrush brush2 = new SolidBrush(Color.Gray);
            g.DrawRectangle(new Pen(brush2), loc.X - 2, loc.Y - 2, 4, 4);
            brush2.Dispose();
        }
        void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (backBuffer != null)
            {
                pictureBox1.CreateGraphics().DrawImage(backBuffer, new Rectangle(0, 0, (int)(CanvasWidth * CanvasScale), (int)(CanvasHeight * CanvasScale)));
                //e.Graphics.DrawImage(backBuffer, new Rectangle(0, 0, (int)(CanvasWidth * CanvasScale), (int)(CanvasHeight * CanvasScale)));
            }
        }
        void Form1_CreateBackBuffer(object sender, EventArgs e)
        {
            if (backBuffer != null)
                backBuffer.Dispose();

            backBuffer = new Bitmap(CanvasWidth, CanvasHeight);
            pictureBox1.Width = (int)(CanvasWidth * CanvasScaleInt / 100);
            pictureBox1.Height = (int)(CanvasHeight * CanvasScaleInt / 100);
            label14.Text = string.Format("({0} X {1})", CanvasWidth, CanvasHeight);
        }
        private void button1_Click(object sender, EventArgs e)
        {

            Console.WriteLine(ShowFileOpenDialog());
        }
        public string ShowFileOpenDialog()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Asset";
            //ofd.FileName = "*";
            ofd.Filter = "Image Files(*.jpg;*.png)|*.jpg;*.png";
            //ofd.RestoreDirectory = true;

            DialogResult dr = ofd.ShowDialog();

            if (dr == DialogResult.OK && ofd != null)
            {
                string filename = ofd.SafeFileName;
                string fileFullName = ofd.FileName;
                string filePath = fileFullName.Replace(filename, "");
                int aa = fileFullName.LastIndexOf("Resources\\");
                string filePathName = fileFullName.Substring(aa + 10, fileFullName.Length - aa - 10);

                mainPath = filePath;

                CocosNode node = new CocosNode(fileFullName);
                //node.setParent(this);
                node.hPosition = new Point(CanvasWidth / 2, CanvasHeight / 2);
                node.hSafeFilename = filePathName;// filename;
                cocosNodes.Add(node);
                node.hOffsetX = (node.hPosition.X - CanvasWidth * 0.5f) / (float)CanvasWidth;
                node.hOffsetY = (node.hPosition.Y - CanvasHeight * 0.5f) / (float)CanvasHeight;
                //node.hOffset = new Anchor(((node.hPosition.X - CanvasWidth * 0.5f) / (float)CanvasWidth), ((node.hPosition.Y - CanvasHeight * 0.5f) / (float)CanvasHeight));
                //node.hDocking = new NodeRect(
                node.hLeft = node.hPosition.X;
                node.hTop = node.hPosition.Y;
                node.hRight = (CanvasWidth - node.hPosition.X);
                node.hBottom = (CanvasHeight - node.hPosition.Y);
                //);

                selectedNode = node;
                textBox1.Text = selectedNode.hAssetPath;
                propertyGrid1.SelectedObject = selectedNode;

                //propertyGrid1.BrowsableAttributes = new AttributeCollection(
                //    new CategoryAttribute("UIE"),
                //    new CategoryAttribute("Node")
                //    );

                isChanged = true;

                return fileFullName;
            }
            else if (dr == DialogResult.Cancel)
            {
                return "";
            }

            return "";
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int percent = Int32.Parse(numericUpDown1.Value.ToString());
            CanvasScale = (float)percent * 0.01f;
            CanvasScaleInt = percent;// (int)(CanvasScale * 100);
            Form1_CreateBackBuffer(this, EventArgs.Empty);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 prompt = new Form2(ProjectName, CanvasWidth, CanvasHeight);
            DialogResult dr = prompt.ShowDialog();

            if (dr == DialogResult.OK)
            {
                cocosNodes.Clear();
                ProjectName = prompt.passName;
                CanvasWidth = Int32.Parse(prompt.passWidth);
                CanvasHeight = Int32.Parse(prompt.passHeight);
                Form1_CreateBackBuffer(prompt, EventArgs.Empty);
                label3.Text = ProjectName;
            }

            //prompt.Close += Eve
            //Console.WriteLine(prompt.passName);
            //Console.WriteLine(prompt.passWidth);
            //Console.WriteLine(prompt.passHeight);

        }
        bool mouseIsDown = false;
        CocosNode selectedNode = null;
        public void KeyDowned(object sender, KeyEventArgs e)
        {
            if (selectedNode == null)
                return;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    selectedNode.hPosition = new Point((int)selectedNode.hPosition.X, (int)selectedNode.hPosition.Y - 1);
                    propertyGrid1.SelectedObject = selectedNode;
                    break;
                case Keys.Down:
                    selectedNode.hPosition = new Point((int)selectedNode.hPosition.X, (int)selectedNode.hPosition.Y + 1);
                    propertyGrid1.SelectedObject = selectedNode;
                    break;
                case Keys.Left:
                    selectedNode.hPosition = new Point((int)selectedNode.hPosition.X - 1, (int)selectedNode.hPosition.Y);
                    propertyGrid1.SelectedObject = selectedNode;
                    break;
                case Keys.Right:
                    selectedNode.hPosition = new Point((int)selectedNode.hPosition.X + 1, (int)selectedNode.hPosition.Y);
                    propertyGrid1.SelectedObject = selectedNode;
                    break;
                case Keys.Escape:
                    selectedNode = null;
                    textBox1.Text = "";
                    propertyGrid1.SelectedObject = null;
                    checkBox1.Text = "Left";
                    checkBox2.Text = "Top";
                    checkBox3.Text = "Right";
                    checkBox4.Text = "Bottom";
                    label2.Text = "Vec2";
                    label5.Text = "Offset";
                    break;
            }
        }
        public void Controll_MouseDown(object sender, MouseEventArgs e)
        {
            Point loc = e.Location;
            loc.X = loc.X * 100 / CanvasScaleInt;
            loc.Y = loc.Y * 100 / CanvasScaleInt;
            //Console.WriteLine(string.Format("x={0},{1}", e.X, loc.X));
            //Console.WriteLine(string.Format("y={0},{1}", e.Y, loc.Y));

            GraphicsUnit gunit = GraphicsUnit.Point;
            List<int> zorders = new List<int>();
            zorders.Clear();
            foreach (CocosNode a in cocosNodes)
            {
                int zorder = a.hZorder;
                if (zorders.IndexOf(zorder) == -1)
                    zorders.Add(zorder);
            }
            zorders.Sort();
            zorders.Reverse();

            foreach (int z in zorders)
            {
                for (int i = cocosNodes.Count() - 1; i >= 0; i--)
                {
                    CocosNode temp = cocosNodes.ElementAt(i);
                    if (temp.image == null && temp.nKind != NodeKind.Label && temp.nKind != NodeKind.BMFLabel)
                        continue;

                    if (temp.hZorder != z)
                        continue;

                    Rectangle rect = new Rectangle();
                    int _x = 0;
                    int _y = 0;
                    SizeF size = new SizeF();

                    if (temp.nKind == NodeKind.Scale9Sprite && temp.insectImage != null)
                    {
                        RectangleF frect = temp.insectImage.GetBounds(ref gunit);
                        rect = new Rectangle((int)frect.X, (int)frect.Y, (int)frect.Width, (int)frect.Height);
                        _x = (int)(temp.hPosition.X - temp.insectImage.Width * temp.hAnchorX * temp.hScale);
                        _y = (int)(temp.hPosition.Y - temp.insectImage.Height * temp.hAnchorY * temp.hScale);
                        size = new SizeF(temp.insectImage.Width * temp.hScale, temp.insectImage.Height * temp.hScale);
                    }
                    else if (temp.nKind != NodeKind.Label && temp.nKind != NodeKind.BMFLabel)
                    {
                        RectangleF frect = temp.image.GetBounds(ref gunit);
                        rect = new Rectangle((int)frect.X, (int)frect.Y, (int)frect.Width, (int)frect.Height);
                        _x = (int)(temp.hPosition.X - temp.image.Width * temp.hAnchorX * temp.hScale);
                        _y = (int)(temp.hPosition.Y - temp.image.Height * temp.hAnchorY * temp.hScale);
                        size = new SizeF(temp.image.Width * temp.hScale, temp.image.Height * temp.hScale);
                    }

                    else if (temp.nKind == NodeKind.Label || temp.nKind == NodeKind.BMFLabel)
                    {
                        Graphics g = Graphics.FromImage(backBuffer);
                        string fontname = temp.lFont;
                        int fontsize = temp.lFontSize;
                        if (fontsize < 5)
                        {
                            fontsize = 5;
                            temp.lFontSize = 5;
                        }
                        Color fontcolor = temp.lColor;//Color.FromName(temp.Func3);
                        //if (fontname == "")
                        //    fontname = "Arial";
                        if (fontcolor.A == 0)
                            fontcolor = Color.Black;
                        Font font = new Font(fontname, fontsize * temp.hScale);
                        SizeF textsize = g.MeasureString(temp.lText, font);
                        _x = (int)(temp.hPosition.X - textsize.Width * temp.hAnchorX * temp.hScale);
                        _y = (int)(temp.hPosition.Y - textsize.Height * temp.hAnchorY * temp.hScale);
                        size.Width = textsize.Width * temp.hScale;
                        size.Height = textsize.Height * temp.hScale;
                    }
                    rect.X = _x;
                    rect.Y = _y;
                    rect.Width = (int)size.Width;
                    rect.Height = (int)size.Height;
                    if (rect.Contains(loc))
                    {
                        clickAdj = new Point(temp.hPosition.X - loc.X, temp.hPosition.Y - loc.Y);
                        clickStart = loc;
                        mouseIsDown = true;
                        selectedNode = temp;
                        textBox1.Text = selectedNode.hAssetPath;
                        propertyGrid1.SelectedObject = selectedNode;

                        UndoCashing("./temp.tmp");
                        button8.Enabled = true;
                        return;
                    }
                }
            }
        }
        public void Controll_MouseMove(object sender, MouseEventArgs e)
        {
            if (!mouseIsDown)
                return;

            //Point ePos = e.Location;
            Point loc = e.Location;
            loc.X = loc.X * 100 / CanvasScaleInt;
            loc.Y = loc.Y * 100 / CanvasScaleInt;

            if (ModifierKeys == Keys.Shift)
            {
                float _adjX = Math.Abs(clickStart.X - loc.X);
                float _adjY = Math.Abs(clickStart.Y - loc.Y);

                if (_adjX > _adjY)
                    loc.Y = clickStart.Y;
                if (_adjX < _adjY)
                    loc.X = clickStart.X;
            }

            if (selectedNode != null)
            {
                //Console.WriteLine("{0} {1}", loc.ToString(), clickAdj.ToString());
                selectedNode.hPosition = new Point(loc.X + clickAdj.X, loc.Y + clickAdj.Y);//new Point((loc.X + clickAdj.X) * CanvasScaleInt / 100, (loc.Y + clickAdj.Y) * CanvasScaleInt / 100);
                //selectedNode.hOffset = new Anchor(((selectedNode.hPosition.X - CanvasWidth * 0.5f) / (float)CanvasWidth), ((selectedNode.hPosition.Y - CanvasHeight * 0.5f) / (float)CanvasHeight));
                //selectedNode.hDocking = new NodeRect(
                //    selectedNode.hPosition.X,
                //    selectedNode.hPosition.Y,
                //    (CanvasWidth - selectedNode.hPosition.X),
                //    (CanvasHeight - selectedNode.hPosition.Y)
                //    );
                selectedNode.hOffsetX = (selectedNode.hPosition.X - CanvasWidth * 0.5f) / (float)CanvasWidth;
                selectedNode.hOffsetY = (selectedNode.hPosition.Y - CanvasHeight * 0.5f) / (float)CanvasHeight;
                selectedNode.hLeft = selectedNode.hPosition.X;
                selectedNode.hTop = selectedNode.hPosition.Y;
                selectedNode.hRight = (CanvasWidth - selectedNode.hPosition.X);
                selectedNode.hBottom = (CanvasHeight - selectedNode.hPosition.Y);

                //label2.Text = string.Format("Vec2 {0}, {1}", selectedNode.hPosition.X, selectedNode.hPosition.Y);
                //label5.Text = string.Format("Offset {0}, {1}", selectedNode.hOffset.X, selectedNode.hOffset.Y);
                //checkBox1.Text = selectedNode.hDocking.top.ToString();
                //checkBox2.Text = selectedNode.hDocking.left.ToString();
                //checkBox3.Text = selectedNode.hDocking.right.ToString();
                //checkBox4.Text = selectedNode.hDocking.bottom.ToString();
            }
        }
        public void Controll_MouseUp(object sender, MouseEventArgs e)
        {
            mouseIsDown = false;
            propertyGrid1.SelectedObject = selectedNode;

            if (selectedNode != null)
            {
                //selectedNode.hOffset = new Anchor(((selectedNode.hPosition.X - CanvasWidth * 0.5f) / (float)CanvasWidth), ((selectedNode.hPosition.Y - CanvasHeight * 0.5f) / (float)CanvasHeight));
                //selectedNode.hDocking = new NodeRect(
                //    selectedNode.hPosition.X,
                //    selectedNode.hPosition.Y,
                //    (CanvasWidth - selectedNode.hPosition.X),
                //    (CanvasHeight - selectedNode.hPosition.Y)
                //    );
                selectedNode.hOffsetX = (selectedNode.hPosition.X - CanvasWidth * 0.5f) / (float)CanvasWidth;
                selectedNode.hOffsetY = (selectedNode.hPosition.Y - CanvasHeight * 0.5f) / (float)CanvasHeight;
                selectedNode.hLeft = selectedNode.hPosition.X;
                selectedNode.hTop = selectedNode.hPosition.Y;
                selectedNode.hRight = (CanvasWidth - selectedNode.hPosition.X);
                selectedNode.hBottom = (CanvasHeight - selectedNode.hPosition.Y);
                propertyGrid1.Refresh();

                label2.Text = string.Format("Vec2 {0}, {1}", selectedNode.hPosition.X, selectedNode.hPosition.Y);
                label5.Text = string.Format("Offset {0:F3}, {1:F3}", selectedNode.hOffsetX, selectedNode.hOffsetY);
                checkBox2.Text = selectedNode.hTop.ToString();
                checkBox1.Text = selectedNode.hLeft.ToString();
                checkBox3.Text = selectedNode.hRight.ToString();
                checkBox4.Text = selectedNode.hBottom.ToString();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
        }

        private void button7_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            //to my game
            switch (donateLink)
            {
                case 0:
                    System.Diagnostics.Process.Start("https://play.google.com/store/apps/details?id=com.mrgames.oxfighter");
                    break;
                case 1:
                    System.Diagnostics.Process.Start("http://onestore.co.kr/userpoc/game/view?pid=0000684721");
                    break;
                case 2:
                    System.Diagnostics.Process.Start("https://play.google.com/store/apps/details?id=com.mrgames.battlesiegewarfare");
                    break;
                case 3:
                    System.Diagnostics.Process.Start("https://play.google.com/store/apps/details?id=com.ggame.radishdx");
                    break;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (selectedNode == null)
                return;
            selectedNode.hPosmode = PosKind.Vec2;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (selectedNode == null)
                return;
            selectedNode.hPosmode = PosKind.Offset;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (selectedNode == null)
                return;
            selectedNode.hPosmode = PosKind.Docking;
        }

        bool locker = false;

        private void button5_Click(object sender, EventArgs e)
        {
            //if(selectedNode==null)
            //{
            //    MessageBox.Show("NOtice", "Not Node selected", MessageBoxButtons.OK);
            //    return;
            //}
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Asset";
            ofd.Filter = "Image Files(*.jpg;*.png)|*.jpg;*.png";

            DialogResult dr = ofd.ShowDialog();

            if (dr == DialogResult.OK && ofd != null)
            {
                if (selectedNode == null)
                {
                    string filename = ofd.SafeFileName;
                    string fileFullName = ofd.FileName;
                    string filePath = fileFullName.Replace(filename, "");
                    int aa = fileFullName.LastIndexOf("Resources\\");
                    string filePathName = fileFullName.Substring(aa + 10, fileFullName.Length - aa - 10);
                    mainPath = filePath;

                    CocosNode node = new CocosNode(fileFullName);
                    //node.setParent(this);
                    node.hPosition = new Point(CanvasWidth / 2, CanvasHeight / 2);
                    node.hSafeFilename = filePathName;// filename;
                    cocosNodes.Add(node);

                    selectedNode = node;
                    textBox1.Text = selectedNode.hAssetPath;
                    propertyGrid1.SelectedObject = selectedNode;
                    //nodeProperties();

                    isChanged = true;
                }
                else
                {
                    //selectedNode.renewBitmap(ofd.FileName);
                    selectedNode.hAssetPath = ofd.FileName;
                    textBox1.Text = selectedNode.hAssetPath;
                    isChanged = true;
                }
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 prompt = new Form2(ProjectName, CanvasWidth, CanvasHeight);
            DialogResult dr = prompt.ShowDialog();

            if (dr == DialogResult.OK && prompt != null)
            {
                cocosNodes.Clear();
                ProjectName = prompt.passName;
                CanvasWidth = Int32.Parse(prompt.passWidth);
                CanvasHeight = Int32.Parse(prompt.passHeight);
                Form1_CreateBackBuffer(prompt, EventArgs.Empty);
                label3.Text = ProjectName;
            }
        }
        private Hashtable encodeHash()
        {
            Hashtable data = new Hashtable();

            data.Add("prName", ProjectName);
            data.Add("prWidth", CanvasWidth);
            data.Add("prHeight", CanvasHeight);
            data.Add("prScale", CanvasScale);
            data.Add("prCross", showCross);
            data.Add("prBorder", showBorder);
            data.Add("prAllBorder", showAllBorder);
            data.Add("mainPath", mainPath);

            int idx = 0;
            foreach (CocosNode node in cocosNodes)
            {
                data.Add(string.Format("Node{0}", idx++), node);
            }

            return data;
        }
        private void decodeHash(Hashtable data)
        {
            ProjectName = (string)data["prName"];
            CanvasWidth = (int)data["prWidth"];
            CanvasHeight = (int)data["prHeight"];
            CanvasScale = (float)data["prScale"];
            showCross = (bool)data["prCross"];
            showBorder = (bool)data["prBorder"];
            showAllBorder = (bool)data["prAllBorder"];
            mainPath = (string)data["mainPath"];

            int idx = 0;
            bool roof = true;
            cocosNodes.Clear();
            while (roof)
            {
                string key = string.Format("Node{0}", idx++);
                if (data.Contains(key))
                {
                    CocosNode node = (CocosNode)data[key];
                    //if (node.nKind == NodeKind.Scale9Sprite)
                    //    node.bitmapInsect();
                    cocosNodes.Add(node);
                }
                else
                    roof = false;
            }
        }
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isChanged)
            {
                string msg = string.Format("CAUTION!!\n\n{0} is not Saved", fileName);
                if (fileName.Length == 0)
                    msg = string.Format("CAUTION!!\n\n{0} is not Saved", ProjectName);
                var window = MessageBox.Show(msg, "Save or Discard? (Yes-save)",
                    MessageBoxButtons.YesNoCancel);
                switch (window)
                {
                    case DialogResult.Yes:
                        SaveAlert();
                        System.Windows.Forms.Application.ExitThread();
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        return;
                }
            }

            //Load
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load Project";
            ofd.Filter = "GUI files(*.gui)|*.gui";

            Hashtable readData = null;

            DialogResult dr = ofd.ShowDialog();

            if (dr == DialogResult.OK && ofd != null)
            {
                Stream ofds = ofd.OpenFile();
                try
                {
                    string _filename = ofd.SafeFileName;
                    if (_filename.Length == 0)
                        return;
                    string _fileFullName = ofd.FileName;
                    string _filePath = _fileFullName.Replace(_filename, "");
                    fileName = _filename;

                    BinaryFormatter bf = new BinaryFormatter();
                    readData = (Hashtable)bf.Deserialize(ofds);

                    decodeHash(readData);

                    if (mainPath == null)
                        mainPath = _filePath;

                    //데이타에 저장된 패스와 실제 읽어온 패스가 다른 경우 컨버트 시도
                    if (mainPath != _filePath)
                    {
                        int idx = mainPath.LastIndexOf("Resources\\");
                        if (idx != -1)
                        {
                            string basePath = mainPath.Substring(0, idx + 10);
                            string msg = string.Format("Do you want to convert the default folder for all resources to {0} ?", basePath);
                            var window = MessageBox.Show(msg, "Convert Path",
                                MessageBoxButtons.YesNo);
                            switch (window)
                            {
                                case DialogResult.Yes:
                                    foreach (CocosNode node in cocosNodes)
                                    {
                                        //base
                                        string _AssetPath = node.hAssetPath.Replace(getRootPath(node.hAssetPath), basePath);
                                        if (_AssetPath != null)
                                            node.hAssetPath = _AssetPath;
                                        //selected
                                        if (node.hpressAsset.Length > 0)
                                        {
                                            string _SelectedPath = node.hpressAsset.Replace(getRootPath(node.hpressAsset), basePath);
                                            if (_SelectedPath != null)
                                                node.hpressAsset = _SelectedPath;
                                        }
                                        //disabled
                                        if (node.hdisableAsset.Length > 0)
                                        {
                                            string _DisabledPath = node.hdisableAsset.Replace(getRootPath(node.hdisableAsset), basePath);
                                            if (_DisabledPath != null)
                                                node.hdisableAsset = _DisabledPath;
                                        }
                                        //checked
                                        if (node.hcheckedAsset.Length > 0)
                                        {
                                            string _CheckedPath = node.hcheckedAsset.Replace(getRootPath(node.hcheckedAsset), basePath);
                                            if (_CheckedPath != null)
                                                node.hcheckedAsset = _CheckedPath;
                                        }
                                        //checked disabled
                                        if (node.hcheckedDisableAsset.Length > 0)
                                        {
                                            string _DCheckedPath = node.hcheckedDisableAsset.Replace(getRootPath(node.hcheckedDisableAsset), basePath);
                                            if (_DCheckedPath != null)
                                                node.hcheckedDisableAsset = _DCheckedPath;
                                        }
                                    }
                                    break;
                                case DialogResult.No:
                                    break;
                            }
                        }
                    }

                }
                catch (SerializationException ex)
                {
                    Console.WriteLine("" + ex.Message);
                    throw;
                }
                finally
                {
                    ofds.Close();
                }

                label3.Text = ProjectName;
                numericUpDown1.Value = (decimal)(CanvasScale * 100);
                checkBox5.Checked = showBorder;
                checkBox6.Checked = showCross;
                checkBox7.Checked = showAllBorder;
                Form1_CreateBackBuffer(this, EventArgs.Empty);
                propertyGrid1.SelectedObject = selectedNode;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Save As
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save Project";
            sfd.FileName = ProjectName;
            sfd.Filter = "GUI files(*.gui)|*.gui";
            sfd.ShowDialog();

            Hashtable data = encodeHash();

            if (sfd.FileName != "" && sfd != null)
            {
                //string _filename = sfd.SafeFileName;
                string _fileFullName = sfd.FileName;
                string _filePath = _fileFullName.Replace(mainPath, "");
                //mainPath = _filePath;
                fileName = _filePath;

                BinaryFormatter bf = new BinaryFormatter();
                try
                {
                    Stream sfds = sfd.OpenFile();
                    StreamWriter ws = new StreamWriter(sfds);
                    bf.Serialize(ws.BaseStream, data);
                    isChanged = false;
                    ws.BaseStream.Close();
                    sfds.Close();
                    //ws.Close();
                }
                catch (SerializationException ex)
                {
                    Console.WriteLine("" + ex.Message);
                    throw;
                }
                finally
                {
                }
            }
        }
        private void UndoCashing(string path)
        {
            Hashtable data = encodeHash();

            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(path, FileMode.Create);
            try
            {
                bf.Serialize(fs, data);
                isChanged = true;
            }
            catch (SerializationException ex)
            {
                Console.WriteLine("" + ex.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }
        private void UndoLoading(string path)
        {
            Hashtable readData = null;

            FileStream fs = new FileStream(path, FileMode.Open);
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                readData = (Hashtable)bf.Deserialize(fs);

                decodeHash(readData);
            }
            catch (SerializationException ex)
            {
                Console.WriteLine("" + ex.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }

            label3.Text = ProjectName;
            numericUpDown1.Value = (decimal)(CanvasScale * 100);
            checkBox5.Checked = showBorder;
            checkBox6.Checked = showCross;
            checkBox7.Checked = showAllBorder;
            Form1_CreateBackBuffer(this, EventArgs.Empty);
            propertyGrid1.SelectedObject = selectedNode;

        }

        private void cCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //export1

            string[] prefixes =
            {
                    "spr_",
                    "menu_",
                    "btn_",
                    "label_",
                    "bmlabel_",
                    "loading_",
                    "checkbox_",
                    "slider_",
                    "s9spr_",
                    /*
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
                     */
                };

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save Project";
            sfd.FileName = "";
            sfd.Filter = "cpp files(*.cpp)|*.cpp";
            //sfd.RestoreDirectory = true;
            sfd.ShowDialog();

            List<string> headerbuffers = new List<string>();
            List<string> plistbuffers = new List<string>();
            List<string> createbuffers = new List<string>();

            if (sfd.FileName != "" && sfd != null)
            {
                //Header
                //headerbuffers.Add("# ifndef __{}_H__");
                //headerbuffers.Add("#define __{}_H__");

                //headerbuffers.Add("# include \"cocos2d.h\"");
                //headerbuffers.Add("# include \"Box2D\\Box2D.h\"");


                //headerbuffers.Add("using namespace std;");
                //headerbuffers.Add("using namespace cocos2d;");


                //headerbuffers.Add("class GameLayer : public cocos2d::Layer, public b2ContactListener");
                //headerbuffers.Add("{");
                //headerbuffers.Add("");
                //headerbuffers.Add("}");


                //cpp top
                //plistbuffers.Add(string.Format("#include \"{0}.h\";", ProjectName));
                plistbuffers.Add("");
                plistbuffers.Add("#define ScreenWidth " + CanvasWidth.ToString());
                plistbuffers.Add("#define ScreenHeight " + CanvasHeight.ToString());
                plistbuffers.Add("");
                plistbuffers.Add("");

                int idx = 0;
                foreach (CocosNode node in cocosNodes)
                {

                    string nodeName = node.hName;// prefixes[(int)node.nKind];
                    if (node.hName == "")
                    {
                        nodeName = getExpRemoved(getRemoveAllPath(node.hSafeFilename));//루트폴더와 확장자가 제거된 이름
                    }

                    string createName = prefixes[(int)node.nKind] + nodeName;

                    //plist
                    switch (node.nKind)
                    {
                        case NodeKind.Sprite:
                            if (node.hRscFrom == RSC.PLIST)
                            {
                                plistbuffers.Add(string.Format("auto frame_{0} = SpriteFrame::create({1}, Rect(0, 0, {2}, {3}));", nodeName, node.hSafeFilename, node.image.Width, node.image.Height));
                                plistbuffers.Add(string.Format("SpriteFrameCache::getInstance()->addSpriteFrame(frame_{0}, \"{1}\");", nodeName, getRemoveAllPath(node.hSafeFilename)));

                                createbuffers.Add(string.Format("auto {0} = Sprite::createWithSpriteFrameName(\"{1}\");", createName, getRemoveAllPath(node.hSafeFilename)));
                            }
                            else
                                createbuffers.Add(string.Format("auto {0} = Sprite::create(\"{1}\");", createName, node.hSafeFilename));
                            break;
                        case NodeKind.Button:
                            {
                                List<string> buttons = new List<string>();
                                buttons = getButtons(node);

                                if (node.hRscFrom == RSC.Local)
                                    createbuffers.Add(string.Format("auto {0} = Button::create(\"{1}\", \"{2}\", \"{3}\", Widget::TextureResType::LOCAL);", createName, buttons.ElementAt(0), buttons.ElementAt(1), buttons.ElementAt(2)));
                                else
                                {
                                    foreach (string subrsc in buttons)
                                    {
                                        plistbuffers.Add(string.Format("auto frame_{0} = SpriteFrame::create({1}, Rect(0, 0, {2}, {3}));", nodeName, subrsc, node.image.Width, node.image.Height));
                                        plistbuffers.Add(string.Format("SpriteFrameCache::getInstance()->addSpriteFrame(frame_{0}, \"{1}\");", nodeName, getRemoveAllPath(subrsc)));

                                    }
                                    createbuffers.Add(string.Format("auto {0} = Button::create(\"{1}\", \"{2}\", \"{3}\", Widget::TextureResType::PLIST);", createName, buttons.ElementAt(0), buttons.ElementAt(1), buttons.ElementAt(2)));
                                }

                                //button title text
                                if (node.lText != null && node.lText != "")
                                {
                                    if(node.lColor!=null)
                                    {
                                        var argbarray = BitConverter.GetBytes(node.lColor.ToArgb()).Reverse().ToArray();
                                        createbuffers.Add(string.Format("{0}->setTitleColor(Color3B({1}, {2}, {3}));",
                                            createName,
                                            argbarray[1],
                                            argbarray[2],
                                            argbarray[3]
                                            ));
                                    }
                                    createbuffers.Add(string.Format("{0}->setTitleFontSize({1});", createName, node.lFontSize));
                                    createbuffers.Add(string.Format("{0}->setTitleText(\"{1}\");", createName, node.lText));
                                    if (node.lFont != null && node.lFont != "")
                                    {
                                        createbuffers.Add(string.Format("{0}->setTitleFontName(\"{1}\");", createName, node.lFont));
                                    }

                                }

                                createbuffers.Add(string.Format("{0}->setSwallowTouches(true);", createName));//swallow
                                //callback
                                createbuffers.Add(string.Format("{0}->addTouchEventListener([=](Ref* sender, Widget::TouchEventType eventtype)", createName));
                                createbuffers.Add("{");
                                createbuffers.Add("switch(eventtype)");
                                createbuffers.Add("{");
                                createbuffers.Add("case Widget::TouchEventType::BEGAN:");
                                createbuffers.Add("break;");
                                createbuffers.Add("case Widget::TouchEventType::MOVED:");
                                createbuffers.Add("break;");
                                createbuffers.Add("case Widget::TouchEventType::ENDED:");
                                createbuffers.Add("break;");
                                createbuffers.Add("}");
                                createbuffers.Add("});");

                            }
                            break;
                        case NodeKind.Menu:
                            {
                                string menuname = createName + "item";
                                List<string> buttons = new List<string>();
                                buttons = getButtons(node);
                                if (buttons.ElementAt(0) == buttons.ElementAt(1))
                                {
                                    //MenuItemFont
                                }
                                else
                                {
                                    //MenuItemImage
                                    if (node.hRscFrom == RSC.Local)
                                    {
                                        createbuffers.Add(string.Format("auto {0} = MenuItemImage::create(\"{1}\", \"{2}\", \"{3}\", nullptr);", menuname, buttons.ElementAt(0), buttons.ElementAt(1), buttons.ElementAt(2)));
                                    }
                                    else
                                    {
                                        foreach (string subrsc in buttons)
                                        {
                                            plistbuffers.Add(string.Format("auto frame_{0} = SpriteFrame::create({1}, Rect(0, 0, {2}, {3}));", nodeName, subrsc, node.image.Width, node.image.Height));
                                            plistbuffers.Add(string.Format("SpriteFrameCache::getInstance()->addSpriteFrame(frame_{0}, \"{1}\");", nodeName, getRemoveAllPath(subrsc)));

                                        }
                                        createbuffers.Add(string.Format("auto {0} = MenuItemImage::create();", menuname));
                                        createbuffers.Add(string.Format("{0}->setNormalSpriteFrame(SpriteFrameCache::getInstance()->getSpriteFrameByName(\"{1}\"));",
                                            menuname,
                                            buttons.ElementAt(0)
                                            ));
                                        createbuffers.Add(string.Format("{0}->setSelectedSpriteFrame(SpriteFrameCache::getInstance()->getSpriteFrameByName(\"{1}\"));",
                                            menuname,
                                            buttons.ElementAt(1)
                                            ));
                                        createbuffers.Add(string.Format("{0}->setDisabledSpriteFrame(SpriteFrameCache::getInstance()->getSpriteFrameByName(\"{1}\"));",
                                            menuname,
                                            buttons.ElementAt(2)
                                            ));
                                    }
                                }
                                //menu common
                                createbuffers.Add(string.Format("auto {0} = Menu::create({1}, NULL);", createName, menuname));

                                //menu callback
                                createbuffers.Add(string.Format("{0}->initWithCallback([=](Ref* sender) {{", menuname));
                                createbuffers.Add("MenuItem* menu = dynamic_cast<MenuItem*>(sender);");
                                createbuffers.Add("});");
                            }
                            break;
                        case NodeKind.CheckBox:
                            {
                                List<string> buttons = new List<string>();
                                buttons = getButtons(node);
                                buttons.Add(getProjectPathRemoved(node.hcheckedAsset));
                                buttons.Add(getProjectPathRemoved(node.hcheckedDisableAsset));

                                createbuffers.Add(string.Format("auto checkbox_{0} = CheckBox::create(\"{1}\",", nodeName, buttons.ElementAt(0)));//normal
                                createbuffers.Add(string.Format("\"{0}\",", buttons.ElementAt(1)));//press
                                createbuffers.Add(string.Format("\"{0}\",", buttons.ElementAt(2)));//disabled
                                createbuffers.Add(string.Format("\"{0}\",", buttons.ElementAt(3)));//checked
                                createbuffers.Add(string.Format("\"{0}\",", buttons.ElementAt(4)));//checked disabled

                                if (node.hRscFrom == RSC.Local)
                                {
                                    createbuffers.Add("Widget::TextureResType::LOCAL);");//from plist
                                }
                                else
                                {
                                    foreach (string subrsc in buttons)
                                    {
                                        plistbuffers.Add(string.Format("auto frame_{0} = SpriteFrame::create({1}, Rect(0, 0, {2}, {3}));", nodeName, subrsc, node.image.Width, node.image.Height));
                                        plistbuffers.Add(string.Format("SpriteFrameCache::getInstance()->addSpriteFrame(frame_{0}, \"{1}\");", nodeName, getRemoveAllPath(subrsc)));

                                    }
                                    createbuffers.Add("Widget::TextureResType::PLIST);");//from plist
                                }

                                if (node.hbaseCheck)
                                    createbuffers.Add(string.Format("{0}->setSelected(true);", createName));
                                    
                                //callback
                                createbuffers.Add(string.Format("{0}->setSwallowTouches(true);", createName));//swallow
                                createbuffers.Add(string.Format("{0}->addTouchEventListener([=](Ref* sender, Widget::TouchEventType eventtype)", createName));
                                createbuffers.Add("{");
                                createbuffers.Add("switch(eventtype)");
                                createbuffers.Add("{");
                                createbuffers.Add("case Widget::TouchEventType::BEGAN:");
                                createbuffers.Add("break;");
                                createbuffers.Add("case Widget::TouchEventType::MOVED:");
                                createbuffers.Add("break;");
                                createbuffers.Add("case Widget::TouchEventType::ENDED:");
                                createbuffers.Add("break;");
                                createbuffers.Add("}");
                                createbuffers.Add("});");

                            }
                            break;
                        case NodeKind.LoadingBar:
                            {
                                if (node.hRscFrom == RSC.Local)
                                {
                                    createbuffers.Add(string.Format("auto {0} = LoadingBar::create(\"{1}\", Widget::TextureResType::LOCAL);",
                                        createName,
                                        node.hSafeFilename
                                        ));
                                }
                                else
                                {
                                    plistbuffers.Add(string.Format("auto frame_{0}_base = SpriteFrame::create({1}, Rect(0, 0, {2}, {3}));", nodeName, node.hSafeFilename, node.image.Width, node.image.Height));
                                    plistbuffers.Add(string.Format("SpriteFrameCache::getInstance()->addSpriteFrame(frame_{0}_base, \"{1}\");", nodeName, node.hSafeFilename));
                                    createbuffers.Add(string.Format("auto {0} = LoadingBar::create(\"{1}\", Widget::TextureResType::PLIST);",
                                        createName,
                                        node.hSafeFilename
                                        ));
                                }
                                createbuffers.Add(string.Format("{0}->setPercent({1});", createName, node.hbarPercent));
                                if(node.hbarDir== Direction.RIGHT)
                                    createbuffers.Add(string.Format("{0}->setDirection(LoadingBar::Direction::RIGHT);", nodeName));
                            }
                            break;
                        case NodeKind.Slider:
                            {
                                string TextureResType = "Widget::TextureResType::LOCAL";
                                if (node.hRscFrom == RSC.PLIST)
                                    TextureResType = "Widget::TextureResType::PLIST";
                                createbuffers.Add(string.Format("auto {0} = Slider::create();", createName));
                                createbuffers.Add(string.Format("{0}->loadBarTexture(\"{1}\", {2});",
                                    createName,
                                    node.hRscFrom == RSC.Local ? node.hSafeFilename: getRemoveAllPath(node.hAssetPath),
                                    TextureResType));
                                createbuffers.Add(string.Format("{0}->loadSlidBallTextures(\"{1}\", \"{2}\", \"{3}\", {4});",
                                    createName,
                                    node.hRscFrom == RSC.Local ? getProjectPathRemoved(node.hballAsset) : getRemoveAllPath(node.hballAsset),
                                    node.hRscFrom == RSC.Local ? getProjectPathRemoved(node.hpressAsset) : getRemoveAllPath(node.hpressAsset),
                                    node.hRscFrom == RSC.Local ? getProjectPathRemoved(node.hdisableAsset) : getRemoveAllPath(node.hdisableAsset),
                                    TextureResType
                                    ));
                                createbuffers.Add(string.Format("{0}->loadProgressBarTexture(\"{1}\", {2});",
                                    createName,
                                    node.hRscFrom == RSC.Local ? getProjectPathRemoved(node.hbarAsset) : getRemoveAllPath(node.hbarAsset),
                                    TextureResType
                                    ));

                                if (node.hRscFrom == RSC.PLIST)
                                {
                                    //frame regist base-bar-ball
                                    plistbuffers.Add(string.Format("auto frame_{0}_base = SpriteFrame::create({1}, Rect(0, 0, {2}, {3}));", nodeName, node.hSafeFilename, node.image.Width, node.image.Height));
                                    plistbuffers.Add(string.Format("SpriteFrameCache::getInstance()->addSpriteFrame(frame_{0}_base, \"{1}\");", nodeName, node.hSafeFilename));
                                    plistbuffers.Add(string.Format("auto frame_{0}_bar = SpriteFrame::create({1}, Rect(0, 0, {2}, {3}));", nodeName, getProjectPathRemoved(node.hbarAsset), node.barImage.Width, node.barImage.Height));
                                    plistbuffers.Add(string.Format("SpriteFrameCache::getInstance()->addSpriteFrame(frame_{0}_bar, \"{1}\");", nodeName, getRemoveAllPath(node.hbarAsset)));
                                    plistbuffers.Add(string.Format("auto frame_{0}_sliderball = SpriteFrame::create({1}, Rect(0, 0, {2}, {3}));", nodeName, getProjectPathRemoved(node.hballAsset), node.ballImage.Width, node.ballImage.Height));
                                    plistbuffers.Add(string.Format("SpriteFrameCache::getInstance()->addSpriteFrame(frame_{0}_ball, \"{1}\");", nodeName, getRemoveAllPath(node.hballAsset)));
                                }

                                //if (node.hbarDir == Direction.Vertical)
                                //    createbuffers.Add(string.Format("{0}->setRotate(90);", nodeName));

                                createbuffers.Add(string.Format("{0}->setPercent({1});", createName, node.hbarPercent));
                                createbuffers.Add(string.Format("{0}->setSwallowTouches(true);", createName));//swallow
                                //callback
                                createbuffers.Add(string.Format("{0}->addTouchEventListener([=](Ref* sender, Widget::TouchEventType eventtype)", createName));
                                createbuffers.Add("{");
                                createbuffers.Add("switch(eventtype)");
                                createbuffers.Add("{");
                                createbuffers.Add("case Widget::TouchEventType::BEGAN:");
                                createbuffers.Add("break;");
                                createbuffers.Add("case Widget::TouchEventType::MOVED:");
                                createbuffers.Add("break;");
                                createbuffers.Add("case Widget::TouchEventType::ENDED:");
                                createbuffers.Add("break;");
                                createbuffers.Add("}");
                                createbuffers.Add("});");
                            }
                            break;
                        case NodeKind.Scale9Sprite:
                            if (node.hRscFrom == RSC.PLIST)
                            {
                                int aa = node.hSafeFilename.LastIndexOf("\\");
                                string fname = node.hSafeFilename.Substring(aa + 1, node.hSafeFilename.Length - aa - 1);
                                plistbuffers.Add(string.Format("auto frame_{0} = SpriteFrame::create({1}, Rect(0, 0, {2}, {3}));", nodeName, node.hSafeFilename, node.image.Width, node.image.Height));
                                plistbuffers.Add(string.Format("SpriteFrameCache::getInstance()->addSpriteFrame(frame_{0}, \"{1}\");", nodeName, fname));

                                createbuffers.Add(string.Format("auto {0} = Scale9Sprite::createWithSpriteFrameName(\"{1}\");", createName, fname));
                            }
                            else
                                createbuffers.Add(string.Format("auto {0} = Scale9Sprite::create(\"{1}\");", createName, node.hSafeFilename));
                            createbuffers.Add(string.Format("{0}->setCapInsets(Rect({1}, {2}, {3}, {4}));", createName, node.hCapInsets.X, node.hCapInsets.Y, node.hCapInsets.Width, node.hCapInsets.Height));
                            createbuffers.Add(string.Format("{0}->setContentSize(Size({1}, {2}));", createName, node.hContentSize.Width, node.hContentSize.Height));
                            break;
                        case NodeKind.Label:
                            {
                                if (node.hName != "")
                                    createName = string.Format("label_{0}", node.hName);
                                else
                                    createName = string.Format("label_{0}", idx);
                                string sizestr = "Size::ZERO";
                                string fontCreator = "createWithTTF";
                                if (node.lFont == "" || node.lFont == null)
                                    fontCreator = "createWithSystemFont";
                                //if (node.hContentSize.Width > 0 && node.hContentSize.Height > 0)
                                //    sizestr =string.Format("Size({0}, {1})", node.hContentSize.Width, node.hContentSize.Height);
                                createbuffers.Add(string.Format("auto {0} = Label::{4}(\"{1}\", \"{2}.ttf\", {5}, {3}, TextHAlignment::CENTER, TextVAlignment::CENTER);",
                                    createName,
                                    node.lText,
                                    node.lFont,
                                    sizestr,
                                    fontCreator,
                                    node.lFontSize
                                    ));

                                //label common
                                if (node.lColor != null)
                                {
                                    var argbarray = BitConverter.GetBytes(node.lColor.ToArgb()).Reverse().ToArray();
                                    createbuffers.Add(string.Format("{0}->setTextColor(Color4B({1}, {2}, {3}, {4}));",
                                        createName,
                                        argbarray[1],
                                        argbarray[2],
                                        argbarray[3],
                                        argbarray[0]
                                        ));
                                }
                            }
                            break;
                        case NodeKind.BMFLabel:
                            {
                                if (node.hName != "")
                                    createName = string.Format("label_{0}", node.hName);
                                else
                                    createName = string.Format("label_{0}", idx);
                                createbuffers.Add(string.Format("auto {0} = Label::createWithBMFont(\"{1}\", \"{2}\", TextHAlignment::CENTER, 0, Vec2::ZERO);",
                                    createName,
                                    node.lFont,
                                    node.lText
                                    ));

                                //bmfont common
                                createbuffers.Add(string.Format("{0}->setBMFontSize({1});", createName, node.lFontSize));
                                if (node.lColor != null)
                                {
                                    var argbarray = BitConverter.GetBytes(node.lColor.ToArgb()).Reverse().ToArray();
                                    createbuffers.Add(string.Format("{0}->setColor(Color3B({1}, {2}, {3}));",
                                        createName,
                                        argbarray[1],
                                        argbarray[2],
                                        argbarray[3]
                                        ));
                                }
                            }
                            break;
                    }
                    //plistbuffers.Add("");
                    //createbuffers.Add("");

                    //Common
                    string _xx = node.hPosition.X.ToString();
                    string _yy = (CanvasHeight - node.hPosition.Y).ToString();
                    switch (node.hPosmode)
                    {
                        case PosKind.Vec2:
                            createbuffers.Add(string.Format("{0}->setPosition(Vec2({1}, {2}));", createName, _xx, _yy));
                            break;
                        case PosKind.Offset:
                            _xx = string.Format("ScreenWidth * ({0} - 0.5f)", node.hOffsetX);
                            _yy = string.Format("ScreenHeight * ({0} - 0.5f)", node.hOffsetY);
                            createbuffers.Add(string.Format("{0}->setNormalizedPosition(Vec2({1}, {2}));", createName, _xx, _yy));
                            break;
                        case PosKind.Docking:
                            if (checkBox3.Checked)
                                _xx = string.Format("ScreenWidth - {0}", node.hRight);
                            else
                                _xx = string.Format("{0}", node.hLeft);
                            if (checkBox2.Checked)
                                _yy = string.Format("ScreenHeight - {0}", node.hTop);
                            else
                                _yy = string.Format("{0}", node.hBottom);
                            createbuffers.Add(string.Format("{0}->setPosition(Vec2({1}, {2}));", createName, _xx, _yy));
                            break;
                    }
                    if(node.hAnchorX != 0.5f || node.hAnchorY != 0.5f)
                        createbuffers.Add(string.Format("{0}->setAnchorPoint(Vec2({1}, {2}));", createName, node.hAnchorX, node.hAnchorY));
                    if (node.hOpacity < 255)
                        createbuffers.Add(string.Format("{0}->setOpacity({1});", createName, node.hOpacity));
                    if (node.hScale != 1.0f)
                        createbuffers.Add(string.Format("{0}->setScale({1});", createName, node.hScale));
                    if (node.hTag != 0)
                        createbuffers.Add(string.Format("{0}->setTag({1});", createName, node.hTag));
                    if (node.hName.Length > 0)
                        createbuffers.Add(string.Format("{0}->setName({1});", createName, node.hName));
                    if(node.hFlipX)
                        createbuffers.Add(string.Format("{0}->setFlippedX(true);", createName));
                    if (node.hFlipY)
                        createbuffers.Add(string.Format("{0}->setFlippedY(true);", createName));

                    if (node.hZorder != 0)
                        createbuffers.Add(string.Format("addChild({0}. {1});", createName, node.hZorder));
                    else
                        createbuffers.Add(string.Format("addChild({0});", createName));
                    createbuffers.Add("");

                    idx++;
                }


                //node declare and creation
                //foreach (CocosNode node in cocosNodes)
                //{
                //    string nodeName = prefixes[(int)node.nKind];
                //    if (node.hName == "")
                //    {
                //        int aa = node.hSafeFilename.LastIndexOf("\\");
                //        string _name = node.hSafeFilename.Substring(aa + 1, node.hSafeFilename.Length - aa - 1);
                //        if (_name.LastIndexOf(".png") != -1)
                //            nodeName += _name.Replace(".png", "");
                //        else if (_name.LastIndexOf(".jpg") != -1)
                //            nodeName += _name.Replace(".jpg", "");
                //        else
                //            nodeName += _name;
                //    }

                //}

                //write
                string exportname = sfd.FileName;
                TextWriter tw = new StreamWriter(exportname);
                foreach (string s in headerbuffers)
                {
                    string _s = s.Replace("\\", "/");
                    tw.WriteLine(_s);
                }
                foreach (string s in plistbuffers)
                {
                    string _s = s.Replace("\\", "/");
                    tw.WriteLine(_s);
                }
                foreach (string s in createbuffers)
                {
                    string _s = s.Replace("\\", "/");
                    tw.WriteLine(_s);
                }

                tw.Close();

                //Stream sfds = sfd.OpenFile();
                //System.IO.StreamWriter file = new System.IO.StreamWriter(sfds);
                //file.Close();
                //sfds.Close();
                MessageBox.Show(string.Format("Complete export {0}", exportname));

                sfd.Dispose();
            }

            //    //file.WriteLine(string.Format("#include \"{0}.h\";", fi.Name.Replace(".cpp", "")));

            //    file.WriteLine("{");
            //    file.WriteLine("\t//");
            //    file.WriteLine(string.Format("\t//\t{0} cpp Code", fi.Name.Replace(".cpp", "")));
            //    file.WriteLine("\t//");
            //    file.WriteLine("\t//\tGenerated by D.D.Jerry.Do's cocos-GUI-Editor");
            //    file.WriteLine("\t//");
            //    file.WriteLine("\t//");
            //    file.WriteLine("\t//");
            //    file.WriteLine("");
            //    file.WriteLine("#define ScreenWidth " + CanvasWidth.ToString());
            //    file.WriteLine("#define ScreenHeight " + CanvasHeight.ToString());
            //    file.WriteLine("");

            //    int cnt = 0;
            //    foreach(CocosNode node in cocosNodes)
            //    {
            //        string nodeName;
            //        if (node.hName != "")
            //            nodeName = string.Format("{0}{1}{2}", nodeSuffix[node.nKind], node.hName, cnt);
            //        else
            //            nodeName = string.Format("{0}{1}", nodeSuffix[node.nKind], cnt);

            //        switch (node.nKind)
            //        {
            //            case 3://Label TTF
            //                string fontname = node.Func4;
            //                int fontsize;
            //                if (!int.TryParse(node.Func2, out fontsize))
            //                {
            //                    fontsize = 16;
            //                }
            //                //if (fontname != "")
            //                //    fontname += ".ttf";
            //                file.WriteLine(string.Format("\t" + cppNode[node.nKind], nodeName, node.Func1, fontname, fontsize));
            //                if (node.Func3 != "")
            //                    file.WriteLine(string.Format("\t" + cppCommon[8], nodeName, "Color4B." + node.Func3));
            //                break;
            //            case 4://Label BMF
            //                string bmfontname = node.Func4;
            //                file.WriteLine(string.Format("\t" + cppNode[node.nKind], nodeName, bmfontname, node.Func1));
            //                break;
            //            default:
            //                file.WriteLine(string.Format("\t" + cppNode[node.nKind], nodeName, node.safeFilename));
            //                break;
            //        }

            //        float _x = node.hPosition.X;
            //        float _y = CanvasHeight - node.hPosition.Y;
            //        string _xs = _x.ToString();
            //        string _ys = _y.ToString();
            //        switch (node.PosMode)
            //        {
            //            //default://Vec2
            //            //    _x = node.hPosition.X;
            //            //    _y = CanvasHeight - node.hPosition.Y;
            //            //    break;
            //            case 1://Offset
            //                _xs = string.Format("ScreenWidth * ({0} - 0.5f)", node.relativeX);
            //                _ys = string.Format("ScreenHeight * ({0} - 0.5f)", node.relativeY);
            //                break;
            //            case 2://Dock
            //                if (checkBox3.Checked)
            //                    _xs = string.Format("ScreenWidth - {0}", node.Right);
            //                else
            //                    _xs = string.Format("{0}", node.Left);
            //                if (checkBox2.Checked)
            //                    _ys = string.Format("ScreenHeight - {0}", node.Top);
            //                else
            //                    _ys = string.Format("{0}", node.Bottom);
            //                break;
            //        }
            //        if (node.nKind == 0)
            //            file.WriteLine(string.Format("\t" + cppCommon[0], nodeName, _xs, _ys));
            //        else
            //            file.WriteLine(string.Format("\t" + cppCommon[1], nodeName, _xs, _ys));

            //        if (node.Opacity < 255)
            //            file.WriteLine(string.Format("\t" + cppCommon[2], nodeName, node.Opacity));
            //        if (node.Tag != 0)
            //            file.WriteLine(string.Format("\t" + cppCommon[3], nodeName, node.Tag));
            //        if (node.Name != "")
            //            file.WriteLine(string.Format("\t" + cppCommon[4], nodeName, node.Name));
            //        if (node.hZorder != 0)
            //            file.WriteLine(string.Format("\t" + cppCommon[5], nodeName, node.hZorder));
            //        if (node.hScale != 1.0f)
            //            file.WriteLine(string.Format("\t" + cppCommon[6], nodeName, node.hScale));
            //        if (node.Func2 != "" && node.nKind != 3)
            //            file.WriteLine(string.Format("\t" + cppCommon[7], nodeName, node.Func2));//color
            //        if (node.Func1 != "" && (node.nKind != 3 || node.nKind != 4))
            //            file.WriteLine(string.Format("\t" + cppCommon[cppCommon.Length - 2], nodeName, node.Func1));
            //        if (node.Func3 != "")
            //            file.WriteLine(string.Format("\t" + cppCommon[cppCommon.Length - 2], nodeName, node.Func3));

            //        file.WriteLine(string.Format("\t" + cppCommon[cppCommon.Length - 1], nodeName));//addChild
            //        file.WriteLine("");




            //        cnt++;
            //    }


            //    file.Close();
            //}
        }

        private void jSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //export2
            MessageBox.Show("Sorry! JSON or XML export not yet support.");
        }

        private void xMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //export3
            MessageBox.Show("Sorry! JSON or XML export not yet support.");
        }

        private void systemQuit(object sender, FormClosingEventArgs e)
        {
            if (isChanged)
            {
                string msg = string.Format("CAUTION!!\n\n{0} is not Saved", fileName);
                if (fileName.Length == 0)
                    msg = string.Format("CAUTION!!\n\n{0} is not Saved", ProjectName);
                var window = MessageBox.Show(msg, "Save or Discard?",
                    MessageBoxButtons.YesNoCancel);
                //Form3 alert = new Form3(string.Format("CAUTION!!\n\n{0} is not Saved. Save? or Discard?", ProjectName), this);
                //alert.ShowDialog();
                switch (window)
                {
                    case DialogResult.Yes:
                        SaveAlert();
                        System.Windows.Forms.Application.ExitThread();
                        break;
                    case DialogResult.No:
                        System.Windows.Forms.Application.ExitThread();
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
            else
                System.Windows.Forms.Application.ExitThread();
        }
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //quit
            if (isChanged)
            {
                string msg = string.Format("CAUTION!!\n\n{0} is not Saved", fileName);
                if (fileName.Length == 0)
                    msg = string.Format("CAUTION!!\n\n{0} is not Saved", ProjectName);
                var window = MessageBox.Show(msg, "Save or Discard?",
                    MessageBoxButtons.YesNoCancel);
                //Form3 alert = new Form3(string.Format("CAUTION!!\n\n{0} is not Saved. Save? or Discard?", ProjectName), this);
                //alert.ShowDialog();
                switch (window)
                {
                    case DialogResult.Yes:
                        SaveAlert();
                        System.Windows.Forms.Application.ExitThread();
                        break;
                    case DialogResult.No:
                        System.Windows.Forms.Application.ExitThread();
                        break;
                    case DialogResult.Cancel:
                        break;
                }
            }
            else
                System.Windows.Forms.Application.ExitThread();
        }
        public void SaveAlert()
        {
            saveToolStripMenuItem_Click(this, EventArgs.Empty);
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cocosNodes.Clear();
            selectedNode = null;
            propertyGrid1.SelectedObject = null;
            propertyGrid1.Refresh();
        }

        private void resizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 prompt = new Form2(ProjectName, CanvasWidth, CanvasHeight);
            DialogResult dr = prompt.ShowDialog();

            if (dr == DialogResult.OK && prompt != null)
            {
                ProjectName = prompt.passName;
                CanvasWidth = Int32.Parse(prompt.passWidth);
                CanvasHeight = Int32.Parse(prompt.passHeight);
                Form1_CreateBackBuffer(prompt, EventArgs.Empty);
                label3.Text = ProjectName;
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (selectedNode != null)
            {
                cocosNodes.Remove(selectedNode);
                selectedNode = null;
                textBox1.Text = "";
                propertyGrid1.SelectedObject = null;
                checkBox1.Text = "Top";
                checkBox2.Text = "Left";
                checkBox3.Text = "Right";
                checkBox4.Text = "Bottom";
                label2.Text = "Vec2";
                label5.Text = "Offset";
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            showBorder = checkBox5.Checked;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            showCross = checkBox6.Checked;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            //int percent = Int32.Parse(numericUpDown1.Value.ToString());
            //CanvasScale = (float)percent * 0.01f;
            //Form1_CreateBackBuffer(this, EventArgs.Empty);

            CanvasScaleInt -= 10;
            if (CanvasScaleInt < 10)
                CanvasScaleInt = 10;
            numericUpDown1.Value = (decimal)CanvasScaleInt;
            Form1_CreateBackBuffer(this, EventArgs.Empty);
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            CanvasScaleInt += 10;
            if (CanvasScaleInt > 1000)
                CanvasScaleInt = 1000;
            numericUpDown1.Value = (decimal)CanvasScaleInt;
            Form1_CreateBackBuffer(this, EventArgs.Empty);
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            if (selectedNode == null)
                return;

            //to Forward
            int idx = 0;
            for (int i = 0; i < cocosNodes.Count(); i++)
            {
                if (cocosNodes.ElementAt(i) == selectedNode)
                {
                    idx = i;
                    break;
                }
            }
            if (idx == cocosNodes.Count() - 1)
                return;

            CocosNode temp = cocosNodes.ElementAt(idx);
            cocosNodes.Remove(selectedNode);
            cocosNodes.Insert(idx + 1, temp);
            selectedNode = temp;
            textBox1.Text = selectedNode.hAssetPath;
            propertyGrid1.SelectedObject = selectedNode;
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            if (selectedNode == null)
                return;

            //to Backward
            int idx = 0;
            for (int i = 0; i < cocosNodes.Count(); i++)
            {
                if (cocosNodes.ElementAt(i) == selectedNode)
                {
                    idx = i;
                    break;
                }
            }
            if (idx == 0)
                return;

            CocosNode temp = cocosNodes.ElementAt(idx);
            cocosNodes.Remove(selectedNode);
            cocosNodes.Insert(idx - 1, temp);
            selectedNode = temp;
            textBox1.Text = selectedNode.hAssetPath;
            propertyGrid1.SelectedObject = selectedNode;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            showAllBorder = checkBox7.Checked;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            UndoLoading("./temp.tmp");
            System.IO.File.Delete("./temp.tmp");

            button8.Enabled = false;
        }

        private void quickSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UndoCashing("./quicktemp.tmp");
        }

        private void quickLoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UndoLoading("./quicktemp.tmp");
        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            //Save
            string fullpath = string.Format("{0}{1}", mainPath, fileName);
            FileStream fcreate = File.Open(fullpath, FileMode.Create);

            Hashtable data = encodeHash();

            //System.IO.StreamWriter file = new System.IO.StreamWriter(sfd.OpenFile());
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                bf.Serialize(fcreate, data);
                isChanged = false;
            }
            catch (SerializationException ex)
            {
                Console.WriteLine("" + ex.Message);
                throw;
            }
            finally
            {
                fcreate.Close();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (selectedNode == null)
                return;
            selectedNode.hAssetPath = textBox1.Text;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //add label
            CocosNode labelNode = new CocosNode();
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label.ToString() == "Kind")
            {
                switch (e.ChangedItem.Value.ToString())
                {
                    case "Sprite":
                        break;
                    case "Menu":
                        break;
                    case "Button":
                        break;
                    case "Label":
                        break;
                    case "BMFLabel":
                        break;
                    case "LoadingBar":
                        break;
                    case "CheckBox":
                        break;
                    case "Slider":
                        break;
                    case "Scale9Sprite":
                        if (selectedNode.hContentSize.Width < selectedNode.image.Width ||
                            selectedNode.hContentSize.Height < selectedNode.image.Height)
                            selectedNode.hContentSize = selectedNode.image.Size;//slice9에 한해 원래 이미지 사이즈보다 작으면 강제로 리셋
                        selectedNode.bitmapInsect();
                        break;
                }
            }

            //selectedNode.hOffset = new Anchor(((selectedNode.hPosition.X - CanvasWidth * 0.5f) / (float)CanvasWidth), ((selectedNode.hPosition.Y - CanvasHeight * 0.5f) / (float)CanvasHeight));
            //selectedNode.hDocking = new NodeRect(
            //    selectedNode.hPosition.X,
            //    selectedNode.hPosition.Y,
            //    (CanvasWidth - selectedNode.hPosition.X),
            //    (CanvasHeight - selectedNode.hPosition.Y)
            //    );
            //label2.Text = string.Format("Vec2 {0}, {1}", selectedNode.hPosition.X, selectedNode.hPosition.Y);
            //label5.Text = string.Format("Offset {0:F3}, {1:F3}", selectedNode.hOffset.X, selectedNode.hOffset.Y);
            //checkBox2.Text = selectedNode.hDocking.top.ToString();
            //checkBox1.Text = selectedNode.hDocking.left.ToString();
            //checkBox3.Text = selectedNode.hDocking.right.ToString();
            //checkBox4.Text = selectedNode.hDocking.bottom.ToString();

            UndoCashing("./temp.tmp");
            button8.Enabled = true;

            switch (e.ChangedItem.Label.ToString())
            {
                case "Docking Left":
                    selectedNode.hPosition = new Point(selectedNode.hLeft, selectedNode.hPosition.Y);
                    break;
                case "Docking Right":
                    selectedNode.hPosition = new Point(CanvasWidth - selectedNode.hRight, selectedNode.hPosition.Y);
                    break;
                case "Docking Top":
                    selectedNode.hPosition = new Point(selectedNode.hPosition.X, selectedNode.hTop);
                    break;
                case "Docking Bottom":
                    selectedNode.hPosition = new Point(selectedNode.hPosition.X, CanvasHeight - selectedNode.hBottom);
                    break;
                case "Offset X":
                    selectedNode.hPosition = new Point((int)((selectedNode.hOffsetX + 0.5f) * CanvasWidth), selectedNode.hPosition.Y);
                    break;
                case "Offset Y":
                    selectedNode.hPosition = new Point(selectedNode.hPosition.X, (int)((selectedNode.hOffsetY + 0.5f) * CanvasHeight));
                    break;
                default:
                    break;
            }
            selectedNode.hOffsetX = (selectedNode.hPosition.X - CanvasWidth * 0.5f) / (float)CanvasWidth;
            selectedNode.hOffsetY = (selectedNode.hPosition.Y - CanvasHeight * 0.5f) / (float)CanvasHeight;
            selectedNode.hLeft = selectedNode.hPosition.X;
            selectedNode.hTop = selectedNode.hPosition.Y;
            selectedNode.hRight = (CanvasWidth - selectedNode.hPosition.X);
            selectedNode.hBottom = (CanvasHeight - selectedNode.hPosition.Y);

            label2.Text = string.Format("Vec2 {0}, {1}", selectedNode.hPosition.X, selectedNode.hPosition.Y);
            label5.Text = string.Format("Offset {0:F3}, {1:F3}", selectedNode.hOffsetX, selectedNode.hOffsetY);
            checkBox2.Text = selectedNode.hTop.ToString();
            checkBox1.Text = selectedNode.hLeft.ToString();
            checkBox3.Text = selectedNode.hRight.ToString();
            checkBox4.Text = selectedNode.hBottom.ToString();

            if (selectedNode.nKind == NodeKind.Scale9Sprite)
                selectedNode.bitmapInsect();

            propertyGrid1.Refresh();

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Welcome cocos GUI Editor!!\n(version 0.5)\n\nCaution!\nRotate is not complete.\nNow support only cpp syle Export.");
        }

        private void propertyGrid1_SelectedObjectsChanged(object sender, EventArgs e)
        {

        }

        private void openRecentListToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedNode == null)
            {
                MessageBox.Show("Click source Node.");
                return;
            }

            CocosNode cloneNode = (CocosNode)selectedNode.Clone();
            cloneNode.hPosition = new Point(cloneNode.hPosition.X + 10, cloneNode.hPosition.Y + 10);
            cocosNodes.Add(cloneNode);

            selectedNode = cloneNode;
        }

        //private void Form1_KeyDown(object sender, KeyEventArgs e)
        //{

        //    if (selectedNode == null)
        //        return;

        //    switch (e.KeyCode)
        //    {
        //        case Keys.Up:
        //            selectedNode.hPosition = new PointF(selectedNode.hPosition.X, selectedNode.hPosition.Y - 1);
        //            propertyGrid1.SelectedObject = selectedNode;
        //            break;
        //        case Keys.Down:
        //            selectedNode.hPosition = new PointF(selectedNode.hPosition.X, selectedNode.hPosition.Y + 1);
        //            propertyGrid1.SelectedObject = selectedNode;
        //            break;
        //        case Keys.Left:
        //            selectedNode.hPosition = new PointF(selectedNode.hPosition.X - 1, selectedNode.hPosition.Y);
        //            propertyGrid1.SelectedObject = selectedNode;
        //            break;
        //        case Keys.Right:
        //            selectedNode.hPosition = new PointF(selectedNode.hPosition.X + 1, selectedNode.hPosition.Y);
        //            propertyGrid1.SelectedObject = selectedNode;
        //            break;
        //        case Keys.Escape:
        //            selectedNode = null;
        //            propertyGrid1.SelectedObject = null;
        //            checkBox1.Text = "Top";
        //            checkBox2.Text = "Left";
        //            checkBox3.Text = "Right";
        //            checkBox4.Text = "Bottom";
        //            label2.Text = "Vec2";
        //            label5.Text = "Offset";
        //            break;
        //    }
        //}

        public Image SetImageOpacity(Image image, float opacity, float scale)
        {
            try
            {
                Bitmap bmp = new Bitmap((int)(image.Width * scale), (int)(image.Height * scale));
                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    ColorMatrix matrix = new ColorMatrix();
                    matrix.Matrix33 = opacity;
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height));
                }
                return bmp;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
        //public string getSafeRscPath(string path)
        //{

        //}
        public string getProjectPathRemoved(string srcpath)
        {
            //cut off ~\Resources\
            int idx = srcpath.LastIndexOf("Resources\\");
            if (idx != -1)
            {
                return srcpath.Substring(idx + 10, srcpath.Length - idx - 10);

            }
            return "";
        }
        public string getRootPath(string txt)
        {
            if (txt == null)
                return null;

            int idx = txt.LastIndexOf("Resources\\");
            if (idx != -1)
            {
                return txt.Substring(0, idx + 10);

            }
            return null;
        }
        public string getExpRemoved(string srcpath)
        {
            if (srcpath.LastIndexOf(".png") != -1)
                return srcpath.Replace(".png", "");
            else if (srcpath.LastIndexOf(".jpg") != -1)
                return srcpath.Replace(".jpg", "");
            else
                return srcpath;

        }
        public string getRemoveAllPath(string srcpath)
        {
            int idx = srcpath.LastIndexOf("\\");
            if (idx != -1)
            {
                return srcpath.Substring(idx + 1, srcpath.Length - idx - 1);

            }
            return "";
        }
        public List<string> getButtons(CocosNode node)
        {
            List<string> buttons = new List<string>();
            //base
            buttons.Add(node.hSafeFilename);
            //pressed
            if (node.hpressAsset != "")
                buttons.Add(getProjectPathRemoved(node.hpressAsset));
            else
                buttons.Add(node.hSafeFilename);
            //disabled
            if (node.hdisableAsset != "")
                buttons.Add(getProjectPathRemoved(node.hdisableAsset));
            else
                buttons.Add(node.hSafeFilename);

            return buttons;
        }
    }
}
