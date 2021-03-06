﻿using SharpOSC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Master;

namespace EmotionViewer
{
    public class Sender
    {
        Stopwatch timer = new Stopwatch();
        List<int> valenceList = new List<int>();
        int person = 0;
        VideoPacket packet;

        private string[] EmotionLabels = { "ANGER", "CONTEMPT", "DISGUST", "FEAR", "JOY", "SADNESS", "SURPRISE" };
        private string[] SentimentLabels = { "NEGATIVE", "POSITIVE", "NEUTRAL" };

        public Sender()
        {
            timer.Start();
            packet = new VideoPacket(person);
        }

        public void setPerson(int person)
        {
            this.person = person;
        }

        public void update(string emotion, float emotionIntensity, string valence, float valenceIntensity)
        {
            /* 
            initialize OSC object..
            *                          * 
            * Collect 30 seconds of  Valence  in an array .
            * Then find  the most occured 
            * 
            * Send the via OSC
            */
            if (timer.Elapsed.TotalSeconds < TimeSpan.FromSeconds(3).TotalSeconds)
            {
                // valenceList.Add(valenceID);
                VideoFrame frame = new VideoFrame(DateTime.Now, emotion, emotionIntensity, valence, valenceIntensity);
                packet.addVideoFrame(frame);
            }
            else
            {
                //var mostOccured = valenceList.GroupBy(i => i).OrderByDescending(grp => grp.Count())
                //            .Select(grp => grp.Key).First();
                // Send the valence to the affective model to be checked
                TransmitData(packet);
                valenceList.Clear();

                packet = new VideoPacket(person);

                // reset and start timer
                timer.Reset();
                timer.Start();
            }
        }

        private void TransmitData(VideoPacket packet)
        {
            var message = new OscMessage("/test/1", ObjectToString(packet));
            var udpSender = new UDPSender("127.0.0.1", 55555);
            udpSender.Send(message);
        }

        public string ObjectToString(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}
