namespace BsBrowserClient;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // 解析命令行参数
        var configId = GetArgValue(args, "--config-id", "0");
        var configName = GetArgValue(args, "--config-name", "未命名配置");  // 🔥 新增配置名参数
        var port = GetArgValue(args, "--port", "9527");
        var platform = GetArgValue(args, "--platform", "YunDing28");
        var platformUrl = GetArgValue(args, "--url", "");
        
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1(configId, configName, int.Parse(port), platform, platformUrl));
    }
    
    private static string GetArgValue(string[] args, string argName, string defaultValue)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(argName, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        return defaultValue;
    }
}