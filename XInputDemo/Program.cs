using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using XInputDotNetPure;

namespace XInputDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("program <IP address> <IP broadcast>");
                Environment.Exit(1);
            }

            Network broadcast = new Network();
            Network capcom = new Network();
            Video video = new Video();
            var tokenSource2 = new CancellationTokenSource();
            CancellationToken ct = tokenSource2.Token;

            capcom.ConnectedHandler += (String messages, NetworkStream stream) =>
            {
                Console.WriteLine("Robo connected.");
                tokenSource2.Cancel();
                byte[] buffer;
                byte[] oldBuffer = null;

                while (true)
                {
                    GamePadState state = GamePad.GetState(PlayerIndex.One);
                    if (state.IsConnected)
                    {
                        if (state.Buttons.Start == ButtonState.Pressed)
                        {
                            video.Stop();
                            Environment.Exit(1); // fuck
                        }
                        buffer = new byte[16];
                        
                        BitConverter.GetBytes(state.ThumbSticks.Left.X).CopyTo(buffer, 0);
                        BitConverter.GetBytes(state.ThumbSticks.Left.Y).CopyTo(buffer, 4);
                        BitConverter.GetBytes(state.Triggers.Right).CopyTo(buffer, 8);
                        BitConverter.GetBytes(state.Triggers.Left).CopyTo(buffer, 12);

                        if (oldBuffer == null || !buffer.SequenceEqual(oldBuffer))
                        {
                            stream.Write(buffer, 0, buffer.Length);
                            oldBuffer = buffer;
                        }
                    }

                    Thread.Sleep(16);
                }
            };

            video.Start(12345);
            Task.Factory.StartNew(() => {
                while(!ct.IsCancellationRequested)
                {
                    //ct.ThrowIfCancellationRequested();
                    //broadcast.SendBroadcast("192.168.134.255", 11000, "HED-Capcom v1.0\nIP:192.168.11.3\nCapcom:12000\nStream:12345"); // fuck
                    broadcast.SendBroadcast(args[1], 11000, String.Format("HED-Capcom v1.0\nIP:{0}\nCapcom:12000\nStream:12345",args[0])); // fuck
                    Thread.Sleep(1000);
                }
            }, tokenSource2.Token);

            // TODO resume broadcast when tcp connection closed

            Task.Factory.StartNew(() => {
                capcom.StartListener(12000);
            });

            Console.ReadKey();
            /*while (true)
            {
                GamePadState state = GamePad.GetState(PlayerIndex.One);
                Console.WriteLine("IsConnected {0} Packet #{1}", state.IsConnected, state.PacketNumber);
                Console.WriteLine("\tTriggers {0} {1}", state.Triggers.Left, state.Triggers.Right);
                Console.WriteLine("\tD-Pad {0} {1} {2} {3}", state.DPad.Up, state.DPad.Right, state.DPad.Down, state.DPad.Left);
                Console.WriteLine("\tButtons Start {0} Back {1} LeftStick {2} RightStick {3} LeftShoulder {4} RightShoulder {5} Guide {6} A {7} B {8} X {9} Y {10}",
                    state.Buttons.Start, state.Buttons.Back, state.Buttons.LeftStick, state.Buttons.RightStick, state.Buttons.LeftShoulder, state.Buttons.RightShoulder,
                    state.Buttons.Guide, state.Buttons.A, state.Buttons.B, state.Buttons.X, state.Buttons.Y);
                Console.WriteLine("\tSticks Left {0} {1} Right {2} {3}", state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y, state.ThumbSticks.Right.X, state.ThumbSticks.Right.Y);
                GamePad.SetVibration(PlayerIndex.One, state.Triggers.Left, state.Triggers.Right);
                Thread.Sleep(16);
            }*/
        }
    }
}
