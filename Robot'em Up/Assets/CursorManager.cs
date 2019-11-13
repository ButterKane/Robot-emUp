using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public Camera MainCamera;
    public Image Player1Cursor;
    public Image Player2Cursor;
    public Image BallCursor;
    

    private Transform _playerOne;
    private Transform _playerTwo;
    private Transform _ball;

    // Start is called before the first frame update
    void Start()
    {
        _playerOne = GameManager.i.playerOne.transform;
        _playerTwo = GameManager.i.playerTwo.transform;
        _ball = GameManager.i.ball.transform;
    }

    // Update is called once per frame
    void Update()
    {
        Player1Cursor.transform.position = MainCamera.WorldToScreenPoint(_playerOne.position);
        Player2Cursor.transform.position = MainCamera.WorldToScreenPoint(_playerTwo.position);
        if (_ball == null )
        {
            _ball = GameManager.i.ball.transform;
            Debug.Log("_ball null");
        }
        BallCursor.transform.position = MainCamera.WorldToScreenPoint(_ball.position);

        Player1Cursor.transform.position = new Vector3(Player1Cursor.transform.position.x, Player1Cursor.transform.position.y + 20, Player1Cursor.transform.position.z);
        Player2Cursor.transform.position = new Vector3(Player2Cursor.transform.position.x, Player2Cursor.transform.position.y + 20, Player2Cursor.transform.position.z);
        BallCursor.transform.position = new Vector3(BallCursor.transform.position.x, BallCursor.transform.position.y + 20, BallCursor.transform.position.z);
    }
}
