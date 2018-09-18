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
    /// Main game methods and data separated for readability
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        int score = 0;
        int multiplayerID = -1;
        List<Sprite> enemyTeam = new List<Sprite>();
        Sprite character;
        Sprite mainObject;
        double timer = 30;
        bool enemyMovingRight = false;

        //called 40x per second
        public void UpdateLoop(object sender, EventArgs e)
        {
            foreach (Sprite s in enemyTeam)
                s.UpdateVelocity();

            timer-=0.025;
            string time = timer.ToString();
            time = time.Substring(0, 2);
            timerLabel.Content = time;

            if (timer < 0)
            {
                GameLoop.Stop();
                EndGame(null, null);
            }

            characterImage.Margin = character.XY;
            character.UpdateVelocity();

            objectImage.Margin = mainObject.XY;
            mainObject.UpdateVelocity();

            enemyImage.Margin = enemyTeam[0].XY;
            //enemyTeam[0].UpdateVelocity();

            PollInput();

            MainObjectCollisions();

            if (enemyTeam[0].XY.Left > 200)
                enemyTeam[0].VelocityX = -5;
            if (enemyTeam[0].XY.Left < -200)
                enemyTeam[0].VelocityX = 5;
        }

        //basic collision detection, only on the main object
        //also sets boundaries and deceleration
        public void MainObjectCollisions()
        {
            foreach (Sprite s in enemyTeam)
            {
                if (s.XY.Left + 50 > mainObject.XY.Left && s.XY.Left + 50 < mainObject.XY.Left + 50 && s.XY.Top + 25 > mainObject.XY.Top && s.XY.Top + 25 < mainObject.XY.Top + 50)
                    mainObject.VelocityX += 20;
                if (s.XY.Left < mainObject.XY.Left + 50 && s.XY.Left + 50 > mainObject.XY.Left + 50 && mainObject.XY.Top > s.XY.Top && mainObject.XY.Top < s.XY.Top + 50)
                    mainObject.VelocityX -= 20;

                if (s.XY.Top < mainObject.XY.Top + 50 && s.XY.Top + 50 > mainObject.XY.Top + 50 && (s.XY.Left + 25 > mainObject.XY.Left && s.XY.Left + 25 < mainObject.XY.Left + 50))
                    mainObject.VelocityY += 20;
                if (s.XY.Top < mainObject.XY.Top + 50 && s.XY.Top + 50 > mainObject.XY.Top + 50 && s.XY.Left > character.XY.Left && mainObject.XY.Left < s.XY.Left + 50)
                    mainObject.VelocityY += 20;
            }

            if (character.XY.Left + 50 > mainObject.XY.Left && character.XY.Left + 50 < mainObject.XY.Left + 50 && character.XY.Top + 25 > mainObject.XY.Top && character.XY.Top + 25 < mainObject.XY.Top + 50) 
                mainObject.VelocityX += 20;
            if (character.XY.Left < mainObject.XY.Left + 50 && character.XY.Left + 50 > mainObject.XY.Left + 50 && mainObject.XY.Top > character.XY.Top && mainObject.XY.Top < character.XY.Top + 50)
                mainObject.VelocityX -= 20;
            
            if (character.XY.Top < mainObject.XY.Top + 50 && character.XY.Top + 50 > mainObject.XY.Top + 50 && (character.XY.Left + 25 > mainObject.XY.Left && character.XY.Left + 25 < mainObject.XY.Left + 50))
                mainObject.VelocityY -= 20;
            if (character.XY.Top < mainObject.XY.Top + 50 && character.XY.Top + 50 > mainObject.XY.Top + 50 && mainObject.XY.Left > character.XY.Left && mainObject.XY.Left < character.XY.Left + 50)
                mainObject.VelocityY += 20;


            if (mainObject.VelocityX > 0)
                mainObject.VelocityX--;
            if (mainObject.VelocityX < 0)
                mainObject.VelocityX++;

            if (mainObject.VelocityY > 0)
                mainObject.VelocityY--;
            if (mainObject.VelocityY < 0)
                mainObject.VelocityY++;

            if (mainObject.XY.Left > -100 && mainObject.XY.Left < 100 && mainObject.XY.Top > 0 && mainObject.XY.Top < 50)
            {
                score++;
                scoreLabel.Content = score;

                mainObject.XY.Top = 400;
                mainObject.XY.Left = 0;
                mainObject.VelocityX = 0;
                mainObject.VelocityY = 0;
            }


            if (mainObject.XY.Left < 0 || mainObject.XY.Left > 1300 || mainObject.XY.Top < -50 || mainObject.XY.Top > 750)
            {
                mainObject.XY.Top = 400;
                mainObject.XY.Left = 0;
                mainObject.VelocityX = 0;
                mainObject.VelocityY = 0;
            }
        }

        //check inputs
        public void PollInput()
        {
            //Keyboard control checks
            if (Keyboard.IsKeyDown(Key.A))
                character.Move(-10, 0);
            else if (Keyboard.IsKeyDown(Key.D))
                character.Move(10, 0);
            else if (Keyboard.IsKeyDown(Key.W))
                character.Move(0, -5);
            else if (Keyboard.IsKeyDown(Key.S))
                character.Move(0, 5);
        }

        //retrieve the servers current settings
        public void JoinSingleplayerCurrent()
        {
            //Get current settings from the server and set them to the global variables
            byte[] data = Encoding.ASCII.GetBytes("CURRENT");
            byte[] byteResults = new byte[1024];

            sender.Send(data);
            sender.Receive(byteResults);

            string result = Encoding.ASCII.GetString(byteResults);
            result = result.Remove(result.Length - 1);
            string[] results = result.Split(',');

            for (int i = 0; i < 5; i++)
                currentSettings[i] = results[i];

            HideMainMenu();
            LoadSingleplayer();
        }

        //start a game using some custom settings from the database
        public void JoinSingleplayerCustom()
        {
            //Request the database entries for custom gamemodes
            byte[] data = Encoding.ASCII.GetBytes("CUSTOM");
            sender.Send(data);
            HideMainMenu();
            CustomMenu();
        }

        //incomplete
        public void JoinMultiplayer()
        {
            //Request the start game for multiplayer
            byte[] data = Encoding.ASCII.GetBytes("MULTIPLAYER");
            sender.Send(data);
            HideMainMenu();
            LoadMultiplayer();
        }

        public void SetSingleplayerCustom()
        {
            //Once retrieved, the currentsettings need to be set from the array used when retrieving the information from server
            for (int i = 0; i < 5; i++)
                currentSettings[i] = dividedResults[customMenuResult, i];
            LoadSingleplayer();
        }

        //load single player from the local settings which have been set either through the custom or latest server settings
        public void LoadSingleplayer()
        {
            //Settings have already been loaded into the global values

            //Begin method to update 40x per second.
            GameLoop = new DispatcherTimer();
            GameLoop.Tick += new EventHandler(UpdateLoop);
            GameLoop.Interval = new TimeSpan(0, 0, 0, 0, 25);

            Logo.Visibility = Visibility.Hidden;

            //set the background image
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();

            if (currentSettings[1] == "Football")
            {
                bi.UriSource = new Uri(@"../footballsp.png", UriKind.Relative);
                mainObject = new Sprite("BALL", 0, 400);
            }

            else if (currentSettings[1] == "X Factor")
            {
                bi.UriSource = new Uri(@"../xfactor.png", UriKind.Relative);
                mainObject = new Sprite("COWELL", 0, 400);
            }

            else if (currentSettings[1] == "Boxing")
            {
                bi.UriSource = new Uri(@"../basketball.png", UriKind.Relative);
                mainObject = new Sprite("BASKETBALL", 0, 400);
            }

            bi.EndInit();
            gameBack.Source = bi;
            gameBack.Visibility = Visibility.Visible;

            //move to back
            Panel.SetZIndex(gameBack, 0);


            //checking against the currently hard coded possible settings.
            //could later be loaded dynamically from a text file to allow more customization
            if (currentSettings[4] == "London")
                locationLabel.Content = "LONDON";

            else if (currentSettings[4] == "USA")
                locationLabel.Content = "United States";

            else if (currentSettings[4] == "UK")
                locationLabel.Content = "United Kingdom";


            if (currentSettings[2] == "Trump")
                character = new Sprite("TRUMP", 0, 500);

            else if (currentSettings[2] == "David Cameron")
                character = new Sprite("CAMERON", 0, 500);

            else if (currentSettings[2] == "The Queen")
                character = new Sprite("QUEEN", 0, 500);


            if (currentSettings[3] == "David Cameron")
                enemyTeam.Add(new Sprite("CAMERON", 0, 80));

            else if (currentSettings[3] == "Trump")
                enemyTeam.Add(new Sprite("TRUMP", 0, 80));

            else if (currentSettings[3] == "The Queen")
                enemyTeam.Add(new Sprite("QUEEN", 0, 80));


            //bind the wpf image instances to the sprites used to hold data
            objectImage.Source = mainObject.bi;
            objectImage.Width = mainObject.width;
            objectImage.Height = mainObject.height;

            characterImage.Source = character.bi;
            characterImage.Width = character.width;
            characterImage.Height = character.height;

            enemyTeam[0].VelocityX = 5;
            enemyImage.Source = enemyTeam[0].bi;
            enemyImage.Width = enemyTeam[0].width;
            enemyImage.Height = enemyTeam[0].height;

            //make visible game objects
            objectImage.Visibility = Visibility.Visible;
            enemyImage.Visibility = Visibility.Visible;
            characterImage.Visibility = Visibility.Visible;
            locationLabel.Visibility = Visibility.Visible;

            score = 0;
            scoreLabel.Content = score;

            GameLoop.Start();
        }

        //hide game objects and show main menu
        public void EndGame(object source, ElapsedEventArgs e)
        {
            objectImage.Visibility = Visibility.Collapsed;
            enemyImage.Visibility = Visibility.Collapsed;
            characterImage.Visibility = Visibility.Collapsed;
            gameBack.Visibility = Visibility.Collapsed;
            scoreLabel.Content = "";
            timerLabel.Content = "";
            locationLabel.Content = "";
            LoadMainMenu();
            Logo.Visibility = Visibility.Visible;
            //SingleplayerCurrent.IsPressed = false;

            byte[] hs = Encoding.ASCII.GetBytes("HIGHSCORE:" + score);
            sender.Send(hs);
            sender.Receive(hs);

            timer = 30;
        }


        public void LoadMultiplayer()
        {

        }
    }
}