using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleMovement : MonoBehaviour
{
    
    // constants
    private float airResistanceConstant = 0;
    private float RollResistanceConstant = 0;
    
    public float EngineForce;
    private float engineForce;

    public float BrakingPower;
    private float brakingPower;

    // variables

    private Vector3 traction;

    public float coefficientOfFriction = 0.3f;
    private float airDensity = 1.29f;
    private float frontalArea = 2.2f;
    private Vector3 airResistance;

    public float rollingResistanceModifier = 10;
    private Vector3 rollingResistance;

    private Vector3 brakingForce;

    private Vector3 longitudinalForce;

    private Vector3 driveForce;
    
    public AnimationCurve torqueCurve;

    private float engineTorque;
    public float maxTorque = 450;
    public float[] gearRatio = {3.1f, 1.9f, 1.3f, 1.0f, 0.8f};
    private int gear = 1;
    private float diffRatio = 3.1f;
    private float transmissionEfficiency = 0.7f;
    private float wheelRadius = 0.34f;

    private float rpm = 0;
    private float maxRPM = 6000;

    private float axleRPM;

    private float clutchEngagement = 0;

    private float driveTorque;

    private float wheelRotation;
    
    // Components
    private Rigidbody rb;

    private Rigidbody RR_Wheel;
    
    // Externals
    public WindowGraph speedGraph;
    public WindowGraph accelGraph;
    public WindowGraph RRGraph;
    public WindowGraph dragGraph;
    public WindowGraph torqueGraph;
    public WindowGraph finalGraph;

    private Vector3 prevVelocity;
    private Vector3 acceleration = Vector3.zero;

    public TextMeshProUGUI RPM_Text;

    // Visuals
    public Transform carBody;

    private Vector3 resistiveForces;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        InvokeRepeating("AddValueToGraph", 0f, 0.3f);

        airResistanceConstant = 0.5f * airDensity * coefficientOfFriction * frontalArea;
        RollResistanceConstant = airResistanceConstant * 30;

        RR_Wheel = GameObject.Find("RR Wheel").GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Input
        if (Input.GetKey(KeyCode.W))
        {
            rpm += 5;
            clutchEngagement = Mathf.Min(clutchEngagement + 0.1f, 1);
            
            
        }
        else
        {
            rpm -= 5;
            clutchEngagement = 0;
        }

        // braking, but only when we're moving!
        if (Input.GetKey(KeyCode.S) && rb.velocity.magnitude > 0)
            brakingPower = BrakingPower;
        else
            brakingPower = 0;


        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0, -1, 0, Space.World);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0, 1, 0, Space.World);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            gear = Mathf.Min(gear + 1, gearRatio.Length);
            
        }
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            gear = Mathf.Max(gear - 1, 1);
        }

        RPM_Text.text = rpm.ToString();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if (rpm < 1000)
            rpm = 1000;
        if (rpm > maxRPM)
            rpm = maxRPM;

        axleRPM = rpm / gearRatio[gear - 1] / diffRatio;
        
        // Calculating straight-line force

        float maxEngineTorque = torqueCurve.Evaluate(rpm / maxRPM);
        
        engineTorque = (maxEngineTorque * maxTorque) * clutchEngagement; // * throttle position
        driveTorque = (engineTorque * diffRatio *
                     transmissionEfficiency * gearRatio[gear-1]);
        
        driveForce = driveTorque / wheelRadius * transform.forward;


        //traction = transform.forward * engineForce;

        airResistance = -rb.velocity * rb.velocity.magnitude * airResistanceConstant;

        rollingResistance = RollResistanceConstant * -rb.velocity;

        brakingForce = transform.forward * -1 * brakingPower;

        resistiveForces = airResistance + rollingResistance + brakingForce;

        longitudinalForce = driveForce + airResistance + rollingResistance + brakingForce;
        
        //rb.AddRelativeForce(longitudinalForce, ForceMode.Force);
        rb.AddForce(longitudinalForce, ForceMode.Force);


        // a = dV/dT
        acceleration = (rb.velocity - prevVelocity) / Time.deltaTime;

        prevVelocity = rb.velocity;

        Debug.Log(acceleration.magnitude);
        // Weight Transfer
        carBody.eulerAngles = new Vector3(-acceleration.z, carBody.eulerAngles.y, carBody.eulerAngles.z);

        RR_Wheel.AddRelativeTorque(new Vector3(0, driveTorque, 0));
    }
    
    
    void AddValueToGraph()
    {
        if (speedGraph)
        {
            speedGraph.AddValue(rb.velocity.magnitude);
        }

        if (accelGraph)
        {
            accelGraph.AddValue(acceleration.magnitude);
        }

        if (RRGraph)
        {
            RRGraph.AddValue(rollingResistance.magnitude);
        }

        if (dragGraph)
        {
            dragGraph.AddValue(airResistance.magnitude);
        }

        if (torqueGraph)
        {
            torqueGraph.AddValue(driveForce.magnitude);
        }

        if (finalGraph)
        {
            finalGraph.AddValue(driveForce.magnitude - airResistance.magnitude - rollingResistance.magnitude - brakingForce.magnitude);
        }
    }

    private void OnDrawGizmos()
    {
        rb = GetComponent<Rigidbody>();
        Gizmos.DrawIcon(transform.position + rb.centerOfMass,"Circle.png", true, Color.blue);

        float divValue = 100;

        Vector3 adjustedPosition = new Vector3(transform.position.x, transform.position.y + 5, transform.position.z);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(adjustedPosition, adjustedPosition + (driveForce / divValue));
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(adjustedPosition, adjustedPosition + (resistiveForces / divValue));
    }
}