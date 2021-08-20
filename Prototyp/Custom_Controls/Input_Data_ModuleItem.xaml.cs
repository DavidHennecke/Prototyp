using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prototyp.Custom_Controls
{
    /// <summary>
    /// Interaction logic for Input_Data_ModuleItem.xaml
    /// </summary>
    public partial class Input_Data_ModuleItem : UserControl
    {
        public Input_Data_ModuleItem()
        {
            InitializeComponent();
        }

        public Point NodePos = new Point();

        private void importNode_Click(object sender, RoutedEventArgs e)
        {
            UIElement importNodeButtonUI = importNode;

            NodePos = importNodeButtonUI.TranslatePoint(new Point(0, 0), MainWindow.AppWindow.NodeEditor);
            NodePos.X = NodePos.X + 5;
            NodePos.Y = NodePos.Y + 5;

            var test = "";
            test += NodePos;
            MessageBox.Show(test);
        }
    }
}
