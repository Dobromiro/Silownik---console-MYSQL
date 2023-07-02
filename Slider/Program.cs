using System;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace Slider
{

    class Program
    {
        static bool _continue;
        static SerialPort _serialPort;
        static SerialPort _serialPort_COM2;

        int StartSlide(int x, int temp_x)
        {
            x = 0;
            temp_x = 0;
            return 0;
        }

        public static void Main()
        {
            string name;
            string message;


            //### Połączenie z bazą danych ###
            MySqlConnection sqlconn;
            //string connsqlstring = "server=192.168.226.11;port=3306;username=user;password=mati;database=tcon";
            string connsqlstring = "server=192.168.226.11;port=3306;username=user;pwd=mati;database=tcon;charset=utf8;SslMode=none";
            sqlconn = new MySqlConnection(connsqlstring);
            sqlconn.Open();
            String status_polacznenia = sqlconn.State.ToString();
            Console.WriteLine($"MySQL DB is: {status_polacznenia}");
            sqlconn.Close();

            System.IO.StreamReader file = new System.IO.StreamReader(@"line.txt");
            string line = file.ReadLine();


            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();
            _serialPort_COM2 = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = SetPortName(_serialPort.PortName);
            _serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);
            _serialPort.Parity = SetPortParity(_serialPort.Parity);
            _serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
            _serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);
            _serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);

            _serialPort_COM2.PortName = SetPortName_COM2(_serialPort_COM2.PortName);
            _serialPort_COM2.BaudRate = SetPortBaudRate_COM2(_serialPort_COM2.BaudRate);
            _serialPort_COM2.Parity = SetPortParity_COM2(_serialPort_COM2.Parity);
            _serialPort_COM2.DataBits = SetPortDataBits_COM2(_serialPort_COM2.DataBits);
            _serialPort_COM2.StopBits = SetPortStopBits_COM2(_serialPort_COM2.StopBits);
            _serialPort_COM2.Handshake = SetPortHandshake_COM2(_serialPort_COM2.Handshake);

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 1500;
            _serialPort.WriteTimeout = 1500;
            _serialPort_COM2.ReadTimeout = 1500;
            _serialPort_COM2.WriteTimeout = 1500;

            _serialPort.Open();
            _serialPort_COM2.Open();
            _continue = true;
            readThread.Start();

            //Console.Write("Name: ");
            name = "mati";

            Console.WriteLine("Type QUIT to exit");
            //DATAin = _serialPort.ReadExisting();
            // _serialPort_COM2.WriteLine(DATAin);
            while (_continue)
            {
                //message = _serialPort.ReadTo("\n");
                message = "dupa";

                if (stringComparer.Equals("quit", message))
                {
                    _continue = false;
                }
                else
                {
                    // _serialPort.WriteLine(
                    // String.Format("<{0}>: {1}", message));
                    //String.Format("<{0}>: {1}", name, message));
                    //String.Format(name, message));

                    //DATAin = _serialPort.ReadExisting();
                    // _serialPort_COM2.WriteLine(DATAin);
                }
            }

            readThread.Join();
            _serialPort.Close();
            _serialPort_COM2.Close();
        }

        // MATI HS. Reading form port and send to arduino
        public static void Read()
        {
            //### Połączenie z bazą danych ###
            MySqlConnection sqlconn;
            //string connsqlstring = "server=192.168.226.11;port=3306;username=user;password=mati;database=tcon";
            string connsqlstring = "server=192.168.226.11;port=3306;username=user;pwd=mati;database=tcon;charset=utf8;SslMode=none";
            sqlconn = new MySqlConnection(connsqlstring);
            sqlconn.Open();
            int x = 0;
            int temp_x = 0;
            int basic_x = 0;
            string line = "";
            
            System.IO.StreamReader file = new System.IO.StreamReader(@"line.txt");
            line = file.ReadLine();

            //String status_polacznenia = sqlconn.State.ToString();
            //Console.WriteLine($"MySQL DB is: {status_polacznenia}");


            while (_continue)
            {
                int count;
                try
                {
                    string DATAin = _serialPort.ReadTo("\r\n");
                    //string DATAin = "";
                    //DATAin = Console.ReadLine();
                    char OneChar;
                    string module = DATAin;
                    string EAJ = "";
                    string eaj = "EAJ";

                    if (DATAin == "NOREAD")
                    {
                        //_serialPort_COM2.WriteLine(DATAin);
                        Console.WriteLine(DATAin);

                    }
                    else
                    
                    {
                        EAJ = eaj.ToString() + module[10].ToString() + module[11].ToString() + module[12].ToString() + module[13].ToString() + module[14].ToString() + module[15].ToString() + module[16].ToString() + module[17].ToString();
                        using (var cmd = new MySqlCommand())
                        {
                            
                            Console.WriteLine(EAJ);
                            
                            cmd.CommandType = System.Data.CommandType.Text;
                            cmd.Connection = sqlconn;
                            cmd.CommandText = $"SELECT `ServoSlide` FROM `common-{line}` WHERE `LGE_PN` = '{EAJ}'";
                            cmd.ExecuteNonQuery();
                            string OutputString = "";
                            OutputString = Convert.ToString(cmd.ExecuteScalar());
                            //Console.WriteLine(OutputString);
                            /// ### konwertowanie string to int ###
                            try
                            {
                                temp_x = Int32.Parse(OutputString);
                                if (basic_x == 0)
                                {
                                    basic_x = temp_x;
                                    if (x == 0)
                                    {
                                        x = temp_x;
                                    }
                                    else if (x != temp_x)
                                    {
                                        x = (x - temp_x) * (-1);
                                    }
                                    OutputString = x.ToString();
                                    Console.WriteLine($"Basic x: {basic_x}");
                                    Console.WriteLine($"x: {x}");
                                    Console.WriteLine($"temp_x: {temp_x}");
                                    Console.WriteLine($"OutputString: {OutputString}");
                                    _serialPort_COM2.WriteLine(OutputString);

                                }
                                
                                else if (temp_x != basic_x)
                                {
                                    basic_x = temp_x;

                                    if (x == 0)
                                    {
                                        x = temp_x;
                                    }
                                    else if (x != temp_x)
                                    {
                                        x = (x - temp_x) * (-1);
                                    }
                                    
                                    OutputString = x.ToString();
                                    Console.WriteLine($"Basic x: {basic_x}");
                                    Console.WriteLine($"x: {x}");
                                    Console.WriteLine($"temp_x: {temp_x}");
                                    Console.WriteLine($"OutputString: {OutputString}");
                                    _serialPort_COM2.WriteLine(OutputString);
                                    x = basic_x;
                                }
                                else if (temp_x == basic_x)
                                {
                                    Console.WriteLine("Don't do anything");
                                }
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine($"Unable to parse '{OutputString}'");
                            }
                            
                            
                        }
                    }
                }
                catch (TimeoutException) { }
            }
        }

        // Display Port values and prompt user to enter a port.
        public static string SetPortName(string defaultPortName)
        {
            string portName;
            // *** Wczytywanie nazwy portu COM z txt ***
            string linia_teksu = "";
            string[] porty = new string[3];
            int licznik = 0;

            System.IO.StreamReader file = new System.IO.StreamReader(@"config.txt");
            while ((linia_teksu = file.ReadLine()) != null)
            {
                if (licznik == 0)
                {
                    porty[1] = linia_teksu;
                }
                licznik++;
            }
            file.Close();

            portName = (porty[1]);

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter COM port value (Default: {0}): \n", defaultPortName);
            //portName = Console.ReadLine();
            //portName = "COM1";
            if (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                portName = defaultPortName;
            }
            return portName;
        }
        // Display BaudRate values and prompt user to enter a value.
        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate(default:{0}): \n", defaultPortBaudRate);
            //baudRate = Console.ReadLine();
            baudRate = "9600";
            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }

        // Display PortParity values and prompt user to enter a value.
        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Parity value (Default: {0}): \n", defaultPortParity.ToString(), true);
            //parity = Console.ReadLine();
            parity = defaultPortParity.ToString();
            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity, true);
        }
        // Display DataBits values and prompt user to enter a value.
        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
            //dataBits = Console.ReadLine();
            dataBits = defaultPortDataBits.ToString();
            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits.ToUpperInvariant());
        }

        // Display StopBits values and prompt user to enter a value.
        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available StopBits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter StopBits value (None is not supported and \n" +
             "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
            //stopBits = Console.ReadLine();
            stopBits = defaultPortStopBits.ToString();
            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
        }
        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
            //handshake = Console.ReadLine();
            handshake = defaultPortHandshake.ToString();
            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
        }

        //COM2

        // Display Port values and prompt user to enter a port.
        public static string SetPortName_COM2(string defaultPortName)
        {
            string portName;
            // *** Wczytywanie nazwy portu COM z txt ***
            string linia_teksu = "";
            string[] porty = new string[3];
            int licznik = 0;

            System.IO.StreamReader file = new System.IO.StreamReader(@"config.txt");
            while ((linia_teksu = file.ReadLine()) != null)
            {
                if (licznik == 1)
                {
                    porty[2] = linia_teksu;
                }
                licznik++;
            }
            file.Close();
            portName = porty[2];

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter COM port value (Default: {0}): \n", defaultPortName);
            //portName = Console.ReadLine();
            //portName = "COM7";
            if (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                portName = defaultPortName;
            }
            return portName;
        }
        // Display BaudRate values and prompt user to enter a value.
        public static int SetPortBaudRate_COM2(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate(default:{0}): \n", defaultPortBaudRate);
            //baudRate = Console.ReadLine();
            baudRate = "9600";
            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }

        // Display PortParity values and prompt user to enter a value.
        public static Parity SetPortParity_COM2(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Parity value (Default: {0}): \n", defaultPortParity.ToString(), true);
            //parity = Console.ReadLine();
            parity = defaultPortParity.ToString();
            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity, true);
        }
        // Display DataBits values and prompt user to enter a value.
        public static int SetPortDataBits_COM2(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
            //dataBits = Console.ReadLine();
            dataBits = defaultPortDataBits.ToString();
            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits.ToUpperInvariant());
        }

        // Display StopBits values and prompt user to enter a value.
        public static StopBits SetPortStopBits_COM2(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available StopBits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter StopBits value (None is not supported and \n" +
             "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
            //stopBits = Console.ReadLine();
            stopBits = defaultPortStopBits.ToString();
            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
        }
        public static Handshake SetPortHandshake_COM2(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
            //handshake = Console.ReadLine();
            handshake = defaultPortHandshake.ToString();
            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
        }
    }
}
