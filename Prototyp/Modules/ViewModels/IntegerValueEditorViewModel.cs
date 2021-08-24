using NodeNetwork.Toolkit.ValueNode;
using Prototyp.Modules.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prototyp.Modules.ViewModels
{
    public class IntegerValueEditorViewModel : ValueEditorViewModel<int>
    {
        static IntegerValueEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new IntegerValueEditorView(), typeof(IViewFor<IntegerValueEditorViewModel>));
        }

        #region IntValue
        private int _intValue;
        public int IntValue
        {
            get => _intValue;
            set => this.RaiseAndSetIfChanged(ref _intValue, value);
        }
        #endregion

        public IntegerValueEditorViewModel()
        {
            this.WhenAnyValue(vm => vm.IntValue)
                .BindTo(this, vm => vm.Value);
        }
    }
}
