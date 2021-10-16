﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using RustedWarfareClient.Models;

using static RustedWarfareClient.PacketUtils;

namespace RustedWarfareClient
{
    internal class Program
    {
        public static void Main()
        {
            Socket socket = new(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("192.168.0.100", 5123);
            //socket.Connect("localhost", 5123);
            SendPreregisterConnection(socket, new PreregisterPacketTemplate());
            RegisterPacketTemplate? registered = ReceiveRegisterConnection(socket);
            SendPlayerInfo(socket, new SendPlayerTemplate("1dNDN", registered.ServerKey, registered.ServerUuid));
            byte[] bytes = new byte[4000];
            socket.Receive(bytes);
            
            Console.WriteLine(registered.ServerKey);
            socket.Close();
        }
        
        /**
         * Registering players with the server
         * Trigger conditions : 161
         * Send : Packet-110
         */
        private static void SendPlayerInfo(Socket socket, SendPlayerTemplate template)
        {
            List<byte> bytes = new();
            byte[] randomBytes = new byte[64];
            new Random().NextBytes(randomBytes);
            
            WriteStringToPacket(ref bytes, template.PackageName);
            WriteIntToPacket(ref bytes, template.ProtocolVersion);
            WriteIntToPacket(ref bytes, template.GameVersion);
            WriteIntToPacket(ref bytes, template.GameVersion);
            WriteStringToPacket(ref bytes, template.Nickname);

            if (template.Password == "")
            {
                bytes.Add(0);
            } else
            {
                bytes.Add(1);
                WriteStringToPacket(ref bytes, ComputeSha256Hash(template.Password).ToUpper());
            }

            WriteStringToPacket(ref bytes, template.AnotherPackageName);
            WriteStringToPacket(ref bytes, ComputeUuidForPacket(template.ClientUuid, template.ServerUuid));
            WriteIntToPacket(ref bytes, template.AnotherMagicValue);
            WriteStringToPacket(ref bytes, template.Token);
            Console.WriteLine(template.Token);
            socket.Send(CreatePacket(PacketType.PACKET_PLAYER_INFO, bytes));
        }

        private static RegisterPacketTemplate ReceiveRegisterConnection(Socket socket)
        {
            List<byte> bytes = new();
            byte[] buffer = new byte[1024];
            do
            {
                socket.Receive(buffer, buffer.Length, 0);
                bytes.AddRange(buffer);
            } while (socket.Available > 0);

            int offset = 0;

            return new RegisterPacketTemplate {
                PayloadSize = ReadIntFromPacket(bytes, ref offset),
                Type = ReadIntFromPacket(bytes, ref offset),
                ServerId = ReadStringFromPacket(bytes, ref offset),
                ProtocolVersion = ReadIntFromPacket(bytes, ref offset),
                GameVersion = ReadIntFromPacket(bytes, ref offset),
                AnotherGameVersion = ReadIntFromPacket(bytes, ref offset),
                PkgName = ReadStringFromPacket(bytes, ref offset),
                ServerUuid = ReadStringFromPacket(bytes, ref offset),
                ServerKey = ReadIntFromPacket(bytes, ref offset)
            };
        }
        
        /**
         * Send initial packet to server, start handshake with server
         * Trigger conditions : Not
         * Send : Packet-160
         */

        private static void SendPreregisterConnection(Socket socket, PreregisterPacketTemplate template )
        {
            List<byte> bytes = new();

            WriteStringToPacket(ref bytes, template.PackageName);
            WriteIntToPacket(ref bytes, template.ProtocolVersion);
            WriteIntToPacket(ref bytes, template.GameVersion);
            WriteIntToPacket(ref bytes, template.AnotherGameVersion);

            if (template.ProtocolVersion >= 2)
            {
                WriteIsStringToPacket(ref bytes,template.RelayID);
            }
            if (template.ProtocolVersion >= 3)
            {
                WriteStringToPacket(ref bytes,template.Nickname);
            }

            socket.Send(CreatePacket(PacketType.PACKET_PREREGISTER_CONNECTION, bytes));
        }
        
        /**
         * Return ping packets to the server, so that the server normal display delay
         * Trigger conditions : Packet-108
         * Send : Packet-109
         */
        
        private static void SendReturnPingPacket(Socket socket, long time)
        {
            List<byte> bytes = new();
            
            //TODO Write Long
            /*
             * Long : [Packet-108] -> First eight valid bytes
             * Byte : 1
             * Byte : 60
             */
            bytes.Add(1);
            bytes.Add(60);


            socket.Send(CreatePacket(PacketType.PACKET_HEART_BEAT_RESPONSE, bytes));
        }
    }
}