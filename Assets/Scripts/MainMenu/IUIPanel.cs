using System.Collections;

namespace UI.MainMenu
{
    public interface IUIPanel
    {
        public IEnumerator InitBehavior()
        {
            yield return null;
        }

        public void EndBehavior()
        {
        
        }
    
        public void Back()
        {
        
        }
    }
}
