using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prototyp.Modules
{
    public class TableImport_Module : NodeViewModel
    {
        public event System.EventHandler ProcessStatusChanged;
        public ValueNodeInputViewModel<string> importNodeInput { get; }
        public ValueNodeOutputViewModel<Prototyp.Elements.TableData> importNodeOutput { get; }
        public double IntID { get; }
        public int Status;

        public void ChangeStatus(int statusNumber)
        {
            Status = statusNumber;
            ProcessStatusChanged?.Invoke(Status, System.EventArgs.Empty);
        }

        public TableImport_Module(string dataName, string dataType, int dataID)
        {
            Name = dataName;
            IntID = dataID;
            importNodeOutput = new ValueNodeOutputViewModel<Prototyp.Elements.TableData>();
            Outputs.Add(importNodeOutput);
            Prototyp.Elements.TableData placeholder = new Prototyp.Elements.TableData(-1);
            placeholder.Name = dataType;

            importNodeOutput.Name = dataType;
            importNodeOutput.Value = System.Reactive.Linq.Observable.Return(placeholder);
            importNodeOutput.SetDataID(dataID);
        }
        static TableImport_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new Views.csvImportModuleView(), typeof(IViewFor<TableImport_Module>));
        }

    }
}
