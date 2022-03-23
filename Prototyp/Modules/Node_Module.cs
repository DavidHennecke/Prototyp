﻿using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using Prototyp.Elements;
using Prototyp.Modules.ViewModels;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Windows;

namespace Prototyp.Modules
{
    public class Node_Module : NodeViewModel
    {
        public FloatSliderViewModel sliderEditor { get; }
        public DropDownMenuViewModel dropDownEditor { get; }
        public ValueNodeInputViewModel<Prototyp.Elements.VectorData> vectorInput { get; }
        public ValueNodeInputViewModel<Prototyp.Elements.RasterData> rasterInput { get; }
        public ValueNodeInputViewModel<float> valueFloatInput { get; }
        public ValueNodeInputViewModel<string> valueStringInput { get; }
        public ValueNodeOutputViewModel<Prototyp.Elements.VectorData> vectorOutput { get; }
        public ValueNodeOutputViewModel<Prototyp.Elements.RasterData> rasterOutput { get; }
        public int port { get; }
        public GrpcServer.ControlConnector.ControlConnectorClient grpcConnection { get; }

        public Node_Module(string pathXML, int port, GrpcServer.ControlConnector.ControlConnectorClient grpcConnection)
        {
            VorteXML newModule = new VorteXML(pathXML);
            this.Name = newModule.NodeTitle;
            this.port = port;
            this.grpcConnection = grpcConnection;

            foreach (VorteXML.ToolRow toolRow in newModule.ToolRows)
            {
                if (toolRow.rowType == VorteXML.RowType.Input)
                {
                    for (int i = 0; i < toolRow.inputRow.inputTypes.Length; i++)
                    {
                        if (toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.VectorLine | toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.VectorPoint | toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType.VectorPolygon)
                        {
                            vectorInput = new ValueNodeInputViewModel<Prototyp.Elements.VectorData>();
                            vectorInput.ValueChanged.Subscribe(vectorInputSource =>
                            {
                                if (vectorInputSource != null)
                                {
                                    vectorInput.Name = vectorInputSource.Name;
                                }
                            });
                            this.Inputs.Add(vectorInput);
                            break;
                        }
                        //else if (toolRow.inputRow.inputTypes[i] == VorteXML.ConnectorType...) //TODO: z.B. Raster-Typ
                        //{
                            //rasterInput = new ValueNodeInputViewModel<Prototyp.Elements.RasterData>();
                            //rasterInput.ValueChanged.Subscribe(rasterInputSource =>
                            //{
                            //    if (rasterInputSource != null)
                            //    {
                            //        rasterInput.Name = rasterInputSource.Name;
                            //    }
                            //});
                            //this.Inputs.Add(rasterInput);
                            //break;
                        //}
                        //...
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
                            this.Outputs.Add(vectorOutput);
                            break;
                        }
                        //else if (toolRow.outputRow.outputTypes[i] == VorteXML.ConnectorType...) //TODO: z.B. Raster-Typ
                        //{
                        //    rasterOutput = new ValueNodeOutputViewModel<Elements.RasterData>();
                        //    this.Outputs.Add(rasterOutput);
                        //    break;
                        //}
                        //...
                        else
                        {
                            throw new System.Exception("No implemented output connector type specified.");
                        }
                    }
                }
                else if (toolRow.rowType == VorteXML.RowType.Control)
                {
                    if (toolRow.controlRow.controlType == VorteXML.ControlType.Slider)
                    {
                        valueFloatInput = new ValueNodeInputViewModel<float>();
                        sliderEditor = new FloatSliderViewModel(toolRow.Name, toolRow.controlRow.slider.Start, toolRow.controlRow.slider.End, toolRow.controlRow.slider.TickFrequency, toolRow.controlRow.slider.Unit);
                        valueFloatInput.Editor = sliderEditor;
                        valueFloatInput.Port.IsVisible = false;
                        this.Inputs.Add(valueFloatInput);
                    }
                    else if (toolRow.controlRow.controlType == VorteXML.ControlType.Dropdown)
                    {
                        valueStringInput = new ValueNodeInputViewModel<string>();
                        dropDownEditor = new DropDownMenuViewModel(toolRow.Name, toolRow.controlRow.dropdown.Values);
                        valueStringInput.Editor = dropDownEditor;
                        valueStringInput.Port.IsVisible = false;
                        this.Inputs.Add(valueStringInput);
                    }
                }
            }
        }

        static Node_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Node_Module>));
        }
    }
}
