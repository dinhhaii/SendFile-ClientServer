using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Server
{
    public partial class Server : Form
    {
        private const int MAXLENGTH = 1024 * 1024;

        public Server()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Connect();
        }

        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            foreach (Socket item in clientList)
            {
                Send(item);
            }
            listViewStatus.Items.Add(new ListViewItem() { Text = "Sent successfully - " + txbMessage.Text });
            txbMessage.Clear();
            
        }
        IPEndPoint IP;
        Socket server;
        List<Socket> clientList;    

        void Connect()
        {
            clientList = new List<Socket>();
            IP = new IPEndPoint(IPAddress.Any, 9999);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            server.Bind(IP);

            Thread Listen = new Thread(()=> {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        clientList.Add(client);

                        Thread receive = new Thread(Receive);
                        receive.IsBackground = true;
                        receive.Start(client);
                    }
                }
                catch
                {
                    IP = new IPEndPoint(IPAddress.Any, 9999);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
               
                

            });
            Listen.IsBackground = true;
            Listen.Start();
        }

        void Close()
        {
            server.Close();
        }

        void Send(Socket client)
        {

            if (client != null && txbMessage.Text != string.Empty)
            {
                client.Send(Encoding.ASCII.GetBytes(splitFileName(txbMessage.Text)));

                BinaryReader binaryReader = new BinaryReader(new FileStream(txbMessage.Text,FileMode.Open));
                progressBarServer.Maximum = (int)binaryReader.BaseStream.Length;

                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    byte[] data = new byte[MAXLENGTH];
                    data = binaryReader.ReadBytes(MAXLENGTH);
                    client.Send(data);
                    progressBarServer.Value += data.Length;
                }

                progressBarServer.Value = 0;
                binaryReader.Close();

            }
               
        }
    
        void Receive(object obj)
        {
            while (true)
            {
                Socket client = obj as Socket;
                byte[] data = new byte[MAXLENGTH];
                int rev = client.Receive(data);
                string message = Encoding.ASCII.GetString(data, 0, rev);
                string filename = Directory.GetCurrentDirectory() + @"\" + message;
                BinaryWriter bw = new BinaryWriter(new FileStream(filename, FileMode.Create));
                bw.Close();


                while (true)
                {
                    data = new byte[MAXLENGTH];
                    int receiveByte = client.Receive(data);

                    using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(filename, FileMode.Append)))
                    {
                        binaryWriter.Write(data, 0, receiveByte);
                    }

                    //string message = (string)Deseriallize(data);
                    AddMessage(receiveByte.ToString());
                    if (receiveByte < MAXLENGTH)
                    {
                        break;
                    }
                }
                listViewStatus.Items.Add(new ListViewItem() { Text = "Received successfully - " + message});

            }


        }
        void AddMessage(string s)
        {
            lsvMessage.Items.Add(new ListViewItem() { Text = s });
        }

        byte[] Seriallize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, obj);
            return stream.ToArray();
        }
        object Deseriallize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();


            return formatter.Deserialize(stream);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Create OpenFileDialog 
            OpenFileDialog dlg = new OpenFileDialog();


            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".*";
            dlg.Filter = "All Files (*.*)|*.*";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<DialogResult> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result != DialogResult.Cancel)
            {
                // Open document 
                string filename = dlg.FileName;
                txbMessage.Text = filename;
            }
        }

        string splitFileName(string path)
        {
           
            StringBuilder directory = new StringBuilder("");
            StringBuilder fileName = new StringBuilder("");
            StringBuilder fileExtension = new StringBuilder("");

            string[] tokens = path.Split(new string[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tokens.Length; i++)
            {
                tokens[i] = tokens[i].Trim();
            }
            if (tokens.Length != 0)
            {
                int index = tokens[tokens.Length - 1].LastIndexOf('.');
                if (index != -1)
                {
                    fileExtension.Append(tokens[tokens.Length - 1].Substring(index, tokens[tokens.Length - 1].Length - index));
                    fileName.Append(tokens[tokens.Length - 1].Substring(0, index));
                    for (int i = 0; i < tokens.Length - 1; i++)
                    {
                        directory.Append(tokens[i]);
                        directory.Append("\\");
                    }
                }
                else
                {
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        directory.Append(tokens[i]);
                        directory.Append("\\");
                    }
                }
            }
            StringBuilder output = new StringBuilder("");
            output.Append(fileName);
            output.Append(fileExtension);
            return output.ToString();

        }
    }
}
