using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace YGOSharp.OCGWrapper
{
    public static unsafe class Api
    {
        #region Native Imports

        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_card_reader(CardReader f);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_message_handler(MessageHandler f);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_script_reader(ScriptReader f);

        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr create_duel(UInt32 seed);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void start_duel(IntPtr pduel, Int32 options);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void end_duel(IntPtr pduel);

        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_player_info(IntPtr pduel, Int32 playerid, Int32 lp, Int32 startcount, Int32 drawcount);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void new_card(IntPtr pduel, UInt32 code, Byte owner, Byte playerid, Byte location, Byte sequence, Byte position);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void new_tag_card(IntPtr pduel, UInt32 code, Byte owner, Byte location);

        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 process(IntPtr pduel);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 get_message(IntPtr pduel, IntPtr buf);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_log_message(IntPtr pduel, IntPtr buf);

        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_responseb(IntPtr pduel, IntPtr buf);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_responsei(IntPtr pduel, UInt32 value);

        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 query_card(IntPtr pduel, Byte playerid, Byte location, Byte sequence, Int32 queryFlag, IntPtr buf, Int32 useCache);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 query_field_count(IntPtr pduel, Byte playerid, Byte location);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 query_field_card(IntPtr pduel, Byte playerid, Byte location, Int32 queryFlag, IntPtr buf, Int32 useCache);
        [DllImport("ocgcore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 query_field_info(IntPtr pduel, IntPtr buf);

        #endregion

        #region Native Types

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ScriptReader(String scriptName, Int32* len);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt32 CardReader(UInt32 code, Card.CardData* pData);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate UInt32 MessageHandler(IntPtr pDuel, UInt32 messageType);

        #endregion

        #region Private Variables

        private static string _rootPath;
        private static string _scriptDirectory;
        private static IntPtr _buffer;

        private static ScriptReader _scriptCallback;
        private static CardReader _cardCallback;
        private static MessageHandler _messageCallback;

        #endregion

        #region Public Functions

        public static void Init(string rootPath = ".", string scriptDirectory = "script", string databaseFile = "cards.cdb")
        {
            _rootPath = rootPath;
            _scriptDirectory = scriptDirectory;

            CardsManager.Init(Path.Combine(Path.GetFullPath(rootPath), databaseFile));

            Duel.Duels = new Dictionary<IntPtr, Duel>();

            _buffer = Marshal.AllocHGlobal(65536);

            _cardCallback = OnCardReader;
            _scriptCallback = OnScriptReader;
            _messageCallback = OnMessageHandler;

            set_card_reader(_cardCallback);
            set_script_reader(_scriptCallback);
            set_message_handler(_messageCallback);
        }

        public static void Dispose()
        {
            foreach (Duel duel in Duel.Duels.Values)
            {
                duel.Dispose();
            }
            Marshal.FreeHGlobal(_buffer);
        }

        #endregion

        #region Private Callbacks

        private static UInt32 OnCardReader(UInt32 code, Card.CardData* pData)
        {
            Card card = CardsManager.GetCard((int) code);
            if (card != null)
            {
                *pData = card.Data;
            }
            return code;
        }

        private static IntPtr OnScriptReader(String scriptName, Int32* len)
        {
            string filename = GetScriptFilename(scriptName);
            if (!File.Exists(filename))
            {
                return IntPtr.Zero;
            }
            byte[] content = File.ReadAllBytes(filename);
            *len = content.Length;
            Marshal.Copy(content, 0, _buffer, content.Length);
            return _buffer;
        }

        private static UInt32 OnMessageHandler(IntPtr pDuel, UInt32 messageType)
        {
            if (Duel.Duels.ContainsKey(pDuel))
            {
                Duel duel = Duel.Duels[pDuel];
                duel.OnMessage(messageType);
            }
            return 0;
        }

        #endregion

        #region Private Functions

        private static string GetScriptFilename(string scriptName)
        {
            return Path.Combine(_rootPath, scriptName.Replace("./script", _scriptDirectory));
        }

        #endregion
    }
}