using System;
using System.Collections.Generic;
using System.IO;
using TLSharp.Core.MTProto;

namespace TLSharp.Core.Requests
{
    class GetMessagesRequest : MTProtoRequest
    {
        List<int> _id;

        public List<Message> messages;
        public List<Chat> chats;
        public List<User> users;

        public GetMessagesRequest(List<int> id)
        {
            _id = id;
        }

        public override void OnSend(BinaryWriter writer)
        {
            writer.Write(0x4222fa74);
            writer.Write(0x1cb5c415); // vector#1cb5c415
            writer.Write(_id.Count); // vector length
            foreach (var id in _id)
            {
                writer.Write(id);
            }
        }

        public override void OnResponse(BinaryReader reader)
        {
            bool messagesSlice = reader.ReadUInt32() == 0xb446ae3; // else messages#8c718e87

            if (messagesSlice) reader.ReadInt32(); // count

            // messages
            var result = reader.ReadUInt32(); // vector#1cb5c415
            int messages_len = reader.ReadInt32();
            messages = new List<Message>(messages_len);
            for (var i = 0; i < messages_len; i++)
            {
                var msgEl = TL.Parse<Message>(reader);

                messages.Add(msgEl);
            }

            // chats
            //reader.ReadUInt32();
            //int chats_len = reader.ReadInt32();
            //chats = new List<Chat>(chats_len);
            //for (int i = 0; i < chats_len; i++)
            //    chats.Add(TL.Parse<Chat>(reader));

            /*
			// users
			reader.ReadUInt32();
			int users_len = reader.ReadInt32();
			users = new List<User>(users_len);
			for (int i = 0; i < users_len; i++)
				users.Add(TL.Parse<User>(reader));
			*/
        }

        public override void OnException(Exception exception)
        {
            throw new NotImplementedException();
        }

        public override bool Confirmed => true;
        public override bool Responded { get; }
    }
}
