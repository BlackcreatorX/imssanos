using UnityEngine;

public class DocumentManager : MonoBehaviour {
    public GameObject[] documentPrefabs; // 1 por tipo
    public Transform spawnParent;
    [Range(0f, 1f)] public float errorChance = 0.5f;

    public int documentsToGenerate = 5;

    void Start() {
        GenerateDocuments(documentsToGenerate);
    }

    void GenerateDocuments(int count) {
        for (int i = 0; i < count; i++) {
            DocumentType randomType = (DocumentType)Random.Range(0, 4);
            GameObject doc = Instantiate(documentPrefabs[(int)randomType], spawnParent);

            DocumentBehavior behavior = doc.GetComponent<DocumentBehavior>();
           // behavior.Init(randomType, Random.value > errorChance); // true = correcto
        }
    }
}
