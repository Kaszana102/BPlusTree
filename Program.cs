//Console.WriteLine("Hello, World!");

    
namespace StukturyBazDanych2
{
    public class IntConvert
    {
        static public byte[] intToByte4(int val)
        {
            byte[] num = new byte[4];

            num[0] = (byte)(val >> 24);
            num[1] = (byte)(val >> 16);
            num[2] = (byte)(val >> 8);
            num[3] = (byte)(val);

            return num;
        }


        static public int byte4Toint(byte[] src)
        {
            int[] num = new int[4]; //val to decipher                
            num[0] = (int)(src[0] << 24);
            num[1] = (int)(src[1] << 16);
            num[2] = (int)(src[2] << 8);
            num[3] = (int)(src[3]);


            return num[0] | num[1] | num[2] | num[3];
        }

    }
    public class Program
    {

        static bool showNumberOfDiskOperations=false;

        //operacje typu numberOperacji argument1 argument2 \n
        static void FromFile()
        {
            TextReader reader = File.OpenText("testFile.txt");
            string text;
            string[] args;
            int choice = 0;


            while (choice != 6)
            {
                text = reader.ReadLine();
                args = text.Split(' ');
                choice = int.Parse(args[0]);                

                switch (choice)
                {
                    case 1:
                        Console.WriteLine("1-dodaj rekord");
                        Console.WriteLine("klucz: " + args[1]);
                        int key = Convert.ToInt32(args[1]);                        
                        int record = Convert.ToInt32(args[2]);
                        Console.WriteLine("rekord: " + record);

                        if (BTreePlus.ReadRecord(key) == null)
                        {
                            BTreePlus.InsertRecord(
                                new(
                                        key, //key
                                    new(record) //simplified record
                                   )
                            );
                        }
                        else
                        {
                            Console.WriteLine("\njuż jest w drzewie!");
                        }
                        break;
                    case 2:
                        Console.WriteLine("2-usun rekord");
                        Console.WriteLine("klucz: " + args[1]);                        

                        BTreePlus.DeleteRecord(Convert.ToInt32(args[1]));
                        break;
                    case 5:
                        Console.WriteLine("5-aktualizuj rekord po kluczu");                        
                        key = Convert.ToInt32(args[1]);
                        Console.WriteLine("klucz: " + key);                        
                        record = Convert.ToInt32(args[2]);
                        Console.WriteLine("rekord: " + record);
                        BTreePlus.UpdateRecord(
                            new(
                                    key, //key
                                new(record) //simplified record
                                )
                        );

                        break;
                }                
                Console.WriteLine();
                Console.WriteLine(BTreePlus.ToString());                
                Console.WriteLine();                
            }

        }


        static void RecordsByHand()
        {

            int choice = -1;

            while (choice != 6)
            {
                choice = -1;
                while (choice != 1 && choice != 2 && choice != 3 && choice != 4 && choice != 5 && choice != 6)
                {
                    Console.WriteLine("1-dodaj rekord");
                    Console.WriteLine("2-usun rekord");
                    Console.WriteLine("3-wyświetl rekord po kluczu");
                    Console.WriteLine("4-wyświetl n-ty rekord");
                    Console.WriteLine("5-aktualizuj rekord po kluczu");
                    Console.WriteLine("6-wyjdź");
                    choice = Convert.ToInt32(Console.ReadLine());
                }

                switch (choice)
                {
                    case 1:
                        Console.WriteLine("klucz");
                        int key = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("rekord (jedna liczba)");
                        int record = Convert.ToInt32(Console.ReadLine());
                        if (BTreePlus.ReadRecord(key) == null)
                        {
                             BTreePlus.InsertRecord(
                                 new(
                                         key, //key
                                     new(record) //simplified record
                                    )
                             );
                        }
                        else
                        {
                            Console.WriteLine("\njuż jest w drzewie!");
                        }
                        break;
                    case 2:
                        Console.WriteLine("klucz");
                        key = Convert.ToInt32(Console.ReadLine());
                        if (BTreePlus.ReadRecord(key) != null)
                        {
                            BTreePlus.DeleteRecord(key);
                        }
                        else
                        {
                            Console.WriteLine("\nNie ma go w drzewie!");
                        }
                        break;
                    case 3:
                        Console.WriteLine("klucz");
                        BTreePlus.ReadRecord(Convert.ToInt32(Console.ReadLine()));
                        break;
                    case 4:
                        Console.WriteLine("indeks");
                        BTreePlus.GetNthRecord(Convert.ToInt32(Console.ReadLine()));
                        break;
                    case 5:
                        Console.WriteLine("klucz");
                        key = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("rekord (jedna liczba)");
                        record = Convert.ToInt32(Console.ReadLine());                        
                        BTreePlus.UpdateRecord(
                            new(
                                    key, //key
                                new(record) //simplified record
                                )
                        );
                        
                        break;
                }                
                Console.WriteLine();                    
                Console.WriteLine(BTreePlus.ToString());
                Console.WriteLine();
                Console.WriteLine();
            }
        }


        static void RandomRecords()
        {

            Console.WriteLine("Podaj liczbę rekordów");
            int numberOfRecords = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Podaj max klucz rekordu");
            int maxKey = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine();

            Random rnd = new Random();

            for (int i = 0; i < numberOfRecords; i++)
            {
                int key = Convert.ToInt32(rnd.Next() % maxKey);
                if (BTreePlus.ReadRecord(key) == null) {
                    BTreePlus.InsertRecord(
                        new(
                            key, //key
                        new(Convert.ToInt32(rnd.Next() % maxKey)) //simplified record
                        ));

                    Console.WriteLine(BTreePlus.ToString());
                    Console.WriteLine();
                }
            }


            Console.WriteLine(BTreePlus.ToString());
        }


        static void FromConsole()
        {

            int choice = -1;

            while (choice != 1 && choice != 2)
            {
                Console.WriteLine("1-pokazuj liczbę operacji dyskowych \n2-nie pokazuj");
                choice = Convert.ToInt32(Console.ReadLine());
            }

            if (choice == 1)
            {
                BTreePlus.showNumberOfDiskOperations = true;
            }



            choice = -1;
            while (choice != 1 && choice != 2)
            {
                Console.WriteLine("1-ręcznie \n2-losowe");
                choice = Convert.ToInt32(Console.ReadLine());
            }

            if (choice == 1)
            {
                RecordsByHand();
            }
            else
            {
                RandomRecords();
            }

        }




        public static void Main()
        {

            bool debug = false;
            if (!debug)
            {


                //GenerateTestFile();

                int choice = -1;

                while (choice != 1 && choice != 2)
                {
                    Console.WriteLine("1-konsola \n2-plik testowy");
                    choice = Convert.ToInt32(Console.ReadLine());
                }

                if (choice == 1)
                {
                    FromConsole();
                }
                else
                {
                    FromFile();
                }

            }
            else
            {
                Random random = new Random();
                int recordsNumber = 100; //95
                for (int i = 0; i < recordsNumber; i++)
                {                                        
                    BTreePlus.InsertRecord(
                        new(
                            i, //key
                        new(i) //simplified record
                        ));
                    Console.WriteLine(i);
                }

                BTreePlus.totalNumberOfReads = 0;
                BTreePlus.totalNumberOfWrites = 0;


                BTreePlus.DeleteRecord(recordsNumber / 2);

                Console.WriteLine(BTreePlus.ToString());
                Console.WriteLine("odczyty " + BTreePlus.totalNumberOfReads);
                Console.WriteLine("zapisy: " + BTreePlus.totalNumberOfWrites);
                //Console.WriteLine("pamięć: " + BTreePlus.MemorySize());
            }

        }



        public static void GenerateTestFile()
        {
            String text = "";


            int maxKey = 200;
            int NumberOfOperations = 1000;

            int choice;

            Random random = new Random(); ;
            for (int i = 0; i < NumberOfOperations; i++)
            {
                choice = random.Next() % 3;
                

                switch (choice)
                {
                    case 0://insert                        
                        text += "1 ";
                        text += random.Next()%maxKey + " " + random.Next()%maxKey;
                        break;
                    case 1://delete
                        text += "2 ";
                        text += random.Next() % maxKey;                        
                        break;
                    case 2://update
                        text += "5 ";
                        text += random.Next() % maxKey + " " + random.Next() % maxKey;
                        break;
                }
                text += '\n';
            }

            text += "6"; //end of working
            File.WriteAllTextAsync("testFile.txt", text);
        }
    }
}


//TODO LIST
///
/// DONE wstawianie rekordu
/// DONE odczyt rekordu
/// DONE przeglądanie całej zawartości pliku i indeksu z kolejnością wartości klucza
/// DONE wyświetlać liczbę odczytów i zapisów dla każdej operacji
/// DONE możliwość wyświetlenia drzewa po każdej operacji wraz indeksem (dla każdego rekordu)
/// RACZEJ aktualizacja rekordu 
/// 
/// DONE dane testowe z pliku
/// 
/// SPRAWDŹ USUWANIE!!! COŚ CHYBA TAM SIĘ PSUJE!!!
///  DODAWANIE DZIAŁA W KOŃCU
