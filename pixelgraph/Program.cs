using emulator.components;

namespace pixelgraph;

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
        var bus = new Bus();
        var ppu = new Ocl2C02();
        var cardridge = new Cartridge();

        var nestest = File.ReadAllBytes("h:\\dev\\nes_csharp\\roms\\nestest.nes");
        cardridge.LoadCartridge(nestest);

        bus.ConnectCpu(cpu);
        bus.ConnectPpu(ppu);
        bus.InsertCartridge(cardridge);

        Application.Run(new FrMain(bus));
    }
}