using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Emulator.Components;

namespace NesUI;

public partial class MainWindow : Window
{
    private void UpdatePaletteVisuals()
    {
        foreach (var (_, palette) in _palletes)
        {
            palette.Stroke = Brushes.Black;
            palette.StrokeThickness = 1;
            palette.InvalidateVisual();
        }

        _palletes[_selectedPalette].Stroke = Brushes.Red;
        _palletes[_selectedPalette].StrokeThickness = 2;
        _palletes[_selectedPalette].InvalidateVisual();
    }
    public void ChangePaletteAction()
    {
        _selectedPalette = (_selectedPalette + 1) & 0x07;
        UpdatePaletteVisuals();
    }

    public void ClockAction()
    {
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

    public void FrameAdvanceAction()
    {
        UpdateFrame(_bus!);
    }

    public void ResetAction()
    {
        _bus!.Reset();
    }

    public void ToggleEmulation()
    {
        _runEmulation = !_runEmulation;
    }
    
    public void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.P)
        {
            ChangePaletteAction();
            e.Handled = true;
        }
        if (e.Key == Key.C)
        {
            ClockAction();
            e.Handled = true;
        }
        if (e.Key == Key.F)
        {
            FrameAdvanceAction();
            e.Handled = true;
        }
        if (e.Key == Key.R)
        {
            ResetAction();
            e.Handled = true;
        }
        if (e.Key == Key.Space)
        {
            ToggleEmulation();
            e.Handled = true;
        }
    }

    

    public MainWindow()
    {
        InitializeComponent();
        InitializeEmulatorWindow();
    }

    public MainWindow(Bus bus)
    : this()
    {
        SetBus(bus);
        _timer.Interval = TimeSpan.FromMilliseconds(33);
        _timer.Tick += (sender, e) =>
        {
            OnUpdate(_bus!, EmulatorInformation, _emulatorScreenBindings,_runEmulation, _showMemory);
        };

        _timer.Start();
    }
}