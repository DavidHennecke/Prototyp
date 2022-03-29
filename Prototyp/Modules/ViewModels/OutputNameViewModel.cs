using NodeNetwork.Toolkit.ValueNode;
using Prototyp.Modules.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prototyp.Modules.ViewModels
{
    public class OutputNameViewModel : ValueEditorViewModel<string>
    {
        public OutputNameViewModel(string outputName)
        {
            Splat.Locator.CurrentMutable.Register(() => new OutputNameView(outputName), typeof(IViewFor<OutputNameViewModel>));
        }

        #region StringValue
        private string _stringValue;
        public string StringValue
        {
            get => _stringValue;
            set => this.RaiseAndSetIfChanged(ref _stringValue, value);
        }
        #endregion

        public OutputNameViewModel()
        {
            this.WhenAnyValue(vm => vm.StringValue)
                .BindTo(this, vm => vm.Value);
        }
    }
}
