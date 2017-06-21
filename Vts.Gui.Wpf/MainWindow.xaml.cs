using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Vts.Gui.Wpf.View;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _numPlotViews;
        private int _numMapViews;
        public static MainWindow Current = null;

        public MainWindow()
        {
            InitializeComponent();

            _numPlotViews = 0;
            _numMapViews = 0;

            Current = this;
        }

        private void InputTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var inputTab = sender as TabControl;
            if (inputTab != null && inputTab.Items != null &&
                OutputTabControl != null && OutputTabControl.Items != null && OutputTabControl.Items.Count > 1)
            {
                var tabItem = inputTab.SelectedItem as TabItem;
                if (tabItem != null)
                {
                    switch (tabItem.Name)
                    {
                        case "TabForward":
                        case "TabInverse":
                        case "TabSpectral":
                        default:
                            OutputTabControl.SelectedItem = OutputTabControl.Items[0];
                            ((TabItem)OutputTabControl.Items[0]).Visibility = Visibility.Visible;
                            ((TabItem)OutputTabControl.Items[1]).Visibility = Visibility.Collapsed;
                            break;
                        case "TabFluence":
                            OutputTabControl.SelectedItem = OutputTabControl.Items[1];
                            ((TabItem)OutputTabControl.Items[1]).Visibility = Visibility.Visible;
                            ((TabItem)OutputTabControl.Items[0]).Visibility = Visibility.Collapsed;
                            break;
                        case "TabMonteCarlo":
                            ((TabItem)OutputTabControl.Items[1]).Visibility = Visibility.Visible;
                            ((TabItem)OutputTabControl.Items[0]).Visibility = Visibility.Visible;
                            break;
                    }
                }
            }
        }

        public void Main_DuplicatePlotView_Executed(object viewModel)
        {
            var thumb = new Thumb { Width = 0, Height = 0 };
            var vm = viewModel as PlotViewModel;
            if (vm != null)
            {
                var plotView = new PlotView
                {
                    DataContext = vm,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 3, 0)
                };
                var plotBorder = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(5), BorderThickness = new Thickness(2), BorderBrush = Brushes.Gray };
                var plotPanel = new StackPanel();
                var closeButton = new Button { Content="x", HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(5), Width = 25, Height = 25 };
                plotPanel.Children.Add(closeButton);
                plotPanel.Children.Add(plotView);
                plotPanel.Children.Add(thumb);
                plotBorder.Child = plotPanel;
                var newPlotWindow = new Popup
                {
                    Name = "wndPlotView" + _numPlotViews++,
                    Child = plotBorder,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Placement = PlacementMode.Relative,
                    IsOpen = true,
                    AllowsTransparency = true
                };

                closeButton.Click += (sender, e) =>
                {
                    newPlotWindow.IsOpen = false;
                };

                newPlotWindow.MouseDown += (sender, e) =>
                {
                    thumb.RaiseEvent(e);
                };

                thumb.DragDelta += (sender, e) =>
                {
                    newPlotWindow.HorizontalOffset += e.HorizontalChange;
                    newPlotWindow.VerticalOffset += e.VerticalChange;
                };
            }
        }

        public void Main_DuplicateMapView_Executed(object viewModel)
        {
            var thumb = new Thumb {Width = 0, Height = 0};
            var vm = viewModel as MapViewModel;
            if (vm != null)
            {
                var mapView = new MapView
                {
                    DataContext = vm,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 3, 0),
                    MapImage =
                    {
                        Stretch = Stretch.Uniform,
                        MinWidth = 500
                    }
                };
                var mapPanel = new StackPanel { Background = Brushes.White, };
                mapPanel.Children.Add(mapView);
                mapPanel.Children.Add(thumb);
                var newMapWindow = new Popup
                {
                    Name = "wndMapView" + _numMapViews++,
                    Child = mapPanel,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0),
                    Width = 700,
                    Height = 540,
                    IsOpen = true,
                };

                newMapWindow.MouseDown += (sender, e) =>
                {
                    thumb.RaiseEvent(e);
                };

                thumb.DragDelta += (sender, e) =>
                {
                    newMapWindow.HorizontalOffset += e.HorizontalChange;
                    newMapWindow.VerticalOffset += e.VerticalChange;
                };
            }
        }
    }
}
