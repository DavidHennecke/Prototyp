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
            Layer Layer = shapefile.InitLayer(shapefile.sFilename);
            TreeViewItem newChild = shapefile.AddTreeViewChild(Layer);
            TableOfContentsVector.Items.Add(newChild);
            var polygons = new List<MapElement>();
            long featureCount = Layer.GetFeatureCount(0);
            for (int i = 0; i < featureCount; i++)
            {
                MapPolygon polygon = new MapPolygon();
                Feature feature = Layer.GetFeature(i);
                Geometry geom = feature.GetGeometryRef();
                Geometry transGeom = shapefile.Transform(geom);
                Geometry ring = transGeom.GetGeometryRef(0);
                var featureType = geom.GetGeometryType();
                //string test = "";
                //test += transGeom.GetGeometryCount();
                //MessageBox.Show(test);


                if (featureType == wkbGeometryType.wkbPolygon)
                {
                    polygon = shapefile.BuildPolygon(ring);
                    polygons.Add(polygon);
                }

                if (featureType == wkbGeometryType.wkbMultiPolygon)
                {
                    int pathCount = transGeom.GetGeometryCount();


                    for (int pc = 0; pc < pathCount; pc++)
                    {
                        Geometry multi = transGeom.GetGeometryRef(pc);

                        for (int k = 0; k < multi.GetGeometryCount(); ++k)
                        {
                            Geometry multiRing = multi.GetGeometryRef(k);
                            polygon = shapefile.BuildPolygon(multiRing);
                            polygons.Add(polygon);
                        }

                    }
                }
            }

            var polygonsLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = polygons
            };
            map.Layers.Add(polygonsLayer);

            if (firstLayer == 0)
            {

                Geopoint newCenter = shapefile.ZoomToExtent();
                map.TrySetViewAsync(newCenter, 10);
                firstLayer += 1;


            }

        }

    }
}

