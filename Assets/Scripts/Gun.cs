using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// Exercice 5 : tire une balle depuis le canon quand on appuie sur la gâchette (trigger)
/// pendant que le pistolet est saisi. À placer sur le parent "Pistol".
[RequireComponent(typeof(XRGrabInteractable))]
public class Gun : MonoBehaviour
{
    public Rigidbody bulletPrefab;
    public Transform muzzle;               // axe Z (bleu) = direction du tir
    public float bulletSpeed = 25f;        // m/s
    public bool bulletGravity = true;
    public float fireCooldown = 0.15f;
    public AudioSource fireSound;          // optionnel

    XRGrabInteractable grab;
    Collider[] gunColliders;
    float nextFire;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        gunColliders = GetComponentsInChildren<Collider>();
    }

    // "Activate" = gâchette dans XRI (quand l'objet est tenu)
    void OnEnable() => grab.activated.AddListener(OnActivated);
    void OnDisable() => grab.activated.RemoveListener(OnActivated);

    void OnActivated(ActivateEventArgs args) => Fire();

    public void Fire()
    {
        if (Time.time < nextFire) return;
        nextFire = Time.time + fireCooldown;

        Rigidbody bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        bullet.useGravity = bulletGravity;

        // La balle ne doit pas percuter le pistolet lui-même
        Collider bc = bullet.GetComponent<Collider>();
        if (bc != null)
            foreach (Collider c in gunColliders) Physics.IgnoreCollision(bc, c);

        // Propulsion : VelocityChange = vitesse directe, indépendante de la masse
        bullet.AddForce(muzzle.forward * bulletSpeed, ForceMode.VelocityChange);

        if (fireSound != null) fireSound.Play();
    }
}
