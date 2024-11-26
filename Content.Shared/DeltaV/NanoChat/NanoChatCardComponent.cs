using Content.Shared.DeltaV.CartridgeLoader.Cartridges;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.DeltaV.NanoChat;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedNanoChatSystem))]
[AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class NanoChatCardComponent : Component
{
    /// <summary>
    ///     The number assigned to this card.
    /// </summary>
    [DataField, AutoNetworkedField]
    [Access(typeof(SharedNanoChatSystem), Other = AccessPermissions.ReadWrite)]
    public uint? Number;

    /// <summary>
    ///     All chat recipients stored on this card.
    /// </summary>
    [DataField]
    [Access(typeof(SharedNanoChatSystem), Other = AccessPermissions.ReadWriteExecute)]
    public Dictionary<uint, NanoChatRecipient> Recipients = new();

    /// <summary>
    ///     All messages stored on this card, keyed by recipient number.
    /// </summary>
    [DataField]
    [Access(typeof(SharedNanoChatSystem), Other = AccessPermissions.ReadWriteExecute)]
    public Dictionary<uint, List<NanoChatMessage>> Messages = new();

    /// <summary>
    ///     The currently selected chat recipient number.
    /// </summary>
    [DataField]
    [Access(typeof(SharedNanoChatSystem), Other = AccessPermissions.ReadWrite)]
    public uint? CurrentChat;

    /// <summary>
    ///     The maximum amount of recipients this card supports.
    /// </summary>
    [DataField]
    public int MaxRecipients = 50;

    /// <summary>
    ///     Last time a message was sent, for rate limiting.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    [Access(typeof(SharedNanoChatSystem), Other = AccessPermissions.ReadWrite)]
    public TimeSpan LastMessageTime; // TODO: actually use this, compare against actor and not the card

    /// <summary>
    ///     Whether to send notifications.
    /// </summary>
    [DataField]
    [Access(typeof(SharedNanoChatSystem), Other = AccessPermissions.ReadWrite)]
    public bool NotificationsMuted;
}
