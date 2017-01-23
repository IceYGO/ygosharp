#if !DEBUG
using System;
using System.IO;
#endif
using System.Threading;
using YGOSharp.OCGWrapper;

namespace YGOSharp
{
    public class Program
    {
        public static uint ClientVersion = 0x133D;

        public static void Main(string[] args)
        {
#if !DEBUG
            try
            {
#endif
                Config.Load(args);

                BanlistManager.Init(Config.GetString("BanlistFile", "lflist.conf"));
                Api.Init(Config.GetString("RootPath", "."), Config.GetString("ScriptDirectory", "script"), Config.GetString("DatabaseFile", "cards.cdb"));

                ClientVersion = Config.GetUInt("ClientVersion", ClientVersion);

                CoreServer server = new CoreServer();
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
