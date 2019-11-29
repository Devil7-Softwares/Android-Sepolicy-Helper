using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Devil7.Android.SepolicyHelper.Controls {
	internal class Spinner : UserControl {
		public Spinner () {
			InitializeComponent ();
		}

		private void InitializeComponent () {
			AvaloniaXamlLoader.Load (this);
		}
	}
}