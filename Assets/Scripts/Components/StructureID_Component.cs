using Unity.Entities;
using Unity.Collections; // Per FixedStringBytes

public struct StructureID_Component : IComponentData
{
    public int ID;
    public FixedString64Bytes Name; // Una stringa di lunghezza fissa (64 byte in questo caso)

    // Costruttore 
    public StructureID_Component(int id, FixedString64Bytes name)
    {
        ID = id;
        Name = name;
    }
}