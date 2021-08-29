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
using System.Reactive.Linq;
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
            DataSource ds = null;
            Layer withInLayer = null;
            SpatialReference srs = new SpatialReference(null);
            srs.ImportFromEPSG(4326);
            wkbGeometryType geom_type = wkbGeometryType.wkbPolygon;
            ds = driver.CreateDataSource("/vsimem", new string[] { });
            //ds = driver.CreateDataSource("C:/Temp/Test", new string[] { });
            withInLayer = ds.CreateLayer("withIn", srs, geom_type, new string[] { });
            var idField = new FieldDefn("id", FieldType.OFTInteger);
            withInLayer.CreateField(idField, 1);
            var newCalc = 0;

            
            Layer inputSourceValue = null;
            Layer inputMaskValue = null;

            withInSourceNodeInput = new ValueNodeInputViewModel<Layer>();
            withInSourceNodeInput.Name = "Source Input";
            withInSourceNodeInput.ValueChanged.Subscribe(newValue =>
            {
                if (newValue != null)
                {
                    inputSourceValue = newValue;
                    srs = newValue.GetSpatialRef();
                    withInSourceNodeInput.Name = newValue.GetName();
                    if (inputMaskValue != null)
                    {
                        calcWithIn(inputSourceValue, inputMaskValue, withInLayer);
                    }
                }

            });

            this.Inputs.Add(withInSourceNodeInput);



            withInMaskNodeInput = new ValueNodeInputViewModel<Layer>();
            withInMaskNodeInput.Name = "Mask Input";
            withInMaskNodeInput.ValueChanged.Subscribe(newValue =>
            {
                if (newValue != null)
                {
                    inputMaskValue = newValue;
                    withInMaskNodeInput.Name = newValue.GetName();

                    if (inputSourceValue != null)
                    {
                        calcWithIn(inputSourceValue, inputMaskValue, withInLayer);
                    }
                }

            });

            this.Inputs.Add(withInMaskNodeInput);



            static void calcWithIn (Layer inputSourceValue, Layer inputMaskValue, Layer withInLayer)
            {
                long featureCountSource = inputSourceValue.GetFeatureCount(0);
                long featureCountMask = inputMaskValue.GetFeatureCount(0);
                for (int i = 0; i < featureCountMask; i++)
                {

                    Feature maskFeature = inputMaskValue.GetFeature(i);
                    OSGeo.OGR.Geometry maskGeom = maskFeature.GetGeometryRef();          
                    var crs_mask = maskGeom.GetSpatialReference();

                    SpatialReference crs_25833 = new SpatialReference(null);
                    SpatialReference crs_4326 = new SpatialReference(null);
                    crs_25833.ImportFromEPSG(25833);
                    crs_4326.ImportFromEPSG(4326);
                    //CoordinateTransformation maskCt = new CoordinateTransformation(crs_mask, crs_25833, new CoordinateTransformationOptions());
                    //maskGeom.Transform(maskCt);
                    string test1 = "";
                    test1 += "Mask";
                    MessageBox.Show(test1);



                    for (int a = 0; a < featureCountSource; a++)
                    {
                        Feature sourceFeature = inputSourceValue.GetFeature(a);
                        Geometry sourceGeom = sourceFeature.GetGeometryRef();
                        //var crs_source = sourceGeom.GetSpatialReference();
                        //CoordinateTransformation sourceCt = new CoordinateTransformation(crs_source, crs_25833, new CoordinateTransformationOptions());
                        //sourceGeom.TransformTo(crs_25833);
                        //sourceGeom.TransformTo(crs_4326);
                        //string test2 = "";
                        //test2 += "Source";
                        //MessageBox.Show(test2);


                        bool checkWithIn = sourceGeom.Within(maskGeom);
                        if (checkWithIn == true)
                        {
                            FeatureDefn featureDefn = withInLayer.GetLayerDefn();
                            Feature withInFeature = new Feature(featureDefn);
                            withInFeature.SetFID(a);
                            withInFeature.SetField("id", a);

                            //CoordinateTransformation ct4326 = new CoordinateTransformation(sourceGeom.GetSpatialReference(), crs_4326, new CoordinateTransformationOptions());
                            //sourceGeom.Transform(ct4326);
                            withInFeature.SetGeometryDirectly(sourceGeom);
                            withInLayer.CreateFeature(withInFeature);
                            withInLayer.SyncToDisk();
                            //string test3 = "";
                            //test3 += "sync";
                            //MessageBox.Show(test3);
                        }
                    }
                    
                    
                }
                
            }



            withInNodeOutput = new ValueNodeOutputViewModel<Layer>();
            withInNodeOutput.Value = this.WhenAnyObservable(vm => vm.withInMaskNodeInput.ValueChanged)
                .Select(value => withInLayer);
            withInNodeOutput.Value = this.WhenAnyObservable(vm => vm.withInSourceNodeInput.ValueChanged)
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
