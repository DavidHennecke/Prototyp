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

        public RasterImport_Module(string dataName, string dataType, double dataID)
        {
            Name = dataName;
            importNodeOutput = new ValueNodeOutputViewModel<Prototyp.Elements.RasterData>();
            importNodeOutput.Value = null;
            importNodeOutput.Name = null;
            Outputs.Add(importNodeOutput);
            Elements.RasterData placeholder = new Elements.RasterData();
            placeholder.Name = dataType;

            importNodeOutput.SetDataID(dataID);
            importNodeOutput.Name = dataType;
            importNodeOutput.Value = System.Reactive.Linq.Observable.Return(placeholder);
        }

        static RasterImport_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<RasterImport_Module>));
        }
    }
}