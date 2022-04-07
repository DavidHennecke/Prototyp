using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using Prototyp.Elements;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Windows;

namespace Prototyp.Modules
{
    //Property extensions, see https://stackoverflow.com/questions/17616239/c-sharp-extend-class-by-adding-properties

    // Extend OUTput
    public static class NodeOutputViewModelExtension
    {
        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<NodeOutputViewModel, IntObject> IDs = new System.Runtime.CompilerServices.ConditionalWeakTable<NodeOutputViewModel, IntObject>();
        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<NodeOutputViewModel, DoubleObject> DataIDs = new System.Runtime.CompilerServices.ConditionalWeakTable<NodeOutputViewModel, DoubleObject>();
        public static int GetID(this NodeOutputViewModel ID) { return IDs.GetOrCreateValue(ID).iValue; }
        public static void SetID(this NodeOutputViewModel ID, int newID) { IDs.GetOrCreateValue(ID).iValue = newID; }
        public static double GetDataID(this NodeOutputViewModel DataID) { return DataIDs.GetOrCreateValue(DataID).dValue; }
        public static void SetDataID(this NodeOutputViewModel DataID, double newDataID) { DataIDs.GetOrCreateValue(DataID).dValue = newDataID; }

        class IntObject
        {
            public int iValue;
        }
        class DoubleObject
        {
            public double dValue;
        }
    }

    // Extend INput
    public static class NodeInputViewModelExtension
    {
        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<NodeInputViewModel, IntObject> IDs = new System.Runtime.CompilerServices.ConditionalWeakTable<NodeInputViewModel, IntObject>();
        public static int GetID(this NodeInputViewModel ID) { return IDs.GetOrCreateValue(ID).Value; }
        public static void SetID(this NodeInputViewModel ID, int newID) { IDs.GetOrCreateValue(ID).Value = newID; }

        class IntObject
        {
            public int Value;
        }
    }

    public class Node_Module : NodeViewModel
    {
        public Modules.ViewModels.FloatSliderViewModel sliderEditor { get; set; }
        public Modules.ViewModels.OutputNameViewModel outNameEditor { get; set; }
        public Modules.ViewModels.DropDownMenuViewModel dropDownEditor { get; set; }
        public ValueNodeInputViewModel<Prototyp.Elements.VectorData> vectorInput { get; set; }
        public ValueNodeInputViewModel<Prototyp.Elements.RasterData> rasterInput { get; set; }
        public ValueNodeInputViewModel<float> valueFloatInput { get; set; }
        public ValueNodeInputViewModel<string> valueStringInput { get; set; }
        public ValueNodeOutputViewModel<Prototyp.Elements.VectorData> vectorOutput { get; set; }
        public ValueNodeOutputViewModel<Prototyp.Elements.RasterData> rasterOutput { get; set; }
        
        private GrpcClient.ControlConnector.ControlConnectorClient IntGrpcConnection;
        private System.Diagnostics.Process IntProcess;
        public string url;
        
        // Getters and setters -------------------------------------------------------------

        public GrpcClient.ControlConnector.ControlConnectorClient grpcConnection
        {
            get { return (IntGrpcConnection); }
        }

        public System.Diagnostics.Process Process
        {
            get { return (IntProcess); }
        }

        // Constructors --------------------------------------------------------------------

        // Parameterless constructor.
        public Node_Module()
        {
            // Nothing much to do here...
        }

        // Used for actually adding something to the main window node editor.
        public Node_Module(string pathXML, GrpcClient.ControlConnector.ControlConnectorClient grpcConnection, string url, System.Diagnostics.Process process)
        {
            VorteXML newModule = new VorteXML(pathXML);
            Name = newModule.NodeTitle;
            this.url = url;
            this.IntProcess = process;
            
            IntGrpcConnection = grpcConnection;

            ParseXML(newModule, true);
        }

        // Used for the module designer preview.
        public Node_Module(VorteXML newModule)
        {
            Name = newModule.NodeTitle;

            ParseXML(newModule, false);
        }

        // Private methods -----------------------------------------------------------------

        private void ParseXML(VorteXML newModule, bool inMain) //Use inMain = true for MainWindow node editor, inMain = false for ModuleDesigner preview.
        {
            foreach (VorteXML.ToolRow toolRow in newModule.ToolRows)
            {
                if (toolRow.rowType == VorteXML.RowType.Input)
                {
                    for (int i = 0; i < toolRow.inputRow.inputTypes.Length; i++)
                    {
                        if (toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.VectorLine | toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.VectorPoint | toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.VectorPolygon)
                        {
                            vectorInput = new ValueNodeInputViewModel<Prototyp.Elements.VectorData>();
                            if (inMain)
                            {
                                vectorInput.SetID(i);
                                vectorInput.ValueChanged.Subscribe(vectorInputSource =>
                                {
                                    if (vectorInputSource != null)
                                    {
                                        vectorInput.Name = vectorInputSource.Name;
                                    }
                                });
                            }
                            else
                            {
                                vectorInput.Name = toolRow.Name;
                            }
                            Inputs.Add(vectorInput);
                            break;
                        }
                        else if (toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.Raster)
                        {
                            rasterInput = new ValueNodeInputViewModel<Prototyp.Elements.RasterData>();
                            if (inMain)
                            {

                                rasterInput.SetID(i);
                                rasterInput.ValueChanged.Subscribe(rasterInputSource =>
                                {
                                    if (rasterInputSource != null)
                                    {
                                        rasterInput.Name = rasterInputSource.Name;
                                    }
                                });
                            }
                            else
                            {
                                rasterInput.Name = toolRow.Name;
                            }
                            Inputs.Add(rasterInput);
                            break;
                        }
                        //... TODO: Support more types?
                        else
                        {
                            throw new System.Exception("No implemented input connector type specified.");
                        }
                    }
                }
                else if (toolRow.rowType == VorteXML.RowType.Output)
                {
                    for (int i = 0; i < toolRow.outputRow.outputTypes.Length; i++)
                    {
                        if (toolRow.outputRow.outputTypes[i] == VorteXML.ConnectorType.VectorLine | toolRow.outputRow.outputTypes[i] == VorteXML.ConnectorType.VectorPoint | toolRow.outputRow.outputTypes[i] == VorteXML.ConnectorType.VectorPolygon)
                        {
                            vectorOutput = new ValueNodeOutputViewModel<Elements.VectorData>();
                            if (inMain)
                            {
                                vectorOutput.SetID(i);
                                VectorData placeholder = new VectorData();
                                // Name-Editor Implementation
                                //outNameEditor = new Modules.ViewModels.OutputNameViewModel(vectorOutput.Name);
                                //outNameEditor.Value = "Vector output";
                                //vectorOutput.Editor = outNameEditor;
                                //outNameEditor.ValueChanged.Subscribe (v => { result.Name = v; });
                                //vectorOutput.Value = this.WhenAnyObservable(vm => vm.outNameEditor.ValueChanged).Select(value => result);
                                placeholder.Name = toolRow.outputRow.outputTypes[i].ToString();
                                vectorOutput.Name = toolRow.outputRow.outputTypes[i].ToString();
                                vectorOutput.Value = System.Reactive.Linq.Observable.Return(placeholder);
                            }
                            else
                            {
                                vectorOutput.Name = toolRow.Name;
                            }
                            Outputs.Add(vectorOutput);

                            break;
                        }
                        else if (toolRow.outputRow.outputTypes[i] == VorteXML.ConnectorType.Raster)
                        {
                            rasterOutput = new ValueNodeOutputViewModel<Elements.RasterData>();
                            if (inMain)
                            {
                                rasterOutput.SetID(i);
                                RasterData placeholder = new RasterData();
                                // Name-Editor Implementation
                                //outNameEditor = new Modules.ViewModels.OutputNameViewModel(rasterOutput.Name);
                                //outNameEditor.Value = "Raster output";
                                //rasterOutput.Editor = outNameEditor;
                                //outNameEditor.ValueChanged.Subscribe(v => { result.Name = v; });
                                //rasterOutput.Value = this.WhenAnyObservable(vm => vm.outNameEditor.ValueChanged).Select(value => result);

                                placeholder.Name = toolRow.outputRow.outputTypes[i].ToString();
                                rasterOutput.Name = toolRow.outputRow.outputTypes[i].ToString();
                                rasterOutput.Value = System.Reactive.Linq.Observable.Return(placeholder);
                            }
                            else
                            {
                                rasterOutput.Name = toolRow.Name;
                            }
                            Outputs.Add(rasterOutput);
                            break;
                        }
                        //... TODO: Support more types?
                        else
                        {
                            throw new System.Exception("An unimplemented output connector type was specified.");
                        }
                    }
                }
                else if (toolRow.rowType == VorteXML.RowType.Control)
                {
                    if (toolRow.controlRow.controlType == VorteXML.ControlType.Slider)
                    {
                        valueFloatInput = new ValueNodeInputViewModel<float>();
                        sliderEditor = new Modules.ViewModels.FloatSliderViewModel(toolRow.Name, toolRow.controlRow.slider.Start, toolRow.controlRow.slider.End, toolRow.controlRow.slider.TickFrequency, toolRow.controlRow.slider.Unit);
                        valueFloatInput.Editor = sliderEditor;
                        valueFloatInput.Port.IsVisible = false;
                        Inputs.Add(valueFloatInput);
                    }
                    else if (toolRow.controlRow.controlType == VorteXML.ControlType.Dropdown)
                    {
                        valueStringInput = new ValueNodeInputViewModel<string>();
                        dropDownEditor = new Modules.ViewModels.DropDownMenuViewModel(toolRow.Name, toolRow.controlRow.dropdown.Values);
                        valueStringInput.Editor = dropDownEditor;
                        valueStringInput.Port.IsVisible = false;
                        Inputs.Add(valueStringInput);
                    }
                }
            }
        }

        // Static methods ------------------------------------------------------------------

        public static bool PortAvailable(int Port) // Source: https://stackoverflow.com/questions/570098/in-c-how-to-check-if-a-tcp-port-is-available
        {
            System.Net.NetworkInformation.IPGlobalProperties ipGlobalProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
            System.Net.IPEndPoint[] listeners = ipGlobalProperties.GetActiveTcpListeners();
                        
            foreach (System.Net.IPEndPoint l in listeners)
            {
                if (l.Port == Port)
                {
                    return (false);
                }
            }

            return (true);
        }

        static Node_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Node_Module>));
        }
    }
}
