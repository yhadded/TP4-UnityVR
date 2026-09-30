# TP4 – Unity VR

## Scripts (`Assets/Scripts/`)

| Exercice | Script | Rôle |
|---|---|---|
| 3 – Déplacement | `JoystickLocomotion.cs` | Joystick gauche : déplacement relatif au regard, avec gravité et collisions (CharacterController). Joystick droit : snap turn 30/45° ou rotation fluide autour de la tête. |
| 4 – UI | `UIDemo.cs` | Relie le bouton, le slider et l'InputField (clavier virtuel XRI) à un texte de feedback. |
| 5 – Pistolet | `Gun.cs` | Tir sur la gâchette (événement `activated` de `XRGrabInteractable`) : instancie la balle au canon et la propulse avec `ForceMode.VelocityChange`. |
| 5-6 – Balle | `Bullet.cs` | Auto-destruction après un délai ou au premier impact. |
| 6 – Cibles | `Target.cs` | Détecte l'impact d'une balle, joue l'effet de particules, incrémente le score et se détruit. |
| 6 – Spawner | `TargetSpawner.cs` | Fait apparaître des cibles aléatoires (cube, cône…) dans une zone, à intervalle régulier. |


https://github.com/yhadded/TP4-UnityVR.git
