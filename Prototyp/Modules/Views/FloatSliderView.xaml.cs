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
            //this.slName.Text = controlName;
            this.slFloatValue.Minimum = minVal;
            this.slFloatValue.Maximum = maxVal;
            this.slFloatValue.TickFrequency = tick;
            this.slUnit.Text = unit;
            this.WhenActivated(d => d(
                this.Bind(ViewModel, vm => vm.FloatValue, v => v.slFloatValue.Value)
            ));
        }
    }
}
