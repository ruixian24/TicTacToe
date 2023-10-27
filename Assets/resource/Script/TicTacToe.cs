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
    

    // �ٵ��� �� 3*3 �迭 (ƽ���� ��) ����
    int[] board = new int[9];

    // ����Ʈ�� ũ�Ⱑ ������ ���� �ʾƼ� ���������� ��� ����
    // �迭�� �����ϸ� ũ�⸦ �����ϱ� ���� �ڵ带 ������ �����ؾ���

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
        // ���� ��� ���·� ����
        state = State.Game;
        // �� �� ����
        stoneTurn = Stone.White;

        // ����(��) �ٵϵ� = ��
        if (tcp.IsServer())
        {
            stoneI = Stone.White;
            stoneYou = Stone.Black;
        }

        // Ŭ���̾�Ʈ(���) �ٵϵ� = ������
        else
        {
            stoneI = Stone.Black;
            stoneYou = Stone.White;
        }
    }

    void UpdateGame()
    {
        bool bSet = false;

        // ���� �� ó��
        if (stoneTurn == stoneI)
            bSet = MyTurn();

        // Ŭ���̾�Ʈ �� ó��
        else
            bSet = YourTurn();

        // 
        if (bSet == false)
            return;

        stoneWinner = CheckBoard();

        if (stoneWinner != Stone.None)
        {
            state = State.End;
          
            Debug.Log("�¸�: " + (int)stoneWinner);
        }

        stoneTurn = (stoneTurn == Stone.White) ? Stone.Black : Stone.White;
    }

    void UpdateEnd()
    {
        
    }

    bool SetStone (int i, Stone stone)
    {
        // ��ġ�Ϸ��� ĭ�� ������ ��ġ���� �ʾ��� ��
        if (board[i] == (int)Stone.None)
        {
            // ĭ�� ���� ��ġ
            board[i] = (int)stone;
            return true;
        }

        // ��ġ�Ϸ��� ĭ�� ������ ��ġ�Ǿ� ������ false ����
        return false;
    }

    // ƽ���� �ǿ��� ��ġ�Ϸ��� ��ġ�� �ľ�
    // ���콺 Ŭ�� �� ��ġ ���� �´� board �ε��� ���� ����
    int PosToNumber(Vector3 pos)
    {
        // ���� ��ġ���� ���콺�� ��ġ���� ���� ������ �޶� ������ ����ؾ� ��
        float x = pos.x - 180;
        float y = Screen.height - 90 - pos.y;

        if (x < 0.0f || x >= 1080.0f) return -1;   // ��ȿ ��ȣ ����
        if (y < 0.0f || y >= 990.0f) return -1;   // ��ȿ ��ȣ ����

        int h = (int)(x / 300.0f);
        int v = (int)(y / 300.0f);
        
        int i = v * 3 + h;

        return i;
    }

    // �� �Ͽ��� �ٵϵ��� ��ġ�ϴ� ���� ����
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

        Debug.Log("����:" + i);

        return true;
    }

    bool YourTurn()
    {
        byte[] data = new byte[1]; 
        int iSize = tcp.Receive(ref data, data.Length);

        if (iSize <= 0) return false;

        // �ٵϵ� ����
        int i = (int)data[0];
        Debug.Log("����:" + i);

        bool ret = SetStone(i, stoneYou);
        if (ret == false) return false;

        return true;
    }

    // ƽ���� ��ġ üũ
    Stone CheckBoard()
    {
        for (int i = 0; i < 2; i++)
        {
            int s;
            if (i == 0)
                s = (int)Stone.White;
            else
                s = (int)Stone.Black;

            // ���� ���� üũ
            if (s == board[0] && s == board[1] && s == board[2])
                return (Stone)s;
            if (s == board[3] && s == board[4] && s == board[5])
                return (Stone)s;
            if (s == board[6] && s == board[7] && s == board[8])
                return (Stone)s;

            // ���� ���� üũ
            if (s == board[0] && s == board[3] && s == board[6])
                return (Stone)s;
            if (s == board[1] && s == board[4] && s == board[7])
                return (Stone)s;
            if (s == board[2] && s == board[5] && s == board[8])
                return (Stone)s;

            // �밢�� ���� üũ
            if (s == board[0] && s == board[4] && s == board[8])
                return (Stone)s;
            if (s == board[2] && s == board[4] && s == board[6])
                return (Stone)s;
        }

        // ƽ���� üũ �� ��ġ�ϴ� �κ��� ������ None ����
        return Stone.None;
    }

    // �ٵϵ��� ��ġ�ϴ� �Լ�
    // �̺�Ʈ �߻� �� �� �����Ӹ��� ȣ�� (Update���� ����)
    private void OnGUI()
    {
        if (!Event.current.type.Equals(EventType.Repaint))
            return;
        if (state == State.Game)
        {
            Graphics.DrawTexture(new Rect(180, 90, 900, 900), texBoard);

            // board ���� ���� �ٵϵ� ���
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
        // "Button" �±װ� ������ ��� ���� ������Ʈ�� ��Ȱ��ȭ�մϴ�.
        GameObject[] buttons = GameObject.FindGameObjectsWithTag("Button");
        
        foreach (GameObject button in buttons)
        {
            button.SetActive(false);
        }
    }
}
