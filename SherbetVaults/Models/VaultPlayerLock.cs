﻿using System;
using System.Collections.Concurrent;

namespace SherbetVaults.Models
{
    /// <summary>
    /// Helps prevent client-side graphical item graphical glitches if a player spams the vault command.
    /// </summary>
    public class VaultPlayerLock
    {
        public ConcurrentDictionary<ulong, bool> m_PlayerBlocks = new ConcurrentDictionary<ulong, bool>();

        public OpenBlockToken TryObtainLock(ulong player, out bool valid)
        {
            if (m_PlayerBlocks.TryGetValue(player, out var blocked))
            {
                if (blocked)
                {
                    valid = false;
                    return default;
                }
            }
            m_PlayerBlocks[player] = true;
            valid = true;
            return new OpenBlockToken(player, this);
        }

        private void Release(ulong player)
        {
            m_PlayerBlocks[player] = false;
        }

        public struct OpenBlockToken : IDisposable
        {
            public ulong PlayerID { get; }
            private VaultPlayerLock Parent { get; }

            private bool Disposed;

            public OpenBlockToken(ulong playerID, VaultPlayerLock parent)
            {
                Disposed = false;
                PlayerID = playerID;
                Parent = parent;
            }

            public void Dispose()
            {
                if (!Disposed)
                {
                    Disposed = true;
                    Parent?.Release(PlayerID);
                }
            }
        }
    }
}