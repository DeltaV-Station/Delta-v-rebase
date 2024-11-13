using System.Linq;
using Content.Server.Objectives.Components;
using Content.Server.GameTicking; //DeltaV port from EE
using Content.Server.Station.Systems;  //DeltaV port from EE
using Content.Server.Revolutionary.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.CCVar;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Random;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// Handles kill person condition logic and picking random kill targets.
/// </summary>
public sealed class KillPersonConditionSystem : EntitySystem
{
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;

    private List<EntityUid> _wasKilled = new();
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KillPersonConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<PickRandomPersonComponent, ObjectiveAssignedEvent>(OnPersonAssigned);
        SubscribeLocalEvent<PickRandomHeadComponent, ObjectiveAssignedEvent>(OnHeadAssigned);
        SubscribeLocalEvent<RoundEndedEvent>(OnRoundEnd); //DeltaV Port from EE
    }

    private void OnGetProgress(EntityUid uid, KillPersonConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        if (!_target.GetTarget(uid, out var target))
            return;

        args.Progress = GetProgress(target.Value, comp.RequireDead);
    }

    private void OnPersonAssigned(EntityUid uid, PickRandomPersonComponent comp, ref ObjectiveAssignedEvent args)
    {
        AssignRandomTarget(uid, args, _ => true);
    }

    private void OnHeadAssigned(EntityUid uid, PickRandomHeadComponent comp, ref ObjectiveAssignedEvent args)
    {
        AssignRandomTarget(uid, args, mind => HasComp<CommandStaffComponent>(uid));
    }

    private void AssignRandomTarget(EntityUid uid, ObjectiveAssignedEvent args, Predicate<EntityUid> filter, bool fallbackToAny = true)
    {
        // invalid prototype
        if (!TryComp<TargetObjectiveComponent>(uid, out var target))
        {
            args.Cancelled = true;
            return;
        }

        // target already assigned
        if (target.Target != null)
            return;

        // Get all alive humans, filter out any with TargetObjectiveImmuneComponent
        var allHumans = _mind.GetAliveHumansExcept(args.MindId)
            .Where(mindId =>
            {
                if (!TryComp<MindComponent>(mindId, out var mindComp) || mindComp.OwnedEntity == null)
                    return false;
                return !HasComp<TargetObjectiveImmuneComponent>(mindComp.OwnedEntity.Value);
            })
            .ToList();

        // Filter out targets based on the filter
        var filteredHumans = allHumans.Where(mind => filter(mind)).ToList();

        // There's no humans and we can't fall back to any other target
        if (filteredHumans.Count == 0 && !fallbackToAny)
        {
            args.Cancelled = true;
            return;
        }

        // Pick between humans matching our filter or fall back to all humans alive
        var selectedHumans = filteredHumans.Count > 0 ? filteredHumans : allHumans;

        _target.SetTarget(uid, _random.Pick(selectedHumans), target);
    }

    private float GetProgress(EntityUid target, bool requireDead)
    {
        // deleted or gibbed or something, counts as dead
        if (!TryComp<MindComponent>(target, out var mind) || mind.OwnedEntity == null)
        {                                                                                //DeltaV Port from EE
            if (!requireDead && !_wasKilled.Contains(target)) _wasKilled.Add(target);    //DeltaV Port from EE
            return 1f;
        }

        // dead is success
        if (_mind.IsCharacterDeadIc(mind))
        {
            if (!requireDead && !_wasKilled.Contains(target)) _wasKilled.Add(target);    //DeltaV Port from EE
            return 1f;
        }

        // if the target was killed once and it isn't a head objective
        if (_wasKilled.Contains(target))
            return 1f;

        return 0f;
    }
        // if the target has to be dead dead then don't check evac stuff
//        if (requireDead)
//            return 0f;

        // if evac is disabled then they really do have to be dead
//        if (!_config.GetCVar(CCVars.EmergencyShuttleEnabled))
//            return 0f;

        // target is escaping so you fail
//        if (_emergencyShuttle.IsTargetEscaping(mind.OwnedEntity.Value))
//            return 0f;
//
//        // evac has left without the target, greentext since the target is afk in space with a full oxygen tank and coordinates off.
//        if (_emergencyShuttle.ShuttlesLeft)
//            return 1f;
//
//        // if evac is still here and target hasn't boarded, show 50% to give you an indicator that you are doing good
//        return _emergencyShuttle.EmergencyShuttleArrived ? 0.5f : 0f;
    // Clear the wasKilled list on round end
    private void OnRoundEnd(RoundEndedEvent ev) //DeltaV port from EE
        => _wasKilled.Clear();  //DeltaV port from EE
}
