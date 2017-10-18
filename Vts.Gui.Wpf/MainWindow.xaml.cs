using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Vts.Gui.Wpf.View;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Current;
        private int _numViews;

        public MainWindow()
        {
            InitializeComponent();
            _numViews = 0;
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
                            ((TabItem) OutputTabControl.Items[0]).Visibility = Visibility.Visible;
                            ((TabItem) OutputTabControl.Items[1]).Visibility = Visibility.Collapsed;
                            break;
                        case "TabFluence":
                            OutputTabControl.SelectedItem = OutputTabControl.Items[1];
                            ((TabItem) OutputTabControl.Items[1]).Visibility = Visibility.Visible;
                            ((TabItem) OutputTabControl.Items[0]).Visibility = Visibility.Collapsed;
                            break;
                        case "TabMonteCarlo":
                            ((TabItem) OutputTabControl.Items[1]).Visibility = Visibility.Visible;
                            ((TabItem) OutputTabControl.Items[0]).Visibility = Visibility.Visible;
                            break;
                    }
                }
            }
        }

        public void Main_DuplicatePlotView_Executed(object viewModel)
        {
            CreatePopupWindow(viewModel, "plot");
        }

        public void Main_DuplicateMapView_Executed(object viewModel)
        {
            CreatePopupWindow(viewModel, "map");
        }

        private void CreatePopupWindow(object viewModel, string type)
        {
            var thumb = new Thumb { Width = 0, Height = 0 };
            object vm;
            if (type == "map")
            {
                vm = viewModel as MapViewModel;
            }
            else
            {
                vm = viewModel as PlotViewModel;
            }
            if (vm == null) return;
            object duplicateView;
            if (type == "map")
            {
                duplicateView = new MapView
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
            }
            else
            {
                duplicateView = new PlotView
                {
                    DataContext = vm,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 3, 0)
                };
            }
            var viewBorder = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(0),
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.Gray
            };
            var viewPanel = new StackPanel();
            var closeButton = new Button
            {
                Content = "X",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5),
                Width = 25,
                Height = 25
            };
            viewPanel.Children.Add(closeButton);
            viewPanel.Children.Add((UIElement)duplicateView);
            viewPanel.Children.Add(thumb);
            viewBorder.Child = viewPanel;
            //Popup behavior is to always be on top so we need to find another solution for this control
            var newViewWindow = new Popup
            {
                Name = "wndView" + _numViews++,
                Child = viewBorder,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Placement = PlacementMode.Relative,
                IsOpen = true,
                AllowsTransparency = true
            };

            closeButton.Click += (sender, e) => { newViewWindow.IsOpen = false; };

            newViewWindow.MouseDown += (sender, e) =>
            {
                var popupWindow = (Popup) sender;
                Topmost = false;
                popupWindow.Focus();
                thumb.RaiseEvent(e);
            };

            thumb.DragDelta += (sender, e) =>
            {
                newViewWindow.HorizontalOffset += e.HorizontalChange;
                newViewWindow.VerticalOffset += e.VerticalChange;
            };
        }
    }
}