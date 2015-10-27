#if !DEBUG
using System;
using System.IO;
#endif
using System.Threading;
using OCGWrapper;

namespace YGOSharp
{
    public class Program
    {
        public const int ProVersion = 0x1337;

        public static void Main(string[] args)
        {
#if !DEBUG
            try
            {
#endif

                CoreConfig config = new CoreConfig();
                if (args.Length != 2 || !config.Load(args[0], args[1]))
                    return;

                BanlistManager.Init("lflist.conf");
                Api.Init();

                CoreServer server = new CoreServer(config);
                server.Start();
                while (server.IsRunning)
                {
                    server.Tick();
                    Thread.Sleep(1);
                }
#if !DEBUG
            }
            catch (Exception ex)
            {
                File.WriteAllText("crash_" + DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt", ex.ToString());
            }
#endif
        }
    }
}
