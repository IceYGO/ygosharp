using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using YGOSharp.OCGWrapper.Enums;

namespace YGOSharp.OCGWrapper
{
    public class Duel
    {
        #region Private Variables

        private readonly IntPtr _duelPtr;
        private readonly IntPtr _buffer;

        private Func<GameMessage, BinaryReader, byte[], int> _analyzer;
        private Action<string> _errorHandler;

        #endregion

        #region Public Methods

        public void SetAnalyzer(Func<GameMessage, BinaryReader, byte[], int> analyzer)
        {
            _analyzer = analyzer;
        }

        public void SetErrorHandler(Action<string> errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public void InitPlayers(int startLp, int startHand, int drawCount)
        {
            Api.set_player_info(_duelPtr, 0, startLp, startHand, drawCount);
            Api.set_player_info(_duelPtr, 1, startLp, startHand, drawCount);
        }

        public void AddCard(int cardId, int owner, CardLocation location)
        {
            Api.new_card(_duelPtr, (uint)cardId, (byte)owner, (byte)owner, (byte)location, 0, 0);
        }

        public void AddTagCard(int cardId, int owner, CardLocation location)
        {
            Api.new_tag_card(_duelPtr, (uint)cardId, (byte)owner, (byte)location);
        }

        public void Start(int options)
        {
            Api.start_duel(_duelPtr, options);
        }

        public int Process()
        {
            int fail = 0;
            while (true)
            {
                int result = Api.process(_duelPtr);
                int len = result & 0xFFFF;

                if (len > 0)
                {
                    fail = 0;
                    byte[] arr = new byte[4096];
                    Api.get_message(_duelPtr, _buffer);
                    Marshal.Copy(_buffer, arr, 0, 4096);
                    result = HandleMessage(new BinaryReader(new MemoryStream(arr)), arr, len);
                    if (result != 0)
                        return result;
                }
                else if (++fail == 10)
                    return -1;
            }
        }

        public void SetResponse(int resp)
        {
            Api.set_responsei(_duelPtr, (uint)resp);
        }

        public void SetResponse(byte[] resp)
        {
            if (resp.Length > 64) return;
            IntPtr buf = Marshal.AllocHGlobal(64);
            Marshal.Copy(resp, 0, buf, resp.Length);
            Api.set_responseb(_duelPtr, buf);
            Marshal.FreeHGlobal(buf);
        }

        public int QueryFieldCount(int player, CardLocation location)
        {
            return Api.query_field_count(_duelPtr, (byte)player, (byte)location);
        }

        public byte[] QueryFieldCard(int player, CardLocation location, int flag = 0xFFFFFF & ~(int)Query.ReasonCard, bool useCache = false)
        {
            int len = Api.query_field_card(_duelPtr, (byte)player, (byte)location, flag, _buffer, useCache ? 1 : 0);
            byte[] result = new byte[len];
            Marshal.Copy(_buffer, result, 0, len);
            return result;
        }

        public byte[] QueryCard(int player, int location, int sequence, int flag = 0xFFFFFF & ~(int)Query.ReasonCard, bool useCache = false)
        {
            int len = Api.query_card(_duelPtr, (byte)player, (byte)location, (byte)sequence, flag, _buffer, useCache ? 1 : 0);
            byte[] result = new byte[len];
            Marshal.Copy(_buffer, result, 0, len);
            return result;
        }

        public byte[] QueryFieldInfo()
        {
            Api.query_field_info(_duelPtr,  _buffer);
            byte[] result = new byte[256];
            Marshal.Copy(_buffer, result, 0, 256);
            return result;
        }

        public void End()
        {
            Api.end_duel(_duelPtr);
            Dispose();
        }

        public IntPtr GetNativePtr()
        {
            return _duelPtr;
        }

        #endregion

        #region Internal Constructor

        internal Duel(IntPtr duelPtr)
        {
            _buffer = Marshal.AllocHGlobal(4096);
            _duelPtr = duelPtr;
            Duels.Add(_duelPtr, this);
        }

        #endregion

        #region Internal Methods

        internal void Dispose()
        {
            Marshal.FreeHGlobal(_buffer);
            Duels.Remove(_duelPtr);
        }

        internal void OnMessage(UInt32 messageType)
        {
            byte[] arr = new byte[256];
            Api.get_log_message(_duelPtr, _buffer);
            Marshal.Copy(_buffer, arr, 0, 256);
            string message = System.Text.Encoding.UTF8.GetString(arr);
            if (message.Contains("\0"))
                message = message.Substring(0, message.IndexOf('\0'));
            if (_errorHandler != null)
                _errorHandler.Invoke(message);
        }

        private int HandleMessage(BinaryReader reader, byte[] raw, int len)
        {
            while (reader.BaseStream.Position < len)
            {
                GameMessage msg = (GameMessage)reader.ReadByte();
                int result = -1;
                if (_analyzer != null)
                    result = _analyzer.Invoke(msg, reader, raw);
                if (result != 0)
                    return result;
            }
            return 0;
        }

        #endregion

        #region Static Variables & Functions

        internal static IDictionary<IntPtr, Duel> Duels;

        public static Duel Create(uint seed)
        {
            MtRandom random = new MtRandom();
            random.Reset(seed);
            IntPtr pDuel = Api.create_duel(random.Rand());
            return Create(pDuel);
        }

        internal static Duel Create(IntPtr pDuel)
        {
            if (pDuel == IntPtr.Zero)
                return null;
            return new Duel(pDuel);
        }

        #endregion
    }
}
