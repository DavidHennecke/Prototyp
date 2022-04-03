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
using System.Windows.Shapes;

namespace Prototyp
{
    /// <summary>
    /// Interaction logic for ModuleProperties.xaml
    /// </summary>
    public partial class ModuleProperties : Window
    {
        public bool OkayClicked;

        public ModuleProperties()
        {
            InitializeComponent();
        }

        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtOkay_Click(object sender, RoutedEventArgs e)
        {
            OkayClicked = true;
            this.Close();
        }
    }
}
