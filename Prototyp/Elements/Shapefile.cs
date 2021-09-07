using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using Prototyp.Custom_Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml.Controls.Maps;

namespace Prototyp.Elements
{
    public class Shapefile
    {
        public string sFilename;
        private Layer Layer;
        private VectorListViewItem newChild = new VectorListViewItem();
        private MapElementsLayer mapLayer = new MapElementsLayer { };
        private Color layerColor = new Color();
        private Random rnd = new Random();
        private int layerOrderNr;
        private int layerOrderCount;
        private int layerOrderZValue;



        public void InitLayer(string sFilename)
        {
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();// Register all drivers
            DataSource ds = Ogr.Open(sFilename, 1); // 0 means read-only, 1 means modifiable
            if (ds == null)
            {
                MessageBox.Show("Failed to open file [{0}]!", sFilename);
            }

            Layer = ds.GetLayerByIndex(0);
            if (Layer == null)
            {
                MessageBox.Show("Get the {0}th layer failed! n", "0");
            }
            AddTreeViewChild();
            AddLayerToMap();
        }

        private void AddTreeViewChild()
        {
            string layerName = Layer.GetName();
            ContextMenu vectorContextMenu = new ContextMenu();
            newChild.VectorListViewItemCheckBox.Unchecked += new RoutedEventHandler(DisableLayer);
            newChild.VectorListViewItemCheckBox.Checked += new RoutedEventHandler(EnableLayer);
            MenuItem ZoomToLayer = new MenuItem();
            ZoomToLayer.Header = "Zoom to Layer";
            ZoomToLayer.Click += new RoutedEventHandler(ZoomToLayer_Click);
            vectorContextMenu.Items.Add(ZoomToLayer);
            MenuItem Remove = new MenuItem();
            Remove.Header = "Remove";
            Remove.Click += new RoutedEventHandler(RemoveLayer);
            vectorContextMenu.Items.Add(Remove);
            MenuItem Properties = new MenuItem();
            Properties.Header = "Properties";
            vectorContextMenu.Items.Add(Properties);
            newChild.ContextMenu = vectorContextMenu;
            newChild.VectorListViewItemText.Text = layerName;

            layerColor = Color.FromArgb(255, (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
            var converter = new System.Windows.Media.BrushConverter();
            var brush = (System.Windows.Media.Brush)converter.ConvertFromString(layerColor.ToString());
            newChild.VectorListViewItemColorPicker.Background = brush;
            newChild.VectorListViewItemColorPicker.Click += new RoutedEventHandler(PickColor);

            newChild.VectorListViewItemText.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler((sender, e) => StartDragEvent(sender, e, newChild));

            Prototyp.MainWindow.AppWindow.TableOfContentsLayer.Items.Add(newChild);
            ((System.Collections.Specialized.INotifyCollectionChanged)Prototyp.MainWindow.AppWindow.TableOfContentsLayer.Items).CollectionChanged += TableOfContentsLayer_CollectionChanged;
        }

        private void StartDragEvent(object sender, System.Windows.Input.MouseButtonEventArgs e, VectorListViewItem newChild)
        {
            string fileName = (string)sFilename;
            string LayerName = (string)Layer.GetName();

            DataObject dataObj = new DataObject();
            dataObj.SetData("Filename", fileName);
            dataObj.SetData("Layername", LayerName);

            DragDrop.DoDragDrop(newChild, dataObj, DragDropEffects.Move);

        }

        

        private void AddLayerToMap()
        {
            var mapLayerElements = new List<MapElement>();
            long featureCount = Layer.GetFeatureCount(0);
            Feature feature;
            for (int i = 0; i < featureCount; i++)
            {
                feature = Layer.GetFeature(i);
                OSGeo.OGR.Geometry geom = feature.GetGeometryRef();
                //OSGeo.OGR.Geometry transGeom = Transform(geom);
                OSGeo.OGR.Geometry ring = null;
                //transGeom.ExportToWkt(out var test);
                //MessageBox.Show(test);
                var featureType = geom.GetGeometryType();
                if (featureType == wkbGeometryType.wkbPolygon)
                {
                    //ring = transGeom.GetGeometryRef(0);
                    ring = geom.GetGeometryRef(0);
                    MapPolygon polygon = new MapPolygon();
                    polygon = BuildPolygon(ring);
                    mapLayerElements.Add(polygon);
                }
                if (featureType == wkbGeometryType.wkbMultiPolygon)
                {
                    //ring = transGeom.GetGeometryRef(0);
                    ring = geom.GetGeometryRef(0);
                    MapPolygon polygon = new MapPolygon();
                    //int pathCount = transGeom.GetGeometryCount();
                    int pathCount = geom.GetGeometryCount();
                    for (int pc = 0; pc < pathCount; pc++)
                    {
                        //Geometry multi = transGeom.GetGeometryRef(pc);
                        Geometry multi = geom.GetGeometryRef(pc);

                        for (int k = 0; k < multi.GetGeometryCount(); ++k)
                        {
                            Geometry multiRing = multi.GetGeometryRef(k);
                            polygon = BuildPolygon(multiRing);
                            mapLayerElements.Add(polygon);
                        }
                    }
                }
                if (featureType == wkbGeometryType.wkbPoint)
                {
                    //Geopoint pos = new Geopoint (new BasicGeoposition () { Latitude = transGeom.GetY(0), Longitude = transGeom.GetX(0) });
                    Geopoint pos = new Geopoint (new BasicGeoposition () { Latitude = geom.GetY(0), Longitude = geom.GetX(0) });
                    MapIcon point = new MapIcon
                    {
                        Location = pos,
                        //NormalizedAnchorPoint = new Windows.Foundation.Point(0.5, 1.0),
                    };
                    mapLayerElements.Add(point);
                }
            }
            mapLayer.MapElements = mapLayerElements;
            mapLayer.ZIndex = layerOrderZValue;
            Prototyp.MainWindow.AppWindow.map.Layers.Add(mapLayer);
        }

        private MapPolygon BuildPolygon(Geometry ring)
        {
            MapPolygon polygon = new MapPolygon
            {
                StrokeColor = Colors.Black,
                FillColor = layerColor,
                StrokeThickness = 1,
                StrokeDashed = true,
            };
            List<BasicGeoposition> polygonPointList = new List<BasicGeoposition>();

            int count = ring.GetPointCount();
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    polygonPointList.Add(new BasicGeoposition() { Latitude = ring.GetY(i), Longitude = ring.GetX(i) });

                }
                polygon.Path = new Geopath(polygonPointList);
            }
            return polygon;
        }

        private void TableOfContentsLayer_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                layerOrderCount = Prototyp.MainWindow.AppWindow.TableOfContentsLayer.Items.Count;
                layerOrderNr = Prototyp.MainWindow.AppWindow.TableOfContentsLayer.Items.IndexOf(newChild);
                layerOrderZValue = layerOrderCount + 1 - layerOrderNr;
                mapLayer.ZIndex = layerOrderZValue;
            }
        }
        private void PickColor(Object sender, RoutedEventArgs e)
        {

        }

        private void RemoveLayer(Object sender, RoutedEventArgs e)
        {
            Prototyp.MainWindow.AppWindow.map.Layers.Remove(mapLayer);
            Prototyp.MainWindow.AppWindow.TableOfContentsLayer.Items.Remove(newChild);
        }

        private void DisableLayer(Object sender, RoutedEventArgs e)
        {
            Prototyp.MainWindow.AppWindow.map.Layers.Remove(mapLayer);
        }

        private void EnableLayer(Object sender, RoutedEventArgs e)
        {
            Prototyp.MainWindow.AppWindow.map.Layers.Add(mapLayer);
        }

        public void ZoomToLayer_Click(Object sender, RoutedEventArgs e)
        {
            Geopoint newCenter = ZoomToExtent();
            Prototyp.MainWindow.AppWindow.map.TrySetViewAsync(newCenter, 10);
        }

        public Geopoint ZoomToExtent()
        {
            Envelope envelope = new Envelope();
            Layer.GetExtent(envelope, 0);
            Geometry ringEnvelope = new Geometry(wkbGeometryType.wkbLinearRing);
            ringEnvelope.AddPoint_2D(envelope.MinX, envelope.MinY);
            ringEnvelope.AddPoint_2D(envelope.MaxX, envelope.MinY);
            ringEnvelope.AddPoint_2D(envelope.MaxX, envelope.MaxY);
            ringEnvelope.AddPoint_2D(envelope.MinX, envelope.MaxY);
            ringEnvelope.AddPoint_2D(envelope.MinX, envelope.MinY);

            Geometry polyEnvelope = new Geometry(wkbGeometryType.wkbPolygon);
            polyEnvelope.AddGeometry(ringEnvelope);
            //Geometry polyEnvelopeTrans = Transform(polyEnvelope);
            //BasicGeoposition newPosition = new BasicGeoposition() { Latitude = polyEnvelopeTrans.Centroid().GetY(0), Longitude = polyEnvelopeTrans.Centroid().GetX(0) };
            BasicGeoposition newPosition = new BasicGeoposition() { Latitude = polyEnvelope.Centroid().GetY(0), Longitude = polyEnvelope.Centroid().GetX(0) };
            Geopoint newCenter = new Geopoint(newPosition);
            return newCenter;
        }

        //public Geometry Transform(Geometry geom)
        //{
        //    SpatialReference sourceRef = Layer.GetSpatialRef();


        //    //Manuelle Eingabe ermöglichen
        //    SpatialReference to_crs = new SpatialReference(null);
        //    to_crs.ImportFromEPSG(4326);
        //    if (sourceRef != to_crs)
        //    {
        //        CoordinateTransformation ct = new CoordinateTransformation(sourceRef, to_crs, new CoordinateTransformationOptions());

        //        if (geom.Transform(ct) != 0)
        //        {
        //            throw new NotSupportedException("projection failed");
        //        }
        //        geom.Transform(ct);
        //    }
        //    return geom;
        //}
    }
}
