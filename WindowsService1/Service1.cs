using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        Uri Wsurl = new Uri("ws://localhost:3000");
        ClientWebSocket client = new ClientWebSocket();

         
        public Service1()
        {
            InitializeComponent();
        }

        private async Task<string> HttpToTallyAsync(String inXml)
        {
            try
            {

                var url = "http://localhost:9000/";

                 var client = new HttpClient();


                var BodyForPost = new StringContent(inXml, Encoding.UTF8, "text/xml");
                var response = await client.PostAsync(url, BodyForPost);

                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception error)
            {
                return null;
            };
        }

        private async Task<string> HttpGetTallyAsync()
        {
            try
            {

                var url = "http://localhost:9000/";

                var client = new HttpClient();

var response = await client.GetAsync(url);

                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception error)
            {
                return null;
            };
        }

private async Task receiveTask()
        {
            var buffer = new byte[1024*4];
            while (true)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType ==WebSocketMessageType.Close)
                {
                    break;
                }
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                WriteToFile("from ws : " + message);
            }

        }

        protected override async void OnStart(string[] args)
        {
            try
            {
                WriteToFile("Service is started at " + DateTime.Now);
                timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
                timer.Interval = 10000; //number in milisecinds
                timer.Enabled = true;

                await client.ConnectAsync(Wsurl,CancellationToken.None);
                
                string msg = "hello0123456789123456789123456789123456789123456789123456789";
                SendMessage(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)));

                await receiveTask();
            }
            catch (Exception error)
            {
                WriteToFile("error " + error);
            };

        }

        private async void SendMessage(ArraySegment<byte> inDataToSend)
        {
            try
            {
                // restricted to 5 iteration only
                if (client.State == WebSocketState.Open)
                {
                    await client.SendAsync(inDataToSend, WebSocketMessageType.Text,
                                         true, CancellationToken.None);
                }
            }
            catch (Exception error)
            {
                WriteToFile("error " + error);
            };
        }

        protected override void OnStop()
        {
        }
        private void ReadXmlToLog()
        {
            string filePath = @"D:\KeshavSoft\datas\kkdcycle\LedgerNamesOnly.xml";
            string filePath1 = @"D:\KeshavSoft\datas\kkdcycle\LedgerNamesOnly.xml";

            XmlDocument xmlDoc = new XmlDocument();

            if (File.Exists(filePath))
            {
                xmlDoc.Load(filePath);
            };

            WriteToFile(xmlDoc.InnerXml);


        }

        private string ReadXml()
        {
            string filePath1 = @"D:\KeshavSoft\datas\kkdcycle\LedgerNamesOnly.xml";
            
            //string filePath = @"D:\KeshavSoft\datas\kkdcycle\LedgerNamesOnly.xml";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "LedgerNamesOnly.xml");

            //  return filePath;
            XmlDocument xmlDoc = new XmlDocument();

            if (File.Exists(filePath))
            {
                xmlDoc.Load(filePath);
            };

            return xmlDoc.InnerXml;

        }

        private string k1()
        {

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "LedgerNamesOnly.xml");

            var resourcedata = WindowsService1.Properties.Resources.LedgerNamesOnly;

            return filePath;

        }
        private async void OnElapsedTime(object source, ElapsedEventArgs e)
        {

            WriteToFile("Service is recall at " + await HttpGetTallyAsync());
            String ToSend = await HttpToTallyAsync(WindowsService1.Properties.Resources.LedgerNamesOnly);

            //WriteToFile("Ledgers " +await HttpToTallyAsync( WindowsService1.Properties.Resources.LedgerNamesOnly));
            SendMessage(new ArraySegment<byte>(Encoding.UTF8.GetBytes("From Service")));

        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }

    }
}
