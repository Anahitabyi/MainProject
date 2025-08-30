using System;
using Unity.Netcode;
using UnityEngine;

public struct CharacterSelection : INetworkSerializable, IEquatable<CharacterSelection>
{
    public ulong clientId;
    public int characterId;

    public CharacterSelection(ulong clientId, int characterId = -1)
    {
        this.clientId = clientId;
        this.characterId = characterId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref characterId);
    }

    public bool Equals(CharacterSelection other)
    {
        return clientId == other.clientId && characterId == other.characterId;
    }
}
