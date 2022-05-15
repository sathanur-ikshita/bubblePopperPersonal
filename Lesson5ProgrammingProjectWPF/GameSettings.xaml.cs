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
using System.Windows.Shapes;

namespace Lesson5ProgrammingProjectWPF
{
    /// <summary>
    /// Interaction logic for GameSettings.xaml
    /// </summary>
    public partial class GameSettings : Window
    {
        public GameMode gameMode;
        public GameDifficulty gameDifficulty;
        public int startingBubbles;
        public int gameTime;

        //constructor
        public GameSettings(GameMode mode, GameDifficulty difficulty, int bubbles, int time )
        {
            InitializeComponent();
            gameMode = mode;
            gameDifficulty = difficulty;
            startingBubbles = bubbles;
            gameTime = time;

            //save settings so they appear in settings window even after restarting game

            //game mode
            if (this.gameMode == GameMode.Queue)
            {
                queueRadioButton.IsChecked = true;
                stackRadioButton.IsChecked = false;
            }
            else 
            {
                queueRadioButton.IsChecked = false;
                stackRadioButton.IsChecked = true;
            }

            //game difficulty
            if (this.gameDifficulty == GameDifficulty.Easy)
            {
                easyRadioButton.IsChecked = true;
                mediumRadioButton.IsChecked = false;
                hardRadioButton.IsChecked = false;
            }
            else if (this.gameDifficulty == GameDifficulty.Medium)
            {
                easyRadioButton.IsChecked = false;
                mediumRadioButton.IsChecked = true;
                hardRadioButton.IsChecked = false;
            }
            else {
                easyRadioButton.IsChecked = false;
                mediumRadioButton.IsChecked = false;
                hardRadioButton.IsChecked = true;
            }

            //starting bubbles textbox
            startBubblesTextBox.Text = this.startingBubbles.ToString();

            //game length textbox
            lengthTextBox.Text = (this.gameTime/1000).ToString();
        }

        //change settings is ok button is clicked
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            //alter Game Mode settings
            if (stackRadioButton.IsChecked == true)
            {
                gameMode = GameMode.Stack;
            }
            else if (queueRadioButton.IsChecked == true) 
            {
                gameMode = GameMode.Queue;
            }

            //alter Game Difficulty Settings
            if (easyRadioButton.IsChecked == true)
            {
                gameDifficulty = GameDifficulty.Easy;
            }
            else if (mediumRadioButton.IsChecked == true)
            {
                gameDifficulty = GameDifficulty.Medium;
            }
            else if (hardRadioButton.IsChecked == true)
            {
                gameDifficulty = GameDifficulty.Hard;
            }

            //alter Game Startup settings
            bool startBubblesValid = int.TryParse(startBubblesTextBox.Text, out int startB);
            if (startBubblesValid == false) {
                MessageBox.Show("Please enter an integer value!");
            }
            else {
                this.startingBubbles = startB; 
            }

            bool lengthValid = int.TryParse(lengthTextBox.Text, out int length);
            if (startBubblesValid == false)
            {
                MessageBox.Show("Please enter an integer value!");
            }
            else {
                this.gameTime = length;
            }

            this.DialogResult = true;
            this.Close();
        }

        //close settings window if cancel button is clicked
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

       
    }
}
