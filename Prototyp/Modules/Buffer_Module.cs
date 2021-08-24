using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using Prototyp.Modules.ViewModels;
using ReactiveUI;
using System;
using System.Windows;

namespace Prototyp.Modules
{
    public class Buffer_Module : NodeViewModel
    {
        static OSGeo.OGR.Driver driver = Ogr.GetDriverByName("Esri Shapefile");
        DataSource ds = null;
        Layer bufferLayer = null;
        SpatialReference srs = null;
        wkbGeometryType geom_type = wkbGeometryType.wkbPolygon;
        public IntegerValueEditorViewModel ValueEditor { get; } = new IntegerValueEditorViewModel();
        public ValueNodeInputViewModel<Layer> bufferNodeInput { get; }
        public ValueNodeInputViewModel<int> RadiusInput { get; }
        public ValueNodeOutputViewModel<Layer> bufferNodeOutput { get; }

        public Buffer_Module()
        {
            this.Name = "Buffer";
            long featureCount = 0;
            ds = driver.CreateDataSource("C:/Temp", new string[] { });
            bufferLayer = ds.CreateLayer("buffer", srs, geom_type, new string[] { });

            bufferNodeInput = new ValueNodeInputViewModel<Layer>();
            Layer inputValue = null;

            bufferNodeInput.ValueChanged.Subscribe(newValue =>
            {
                if (newValue != null)
                {
                    inputValue = newValue;
                    srs = newValue.GetSpatialRef();
                    bufferNodeInput.Name = newValue.GetName();
                    featureCount = newValue.GetFeatureCount(0);
                }
                    
            });

            

            this.Inputs.Add(bufferNodeInput);

            RadiusInput = new ValueNodeInputViewModel<int>();
            RadiusInput.Editor = ValueEditor;
            RadiusInput.Port.IsVisible = false;
            
            
            RadiusInput.ValueChanged.Subscribe(newValue =>
            {
                if (inputValue != null)
                {
                    for (int i = 0; i < featureCount; i++)
                    {
                        Feature feature = inputValue.GetFeature(i);
                        string test = "";
                        test += feature;
                        MessageBox.Show(test);
                        OSGeo.OGR.Geometry geom = feature.GetGeometryRef();
                        string test2 = "";
                        test2 += geom;
                        MessageBox.Show(test2);
                        double radius = Convert.ToDouble(newValue);
                        var bufferGeom = geom.Buffer(radius, 30);
                        string test3 = "";
                        test3 += bufferGeom;
                        MessageBox.Show(test3);
                        
                        FeatureDefn featureDefn = bufferLayer.GetLayerDefn();
                        Feature bufferFeature = new Feature(featureDefn);
                        bufferFeature.SetGeometry(bufferGeom);
                        bufferLayer.CreateFeature(bufferFeature);
                        bufferLayer.SetFeature(bufferFeature);
                        
                    }
                }
                else
                {
                    //string test = "";
                    //test += newValue;
                    //MessageBox.Show(test);
                };

            });
            this.Inputs.Add(RadiusInput);

        

            bufferNodeOutput = new ValueNodeOutputViewModel<Layer>();
            //bufferNodeOutput.Editor = ValueEditor;
            bufferNodeOutput.Value = System.Reactive.Linq.Observable.Return(bufferLayer);
            bufferNodeOutput.Name = "Buffer-Result";
            

            this.Outputs.Add(bufferNodeOutput);
        }

        static Buffer_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Buffer_Module>));
        }

    }
}
