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

                // The CineScope logo - DO NOT MODIFY THIS SECTION
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
                
                // Display the logo with a typewriter effect (but don't clear the screen)
                foreach (string line in logo)
                {
                    Console.WriteLine(line);
                    Thread.Sleep(100); // Pause briefly between lines
                }

                // Display team members with their individual ASCII art tags
                Console.WriteLine("\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(CenterText("DEVELOPMENT TEAM"));
                Console.WriteLine();

                // -------------------------------------------------------------------------
                // OWEN LINDSEY'S ASCII ART TAG
                // -------------------------------------------------------------------------
                // Set pastel yellow color (the closest we can get in console is Yellow)
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(CenterText("Owen Lindsey"));
                
                // Owen's ASCII art tag - OmniV (compact version, around 50% smaller than previous)
                string[] owenTag = {
@" ██████╗ ███╗   ███╗███╗   ██╗██╗██╗   ██╗",
@" ██╔══██╗████╗ ████║████╗  ██║██║██║   ██║",
@" ██║  ██║██╔████╔██║██╔██╗ ██║██║╚██╗ ██╔╝",
@" ╚██████╔╝██║╚██╔╝██║██║╚████║██║ ╚████╔╝ "
                };

                // Display Owen's tag with a subtle animation
                foreach (string line in owenTag)
                {
                    Console.WriteLine(CenterText(line));
                    Thread.Sleep(50); // Slow down a bit to match the blocky style
                }
                
                // -------------------------------------------------------------------------
                // ANDREW MACK'S ASCII ART TAG
                // -------------------------------------------------------------------------
                // This section is reserved for Andrew to add his own ASCII art
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n" + CenterText("Andrew Mack"));
                
                /* 
                // Andrew's ASCII art tag - ADD YOUR ASCII ART HERE
                // Uncomment the block below and replace with your own ASCII art
                string[] andrewTag = {
                    @"",
                    @"",
                    @""
                };

                foreach (string line in andrewTag)
                {
                    Console.WriteLine(CenterText(line));
                    Thread.Sleep(30);
                }
                */
                // Placeholder until Andrew adds his ASCII art
                Console.WriteLine(CenterText("< Developer Signature >"));
                
                // -------------------------------------------------------------------------
                // CARTER WRIGHT'S ASCII ART TAG
                // -------------------------------------------------------------------------
                // This section is reserved for Carter to add his own ASCII art
                Console.ForegroundColor = ConsoleColor.DarkYellow; // Different yellow shade
                Console.WriteLine("\n" + CenterText("Carter Wright"));
                
                /* 
                // Carter's ASCII art tag - ADD YOUR ASCII ART HERE
                // Uncomment the block below and replace with your own ASCII art
                string[] carterTag = {
                    @"",
                    @"",
                    @""
                };

                foreach (string line in carterTag)
                {
                    Console.WriteLine(CenterText(line));
                    Thread.Sleep(30);
                }
                */
                // Placeholder until Carter adds his ASCII art
                Console.WriteLine(CenterText("< Developer Signature >"));
                
                // -------------------------------------------------------------------------
                // RIAN SMART'S ASCII ART TAG
                // -------------------------------------------------------------------------
                // This section is reserved for Rian to add his own ASCII art
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n" + CenterText("Rian Smart"));
                
                /* 
                // Rian's ASCII art tag - ADD YOUR ASCII ART HERE
                // Uncomment the block below and replace with your own ASCII art
                string[] rianTag = {
                    @"",
                    @"",
                    @""
                };

                foreach (string line in rianTag)
                {
                    Console.WriteLine(CenterText(line));
                    Thread.Sleep(30);
                }
                */
                // Placeholder until Rian adds his ASCII art
                Console.WriteLine(CenterText("< Developer Signature >"));

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
            catch (Exception ex)
            {
                // In case of any errors, make sure we don't break application startup
                Console.ResetColor();
                Console.CursorVisible = true;
                Console.WriteLine($"Error displaying intro: {ex.Message}");
                Thread.Sleep(2000); // Brief pause to show the error
            }
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
            
            // Pause for 2 seconds before starting the curtain animation
            Thread.Sleep(2000);
            
            // Open the curtain by creating a growing space in the middle
            // Use fewer steps and longer delays to slow down the animation
            for (int step = 0; step < width / 2; step += 1) // Increment by 1 for smoother movement
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
                
                // Longer delay between animation frames (150ms instead of 15ms)
                Thread.Sleep(150);
            }
            
            // Pause for 2 seconds when curtain is fully open
            Thread.Sleep(2000);
            
            // Do NOT clear the screen after the curtain animation
            // Instead, move the cursor to position after the curtain
            Console.SetCursorPosition(0, height + 1);
        }
    }
}
