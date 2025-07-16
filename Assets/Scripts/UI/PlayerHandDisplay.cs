using UnityEngine;
using UnityEngine.UI;    // Per Button
using TMPro;             // Per TextMeshProUGUI
using System.Collections.Generic;
using Unity.Entities;    // Per EntityManager, Entity, ecc.
using Unity.Collections; // Per Allocator


[System.Serializable]
public class CardUISingleSlot
{
    public GameObject cardRootObject;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI resourceCostText;

    public TextMeshProUGUI successText;     // Es. per "Successo: 7 (D10)"
    public TextMeshProUGUI stealthText;     // Es. per "Stealth: 5 (D10)"
    public TextMeshProUGUI cDamageText;
    public TextMeshProUGUI iDamageText;
    public TextMeshProUGUI aDamageText;


    public Image successIconImage;  
    public Image stealthIconImage; 

    public Button cardButton;

    [HideInInspector] public AttackCardData boundCardData;
    [HideInInspector] public bool isPopulated = false;
    [HideInInspector] public bool wasPlayed = false; // Flag per sapere se la carta è stata usata
                                                  
}

public class PlayerHandDisplay : MonoBehaviour
{
    [Header("Card UI Slots Configuration")]
    public List<CardUISingleSlot> cardUISlots = new List<CardUISingleSlot>();

    [Header("Dependencies")]
    public CardDatabase cardDatabase;
    public UIManager uiManager;

    [Header("Confirmation Popup UI")]
    public GameObject confirmAttackPopupPanel;
    public TextMeshProUGUI confirmationMessageText;
    public Button confirmAttackButton;
    public Button cancelAttackButton;

    private EntityManager entityManager;
    private EntityQuery playerEntityQuery;
    private int currentAvailableCardIndex = 0;
    private bool attackChainBroken = false; // Per gestire l'interruzione della catena

    // Campi per l'attacco in attesa di conferma
    private bool isAwaitingAttackConfirmation = false;
    private Entity pendingTargetStructure = Entity.Null;
    private AttackCardData pendingAttackCardData;
    private int pendingCardSlotIndex = -1; // Memorizza l'indice dello slot cliccato

    void Start()
    {
        //  Controlla se le dipendenze sono assegnate correttamente
        if (cardDatabase == null) { Debug.LogError("PlayerHandDisplay: CardDatabase non assegnato!"); SetAllCardSlotsActive(false); return; }
        if (uiManager == null) { Debug.LogError("PlayerHandDisplay: UIManager non assegnato!"); SetAllCardSlotsActive(false); return; }
        if (confirmAttackPopupPanel == null) { Debug.LogError("PlayerHandDisplay: ConfirmAttack_PopupPanel non assegnato!"); }
        if (confirmationMessageText == null) { Debug.LogError("PlayerHandDisplay: ConfirmationMessage_Text non assegnato!"); }
        if (confirmAttackButton == null) { Debug.LogError("PlayerHandDisplay: ConfirmAttackButton non assegnato!"); }
        if (cancelAttackButton == null) { Debug.LogError("PlayerHandDisplay: CancelAttackButton non assegnato!"); }


        World defaultWorld = World.DefaultGameObjectInjectionWorld;
        if (defaultWorld != null && defaultWorld.IsCreated)
        {
            entityManager = defaultWorld.EntityManager;
            playerEntityQuery = entityManager.CreateEntityQuery(typeof(PlayerResources_Component));
        }
        else
        {
            Debug.LogError("PlayerHandDisplay: DefaultWorld o EntityManager non disponibili!");
            SetAllCardSlotsActive(false);
            return;
        }

        // Nascondi il popup all'inizio e imposta i listener dei suoi bottoni
        if (confirmAttackPopupPanel != null)
        {
            confirmAttackPopupPanel.SetActive(false);
            if (confirmAttackButton != null) confirmAttackButton.onClick.AddListener(OnConfirmAttackProceed);
            if (cancelAttackButton != null) cancelAttackButton.onClick.AddListener(OnConfirmAttackCancel);
        }

        PopulateHandUI();
        // SetupCardButtonListeners(); Chiamati entrambi da PopulatedHAndUI
        // UpdateCardInteractability();
    }

    private void SetAllCardSlotsActive(bool isActive)
    {
        foreach (var slot in cardUISlots)
        {
            if (slot.cardRootObject != null) slot.cardRootObject.SetActive(isActive);
            if (!isActive) slot.isPopulated = false;
        }
    }

    public void PopulateHandUI() 
    {
        if (cardDatabase.availableAttackCards.Count == 0) { Debug.LogWarning("PlayerHandDisplay: CardDatabase non ha carte disponibili."); SetAllCardSlotsActive(false); return; }

        currentAvailableCardIndex = 0; // Resetta l'indice della carta giocabile
        attackChainBroken = false;     // Resetta lo stato della catena
        pendingCardSlotIndex = -1;     // Resetta l'indice della carta in attesa

        // METTO I DATI NELLE SLOT UI E QUINDI NELLA UI
        int cardsToDisplay = Mathf.Min(cardUISlots.Count, cardDatabase.availableAttackCards.Count);
        for (int i = 0; i < cardsToDisplay; i++)
        {
            CardUISingleSlot currentSlotUI = cardUISlots[i];
            AttackCardData dataForThisCard = cardDatabase.availableAttackCards[i];
            if (currentSlotUI.cardRootObject == null) { Debug.LogWarning($"PlayerHandDisplay: cardRootObject non assegnato per slot UI index {i}"); continue; }
            currentSlotUI.cardRootObject.SetActive(true);
            currentSlotUI.boundCardData = dataForThisCard;
            currentSlotUI.isPopulated = true;
            currentSlotUI.wasPlayed = false; // Ogni carta inizia come non giocata

            if (currentSlotUI.nameText != null) currentSlotUI.nameText.text = dataForThisCard.Name.ToString(); 
            if (currentSlotUI.resourceCostText != null) currentSlotUI.resourceCostText.text = $"Costo: {dataForThisCard.ResourceCost}";
            if (currentSlotUI.successText != null) currentSlotUI.successText.text = $"Successo: {dataForThisCard.SuccessStat} (D10)";
            if (currentSlotUI.stealthText != null) currentSlotUI.stealthText.text = $"Stealth: {dataForThisCard.StealthStat} (D10)";
            if (currentSlotUI.cDamageText != null) currentSlotUI.cDamageText.text = $"D C: {dataForThisCard.DamageConfidentiality:F0}";
            if (currentSlotUI.iDamageText != null) currentSlotUI.iDamageText.text = $"D I: {dataForThisCard.DamageIntegrity:F0}";
            if (currentSlotUI.aDamageText != null) currentSlotUI.aDamageText.text = $"D A: {dataForThisCard.DamageAvailability:F0}";

            // le icone di feedback devono essere nascoste all'inizio
            if (currentSlotUI.successIconImage != null) currentSlotUI.successIconImage.gameObject.SetActive(false);
            if (currentSlotUI.stealthIconImage != null) currentSlotUI.stealthIconImage.gameObject.SetActive(false);



        }

        for (int i = cardsToDisplay; i < cardUISlots.Count; i++) { if (cardUISlots[i].cardRootObject != null) cardUISlots[i].cardRootObject.SetActive(false); cardUISlots[i].isPopulated = false; }
        Debug.Log("PlayerHandDisplay: Interfaccia Carte Popolata.");
        SetupCardButtonListeners();    // Imposta i listener DOPO aver popolato
        UpdateCardInteractability(); // Imposta l'interattività iniziale
    }

    void SetupCardButtonListeners() 
    {
        for (int i = 0; i < cardUISlots.Count; i++)
        {
            if (cardUISlots[i].isPopulated && cardUISlots[i].cardButton != null)
            {
                int cardSlotIndex = i; // Cattura l'indice
                cardUISlots[i].cardButton.onClick.RemoveAllListeners();
                cardUISlots[i].cardButton.onClick.AddListener(() => OnCardClicked(cardSlotIndex));
            }
        }
    }

    void UpdateCardInteractability() 
    {
        for (int i = 0; i < cardUISlots.Count; i++)
        {
            if (cardUISlots[i].isPopulated && cardUISlots[i].cardButton != null)
            {
                // Condizioni di Interagibilitò
                bool canPlay = !cardUISlots[i].wasPlayed &&
                               i == currentAvailableCardIndex &&
                               !attackChainBroken;
                cardUISlots[i].cardButton.interactable = canPlay;
            }
            else if (cardUISlots[i].cardButton != null)
            {
                cardUISlots[i].cardButton.interactable = false;
            }
        }

        Debug.Log($"[PlayerHandDisplay] Interattività carte aggiornata. CurrentAvailableCardIndex: {currentAvailableCardIndex}, AttackChainBroken: {attackChainBroken}");


    }

    public void OnCardClicked(int cardSlotIndex)
    {
        // Con controllo sullo stato della carta e della catena
        if (!cardUISlots[cardSlotIndex].isPopulated || cardSlotIndex != currentAvailableCardIndex || attackChainBroken)
        {
            Debug.LogWarning("PlayerHandDisplay: Tentativo di giocare una carta non valida o non disponibile in sequenza.");
            return;
        }

        AttackCardData selectedCardData = cardUISlots[cardSlotIndex].boundCardData;
        Entity targetStructure = uiManager.CurrentSelectedStructure; // Usa la proprietà di UIManager per ottenere la struttura selezionata

        if (targetStructure == Entity.Null)
        {
            Debug.LogWarning("PlayerHandDisplay: Nessuna struttura selezionata per l'attacco.");
            // TODO: Mostra messaggio UI al giocatore (es. un piccolo testo che appare e scompare)
            return;
        }

        // Controlla risorse PRIMA di mostrare il popup
        if (!CanAffordCard(selectedCardData))
        {
            Debug.LogWarning($"PlayerHandDisplay: Risorse insufficienti per '{selectedCardData.Name}'.");
            // TODO: Mostra messaggio UI al giocatore
            return;
        }

        // Memorizza i dati per la conferma e mostra il popup
        pendingTargetStructure = targetStructure;
        pendingAttackCardData = selectedCardData;
        pendingCardSlotIndex = cardSlotIndex; // Memorizza l'indice per la logica di avanzamento

        string targetName = "Bersaglio";
        if (entityManager.HasComponent<StructureID_Component>(targetStructure))
        {
            targetName = entityManager.GetComponentData<StructureID_Component>(targetStructure).Name.ToString();
        }

        if (confirmationMessageText != null)
            confirmationMessageText.text = $"Attaccare '{targetName}' con '{selectedCardData.Name}' (Costo: {selectedCardData.ResourceCost})?";

        if (confirmAttackPopupPanel != null)
            confirmAttackPopupPanel.SetActive(true);

        isAwaitingAttackConfirmation = true;
    }

    private bool CanAffordCard(AttackCardData card)
    {
        if (playerEntityQuery.IsEmptyIgnoreFilter)
        {
            Debug.LogError("PlayerHandDisplay: Entità giocatore non trovata per controllo risorse.");
            return false;
        }
        NativeArray<Entity> playerEntities = playerEntityQuery.ToEntityArray(Allocator.Temp);
        Entity playerEntity = playerEntities[0];
        playerEntities.Dispose();
        if (!entityManager.HasComponent<PlayerResources_Component>(playerEntity)) return false;
        PlayerResources_Component playerResources = entityManager.GetComponentData<PlayerResources_Component>(playerEntity);
        return playerResources.CurrentAmount >= card.ResourceCost;
    }

    void OnConfirmAttackProceed()
    {
        if (!isAwaitingAttackConfirmation) return;
        Debug.Log("Attacco Confermato!");

        // Paga il costo delle risorse
        if (!playerEntityQuery.IsEmptyIgnoreFilter)
        {
            NativeArray<Entity> playerEntities = playerEntityQuery.ToEntityArray(Allocator.Temp);
            Entity playerEntity = playerEntities[0];
            playerEntities.Dispose();

            PlayerResources_Component playerResources = entityManager.GetComponentData<PlayerResources_Component>(playerEntity);
            playerResources.CurrentAmount -= pendingAttackCardData.ResourceCost;

            EntityCommandBuffer ecbPay = new EntityCommandBuffer(Allocator.Temp);
            ecbPay.SetComponent(playerEntity, playerResources);
            ecbPay.Playback(entityManager);
            ecbPay.Dispose();

            uiManager.UpdatePlayerResourcesUI(playerResources.CurrentAmount);
            Debug.Log($"Pagato costo: {pendingAttackCardData.ResourceCost}. Risorse rimanenti: {playerResources.CurrentAmount}");

            CreateAttackRequest(pendingTargetStructure, pendingAttackCardData);

            // Nascondi il popup e resetta lo stato di attesa della conferma
            if (confirmAttackPopupPanel != null) confirmAttackPopupPanel.SetActive(false);
            isAwaitingAttackConfirmation = false;

            // Segna la carta come giocata e disabilitala pendingCardSlotIndex = indice della carta appena confermata
            if (this.pendingCardSlotIndex >= 0 && this.pendingCardSlotIndex < cardUISlots.Count)
            {
                cardUISlots[this.pendingCardSlotIndex].wasPlayed = true;
                if (cardUISlots[this.pendingCardSlotIndex].cardButton != null)
                {
                    cardUISlots[this.pendingCardSlotIndex].cardButton.interactable = false;
                }
                Debug.Log($"[PlayerHandDisplay] Carta nello slot {this.pendingCardSlotIndex} marcata come 'wasPlayed' e bottone disabilitato in attesa di esito.");
            }
        }
        else
        {
        // Se il giocatore non viene trovato
        if (confirmAttackPopupPanel != null) confirmAttackPopupPanel.SetActive(false);
        isAwaitingAttackConfirmation = false;
            
        }
    }

    void OnConfirmAttackCancel()
    {
        if (!isAwaitingAttackConfirmation) return;
        Debug.Log("Attacco Annullato.");

        // Resetta solo lo stato di attesa, non l'intera mano.
        if (confirmAttackPopupPanel != null) confirmAttackPopupPanel.SetActive(false);
        isAwaitingAttackConfirmation = false;
        pendingTargetStructure = Entity.Null;
        pendingAttackCardData = default;
        pendingCardSlotIndex = -1;
    }

    void ResetPendingAttackState()
    {
        isAwaitingAttackConfirmation = false;
        pendingTargetStructure = Entity.Null;
        pendingAttackCardData = default;
        pendingCardSlotIndex = -1;
        UpdateCardInteractability(); // Assicura che lo stato dei bottoni sia corretto
    }

    private void CreateAttackRequest(Entity target, AttackCardData card)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        Entity requestEntity = ecb.CreateEntity();

        ecb.AddComponent(requestEntity, new ConfirmedAttackRequest_Component(target, card));
        // Il costruttore della nuova struct farà:
        // {
        //     TargetStructureEntity = target;
        //     CardUsed = card;
        // }

        Debug.Log($"PlayerHandDisplay: Creata entità Richiesta Attacco: {requestEntity} per carta '{card.Name}' su {target} (usando nuova struct)");
        ecb.Playback(entityManager);
        ecb.Dispose();
    }


    // Metodi per la gestione delle ICONE sulle carte
    public void UpdateFeedbackOnPlayedCard(int playedCardID, AttackOutcome outcome, AttackDiscoveryStatus discovery,
                                           Sprite successSprite, Sprite failureSprite,
                                           Sprite stealthySprite, Sprite discoveredSprite)
    {
        Debug.Log($"[PlayerHandDisplay] Ricevuto feedback per CardID: {playedCardID}, Esito: {outcome}, Scoperta: {discovery}, PendingSlot: {this.pendingCardSlotIndex}");

        if (this.pendingCardSlotIndex >= 0 && this.pendingCardSlotIndex < cardUISlots.Count &&
            cardUISlots[this.pendingCardSlotIndex].isPopulated &&
            cardUISlots[this.pendingCardSlotIndex].boundCardData.ID == playedCardID)
        {
            CardUISingleSlot slotToUpdate = cardUISlots[this.pendingCardSlotIndex];
            // wasPlayed dovrebbe essere già true da OnConfirmAttackProceed

            if (slotToUpdate.successIconImage != null)
            {
                slotToUpdate.successIconImage.sprite = (outcome == AttackOutcome.Success) ? successSprite : failureSprite;
                slotToUpdate.successIconImage.gameObject.SetActive(true);
            }

            if (slotToUpdate.stealthIconImage != null)
            {
                slotToUpdate.stealthIconImage.sprite = (discovery == AttackDiscoveryStatus.Stealthy) ? stealthySprite : discoveredSprite;
                slotToUpdate.stealthIconImage.gameObject.SetActive(true);
            }

            Debug.Log($"[PlayerHandDisplay] Icone UI aggiornate per carta ID {playedCardID} nello slot {this.pendingCardSlotIndex}.");

            // Logica per l'avanzamento/interruzione della catena di attacco
            if (this.pendingCardSlotIndex == currentAvailableCardIndex) // Solo se era la carta giocabile
            {
                if (outcome == AttackOutcome.Success)
                {
                    currentAvailableCardIndex++;
                    Debug.Log($"[PlayerHandDisplay] Attacco riuscito, catena prosegue. Nuovo currentAvailableCardIndex: {currentAvailableCardIndex}");
                }
                else // Attacco Fallito
                {
                    attackChainBroken = true;
                    Debug.Log($"[PlayerHandDisplay] Attacco fallito per carta ID {playedCardID}, CATENA INTERROTTA. currentAvailableCardIndex rimane: {currentAvailableCardIndex}");
                }
            }
            else
            {
                Debug.LogWarning($"[PlayerHandDisplay] Feedback ricevuto per slot {this.pendingCardSlotIndex} ma currentAvailableCardIndex era {currentAvailableCardIndex}. La logica di avanzamento potrebbe non essere corretta.");
            }
        }
        else
        {
            Debug.LogWarning($"[PlayerHandDisplay] UpdateFeedbackOnPlayedCard: Impossibile trovare lo slot corretto per CardID {playedCardID} usando pendingCardSlotIndex {this.pendingCardSlotIndex}. Icone non aggiornate.");
        }

        UpdateCardInteractability(); // Aggiorna sempre l'interattività dei bottoni dopo l'esito
    }

    // METODO CHIAMATO DA UIMANAGER PER RESETTARE LO STATO POST-ATTACCO ---
    public void FinalizeAttackSequence()
    {
        // Questo metodo viene chiamato da UIManager DOPO che tutti i feedback UI sono stati processati incluso l'aggiornamento delle icone sulla carta.
        ResetPendingAttackState();
    }



}