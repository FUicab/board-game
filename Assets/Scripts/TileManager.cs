using UnityEngine;

public class TileManager : MonoBehaviour
{

    [SerializeField]
    public bool posZOpen = false;
    [SerializeField]
    public bool negZOpen = false;
    [SerializeField]
    public bool posXOpen = false;
    [SerializeField]
    public bool negXOpen = false;
    //float rotation = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Debug.DrawRay(transform.position, Vector3.forward * 2, Color.red, 10f);
    }

    public void SetRotation(float newRotation)
    {
        Debug.Log(newRotation);
        bool newPosZOpen = false;
        bool newNegZOpen = false;
        bool newPosXOpen = false;
        bool newNegXOpen = false;
        switch (newRotation) {
            case 0f:
                if (posZOpen) newPosZOpen = true;
                if (negZOpen) newNegZOpen = true;
                if (posXOpen) newPosXOpen = true;
                if (negXOpen) newNegXOpen = true;
                break;
            case 90f:
                if (posZOpen) newPosXOpen = true;
                if (negZOpen) newNegXOpen = true;
                if (posXOpen) newNegZOpen = true;
                if (negXOpen) newPosZOpen = true;
                break;
            case 180f:
                if (posZOpen) newNegZOpen = true;
                if (negZOpen) newPosZOpen = true;
                if (posXOpen) newNegXOpen = true;
                if (negXOpen) newPosXOpen = true;
                break;
            case 270f:
                if (posZOpen) newNegXOpen = true;
                if (negZOpen) newPosXOpen = true;
                if (posXOpen) newPosZOpen = true;
                if (negXOpen) newNegZOpen = true;
                break;
        }
        posZOpen = newPosZOpen;
        negZOpen = newNegZOpen;
        posXOpen = newPosXOpen;
        negXOpen = newNegXOpen;
        //if (newPosZOpen) Debug.DrawRay(transform.position, Vector3.forward * 3, Color.red, 10f);
        //if (newNegZOpen) Debug.DrawRay(transform.position, Vector3.back * 3, Color.red, 10f);
        //if (newPosXOpen) Debug.DrawRay(transform.position, Vector3.right * 3, Color.red, 10f);
        //if (newNegXOpen) Debug.DrawRay(transform.position, Vector3.left * 3, Color.red, 10f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
