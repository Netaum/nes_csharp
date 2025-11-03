using Avalonia.Controls;
using Avalonia.Layout;

namespace NesUI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        TextBlock txtBlock = new TextBlock
        {
            Text = " Hello, NesUI! ",
            Foreground = Avalonia.Media.Brushes.White,
            FontSize = 24
        };
        StackPanel stackPanel = new StackPanel
        {
            Width = 360,
            Height = 250,
            Orientation = Orientation.Vertical,
            Background = Avalonia.Media.Brushes.Blue,
            
            Spacing = 2
        };
        stackPanel.Children.Add(txtBlock);
        Canvas.SetLeft(stackPanel, 8);
        Canvas.SetTop(stackPanel, 268);

        MainWindowCanvas.Children.Add(stackPanel);
    }
}