using UnityEngine;

public class TreeColliderGenerator : MonoBehaviour
{
    public Terrain terrain;
    public GameObject colliderPrefab; // Boş bir obje veya basit bir kapsül prefab

    void Start()
    {
        TerrainData data = terrain.terrainData;

        foreach (TreeInstance tree in data.treeInstances)
        {
            Vector3 worldPos = Vector3.Scale(tree.position, data.size) + terrain.transform.position;
            GameObject col = Instantiate(colliderPrefab, worldPos, Quaternion.identity);
            col.transform.localScale = Vector3.one * 2f; // Çarpışma boyutu ayarı
        }
    }
}
