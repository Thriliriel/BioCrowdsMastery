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
    //is hot?
    public bool isHot;
    //is cold?
    public bool isCold;
    //if cell is part of wall
    public bool isWall;
    //wall value (0 = wall, c = normal)
    public float wallFilter;
    //if cell is a door
    public bool isDoor;

    //D* higher and lower cost, to use for tempereture and density
    //public bool higher, lower;

    //game controller
    private GameController gameController;
    //textures
    static private List<Material> loadedMaterials;
    //old air temperature, to be able to notice change moment
    public float oldAirTemperature;

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

        //higher = lower = isHot = isCold = false;
        isHot = isCold = false;
        gameController = GameObject.Find("GameController").GetComponent<GameController>();

        loadedMaterials = new List<Material>();

        loadedMaterials.Add(Resources.Load("Materials/Freezing") as Material);
        loadedMaterials.Add(Resources.Load("Materials/Cold") as Material);
        loadedMaterials.Add(Resources.Load("Materials/Normal") as Material);
        loadedMaterials.Add(Resources.Load("Materials/Hot") as Material);
        loadedMaterials.Add(Resources.Load("Materials/Hell") as Material);
    }

    private void Start()
    {
        room = transform.parent.gameObject;
        airTemperature = oldAirTemperature = transform.parent.gameObject.GetComponent<RoomController>().airTemperature;

        if (airTemperature < 10)
        {
            GetComponent<Renderer>().sharedMaterial = loadedMaterials[0];
        }
        else if (airTemperature >= 10 && airTemperature < 18)
        {
            GetComponent<Renderer>().sharedMaterial = loadedMaterials[1];
        }
        else if (airTemperature >= 18 && airTemperature < 25)
        {
            GetComponent<Renderer>().sharedMaterial = loadedMaterials[2];
        }
        else if (airTemperature >= 25 && airTemperature < 29)
        {
            GetComponent<Renderer>().sharedMaterial = loadedMaterials[3];
        }
        else if (airTemperature >= 29)
        {
            GetComponent<Renderer>().sharedMaterial = loadedMaterials[4];
        }
    }

    private void Update()
    {
        bool higher = false;
        bool lower = false;

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
        else //if it is not wall and using thermal comfort
        if (!isWall && gameController.thermalComfort)
        {
            //ResetCell();

            //update its material according its temperature
            if (airTemperature < 10.0f && oldAirTemperature >= 10.0f)
            {
                GetComponent<Renderer>().sharedMaterial = loadedMaterials[0];

                //higher cost
                higher = true;
                lower = false;
            }
            else if (airTemperature >= 10.0f && airTemperature < 18.0f && (oldAirTemperature < 10.0f || oldAirTemperature >= 18.0f))
            {
                GetComponent<Renderer>().sharedMaterial = loadedMaterials[1];

                //if old temperature was higher, higher cost
                if (oldAirTemperature >= 18.0f)
                {
                    //higher cost
                    higher = true;
                    lower = false;
                }//else, if old temperature was lower, lower cost
                else if (oldAirTemperature < 10.0f)
                {
                    //lower cost
                    higher = false;
                    lower = true;
                }
            }
            else if (airTemperature >= 18.0f && airTemperature < 25.0f && (oldAirTemperature < 18.0f || oldAirTemperature >= 25.0f))
            {
                GetComponent<Renderer>().sharedMaterial = loadedMaterials[2];

                //lower cost
                higher = false;
                lower = true;
            }
            else if (airTemperature >= 25.0f && airTemperature < 29.0f && (oldAirTemperature < 25.0f || oldAirTemperature >= 29.0f))
            {
                GetComponent<Renderer>().sharedMaterial = loadedMaterials[3];

                //if old temperature was lower, higher cost
                if(oldAirTemperature < 25.0f)
                {
                    //higher cost
                    higher = true;
                    lower = false;
                }//else, if old temperature was higher, lower cost
                else if (oldAirTemperature >= 29.0f)
                {
                    //lower cost
                    higher = false;
                    lower = true;
                }
            }
            else if (airTemperature >= 29.0f && oldAirTemperature < 29.0f)
            {
                GetComponent<Renderer>().sharedMaterial = loadedMaterials[4];

                //higher cost
                higher = true;
                lower = false;
            }
        }

        if (higher || lower)
        {
            UpdateNodes(higher, lower);

            //if cost if higher or lower, should update path to/from this cell
            if (higher)
            {
                gameController.CheckRaisedCell(gameObject);
            }
            else if (lower)
            {
                gameController.CheckLoweredCell(gameObject);
            }
        }
    }

    //update nodes higher and lower with this cell
    private void UpdateNodes(bool nodeHigher, bool nodeLower)
    {
        GameObject[] allAgents = GameObject.FindGameObjectsWithTag("Player");

        //for each agent
        foreach(GameObject ag in allAgents)
        {
            //foreach node in agent path
            foreach(NodeClass nd in ag.GetComponent<AgentController>().fullPath)
            {
                //if has this cell, update value and break
                if(nd.cell.name == name)
                {
                    nd.higher = nodeHigher;
                    nd.lower = nodeLower;
                }
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
