using Content.Server.Speech;
using Content.Server.Speech.EntitySystems;
using Content.Shared.DeltaV.Storage.Components;
using Content.Shared.DeltaV.Storage.EntitySystems;
using Content.Shared.Storage;

namespace Content.Server.DeltaV.Storage.EntitySystems;

public sealed class MouthStorageSystem : SharedMouthStorageSystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MouthStorageComponent, AccentGetEvent>(OnAccent);
    }

    // Force you to mumble if you have items in your mouth
    private void OnAccent(EntityUid uid, MouthStorageComponent component, AccentGetEvent args)
    {
        if (IsMouthBlocked(component))
            args.Message = _replacement.ApplyReplacements(args.Message, "mumble");
    }
}
