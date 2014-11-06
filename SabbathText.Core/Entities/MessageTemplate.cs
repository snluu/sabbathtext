﻿
namespace SabbathText.Core.Entities
{
    public enum MessageTemplate
    {
        BadRequest = 0,
        SubscriberRequired = 1,

        GeneralGreetings = 2,
        SubscriberGreetings = 3,
        Help = 4,
        SubscribedMissingZipCode = 5,
        SubscribedConfirmZipCode = 6,
        ConfirmZipCodeUpdate = 7,
    }
}
