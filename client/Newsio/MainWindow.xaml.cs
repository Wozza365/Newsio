using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Timers;
using System.Windows.Threading;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        string gamemode;
        string characterOne;
        string characterTwo;
        string location;
        ImageBrush canvasBack;
        bool isSinglePlayer = true;

        string usernameGlobal;

        Thickness marginReg;
        Thickness marginLog;

        DispatcherTimer buttonsMove; //animate starting buttons
        DispatcherTimer GameLoop;

        bool moved = false;
        public Socket sender;
        string[] currentSettings = new string[5];

        string[,] dividedResults;

        CustomChoiceWindow customMenu;

        public int customMenuResult=-1;

        public MainWindow()
        {
            InitializeComponent();
            //gameCanvas = new Canvas();
            //gameCanvas.Visibility = Visibility.Hidden;


            marginReg = new Thickness(511, 560, 0, 0);
            marginLog = new Thickness(511, 525, 0, 0);

            RegisterButton.Margin = marginReg;
            LoginButton.Margin = marginLog;

            //Connect to the server
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");

                IPAddress ipAddress = null;
                foreach (var addr in ipHostInfo.AddressList)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)        // this is IPv4
                    {
                        ipAddress = addr;
                        break;
                    }
                }

                if (ipAddress == null)
                    throw new Exception("Error finding an IPv4 address for localhost");
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 4000);
                sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(remoteEP);
                byte[] ba = new byte[10];
                sender.Receive(ba);
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            //GetLatestGame(isSinglePlayer);
        }

        public void LoadMainMenu()
        {
            //Hide login
            userLabel.Visibility = Visibility.Hidden;
            Username.Visibility = Visibility.Hidden;
            passLabel.Visibility = Visibility.Hidden;
            Password.Visibility = Visibility.Hidden;
            LoginButton.Visibility = Visibility.Hidden;
            RegisterButton.Visibility = Visibility.Hidden;
            resultLabel.Visibility = Visibility.Hidden;
            emailLabel.Visibility = Visibility.Hidden;
            EmailAddress.Visibility = Visibility.Hidden;

            //Show main menu
            SingleplayerCustom.Visibility = Visibility.Visible;
            SingleplayerCurrent.Visibility = Visibility.Visible;
            Multiplayer.Visibility = Visibility.Visible;
            Exit.Visibility = Visibility.Visible;
        }

        public void Login()
        {
            //Send username and password and receive results
            byte[] username = Encoding.ASCII.GetBytes("LOGIN:" + Username.Text);
            byte[] password = Encoding.ASCII.GetBytes("PASS:" + Password.Text);

            usernameGlobal = Username.Text;

            sender.Send(username);
            sender.Receive(username);

            sender.Send(password);
            sender.Receive(password);

            string userResult = Encoding.ASCII.GetString(username);
            string passResult = Encoding.ASCII.GetString(password);

            if (userResult.Contains("VALID") && passResult.Contains("VALID"))
                LoadMainMenu();
            else
                resultLabel.Content = "Invalid username or password. Please try again";
        }

        public void Register()
        {
            byte[] username = Encoding.ASCII.GetBytes("REGIS:" + Username.Text);
            byte[] password = Encoding.ASCII.GetBytes("PASS:" + Password.Text);
            byte[] emailadd = Encoding.ASCII.GetBytes("EMAIL:" + EmailAddress.Text);

            usernameGlobal = Username.Text;

            sender.Send(username);
            sender.Receive(username);

            sender.Send(password);
            sender.Receive(password);

            sender.Send(emailadd);
            sender.Receive(emailadd);

            string userResult = Encoding.ASCII.GetString(username);
            string passResult = Encoding.ASCII.GetString(password);
            string emailResult = Encoding.ASCII.GetString(emailadd);

            if (userResult.Contains("VALID") && passResult.Contains("VALID") && emailResult.Contains("VALID"))
                LoadMainMenu();
            else
                resultLabel.Content = "Username or email address already taken.";
        }


        public void GetLatestGame(bool isSinglePlayer)
        {
            byte[] data = Encoding.ASCII.GetBytes("MP");
            int bytesSent = sender.Send(data);
        }

        public void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!moved)
            {
                buttonsMove = new DispatcherTimer();
                buttonsMove.Tick += new EventHandler(MoveButtons);
                buttonsMove.Interval = new TimeSpan(0, 0, 0, 0, 25);
                buttonsMove.Start();
            }
            else
                Register();
        }

        private void MoveButtons(object sender, EventArgs e)
        {
            if (marginReg.Top < 610)
            {
                marginReg.Top += 2;
                marginLog.Top += 2;
                RegisterButton.Margin = marginReg;
                LoginButton.Margin = marginLog;
            }
            else if (marginReg.Top >= 610)
            {
                buttonsMove.Stop();
                moved = true;
                EmailAddress.Visibility = Visibility.Visible;
                emailLabel.Visibility = Visibility.Visible;
            }
        }

        public void HideMainMenu()
        {
            SingleplayerCustom.Visibility = Visibility.Collapsed;
            SingleplayerCurrent.Visibility = Visibility.Collapsed;
            Multiplayer.Visibility = Visibility.Collapsed;
            Exit.Visibility = Visibility.Collapsed;
        }


        public void CustomMenu()
        {
            //receive the list of settings from the server and break them up into separate pieces as they are sent in single string with comma separator.
            byte[] buffer = new byte[1024];
            sender.Receive(buffer);
            string count = Encoding.ASCII.GetString(buffer);
            count = count.Remove(0, 4);
            int number = int.Parse(count);
            sender.Send(Encoding.ASCII.GetBytes("OK"));

            string[] rowResults = new string[number];
            dividedResults = new string[number, 5];
            //receive each database entry as a string separated by commas
            for (int i = 0; i < number; i++)
            {
                sender.Receive(buffer);
                rowResults[i] = Encoding.ASCII.GetString(buffer);
                rowResults[i] = rowResults[i].Remove(rowResults[i].Length - 1);
                string[] splitRes = rowResults[i].Split(',');
                for (int j = 0; j < 5; j++)
                    dividedResults[i, j] = splitRes[j];
                sender.Send(Encoding.ASCII.GetBytes("OK"));
            }

            customMenu = new CustomChoiceWindow();

            List<UniqueGame> items = new List<UniqueGame>();

            //from the data generated above, put data in a format that can be used by the WPF list tool
            for (int i = 0; i < number; i++)
            {
                items.Add(new UniqueGame() { GameMode = dividedResults[i, 1], CharOne = dividedResults[i, 2], CharTwo = dividedResults[i, 3], Location = dividedResults[i, 4] });
            }

            customMenu.listView.ItemsSource = items;
            customMenu.Owner = this;
            customMenu.Show();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void SingleplayerCustom_Click(object sender, RoutedEventArgs e)
        {
            JoinSingleplayerCustom();
        }

        private void SingleplayerCurrent_Click(object sender, RoutedEventArgs e)
        {
            JoinSingleplayerCurrent();
        }

        //not used
        private void Multiplayer_Click(object sender, RoutedEventArgs e)
        {
            JoinMultiplayer();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
    public class UniqueGame
    {
        public string Date { get; set; }
        public string GameMode { get; set; }
        public string CharOne { get; set; }
        public string CharTwo { get; set; }
        public string Location { get; set; }
    }
}