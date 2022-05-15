using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using System.Windows.Threading;

namespace Lesson5ProgrammingProjectWPF
{
    //  namespace enums
    public enum GameMode { Stack, Queue };
    public enum GameDifficulty { Easy, Medium, Hard};

    public partial class MainWindow : Window
    {
        // class constants
        private const int STARTING_BUBBLES = 10;
        private const int QUEUE_START_NUM = 0;
        private readonly string SETTINGS_FULL_PATH;

        // properties  
        private GameMode GameMode { get; set; }
        private int GameScore { get; set; }

        private GameDifficulty GameDifficulty { get; set; }
        private int HighScore { get; set; }
        private int GameSpeed { get; set; }
        private int GameTime { get; set; }
        private int StartingBubbles { get; set; }
        private int nextQueue { get; set; }


        // fields
        private DispatcherTimer _gameTimer;
        private Queue<Button> _queue;
        private Stack<Button> _stack;
        private Random _rng;

        bool startingGame;
        bool gameEnd;
        //interval to add bubbles
        int timerCounter;
        //total ticks the game has played for
        int gameTimer;
        int bubbleSpeed;
        bool yesHighScore;



        // constructor
        public MainWindow()
        {
            InitializeComponent();

            // initialize the path to the settings file
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appDataPath = System.IO.Path.Combine(appDataPath, "BubblePopperGame");
            System.IO.Directory.CreateDirectory(appDataPath);
            SETTINGS_FULL_PATH = System.IO.Path.Combine(appDataPath, "BubblePopperSettings.txt");

            //write to file
            if (!File.Exists(SETTINGS_FULL_PATH))
            {
                using (StreamWriter writer = new StreamWriter(SETTINGS_FULL_PATH))
                {
                    writer.WriteLine(0 + "," + 0 + "," + 10 + "," + 30);
                    writer.Close();
                }
            }
 
            // initialize the game 
            ResetGame();
        }

        private void ResetGame()
        {
            LoadSettings();

            _gameTimer.Stop();
            gameEnd = false;
            startingGame = false;
            nextQueue = this.StartingBubbles;

            //interval to add bubbles
            timerCounter = 0;
            //total ticks the game has played for
            gameTimer = 0;

            yesHighScore = false;

            // reset the scoreboard
            this.GameScore = 0;
            labelScore.Content = $"Score: {this.GameScore}";
            labelHighScore.Content = $"High Score: {this.HighScore}";

            // set the queue and the stack
            this._queue.Clear();
            this._stack.Clear();

            // remove all bubble buttons
            List<UIElement> bubbles = new List<UIElement>();
            foreach (UIElement e in gameCanvas.Children)
            {
                if (e is Button)
                {
                    bubbles.Add(e);
                }
            }
            foreach (UIElement e in bubbles)
            {
                gameCanvas.Children.Remove(e);
            }

            // create the starting number of bubbles
            if (this.GameMode == GameMode.Stack)
            {
                for (int num = 0; num < this.StartingBubbles; num++)
                {
                    Button b = CreateBubbleButton(num);
                    // add the button to the stack
                    _stack.Push(b);
                    // add the button to the canvas
                    gameCanvas.Children.Add(b);
                }
            }
            else
            {
                // create a list of buttons
                Button[] arr = new Button[this.StartingBubbles];
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = CreateBubbleButton(QUEUE_START_NUM + i);
                    // add the button to the queue
                    _queue.Enqueue(arr[i]);
                }
                // add the buttons to the game canvas in revere order
                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    gameCanvas.Children.Add(arr[i]);
                }

            }

        }

        private void LoadSettings()
        {
            //windows registry for high scores
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\BubblePopperGame");
            if (key != null)
            {
                int highScore = int.Parse(key.GetValue("High Score").ToString());
                labelHighScore.Content = "High Score: " + highScore;
                this.HighScore = highScore;
            }

            // initialize timer
            _gameTimer = new DispatcherTimer();
            _gameTimer.Tick += TimerGame_Tick;
            _gameTimer.Interval = new TimeSpan(0,0,1); //1 second between each tick

            // initialize other fields
            _queue = new Queue<Button>();
            _stack = new Stack<Button>();
            _rng = new Random();

            // initalize settings to defaults
            this.GameMode = GameMode.Stack;
            this.GameSpeed = 1000;  // new bubble every 1 second
            this.GameTime = 30000;  // game length is 30 seconds
            this.StartingBubbles = STARTING_BUBBLES;

            // load saved user preference and override defaults
            try
            {
                using (StreamReader reader = new StreamReader(SETTINGS_FULL_PATH))
                {
                    // read first line
                    string line = reader.ReadLine();
                    // read the rest of the file until end
                    if (line != null)
                    {
                        // parse the line as comma separated values
                        string[] parts = line.Split(',');
                        string mode = parts[0];
                        string difficulty = parts[1];
                        string bubbles = parts[2];
                        string time = parts[3];

                        this.GameMode = (GameMode)int.Parse(mode);
                        this.GameDifficulty = (GameDifficulty)int.Parse(difficulty);

                        if (this.GameDifficulty == GameDifficulty.Easy)
                        {
                            bubbleSpeed = 3;
                        }
                        else if (this.GameDifficulty == GameDifficulty.Medium) {
                            bubbleSpeed = 2;
                        }
                        else {
                            bubbleSpeed = 1;
                        }
                        
                        

                        this.StartingBubbles = int.Parse(bubbles);
                        this.GameTime = int.Parse(time) * 1000;
                        labelTimer.Content = "Timer: " + this.GameTime / 1000;
                    }
                }

            }
            catch (FileNotFoundException) {
                MessageBox.Show("File not found!");
            }
        }

        private Button CreateBubbleButton(int num)
        {
            // construct the button control for this bubble
            Button button = new Button();
            // set the text for the button
            button.Content = num.ToString();
            button.FontWeight = FontWeights.Bold;
            // set the background image for the button
            string uri = "pack://application:,,,/Resources/SmallBubble.png";
            if (_rng.Next(0, 10) == 0)
            {
                // 10% chance to get large bubble
                uri = "pack://application:,,,/Resources/LargeBubble.png";
            }
            BitmapImage image = new BitmapImage(new Uri(uri));
            button.Background = new ImageBrush(image);
            // button dimensions
            button.Height = image.Height;
            button.Width = image.Width;
            // remove the border
            button.BorderThickness = new Thickness(0.0);
            // place the bubble on the screen
            int left = _rng.Next((int)button.Width, (int)(this.Width - button.Width * 2));
            int top = _rng.Next((int)button.Height, (int)(this.Height - button.Height * 2));
            Canvas.SetLeft(button, left);
            Canvas.SetTop(button, top);
            // set the style of the button
            button.Style = (Style)FindResource("ButtonStyleNoMouseOver");
            // add the event handler
            button.Click += BubbleButton_Click;
            // return the new button
            return button;
        }


        
        private void BubbleButton_Click(object sender, RoutedEventArgs e)
        {
            //prevents clicking on bubbles once game ends
            if (gameEnd == true) {
                return;            
            }

            //starts the timer when the first bubble is clicked
            if (startingGame == false) {
                _gameTimer.Start();
                startingGame = true;
            }

            Button b = (Button)sender;

            //increases high score if the current game score is higher than the high score
            if (GameScore > HighScore)
            {
                HighScore = GameScore;
            }

            if (this.GameMode == GameMode.Stack)
            {
                //removed a bubble in stack mode   
                if (b == _stack.Peek())
                {
                    gameCanvas.Children.Remove((Button)sender);
                    _stack.Pop();
                    GameScore++;
                    labelScore.Content = "Score: " + GameScore;
                }
                
            }
            else
            {
                //removes a bubble in queue mode
                if (b == _queue.Peek())
                {
                    gameCanvas.Children.Remove((Button)sender);
                    _queue.Dequeue();
                    GameScore++;
                    labelScore.Content = "Score: " + GameScore;
                }
                
            }
        }

        //handles timer controle
        private void TimerGame_Tick(object sender, EventArgs e)
        {
            
            //updates the gameTimer and the timer label on the screen everytime the timer ticks
            gameTimer++;
            labelTimer.Content = "Timer: " + (this.GameTime/1000 - gameTimer);

            //ends the game and updates highscore after game ends
            if (GameScore > HighScore)
            {
                HighScore = GameScore;

                //store high score in registry
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\BubblePopperGame");
                key.SetValue("High Score", HighScore);
                key.Close();

                yesHighScore = true;
            }
            //code for when game ends
            if (gameTimer == this.GameTime/1000)
            {
                labelHighScore.Content = "High Score: " + HighScore;
                if (yesHighScore == true)
                {
                    MessageBox.Show("New High Score!");
                }
                else {
                    MessageBox.Show("Game Over!");
                }
                gameEnd = true;
                _gameTimer.Stop();
                return;
            }
            
            //adds bubbles every 10 seconds
            if (timerCounter < bubbleSpeed)
            {
                timerCounter++;
            }
            else if (timerCounter == bubbleSpeed)
            {
                if (this.GameMode == GameMode.Stack)
                {
                    //adds a bubble in stack mode
                    Button b = CreateBubbleButton(_stack.Count);
                    gameCanvas.Children.Add(b);
                    _stack.Push(b);
                }
                else
                {
                    //adds a bubble in queue mode
                    Button b = CreateBubbleButton(nextQueue);
                    nextQueue++;
                    gameCanvas.Children.Insert(0, b);
                    _queue.Enqueue(b);
                }
                timerCounter = 0;
            }
        }
 
        //settings
        private void Settings_MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            GameSettings gsw = new GameSettings(this.GameMode, this.GameDifficulty, this.StartingBubbles, this.GameTime);

            gsw.ShowDialog();
            if (gsw.DialogResult == true)
            {
                saveSettings(gsw);
            }
            ResetGame();
        }

        //save the user's settings
        private void saveSettings(GameSettings gsw) {
            int mode;
            int difficulty;
            if (gsw.gameMode == GameMode.Stack)
            {
                mode = 0;
            }
            else
            {
                mode = 1;
            }

            if (gsw.gameDifficulty == GameDifficulty.Easy)
            {
                difficulty = 0;
            }
            else if (gsw.gameDifficulty == GameDifficulty.Medium)
            {
                difficulty = 1;
            }
            else
            {
                difficulty = 2;
            }

            //write settings to file
            StreamWriter writer = new StreamWriter(SETTINGS_FULL_PATH);
            writer.WriteLine(mode + "," + difficulty + "," + gsw.startingBubbles + "," + gsw.gameTime);
            writer.Close();
        }

        //quit
        private void quitMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //new
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
        }
    }
}
