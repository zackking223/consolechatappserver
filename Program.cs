using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ChatAppConsoleServer
{
    class Program
    {
        const int MAX_CONNECTION = 90;
        const int PORT_NUMBER = 9999;
        
        public static Dictionary<string, SocketProfile> MySocketList = new Dictionary<string, SocketProfile>();
        public static Dictionary<string, List<string>> ListOfList = new Dictionary<string, List<string>>(); 

        static TcpListener listener;
        public static string EXIT_ROOM_CODE = "qwjeklqwjklfnmmasasm,dasn,dwqqwkl:wqe;;e";
        public static string INVITATION_CODE = "qwjeklqwjk;dqwlw.q/we.qwe,qweqwrk]";
        public static string ENTER_ROOM_CODE = "vmaskklrqwopwqekdklsldadwq,;wqeq";

        public static void Main()
        {
            IPAddress address = IPAddress.Parse("127.0.0.1");

            listener = new TcpListener(address, PORT_NUMBER);
            Console.WriteLine("Waiting for connection...");
            listener.Start();

            for (int i = 0; i < MAX_CONNECTION; i++)
            {
                Task.Run(DoWork);
            }
            Console.ReadKey();
        }

        static async Task SendMessageToId(string senderID, string receiverID, string message)
        {
            using (NetworkStream stream = new NetworkStream(MySocketList[receiverID].theSocket))
            {
                try
                {
                    var writer = new StreamWriter(stream);
                    writer.AutoFlush = true;

                    if (MySocketList[receiverID].type == "PrivateChat")
                    {
                        if (MySocketList[receiverID].receiverName == senderID)
                        {
                            if (message == EXIT_ROOM_CODE)
                            {
                                MessageObj msg = new MessageObj()
                                {
                                    Type = "SendMessage",
                                    Sender = senderID,
                                    Receiver = receiverID,
                                    Message = $"Người dùng {senderID} đã rời khỏi cuộc trò truyện",
                                    Role = "None",
                                    RoomName = "None"
                                };
                                string jsonString = JsonSerializer.Serialize<MessageObj>(msg);
                                await writer.WriteLineAsync(jsonString);
                                Console.WriteLine($"{senderID} sent {receiverID}:Người dùng {senderID} đã rời khỏi cuộc trò truyện");
                            }
                            
                            else
                            {
                                MessageObj msg = new MessageObj()
                                {
                                    Type = "SendMessage",
                                    Sender = senderID,
                                    Receiver = receiverID,
                                    Message = $"{senderID}: {message}",
                                    Role = "None",
                                    RoomName = "None"
                                };
                                string jsonString = JsonSerializer.Serialize<MessageObj>(msg);
                                await writer.WriteLineAsync(jsonString);
                                Console.WriteLine($"{senderID} sent {receiverID}: {message}");
                            }
                        }
                    } 
                    writer.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                }
            }
        }

        static async Task SendMessageToId(string senderID, string receiverID, string message, StreamWriter senderWriter)
        {
            using (NetworkStream stream = new NetworkStream(MySocketList[receiverID].theSocket))
            {
                try
                {
                    var writer = new StreamWriter(stream);
                    writer.AutoFlush = true;

                    //What is the socket's type? PrivateChat or GroupChat
                    if (MySocketList[receiverID].type == "PrivateChat")
                    {
                        //If it's PrivateChat then whom it talking with?
                        if (MySocketList[receiverID].receiverName == senderID)
                        {
                            if (message == EXIT_ROOM_CODE)
                            {
                                MessageObj msg = new MessageObj()
                                {
                                    Type = "SendMessage",
                                    Sender = senderID,
                                    Receiver = receiverID,
                                    Message = $"Người dùng {senderID} đã rời khỏi cuộc trò truyện",
                                    Role = "None",
                                    RoomName = "None"
                                };
                                string jsonString = JsonSerializer.Serialize<MessageObj>(msg);
                                await writer.WriteLineAsync(jsonString);
                                Console.WriteLine($"{senderID} sent {receiverID}:Người dùng {senderID} đã rời khỏi cuộc trò truyện");
                            }

                            else
                            {
                                MessageObj msg = new MessageObj()
                                {
                                    Type = "SendMessage",
                                    Sender = senderID,
                                    Receiver = receiverID,
                                    Message = $"{senderID}: {message}",
                                    Role = "None",
                                    RoomName = "None"
                                };
                                string jsonString = JsonSerializer.Serialize<MessageObj>(msg);
                                await writer.WriteLineAsync(jsonString);
                            }
                        }
                    } else
                    {
                        MessageObj msg = new MessageObj()
                        {
                            Type = "SendMessage",
                            Sender = senderID,
                            Receiver = senderID,
                            Message = $"Người dùng {receiverID} chưa tham gia phòng!",
                            Role = "None",
                            RoomName = "None"
                        };
                        string jsonString = JsonSerializer.Serialize<MessageObj>(msg);
                        await senderWriter.WriteLineAsync(jsonString);
                        Console.WriteLine($"{senderID} sent {receiverID}: {message}");
                    }
                    writer.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                }
            }
        }

        static async Task SendMessageToGroup(string senderID, string RoomID, List<string> receiverIDs, string message)
        {
            foreach (string receiverID in receiverIDs)
            {
                //Kiem tra xem user con online ko
                if (MySocketList.ContainsKey(receiverID))
                {
                    if (SocketConnected(MySocketList[receiverID].theSocket))
                    {
                        using (NetworkStream stream = new NetworkStream(MySocketList[receiverID].theSocket))
                        {
                            try
                            {
                                var writer = new StreamWriter(stream);
                                writer.AutoFlush = true;

                                //What is socket's type? PrivateChat or GroupChat
                                if (MySocketList[receiverID].type == "GroupChat")
                                {
                                    //If it's GroupChat then what is the Room name?
                                    if (MySocketList[receiverID].roomName == RoomID && senderID != receiverID)
                                    {
                                        if (message == EXIT_ROOM_CODE)
                                        {
                                            MessageObj msg = new MessageObj()
                                            {
                                                Type = "SendMessage",
                                                Sender = senderID,
                                                Receiver = receiverID,
                                                Message = $"Người dùng {senderID} đã rời khỏi cuộc trò truyện",
                                                Role = "None",
                                                RoomName = "None"
                                            };
                                            string jsonString = JsonSerializer.Serialize<MessageObj>(msg);
                                            await writer.WriteLineAsync(jsonString);
                                            Console.WriteLine($"{senderID} sent room {RoomID}: Người dùng {senderID} đã rời khỏi cuộc trò truyện");
                                        }
                                        else if (message == ENTER_ROOM_CODE)
                                        {
                                            MessageObj msg = new MessageObj()
                                            {
                                                Type = "SendMessage",
                                                Sender = senderID,
                                                Receiver = receiverID,
                                                Message = $"Người dùng {senderID} đã tham gia cuộc trò truyện",
                                                Role = "None",
                                                RoomName = "None"
                                            };
                                            string jsonString = JsonSerializer.Serialize<MessageObj>(msg);
                                            await writer.WriteLineAsync(jsonString);
                                            Console.WriteLine($"{senderID} sent room {RoomID}: Người dùng {senderID} đã tham gia cuộc trò truyện");
                                        }
                                        else 
                                        {
                                            MessageObj msg = new MessageObj()
                                            {
                                                Type = "SendMessage",
                                                Sender = senderID,
                                                Receiver = receiverID,
                                                Message = $"{senderID}: {message}",
                                                Role = "None",
                                                RoomName = "None"
                                            };
                                            string jsonString = JsonSerializer.Serialize<MessageObj>(msg);
                                            await writer.WriteLineAsync(jsonString);
                                            Console.WriteLine($"{senderID} sent room {RoomID}: {message}");
                                        }
                                    }
                                }
                                writer.Close();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error: " + ex);
                            }
                        }
                    }
                }
            }
        }


        static bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        static async Task DoWork()
        {
            while (true)
            {
                Socket soc = await listener.AcceptSocketAsync();

                var stream = new NetworkStream(soc);
                var writer = new StreamWriter(stream);
                writer.AutoFlush = true;
                var reader = new StreamReader(stream);

                Console.WriteLine("Start deserlizing...");
                //Check if client request for private or group chat
                string jsonString = await reader.ReadLineAsync();
                Console.WriteLine(jsonString);
                MessageObj roomType = JsonSerializer.Deserialize<MessageObj>(jsonString);

                Console.WriteLine("Room type: " + roomType.Type);
                if (roomType.Type == "PrivateChat")
                {
                    //Get sender's username and receiver's username
                    string socketID = roomType.Sender;
                    string id = roomType.Receiver;
                    Console.WriteLine("Adding new PrivateChat socket to list");
                    MySocketList.Add(socketID, new SocketProfile() { theSocket = soc, type = roomType.Type, receiverName = id, roomName = "" });
                    Console.WriteLine("Adding success");
                    Console.WriteLine("\nConnection received from: {0}, Username: {1}, Receiver: {2}",
                                        soc.RemoteEndPoint, socketID, id);
                    Console.WriteLine("--Clients count: {0}--", MySocketList.Count);
                    try
                    {
                        while (true)
                        {
                            //Wait for new messages:
                            jsonString = await reader.ReadLineAsync();
                            MessageObj message = JsonSerializer.Deserialize<MessageObj>(jsonString);

                            if (message.Message == EXIT_ROOM_CODE)
                            {
                                if (MySocketList.ContainsKey(id))
                                {
                                    //Tell the receiver the sender has left the room
                                    await SendMessageToId(socketID, id, EXIT_ROOM_CODE);
                                    Console.WriteLine("Say good bye complete");
                                    //Tell form to close
                                    MessageObj msg = new MessageObj()
                                    {
                                        Type = "CloseForm",
                                        Sender = socketID,
                                        Receiver = "None",
                                        Message = "None",
                                        Role = "None",
                                        RoomName = "None"
                                    };
                                    jsonString = JsonSerializer.Serialize<MessageObj>(msg);
                                    await writer.WriteLineAsync(jsonString);
                                }
                                break;
                            }

                            //Check if user is online
                            if (MySocketList.ContainsKey(id))
                            {
                                if (SocketConnected(MySocketList[id].theSocket))
                                {
                                    await SendMessageToId(socketID, id, message.Message, writer);
                                }
                            }
                            else
                            {
                                //Tell the user the receiver is not online!
                                MessageObj msg = new MessageObj()
                                {
                                    Type = "SendMessage",
                                    Sender = socketID,
                                    Receiver = id,
                                    Message = $"Người dùng {id} không online!",
                                    Role = "None",
                                    RoomName = "None"
                                };
                                jsonString = JsonSerializer.Serialize<MessageObj>(msg);
                                await writer.WriteLineAsync(jsonString);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex);
                    }
                    if (MySocketList.Remove(socketID))
                    {
                        Console.WriteLine("\nClient disconnected: {0}, Username: {1}, Receiver: {2}",
                                            soc.RemoteEndPoint, socketID, id);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to remove {socketID}, privateChat from SocketList");
                    }
                }

                else if (roomType.Type == "GroupChat")
                {
                    //List of ids inside the room
                    List<string> receiverIDs = new List<string>();

                    //Get sender's username and room's id
                    string socketID = roomType.Sender;
                    Console.WriteLine($"SocketID: {socketID}");

                    string roomID = roomType.RoomName; //Also = Owner's name
                    Console.WriteLine($"RoomID: {roomID}");

                    //Get user's role, Owner or Guest?
                    string role = roomType.Role;
                    Console.WriteLine($"Role: {role}");

                    Console.WriteLine("Adding new GroupChat socket to list");
                    MySocketList.Add(socketID, new SocketProfile() { theSocket = soc, type = roomType.Type, receiverName = "", roomName = roomID });
                    Console.WriteLine("Add success");

                    if (role == "Owner")
                    {
                        receiverIDs.Add(socketID);
                        ListOfList.Add(roomID, receiverIDs);
                    }
                    else
                    {
                        if (!ListOfList[roomID].Contains(socketID))
                        {
                            ListOfList[roomID].Add(socketID);
                            await SendMessageToGroup(socketID, roomID, ListOfList[roomID], ENTER_ROOM_CODE);
                        }
                    }

                    Console.WriteLine("\nConnection received from a groupChat: {0}, Username: {1}, Room: {2}",
                                        soc.RemoteEndPoint, socketID, roomID);
                    Console.WriteLine("--Clients count: {0}--", MySocketList.Count);
                    try
                    {
                        while (true)
                        {
                            MessageObj requestType = JsonSerializer.Deserialize<MessageObj>(await reader.ReadLineAsync());

                            if (requestType.Type == "SendMessage")
                            {
                                await SendMessageToGroup(socketID, roomID, ListOfList[roomID], requestType.Message);
                            }
                            else if (requestType.Type == "AddReceiver")
                            {
                                string receiverID = requestType.Receiver;

                                //Check if user is online
                                if (MySocketList.ContainsKey(receiverID))
                                {
                                    if (SocketConnected(MySocketList[receiverID].theSocket))
                                    {
                                        using (NetworkStream receiverStream = new NetworkStream(MySocketList[receiverID].theSocket))
                                        {
                                            try
                                            {
                                                var receiverWriter = new StreamWriter(receiverStream);
                                                receiverWriter.AutoFlush = true;
                                                var receiverReader = new StreamReader(receiverStream);
                                                //What is the socket's type? PrivateChat or GroupChat

                                                if (MySocketList[receiverID].type == "Lobby")
                                                {
                                                    MessageObj msg2 = new MessageObj()
                                                    {
                                                        Type = "InviteToRoom",
                                                        Sender = socketID,
                                                        Receiver = receiverID,
                                                        Message = $"Người dùng {socketID} mời bạn tham gia nhóm chat",
                                                        Role = role,
                                                        RoomName = roomID
                                                    };
                                                    jsonString = JsonSerializer.Serialize<MessageObj>(msg2);
                                                    await receiverWriter.WriteLineAsync(jsonString);
                                                    Console.WriteLine($"Người dùng {socketID} mời bạn tham gia nhóm chat");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine("Error: " + ex);
                                            }
                                        }
                                        MessageObj msg = new MessageObj()
                                        {
                                            Type = "SendMessage",
                                            Sender = socketID,
                                            Receiver = "None",
                                            Message = $"Đã gửi lời mời đến {receiverID}!",
                                            Role = role,
                                            RoomName = roomID
                                        };
                                        jsonString = JsonSerializer.Serialize<MessageObj>(msg);
                                        await writer.WriteLineAsync(jsonString);
                                    }
                                }
                                else
                                {
                                    MessageObj msg = new MessageObj()
                                    {
                                        Type = "SendMessage",
                                        Sender = socketID,
                                        Receiver = socketID,
                                        Message = $"{receiverID} Không online!",
                                        Role = role,
                                        RoomName = roomID
                                    };
                                    jsonString = JsonSerializer.Serialize<MessageObj>(msg);
                                    await writer.WriteLineAsync(jsonString);
                                    await SendMessageToGroup(socketID, socketID, receiverIDs, $"{receiverID} Không online!");
                                }
                            }
                            else if (requestType.Type == EXIT_ROOM_CODE)
                            {
                                //Say goodbye to other users
                                await SendMessageToGroup(socketID, roomID, ListOfList[roomID], EXIT_ROOM_CODE);
                                Console.WriteLine("Say good bye complete");
                                //Tell form to close
                                MessageObj msg = new MessageObj()
                                {
                                    Type = "CloseForm",
                                    Sender = socketID,
                                    Receiver = "None",
                                    Message = "None",
                                    Role = role,
                                    RoomName = roomID
                                };
                                jsonString = JsonSerializer.Serialize<MessageObj>(msg);
                                await writer.WriteLineAsync(jsonString);
                                
                                //Break the loop
                                break;
                            }
                        }
                        if (ListOfList[roomID].Count <= 1)
                        {
                            ListOfList.Remove(roomID);
                        }
                        else
                        {
                            ListOfList[roomID].Remove(socketID);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex);
                    }
                    
                    if (MySocketList.Remove(socketID))
                    {
                        Console.WriteLine("\nClient disconnected from room: {0}, Username: {1}, Room: {2}",
                                            soc.RemoteEndPoint, socketID, roomID);
                    } else
                    {
                        Console.WriteLine($"Failed to remove groupchatmember {socketID}, room {roomID} from SocketList");
                    }
                }

                else if (roomType.Type == "Lobby")
                {
                    //Request sender's name:
                    string socketID = roomType.Sender;
                    Console.WriteLine("Adding new LobbyChat socket to list");
                    try
                    {
                        if (MySocketList.ContainsKey(socketID))
                        {
                            try
                            {
                                if (MySocketList.Remove(socketID))
                                {
                                    Console.WriteLine($"Removed {socketID} from list");
                                }
                            } catch (Exception ex)
                            {
                                Console.WriteLine("Error: " + ex);
                            }
                        }
                        MySocketList.Add(socketID, new SocketProfile() { theSocket = soc, type = roomType.Type, receiverName = "", roomName = "" });
                        Console.WriteLine("Adding succeed!");
                    } catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex);
                    }

                    Console.WriteLine("\nConnection received from a Lobby: {0}, Username: {1}",
                                        soc.RemoteEndPoint, socketID);
                    Console.WriteLine("--Clients count: {0}--", MySocketList.Count);

                    while (true)
                    {
                        MessageObj message = JsonSerializer.Deserialize<MessageObj>(await reader.ReadLineAsync());
                        Console.WriteLine($"{socketID} send: {message.Message}");
                        if (message.Type == EXIT_ROOM_CODE)
                        {
                            break;
                        }
                        else if (message.Type == "DenyInvitation")
                        {
                            await SendMessageToGroup(socketID, message.RoomName, ListOfList[message.RoomName], message.Message);
                        }
                        else if (message.Type == "InviteReceived")
                        {
                            Console.WriteLine($"{message.Sender}: {message.Message}");
                        }

                    }
                   
                    if (MySocketList.Remove(socketID))
                    {
                        Console.WriteLine("\nLobby disconnected: {0}, Username: {1}",
                                            soc.RemoteEndPoint, socketID);
                    } else
                    {
                        Console.WriteLine($"Failed to remove lobby {socketID} from SocketList");
                    }
                }
                reader.Close();
                writer.Close();
                stream.Close();
                Console.WriteLine("\n--Clients count: {0}--", MySocketList.Count);
                soc.Close();
            }
        }
    }
}
