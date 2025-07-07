using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class Patient
{
    public string initialPhrase;
    public string[] detailedQuestions;
    public string[] directQuestions;
    public string[] ignoreResponses;
    public string consequence;
}

[System.Serializable]
public class PatientList
{
    public List<Patient> patients;
}

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI patientText;
    public Button detailedButton;
    public Button directButton;
    public Button ignoreButton;

    private List<Patient> patients = new List<Patient>();
    private int currentPatientIndex = 0;
    private bool decisionMade = false;
    public float decisionTime = 10f;

    void Start()
    {
        LoadPatientsFromJSON();
        SetupButtons();
        if (patients.Count > 0)
        {
            StartDialogue();
        }
    }

    void LoadPatientsFromJSON()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("patients");
        if (jsonFile != null)
        {
            PatientList loadedData = JsonUtility.FromJson<PatientList>(jsonFile.text);
            patients = loadedData.patients;
            Debug.Log("✔️ Pacientes cargados: " + patients.Count);
        }
        else
        {
            Debug.LogError("❌ Archivo patients.json no encontrado en Resources.");
        }
    }

    void SetupButtons()
    {
        detailedButton.onClick.AddListener(ChooseDetailedQuestion);
        directButton.onClick.AddListener(ChooseDirectQuestion);
        ignoreButton.onClick.AddListener(ChooseIgnore);
    }

    void StartDialogue()
    {
        decisionMade = false;

        patientText.text = "🗣️ " + patients[currentPatientIndex].initialPhrase;

        StartCoroutine(DecisionTimer());
    }

    IEnumerator DecisionTimer()
    {
        yield return new WaitForSeconds(decisionTime);
        if (!decisionMade)
        {
            patientText.text = "⏱️ No decidiste a tiempo. Paciente enviado a espera.";
            Invoke(nameof(NextPatient), 2f);
        }
    }

    public void ChooseDetailedQuestion()
    {
        if (decisionMade) return;

        string question = GetRandom(patients[currentPatientIndex].detailedQuestions);
        ShowResult("🔍 " + question, patients[currentPatientIndex].consequence);
    }

    public void ChooseDirectQuestion()
    {
        if (decisionMade) return;

        string question = GetRandom(patients[currentPatientIndex].directQuestions);
        ShowResult("⚡ " + question, patients[currentPatientIndex].consequence);
    }

    public void ChooseIgnore()
    {
        if (decisionMade) return;

        string response = GetRandom(patients[currentPatientIndex].ignoreResponses);
        ShowResult("🚪 " + response, patients[currentPatientIndex].consequence);
    }

    void ShowResult(string answer, string consequence)
    {
        patientText.text = answer + "\n\n📜 " + consequence;
        decisionMade = true;
        Invoke(nameof(NextPatient), 3f);
    }

    void NextPatient()
    {
        currentPatientIndex++;
        if (currentPatientIndex < patients.Count)
        {
            StartDialogue();
        }
        else
        {
            patientText.text = "🏥 Todos los pacientes fueron atendidos.";
        }
    }

    string GetRandom(string[] options)
    {
        return options[Random.Range(0, options.Length)];
    }
}
