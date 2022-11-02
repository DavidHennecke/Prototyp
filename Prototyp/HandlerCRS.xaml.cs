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
    /// Interaction logic for HandlerCRS.xaml
    /// </summary>
    public partial class HandlerCRS : Window
    {
        public HandlerCRS()
        {
            InitializeComponent();
        }

        private void BtnOkayHandlerCrs_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnCancelHandlerCrs_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
