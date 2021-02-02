using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGroupManager : MonoBehaviour
{
    private List<float> highestValues = new List<float>();

    private int numberOfSubbedGraphs = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public int Subscribe()
    {
        int ID = numberOfSubbedGraphs;

        highestValues.Add(0);
        
        numberOfSubbedGraphs++;
        return ID;
        
    }

    public void GiveHighestValue(float value, int ID)
    {
        highestValues[ID] = value;
    }

    public float GetHighestValue()
    {
        float highest = 0;
        for (int i = 0; i < highestValues.Count; i++)
        {
            if (highestValues[i] > highest)
                highest = highestValues[i];
        }

        return Mathf.Max(highest, 0.01f);
    }

    public int GetSubscriptions()
    {
        return numberOfSubbedGraphs;
    }
}
