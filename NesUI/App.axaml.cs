using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Emulator.Components;

namespace NesUI;

public partial class App : Application
{
    private readonly Bus _bus = new Bus();
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        SetupEmulator();
    }

    private void SetupEmulator()
    {
        var cpu = new Ocl6502();
        var ppu = new Ocl2C02();
        var cartridge = new Cartridge();

        var nestest = File.ReadAllBytes("/home/netaum/Development/nes_csharp/roms/nestest.nes");
        cartridge.LoadCartridge(nestest);

        _bus.ConnectCpu(cpu);
        _bus.ConnectPpu(ppu);
        _bus.InsertCartridge(cartridge);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var mainApp = new MainWindow(_bus);
        mainApp.SetBus(_bus);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = mainApp;
        }

        base.OnFrameworkInitializationCompleted();
    }
}