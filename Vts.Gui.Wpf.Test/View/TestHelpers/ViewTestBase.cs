using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Vts.Gui.Wpf.Test.View.TestHelpers;

public abstract class ViewTestBase
{
    protected Window? HostWindow;

    // keep temporary HwndSource instances so we can dispose them in TeardownHost
    private readonly List<HwndSource> _tempHwndSources = [];

    protected void SetupHost()
    {
        ViewTestHelpers.EnsureApplication();
        // all UI ops on dispatcher to ensure ownership
        Application.Current!.Dispatcher.Invoke(() =>
        {
            ViewTestHelpers.AddDefaultResources();
            HostWindow = ViewTestHelpers.CreateHostWindow();
            HostWindow.Width = 800;
            HostWindow.Height = 600;
            HostWindow.Show();
            HostWindow.UpdateLayout();
        }, DispatcherPriority.Send);
    }

    protected void TeardownHost()
    {
        if (Application.Current == null) return;
        Application.Current.Dispatcher.Invoke(() =>
        {
            try { HostWindow?.Close(); } catch { }
            if (Application.Current.MainWindow == HostWindow)
                Application.Current.MainWindow = null;
            HostWindow = null;
            // dispose any temporary HwndSources we created during tests
            foreach (var hs in _tempHwndSources)
            {
                try { hs.Dispose(); } catch { }
            }
            _tempHwndSources.Clear();

            ViewTestHelpers.RemoveDefaultResources();
        }, DispatcherPriority.Send);
    }

    protected static void InvokeOnUI(Action action)
    {
        Application.Current!.Dispatcher.Invoke(action, DispatcherPriority.Send);
    }

    protected KeyEventArgs CreateKeyEventArgsFor(Visual target, Key key)
    {
        KeyEventArgs args = null!;
        Application.Current!.Dispatcher.Invoke(() =>
        {
            // Try to get an existing PresentationSource
            var presentationSources = PresentationSource.FromVisual(target) ?? PresentationSource.FromVisual(HostWindow!);

            // If none exists (headless or coverage runner), create a tiny HwndSource to act as PresentationSource
            if (presentationSources == null)
            {
                var parms = new HwndSourceParameters("TestPresentationSource")
                {
                    Width = 1,
                    Height = 1,
                    // keep a simple window style
                    WindowStyle = 0
                };
                var hwndSource = new HwndSource(parms);
                _tempHwndSources.Add(hwndSource);
                presentationSources = hwndSource;
            }

            args = new KeyEventArgs(Keyboard.PrimaryDevice, presentationSources, 0, key) { RoutedEvent = Keyboard.KeyDownEvent };
        }, DispatcherPriority.Send);
        return args;
    }
}