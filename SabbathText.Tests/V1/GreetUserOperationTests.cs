﻿namespace SabbathText.Tests.V1
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// Tests GreetUserOperation
    /// </summary>
    [TestClass]
    public class GreetUserOperationTests : TestBase
    {
        /// <summary>
        /// This test makes sure GreetUserOperation will return HTTP Accepted
        /// on successful cases.
        /// </summary>
        [TestMethod]
        public void GreetUserOperation_ShouldReturnAccepted()
        {
            GreetUserOperation operation = new GreetUserOperation(CreateContext());
            OperationResponse<bool> response = operation.Run().Result;

            Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
        }

        /// <summary>
        /// This test makes sure that the GreetUserOperation is pushed to completion,
        /// and will send out messages
        /// </summary>
        [TestMethod]
        public void GreetUserOperation_ShouldFinishSendingTheMessage()
        {
            OperationContext context = CreateContext();
            GreetUserOperation operation = new GreetUserOperation(context);
            OperationResponse<bool> response = operation.Run().Result;

            Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);

            RunCheckpointWorker();

            Message message =
                ((InMemoryMessageClient)GoodieBag.Create().MessageClient).Messages
                .FirstOrDefault(m => m.Recipient == context.Account.PhoneNumber && m.Template == MessageTemplate.Greetings);

            AccountEntity account =
                GoodieBag.Create().AccountStore.Get(context.Account.PartitionKey, context.Account.RowKey, CancellationToken.None).Result;

            MessageEntity messageEntity = account.RecentMessages.FirstOrDefault(m =>
                m.Recipient == context.Account.PhoneNumber &&
                m.Direction == MessageDirection.Outgoing &&
                m.Status == MessageStatus.Sent &&
                m.Template == MessageTemplate.Greetings);

            Assert.IsNotNull(messageEntity, "Cannot find the message entity from the recent messages");
        }

        /// <summary>
        /// We can only send the user a greeting message once.
        /// We don't want to spam the user.
        /// </summary>
        [TestMethod]
        public void GreetUserOperation_CanOnlyMessageUserOnce()
        {
            AccountEntity account = CreateAccount();

            OperationContext context = CreateContext(account);
            GreetUserOperation operation = new GreetUserOperation(context);
            OperationResponse<bool> response = operation.Run().Result;

            Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);

            // wait 1 second to make sure the compensation agent can pick up the queue item
            RunCheckpointWorker();

            OperationContext context2 = CreateContext(GetAccount(account.AccountId));
            GreetUserOperation operation2 = new GreetUserOperation(context2);
            OperationResponse<bool> response2 = operation2.Run().Result;

            Assert.AreEqual(HttpStatusCode.Forbidden, response2.StatusCode);
            Assert.AreEqual(GreetUserOperation.UserHasBeenGreetedError, response2.ErrorCode);
        }
    }
}
