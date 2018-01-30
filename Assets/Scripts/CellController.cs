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
    private Dictionary<string, List<GameObject>> cellPath = new Dictionary<string, List<GameObject>>();
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
    }

    private void Awake()
    {
        agentsDensity = new List<GameObject>();
    }

    private void Update()
    {
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
        }
    }

    public void StartList()
    {
        myAuxins = new List<AuxinController>();
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
