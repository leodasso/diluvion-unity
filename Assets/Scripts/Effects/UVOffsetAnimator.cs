using UnityEngine;
using System.Collections;

public class UVOffsetAnimator : MonoBehaviour {

	public float offsetSpeed;
	public enum OffsetAxis { X = 0, Y = 1}
	public OffsetAxis axis = OffsetAxis.X;
	public bool useMaxOffset = false;
	public float maxOffset = 10;

	private float curOffset;
	private bool increasing = true;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if (useMaxOffset){
			if (increasing){
				curOffset += (Time.deltaTime * offsetSpeed);

				if (curOffset >= maxOffset){
					increasing = false;
				}
			}
			else {
				curOffset -= (Time.deltaTime * offsetSpeed);
				
				if (curOffset <= -maxOffset){
					increasing = true;
				}

			}
		}
		else {
			curOffset += (Time.deltaTime * offsetSpeed);
		}
	
		if (axis == OffsetAxis.X) {
			gameObject.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(curOffset, 0);
		}
		else if (axis == OffsetAxis.Y) {
			gameObject.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0, curOffset);
		}

	}
}
