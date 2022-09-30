using System;
using System.Windows;
using System.Windows.Controls;
using Prototyp.Modules.ViewModels;
using ReactiveUI;

namespace Prototyp.Modules.Views
{
    public partial class DropDownMenuView : IViewFor<DropDownMenuViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(DropDownMenuViewModel), typeof(DropDownMenuView), new PropertyMetadata(null));

        public DropDownMenuViewModel ViewModel
        {
            get => (DropDownMenuViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (DropDownMenuViewModel)value;
        }
        #endregion

        public DropDownMenuView(string controlName, string[] items)
        {
            InitializeComponent();
            
            this.WhenActivated(d => {
            foreach (var item in ViewModel.StringItems)
                {
                    this.comboMenu.Items.Add(item);
                };
                this.Bind(ViewModel, vm => vm.StringItem, v => v.comboMenu.SelectedItem);
            });
        }
    }
}
