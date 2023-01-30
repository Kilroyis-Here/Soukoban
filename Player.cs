using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject[] Walls;

    private int renge = default;
    private float whereToGo_X = default;
    private float whereToGo_Y = default;

    private bool canMove = false;

    private Vector3 WTG;


    // Start is called before the first frame update
    void Awake()
    {
        GetWallPosition();
    }

    private void GetWallPosition()
    {
        Walls = GameObject.FindGameObjectsWithTag("Wall");
        //foreach (GameObject Wall in Walls)
        //{
        //    for (int i = 0; i < Walls.Length; i++)
        //    {
        //        WallPos[i] = Wall.transform.position;
        //    }
        //}

    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            this.gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
            whereToGo_X = this.gameObject.transform.position.x;
            whereToGo_Y = this.gameObject.transform.position.y + 1.0f;
            CheckCanPlayerMove();
            Move();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            this.gameObject.transform.eulerAngles = new Vector3(0, 0, 90);
            whereToGo_X = this.gameObject.transform.position.x - 1.0f;
            whereToGo_Y = this.gameObject.transform.position.y;
            CheckCanPlayerMove();
            Move();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.gameObject.transform.eulerAngles = new Vector3(0, 0, -90);
            whereToGo_X = this.gameObject.transform.position.x + 1.0f;
            whereToGo_Y = this.gameObject.transform.position.y;
            CheckCanPlayerMove();
            Move();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            this.gameObject.transform.eulerAngles = new Vector3(0, 0, 180);
            whereToGo_X = this.gameObject.transform.position.x;
            whereToGo_Y = this.gameObject.transform.position.y - 1.0f;
            CheckCanPlayerMove();
            Move();
        }
    }
    private void CheckCanPlayerMove()
    {
        WTG = new Vector3(whereToGo_X, whereToGo_Y, 0.0f);

        //壁配列とプレイヤーの移動位置を比較
        for (int count = 0; count < Walls.Length; count++)
        {
            if(WTG != Walls[count].transform.position)
            {
                canMove = true;
            }
            else
            {
                canMove = false;
            }
        }
    }
    // 指定された位置に存在するゲームオブジェクトを返します
    

    private void Move()
    {
        if (canMove)
        {
            this.gameObject.transform.position = WTG;
        }
        else
        {
            canMove = true;
        }
    }
}
