using NodeNetwork.Toolkit.ValueNode;
using Prototyp.Modules.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prototyp.Modules.ViewModels
{
    public class FloatSliderViewModel : ValueEditorViewModel<float>
    {
        public FloatSliderViewModel(string controlName, float minVal, float maxVal, float tick, string unit)
        {
            Splat.Locator.CurrentMutable.Register(() => new FloatSliderView(controlName, minVal, maxVal, tick, unit), typeof(IViewFor<FloatSliderViewModel>));
        }

        #region FloatValue
        private float _floatValue;
        public float FloatValue
        {
            get => _floatValue;
            set => this.RaiseAndSetIfChanged(ref _floatValue, value);
        }
        #endregion

        public FloatSliderViewModel()
        {
            this.WhenAnyValue(vm => vm.FloatValue)
                .BindTo(this, vm => vm.Value);
        }
    }
}
