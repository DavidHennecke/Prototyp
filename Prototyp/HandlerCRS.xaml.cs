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
    /// 
    public class EPSGDictionary
    {
        public string Name { get; set; }
        public string EPSG { get; set; }
    }

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
            if (Search_CRS.Text == "Search for CRS info...")
            {
                Search_CRS.Text = "";
            }
        }

        private void Search_CRS_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Search_CRS.Text == "")
            {
                Search_CRS.Text = "Search for CRS info...";
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
            // ---- Build database:
            //OSGeo.OSR.SpatialReference reference = new OSGeo.OSR.SpatialReference(null);
            //string Database = "";

            //for (int i = 0; i < 99999; i++)
            //{
            //    System.Diagnostics.Debug.WriteLine(i);
            //    try
            //    {
            //        reference.ImportFromEPSG(i);
            //        Database = Database + reference.GetAttrValue("AUTHORITY", 1) + " && " + reference.GetName() + Environment.NewLine;
            //    }
            //    catch { }
            //}
            // ---- Build database.

            if (e.Key == System.Windows.Input.Key.Enter && Search_CRS.Text != "")
            {
                string DBString = System.IO.File.ReadAllText(MainWindow.ParentDir.FullName + "\\EPSG database.txt");
                string[] DBLines = DBString.Split(Environment.NewLine);
                List<EPSGDictionary> Database = new List<EPSGDictionary>();
                foreach (string DBLine in DBLines)
                {
                    string[] LineSplit = DBLine.Split(" && ");

                    EPSGDictionary Entry = new EPSGDictionary();
                    Entry.EPSG = LineSplit[0];
                    Entry.Name = LineSplit[1];

                    Database.Add(Entry);
                }

                OSGeo.OSR.SpatialReference reference = new OSGeo.OSR.SpatialReference(null);
                try
                {
                    CRS_List.Items.Clear();

                    if (IsDigitsOnly(Search_CRS.Text))
                    {
                        reference.ImportFromEPSG(Int32.Parse(Search_CRS.Text));
                        ListViewItem lvi = new ListViewItem();
                        lvi.Content = new EPSGDictionary { Name = reference.GetName(), EPSG = reference.GetAttrValue("AUTHORITY", 1) };
                        lvi.MouseDoubleClick += (sender, e) => doubleClickTreeBoxItem(sender, e, reference.GetName(), reference.GetAttrValue("AUTHORITY", 1));
                        CRS_List.Items.Add(lvi);
                    }
                    else
                    {
                        foreach(EPSGDictionary d in Database)
                        {
                            if (d.EPSG.ToLower().Contains(Search_CRS.Text.ToLower()) | d.Name.ToLower().Contains(Search_CRS.Text.ToLower()))
                            {
                                ListViewItem lvi = new ListViewItem();
                                lvi.Content = d;
                                lvi.MouseDoubleClick += (sender, e) => doubleClickTreeBoxItem(sender, e, d.Name, d.EPSG);
                                CRS_List.Items.Add(lvi);
                            }
                        }
                    }

                    //else if (Search_CRS.Text.Contains("[")) // Probably WKT string.
                    //{
                    //    string InputText = Search_CRS.Text;
                    //    reference.ImportFromWkt(ref InputText);
                    //    ListBoxItem lvi = new ListBoxItem();
                    //    lvi.Content = reference.GetName() + "         " + "EPSG: " + reference.GetAttrValue("AUTHORITY", 1);
                    //    lvi.MouseDoubleClick += (sender, e) => doubleClickTreeBoxItem(sender, e, reference.GetName(), reference.GetAttrValue("AUTHORITY", 1));
                    //    CRS_List.Items.Add(lvi);
                    //}
                    //else  // Fallback, assume that entered string is proj4.
                    //{
                    //    reference.ImportFromProj4(Search_CRS.Text);
                    //    ListBoxItem lvi = new ListBoxItem();
                    //    lvi.Content = reference.GetName() + "         " + "EPSG: " + reference.GetAttrValue("AUTHORITY", 1);
                    //    lvi.MouseDoubleClick += (sender, e) => doubleClickTreeBoxItem(sender, e, reference.GetName(), reference.GetAttrValue("AUTHORITY", 1));
                    //    CRS_List.Items.Add(lvi);
                    //}
                }
                catch
                {
                    MessageBox.Show("No coordinate system with this search info available in the database.");
                }
            }
        }
    }
}
