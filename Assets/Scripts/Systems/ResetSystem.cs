using Unity.Entities;    // Namespace principale per ECS
using Unity.Collections; // Per NativeArray, Allocator, e FixedString
using UnityEngine;       // Usato per Debug.Log 
using Unity.Burst;       // Per BurstCompile

[BurstCompile] // compilazione Burst per performance migliorate
[UpdateInGroup(typeof(SimulationSystemGroup))] // Esegue questo sistema nel gruppo di aggiornamento per la logica di gioco
public partial struct ResetSystem : ISystem // I sistemi sono struct parziali che implementano ISystem
{
    // Valore a cui resettare le risorse del giocatore.
    private const int INITIAL_PLAYER_RESOURCES = 10;

    // OnCreate viene chiamato una volta quando il sistema viene creato.
    public void OnCreate(ref SystemState state)
    {
        // eseguire OnUpdate() SOLO se
        // esiste almeno un'entità con il componente Reset_Tag.
        // Appena l'entità con Reset_Tag viene distrutta, OnUpdate() smette di essere chiamato
        // finché non viene creata una nuova entità con Reset_Tag.
        state.RequireForUpdate<Reset_Tag>();
        Debug.Log("[ResetSystem] OnCreate: Sistema inizializzato e pronto a ricevere comandi di reset.");
    }

    // OnDestroy viene chiamato quando il sistema viene distrutto (es. chiusura del mondo ECS).
    public void OnDestroy(ref SystemState state)
    {
      
    }

    // OnUpdate viene chiamato ogni frame, MA SOLO SE la condizione in RequireForUpdate (in OnCreate) è soddisfatta.
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("[ResetSystem] OnUpdate: Rilevato Reset_Tag. Inizio procedura di reset dello stato ECS.");

        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        //  Resetta Stato Strutture Critiche, iteriando su tutte le entità 
        foreach (var (currentHealth, maxHealth) in
                 SystemAPI.Query<RefRW<CurrentHealthCIA_Component>, RefRO<MaxHealthCIA_Component>>())
        {
            // RefRW (ReadWrite) permette la modifica diretta del componente.
            // RefRO (ReadOnly) permette solo la lettura.
            currentHealth.ValueRW.Confidentiality = maxHealth.ValueRO.MaxConfidentiality;
            currentHealth.ValueRW.Integrity = maxHealth.ValueRO.MaxIntegrity;
            currentHealth.ValueRW.Availability = maxHealth.ValueRO.MaxAvailability;
        }
        Debug.Log("[ResetSystem] Stato salute CIA delle strutture resettato ai valori massimi.");

        // Resetta Risorse Giocatore,una sola entità con PlayerResources_Component (un singleton).
        if (SystemAPI.TryGetSingletonRW<PlayerResources_Component>(out RefRW<PlayerResources_Component> playerResources))
        {
            playerResources.ValueRW.CurrentAmount = INITIAL_PLAYER_RESOURCES;
            Debug.Log($"[ResetSystem] Risorse del giocatore resettate a: {INITIAL_PLAYER_RESOURCES}");
        }
        else
        {
            // Questo non dovrebbe accadere se l'AttackResultAuthoring ha creato l'entità.
            Debug.LogError("[ResetSystem] Impossibile trovare il singleton PlayerResources_Component per il reset!");
        }

        // Deseleziono Tutte le Entità
        // query per trovare tutte le entità che attualmente hanno il Selected_TagComponent
        EntityQuery selectedQuery = SystemAPI.QueryBuilder().WithAll<Selected_TagComponent>().Build();
        // Controlliamo se la query ha trovato qualcosa
        if (!selectedQuery.IsEmptyIgnoreFilter)
        {
            // Otteniamo un array di tutte le entità che soddisfano la query.
            using (NativeArray<Entity> entitiesToDeselect = selectedQuery.ToEntityArray(Allocator.TempJob))
            {
                foreach (Entity entity in entitiesToDeselect)
                {
                    // registrazione del comando per rimuovere il tag dall'entità.
                    ecb.RemoveComponent<Selected_TagComponent>(entity);
                }
            }
            Debug.Log("[ResetSystem] Selected_TagComponent rimosso da tutte le entità precedentemente selezionate.");
        }

        // Resetta LastAttackResult_Component Singleton, troviamo il singleton dell'esito dell'attacco e resettiamo i suoi campi.
        if (SystemAPI.TryGetSingletonRW<LastAttackResult_Component>(out RefRW<LastAttackResult_Component> lastAttackResult))
        {
            lastAttackResult.ValueRW.AttackerCardID = 0;
            lastAttackResult.ValueRW.AttackerCardName = new FixedString128Bytes("N/A"); // Usa il costruttore di FixedString
            lastAttackResult.ValueRW.TargetStructureName = new FixedString128Bytes("N/A");
            lastAttackResult.ValueRW.Outcome = AttackOutcome.None;
            lastAttackResult.ValueRW.DiscoveryStatus = AttackDiscoveryStatus.None;
            lastAttackResult.ValueRW.DamageDealtC = 0f;
            lastAttackResult.ValueRW.DamageDealtI = 0f;
            lastAttackResult.ValueRW.DamageDealtA = 0f;
            lastAttackResult.ValueRW.SuccessRollValue = 0;
            lastAttackResult.ValueRW.SuccessRollThreshold = 0;
            lastAttackResult.ValueRW.StealthRollValue = 0;
            lastAttackResult.ValueRW.StealthRollThreshold = 0;
            lastAttackResult.ValueRW.WasProcessed = true; // IMPORTANTE: nessun "vecchio" risultato deve essere mostrato dalla UI
            Debug.Log("[ResetSystem] LastAttackResult_Component resettato ai valori di default.");
        }
        else
        {
            Debug.LogError("[ResetSystem] Impossibile trovare il singleton LastAttackResult_Component per il reset!");
        }

        // Eliminare l'Entità Comando Reset_Tag
        // Troviamo l'entità con il Reset_Tag tramite query
        EntityQuery commandQuery = SystemAPI.QueryBuilder().WithAll<Reset_Tag>().Build();
        using (NativeArray<Entity> commandEntities = commandQuery.ToEntityArray(Allocator.TempJob))
        {
            foreach (Entity commandEntity in commandEntities)
            {
                // registrazione del comando per distruggere l'entità segnale, FONDAMENTALE per evitare che il ResetSystem esegua il reset ad ogni frame.
                ecb.DestroyEntity(commandEntity);
                Debug.Log($"[ResetSystem] Entità comando di reset {commandEntity} marcata per la distruzione.");
            }
        }

        // Applica tutte le operazioni registrate nell'EntityCommandBuffer.
        // cioè la rimozione dei Selected_TagComponent e la distruzione delle entità Reset_Tag.
        ecb.Playback(state.EntityManager);
        ecb.Dispose(); // Libera sempre la memoria dell'ECB.

        Debug.Log("[ResetSystem] Procedura di reset dello stato ECS completata. Il sistema attenderà un nuovo Reset_Tag.");
    }
}