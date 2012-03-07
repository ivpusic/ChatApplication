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
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ChatServerApp
{

    public partial class MainWindow : Window
    {
        private delegate void UpdateStatusCallback(string strMessage);
        private string ipAdress = "127.0.0.1";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnListen_Click(object sender, RoutedEventArgs e)
        {
            btnListen.Content = "Server pokrenut";

            IPAddress ipAddr = IPAddress.Parse(ipAdress);

            ChatServer mainServer = new ChatServer(ipAddr);

            ChatServer.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged);
 
            mainServer.StartListening();
  
            txtLog.Items.Add("Cekam zahtjeve klijenata...\r\n");
        }

        public void mainServer_StatusChanged(object sender, StatusChangedEventArgs e)
        {
        
            this.Dispatcher.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage });
        }

        private void UpdateStatus(string strMessage)
        {
   
            txtLog.Items.Add(strMessage + "\r\n");
        }

        private void ChatServet_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ChatServer.RequestStop();
            Application.Current.Shutdown();
        }
    }
}
