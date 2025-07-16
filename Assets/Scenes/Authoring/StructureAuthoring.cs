using Unity.Entities;
using Unity.Collections; // Per FixedString
using UnityEngine;
using System;

public class StructureAuthoring : MonoBehaviour
{
    [Header("Structure Identification")]
    public int structureID;
    public string structureName = "Default Structure"; // Usiamo string qui per facilità nell'Inspector

    [Header("CIA Stats (Max Health)")]
    public float maxConfidentiality = 100f;
    public float maxIntegrity = 100f;
    public float maxAvailability = 100f;

    // Non esponiamo CurrentHealthCIA qui, verrà inizializzato da MaxHealthCIA
    // Non esponiamo Selectable_TagComponent o Selected_TagComponent, verranno aggiunti di default

    class Baker : Baker<StructureAuthoring>
    {
        public override void Bake(StructureAuthoring authoring)
        {
            // Ottieni l'entità per questo GameObject
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Aggiungiamo StructureID_Component
            // Convertiamo la stringa dell'Inspector in FixedString
            AddComponent(entity, new StructureID_Component
            {
                ID = authoring.structureID,
                Name = new FixedString64Bytes(authoring.structureName)
            });

            // Aggiungimo MaxHealthCIA_Component
            AddComponent(entity, new MaxHealthCIA_Component
            {
                MaxConfidentiality = authoring.maxConfidentiality,
                MaxIntegrity = authoring.maxIntegrity,
                MaxAvailability = authoring.maxAvailability
            });

            // Aggiungi CurrentHealthCIA_Component, inizializzandolo con i valori massimi
            AddComponent(entity, new CurrentHealthCIA_Component
            {
                Confidentiality = authoring.maxConfidentiality,
                Integrity = authoring.maxIntegrity,
                Availability = authoring.maxAvailability
            });

            // Aggiungi i componenti Tag
            AddComponent<Selectable_TagComponent>(entity);

        }
    }
}