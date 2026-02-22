using Interfaces;

namespace PixelGraph;

public partial class Form1 : Form
{
    private IBus _bus;
    public Form1(IBus bus)
    {
        _bus = bus;
        InitializeComponent();
    }

    
}
