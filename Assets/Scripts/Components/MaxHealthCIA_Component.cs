using Unity.Entities;

public struct MaxHealthCIA_Component : IComponentData
{
    public float MaxConfidentiality;
    public float MaxIntegrity;
    public float MaxAvailability;
    public MaxHealthCIA_Component(float maxC, float maxI, float maxA)
    {
        MaxConfidentiality = maxC;
        MaxIntegrity = maxI;
        MaxAvailability = maxA;
    }
}