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
        cpu.ConnectBus(bus);

        Application.Run(new FrMain(cpu));
    }
}