using Chips;
using Components;

namespace PixelGraph;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        var cpu = new Ocl6502();
        var ppu = new Ocl2C02();

        var bus = new Bus(cpu, ppu);
        var nestest = File.ReadAllBytes("d:\\Backup Win11\\dev\\nes_csharp\\roms\\nestest.nes");
        var cardridge = Cartridge.CreateCartridge(nestest);
        bus.InsertCartridge(cardridge);

        Application.Run(new Form1(bus));
    }    
}