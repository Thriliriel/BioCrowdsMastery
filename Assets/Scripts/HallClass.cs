using UnityEngine;

public class HallClass
{
    //constructors
    public HallClass()
    {
        //default values
        intimateZone.x = 0;
        intimateZone.y = 0.45f;
        personalZone.x = intimateZone.y;
        personalZone.y = 1.2f;
        socialZone.x = personalZone.y;
        socialZone.y = 3.6f;
        publicZone.x = socialZone.y;
        publicZone.y = 100;
    }
    
    //VECTOR2: X = MINVALUE; Y = MAXVALUE
	//intimate zone
	private Vector2 intimateZone;
    //personal zone
    private Vector2 personalZone;
    //social zone
    private Vector2 socialZone;
    //public zone
    private Vector2 publicZone;

    //Getters and Setters
    public Vector2 GetIntimateZone()
    {
        return intimateZone;
    }
    public void SetIntimateZone(Vector2 value)
    {
        intimateZone = value;
    }
    public Vector2 GetPersonalZone()
    {
        return personalZone;
    }
    public void SetPersonalZone(Vector2 value)
    {
        personalZone = value;
    }
    public Vector2 GetSocialZone()
    {
        return socialZone;
    }
    public void SetSocialZone(Vector2 value)
    {
        socialZone = value;
    }
    public Vector2 GetPublicZone()
    {
        return publicZone;
    }
    public void SetPublicZone(Vector2 value)
    {
        publicZone = value;
    }
}
