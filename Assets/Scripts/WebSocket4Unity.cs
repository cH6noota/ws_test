using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace WebSocket4Unity
{
    public class WebSocket4Unity : WebSocket
    {
        public event EventHandler OnOpened;
        public event EventHandler<ErrorEventArgs> OnErrored;
        public event EventHandler<MessageEventArgs> OnMessaged;
        public event EventHandler<CloseEventArgs> OnClosed;
        public WebSocket4Unity(string url, params string[] protocols) : base(url, protocols)
        {
            Interaction.Initialize();
            OnOpen += open;
            OnError += error;
            OnMessage += message;
            OnClose += close;
        }
        private void open(object sender, EventArgs e)
        {
            Interaction.QueueOnMainThread((p1, p2) =>
            {
                if(OnOpened!=null)
                {
                    OnOpened.Emit(p1, (EventArgs)p2);
                }
            }, sender, e);
        }
        private void error(object sender, ErrorEventArgs e)
        {
            Interaction.QueueOnMainThread((p1, p2) =>
            {
                if (OnErrored != null)
                {
                    OnErrored.Emit(p1, (ErrorEventArgs)p2);
                }
            }, sender, e);
        }
        private void message(object sender, MessageEventArgs e)
        {
            Interaction.QueueOnMainThread((p1, p2) =>
            {
                if (OnMessaged != null)
                {
                    OnMessaged.Emit(p1, (MessageEventArgs)p2);
                }
            }, sender, e);
        }
        private void close(object sender, CloseEventArgs e)
        {
            Interaction.QueueOnMainThread((p1, p2) =>
            {
                if (OnClosed != null)
                {
                    OnClosed.Emit(p1, (CloseEventArgs)p2);
                }
            }, sender, e);
        }
    }
}
