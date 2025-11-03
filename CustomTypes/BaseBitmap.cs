using System.Drawing;

namespace CustomTypes;

public class BaseBitmap
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    private int[] data;

    private BaseBitmap(int width, int height)
    {
        Width = width;
        Height = height;

        data = new int[width * height * 4];
    }

    public static BaseBitmap Create(int width, int height)
    {
        return new BaseBitmap(width, height);
    }

    public void SetPixel(int x, int y, Color color)
    {
        SetPixel(x, y, color.R, color.G, color.B, color.A);
    }

    public void SetPixel(int x, int y, int red, int green, int blue)
    {
        SetPixel(x, y, red, green, blue, 255);
    }

    public void SetPixel(int x, int y, int red, int green, int blue, int alpha)
    {
        int index = (y * Width + x) * 4;
        data[index] = blue;
        data[index + 1] = green;
        data[index + 2] = red;
        data[index + 3] = alpha;
    }
}
