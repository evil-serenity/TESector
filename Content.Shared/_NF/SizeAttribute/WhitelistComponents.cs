using Robust.Shared.Serialization;

namespace Content.Shared._NF.SizeAttribute;

/// <summary>
/// Whitelist data to apply when an entity is marked as "tall".
/// Provides cosmetic scale and optional physical adjustments.
/// </summary>
[RegisterComponent]
public sealed partial class TallWhitelistComponent : Component
{
    [DataField("scale")]
    public float Scale = 1f;

    [DataField("density")]
    public float Density = 1f;

    [DataField("cosmeticOnly")]
    public bool CosmeticOnly = true;

    [DataField("pseudoItem")]
    public bool PseudoItem = false;

    [DataField("shape")]
    public List<Box2i>? Shape;

    [DataField("storedOffset")]
    public Vector2i? StoredOffset;

    [DataField("storedRotation")]
    public float StoredRotation = 0f;
}

/// <summary>
/// Whitelist data to apply when an entity is marked as "short".
/// Provides cosmetic scale and optional physical adjustments.
/// </summary>
[RegisterComponent]
public sealed partial class ShortWhitelistComponent : Component
{
    [DataField("scale")]
    public float Scale = 1f;

    [DataField("density")]
    public float Density = 1f;

    [DataField("cosmeticOnly")]
    public bool CosmeticOnly = true;

    [DataField("pseudoItem")]
    public bool PseudoItem = false;

    [DataField("shape")]
    public List<Box2i>? Shape;

    [DataField("storedOffset")]
    public Vector2i? StoredOffset;

    [DataField("storedRotation")]
    public float StoredRotation = 0f;
}
