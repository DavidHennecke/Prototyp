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

namespace Prototyp
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            GdalBase.ConfigureAll();

        }

        //void addNewPolygon()
        //{
        //    double centerLatitude = 54.072526355677596;
        //    double centerLongitude = 12.128686880147251;

        //    MapPolygon polygon = new MapPolygon
        //    {
        //        Path = new Geopath(new List<BasicGeoposition> {
        //            new BasicGeoposition() {Latitude=centerLatitude+0.0005, Longitude=centerLongitude-0.001 },
        //            new BasicGeoposition() {Latitude=centerLatitude-0.0005, Longitude=centerLongitude-0.001 },
        //            new BasicGeoposition() {Latitude=centerLatitude-0.0005, Longitude=centerLongitude+0.001 },
        //            new BasicGeoposition() {Latitude=centerLatitude+0.0005, Longitude=centerLongitude+0.001 },
        //        }),
        //        ZIndex = 1,
        //        FillColor = Colors.Red,
        //        StrokeColor = Colors.Blue,
        //        StrokeThickness = 3,
        //        StrokeDashed = false,
        //    };

        //    map.MapElements.Add(polygon);
        //}
        public void addNewPolygon(string sFilename)
        {
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();// Register all drivers
            DataSource ds = Ogr.Open(sFilename, 1); // 0 means read-only, 1 means modifiable
            if (ds == null)
            {
                MessageBox.Show("Failed to open file [{0}]!", sFilename);
                return;
            }

            // Get the first layer
            int iLayerCount = ds.GetLayerCount();
            Layer Layer = ds.GetLayerByIndex(0);
            if (Layer == null)
            {
                MessageBox.Show("Get the {0}th layer failed! n", "0");
                return;
            }

            string test = "";
            string layerName = Layer.GetName();

            Envelope envelope = new Envelope();
            Layer.GetExtent(envelope, 0);
            var x = envelope.MaxX;

            
            var layerRef = Layer.GetSpatialRef();

            Layer.ResetReading();
            Feature feature = null;
            do
            {
                feature = Layer.GetNextFeature();
                if (feature != null)
                {
                    Geometry geom = feature.GetGeometryRef();
                    wkbGeometryType type = geom.GetGeometryType();
                    SpatialReference sourceRef = geom.GetSpatialReference();

                    SpatialReference to_crs = new SpatialReference(null);
                    to_crs.ImportFromEPSG(4326);

                    CoordinateTransformation ct = new CoordinateTransformation(sourceRef, to_crs, new CoordinateTransformationOptions());
                // You can use the CoordinateTransformationOptions to set the operation or area of interet etc

                    if (geom.Transform(ct) != 0)
                    {
                        throw new NotSupportedException("projection failed");
                    }

                    geom.Transform(ct);

                    string wkt;
                    SpatialReference targetRef = geom.GetSpatialReference();
                    targetRef.ExportToWkt(out wkt, null);

                    test += wkt + Environment.NewLine;

                    int count = geom.GetPointCount();
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            double[] argout = new double[3];
                            geom.GetPoint(i, argout);

                            // do your processing here
                        }
                    }
                }
            } while (feature != null);



            
            txtEditor.Text = test;

        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.DefaultExt = ".*";
            dlg.Filter = "SHP Files (*.shp)|*.shp|All files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();
            string sFilenames = "";
            if (result == true)
            {

                foreach (string sFilename in dlg.FileNames)
                {
                    sFilenames += sFilename + Environment.NewLine;
                    addNewPolygon(sFilename);
                }
                sFilenames = sFilenames.Substring(1);
                //txtEditor.Text = sFilenames;


            }

        }
    }
}
