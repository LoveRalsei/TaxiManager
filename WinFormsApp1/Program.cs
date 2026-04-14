namespace TaxiManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            DataLoader.Load();
            Application.Run(new MapForm());
        }
    }
}