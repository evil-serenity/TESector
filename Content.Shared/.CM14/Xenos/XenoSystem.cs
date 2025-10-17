using System;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.CM14.Xenos.Evolution;
using Content.Shared.Mind;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared.CM14.Xenos;

public sealed class XenoSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoComponent, MapInitEvent>(OnXenoMapInit);
        SubscribeLocalEvent<XenoComponent, EntityUnpausedEvent>(OnXenoUnpaused);
        SubscribeLocalEvent<XenoComponent, XenoOpenEvolutionsEvent>(OnXenoEvolve);
        SubscribeLocalEvent<XenoComponent, EvolveBuiMessage>(OnXenoEvolveBui);
    }

    private void OnXenoMapInit(Entity<XenoComponent> ent, ref MapInitEvent args)
    {
        // Legacy action list registration
        foreach (var actionId in ent.Comp.ActionIds)
        {
            if (!ent.Comp.Actions.ContainsKey(actionId) &&
                _action.AddAction(ent, actionId) is { } newAction)
            {
                ent.Comp.Actions[actionId] = newAction;
            }
        }

        // Evolution action
        if (ent.Comp.EvolvesTo.Count > 0)
        {
            _action.AddAction(ent, ref ent.Comp.EvolveAction, ent.Comp.EvolveActionId);
            _action.SetCooldown(ent.Comp.EvolveAction, _timing.CurTime, _timing.CurTime + ent.Comp.EvolveIn);
        }
    }

    private void OnXenoUnpaused(Entity<XenoComponent> ent, ref EntityUnpausedEvent args)
    {
        ent.Comp.NextPlasmaRegenTime += args.PausedTime;
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenoComponent>();
        var time = _timing.CurTime;

        while (query.MoveNext(out var uid, out var xeno))
        {
            if (time < xeno.NextPlasmaRegenTime)
                continue;

            xeno.Plasma += xeno.PlasmaRegen;
            xeno.NextPlasmaRegenTime = time + xeno.PlasmaRegenCooldown;
            Dirty(uid, xeno);
        }
    }

    public bool HasPlasma(Entity<XenoComponent> xeno, int plasma)
    {
        return xeno.Comp.Plasma >= plasma;
    }

    public bool TryRemovePlasmaPopup(Entity<XenoComponent> xeno, int plasma)
    {
        if (!HasPlasma(xeno, plasma))
        {
            _popup.PopupClient(Loc.GetString("cm-xeno-not-enough-plasma"), xeno, xeno);
            return false;
        }

        RemovePlasma(xeno, plasma);
        return true;
    }

    public void RemovePlasma(Entity<XenoComponent> xeno, int plasma)
    {
        xeno.Comp.Plasma = Math.Max(xeno.Comp.Plasma - plasma, 0);
        Dirty(xeno);
        if (xeno.Comp.EvolvesTo.Count == 0)
            return;

        _action.AddAction(xeno, ref xeno.Comp.EvolveAction, xeno.Comp.EvolveActionId);
        _action.SetCooldown(xeno.Comp.EvolveAction, _timing.CurTime, _timing.CurTime + xeno.Comp.EvolveIn);
    }

    private void OnXenoEvolve(Entity<XenoComponent> ent, ref XenoOpenEvolutionsEvent args)
    {
        if (_net.IsClient || !TryComp(ent, out ActorComponent? actor))
            return;

        _ui.OpenUi(ent.Owner, XenoEvolutionUIKey.Key, actor.PlayerSession);
    }

    private void OnXenoEvolveBui(Entity<XenoComponent> ent, ref EvolveBuiMessage args)
    {
        if (!_mind.TryGetMind(ent, out var mindId, out _))
            return;

        var choices = ent.Comp.EvolvesTo.Count;
        if (args.Choice >= choices || args.Choice < 0)
        {
            Log.Warning($"User {ToPrettyString(args.Actor)} sent an out of bounds evolution choice: {args.Choice}. Choices: {choices}");
            return;
        }

        var evolution = Spawn(ent.Comp.EvolvesTo[args.Choice], _transform.GetMoverCoordinates(ent.Owner));
        _mind.TransferTo(mindId, evolution);
        _mind.UnVisit(mindId);
        Del(ent.Owner);

        if (TryComp(ent, out ActorComponent? actor))
            _ui.CloseUi(ent.Owner, XenoEvolutionUIKey.Key, actor.PlayerSession);
    }
}
