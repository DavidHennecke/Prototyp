using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using OSGeo.OGR;
using Prototyp.Modules.ViewModels;
using ReactiveUI;
using System;

namespace Prototyp.Modules
{
    public class Buffer_Module : NodeViewModel
    {
        public IntegerValueEditorViewModel ValueEditor { get; } = new IntegerValueEditorViewModel();
        public ValueNodeInputViewModel<Layer> bufferNodeInput { get; }
        public ValueListNodeInputViewModel<int> test { get; }
        public ValueNodeOutputViewModel<int> bufferNodeOutput { get; }

        public Buffer_Module()
        {
            this.Name = "Buffer";

            bufferNodeInput = new ValueNodeInputViewModel<Layer>();

            bufferNodeInput.ValueChanged.Subscribe(newValue =>
            {
                if (newValue != null)
                {
                    bufferNodeInput.Name = newValue.GetName();
                }
                    
            });

            this.Inputs.Add(bufferNodeInput);

            test = new ValueListNodeInputViewModel<int>();
            test.Editor = ValueEditor;
            test.Port.IsVisible = false;
            this.Inputs.Add(test);
        

            bufferNodeOutput = new ValueNodeOutputViewModel<int>();
            bufferNodeOutput.Editor = ValueEditor;
            bufferNodeOutput.Value = this.WhenAnyValue(vm => vm.ValueEditor.Value);
            bufferNodeOutput.Name = "Result";

            this.Outputs.Add(bufferNodeOutput);
        }

        static Buffer_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Buffer_Module>));
        }

    }
}
