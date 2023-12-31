﻿/*
   Copyright 2023 Christopher-Marios Mamaloukas

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License. 
 */

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

namespace _8Ball
{
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

        private void GoToFeedback(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo("https://github.com/xmamalou/eight-ball/issues/new/choose") { UseShellExecute = true });
        }

        private void Clicker(object sender, RoutedEventArgs e)
        {
            if (Question.Text != "")
            {
                Question.PlaceholderText = Question.Text;

                int choice = 0;

                /*
                 * The game is rigged.
                 * 
                 * Every time the user asks for an answer, the 
                 * app is more likely to answer negatively
                 *
                 * Initially, the chance of getting a negative 
                 * answer is 20%
                 * 
                 * However, if the probability of having a negative
                 * answer exceeds 100%, then the user will receive 
                 * a neutral answer.
                 * 
                 * In the meantime, the app keeps track of how many 
                 * neutral answers there have been given (in other words,
                 * how many "cycles" there have been). The more cycles,
                 * the faster the next cycle will be (making negative
                 * answers more likely as well), until the user receives
                 * no more but neutral answers.
                 * 
                 * The app, however, will always give out a positive answer
                 * in any question that contains "Rick roll" and open 
                 * "Never Gonna Give You Up" in a browser (Unless there is 
                 * an exception, in which case the app just prints the song line). 
                 * Note that any such answer still counts in shifting the 
                 * probabilities of the answers.
                 */

                weightToNegative += 5 + weightToNeutral;
                if (weightToNegative > 80)
                {
                    weightToNeutral += 5;
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

                if (Question.Text.IndexOf("Rick Roll", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new ProcessStartInfo("https://www.youtube.com/watch?v=dQw4w9WgXcQ") { UseShellExecute = true });
                        choice = 0;
                        Fortune.Foreground = new SolidColorBrush(Colors.Green);
                    }
                    catch (Win32Exception)
                    {
                        Fortune.Foreground = new SolidColorBrush(Colors.Yellow);
                        str.Append("Answer: Never gonna give you up, never gonna let you down, never gonna run around and desert you!");
                    }
                }
                else if (Question.Text.IndexOf("Is it Wednesday", StringComparison.OrdinalIgnoreCase) >= 0 )
                {
                    var time = DateTime.Now;
                    if(time.DayOfWeek == DayOfWeek.Wednesday)
                    {
                        System.Diagnostics.Process.Start(new ProcessStartInfo("https://www.youtube.com/watch?v=du-TY1GUFGk") { UseShellExecute = true });
                        Fortune.Foreground = new SolidColorBrush(Colors.Green);
                        str = new StringBuilder("Answer: It's Wednesday, my dudes.");
                    }
                    else
                    {
                        Fortune.Foreground = new SolidColorBrush(Colors.Red);
                        str.Append("It's ");
                        switch(time.DayOfWeek)
                        {
                            case DayOfWeek.Monday:
                                str.Append("Monday");
                                break;
                            case DayOfWeek.Tuesday:
                                str.Append("Tuesday");
                                break;
                            case DayOfWeek.Thursday:
                                str.Append("Thursday");
                                break;
                            case DayOfWeek.Friday:
                                str.Append("Friday");
                                break;
                            case DayOfWeek.Saturday:
                                str.Append("Saturday");
                                break;
                            case DayOfWeek.Sunday:
                                str.Append("Sunday");
                                break;
                        }
                        str.Append(", you idiot!");
                    }
                }
                else
                    str.Append($" {answers[choice][counter]}");

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
