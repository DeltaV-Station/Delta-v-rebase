﻿using Content.Server.Emp;
using Content.Server.Explosion.Components;
using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Shared.CombatMode;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.DeltaV.CCVars;
using Content.Shared.FixedPoint;
using Robust.Server.Player;
using Robust.Shared.Configuration;

namespace Content.Server.DeltaV.RoundEnd;

public sealed class PacifiedRoundEnd : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly StoreSystem _storeSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    private bool _enabled;

    public override void Initialize()
    {
        base.Initialize();
        _configurationManager.OnValueChanged(DCCVars.RoundEndPacifist, v => _enabled = v, true);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnded);
    }

    private void OnRoundEnded(RoundEndTextAppendEvent ev)
    {
        if (!_enabled)
            return;

        var harmQuery = EntityQueryEnumerator<CombatModeComponent>();
        while (harmQuery.MoveNext(out var uid, out var _))
        {
            _entityManager.EnsureComponent<PacifiedComponent>(uid);
        }

        var grenadeQuery = EntityQueryEnumerator<ExplodeOnTriggerComponent>();
        while (grenadeQuery.MoveNext(out var uid, out var _))
        {
            _entityManager.RemoveComponent<ExplodeOnTriggerComponent>(uid);
        }

        var empQuery = EntityQueryEnumerator<EmpOnTriggerComponent>();
        while (empQuery.MoveNext(out var uid, out var _))
        {
            _entityManager.RemoveComponent<EmpOnTriggerComponent>(uid);
        }

        var uplinkQuery = EntityQueryEnumerator<StoreComponent>();
        while (uplinkQuery.MoveNext(out var uid, out var store))
        {
            var allCurrency = store.Balance;
            foreach (var money in allCurrency)
            {
                _storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2> { { money.Key, -money.Value } }, uid,
                    store);
            }
        }
    }
}
