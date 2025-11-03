using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Layout;
using Emulator.Components;
using Emulator.Debug;

namespace NesUI;

public partial class MainWindow : Window
{
    private Bus? _bus = null!;

    [MemberNotNull(nameof(_bus))]
    public void SetBus(Bus bus)
    {
        _bus = bus;
        _bus.Reset();
    }

    public MainWindow()
    {
        InitializeComponent();
    }

    private void DrawMemoryPage(StackPanel panel, int address)
    {
        StringBuilder sb = new StringBuilder();
        var cpu = _bus!.Cpu;

        for (int row = 0; row < 16; row++)
        {
            sb.Clear();
            sb.AppendFormat("${0:X4}:", address);
            for (int col = 0; col < 16; col++)
            {
                sb.AppendFormat(" {0:X2}", cpu.ReadMemory(address));
                address++;
            }
            string line = sb.ToString();
            TextBlock txtBlock = new TextBlock
            {
                Text = line,
                Foreground = Avalonia.Media.Brushes.White,
                FontFamily = "Courier New",
                FontSize = 14,
                Height = 16
            };
            panel.Children.Add(txtBlock);
        }
    }

    private StackPanel CreateMemoryPage()
    {
        StackPanel stackPanel = new StackPanel
        {
            Width = 360,
            Height = 290,
            Orientation = Orientation.Vertical,
            Background = Avalonia.Media.Brushes.Blue,
            Spacing = 2
        };

        return stackPanel;
    }

    public MainWindow(Bus bus)
    : this()
    {
        InitializeComponent();
        SetBus(bus);

        StackPanel page0 = CreateMemoryPage();
        DrawMemoryPage(page0, 0x0000);

        StackPanel page8 = CreateMemoryPage();
        DrawMemoryPage(page8, 0x8000);

        Canvas.SetLeft(page0, 8);
        Canvas.SetTop(page0, 8);

        Canvas.SetLeft(page8, 8);
        Canvas.SetTop(page8, 300);

        var code = Disassemble.FromMemory(bus.Cpu, 0x0000, 0xFFFF);

        var splice = code.Where(w => w.Key >= 0x8000)
                         .Take(20)
                         .ToDictionary(k => k.Key, v => v.Value);
        
        MainWindowCanvas.Children.Add(page0);
        MainWindowCanvas.Children.Add(page8);
    }
}