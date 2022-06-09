using AvalonDock;
using AvalonDock.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace AvalonDock_Viewbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FixZoom(object sender, RoutedEventArgs e)
        {
            var autoHideWindows = PresentationSource.CurrentSources
                            .OfType<HwndSource>()
                            .Select(x => x.RootVisual)
                            .OfType<FrameworkElement>()
                            .Select(content => (window: content.Parent as LayoutAutoHideWindowControl, content))
                            .Where(x => x.window != null)
                            .ToList();

            foreach (var w in autoHideWindows)
            {
                if (ComputeTransform(w.window.Model.Root.Manager) is Transform transform && w.content.LayoutTransform.Value != transform.Value)
                {
                    w.window.LayoutTransform = (Transform)transform.Inverse;
                    w.content.LayoutTransform = transform;
                }
            }
        }

        private static Transform ComputeTransform(DockingManager dockingManager)
        {
            var viewboxes = dockingManager.GetParents().OfType<Viewbox>().ToList();

            if (viewboxes.Any())
            {
                if (dockingManager.TransformToAncestor(viewboxes[viewboxes.Count - 1]) is Transform transform)
                {
                    if (!transform.Value.IsIdentity)
                    {
                        var origin = transform.Transform(new Point());

                        var newTransformGroup = new TransformGroup();
                        newTransformGroup.Children.Add(transform);
                        newTransformGroup.Children.Add(new TranslateTransform(-origin.X, -origin.Y));
                        return newTransformGroup;
                    }
                }
            }

            return Transform.Identity;
        }

        private void Set100PercentZoomRemoveFromZoombox(object sender, RoutedEventArgs e)
        {
            Viewbox.Child = null;
            ViewboxParent.Children.Add(ViewboxContent);

            Viewbox.Height = double.NaN;
            Viewbox.Width = double.NaN;
        }
        private void Set100PercentZoom(object sender, RoutedEventArgs e)
        {
            SetZoom(1.00);
        }
        private void Set150PercentZoom(object sender, RoutedEventArgs e)
        {
            SetZoom(1.50);
        }
        private void Set200PercentZoom(object sender, RoutedEventArgs e)
        {
            SetZoom(2.00);
        }
        private void Set250PercentZoom(object sender, RoutedEventArgs e)
        {
            SetZoom(2.50);
        }

        private void SetZoom(double percent)
        {
            if (ViewboxParent != null)
            {
                if (ViewboxParent.Children.Contains(ViewboxContent))
                {
                    ViewboxParent.Children.Remove(ViewboxContent);
                    Viewbox.Child = ViewboxContent;
                }

                Viewbox.Width = percent * ViewboxContent.Width;
                Viewbox.Height = percent * ViewboxContent.Height;
            }
        }
    }

    public static class Extensions
    {
        // This is AvalonDock.Extensions.GetParents
        public static IEnumerable<DependencyObject> GetParents(this DependencyObject dependencyObject)
        {
            while (dependencyObject != null)
            {
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                if (dependencyObject != null)
                    yield return dependencyObject;
            }
        }
    }
}
