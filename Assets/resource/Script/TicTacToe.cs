using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToe : MonoBehaviour
{
    enum State
    {
        Start = 0,
        Game,
        End,
    }

    enum Stone
    {
        None = 0,
        White,
        Black,
    }

    enum Turn
    {
        I = 0,
        You,
    }

    Tcp tcp;
    public InputField ip;
    public Texture texBoard;
    public Texture texWhite;
    public Texture texBlack;
    public Texture whiteWinner;
    public Texture blackWinner;
    

    // 바둑판 위 3*3 배열 (틱택토 판) 선언
    int[] board = new int[9];

    // 리스트는 크기가 정해져 있지 않아서 유동적으로 사용 가능
    // 배열로 선언하면 크기를 변경하기 위해 코드를 일일히 수정해야함

    State state;
    Stone stoneTurn;
    Stone stoneI;
    Stone stoneYou;
    Stone stoneWinner;

    void Start()
    {
        tcp = GetComponent<Tcp>();
        state = State.Start;

        for (int i = 0; i < board.Length; ++i)
        {
            board[i] = (int)Stone.None;
        }
    }

    public void ServerStart()
    {
        tcp.StartServer(10000, 10);
    }

    public void ClientStart()
    {
        tcp.Connect(ip.text, 10000);
    }

    void Update()
    {
        if (!tcp.IsConnect()) return;

        if (state == State.Start)
            UpdateStart();
            DisableUIButtons();

        if (state == State.Game)
            UpdateGame();

        if (state == State.End)
            UpdateEnd();
    }

    
    void UpdateStart()
    {
        // 게임 모드 상태로 변경
        state = State.Game;
        // 흰 돌 차례
        stoneTurn = Stone.White;

        // 서버(나) 바둑돌 = 흰돌
        if (tcp.IsServer())
        {
            stoneI = Stone.White;
            stoneYou = Stone.Black;
        }

        // 클라이언트(상대) 바둑돌 = 검은돌
        else
        {
            stoneI = Stone.Black;
            stoneYou = Stone.White;
        }
    }

    void UpdateGame()
    {
        bool bSet = false;

        // 서버 턴 처리
        if (stoneTurn == stoneI)
            bSet = MyTurn();

        // 클라이언트 턴 처리
        else
            bSet = YourTurn();

        // 
        if (bSet == false)
            return;

        stoneWinner = CheckBoard();

        if (stoneWinner != Stone.None)
        {
            state = State.End;
          
            Debug.Log("승리: " + (int)stoneWinner);
        }

        stoneTurn = (stoneTurn == Stone.White) ? Stone.Black : Stone.White;
    }

    void UpdateEnd()
    {
        
    }

    bool SetStone (int i, Stone stone)
    {
        // 배치하려는 칸에 스톤이 배치되지 않았을 때
        if (board[i] == (int)Stone.None)
        {
            // 칸에 스톤 배치
            board[i] = (int)stone;
            return true;
        }

        // 배치하려는 칸에 스톤이 배치되어 있으면 false 리턴
        return false;
    }

    // 틱택토 판에서 배치하려는 위치를 파악
    // 마우스 클릭 시 위치 값에 맞는 board 인덱스 값을 리턴
    int PosToNumber(Vector3 pos)
    {
        // 실제 위치값과 마우스의 위치값은 축의 방향이 달라 역으로 계산해야 함
        float x = pos.x - 180;
        float y = Screen.height - 90 - pos.y;

        if (x < 0.0f || x >= 1080.0f) return -1;   // 유효 번호 없음
        if (y < 0.0f || y >= 990.0f) return -1;   // 유효 번호 없음

        int h = (int)(x / 300.0f);
        int v = (int)(y / 300.0f);
        
        int i = v * 3 + h;

        return i;
    }

    // 내 턴에서 바둑돌을 배치하는 로직 구현
    bool MyTurn()
    {
        bool bClick = Input.GetMouseButtonDown(0);
        if (!bClick) return false;

        Vector3 pos = Input.mousePosition;

        int i = PosToNumber(pos);
        if (i == -1) return false;

        bool bSet = SetStone(i, stoneI);
        if (bSet == false) return false;

        byte[] data = new byte[1];
        data[0] = (byte)i;
        tcp.Send(data, data.Length);

        Debug.Log("보냄:" + i);

        return true;
    }

    bool YourTurn()
    {
        byte[] data = new byte[1]; 
        int iSize = tcp.Receive(ref data, data.Length);

        if (iSize <= 0) return false;

        // 바둑돌 세팅
        int i = (int)data[0];
        Debug.Log("받음:" + i);

        bool ret = SetStone(i, stoneYou);
        if (ret == false) return false;

        return true;
    }

    // 틱택토 배치 체크
    Stone CheckBoard()
    {
        for (int i = 0; i < 2; i++)
        {
            int s;
            if (i == 0)
                s = (int)Stone.White;
            else
                s = (int)Stone.Black;

            // 가로 방향 체크
            if (s == board[0] && s == board[1] && s == board[2])
                return (Stone)s;
            if (s == board[3] && s == board[4] && s == board[5])
                return (Stone)s;
            if (s == board[6] && s == board[7] && s == board[8])
                return (Stone)s;

            // 세로 방향 체크
            if (s == board[0] && s == board[3] && s == board[6])
                return (Stone)s;
            if (s == board[1] && s == board[4] && s == board[7])
                return (Stone)s;
            if (s == board[2] && s == board[5] && s == board[8])
                return (Stone)s;

            // 대각선 방향 체크
            if (s == board[0] && s == board[4] && s == board[8])
                return (Stone)s;
            if (s == board[2] && s == board[4] && s == board[6])
                return (Stone)s;
        }

        // 틱택토 체크 시 일치하는 부분이 없으면 None 리턴
        return Stone.None;
    }

    // 바둑돌을 배치하는 함수
    // 이벤트 발생 시 매 프레임마다 호출 (Update보다 후위)
    private void OnGUI()
    {
        if (!Event.current.type.Equals(EventType.Repaint))
            return;
        if (state == State.Game)
        {
            Graphics.DrawTexture(new Rect(180, 90, 900, 900), texBoard);

            // board 값에 따라 바둑돌 출력
            for (int i = 0; i < board.Length; ++i)
            {
                if (board[i] != (int)Stone.None)
                {
                    float x = 215 + (i % 3) * 295;
                    float y =  125 + (i / 3) * 300;

                    Texture tex = (board[i] == (int)Stone.White) ? texWhite : texBlack;
                    Graphics.DrawTexture(new Rect(x, y, 250, 250), tex);
                }
            }
        }

        if (state == State.End)
        {
           
            if (stoneWinner == Stone.White)
                Graphics.DrawTexture(new Rect(400, 300, 400, 400), whiteWinner);
            else
                Graphics.DrawTexture(new Rect(400, 300, 400, 400), blackWinner);
        }
    }

    void DisableUIButtons()
    {
        // "Button" 태그가 지정된 모든 게임 오브젝트를 비활성화합니다.
        GameObject[] buttons = GameObject.FindGameObjectsWithTag("Button");
        
        foreach (GameObject button in buttons)
        {
            button.SetActive(false);
        }
    }
}
