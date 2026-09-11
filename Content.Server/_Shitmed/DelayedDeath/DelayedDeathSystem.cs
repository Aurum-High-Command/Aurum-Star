using System.Linq; // Aurum
using Content.Server.Body.Components;
using Content.Server.Chat.Systems;
using Content.Shared.Body.Organ; // Aurum
using Content.Shared.Body.Systems; // Aurum
using Content.Shared.Chat; // Einstein Engines - Languages
using Content.Shared.Medical;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Rejuvenate; // Aurum

namespace Content.Server._Shitmed.DelayedDeath;

public partial class DelayedDeathSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!; // Aurum

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DelayedDeathComponent, TargetBeforeDefibrillatorZapsEvent>(OnDefibZap);
        SubscribeLocalEvent<DelayedDeathComponent, RejuvenateEvent>(OnRejuvenate); // Aurum
    }

    private void OnRejuvenate(EntityUid uid, DelayedDeathComponent component, RejuvenateEvent args) { RemComp<DelayedDeathComponent>(uid); } // Aurum

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        using var query = EntityQueryEnumerator<DelayedDeathComponent, MobStateComponent>();
        while (query.MoveNext(out var ent, out var comp, out var mob))
        {
            comp.DeathTimer += frameTime;

            if (comp.DeathTimer >= comp.DeathTime && !_mobState.IsDead(ent, mob))
            {
                // go crit then dead so deathgasp can happen
                _mobState.ChangeMobState(ent, MobState.Critical, mob);
                _mobState.ChangeMobState(ent, MobState.Dead, mob);
            }
        }
    }

    private void OnDefibZap(Entity<DelayedDeathComponent> ent, ref TargetBeforeDefibrillatorZapsEvent args)
    {
        // Aurum Start
        if (ent.Comp.FromHeartFailure
            && _bodySystem.TryGetBodyOrganEntityComps<BrainComponent>(ent.Owner, out var brains)
            && TryComp<OrganComponent>(brains.First(), out var organ) // Aurum
            && organ.Enabled)
            return; // Aurum end

        // can't defib someone without a heart or brain pal
        args.Cancel();

        _chat.TrySendInGameICMessage(args.Defib, Loc.GetString("defibrillator-missing-organs"),
            InGameICChatType.Speak, true);
    }
}
