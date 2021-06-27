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

namespace Prototyp
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            GdalBase.ConfigureAll();

        }

        private async void addPolygons(string sFilename)
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

            Layer Layer = ds.GetLayerByIndex(0);
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
                await BuildPolygon(feature, test);
               
            } while (feature != null);

            txtEditor.Text = test;

        }

        public async Task BuildPolygon(Feature feature, string test)
        {
            if (feature != null)
            {
                test += "1" + Environment.NewLine;
                Geometry geom = feature.GetGeometryRef();
                SpatialReference sourceRef = geom.GetSpatialReference();
                SpatialReference to_crs = new SpatialReference(null);
                to_crs.ImportFromEPSG(4326);

                if (sourceRef != to_crs)
                {
                    CoordinateTransformation ct = new CoordinateTransformation(sourceRef, to_crs, new CoordinateTransformationOptions());

                    if (geom.Transform(ct) != 0)
                    {
                        throw new NotSupportedException("projection failed");
                    }

                    geom.Transform(ct);
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
                test += polygonPointList + Environment.NewLine;

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
                await Task.Delay(1000);

            };
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
                        addPolygons(sFilename);
                    }
                    
                }
            }
        }
    }

    internal class Test
    {

    }
}
