using UnityEngine;
using Wenjian.Client.Net;

namespace Wenjian.Client.Presentation
{
    public sealed class RogueDebugController : MonoBehaviour
    {
        [SerializeField]
        private FirstChainHudController hudController;

        [SerializeField]
        private int rogueId = 4001;

        public FirstChainHudController HudController
        {
            get => hudController;
            set => hudController = value;
        }

        public int RogueId
        {
            get => rogueId;
            set => rogueId = Mathf.Max(1, value);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                TryStartRogue();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                TryFinishRogue();
            }
        }

        public bool TryStartRogue()
        {
            return hudController != null && hudController.QueueStartRogueCommand(rogueId);
        }

        public bool TryFinishRogue()
        {
            return hudController != null && hudController.QueueFinishRogueCommand();
        }
    }
}
