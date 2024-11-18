using Content.Shared.Bed.Sleep;
using Content.Server.Chat.Managers;  // DeltaV Narcolepsy port from EE
using Content.Shared.StatusEffect;
using Content.Shared.Dataset; // DeltaV Narcolepsy port from EE
using Content.Shared.Popups; // DeltaV Narcolepsy port from EE
using Robust.Shared.Prototypes; // DeltaV Narcolepsy port from EE
using Robust.Shared.Random;

namespace Content.Server.Traits.Assorted;

/// <summary>
/// This handles narcolepsy, causing the affected to fall asleep uncontrollably at a random interval.
/// </summary>
public sealed class NarcolepsySystem : EntitySystem
{
    [ValidatePrototypeId<StatusEffectPrototype>]
    private const string StatusEffectKey = "ForcedSleep"; // Same one used by N2O and other sleep chems.

    [Dependency] private readonly IChatManager _chatMan = default!; // DeltaV Narcolepsy port from EE
    [Dependency] private readonly SharedPopupSystem _popups = default!;  // DeltaV Narcolepsy port from EE
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<NarcolepsyComponent, ComponentStartup>(SetupNarcolepsy);
        SubscribeLocalEvent<NarcolepsyComponent, SleepStateChangedEvent>(OnSleepChanged);  // DeltaV Narcolepsy port from EE
    }

    private void SetupNarcolepsy(EntityUid uid, NarcolepsyComponent component, ComponentStartup args)
    {
        PrepareNextIncident((uid, component));
    }

    private void OnSleepChanged(Entity<NarcolepsyComponent> ent, ref SleepStateChangedEvent args)
    {
        // When falling asleep while an incident is nigh, force it to happen immediately.
        if (args.FellAsleep)
        {
            StartIncident(ent);
            return;
        }

        // When waking up after sleeping, show a popup.
        {
            DoWakeupPopup(ent);
        }
    }

    public void AdjustNarcolepsyTimer(EntityUid uid, float setTime, NarcolepsyComponent? narcolepsy = null)
    {
        if (!Resolve(uid, ref narcolepsy, false) || narcolepsy.NextIncidentTime > setTime)
            return;

        narcolepsy.NextIncidentTime = setTime;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NarcolepsyComponent>();
        while (query.MoveNext(out var uid, out var narcolepsy))
        {
            if (HasComp<SleepingComponent>(uid))
                continue;

            narcolepsy.NextIncidentTime -= frameTime;
            if (narcolepsy.NextIncidentTime <= narcolepsy.TimeBeforeWarning && narcolepsy.NextIncidentTime < narcolepsy.LastWarningRollTime - 1f)
            {
                // Roll for showing a popup. There should really be a class for doing this.
                narcolepsy.LastWarningRollTime = narcolepsy.NextIncidentTime;
                if (_random.Prob(narcolepsy.WarningChancePerSecond))
                {
                    DoWarningPopup(uid);
                    narcolepsy.LastWarningRollTime = 0f; // Do not show any more popups for the upcoming incident
                }
            }

            if (narcolepsy.NextIncidentTime >= 0)
                continue;

            StartIncident((uid, narcolepsy));
        }
    }

    public void StartIncident(Entity<NarcolepsyComponent> ent)
    {
        var duration = _random.NextFloat(ent.Comp.DurationOfIncident.X, ent.Comp.DurationOfIncident.Y);
        PrepareNextIncident(ent, duration);

        _statusEffects.TryAddStatusEffect<ForcedSleepingComponent>(ent, StatusEffectKey, TimeSpan.FromSeconds(duration), false);
        var sleepingcomponent = EntityManager.GetComponent<ForcedSleepingComponent> (ent);
        if (sleepingcomponent != EntityManager.GetComponent<ForcedSleepingComponent>(ent))
            Logger.Error($"Narcoleptic didn't go to bed when they should have on {EntityManager.GetComponent<ForcedSleepingComponent> (ent)}");

        {

        }
    }

    private void PrepareNextIncident(Entity<NarcolepsyComponent> ent, float startingFrom = 0f)
    {
        var time = _random.NextFloat(ent.Comp.TimeBetweenIncidents.X, ent.Comp.TimeBetweenIncidents.Y);
        ent.Comp.NextIncidentTime = startingFrom + time;
        ent.Comp.LastWarningRollTime = float.MaxValue;
    }

    private string GetRandomWakeup()
    {
        return Loc.GetString(_random.Pick(_prototypeManager.Index<LocalizedDatasetPrototype>("NarcolepsyWakeupDataset").Values));
    }
    private string GetRandomWarning()
    {
        return Loc.GetString(_random.Pick(_prototypeManager.Index<LocalizedDatasetPrototype>("NarcolepsyWarningDataset").Values));
    }

    private void DoWakeupPopup(EntityUid ent)
    {
        _popups.PopupEntity(GetRandomWakeup(),ent,ent);
    }

    private void DoWarningPopup(EntityUid ent)
    {
        _popups.PopupEntity(GetRandomWarning(),ent,ent);
    }

}
