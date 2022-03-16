using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
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
        public ValueNodeInputViewModel<Layer> layerInput { get; }
        public ValueNodeInputViewModel<float> valueFloatInput { get; }
        public ValueNodeInputViewModel<string> valueStringInput { get; }
        public ValueNodeOutputViewModel<Layer> layerOutput { get; }

        public Node_Module(string pathXML)
        {
            VorteXML newModule = new VorteXML(pathXML);
            this.Name = newModule.NodeTitle;

            foreach (VorteXML.ToolRow toolRow in newModule.ToolRows)
            {
                if (toolRow.rowType == VorteXML.RowType.Input)
                {
                    layerInput = new ValueNodeInputViewModel<Layer>();
                    layerInput.ValueChanged.Subscribe(layerInputSource =>
                    {
                        if (layerInputSource != null)
                        {
                            layerInput.Name = layerInputSource.GetName();
                        }

                    });

                    this.Inputs.Add(layerInput);
                }

                else if (toolRow.rowType == VorteXML.RowType.Output)
                {
                    layerOutput = new ValueNodeOutputViewModel<Layer>();
                    this.Outputs.Add(layerOutput);
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
