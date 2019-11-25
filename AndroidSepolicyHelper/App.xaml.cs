using Avalonia;
using Avalonia.Markup.Xaml;

namespace AndroidSepolicyHelper
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
