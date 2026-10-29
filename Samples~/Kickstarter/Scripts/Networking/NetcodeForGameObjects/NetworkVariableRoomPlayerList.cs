using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace HnSF.sessionhandling.handlers.NGO
{
    [Serializable]
    public class NetworkVariableRoomPlayerList : NetworkVariableBase
    {
        /// <summary>
        /// Delegate type for value changed event
        /// </summary>
        /// <param name="previousValue">The value before the change</param>
        /// <param name="newValue">The new value</param>
        public delegate void OnValueChangedDelegate();
        /// <summary>
        /// The callback to be invoked when the value gets changed
        /// </summary>
        public OnValueChangedDelegate OnValueChanged;
        
        private List<SessionHandlerTradionalLobbyNGO.LobbyRepresentation.Player> players =
            new List<SessionHandlerTradionalLobbyNGO.LobbyRepresentation.Player>();

        public virtual List<SessionHandlerTradionalLobbyNGO.LobbyRepresentation.Player> Value
        {
            get => players;
            set
            {
                /*
                if (CannotWrite)
                {
                    LogWritePermissionError();
                    return;
                }*/

                // Compare the Value being applied to the current value
                /*if (!NetworkVariableSerialization<T>.AreEqual(ref m_InternalValue, ref value))
                {
                    T previousValue = m_InternalValue;
                    m_InternalValue = value;
                    NetworkVariableSerialization<T>.Duplicate(m_InternalValue, ref m_InternalOriginalValue);
                    SetDirty(true);
                    m_IsDisposed = false;
                    OnValueChanged?.Invoke(previousValue, m_InternalValue);
                }*/
                players = value;
                SetDirty(true);
                OnValueChanged?.Invoke();
            }
        }
        
        public bool CheckDirtyState(bool forceCheck = false)
        {
            SetDirty(true);
            return true;
            /*
            var isDirty = base.IsDirty();

            // A client without permissions invoking this method should only check to assure the current value is equal to the last known current value
            if (CannotWrite)
            {
                // If modifications are detected, then revert back to the last known current value
                if (!NetworkVariableSerialization<T>.AreEqual(ref m_InternalValue, ref m_InternalOriginalValue))
                {
                    NetworkVariableSerialization<T>.Duplicate(m_InternalOriginalValue, ref m_InternalValue);
                }
                return false;
            }

            // Compare the previous with the current if not dirty or forcing a check.
            if ((!isDirty || forceCheck) && !NetworkVariableSerialization<T>.AreEqual(ref m_PreviousValue, ref m_InternalValue))
            {
                SetDirty(true);
                OnValueChanged?.Invoke(m_PreviousValue, m_InternalValue);
                m_IsDisposed = false;
                isDirty = true;
            }
            return isDirty;*/
        }
        
        public override void WriteField(FastBufferWriter writer)
        {
            writer.WriteValueSafe(players.Count);
            foreach (var playerEntry in players)
            {
                playerEntry.WriteField(ref writer);
            }
        }

        public override void ReadField(FastBufferReader reader)
        {
            var itemsToUpdate = (int)0;
            reader.ReadValueSafe(out itemsToUpdate);
            players.Clear();

            for (int i = 0; i < itemsToUpdate; i++)
            {
                var newPlayer = new SessionHandlerTradionalLobbyNGO.LobbyRepresentation.Player();

                newPlayer.ReadField(ref reader);
                
                players.Add(newPlayer);
            }
        }

        public enum EventType
        {
            Add,
            Remove,
            Value,
            Clear,
            Full
        }
        
        public override void WriteDelta(FastBufferWriter writer)
        {
            writer.WriteValueSafe((ushort)1);
            writer.WriteValueSafe(EventType.Full);
            WriteField(writer);
            OnValueChanged?.Invoke();
        }
        
        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            reader.ReadValueSafe(out ushort deltaCount);
            reader.ReadValueSafe(out EventType eventType);

            switch (eventType)
            {
                case EventType.Full:
                    ReadField(reader);
                    ResetDirty();
                    OnValueChanged?.Invoke();
                    break;
            }
        }
    }
}