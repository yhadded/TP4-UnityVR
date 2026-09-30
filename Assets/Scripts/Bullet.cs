using UnityEngine;

/// Exercices 5-6 : balle auto-détruite après un délai ou au premier impact.
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public float lifetime = 3f;

    void Start() => Destroy(gameObject, lifetime);

    void OnCollisionEnter(Collision col) => Destroy(gameObject);
}
