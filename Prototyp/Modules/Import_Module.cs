using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prototyp.Modules
{
    public class Import_Module : NodeViewModel
    {
        public ValueNodeInputViewModel<string> importNodeInput { get; }
        public ValueNodeOutputViewModel<Prototyp.Elements.VectorData> importNodeOutput { get; }

        public Import_Module()
        {
            this.Name = "Dataset";
            importNodeOutput = new ValueNodeOutputViewModel<Prototyp.Elements.VectorData>();
            importNodeOutput.Value = null;
            importNodeOutput.Name = null;
            this.Outputs.Add(importNodeOutput);
        }

        static Import_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<Import_Module>));
        }
    }
}