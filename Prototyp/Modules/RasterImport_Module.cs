using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace Prototyp.Modules
{
    public class RasterImport_Module : NodeViewModel
    {
        public event System.EventHandler ProcessStatusChanged;
        public ValueNodeInputViewModel<string> importNodeInput { get; }
        public ValueNodeOutputViewModel<Prototyp.Elements.RasterData> importNodeOutput { get; }
        public double IntID { get; }
        public int Status;

        public void ChangeStatus(int statusNumber)
        {
            Status = statusNumber;
            ProcessStatusChanged?.Invoke(Status, System.EventArgs.Empty);
        }

        public RasterImport_Module(string dataName, string dataType, int dataID)
        {
            Name = dataName;
            IntID = dataID;
            importNodeOutput = new ValueNodeOutputViewModel<Prototyp.Elements.RasterData>();
            Outputs.Add(importNodeOutput);
            Prototyp.Elements.RasterData placeholder = new Prototyp.Elements.RasterData(-1);
            placeholder.Name = dataType;

            importNodeOutput.Name = dataType;
            importNodeOutput.Value = System.Reactive.Linq.Observable.Return(placeholder);
            importNodeOutput.SetDataID(dataID);
        }

        static RasterImport_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new Views.RasterImportModuleView(), typeof(IViewFor<RasterImport_Module>));
        }
    }
}