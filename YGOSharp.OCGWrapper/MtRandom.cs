namespace YGOSharp.OCGWrapper
{
    internal class MtRandom
    {
        private const int N = 624;
        private const int M = 397;

        private uint _current;
        private uint _left = 1;
        private uint[] _state = new uint[N];

        internal MtRandom()
        {
            Init();
        }

        internal MtRandom(uint seed)
        {
            Init(seed);
        }

        internal void Init(uint seed = 19650218U)
        {
            _state[0] = seed & 4294967295U;
            for (int j = 1; j < N; ++j)
            {
                _state[j] = (uint)(1812433253U * (_state[j - 1] ^ (_state[j - 1] >> 30)) + j);
                _state[j] &= 4294967295U;
            }
        }

        internal uint Rand()
        {
            uint y;
            if (0 == --_left)
                NextState();
            y = _state[_current++];
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680U;
            y ^= (y << 15) & 0xefc60000U;
            y ^= (y >> 18);
            return y;
        }

        internal void Reset(uint rs)
        {
            Init(rs);
            NextState();
        }

        private void NextState()
        {
            int k = 0;
            for (int i = N - M + 1; --i != 0; )
            {
                _state[k] = (_state[k + M] ^ Twist(_state[k], _state[k + 1]));
                k = k + 1;
            }
            for (int i = M; --i != 0; )
            {
                _state[k] = (_state[k + M - N] ^ Twist(_state[k], _state[k + 1]));
                k = k + 1;
            }
            _state[k] = (_state[k + M - N] ^ Twist(_state[k], _state[0]));
            _left = N;
            _current = 0;
        }

        private static uint Twist(uint u, uint v)
        {
            return ((MixBits(u, v) >> 1) ^ ((v & 1U) != 0 ? 2567483615U : 0U));
        }

        private static uint MixBits(uint u, uint v)
        {
            return (u & 2147483648U) | (v & 2147483647U);
        }
    }
}