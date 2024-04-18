using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;


public class GameController : MonoBehaviour
{
    //5*5のint型２次元配列を定義
    private GameObject[,] board = new GameObject[6, 6];
    private int[,] squares = new int[6, 6];
    private int[,] linesVar= new int[5 + 2, 5 + 2];
    private int[,] linesHori= new int[5 + 2, 5 + 2];

   
    //RED=1,BLUE=-1で定義
    private const int EMPTY = 0;
    private const int RED = 1;
    private const int BLUE = -1;
    private const int DELETING = 2;
    private const int DELETED = 3;

    //現在のプレイヤー(初期プレイヤーは赤)
    private int currentPlayer = RED;

    //ターン数
    private int turnCount = 1;

    //スコア
    private int scoreRed = 0;
    private int scoreBlue = 0;
    
    //x,y座標を持つクラス
    class Point_t
    {
        public int x, y;
        public Point_t(int y, int x)
        {
            this.x = x;
            this.y = y;
        }

    }
    
    Point_t[] directions = new Point_t[4];
  
    // Start is called before the first frame update
    void Start()
    {
        InitializeArray();
        SetUpDirections();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(pos, new Vector3(0, 0, 1), 100);


            //垂直の線について
            if (hit.collider.gameObject.transform.rotation.z == 0)
            {
                int x = (int)(hit.collider.gameObject.transform.position.x + 0.5);
                int y = (int)hit.collider.gameObject.transform.position.y;
                if (linesVar[y, x] == EMPTY)
                {
                    if (currentPlayer == RED && CheckLineConected_Vartical(y,x,RED))
                    {
                        linesVar[y, x] = RED;
                        //gameObject.GetComponent<Renderer>().material.color = Color.red;

                        hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;
                        currentPlayer = BLUE;
                        if (x - 1 >= 0) DrowBoard(y, x - 1, RED);
                        if(x < 6) DrowBoard(y, x, RED);

                        Debug.Log("REDがおいた");
                        

                    }
                    else if (currentPlayer == BLUE && CheckLineConected_Vartical(y,x,BLUE))
                    {
                        linesVar[y, x] = BLUE;


                        hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.blue;

                        turnCount++;
                        currentPlayer = RED;
                        if (x - 1 >= 0) DrowBoard(y, x - 1, BLUE);
                        if(x < 6) DrowBoard(y, x, BLUE);
                        
                        Debug.Log("BLUEがおいた");
                    }
                }

            }
            //水平の線について
            else //if(hit.collider.gameObject.transform.rotation.z == 90)
            {
                int x = (int)(hit.collider.gameObject.transform.position.x);
                int y = (int)(hit.collider.gameObject.transform.position.y + 0.5);
                if (linesHori[y, x] == EMPTY)
                {
                    if (currentPlayer == RED && CheckLineConected_Horizontal(y, x, RED))
                    {
                        linesHori[y, x] = RED;
                        //gameObject.GetComponent<Renderer>().material.color = Color.red;

                        hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red; ;

                        currentPlayer = BLUE;
                        if (y - 1 >= 0) DrowBoard(y-1, x , RED);
                        if (y < 6) DrowBoard(y, x, RED);
                        Debug.Log("REDがおいた");
                        

                    }
                    else if (currentPlayer == BLUE && CheckLineConected_Horizontal(y, x, BLUE))
                    {
                        linesHori[y, x] = BLUE;


                        hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.blue;

                        turnCount++;
                        currentPlayer = RED;
                        if (y - 1 >= 0) DrowBoard(y - 1, x, BLUE);
                        if (y < 6) DrowBoard(y, x, BLUE);
                        Debug.Log("BLUEがおいた");
                    }
                }
            }


            

        }

    }

    private void DrowBoard(int y, int x, int color)
    {
        var strQ = new Queue<Point_t>();
        Point_t start = new Point_t(y, x);
        int[,] visited = new int[6, 6];
        int[,] preSquares = new int[6, 6];
        int[,] preLinesVar = new int[7, 7];
        int[,] preLinesHori = new int[7, 7];
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                visited[i, j] = 0;
                preSquares[i, j] = squares[i, j];
            }
        }
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                preLinesVar[i, j] = linesVar[i, j];
                preLinesHori[i, j] = linesHori[i, j];
            }
        }
        preSquares[y, x] = color;
        visited[y, x] = 1;
        strQ.Enqueue(start);
        while (strQ.Count > 0)
        {
            Point_t here = strQ.Dequeue();
            preSquares[here.y, here.x] = color;
            
            for (int i = 0; i < 4; i++)
            {
                if (here.x + directions[i].x >= 0 && here.x + directions[i].x < 6 &&//配列がはみ出してないかの判定
                    here.y + directions[i].y >= 0 && here.y + directions[i].y < 6 &&
                    canGo(here.y, here.x, directions[i]) == EMPTY)// 進めるかどうかの判定
                {
                    if (i == 0) preLinesVar[here.y, here.x + 1] = DELETING; //囲まれてたら中の線を消すための前処理
                    if (i == 1) preLinesVar[here.y, here.x] = DELETING;
                    if (i == 2) preLinesHori[here.y + 1, here.x] = DELETING;
                    if (i == 3) preLinesHori[here.y, here.x] = DELETING;

                    if (visited[here.y + directions[i].y, here.x + directions[i].x] == 0)//訪問済みかの判定
                    {
                        visited[here.y + directions[i].y, here.x + directions[i].x] = visited[here.y, here.x] + 1;
                        Point_t enqueue = new Point_t(here.y + directions[i].y, here.x + directions[i].x);
                        enqueue.x = here.x + directions[i].x;
                        enqueue.y = here.y + directions[i].y;

                        strQ.Enqueue(enqueue);

                    }
                }
                else if (canGo(here.y, here.x, directions[i]) == EMPTY)
                {
                    if (here.x + directions[i].x < 0 || here.x + directions[i].x >= 6 ||//配列がはみ出してないかの判定
                     here.y + directions[i].y < 0 || here.y + directions[i].y >= 6)
                    {
                       
                        return;
                    }
                }


            }
        }
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                squares[i, j] = preSquares[i, j];
                if (squares[i, j] == RED) board[i, j].GetComponent<Renderer>().material.color = Color.red;
                if (squares[i, j] == BLUE) board[i, j].GetComponent<Renderer>().material.color = Color.blue;
            }
        }

        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {

                linesVar[i, j] = preLinesVar[i, j];
                linesHori[i, j] = preLinesHori[i, j];
                
                if (linesVar[i, j] == DELETING) { LineDelete(i, j - 0.5f); linesVar[i, j] = DELETED; }
                if (linesHori[i, j] == DELETING) { LineDelete(i - 0.5f, j); linesHori[i, j] = DELETED; }

            }
        }
    }
    private void LineDelete(float y,float x)
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector3(x,y,-10), new Vector3(0, 0, 1), 100);
        Destroy(hit.collider.gameObject); 
    }

    //方向の初期化
    private void SetUpDirections()
    {
        for (int i = 0; i < 4; i++)
        {
            directions[i] = new Point_t(0, 0);
        }
        directions[0].x = 1;
        directions[0].y = 0;
        directions[1].x = -1;
        directions[1].y = 0;
        directions[2].x = 0;
        directions[2].y = 1;
        directions[3].x = 0;
        directions[3].y = -1;
    }

    private int canGo(int y, int x, Point_t direct)
    {
        if (direct.y == 0)
        {
            if (direct.x > 0) return linesVar[y, x + 1];//東
            else return linesVar[y, x];//西
        }
        else
        {
            if (direct.y > 0) return linesHori[y + 1, x];//北
            else return linesHori[y, x];//南
        }
    }

    //配列情報を初期化する
    private void InitializeArray()
    {
        //for文を利用して配列にアクセスする
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                //配列を空（値を０）にする
                linesVar[i, j] = EMPTY;
                linesHori[i, j] = EMPTY;
                
        
            }
        }
        
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                squares[i, j] = EMPTY;
                GameObject cube = GameObject.Find("Cube ("+(i*6+j)+")");//オブジェクトを二次元配列に入れる
                board[i, j] = cube;
            }
            
               
        }

    }

    private bool CheckLineConected_Horizontal(int y,int x,int color)
    {
        if (turnCount == 1) return true;
        else if (linesVar[y, x] == color) return true; // 左上の判定
        else if (x - 1 >= 0 && linesHori[y, x - 1] == color) return true;  //左の判定
        else if (y - 1 >= 0 && linesVar[y - 1, x] == color) return true;  //左下の判定
        else if (x + 1 < 7 && linesVar[y, x + 1] == color) return true;  //右上の判定
        else if (x + 1 < 7 && linesHori[y, x + 1] == color) return true;  //右の判定
        else if ((y - 1 >= 0 && x + 1 < 7) && linesVar[y - 1, x + 1] == color) return true;//右下の判定
        else return false;   
    }

    private bool CheckLineConected_Vartical(int y, int x, int color)
    {
        if (turnCount == 1) return true;
        else if ((x - 1 >= 0 && y + 1 < 7) && linesHori[y + 1, x - 1] == color) return true; // 左上の判定
        else if (y + 1 < 7 && linesVar[y + 1, x] == color) return true;  //上の判定
        else if (x - 1 >= 0 && linesHori[y , x - 1] == color) return true;  //左下の判定
        else if (y + 1 < 7 && linesHori[y + 1,x] == color) return true;  //右上の判定
        else if (y - 1 >= 0 && linesVar[y - 1, x] == color) return true;  //下の判定
        else if (linesHori[y, x] == color) return true;//右下の判定
        else return false;
    }

    private void ScoreCount()
    {
        for(int i = 0; i < 6; i++)
        {
            for(int j = 0;j < 6; j++)
            {
                if (squares[i, j] == RED) scoreRed++;
                if (squares[i, j] == BLUE) scoreBlue++;
            }
        }

    }

   


}
