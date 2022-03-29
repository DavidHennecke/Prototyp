using System;
using System.Windows;
using System.Windows.Controls;
using Prototyp.Modules.ViewModels;
using ReactiveUI;

namespace Prototyp.Modules.Views
{
    public partial class OutputNameView : UserControl, IViewFor<OutputNameViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(OutputNameViewModel), typeof(OutputNameView), new PropertyMetadata(null));

        public OutputNameViewModel ViewModel
        {
            get => (OutputNameViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (OutputNameViewModel)value;
        }
        #endregion

        public OutputNameView(string outputName)
        {
            InitializeComponent();
            this.outputName.Text = outputName;
            this.WhenActivated(d => d(
                this.Bind(ViewModel, vm => vm.Value, v => v.outputName.Text)
            ));
           

        }
    }
}
