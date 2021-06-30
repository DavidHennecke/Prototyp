using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

        public Layer InitLayer(string sFilename)
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
            return Layer;
        }

        public TreeViewItem AddTreeViewChild(Layer Layer)
        {
            string layerName = Layer.GetName();
            TreeViewItem newChild = new TreeViewItem();
            ContextMenu vectorContextMenu = new ContextMenu();

            MenuItem ZoomToLayer = new MenuItem();
            ZoomToLayer.Header = "Zoom to Layer";
            MenuItem Remove = new MenuItem();
            Remove.Header = "Remove";

            vectorContextMenu.Items.Add(ZoomToLayer);
            vectorContextMenu.Items.Add(Remove);

            newChild.ContextMenu = vectorContextMenu;
            newChild.Header = layerName;
            return newChild;
        }

        public MapPolygon BuildPolygon(Geometry ring)
        {
            MapPolygon polygon = new MapPolygon
            {
                StrokeColor = Colors.Black,
                StrokeThickness = 1,
                StrokeDashed = true,
            };
            List<BasicGeoposition> polygonPointList = new List<BasicGeoposition>();

            int count = ring.GetPointCount();
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    polygonPointList.Add(new BasicGeoposition() { Latitude = ring.GetX(i), Longitude = ring.GetY(i) });

                }
                polygon.Path = new Geopath(polygonPointList);
            }



            return polygon;
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
            Geometry polyEnvelopeTrans = Transform(polyEnvelope);
            BasicGeoposition newPosition = new BasicGeoposition() { Latitude = polyEnvelopeTrans.Centroid().GetX(0), Longitude = polyEnvelopeTrans.Centroid().GetY(0) };
            Geopoint newCenter = new Geopoint(newPosition);
            return newCenter;
        }

        public Geometry Transform(Geometry geom)
        {
            SpatialReference sourceRef = Layer.GetSpatialRef();
            //Manuelle Eingabe ermöglichen
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
            return geom;
        }
    }
}
