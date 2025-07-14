using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    private Camera mainCamera;
    private GameObject selectedObject;
    private Vector3 offset;

    [Header("Puntaje y Energía")]
    public int Puntuacion = 0;
    public int energia = 100;
    public int EnClick = 2;

    [Header("Documentos")]
    public GameObject[] documentPrefabs;
    public Transform spawnParent;
    [Range(0f, 1f)] public float errorChance = 0.5f;
    public int documentsToGenerate = 5;
    public TipoArchivo tipoActual = TipoArchivo.Rojo;

    [Header("Comida")]
    private bool enEspera = false;
    private float tiempoEsperaRestante = 0f;

    [Header("Temporizador")]
    public float duracion = 60f;
    private float tiempoRestante;
    private float intervaloMensaje = 15f;
    private float siguienteMensaje;
    private bool temporizadorActivo = false;

    [Header("Cajas")]
    public GameObject cajasRojo;
    public GameObject cajasVerde;
    public GameObject cajasAzul;
    public GameObject cajasAmarillo;

    [Header("UI")]
    public GameObject BotonesUI;
    public bool MostrarBotonesB = false;
    public bool PacienteActivo = false;
    public GameObject npc;
    public TextMeshProUGUI energiaText;
    public TextMeshProUGUI ColorDoc;

    void Start()
    {
        mainCamera = Camera.main;
        Debug.Log("Energía inicial: " + energia);
        IniciarTemporizador(duracion);
    }

    void Update()
    {
        if (enEspera)
        {
            tiempoEsperaRestante -= Time.deltaTime;
            if (tiempoEsperaRestante <= 0)
            {
                enEspera = false;
                Debug.Log("✅ Espera terminada. Puedes volver a jugar.");
            }
            return;
        }

        // Tecla Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (MostrarBotonesB)
                OcultarBotones();
            else
                MostrarBotones();
        }

        if (Input.GetMouseButtonDown(0)) OnLeftClick();
        if (Input.GetMouseButtonDown(1)) OnRightClick();

        HandleObjectDragging();
        CheckEnergia();
        TemporizadorUpdate();
        ActualizarEnergiaUI();
    }

    void OnLeftClick()
    {
        DetectarComida();
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider == null) return;
        string tag = hit.collider.tag;

        if (tag == "Aceptar")
        {
            if (!PacienteActivo) ActivarPaciente();
            else DesactivarPaciente("urgencias");
        }
        else if (tag == "Denegar")
        {
            if (PacienteActivo) DesactivarPaciente("alta");
            else Debug.Log("No hay paciente activo");
        }
    }

    void OnRightClick()
    {
        if (enEspera)
        {
            Debug.Log("❗ No puedes hacer clic derecho mientras comes.");
            return;
        }

        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider == null) return;

        string tag = hit.collider.tag;
        string nombre = hit.collider.gameObject.name;

        if (tag == "TDoc" || hit.collider.transform.parent?.CompareTag("TDoc") == true)
        {
            Debug.Log("CLICK DERECHO sobre documento: " + nombre);
            GastarEnergia(EnClick);
        }
        else if (EsCajaValida(tag))
        {
            Debug.Log("CLICK DERECHO sobre caja: " + nombre);
            GastarEnergia(EnClick);
        }
    }

    void ActivarPaciente()
    {
        PacienteActivo = true;
        npc.SetActive(true);
        Debug.Log("Paciente activo");
        tipoActual = (TipoArchivo)Random.Range(0, 4);
        Debug.Log("Tipo de archivo generado: " + tipoActual);
        GenerateDocuments(documentsToGenerate);
        ColorDeArchivo();
    }

    void DesactivarPaciente(string motivo)
    {
        PacienteActivo = false;
        npc.SetActive(false);
        Debug.Log($"Paciente fue ingresado a {motivo}");
        DestruirTodosLosDocumentos();
    }

    void GenerateDocuments(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject doc = Instantiate(documentPrefabs[(int)tipoActual], spawnParent);
            DocumentBehavior behavior = doc.GetComponent<DocumentBehavior>();
            behavior.Init(tipoActual, Random.value > errorChance, (int)tipoActual);
        }
    }

    void HandleObjectDragging()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("TDoc"))
            {
                selectedObject = hit.collider.gameObject;
                offset = selectedObject.transform.position - mainCamera.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        if (Input.GetMouseButton(0) && selectedObject != null)
        {
            Vector3 newPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition) + offset;
            newPosition.z = 0;
            selectedObject.transform.position = newPosition;
        }

        if (Input.GetMouseButtonUp(0) && selectedObject != null)
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                GameObject caja = hit.collider.gameObject;

                if (EsCajaValida(caja.tag))
                    VerificarCaja(caja);
                else
                    Debug.Log("❗ Documento soltado fuera de una caja.");
            }
            else
            {
                Debug.Log("❗ Documento soltado fuera de cualquier objeto.");
            }

            selectedObject = null;
        }
    }

    void VerificarCaja(GameObject caja)
    {
        if (selectedObject == null) return;

        string tagEsperado = "Caja" + tipoActual.ToString();
        bool enCajaCorrecta = caja.CompareTag(tagEsperado);

        Debug.Log(enCajaCorrecta ? "✅ Caja correcta" : "❌ Caja incorrecta");
        Destroy(selectedObject);
    }

    void DetectarComida()
    {
        if (enEspera) return;

        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Comida"))
            Comer(hit.collider.gameObject);
    }

    void Comer(GameObject comida)
    {
        energia = Mathf.Clamp(energia + 50, 0, 100);
        Debug.Log("🍽️ Comiendo... Energía recuperada: " + energia);
        enEspera = true;
        tiempoEsperaRestante = 10f;
    }

    void CheckEnergia()
    {
        if (energia <= 0)
            Debug.Log("Energía agotada. Se debe de comer.");
    }

    void GastarEnergia(int cantidad)
    {
        energia = Mathf.Clamp(energia - cantidad, 0, 100);
        Debug.Log("Energía restante: " + energia);
    }

    void ActualizarEnergiaUI()
    {
        energiaText?.SetText("Energía: " + energia);
    }

    void IniciarTemporizador(float tiempo)
    {
        duracion = tiempo;
        tiempoRestante = tiempo;
        siguienteMensaje = tiempo - intervaloMensaje;
        temporizadorActivo = true;
    }

    void TemporizadorUpdate()
    {
        if (!temporizadorActivo || tiempoRestante <= 0) return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= siguienteMensaje && tiempoRestante > 0)
        {
            Debug.Log("Tiempo restante: " + Mathf.Ceil(tiempoRestante) + " segundos");
            energia = Mathf.Clamp(energia - 2, 0, 100);
            Debug.Log("Energía actual: " + energia);
            siguienteMensaje -= intervaloMensaje;
        }

        if (tiempoRestante <= 0)
        {
            Debug.Log("¡El tiempo se ha acabado!");
            temporizadorActivo = false;
        }
    }

    void ColorDeArchivo()
    {
        if (ColorDoc != null)
            ColorDoc.text = tipoActual.ToString();
    }

    public void MostrarBotones()
    {
        BotonesUI?.SetActive(true);
        MostrarBotonesB = true;
        Debug.Log("Botones de UI mostrados");
    }

    public void OcultarBotones()
    {
        BotonesUI?.SetActive(false);
        MostrarBotonesB = false;
        Debug.Log("Botones de UI ocultos");
    }

    void DestruirTodosLosDocumentos()
    {
        foreach (Transform child in spawnParent)
        {
            Destroy(child.gameObject);
        }
    }

    bool EsCajaValida(string tag)
    {
        return tag == "CajaRojo" || tag == "CajaVerde" || tag == "CajaAzul" || tag == "CajaAmarillo";
    }
}

public enum TipoArchivo
{
    Rojo,
    Verde,
    Azul,
    Amarillo
}

/*using UnityEngine;
using TMPro; // ← Asegúrate de tener esto arriba


public class GameManager : MonoBehaviour
{

    private Camera mainCamera;
    private GameObject selectedObject;
    private Vector3 offset;
    public int Puntuacion = 0;

    [Header("Documentos")]
    public GameObject[] documentPrefabs;
    public Transform spawnParent;
    [Range(0f, 1f)] public float errorChance = 0.5f;
    public int documentsToGenerate = 5;
   

    [Header("Energía")]
    public int energia = 100;
    public int EnClick = 2; // Energía gastada por clic derecho

    [Header("Comida")]
    private bool enEspera = false;
    private float tiempoEsperaRestante = 0f;


    [Header("Temporizador")]
    public float duracion = 60f;
    private float tiempoRestante;
    private float intervaloMensaje = 15f;
    private float siguienteMensaje;
    private bool temporizadorActivo = false;
 

    
    [Header("CajarsArchivos")]
    public GameObject cajasRojo;
    public GameObject cajasVerde;
    public GameObject cajasAzul;
    public GameObject cajasAmarillo;
    public int TipoDeArchivo; // 0 = Rojo, 1 = Verde, 2 = Azul, 3 = Amarillo

    [Header("Botones de UI")]
    public GameObject BotonesUI;
    public bool MostrarBotonesB = false;
    public bool PacienteActivo = false;
    public GameObject npc;
    [Header("UI - Energía")]
    public TextMeshProUGUI energiaText;
    public TextMeshProUGUI ColorDoc;





    void Start()
    {
        mainCamera = Camera.main;
        Debug.Log("Energía inicial: " + energia);
        IniciarTemporizador(duracion);

    }

   void Update()
    {
      if (Input.GetKeyDown(KeyCode.Tab) & MostrarBotonesB==false) // Detecta si se presionó la tecla Tab
        {
            MostrarBotones(); // Llama a la función para mostrar los botones
           
        }
        else if (Input.GetKeyDown(KeyCode.Tab) & MostrarBotonesB==true) // Detecta si se presionó la tecla Escape
        {
            OcultarBotones(); // Llama a la función para ocultar los botones
           
        }
    if (enEspera)
    {
        tiempoEsperaRestante -= Time.deltaTime;
        if (tiempoEsperaRestante <= 0)
        {
            enEspera = false;
            Debug.Log("✅ Espera terminada. Puedes volver a jugar.");
        }
        return; // ← Bloquea acciones
    }

    HandleObjectDragging();
    CheckEnergia();
    TemporizadorUpdate();
    DetectarClickDerecho();
    DetectarComida();
    ActualizarEnergiaUI();


   if (Input.GetMouseButtonDown(0))
{
    Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

    if (hit.collider != null)
    {
        string hitTag = hit.collider.tag;

        if (hitTag == "Aceptar")
        {
            if (!PacienteActivo)
            {
                PacienteActivo = true;
                Debug.Log("Paciente activo");
                GenerateDocuments(documentsToGenerate);
                TipoDeArchivo = Random.Range(0, 4);
                Debug.Log("Tipo de archivo generado: " + TipoDeArchivo);
                npc.SetActive(true);
                ColorDeArchivo();
                
                
            }
            else
            {
                PacienteActivo = false;
                npc.SetActive(false);
                Debug.Log("Paciente fue ingresado a urgencias");
                DestruirTodosLosDocumentos();
            }
        }
        else if (hitTag == "Denegar")
        {
            if (PacienteActivo)
            {
                PacienteActivo = false;
                npc.SetActive(false);
                Debug.Log("Paciente fue dado de alta");
                Debug.Log("Energía restante: " + energia);
                DestruirTodosLosDocumentos();
            }
            else
            {
                Debug.Log("No hay paciente activo");
            }
        }
    }
}


    }

    // =========================
    // Generación de documentos
    // =========================
    void GenerateDocuments(int count)
    {   
    for (int i = 0; i < count; i++)
    {
        DocumentType tipoEnum = (DocumentType)TipoDeArchivo;
        GameObject doc = Instantiate(documentPrefabs[TipoDeArchivo], spawnParent);

        DocumentBehavior behavior = doc.GetComponent<DocumentBehavior>();
        behavior.Init(tipoEnum, Random.value > errorChance, TipoDeArchivo); // <- aquí se pasa el tipo elegido
    }
    }

    // ====================
    // Arrastrar documentos
    // ====================
   void HandleObjectDragging()
{
    if (Input.GetMouseButtonDown(0))
    {
        RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null && hit.collider.CompareTag("TDoc"))
        {
            selectedObject = hit.collider.gameObject;
            offset = selectedObject.transform.position - mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    if (Input.GetMouseButton(0) && selectedObject != null)
    {
        Vector3 newPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition) + offset;
        newPosition.z = 0;
        selectedObject.transform.position = newPosition;
    }

    if (Input.GetMouseButtonUp(0) && selectedObject != null)
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            GameObject caja = hit.collider.gameObject;

            if (caja.CompareTag("CajaRojo") || caja.CompareTag("CajaVerde") ||
                caja.CompareTag("CajaAzul") || caja.CompareTag("CajaAmarillo"))
            {
                VerificarCaja(caja);
            }
            else
            {
                Debug.Log("❗ Documento soltado fuera de una caja.");
            }
        }
        else
        {
            Debug.Log("❗ Documento soltado fuera de cualquier objeto.");
        }

        selectedObject = null;
    }
}


    // ====================
    // Energía del jugador
    // ====================
    void CheckEnergia()
    {
        if (energia <= 0)
        {
            Debug.Log("Energía agotada. Se debe de comer.");
        }
    }

    public void Comida()
    {
        energia += 50;
        Debug.Log("Comiendo... Energía aumentada a: " + energia);
    }

    public void GastarEnergia(int cantidad)
    {
        energia -= cantidad;
        if (energia < 0) energia = 0;
        Debug.Log("Energía restante: " + energia);
    }
    void DetectarClickDerecho()
{
    if (!Input.GetMouseButtonDown(1)) return;
    if (enEspera)
    {
        Debug.Log("❗ No puedes hacer clic derecho mientras comes.");
        return;
    }

    Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

    if (hit.collider == null) return;

    string tag = hit.collider.tag;
    string nombre = hit.collider.gameObject.name;

    if (tag == "TDoc" || hit.collider.transform.parent?.CompareTag("TDoc") == true)
    {
        Debug.Log("CLICK DERECHO sobre documento: " + nombre);
        GastarEnergia(EnClick);
    }
    else if (EsCajaValida(tag))
    {
        Debug.Log("CLICK DERECHO sobre caja: " + nombre);
        GastarEnergia(EnClick);
    }
}

    void ActualizarEnergiaUI()
{
    if (energiaText != null)
        energiaText.text = "Energía: " + energia;
}

    // ====================
    // Comer comida
    // ====================
    public void Comer(GameObject comida)
    {
        if (enEspera) return;

        // opcional: si quieres que desaparezca
        energia += 50;
        if (energia > 100) energia = 100;

        Debug.Log("🍽️ Comiendo... Energía recuperada: " + energia);

        enEspera = true;
        tiempoEsperaRestante = 10f;

        // Aquí puedes disparar una animación si tienes un Animator
        // animator.SetTrigger("Esperando");
    }
    void DetectarComida()
{
    if (enEspera || !Input.GetMouseButtonDown(0)) return;

    Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

    if (hit.collider != null && hit.collider.CompareTag("Comida"))
    {
        Comer(hit.collider.gameObject);
    }
}

    // ====================
    // Temporizador
    // ====================
    void IniciarTemporizador(float tiempo)
    {
        duracion = tiempo;
        tiempoRestante = tiempo;
        siguienteMensaje = tiempo - intervaloMensaje;
        temporizadorActivo = true;
    }

    void TemporizadorUpdate()
    {
        if (!temporizadorActivo) return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= siguienteMensaje && tiempoRestante > 0)
        {
            Debug.Log("Tiempo restante: " + Mathf.Ceil(tiempoRestante) + " segundos");
            energia -= 2;
            if (energia < 0) energia = 0;
            Debug.Log("Energía actual: " + energia);
            siguienteMensaje -= intervaloMensaje;
        }

        if (tiempoRestante <= 0)
        {
            Debug.Log("¡El tiempo se ha acabado!");
            temporizadorActivo = false;
        }
    }

    // ====================
    // Cajas de archivos
    // ====================

    void VerificarCaja(GameObject caja)
{
    if (selectedObject == null) return;

    var doc = selectedObject.GetComponent<DocumentBehavior>();
    bool enCajaCorrecta = false;

    if (caja.CompareTag("CajaRojo") && TipoDeArchivo == 0) enCajaCorrecta = true;
    else if (caja.CompareTag("CajaVerde") && TipoDeArchivo == 1) enCajaCorrecta = true;
    else if (caja.CompareTag("CajaAzul") && TipoDeArchivo == 2) enCajaCorrecta = true;
    else if (caja.CompareTag("CajaAmarillo") && TipoDeArchivo == 3) enCajaCorrecta = true;

    if (enCajaCorrecta)
        Debug.Log("✅ Caja correcta");
    else
        Debug.Log("❌ Caja incorrecta");
    Destroy(selectedObject);
 
}
    bool EsCajaValida(string tag)
    {
        return tag == "CajaRojo" || tag == "CajaVerde" || tag == "CajaAzul" || tag == "CajaAmarillo";
    }
       void ColorDeArchivo()
{
    if (ColorDoc != null)
        switch (TipoDeArchivo)
        {
            case 0:
                ColorDoc.text = "Rojo";
                break;
            case 1:
                ColorDoc.text = "Verde";
                break;
            case 2:
                ColorDoc.text = "Azul";
                break;
            case 3:
                ColorDoc.text = "Amarillo";
                break;
            default:
                ColorDoc.text = "Desconocido";
                break;
        }
}

    // ====================
    // Botones de UI
    // ====================
    public void MostrarBotones()
    {
        BotonesUI.SetActive(true);
        MostrarBotonesB = true;
        Debug.Log("Botones de UI mostrados");
    }
    public void OcultarBotones()
    {
        BotonesUI.SetActive(false);
        MostrarBotonesB = false;
        Debug.Log("Botones de UI ocultos");
    }

void DestruirTodosLosDocumentos()
{
    foreach (Transform child in spawnParent)
    {
        Destroy(child.gameObject);
    }
}



}*/
