using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController_human : MonoBehaviour
{
    public int game_count = 8;

    private const int OPEN_FOUR = 0;
    private const int CLOSE_FOUR = 1;
    private const int OPEN_THREE = 2;
    private const int CLOSE_THREE =3;
    private const int OPEN_TWO = 4;
    private const int CLOSE_TWO = 5;
    private const int FIVE = 6;

    private int[] EvalValues = new int[7] { 20, 8, 8, 4, 4, 2, 100 };

    private int INFINITYVAL=32000;
    public int WINNING = 30000;
    public int MAXDEPTH = 0;
    public int size = 15;
    public int[,] squares = new int[15,15];
    private int nextZ, nextX;

    private const int EMPTY = 0;
    private const int WHITE= 1;
    private const int BLACK = -1;

    private int currentPlayer = BLACK;
    private int rootColor;

    private Camera camera_object;
    private RaycastHit hit;

    public GameObject whiteStone;
    public GameObject blackStone;

    private int flag = 0;
    //private int rand = Random.Range(0, 5);

   
    public int[,] potentialEvaluation = new int[15, 15]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0 },
            { 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0 },
            { 0, 2, 2, 2, 5, 5, 5, 5, 5, 5, 5, 2, 2, 2, 0 },
            { 0, 2, 2, 2, 5, 5, 5, 5, 5, 5, 5, 2, 2, 2, 0 },
            { 0, 2, 2, 2, 5, 5, 5, 5, 5, 5, 5, 2, 2, 2, 0 },
            { 0, 2, 2, 2, 5, 5, 5, 10, 5, 5, 5, 2, 2, 2, 0 },
            { 0, 2, 2, 2, 5, 5, 5, 5, 5, 5, 5, 2, 2, 2, 0 },
            { 0, 2, 2, 2, 5, 5, 5, 5, 5, 5, 5, 2, 2, 2, 0 },
            { 0, 2, 2, 2, 5, 5, 5, 5, 5, 5, 5, 2, 2, 2, 0 },
            { 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        };
    private int blackOpenFour;      // 黒い石を四つ並びの数、ブロックなし： * B B B B *
    private int blackClosedFour;    // 黒い石を四つ並びの数、白い石のブロック (W B B B B *) か盤の端にブロック (E B B B B *)
    private int blackOpenThree;     // 黒い石を三つ並びの数、ブロックなし： * B B B *
    private int blackClosedThree;   // 黒い石を三つ並びの数、白い石のブロック (W B B B *) か盤の端にブロック (E B B B *)
    private int blackOpenTwo;       // 黒い石を二つ並びの数、ブロックなし： * B B *
    private int blackClosedTwo;     // 黒い石を二つ並びの数、白い石のブロック (W B B *) か盤の端にブロック (E B B *)
    private int whiteOpenFour;      // 白い石を四つ並びの数、黒と同様
    private int whiteClosedFour;    // 白い石を四つ並びの数、黒と同様
    private int whiteOpenThree;     // 白い石を三つ並びの数、黒と同様
    private int whiteClosedThree;   // 白い石を三つ並びの数、黒と同様
    private int whiteOpenTwo;       // 白い石を二つ並びの数、黒と同様
    private int whiteClosedTwo;		// 白い石を二つ並びの数、黒と同様
    // Start is called before the first frame update


    void Start()
    {
        camera_object = GameObject.Find("Camera").GetComponent<Camera>();
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
                if (game_count > 0)
                {
                    game_count--;
                    InitializeArray();
                    GameObject[] destroy = GameObject.FindGameObjectsWithTag("Stone");
                    foreach(GameObject stone in destroy)
                    {
                        Destroy(stone);
                    }
                    flag = 0;
                }
                else
                {
                    return;
                }
                
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

                        decideAIMove(currentPlayer);
                        z = nextZ;
                        x=nextX;
                        squares[z,x] = WHITE;
                        GameObject stone = Instantiate(whiteStone);
                        stone.transform.position =new Vector3(x,0.3f,z);

                        currentPlayer = BLACK;
                    }
                    else if (currentPlayer == BLACK)
                    {
                        squares[z, x] = BLACK;
                        GameObject stone = Instantiate(blackStone);
                        stone.transform.position = new Vector3(x, 0.3f, z) ;
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
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                squares[i, j] = EMPTY;
            }
        }
    }

    public void DebugArray()
    {
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                Debug.Log("(i,j)=(" + i + "," + j + ")=" + squares[i, j]);
            }
        }
    }
    private bool CheckStone(int color,int z,int x)
    {
        int count = 0;

        //row judge
        for(int i = 0; i < size; ++i)
        {
            for(int j = 0; j < size; ++j)
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
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
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
            if((z + c>=0 && x + c >= 0) && (z + c < size && x + c < size))
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
            if ((z + c >= 0 && x - c >= 0) && (z + c < size && x - c < size))
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

    private void decideAIMove(int color)
    {
        int score;
        rootColor = color;
        score = alphaBetaSearch(0, rootColor, -INFINITYVAL, INFINITYVAL);
        //score = evaluate(color, color, 0);
        
    }

    private int alphaBetaSearch(int depth,int color,int alpha,int beta)
    {
        int score, eval;
        if (depth == MAXDEPTH)
        {
            return evaluate(rootColor, color, depth);
        }
        if (color == rootColor)
        {
            score = -INFINITYVAL;
        }
        else
        {
            score = INFINITYVAL;
        }
        for(int i = 0; i < size; ++i)
        {
            for(int j = 0; j < size; ++j)
            {
                if (squares[i, j] == EMPTY)
                {
                    squares[i, j] = color;
                }
                if (CheckStone(color, i, j) == true)
                {
                    if (rootColor == color)
                    {
                        squares[i, j] = EMPTY;
                        if (depth == 0)
                        {
                            nextX = j;
                            nextZ = i;
                        }
                        return WINNING - depth;
                    }
                    else
                    {
                        squares[i, j] = EMPTY;
                        return -(WINNING - depth);
                    }
                }
                else
                {
                    eval = alphaBetaSearch(depth + 1, color * (-1), alpha, beta);
                    squares[i, j] = EMPTY;
                    if (rootColor == color)
                    {
                        if (eval > score)
                        {
                            score = eval;
                            if (depth == 0)
                            {
                                nextX = j;
                                nextZ = i;
                            }
                        }
                        if (score >= beta)
                        {
                            return score;
                        }
                        if (score > alpha)
                        {
                            alpha = score;
                        }
                    }
                    else
                    {
                        if (eval < score)
                        {
                            score = eval;
                        }

                        if (score <= alpha)
                        {
                            return score;
                        }
                        if (score > beta)
                        {
                            beta = score;
                        }
                    }
                }
            }
        }
        return score;
    }
    
    private int evaluate(int side,int nextcolor,int depth)
    {
        int eval = 0;
        int connect_num;          // 連続の同じ色の石の数
        int expected_emp_num;         // 連結はどこまで伸ばせる
        int blocked;			// 石の連結はブロックされている
        int countZ, countX;

        blackOpenFour = 0;
        blackClosedFour = 0;
        blackOpenThree = 0;
        blackClosedThree = 0;
        blackOpenTwo = 0;
        blackClosedTwo = 0;
        whiteOpenFour = 0;
        whiteClosedFour = 0;
        whiteOpenThree = 0;
        whiteClosedThree = 0;
        whiteOpenTwo = 0;
        whiteClosedTwo = 0;


        for(int i = 0; i< size; ++i)
        {
            for(int j = 0; j < size; ++j)
            {
                if (squares[i,j] == BLACK)
                {

                    //North_East
                    connect_num = 1;
                    expected_emp_num = 0;
                    if((i==0 || j==0) &&(i>0 && j>0 && squares[i - 1, j - 1] == WHITE)){
                        blocked = 1;
                    }
                    else
                    {
                        blocked = 0;
                        expected_emp_num ++;
                    }
                    for (countZ = i + 1, countX = j + 1; countZ < size && countX < size; countZ++,countX++){
                        if (squares[countZ,countX] == BLACK)
                        {
                            connect_num++;
                        }
                        else if (squares[countZ,countX] == WHITE)
                        {
                            blocked++;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if(blocked<=1 && connect_num > 1)
                    {
                        for(;countZ<size && countX < size && squares[countZ,countX]==EMPTY; ++countZ, ++countX)
                        {
                            expected_emp_num++;
                        }
                    }
                    blackConnect(connect_num, blocked, expected_emp_num);

                    //East
                    connect_num = 1;
                    expected_emp_num = 0;
                    if (i == 0  && (i > 0 && squares[i - 1, j] == WHITE)){
                        blocked = 1;
                    }
                    else
                    {
                        blocked = 0;
                        expected_emp_num++;
                    }
                    countX = j;
                    for (countZ = i + 1; countZ < size; countZ++)
                    {
                        if (squares[countZ,countX] == BLACK)
                        {
                            connect_num++;
                        }
                        else if (squares[countZ,countX] == WHITE)
                        {
                            blocked++;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (blocked <= 1 && connect_num > 1)
                    {
                        for (; countZ < size && squares[countZ,countX]==EMPTY; ++countZ)
                        {
                            expected_emp_num++;
                        }
                    }
                    blackConnect(connect_num, blocked, expected_emp_num);

                    //South_East
                    connect_num = 1;
                    expected_emp_num = 0;
                    if ((i == 0 || j == size-1) && (i > 0 && j <size-1 && squares[i - 1, j + 1] == WHITE)){
                        blocked = 1;
                    }
                    else
                    {
                        blocked = 0;
                        expected_emp_num++;
                    }
                    for (countZ = i + 1, countX = j - 1; countZ < size && countX>=0; countZ++, countX--)
                    {
                        if (squares[countZ, countX] == BLACK)
                        {
                            connect_num++;
                        }
                        else if (squares[countZ, countX] == WHITE)
                        {
                            blocked++;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (blocked <= 1 && connect_num > 1)
                    {
                        for (; countZ < size && countX>=0 && squares[countZ, countX] == EMPTY; ++countZ, --countX)
                        {
                            expected_emp_num++;
                        }
                    }
                    blackConnect(connect_num, blocked, expected_emp_num);

                    //South
                    connect_num = 1;
                    expected_emp_num = 0;
                    if ((j == size - 1) && (j < size - 1 && squares[i , j + 1] == WHITE)){
                        blocked = 1;
                    }
                    else
                    {
                        blocked = 0;
                        expected_emp_num++;
                    }
                    for (countX = j - 1; countX >= 0; countX--)
                    {
                        if (squares[countZ, countX] == BLACK)
                        {
                            connect_num++;
                        }
                        else if (squares[countZ, countX] == WHITE)
                        {
                            blocked++;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (blocked <= 1 && connect_num > 1)
                    {
                        for (; countX >= 0 && squares[countZ, countX] == EMPTY; --countX)
                        {
                            expected_emp_num++;
                        }
                    }
                    blackConnect(connect_num, blocked, expected_emp_num);
                }
                else if (squares[i, j] == WHITE)
                {

                    //North_East
                    connect_num = 1;
                    expected_emp_num = 0;
                    if ((i == 0 || j == 0) && (i > 0 && j > 0 && squares[i - 1, j - 1] == BLACK)){
                        blocked = 1;
                    }
                    else
                    {
                        blocked = 0;
                        expected_emp_num++;
                    }
                    for (countZ = i + 1, countX = j + 1; countZ < size && countX < size; countZ++, countX++)
                    {
                        if (squares[countZ, countX] == BLACK)
                        {
                            connect_num++;
                        }
                        else if (squares[countZ, countX] == BLACK)
                        {
                            blocked++;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (blocked <= 1 && connect_num > 1)
                    {
                        for (; countZ < size && countX < size && squares[countZ, countX] == EMPTY; ++countZ, ++countX)
                        {
                            expected_emp_num++;
                        }
                    }
                    blackConnect(connect_num, blocked, expected_emp_num);

                    //East
                    connect_num = 1;
                    expected_emp_num = 0;
                    if (i == 0 && (i > 0 && squares[i - 1, j] == BLACK)){
                        blocked = 1;
                    }
                    else
                    {
                        blocked = 0;
                        expected_emp_num++;
                    }
                    countX = j;
                    for (countZ = i + 1; countZ < size; countZ++)
                    {
                        if (squares[countZ, countX] == BLACK)
                        {
                            connect_num++;
                        }
                        else if (squares[countZ, countX] == BLACK)
                        {
                            blocked++;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (blocked <= 1 && connect_num > 1)
                    {
                        for (; countZ < size && squares[countZ, countX] == EMPTY; ++countZ)
                        {
                            expected_emp_num++;
                        }
                    }
                    blackConnect(connect_num, blocked, expected_emp_num);

                    //South_East
                    connect_num = 1;
                    expected_emp_num = 0;
                    if ((i == 0 || j == size - 1) && (i > 0 && j < size - 1 && squares[i - 1, j + 1] == BLACK)){
                        blocked = 1;
                    }
                    else
                    {
                        blocked = 0;
                        expected_emp_num++;
                    }
                    for (countZ = i + 1, countX = j - 1; countZ < size && countX >= 0; countZ++, countX--)
                    {
                        if (squares[countZ, countX] == BLACK)
                        {
                            connect_num++;
                        }
                        else if (squares[countZ, countX] == BLACK)
                        {
                            blocked++;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (blocked <= 1 && connect_num > 1)
                    {
                        for (; countZ < size && countX >= 0 && squares[countZ, countX] == EMPTY; ++countZ, --countX)
                        {
                            expected_emp_num++;
                        }
                    }
                    blackConnect(connect_num, blocked, expected_emp_num);

                    //South
                    connect_num = 1;
                    expected_emp_num = 0;
                    if ((j == size - 1) && (j < size - 1 && squares[i, j + 1] == BLACK)){
                        blocked = 1;
                    }
                    else
                    {
                        blocked = 0;
                        expected_emp_num++;
                    }
                    for (countX = j - 1; countX >= 0; countX--)
                    {
                        if (squares[countZ, countX] == BLACK)
                        {
                            connect_num++;
                        }
                        else if (squares[countZ, countX] == BLACK)
                        {
                            blocked++;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (blocked <= 1 && connect_num > 1)
                    {
                        for (; countX >= 0 && squares[countZ, countX] == EMPTY; --countX)
                        {
                            expected_emp_num++;
                        }
                    }
                    blackConnect(connect_num, blocked, expected_emp_num);
                }
                if(side==BLACK && squares[i,j] == BLACK)
                {
                    eval += potentialEvaluation[i,j];
                }
                else if(side==WHITE && squares[i,j] == WHITE)
                {
                    eval -= potentialEvaluation[i, j];
                }
            }
        }

        //eval += (rand % 5);

        eval += (blackOpenFour - whiteOpenFour) * EvalValues[OPEN_FOUR];
        eval += (blackClosedFour - whiteClosedFour) * EvalValues[CLOSE_FOUR];
        eval += (blackOpenThree - whiteOpenThree) * EvalValues[OPEN_THREE];
        eval += (blackClosedThree - whiteClosedThree) * EvalValues[CLOSE_THREE];
        eval += (blackOpenTwo - whiteOpenTwo) * EvalValues[OPEN_TWO];
        eval += (blackClosedTwo - whiteClosedTwo) * EvalValues[CLOSE_TWO];

        if (side == WHITE)
        {
            return -eval;
        }
        return eval;
    }

    void whiteConnect(int connect_num,int blocked, int openSquare)
    {
        if (blocked >= 2)
        {

        }
        else if (connect_num + openSquare < 4
            )
        {

        }
        else if (connect_num == 4)
        {
            if (blocked == 0)
            {
                whiteOpenFour++;
            }
            else if (blocked == 1)
            {
                whiteClosedFour++;
            }
        }
        else if (connect_num == 3)
        {
            if (blocked == 0)
            {
                whiteOpenThree++;
            }
            else if (blocked == 1)
            {
                whiteClosedThree++;
            }
        }
        else if (connect_num == 2)
        {
            if (blocked == 0)
            {
                whiteOpenTwo++;
            }
            else if (blocked == 1)
            {
                whiteClosedTwo++;
            }
        }
    }

    void blackConnect(int connect_num, int blocked, int openSquare)
    {
        if (blocked >= 2)
        {

        }
        else if (connect_num + openSquare < 5)
        {

        }
        else if (connect_num == 4)
        {
            if (blocked == 0)
            {
                whiteOpenFour++;
            }
            else if (blocked == 1)
            {
                whiteClosedFour++;
            }
        }
        else if (connect_num == 3)
        {
            if (blocked == 0)
            {
                whiteOpenThree++;
            }
            else if (blocked == 1)
            {
                whiteClosedThree++;
            }
        }
        else if (connect_num == 2)
        {
            if (blocked == 0)
            {
                whiteOpenTwo++;
            }
            else if (blocked == 1)
            {
                whiteClosedTwo++;
            }
        }
    }
}



