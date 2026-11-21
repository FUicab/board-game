using System.Collections;
using UnityEngine;

public class SelfErase : MonoBehaviour
{
    public bool useSelfErase = false;
    public float waitTime = 3f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (useSelfErase)
        {
            StartCoroutine(Erase());
        }
    }

    private IEnumerator Erase()
    {
        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
