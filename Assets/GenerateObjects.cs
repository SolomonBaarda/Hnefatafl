using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateObjects : MonoBehaviour
{
    public List<GameObject> prefabs;
    public int countMin = 10, countMax = 20;
    public float scaleMin = 0.5f, scaleMax = 10f;

    public Vector2 min = new Vector2(), max = new Vector2();

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject g in prefabs)
        {
            int count = (Random.Range(countMin, countMax));
            for(int i = 0; i < count; i++)
            {
                GameObject o = Instantiate(g);
                o.transform.localScale = new Vector3(Random.Range(scaleMin, scaleMax), Random.Range(scaleMin, scaleMax), Random.Range(scaleMin, scaleMax));

                // Set all the positions and rotation
                Vector3 pos = new Vector3(Random.Range(min.x, max.x), 50, Random.Range(min.y, max.y));
                o.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, Random.Range(0, 360), 0));

                // Parent
                o.transform.parent = GameObject.FindGameObjectWithTag("Objects").transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
