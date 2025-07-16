using Unity.Collections; 

[System.Serializable]
public struct AttackCardData
{
    public int ID;
    public FixedString64Bytes Name; 
    public int ResourceCost;

    public float DamageConfidentiality;
    public float DamageIntegrity;
    public float DamageAvailability;

    public int SuccessStat;
    public int StealthStat;

    // Costruttore 
    public AttackCardData(int id, FixedString64Bytes name, int cost, 
                          float damageC, float damageI, float damageA,
                          int successThreshold, int stealthThreshold)
    {
        ID = id;
        Name = name; 
        ResourceCost = cost;
        DamageConfidentiality = damageC;
        DamageIntegrity = damageI;
        DamageAvailability = damageA;
        SuccessStat = successThreshold;
        StealthStat = stealthThreshold;
    }

}