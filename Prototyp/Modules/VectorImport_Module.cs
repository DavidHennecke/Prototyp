using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace Prototyp.Modules
{
    public class VectorImport_Module : NodeViewModel
    {
        public ValueNodeInputViewModel<string> importNodeInput { get; }
        public ValueNodeOutputViewModel<Prototyp.Elements.VectorData> importNodeOutput { get; }

        public VectorImport_Module(string dataName, string geomType, double dataID)
        {
            Name = dataName;
            importNodeOutput = new ValueNodeOutputViewModel<Prototyp.Elements.VectorData>();
            Outputs.Add(importNodeOutput);
            Prototyp.Elements.VectorData placeholder = new Prototyp.Elements.VectorData();
            placeholder.Name = geomType;

            importNodeOutput.Name = geomType;
            importNodeOutput.Value = System.Reactive.Linq.Observable.Return(placeholder);
            importNodeOutput.SetDataID(dataID);
        }

        static VectorImport_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeView(), typeof(IViewFor<VectorImport_Module>));
        }
    }
}