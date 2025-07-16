using Unity.Entities;

public struct PendingAttackRequest_Component : IComponentData
{
    public Entity TargetStructureEntity; // L'entit� della struttura che si attacca
    public AttackCardData CardToUse;     // I dati della carta selezionata per l'attacco

    // Costruttore 
    public PendingAttackRequest_Component(Entity target, AttackCardData card)
    {
        TargetStructureEntity = target;
        CardToUse = card;
    }
}