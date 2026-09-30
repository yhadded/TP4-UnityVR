# TP4 – Unity VR

Yassine Hadded – EFREI Paris, Technologies Immersives & IA

Unity 6 (6000.6) · URP · XR Interaction Toolkit 3.6 · OpenXR · Meta Quest 3 (Quest Link)

## Scripts (`Assets/Scripts/`)

| Exercice | Script | Rôle |
|---|---|---|
| 3 – Déplacement | `JoystickLocomotion.cs` | Joystick gauche : déplacement relatif au regard, avec gravité et collisions (CharacterController). Joystick droit : snap turn 30/45° ou rotation fluide autour de la tête. |
| 4 – UI | `UIDemo.cs` | Relie le bouton, le slider et l'InputField (clavier virtuel XRI) à un texte de feedback. |
| 5 – Pistolet | `Gun.cs` | Tir sur la gâchette (événement `activated` de `XRGrabInteractable`) : instancie la balle au canon et la propulse avec `ForceMode.VelocityChange`. |
| 5-6 – Balle | `Bullet.cs` | Auto-destruction après un délai ou au premier impact. |
| 6 – Cibles | `Target.cs` | Détecte l'impact d'une balle, joue l'effet de particules, incrémente le score et se détruit. |
| 6 – Spawner | `TargetSpawner.cs` | Fait apparaître des cibles aléatoires (cube, cône…) dans une zone, à intervalle régulier. |

## Mise en place automatique

`Assets/Editor/TP4SceneBuilder.cs` ajoute un menu **TP4** dans l'éditeur :

1. **TP4 → 1. Installer OpenXR + samples XRI** : ajoute OpenXR et importe *Starter Assets* et *XR Interaction Simulator*.
2. Activer OpenXR (*XR Plug-in Management → PC*), ajouter les profils Meta Quest Touch Plus et Oculus Touch, puis *Fix All*.
3. **TP4 → 2. Construire la scène** : crée le sol, le XR Origin (avec `JoystickLocomotion`), le cube saisissable, le Canvas UI, le pistolet, les prefabs Bullet, Explosion et Target, et le spawner, puis sauvegarde la scène.

## Mise en place manuelle

1. **XR** : *Project Settings → XR Plug-in Management → PC → OpenXR*, puis dans *Interaction Profiles*, ajouter *Meta Quest Touch Plus* et *Oculus Touch*. Terminer avec *Project Validation → Fix All*.
2. **XRI** : installé via `Packages/manifest.json`. Importer les samples *Starter Assets*, et *XR Interaction Simulator* pour tester sans casque.
3. **Scène** :
   - `XR Origin (XR Rig)` avec `JoystickLocomotion` (et locomotion Starter Assets désactivée)
   - Cube avec `XR Grab Interactable`
   - Canvas XR avec `UIDemo`
   - Pistolet avec `XR Grab Interactable` et `Gun` (+ Muzzle, AttachPoint)
   - Prefabs Bullet et Target, et `TargetSpawner`
4. **Quest Link** : le runtime OpenXR doit être *Meta Horizon Link*, puis Play.
