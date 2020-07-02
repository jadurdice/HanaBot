using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace hana
{
    class Program
    {
        private DiscordSocketClient _client;
        public static CommandService _commands;
        public static IServiceProvider _services;

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            LoadVocab();

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info
            });
            _client.Log += Log;
            _commands = new CommandService();
            _services = new ServiceCollection().BuildServiceProvider();
            _client.MessageReceived += CommandRecieved;
            //次の行に書かれているstring token = "hoge"に先程取得したDiscordTokenを指定する。

            string token = "";

            StreamReader sr = new StreamReader(@"Token.txt", Encoding.GetEncoding("UTF-8"));
            token = sr.ReadToEnd();
            sr.Close();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            

            await Task.Delay(-1);
        }

        /// <summary>
        /// 何かしらのメッセージの受信
        /// </summary>
        /// <param name="msgParam"></param>
        /// <returns></returns>
        private async Task CommandRecieved(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;

            //デバッグ用メッセージを出力
            Console.WriteLine("{0} {1}:{2}", message.Channel.Name, message.Author.Username, message);
            //メッセージがnullの場合
            if (message == null)
                return;

            //発言者がBotの場合無視する
            if (message.Author.IsBot)
                return;

            var context = new CommandContext(_client, message);

            //ここから記述--------------------------------------------------------------------------
            var CommandContext = message.Content;

            if (Dayone(ref CommandContext))
            {
                await message.Channel.SendMessageAsync(CommandContext);
            }

        }

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        List<string> includeDa = new List<string>();

        List<string> notIncludeDa = new List<string>();

        string dayone = "だよね！";

        void LoadVocab()
        {
            Console.WriteLine("ファイル読み込む");
            StreamReader sr = new StreamReader(@"DaIn.txt", Encoding.GetEncoding("UTF-8"));
            string str = "";
            while (sr.EndOfStream == false)
            {
                str = sr.ReadLine();
                includeDa.Add(str);
            }
            sr.Close();

            sr = new StreamReader(@"DaNotIn.txt", Encoding.GetEncoding("UTF-8"));
            while (sr.EndOfStream == false)
            {
                str = sr.ReadLine();
                notIncludeDa.Add(str);
            }
            sr.Close();

            string context = "";
            context += "☆「だ」含む\n";

            foreach (var item in includeDa)
            {
                context += item + "\n";
            }

            context += "\n☆「だ」含まない\n";

            foreach (var item in notIncludeDa)
            {
                context += item + "\n";
            }

            Console.Write(context);
        }

        void SaveVocab()
        {
            StreamWriter sw = new StreamWriter(@"DaIn.txt", false, Encoding.GetEncoding("UTF-8"));
            foreach (var v in includeDa)
            {
                sw.WriteLine(v);
                Console.WriteLine(v);
            }
            sw.Close();

            sw = new StreamWriter(@"DaNotIn.txt", false, Encoding.GetEncoding("UTF-8"));
            foreach (var v in notIncludeDa)
            {
                sw.WriteLine(v); Console.WriteLine(v);
            }
            sw.Close();
            
        }

        void ClearVocab()
        {

        }

        bool Dayone(ref string context)
        {
            { //ADD
                if (context.Contains("/add "))
                {
                    //削除する文字の配列
                    char[] removeChars = {'/', 'a', 'd', ' ' };

                    //削除する文字を1文字ずつ削除する
                    foreach (char c in removeChars)
                    {
                        context = context.Replace(c.ToString(),"");
                    }

                    if(context.Length < 3)
                    {
                        context = "2文字以内だめって言ったよね！";
                        return true;
                    }

                    foreach (var item in includeDa)
                    {
                        if (context.Contains(item))
                        {
                            context = context + "のこれ、もうあったよね！";
                            return true;
                        }
                    }

                    foreach (var item in notIncludeDa)
                    {
                        if (context.Contains(item))
                        {
                            context = context + "のこれ、もうあったよね！";
                            return true;
                        }
                    }

                    if (context.Contains("た")|| context.Contains("だ"))
                    {
                        includeDa.Add(context);
                    }
                    else
                    {
                        notIncludeDa.Add(context);
                    }

                    SaveVocab();

                    context = "これ、" + context + "を追加しだよね！";
                    return true;
                }
            }

            { 
            if(context == "/help")
            {
                context = "これ、対応するキーワード：\n";

                context += "☆「だ」含む\n";

                foreach (var item in includeDa)
                {
                    context += item + "\n";
                }

                context += "\n☆「だ」含まない\n";

                foreach (var item in notIncludeDa)
                {
                    context += item + "\n";
                }
                context += "\nだよね！";

                return true;
            }
            }


            string kore = "これ";
            string da = "だ";
            string yone = "よね！";

            if (context==(dayone))
            {
                context = dayone;
                return true;
            }

            foreach (var item in includeDa)
            {
                if (context.Contains(item))
                {
                    context = kore + item + yone;
                    return true;
                }
            }

            foreach (var item in notIncludeDa)
            {
                if (context.Contains(item))
                {
                    context = kore + item + da + yone;
                    return true;
                }
            }
            
            return false;
        }

        
    }
    
}