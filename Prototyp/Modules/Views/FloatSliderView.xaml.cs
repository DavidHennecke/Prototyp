using System;
using System.Windows;
using System.Windows.Controls;
using Prototyp.Modules.ViewModels;
using ReactiveUI;

namespace Prototyp.Modules.Views
{
    public partial class FloatSliderView : UserControl, IViewFor<FloatSliderViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(FloatSliderViewModel), typeof(FloatSliderView), new PropertyMetadata(null));

        public FloatSliderViewModel ViewModel
        {
            get => (FloatSliderViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FloatSliderViewModel)value;
        }
        #endregion

        public FloatSliderView(string controlName, float minVal, float maxVal, float tick, string unit)
        {
            InitializeComponent();
            this.WhenActivated(d => {
                this.Bind(ViewModel, vm => vm.FloatValue, v => v.slFloatValue.Value);
                this.Bind(ViewModel, vm => vm.Unit, v => v.slUnit.Text);
                this.Bind(ViewModel, vm => vm.MaximumValue, v => v.slFloatValue.Maximum);
                this.Bind(ViewModel, vm => vm.MinimumValue, v => v.slFloatValue.Minimum);
                this.Bind(ViewModel, vm => vm.TickValue, v => v.slFloatValue.TickFrequency);
            });
        }
    }
}
