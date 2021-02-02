using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class WindowGraph : MonoBehaviour
{

    [SerializeField] private Sprite circleSprite;
    private RectTransform graphContainer;

    private List<float> valueList = new List<float>();

    private List<GameObject> graphObjects = new List<GameObject>();

    private int maxValues = 50;

    public Color lineColor = new Color(0, 0, 0, 0.5f);

    public GraphGroupManager groupManager;
    private int ID = 0;

    private Image background;
    private void Awake()
    {
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();

        for (int i = 0; i < maxValues; i++)
        {
            valueList.Add(0);
        }

        //if(valueList.Count > 1)
        //    ShowGraph(valueList);

        if (groupManager)
        {
            ID = groupManager.Subscribe();

            background = transform.GetChild(0).GetChild(0).GetComponent<Image>();

            if (groupManager.GetSubscriptions() > 1)
                background.enabled = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame


    private void ShowGraph(List<float> valueList)
    {
        float graphHeight = Mathf.Abs(graphContainer.GetComponent<RectTransform>().rect.height);

        float graphWidth = Mathf.Abs(graphContainer.GetComponent<RectTransform>().rect.width);

        float yMax = 0;
        if (groupManager)
            yMax = groupManager.GetHighestValue();
        else
            yMax = Mathf.Max(GetLargestValue(valueList), 0.01f); 
        
        float xSize = graphWidth / valueList.Count;
        
        if(test == 0)
            Debug.Log(graphWidth + "/" + valueList.Count + "=" + xSize);
        
        GameObject prevPoint = null;
        for (int i = 0; i < valueList.Count; i++)
        {
            float xPosition = xSize + i * xSize;
            float yPosition = (valueList[i] / yMax) * graphHeight;
            
            GameObject currPoint = CreateCircle(new Vector2(xPosition, yPosition));

            if (prevPoint != null)
            {
                CreateDotConnection(prevPoint.GetComponent<RectTransform>().anchoredPosition, currPoint.GetComponent<RectTransform>().anchoredPosition);
            }

            prevPoint = currPoint;

            //https://www.youtube.com/watch?v=CmU5-v-v1Qo
        }
    }
    
    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(1, 1);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        graphObjects.Add(gameObject);
        
        return gameObject;
    }

    private int test = 1;
    private void CreateDotConnection(Vector2 pointA, Vector2 pointB)
    {
        GameObject gameObject = new GameObject("Connection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);

        gameObject.GetComponent<Image>().color = lineColor;
        
        Vector2 dir = (pointB - pointA).normalized;
        float distance = Vector2.Distance(pointA, pointB);
        
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(distance, 3); // sizeDelta here is ugly and will only work when all anchors are in the same place
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        if (test == 0)
        {
            Debug.Log(pointA);
            Debug.Log(pointB);
            Debug.Log(dir);
            Debug.Log(distance);
        }

        rectTransform.anchoredPosition = pointA + dir * distance * 0.5f;

        rectTransform.localEulerAngles = new Vector3(0, 0, GetAngle(pointA, pointB));

        graphObjects.Add(gameObject);

        if (test == 0)
            test = 1;
    }

    private float GetLargestValue(List<float> list)
    {
        float highestValue = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] > highestValue)
                highestValue = list[i];
        }

        return highestValue;
    }
    
    // TODO: Add to radiator games util
    public float GetAngle(Vector2 me, Vector2 target)
    {
        return (float) (Mathf.Atan2(target.y - me.y, target.x - me.x) * (180 / Mathf.PI));
    }

    private void ClearGraph()
    {
        int graphObjectCount = graphObjects.Count;
        for (int i = 0; i < graphObjectCount; i++)
        {
            Destroy(graphObjects[0]);

            graphObjects.RemoveAt(0);
        }
    }

    private int valueIndex = 0;
    public void AddValue(float value)
    {
        if(test == 0)
            Debug.Log(valueIndex % 50 + ":" + valueList.Count);
        
        valueList[valueIndex % 50] = value;
        
        ClearGraph();
        ShowGraph(valueList);
        
        if(groupManager)
            groupManager.GiveHighestValue(GetLargestValue(valueList), ID);

        valueIndex++;
    }
}
