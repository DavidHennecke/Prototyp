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
    public partial class RasterImportModuleView : IViewFor<RasterImport_Module>
    {

        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(RasterImport_Module), typeof(RasterImportModuleView), new PropertyMetadata(null));

        public RasterImport_Module ViewModel
        {
            get => (RasterImport_Module)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (RasterImport_Module)value;
        }
        

        #endregion
        public RasterImportModuleView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {           
                this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.NodeView.ViewModel).DisposeWith(d);
            });  
        }
    }
}
