using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    private Vector3 acceleration = new Vector3(0, 0, 0.1f);
    private Vector3 finalAcceleration;

    private CarBase carBase;

    [SerializeField] private float diameter;
    private const float magicAVConstant = 9.549297f;
    private float wheelRPM;

    [SerializeField] private float surfaceFriction = 0.9f;

    private float maxTractionForce;
    private float tractionForce;

    private float driveForce;

    private const float gravity = 9.81f;
    
    

    // Start is called before the first frame update
    void Start()
    {
        carBase = transform.parent.GetComponent<CarBase>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    public float GetTractionForce(float torque, float RPM, float speed, float massOnWheel)
    {
        float RadS = speed / (2 * Mathf.PI * (diameter / 2));
        //Debug.Log(RadS + "=" + speed + "," + diameter);
        wheelRPM = RadS * magicAVConstant;

        if (wheelRPM == 0 && torque == 0)
        {
            return 0;
        }
        
        maxTractionForce = surfaceFriction * massOnWheel * gravity;

        
        
        // = (engine torque * gearing efficiency (1) / wheel radius) * (engine RPM / wheel RPM)
        tractionForce = (torque / (diameter / 2)) * (RPM / wheelRPM);
        //Debug.Log(torque + "," + diameter + "," + RPM + "," + wheelRPM);
        
        //Debug.Log(tractionForce);

        driveForce = torque / (diameter / 2);
        
        return driveForce;
    }

    public float GetRPM()
    {
        return wheelRPM;
    }
}