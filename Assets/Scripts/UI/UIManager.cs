using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Structure Info Panel Elements")]
    public GameObject structureInfoPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI confidentialityText;
    public TextMeshProUGUI integrityText;
    public TextMeshProUGUI availabilityText;
    public Button closePanelButton;

    [Header("Player Resource Info")] 
    public TextMeshProUGUI playerResourcesText;

    // --- INIZIO NUOVI CAMPI PER RESULT_PANEL ---
    [Header("Attack Result Panel (Result_Panel)")]
    public GameObject resultPanel; 
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultAttackText;
    public TextMeshProUGUI resultSuccessText;
    public TextMeshProUGUI resultStealthText;
    public TextMeshProUGUI resultDamageText;
    public Button closeResultPanelButton;

    // TASTO RESTART
    [Header("Restart Button")]
    public Button resetGameButton; 

    [Header("Dependencies")]
    public PlayerHandDisplay playerHandDisplay; // Riferimento a PlayerHandDisplay

    [Header("Feedback Icon Sprites (Assegna in Inspector)")]
    public Sprite iconSuccess;
    public Sprite iconFailure;
    public Sprite iconStealthy;
    public Sprite iconDiscovered;




    private EntityManager entityManager;
    private EntityQuery selectedEntityQuery;
    private EntityQuery playerEntityQuery;
    private EntityQuery attackResultQuery;

    // Proprietà pubblica per esporre l'entità selezionata 
    public Entity CurrentSelectedStructure { get; private set; } = Entity.Null;


    void Start()
    {
        World defaultWorld = World.DefaultGameObjectInjectionWorld;
        if (defaultWorld == null)
        {
            Debug.LogError("UIManager: DefaultGameObjectInjectionWorld non trovato!");
            this.enabled = false;
            return;
        }
        entityManager = defaultWorld.EntityManager;

        selectedEntityQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Selected_TagComponent>());
        playerEntityQuery = entityManager.CreateEntityQuery(typeof(PlayerResources_Component));
        attackResultQuery = entityManager.CreateEntityQuery(typeof(LastAttackResult_Component));

        if (closePanelButton != null) closePanelButton.onClick.AddListener(OnClosePanelButtonClicked);
        else Debug.LogWarning("UIManager: Close_Button non assegnato nell'Inspector.");

        if (structureInfoPanel != null) structureInfoPanel.SetActive(false);
        else { Debug.LogError("UIManager: StructureInfo_Panel non assegnato nell'Inspector!"); this.enabled = false; }

        if (resultPanel != null)
        {
            if (closeResultPanelButton != null) closeResultPanelButton.onClick.AddListener(OnCloseResultPanelClicked);
            else Debug.LogWarning("UIManager: closeResultPanelButton (per Result_Panel) non assegnato.");
            resultPanel.SetActive(false); // Nascondi all'inizio
        }
        else Debug.LogWarning("UIManager: Result_Panel non assegnato. L'esito dell'attacco non verrà mostrato.");

        // Listener per il ttone di reset del gioco
        if (resetGameButton != null)
        {
            resetGameButton.onClick.AddListener(OnResetGameClicked); // Collega al nuovo metodo
        }
        else
        {
            Debug.LogWarning("UIManager: resetGameButton non assegnato nell'Inspector!");
        }

        // Inizializza la UI delle risorse del giocatore
        // InitializePlayerResourcesUI(); TOLTO per problemi di timing con ECS
        Debug.Log("UIManager Inizializzato.");
    }



    void Update()
    {
        if (entityManager == null ) return; 

        // Gestione Pannello Info Struttura
        if (selectedEntityQuery.IsEmptyIgnoreFilter)
        {
            CurrentSelectedStructure = Entity.Null; 
            if (structureInfoPanel != null && structureInfoPanel.activeSelf)
            {
                structureInfoPanel.SetActive(false);
            }
        }
        else
        {
            NativeArray<Entity> selectedEntities = selectedEntityQuery.ToEntityArray(Allocator.Temp);
            if (selectedEntities.Length > 0)
            {
                CurrentSelectedStructure = selectedEntities[0]; // IMPOSTA SELEZIONE CORRENTE
                UpdatePanelInfo(CurrentSelectedStructure);

                if (structureInfoPanel != null && !structureInfoPanel.activeSelf)
                {
                    structureInfoPanel.SetActive(true);
                }
            }
            else
            {
                CurrentSelectedStructure = Entity.Null; // NESSUNA SELEZIONE 
                if (structureInfoPanel != null && structureInfoPanel.activeSelf)
                {
                    structureInfoPanel.SetActive(false);
                }
            }
            selectedEntities.Dispose();
        }


        HandleAttackResultDisplay();
        // Aggiorna la UI delle risorse del giocatore 
        UpdatePlayerResourcesUIDynamically(); 
    }

    // Metodo per aggiornare le informazioni del pannello struttura
    void UpdatePanelInfo(Entity entity)
    {
  
        if (structureInfoPanel == null || !entityManager.Exists(entity)) return;
        string displayName = "N/A";
        if (entityManager.HasComponent<StructureID_Component>(entity))
        {
            StructureID_Component idComp = entityManager.GetComponentData<StructureID_Component>(entity);
            displayName = idComp.Name.ToString();
        }
        string confText = "Conf: -/-";
        string intText = "Int: -/-";
        string availText = "Disp: -/-";
        if (entityManager.HasComponent<CurrentHealthCIA_Component>(entity) &&
            entityManager.HasComponent<MaxHealthCIA_Component>(entity))
        {
            CurrentHealthCIA_Component currentCIA = entityManager.GetComponentData<CurrentHealthCIA_Component>(entity);
            MaxHealthCIA_Component maxCIA = entityManager.GetComponentData<MaxHealthCIA_Component>(entity);
            confText = $"Confidenzialità: {currentCIA.Confidentiality:F0} / {maxCIA.MaxConfidentiality:F0}";
            intText = $"Integrità: {currentCIA.Integrity:F0} / {maxCIA.MaxIntegrity:F0}";
            availText = $"Disponibilità: {currentCIA.Availability:F0} / {maxCIA.MaxAvailability:F0}";
        }
        if (nameText != null) nameText.text = $"Nome: {displayName}";
        if (confidentialityText != null) confidentialityText.text = confText;
        if (integrityText != null) integrityText.text = intText;
        if (availabilityText != null) availabilityText.text = availText;
        if (titleText != null && string.IsNullOrEmpty(titleText.text)) titleText.text = "Dettagli Struttura";
    }

    // Metodo chiamato quando il pulsante Chiudi viene cliccato
    void OnClosePanelButtonClicked()
    {
        Debug.Log("Close Panel Button Clicked!");
        if (entityManager == null || CurrentSelectedStructure == Entity.Null) // Usa la proprietà
        {
            if (structureInfoPanel != null) structureInfoPanel.SetActive(false);
            return;
        }

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        if (entityManager.HasComponent<Selected_TagComponent>(CurrentSelectedStructure))
        {
            ecb.RemoveComponent<Selected_TagComponent>(CurrentSelectedStructure);
            Debug.Log($"UIManager: Richiesta rimozione Selected_Tag da {CurrentSelectedStructure} tramite pulsante Chiudi.");
        }
        ecb.Playback(entityManager);
        ecb.Dispose();

        // CurrentSelectedStructure verrà impostato a Entity.Null nel prossimo Update
        if (structureInfoPanel != null)
        {
            structureInfoPanel.SetActive(false); // Nascondi
        }
    }

    // METODO ELIMINATO A CAUSA DI PROBLEMI DI TIMING, perchè start viene invocato prima che il sistema ECS abbia completato la creazione delle entità

   /* private void InitializePlayerResourcesUI()
    {
        if (playerResourcesText == null)
        {
            Debug.LogWarning("UIManager: playerResourcesText non assegnato nell'Inspector. Impossibile visualizzare le risorse.");
            return;
        }

        if (playerEntityQuery.IsEmptyIgnoreFilter)
        {
            playerResourcesText.text = "Risorse: N/D"; // Nessuna entità giocatore trovata
            Debug.LogWarning("UIManager: Nessuna entità giocatore con PlayerResources_Component trovata all'avvio.");
        }
        else
        {
            NativeArray<Entity> playerEntities = playerEntityQuery.ToEntityArray(Allocator.Temp);
            if (playerEntities.Length > 0)
            {
                PlayerResources_Component res = entityManager.GetComponentData<PlayerResources_Component>(playerEntities[0]);
                UpdatePlayerResourcesUI(res.CurrentAmount);
            }
            playerEntities.Dispose();
        }
    }*/

    // Questo metodo viene chiamato da PlayerHandDisplay dopo che le risorse sono state modificate
    public void UpdatePlayerResourcesUI(int currentAmount)
    {
        if (playerResourcesText != null)
        {
            playerResourcesText.text = $"Risorse: {currentAmount}";
        }
    }

    // Metodo per aggiornare la UI risorse
    private void UpdatePlayerResourcesUIDynamically()
    {
        if (playerResourcesText == null || playerEntityQuery.IsEmptyIgnoreFilter) return;

        NativeArray<Entity> playerEntities = playerEntityQuery.ToEntityArray(Allocator.Temp);
        if (playerEntities.Length > 0)
        {
            PlayerResources_Component res = entityManager.GetComponentData<PlayerResources_Component>(playerEntities[0]);
            // Aggiorna il testo solo se il valore è cambiato, per evitare lavoro inutile
            string newText = $"Risorse: {res.CurrentAmount}";
            if (playerResourcesText.text != newText)
            {
                playerResourcesText.text = newText;
            }
        }
        playerEntities.Dispose();
    }

    // Metodo pubblico usato da PlayerHandDisplay per sapere cosa è selezionato
    public Entity GetSelectedStructureEntity()
    {
        // CurrentSelectedStructure viene già aggiornato nell'Update() di UIManager
        return CurrentSelectedStructure;
    }


    // METODI PANNELO RISULTATO ATTACCO

    void OnCloseResultPanelClicked()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
            Debug.Log("Result_Panel chiuso dall'utente.");
        }
    }

    // --- INIZIO NUOVI METODI PER RESULT_PANEL ---
    void HandleAttackResultDisplay()
    {
        if (attackResultQuery == null || attackResultQuery.IsEmptyIgnoreFilter) return;

        NativeArray<Entity> resultEntities = attackResultQuery.ToEntityArray(Allocator.Temp);
        if (resultEntities.Length == 0) { resultEntities.Dispose(); return; }
        Entity resultSingletonEntity = resultEntities[0];
        resultEntities.Dispose();

        if (!entityManager.HasComponent<LastAttackResult_Component>(resultSingletonEntity))
        {
            // Questo è un errore se ci aspettiamo che l'entità esista sempre
            // Debug.LogError("UIManager: L'entità singleton dell'esito non ha LastAttackResult_Component!");
            return;
        }
        LastAttackResult_Component resultData = entityManager.GetComponentData<LastAttackResult_Component>(resultSingletonEntity);

        if (!resultData.WasProcessed)
        {
            Debug.Log($"[UIManager] Nuovo esito attacco da visualizzare. Carta ID: {resultData.AttackerCardID}");

            if (resultPanel != null)
            {
                if (resultTitleText != null) resultTitleText.text = "Esito Attacco";
                if (resultAttackText != null) resultAttackText.text = $"Attacco con {resultData.AttackerCardName.ToString()} sull'infrastruttura {resultData.TargetStructureName.ToString()}";


                // Messaggio di Success e Stealth
                if (resultSuccessText != null)
                {
                    string successMessage = "";
                    if (resultData.Outcome == AttackOutcome.Success)
                    {
                        successMessage = "Attacco Riuscito!";
                    }
                    else 
                    {
                        successMessage = "Attacco Fallito! Catena Interrotta";
                    }
                    // Aggiungiamo i dettagli del dado
                    successMessage += $"\n(Success Roll: {resultData.SuccessRollValue} vs Soglia Richiesta: {resultData.SuccessRollThreshold})";
                    resultSuccessText.text = successMessage;
                }

                if (resultStealthText != null)
                {
                    string stealthMessage = "";
                    if (resultData.DiscoveryStatus == AttackDiscoveryStatus.Stealthy)
                    {
                        stealthMessage = "Attacco Stealth Riuscito!";
                    }
                    else // AttackDiscoveryStatus.Discovered (o None)
                    {
                        stealthMessage = "Attacco Individuato!";
                    }
                    // Aggiungiamo i dettagli del dado
                    stealthMessage += $"\n(Stealth Roll: {resultData.StealthRollValue} vs Soglia Richiesta: {resultData.StealthRollThreshold})";
                    resultStealthText.text = stealthMessage;
                }


                if (resultData.Outcome == AttackOutcome.Success)
                {
                    if (resultDamageText != null) resultDamageText.text = $"Danno Inflitto: C:{resultData.DamageDealtC:F0}, I:{resultData.DamageDealtI:F0}, A:{resultData.DamageDealtA:F0}";
                }
                else
                {
                    if (resultDamageText != null) resultDamageText.text = "Nessun danno inflitto.";
                }


                resultPanel.SetActive(true);
                Debug.Log("[UIManager] Result_Panel attivato con i dati dell'esito.");
            }

            if (playerHandDisplay != null)
            {
                // che gli sprite siano stati assegnati nell'inspector!
                if (iconSuccess != null && iconFailure != null && iconStealthy != null && iconDiscovered != null)
                {
                    // Chiama il metodo sull'ISTANZA 'playerHandDisplay' 
                    playerHandDisplay.UpdateFeedbackOnPlayedCard(
                        resultData.AttackerCardID,
                        resultData.Outcome,
                        resultData.DiscoveryStatus,
                        iconSuccess,
                        iconFailure,
                        iconStealthy,
                        iconDiscovered
                    );
                }
                else
                {
                    Debug.LogError("UIManager: Sprite delle icone non assegnati nell'Inspector!");
                }
            }
            else
            {
                Debug.LogError("UIManager: Riferimento a PlayerHandDisplay non assegnato nell'Inspector!");
            }


            // Marca come processato
            resultData.WasProcessed = true;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            ecb.SetComponent(resultSingletonEntity, resultData);
            ecb.Playback(entityManager);
            ecb.Dispose();
            Debug.Log("[UIManager] Risultato attacco marcato come processato.");

            if (playerHandDisplay != null)
            {
                // Chiama il metodo sull'istanza 'playerHandDisplay'
                playerHandDisplay.FinalizeAttackSequence();
            }
        }
    }

    // METODO CHIAMATO DAL PULSANTE DI RESET DEL GIOCO

    void OnResetGameClicked()
    {
        Debug.Log("[UIManager] Pulsante Riavvia Partita cliccato! Inizio procedura di reset...");

        // Resetta la mano delle carte e lo stato del PlayerHandDisplay
        // PlayerHandDisplay.PopulateHandUI() resetta l'indice della carta,
        // la catena di attacco, e lo stato 'wasPlayed' delle carte.
        if (playerHandDisplay != null)
        {
            playerHandDisplay.PopulateHandUI(); // Questo resetta la mano alla sua configurazione iniziale
            Debug.Log("[UIManager] PlayerHandDisplay.PopulateHandUI() chiamato per il reset.");
        }
        else
        {
            Debug.LogWarning("[UIManager] Riferimento a playerHandDisplay è nullo. Impossibile resettare la mano.");
        }

        // Nascondi tutti i pannelli principali 
        if (structureInfoPanel != null && structureInfoPanel.activeSelf)
        {
            structureInfoPanel.SetActive(false);
            Debug.Log("[UIManager] StructureInfo_Panel nascosto.");
        }
        if (resultPanel != null && resultPanel.activeSelf)
        {
            resultPanel.SetActive(false);
            Debug.Log("[UIManager] Result_Panel nascosto.");
        }

        //Deseleziona qualsiasi entità 
        CurrentSelectedStructure = Entity.Null; // Assicura che la nostra cache sia pulita

        // CREARE L'ENTITÀ COMANDO PER IL RESET ECS
        if (entityManager != null ) // Assicurati che l'entityManager sia valido
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            Entity commandEntity = ecb.CreateEntity();
            ecb.AddComponent<Reset_Tag>(commandEntity); 
            ecb.Playback(entityManager);
            ecb.Dispose();
            Debug.Log($"[UIManager] Creata entità comando di reset: {commandEntity} con Reset_Tag.");
        }
        else
        {
            Debug.LogError("[UIManager] EntityManager non valido, impossibile creare il comando di reset ECS!");
        }
        // ------------------------------------------------------------

        Debug.Log("[UIManager] Reset parziale della UI completato. In attesa del reset dello stato ECS.");
    }

}