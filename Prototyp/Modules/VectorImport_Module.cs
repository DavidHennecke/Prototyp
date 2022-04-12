using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace Prototyp.Modules
{
    public class VectorImport_Module : NodeViewModel
    {
        public event System.EventHandler ProcessStatusChanged;
        public ValueNodeInputViewModel<string> importNodeInput { get; }
        public ValueNodeOutputViewModel<Prototyp.Elements.VectorData> importNodeOutput { get; }
        public double IntID { get; }
        public int Status;

        public void ChangeStatus(int statusNumber)
        {
            Status = statusNumber;
            ProcessStatusChanged?.Invoke(Status, System.EventArgs.Empty);
        }

        public VectorImport_Module(string dataName, string geomType, double dataID)
        {
            Name = dataName;
            IntID = dataID;
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
            Splat.Locator.CurrentMutable.Register(() => new Views.VectorImportModuleView(), typeof(IViewFor<VectorImport_Module>));
        }
    }
}