using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BitOnLive_MonitoringApp
{
    class Program
    {
        public static string key = "Your Key (32)";//Your Key 
        public static int port = 10777;//Your UDP port
        public static List<Monitor> Lista_Monitor_Rilevati = new List<Monitor>();
        public static bool ValueCheckMonitor = false;
        public Monitor monitor_Autorizzato;
        public static bool erroreFile = false;
        public static bool fileNotExist = false;


        public static string DecryptString(string key, string cipherText)
        {
            
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static void CheckMonitor(int id, string nome, int idGpu)
        {
            bool presente = false;
            foreach (Monitor m in Lista_Monitor_Rilevati)
            {
                if (m.Name == nome && m.ID_Monitor == id)
                {
                    presente = true;
                }
            }
            ValueCheckMonitor = presente;

        }

        public static void cercaMonitor() //Crea un file con le informazioni dei monitor chiamato MonitorList.txt
        {

            string strCmdText;
            strCmdText = "/C AppVerificaMonitor_Ver2.exe > MonitorList.txt";


            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "CMD.exe";
            startInfo.Arguments = strCmdText;
            startInfo.CreateNoWindow = true; // Imposta questa proprietà su true per eseguire in modo "incognito"
            startInfo.UseShellExecute = false;

            Process.Start(startInfo);



            


            Lista_Monitor_Rilevati = new List<Monitor>();
            string displayFile = @"MonitorList.txt";
            if (!File.Exists(displayFile))
            {
                //Console.WriteLine("File not found: " + "FileAutorizzazioneMonitor.txt");
                return;
            }
            else
            {
                Thread.Sleep(500);
                using (StreamReader sr = File.OpenText(displayFile))
                {
                    string row = "";
                    while ((row = sr.ReadLine()) != null)
                    {
                        Monitor m = new Monitor();
                        int found = row.IndexOf("DI");
                        row = row.Substring(found);
                        string[] info = row.Split(';');


                        m.DisplayDevice = info[0];
                        m.ID_GPU = Convert.ToInt32(info[1]);
                        m.Refresh_Rate = Convert.ToInt32(info[2]);
                        m.ID_Monitor = Convert.ToInt32(info[3]);
                        m.Name = info[4];

                        Lista_Monitor_Rilevati.Add(m);

                    }
                }
            }

        }


        static void Main(string[] args)
        {
            bool shouldStop = false;
            int timeToStop = 180;
            string percorsoAutorizzazione = @"C:\Certificate\PlayerCertificate.txt";

            cercaMonitor();
            Thread.Sleep(1000);
            
           

            //if (!File.Exists("FileAutorizzazioneMonitor.txt"))
            if (!File.Exists(percorsoAutorizzazione))
            {
                //Console.WriteLine("File not found: " + "FileAutorizzazioneMonitor.txt");
                fileNotExist = true;
                string directoryPath = Path.GetDirectoryName(percorsoAutorizzazione);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    //Console.WriteLine("Cartella creata con successo: " + directoryPath);
                }
                //return;
            }
            string idMon;
            string NomeMon;
            string IdGpu;
            string fileContent= "";
            try///
            {
                //fileContent = File.ReadAllText("FileAutorizzazioneMonitor.txt");
                fileContent = File.ReadAllText(percorsoAutorizzazione);

            }
            catch
            {
                fileNotExist = true;
            }
            //string fileContent = File.ReadAllText("FileAutorizzazioneMonitor.txt");
            string decryptedString;
            try///
            {
                decryptedString = DecryptString(key, fileContent);
            }
            catch///
            {
                decryptedString = "Error;in;the;file;404";//stringa generica per creare l'avviso
                erroreFile = true;
            }



            //Console.WriteLine($"decrypted string = {decryptedString}");

            string[] parts = decryptedString.Split(';');
            if (parts.Length == 3)
            {
                idMon = parts[0];
                NomeMon = parts[1];
                IdGpu = parts[2];
                erroreFile = false;///
            }
            else
            {
                //Console.WriteLine("The string is not in the correct format.");
                erroreFile = true;///
                ///
                idMon = "Errore";
                NomeMon = "Errore";
                IdGpu = "Errore";
                ///

                //return;
            }

            if (!File.Exists("AppVerificaMonitor_Ver2.exe"))
            {
                //Console.WriteLine("~~~~~~~~Executable is missing: AppVerificaMonitor_Ver2~~~~~~~~~~");
                erroreFile = true;
                //return;
            }


            Thread monitorThread = new Thread(() =>
            {
                DateTime startTime = DateTime.Now;
                int counter = 0;///

                while (!shouldStop)
                {
                    //Lista monitor connessi
                    cercaMonitor();

                    //Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                    //Controlla presenza monitor autorizzato



                    ///
                    if (!erroreFile && !fileNotExist)///
                    {
                        CheckMonitor(Convert.ToInt32(idMon), NomeMon, Convert.ToInt32(IdGpu));
                        //Console.WriteLine("Is the monitor connected? -> " + ValueCheckMonitor);
                    }
                    else/// deve controllare che sia stato inserito il file
                    {
                        //Console.WriteLine("Errore");
                        ValueCheckMonitor = false;
                    }

                    if(counter == 5 || fileNotExist || erroreFile)//controlla esista il file di autorizzazione dopo 100 cicli
                    {
                        //if (!File.Exists("FileAutorizzazioneMonitor.txt"))
                        if (!File.Exists(percorsoAutorizzazione))
                        {
                            //Console.WriteLine("File not found: " + "FileAutorizzazioneMonitor.txt");
                            fileNotExist = true;
                        }else{fileNotExist = false;}

                        //try{fileContent = File.ReadAllText("FileAutorizzazioneMonitor.txt");}
                        try{fileContent = File.ReadAllText(percorsoAutorizzazione); }
                        catch {fileNotExist = true;}
                        try
                        {
                            decryptedString = DecryptString(key, fileContent);
                            erroreFile = false;
                        }catch{
                            decryptedString = "Error;in;the;file;404";//stringa generica per creare l'avviso
                            erroreFile = true;
                        }
                        //Console.WriteLine($"decrypted string = {decryptedString}");
                        parts = decryptedString.Split(';');
                        if (parts.Length == 3)
                        {
                            idMon = parts[0];
                            NomeMon = parts[1];
                            IdGpu = parts[2];
                            erroreFile = false;///
                        }
                        else
                        {
                            //Console.WriteLine("The string is not in the correct format.");
                            erroreFile = true;///
                            idMon = "Errore";
                            NomeMon = "Errore";
                            IdGpu = "Errore";
                        }

                        if (counter == 5) counter = 0;
                    }
                    counter++;


                    // Esegui i controlli ogni 3 secondi
                    Thread.Sleep(3000);



                    // Termina il thread se sono passati più di 90 secondi
                    /*if ((DateTime.Now - startTime).TotalSeconds > timeToStop)
                    {
                        Console.WriteLine("Terminating monitor thread...");
                        break;
                    }*/
                }
            });

            

            Thread communicationThread = new Thread(() =>
            {
                DateTime startTime = DateTime.Now;
                int counter = 0;
                UdpClient udpServer = new UdpClient(port); // Sostituisci 10777 con la porta desiderata
                while (!shouldStop)
                {
                    /////////////////////////////////////////////////
                    
                    IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udpServer.Receive(ref clientEndPoint);

                    // Converte i dati ricevuti in una stringa
                    string request = Encoding.ASCII.GetString(data);


                    if (counter == 300)
                    {

                    }


                    // Se la richiesta è "checkMonitor", invia il valore di checkMonitor
                    if (request.Trim() == "checkMonitor")
                    {
                        string response = ValueCheckMonitor.ToString();//
                        // Converte il valore booleano in una stringa


                        if (fileNotExist)///
                        {
                            response = "False <NOFILE>";
                        }
                        else
                        {
                            if (erroreFile)//
                            {
                                response += " <ERROR>";
                            }
                        }

                        if (ValueCheckMonitor && !fileNotExist && !erroreFile) response += " " + NomeMon;
                        //string response = ValueCheckMonitor.ToString();

                        // Invia la risposta al mittente
                        byte[] responseData = Encoding.ASCII.GetBytes(response);
                        udpServer.Send(responseData, responseData.Length, clientEndPoint);




                        /*if ((DateTime.Now - startTime).TotalSeconds > timeToStop)
                        {
                            Console.WriteLine("Terminating Udp thread...");
                            udpServer.Close();
                            break;
                        }*/
                    }
                    ////////////////////////////////////////////////////
                }

            });

            // Avvia il thread del monitoraggio
            monitorThread.Start();
            communicationThread.Start();

            /*// Attendi l'input dell'utente per terminare il programma
            Console.WriteLine("Premi INVIO per uscire...");
            Console.ReadLine();

            // Imposta la variabile di stato per terminare il thread
            shouldStop = true;
            
*/

        }
    }
}
