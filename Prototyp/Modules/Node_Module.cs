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
        public ValueNodeInputViewModel<Layer> layerInput { get; }
        public ValueNodeInputViewModel<float> valueFloatInput { get; }
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
                        //toDo: Integration Slider Eigenschaften in XML(Klasse)
                        sliderEditor = new FloatSliderViewModel(toolRow.Name, (float)100.0, (float)5000.0, (float)100.0, "m");
                        valueFloatInput.Editor = sliderEditor;
                        valueFloatInput.Port.IsVisible = false;
                        this.Inputs.Add(valueFloatInput);
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
