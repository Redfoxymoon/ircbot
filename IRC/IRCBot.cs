using System.Net.Sockets;

namespace IRC;

    public class IrcBot
    {
        private readonly string _server;
        private readonly int _port;
        private readonly string _user;
        private readonly string _nick;
        private readonly string _channel;

        private List<ICommand> cmds;
        private readonly int _maxRetries;

        private StreamReader _reader;
        private StreamWriter _writer;
        private CommandHandler _commandhandler;
        
        public IrcBot(string server, int port, string user, string nick, string channel, int maxRetries = 3)
        {
            _server = server;
            _port = port;
            _user = user;
            _nick = nick;
            _channel = channel;
            _maxRetries = maxRetries;
        }

        public void Start()
        {
            var retry = false;
            var retryCount = 0;
            
            do
            {
                try
                {
                    var irc = new TcpClient(_server, _port);
                    var stream = irc.GetStream();
                    _reader = new StreamReader(stream);
                    _writer = new StreamWriter(stream);
                    Msg.SendRawMsg(_writer, "NICK " + _nick);
                    Msg.SendRawMsg(_writer, _user);

                    Msg.SendPrivMsg(_writer, "NickServ", "identify <snip>);
                    
                    _commandhandler = new CommandHandler(_reader, _writer, _channel, _nick);
 
                    _commandhandler.RegisterCmd(new HelloCommand());
                    _commandhandler.RegisterCmd(new NickCommand());
                    _commandhandler.RegisterCmd(new JoinCommand());
                    _commandhandler.RegisterCmd(new NtStatusCommand());
                    _commandhandler.RegisterCmd(new TestCommand());

                    while (true)
                    {
                        // ReSharper disable once MoveVariableDeclarationInsideLoopCondition
                        string inputLine;
                        while ((inputLine = _reader.ReadLine()) != null)
                            _commandhandler.Invoke(inputLine);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Thread.Sleep(5000);
                    retry = ++retryCount <= _maxRetries;
                }
            } while (retry);
        }
    }