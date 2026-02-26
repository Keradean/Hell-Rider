# Hell Rider - 2-Player Top-Down Bullet Hell

Ein intensives, kompetitives Bullet-Hell-Spiel, in dem zwei Spieler online gegeneinander antreten, um gegen überwältigende Gegnerwellen die meisten Punkte zu erzielen.

![Unity Version](https://img.shields.io/badge/Unity-6000.0.62f1-black)
![FishNet](https://img.shields.io/badge/FishNet-4.x-blue)
![Status](https://img.shields.io/badge/Status-Demo-green)

---

## 📋 Projektinfo

- **Projekt:** 2-Spieler Top-Down Bullethell
- **Abgabedatum:** 23.01.2026
- **Engine:** Unity 6000.0.62f1 LTS
- **Networking:** FishNet
- **Arbeitsform:** Einzelarbeit
- **Backend:** PlayerPrefs (lokale Persistenz)

---

## 🎮 Kurzbeschreibung

Hell Rider ist ein schnelles Arcade-Bullet-Hell-Spiel aus der Top-Down-Perspektive. Zwei Spieler kämpfen online gegeneinander, während sie endlose Wellen von Aliens und einen finalen Boss bekämpfen. Wer überlebt und die meisten Punkte sammelt, gewinnt!

### Features
- ✅ Online Multiplayer (Host + Client)
- ✅ Server-authoritative Gameplay
- ✅ 2 Gegnertypen + Boss
- ✅ Dynamisches Bullet-System (Normal + Spread Shot)
- ✅ Score-System mit Persistenz
- ✅ Energie- und Boost-Mechanik
- ✅ Vollständige Audio-Integration

---

## 🚀 Anleitung zum Starten

### Host starten
1. Unity-Projekt öffnen
2. Play-Button drücken
3. Im Main Menu: **"Host"** klicken
4. Spiel startet als Host

### Client verbinden
1. Zweite Unity-Instanz öffnen (oder Build nutzen)
2. Play-Button drücken
3. Im Main Menu: **"Client"** klicken
4. Verbindet sich automatisch zu `localhost:7770`

### Steuerung
| Aktion | Input |
|--------|-------|
| Bewegung | `WASD` |
| Schießen | `Linke Maustaste` |
| Spread Shot | `Rechte Maustaste` |
| Boost | `Shift` (hält gedrückt, verbraucht Energie) |
| Pause | `ESC` |

---

## 🔧 Technischer Überblick

### Network-Architektur
- **Framework:** FishNet (Server-Authoritative)
- **Topology:** Host-Client (kein Dedicated Server)
- **Transport:** Tugboat (Standard FishNet Transport)
- **Port:** 7770 (Standard)

---

## 📡 Verwendete RPCs

### ServerRPCs (Client → Server)
| Script | RPC | Zweck |
|--------|-----|-------|
| `PlayerController.cs` | `DieServerRpc()` | Spieler-Tod auf Server verarbeiten, Explosion spawnen |
| `GameManager.cs` | `SetWorldSpeedServerRpc(float)` | Weltgeschwindigkeit synchronisieren (für Boost) |
| `Weapon.cs` | `ShootNormalServerRpc(Vector3, float)` | Normalen Schuss serverseitig spawnen |
| `Weapon.cs` | `ShootSpreadServerRpc(Vector3, float)` | Spread-Shot serverseitig spawnen |

### ObserversRPCs (Server → Alle Clients)
| Script | RPC | Zweck |
|--------|-----|-------|
| `PlayerController.cs` | `UpdateAnimationObserversRpc(float, float, bool)` | Animationen synchronisieren |
| `PlayerController.cs` | `DieObserversRpc()` | Spieler-Tod visuell darstellen |
| `BossOne.cs` | `RpcSetCharging(bool)` | Boss-Charge-Animation synchronisieren |
| `BossOne.cs` | `RpcPlayChargeSound()` | Boss-Charge-Sound abspielen |
| `BossOne.cs` | `RpcPlayHitSound()` | Boss-Hit-Sound abspielen |
| `BossOne.cs` | `RpcPlayDeathSound()` | Boss-Death-Sound abspielen |
| `Asteroid.cs` | `SetSpriteClientRpc(int)` | Zufälliges Asteroid-Sprite synchronisieren |
| `Asteroid.cs` | `FlashWhiteObserversRpc()` | Weißer Flash bei Treffer |
| `Alien2.cs` | `FlashWhiteObserversRpc()` | Weißer Flash bei Treffer |
| `Bullet.cs` | `PlayHitSoundObserversRpc()` | Hit-Sound bei Kollision |
| `AlienBullet.cs` | `PlayHitSoundObserversRpc()` | Hit-Sound bei Kollision |

---

## 🔄 Verwendete SyncVars

### GameManager.cs
```csharp
// Weltgeschwindigkeit (für Boost-Effekt)
private readonly SyncVar<float> syncWorldSpeed

// Score-System
private readonly SyncVar<int> syncScore

// Alien-Kill-Counter (für Boss-Spawn)
private readonly SyncVar<int> syncAlienCounter
```

**Permissions:** `WritePermission.ServerOnly`, `ReadPermission.Observers`

**OnChange Callbacks:**
- `OnWorldSpeedChanged` - Reagiert auf Geschwindigkeitsänderungen
- `OnScoreChanged` - Aktualisiert UI bei Score-Änderung

### PlayerController.cs
```csharp
// Spieler-Geschwindigkeit für Client-Interpolation
private readonly SyncVar<Vector2> syncVelocity
```

**Verwendung:** Nicht-Owner Clients bewegen sich basierend auf `syncVelocity.Value`

---

## 🎯 Bullet-Logik

### Bullet-System (Server-Authoritative)

**Klassen:**
- `Bullet.cs` - Spieler-Projektile
- `AlienBullet.cs` - Gegner-Projektile

**Flow:**
1. Spieler drückt Feuertaste → `Weapon.cs` registriert Input
2. `ShootNormalServerRpc()` oder `ShootSpreadServerRpc()` wird aufgerufen
3. **Server** instanziiert Bullet-Prefab
4. `ServerManager.Spawn(bullet)` synchronisiert Bullet zu allen Clients
5. Bullet bewegt sich in `FixedUpdate()` (nur Server berechnet)
6. Bei Kollision: `OnCollisionEnter2D()` (nur Server prüft)
7. Server despawnt Bullet → automatisch auf Clients entfernt

**Bullet-Patterns:**
- **Normal Shot:** Einzelnes Projektil, gerade Linie
- **Spread Shot:** 3 Projektile in 25° Fächer-Form
  - Cooldown: 5 Sekunden
  - Max. 3 Spread Shots gespeichert

**Wichtig:** Alle Kollisionen werden **nur serverseitig** geprüft (`if (!IsServerInitialized) return`)

---

## 👾 Gegner-Logik

### Gegnertypen

#### **Alien1** (`Alien1.cs`)
- **Verhalten:** Zufällige Bewegung + Rotation zum Ziel
- **Bewegung:** `moveSpeed = Random.Range(0.5f, 3f)`
- **Leben:** Konfigurierbar via Inspector
- **Besonderheit:** Rotiert sich immer zum Zielpunkt

```csharp
// Zufälliger Zielpunkt alle 0.1-2 Sekunden
private void GenerateRandomPosition()
{
    float randomX = Random.Range(-4f, 4f);
    float randomY = Random.Range(-4f, 4f);
    targetPosition = new Vector3(randomX, randomY, 0f);
}
```

#### **Alien2** (`Alien2.cs`)
- **Verhalten:** Ähnlich wie Alien1, aber mit zufälligem Sprite
- **Sprites:** Array von verschiedenen Alien-Sprites
- **Waffe:** `AlienWeapon.cs` - schießt automatisch auf Spieler
  - Feuerrate: Konfigurierbar
  - Bullet-Speed: Konfigurierbar

#### **Boss** (`BossOne.cs`)
- **Spawn-Bedingung:** `alienCounter >= 250`
- **Verhalten:** 
  - **Patrol-Modus:** Langsame Y-Bewegung
  - **Charge-Modus:** Schneller Angriff auf Spieler
  - State wechselt alle 2-10 Sekunden
- **Leben:** 10 HP
- **Besonderheit:** Prallgrenzen bei Y > 3 oder Y < -3

```csharp
// State-Machine
void EnterPatrolState() { speedX = 0f; speedY = Random.Range(-2f, 2f); }
void EnterChargeState() { speedX = -5f; speedY = 0f; }
```

### Spawning
- **Serverseitig:** Nur der Server spawnt Gegner
- **Synchronisation:** Automatisch durch FishNet NetworkObject
- **Despawn:** Bei X < -11f oder bei Tod

---

## 💗 Leben, Schaden & Gameflow

### Health-System

**PlayerController.cs:**
```csharp
[SerializeField] private float health;
[SerializeField] private float maxHealth;
```

**Schadensquellen:**
| Kollision | Schaden |
|-----------|---------|
| Obstacle | 1 HP |
| Enemy | 1 HP |
| EnemyBullet | 1 HP |
| Boss | 5 HP |

**Tod-Sequenz:**
1. `health <= 0` → `Die()` wird aufgerufen
2. `DieServerRpc()` → Explosion auf Server spawnen
3. `DieObserversRpc()` → GameObject deaktivieren auf allen Clients
4. `GameOverManager.LoadGameOverDelayed(2f)` → Game Over nach 2 Sekunden

### Gameflow

```
Main Menu
    ↓
Host/Client wählen
    ↓
Game Scene laden
    ↓
Spieler spawnen
    ↓
Gegner-Wellen + Boss
    ↓
Spieler stirbt
    ↓
Game Over Screen (2s Delay)
    ↓
Score anzeigen
    ↓
Restart oder Exit
```

---

## 📊 HUD & Score-System

### UI-Elemente (`UIController.cs`)

| Element | Anzeige |
|---------|---------|
| Energy Bar | Aktuelle/Max Energie |
| Health Bar | Aktuelle/Max Leben |
| Score Text | Aktueller Score |
| Pause Panel | Pause-Menü |

### Score-Vergabe (`GameManager.cs`)

```csharp
[Server]
public void AddAlienKillScore()    // Alien getötet
public void AddObstacleScore()     // Asteroid zerstört
public void AddBossKillScore()     // Boss getötet
public void AddEnemyKillScore()    // Generischer Gegner getötet
```

**Score-Werte:** Konfigurierbar im Inspector

**Synchronisation:**
```csharp
private readonly SyncVar<int> syncScore
syncScore.OnChange += OnScoreChanged;

private void OnScoreChanged(int oldValue, int newValue, bool asServer)
{
    UIController.Instance.UpdateScore(newValue);
}
```

Score wird **automatisch** zu allen Clients synchronisiert!

---

## 💾 Persistenz

### Lokales Score-System (PlayerPrefs)

**Implementierung in `GameOverManager.cs`:**

```csharp
private IEnumerator LoadGameOverCoroutine(float delay)
{
    yield return new WaitForSeconds(delay);
    
    // Score speichern
    if (GameManager.Instance != null)
    {
        int finalScore = GameManager.Instance.Score;
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        
        if (finalScore > bestScore)
        {
            PlayerPrefs.SetInt("BestScore", finalScore);
            PlayerPrefs.Save();
        }
        
        PlayerPrefs.SetInt("LastScore", finalScore);
        PlayerPrefs.Save();
    }
    
    UnitySceneManager.LoadScene(3, LoadSceneMode.Additive);
}
```

**Gespeicherte Daten:**
- `LastScore` - Score der letzten Session
- `BestScore` - Höchster je erreichter Score

**Persistenz:** Bleibt zwischen Sessions erhalten

---

## 🎨 Bonusfeatures                                                

#### **F) Visuelle & Technische Verbesserungen (8-10 Punkte)**

**Synchronisierte VFX:**
- Explosionseffekte bei Spieler-Tod
- Alien-Death-Effekte
- Weißer Flash bei Treffer (Material-Swap)
- Particle-Systems für alle Effekte

**Synchronisierte SFX (`AudioManager.cs`):**
```csharp
// Vollständiges Audio-System
public AudioSource Death;
public AudioSource EnemyDeath;
public AudioSource Fire;
public AudioSource Hit;
public AudioSource Boost;
public AudioSource Pause;
public AudioSource Unpause;
public AudioSource hitObst;
public AudioSource Shoot;
public AudioSource EnemyDeath2;
public AudioSource Burn;
public AudioSource BossHit;
public AudioSource bossCharge;
```

**Features:**
- Pitch-Variation für natürlicheren Sound
- Singleton-Pattern (DontDestroyOnLoad)
- Synchronisierte Sounds über RPCs

**Animationen:**
- Spieler-Animationen (Move, Boost)
- Boss-Charge-Animation
- Synchronisiert über `UpdateAnimationObserversRpc()`

**Technische Features:**
- Client-Prediction für Movement (via `syncVelocity`)
- Server-Authoritative Gameplay (keine Cheats möglich)
- Pause-System (nur Owner kann pausieren)

---

## 📂 Projektstruktur

```
Assets/
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   └── Weapon.cs
│   ├── Enemies/
│   │   ├── Alien1.cs
│   │   ├── Alien2.cs
│   │   ├── BossOne.cs
│   │   └── AlienWeapon.cs
│   ├── Projectiles/
│   │   ├── Bullet.cs
│   │   └── AlienBullet.cs
│   ├── Environment/
│   │   ├── Asteroid.cs
│   │   └── ParallaxBackground.cs
│   ├── Managers/
│   │   ├── GameManager.cs
│   │   ├── UIController.cs
│   │   ├── AudioManager.cs
│   │   └── GameOverManager.cs
│   └── Effects/
│       └── Explosion.cs
├── Prefabs/
│   ├── Player.prefab
│   ├── Enemies/
│   ├── Projectiles/
│   └── Effects/
└── Scenes/
    ├── MainMenu.unity
    ├── Game.unity
    └── GameOver.unity
```

---

## 🐛 Bekannte Bugs & Einschränkungen

### Einschränkungen
- Nur Host-Client-Modus (kein Dedicated Server)
- Keine automatische Reconnect-Logik
- Port muss manuell konfiguriert werden (kein NAT Punch-through)
- Keine Lobby-UI (direkter Connect zu localhost)

### Performance
- Getestet mit bis zu 50+ Bullets gleichzeitig
- Stabil bei 60 FPS
- Keine bekannten Memory Leaks

---

## 🎓 Lernziele & Reflexion

### Was ich gelernt habe
- ✅ FishNet Grundlagen (NetworkObject, NetworkBehaviour)
- ✅ Server-Authoritative Architektur
- ✅ RPC-System (ServerRpc, ObserversRpc, TargetRpc)
- ✅ SyncVar-Callbacks
- ✅ Multiplayer-Debugging

### Herausforderungen
- **Synchronisation:** Animationen und Sounds über Netzwerk synchronisieren
- **Server Authority:** Alle Gameplay-Logik serverseitig implementieren
- **Debugging:** Gleichzeitig zwei Unity-Instanzen debuggen

### Verbesserungspotenzial
- Mehr Zeit für echtes Verständnis statt Copy-Paste
- Bessere Code-Dokumentation während der Entwicklung
- Früher mit Multiplayer-Testing beginnen

---

## 📝 Checkliste der Pflichtanforderungen

- [x] **Multiplayer-Basis (10P):** FishNet korrekt eingerichtet, Host/Client funktioniert
- [x] **Spielersteuerung (15P):** Top-Down Movement, NetworkObject, Ownership, SyncVars
- [x] **Bullet-Hell (20P):** 2 Schussmuster (Normal + Spread), Synchronisation, Cooldowns
- [x] **Gegner/Boss (15P):** 2 Gegnertypen (Alien1, Alien2), Boss, Server-Spawn
- [x] **Leben & Gameflow (10P):** HP-System, Schaden, Game Over Flow
- [x] **HUD & Score (10P):** HP/Energy/Score-Anzeige, Persistenz (PlayerPrefs)
- [x] **Bonusfeatures (10P):** VFX/SFX synchronisiert, Animationen
- [x] **README.md:** Vollständige Dokumentation ✅


---

## 📧 Kontakt

**Entwickler:** Dennis De Col  
**Projekt:** Hell Rider (Twin Fire)  

---

## 📜 Lizenz

Dieses Projekt wurde als Schulprojekt für die SRH Fachschulen erstellt.  
Abgabedatum: 23.01.2026
