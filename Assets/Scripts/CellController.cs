using UnityEngine;
using System.Collections.Generic;

public class CellController : MonoBehaviour {

    //markers
    public List<AuxinController> myAuxins;

    //agents who passed through this cell
    public List<GameObject> agentsDensity;

    //number of agents
    public int qntAgents;
    //paint heatMaps?
    public bool paintHeatMap;

    //thermical confort variables
    //air temperature (ta)
    public float airTemperature;
    //room it belongs
    public GameObject room;
    //agents who are inside it
    public List<GameObject> agents;
    //any source of heat
    public GameObject heatSource;
    //if cell is part of wall
    public bool isWall;
    //wall value (0 = wall, c = normal)
    public float wallFilter;
    //if cell is a door
    public bool isDoor;

    //cell name
    public string cellName
    {
        set
        {
            name = value;
            cellCache[name] = gameObject;
        }
    }
    //static structure to easier find cells by name
    private static Dictionary<string, GameObject> cellCache = new Dictionary<string, GameObject>();
    
    public static GameObject GetCellByName(string idName)
    {
        if (cellCache.ContainsKey(idName))
        {
            return cellCache[idName];
        }
        else
        {
            //Debug.LogWarning(idName + ": Cell not found!!!");
            return null;
        }
    }

    //structure to store all paths from this cell
    /*private SerializableDictionary<string, List<GameObject>> cellPath = new SerializableDictionary<string, List<GameObject>>();
    public void SetPath(string idName, List<GameObject> pathi)
    {
        cellPath[idName] = pathi;
    }
    public List<GameObject> GetCellPathByName(string idName)
    {
        if (cellPath.ContainsKey(idName))
        {
            return cellPath[idName];
        }
        else
        {
            //Debug.LogWarning(idName + ": Cell not found!!!");
            return new List<GameObject>();
        }
    }*/

    private void Awake()
    {
        agentsDensity = new List<GameObject>();

        agents = new List<GameObject>();
        // heat diffusivity(air diffusivity = 1.9 × 10 - 5)
        if (!isWall)
        {
            wallFilter = 0.019f;
        }
    }

    private void Start()
    {
        room = transform.parent.gameObject;
        airTemperature = transform.parent.gameObject.GetComponent<RoomController>().airTemperature;
    }

    private void Update()
    {
        //if heat map, use it
        if (paintHeatMap)
        {
            //update heatmap
            if (agentsDensity.Count > qntAgents * 0.8f)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("CrowdedDensity") as Material;
            }
            else if (agentsDensity.Count > qntAgents * 0.6f)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("HighDensity") as Material;
            }
            else if (agentsDensity.Count > qntAgents * 0.4f)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("MediumDensity") as Material;
            }
            else if (agentsDensity.Count > qntAgents * 0.2f)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("LowDensity") as Material;
            }
            else
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("DesertDensity") as Material;
            }
        }//otherwise, use the thermal
        else //if it is not wall
        if (!isWall)
        {
            //ResetCell();

            //update its material according its temperature
            if (airTemperature < 10)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("Materials/Freezing") as Material;
            }
            else if (airTemperature >= 10 && airTemperature < 18)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("Materials/Cold") as Material;
            }
            else if (airTemperature >= 18 && airTemperature < 25)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("Materials/Normal") as Material;
            }
            else if (airTemperature >= 25 && airTemperature < 29)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("Materials/Hot") as Material;
            }
            else if (airTemperature >= 29)
            {
                GetComponent<Renderer>().sharedMaterial = Resources.Load("Materials/Hell") as Material;
            }
        }
    }

    public void StartList()
    {
        myAuxins = new List<AuxinController>();
        StartAgentList();
    }

    public void StartAgentList()
    {
        agents = new List<GameObject>();
    }

    //add a new auxin on myAuxins
    public void AddAuxin(AuxinController auxin)
    {
        myAuxins.Add(auxin);
    }

    //return all auxins in this cell
    public List<AuxinController> GetAuxins() {
        return myAuxins;     
    }
}
