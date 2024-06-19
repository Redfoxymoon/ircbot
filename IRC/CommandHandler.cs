using System.Threading.Channels;

namespace IRC;

public class CommandHandler(StreamReader reader, StreamWriter writer, string channel, string nick)
{
    private const char Trigger = '.';
    private List<ICommand> _cmds = [];
    
    /*private Command _ntStatusCommand = new NtStatusCommand(reader, writer);
    private Command _helloCommand = new HelloCommand(reader, writer);*/
    //private Command _pingCommand = new PingCommand(reader, writer, nick, channel);
    /*private Command _joinCommand = new JoinCommand(reader, writer);
    private Command _nickCommand = new NickCommand(reader, writer);*/

    public void RegisterCmd<T>(T cmd) where T : ICommand
    {
        _cmds.Add(cmd);
    }
    public async void Invoke(string inputLine)
    {
        try
        {
#if !DEBUG
            if(!inputLine.StartsWith("PING"))
#endif
            Console.WriteLine(inputLine);
            var ircPieces = inputLine.Split(' ');
            var isPrivmsg = inputLine.Contains("PRIVMSG");
            var ircMessageIndex = inputLine.IndexOf(" :", StringComparison.Ordinal);
            var ircMessage = "";
            var senderNick = "";
            IrcLine ircLine;
            if (ircPieces[0] == "PING")
            {
                ircLine = new IrcLine(ircPieces[0], ircPieces[1]);
                await Task.Run(() => new PingCommand().Run(reader, writer, ircLine));
            }
            else if (ircPieces[1] == "001")
            {
                ircLine = new IrcLine(channel);
                await Task.Run(() => new JoinCommand().Run(reader, writer, ircLine));
            }
            else if (ircMessageIndex != -1 && isPrivmsg)
            {
                senderNick = inputLine.Split('!')[0].Substring(1);
                ircMessage = inputLine.Substring(ircMessageIndex + 2);
                if (ircMessage.StartsWith(nick + ",") || ircMessage.StartsWith(nick + ":"))
                {
                    ircLine = new IrcLine("", senderNick, "", channel, "", "", "");
                    await Task.Run(() => new HelloCommand().Run(reader, writer, ircLine));
                }
                else
                {
                    foreach (var cmd in _cmds)
                    {
                        if (!ircMessage.StartsWith(Trigger + cmd.Cmd))
                            continue;
                        var arguments = HasArguments(ircMessage) ? ircMessage.Split(' ')[1] : "";
                        
                        ircLine = new IrcLine(ircPieces[0], senderNick, ircPieces[1], ircPieces[2], cmd.Cmd,
                            arguments, nick);
                        Console.WriteLine($"executing {cmd.Cmd}!");
                        await Task.Run(() => cmd.Run(reader, writer, ircLine));
                    }
                }
            }
        }
        catch (IndexOutOfRangeException e) // :-)
        {}
    }
    
    private static bool HasArguments(string s)
    {
        return s.Contains(' ');
    }
}