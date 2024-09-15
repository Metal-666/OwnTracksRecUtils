using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace OwnTracksRecUtils;

public partial class App : Application {

	public App() {

		InitializeComponent();

		MainPage = new AppShell();

	}

	protected override Window CreateWindow(IActivationState? activationState) {

		Window window = base.CreateWindow(activationState);

		window.Width = 600;
		window.Height = 400;

		return window;

	}

}