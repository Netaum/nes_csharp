using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CustomTypes;
using Emulator.Components.Interfaces;

namespace NesUI.Helpers
{
    public class ScreenBinding
    {
        private WriteableBitmap _writeableBitmap;
        private Image _imageControl;
        private ScreenSelection _screenSelection;

        public ScreenBinding(ScreenSelection screenSelection, Image imageControl, int width, int height)
        {
            _writeableBitmap = new WriteableBitmap(PixelSize.FromSize(new Size(width, height), 1), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
            _screenSelection = screenSelection;
            _imageControl = imageControl;
        }

        public WriteableBitmap WriteableBitmap => _writeableBitmap;
        public Image ImageControl => _imageControl;
        public ScreenSelection ScreenSelection => _screenSelection;

        public void OnUpdate(IPpu ppu)
        {
            var baseBitmap = _screenSelection switch
            {
                ScreenSelection.EmulatorScreen => ppu.GetScreen(),
                ScreenSelection.PatternTable1 => ppu.GetPatternTable(0, 1),
                ScreenSelection.PatternTable2 => ppu.GetPatternTable(1, 1),
                _ => throw new ArgumentOutOfRangeException()
            };
            var writeableBitmap = UpdateBitmap(_writeableBitmap, baseBitmap);
            _imageControl.Source = writeableBitmap;
            _imageControl.InvalidateVisual();
        }
        
        private unsafe static WriteableBitmap UpdateBitmap(WriteableBitmap bitmap, BaseBitmap baseBitmap)
        {
            using ILockedFramebuffer lockedFramebuffer = bitmap.Lock();

            void* backBuffer = (void*)lockedFramebuffer.Address;
            Span<byte> buffer = new Span<byte>(backBuffer, baseBitmap.Width * baseBitmap.Height * 4);
            var img = baseBitmap.ToBgraFormat();

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = img[i];
            }
            return bitmap;
        }
    }
}