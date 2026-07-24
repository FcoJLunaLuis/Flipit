using UnityEngine;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Creates Unity GameObjects for each cell/prop type.
    /// All created objects are assigned to the CityGeometry layer.
    /// </summary>
    public static class GeometryFactory
    {
        private const string LayerName = "CityGeometry";

        private static int GetCityGeometryLayer()
        {
            int layer = LayerMask.NameToLayer(LayerName);
            if (layer == -1)
            {
                Debug.LogWarning($"[GeometryFactory] Layer '{LayerName}' does not exist. Using default layer. Please add '{LayerName}' to your project's layer settings.");
                return 0;
            }
            return layer;
        }

        private static Material CreateUnlitMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = color;
            return mat;
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }

        /// <summary>
        /// Creates a ground plane as a single quad mesh oriented on XZ (normal Y-up)
        /// at world position (0,0,0) with a MeshCollider.
        /// </summary>
        public static GameObject CreateGroundPlane(float width, float depth, Color color, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "GroundPlane";

            // Rotate quad to face Y-up (default quad faces Z+)
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(width, depth, 1f);

            // Position at center of the grid
            // Grid goes from (0,0,0) with cells at col*cellSize, row*cellSize
            // So center is at (width/2, 0, depth/2)
            go.transform.position = new Vector3(width * 0.5f, 0f, depth * 0.5f);

            // Replace default material with unlit flat-color
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateUnlitMaterial(color);

            // Replace the default MeshCollider (Quad primitive already has one)
            // Ensure MeshCollider uses the quad mesh
            var collider = go.GetComponent<MeshCollider>();
            if (collider == null)
            {
                collider = go.AddComponent<MeshCollider>();
            }
            collider.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;

            int layer = GetCityGeometryLayer();
            go.layer = layer;

            if (parent != null)
                go.transform.SetParent(parent);

            return go;
        }

        /// <summary>
        /// Creates a flat quad at Y=0 for a street cell.
        /// </summary>
        public static GameObject CreateStreetCell(Vector3 position, float cellSize, Color color, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = $"Street_{position.x:F0}_{position.z:F0}";

            // Rotate quad to face Y-up
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(cellSize, cellSize, 1f);
            go.transform.position = new Vector3(position.x, 0f, position.z);

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateUnlitMaterial(color);

            // Remove default collider (streets don't need individual colliders; ground plane covers it)
            Object.DestroyImmediate(go.GetComponent<MeshCollider>());

            int layer = GetCityGeometryLayer();
            go.layer = layer;

            if (parent != null)
                go.transform.SetParent(parent);

            return go;
        }

        /// <summary>
        /// Creates a flat quad at Y=0.05 for a sidewalk cell.
        /// </summary>
        public static GameObject CreateSidewalkCell(Vector3 position, float cellSize, Color color, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = $"Sidewalk_{position.x:F0}_{position.z:F0}";

            // Rotate quad to face Y-up
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(cellSize, cellSize, 1f);
            go.transform.position = new Vector3(position.x, 0.05f, position.z);

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateUnlitMaterial(color);

            // Remove default collider
            Object.DestroyImmediate(go.GetComponent<MeshCollider>());

            int layer = GetCityGeometryLayer();
            go.layer = layer;

            if (parent != null)
                go.transform.SetParent(parent);

            return go;
        }

        /// <summary>
        /// Creates a scaled cube for a building. Base sits on ground (Y=0).
        /// BoxCollider matching scale is included by default with cube primitive.
        /// </summary>
        public static GameObject CreateBuilding(Vector3 position, float cellSize, float height, Color color, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Building_{position.x:F0}_{position.z:F0}";

            // Scale: cellSize x height x cellSize
            go.transform.localScale = new Vector3(cellSize, height, cellSize);

            // Position so base sits on ground: center Y = height / 2
            go.transform.position = new Vector3(position.x, height * 0.5f, position.z);

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateUnlitMaterial(color);

            // Cube primitive already comes with a BoxCollider that matches its scale
            int layer = GetCityGeometryLayer();
            go.layer = layer;

            if (parent != null)
                go.transform.SetParent(parent);

            return go;
        }

        /// <summary>
        /// Creates a composite tree: cylinder trunk (radius 0.15, height 1.5) +
        /// sphere canopy (radius 0.5, center at Y=2.0 from cell surface).
        /// CapsuleCollider on root encompassing geometry.
        /// </summary>
        public static GameObject CreateTree(Vector3 position, Color trunkColor, Color canopyColor, Transform parent)
        {
            var root = new GameObject($"Tree_{position.x:F0}_{position.z:F0}");
            root.transform.position = position;

            // Trunk: cylinder with radius 0.15, height 1.5
            // Unity cylinder is height 2, radius 0.5 by default at scale (1,1,1)
            // To get radius 0.15: scaleX = scaleZ = 0.15 / 0.5 = 0.3
            // To get height 1.5: scaleY = 1.5 / 2.0 = 0.75
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(root.transform);
            trunk.transform.localScale = new Vector3(0.3f, 0.75f, 0.3f);
            // Center of cylinder at half its height: 1.5/2 = 0.75
            trunk.transform.localPosition = new Vector3(0f, 0.75f, 0f);

            var trunkRenderer = trunk.GetComponent<MeshRenderer>();
            trunkRenderer.sharedMaterial = CreateUnlitMaterial(trunkColor);

            // Remove trunk's default collider
            Object.DestroyImmediate(trunk.GetComponent<Collider>());

            // Canopy: sphere with radius 0.5, center at Y=2.0 from surface
            // Unity sphere is radius 0.5 at scale (1,1,1) — perfect as-is
            var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "Canopy";
            canopy.transform.SetParent(root.transform);
            canopy.transform.localScale = new Vector3(1f, 1f, 1f);
            canopy.transform.localPosition = new Vector3(0f, 2.0f, 0f);

            var canopyRenderer = canopy.GetComponent<MeshRenderer>();
            canopyRenderer.sharedMaterial = CreateUnlitMaterial(canopyColor);

            // Remove canopy's default collider
            Object.DestroyImmediate(canopy.GetComponent<Collider>());

            // Add CapsuleCollider on root encompassing geometry
            // Tree extends from Y=0 to Y=2.5 (canopy top = 2.0 + 0.5)
            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 1.25f, 0f);
            capsule.height = 2.5f;
            capsule.radius = 0.5f;

            int layer = GetCityGeometryLayer();
            SetLayerRecursive(root, layer);

            if (parent != null)
                root.transform.SetParent(parent);

            return root;
        }

        /// <summary>
        /// Creates a composite car: lower cube body (2x0.5x1) + upper cube cabin (1.2x0.4x0.8).
        /// BoxCollider on root. Y-rotation as specified.
        /// </summary>
        public static GameObject CreateCar(Vector3 position, float yRotation, Color color, Transform parent)
        {
            var root = new GameObject($"Car_{position.x:F0}_{position.z:F0}");
            root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

            Material carMat = CreateUnlitMaterial(color);

            // Body: fits within cell (3 long x 1.2 high x 1.5 wide)
            // Ratio is ~2:1 vs player height (player=2, car height=1.2+0.8=2.0, length=3)
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform);
            body.transform.localScale = new Vector3(3f, 1.0f, 1.5f);
            body.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            var bodyRenderer = body.GetComponent<MeshRenderer>();
            bodyRenderer.sharedMaterial = carMat;

            Object.DestroyImmediate(body.GetComponent<Collider>());

            // Cabin: smaller cube on top
            var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.SetParent(root.transform);
            cabin.transform.localScale = new Vector3(1.8f, 0.7f, 1.3f);
            cabin.transform.localPosition = new Vector3(0f, 1.35f, 0f);

            var cabinRenderer = cabin.GetComponent<MeshRenderer>();
            cabinRenderer.sharedMaterial = carMat;

            Object.DestroyImmediate(cabin.GetComponent<Collider>());

            // BoxCollider encompassing full car
            var box = root.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.85f, 0f);
            box.size = new Vector3(3f, 1.7f, 1.5f);

            int layer = GetCityGeometryLayer();
            SetLayerRecursive(root, layer);

            if (parent != null)
                root.transform.SetParent(parent);

            return root;
        }

        /// <summary>
        /// Creates a composite lamppost: thin cylinder (radius 0.05, height 3.0) +
        /// sphere (radius 0.15) at top. CapsuleCollider on root.
        /// </summary>
        public static GameObject CreateLamppost(Vector3 position, Color color, Transform parent)
        {
            var root = new GameObject($"Lamppost_{position.x:F0}_{position.z:F0}");
            root.transform.position = position;

            Material lampMat = CreateUnlitMaterial(color);

            // Pole: cylinder with radius 0.05, height 3.0
            // Unity cylinder: radius 0.5, height 2 at scale (1,1,1)
            // To get radius 0.05: scaleX = scaleZ = 0.05 / 0.5 = 0.1
            // To get height 3.0: scaleY = 3.0 / 2.0 = 1.5
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.SetParent(root.transform);
            pole.transform.localScale = new Vector3(0.1f, 1.5f, 0.1f);
            // Center at half height: 3.0 / 2 = 1.5
            pole.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            var poleRenderer = pole.GetComponent<MeshRenderer>();
            poleRenderer.sharedMaterial = lampMat;

            // Remove pole's default collider
            Object.DestroyImmediate(pole.GetComponent<Collider>());

            // Light sphere: radius 0.15 at top of pole (Y=3.0)
            // Unity sphere: radius 0.5 at scale (1,1,1)
            // To get radius 0.15: scale = 0.15 / 0.5 = 0.3
            var light = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            light.name = "Light";
            light.transform.SetParent(root.transform);
            light.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            light.transform.localPosition = new Vector3(0f, 3.0f, 0f);

            var lightRenderer = light.GetComponent<MeshRenderer>();
            lightRenderer.sharedMaterial = lampMat;

            // Remove light's default collider
            Object.DestroyImmediate(light.GetComponent<Collider>());

            // CapsuleCollider on root encompassing geometry
            // From Y=0 to Y=3.15 (sphere top = 3.0 + 0.15)
            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 1.575f, 0f);
            capsule.height = 3.15f;
            capsule.radius = 0.15f;

            int layer = GetCityGeometryLayer();
            SetLayerRecursive(root, layer);

            if (parent != null)
                root.transform.SetParent(parent);

            return root;
        }

        /// <summary>
        /// Creates a bench: single flattened cube (1.2x0.4x0.5) raised 0.2 above surface.
        /// BoxCollider included by default with cube primitive.
        /// </summary>
        public static GameObject CreateBench(Vector3 position, float yRotation, Color color, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Bench_{position.x:F0}_{position.z:F0}";

            go.transform.localScale = new Vector3(1.2f, 0.4f, 0.5f);
            // Raised 0.2 above surface, center at 0.2 + 0.4/2 = 0.4
            go.transform.position = new Vector3(position.x, 0.4f, position.z);
            go.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateUnlitMaterial(color);

            // Cube primitive already has BoxCollider
            int layer = GetCityGeometryLayer();
            go.layer = layer;

            if (parent != null)
                go.transform.SetParent(parent);

            return go;
        }

        /// <summary>
        /// Creates a trash can: single cylinder (radius 0.2, height 0.6) on surface.
        /// CapsuleCollider on the object.
        /// </summary>
        public static GameObject CreateTrashCan(Vector3 position, Color color, Transform parent)
        {
            // Unity cylinder: radius 0.5, height 2 at scale (1,1,1)
            // To get radius 0.2: scaleX = scaleZ = 0.2 / 0.5 = 0.4
            // To get height 0.6: scaleY = 0.6 / 2.0 = 0.3
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = $"TrashCan_{position.x:F0}_{position.z:F0}";

            go.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
            // Center at half height: 0.6 / 2 = 0.3
            go.transform.position = new Vector3(position.x, 0.3f, position.z);

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateUnlitMaterial(color);

            // Remove default CapsuleCollider that comes with cylinder primitive
            Object.DestroyImmediate(go.GetComponent<Collider>());

            // Add CapsuleCollider sized to match
            var capsule = go.AddComponent<CapsuleCollider>();
            capsule.center = Vector3.zero;
            capsule.radius = 0.2f;
            capsule.height = 0.6f;

            int layer = GetCityGeometryLayer();
            go.layer = layer;

            if (parent != null)
                go.transform.SetParent(parent);

            return go;
        }

        /// <summary>
        /// Creates an NPC capsule primitive with CapsuleCollider (radius 0.5, height 2.0).
        /// Named as specified. Color applied.
        /// </summary>
        public static GameObject CreateNPCCapsule(Vector3 position, Color color, string name, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;

            // Unity capsule at scale (1,1,1): radius 0.5, height 2.0 — matches spec exactly
            go.transform.position = new Vector3(position.x, 1.0f, position.z);

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateUnlitMaterial(color);

            // Capsule primitive already comes with a CapsuleCollider (radius 0.5, height 2.0)
            int layer = GetCityGeometryLayer();
            go.layer = layer;

            if (parent != null)
                go.transform.SetParent(parent);

            return go;
        }
    }
}
