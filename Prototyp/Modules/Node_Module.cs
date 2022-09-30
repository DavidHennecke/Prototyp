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
        public event EventHandler ProcessStatusChanged;

        public System.Collections.Generic.List<Modules.ViewModels.FloatSliderViewModel> sliderEditor = new System.Collections.Generic.List<Modules.ViewModels.FloatSliderViewModel>();
        public System.Collections.Generic.List<Modules.ViewModels.DropDownMenuViewModel> dropdownEditor = new System.Collections.Generic.List<Modules.ViewModels.DropDownMenuViewModel>();
        public Modules.ViewModels.DropDownMenuViewModel dropDownEditor { get; set; }

        public Modules.ViewModels.OutputNameViewModel outNameEditor { get; set; }

        public System.Collections.Generic.List<ValueNodeInputViewModel<float>> FloatInput = new System.Collections.Generic.List<ValueNodeInputViewModel<float>>();
        public System.Collections.Generic.List<ValueNodeInputViewModel<string>> StringInput = new System.Collections.Generic.List<ValueNodeInputViewModel<string>>();
        public System.Collections.Generic.List<ValueNodeOutputViewModel<float>> FloatOutput = new System.Collections.Generic.List<ValueNodeOutputViewModel<float>>();
        public System.Collections.Generic.List<ValueNodeOutputViewModel<string>> StringOutput = new System.Collections.Generic.List<ValueNodeOutputViewModel<string>>();

        public ValueNodeInputViewModel<Prototyp.Elements.VectorPointData> vectorInputPoint { get; set; }
        public ValueNodeInputViewModel<Prototyp.Elements.VectorLineData> vectorInputLine { get; set; }
        public ValueNodeInputViewModel<Prototyp.Elements.VectorPolygonData> vectorInputPolygon { get; set; }
        public ValueNodeInputViewModel<Prototyp.Elements.VectorMultiPolygonData> vectorInputMultiPolygon { get; set; }
        public ValueNodeInputViewModel<Prototyp.Elements.RasterData> rasterInput { get; set; }
        public ValueNodeInputViewModel<Prototyp.Elements.TableData> tableInput { get; set; }

        public ValueNodeOutputViewModel<Prototyp.Elements.VectorPointData> vectorOutputPoint { get; set; }
        public ValueNodeOutputViewModel<Prototyp.Elements.VectorLineData> vectorOutputLine { get; set; }
        public ValueNodeOutputViewModel<Prototyp.Elements.VectorPolygonData> vectorOutputPolygon { get; set; }
        public ValueNodeOutputViewModel<Prototyp.Elements.VectorMultiPolygonData> vectorOutputMultiPolygon { get; set; }
        public ValueNodeOutputViewModel<Prototyp.Elements.RasterData> rasterOutput { get; set; }
        public ValueNodeOutputViewModel<Prototyp.Elements.TableData> tableOutput { get; set; }

        private string _PathXML;
        private GrpcClient.ControlConnector.ControlConnectorClient _GrpcConnection;
        private System.Diagnostics.Process _Process;
        private string _Url;
        private bool _showGUI = false;

        public NodeProgress Status;

        [Serializable]
        public class SliderParams
        {
            public string Name;
            public string Value;
        }

        [Serializable]
        public class DropdownParams
        {
            public string Name;
            public string[] Entries;
        }

        [Serializable]
        public class ParamData
        {
            public SliderParams[] Slid;
            public DropdownParams[] DD;
        }

        public void ChangeStatus(NodeProgress statusNumber)
        {
            Status = statusNumber;
            ProcessStatusChanged?.Invoke(Status, EventArgs.Empty);
        }

        // Getters and setters -------------------------------------------------------------

        public string PathXML
        {
            get { return (_PathXML); }
            set { _PathXML = value; }
        }

        public GrpcClient.ControlConnector.ControlConnectorClient grpcConnection
        {
            get { return (_GrpcConnection); }
            set { _GrpcConnection = value; }
        }

        public System.Diagnostics.Process Process
        {
            get { return (_Process); }
        }

        public string Url
        {
            get { return (_Url); }
            set { _Url = value; }
        }

        public bool ShowGUI
        {
            get { return (_showGUI); }
            set { _showGUI = value; }
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
            _PathXML = pathXML;

            VorteXML newModule = new VorteXML(pathXML);

            Name = newModule.NodeTitle;

            _Url = url;
            _Process = process;            
            _GrpcConnection = grpcConnection;
            _showGUI = newModule.ShowGUI;
            Status = NodeProgress.Waiting; // Korrekt?

            ParseXML(newModule, true, ShowGUI);
        }

        // Used for workflow loading procedure.
        public Node_Module(VorteXML XML, string Title, GrpcClient.ControlConnector.ControlConnectorClient grpcConnection, string url, System.Diagnostics.Process process)
        {
            Name = Title;

            _Url = url;
            _Process = process;
            _GrpcConnection = grpcConnection;
            Status = NodeProgress.Waiting; // Korrekt?

            ParseXML(XML, true, ShowGUI);
        }

        // Used for the module designer preview.
        public Node_Module(VorteXML newModule)
        {
            Name = newModule.NodeTitle;

            ParseXML(newModule, false, ShowGUI);
        }

        // Private methods -----------------------------------------------------------------

        // Verändert, damit die Verlinkungen im Knoteneditor wieder funktionieren, muss nochmal geprüft/überarbeitet werden
        private void ParseXML(VorteXML newModule, bool inMain, bool GUI) //Use inMain = true for MainWindow node editor, inMain = false for ModuleDesigner preview.
        {
            _showGUI = GUI;

            //Counters for correctly tracking IDs
            int inputRowCounter = 0;
            int outputRowCounter = 0;
            foreach (VorteXML.ToolRow toolRow in newModule.ToolRows)
            {
                if (toolRow.rowType == VorteXML.RowType.Input)
                {
                    for (int i = 0; i < toolRow.inputRow.inputTypes.Length; i++)
                    {
                        if (toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.VectorPoint)
                        {
                            vectorInputPoint = new ValueNodeInputViewModel<Prototyp.Elements.VectorPointData>();
                            if (inMain)
                            {
                                vectorInputPoint.SetID(inputRowCounter);
                                vectorInputPoint.Name = toolRow.Name;
                                //vectorInputPoint.Name = toolRow.inputRow.inputTypes[i].ToString();
                            }
                            else
                            {
                                vectorInputPoint.Name = toolRow.Name;
                            }
                            Inputs.Add(vectorInputPoint);
                            continue;
                        }
                        else if (toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.VectorLine)
                        {
                            vectorInputLine = new ValueNodeInputViewModel<Prototyp.Elements.VectorLineData>();
                            if (inMain)
                            {
                                vectorInputLine.SetID(inputRowCounter);
                                vectorInputLine.Name = toolRow.Name;

                            }
                            else
                            {
                                vectorInputLine.Name = toolRow.Name;
                            }
                            Inputs.Add(vectorInputLine);
                            continue;
                        }
                        else if (toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.VectorPolygon)
                        {
                            vectorInputPolygon = new ValueNodeInputViewModel<Prototyp.Elements.VectorPolygonData>();
                            if (inMain)
                            {
                                vectorInputPolygon.SetID(inputRowCounter);
                                vectorInputPolygon.Name = toolRow.Name;

                            }
                            else
                            {
                                vectorInputPolygon.Name = toolRow.Name;
                            }
                            Inputs.Add(vectorInputPolygon);
                            continue;
                        }
                        else if (toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.VectorMultiPolygon)
                        {
                            vectorInputMultiPolygon = new ValueNodeInputViewModel<Prototyp.Elements.VectorMultiPolygonData>();
                            if (inMain)
                            {
                                vectorInputMultiPolygon.SetID(inputRowCounter);
                                vectorInputMultiPolygon.Name = toolRow.Name;

                            }
                            else
                            {
                                vectorInputMultiPolygon.Name = toolRow.Name;
                            }
                            Inputs.Add(vectorInputMultiPolygon);
                            continue;
                        }
                        else if (toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.Raster)
                        {
                            rasterInput = new ValueNodeInputViewModel<Prototyp.Elements.RasterData>();
                            if (inMain)
                            {
                                rasterInput.SetID(inputRowCounter);
                                rasterInput.Name = toolRow.Name;

                            }
                            else
                            {
                                rasterInput.Name = toolRow.Name;
                            }
                            Inputs.Add(rasterInput);
                            continue;
                        }
                        else if (toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.Table)
                        {
                            tableInput = new ValueNodeInputViewModel<Prototyp.Elements.TableData>();
                            if (inMain)
                            {
                                tableInput.SetID(inputRowCounter);
                                tableInput.Name = toolRow.Name;
                            }
                            else
                            {
                                tableInput.Name = toolRow.Name;
                            }
                            Inputs.Add(tableInput);
                            continue;
                        }
                        else if (toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.Float)
                        {
                            FloatInput.Add(new ValueNodeInputViewModel<float>());
                            FloatInput[FloatInput.Count - 1].SetID(inputRowCounter);
                            FloatInput[FloatInput.Count - 1].Name = toolRow.Name;

                            Inputs.Add(FloatInput[FloatInput.Count - 1]);
                            continue;
                        }
                        //... TODO: Support more types?
                        else
                        {
                            throw new System.Exception("No implemented input connector type specified.");
                        }
                    }
                    inputRowCounter++;
                }
                else if (toolRow.rowType == VorteXML.RowType.Output)
                {
                    for (int i = 0; i < toolRow.outputRow.outputTypes.Length; i++)
                    {
                        if (toolRow.outputRow.outputTypes[i] == VorteXML.ConnectorType.VectorPoint)
                        {
                            vectorOutputPoint = new ValueNodeOutputViewModel<Elements.VectorPointData>();
                            if (inMain)
                            {
                                vectorOutputPoint.SetID(outputRowCounter);
                                VectorPointData placeholder = new VectorPointData(-1);
                                // Name-Editor Implementation
                                //outNameEditor = new Modules.ViewModels.OutputNameViewModel(vectorOutput.Name);
                                //outNameEditor.Value = "Vector output";
                                //vectorOutput.Editor = outNameEditor;
                                //outNameEditor.ValueChanged.Subscribe (v => { result.Name = v; });
                                //vectorOutput.Value = this.WhenAnyObservable(vm => vm.outNameEditor.ValueChanged).Select(value => result);

                                // Alternativ: ...outputTypes.Last().ToString();
                                // Grundsätzlich: Was tun bei mehreren validen Outputtypen?
                                placeholder.Name = toolRow.outputRow.outputTypes[i].ToString();
                                vectorOutputPoint.Name = toolRow.outputRow.outputTypes[i].ToString();
                                vectorOutputPoint.Value = System.Reactive.Linq.Observable.Return(placeholder);
                            }
                            else
                            {
                                vectorOutputPoint.Name = toolRow.Name;
                            }
                            Outputs.Add(vectorOutputPoint);
                            continue;
                        }
                        else if (toolRow.outputRow.outputTypes[i] == VorteXML.ConnectorType.VectorLine)
                        {
                            vectorOutputLine = new ValueNodeOutputViewModel<Elements.VectorLineData>();
                            if (inMain)
                            {
                                vectorOutputLine.SetID(outputRowCounter);
                                VectorLineData placeholder = new VectorLineData(-1);
                                // Name-Editor Implementation
                                //outNameEditor = new Modules.ViewModels.OutputNameViewModel(vectorOutput.Name);
                                //outNameEditor.Value = "Vector output";
                                //vectorOutput.Editor = outNameEditor;
                                //outNameEditor.ValueChanged.Subscribe (v => { result.Name = v; });
                                //vectorOutput.Value = this.WhenAnyObservable(vm => vm.outNameEditor.ValueChanged).Select(value => result);

                                // Alternativ: ...outputTypes.Last().ToString();
                                // Grundsätzlich: Was tun bei mehreren validen Outputtypen?
                                placeholder.Name = toolRow.outputRow.outputTypes[i].ToString();
                                vectorOutputLine.Name = toolRow.outputRow.outputTypes[i].ToString();
                                vectorOutputLine.Value = System.Reactive.Linq.Observable.Return(placeholder);
                            }
                            else
                            {
                                vectorOutputLine.Name = toolRow.Name;
                            }
                            Outputs.Add(vectorOutputLine);
                            continue;
                        }
                        else if (toolRow.outputRow.outputTypes[i] == VorteXML.ConnectorType.VectorPolygon)
                        {
                            vectorOutputPolygon = new ValueNodeOutputViewModel<Elements.VectorPolygonData>();
                            if (inMain)
                            {
                                vectorOutputPolygon.SetID(outputRowCounter);
                                VectorPolygonData placeholder = new VectorPolygonData(-1);
                                // Name-Editor Implementation
                                //outNameEditor = new Modules.ViewModels.OutputNameViewModel(vectorOutput.Name);
                                //outNameEditor.Value = "Vector output";
                                //vectorOutput.Editor = outNameEditor;
                                //outNameEditor.ValueChanged.Subscribe (v => { result.Name = v; });
                                //vectorOutput.Value = this.WhenAnyObservable(vm => vm.outNameEditor.ValueChanged).Select(value => result);

                                // Alternativ: ...outputTypes.Last().ToString();
                                // Grundsätzlich: Was tun bei mehreren validen Outputtypen?
                                placeholder.Name = toolRow.outputRow.outputTypes[i].ToString();
                                vectorOutputPolygon.Name = toolRow.outputRow.outputTypes[i].ToString();
                                vectorOutputPolygon.Value = System.Reactive.Linq.Observable.Return(placeholder);
                            }
                            else
                            {
                                vectorOutputPolygon.Name = toolRow.Name;
                            }
                            Outputs.Add(vectorOutputPolygon);
                            continue;
                        }
                        else if (toolRow.outputRow.outputTypes[i] == VorteXML.ConnectorType.VectorMultiPolygon)
                        {
                            vectorOutputMultiPolygon = new ValueNodeOutputViewModel<Elements.VectorMultiPolygonData>();
                            if (inMain)
                            {
                                vectorOutputMultiPolygon.SetID(outputRowCounter);
                                VectorMultiPolygonData placeholder = new VectorMultiPolygonData(-1);
                                // Name-Editor Implementation
                                //outNameEditor = new Modules.ViewModels.OutputNameViewModel(vectorOutput.Name);
                                //outNameEditor.Value = "Vector output";
                                //vectorOutput.Editor = outNameEditor;
                                //outNameEditor.ValueChanged.Subscribe (v => { result.Name = v; });
                                //vectorOutput.Value = this.WhenAnyObservable(vm => vm.outNameEditor.ValueChanged).Select(value => result);

                                // Alternativ: ...outputTypes.Last().ToString();
                                // Grundsätzlich: Was tun bei mehreren validen Outputtypen?
                                placeholder.Name = toolRow.outputRow.outputTypes[i].ToString();
                                vectorOutputMultiPolygon.Name = toolRow.outputRow.outputTypes[i].ToString();
                                vectorOutputMultiPolygon.Value = System.Reactive.Linq.Observable.Return(placeholder);
                            }
                            else
                            {
                                vectorOutputMultiPolygon.Name = toolRow.Name;
                            }
                            Outputs.Add(vectorOutputMultiPolygon);
                            continue;
                        }
                        else if (toolRow.outputRow.outputTypes[i] == VorteXML.ConnectorType.Raster)
                        {
                            rasterOutput = new ValueNodeOutputViewModel<Elements.RasterData>();
                            if (inMain)
                            {
                                rasterOutput.SetID(outputRowCounter);
                                RasterData placeholder = new RasterData(-1);
                                // Name-Editor Implementation
                                //outNameEditor = new Modules.ViewModels.OutputNameViewModel(rasterOutput.Name);
                                //outNameEditor.Value = "Raster output";
                                //rasterOutput.Editor = outNameEditor;
                                //outNameEditor.ValueChanged.Subscribe(v => { result.Name = v; });
                                //rasterOutput.Value = this.WhenAnyObservable(vm => vm.outNameEditor.ValueChanged).Select(value => result);

                                // Alternativ: ...outputTypes.Last().ToString();
                                // Grundsätzlich: Was tun bei mehreren validen Outputtypen?
                                placeholder.Name = toolRow.outputRow.outputTypes[i].ToString();
                                rasterOutput.Name = toolRow.outputRow.outputTypes[i].ToString();
                                rasterOutput.Value = System.Reactive.Linq.Observable.Return(placeholder);
                            }
                            else
                            {
                                rasterOutput.Name = toolRow.Name;
                            }
                            Outputs.Add(rasterOutput);
                            continue;
                        }
                        else if (toolRow.outputRow.outputTypes[i] == VorteXML.ConnectorType.Table)
                        {
                            tableOutput = new ValueNodeOutputViewModel<Elements.TableData>();
                            if (inMain)
                            {
                                tableOutput.SetID(outputRowCounter);
                                TableData placeholder = new TableData(-1);

                                placeholder.Name = toolRow.outputRow.outputTypes[i].ToString();
                                tableOutput.Name = toolRow.outputRow.outputTypes[i].ToString();
                                tableOutput.Value = System.Reactive.Linq.Observable.Return(placeholder);
                            }
                            else
                            {
                                tableOutput.Name = toolRow.Name;
                            }
                            Outputs.Add(tableOutput);
                            continue;
                        }
                        else if (toolRow.outputRow.outputTypes[i] == VorteXML.ConnectorType.Float)
                        {
                            float placeholder = new float();
                            
                            FloatOutput.Add(new ValueNodeOutputViewModel<float>());
                            FloatOutput[FloatOutput.Count - 1].SetID(outputRowCounter);
                            FloatOutput[FloatOutput.Count - 1].Name = toolRow.outputRow.outputTypes[i].ToString();
                            FloatOutput[FloatOutput.Count - 1].Value = System.Reactive.Linq.Observable.Return(placeholder);

                            Outputs.Add(FloatOutput[FloatOutput.Count - 1]);
                            continue;
                        }
                        //... TODO: Support more types?
                        else
                        {
                            throw new System.Exception("An unimplemented output connector type was specified.");
                        }
                    }
                    outputRowCounter++;
                }
                else if (toolRow.rowType == VorteXML.RowType.Control)
                {
                    if (toolRow.controlRow.controlType == VorteXML.ControlType.Slider)
                    {
                        /////////// Work in progress start

                        sliderEditor.Add(new Modules.ViewModels.FloatSliderViewModel(toolRow.Name, toolRow.controlRow.slider.Start, toolRow.controlRow.slider.End, toolRow.controlRow.slider.TickFrequency, toolRow.controlRow.slider.Unit));
                        sliderEditor[sliderEditor.Count - 1].FloatValue = toolRow.controlRow.slider.Default;
                        sliderEditor[sliderEditor.Count - 1].MinimumValue = toolRow.controlRow.slider.Start;
                        sliderEditor[sliderEditor.Count - 1].MaximumValue = toolRow.controlRow.slider.End;
                        sliderEditor[sliderEditor.Count - 1].TickValue = toolRow.controlRow.slider.TickFrequency;
                        sliderEditor[sliderEditor.Count - 1].Unit = toolRow.controlRow.slider.Unit;

                        FloatInput.Add(new ValueNodeInputViewModel<float>());
                        FloatInput[FloatInput.Count - 1].Editor = sliderEditor[sliderEditor.Count - 1];
                        FloatInput[FloatInput.Count - 1].Port.IsVisible = true;
                        FloatInput[FloatInput.Count - 1].Name = toolRow.Name;
                        FloatInput[FloatInput.Count - 1].SetID(inputRowCounter);

                        Inputs.Add(FloatInput[FloatInput.Count - 1]);

                        /////////// Work in progress end
                        
                        //valueFloatInput.ValueChanged.Subscribe(newValue =>
                        //{
                        //    if (newValue != null)
                        //    {
                                    //hier sendChange grpc
                        //    }
                        //});                        
                    }
                    else if (toolRow.controlRow.controlType == VorteXML.ControlType.Dropdown)
                    {
                        dropdownEditor.Add(new Modules.ViewModels.DropDownMenuViewModel(toolRow.Name, toolRow.controlRow.dropdown.Values));
                        //dropdownEditor[dropdownEditor.Count - 1].StringItems = toolRow.controlRow.dropdown.Values;

                        StringInput.Add(new ValueNodeInputViewModel<string>());
                        StringInput[StringInput.Count - 1].Editor = dropdownEditor[StringInput.Count - 1];
                        StringInput[StringInput.Count - 1].Port.IsVisible = false;
                        StringInput[StringInput.Count - 1].Name = toolRow.Name;
                        StringInput[StringInput.Count - 1].SetID(inputRowCounter);
                        Inputs.Add(StringInput[StringInput.Count - 1]);
                    }
                    inputRowCounter++;
                }
            }
        }

        // Public methods ------------------------------------------------------------------

        public Google.Protobuf.WellKnownTypes.Struct ParamsToProtobufStruct()
        {
            var Params = new Google.Protobuf.WellKnownTypes.Struct();

            foreach (NodeInputViewModel i in Inputs.Items)
            {
                if (i.Editor != null)
                {
                    if (i.Editor is Prototyp.Modules.ViewModels.FloatSliderViewModel)
                    {
                        Params.Fields.Add(i.Name, Google.Protobuf.WellKnownTypes.Value.ForNumber(((ViewModels.FloatSliderViewModel)i.Editor).FloatValue));
                    }
                    else if (i.Editor is Prototyp.Modules.ViewModels.DropDownMenuViewModel)
                    {
                        if (((Prototyp.Modules.ViewModels.DropDownMenuViewModel)i.Editor).StringItem != null)
                        {
                            Params.Fields.Add(i.Name, Google.Protobuf.WellKnownTypes.Value.ForString(((Prototyp.Modules.ViewModels.DropDownMenuViewModel)i.Editor).StringItem));
                        }
                        else
                        {
                            Params.Fields.Add(i.Name, Google.Protobuf.WellKnownTypes.Value.ForString(""));
                        }
                        
                        // TODO: Welchen String schicken, d.h. welcher ist im UI ausgewählt?
                        // Müsste in etwa so gehen:

                        // string SelectedText = (string)this.comboMenu.SelectedItem;

                        // 'this' müsste dabei etwa sein: Prototyp.Modules.Views.DropDownMenuView
                        // siehe Konstruktor in DropDownMenuView.xaml.cs
                    }
                }
            }
            return Params;
        }

        public string ParamsToJson() /////////// Work in progress
        {
            int SliderCount = 0;
            int DropDownCount = 0;

            foreach (NodeInputViewModel i in Inputs.Items)
            {
                if (i.Editor != null)
                {
                    string MyName = i.Name;
                    if (i.Editor is Prototyp.Modules.ViewModels.FloatSliderViewModel)
                    {
                        SliderCount++;
                    }
                    else if (i.Editor is Prototyp.Modules.ViewModels.DropDownMenuViewModel)
                    {
                        DropDownCount++;
                    }
                }
            }
            
            ParamData Params = new ParamData();
            Params.Slid = new SliderParams[SliderCount];
            Params.DD = new DropdownParams[DropDownCount];

            SliderCount = 0;
            DropDownCount = 0;
            foreach (NodeInputViewModel i in Inputs.Items)
            {
                if (i.Editor != null)
                {
                    string MyName = i.Name;
                    if (i.Editor is Prototyp.Modules.ViewModels.FloatSliderViewModel)
                    {
                        Params.Slid[SliderCount] = new SliderParams();

                        Params.Slid[SliderCount].Name = i.Name;
                        Params.Slid[SliderCount].Value = ((Prototyp.Modules.ViewModels.FloatSliderViewModel)i.Editor).FloatValue.ToString();
                        
                        SliderCount++;
                    }
                    else if (i.Editor is Prototyp.Modules.ViewModels.DropDownMenuViewModel)
                    {
                        Params.DD[DropDownCount] = new DropdownParams();

                        Params.DD[DropDownCount].Name = i.Name;
                        //Params.DD[DropDownCount].Entries = ((Prototyp.Modules.ViewModels.DropDownMenuViewModel)i.Editor).StringItems;

                        DropDownCount++;
                    }
                }
            }

            return (Newtonsoft.Json.JsonConvert.SerializeObject(Params));
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

        public static int GetNextPort(int StartPort)
        {
            int port = StartPort;
            while (!Node_Module.PortAvailable(port)) port++;
            if (port >= MainWindow.MAX_UNSIGNED_SHORT) throw new System.Exception("Could not find any free port.");

            return (port);
        }

        static Node_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new Views.NodeModuleView(), typeof(IViewFor<Node_Module>));
        }
    }
}
