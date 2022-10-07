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
    /// Interaktionslogik für Developer_mode.xaml
    /// </summary>
    public partial class Developer_mode : Window
    {
        public Developer_mode()
        {
            InitializeComponent();
        }

        public bool OkayClicked;

        private void CmdDevOkay_Click(object sender, RoutedEventArgs e)
        {
            OkayClicked = true;
            this.Close();
        }

        private void CmdDevCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
