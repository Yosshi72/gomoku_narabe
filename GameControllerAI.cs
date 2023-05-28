using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;

public class GameController_AI3 : MonoBehaviour
{
    public int game_count = 8;

    private int[] EvalValues = new int[7] { 2000, 10, 10, 4, 4, 2, 100 };
    private int[] enemyEvalValues = new int[7] { 4000, 30, 30, 8, 8, 2, 100 };

    private float INFINITYVAL=350000;
    private int WINNING = 30000;

    private int start_depth=2;
    public int Black_MAXDEPTH = 1;
    public int White_MAXDEPTH = 1;
    private int first_search_range=15;
    public int search_range=5;
    public int size = 15;
    public int[,] squares = new int[16,16];
    private int nextZ, nextX;

    private const int EMPTY = 0;
    private const int WHITE= -1;
    private const int BLACK = 1;

    public int BLACK_AI = 0; // 黒石がAIであるかの情報を保持
    public int WHITE_AI = 0; // 白石がAIであるかの情報を保持

    private int currentPlayer = BLACK; // 現在の黒と白，どちらの手番かの情報を保持
    private int PlayerColor;

    private Camera camera_object;
    private RaycastHit hit;

    public GameObject whiteStone;
    public GameObject blackStone;

    private int flag = 0; // ゲーム終了判定に使うフラグ
    private int prohibit_flag=0; // 禁じ手判定に使うフラグ
    private int center_z=7;
    private int center_x=7;
    private int pre_z=7;
    private int pre_x=7;
    
    // evaluate based on board position. 
    private int[,] potentialEvaluation = new int[15, 15]
    {//   0  1  2  3  4  5  6  7  8  9  10 11 12 13 14
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },//0
        { 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0 },//1
        { 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0 },//2
        { 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0 },//3
        { 0, 2, 2, 2, 5, 5, 5, 5, 5, 5, 5, 2, 2, 2, 0 },//4
        { 0, 2, 2, 2, 5, 5, 5, 5, 5, 5, 5, 2, 2, 2, 0 },//5
        { 0, 2, 2, 2, 5, 5, 5, 5, 5, 5, 5, 2, 2, 2, 0 },//6
        { 0, 2, 2, 2, 5, 5, 5, 10, 5, 5, 5, 2, 2, 2, 0 },//7
        { 0, 2, 2, 2, 5, 5, 5, 5, 5, 5, 5, 2, 2, 2, 0 },//8
        { 0, 2, 2, 2, 5, 5, 5, 5, 5, 5, 5, 2, 2, 2, 0 },//9
        { 0, 2, 2, 2, 5, 5, 5, 5, 5, 5, 5, 2, 2, 2, 0 },//10
        { 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0 },//11
        { 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0 },//12
        { 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0 },//13
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },//14
    };

    private int open4=0; // 四連で，両端が空いている
    private int normal4=0; // 四連で，片側が黒石 or boardの端で塞がれている
    private int open3=0; // 三連で，両端が空いている
    private int normal3=0; // 三連で，片側が黒石 or boardの端で塞がれている

    private int move_count=0; // 現在のゲームが何手目であるかの情報を保持
    private int node_count=0;
    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        camera_object = GameObject.Find("Camera").GetComponent<Camera>();
        InitializeArray();
        //DebugArray();
    }

    // Update is called once per frame
    void Update()
    {
        // if board is filled with stones
        if(move_count==size*size){
            Debug.Log("Tie Game.");
            if (game_count > 0)
                {
                    game_count--;
                    InitializeArray();
                    GameObject[] destroy = GameObject.FindGameObjectsWithTag("Stone");
                    foreach (GameObject stone in destroy)
                    {
                        Destroy(stone);
                    }
                    move_count=0;
                }
                else
                {
                    return;
                }
        }
        //AI PLAY
        if (currentPlayer == WHITE && WHITE_AI == 1 )
        {
            if (flag == 1 )
            {
                if (game_count > 0)
                {
                    game_count--;
                    InitializeArray();
                    GameObject[] destroy = GameObject.FindGameObjectsWithTag("Stone");
                    foreach (GameObject stone in destroy)
                    {
                        Destroy(stone);
                    }
                    flag = 0;
                    move_count=0;
                }
                else
                {
                    return;
                }
            }
            decideAIMove(currentPlayer);
            int z = nextZ;
            int x = nextX;
            if (squares[z, x] == EMPTY)
            {
                squares[z, x] = WHITE;
                center_z=z;
                center_x=x;
                pre_z=z;
                pre_x=x;
                GameObject stone = Instantiate(whiteStone);
                stone.transform.position = new Vector3(x, 0.1f, z);
                move_count++;
                currentPlayer = BLACK;
                // Debug.Log("Player2 : " + z + ", " + x);
            }
            // Debug.Log(node_count);
            init_count();
            line_nbr(WHITE,z,x,5);
            
            if(flag==1){
                Debug.Log("Player2 won.");
            }
        }
        else if (currentPlayer == BLACK && BLACK_AI == 1 )
        {
            if (flag == 1 || prohibit_flag==1)
            {
                if (game_count > 0)
                {
                    game_count--;
                    InitializeArray();
                    GameObject[] destroy = GameObject.FindGameObjectsWithTag("Stone");
                    foreach (GameObject stone in destroy)
                    {
                        Destroy(stone);
                    }
                    flag = 0;
                    prohibit_flag=0;
                    move_count=0;
                }
                else
                {
                    return;
                }
            }
            decideAIMove(currentPlayer);
            int z = nextZ;
            int x = nextX;
            if (squares[z, x] == EMPTY)
            {
                center_z=z;
                center_x=x;
                pre_z=z;
                pre_x=x;
                squares[z, x] = BLACK;
                GameObject stone = Instantiate(blackStone);
                stone.transform.position = new Vector3(x, 0.1f, z);
                move_count++;
                currentPlayer = WHITE;
                // Debug.Log("Player1 : " + x + ", " + z);
                init_count();
                line_nbr(BLACK,z,x,5);
                
                
                if(prohibit_flag==1){
                    Debug.Log("Player2 won.");
                }
                if(flag==1){
                    Debug.Log("Player1 won.");
                }
            }
        }

        //HUMAN PLAY
        if (Input.GetMouseButtonDown(0))
        {
            if (flag==1 ||prohibit_flag==1)
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
                    prohibit_flag=0;
                    move_count=0;
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
                    if (currentPlayer == WHITE && WHITE_AI==0)
                    {
                        center_z=z;
                        center_x=x;
                        pre_z=z;
                        pre_x=x;
                        squares[z,x] = WHITE;
                        GameObject stone = Instantiate(whiteStone);
                        stone.transform.position =new Vector3(x,0.1f,z);
                        move_count++;
                        currentPlayer = BLACK;
                        // Debug.Log("Player2 : " + z + ", " + x);
                        init_count();
                        line_nbr(WHITE,z,x,5);
                        if(flag==1){
                            Debug.Log("Player2 won.");
                        }
                    }
                    else if (currentPlayer == BLACK && BLACK_AI==0)
                    {
                        center_z=z;
                        center_x=x;
                        pre_z=z;
                        pre_x=x;
                        squares[z, x] = BLACK;
                        GameObject stone = Instantiate(blackStone);
                        stone.transform.position = new Vector3(x, 0.1f, z) ;
                        move_count++;
                        currentPlayer = WHITE;
                        // Debug.Log("Player1 : " + z + ", " + x);
                        init_count();
                        line_nbr(BLACK,z,x,5);
                        
                        if(prohibit_flag==1){
                            Debug.Log("Player2 won.");
                        }
                        if(flag==1){
                            Debug.Log("Player1 won.");
                        }
                    }
                }
            }
        }
    }

    //initialize board
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
    private void decideAIMove(int color)
    {
        float score=0f;
        PlayerColor = color;
        node_count=0;
        if(move_count<=2){
            establish_tactics();
        }
        // else if(move_count<=10){
        //     score = alphaBetaSearch(0, start_depth,first_search_range,PlayerColor, -INFINITYVAL, INFINITYVAL,center_z,center_x,pre_z,pre_x);

        // }
        else {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            if(PlayerColor==BLACK){
                score = alphaBetaSearch(0, Black_MAXDEPTH,search_range,PlayerColor, -INFINITYVAL, INFINITYVAL,center_z,center_x,pre_z,pre_x);
            }
            else{
                score = alphaBetaSearch(0, White_MAXDEPTH,search_range,PlayerColor, -INFINITYVAL, INFINITYVAL,center_z,center_x,pre_z,pre_x);
            }
            sw.Stop();
            // if(squares[nextZ,nextX]==EMPTY){
            //      Debug.Log(PlayerColor+","+nextX+","+nextZ+","+sw.ElapsedMilliseconds);
            // }
            // StreamWriter txt=new StreamWriter("../depth3rangeall_v8_noalpha.txt",true);
            // txt.WriteLine(sw.ElapsedMilliseconds);
            // txt.Flush();
            // txt.Close();
            if(node_count<2){
            InitializeArray();
            GameObject[] destroy = GameObject.FindGameObjectsWithTag("Stone");
            foreach (GameObject stone in destroy)
            {
                Destroy(stone);
            }
            flag = 0;
            move_count=0;
            establish_tactics();
        }
        }
        // Debug.Log(node_count);
        Debug.Log(squares[nextZ,nextX]+","+color+",Final eval: "+score+","+nextX+","+nextZ);
    }

    /* 
        AIが次の一手を決める方針(1~3手目)
        初手は天元，2および3手目は基本珠型に基づく
    */
    private void establish_tactics(){
        if(move_count==0){
            nextZ=7; nextX=7;
        }
        else if(move_count==1){
            int r=Random.Range(0,2);
            if(r==0){
                nextZ=7; nextX=8;
            }
            else{
                nextZ=8; nextX=8;
            }
        }
        else if(move_count==2){
            int r=Random.Range(0,13);
            switch(r){
                case(0): if(pre_z==7){nextZ=7; nextX=9;}else{nextZ=7;nextX=5;} break;
                case(1): if(pre_z==7){nextZ=8; nextX=9;}else{nextZ=6;nextX=5;} break;
                case(2): if(pre_z==7){nextZ=9; nextX=9;}else{nextZ=5;nextX=5;} break;
                case(3): if(pre_z==7){nextZ=8; nextX=8;}else{nextZ=6;nextX=6;} break;
                case(4): nextZ=9; nextX=8; break;
                case(5): nextZ=8; nextX=7; break;
                case(6): nextZ=9; nextX=7; break;
                case(7): nextZ=7; nextX=6; break;
                case(8): if(pre_z==7){nextZ=8; nextX=6;}else{nextZ=6;nextX=6;} break;
                case(9): nextZ=9; nextX=6; break;
                case(10): nextZ=7; nextX=5; break;
                case(11): nextZ=8; nextX=5; break;
                case(12): nextZ=9; nextX=5; break;
            }
        }
    }

    // alpha-beta法
    private float alphaBetaSearch(int depth,int max_depth,int range,int color,float alpha,float beta,int cz,int cx,int pz,int px)
    { 
        node_count++;
        init_count();
        line_nbr(color*(-1),cz,cx,6);
        int tmp_open4=open4;
        int tmp4=normal4;
        int tmp_open3=open3;
        int tmp_3=normal3;
        int tmp1_flag,tmp1_prohibit_flag,tmp1_open4,tmp1_4,tmp1_open3,tmp1_3;

        float score, eval;
        if (depth == max_depth)
        {
            return remake_evaluate(PlayerColor,(-1)*color,depth, cz,cx,pz,px);
        }
        if (color == PlayerColor)
        {
            score = -INFINITYVAL;
        }
        else
        {
            score = INFINITYVAL;
        }
        for(int i = cz-range; i < cz+range+1; ++i)
        {
            for(int j = cx-range; j < cx+range+1; ++j)
            {
                if(i>=0 && i<size &&j>=0 && j<size){
                    if (squares[i, j] == EMPTY)
                    {
                        if(nbr_isStone(i,j))
                        {
                            squares[i, j] = color;
                            init_count();
                            line_nbr(color,i,j,6);
                            tmp1_flag=flag; tmp1_prohibit_flag=prohibit_flag; tmp1_open4=open4; tmp1_4=normal4; tmp1_open3=open3; tmp1_3=normal3;

                            init_count();
                            line_nbr(color*(-1),cz,cx,6);
                            if (tmp1_flag==1)
                            {
                                // if(squares[i,j]!=EMPTY){
                                //     Debug.Log("five,"+squares[i,j]+",eval: "+(WINNING-depth)*PlayerColor*color+",now:["+j+"," +i+"], prev: ["+cx+", "+cz);
                                // }   
                                squares[i, j] = EMPTY;             
                                if (PlayerColor == color)
                                {
                                    if (depth == 0)
                                    {

                                        nextX = j;
                                        nextZ = i;
                                    }
                                    return WINNING - depth;
                                }
                                else
                                {
                                    return -(WINNING - depth);
                                }
                            }
                            else if(tmp1_prohibit_flag==1){
                                // if(squares[i,j]!=EMPTY){
                                //     Debug.Log("prohibit,"+squares[i,j]+",eval: "+(-WINNING+depth)*PlayerColor*color+",now:["+j+"," +i+"], prev: ["+cx+", "+cz);
                                // }     
                                squares[i, j] = EMPTY;    
                                if (PlayerColor == color)
                                {
                                    if(depth>0){return -(WINNING - depth);} else{ continue;}
                                }
                            }
                            else if(open4>=tmp_open4 && tmp_open4!=0){
                                // if(squares[i,j]!=EMPTY){
                                //     Debug.Log("open1,"+squares[i,j]+",eval: "+(-WINNING/2+depth)*PlayerColor*color+",now:["+j+"," +i+"], prev: ["+cx+", "+cz);
                                // }   
                                squares[i, j] = EMPTY;
                                if(PlayerColor==color){
                                    if(depth>0){return -(WINNING/2-depth);}
                                }
                                else{
                                    return WINNING/2 - depth;
                                }
                            }
                            else if(normal4>=tmp4 && tmp4!=0){
                                // if(squares[i,j]!=EMPTY){
                                //     Debug.Log("four1,"+squares[i,j]+",eval: "+(-WINNING/2+depth)*PlayerColor*color+",now:["+j+"," +i+"], prev: ["+cx+", "+cz);
                                // }   
                                squares[i, j] = EMPTY;
                                if(PlayerColor==color){
                                    if(depth>0){return -(WINNING/2-depth);}
                                }
                                else{
                                    return WINNING/2 - depth;
                                }
                            }
                            else if(open3>=tmp_open3 && tmp_open4+tmp4==0 && tmp_open3!=0){
                                // if(squares[i,j]!=EMPTY){
                                //     Debug.Log("open3,"+squares[i,j]+",eval: "+(-WINNING/2+depth)*PlayerColor*color+",now:["+j+"," +i+"], prev: ["+cx+", "+cz);
                                // }   
                                squares[i, j] = EMPTY;
                                if(PlayerColor==color){
                                    if(depth>0){return -(WINNING/2-depth);}
                                }
                                else{
                                    return WINNING/3 - depth;
                                }
                            }
                            else if(tmp1_open4>0 && open4+normal4==0 ){
                                // if(squares[i,j]!=EMPTY){
                                //     Debug.Log("open42,"+squares[i,j]+",eval: "+(WINNING/2-depth)*PlayerColor*color+",now:["+j+"," +i+"], prev: ["+cx+", "+cz);
                                // }   
                                squares[i, j] = EMPTY;
                                if(PlayerColor==color){
                                    if (depth == 0)
                                    {
                                        nextX = j;
                                        nextZ = i;
                                    }
                                    return WINNING/2-depth;  
                                }
                                else{
                                    return -(WINNING/2 - depth);
                                }
                            }
                            else if(tmp_open4+tmp4+tmp_open3+tmp_3!=0 && 8*open4+4*normal4+2*open3+normal3>=8*tmp_open4+4*tmp4+2*tmp_open3+tmp_3 && tmp1_open4+tmp1_4+tmp1_open3+tmp1_3==0){
                                // if(depth==2 &&squares[i,j]!=EMPTY){
                                //     Debug.Log(","+open4+normal4+open3+normal3+","+tmp_open4+tmp4+tmp_open3+tmp_3+",2,"+squares[i,j]+",eval: "+(WINNING/4-depth)*PlayerColor*color+",now:["+j+"," +i+"], prev: ["+cx+", "+cz);
                                // }  
                                squares[i, j] = EMPTY;
                                if(PlayerColor==color){
                                    if(depth>0){return -(WINNING/4-depth);}
                                }
                                else{
                                    return WINNING/4 - depth;
                                }
                            }
                            else
                            {
                                eval = alphaBetaSearch(depth + 1, max_depth,range,color * (-1), alpha, beta,i,j,cz,cx);
                                // if(squares[i,j]!=EMPTY){
                                //     Debug.Log("depth:"+depth+"color:"+color+",eval: "+eval+",now:["+j+"," +i+"], prev: ["+cx+", "+cz);
                                // }                            
                                squares[i, j] = EMPTY;
                                if (PlayerColor == color)
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
                }
            }
        }
        return score;
    }
    //BLACK =1; WHITE=-1;
    float remake_evaluate(int PlayerColor,int nextcolor,int depth,int now_i,int now_j,int pre_i,int pre_j){
        
        int plus_or_minus=PlayerColor*nextcolor;
        float eval=0f;
        eval+=potentialEvaluation[now_i,now_j]*plus_or_minus;
        int rand = Random.Range(0, 5);
        eval += (rand % 5)*plus_or_minus;
        init_count();
        if(prohibit_flag==1){
            //nextcolor==BLACKのみ
            return -(WINNING-depth)*plus_or_minus;
        }
        if(flag==1){
            flag=0;
            return (WINNING-depth)*plus_or_minus;
        }
        init_count();
        line_nbr(nextcolor*(-1),pre_i,pre_j,6);
        if(open4>0){
            return -(WINNING/2-depth)*plus_or_minus;
        }
        else{
            if(normal4>0){
                return -(WINNING/2-depth)*plus_or_minus;
            }
        }
        init_count();
        line_nbr(nextcolor,now_i,now_j,6);
        if(open4>0){
            return (WINNING/2-depth)*plus_or_minus;
        }
        eval+=(normal4*open3*1000+(open3-1)*500+(normal4-1)*500+normal4*150+open3*150+normal3*50)*plus_or_minus;
        init_count();
        line_nbr(nextcolor*(-1),pre_i,pre_j,6);
        eval-=((normal4+open3)*1500+normal3*30)*plus_or_minus;
        init_count();
        return eval;
    }

    bool nbr_isStone(int i,int j){
        if(i>=j && i+j>=size-1){
            if(j>=1 && squares[i-1,j-1]!=EMPTY){return true;}
            if(squares[i-1,j]!=EMPTY){return true;}
            if(j<size-1 && squares[i-1,j+1]!=EMPTY){return true;}
            // if(j>=2 && squares[i-2,j-2]!=EMPTY){return true;}
            // if(j>=1 && squares[i-2,j-1]!=EMPTY){return true;}
            // if(squares[i-2,j]!=EMPTY){return true;}
            // if(j<size-1 && squares[i-2,j+1]!=EMPTY){return true;}
            // if(j<size-2 && squares[i-2,j+2]!=EMPTY){return true;}
            return false;
        }
        else if(i<j && i+j<size-1){
            if(j>=1 &&squares[i+1,j-1]!=EMPTY){return true;}
            else if(squares[i+1,j]!=EMPTY){return true;}
            else if(j<size-1 && squares[i+1,j+1]!=EMPTY){return true;}
            // else if(j>=2 && squares[i+2,j-2]!=EMPTY){return true;}
            // else if(j>=1 && squares[i+2,j-1]!=EMPTY){return true;}
            // else if(squares[i+2,j]!=EMPTY){return true;}
            // else if(j<size-1 && squares[i+2,j+1]!=EMPTY){return true;}
            // else if(j<size-2 && squares[i+2,j+2]!=EMPTY){return true;}
            else{return false;}
        }
        else if(i>=j && i+j<size-1){
            if(i>=1 && squares[i-1,j+1]!=EMPTY){return true;}
            else if(squares[i,j+1]!=EMPTY){return true;}
            else if(i<size-1 && squares[i+1,j+1]!=EMPTY){return true;}
            // else if(i>=2 && squares[i-2,j+2]!=EMPTY){return true;}
            // else if(i>=1 && squares[i-1,j+2]!=EMPTY){return true;}
            // else if(squares[i,j+2]!=EMPTY){return true;}
            // else if(i<size-1&& squares[i+1,j+2]!=EMPTY){return true;}
            // else if(i<size-2 && squares[i+2,j+2]!=EMPTY){return true;}
            else{return false;}
        }
        else{
            if(i>=1 && squares[i-1,j-1]!=EMPTY){return true;}
            else if(squares[i,j-1]!=EMPTY){return true;}
            else if(i<size-1 && squares[i+1,j-1]!=EMPTY){return true;}
            // else if(i>=2&& squares[i-2,j-2]!=EMPTY){return true;}
            // else if(i>=1 && squares[i-1,j-2]!=EMPTY){return true;}
            // else if(squares[i,j-2]!=EMPTY){return true;}
            // else if(i<size-1 && squares[i+1,j-2]!=EMPTY){return true;}
            // else if(i<size-2 && squares[i+2,j-2]!=EMPTY){return true;}
            else{return false;}
        }
    }

    /*
        手番の石の色color, 着手する座標(i,j), 探索範囲Nで，探索する
        (i,j)を中心に，長さNだけ，八方位について探索する
    */
    void line_nbr(int color,int i,int j,int N){
        int con1,con2,con3,con4;
        StringBuilder connect1 =new StringBuilder();
        StringBuilder connect2 =new StringBuilder();
        StringBuilder connect3 =new StringBuilder();
        StringBuilder connect4 =new StringBuilder();
        for(int c=0;c<2*N+1;++c){
            //南北方向
            string str=assign_char(color,i-N+c,j);
            connect1.Append(str);
            //東西方向
            str=assign_char(color,i,j-N+c);
            connect2.Append(str);
            //南西から北東方向
            str=assign_char(color,i-N+c,j-N+c);
            connect3.Append(str);
            //北西から南東方向
            str=assign_char(color,i-N+c,j+N-c);
            connect4.Append(str);
        }
        string c1=connect1.ToString();
        string c2=connect2.ToString();
        string c3=connect3.ToString();
        string c4=connect4.ToString();

        // 得られた長さ2N+1の4つの文字列に対して，連結の仕方の判定をする，
        if(color==BLACK){
            con1=judge_Blackconnect(c1);
            prohibit_judge(con1);
            con2=judge_Blackconnect(c2);
            prohibit_judge(con2);
            con3=judge_Blackconnect(c3);
            prohibit_judge(con3);
            con4=judge_Blackconnect(c4);
            prohibit_judge(con4);
            // if(normal4+open4>=2){
            //     prohibit_flag=1;
            // }
            if(open3>=2){
                prohibit_flag=1;
            }
        }
        if(color==WHITE){
            con1=judge_Whiteconnect(c1);
            con2=judge_Whiteconnect(c2);
            con3=judge_Whiteconnect(c3);
            con4=judge_Whiteconnect(c4);
            prohibit_judge(con1);
            prohibit_judge(con2);
            prohibit_judge(con3);
            prohibit_judge(con4);
        }
    }

    // 指定された座標(i,j)が，黒石か白石か空白かを調べる
    string assign_char(int color,int i,int j){
        //(i,j)がboard内の座標
        if(i>=0 && i<size && j>=0 && j<size){
            if(squares[i,j]==WHITE){
                return "W";
            }
            else if(squares[i,j]==BLACK){
                return "B";
            }
            else if(squares[i,j]==EMPTY){
                return "E";
            } 
        }
        //(i,j)がboardの外の場合，壁と見做すために手番の色と逆の色を返す
        if(color==BLACK){
            return "W";
        }
        else{
            return "B";
        }
    }

    /*
        黒石の連結の仕方に応じて，値を返す
        0:5連で勝利, 1:両側空き4連でリーチ, 2:4連, 3:両側空き３連, 4: 3or2 5:その他, 6: 直線状4*2つで禁じ手,7:6連で禁じ手
    */
    int judge_Blackconnect(string str){
        string[] B4E1=new string[5]{"EBBBB","BEBBB","BBEBB","BBBEB","BBBBE"};
        string[] B3E1=new string[4]{"BBB","BEBB","BBEB","BBB"};
        string[] straight44=new string[3]{"BEBBBEB","BBEBBEBB","BBBEBEBBB"};

        bool five_B=Regex.IsMatch(str,"BBBBB");
        if(five_B){
            bool six_B=Regex.IsMatch(str,"BBBBBB");
            if(six_B){
                return 7;
            }
            return 0;
        }
        foreach(string ans in straight44){
            bool a=Regex.IsMatch(str,ans);
            if(a){
               return 6;
            }
        }
        bool open_four=Regex.IsMatch(str,"EBBBBE");
        if(open_four){
            bool essential_six0=Regex.IsMatch(str,"BEBBBBE");
            bool essential_six1=Regex.IsMatch(str,"EBBBBEB");
            if(!(essential_six0 || essential_six1)){
                return 1;
            }
        }
        foreach(string ans in B4E1){
            bool a=Regex.IsMatch(str,ans);
            if(a){
                    return 2;
            }
        }
        bool blocked4=Regex.IsMatch(str,"WBBBBW");
        if(blocked4){
            return 5;
        }
        foreach(string ans in B3E1){
            bool a=Regex.IsMatch(str,ans);
            if(a){
                bool a1=Regex.IsMatch(str,"E"+ans+"E");
                if(a1){
                    return 3;
                }
                bool a2=Regex.IsMatch(str,"W"+ans+"W");
                if(a2){
                    return 5;
                }
                return 4;
            }
        }
        bool two =Regex.IsMatch(str,"EBBE");
        if(two){
            return 4;
        }
        two =Regex.IsMatch(str,"EBEBE");
        if(two){
            return 4;
        }
        return 5;
    }
    
    /*  
        白石の連結の仕方に応じて，値を返す．
        0: ５連, 1: 両開き４連, 2: ４連, 3:両開き3連　4: 3連or両開き2連　５；その他
    */
    int judge_Whiteconnect(string str){
        string[] W4E1=new string[5]{"EWWWW","WEWWW","WWEWW","WWWEW","WWWWE"};
        string[] W3E1=new string[4]{"WWW","WEWW","WWEW","WWW"};
        bool five_W=Regex.IsMatch(str,"WWWWW");
        if(five_W){
            return 0;
        }
        bool open_four =Regex.IsMatch(str,"EWWWWE");
        if(open_four){
            return 1;
        }
        foreach(string str0 in W4E1){
            bool four=Regex.IsMatch(str,str0);
            if(four){
                return 2;
            }
        }
        bool blocked4=Regex.IsMatch(str,"BWWWWB");
        if(blocked4){
            return 5;
        }
        foreach(string str0 in W3E1){
            bool open_three=Regex.IsMatch(str,"E"+str0+"E");
            if(open_three){
                return 3;
            }
            bool blocked=Regex.IsMatch(str,"B"+str0+"B");
            if(blocked){
                return 5;
            }
            bool three=Regex.IsMatch(str,str0);
            if(three){
                return 4;
            }
        }
        bool two =Regex.IsMatch(str,"EWWE");
        if(two){
            return 4;
        }
        two =Regex.IsMatch(str,"EWEWE");
        if(two){
            return 4;
        }
        return 5;
    }

    // 禁じ手の判定
    void prohibit_judge(int num){
        switch(num){
            case 6:
            prohibit_flag=1;  break;
            case 7: 
            prohibit_flag=1; break;
            case 0:
            flag=1; break;
            case 1:
            open4++; break;
            case 2:
            normal4++; break;
            case 3:
            open3++; break;
            case 4:
            normal3++; break;
        }
    }

    // 役のカウントの初期化
    void init_count(){
        open4=0;
        normal4=0;
        open3=0;
        normal3=0;
        prohibit_flag=0;
        flag=0;
    }
}


