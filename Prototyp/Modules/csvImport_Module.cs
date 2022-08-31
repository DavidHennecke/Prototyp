using DynamicData;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prototyp.Modules
{
    public class csvImport_Module : NodeViewModel
    {
        public event System.EventHandler ProcessStatusChanged;
        public ValueNodeInputViewModel<string> importNodeInput { get; }
        public ValueNodeOutputViewModel<string> importNodeOutput { get; }
        public double IntID { get; }
        public int Status;

        public void ChangeStatus(int statusNumber)
        {
            Status = statusNumber;
            ProcessStatusChanged?.Invoke(Status, System.EventArgs.Empty);
        }

        public csvImport_Module(string dataName, string dataType, double dataID)
        {
            Name = dataName;
            IntID = dataID;
            importNodeOutput = new ValueNodeOutputViewModel<string>();
            Outputs.Add(importNodeOutput);

            importNodeOutput.Name = dataType;
            //importNodeOutput.Value = System.Reactive.Linq.Observable.Return(placeholder);
            importNodeOutput.SetDataID(dataID);
        }

        static csvImport_Module()
        {
            Splat.Locator.CurrentMutable.Register(() => new Views.csvImportModuleView(), typeof(IViewFor<csvImport_Module>));
        }

    }
}
