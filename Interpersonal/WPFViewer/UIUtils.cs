

namespace Interpersonal.WPFViewer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    class UIUtils
    {
        public static BitmapImage GetImageSource(string relativePath)
        {
            return GetImageSource(new Uri(relativePath, UriKind.Relative));
        }

        public static BitmapImage GetImageSource(Uri relativeUri)
        {
            var newImage = new BitmapImage();

            newImage.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.IgnoreImageCache;
            newImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.None;
            Uri urisource = relativeUri;
            newImage.BeginInit();
            newImage.UriSource = urisource;
            newImage.EndInit();

            return newImage;
        }
        public static T GetFrameworkElementByName<T>(FrameworkElement referenceElement) where T : FrameworkElement
        {

            FrameworkElement child = null;

            for (Int32 i = 0; i < VisualTreeHelper.GetChildrenCount(referenceElement); i++)
            {

                child = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;

                System.Diagnostics.Debug.WriteLine(child);

                if (child != null && child.GetType() == typeof(T))

                { break; }

                else if (child != null)
                {

                    child = GetFrameworkElementByName<T>(child);

                    if (child != null && child.GetType() == typeof(T))
                    {

                        break;

                    }

                }

            }

            return child as T;
        }

    }
}
