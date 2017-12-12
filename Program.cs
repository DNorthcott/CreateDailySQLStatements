using System;
using System.Collections.Generic;
using System.IO;


namespace CreateDummyData
{
    /// <summary>
    /// Reads the input.txt file, if the input file is correctly structured it will return 
    /// the SQL statements for inserting fake data into for another project.  This is an 
    /// assistant script to reduce time creating fake inputs into a DB for testing purposes.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //Local variables
            StreamReader reader;
            string stockPile;
            string blend;
            List<string> movements;
            string dateOfEvent;
            StreamWriter writer;

            //Console text.
            Console.WriteLine("Please ensure the file is updated to represent todays coal mining data requirements.");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
            Console.WriteLine("Loading input.txt file");

            //Delete file and recreate same file.  This will give a empty file.
            if (File.Exists("output.txt"))
            {
                File.Delete("output.txt");
            }

            //Connect to the input.txt and output.txt for reading and writing.
            reader = new StreamReader("input.txt");
            writer = new StreamWriter("output.txt");

            //Read first line which is just a schema layout.
            string line = reader.ReadLine();
            //Read date.
            dateOfEvent = reader.ReadLine();
            // Check the schema was correct.
            if (line ==
                "Stockpiles: (date, priority, stockpile1, stockpile2, stockpile3, stockpile4, stockpile5," +
                " stockpile6, stockpile7, stockpile8, stockpile9, stockpile10)"
            )
            {
                //Read and write information for stockpiles.
                writer.WriteLine((ReadStockpile(reader, dateOfEvent)));
            }

            // Read next schema
            line = reader.ReadLine();
            if (line == "Blend: (date, Priority,	Coal1, Coal2, Coal3, Coal4,	Coal5, Coal6, Coal7, Coal8,	Coal9, " +
                "Coal10)"
            )
            {
                writer.WriteLine(ReadBlend(reader, dateOfEvent));
            }

            // Check schema for coal movements.
            line = reader.ReadLine();
            if (line == "CoalMovements (Coal, Daily mining (repeat until sequence for different coals mined))"
            )
            {
                List<string> allMovements = new List<string>(CreateCoalMovements(reader, dateOfEvent));

                //For every movement returned add to text file.
                foreach (string i in allMovements)
                {
                    writer.WriteLine(i);
                }
                Console.WriteLine("Writing of sql complete.  Check text file for errors.");
                Console.WriteLine("There was " + allMovements.Count + " coal movements generated");
                Console.WriteLine("Press any key to exit.");
            }

            writer.Close();
            Console.ReadKey();
        }

        /// <summary>
        /// Creates a list of coal movements.
        /// </summary>
        /// <param name="movementReader">The streamreader.</param>
        /// <param name="date">The date of the coal movements.</param>
        /// <returns>A list of strings that contain SQL statements for inserting coalmovements into the DB.</returns>
        private static List<string> CreateCoalMovements(StreamReader movementReader, string date)
        {

            List<string> trucks = new List<string>();
            List<string> coalMovement = new List<string>();

            // Make fake trucks.  Creates truck 1 through to 10.
            for (int i = 1; i < 11; i++)
            {
                string truck = "'Truck" + i;
                trucks.Add(truck);
            }

            //Current dummy variable for currentRow to enter look.
            string currentRow = "start";

            //Loop through text file.
            while (currentRow != "END")
            {
                string coalName = movementReader.ReadLine();
                try
                {
                    int tonnage = Int32.Parse(movementReader.ReadLine());
                    //Add to end of the list.
                    coalMovement.AddRange(GetCoalMovements(date, coalName, tonnage, trucks));
                }
                //If line fails to parse to int break out of while loop.
                catch
                {
                    break;
                }
            }

            return coalMovement;
        }

        /// <summary>
        /// Returns a list of coal movements for an individual coal type.  If 10000 tonnes are mined
        /// on a single day this is broken down to 220t movements.  It is presumed that over a 24 hour period
        /// that the 10000 is spread out evenly.
        /// </summary>
        /// <param name="date">The date the coal movements occur.</param>
        /// <param name="coalName">The name of coal.</param>
        /// <param name="tonnage">The expected daily sum of this coal type moved in a single day.</param>
        /// <param name="trucks">A list of trucks that the coal will be assigned to at random.</param>
        /// <returns></returns>
        private static List<string> GetCoalMovements(string date, string coalName, int tonnage, List<string> trucks)
        {
            //Sum of truck movements.
            int numberOfMovements = tonnage / 220;
            //The seconds between movement of the same coal type.
            double secondsBetweenMovements = 86400 / (double)numberOfMovements;
            DateTime startTime = Convert.ToDateTime(date);
            Random random = new Random();
            List<string> movements = new List<string>();

            for (int i = 0; i < numberOfMovements; i++)
            {

                startTime = startTime.AddSeconds(secondsBetweenMovements);

                string sql = "INSERT INTO CoalMovement(Coal, Truck, DateTimeArrival) VALUES('" + coalName + "',";

                int truckNum = random.Next(0, 10);

                sql += trucks[truckNum] + "', '" + startTime.ToString("yyyy-MM-dd HH:mm:ss") + "');";

                movements.Add(sql);

            }

            return movements;
        }

        /// <summary>
        /// Reads the blend for the text file and returns the sql statement.
        /// </summary>
        /// <param name="blendReader">The streamReader.</param>
        /// <param name="date">The date of the blend.</param>
        /// <returns>A string with the sql statement for inserting the blend into the db.</returns>
        private static string ReadBlend(StreamReader blendReader, string date)
        {
            string blendString =
                "INSERT INTO Blend(date, Priority, Coal1, Coal2, Coal3, Coal4, Coal5, Coal6, Coal7, Coal8, Coal9, Coal10) VALUES ('" +
                date + "',";

            string test = blendReader.ReadLine();

            blendString += test;

            string holdingString = blendReader.ReadLine();

            while (holdingString != "END")
            {

                blendString += ",";

                if (holdingString == "NULL")
                {
                    blendString += "NULL";
                }
                else
                {
                    blendString += "'" + holdingString + "'";
                }


                holdingString = blendReader.ReadLine();
            }

            blendString += ");";

            return blendString;

        }

        /// <summary>
        /// Reads the stockpiles from the text file and returns the sql statement for inserting into the db.
        /// </summary>
        /// <param name="stockPileReader">The streamReader.</param>
        /// <param name="date">The date.</param>
        /// <returns>The sql statement for inserting the stockpiles into the DB.</returns>
        private static string ReadStockpile(StreamReader stockPileReader, string date)
        {
            string stockPile =
                 "INSERT INTO Stockpile (date, priority, stockpile1, stockpile2, stockpile3, stockpile4, stockpile5, " +
                 "stockpile6, stockpile7, stockpile8, stockpile9, stockpile10) VALUES	('" + date + "', ";

            //Read priority.
            stockPile += stockPileReader.ReadLine();

            string holdingString = stockPileReader.ReadLine();

            while (holdingString != "END")
            {
                if (holdingString == "NULL")
                {
                    stockPile += ",Null";
                }
                else
                {
                    stockPile += ",'" + holdingString + "'";
                }


                holdingString = stockPileReader.ReadLine();
            }

            stockPile += ");";

            return stockPile;
        }
    }
}




