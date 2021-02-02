using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Car : MonoBehaviour
{

    // GUI
    public Slider RPMGuage;
    public TextMeshProUGUI gearText;
    public TextMeshProUGUI rpmText;
    public TextMeshProUGUI speedText;

    
    // Forces
    private Vector3 tractionForce;
    private Vector3 dragForce;
    private Vector3 netForce;

    // Physics
    private Vector3 velocity;
    
    // Engine
    private float rpm = 1000;

    private float maxRPM = 6000;
    
    public AnimationCurve torqueCurve;

    private float throttlePosition = 0f;
    
    // Gearbox & diff
    [SerializeField] private float[] gearRatio = {3.1f, 1.9f, 1.3f, 1.0f, 0.8f};
    private int gear = 1;
    private float diffRatio = 3.1f;
    
    private float transmissionEfficiency = 0.7f;

    private Vector3 maxGearVelocity;

    // Wheels
    private float wheelAngularVelocity;
    private float traction = 1.0f; // full traction assumed for now
    
    private float wheelRadius = 0.3f;
    private float wheelMass = 12;


    private float tractionTorque;
    private float driveTorque;
    private float totalTorque;

    private float maxWheelAngularVelocity;
    
    // Car Body
    private float coefficientOfFriction = 0.3f;
    private float frontalArea = 2.2f;
    private float dragConstant;
    
    // Environment
    private float airDensity = 1.29f;

    private Rigidbody rb;
    
    // Modifiers
    [SerializeField] private float dragModifier; 

    
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        dragConstant = 0.5f * airDensity * coefficientOfFriction * frontalArea * dragModifier;
        
        maxWheelAngularVelocity = 2 * Mathf.PI * maxRPM / (60 * gearRatio[gear - 1] * diffRatio);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
            throttlePosition = 1.0f;
        //rpm += 30; // this value is directly related to how powerful the engine is. will need tweaking per vehicle. maybe find a relationship between a hp value and this number
        else
            throttlePosition = 0.0f;
        //rpm -= 30;

        
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // check we have a gear before we shift into it
            // gear is NOT indexed, but a real value. Length will return 5 if there are 5 gears
            if (gear + 1 <= gearRatio.Length)
            {
                Debug.Log(gearRatio.Length.ToString());
                string logString = "Gear: " + gear.ToString() + " -> " + (gear + 1).ToString() + " == RPM: " +
                                   rpm.ToString();

                // update the gear, clamped to how many gears we have
                gear = Mathf.Min(gear + 1, gearRatio.Length);

                // update our rpm to match the rpm the engine would be at the speed the wheels are spinning
                // gear-1 because gear is not an index but a real value for what gear the vehicle is in
                rpm = wheelAngularVelocity / gearRatio[gear-1] / diffRatio * (60 / 2 * Mathf.PI) / 10;

                logString += " -> " + rpm.ToString();
                Debug.Log(logString);
                
                // calculate the maximum velocity possible in this gear
                maxWheelAngularVelocity = 2 * Mathf.PI * maxRPM / (60 * gearRatio[gear - 1] * diffRatio);
            }
            else
            {
                Debug.Log("At top gear");
            }
        }
        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            if (gear - 1 > 0)
            {
                string logString = "Gear: " + gear.ToString() + " -> " + (gear - 1).ToString() + " == RPM: " +
                                   rpm.ToString();

                gear = Mathf.Max(gear - 1, 1);

                rpm = wheelAngularVelocity / gearRatio[gear-1] / diffRatio * (60 / 2 * Mathf.PI) / 10;

                logString += " -> " + rpm.ToString();
                Debug.Log(logString);
                
                maxWheelAngularVelocity = 2 * Mathf.PI * maxRPM / (60 * gearRatio[gear - 1] * diffRatio);
            }
            else
            {
                Debug.Log("In first gear");
            }
        }
        
        
        UpdateUI();
    }

    private void FixedUpdate()
    {
        //Debug.Log("Start of physics frame");

        // calculate rpm
        Debug.Log(wheelAngularVelocity + "*" + gearRatio[gear-1] +"*"+ diffRatio +"*"+ 60 +"/"+ 2 +"*"+ Mathf.PI);
        rpm = (wheelAngularVelocity * gearRatio[gear-1] * diffRatio * 60) / (2 * Mathf.PI);

        //rpm = velocity.z * 60 * gearRatio[gear - 1] * diffRatio / (2 * Mathf.PI * wheelRadius);
        Debug.Log(rpm);
        
        // clamp rpm
        if (rpm < 1000)
            rpm = 1000;
        else
        {
            rpm = Mathf.Min(rpm, maxRPM);
        }
        

        //tractionForce = transform.forward * (wheelAngularVelocity * traction);
        tractionForce.z = wheelAngularVelocity * traction * throttlePosition;
        
        // Calculate drag on the car
        dragForce = velocity * (-dragConstant * velocity.magnitude);
        
        //Debug.Log(rpm.ToString()+ " -> " + (wheelAngularVelocity * gearRatio[gear - 1] * diffRatio * (60 / 2 * Mathf.PI) / 10).ToString());

        // calculate total force on car
        netForce = (tractionForce) + dragForce;

        Vector3 acceleration = netForce / rb.mass;

        velocity = velocity + Time.deltaTime * acceleration;

        transform.position += Time.deltaTime * velocity;

        
        
        // set our rpm to what our speed tells us it is
        // /10 because the values returned were 10 times larger than they should have been
        
        
        // clamp our velocity to what our gear tops out at
        // unsure if this can be done without the need for outside input
        
        // calculate traction torque
        tractionTorque = tractionForce.magnitude * wheelRadius;

        float inertia = wheelMass * wheelRadius * wheelRadius / 2;
        
        Debug.Log(rpm + "/" + maxRPM);
        Debug.Log(torqueCurve.Evaluate(rpm).ToString() +  "*" + throttlePosition.ToString() + "*" + diffRatio + "*" +
                  transmissionEfficiency + "*" + gearRatio[gear-1]);
        //                                  engine Torque
        driveTorque = (torqueCurve.Evaluate(rpm) * throttlePosition) * diffRatio *
                            transmissionEfficiency * gearRatio[gear-1];
        totalTorque = driveTorque + tractionTorque;


        float angularAcceleration = totalTorque / inertia;

        wheelAngularVelocity += angularAcceleration * Time.deltaTime;
        
        
        maxWheelAngularVelocity = 2 * Mathf.PI * rpm / (60 * gearRatio[gear - 1] * diffRatio);
        
        // clamp wheelAngularVelocity
        wheelAngularVelocity = Mathf.Min(wheelAngularVelocity, maxWheelAngularVelocity);

        
        // calculate wheel angular velocity from the engine RPM, gear ratio and diff Ratio
        //wheelAngularVelocity = 2 * Mathf.PI * rpm / (60 * gearRatio[gear - 1] * diffRatio);
        Debug.Log(wheelAngularVelocity);

    }


    private void UpdateUI()
    {
        if (RPMGuage)
            RPMGuage.value = rpm / maxRPM;

        if (gearText)
            gearText.text = gear.ToString();

        if (rpmText)
            rpmText.text = rpm.ToString();
        
        if (speedText)
            speedText.text = velocity.magnitude.ToString();
    }
}