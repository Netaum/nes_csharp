using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Emulator.Components;
using Emulator.Primitives;

namespace NesUI;

public partial class MainWindow : Window
{
    public void ChangePaletteAction()
    {
        _selectedPalette = (_selectedPalette + 1) & 0x07;
        UpdateView(_bus!, EmulatorInformation, _emulatorScreenBindings, _showMemory, _selectedPalette, _paletteViewers);
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
        UpdateView(_bus!, EmulatorInformation, _emulatorScreenBindings, _showMemory, _selectedPalette, _paletteViewers);
    }

    public void FrameAdvanceAction()
    {
        UpdateFrame(_bus!);
        UpdateView(_bus!, EmulatorInformation, _emulatorScreenBindings, _showMemory, _selectedPalette, _paletteViewers);
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
            OnTick(_bus!, EmulatorInformation, _emulatorScreenBindings, _runEmulation, _showMemory, _selectedPalette, _paletteViewers);
        };
        
        UpdateView(_bus!, EmulatorInformation, _emulatorScreenBindings, _showMemory, _selectedPalette, _paletteViewers);

        _timer.Start();
    }
}