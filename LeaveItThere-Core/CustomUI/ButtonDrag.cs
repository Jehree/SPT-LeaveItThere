using EFT.UI.Ragfair;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LeaveItThere.CustomUI
{
    public class ButtonDrag : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        private RectTransform _draggableRect;
        private Vector2 _clickPos;

        /// <summary>
        /// Component should be added to the UI element that needs to be clicked in order to initiate dragging (like a button)
        /// </summary>
        /// <param name="rectToDrag">Rect that actually gets dragged</param>
        public void Init(RectTransform rectToDrag)
        {
            _draggableRect = rectToDrag;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // cache pointer pos on button down to calculate offset
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _draggableRect,
                eventData.position,
                eventData.pressEventCamera,
                out _clickPos
            );
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _draggableRect,
                eventData.position,
                eventData.pressEventCamera,
                out position
            );

            _draggableRect.position = _draggableRect.transform.TransformPoint(position - _clickPos);

            // somehow (I don't know how, just copied what BSG did lol) this clamps the menu to the screen bounds
            _draggableRect.CorrectPositionResolution();
        }
    }
}
