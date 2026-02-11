namespace ResumeMaker;

using QuestPDF.Infrastructure;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
