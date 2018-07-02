using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cocosUiEditor
{
    public partial class Form3 : Form
    {

        public Form1 parent;
        CocosNode data;

        public Form3()
        {
            InitializeComponent();
            data = new CocosNode();
            propertyGrid1.SelectedObject = data;
        }
        public PropertyGrid getGrid()
        {
            return propertyGrid1;
        }

    }
}
