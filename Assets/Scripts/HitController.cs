using UnityEngine;

public class HitController : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;

    private void Awake()
    {
        inputReader.ChangeActionMap(InputReader.ActionMapType.Player);

        inputReader.LeftButton  += () => OnHit(NoteInputType.LB);
        inputReader.LeftTrigger += () => OnHit(NoteInputType.LT);
        inputReader.RightButton += () => OnHit(NoteInputType.RB);
        inputReader.RightTrigger+= () => OnHit(NoteInputType.RT);
    }

    private void OnHit(NoteInputType type)
    {
        // Pregunta al NoteTargetRegistry si hay una nota en evaluación de este tipo
        NoteBehavior note = NoteTargetRegistry.Instance.CurrentlyEvaluating(type);
        if (note != null)
        {
            note.ReceiveInput(type);
        }
    }
}