namespace DynamicBatching;

static class Program
{
	static void Main()
	{
		var app = new Application();
		Console.WriteLine("Running with batching enabled. Press 'Space' to grow mesh. Press 'R' to reset mesh.");
		Console.WriteLine("Close window or press 'Escape' to open next window, which has batching.");
		app.RunWithoutBatching();

		Console.Clear();
		Console.WriteLine("Running with batching enabled. Press 'Space' to grow mesh. Press 'R' to reset mesh.");
		app.RunWithBatching();
	}
}
