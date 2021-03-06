﻿namespace SabbathText.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SabbathText.Entities;
    using SabbathText.Location.V1;

    /// <summary>
    /// This operation processes incoming "subscribe" messages
    /// </summary>
    public class SubscribeMessageOperation : BaseOperation<bool>
    {
        private SubscribeMessageOperationCheckpointData checkpointData;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="context">The operation context.</param>
        public SubscribeMessageOperation(OperationContext context)
            : base(context, "SubscribeMessageOperation.V1")
        {
        }

        /// <summary>
        /// Run the operation.
        /// </summary>
        /// <param name="incomingMessage">The incoming message.</param>
        /// <returns>The operation response.</returns>
        public Task<OperationResponse<bool>> Run(Message incomingMessage)
        {
            this.checkpointData = new SubscribeMessageOperationCheckpointData(this.Context.Account.AccountId);
            return this.TransitionToProcessMessage(incomingMessage);
        }

        /// <summary>
        /// Resumes the operation.
        /// </summary>
        /// <param name="serializedCheckpointData">The serialized checkpoint data.</param>
        /// <returns>The operation response.</returns>
        protected override Task<OperationResponse<bool>> Resume(string serializedCheckpointData)
        {
            this.checkpointData = JsonConvert.DeserializeObject<SubscribeMessageOperationCheckpointData>(serializedCheckpointData);

            switch (this.checkpointData.State)
            {
                case RespondingMessageOperationState.ProcessingMessage:
                    return this.EnterProcessMessage();
                case RespondingMessageOperationState.UpdatingAccount:
                    return this.EnterUpdateAccount();
            }

            throw new NotImplementedException();
        }

        private async Task<OperationResponse<bool>> TransitionToProcessMessage(Message incomingMessage)
        {
            this.checkpointData.State = RespondingMessageOperationState.ProcessingMessage;
            this.checkpointData.IncomingMessage = incomingMessage;
            this.checkpointData.OutgoingMessageId = Guid.NewGuid().ToString();

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterProcessMessage();
        }

        private async Task<OperationResponse<bool>> EnterProcessMessage()
        {
            Message outgoingMessage;

            if (this.Context.Account.Status == AccountStatus.Subscribed &&
                string.IsNullOrWhiteSpace(this.Context.Account.ZipCode) == false &&
                LocationInfo.FromZipCode(this.Context.Account.ZipCode) != null)
            {
                LocationInfo location = LocationInfo.FromZipCode(this.Context.Account.ZipCode);
                outgoingMessage =
                    Message.CreateSubscribedForLocation(this.Context.Account.PhoneNumber, location);
            }
            else
            {
                outgoingMessage =
                    Message.CreatePromptZipCode(this.Context.Account.PhoneNumber);
            }

            await this.Bag.MessageClient.SendMessage(outgoingMessage, this.checkpointData.OutgoingMessageId, this.Context.CancellationToken);
            return await this.TransitionToUpdateAccount(outgoingMessage);
        }

        private async Task<OperationResponse<bool>> TransitionToUpdateAccount(Message outgoingMessage)
        {
            this.checkpointData.OutgoingMessage = outgoingMessage;
            this.checkpointData.State = RespondingMessageOperationState.UpdatingAccount;
            this.checkpointData.IncomingMessageId = Guid.NewGuid().ToString();

            return
                await this.SetCheckpoint(this.checkpointData) ??
                await this.EnterUpdateAccount();
        }

        private async Task<OperationResponse<bool>> EnterUpdateAccount()
        {
            this.Context.Account.Status = AccountStatus.Subscribed;

            MessageEntity incomingEntity = this.checkpointData.IncomingMessage.ToEntity(
                this.Context.Account.AccountId,
                this.checkpointData.IncomingMessageId,
                MessageDirection.Incoming,
                MessageStatus.Responded);
            TryAddMessageEntity(this.Context.Account, incomingEntity);

            MessageEntity outgoingEntity = this.checkpointData.OutgoingMessage.ToEntity(
                this.Context.Account.AccountId,
                this.checkpointData.OutgoingMessageId,
                MessageDirection.Outgoing,
                MessageStatus.Sent);
            TryAddMessageEntity(this.Context.Account, outgoingEntity);

            await this.Bag.AccountStore.Update(this.Context.Account, this.Context.CancellationToken);

            return await this.CompleteCheckpoint(this.checkpointData, HttpStatusCode.OK, true);
        }
    }
}
