using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Utility;

namespace Content.Server._Starlight.Shadekin;

public sealed class StationTheDarkSystem : EntitySystem
{
    [Dependency] private readonly MapLoaderSystem _loader = default!;

    private readonly ResPath _map = new("/Maps/_HL/TheDark.yml");
    private EntityUid? _thedark;

    public override void Initialize()
    {
        SubscribeLocalEvent<StationTheDarkComponent, MapInitEvent>(OnStationInit);
    }

    private void OnStationInit(EntityUid uid, StationTheDarkComponent component, MapInitEvent args)
    {
        if (_thedark is not null)
            return;

        var opts = DeserializationOptions.Default with { InitializeMaps = true };
        if (_loader.TryLoadMap(_map, out var map, out _, opts))
            _thedark = map;
    }
}

[RegisterComponent]
public sealed partial class StationTheDarkComponent : Component { }
