using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class board : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        int line = 16;
        int BoardSize = line * line;
        GameObject[] obj1 = new GameObject[BoardSize];
        for (int i0 = 0; i0 < BoardSize ; ++i0)
        {
            int i = i0 / 2;
            if (i0 % 2 == 0)
            {
                obj1[i0] = (GameObject)Resources.Load("Cube1");
                Instantiate(obj1[i0], new Vector3((2 * i) % line + (i / (line/2)) % 2, 0, i / (line/2)), Quaternion.identity);
                //obj1[i0].GetComponent<Renderer>().sharedmaterial.color = Color.red ;
            }
            else
            {
                obj1[i0] = (GameObject)Resources.Load("Cube2");
                Instantiate(obj1[i0], new Vector3((2 * i) % line + (i / (line/2) + 1) % 2, 0, i / (line/2)), Quaternion.identity);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
