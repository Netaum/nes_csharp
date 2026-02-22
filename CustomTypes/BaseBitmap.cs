using System.Drawing;

namespace CustomTypes;

public class BaseBitmap
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    private readonly Color[] data;

    private BaseBitmap(int width, int height)
    {
        Width = width;
        Height = height;

        data = new Color[width * height];
    }

    public static BaseBitmap Create(int width, int height)
    {
        return new BaseBitmap(width, height);
    }

    public void SetPixel(int x, int y, Color color)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return;
        data[y * Width + x] = color;
    }

    public byte[] ToBgraFormat()
    {
        byte[] bgraData = new byte[Width * Height * 4];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Color color = data[y * Width + x];
                int index = (y * Width + x) * 4;
                bgraData[index] = color.B;
                bgraData[index + 1] = color.G;
                bgraData[index + 2] = color.R;
                bgraData[index + 3] = color.A;
            }
        }

        return bgraData;
    }
}
