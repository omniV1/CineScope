using System;
using System.Threading;

namespace CineScope.Server
{
    /// <summary>
    /// Static class that provides ASCII art and animation for the console introduction
    /// </summary>
    public static class ConsoleIntro
    {
        /// <summary>
        /// Displays the CineScope ASCII art intro in the console with animation effects
        /// </summary>
        /// <param name="waitForKeypress">If true, waits for user to press a key before continuing</param>
        public static void ShowIntro(bool waitForKeypress = true)
        {
            try
            {
                // Clear the console and hide the cursor for cleaner animations
                Console.Clear();
                Console.CursorVisible = false;

                // Set the color for the CineScope logo
                Console.ForegroundColor = ConsoleColor.Red;

                // Start with a theater curtain opening animation for dramatic effect
                DrawCurtainAnimation();


                // The CineScope logo
                string[] logo = {
@"     ██████╗██╗███╗   ██╗███████╗███████╗ ██████╗ ██████╗ ██████╗ ███████╗",
@"    ██╔════╝██║████╗  ██║██╔════╝██╔════╝██╔════╝██╔═══██╗██╔══██╗██╔════╝",
@"    ██║     ██║██╔██╗ ██║█████╗  ███████╗██║     ██║   ██║██████╔╝█████╗  ",
@"    ██║     ██║██║╚██╗██║██╔══╝  ╚════██║██║     ██║   ██║██╔═══╝ ██╔══╝  ",
@"    ╚██████╗██║██║ ╚████║███████╗███████║╚██████╗╚██████╔╝██║     ███████╗",
@"     ╚═════╝╚═╝╚═╝  ╚═══╝╚══════╝╚══════╝ ╚═════╝ ╚═════╝ ╚═╝     ╚══════╝"
};

                // Add some spacing before the logo
                Console.WriteLine();

                // Determine the length of the longest line in the logo
                int maxLength = 0;
                foreach (string line in logo)
                {
                    maxLength = Math.Max(maxLength, line.Length);
                }

                // Calculate the padding needed to center the logo
                int padding = Math.Max(0, (Console.WindowWidth - maxLength) / 2);
                string paddingStr = new string(' ', padding);

                // Display the logo with a typewriter effect (but don't clear the screen)
                foreach (string line in logo)
                {
                    Console.WriteLine(paddingStr + line);
                    Thread.Sleep(50); // Pause briefly between lines
                }


                // Prepare the title for development team
                Console.WriteLine("\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(CenterText("DEVELOPMENT TEAM"));
                Console.WriteLine();

                // Display each developer one at a time, with 6-7 seconds for each (reduced from 10)
                DisplayDeveloperWithTimeout("Owen Lindsey", ConsoleColor.Yellow, GetOwenTag(), 1500);
                DisplayDeveloperWithTimeout("Andrew Mack", ConsoleColor.Green, GetAndrewTag(), 1500);
                DisplayDeveloperWithTimeout("Carter Wright", ConsoleColor.DarkYellow, GetCarterTag(), 1500);
                DisplayDeveloperWithTimeout("Rian Smart", ConsoleColor.Magenta, GetRianTag(), 1500);

                // Return to main program flow
                Console.ForegroundColor = ConsoleColor.White;

                // Display the tagline with a typewriter effect
                string tagline = "FOR MOVIE LOVERS, BY MOVIE LOVERS";
                Console.WriteLine("\n");
                Console.WriteLine(CenterText(tagline));

                // Brief pause for effect
                Thread.Sleep(500);

                // Display the core values with a dramatic reveal
                string values = "EXPLORE. CONNECT. DISCOVER.";
                Console.WriteLine();
                Console.WriteLine(CenterText(values));

                // Add spacing before loading message
                Console.WriteLine();
                Console.WriteLine();

                // Show loading message with typewriter effect
                string loading = "Starting CineScope server...";
                TypewriterEffect(CenterText(loading), 30);

                // Display loading progress bar animation
                Console.WriteLine();
                SimulateLoading();

                // Show version info in gray at the bottom
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine(CenterText("v1.0.0 | © 2025 Team CineScope"));

                // If waitForKeypress is true, wait for user input before continuing
                if (waitForKeypress)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();
                    Console.WriteLine(CenterText("Press any key to continue..."));
                    Console.CursorVisible = true;
                    Console.ReadKey(true);
                }
                else
                {
                    // Give user more time to see the animation if not waiting for keypress
                    Thread.Sleep(3000);
                }

                // Reset console color and cursor visibility before exiting
                Console.ResetColor();
                Console.CursorVisible = true;
            }
            catch (Exception ex)            {
                // In case of any errors, make sure we don't break application startup
                Console.ResetColor();
                Console.CursorVisible = true;
                Console.WriteLine($"Error displaying intro: {ex.Message}");
                Thread.Sleep(2000); // Brief pause to show the error
            }
        }

        /// <summary>
        /// Displays a developer name and tag for a specified duration, then clears it
        /// </summary>
        /// <param name="name">Developer name</param>
        /// <param name="color">Color to use for the developer's display</param>
        /// <param name="tag">ASCII art tag lines</param>
        /// <param name="durationMs">How long to display in milliseconds</param>
        private static void DisplayDeveloperWithTimeout(string name, ConsoleColor color, string[] tag, int durationMs)
        {
            // Save the current cursor position to return here after clearing
            int startLine = Console.CursorTop;

            // Set the developer's color
            Console.ForegroundColor = color;
            Console.WriteLine(CenterText(name));

            // Display the developer's tag with animation
            foreach (string line in tag)
            {
                Console.WriteLine(CenterText(line));
                Thread.Sleep(50);
            }

            // Wait for the specified duration
            Thread.Sleep(durationMs);

            // Clear the developer's display by returning to the start position
            // and writing blank lines over the existing content
            Console.SetCursorPosition(0, startLine);
            for (int i = 0; i < tag.Length + 1; i++) // +1 for the name line
            {
                Console.WriteLine(new string(' ', Console.WindowWidth - 1));
            }

            // Return to the starting cursor position for the next developer
            Console.SetCursorPosition(0, startLine);
        }

        /// <summary>
        /// Gets Owen's ASCII art tag
        /// </summary>
        private static string[] GetOwenTag()
        {
            return new string[] {
                @" ██████╗ ███╗   ███╗███╗   ██╗██╗██╗   ██╗",
                @" ██╔══██╗████╗ ████║████╗  ██║██║██║   ██║",
                @" ██║  ██║██╔████╔██║██╔██╗ ██║██║╚██╗ ██╔╝",
                @" ╚██████╔╝██║╚██╔╝██║██║╚████║██║ ╚████╔╝ "
            };
        }

        /// <summary>
        /// Gets Andrew's ASCII art tag
        /// </summary>
        private static string[] GetAndrewTag()
        {
            // TODO: Andrew should replace this with his own ASCII art
            return new string[] {
                @"     ___              __                   ",
                @"    /   |  ____  ____/ /_______  _      __",
                @"   / /| | / __ \/ __  / ___/ _ \| | /| / /",
                @"  / ___ |/ / / / /_/ / /  /  __/| |/ |/ / ",
                @" /_/  |_/_/ /_/\__,_/_/   \___/ |__/|__/  "
            };
        }

        /// <summary>
        /// Gets Carter's ASCII art tag
        /// </summary>
        private static string[] GetCarterTag()
        {
            // TODO: Carter should replace this with his own ASCII art
            return new string[] {
                @"   ______           __            ",
                @"  / ____/___ ______/ /____  _____",
                @" / /   / __ `/ ___/ __/ _ \/ ___/",
                @"/ /___/ /_/ / /  / /_/  __/ /    ",
                @"\____/\__,_/_/   \__/\___/_/     "
            };
        }

        /// <summary>
        /// Gets Rian's ASCII art tag
        /// </summary>
        private static string[] GetRianTag()
        {
            // TODO: Rian should replace this with his own ASCII art
            return new string[] {
                @"    ____  _           ",
                @"   / __ \(_)___ _____ ",
                @"  / /_/ / / __ `/ __ \",
                @" / _, _/ / /_/ / / / /",
                @"/_/ |_/_/\__,_/_/ /_/ "
            };
        }

        /// <summary>
        /// Centers text in the console window
        /// </summary>
        /// <param name="text">The text to center</param>
        /// <returns>The text with appropriate spacing to appear centered</returns>
        private static string CenterText(string text)
        {
            // Calculate padding to center the text and handle potential error cases
            return new string(' ', Math.Max(0, (Console.WindowWidth - text.Length) / 2)) + text;
        }

        /// <summary>
        /// Creates a typewriter effect for text
        /// </summary>
        /// <param name="text">The text to display with the typewriter effect</param>
        /// <param name="delayMs">Millisecond delay between characters</param>
        private static void TypewriterEffect(string text, int delayMs = 50)
        {
            // Print each character with a delay to create typing effect
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
            // Draw the empty progress bar container
            Console.Write(CenterText("[                    ]"));

            // Position cursor at the start of the progress bar
            int cursorLeft = Console.CursorLeft;
            int cursorTop = Console.CursorTop;
            Console.SetCursorPosition(Math.Max(0, cursorLeft - 21), cursorTop);

            // Fill the progress bar one block at a time
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
            // Get the current console width and define curtain height
            int width = Console.WindowWidth;
            int height = 5;

            // Draw initial closed curtain (solid blocks across the width)
            for (int y = 0; y < height; y++)
            {
                Console.WriteLine(new string('█', width));
            }

            // Pause for 1 second before starting the curtain animation (reduced from 2)
            Thread.Sleep(500);

            // Open the curtain by creating a growing space in the middle
            // Use fewer steps and longer delays to slow down the animation
            for (int step = 0; step < width / 2; step += 2) // Increment by 2 for faster movement
            {
                // Return cursor to the start position
                Console.SetCursorPosition(0, 0);

                // Draw each line of the curtain
                for (int y = 0; y < height; y++)
                {
                    // Calculate left curtain, center gap, and right curtain
                    string leftCurtain = new string('█', Math.Max(0, width / 2 - step));
                    string space = new string(' ', Math.Min(width, step * 2));
                    string rightCurtain = new string('█', Math.Max(0, width / 2 - step));

                    // Ensure we don't exceed console width
                    string line = (leftCurtain + space + rightCurtain);
                    if (line.Length > width)
                        line = line.Substring(0, width);

                    Console.WriteLine(line);
                }

                // Reduced delay between animation frames (100ms instead of 150ms)
                Thread.Sleep(100);
            }

            // Pause for 1 second when curtain is fully open (reduced from 2)
            Thread.Sleep(1000);

            // Do NOT clear the screen after the curtain animation
            // Instead, move the cursor to position after the curtain
            Console.SetCursorPosition(0, height + 1);
        }
    }
}