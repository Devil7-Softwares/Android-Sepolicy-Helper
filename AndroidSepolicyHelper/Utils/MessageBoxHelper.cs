using Avalonia.Controls;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Devil7.Android.SepolicyHelper.Utils
{
    public static class MessageBoxHelper
    {
        public static async Task Info(string message, string title, Window parent = null)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                MessageBoxWindow messageBox = new MessageBoxWindow(new MessageBoxParams()
                {
                    Button = ButtonEnum.Ok,
                    CanResize = false,
                    ContentMessage = message,
                    ContentTitle = title,
                    Icon = Icon.Success,
                    ShowInCenter = true,
                    WindowIcon = null
                });
                if (parent == null)
                    await messageBox.Show();
                else
                    await messageBox.ShowDialog(parent);
            });
        }

        public static async Task Error(string message, string title, Window parent = null)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                MessageBoxWindow messageBox = new MessageBoxWindow(new MessageBoxParams()
                {
                    Button = ButtonEnum.Ok,
                    CanResize = false,
                    ContentMessage = message,
                    ContentTitle = title,
                    Icon = Icon.Error,
                    ShowInCenter = true,
                    WindowIcon = null
                });
                if (parent == null)
                    await messageBox.Show();
                else
                    await messageBox.ShowDialog(parent);
            });
        }
    }
}
