namespace IRC;

public class HelloCommand : ICommand
{
    public string Cmd => "abc";

    public void Run(StreamReader reader, StreamWriter writer, IrcLine ircLine)
    {
        Console.WriteLine("hello command" + ircLine.command);
        if (ircLine.senderNick == "midipix")
            Msg.SendPrivMsg(writer, ircLine.channel, (char)0x01 + "ACTION beheads midipix" + (char)0x01);//<-- dunno
        else
            Msg.SendPrivMsg(writer, ircLine.channel, $"{ircLine.senderNick}, hi!");
    }
}