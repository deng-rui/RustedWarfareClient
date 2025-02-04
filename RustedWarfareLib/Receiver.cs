using System;
using System.Collections.Generic;
using System.Threading;

using RustedWarfareLib.Models;

namespace RustedWarfareLib
{
    public partial class Api
    {
        private void ReceiveThread(object obj)
        {
            CancellationToken token = (CancellationToken)obj;

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(10);

                List<byte> bytes = ReceiveBytes();
                if (bytes.Count == 0)
                    continue;
                SwitchPackets(bytes);
            }

            SendDisconnect();
        }

        private List<byte> ReceiveBytes()
        {
            List<byte> bytes = new();
            byte[] buffer = new byte[1024];

            while (socket.Available > 0)
            {
                socket.Receive(buffer, buffer.Length, 0);
                bytes.AddRange(buffer);
            }

            return bytes;
        }

        private void SwitchPackets(List<byte> bytes)
        {
            switch (Packet.GetType(bytes.ToArray()))
            {

                case PacketType.PACKET_UNKNOWN: break;
                case PacketType.PACKET_SERVER_DEBUG: break;
                case PacketType.PACKET_REGISTER_CONNECTION:
                    SendRegisterConnection(bytes);
                    SendPlayerInfo();
                    break;
                case PacketType.PACKET_TEAM_LIST:
                    ReceiveTeamList(bytes);
                    break;
                case PacketType.PACKET_HEART_BEAT:
                    long pingNumber = ReceiveHeartBeat(bytes);
                    SendHeartBeatResponse(pingNumber);
                    break;
                case PacketType.PACKET_SEND_CHAT: break;
                case PacketType.PACKET_SERVER_INFO:
                    ReceiveServerInfo(bytes);
                    break;
                case PacketType.PACKET_KICK: break;
                case PacketType.PACKET_SYNCCHECKSUM_STATUS: break;
                case PacketType.PACKET_A: break;
                case PacketType.PACKET_PREREGISTER_CONNECTION: break;
                case PacketType.PACKET_HEART_BEAT_RESPONSE: break;
                case PacketType.PACKET_ADD_CHAT: break;
                case PacketType.PACKET_PLAYER_INFO: break;
                case PacketType.PACKET_DISCONNECT: break;
                case PacketType.PACKET_ACCEPT_START_GAME: break;
                case PacketType.PACKET_ACCEPT_BUTTON_GAME: break;
                case PacketType.PACKET_TICK: break;
                case PacketType.PACKET_SYNC: break;
                case PacketType.PACKET_START_GAME: break;
                case PacketType.PACKET_PASSWD_ERROR: break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
