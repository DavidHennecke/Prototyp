using DynamicData;
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
        public ValueNodeInputViewModel<float> valueFloatInput { get; }
        public ValueNodeInputViewModel<string> valueStringInput { get; }
        public ValueNodeOutputViewModel<Prototyp.Elements.VectorData> vectorOutput { get; }

        public Node_Module(string pathXML)
        {
            VorteXML newModule = new VorteXML(pathXML);
            this.Name = newModule.NodeTitle;

            foreach (VorteXML.ToolRow toolRow in newModule.ToolRows)
            {
                if (toolRow.rowType == VorteXML.RowType.Input)
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
                }
                else if (toolRow.rowType == VorteXML.RowType.Output)
                {
                    vectorOutput = new ValueNodeOutputViewModel<Elements.VectorData>();
                    this.Outputs.Add(vectorOutput);
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
