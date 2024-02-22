namespace Vts.Gui.Wpf.Model
{
    public interface IDataPoint
    {
        double X { get; set; }  // only X here to work for both Double and Complex points
    }
}