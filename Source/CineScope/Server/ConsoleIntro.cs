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
