using UnityEngine;
using System.Collections;
using PathologicalGames;

public class AddDirectionalForce : MonoBehaviour {

	public Vector3 force;
	public Vector3 torque;
	public Vector3 oscillateForce;
	Vector3 actualOscillateForce;
	public float oscillateSpeed = 1;
	float oscNumber = 0;
	public bool local = false;
    public bool onSpawned = false;
	public bool onAwake = false;
	public bool randomTorque = false;
	public bool randomForce = false;

    void RandomExplosiveForce()
    {

        if (!GetComponent<Rigidbody>()) return;                  
        //explosive force
        GetComponent<Rigidbody>().AddRelativeForce(force);
        GetComponent<Rigidbody>().AddRelativeTorque(torque);           
        
    }


    // Use this for initialization
    void Start () {

        RandomExplosiveForce();
        if (onAwake&&!onSpawned)
            RandomExplosiveForce();

    }
	
    void RollForce()
    {
      
            if (randomTorque)
            {
                torque = Random.Range(-1f, 1f) * torque;
            }

            if (randomForce)
            {
                force = Random.Range(-1f, 1f) * force;
            }
    }


    void OnSpawned()
    {
        RollForce();      
        if (onSpawned&&!onAwake)
            RandomExplosiveForce();
    }

	// Update is called once per frame
	void FixedUpdate () {

		if (GetComponent<Rigidbody>()) {

			if (!onAwake&&!onSpawned) {

				if (local) {

					GetComponent<Rigidbody>().AddRelativeForce((force + actualOscillateForce) * Time.deltaTime);

				}else {

					GetComponent<Rigidbody>().AddForce((force + actualOscillateForce) * Time.deltaTime);
				}
			}
		}
	}

	void Update() {

		oscNumber = Mathf.Sin(Time.time * oscillateSpeed);

		actualOscillateForce = oscillateForce * oscNumber;

	}
}
