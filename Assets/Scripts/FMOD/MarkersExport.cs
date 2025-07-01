using System;

[Serializable]
public class MarkerExport
{
    public EventMarkers[] events;
}

[Serializable]
public class EventMarkers
{
    public string eventPath;
    public MarkerData[] markers;
}

[Serializable]
public class MarkerData
{
    public string name;
    public float time; // tiempo en segundos (ya que así lo exportas desde FMOD)
}