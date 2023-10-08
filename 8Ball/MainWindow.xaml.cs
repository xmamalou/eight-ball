using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Media;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Microsoft.VisualBasic;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace _8Ball
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        //public Frame windowFrame { get; private set; }
        private Random rnd;

        private List<string>[] answers;

        private List<string> paths;
        string installedPath;

        private int weightToNegative;
        private int weightToNeutral;

        SoundPlayer player;

        public MainWindow()
        {
            //windowFrame = frame;

            InitializeComponent();
            SetTitleBar(EightballTitleBar);

            Title.Text = "Ask a question and click 'Tell me'";

            weightToNegative = 0;
            weightToNeutral = 0;
            rnd = new Random();

            answers = new List<string>[] {
                new List<string> // positive answers
                {
                    "It is certain!",
                    "It is decidedly so!",
                    "Without a doubt!",
                    "Yes, definitely!",
                    "You may rely on it!",
                    "As I see it, yes!",
                    "Most likely!",
                    "Outlook good!",
                    "Yes!",
                    "Signs point to yes!",
                    "Of course!",
                    "Totally!",
                    "Heck yeah!",
                    "It's obvious!",
                    "Obviously!",
                    "What do you think"
                },
                new List<string> // negative answers
                {
                    "Don't count on it.",
                    "My reply is no.",
                    "My sources say no.",
                    "Outlook not so good.",
                    "Very doubtful.",
                    "Naw!",
                    "No way!",
                    "Think again!",
                    "Unlikely!",
                    "Hah, nice one!",
                    "Come again?",
                    "How should I put it... no.",
                    "Let my silence be your answer",
                    "What do you think?",
                    "I don't think so",
                    "Nope",
                    "No",
                    "No, no, no, no, no, no, no, no!",
                    "Hahahahahahaha",
                    "No, you idiot",
                    "*Facepalm*",
                    "I would say yes, but nah.",
                    "I would say yes, if you were somebody else.",
                    "I would say yes, but I am told lying is bad.",
                    "I would say yes, but I don't want to.",
                    "I would say yes, but I don't like you.",
                    "I would say yes, but I don't like your face.",
                    "Yes. Just kidding, no.",
                    "Yes (It's opposite day)",
                    "seY",
                    "-Yes",
                    "Yes (this yes is a paid actor)",
                    "I would say yes, but you're not your mom.",
                    "Όχι",
                    "You don't deserve a yes",
                    "Believe me, you wouldn't want me to say yes",
                },
                new List<string> // neutral answers
                {
                    "Reply hazy, try again",
                    "Ask again later",
                    "Better not tell you now",
                    "Cannot predict now",
                    "Concentrate and ask again",
                    "Uuuh, what did you say?",
                    "Error 404",
                    "We are sorry, but we cannot help you at this moment",
                    "*Whistles*",
                    ";)",
                    "You ran out of question points, buy some in your local [REDACTED] store",
                    "The goal of this game is to ask questions, not to spam the button",
                    "You are not allowed to ask questions anymore",
                    "Shut up and go away",
                    "I don't know, ask someone else",
                    "I don't know, ask your mom",
                    "You are a meanie, I won't answer you",
                    "If I answer you, will you go away?",
                    "You annoy me, go away",
                    "What do I seem like, a fortune teller?"
                }
            };

            paths = new List<string>
            {
                "ding.wav",
                "pipe.wav"
            };

            installedPath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;

            player = new System.Media.SoundPlayer();
        }

        private void Clicker(object sender, RoutedEventArgs e)
        {
            if (Question.Text != "")
            {
                Question.PlaceholderText = Question.Text;

                int choice = 0;

                weightToNegative += 2;
                if (weightToNegative > 80 / (weightToNeutral + 1))
                {
                    weightToNeutral++;
                    weightToNegative = 0;
                    choice = 2;
                    Fortune.Foreground = new SolidColorBrush(Colors.Gray);
                }
                else
                {
                    int which = rnd.Next(1, 100);
                    if (which < 20 + weightToNegative)
                    {
                        choice = 1;
                        Fortune.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                        Fortune.Foreground = new SolidColorBrush(Colors.Green);
                }

                var str = new StringBuilder();
                str.Append("Answer: ");
                int counter = rnd.Next(0, answers[choice].Count - 1);
                str.Append($" {answers[choice][counter]}");

                if (Question.Text.IndexOf("Rick Roll", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Fortune.Foreground = new SolidColorBrush(Colors.Yellow);
                    str = new StringBuilder("Answer: Never gonna give you up, never gonna let you down, never gonna run around and desert you!");
                }

                Fortune.Text = str.ToString();
                Fortune.FontStyle = Windows.UI.Text.FontStyle.Oblique;

                var soundPath = Path.Join(installedPath, "Assets/CoolSounds", "ding.wav");

                player.SoundLocation = soundPath;
                player.Play();
                player.Dispose();

                Question.Text = "";
            }
        }

        private void GoToGithub(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo("https://github.com/xmamalou") { UseShellExecute = true });
            }
            catch (Win32Exception)
            {
                ButtonText.Text = "Oops!";
            }
            
        }
    }
}
