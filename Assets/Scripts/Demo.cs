using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Net;
using System.Threading;

namespace WebSocket4Unity
{
    public class Demo : MonoBehaviour
    {
        WebSocket4Unity webSocket4Unity;
        public Button _btn;
        public Text _info1;
        public Text _info2;

        // Use this for initialization
        void Start()
        {
            //在线测试地址（可能有变动，可以自行百度查询）
            webSocket4Unity = new WebSocket4Unity("ws://118.25.40.163:8088");

            webSocket4Unity.OnOpened += new EventHandler(websocketOpend);
            webSocket4Unity.OnErrored += new EventHandler<ErrorEventArgs>(websocketError);
            webSocket4Unity.OnClosed += new EventHandler<CloseEventArgs>(websocketClosed);
            webSocket4Unity.OnMessaged += new EventHandler<MessageEventArgs>(websocketReceived);
            webSocket4Unity.EmitOnPing = true;
            webSocket4Unity.Connect();

            _btn.onClick.AddListener(onclick);
        }
        private void onclick()
        {
            webSocket4Unity.Send("WebSocket4Unity 测试!!!");
        }
        public void OnDisable()
        {
            if (webSocket4Unity != null)
            {
                webSocket4Unity.Close();
            }
        }
        private void websocketOpend(object sender, EventArgs e)
        {
            _info1.text = sender.ToString();
            _info2.text = e.ToString();
        }
        private void websocketError(object sender, EventArgs e)
        {
            _info1.text = sender.ToString();
            _info2.text = e.ToString();
        }
        private void websocketReceived(object sender, MessageEventArgs e)
        {
            if (e.IsBinary)
            {
                _info1.text = System.Text.Encoding.UTF8.GetString(e.RawData);
                _info2.text = System.Text.Encoding.UTF8.GetString(e.RawData);
            }
            else if(e.IsPing)
            {
                _info1.text = e.Data;
                _info2.text = System.Text.Encoding.UTF8.GetString(e.RawData);
            }
            else if(e.IsText)
            {
                _info1.text = e.Data;
                _info2.text = e.Data;
            }
        }
        private void websocketClosed(object sender, EventArgs e)
        {
        }
    }
}