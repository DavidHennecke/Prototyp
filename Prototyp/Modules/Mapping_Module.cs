using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using OSGeo.OGR;
using OSGeo.OSR;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml.Controls.Maps;

namespace Prototyp.Modules
{
    public class Mapping_Module : NodeViewModel
    {
        private MapElementsLayer mapLayer = new MapElementsLayer { };
       

        Color layerColor = new Color();
        Random rnd = new Random();
        public ValueNodeInputViewModel<Layer> mappingNodeInput { get; }

        public Mapping_Module()
        {
            this.Name = "Mapping";

            var cutoff = 0;
            mappingNodeInput = new ValueNodeInputViewModel<Layer>();
            mappingNodeInput.ValueChanged.Subscribe(mappingInputValue =>
                {
                    
                    if (mappingInputValue != null)
                    {
                        //string test2 = "";
                        //test2 += newValue.GetFeatureCount(0);
                        //MessageBox.Show(test2);
                        mappingNodeInput.Name = mappingInputValue.GetName();
                        layerColor = Color.FromArgb(255, (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
                        AddLayerToMap(mappingInputValue);
                        cutoff = 1;
                    }
                    else
                    {
                        if (cutoff == 1)
                        {
                            cutoff = 0;
                            Prototyp.MainWindow.AppWindow.map.Layers.Remove(mapLayer);
                            mappingNodeInput.Name = null;
                        }
                    };
                });

            this.Inputs.Add(mappingNodeInput);
        }

        static Mapping_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Mapping_Module>));
        }

        private void AddLayerToMap(Layer Layer)
        {
            var mapLayerElements = new List<MapElement>();
            long featureCount = Layer.GetFeatureCount(0);
            OSGeo.OGR.Geometry geom = null;
            //string test3 = "";
            //test3 += featureCount;
            //MessageBox.Show(test3);
            for (int i = 0; i < featureCount; i++)
            {
                Feature feature = Layer.GetFeature(i);
                geom = feature.GetGeometryRef();
                //string test = "";
                //test += geom;
                //MessageBox.Show(test);

                //OSGeo.OGR.Geometry transGeom = Transform(geom, Layer);
            
                //transGeom.ExportToWkt(out var test);
                //MessageBox.Show(test);
                var featureType = geom.GetGeometryType();
                if (featureType == wkbGeometryType.wkbPolygon)
                {
                    //OSGeo.OGR.Geometry ring = transGeom.GetGeometryRef(0);
                    OSGeo.OGR.Geometry ring = geom.GetGeometryRef(0);
                    MapPolygon polygon = new MapPolygon();
                    polygon = BuildPolygon(ring);
                    mapLayerElements.Add(polygon);
                }
                if (featureType == wkbGeometryType.wkbMultiPolygon)
                {
                    //OSGeo.OGR.Geometry ring = transGeom.GetGeometryRef(0);
                    OSGeo.OGR.Geometry ring = geom.GetGeometryRef(0);
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
                    //Geopoint pos = new Geopoint(new BasicGeoposition() { Latitude = transGeom.GetY(0), Longitude = transGeom.GetX(0) });
                    Geopoint pos = new Geopoint(new BasicGeoposition() { Latitude = geom.GetY(0), Longitude = geom.GetX(0) });
                    MapIcon point = new MapIcon
                    {
                        Location = pos,
                        //NormalizedAnchorPoint = new Windows.Foundation.Point(0.5, 1.0),
                    };
                    mapLayerElements.Add(point);
                }
            }
            mapLayer.MapElements = mapLayerElements;
           
            Prototyp.MainWindow.AppWindow.map.Layers.Add(mapLayer);
        }

        private MapPolygon BuildPolygon(Geometry ring)
        {
            MapPolygon polygon = new MapPolygon
            {
                StrokeColor = Colors.Black,
                //FillColor = layerColor,
                FillColor = Color.FromArgb(100, 255, 255, 0),

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

        //private Geometry Transform(Geometry geom, Layer Layer)
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
