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

        private void Search_Toolbar_GotFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void Search_Toolbar_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Search_Toolbar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                OSGeo.OSR.SpatialReference reference = new OSGeo.OSR.SpatialReference(null);
                try
                {
                    reference.ImportFromEPSG(Int32.Parse(Search_Toolbar.Text));
                    MessageBox.Show(reference.GetName());
                }
                catch (Exception)
                {
                    MessageBox.Show("No coordinate system with this EPSG code available in the database.");
                }
            }
        }
    }
}
