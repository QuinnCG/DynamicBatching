namespace DynamicBatching;

static class Program
{
	static void Main()
	{
		var app = new Application();
		Console.WriteLine("Running without batching enabled. Press 'Space' to grow mesh.");
		app.RunWithoutBatching();

		Console.WriteLine("Running with batching enabled. Press 'Space' to grow mesh.");
		app.RunWithBatching();
	}
}
