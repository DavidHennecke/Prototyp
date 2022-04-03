using System.Windows;

namespace Prototyp
{
    /// <summary>
    /// Interaction logic for ModuleDesigner.xaml
    /// </summary>
    public partial class ModuleDesigner : Window
    {
        public class ListViewEntry
        {
            public string SlotType { get; set; }
            public string RowName;
            
            public System.Collections.Generic.List<string> InputTypes = new System.Collections.Generic.List<string>();

            public string ControlType;

            public float SliderStart;
            public float SliderEnd;
            public float SliderDefault;
            public float SliderTickFrequency;
            public string SliderUnit;

            public System.Collections.Generic.List<string> DropDownEntries = new System.Collections.Generic.List<string>();

            public System.Collections.Generic.List<string> OutputTypes = new System.Collections.Generic.List<string>();
        }

        public System.Collections.Generic.List<ListViewEntry> ListViewEntries = new System.Collections.Generic.List<ListViewEntry>();

        public bool OkayClicked;

        public ModuleDesigner()
        {
            InitializeComponent();

            MyListBox.ItemsSource = ListViewEntries;
        }

        private void CmdAddInput_Click(object sender, RoutedEventArgs e)
        {
            ListViewEntries.Add(new ListViewEntry() { SlotType = "Input" });
            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;
        }

        private void CmdAddControl_Click(object sender, RoutedEventArgs e)
        {
            ControlTypeSelector controlTypeSelector = new ControlTypeSelector();
            controlTypeSelector.Owner = this;
            controlTypeSelector.ShowDialog();

            if ((bool)controlTypeSelector.RdoSlider.IsChecked)
            {
                ListViewEntries.Add(new ListViewEntry() { SlotType = "Control (slider)" });
                ListViewEntries[ListViewEntries.Count - 1].ControlType = "Slider";
            }
            else if ((bool)controlTypeSelector.RdoDropdown.IsChecked)
            {
                ListViewEntries.Add(new ListViewEntry() { SlotType = "Control (dropdown)" });
                ListViewEntries[ListViewEntries.Count - 1].ControlType = "Dropdown";
            }

            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;
        }

        private void CmdAddOutput_Click(object sender, RoutedEventArgs e)
        {
            ListViewEntries.Add(new ListViewEntry() { SlotType = "Output" });
            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;
        }

        private void CmdEditProperties_Click(object sender, RoutedEventArgs e)
        {
            if (MyListBox.SelectedIndex == -1) return;

            ModuleProperties moduleProperties = new ModuleProperties();
            moduleProperties.Owner = this;

            if (ListViewEntries[MyListBox.SelectedIndex].RowName != null) moduleProperties.TxtRowName.Text = ListViewEntries[MyListBox.SelectedIndex].RowName;

            if (ListViewEntries[MyListBox.SelectedIndex].SlotType == "Input")
            {
                moduleProperties.TxtSliderStart.IsEnabled = false;
                moduleProperties.TxtSliderEnd.IsEnabled = false;
                moduleProperties.TxtSliderDefault.IsEnabled = false;
                moduleProperties.TxtSliderTick.IsEnabled = false;
                moduleProperties.TxtSliderUnit.IsEnabled = false;
                moduleProperties.TxtDropdownEntries.IsEnabled = false;
                moduleProperties.ChkOutVectorPoint.IsEnabled = false;
                moduleProperties.ChkOutVectorLine.IsEnabled = false;
                moduleProperties.ChkOutVectorPolygon.IsEnabled = false;
                moduleProperties.ChkOutRaster.IsEnabled = false;

                foreach (string InpTy in ListViewEntries[MyListBox.SelectedIndex].InputTypes) { if (InpTy == "VectorPoint") moduleProperties.ChkInVectorPoint.IsChecked = true; }
                foreach (string InpTy in ListViewEntries[MyListBox.SelectedIndex].InputTypes) { if (InpTy == "VectorLine") moduleProperties.ChkInVectorLine.IsChecked = true; }
                foreach (string InpTy in ListViewEntries[MyListBox.SelectedIndex].InputTypes) { if (InpTy == "VectorPolygon") moduleProperties.ChkInVectorPolygon.IsChecked = true; }
                foreach (string InpTy in ListViewEntries[MyListBox.SelectedIndex].InputTypes) { if (InpTy == "Raster") moduleProperties.ChkInRaster.IsChecked = true; }

                moduleProperties.ShowDialog();
                if (!moduleProperties.OkayClicked) return;

                ListViewEntries[MyListBox.SelectedIndex].RowName = moduleProperties.TxtRowName.Text;

                if ((bool)moduleProperties.ChkInVectorPoint.IsChecked) ListViewEntries[MyListBox.SelectedIndex].InputTypes.Add("VectorPoint");
                if ((bool)moduleProperties.ChkInVectorLine.IsChecked) ListViewEntries[MyListBox.SelectedIndex].InputTypes.Add("VectorLine");
                if ((bool)moduleProperties.ChkInVectorPolygon.IsChecked) ListViewEntries[MyListBox.SelectedIndex].InputTypes.Add("VectorPolygon");
                if ((bool)moduleProperties.ChkInRaster.IsChecked) ListViewEntries[MyListBox.SelectedIndex].InputTypes.Add("Raster");
            }
            else if (ListViewEntries[MyListBox.SelectedIndex].SlotType.Contains("Control"))
            {
                if (ListViewEntries[MyListBox.SelectedIndex].ControlType == "Slider")
                {
                    moduleProperties.ChkInVectorPoint.IsEnabled = false;
                    moduleProperties.ChkInVectorLine.IsEnabled = false;
                    moduleProperties.ChkInVectorPolygon.IsEnabled = false;
                    moduleProperties.ChkInRaster.IsEnabled = false;
                    moduleProperties.TxtDropdownEntries.IsEnabled = false;
                    moduleProperties.ChkOutVectorPoint.IsEnabled = false;
                    moduleProperties.ChkOutVectorLine.IsEnabled = false;
                    moduleProperties.ChkOutVectorPolygon.IsEnabled = false;
                    moduleProperties.ChkOutRaster.IsEnabled = false;

                    if (ListViewEntries[MyListBox.SelectedIndex].SliderUnit != null)
                    {
                        moduleProperties.TxtSliderStart.Text = ListViewEntries[MyListBox.SelectedIndex].SliderStart.ToString();
                        moduleProperties.TxtSliderEnd.Text = ListViewEntries[MyListBox.SelectedIndex].SliderEnd.ToString();
                        moduleProperties.TxtSliderDefault.Text = ListViewEntries[MyListBox.SelectedIndex].SliderDefault.ToString();
                        moduleProperties.TxtSliderTick.Text = ListViewEntries[MyListBox.SelectedIndex].SliderTickFrequency.ToString();
                        moduleProperties.TxtSliderUnit.Text = ListViewEntries[MyListBox.SelectedIndex].SliderUnit;
                    }

                    moduleProperties.ShowDialog();
                    if (!moduleProperties.OkayClicked) return;

                    ListViewEntries[MyListBox.SelectedIndex].RowName = moduleProperties.TxtRowName.Text;

                    ListViewEntries[MyListBox.SelectedIndex].SliderStart = float.Parse(moduleProperties.TxtSliderStart.Text);
                    ListViewEntries[MyListBox.SelectedIndex].SliderEnd = float.Parse(moduleProperties.TxtSliderEnd.Text);
                    ListViewEntries[MyListBox.SelectedIndex].SliderDefault = float.Parse(moduleProperties.TxtSliderDefault.Text);
                    ListViewEntries[MyListBox.SelectedIndex].SliderTickFrequency = float.Parse(moduleProperties.TxtSliderTick.Text);
                    ListViewEntries[MyListBox.SelectedIndex].SliderUnit = moduleProperties.TxtSliderUnit.Text;
                }
                else if (ListViewEntries[MyListBox.SelectedIndex].ControlType == "Dropdown")
                {
                    moduleProperties.ChkInVectorPoint.IsEnabled = false;
                    moduleProperties.ChkInVectorLine.IsEnabled = false;
                    moduleProperties.ChkInVectorPolygon.IsEnabled = false;
                    moduleProperties.ChkInRaster.IsEnabled = false;
                    moduleProperties.TxtSliderStart.IsEnabled = false;
                    moduleProperties.TxtSliderEnd.IsEnabled = false;
                    moduleProperties.TxtSliderDefault.IsEnabled = false;
                    moduleProperties.TxtSliderTick.IsEnabled = false;
                    moduleProperties.TxtSliderUnit.IsEnabled = false;
                    moduleProperties.ChkOutVectorPoint.IsEnabled = false;
                    moduleProperties.ChkOutVectorLine.IsEnabled = false;
                    moduleProperties.ChkOutVectorPolygon.IsEnabled = false;
                    moduleProperties.ChkOutRaster.IsEnabled = false;

                    moduleProperties.TxtDropdownEntries.Text = string.Join(System.Environment.NewLine, ListViewEntries[MyListBox.SelectedIndex].DropDownEntries);

                    moduleProperties.ShowDialog();
                    if (!moduleProperties.OkayClicked) return;

                    ListViewEntries[MyListBox.SelectedIndex].RowName = moduleProperties.TxtRowName.Text;

                    string[] DropDownList = moduleProperties.TxtDropdownEntries.Text.Split(System.Environment.NewLine);
                    foreach (string LstStr in DropDownList) ListViewEntries[MyListBox.SelectedIndex].DropDownEntries.Add(LstStr);
                }
            }
            else if (ListViewEntries[MyListBox.SelectedIndex].SlotType == "Output")
            {
                moduleProperties.ChkInVectorPoint.IsEnabled = false;
                moduleProperties.ChkInVectorLine.IsEnabled = false;
                moduleProperties.ChkInVectorPolygon.IsEnabled = false;
                moduleProperties.ChkInRaster.IsEnabled = false;
                moduleProperties.TxtSliderStart.IsEnabled = false;
                moduleProperties.TxtSliderEnd.IsEnabled = false;
                moduleProperties.TxtSliderDefault.IsEnabled = false;
                moduleProperties.TxtSliderTick.IsEnabled = false;
                moduleProperties.TxtSliderUnit.IsEnabled = false;
                moduleProperties.TxtDropdownEntries.IsEnabled = false;

                foreach (string OutTy in ListViewEntries[MyListBox.SelectedIndex].OutputTypes) { if (OutTy == "VectorPoint") moduleProperties.ChkOutVectorPoint.IsChecked = true; }
                foreach (string OutTy in ListViewEntries[MyListBox.SelectedIndex].OutputTypes) { if (OutTy == "VectorLine") moduleProperties.ChkOutVectorLine.IsChecked = true; }
                foreach (string OutTy in ListViewEntries[MyListBox.SelectedIndex].OutputTypes) { if (OutTy == "VectorPolygon") moduleProperties.ChkOutVectorPolygon.IsChecked = true; }
                foreach (string OutTy in ListViewEntries[MyListBox.SelectedIndex].OutputTypes) { if (OutTy == "Raster") moduleProperties.ChkOutRaster.IsChecked = true; }

                moduleProperties.ShowDialog();
                if (!moduleProperties.OkayClicked) return;

                ListViewEntries[MyListBox.SelectedIndex].RowName = moduleProperties.TxtRowName.Text;

                if ((bool)moduleProperties.ChkOutVectorPoint.IsChecked) ListViewEntries[MyListBox.SelectedIndex].OutputTypes.Add("VectorPoint");
                if ((bool)moduleProperties.ChkOutVectorLine.IsChecked) ListViewEntries[MyListBox.SelectedIndex].OutputTypes.Add("VectorLine");
                if ((bool)moduleProperties.ChkOutVectorPolygon.IsChecked) ListViewEntries[MyListBox.SelectedIndex].OutputTypes.Add("VectorPolygon");
                if ((bool)moduleProperties.ChkOutRaster.IsChecked) ListViewEntries[MyListBox.SelectedIndex].OutputTypes.Add("Raster");
            }
        }

        private void CmdDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MyListBox.SelectedIndex == -1) return;

            ListViewEntries.RemoveAt(MyListBox.SelectedIndex);
            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;
        }

        private void CmdUp_Click(object sender, RoutedEventArgs e)
        {
            if (MyListBox.SelectedIndex <= 0) return;

            int SwapPos = MyListBox.SelectedIndex;

            ListViewEntry Temp = ListViewEntries[SwapPos - 1];
            ListViewEntries[SwapPos - 1] = ListViewEntries[SwapPos];
            ListViewEntries[SwapPos] = Temp;

            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;

            MyListBox.SelectedIndex = SwapPos - 1;
        }

        private void CmdDown_Click(object sender, RoutedEventArgs e)
        {
            if (MyListBox.SelectedIndex >= ListViewEntries.Count - 1) return;

            int SwapPos = MyListBox.SelectedIndex;

            ListViewEntry Temp = ListViewEntries[SwapPos + 1];
            ListViewEntries[SwapPos + 1] = ListViewEntries[SwapPos];
            ListViewEntries[SwapPos] = Temp;

            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;

            MyListBox.SelectedIndex = SwapPos + 1;
        }

        private void CmdClear_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewEntries.Count == 0) return;
            if (MessageBox.Show("Are you sure?", this.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

            ListViewEntries.Clear();

            MyListBox.ItemsSource = null;
            MyListBox.ItemsSource = ListViewEntries;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnOkay_Click(object sender, RoutedEventArgs e)
        {
            OkayClicked = true;
            this.Close();
        }
    }
}
