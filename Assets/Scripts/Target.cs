using UnityEngine;

/// Exercice 6 : détecte l'impact d'une balle, joue l'effet de particules et détruit la cible.
[RequireComponent(typeof(Collider))]
public class Target : MonoBehaviour
{
    public GameObject hitEffectPrefab;     // prefab avec Particle System
    public float lifetime = 0f;            // 0 = reste jusqu'à être touchée

    public static int Score;

    void Start()
    {
        if (lifetime > 0f) Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.GetComponent<Bullet>() == null) return;

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        Score++;
        Debug.Log($"Cible touchée ! Score = {Score}");
        Destroy(gameObject);
    }
}
