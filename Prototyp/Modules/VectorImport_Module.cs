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
        public virtual double IntID { get; }
        public NodeProgress Status;

        public void ChangeStatus(NodeProgress statusNumber)
        {
            Status = statusNumber;
            ProcessStatusChanged?.Invoke(Status, System.EventArgs.Empty);
        }

        public VectorImport_Module()
        {
            // Nothing much to do here...
        }

        public VectorImport_Module(string dataName, string geomType, int dataID)
        {
            Name = dataName;
            IntID = dataID;
            importNodeOutput = new ValueNodeOutputViewModel<Prototyp.Elements.VectorData>();
            Outputs.Add(importNodeOutput);
            Prototyp.Elements.VectorData placeholder = new Prototyp.Elements.VectorData(-1);
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


    // Derived classes -------------------------------------------------------------------------

    public class VectorImport_ModulePoint : VectorImport_Module
    {
        public new ValueNodeOutputViewModel<Prototyp.Elements.VectorPointData> importNodeOutput { get; }
        public override double IntID { get; }

        public VectorImport_ModulePoint(string dataName, string geomType, int dataID)
        {
            Name = dataName;
            IntID = dataID;
            importNodeOutput = new ValueNodeOutputViewModel<Prototyp.Elements.VectorPointData>();
            Outputs.Add(importNodeOutput);
            Prototyp.Elements.VectorPointData placeholder = new Prototyp.Elements.VectorPointData(-1);
            placeholder.Name = geomType;

            importNodeOutput.Name = geomType;
            importNodeOutput.Value = System.Reactive.Linq.Observable.Return(placeholder);
            importNodeOutput.SetDataID(dataID);
        }

        static VectorImport_ModulePoint()
        {
            Splat.Locator.CurrentMutable.Register(() => new Views.VectorPointImportModuleView(), typeof(IViewFor<VectorImport_ModulePoint>));
        }
    }

    public class VectorImport_ModuleLine : VectorImport_Module
    {
        public new ValueNodeOutputViewModel<Prototyp.Elements.VectorLineData> importNodeOutput { get; }
        public override double IntID { get; }

        public VectorImport_ModuleLine(string dataName, string geomType, int dataID)
        {
            Name = dataName;
            IntID = dataID;
            importNodeOutput = new ValueNodeOutputViewModel<Prototyp.Elements.VectorLineData>();
            Outputs.Add(importNodeOutput);
            Prototyp.Elements.VectorLineData placeholder = new Prototyp.Elements.VectorLineData(-1);
            placeholder.Name = geomType;

            importNodeOutput.Name = geomType;
            importNodeOutput.Value = System.Reactive.Linq.Observable.Return(placeholder);
            importNodeOutput.SetDataID(dataID);
        }

        static VectorImport_ModuleLine()
        {
            Splat.Locator.CurrentMutable.Register(() => new Views.VectorLineImportModuleView(), typeof(IViewFor<VectorImport_ModuleLine>));
        }
    }

    public class VectorImport_ModulePolygon : VectorImport_Module
    {
        public new ValueNodeOutputViewModel<Prototyp.Elements.VectorPolygonData> importNodeOutput { get; }
        public override double IntID { get; }

        public VectorImport_ModulePolygon(string dataName, string geomType, int dataID)
        {
            Name = dataName;
            IntID = dataID;
            importNodeOutput = new ValueNodeOutputViewModel<Prototyp.Elements.VectorPolygonData>();
            Outputs.Add(importNodeOutput);
            Prototyp.Elements.VectorPolygonData placeholder = new Prototyp.Elements.VectorPolygonData(-1);
            placeholder.Name = geomType;

            importNodeOutput.Name = geomType;
            importNodeOutput.Value = System.Reactive.Linq.Observable.Return(placeholder);
            importNodeOutput.SetDataID(dataID);
        }

        static VectorImport_ModulePolygon()
        {
            Splat.Locator.CurrentMutable.Register(() => new Views.VectorPolygonImportModuleView(), typeof(IViewFor<VectorImport_ModulePolygon>));
        }
    }

    public class VectorImport_ModuleMultiPolygon : VectorImport_Module
    {
        public new ValueNodeOutputViewModel<Prototyp.Elements.VectorMultiPolygonData> importNodeOutput { get; }
        public override double IntID { get; }

        public VectorImport_ModuleMultiPolygon(string dataName, string geomType, int dataID)
        {
            Name = dataName;
            IntID = dataID;
            importNodeOutput = new ValueNodeOutputViewModel<Prototyp.Elements.VectorMultiPolygonData>();
            Outputs.Add(importNodeOutput);
            Prototyp.Elements.VectorMultiPolygonData placeholder = new Prototyp.Elements.VectorMultiPolygonData(-1);
            placeholder.Name = geomType;

            importNodeOutput.Name = geomType;
            importNodeOutput.Value = System.Reactive.Linq.Observable.Return(placeholder);
            importNodeOutput.SetDataID(dataID);
        }

        static VectorImport_ModuleMultiPolygon()
        {
            Splat.Locator.CurrentMutable.Register(() => new Views.VectorMultiPolygonImportModuleView(), typeof(IViewFor<VectorImport_ModuleMultiPolygon>));
        }
    }
}
