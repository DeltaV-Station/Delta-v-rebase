﻿using Content.Shared.DeltaV.Biscuit;

namespace Content.Server.DeltaV.Biscuit;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class BiscuitComponent : SharedBiscuitComponent
{
    [DataField]
    public bool Cracked { get; set; }
}
