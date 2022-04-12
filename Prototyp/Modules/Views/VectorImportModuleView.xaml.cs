using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using Prototyp.Modules.ViewModels;
using ReactiveUI;

namespace Prototyp.Modules.Views
{
    /// <summary>
    /// Interaction logic for NodeModuleView.xaml
    /// </summary>
    public partial class VectorImportModuleView : IViewFor<VectorImport_Module>
    {

        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(VectorImport_Module), typeof(VectorImportModuleView), new PropertyMetadata(null));

        public VectorImport_Module ViewModel
        {
            get => (VectorImport_Module)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (VectorImport_Module)value;
        }
        

        #endregion
        public VectorImportModuleView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {           
                this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.NodeView.ViewModel).DisposeWith(d);
            });  
        }
    }
}
