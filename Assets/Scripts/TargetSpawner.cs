using System.Collections.Generic;
using UnityEngine;

/// Exercice 6 : fait apparaître des cibles aléatoires dans une zone, à intervalle régulier.
/// À placer sur un GameObject vide positionné devant le joueur.
public class TargetSpawner : MonoBehaviour
{
    public Target[] targetPrefabs;                    // cube, cône, sphère...
    public Vector3 areaSize = new Vector3(8f, 3f, 2f);
    public float spawnInterval = 2f;
    public int maxTargets = 8;

    readonly List<Target> alive = new List<Target>();
    float timer;

    void Update()
    {
        alive.RemoveAll(t => t == null);              // cibles détruites

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            if (alive.Count < maxTargets) Spawn();
        }
    }

    void Spawn()
    {
        if (targetPrefabs == null || targetPrefabs.Length == 0) return;

        Target prefab = targetPrefabs[Random.Range(0, targetPrefabs.Length)];
        Vector3 local = new Vector3(
            Random.Range(-0.5f, 0.5f) * areaSize.x,
            Random.Range(-0.5f, 0.5f) * areaSize.y,
            Random.Range(-0.5f, 0.5f) * areaSize.z);

        Target t = Instantiate(prefab, transform.TransformPoint(local),
                               Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        alive.Add(t);
    }

    // Visualise la zone de spawn dans la vue Scene
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, areaSize);
    }
}
