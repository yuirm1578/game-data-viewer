using System.Windows;
using System.Windows.Controls;

namespace GameDataViewer.App.Views;

public partial class ItemDetailView : UserControl
{
    public ItemDetailView()
    {
        InitializeComponent();
        DataContextChanged += (_, e) => ToggleView(e.NewValue is not null);
    }

    private void ToggleView(bool hasData)
    {
        EmptyHint.Visibility  = hasData ? Visibility.Collapsed : Visibility.Visible;
        DetailPanel.Visibility = hasData ? Visibility.Visible  : Visibility.Collapsed;
    }
}
