using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;

public class morita : MonoBehaviour
{
    // Start is called before the first frame update
   WebSocket ws;
   void Start()
    {
        ws = new WebSocket("ws://localhost:3000/");
        
        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Open");
        };
        
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log( "このデータが来た-->" + e.Data);
        };
        
        ws.OnError += (sender, e) =>
        {
            Debug.Log("WebSocket Error Message: " + e.Message);
        };
        
        ws.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket Close");
        };
        
        ws.Connect();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            string send_message = "スペースがunityで押された";
            Debug.Log("送りますよー");
            ws.Send(send_message);
        }
    }
    void OnDestroy()
    {
        ws.Close();
        ws = null;
    }
}
