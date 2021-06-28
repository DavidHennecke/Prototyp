using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml.Controls.Maps;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using MaxRev.Gdal.Core;
using System.Globalization;
using System.Windows.Controls;
using System.IO;
using System.Threading.Tasks;
using Prototyp.Elements;

namespace Prototyp
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            GdalBase.ConfigureAll();

        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.DefaultExt = ".*";
            dlg.Filter = "SHP Files (*.shp)|*.shp|All files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {

                foreach (string sFilename in dlg.FileNames)
                {
                    string ext = Path.GetExtension(sFilename);

                    if (ext == ".shp")
                    {
                        AddShapefile(sFilename);
                    }

                }
            }
        }

        public void AddShapefile(string sFilename)
        {
            Shapefile shapefile = new Shapefile();
            shapefile.sFilename = sFilename;
            TreeViewItem newChild = shapefile.InitLayer(shapefile.sFilename);
            TableOfContentsVector.Items.Add(newChild);
            MapPolygon polygon = shapefile.BuildElements();
            map.MapElements.Add(polygon);
        }

    }
    }

