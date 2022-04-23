using LibMessage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //IPAddress ip = IPAddress.Parse("127.0.0.1");
        //int port = 5000;
        TcpClient client = new TcpClient();
        NetworkStream ns;
        Thread thread;
        private ChatMessage _message = new ChatMessage(); 


        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fileName = "setting.txt";
                IPAddress ip;
                int port;
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        ip = IPAddress.Parse(sr.ReadLine());
                        port = int.Parse(sr.ReadLine());
                    }
                }
                _message.UserName = txtUserName.Text;
                _message.UserId = Guid.NewGuid().ToString();
                client.Connect(ip, port);
                lbInfo.Items.Add("Підключення до сервера "+ ip.ToString()+":"+port);
                ns = client.GetStream();
                thread = new Thread(o => ReceiveData((TcpClient)o));
                thread.Start(client);

                _message.MessageType = TypeMessage.Login;
                _message.Text = "Приєднався до чату";
                byte[] buffer = _message.Serialize(); //Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_message));
                ns.Write(buffer, 0, buffer.Length);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Problem Connection Server "+ ex.Message);
            }

        }

        private void bntSend_Click(object sender, RoutedEventArgs e)
        {
            _message.MessageType = TypeMessage.Message;
            _message.Text = txtText.Text;
            byte[] buffer = _message.Serialize(); //Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_message));
            ns.Write(buffer, 0, buffer.Length);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _message.MessageType = TypeMessage.Logout;
            _message.Text = "Покинув чат";
            byte[] buffer = _message.Serialize(); //Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_message));
            ns.Write(buffer, 0, buffer.Length);

            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            ns.Close();
            client.Close();
            //Console.WriteLine("disconnect from server!!");
        }
        private void ReceiveData(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;
            string data="";
            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {

                        ChatMessage message = ChatMessage.Desserialize(receivedBytes);
                        switch (message.MessageType)
                        {
                            case TypeMessage.Login:
                                {
                                    if (message.UserId != _message.UserId)
                                        lbInfo.Items.Add(message.UserName + ":" + message.Text);
                                    break;

                                }
                            case TypeMessage.Logout:
                                {
                                    if (message.UserId != _message.UserId)
                                        lbInfo.Items.Add(message.UserName + ":" + message.Text);
                                    break;

                                }
                            case TypeMessage.Message:
                                {
                                    lbInfo.Items.Add(message.UserName + ":" + message.Text);
                                    break;

                                }
                        }
                        lbInfo.Items.MoveCurrentToLast();
                        lbInfo.ScrollIntoView(lbInfo.Items.CurrentItem);
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("Deserialize object problem " + ex.Message);
                    }

                }));
                //data += Encoding.UTF8.GetString(receivedBytes, 0, byte_count);

            }
            
           
        }

        
    }
}
