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

namespace Client
{
    public partial class Client : Form
    {
        private const int MAXLENGTH = 1024 * 1024;

        public Client()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            textBoxIP.Text = "127.0.0.1";
            textBoxPort.Text = "9999";
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            Send();
            
        }

        IPEndPoint IP;
        Socket client;

        void Connect(string ip, int port)
        {
            IP = new IPEndPoint(IPAddress.Parse(ip), port);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            try
            {
                client.Connect(IP);
                MessageBox.Show("Connected Successfully!");
            }
            catch
            {
                MessageBox.Show("Can't connect Server!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();
        }

        void Close()
        {
            client.Close();
        }

        void Send()
        {
            if (txbMessage.Text != string.Empty)
            {

                BinaryReader binaryReader = new BinaryReader(new FileStream(txbMessage.Text, FileMode.Open));
                progressBarClient.Maximum = (int)binaryReader.BaseStream.Length;
  
                client.Send(Encoding.ASCII.GetBytes(splitFileName(txbMessage.Text)));

                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    byte[] data = new byte[MAXLENGTH];
                    data = binaryReader.ReadBytes(MAXLENGTH);
                    progressBarClient.Value += data.Length;
                    client.Send(data);
                }

                listViewStatus.Items.Add(new ListViewItem() { Text = "Sent successfully - " + txbMessage.Text });
                MessageBox.Show("Transfer successfully!");
                progressBarClient.Value = 0;
                
            }
        }

        void Receive()
        {

            if (textBoxLocated.Text != null && textBoxLocated.Text.Length != 0)
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    int rev = client.Receive(data);
                    string message = Encoding.ASCII.GetString(data,0,rev);
                    string path = textBoxLocated.Text + message;

                    BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.Create));
                    bw.Close();

                    while (true)
                    {
                        data = new byte[MAXLENGTH];
                        int receiveByte = client.Receive(data);

                        using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(path, FileMode.Append)))
                        {
                            binaryWriter.Write(data, 0, receiveByte);
                        }

                        //string message = (string)Deseriallize(data);

                        AddMessage(receiveByte.ToString());

                        if (receiveByte < MAXLENGTH)
                        {
                            listViewStatus.Items.Add(new ListViewItem() { Text = "Received successfully - " + message });
                            break;
                        }

                    }
                }
            }
            
  


        }
        void AddMessage(string s)
        {
            lsvMessage.Items.Add(new ListViewItem() { Text = s });
            txbMessage.Clear();
        }
        //byte[] Seriallize(object obj)
        //{
        //    MemoryStream stream = new MemoryStream();
        //    BinaryFormatter formatter = new BinaryFormatter();
        //
        //    formatter.Serialize(stream, obj);
        //    return stream.ToArray();
        //}
        //object Deseriallize(byte[] data)
        //{
        //    MemoryStream stream = new MemoryStream(data);
        //    BinaryFormatter formatter = new BinaryFormatter();
        //
        //
        //    return formatter.Deserialize(stream);
        //}

        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void Client_Load(object sender, EventArgs e)
        {

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

        private void connectButton_Click(object sender, EventArgs e)
        {
            if(textBoxIP.Text != null && textBoxPort.Text != null)
            {
                int temp;
                int port;
                if(int.TryParse(textBoxPort.Text, out temp)){
                    port = int.Parse(textBoxPort.Text);
                    Connect(textBoxIP.Text, port);
                }
                
            }
        }
    }
}
