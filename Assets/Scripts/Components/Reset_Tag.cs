using Unity.Entities;

public struct Reset_Tag : IComponentData
{
    // Questo � un componente "tag" usato come comando.
    // La sua esistenza innesca il ResetSystem
}