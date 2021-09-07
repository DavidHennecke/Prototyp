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
using System.Threading.Tasks;
using System.Windows;

namespace Prototyp.Modules
{
    public class WithIn_Module : NodeViewModel
    {

        public ValueNodeInputViewModel<Layer> withInSourceNodeInput { get; }
        public ValueNodeInputViewModel<Layer> withInMaskNodeInput { get; }
        public ValueNodeOutputViewModel<Layer> withInNodeOutput { get; }

        public WithIn_Module()
        {
            this.Name = "WithIn";
            

            OSGeo.OGR.Driver driver = Ogr.GetDriverByName("Esri Shapefile");
            SpatialReference srs = new SpatialReference(null);
            srs.ImportFromEPSG(4326);
            wkbGeometryType geom_type = wkbGeometryType.wkbPolygon;
            string filepath = Path.GetTempPath();
            DataSource ds = driver.CreateDataSource(filepath, new string[] { });
            string filename = System.IO.Path.GetTempFileName();
            Layer withInLayer = ds.CreateLayer(filename, srs, geom_type, new string[] { });
            var idField = new FieldDefn("id", FieldType.OFTInteger);
            withInLayer.CreateField(idField, 1);
            var newCalc = 0;

            
            Layer inputSourceValue = null;
            Layer inputMaskValue = null;

            withInSourceNodeInput = new ValueNodeInputViewModel<Layer>();
            withInSourceNodeInput.Name = "Source Input";
            withInSourceNodeInput.ValueChanged.Subscribe(async newValueSource =>
            {
                if (newValueSource != null)
                {
                    inputSourceValue = newValueSource;
                    if (newCalc > 0)
                    {
                        for (int i = 0; i < withInLayer.GetFeatureCount(0); i++)
                        {
                            withInLayer.DeleteFeature(i);
                        }
                        newCalc = 0;
                    }
                    //srs = newValueSource.GetSpatialRef();
                    withInSourceNodeInput.Name = newValueSource.GetName();
                    if (inputMaskValue != null)
                    {
                        Task<Layer> calc = calcWithIn(inputSourceValue, inputMaskValue, withInLayer);
                        withInLayer = await calc;
                //        withInNodeOutput.Value = this.WhenAnyObservable(vm => vm.withInMaskNodeInput.ValueChanged)
                //.Select(value => withInLayer);
                        newCalc = 1;
                    }
                }

            });

            this.Inputs.Add(withInSourceNodeInput);

            withInMaskNodeInput = new ValueNodeInputViewModel<Layer>();
            withInMaskNodeInput.Name = "Mask Input";
            withInMaskNodeInput.ValueChanged.Subscribe(async newValueMask =>
            {
                if (newValueMask != null)
                {
                    if (newCalc > 0)
                    {
                        for (int i = 0; i < withInLayer.GetFeatureCount(0); i++)
                        {
                            withInLayer.DeleteFeature(i);
                        }
                        newCalc = 0;
                    }

                    inputMaskValue = newValueMask;
                    withInMaskNodeInput.Name = newValueMask.GetName();

                    if (inputSourceValue != null)
                    {
                        Task<Layer> calc = calcWithIn(inputSourceValue, inputMaskValue, withInLayer);
                        withInLayer = await calc;
                //        withInNodeOutput.Value = this.WhenAnyObservable(vm => vm.withInSourceNodeInput.ValueChanged)
                //.Select(value => withInLayer);
                        newCalc = 1;
                    }
                }

            });

            this.Inputs.Add(withInMaskNodeInput);



            static async Task<Layer> calcWithIn (Layer inputSourceValue, Layer inputMaskValue, Layer withInLayer)
            {
                long featureCountSource = inputSourceValue.GetFeatureCount(0);
                long featureCountMask = inputMaskValue.GetFeatureCount(0);
                Feature maskFeature = inputMaskValue.GetNextFeature();
                Layer tempWithInLayer = withInLayer;
                while (maskFeature != null)
                {
                    
                    OSGeo.OGR.Geometry maskGeom = maskFeature.GetGeometryRef();          
                    var crs_mask = maskGeom.GetSpatialReference();
                  
                    Feature sourceFeature = inputSourceValue.GetNextFeature();
                    while (sourceFeature != null)
                    {
                        
                        Geometry sourceGeom = sourceFeature.GetGeometryRef();
                        var id = sourceFeature.GetFieldAsInteger(6);
                        
                        bool checkWithIn = sourceGeom.Within(maskGeom);
                        if (checkWithIn == true)
                        {
                            FeatureDefn featureDefn = withInLayer.GetLayerDefn();
                            Feature withInFeature = new Feature(featureDefn);
                            withInFeature.SetFID(id);
                            withInFeature.SetField("id", id);
                            
                            
                            withInFeature.SetGeometryDirectly(sourceGeom);
                            tempWithInLayer.CreateFeature(withInFeature);
                            tempWithInLayer.SyncToDisk();
                            
                        }
                        else
                        {
                            //string test3 = "";
                            //test3 += "ausserhalb";
                            //MessageBox.Show(test3);

                        }
                        sourceFeature = inputSourceValue.GetNextFeature();
                    }

                    maskFeature = inputMaskValue.GetNextFeature();
                }
                return tempWithInLayer;
            }



            withInNodeOutput = new ValueNodeOutputViewModel<Layer>();

            withInNodeOutput.Value = this.WhenAnyObservable(vm => vm.withInSourceNodeInput.ValueChanged)
    .Select(value => withInLayer);
            withInNodeOutput.Value = this.WhenAnyObservable(vm => vm.withInMaskNodeInput.ValueChanged)
    .Select(value => withInLayer);
            withInNodeOutput.Name += "WithIn-Result";
            this.Outputs.Add(withInNodeOutput);


            
        }

        static WithIn_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<WithIn_Module>));
        }

    }


}
