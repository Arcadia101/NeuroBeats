using UnityEngine;

public class PlayerInputListener : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;

    private void Awake()
    {
        inputReader.EnableGameplayInput();

        inputReader.Direction += OnMove;
        inputReader.LeftTrigger += () => Debug.Log("LT pressed");
        inputReader.LeftButton += () => Debug.Log("LB pressed");
        inputReader.RightTrigger += () => Debug.Log("RT pressed");
        inputReader.RightButton += () => Debug.Log("RB pressed");
    }

    private void OnDestroy()
    {
        inputReader.Direction -= OnMove;
        inputReader.LeftTrigger -= () => Debug.Log("LT pressed");
        inputReader.LeftButton -= () => Debug.Log("LB pressed");
        inputReader.RightTrigger -= () => Debug.Log("RT pressed");
        inputReader.RightButton -= () => Debug.Log("RB pressed");

        inputReader.DisableGameplayInput();
    }

    private void OnMove(Vector2 dir)
    {
        Debug.Log("Moving: " + dir);
    }
}