using System;
using System.Windows;
using System.Windows.Controls;
using Prototyp.Modules.ViewModels;
using ReactiveUI;

namespace Prototyp.Modules.Views
{
    public partial class FloatValueEditorView : UserControl, IViewFor<FloatValueEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(FloatValueEditorViewModel), typeof(FloatValueEditorView), new PropertyMetadata(null));

        public FloatValueEditorViewModel ViewModel
        {
            get => (FloatValueEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FloatValueEditorViewModel)value;
        }
        #endregion

        public FloatValueEditorView(string controlName, float minVal, float maxVal, float tick, string unit)
        {
            InitializeComponent();
            this.slName.Text = controlName;
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
