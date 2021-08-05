using UnityEngine;
using UnityEngine.EventSystems;

public class PanelDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler {

	[SerializeField] RectTransform panelToDrag;
	Vector2 cursorOffset;
	Vector2 pivotOffset;
	RectTransform canvasRectTransform;
	Vector3[] canvasCorners = new Vector3[4];
	Vector3[] panelCorners = new Vector3[4];
	float panelWidth;
	float panelHeight;

	void Start () {
		canvasRectTransform = GetComponentInParent<Canvas> ().transform as RectTransform;
		canvasRectTransform.GetWorldCorners (canvasCorners);
		panelToDrag.GetWorldCorners (panelCorners);
		panelWidth = panelCorners [2].x - panelCorners [0].x;
		panelHeight = panelCorners [2].y - panelCorners [0].y;
		Vector2 midPoint = new Vector2 (panelCorners [0].x + panelWidth/2, panelCorners[0].y + panelHeight/2);
		pivotOffset = (Vector2)panelToDrag.position - midPoint;
	}

	#region IBeginDragHandler implementation

	public void OnBeginDrag (PointerEventData eventData)
	{
		cursorOffset = (Vector2)panelToDrag.position - pivotOffset - eventData.position;
	}

	#endregion

	#region IDragHandler implementation

	public void OnDrag (PointerEventData eventData)
	{
		Vector2 pos = eventData.position + cursorOffset;

		pos.x = Mathf.Clamp (pos.x, canvasCorners[0].x + panelWidth/2, canvasCorners[2].x - panelWidth/2);
		pos.y = Mathf.Clamp (pos.y, canvasCorners[0].y + panelHeight/2, canvasCorners[2].y - panelHeight/2);

		panelToDrag.position = pos + pivotOffset;
	}

	#endregion
}
