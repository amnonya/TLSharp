using System;
using System.Collections.Generic;
using System.IO;
using TLSharp.Core.MTProto;

namespace TLSharp.Core.Requests
{
    class GetContactsRequest : MTProtoRequest
    {
        string _hash;

        public List<Contact> contacts;

        public GetContactsRequest(string hash)
        {
            _hash = hash;
        }

        public override void OnSend(BinaryWriter writer)
        {
            writer.Write(0x22c6aa08);
            Serializers.String.write(writer, _hash);
        }

        public override void OnResponse(BinaryReader reader)
        {
            bool contactsModified = reader.ReadUInt32() == 0x6f8b8cb2; // else contacts.contactsNotModified#b74ba9d2  

            if (contactsModified)
            {
                //reader.ReadInt32(); // count

                var result = reader.ReadUInt32(); // vector#1cb5c415
                int contacts_len = reader.ReadInt32();
                contacts = new List<Contact>(contacts_len);
                for (var i = 0; i < contacts_len; i++)
                {
                    var contactEl = TL.Parse<Contact>(reader);

                    contacts.Add(contactEl);
                }
            }
        }

        public override void OnException(Exception exception)
        {
            throw new NotImplementedException();
        }

        public override bool Confirmed => true;
        public override bool Responded { get; }
    }
}
