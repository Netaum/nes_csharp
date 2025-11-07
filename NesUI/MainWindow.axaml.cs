using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Emulator.Components;

namespace NesUI;

public partial class MainWindow : Window
{
    public void CpuAction(object? sender, RoutedEventArgs e)
    {
        _showMemory = !_showMemory;
    }
    
    public void ClockAction(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button)
            return;

        var cpu = _bus!.Cpu;

        do
        {
            _bus.Clock();
        } while (!cpu.Complete);

        do
        {
            _bus.Clock();
        } while (cpu.Complete);
    }

    public void FrameAdvanceAction(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button)
            return;

        var cpu = _bus!.Cpu;

        do
        {
            _bus.Clock();
        } while (!_bus.Ppu.FrameComplete);

        do
        {
            _bus.Clock();
        } while (!cpu.Complete);

        _bus.Ppu.FrameComplete = false;
    }

    public void ResetAction(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button)
            return;

        _bus!.Reset();
    }

    public MainWindow()
    {
        InitializeComponent();
        InitializeEmulatorWindow();
    }
    public MainWindow(Bus bus)
    : this()
    {
        InitializeComponent();
        SetBus(bus);

        _timer.Interval = TimeSpan.FromMilliseconds(33);

        _timer.Tick += (sender, e) =>
        {
            DrawEmulation(
                _bus,
                ContentBlock,
                _emulatorScreens,
                _showMemory);
        };

        _timer.Start();
    }
}