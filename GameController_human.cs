using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private int[,] squares = new int[16,16];

    private const int EMPTY = 0;
    private const int WHITE= 1;
    private const int BLACK = -1;

    private int currentPlayer = WHITE;

    private Camera camera_object;
    private RaycastHit hit;

    public GameObject whiteStone;
    public GameObject blackStone;

    public int flag = 0;
    // Start is called before the first frame update
    void Start()
    {
        camera_object = GameObject.Find("Camera").GetComponent<Camera>();
        whiteStone = (GameObject)Resources.Load("white_stone");
         blackStone = (GameObject)Resources.Load("black_stone");
        InitializeArray();
        //DebugArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (flag==1)
            {
                return;
            }
            Ray ray = camera_object.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray,out hit))
            {
                int x = (int)hit.collider.gameObject.transform.position.x;
                int z = (int)hit.collider.gameObject.transform.position.z;
                if (squares[z, x] == EMPTY)
                {
                    if (currentPlayer == WHITE)
                    {
                        squares[z, x] = WHITE;
                        GameObject stone = Instantiate(whiteStone);
                        stone.transform.position = hit.collider.gameObject.transform.position;

                        currentPlayer = BLACK;
                    }
                    else if (currentPlayer == BLACK)
                    {
                        squares[z, x] = BLACK;
                        GameObject stone = Instantiate(blackStone);
                        stone.transform.position = hit.collider.gameObject.transform.position;
                        currentPlayer = WHITE;
                    }
                }
                if (CheckStone(WHITE, z, x) || CheckStone(BLACK, z, x))
                {
                    flag = 1;
                }
            }
        }
    }
    public void InitializeArray()
    {
        for (int i = 0; i < 16; ++i)
        {
            for (int j = 0; j < 16; ++j)
            {
                squares[i, j] = EMPTY;
            }
        }
    }

    public void DebugArray()
    {
        for (int i = 0; i < 16; ++i)
        {
            for (int j = 0; j < 16; ++j)
            {
                Debug.Log("(i,j)=(" + i + "," + j + ")=" + squares[i, j]);
            }
        }
    }
    private bool CheckStone(int color,int z,int x)
    {
        int count = 0;

        //row judge
        for(int i = 0; i < 16; ++i)
        {
            for(int j = 0; j < 16; ++j)
            {
                if(squares[i,j]==EMPTY || squares[i,j] != color)
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count == 5)
                {
                    if (color == WHITE)
                    {
                        Debug.Log("Player1 won!!");
                    }
                    else
                    {
                        Debug.Log("Player2 won!!");
                    }
                    return true;
                }
            }
        }
        //column judge
        count = 0;
        for (int i = 0; i < 16; ++i)
        {
            for (int j = 0; j < 16; ++j)
            {
                if (squares[j, i] == EMPTY || squares[j, i] != color)
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count == 5)
                {
                    if (color == WHITE)
                    {
                        Debug.Log("Player1 won!!");
                    }
                    else
                    {
                        Debug.Log("Player2 won!!");
                    }
                    return true;
                }
            }
        }

        count = 0;
        int c = -4;
        while (c!=5)
        {
            if((z + c>=0 && x + c >= 0) && (z + c < 16 && x + c < 16))
            {
                if(squares[z+c,x+c]==EMPTY || squares[z + c, x + c] != color)
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count == 5)
                {
                    if (color == WHITE)
                    {
                        Debug.Log("Player1 won!!");
                    }
                    else
                    {
                        Debug.Log("Player2 won!!");
                    }
                    return true;
                }
            }
            ++c;
        }
        count = 0;
        c = -4;
        while (c != 5)
        {
            if ((z + c >= 0 && x - c >= 0) && (z + c < 16 && x - c < 16))
            {
                if (squares[z + c, x - c] == EMPTY || squares[z + c, x - c] != color)
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count == 5)
                {
                    if (color == WHITE)
                    {
                        Debug.Log("Player1 won!!");
                    }
                    else
                    {
                        Debug.Log("Player2 won!!");
                    }
                    return true;
                }
            }
            ++c;
        }
        return false;
    }
}
