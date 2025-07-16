using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    public int initialResources = 10; // Valore iniziale delle risorse, configurabile dall'Inspector

    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            // Ottieni l'entità. Essendo un'entità puramente logica TransformUsageFlags.None è appropriato.
            // Se dovesse avere un avatar useremmo Dynamic.
            var entity = GetEntity(TransformUsageFlags.None);

            // Aggiungi il PlayerResources_Component con le risorse iniziali
            AddComponent(entity, new PlayerResources_Component(authoring.initialResources));

        }
    }
}