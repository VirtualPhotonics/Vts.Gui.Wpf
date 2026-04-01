using System;

namespace Vts.Gui.Wpf.View.Helpers;

/// <summary>
///     A class for mapping grayscale intensity to a color via lookup tables
/// </summary>
public class Colormap
{
    public Colormap(ColormapType colormapType)
    {
        const int colormapSize = 256;

        RedByte = new byte[colormapSize];
        GreenByte = new byte[colormapSize];
        BlueByte = new byte[colormapSize];

        ColormapLookUpTable colormapLookUpTable = colormapType switch
        {
            ColormapType.Hot => new HotColormapLookUpTable(),
            ColormapType.Jet => new JetColormapLookUpTable(),
            ColormapType.HSV => new HsvColormapLookUpTable(),
            ColormapType.Copper => new CopperColormapLookUpTable(),
            ColormapType.Binary => new BinaryColormapLookUpTable(),
            ColormapType.Bone => new BoneColormapLookUpTable(),
            _ => new GrayColormapLookUpTable()
        };

        var mappingRatio = colormapSize/colormapLookUpTable.Length;
        try
        {
            for (var j = 0; j < colormapSize; j++)
            {
                RedByte[j] = (byte) (255*colormapLookUpTable.Red[j/mappingRatio]);
                GreenByte[j] = (byte) (255*colormapLookUpTable.Green[j/mappingRatio]);
                BlueByte[j] = (byte) (255*colormapLookUpTable.Blue[j/mappingRatio]);
            }
        }
        catch (IndexOutOfRangeException e)
        {
            Console.WriteLine(e);
        }
    }

    public byte[] RedByte { get; }

    public byte[] GreenByte { get; }

    public byte[] BlueByte { get; }
}