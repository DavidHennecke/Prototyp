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
    public class VectorImport_Module : NodeViewModel
    {
        public ValueNodeInputViewModel<string> importNodeInput { get; }
        public ValueNodeOutputViewModel<Prototyp.Elements.VectorData> importNodeOutput { get; }

        public VectorImport_Module(string dataName, string geomType, int dataPointer)
        {
            Name = dataName;
            importNodeOutput = new ValueNodeOutputViewModel<Prototyp.Elements.VectorData>();
            importNodeOutput.Value = null;
            importNodeOutput.Name = null;
            Outputs.Add(importNodeOutput);
            Elements.VectorData data = new Elements.VectorData();
            data.Name = geomType;

            importNodeOutput.SetDataPointer(dataPointer);
            importNodeOutput.Name = geomType;
            importNodeOutput.Value = System.Reactive.Linq.Observable.Return(data);
        }

        static VectorImport_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<VectorImport_Module>));
        }
    }
}