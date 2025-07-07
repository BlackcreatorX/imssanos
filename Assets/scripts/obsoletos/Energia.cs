using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Energia : MonoBehaviour
{
    public int energia = 100; // Energía inicial del objeto (ahora es una variable de clase)

    void Start()
    {
        // Muestra la energía inicial del objeto
        Debug.Log("Energía inicial: " + energia);
    }
	void Update()
	{
		if (energia <= 0)
        {
            Debug.Log("Energía agotada se debe de comer");

            // Aquí puedes agregar lógica adicional para manejar el estado del objeto cuando se queda sin energía
        }
        else
        {
         // Debug.Log("Energía restante: " + energia);
        }
	}

    public void Comida()
    {
        energia += 50; // Aumenta la energía al comer
        Debug.Log("Comiendo... Energía aumentada a: " + energia);
        Debug.Log("Energía después de comer: " + energia);
    }
}
