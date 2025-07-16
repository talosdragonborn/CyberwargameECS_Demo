# CyberwargameECS_Demo

Benvenuto nel repository ufficiale di **CyberwargameECS_Demo**!  
Questo progetto è un esempio/demo sviluppato con Unity che utilizza l’Entity Component System (ECS) per mostrare funzionalità di gameplay legate a un cyber wargame.

## Sommario

- [Descrizione](#descrizione)
- [Requisiti](#requisiti)
- [Installazione](#installazione)
- [Avvio del Progetto](#avvio-del-progetto)
- [Struttura delle Cartelle](#struttura-delle-cartelle)
- [FAQ e Note utili](#faq-e-note-utili)
- [Contribuire](#contribuire)
- [Licenza](#licenza)

---

## Descrizione

**CyberwargameECS_Demo** è un progetto Unity progettato per mostrare meccaniche di gioco e simulazioni basate su architettura ECS.  
L’obiettivo è fornire un esempio pratico di gestione di entità, componenti e sistemi in un contesto di simulazione di cyber war.

## Requisiti

- **Unity** (versione consigliata: vedere file `ProjectVersion.txt` nella cartella `ProjectSettings/`)
- Sistema operativo: Windows, Mac o Linux (compatibile con Unity)
- Almeno 4 GB di RAM
- Git (opzionale, raccomandato per gestire il repository)

## Installazione

1. **Clona il repository:**
   ```sh
   git clone https://github.com/talosdragonborn/CyberwargameECS_Demo.git
   ```
   Oppure scarica il progetto come zip ed estrailo.

2. **Apri il progetto in Unity:**
   - Avvia Unity Hub.
   - Clicca su “Open” e seleziona la cartella dove hai clonato o estratto il progetto.
   - Seleziona la versione di Unity richiesta.

## Avvio del Progetto

1. All’avvio, Unity potrebbe mostrare una scena vuota chiamata **"Untitled"**.  
   Questo è normale: il progetto non apre automaticamente le scene.

2. **Per aprire la scena principale:**
   - Vai nel pannello `Project` (in basso).
   - Naviga in `Assets/Scenes/`.
   - Fai doppio clic su `ScenaPrincipale.unity` (o sulla scena che vuoi esplorare).

3. **Se vuoi includere altre scene nella build:**
   - Vai su `File > Build Settings`.
   - Usa “Add Open Scenes” per aggiungere la scena corrente.

## Struttura delle Cartelle

```text
CyberwargameECS_Demo/
│
├── Assets/                # Asset del progetto (scene, script, prefab, materiali, ecc.)
│   └── Scenes/            # Scene di gioco (es: ScenaPrincipale.unity)
├── Packages/              # Dipendenze del progetto gestite da Unity Package Manager
├── ProjectSettings/       # Configurazioni del progetto Unity
├── .gitignore             # File per ignorare asset auto-generati o inutili su Git
└── README.md              # Questo file
```

> ⚠️ **Nota:**  
Non sono inclusi nel repository:
- `Library/`, `Temp/`, `Obj/`, `Builds/`, e altre cartelle generate automaticamente da Unity
- Cartella `Build/` (le build vanno generate localmente)


## Licenza

Questo progetto è distribuito sotto licenza MIT.  
Vedi il file `LICENSE` per maggiori dettagli.

---

**Repository:** [github.com/talosdragonborn/CyberwargameECS_Demo](https://github.com/talosdragonborn/CyberwargameECS_Demo)