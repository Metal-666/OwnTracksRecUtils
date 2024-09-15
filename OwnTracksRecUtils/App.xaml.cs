using Microsoft.Maui.Controls;

namespace OwnTracksRecUtils;

public partial class App : Application {

	public App() {

		InitializeComponent();

		MainPage = new AppShell();

	}

}