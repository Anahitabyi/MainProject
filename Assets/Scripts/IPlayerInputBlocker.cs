using Unity.Netcode;
public interface IPlayerInputBlocker
{
    bool isInputBlocked { get; set; }
    //NetworkVariable<bool> isInputBlockedNet { get; }
}
