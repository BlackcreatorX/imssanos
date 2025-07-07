public enum DocumentType { ID, Seguro, Pasaporte, Licencia }

public class DocumentData {
    public DocumentType type;
    public bool hasError;
    public string  errorField; // nombre del campo con error
}
