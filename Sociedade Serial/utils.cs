
using System.IO.Ports;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System;
using System.Threading;
namespace Sociedade_Serial
{
    public class utils
    {
        public struct script
        {
            public string name;
            public List<command> commands;
        }
        public struct command
        {
            public string name;
            public string send_script;
            public string send;
            public crc checksum;
            public string receive_script;
            public string receive;

        }
        
        public struct testFrames
        {
            public string raw, processed;
        }
        public struct test_results {
            public testFrames sent;
            public string expectedAnswer;
            public testFrames realAnswer;
            public bool evaluation;
        }

        public struct ScriptAnswer
        {
            public bool newRound;
            public string processed_frame;
            public string error;
        };


        public class constants
        {
            public readonly int[] baudRates = new int[] { 110, 300, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 };
            public readonly int[] dataBits = new int[] { 5, 6, 7, 8, };
            public readonly string[] stopBits = new string[] { "1", "2", "1.5" };
            public readonly string[] parity = new string[] { "None", "Odd", "Even", "Mark", "Space" };
            public readonly string[] flowControl = new string[] { "None", "XOnXOff", "RequestToSend", "RequestToSendXOnXOff" };
        }

        public static script loadScript(string address)
        {
             using (StreamReader r = new StreamReader(address))
            {
                string json = r.ReadToEnd();
                script x = JsonConvert.DeserializeObject<script>(json);

                return x;

            }


        }

        public static void saveScript(string address, script s)
        {
            using (StreamWriter w = new StreamWriter(address))
            {
                w.Write(JsonConvert.SerializeObject(s));
                
            }


        }
        public static string getFrameFormat(string str, bool inverse)
        {
            string s = "";
            str = str.Replace(" ", "");
            if (!inverse)
            {
                for (int i=0; i < str.Length; i++)
                {
                    if (i % 2 == 0 & i > 0) s += " ";
                    s += str[i];
                }
            }
            else
            {

                for (int i = str.Length-1; i >= 0; i-=2)
                {                   
                    s += $"{str[i-1]}{str[i]} ";
                }
            }
            return s;
        }

        public static ScriptAnswer callScript(string script_name, string raw_send, string raw_answer, int round)
        {
            Process proc = new Process();
            
            if (script_name.IndexOf("\\") < 0 && script_name.IndexOf("/") < 0) script_name = $"T:\\Laboratórios\\Equipamentos de Uso Profissional e Infra-Estrutura\\Verificação de Software\\11 - Compartilhado\\06 - Scripts\\{script_name}";

            if (!File.Exists(script_name)){
                return new ScriptAnswer()
                {
                    error = $"Script {script_name} não encontrado!",
                    newRound = false,
                    processed_frame = ""
                };

            }
            proc.StartInfo = new ProcessStartInfo
            {
                FileName = "node.exe",
                CreateNoWindow = true,
                Arguments = $"\"{script_name}\" \"raw_send:{raw_send}\" \"round:{round.ToString()}\" \"raw_answer:{raw_answer}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            ScriptAnswer sa = new ScriptAnswer();

            proc.Start();

            string errorMessage = proc.StandardError.ReadToEnd();

            while (!proc.HasExited && proc.Responding)
            {
                Thread.Sleep(50);
            }

            if (proc.ExitCode == 0)
            {

                sa.newRound = false;

                string error = "";

                while (!proc.StandardOutput.EndOfStream)
                {
                    string str = proc.StandardOutput.ReadLine().Replace(",", "").Replace("'", "");

                    error += $"{str}\n";

                    if (str.IndexOf("newRound") != -1)
                    {
                        string pstr = str.Substring(str.IndexOf("newRound"));
                        sa.newRound = pstr.Split(':')[1].Trim() == "true";
                    }
                    else if (str.IndexOf("processed_frame") != -1)
                    {
                        string pstr = str.Substring(str.IndexOf("processed_frame"));
                        sa.processed_frame = pstr.Split(':')[1].Trim();
                    }
                    else if (str.IndexOf("error") != -1)
                    {
                        string pstr = str.Substring(str.IndexOf("error"));
                        sa.error = pstr.Split(':')[1].Trim();
                    }
                }

            }
            else
            {
                sa.error = errorMessage;
                
            }




            
            return sa;
        }
        
        public static byte[] str2Hex(string str)
        {
            
            List<byte> b = new List<byte>();
            str = str.Replace(" ", "");

            bool i = false;
            string strHex = "";

            if (str.Length % 2 > 0)
                str += "0";

            foreach (char c in str)
            {
                strHex += c.ToString();
                if (i)
                {
                    try
                    {
                        b.Add(byte.Parse(strHex, NumberStyles.HexNumber));
                        strHex = "";
                    }
                    catch
                    {
                        Console.WriteLine(str);
                    }
                    
                }
                i = !i;

            }

            return b.ToArray();
        }


    }

  
    
}
