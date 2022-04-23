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

                client.Connect(ip, port);
                lbInfo.Items.Add("Connect to server "+ ip.ToString()+":"+port);
                ns = client.GetStream();
                thread = new Thread(o => ReceiveData((TcpClient)o));
                thread.Start(client);

            }
            catch(Exception ex)
            {
                MessageBox.Show("Problem Connection Server");
            }

        }

        private void bntSend_Click(object sender, RoutedEventArgs e)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(txtText.Text);
            ns.Write(buffer, 0, buffer.Length);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            ns.Close();
            client.Close();
            Console.WriteLine("disconnect from server!!");
        }
        private void ReceiveData(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                        lbInfo.Items.Add(Encoding.UTF8.GetString(receivedBytes, 0, byte_count));
                }));
            }
        }

        
    }
}
