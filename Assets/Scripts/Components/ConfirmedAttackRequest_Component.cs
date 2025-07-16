using Unity.Entities;

public struct ConfirmedAttackRequest_Component : IComponentData
{
    public Entity TargetStructureEntity;
    public AttackCardData CardUsed;

    public ConfirmedAttackRequest_Component(Entity target, AttackCardData card)
    {
        TargetStructureEntity = target;
        CardUsed = card;
    }
}