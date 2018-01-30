using UnityEngine;

//to let me store this data in pre-proccessing
[System.Serializable]

public class AuxinController {
	//is auxin taken?
	public bool taken = false;
    //position
    public Vector3 position;
    //name
    public string name;
    //cell who has this auxin
    public GameObject cell;
    //should this auxin be ignored?
    public bool ignoreAuxin = false;

    //min distance from a taken agent
    //when a new agent find it in his personal space, test the distance with this value to see which one is smaller
    private float minDistance = 2;
	//agent who took this auxin
	private GameObject agent;

	//Reset auxin to his default state, for each update
	public void ResetAuxin(){
		SetMinDistance (2);
		SetAgent (null);
		taken = false;
        ignoreAuxin = false;
	}

	//GET-SET
	public float GetMinDistance(){
		return this.minDistance;
	}
	public void SetMinDistance(float minDistance){
		this.minDistance = minDistance;
	}
	public GameObject GetAgent(){
		return this.agent;
	}
	public void SetAgent(GameObject agent){
		this.agent = agent;
	}
    public GameObject GetCell()
    {
        return this.cell;
    }
    public void SetCell(GameObject cell)
    {
        this.cell = cell;
    }
}
