using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// Outil d'éditeur : construit automatiquement la scène du TP4.
/// Menu "TP4" dans la barre du haut :
///   1. Installer OpenXR + samples XRI
///   2. Construire la scène
public static class TP4SceneBuilder
{
    const string XRI = "com.unity.xr.interaction.toolkit";
    const string PrefabDir = "Assets/Prefabs";
    const string MatDir = "Assets/Materials";

    // ─────────────────────────────── 1. Installation ───────────────────────────────

    [MenuItem("TP4/1. Installer OpenXR + samples XRI")]
    static void InstallXR()
    {
        Client.Add("com.unity.xr.openxr");   // version compatible choisie par Unity

        var samples = Sample.FindByPackage(XRI, null).ToList();
        int imported = 0;
        foreach (var s in samples)
        {
            if (s.displayName == "Starter Assets" || s.displayName == "XR Interaction Simulator")
            {
                if (!s.isImported) s.Import(Sample.ImportOptions.OverridePreviousImports);
                imported++;
            }
        }

        EditorUtility.DisplayDialog("TP4",
            $"OpenXR en cours d'installation, {imported} sample(s) XRI importé(s).\n\n" +
            "Ensuite :\n" +
            "• Project Settings → XR Plug-in Management → PC → cocher OpenXR\n" +
            "• OpenXR → Interaction Profiles → + Meta Quest Touch Plus et Oculus Touch\n" +
            "• Project Validation → Fix All\n" +
            "• Puis menu TP4 → 2. Construire la scène", "OK");
    }

    // ─────────────────────────────── 2. Scène ───────────────────────────────

    [MenuItem("TP4/2. Construire la scène")]
    static void BuildScene()
    {
        var rigPrefab = FindPrefab("XR Origin (XR Rig)", "Starter Assets");
        if (rigPrefab == null)
        {
            EditorUtility.DisplayDialog("TP4",
                "Prefab 'XR Origin (XR Rig)' introuvable.\nLance d'abord TP4 → 1. Installer OpenXR + samples XRI.", "OK");
            return;
        }

        EnsureFolder(PrefabDir);
        EnsureFolder(MatDir);

        // Nettoyage
        DestroyIfExists("Main Camera");
        DestroyIfExists("TP4");
        DestroyIfExists("XR Origin (XR Rig)");
        var root = new GameObject("TP4");

        // ── Ex 1 : sol + rig ──
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Sol";
        floor.transform.SetParent(root.transform);
        floor.transform.localScale = new Vector3(5, 1, 5);
        floor.GetComponent<Renderer>().sharedMaterial = Mat("Sol", new Color(0.75f, 0.75f, 0.75f));

        var rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
        rig.transform.position = Vector3.zero;

        // ── Ex 3 : locomotion custom (désactive celle des Starter Assets) ──
        var loco = FindChild(rig.transform, "Locomotion");
        if (loco != null) loco.gameObject.SetActive(false);
        if (rig.GetComponent<JoystickLocomotion>() == null) rig.AddComponent<JoystickLocomotion>();

        // ── Ex 2 : table + cube saisissable ──
        var table = Cube("Table", root.transform, new Vector3(0, 0.4f, 1f), new Vector3(1.2f, 0.8f, 0.6f),
                         Mat("Table", new Color(0.45f, 0.3f, 0.2f)));
        var grabCube = Cube("Cube Grab", root.transform, new Vector3(-0.3f, 0.9f, 1f), Vector3.one * 0.1f,
                            Mat("Cube", new Color(0.2f, 0.6f, 1f)));
        var rb = grabCube.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        var grab = grabCube.AddComponent<XRGrabInteractable>();
        grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        grab.throwOnDetach = true;

        // ── Ex 5-6 : prefabs ──
        var explosion = BuildExplosionPrefab();
        var bullet = BuildBulletPrefab();
        var targets = new[]
        {
            BuildTargetPrefab("Target Cube", PrimitiveType.Cube, Color.red, explosion),
            BuildTargetPrefab("Target Sphere", PrimitiveType.Sphere, new Color(1f, 0.6f, 0f), explosion),
            BuildTargetPrefab("Target Capsule", PrimitiveType.Capsule, Color.magenta, explosion),
        };

        // ── Ex 5 : pistolet ──
        BuildGun(root.transform, new Vector3(0.2f, 0.9f, 1f), bullet);

        // ── Ex 6 : spawner ──
        var spawnerGo = new GameObject("TargetSpawner");
        spawnerGo.transform.SetParent(root.transform);
        spawnerGo.transform.position = new Vector3(0, 1.5f, 6f);
        var spawner = spawnerGo.AddComponent<TargetSpawner>();
        spawner.targetPrefabs = targets;

        // ── Ex 4 : UI ──
        BuildUI(root.transform);

        // Simulateur (optionnel, désactivé par défaut pour ne pas gêner le casque)
        var simPrefab = FindPrefab("XR Interaction Simulator", "XR Interaction Simulator");
        if (simPrefab != null && GameObject.Find("XR Interaction Simulator") == null)
        {
            var sim = (GameObject)PrefabUtility.InstantiatePrefab(simPrefab);
            sim.SetActive(false);
        }

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("TP4",
            "Scène construite et sauvegardée ✔\n\n" +
            "• Casque : Quest Link lancé → Play\n" +
            "• Sans casque : activer 'XR Interaction Simulator' dans la Hierarchy → Play", "OK");
    }

    // ─────────────────────────────── Pistolet ───────────────────────────────

    static void BuildGun(Transform parent, Vector3 pos, Rigidbody bulletPrefab)
    {
        var gun = new GameObject("Pistol");
        gun.transform.SetParent(parent);
        gun.transform.position = pos;

        var dark = Mat("Pistol", new Color(0.15f, 0.15f, 0.15f));
        var model = new GameObject("Model").transform;
        model.SetParent(gun.transform, false);

        // Corps + canon + crosse (primitives, sans collider individuel)
        Part(PrimitiveType.Cube, "Body", model, new Vector3(0, 0, 0.04f), new Vector3(0.035f, 0.05f, 0.16f), dark);
        var barrel = Part(PrimitiveType.Cylinder, "Barrel", model, new Vector3(0, 0.01f, 0.14f),
                          new Vector3(0.018f, 0.04f, 0.018f), dark);
        barrel.localRotation = Quaternion.Euler(90, 0, 0);
        var gripT = Part(PrimitiveType.Cube, "Grip", model, new Vector3(0, -0.06f, -0.01f),
                         new Vector3(0.03f, 0.1f, 0.04f), dark);
        gripT.localRotation = Quaternion.Euler(-15, 0, 0);

        var col = gun.AddComponent<BoxCollider>();
        col.center = new Vector3(0, -0.02f, 0.05f);
        col.size = new Vector3(0.04f, 0.14f, 0.22f);

        var rb = gun.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        var muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(gun.transform, false);
        muzzle.localPosition = new Vector3(0, 0.01f, 0.19f);

        var attach = new GameObject("AttachPoint").transform;
        attach.SetParent(gun.transform, false);
        attach.localPosition = new Vector3(0, -0.06f, -0.01f);

        var grab = gun.AddComponent<XRGrabInteractable>();
        grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
        grab.attachTransform = attach;

        var g = gun.AddComponent<Gun>();
        g.muzzle = muzzle;
        g.bulletPrefab = bulletPrefab;
    }

    // ─────────────────────────────── Prefabs ───────────────────────────────

    static Rigidbody BuildBulletPrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Bullet";
        go.transform.localScale = Vector3.one * 0.03f;
        go.GetComponent<Renderer>().sharedMaterial = Mat("Bullet", Color.yellow);
        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 0.05f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        go.AddComponent<Bullet>();
        var prefab = SavePrefab(go, "Bullet");
        return prefab.GetComponent<Rigidbody>();
    }

    static GameObject BuildExplosionPrefab()
    {
        var go = new GameObject("Explosion");
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.5f, 0f), Color.yellow);
        main.gravityModifier = 0.5f;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 50) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.6f, 0f), 0f), new GradientColorKey(Color.gray, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0));

        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader != null)
        {
            var m = SaveMat(new Material(shader), "Explosion");
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = m;
        }

        return SavePrefab(go, "Explosion");
    }

    static Target BuildTargetPrefab(string name, PrimitiveType type, Color c, GameObject explosion)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.localScale = Vector3.one * 0.4f;
        go.GetComponent<Renderer>().sharedMaterial = Mat(name, c);
        var t = go.AddComponent<Target>();
        t.hitEffectPrefab = explosion;
        t.lifetime = 8f;
        return SavePrefab(go, name).GetComponent<Target>();
    }

    // ─────────────────────────────── UI ───────────────────────────────

    static void BuildUI(Transform parent)
    {
        // EventSystem XR
        var es = Object.FindFirstObjectByType<EventSystem>();
        if (es == null) es = new GameObject("EventSystem").AddComponent<EventSystem>();
        foreach (var m in es.GetComponents<BaseInputModule>())
            if (!(m is XRUIInputModule)) Object.DestroyImmediate(m);
        if (es.GetComponent<XRUIInputModule>() == null) es.gameObject.AddComponent<XRUIInputModule>();

        // Canvas monde
        var canvasGo = new GameObject("Canvas UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                                      typeof(TrackedDeviceGraphicRaycaster));
        canvasGo.transform.SetParent(parent);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var rt = (RectTransform)canvasGo.transform;
        rt.sizeDelta = new Vector2(500, 400);
        rt.position = new Vector3(-1.2f, 1.5f, 2f);
        rt.rotation = Quaternion.Euler(0, -20, 0);
        rt.localScale = Vector3.one * 0.002f;

        var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(canvasGo.transform, false);
        Stretch((RectTransform)bg.transform);
        bg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

        var tmpRes = new TMP_DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
        };
        var uiRes = new DefaultControls.Resources
        {
            standard = tmpRes.standard,
            background = tmpRes.background,
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
        };

        var feedback = TMP_DefaultControls.CreateText(tmpRes).GetComponent<TMP_Text>();
        Place(feedback.gameObject, canvasGo.transform, new Vector2(0, 140), new Vector2(460, 60));
        feedback.text = "TP4 – Interagis avec l'UI";
        feedback.fontSize = 30;
        feedback.alignment = TextAlignmentOptions.Center;
        feedback.color = Color.white;

        var button = TMP_DefaultControls.CreateButton(tmpRes);
        Place(button, canvasGo.transform, new Vector2(0, 50), new Vector2(260, 60));
        button.GetComponentInChildren<TMP_Text>().text = "Clique-moi";

        var slider = DefaultControls.CreateSlider(uiRes);
        Place(slider, canvasGo.transform, new Vector2(0, -40), new Vector2(360, 30));

        var input = TMP_DefaultControls.CreateInputField(tmpRes);
        Place(input, canvasGo.transform, new Vector2(0, -120), new Vector2(360, 60));

        var demo = canvasGo.AddComponent<UIDemo>();
        demo.button = button.GetComponent<Button>();
        demo.slider = slider.GetComponent<Slider>();
        demo.inputField = input.GetComponent<TMP_InputField>();
        demo.feedback = feedback;
    }

    // ─────────────────────────────── Helpers ───────────────────────────────

    static GameObject FindPrefab(string name, string pathHint)
    {
        var paths = AssetDatabase.FindAssets($"\"{name}\" t:Prefab")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => System.IO.Path.GetFileNameWithoutExtension(p) == name)
            .OrderByDescending(p => p.Contains(pathHint))
            .ThenByDescending(p => p)      // version la plus récente en premier
            .ToList();
        return paths.Count > 0 ? AssetDatabase.LoadAssetAtPath<GameObject>(paths[0]) : null;
    }

    static Transform FindChild(Transform t, string name) =>
        t.GetComponentsInChildren<Transform>(true).FirstOrDefault(c => c.name == name);

    static void DestroyIfExists(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) Object.DestroyImmediate(go);
    }

    static GameObject Cube(string name, Transform parent, Vector3 pos, Vector3 scale, Material m)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = m;
        return go;
    }

    static Transform Part(PrimitiveType type, string name, Transform parent, Vector3 lpos, Vector3 lscale, Material m)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = lpos;
        go.transform.localScale = lscale;
        go.GetComponent<Renderer>().sharedMaterial = m;
        return go.transform;
    }

    static void Place(GameObject go, Transform parent, Vector2 pos, Vector2 size)
    {
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static Material Mat(string name, Color c)
    {
        var path = $"{MatDir}/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(shader);
        m.SetColor("_BaseColor", c);
        m.color = c;
        return SaveMat(m, name);
    }

    static Material SaveMat(Material m, string name)
    {
        var path = $"{MatDir}/{name}.mat";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static GameObject SavePrefab(GameObject go, string name)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/{name}.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder("Assets", path.Substring("Assets/".Length));
    }
}
