using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Media;
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
using Prototyp.Custom_Controls;
using Geometry = OSGeo.OGR.Geometry;

namespace Prototyp
{

    public partial class MainWindow : Window
    {
        public static MainWindow AppWindow;
        public MainWindow()
        {

            InitializeComponent();
            GdalBase.ConfigureAll();
            AppWindow = this;
        }

        public int firstLayer = 0;
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
            shapefile.InitLayer(shapefile.sFilename);
            if (firstLayer == 0)
            {
                Geopoint newCenter = shapefile.ZoomToExtent();
                map.TrySetViewAsync(newCenter, 10);
                firstLayer += 1;
            }
        }



    }
}

