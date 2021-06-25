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

namespace Prototyp
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            GdalBase.ConfigureAll();

        }

        public void addNewPolygon(string sFilename)
        {
            SpatialReference to_crs = new SpatialReference(null);
            to_crs.ImportFromEPSG(4326);
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
            for (int iLayer = 0; iLayer < ds.GetLayerCount(); iLayer++)
            {
                Layer Layer = ds.GetLayerByIndex(iLayer);
                if (Layer == null)
                {
                    MessageBox.Show("Get the {0}th layer failed! n", "0");
                    return;
                }

                string test = "";
                string layerName = Layer.GetName();

                TreeViewItem newChild = new TreeViewItem();
                newChild.Header = layerName;
                TableOfContentsVector.Items.Add(newChild);

                //Envelope envelope = new Envelope();
                //Layer.GetExtent(envelope, 0);
                //Geometry ringEnvelope = new Geometry(wkbGeometryType.wkbLinearRing);
                //ringEnvelope.AddPoint_2D(envelope.MinX, envelope.MinY);
                //ringEnvelope.AddPoint_2D(envelope.MaxX, envelope.MinY);
                //ringEnvelope.AddPoint_2D(envelope.MaxX, envelope.MaxY);
                //ringEnvelope.AddPoint_2D(envelope.MinX, envelope.MaxY);
                //ringEnvelope.AddPoint_2D(envelope.MinX, envelope.MinY);

                //Geometry polyEnvelope = new Geometry(wkbGeometryType.wkbPolygon);
                //var centeroid = polyEnvelope.Centroid();
                //test += centeroid + Environment.NewLine;


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

                        if (sourceRef != to_crs)
                        {
                            CoordinateTransformation ct = new CoordinateTransformation(sourceRef, to_crs, new CoordinateTransformationOptions());
                            // You can use the CoordinateTransformationOptions to set the operation or area of interet etc

                            if (geom.Transform(ct) != 0)
                            {
                                throw new NotSupportedException("projection failed");
                            }

                            geom.Transform(ct);


                            SpatialReference targetRef = geom.GetSpatialReference();
                            //string wkt;
                            //targetRef.ExportToWkt(out wkt, null);
                        }
                        Geometry ring = geom.GetGeometryRef(0);
                        int count = ring.GetPointCount();
                        List<BasicGeoposition> polygonPointList = new List<BasicGeoposition>();
                        if (count > 0)
                            for (int i = 0; i < count; i++)
                            {
                                polygonPointList.Add(new BasicGeoposition() { Latitude = ring.GetX(i), Longitude = ring.GetY(i) });

                                
                                // do your processing here
                            }

                        MapPolygon polygon = new MapPolygon
                        {
                            Path = new Geopath(polygonPointList),
                            ZIndex = 1,
                            FillColor = Colors.Red,
                            StrokeColor = Colors.Blue,
                            StrokeThickness = 3,
                            StrokeDashed = false,
                        };

                        map.MapElements.Add(polygon);


                    }
                } while (feature != null);




                txtEditor.Text = test;
            }

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
