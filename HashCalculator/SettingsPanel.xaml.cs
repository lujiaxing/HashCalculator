using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HashCalculator;

/// <summary>
/// SettingsPanel.xaml 的交互逻辑
/// </summary>
public partial class SettingsPanel : Window
{
    public SettingsPanel()
    {
        this.InitializeComponent();
        this.InitializeFromConfigure(Settings.Current);
        this.uiComboBox_SearchPolicy1.SelectionChanged += this.ComboBoxes_ActivateApplyButton_SelectionChanged;
        this.uiComboBox_SearchPolicy2.SelectionChanged += this.ComboBoxes_ActivateApplyButton_SelectionChanged;
        this.uiComboBox_SimulCalculate.SelectionChanged += this.ComboBoxes_ActivateApplyButton_SelectionChanged;
    }

    private void InitializeFromConfigure(Configure config)
    {
        this.uiCheckBox_RembMainSize.IsChecked = config.SaveMainWindowSize;
        this.uiComboBox_SearchPolicy1.SelectedIndex = (int)config.FileSearchPolicy;
        this.uiCheckBox_UseLowercaseHash.IsChecked = config.UseLowercaseHash;
        this.uiCheckBox_RemMainWinPos.IsChecked = config.SaveMainWindowPosition;
        this.uiComboBox_SimulCalculate.SelectedIndex = (int)config.TaskNumber;
        this.uiComboBox_SearchPolicy2.SelectedIndex = (int)config.QuickVerifyFileSearchPolicy;
        this.uiCHeckBox_ShowResultText.IsChecked = config.ShowResultText;
        this.uiCHeckBox_RecalculateIncomplete.IsChecked = config.RecalculateIncomplete;
        this.uiCHeckBox_NoExportColumn.IsChecked = config.NoExportColumn;
        this.uiCHeckBox_NoDurationColumn.IsChecked = config.NoDurationColumn;
        this.Width = config.SettingsWindowWidth;
        this.Height = config.SettingsWindowHeight;
    }

    private void Button_Apply_Click(object sender, RoutedEventArgs e)
    {
        this.uiButton_Apply.IsEnabled = false;
        this.uiButton_LoadDefault.IsEnabled = true;
        Configure config = Settings.Current;
        config.SaveMainWindowSize = this.uiCheckBox_RembMainSize.IsChecked ?? false;
        config.FileSearchPolicy = (SearchPolicy)this.uiComboBox_SearchPolicy1.SelectedIndex;
        config.UseLowercaseHash = this.uiCheckBox_UseLowercaseHash.IsChecked ?? false;
        config.SaveMainWindowPosition = this.uiCheckBox_RemMainWinPos.IsChecked ?? false;
        config.TaskNumber = (Concurrency)this.uiComboBox_SimulCalculate.SelectedIndex;
        config.QuickVerifyFileSearchPolicy = (SearchPolicy)this.uiComboBox_SearchPolicy2.SelectedIndex;
        config.ShowResultText = this.uiCHeckBox_ShowResultText.IsChecked ?? false;
        config.RecalculateIncomplete = this.uiCHeckBox_RecalculateIncomplete.IsChecked ?? false;
        config.NoExportColumn = this.uiCHeckBox_NoExportColumn.IsChecked ?? false;
        config.NoDurationColumn = this.uiCHeckBox_NoDurationColumn.IsChecked ?? false;
        if (AppViewModel.Instance == null)
            return;
        AppViewModel.Instance.RefreshCmpResultDisplayStyle();
        AppViewModel.Instance.SetColumnVisibility(config.NoExportColumn, config.NoDurationColumn);
        Task.Run(() => { AppViewModel.Instance.SetConcurrent(config.TaskNumber); });
    }

    private void Button_Cancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void Button_LoadDefault_Click(object sender, RoutedEventArgs e)
    {
        this.uiButton_LoadDefault.IsEnabled = false;
        this.uiButton_Apply.IsEnabled = true;
        this.InitializeFromConfigure(new Configure());
    }

    private void Window_SettingsPanel_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Settings.Current.SettingsWindowWidth = this.Width;
        Settings.Current.SettingsWindowHeight = this.Height;
        Settings.SaveConfigure();
    }

    private void CheckBoxes_ActivateApplyButton_Click(object sender, RoutedEventArgs e)
    {
        this.uiButton_Apply.IsEnabled = true;
    }

    private void ComboBoxes_ActivateApplyButton_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        this.uiButton_Apply.IsEnabled = true;
    }
}
