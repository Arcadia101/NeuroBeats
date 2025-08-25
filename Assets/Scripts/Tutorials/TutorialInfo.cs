using UnityEngine;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "TutorialInfo", menuName = "Tutorials/TutorialInfo")]
public class TutorialInfo : ScriptableObject
{
    public Sprite Right1;
    public Sprite Right2;
    public Sprite Left1;
    public Sprite Left2;
    public Sprite StartButton;
    public Sprite BackButton;
    public Sprite SubmitButton;
    public Sprite PreviousButton;
    public Sprite NextButton;
    
    public VideoClip[] videoClips;
    [TextArea(5,10)]
    public string[] textInfo;
}
