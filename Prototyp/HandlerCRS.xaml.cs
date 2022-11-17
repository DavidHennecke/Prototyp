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
        public bool OkayClicked;
        public HandlerCRS()
        {
            InitializeComponent();
        }

        private void BtnOkayHandlerCrs_Click(object sender, RoutedEventArgs e)
        {
            OkayClicked = true;
            this.Close();
        }

        private void BtnCancelHandlerCrs_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Search_CRS_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Search_CRS.Text == "Search for EPSG/WKT/Proj4 info...")
            {
                Search_CRS.Text = "";
            }
        }

        private void Search_CRS_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Search_CRS.Text == "")
            {
                Search_CRS.Text = "Search for EPSG/WKT/Proj4 info...";
            }
        }
        private void doubleClickTreeBoxItem(object sender, System.Windows.Input.MouseButtonEventArgs e, string name, string epsg)
        {
            CrsName.Content = name;
            EPSG.Content = epsg;
        }
        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
        private void Search_CRS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && Search_CRS.Text != "")
            {
                OSGeo.OSR.SpatialReference reference = new OSGeo.OSR.SpatialReference(null);
                try
                {
                    if (IsDigitsOnly(Search_CRS.Text))
                    {
                        reference.ImportFromEPSG(Int32.Parse(Search_CRS.Text));
                        ListBoxItem lvi = new ListBoxItem();
                        lvi.Content = reference.GetName() + "         " + "EPSG: " + reference.GetAttrValue("AUTHORITY", 1);
                        lvi.MouseDoubleClick += (sender, e) => doubleClickTreeBoxItem(sender, e, reference.GetName(), reference.GetAttrValue("AUTHORITY", 1));
                        CRS_List.Items.Add(lvi);
                    }
                    else if (Search_CRS.Text.Contains("[")) // Probably WKT string.
                    {
                        string InputText = Search_CRS.Text;
                        reference.ImportFromWkt(ref InputText);
                        ListBoxItem lvi = new ListBoxItem();
                        lvi.Content = reference.GetName() + "         " + "EPSG: " + reference.GetAttrValue("AUTHORITY", 1);
                        lvi.MouseDoubleClick += (sender, e) => doubleClickTreeBoxItem(sender, e, reference.GetName(), reference.GetAttrValue("AUTHORITY", 1));
                        CRS_List.Items.Add(lvi);
                    }
                    else  // Fallback, assume that entered string is proj4.
                    {
                        reference.ImportFromProj4(Search_CRS.Text);
                        ListBoxItem lvi = new ListBoxItem();
                        lvi.Content = reference.GetName() + "         " + "EPSG: " + reference.GetAttrValue("AUTHORITY", 1);
                        lvi.MouseDoubleClick += (sender, e) => doubleClickTreeBoxItem(sender, e, reference.GetName(), reference.GetAttrValue("AUTHORITY", 1));
                        CRS_List.Items.Add(lvi);
                    }
                }
                catch
                {
                    MessageBox.Show("No coordinate system with this EPSG code available in the database.");
                }
            }
        }
    }
}
