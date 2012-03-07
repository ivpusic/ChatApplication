using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;


namespace ChatApplication
{

    public partial class MainWindow : Window
    {
        private string ipAdress = "127.0.0.1";
        private string UserName = "Unknown";
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpServer;

        private delegate void UpdateLogCallback(string strMessage);
    
        private delegate void CloseConnectionCallback(string strReason);
        private Thread thrMessaging;
        private IPAddress ipAddr;
        private bool Connected;


        public MainWindow()
        {
            InitializeComponent();
            sendButton.IsEnabled = false;
            message.IsEnabled = false;
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
    
            if (Connected == false)
            {
              
                InitializeConnection();
            }
            else
            {
                CloseConnection("LOGOUT");
            }
        }

        private void InitializeConnection()
        {
       
            ipAddr = IPAddress.Parse(ipAdress);
         
            tcpServer = new TcpClient();
            tcpServer.Connect(ipAddr, 1986);

        
            Connected = true;
         
            UserName = usrName.Text;

        
            usrName.IsEnabled = false;
            message.IsEnabled = true;
            sendButton.IsEnabled = true;
            connectButton.Content = "Odjavi se";

       
            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine(usrName.Text);
            swSender.Flush();

        
            thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
            thrMessaging.Start();
        }

        private void ReceiveMessages()
        {
            
            srReceiver = new StreamReader(tcpServer.GetStream());
        
            string ConResponse = srReceiver.ReadLine();
           
            if (ConResponse[0] == '1')
            {
      
                this.Dispatcher.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Uspjesno spojeno!" });
            }
            else 
            {
                string Reason = "Not Connected: ";
        
                Reason += ConResponse.Substring(2, ConResponse.Length - 2);

                this.Dispatcher.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { Reason });
              
                return;
            }
      
            while (Connected)
            {
                if (srReceiver != null) // !!!!!!
                {
                  
                    this.Dispatcher.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { srReceiver.ReadLine() });
                }
            }
        }

       
        private void UpdateLog(string strMessage)
        {
            if (!Connected) return;
      
            listBoxMessage.Items.Add(strMessage + "\r\n");
        }


        private void CloseConnection(string Reason)
        {
            swSender.Flush();
        
            listBoxMessage.Items.Add(Reason + "\r\n");
          
            usrName.IsEnabled = true;
            message.IsEnabled = false;
            sendButton.IsEnabled = false;
            connectButton.Content = "Spoji";

        
            Connected = false;
            srReceiver.Close();
        
            thrMessaging.Abort();
            swSender.Close();
       
            tcpServer.Close();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

     
        private void SendMessage()
        {
            if (message.LineCount >= 1)
            {
                swSender.WriteLine(message.Text);
                swSender.Flush();
                message.Text = null;
            }
            message.Text = "";
        }

        private void message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
         
            Connected = false;
            if(srReceiver != null)
                srReceiver.Close();
         
            if(thrMessaging != null)
                thrMessaging.Abort();
            if(swSender != null)
                swSender.Close();
           
            if(tcpServer != null)
                tcpServer.Close();
        }

    }
}
