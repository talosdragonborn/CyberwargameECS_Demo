using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // !!! AGGIUNTO QUESTO USING !!!

[BurstCompile]
public partial struct SelectionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // 1. Controlla click con Input System
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        // Controllo se il mouse � sopra un elemento UI !!!
        // � importante eseguire questo controllo PRIMA di qualsiasi logica di raycasting nella scena 3D,
        // per evitare che un click sulla UI venga interpretato anche come un click di selezione/deselezione nel mondo 3D.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // Se il mouse � sopra un oggetto UI (come un bottone),
            // il SelectionSystem ignora questo click e non fa nulla.
            // L'input verr� gestito dagli script della UI (es. PlayerHandDisplay).
            return;
        }
        // !!! FINE MODIFICA !!!

        var cam = Camera.main;
        if (cam == null)
        {
            // Se non c'� una camera principale, non possiamo fare il raycast.
            // Potrebbe essere utile loggare un warning qui se questo stato � inaspettato.
            // Debug.LogWarning("SelectionSystem: Camera.main non trovata.");
            return;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        UnityEngine.Ray ray = cam.ScreenPointToRay(mousePos);

        // Otteniamo il mondo della fisica ECS. � importante che BuildPhysicsWorld sia stato eseguito.
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        RaycastInput raycastInput = new RaycastInput
        {
            Start = ray.origin,
            End = ray.origin + ray.direction * 100f, // 100f � la distanza del raggio
            Filter = CollisionFilter.Default //  filtro di collisione di default
        };

        // Usiamo un EntityCommandBuffer per registrare i comandi di modifica perch� le modifiche vengono applicate tutte insieme alla fine.
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Toglgo il tag Selected_TagComponent da TUTTE le entit� che lo possiedono
        foreach (var (selectedTag, entity) in SystemAPI.Query<RefRO<Selected_TagComponent>>().WithEntityAccess())
        {
            // selectedTag non � usato, ma la query necessita di un tipo di componente.
            // WithEntityAccess() ci d� l'entit�.
            ecb.RemoveComponent<Selected_TagComponent>(entity);
        }

        // Raycast nel mondo della fisica ECS.
        if (physicsWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit hit))
        {
            // se il raycast ha colpito qualcosa. Otteniamo l'entit� associata al modello colpito.
            var entityHit = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;

            // Controlliamo se l'entit� colpita � una di quelle che possiamo selezionare (cio�, se ha il nostro Selectable_TagComponent).
            if (state.EntityManager.HasComponent<Selectable_TagComponent>(entityHit))
            {
                // Se � selezionabile, le aggiungiamo il Selected_TagComponent.
                ecb.AddComponent<Selected_TagComponent>(entityHit);
                Debug.Log($"SelectionSystem: Selezionata entit� {entityHit}");
            }
            // Se il raycast colpisce qualcosa che NON ha Selectable_TagComponent non facciamo nulla
        }
        // Se il raycast NON colpisce nulla (physicsWorld.CastRay restituisce false),
        // significa che abbiamo cliccato nel "vuoto". Anche in questo caso il loop ha gi� cancellato qualsiasi selezione precedente.

        // Applica tutte le modifiche registrate nell'EntityCommandBuffer.
        ecb.Playback(state.EntityManager);
        ecb.Dispose(); // Libera le risorse dal CommandBuffer.
    }
}