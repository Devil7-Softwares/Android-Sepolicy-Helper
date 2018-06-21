using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AndroidSepolicyHelper.Controls {
	internal class Spinner : UserControl {
		public Spinner () {
			InitializeComponent ();
		}

		private void InitializeComponent () {
			AvaloniaXamlLoader.Load (this);
		}
	}
}