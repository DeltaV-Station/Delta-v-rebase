using Content.Shared.Access.Systems;
using Content.Shared.Popups;
using Content.Shared.Shipyard.Prototypes;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared.Shipyard;

/// <summary>
/// Handles shipyard console interaction.
/// <c>ShipyardSystem</c> does the heavy lifting serverside.
/// </summary>
public abstract class SharedShipyardConsoleSystem : EntitySystem
{
    [Dependency] protected readonly AccessReaderSystem _access = default!;
    [Dependency] protected readonly IPrototypeManager _proto = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<ShipyardConsoleComponent>(ShipyardConsoleUiKey.Key, subs =>
        {
            subs.Event<ShipyardConsolePurchaseMessage>(OnPurchase);
        });
    }

    private void OnPurchase(Entity<ShipyardConsoleComponent> ent, ref ShipyardConsolePurchaseMessage msg)
    {
        if (msg.Session.AttachedEntity is not {} user)
            return;

        if (!_access.IsAllowed(user, ent.Owner))
        {
            Popup.PopupClient(Loc.GetString("comms-console-permission-denied"), ent, user);
            Audio.PlayPredicted(ent.Comp.DenySound, ent, user);
            return;
        }

        if (!_proto.TryIndex(msg.Vessel, out var vessel) || vessel.Whitelist?.IsValid(ent) == false)
            return;

        TryPurchase(ent, user, vessel);
    }

    protected virtual void TryPurchase(Entity<ShipyardConsoleComponent> ent, EntityUid user, VesselPrototype vessel)
    {
    }
}
