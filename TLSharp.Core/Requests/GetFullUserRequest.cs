using System;
using System.IO;
using TLSharp.Core.MTProto;

namespace TLSharp.Core.Requests
{
    class GetFullUserRequest : MTProtoRequest
    {
        InputUser _id;

        public User user;

        public GetFullUserRequest(InputUser id)
        {
            _id = id;
        }

        public override void OnSend(BinaryWriter writer)
        {
            writer.Write(0xca30a5b1);
            _id.Write(writer);
        }
        
        public override void OnResponse(BinaryReader reader)
        {
            var code = reader.ReadUInt32(); // users.userFull#771095da 
            user = TL.Parse<User>(reader);
        }

        public override void OnException(Exception exception)
        {
            throw new NotImplementedException();
        }

        public override bool Confirmed => true;
        public override bool Responded { get; }
    }
}
