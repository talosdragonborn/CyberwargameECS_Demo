using Unity.Entities;

public struct CurrentHealthCIA_Component : IComponentData
{
    public float Confidentiality;
    public float Integrity;
    public float Availability;
    public CurrentHealthCIA_Component(float initialC, float initialI, float initialA)
    {
        Confidentiality = initialC;
        Integrity = initialI;
        Availability = initialA;
    }
}