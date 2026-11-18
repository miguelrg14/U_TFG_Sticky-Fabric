using UnityEngine;
using UnityEngine.SceneManagement;

namespace StickyFabric.Demo
{
    /// <summary>
    /// On-screen controls for the demo scene: live tuning of the fabric, FPS counter,
    /// and manual control of the moving sphere. Spawns itself in the demo scene.
    /// </summary>
    public sealed class DemoHud : MonoBehaviour
    {
        private const string DemoSceneName = "StickyFabricDemo";
        private const float ManualSpeed = 1.5f;

        private StickyFabricMesh fabric;
        private PingPongMover mover;
        private Vector3 moverStart;
        private float smoothedDeltaTime = 1f / 60f;
        private bool showPanel = true;
        private GUIStyle labelStyle;

        // RuntimeInitializeOnLoadMethod only runs once, when Play starts. Listening to sceneLoaded
        // also covers the demo scene being loaded later (from another scene or from a test).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneHook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != DemoSceneName || FindAnyObjectByType<DemoHud>() != null)
            {
                return;
            }

            var hud = new GameObject(nameof(DemoHud));
            SceneManager.MoveGameObjectToScene(hud, scene);
            hud.AddComponent<DemoHud>();
        }

        private void Start()
        {
            fabric = FindAnyObjectByType<StickyFabricMesh>();
            mover = FindAnyObjectByType<PingPongMover>();
            if (mover != null)
            {
                moverStart = mover.transform.position;
            }
        }

        private void Update()
        {
            smoothedDeltaTime = Mathf.Lerp(smoothedDeltaTime, Time.unscaledDeltaTime, 0.05f);

            if (Input.GetKeyDown(KeyCode.H))
            {
                showPanel = !showPanel;
            }

            if (mover == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                mover.enabled = !mover.enabled;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                mover.transform.position = moverStart;
                if (fabric != null)
                {
                    fabric.ResetFabric();
                }
            }

            if (!mover.enabled)
            {
                // WASD / arrows move in X and Y, Q and E move along Z, through the fabric.
                float depth = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);
                var input = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), depth);
                mover.transform.position += Vector3.ClampMagnitude(input, 1f) * (ManualSpeed * Time.deltaTime);
            }
        }

        private void OnGUI()
        {
            labelStyle ??= new GUIStyle(GUI.skin.label) { richText = true };

            if (!showPanel)
            {
                GUI.Label(new Rect(10, 10, 300, 24), "Press <b>H</b> to show controls", labelStyle);
                return;
            }

            GUILayout.BeginArea(new Rect(10, 10, 290, 380), GUI.skin.box);
            GUILayout.Label("<b>Sticky Fabric</b> - chewing-gum mesh deformation", labelStyle);
            GUILayout.Label($"FPS: {1f / Mathf.Max(smoothedDeltaTime, 1e-5f):0}");

            if (fabric != null)
            {
                GUILayout.Label($"Vertices: {fabric.VertexCount}   Glued: {fabric.GluedVertexCount}");

                GUILayout.Label($"Subdivisions: {fabric.Subdivisions}");
                fabric.Subdivisions = Mathf.RoundToInt(GUILayout.HorizontalSlider(fabric.Subdivisions, 2f, 40f));

                GUILayout.Label($"Elastic limit: {fabric.ElasticLimit:0.00}");
                fabric.ElasticLimit = GUILayout.HorizontalSlider(fabric.ElasticLimit, 0.05f, 2f);

                GUILayout.Label($"Return stiffness: {fabric.ReturnStiffness:0.0} /s");
                fabric.ReturnStiffness = GUILayout.HorizontalSlider(fabric.ReturnStiffness, 1f, 60f);

                GUILayout.Label($"Contact radius: {fabric.ContactRadius:0.000}");
                fabric.ContactRadius = GUILayout.HorizontalSlider(fabric.ContactRadius, 0.01f, 0.15f);
            }

            if (mover != null)
            {
                GUILayout.Label(mover.enabled
                    ? "<b>Space</b>: take manual control"
                    : "<b>WASD</b>: move   <b>Q/E</b>: depth   <b>Space</b>: auto", labelStyle);
                GUILayout.Label("<b>R</b>: reset   <b>H</b>: hide panel", labelStyle);
            }

            GUILayout.EndArea();
        }
    }
}
