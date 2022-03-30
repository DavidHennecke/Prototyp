﻿using DynamicData;
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
    //Property Extension 
    //https://stackoverflow.com/questions/17616239/c-sharp-extend-class-by-adding-properties

    public static class NodeOutputViewModelExtension
    {
        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<NodeOutputViewModel, IntObject> IDs = new System.Runtime.CompilerServices.ConditionalWeakTable<NodeOutputViewModel, IntObject>();
        public static int GetID(this NodeOutputViewModel ID) { return IDs.GetOrCreateValue(ID).Value; }

        public static void SetID(this NodeOutputViewModel ID, int newID) { IDs.GetOrCreateValue(ID).Value = newID; }

        class IntObject
        {
            public int Value;
        }
    }

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
        public Modules.ViewModels.FloatSliderViewModel sliderEditor { get; }
        public Modules.ViewModels.OutputNameViewModel outNameEditor { get; }
        public Modules.ViewModels.DropDownMenuViewModel dropDownEditor { get; }
        public ValueNodeInputViewModel<Prototyp.Elements.VectorData> vectorInput { get; }
        public ValueNodeInputViewModel<Prototyp.Elements.RasterData> rasterInput { get; }
        public ValueNodeInputViewModel<float> valueFloatInput { get; }
        public ValueNodeInputViewModel<string> valueStringInput { get; }
        public ValueNodeOutputViewModel<Prototyp.Elements.VectorData> vectorOutput { get; }
        public ValueNodeOutputViewModel<Prototyp.Elements.RasterData> rasterOutput { get; }
        private GrpcClient.ControlConnector.ControlConnectorClient IntGrpcConnection;
        public string url;
        public string inputPortNr;
       


        // Getters and setters -------------------------------------------------------------

        public GrpcClient.ControlConnector.ControlConnectorClient grpcConnection
        {
            get { return (IntGrpcConnection); }
        }

        // Constructors --------------------------------------------------------------------

        // Parameterless constructor.
        public Node_Module()
        {
            // Nothing much to do here...
        }

        public Node_Module(string pathXML, GrpcClient.ControlConnector.ControlConnectorClient grpcConnection, string url)
        {
            VorteXML newModule = new VorteXML(pathXML);
            Name = newModule.NodeTitle;
            this.url = url;
            
            IntGrpcConnection = grpcConnection;

            foreach (VorteXML.ToolRow toolRow in newModule.ToolRows)
            {
                if (toolRow.rowType == VorteXML.RowType.Input)
                {
                    for (int i = 0; i < toolRow.inputRow.inputTypes.Length; i++)
                    {
                        if (toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.VectorLine | toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.VectorPoint | toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.VectorPolygon)
                        {
                            vectorInput = new ValueNodeInputViewModel<Prototyp.Elements.VectorData>();
                            vectorInput.SetID(i);
                            vectorInput.ValueChanged.Subscribe(vectorInputSource =>
                            {
                                if (vectorInputSource != null)
                                {
                                    vectorInput.Name = vectorInputSource.Name;
                                }
                            });
                            Inputs.Add(vectorInput);
                            break;
                        }
                        else if (toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.Raster)
                        {
                            rasterInput = new ValueNodeInputViewModel<Prototyp.Elements.RasterData>();
                            rasterInput.SetID(i);
                            rasterInput.ValueChanged.Subscribe(rasterInputSource =>
                            {
                                if (rasterInputSource != null)
                                {
                                    rasterInput.Name = rasterInputSource.Name;
                                }
                            });
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
                            vectorOutput.SetID(i);
                            VectorData result = new VectorData();
                            outNameEditor = new Modules.ViewModels.OutputNameViewModel(vectorOutput.Name);
                            outNameEditor.Value = "Vector output";
                            vectorOutput.Editor = outNameEditor;
                            outNameEditor.ValueChanged.Subscribe (v => { result.Name = v; });
                            vectorOutput.Value = this.WhenAnyObservable(vm => vm.outNameEditor.ValueChanged).Select(value => result);
                            Outputs.Add(vectorOutput);
                            
                            break;
                        }
                        else if (toolRow.outputRow.outputTypes[i] == VorteXML.ConnectorType.Raster)
                        {
                            rasterOutput = new ValueNodeOutputViewModel<Elements.RasterData>();
                            rasterOutput.SetID(i);
                            RasterData result = new RasterData();
                            outNameEditor = new Modules.ViewModels.OutputNameViewModel(rasterOutput.Name);
                            outNameEditor.Value = "Raster output";
                            rasterOutput.Editor = outNameEditor;
                            outNameEditor.ValueChanged.Subscribe(v => { result.Name = v; });
                            rasterOutput.Value = this.WhenAnyObservable(vm => vm.outNameEditor.ValueChanged).Select(value => result);
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
            System.Net.NetworkInformation.TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (System.Net.NetworkInformation.TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == Port)
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
