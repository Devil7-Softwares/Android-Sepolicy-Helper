using Avalonia;
using Avalonia.Markup.Xaml;

namespace Devil7.Android.SepolicyHelper
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
