using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;


namespace PVChat.WPF.Helpers
{
    //Helper for TaskBarItemInfo sets the overlay icon and notification count on task bar icon
    public class TaskBarItemHelper
    {
        public static readonly DependencyProperty TextProperty =
               DependencyProperty.RegisterAttached("Text", typeof(object), typeof(TaskBarItemHelper), new PropertyMetadata(OnPropertyChanged));

        public static readonly DependencyProperty TemplateProperty =
            DependencyProperty.RegisterAttached("Template", typeof(DataTemplate), typeof(TaskBarItemHelper), new PropertyMetadata(OnPropertyChanged));


        public static object GetText(DependencyObject d)
        {
            return d.GetValue(TextProperty);
        }

        public static void SetText(DependencyObject d, object content)
        {
            d.SetValue(TextProperty, content);
        }

        public static DataTemplate GetTemplate(DependencyObject d)
        {
            return (DataTemplate)d.GetValue(TemplateProperty);
        }

        public static void SetTemplate(DependencyObject d, DataTemplate template)
        {
            d.SetValue(TemplateProperty, template);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var taskbarItemInfo = (TaskbarItemInfo)d;
            var content = GetText(taskbarItemInfo);
            var template = GetTemplate(taskbarItemInfo);

            if (template == null || content == null)
            {
                taskbarItemInfo.Overlay = null;
                return;
            }

            const int ICON_WIDTH = 16;
            const int ICON_HEIGHT = 16;

            var bmp =
                new RenderTargetBitmap(ICON_WIDTH, ICON_HEIGHT, 96, 96, PixelFormats.Default);
            var root = new ContentControl
            {
                ContentTemplate = template,
                Content = content
            };
            root.Arrange(new Rect(0, 0, ICON_WIDTH, ICON_HEIGHT));
            bmp.Render(root);

            taskbarItemInfo.Overlay = bmp;
        }
    }
}

