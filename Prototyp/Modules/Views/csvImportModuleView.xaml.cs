﻿using System;
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
    public partial class csvImportModuleView : IViewFor<csvImport_Module>
    {

        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(csvImport_Module), typeof(csvImportModuleView), new PropertyMetadata(null));

        public csvImport_Module ViewModel
        {
            get => (csvImport_Module)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (csvImport_Module)value;
        }
        

        #endregion
        public csvImportModuleView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {           
                this.WhenAnyValue(v => v.ViewModel).BindTo(this, v => v.NodeView.ViewModel).DisposeWith(d);
                this.ViewModel.ProcessStatusChanged += ViewModel_ProcessStatusChanged;



            });
        }

        private void ViewModel_ProcessStatusChanged(object sender, EventArgs e)
        {
            // TODO: Für Status besser Enum verwenden.

            if (this.ViewModel.Status == 0)
            {
                this.NodeView.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#FF212225");
            }
            else if (this.ViewModel.Status == 1)
            {
                this.NodeView.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#3b794e");
            }
            else if (this.ViewModel.Status == 2)
            {
                this.NodeView.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#793b3b");
            }
            else if (this.ViewModel.Status == 3)
            {
                this.NodeView.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#e6f0ef");
            }
            else if (this.ViewModel.Status == 4)
            {
                this.NodeView.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#e5a31f");
            }
            else if (this.ViewModel.Status == 5)
            {
                this.NodeView.Background = (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFromString("#345282");
            }
        }
    }
}
