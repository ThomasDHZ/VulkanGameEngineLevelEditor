using VulkanGameEngineLevelEditor.Registries;

namespace VulkanGameEngineLevelEditor
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

            Application.ThreadException += (s, e) => MessageBox.Show(e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                MessageBox.Show(ex?.Message ?? e.ExceptionObject.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            ComponentRegistry.Initialize();
            ApplicationConfiguration.Initialize();
            Application.Run(new RenderViewForm());
        }
    }
}