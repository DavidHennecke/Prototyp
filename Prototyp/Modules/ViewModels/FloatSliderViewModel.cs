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
        private float _minimumValue;
        private float _maximumValue;
        private float _tickValue;
        private string _unit;
        public float FloatValue
        {
            get => _floatValue;
            set => this.RaiseAndSetIfChanged(ref _floatValue, value);
        }

        public float MinimumValue
        {
            get => _minimumValue;
            set => this.RaiseAndSetIfChanged(ref _minimumValue, value);
        }

        public float MaximumValue
        {
            get => _maximumValue;
            set => this.RaiseAndSetIfChanged(ref _maximumValue, value);
        }

        public float TickValue
        {
            get => _tickValue;
            set => this.RaiseAndSetIfChanged(ref _tickValue, value);
        }
        public string Unit
        {
            get => _unit;
            set => this.RaiseAndSetIfChanged(ref _unit, value);
        }
        #endregion

        public FloatSliderViewModel()
        {
            this.WhenAnyValue(vm => vm.FloatValue)
                .BindTo(this, vm => vm.Value);
        }
    }
}
