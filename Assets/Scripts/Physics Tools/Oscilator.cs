using UnityEngine;
using HeavyDutyInspector;
using SpiderWeb;

/// <summary>
/// Can oscilate translation & rotation, using a sine wave.
/// </summary>
public class Oscilator : MonoBehaviour {

    public bool updateUnscaledTime;
    public bool localAxis = false;

	public bool oscRotation;
    [HideConditional(true, "oscRotation", true)]
	public float rotationAmt = 10;
    [HideConditional(true, "oscRotation", true)]
    public Vector3 rotationAxis = new Vector3(0, 0, 1);
	public bool oscTranslation;
    [HideConditional(true, "oscTranslation", true)]
    public Vector3 oscTranslateAxis;
    [Space]
	public float frequency = 1;
    public bool randomOffset;
    [HideConditional(true, "randomOffset", true)]
    public float maxOffset = 2;
    [HideConditional(true, "randomOffset", true)]
    public float minOffset = 0;

    [HideConditional(true, "randomOffset", false)]
    public float offset = 0;

    public bool constantSpeed;

    float t = 0;
	float input;
	float initRotZ;
	Vector3 initLocalPos;
    Vector3 initLocalRot;

    // Use this for initialization
    void Start () {

        initLocalRot = transform.localEulerAngles;
		initLocalPos = transform.localPosition;

        if ( randomOffset ) offset = Random.Range(minOffset, maxOffset);
	}

    /// <summary>
    /// Resets the time so it will start off at the center point of the wave when this is called.
    /// </summary>
    public void ResetTime()
    {
        t = -offset;
    }
	
	// Update is called once per frame
	void Update() {

        
        if ( updateUnscaledTime ) t += Time.unscaledDeltaTime;
        else t += Time.deltaTime;

        input = Mathf.Sin((t + offset) * frequency);

        if (constantSpeed)
        {
            if (input > 0) input = Time.deltaTime;
            else input = -Time.deltaTime;

            transform.Translate(input * oscTranslateAxis);

            return;
        }

		//oscilate rotation
		if (oscRotation) {

            Vector3 additionalRot = input * rotationAmt * rotationAxis;

            Vector3 finalEuler = initLocalRot + additionalRot;

			transform.localEulerAngles = finalEuler;
		}

		//oscilate position
		if (oscTranslation)
        {
            Vector3 posOutput = (input * oscTranslateAxis) + initLocalPos;
            if (localAxis) posOutput = transform.rotation * posOutput;
            transform.localPosition = posOutput;
		}

	}
}
