﻿using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TLSharp.Core;
using TLSharp.Core.Auth;
using TLSharp.Core.MTProto;
using TLSharp.Core.Network;

namespace TLSharp.Tests
{
	[TestClass]
	public class TLSharpTests
	{
		private string NumberToSendMessage { get; set; }

		private string NumberToAuthenticate { get; set; }

		private string UserNameToSendMessage { get; set; }

        private string Query { get; set; }

        [TestInitialize]
		public void Init()
		{
			// Setup your phone numbers in app.config
			NumberToAuthenticate = ConfigurationManager.AppSettings["numberToAuthenticate"];
			if (string.IsNullOrEmpty(NumberToAuthenticate))
				throw new InvalidOperationException("NumberToAuthenticate is null. Specify number in app.config");

			NumberToSendMessage = ConfigurationManager.AppSettings["numberToSendMessage"];
			if (string.IsNullOrEmpty(NumberToSendMessage))
				throw new InvalidOperationException("NumberToSendMessage is null. Specify number in app.config");

			UserNameToSendMessage = ConfigurationManager.AppSettings["userNameToSendMessage"];
			if (string.IsNullOrEmpty(UserNameToSendMessage))
				throw new InvalidOperationException("UserNameToSendMessage is null. Specify userName in app.config");

            Query = ConfigurationManager.AppSettings["query"];
            if (string.IsNullOrEmpty(Query))
                throw new InvalidOperationException("Query is null. Specify query in app.config");

        }

		[TestMethod]
		public async Task AuthUser()
		{
			var store = new FileSessionStore();
			var client = new TelegramClient(store, "session");

			await client.Connect();

			var hash = await client.SendCodeRequest(NumberToAuthenticate);
			var code = "90579"; // you can change code in debugger

			var user = await client.MakeAuth(NumberToAuthenticate, hash, code);

			Assert.IsNotNull(user);
		}

		[TestMethod]
		public async Task CheckPhones()
		{
			var store = new FileSessionStore();
			var client = new TelegramClient(store, "session");
			await client.Connect();

			var result = await client.IsPhoneRegistered(NumberToAuthenticate);
			Assert.IsTrue(result);
		}

		[TestMethod]
		public async Task ImportContactByPhoneNumber()
		{
			// User should be already authenticated!

			var store = new FileSessionStore();
			var client = new TelegramClient(store, "session");
			
			await client.Connect();

			Assert.IsTrue(client.IsUserAuthorized());

			var res = await client.ImportContactByPhoneNumber(NumberToSendMessage);

			Assert.IsNotNull(res);
		}

		[TestMethod]
		public async Task ImportByUserName()
		{
			var store = new FileSessionStore();
			var client = new TelegramClient(store, "session");

			await client.Connect();

			Assert.IsTrue(client.IsUserAuthorized());

			var res = await client.ImportByUserName(UserNameToSendMessage);

			Assert.IsNotNull(res);
		}

		[TestMethod]
		public async Task ImportByUserNameAndSendMessage()
		{
			var store = new FileSessionStore();
			var client = new TelegramClient(store, "session");

			await client.Connect();

			Assert.IsTrue(client.IsUserAuthorized());

			var res = await client.ImportByUserName(UserNameToSendMessage);

			Assert.IsNotNull(res);

			await client.SendMessage(res.Value, "Test message from TelegramClient");
		}

		[TestMethod]
		public async Task ImportContactByPhoneNumberAndSendMessage()
		{
			// User should be already authenticated!

			var store = new FileSessionStore();
			var client = new TelegramClient(store, "session");
			await client.Connect();

			Assert.IsTrue(client.IsUserAuthorized());

			var res = await client.ImportContactByPhoneNumber(NumberToSendMessage);

			Assert.IsNotNull(res);

			await client.SendMessage(res.Value, "Test message from TelegramClient");
		}

		[TestMethod]
		public async Task GetHistory()
		{
			var store = new FileSessionStore();
			var client = new TelegramClient(store, "session");
			await client.Connect();

			Assert.IsTrue(client.IsUserAuthorized());

			var res = await client.ImportContactByPhoneNumber(NumberToSendMessage);

			Assert.IsNotNull(res);

			var hist = await client.GetMessagesHistoryForContact(res.Value, 0, 5);

			Assert.IsNotNull(hist);
		}

        [TestMethod]
        public async Task GetMessages()
        {
            var store = new FileSessionStore();
            var client = new TelegramClient(store, "session");
            await client.Connect();

            Assert.IsTrue(client.IsUserAuthorized());

            var hist = await client.GetMessages(Enumerable.Range(151,75).ToList());

            Assert.IsNotNull(hist);
        }

        [TestMethod]
        public async Task GetContacts()
        {
            var store = new FileSessionStore();
            var client = new TelegramClient(store, "session");
            await client.Connect();

            Assert.IsTrue(client.IsUserAuthorized());

            var res = await client.ImportContactByPhoneNumber(NumberToSendMessage);

            Assert.IsNotNull(res);

            var cnts = await client.GetContacts();

            Assert.IsNotNull(cnts);
        }

        [TestMethod]
        public async Task GetUsers()
        {
            var store = new FileSessionStore();
            var client = new TelegramClient(store, "session");
            await client.Connect();

            Assert.IsTrue(client.IsUserAuthorized());

            var ids = (await client.GetContacts()).Cast<ContactConstructor>();

            var ius = ids.Select(r => TL.inputUserContact(r.user_id)).ToList();

            var res = new User[ius.Count];
            for (int i = 0; i < res.Length; i++)
            {
                var r = await client.GetFullUser(ius[i]);
                res[i] = r;
            }      

            Assert.IsNotNull(res);
        }

        [TestMethod]
        public async Task SearchMessage()
        {
            var store = new FileSessionStore();
            var client = new TelegramClient(store, "session");
            await client.Connect();

            Assert.IsTrue(client.IsUserAuthorized());

            var res = await client.ImportContactByPhoneNumber(NumberToSendMessage);

            Assert.IsNotNull(res);

            //int d = (int)new TimeSpan().TotalMilliseconds;

            var hist = await client.SearchMessagesForContact(res.Value, Query,
                new InputMessagesFilterPhotosConstructor(), -1, -1, 0, 5);

            Assert.IsNotNull(hist);
        }

        [TestMethod]
		public async Task UploadAndSendMedia()
		{
			var store = new FileSessionStore();
			var client = new TelegramClient(store, "session");
			await client.Connect();

			Assert.IsTrue(client.IsUserAuthorized());

			var res = await client.ImportContactByPhoneNumber(NumberToSendMessage);

			Assert.IsNotNull(res);
			const string testFile = "TEST";

			var file = File.ReadAllBytes("../../data/cat.jpg");

			var mediaFile = await client.UploadFile("test_file.jpg", file);

			Assert.IsNotNull(mediaFile);

			var state = await client.SendMediaMessage(res.Value, mediaFile);

			Assert.IsTrue(state);
		}

		[TestMethod]
		public async Task TestConnection()
		{
			var store = new FakeSessionStore();
			var client = new TelegramClient(store, "");

			Assert.IsTrue(await client.Connect());
		}

		[TestMethod]
		public async Task AuthenticationWorks()
		{
			using (var transport = new TcpTransport("91.108.56.165", 443))
			{
				var authKey = await Authenticator.DoAuthentication(transport);

				Assert.IsNotNull(authKey.AuthKey.Data);
			}
		}
	}
}
