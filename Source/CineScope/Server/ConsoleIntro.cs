using System;
using System.Threading;

namespace CineScope.Server
{
    public static class ConsoleIntro
    {
        /// <summary>
        /// Displays the CineScope ASCII art intro in the console with animation effects
        /// </summary>
        public static void ShowIntro()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;

            // Simulate a theater curtain opening effect
            DrawCurtainAnimation();

            // Display the updated CineScope logo
            string[] logo = {
@"     ██████╗██╗███╗   ██╗███████╗███████╗ ██████╗ ██████╗ ██████╗ ███████╗",
@"    ██╔════╝██║████╗  ██║██╔════╝██╔════╝██╔════╝██╔═══██╗██╔══██╗██╔════╝",
@"    ██║     ██║██╔██╗ ██║█████╗  ███████╗██║     ██║   ██║██████╔╝█████╗  ",
@"    ██║     ██║██║╚██╗██║██╔══╝  ╚════██║██║     ██║   ██║██╔═══╝ ██╔══╝  ",
@"    ╚██████╗██║██║ ╚████║███████╗███████║╚██████╗╚██████╔╝██║     ███████╗",
@"     ╚═════╝╚═╝╚═╝  ╚═══╝╚══════╝╚══════╝ ╚═════╝ ╚═════╝ ╚═╝     ╚══════╝"
            };

            Console.WriteLine();
            // Display the logo with a typewriter effect
            foreach (string line in logo)
            {
                Console.WriteLine(line);
                Thread.Sleep(100);
            }

            Console.ForegroundColor = ConsoleColor.White;

            // Tagline with a typewriter effect
            string tagline = "FOR MOVIE LOVERS, BY MOVIE LOVERS";
            Console.WriteLine();
            Console.WriteLine(CenterText(tagline));

            Thread.Sleep(500);

            // Values with a dramatic reveal
            string values = "EXPLORE. CONNECT. DISCOVER.";
            Console.WriteLine();
            Console.WriteLine(CenterText(values));

            Console.WriteLine();
            Console.WriteLine();

            string loading = "Starting CineScope server...";
            TypewriterEffect(CenterText(loading), 30);

            // Simulate loading
            Console.WriteLine();
            SimulateLoading();

            // Display version info
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.WriteLine(CenterText("v1.0.0 | © 2025 Team CineScope"));

            // Reset console color
            Console.ResetColor();
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Centers text in the console window
        /// </summary>
        private static string CenterText(string text)
        {
            return new string(' ', (Console.WindowWidth - text.Length) / 2) + text;
        }

        /// <summary>
        /// Creates a typewriter effect for text
        /// </summary>
        private static void TypewriterEffect(string text, int delayMs = 50)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delayMs);
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Simulates a loading progress bar
        /// </summary>
        private static void SimulateLoading()
        {
            Console.Write(CenterText("[                    ]"));
            Console.SetCursorPosition(Console.CursorLeft - 21, Console.CursorTop);

            for (int i = 0; i < 20; i++)
            {
                Thread.Sleep(100);
                Console.Write("█");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Draws a curtain opening animation effect
        /// </summary>
        private static void DrawCurtainAnimation()
        {
            int width = Console.WindowWidth;
            int height = 5;

            // Draw initial closed curtain
            for (int y = 0; y < height; y++)
            {
                Console.WriteLine(new string('█', width));
            }

            // Open the curtain
            for (int step = 0; step < width / 2; step++)
            {
                Console.SetCursorPosition(0, 0);
                for (int y = 0; y < height; y++)
                {
                    string leftCurtain = new string('█', width / 2 - step);
                    string space = new string(' ', step * 2);
                    string rightCurtain = new string('█', width / 2 - step);
                    Console.WriteLine(leftCurtain + space + rightCurtain);
                }
                Thread.Sleep(15);
            }

            Console.Clear();
        }
    }
}