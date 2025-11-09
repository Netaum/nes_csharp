using System.Collections.Generic;
using System.Linq;
using Emulator.Components.Interfaces;

namespace NesUI.Helpers
{
    public class PaletteViewer
    {
        private readonly List<Avalonia.Controls.Shapes.Rectangle> _rectangles;
        private Avalonia.Controls.Shapes.Rectangle? _borderRectangle;

        public PaletteViewer(IEnumerable<Avalonia.Controls.Shapes.Rectangle> rectangles)
        {
            _rectangles = rectangles.ToList();
        }

        public void SetBorderRectangle(Avalonia.Controls.Shapes.Rectangle borderRectangle)
        {
            _borderRectangle = borderRectangle;
        }

        public void SetActive(bool isActive)
        {
            if (_borderRectangle != null)
            {
                _borderRectangle.Stroke = isActive ? Avalonia.Media.Brushes.Yellow : Avalonia.Media.Brushes.Black;
                _borderRectangle.InvalidateVisual();
            }
        }

        public void OnUpdate(IPpu ppu, int palette)
        {
            for (int i = 0; i < 4; i++)
            {
                var color = ppu.GetColorFromPaletteRam(palette, i);
                var avaloniaColor = Avalonia.Media.Color.FromRgb(color.R, color.G, color.B);
                _rectangles[i].Fill = new Avalonia.Media.SolidColorBrush(avaloniaColor);
                _rectangles[i].InvalidateVisual();
            }
            _borderRectangle?.InvalidateVisual();
        }

    }
}