using NodeNetwork.Toolkit.ValueNode;
using Prototyp.Modules.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prototyp.Modules.ViewModels
{
    public class DropDownMenuViewModel : ValueEditorViewModel<string>
    {
        public DropDownMenuViewModel(string controlName, string[] items)
        {
            Splat.Locator.CurrentMutable.Register(() => new DropDownMenuView(controlName, items), typeof(IViewFor<DropDownMenuViewModel>));
        }

        #region FloatValue
        private float _floatValue;
        public float FloatValue
        {
            get => _floatValue;
            set => this.RaiseAndSetIfChanged(ref _floatValue, value);
        }
        #endregion

        public DropDownMenuViewModel()
        {
            this.WhenAnyValue(vm => vm.FloatValue)
                .BindTo(this, vm => vm.Value);
        }
    }
}
