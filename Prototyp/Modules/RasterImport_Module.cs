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
    public class RasterImport_Module : NodeViewModel
    {
        public ValueNodeInputViewModel<string> importNodeInput { get; }
        public ValueNodeOutputViewModel<Prototyp.Elements.RasterData> importNodeOutput { get; }

        public RasterImport_Module(Elements.RasterData data)
        {
            Name = "Dataset";
            importNodeOutput = new ValueNodeOutputViewModel<Prototyp.Elements.RasterData>();
            importNodeOutput.Value = null;
            importNodeOutput.Name = null;
            importNodeOutput.Name = data.GetType().ToString();
            importNodeOutput.Value = System.Reactive.Linq.Observable.Return(data);
            Outputs.Add(importNodeOutput);
        }

        static RasterImport_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<RasterImport_Module>));
        }
    }
}