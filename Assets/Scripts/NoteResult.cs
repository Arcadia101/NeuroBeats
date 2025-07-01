/// <summary>
/// Almacena la información de la evaluación de una nota,
/// para luego exportarla o procesarla en estadísticas.
/// </summary>
[System.Serializable]
public class NoteResult
{
    public string markerName;        // Nombre del marcador en FMOD
    public float targetTime;         // Tiempo objetivo en el que debería pulsar (s)
    public float hitTime;            // Tiempo real en que el jugador pulsó (s)
    public string inputReceived;     // Tipo de input que el jugador pulsó
    public string result;            // "Perfect", "Good" o "Miss"
    public float evaluationDuration; // Duración desde que comenzó la evaluación hasta el resultado (s)
    public string side;             // "Left" o "Right"
}