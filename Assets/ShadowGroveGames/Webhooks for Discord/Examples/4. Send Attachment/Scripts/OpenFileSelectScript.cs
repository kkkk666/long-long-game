#if (UNITY_EDITOR)
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.UI;

namespace ShadowGroveGames.WebhooksForDiscord.Examples.SendAttachment
{
    public class OpenFileSelectScript : MonoBehaviour
    {
        [SerializeField]
        private InputField _filePathInput;

        public void OpenFileSelect()
        {
#if (UNITY_EDITOR)
            // This only work in editor. To implement a file explorer ingame checkout https://github.com/yasirkula/UnitySimpleFileBrowser
            _filePathInput.text = EditorUtility.OpenFilePanel("Select Attachment", "", "");
#else
            Debug.LogError("This only work in editor. To implement a file explorer ingame checkout https://github.com/yasirkula/UnitySimpleFileBrowser");
#endif
        }
    }
}
