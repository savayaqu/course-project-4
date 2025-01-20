using System.Text.RegularExpressions;
using PicsyncAdmin.ViewModels;

namespace PicsyncAdmin.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
        InitializeComponent();
        BindingContext = new SettingsViewModel();
    }
    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = sender as Entry;
        if (entry == null) return;

        // ���������, ��� �������� ����� ������� ������ �� ����
        bool isValid = Regex.IsMatch(e.NewTextValue, @"^\d{0,2}$");

        if (!isValid)
        {
            // ���� �������� ����� �������� ������������ �������, ���������� ���������
            entry.Text = e.OldTextValue;
        }
    }
}