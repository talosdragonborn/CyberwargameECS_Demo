using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering; // Namespace necessario per URPMaterialPropertyEmissionColor

[UpdateInGroup(typeof(PresentationSystemGroup))] // Esegui nel gruppo di presentazione
[BurstCompile]
public partial struct EmissionHighlightSystem : ISystem
{
    private float4 highlightEmissionColor;
    private float4 defaultEmissionColor;

    public void OnCreate(ref SystemState state)
    {
        highlightEmissionColor = new float4(1.5f, 1.2f, 0.5f, 1f); // Giallo-Arancio HDR
        defaultEmissionColor = new float4(0f, 0f, 0f, 1f);       // Nero 
       state.RequireForUpdate<URPMaterialPropertyEmissionColor>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Imposto colore di emissione per le entità selezionate
        foreach (var emissiveColor in
                 SystemAPI.Query<RefRW<URPMaterialPropertyEmissionColor>>()
                     .WithAll<Selected_TagComponent, Selectable_TagComponent>())
        {
            emissiveColor.ValueRW.Value = highlightEmissionColor;
        }

        // Imposto colore di emissione di default per le entità selezionabili NON selezionate
        foreach (var emissiveColor in
                 SystemAPI.Query<RefRW<URPMaterialPropertyEmissionColor>>()
                     .WithAll<Selectable_TagComponent>()
                     .WithNone<Selected_TagComponent>())
        {
            emissiveColor.ValueRW.Value = defaultEmissionColor;
        }
    }

    public void OnDestroy(ref SystemState state) { }
}