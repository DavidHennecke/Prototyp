using System;
using System.Windows;
using System.Windows.Controls;
using Prototyp.Modules.ViewModels;
using ReactiveUI;

namespace Prototyp.Modules.Views
{
    public partial class DropDownMenuView : UserControl, IViewFor<DropDownMenuViewModel>
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
            //this.comboName.Text = controlName;
            foreach (string item in items)
            {
                this.comboMenu.Items.Add(item);
            }
            //string SelectedText = (string)this.comboMenu.SelectedItem;
        }
    }
}
