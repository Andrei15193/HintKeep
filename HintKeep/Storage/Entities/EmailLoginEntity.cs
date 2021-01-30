using System;
using Microsoft.Azure.Cosmos.Table;

namespace HintKeep.Storage.Entities
{
    public class EmailLoginEntity : TableEntity
    {
        public string PasswordSalt { get; set; }

        public string PasswordHash { get; set; }

        public string State { get; set; }

        public string UserId { get; set; }
    }
}