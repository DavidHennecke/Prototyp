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
using System.IO;
using System.Reactive.Linq;
using System.Windows;

namespace Prototyp.Modules
{
    public class Buffer_Module : NodeViewModel
    {
        
        public IntegerValueEditorViewModel ValueEditor { get; } = new IntegerValueEditorViewModel();
        public ValueNodeInputViewModel<Layer> bufferNodeInput { get; }
        public ValueNodeInputViewModel<int> RadiusInput { get; }
        public ValueNodeOutputViewModel<Layer> bufferNodeOutput { get; }

        public Buffer_Module()
        {
            this.Name = "Buffer";
            long featureCount = 0;

            OSGeo.OGR.Driver driver = Ogr.GetDriverByName("Esri Shapefile");
            SpatialReference srs = new SpatialReference(null);
            srs.ImportFromEPSG(4326);
            wkbGeometryType geom_type = wkbGeometryType.wkbPolygon;
            string filepath = Path.GetTempPath();
            DataSource ds = driver.CreateDataSource(filepath, new string[] { });
            string filename = System.IO.Path.GetTempFileName();
            Layer bufferLayer = ds.CreateLayer(filename, srs, geom_type, new string[] { });
            var idField = new FieldDefn("id", FieldType.OFTInteger);
            bufferLayer.CreateField(idField, 1);
            var newCalc = 0;

            bufferNodeInput = new ValueNodeInputViewModel<Layer>();
            Layer inputValue = null;
            bufferNodeInput.ValueChanged.Subscribe(bufferSourceInput =>
            {
                if (bufferSourceInput != null)
                {
                    
                    inputValue = bufferSourceInput;
                    srs = bufferSourceInput.GetSpatialRef();
                    bufferNodeInput.Name = bufferSourceInput.GetName();
                    featureCount = bufferSourceInput.GetFeatureCount(0);
                    
                    
                }

            });
            



            this.Inputs.Add(bufferNodeInput);
           

            RadiusInput = new ValueNodeInputViewModel<int>();
            RadiusInput.Editor = ValueEditor;
            RadiusInput.Port.IsVisible = false;


            RadiusInput.ValueChanged.Subscribe(radiusInputValue =>
            {
                if (inputValue != null)
                {
                    if (newCalc > 0)
                    {
                        for (int i = 0; i < bufferLayer.GetFeatureCount(0); i++)
                        {
                            bufferLayer.DeleteFeature(i);
                        }
                        
                    }
                    for (int i = 0; i < featureCount; i++)
                    {

                        Feature feature = inputValue.GetFeature(i);

                        OSGeo.OGR.Geometry geom = feature.GetGeometryRef();
                        double radius = Convert.ToDouble(radiusInputValue);
                        var crs_source = geom.GetSpatialReference();

                        SpatialReference crs_25833 = new SpatialReference(null);
                        SpatialReference crs_4326 = new SpatialReference(null);
                        crs_25833.ImportFromEPSG(25833);
                        crs_4326.ImportFromEPSG(4326);
                      
                        geom.TransformTo(crs_25833);

                        var bufferGeom = geom.Buffer(radiusInputValue, 30);

                        bufferGeom.TransformTo(crs_4326);
                        bufferGeom.SwapXY();

                        //string test3 = "";
                        //test3 += bufferGeom.GetSpatialReference();
                        //MessageBox.Show(test3);
                        FeatureDefn featureDefn = bufferLayer.GetLayerDefn();
                        Feature bufferFeature = new Feature(featureDefn);
                        bufferFeature.SetFID(i);
                        bufferFeature.SetField("id", i);
                        bufferFeature.SetGeometryDirectly(bufferGeom);
                        bufferLayer.CreateFeature(bufferFeature);
                        bufferLayer.SyncToDisk();
                    }
                    newCalc = 1;
                };

            });
            this.Inputs.Add(RadiusInput);



            bufferNodeOutput = new ValueNodeOutputViewModel<Layer>();
            //bufferNodeOutput.Editor = ValueEditor;
            //bufferNodeOutput.Value = System.Reactive.Linq.Observable.Return(bufferLayer);
            bufferNodeOutput.Value = this.WhenAnyObservable(vm => vm.RadiusInput.ValueChanged)
                .Select(value => bufferLayer);
            bufferNodeOutput.Name += "Buffer-Result";
            this.Outputs.Add(bufferNodeOutput);
        }

        static Buffer_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Buffer_Module>));
        }

    }


}
