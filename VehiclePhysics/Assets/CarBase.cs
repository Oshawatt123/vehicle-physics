using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class CarBase : MonoBehaviour
{

    [SerializeField] private List<Wheel> wheels;
    private Vector3 wheelAcceleration;
    private Vector3 drivingForce;
    
    

    private Vector3 velocity;

    [Header("Gamey stuff")] 
    [SerializeField] private float stopSpeed;

    [Header("General")]
    [SerializeField] private float mass;

    private const float gravity = 9.81f;

    [SerializeField] private float RPMFalloff;
    [SerializeField] private float RPMGain;
    
    [Header("Engine")]
    public AnimationCurve torqueCurve;

    private float maxTorque;
    private float actualTorque;
    
    private float throttlePosition;

    [SerializeField] private float idleRPM = 800;
    private float RPM;
    private float maxRPM; // defined from torqueCurve

    [Header("Drag")]
    [SerializeField] private float coefficientOfFriction = 0.3f;
    
    private Vector3 dragForce;
    private float dragConstant;

    private float airDensity = 1.29f;
    [SerializeField] private float frontalArea = 2.2f;

    [Header("Roll resistance")]
    [SerializeField] private float tirePressure = 2.9f;
    
    private Vector3 rollingResistance;
    private float rollResistanceConstant; // this exists for the pure reason of avoiding one massive line of code :[


    private Vector3 netForce;

    // Start is called before the first frame update
    void Start()
    {
        // start at idle
        RPM = idleRPM;
        
        dragConstant = 0.5f * airDensity * coefficientOfFriction * frontalArea;
        rollResistanceConstant = 0.005f + (1 / tirePressure) * (0.01f + (0.0095f));
        maxRPM = torqueCurve[torqueCurve.length-1].time;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        throttlePosition = Input.GetAxis("Vertical");

        maxTorque = torqueCurve.Evaluate(RPM);
        actualTorque = maxTorque * throttlePosition;
        
        
        // calculate all forces from wheels, and convert that to a vector in local space
        float wheelForce = 0;
        
        // get force from all wheels combined
        foreach (Wheel wheel in wheels)
        {
            wheelForce += wheel.GetTractionForce(actualTorque, RPM, velocity.magnitude, mass / 4);
        }

        drivingForce = (wheelForce * transform.forward);
        
        
        // calculate air resistance
        dragForce = velocity * (-dragConstant * velocity.magnitude);

        // calculare rolling resistance
        rollingResistance = ((rollResistanceConstant * Mathf.Pow(velocity.magnitude / 100f, 2.0f)) * mass * gravity) * -transform.forward;

        // sum forces to get net
        netForce = drivingForce + dragForce + rollingResistance;
        
        // add velocity
        velocity += netForce / mass;
        Debug.Log("Velocity before clamping:" + velocity);

        // stop car if it is rolling slow enough, only checked if car is moving otherwise creates some jank when launching
        if (velocity.magnitude != 0)
        {
            if (velocity.magnitude < stopSpeed && velocity.magnitude > -stopSpeed)
            {
                velocity = Vector3.zero;
            }
        }

        Vector3 translation = velocity * Time.deltaTime; // make sure we are true to frame rate

        // translation
        transform.position = new Vector3(transform.position.x + translation.x, transform.position.y + translation.y, transform.position.z + translation.z);
        
        // feedback

        RPM = wheels[0].GetRPM();
        Debug.Log("RPM derived from wheels:" + RPM);

        if (Input.GetAxis("Vertical") != 0)
            RPM += Mathf.Abs(Input.GetAxis("Vertical")) * RPMGain;
        else
            RPM -= RPMFalloff;

        // idle & redline
        if (RPM < idleRPM)
            RPM = idleRPM;
        if (RPM > maxRPM)
            RPM = maxRPM;

    }
    
}