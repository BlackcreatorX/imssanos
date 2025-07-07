using UnityEngine;

public class DocumentBehavior : MonoBehaviour {
    public DocumentType type;
    public bool isCorrect;
    public int tipoArchivoAsignado; // 0 = Rojo, 1 = Verde, 2 = Azul, 3 = Amarillo

    [SerializeField] public GameObject[] b1; // Campos del documento (interactivos)
    [SerializeField] public GameObject[] b2; // Otra capa visual opcional (sin cambios de color)

    public string errorField;

 public void Init(DocumentType docType, bool isCorrectDoc, int tipoArchivo)
    {
    this.type = docType;
    this.isCorrect = isCorrectDoc;
    this.tipoArchivoAsignado = tipoArchivo;

        if (!isCorrect) {
            // Seleccionar aleatoriamente un campo de b1 con error
            int errorIndex = Random.Range(0, b1.Length);
            for (int i = 0; i < b1.Length; i++) {
                var sr = b1[i].GetComponent<SpriteRenderer>();
                if (sr != null) {
                    if (i == errorIndex) {
                        sr.color = Color.red;
                        errorField = b1[i].name;
                    } else {
                        sr.color = Color.green;
                    }
                }
            }
        } else {
            // Si es correcto, todos los campos de b1 en verde
            foreach (var field in b1) {
                var sr = field.GetComponent<SpriteRenderer>();
                if (sr != null) {
                    sr.color = Color.green;
                }
            }
        }
    }

    void Update() {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Clic izquierdo
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.transform.IsChildOf(transform)) {
                string fieldName = hit.collider.gameObject.name;
               // Debug.Log("Campo clicado con IZQUIERDO: " + fieldName);

                if (!isCorrect && fieldName == errorField) {
                   // Debug.Log("Error detectado en: " + fieldName);
                    //Debug.Log("aqui se debe de mostrar el error");
                }
            }
        }

        // Clic derecho
        if (Input.GetMouseButtonDown(1)) {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null) {
                // Verifica si se hizo clic en este documento o alguno de sus hijos
                if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)) {
                    string fieldName = hit.collider.gameObject.name;
                   // Debug.Log("CLICK DERECHO sobre: " + fieldName);
                }
            }
        }
    }
}
