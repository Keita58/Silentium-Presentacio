using UnityEngine;

public class AnimationEvents : MonoBehaviour
{

    public void OnHyeroglyphicAnimationEnd()
    {
        PuzzleManager.instance.HieroglyphicPuzzleExitAnimation2();
    }
    
    public void OnHyeroglyphicAnimationEnd2()
    {
        PuzzleManager.instance.HieroglyphicPuzzleExit(true);
    }
}
