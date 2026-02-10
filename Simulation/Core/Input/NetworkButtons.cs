using System;
using System.Runtime.InteropServices;

namespace Quantum
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct NetworkButtons
    {
        public const int SIZE = (sizeof(int));

        [FieldOffset(0)]
        private int _bits;

        public int Bits => _bits;

        public unsafe static void Serialize(void* ptr, FrameSerializer serializer)
        {
            serializer.Stream.Serialize(&((NetworkButtons*)ptr)->_bits);
        }

        public NetworkButtons(int buttons)
        {
            _bits = buttons;
        }

        public bool IsSet(int buttons)
        {
            return (_bits & buttons) != 0;
        }

        public void SetDown(int buttons)
        {
            _bits |= buttons;
        }

        public void SetUp(int buttons)
        {
            _bits &= ~(buttons);
        }

        public void Set(int button, bool state)
        {
            if (state)
            {
                SetDown(button);
            }
            else
            {
                SetUp(button);
            }
        }

        public void SetAllUp()
        {
            _bits = 0;
        }

        public void SetAllDown()
        {
            _bits = -1;
        }

        public bool IsSet<T>(T button) where T : unmanaged, Enum
        {
            return IsSet(Convert.ToInt32(button));
        }

        /*
        public void SetDown<T>(T button) where T : unmanaged, Enum
        {
            SetDown(Convert.ToInt32(button));
        }

        public void SetUp<T>(T button) where T : unmanaged, Enum
        {
            SetDown(Convert.ToInt32(button));
        }

        public void Set<T>(T button, bool state) where T : unmanaged, Enum
        {
            Set(Convert.ToInt32(button), state);
        }*/
        
        public void SetDown(ActorInputButtonType button)
        {
            SetDown((int)button);
        }

        public void SetUp(ActorInputButtonType button)
        {
            SetDown((int)button);
        }

        public void Set(ActorInputButtonType button, bool state)
        {
            Set((int)button, state);
        }

        public NetworkButtons GetPressed(NetworkButtons previous)
        {
            previous._bits = (previous._bits ^ _bits) & _bits;
            return previous;
        }

        public NetworkButtons GetReleased(NetworkButtons previous)
        {
            previous._bits = (previous._bits ^ _bits) & previous._bits;
            return previous;
        }

        public bool WasPressed(NetworkButtons previous, int button)
        {
            return GetPressed(previous).IsSet(button);
        }

        public bool WasReleased(NetworkButtons previous, int button)
        {
            return GetReleased(previous).IsSet(button);
        }

        public bool WasPressed<T>(NetworkButtons previous, T button) where T : unmanaged, Enum
        {
            return GetPressed(previous).IsSet(button);
        }

        public bool WasReleased<T>(NetworkButtons previous, T button) where T : unmanaged, Enum
        {
            return GetReleased(previous).IsSet(button);
        }
    }
}
