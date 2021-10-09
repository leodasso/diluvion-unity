using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using HeathenEngineering.Events;

namespace HeathenEngineering.UIX
{
    public class EventHelper : EventTrigger, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IScrollHandler, IUpdateSelectedHandler, ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler
    {
        /// <summary>
        /// Occures when the mouse enters the control
        /// </summary>
        public event RoutedEvent<PointerEventData> onPointerEnter;
        /// <summary>
        /// Occures when the mouse exits the control
        /// </summary>
        public event RoutedEvent<PointerEventData> onPointerExit;
        /// <summary>
        /// Occures when the mouse is within the control and the left click is down
        /// </summary>
        public event RoutedEvent<PointerEventData> onPointerDown;
        /// <summary>
        /// Occures when the mouse is within the control and the left click is released
        /// </summary>
        public event RoutedEvent<PointerEventData> onPointerUp;
        /// <summary>
        /// Occures when the mouse is within the control and the left click is pressed then released
        /// </summary>
        public event RoutedEvent<PointerEventData> onPointerClick;
        /// <summary>
        /// Occures when just before the start of a drag event
        /// </summary>
        public event RoutedEvent<PointerEventData> onBeginDrag;
        /// <summary>
        /// Occures when the mouse is dragging (left click is down and mouse is moving) within the control
        /// </summary>
        public event RoutedEvent<PointerEventData> onDrag;
        /// <summary>
        /// Occures when the drag event finishes
        /// </summary>
        public event RoutedEvent<PointerEventData> onEndDrag;
        /// <summary>
        /// Occures when the mouse left click is released after an onDrag event
        /// </summary>
        public event RoutedEvent<PointerEventData> onDrop;
        /// <summary>
        /// Occures when the mouse is within the control and the scrollwhell is moved
        /// </summary>
        public event RoutedEvent<PointerEventData> onScroll;
        /// <summary>
        /// Occures when the selected state changes
        /// </summary>
        public event RoutedEvent<BaseEventData> onUpdateSelected;
        /// <summary>
        /// Occures when the control is selected e.g. gains focus
        /// </summary>
        public event RoutedEvent<BaseEventData> onSelect;
        /// <summary>
        /// Occures when the control is deselected e.g. losses focus
        /// </summary>
        public event RoutedEvent<BaseEventData> onDeselect;
        /// <summary>
        /// Occures when a move
        /// </summary>
        public event RoutedEvent<AxisEventData> onMove;
        /// <summary>
        /// Occures on submit
        /// </summary>
        public event RoutedEvent<BaseEventData> onSubmit;
        /// <summary>
        /// Occures on cancel
        /// </summary>
        public event RoutedEvent<BaseEventData> onCancel;
        public event RoutedEvent<PointerEventData> onInitializePotentialDrag;

        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnPointerEnter(PointerEventData eventData)
        {
            if (onPointerEnter != null)
                onPointerEnter(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnPointerExit(PointerEventData eventData)
        {
            if (onPointerExit != null)
                onPointerExit(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnPointerDown(PointerEventData eventData)
        {
            if (onPointerDown != null)
                onPointerDown(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnPointerUp(PointerEventData eventData)
        {
            if (onPointerUp != null)
                onPointerUp(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnPointerClick(PointerEventData eventData)
        {
            if (onPointerClick != null)
                onPointerClick(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null)
                onDrag(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnDrop(PointerEventData eventData)
        {
            if (onDrop != null)
                onDrop(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnScroll(PointerEventData eventData)
        {
            if (onScroll != null)
                onScroll(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnUpdateSelected(BaseEventData eventData)
        {
            if (onUpdateSelected != null)
                onUpdateSelected(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnSelect(BaseEventData eventData)
        {
            if (onSelect != null)
                onSelect(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnMove(AxisEventData eventData)
        {
            if (onMove != null)
                onMove(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag != null)
                onBeginDrag(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag != null)
                onEndDrag(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnDeselect(BaseEventData eventData)
        {
            if (onDeselect != null)
                onDeselect(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnSubmit(BaseEventData eventData)
        {
            if (onSubmit != null)
                onSubmit(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnCancel(BaseEventData eventData)
        {
            if (onCancel != null)
                onCancel(gameObject, eventData);
        }
        /// <summary>
        /// Fires the event; this does not simulate the action that normaly fires the event only causes the listeners to be issued the corasponding message
        /// </summary>
        /// <param name="eventData"></param>
        new public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (onInitializePotentialDrag != null)
                onInitializePotentialDrag(gameObject, eventData);
        }
    }
}