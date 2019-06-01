using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;

public class record_and_send : MonoBehaviour
{
       WebSocket ws;
    private AudioClip micClip;
    private float[] microphoneBuffer;
    private FileStream fileStream;
    int maxRecordingTime=300;
    int samplingFrequency=44100;
    private int head;
    private int position;
    public bool isRecording;
    string micDeviceName;
    int counter =0;
    string filePath="morita.wav";
    Byte[] _buffer;
    const int HEADER_SIZE = 44;
    const float rescaleFactor = 32767; //to convert float to Int16
    public int sendc=0;
    public void StartButton()
    {
        isRecording=true;
        StartCoroutine( WavRecording() ) ;
    }
    public void EndButton()
    {
        isRecording=false;
    }
    
    public IEnumerator WavRecording() {
        Debug.Log("録音スタート");
        //Fileストーム
        
        fileStream = new FileStream(filePath, FileMode.Create);
        //Head領域の事前に確保
        byte headerByte = new byte();
        for (int i = 0; i < HEADER_SIZE; i++) //preparing the header 44バイト
        {
            fileStream.WriteByte(headerByte);
        }
        
        
        //Buffer
        microphoneBuffer = new float[maxRecordingTime * samplingFrequency];
        //録音開始
        micClip = Microphone.Start(deviceName: micDeviceName, loop: false, lengthSec: maxRecordingTime, frequency: samplingFrequency);
        //位置を設定
        head = 0;
        counter=0;
        do
        {
            //ディバイス待ち（レンテシー　null）
            position = Microphone.GetPosition(null);
            
            if (position < 0 || head == position)
            {
                yield return null;
            }
            else
            {
                WavBufferwite(fileStream,head, position, micClip);
                head = position;
                sendc++;
            }
            yield return null;
            
        } while (isRecording);
        //マイク録音停止
        
        position = Microphone.GetPosition(null);
        Microphone.End(micDeviceName);
        //Bufferをファイル書き込みしファイナライズ
        WavBufferwite(fileStream,head, position, micClip);
        WavHeaderWrite(fileStream, micClip.channels, samplingFrequency);//サイズが決まってから
        Debug.Log("終わったよ");
        
    }
    
    private void WavBufferwite(FileStream _fileStream, int _head, int _position, AudioClip _clip)
    {
        //Bufferに音声データを取り込み
        _clip.GetData(microphoneBuffer, 0);
        Debug.Log("recClipGetData " + microphoneBuffer.Length + " HEAD " + _head + " Position " + _position);
        //音声データをFileに追加
        if (_head < _position)
        {
            for (int i = _head; i < _position; i++)
            {
                //
                _buffer = BitConverter.GetBytes((short)(microphoneBuffer[i] * rescaleFactor));
                _fileStream.Write(_buffer, 0, 2);
                //ws.Send(_buffer);
                StartCoroutine( sending( ) );
                counter++;
            }
        }
        else
        {
            for (int i = _head; i < microphoneBuffer.Length; i++)
            {
                //
                _buffer = BitConverter.GetBytes((short)(microphoneBuffer[i] * rescaleFactor));
                _fileStream.Write(_buffer, 0, 2);
                //Debug.Log (BitConverter.ToString( _buffer) );
                if (sendc%10==0){
                    StartCoroutine( sending( ) );}
                //ws.Send(_buffer);
                counter++;
            }
            for (int i = 0; i < _position; i++)
            {
                //
                _buffer = BitConverter.GetBytes((short)(microphoneBuffer[i] * rescaleFactor));
                _fileStream.Write(_buffer, 0, 2);
                StartCoroutine( sending( ) );
                //ws.Send(_buffer);
                counter++;
            }
        }
        
    }
    
    private void WavHeaderWrite(FileStream _fileStream,int channels,int samplingFrequency)
    {
        //サンプリング数を計算
        var samples = ((int)_fileStream.Length - HEADER_SIZE)/2;
        
        //おまじない
        _fileStream.Flush();
        _fileStream.Seek(0, SeekOrigin.Begin);
        
        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        _fileStream.Write(riff, 0, 4);
        
        Byte[] chunkSize = BitConverter.GetBytes(_fileStream.Length - 8);
        _fileStream.Write(chunkSize, 0, 4);
        
        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        _fileStream.Write(wave, 0, 4);
        
        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        _fileStream.Write(fmt, 0, 4);
        
        Byte[] subChunk1 = BitConverter.GetBytes(16);
        _fileStream.Write(subChunk1, 0, 4);
        
        //UInt16 _two = 2;
        UInt16 _one = 1;
        
        Byte[] audioFormat = BitConverter.GetBytes(_one);
        _fileStream.Write(audioFormat, 0, 2);
        
        Byte[] numChannels = BitConverter.GetBytes(channels);
        _fileStream.Write(numChannels, 0, 2);
        
        Byte[] sampleRate = BitConverter.GetBytes(samplingFrequency);
        _fileStream.Write(sampleRate, 0, 4);
        
        Byte[] byteRate = BitConverter.GetBytes(samplingFrequency * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
        _fileStream.Write(byteRate, 0, 4);
        
        UInt16 blockAlign = (ushort)(channels * 2);
        _fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);
        
        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        _fileStream.Write(bitsPerSample, 0, 2);
        
        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        _fileStream.Write(datastring, 0, 4);
        
        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        _fileStream.Write(subChunk2, 0, 4);
        
        //必ずクローズ
        _fileStream.Flush();
        _fileStream.Close();
    }
    
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        ws = new WebSocket("ws://localhost:3000/");
         ws.Connect();
        //マイクデバイスを探す
        foreach (string device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
            micDeviceName = device;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator  sending( ){
        //ws.Send (System.Text.Encoding.GetEncoding("Shift_JIS").GetString(_buffer) );
        ws.Send (_buffer);
      //ws.Send (BitConverter.ToString(_buffer) );
      yield return null;
    }
    
}
