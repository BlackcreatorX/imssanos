using UnityEngine;

public class RelojTemporizador : MonoBehaviour
{
    public float duracion = 60f; // Duración del temporizador en segundos
    private float tiempoRestante;
    private float intervaloMensaje = 15f;
    private float siguienteMensaje;
    private bool enMarcha = false;

    [SerializeField] private GameObject levelController; // Objeto que tiene el script Energia
    private Energia energiaScript; // Referencia al script Energia

    void Start()
    {
        // Obtener el script Energia desde LevelController
        if (levelController != null)
        {
            energiaScript = levelController.GetComponent<Energia>();

            if (energiaScript == null)
            {
                Debug.LogError("El objeto LevelController no tiene el script Energia asignado.");
            }
        }
        else
        {
            Debug.LogError("LevelController no está asignado en el Inspector.");
        }

        IniciarTemporizador(duracion);
    }

    public void IniciarTemporizador(float tiempo)
    {
        duracion = tiempo;
        tiempoRestante = tiempo;
        siguienteMensaje = tiempo - intervaloMensaje;
        enMarcha = true;
    }

    void Update()
    {
        if (enMarcha)
        {
            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante <= siguienteMensaje && tiempoRestante > 0)
            {
                Debug.Log("Tiempo restante: " + Mathf.Ceil(tiempoRestante) + " segundos");

                // Reducir la energía si el script está asignado
                if (energiaScript != null)
                {
                    energiaScript.energia -= 2;
                     Debug.Log("Energía inicial: " + energiaScript.energia);
                }

                siguienteMensaje -= intervaloMensaje;
            }

            if (tiempoRestante <= 0)
            {
                Debug.Log("¡El tiempo se ha acabado!");
                enMarcha = false;
            }
        }
    }
}
