using Unity.Entities;

public struct PlayerResources_Component : IComponentData
{
    public int CurrentAmount;

    // Costruttore 
    public PlayerResources_Component(int initialAmount)
    {
        CurrentAmount = initialAmount;
    }
}